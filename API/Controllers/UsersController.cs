using API.Extensions;
using BLL.IServices;
using DTO.Security;
using DTO.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]

        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]

        public ActionResult<UserDto> GetUserById(string id) // שינוי מ-int ל-string
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound(new { message = "המשתמש לא נמצא" });
            return Ok(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult RegisterUser([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = _userService.RegisterUser(model);
                return Ok(new { message = "המשתמש נוצר בהצלחה!", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public ActionResult UpdateUser(string id, [FromBody] RegisterUserDto model)
        {
            try
            {
                string currentUserId = User.GetUserId().ToString();
                bool isAdmin = User.IsInRole("Admin");
                if (id != currentUserId && !isAdmin)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "אינך מורשה לעדכן משתמש זה." });
                }
                _userService.UpdateUser(id, model);
                return Ok(new { message = "המשתמש עודכן בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(string id)
        {
            try
            {
                string currentUserId = User.GetUserId().ToString();
                bool isAdmin = User.IsInRole("Admin");
                if (id != currentUserId && !isAdmin)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "אינך מורשה למחוק משתמש זה." });
                }
                _userService.DeleteUser(id);
                return Ok(new { message = "המשתמש נמחק בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // POST api/user/register
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register(RegisterDto dto)
        {
            if (!_userService.Register(dto))
                return Conflict("Email is already registered.");

            // ההרשמה רק יוצרת את המשתמש. את הטוקן מקבלים בהתחברות
            return Ok();
        }

        // POST api/user/login
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<AuthResponseDto> Login(LoginDto dto)
        {
            var result = _userService.Login(dto);
            if (result is null)
                return Unauthorized("Wrong email or password.");

            return Ok(result);
        }
    }
}