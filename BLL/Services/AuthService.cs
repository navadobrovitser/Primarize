
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BLL.Services
{
    public class AuthService
    {
        private readonly IMemoryCache _cache;
        private readonly EmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly PasswordResetTokenService _resetTokens;
        private readonly IConfiguration _configuration;

        private const int OtpExpiryMinutes = 5;
        private const int MaxOtpAttempts = 5;

        private static readonly TimeSpan ResetLifetime =
            TimeSpan.FromMinutes(30);

        public AuthService(
            IMemoryCache cache,
            EmailService emailService,
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            PasswordResetTokenService resetTokens,
            IConfiguration configuration)
        {
            _cache = cache;
            _emailService = emailService;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _resetTokens = resetTokens;
            _configuration = configuration;
        }

        // =====================================
        // התחברות - שלב ראשון
        // =====================================
        public bool ValidateCredentialsAndSendOtp(
            string idNumber,
            string password)
        {
            if (string.IsNullOrWhiteSpace(idNumber) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            idNumber = idNumber.Trim();

            var user =
                _userRepository.GetByIdNumber(idNumber);

            if (user == null ||
                !user.IsApproved ||
                user.IsDeleted ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return false;
            }

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
                return false;
            }

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                return false;
            }

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

            _cache.Set(
                cacheKey,
                otpData,
                TimeSpan.FromMinutes(OtpExpiryMinutes)
            );

            string subject =
                "קוד אימות להתחברות - תנופה";

            string body =
                $"שלום,\n\n" +
                $"קוד האימות שלך להתחברות לאתר תנופה הוא:\n" +
                $"{code}\n\n" +
                $"הקוד תקף ל-{OtpExpiryMinutes} דקות בלבד.\n" +
                $"אם לא ביקשת להתחבר, אפשר להתעלם מהודעה זו.";

            _emailService.SendEmail(
                user.Email,
                subject,
                body
            );

            return true;
        }

        // =====================================
        // התחברות - שלב שני
        // =====================================
        public AuthResponseDto? VerifyOtpAndLogin(
            string idNumber,
            string enteredCode)
        {
            if (string.IsNullOrWhiteSpace(idNumber) ||
                string.IsNullOrWhiteSpace(enteredCode))
            {
                return null;
            }

            idNumber = idNumber.Trim();

            string cacheKey = $"OTP_{idNumber}";

            if (!_cache.TryGetValue(
                cacheKey,
                out OtpData? otpData))
            {
                return null;
            }

            if (otpData == null)
                return null;

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

            _cache.Remove(cacheKey);

            var user =
                _userRepository.GetByIdNumber(idNumber);

            if (user == null ||
                !user.IsApproved ||
                user.IsDeleted)
            {
                return null;
            }

            return _tokenService.CreateToken(user);
        }

        // =====================================
        // שכחתי סיסמה - שליחת קישור
        // =====================================
        public void RequestPasswordReset(
            string idNumber,
            string email)
        {
            if (string.IsNullOrWhiteSpace(idNumber) ||
                string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            idNumber = idNumber.Trim();
            email = email.Trim();

            if (!Regex.IsMatch(idNumber, @"^\d{9}$"))
                return;

            string rateKey = $"RESET_RATE_{idNumber}";

            // הגבלת בקשות חוזרות
            if (_cache.TryGetValue(rateKey, out _))
                return;

            _cache.Set(
                rateKey,
                true,
                TimeSpan.FromMinutes(2)
            );

            var user =
                _userRepository.GetByIdNumber(idNumber);

            // בדיקת תעודת זהות + מייל
            if (user == null ||
                !string.Equals(
                    user.Email?.Trim(),
                    email,
                    StringComparison.OrdinalIgnoreCase) ||
                !user.IsApproved ||
                user.IsDeleted ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return;
            }

            string frontendUrl =
                _configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Frontend:BaseUrl is missing"
                );

            Guid tokenId = Guid.NewGuid();

            string token = _resetTokens.GenerateToken(
                user.UserId,
                user.Email,
                tokenId
            );

            string resetLink =
                $"{frontendUrl.TrimEnd('/')}/?resetToken=" +
                Uri.EscapeDataString(token);

            // שימוש בשדות שכבר קיימים במסד
            user.PasswordSetupTokenId = tokenId;
            user.PasswordSetupLinkSentAt = DateTime.UtcNow;

            _userRepository.Update(user);

            string subject = "תנופה - איפוס סיסמה";

            string body = $@"שלום {user.Name},

קיבלנו בקשה לאיפוס הסיסמה שלך במערכת תנופה.

כדי להגדיר סיסמה חדשה,
יש להיכנס לקישור הבא:

{resetLink}

הקישור תקף למשך 30 דקות.

אם לא ביקשת לאפס את הסיסמה,
אפשר להתעלם מהודעה זו.

בברכה,
צוות תנופה";

            _emailService.SendEmail(
                user.Email,
                subject,
                body
            );
        }

        // =====================================
        // שכחתי סיסמה - עדכון הסיסמה
        // =====================================
        public bool ResetPassword(
            string token,
            string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                newPassword.Length < 8)
            {
                return false;
            }

            bool valid = _resetTokens.TryValidateToken(
                token,
                out int userId,
                out string email,
                out Guid tokenId
            );

            if (!valid)
                return false;

            var user = _userRepository.GetById(userId);

            if (user == null ||
                !user.IsApproved ||
                user.IsDeleted ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return false;
            }

            if (!string.Equals(
                user.Email,
                email,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // רק הקישור האחרון תקף
            if (!user.PasswordSetupTokenId.HasValue ||
                user.PasswordSetupTokenId.Value != tokenId)
            {
                return false;
            }

            if (!user.PasswordSetupLinkSentAt.HasValue)
                return false;

            DateTime sentAt =
                user.PasswordSetupLinkSentAt.Value;

            if (sentAt > DateTime.UtcNow ||
                DateTime.UtcNow >=
                    sentAt.Add(ResetLifetime))
            {
                return false;
            }

            user.Password = _passwordHasher.HashPassword(
                user,
                newPassword
            );

            // ביטול הקישור
            user.PasswordSetupTokenId = null;
            user.PasswordSetupLinkSentAt = null;

            _userRepository.Update(user);

            // ביטול קוד התחברות קודם
            _cache.Remove($"OTP_{user.Id}");

            return true;
        }

        private class OtpData
        {
            public string Code { get; set; } =
                string.Empty;

            public int Attempts { get; set; }
        }
    }
}
