
using API.Extensions;
using BLL.IServices;
using DTO.Security;
using DTO.Users;
using GenerativeAI.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ==========================================
        // שליפת כל המשתמשים - מנהל בלבד
        // ==========================================

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }

        // ==========================================
        // שליפת משתמש לפי תעודת זהות
        // ==========================================

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<UserDto> GetUserById(string id)
        {
            var user = _userService.GetUserById(id);
            return Ok(user);
        }

        // ==========================================
        // עדכון פרטי המשתמש המחובר
        // ==========================================

        // PUT: api/Users
        [HttpPut]
        [Authorize]
        public IActionResult UpdateUser(
            [FromBody] RegisterUserDto model)
        {
            int currentUserId = User.GetUserId();

            _userService.UpdateUser(currentUserId, model);

            return Ok(new
            {
                message = "המשתמש עודכן בהצלחה!"
            });
        }

        // ==========================================
        // מחיקת המשתמש המחובר
        // ==========================================

        // DELETE: api/Users
        [HttpDelete]
        [Authorize]
        public IActionResult DeleteUser()
        {
            int currentUserId = User.GetUserId();

            _userService.DeleteUser(currentUserId);

            return Ok(new
            {
                message = "המשתמש נמחק בהצלחה!"
            });
        }

        // ==========================================
        // מחיקה לוגית של משתמש - מנהל בלבד
        // ==========================================

        // DELETE: api/Users/{userId}/soft-delete
        [HttpDelete("{userId:int}/soft-delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult SoftDeleteUser(int userId)
        {
            int currentUserId = User.GetUserId();

            // מניעת מחיקה עצמית של המנהל
            if (currentUserId == userId)
            {
                return BadRequest(new
                {
                    message = "לא ניתן להסיר את החשבון של עצמך."
                });
            }

            bool deleted = _userService.SoftDeleteUser(userId);

            if (!deleted)
            {
                return BadRequest(new
                {
                    message = "לא ניתן להסיר את המשתמש. ייתכן שהוא לא נמצא, כבר הוסר או שהוא מנהל."
                });
            }

            return Ok(new
            {
                message = "המשתמש הוסר בהצלחה."
            });
        }

        // ==========================================
        // שחזור משתמש שהוסר - מנהל בלבד
        // ==========================================

        // PUT: api/Users/{userId}/restore
        [HttpPut("{userId:int}/restore")]
        [Authorize(Roles = "Admin")]
        public IActionResult RestoreUser(int userId)
        {
            bool restored = _userService.RestoreUser(userId);

            if (!restored)
            {
                return BadRequest(new
                {
                    message = "לא ניתן לשחזר את המשתמש. ייתכן שהוא לא נמצא, אינו מוסר או שהוא מנהל."
                });
            }

            return Ok(new
            {
                message = "המשתמש שוחזר בהצלחה."
            });
        }



        // ==========================================
        // הרשמה - יצירת בקשה שממתינה לאישור
        // ==========================================

        // POST: api/Users/register
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register(
            [FromBody] RegisterUserDto dto)
        {
            _userService.Register(dto);

            return Ok(new
            {
                message = "בקשת ההצטרפות נשלחה בהצלחה וממתינה לאישור מנהל."
            });
        }

        // ==========================================
        // התחברות
        // ==========================================

        // POST: api/Users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<AuthResponseDto> Login(
            [FromBody] LoginDto dto)
        {
            var result = _userService.Login(dto);

            return Ok(result);
        }

        // ==========================================
        // בקשות שממתינות לאישור מנהל
        // ==========================================

        // GET: api/Users/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetPendingUsers()
        {
            var users = _userService.GetPendingUsers();

            return Ok(users);
        }

        // ==========================================
        // משתמשים שממתינים להגדרת סיסמה
        // ==========================================

        // GET: api/Users/waiting-for-password
        [HttpGet("waiting-for-password")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsersWaitingForPassword()
        {
            var users =
                _userService.GetUsersWaitingForPassword();

            return Ok(users);
        }

        // ==========================================
        // אישור בקשת הצטרפות
        // ==========================================

        // PUT: api/Users/{userId}/approve
        [HttpPut("{userId:int}/approve")]
        [Authorize(Roles = "Admin")]
        public IActionResult ApproveUser(int userId)
        {
            bool approved =
                _userService.ApproveUser(userId);

            if (!approved)
            {
                return BadRequest(new
                {
                    message = "המשתמש לא נמצא או שכבר אושר."
                });
            }

            return Ok(new
            {
                message = "המשתמש אושר ונשלח אליו מייל להגדרת סיסמה."
            });
        }

        // ==========================================
        // שליחה חוזרת של קישור אחרי 24 שעות
        // ==========================================

        // POST: api/Users/{userId}/resend-password-link
        [HttpPost("{userId:int}/resend-password-link")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResendPasswordSetupLink(
            int userId)
        {
            bool sent =
                _userService.ResendPasswordSetupLink(userId);

            if (!sent)
            {
                return BadRequest(new
                {
                    message =
                        "לא ניתן לשלוח קישור חדש. " +
                        "ייתכן שטרם עברו 24 שעות, " +
                        "שהמשתמש אינו מאושר, " +
                        "או שכבר הוגדרה סיסמה."
                });
            }

            return Ok(new
            {
                message = "קישור חדש להגדרת סיסמה נשלח בהצלחה."
            });
        }

        // ==========================================
        // מחיקת בקשת הרשמה
        // ==========================================

        // DELETE: api/Users/{userId}/registration-request
        [HttpDelete("{userId:int}/registration-request")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteRegistrationRequest(
            int userId)
        {
            bool deleted =
                _userService.DeleteRegistrationRequest(userId);

            if (!deleted)
            {
                return BadRequest(new
                {
                    message =
                        "לא ניתן למחוק את הבקשה. " +
                        "המשתמש לא נמצא או שכבר הגדיר סיסמה."
                });
            }

            return Ok(new
            {
                message = "בקשת ההצטרפות נמחקה בהצלחה."
            });
        }

        // ==========================================
        // הגדרת סיסמה באמצעות קישור מהמייל
        // ==========================================

        // POST: api/Users/set-password
        [HttpPost("set-password")]
        [AllowAnonymous]
        public IActionResult SetPassword(
            [FromBody] SetPasswordDto dto)
        {
            bool success =
                _userService.SetPassword(dto);

            if (!success)
            {
                return BadRequest(new
                {
                    message =
                        "הקישור אינו תקין, פג תוקפו " +
                        "או שכבר נעשה בו שימוש."
                });
            }

            return Ok(new
            {
                message =
                    "הסיסמה הוגדרה בהצלחה. כעת ניתן להתחבר."
            });
        }


        // ==========================================
        // שליפת ראש המפלגה
        // GET: api/Users/party-leader
        // גישה לכל משתמש עם JWT תקין
        // ==========================================

        [HttpGet("party-leader")]
        [Authorize]
        public IActionResult GetPartyLeader()
        {
            var leader = _userService.GetPartyLeader();

            if (leader == null)
            {
                return NotFound(new
                {
                    message = "ראש המפלגה לא נמצא"
                });
            }

            return Ok(new
            {
                leader.UserId,
                leader.Name,
                Role = "ראש המפלגה"
            });
        }

    }
}
