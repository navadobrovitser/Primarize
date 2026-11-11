
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Text.Json;

namespace BLL.Services
{
    public class PasswordSetupTokenService
    {
        private readonly ITimeLimitedDataProtector _protector;

        private static readonly TimeSpan TokenLifetime =
            TimeSpan.FromHours(24);

        public PasswordSetupTokenService(
            IDataProtectionProvider provider)
        {
            _protector = provider
                .CreateProtector("Tenufa.PasswordSetup.v2")
                .ToTimeLimitedDataProtector();
        }

        // ==========================================
        // יצירת טוקן להגדרת סיסמה
        // ==========================================

        public string GenerateToken(
            int userId,
            string email,
            Guid tokenId)
        {
            if (userId <= 0 ||
                string.IsNullOrWhiteSpace(email) ||
                tokenId == Guid.Empty)
            {
                throw new ArgumentException(
                    "פרטי יצירת הטוקן אינם תקינים.");
            }

            var payload = new PasswordSetupPayload
            {
                UserId = userId,
                Email = email.Trim(),
                TokenId = tokenId
            };

            string json = JsonSerializer.Serialize(payload);

            return _protector.Protect(
                json,
                TokenLifetime);
        }

        // ==========================================
        // בדיקת תקינות הטוקן
        // ==========================================

        public bool TryValidateToken(
            string token,
            out int userId,
            out string email,
            out Guid tokenId)
        {
            userId = 0;
            email = string.Empty;
            tokenId = Guid.Empty;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                // פענוח הטוקן ובדיקת תוקפו
                string json = _protector.Unprotect(token);

                var payload =
                    JsonSerializer.Deserialize<PasswordSetupPayload>(
                        json);

                if (payload == null ||
                    payload.UserId <= 0 ||
                    string.IsNullOrWhiteSpace(payload.Email) ||
                    payload.TokenId == Guid.Empty)
                {
                    return false;
                }

                userId = payload.UserId;
                email = payload.Email;
                tokenId = payload.TokenId;

                return true;
            }
            catch (Exception)
            {
                // טוקן לא תקין, פג תוקף או לא ניתן לפענוח
                return false;
            }
        }

        // ==========================================
        // המידע שנשמר בתוך הטוקן
        // ==========================================

        public class PasswordSetupPayload
        {
            public int UserId { get; set; }

            public string Email { get; set; } = string.Empty;

            public Guid TokenId { get; set; }
        }
    }
}
