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
        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserDto> GetUserById(string id)
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

        // עדכון המשתמש המחובר - מזהה המשתמש נלקח אך ורק מהטוקן
        [HttpPut]
        public ActionResult UpdateUser([FromBody] RegisterUserDto model)
        {
            try
            {
                int currentUserId = User.GetUserId();
                _userService.UpdateUser(currentUserId, model);
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

        // מחיקת המשתמש המחובר - מזהה המשתמש נלקח אך ורק מהטוקן
        [HttpDelete]
        public ActionResult DeleteUser()
        {
            try
            {
                int currentUserId = User.GetUserId();
                _userService.DeleteUser(currentUserId);
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