using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Security;
using DTO.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using BLL.Exceptions; // הוספת ה-using לחריגות שלנו

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokens;
        private readonly IPasswordHasher<User> _hasher;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ITokenService tokens,
            IPasswordHasher<User> hasher)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tokens = tokens;
            _hasher = hasher;
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        // שליפת משתמש לפי תעודת זהות - זריקת NotFoundException במקום null
        public UserDto GetUserById(string id)
        {
            var user = _userRepository.GetByIdNumber(id);
            if (user == null)
            {
                throw new NotFoundException($"User with identity number {id} was not found");
            }
            return _mapper.Map<UserDto>(user);
        }

        // עדכון משתמש לפי מזהה המשתמש המחובר מהטוקן
        public void UpdateUser(int currentUserId, RegisterUserDto model)
        {
            var user = _userRepository.GetOwned(currentUserId);
            if (user == null)
            {
                throw new NotFoundException("המשתמש לא נמצא.");
            }

            _mapper.Map(model, user);
            _userRepository.Update(user);
        }

        // מחיקת משתמש לפי מזהה המשתמש המחובר מהטוקן
        public void DeleteUser(int currentUserId)
        {
            var user = _userRepository.GetOwned(currentUserId);
            if (user == null)
            {
                throw new NotFoundException("המשתמש לא נמצא.");
            }

            _userRepository.Delete(user.Id);
        }

        // הרשמת משתמש - זריקת ConflictException במידה והאימייל כבר רשום במערכת
        public bool Register(RegisterUserDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (_userRepository.GetByEmail(email) != null)
            {
                throw new ConflictException("כתובת האימייל כבר רשומה במערכת.");
            }

            var user = new User
            {
                Email = email,
                Role = "User",
                Gender = dto.Gender,
                Phone = dto.Phone,
                Id = dto.Id
            };

            user.Password = _hasher.HashPassword(user, dto.Password);

            _userRepository.Add(user);
            return true;
        }

        // התחברות משתמש - זריקת BusinessRuleException במידה והפרטים אינם נכונים
        public AuthResponseDto Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = _userRepository.GetByEmail(email);
            if (user is null)
            {
                throw new BusinessRuleException("אימייל או סיסמה שגויים.");
            }

            var result = _hasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new BusinessRuleException("אימייל או סיסמה שגויים.");
            }

            return _tokens.CreateToken(user);
        }
    }
}