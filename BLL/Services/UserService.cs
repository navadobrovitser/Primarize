using AutoMapper;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Users;
using System;
using System.Collections.Generic;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public UserDto? GetUserById(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return null;
            return _mapper.Map<UserDto>(user);
        }

        public UserDto RegisterUser(RegisterUserDto model)
        {
            var existingUser = _userRepository.GetByEmail(model.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("משתמש עם כתובת דוא\"ל זו כבר קיים במערכת.");
            }

            var user = _mapper.Map<User>(model);
            _userRepository.Add(user);

            return _mapper.Map<UserDto>(user);
        }

        public void UpdateUser(int id, RegisterUserDto model)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }

            _mapper.Map(model, user);
            _userRepository.Update(user);
        }

        public void DeleteUser(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }
            _userRepository.Delete(id);
        }
    }
}