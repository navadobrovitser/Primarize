using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Security;
using DTO.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

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

        // --- מתודות ה-CRUD הקיימות ---

        public IEnumerable<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public UserDto? GetUserById(string id)
        {
            var user = _userRepository.GetByIdNumber(id);
            if (user == null) return null;
            return _mapper.Map<UserDto>(user);
        }

        public UserDto RegisterUser(RegisterUserDto model)
        {
            var existingUser = _userRepository.GetByIdNumber(model.Id);
            if (existingUser != null)
            {
                throw new ArgumentException("משתמש עם מספר תעודת זהות זה כבר קיים במערכת.");
            }

            var user = _mapper.Map<User>(model);
            _userRepository.Add(user);

            return _mapper.Map<UserDto>(user);
        }

        public void UpdateUser(string id, RegisterUserDto model)
        {
            var user = _userRepository.GetByIdNumber(id);
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }

            _mapper.Map(model, user);
            _userRepository.Update(user);
        }

        public void DeleteUser(string id)
        {
            var user = _userRepository.GetByIdNumber(id);
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }
            _userRepository.Delete(id);
        }

        // --- מתודות האבטחה והאימות החדשות ---

        public bool Register(RegisterUserDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            // בדיקה האם המשתמש כבר קיים לפי אימייל או תעודת זהות
            var existingUserByEmail = _userRepository.GetByEmail(email);
            if (existingUserByEmail != null)
                return false;

            var user = _mapper.Map<User>(dto);
            user.Email = email;
            user.Role = "User"; // התפקיד נקבע בשרת

            // הצפנת הסיסמה לפני השמירה
            user.Password = _hasher.HashPassword(user, dto.Password);

            _userRepository.Add(user);
            return true;
        }

        public AuthResponseDto? Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = _userRepository.GetByEmail(email);
            if (user is null)
                return null;

            // אימות הסיסמה המוצפנת
            var result = _hasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            // הנפקת הטוקן במידה והסיסמה נכונה
            return _tokens.CreateToken(user);
        }
    }
}