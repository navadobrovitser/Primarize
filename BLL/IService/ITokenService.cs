using DAL.Entities;
using DTO.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IServices
{
    public interface ITokenService
    {
        AuthResponseDto CreateToken(User user);
    }
}
