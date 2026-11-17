
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Text.Json;

namespace BLL.Services
{//סרוויס לשינוי סיסמה במייל
    public class PasswordResetTokenService
    {
        private readonly ITimeLimitedDataProtector _protector;

        // תוקף קישור איפוס הסיסמה
        private static readonly TimeSpan Lifetime =
            TimeSpan.FromMinutes(30);

        // =====================================================
        // 1. אתחול מנגנון Data Protection
        //
        // יצירת מנגנון להגנה על טוקנים בעלי תוקף מוגבל.
        // שימוש ב-Purpose ייעודי לטוקנים של איפוס סיסמה.
        // =====================================================

        public PasswordResetTokenService(
            IDataProtectionProvider provider)
        {
            _protector = provider
                .CreateProtector("Tenufa.PasswordReset.v1")
                .ToTimeLimitedDataProtector();
        }

        // =====================================================
        // 2. יצירת טוקן לאיפוס סיסמה
        //
        // הגנה על פרטי המשתמש באמצעות Data Protection.
        // הטוקן תקף למשך 30 דקות מרגע יצירתו.
        // =====================================================

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
                    "פרטי הטוקן אינם תקינים.");
            }

            var payload = new ResetPayload
            {
                UserId = userId,
                Email = email.Trim(),
                TokenId = tokenId
            };

            return _protector.Protect(
                JsonSerializer.Serialize(payload),
                Lifetime
            );
        }

        // =====================================================
        // 3. אימות טוקן לאיפוס סיסמה
        //
        // פענוח הטוקן, בדיקת תקפותו ואימות מבנה הנתונים.
        // טוקן פג תוקף או לא תקין נדחה.
        // =====================================================

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
                var payload =
                    JsonSerializer.Deserialize<ResetPayload>(
                        _protector.Unprotect(token)
                    );

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
            catch
            {
                return false;
            }
        }

        // =====================================================
        // 4. מבנה הנתונים המוגן בטוקן
        //
        // שמירת מזהה המשתמש, כתובת המייל
        // ומזהה ייחודי של בקשת האיפוס.
        // =====================================================

        private class ResetPayload
        {
            public int UserId { get; set; }

            public string Email { get; set; } =
                string.Empty;

            public Guid TokenId { get; set; }
        }
    }
}
