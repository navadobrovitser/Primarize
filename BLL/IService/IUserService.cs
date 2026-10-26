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

        void UpdateUser(int currentUserId, RegisterUserDto model);
        void DeleteUser(int currentUserId);

        bool Register(RegisterUserDto dto);
        AuthResponseDto? Login(LoginDto dto);
    }
}