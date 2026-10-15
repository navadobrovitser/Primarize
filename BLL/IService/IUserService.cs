using DTO.Users;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IUserService
    {
        IEnumerable<UserDto> GetAllUsers();
        UserDto? GetUserById(int id);
        UserDto RegisterUser(RegisterUserDto model);
        void UpdateUser(int id, RegisterUserDto model);
        void DeleteUser(int id);
    }
}