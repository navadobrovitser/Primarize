
using BLL.Services;
using DTO.Security;
using DTO.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login-step1")]
        public IActionResult LoginStep1(
            [FromBody] LoginRequestDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.IdNumber) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    message = "חובה להזין תעודת זהות וסיסמה"
                });
            }

            bool success =
                _authService.ValidateCredentialsAndSendOtp(
                    dto.IdNumber,
                    dto.Password
                );

            if (!success)
            {
                return Unauthorized(new
                {
                    message = "תעודת הזהות או הסיסמה שגויים"
                });
            }

            return Ok(new
            {
                message = "קוד אימות נשלח לכתובת המייל שלך"
            });
        }

        [HttpPost("login-step2-verify")]
        public IActionResult LoginStep2Verify(
            [FromBody] VerifyOtpDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.IdNumber) ||
                string.IsNullOrWhiteSpace(dto.Code))
            {
                return BadRequest(new
                {
                    message = "חובה להזין קוד אימות"
                });
            }

            var result = _authService.VerifyOtpAndLogin(
                dto.IdNumber,
                dto.Code
            );

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "קוד האימות שגוי או שפג תוקפו"
                });
            }

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(
            [FromBody] ForgotPasswordDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.IdNumber) ||
                string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new
                {
                    message = "יש להזין תעודת זהות וכתובת מייל"
                });
            }

            if (!Regex.IsMatch(
                dto.IdNumber.Trim(),
                @"^\d{9}$"))
            {
                return BadRequest(new
                {
                    message = "תעודת הזהות חייבת להכיל 9 ספרות"
                });
            }

            try
            {
                _authService.RequestPasswordReset(
                    dto.IdNumber,
                    dto.Email
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send password reset email"
                );
            }

            // לא חושפים האם החשבון קיים
            return Ok(new
            {
                message =
                    "אם הפרטים תואמים לחשבון פעיל, " +
                    "יישלח קישור לאיפוס הסיסמה למייל."
            });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new
                {
                    message = "יש למלא את כל השדות"
                });
            }

            if (dto.NewPassword.Length < 8)
            {
                return BadRequest(new
                {
                    message = "הסיסמה חייבת להכיל לפחות 8 תווים"
                });
            }

            bool success = _authService.ResetPassword(
                dto.Token,
                dto.NewPassword
            );

            if (!success)
            {
                return BadRequest(new
                {
                    message =
                        "הקישור אינו תקין, פג תוקף " +
                        "או שכבר נעשה בו שימוש"
                });
            }

            return Ok(new
            {
                message = "הסיסמה עודכנה בהצלחה"
            });
        }
    }
}
