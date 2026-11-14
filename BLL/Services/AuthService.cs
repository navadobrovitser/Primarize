
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;

namespace BLL.Services
{
    public class AuthService
    {
        private readonly IMemoryCache _cache;
        private readonly EmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;

        private const int OtpExpiryMinutes = 5;
        private const int MaxOtpAttempts = 5;

        public AuthService(
            IMemoryCache cache,
            EmailService emailService,
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher)
        {
            _cache = cache;
            _emailService = emailService;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        // שלב ראשון: אימות תעודת זהות וסיסמה ושליחת קוד למייל
        public bool ValidateCredentialsAndSendOtp(
            string idNumber,
            string password)
        {
            if (string.IsNullOrWhiteSpace(idNumber) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            // חיפוש המשתמש לפי תעודת זהות
            var user = _userRepository.GetByIdNumber(idNumber);

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return false;
            }

            // אימות סיסמה שנשמרה כ-Hash
            PasswordVerificationResult verificationResult;

            try
            {
                verificationResult =
                    _passwordHasher.VerifyHashedPassword(
                        user,
                        user.Password,
                        password
                    );
            }
            catch (FormatException)
            {
                // סיסמה ישנה שלא נשמרה בפורמט Hash תקין
                return false;
            }

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                return false;
            }

            // יצירת קוד אקראי מאובטח בן 6 ספרות
            string code =
                RandomNumberGenerator.GetInt32(
                    100000,
                    1000000
                ).ToString();

            string cacheKey = $"OTP_{idNumber}";

            var otpData = new OtpData
            {
                Code = code,
                Attempts = 0
            };

            // שמירת הקוד למשך 5 דקות
            _cache.Set(
                cacheKey,
                otpData,
                TimeSpan.FromMinutes(OtpExpiryMinutes)
            );

            string subject = "קוד אימות להתחברות - תנופה";

            string body =
                $"שלום,\n\n" +
                $"קוד האימות שלך להתחברות לאתר תנופה הוא:\n" +
                $"{code}\n\n" +
                $"הקוד תקף ל-{OtpExpiryMinutes} דקות בלבד.\n" +
                $"אם לא ביקשת להתחבר, אפשר להתעלם מהודעה זו.";

            // שליחת הקוד למייל הרשום
            _emailService.SendEmail(
                user.Email,
                subject,
                body
            );

            return true;
        }

        // שלב שני: בדיקת קוד האימות והפקת טוקן
        public AuthResponseDto? VerifyOtpAndLogin(
            string idNumber,
            string enteredCode)
        {
            if (string.IsNullOrWhiteSpace(idNumber) ||
                string.IsNullOrWhiteSpace(enteredCode))
            {
                return null;
            }

            string cacheKey = $"OTP_{idNumber}";

            if (!_cache.TryGetValue(
                cacheKey,
                out OtpData? otpData))
            {
                return null;
            }

            if (otpData == null)
            {
                return null;
            }

            // חסימת הקוד אחרי מספר ניסיונות שגויים
            if (otpData.Attempts >= MaxOtpAttempts)
            {
                _cache.Remove(cacheKey);
                return null;
            }

            if (otpData.Code != enteredCode)
            {
                otpData.Attempts++;

                if (otpData.Attempts >= MaxOtpAttempts)
                {
                    _cache.Remove(cacheKey);
                }

                return null;
            }

            // קוד תקין - מוחקים כדי שלא יהיה שימוש חוזר
            _cache.Remove(cacheKey);

            var user = _userRepository.GetByIdNumber(idNumber);

            if (user == null)
            {
                return null;
            }

            // יצירת JWT
            return _tokenService.CreateToken(user);
        }

        // נתוני קוד האימות בזיכרון
        private class OtpData
        {
            public string Code { get; set; } = string.Empty;
            public int Attempts { get; set; }
        }
    }
}
