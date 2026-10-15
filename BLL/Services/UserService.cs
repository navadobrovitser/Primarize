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

        public UserDto? GetUserById(string id) // שינוי מ-int ל-string
        {
            var user = _userRepository.GetByIdNumber(id); // קריאה לפונקציה שמחפשת לפי שדה ה-Id (תעודת זהות)
            if (user == null) return null;
            return _mapper.Map<UserDto>(user);
        }
        public UserDto RegisterUser(RegisterUserDto model)
        {
            // בדיקה האם קיים משתמש עם תעודת זהות (Id) זו כבר במערכת
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
            var user = _userRepository.GetByIdNumber(id); // חיפוש לפי תעודת זהות
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }

            _mapper.Map(model, user);
            _userRepository.Update(user);
        }

        public void DeleteUser(string id)
        {
            var user = _userRepository.GetByIdNumber(id); // חיפוש לפי תעודת זהות
            if (user == null)
            {
                throw new KeyNotFoundException("המשתמש לא נמצא.");
            }
            _userRepository.Delete(id); // שולח את ה-string לפונקציית המחיקה שיצרנו
        }
    }
}