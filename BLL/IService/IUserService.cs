using DTO.Security;
using DTO.Users;
using GenerativeAI.Types;
using System.Collections.Generic;

namespace BLL.IServices
{
    public interface IUserService
    {
        IEnumerable<UserDto> GetAllUsers();
        UserDto GetUserById(string id);
        //UserDto RegisterUser(RegisterUserDto model);

        void UpdateUser(int currentUserId, RegisterUserDto model);
        void DeleteUser(int currentUserId);
        bool SoftDeleteUser(int userId);
        bool RestoreUser(int userId);
        bool Register(RegisterUserDto dto);
        AuthResponseDto Login(LoginDto dto);

        IEnumerable<UserDto> GetPendingUsers();
        bool ApproveUser(int userId);
        bool SetPassword(SetPasswordDto dto);

        IEnumerable<UserDto> GetUsersWaitingForPassword();

        bool ResendPasswordSetupLink(int userId);

        bool DeleteRegistrationRequest(int userId);
    }
}