using Microsoft.AspNetCore.Mvc;
using BLL.IServices;
using DTO.Users;
using System.Collections.Generic;
using System;

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
        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(string id) // שינוי מ-int ל-string
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound(new { message = "המשתמש לא נמצא" });
            return Ok(user);
        }

        [HttpPost]
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
        public ActionResult UpdateUser(string id, [FromBody] RegisterUserDto model) // שינוי ל-string
        {
            try
            {
                _userService.UpdateUser(id, model);
                return Ok(new { message = "המשתמש עודכן בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(string id) // שינוי ל-string
        {
            try
            {
                _userService.DeleteUser(id);
                return Ok(new { message = "המשתמש נמחק בהצלחה!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}