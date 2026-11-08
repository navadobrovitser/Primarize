using BLL.IServices;
using DAL.IRepository;
using DTO.Security;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
namespace BLL.Services
{
    
        public class AuthService
        {
            private readonly IMemoryCache _cache;
            private readonly EmailService _emailService;
            private readonly IUserRepository _userRepository;
            private readonly ITokenService _tokenService;

            public AuthService(
                IMemoryCache cache,
                EmailService emailService,
                IUserRepository userRepository,
                ITokenService tokenService)
            {
                _cache = cache;
                _emailService = emailService;
                _userRepository = userRepository;
                _tokenService = tokenService;
            }

            // שלב 1: אימות ת"ז וסיסמה ויצירת/שליחת קוד סינכרוני
            public bool ValidateCredentialsAndSendOtp(string idNumber, string password)
            {
                // שליפה סינכרונית ממסד הנתונים
                var user = _userRepository.GetByIdNumber(idNumber);

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    return false;
                }

                // בדיקת סיסמה (התאם לפי אופן שמירת הסיסמאות אצלך)
                if (password != user.Password)
                {
                    return false;
                }

                // יצירת קוד אקראי ושמירה בזיכרון המטמון ל-5 דקות
                var code = Random.Shared.Next(100000, 999999).ToString();
                var cacheKey = $"OTP_{idNumber}";

                _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));

                // שליחת מייל סינכרונית
                string subject = "קוד אימות זמני להתחברות";
                string body = $"שלום,\n\nקוד האימות שלך להתחברות הוא: {code}\nהקוד תקף ל-5 דקות בלבד.";

                _emailService.SendEmail(user.Email, subject, body);
                return true;
            }

            // שלב 2: אימות הקוד הסינכרוני והפקת הטוקן
            public AuthResponseDto? VerifyOtpAndLogin(string idNumber, string enteredCode)
            {
                var cacheKey = $"OTP_{idNumber}";

                if (_cache.TryGetValue(cacheKey, out string? storedCode))
                {
                    if (storedCode == enteredCode)
                    {
                        _cache.Remove(cacheKey);

                        var user = _userRepository.GetByIdNumber(idNumber);
                        if (user != null)
                        {
                            return _tokenService.CreateToken(user);
                        }
                    }
                }

                return null;
            }
        }
    }

