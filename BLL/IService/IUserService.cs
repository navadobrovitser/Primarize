using DTO.Security;
using DTO.Users;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IUserService
    {
        IEnumerable<UserDto> GetAllUsers();
        UserDto? GetUserById(string id);
        UserDto RegisterUser(RegisterUserDto model);
        void UpdateUser(string id, RegisterUserDto model);
        void DeleteUser(string id);


        bool Register(RegisterDto dto);
        AuthResponseDto? Login(LoginDto dto);
    }
}