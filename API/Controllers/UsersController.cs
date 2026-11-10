using API.Extensions;
using BLL.IServices;
using DTO.Security;
using DTO.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]//deler it
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }

        // GET: api/Users/id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserDto> GetUserById(string id)
        {
            // ה-Service זורק NotFoundException במידה ואינו מוצא, וה-Filter מחזיר 404
            var user = _userService.GetUserById(id);
            return Ok(user);
        }

        // PUT: api/Users
        [HttpPut]
        [Authorize]
        public ActionResult UpdateUser([FromBody] RegisterUserDto model)
        {
            int currentUserId = User.GetUserId();
            _userService.UpdateUser(currentUserId, model);
            return Ok(new { message = "המשתמש עודכן בהצלחה!" });
        }

        // DELETE: api/Users
        [HttpDelete]
        [Authorize]
        public ActionResult DeleteUser()
        {
            int currentUserId = User.GetUserId();
            _userService.DeleteUser(currentUserId);
            return Ok(new { message = "המשתמש נמחק בהצלחה!" });
        }

        // POST: api/Users/register
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterUserDto dto)
        {
            // במידה והמייל קיים, ה-Service זורק ConflictException וה-Filter מחזיר 409
            _userService.Register(dto);
            return Ok(new { message = "הרישום בוצע בהצלחה!" });
        }

        // POST: api/Users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<AuthResponseDto> Login([FromBody] LoginDto dto)
        {
            // במידה והפרטים שגויים, ה-Service זורק BusinessRuleException וה-Filter מחזיר 400
            var result = _userService.Login(dto);
            return Ok(result);
        }
    }
}