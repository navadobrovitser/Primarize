using DTO.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // 1. Get: מחזיר את כל המשתמשים (רשימה)
        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> GetAllUsers()
        {
            // בינתיים אנחנו שמים רשימה ריקה או דמה, עד שחברה שלך תעלה את ה-BLL
            var users = new List<UserDto>();
            return Ok(users);
        }

        // 2. Get לפי ID: מחזיר משתמש בודד לפי מזהה
        [HttpGet("{id}")]
        public ActionResult<UserDto> GetUserById(int id)
        {
            // בדיקת דמה עד לחיבור ה-BLL
            return Ok(new UserDto { UserId = id, Name = "משתמש לדוגמה", Email = "test@example.com" });
        }

        // 3. Post: הוספת משתמש חדש (הרשמה)
        [HttpPost]
        public ActionResult RegisterUser([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // כאן בהמשך נקרא ללוגיקה של ה-BLL
            return Ok(new { message = "המשתמש נוצר בהצלחה!", data = model });
        }
    }
}