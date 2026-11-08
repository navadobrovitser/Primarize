 using BLL.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

using DTO.Users;
     namespace API.Controllers
    {
   

    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login-step1")]
        public IActionResult LoginStep1([FromBody] LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdNumber) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "חובה להזין תעודת זהות וסיסמה" });

            var success = _authService.ValidateCredentialsAndSendOtp(dto.IdNumber, dto.Password);

            if (!success)
            {
                return Unauthorized(new { message = "תעודת הזהות או הסיסמה שגויים" });
            }

            return Ok(new { message = "הסיסמה אומתה בהצלחה, קוד אימות נשלח לכתובת המייל שלך" });
        }

        [HttpPost("login-step2-verify")]
        public IActionResult LoginStep2Verify([FromBody] VerifyOtpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdNumber) || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "חובה להזין תעודת זהות וקוד אימות" });

            var authResponse = _authService.VerifyOtpAndLogin(dto.IdNumber, dto.Code);

            if (authResponse == null)
            {
                return Unauthorized(new { message = "קוד האימות שגוי או שפג תוקפו" });
            }

            return Ok(authResponse);
        }
    }
}
