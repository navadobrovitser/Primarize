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

        // עדכון משתמש לפי מזהה המשתמש המחובר מהטוקן
        public void UpdateUser(int currentUserId, RegisterUserDto model)
        {
            var user = _userRepository.GetOwned(currentUserId);
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
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
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }

            _userRepository.Delete(user.Id);
        }

        public bool Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (_userRepository.GetByEmail(email) != null)
                return false;

            var user = new User
            {
                Email = email,
                Role = "User",
                Gender = "",
                Phone = "",
                Id = ""
            };

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

            var result = _hasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            return _tokens.CreateToken(user);
        }
    }
}