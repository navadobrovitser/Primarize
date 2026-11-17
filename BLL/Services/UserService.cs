
using AutoMapper;
using BLL.Exceptions;
using BLL.IServices;
using DAL.Entities;
using DAL.IRepository;
using DTO.Security;
using DTO.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokens;
        private readonly IPasswordHasher<User> _hasher;
        private readonly EmailService _emailService;
        private readonly PasswordSetupTokenService _passwordSetupTokens;
        private readonly IConfiguration _configuration;

        private static readonly TimeSpan SetupLinkLifetime =
            TimeSpan.FromHours(24);

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ITokenService tokens,
            IPasswordHasher<User> hasher,
            EmailService emailService,
            PasswordSetupTokenService passwordSetupTokens,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tokens = tokens;
            _hasher = hasher;
            _emailService = emailService;
            _passwordSetupTokens = passwordSetupTokens;
            _configuration = configuration;
        }

        // ==========================================
        // בדיקות כפילות - רק משתמשים שלא נמחקו
        // ==========================================

        private User? FindActiveUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            string normalizedEmail =
                email.Trim().ToLowerInvariant();

            return _userRepository.GetAll()
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    !string.IsNullOrWhiteSpace(u.Email) &&
                    string.Equals(
                        u.Email.Trim(),
                        normalizedEmail,
                        StringComparison.OrdinalIgnoreCase));
        }

        private User? FindActiveUserByIdNumber(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return null;

            string normalizedId = idNumber.Trim();

            return _userRepository.GetAll()
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    u.Id != null &&
                    u.Id.Trim() == normalizedId);
        }

        // ==========================================
        // שליפת משתמשים
        // ==========================================

        public IEnumerable<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public UserDto GetUserById(string id)
        {
            var user = FindActiveUserByIdNumber(id);

            if (user == null)
            {
                throw new NotFoundException(
                    "המשתמש לא נמצא.");
            }

            return _mapper.Map<UserDto>(user);
        }

        // ==========================================
        // עדכון משתמש
        // ==========================================

        public void UpdateUser(
            int currentUserId,
            RegisterUserDto model)
        {
            var user =
                _userRepository.GetOwned(currentUserId);

            if (user == null || user.IsDeleted)
            {
                throw new NotFoundException(
                    "המשתמש לא נמצא.");
            }

            // בדיקה אם המייל החדש שייך למשתמש אחר
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var existingEmail =
                    FindActiveUserByEmail(model.Email);

                if (existingEmail != null &&
                    existingEmail.UserId != user.UserId)
                {
                    throw new ConflictException(
                        "כתובת האימייל כבר רשומה במערכת.");
                }
            }

            // בדיקה אם תעודת הזהות שייכת למשתמש אחר
            if (!string.IsNullOrWhiteSpace(model.Id))
            {
                var existingId =
                    FindActiveUserByIdNumber(model.Id);

                if (existingId != null &&
                    existingId.UserId != user.UserId)
                {
                    throw new ConflictException(
                        "תעודת הזהות כבר רשומה במערכת.");
                }
            }

            _mapper.Map(model, user);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email =
                    user.Email.Trim().ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = user.Id.Trim();
            }

            _userRepository.Update(user);
        }

        // ==========================================
        // מחיקת משתמש מחובר - מחיקה לוגית
        // ==========================================

        public void DeleteUser(int currentUserId)
        {
            var user =
                _userRepository.GetOwned(currentUserId);

            if (user == null)
            {
                throw new NotFoundException(
                    "המשתמש לא נמצא.");
            }

            if (user.Role == "Admin")
            {
                throw new BusinessRuleException(
                    "לא ניתן להסיר חשבון מנהל.");
            }

            if (user.IsDeleted)
            {
                throw new BusinessRuleException(
                    "המשתמש כבר הוסר מהמערכת.");
            }

            user.IsDeleted = true;

            // ביטול קישורי סיסמה ישנים
            user.PasswordSetupTokenId = null;
            user.PasswordSetupLinkSentAt = null;

            _userRepository.Update(user);
        }

        // ==========================================
        // הרשמת משתמש חדש
        // ==========================================

        public bool Register(RegisterUserDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Id) ||
                string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BusinessRuleException(
                    "חובה למלא את פרטי ההרשמה.");
            }

            string email =
                dto.Email.Trim().ToLowerInvariant();

            string idNumber = dto.Id.Trim();

            // בדיקת תעודת זהות בת 9 ספרות
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                idNumber, @"^\d{9}$"))
            {
                throw new BusinessRuleException(
                    "תעודת הזהות חייבת להכיל 9 ספרות.");
            }

            // ======================================
            // חדש: משתמש שנמחק אינו חוסם הרשמה
            // ======================================

            var existingEmailUser = _userRepository.GetAll()
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    string.Equals(
                        u.Email.Trim(),
                        email,
                        StringComparison.OrdinalIgnoreCase));

            if (existingEmailUser != null)
            {
                throw new ConflictException(
                    "כתובת המייל כבר רשומה במערכת. יש להזין כתובת אחרת.");
            }

            var existingIdUser = _userRepository.GetAll()
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    u.Id.Trim() == idNumber);

            if (existingIdUser != null)
            {
                throw new ConflictException(
                    "תעודת הזהות כבר רשומה במערכת. לא ניתן להירשם פעמיים.");
            }




            // יצירת חשבון חדש עם UserId חדש
            var user = new User
            {
                Id = idNumber,
                Name = dto.Name.Trim(),
                Email = email,
                Phone = dto.Phone,
                Gender = dto.Gender,
                Role = "User",
                PartyJoinDate = DateTime.UtcNow,
                IsApproved = false,
                IsDeleted = false,
                Password = string.Empty,
                PasswordSetupLinkSentAt = null,
                PasswordSetupTokenId = null
            };

            _userRepository.Add(user);

            return true;
        }

        // ==========================================
        // התחברות--לא שמיש
        // ==========================================

        public AuthResponseDto Login(LoginDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new BusinessRuleException(
                    "אימייל או סיסמה שגויים.");
            }

            string email =
                dto.Email.Trim().ToLowerInvariant();

            // מחפשים רק חשבון שלא נמחק
            var user = FindActiveUserByEmail(email);

            if (user == null ||
                !user.IsApproved ||
                user.IsDeleted ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                throw new BusinessRuleException(
                    "אימייל או סיסמה שגויים.");
            }

            PasswordVerificationResult result;

            try
            {
                result = _hasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    dto.Password);
            }
            catch (FormatException)
            {
                throw new BusinessRuleException(
                    "אימייל או סיסמה שגויים.");
            }

            if (result == PasswordVerificationResult.Failed)
            {
                throw new BusinessRuleException(
                    "אימייל או סיסמה שגויים.");
            }

            return _tokens.CreateToken(user);
        }

        // ==========================================
        // משתמשים שממתינים לאישור
        // ==========================================

        public IEnumerable<UserDto> GetPendingUsers()
        {
            var users = _userRepository.GetAll()
                .Where(u =>
                    u.Role != "Admin" &&
                    !u.IsApproved &&
                    !u.IsDeleted)
                .ToList();

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        // ==========================================
        // משתמשים שממתינים להגדרת סיסמה
        // ==========================================

        public IEnumerable<UserDto> GetUsersWaitingForPassword()
        {
            var users = _userRepository.GetAll()
                .Where(u =>
                    u.Role != "Admin" &&
                    u.IsApproved &&
                    !u.IsDeleted &&
                    string.IsNullOrEmpty(u.Password))
                .ToList();

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        // ==========================================
        // שליחת קישור להגדרת סיסמה
        // ==========================================

        private void SendPasswordSetupLink(User user)
        {
            Guid tokenId = Guid.NewGuid();

            string token = _passwordSetupTokens.GenerateToken(
                user.UserId,
                user.Email,
                tokenId);
            //מייצר קישור לריאקט
            string frontendUrl =
                _configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Frontend:BaseUrl is missing");
            //מייצר קישור לריאקט באמת עם טוקן

            string setupLink =
                $"{frontendUrl.TrimEnd('/')}/set-password" +
                $"?token={Uri.EscapeDataString(token)}";

            string subject = "תנופה - הגדרת סיסמה";

            string body = $@"שלום {user.Name},

בקשת ההצטרפות שלך למערכת תנופה אושרה!

כדי להשלים את ההרשמה,
יש להגדיר סיסמה באמצעות הקישור הבא:

{setupLink}

הקישור תקף למשך 24 שעות.

בברכה,
צוות תנופה";

            // קודם שומרים את פרטי הקישור במסד
            user.PasswordSetupTokenId = tokenId;
            user.PasswordSetupLinkSentAt = DateTime.UtcNow;

            _userRepository.Update(user);

            // לאחר מכן שולחים מייל
            _emailService.SendEmail(
                user.Email,
                subject,
                body);
        }

        // ==========================================
        // אישור משתמש על ידי מנהל
        // ==========================================

        public bool ApproveUser(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.Role == "Admin" ||
                user.IsApproved ||
                user.IsDeleted)
            {
                return false;
            }

            user.PartyJoinDate = DateTime.UtcNow;
            user.IsApproved = true;

            _userRepository.Update(user);

            SendPasswordSetupLink(user);

            return true;
        }

        // ==========================================
        // שליחה חוזרת של קישור לאחר 24 שעות
        // ==========================================

        public bool ResendPasswordSetupLink(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.Role == "Admin" ||
                !user.IsApproved ||
                user.IsDeleted ||
                !string.IsNullOrEmpty(user.Password))
            {
                return false;
            }

            if (user.PasswordSetupLinkSentAt.HasValue)
            {
                DateTime expiresAt =
                    user.PasswordSetupLinkSentAt.Value
                        .Add(SetupLinkLifetime);

                if (DateTime.UtcNow < expiresAt)
                {
                    return false;
                }
            }

            SendPasswordSetupLink(user);

            return true;
        }

        // ==========================================
        // מחיקת בקשת הרשמה
        // ==========================================

        public bool DeleteRegistrationRequest(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.Role == "Admin" ||
                user.IsDeleted)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                return false;
            }

            // מחיקה לוגית כדי לשמור היסטוריה
            user.IsDeleted = true;
            user.PasswordSetupTokenId = null;
            user.PasswordSetupLinkSentAt = null;

            _userRepository.Update(user);

            return true;
        }

        // ==========================================
        // הגדרת סיסמה ראשונית
        // ==========================================

        public bool SetPassword(SetPasswordDto dto)
        {
            if (dto == null)
                return false;

            bool valid = _passwordSetupTokens.TryValidateToken(
                dto.Token,
                out int userId,
                out string email,
                out Guid tokenId);

            if (!valid)
                return false;

            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.IsDeleted ||
                !user.IsApproved)
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

            if (!user.PasswordSetupTokenId.HasValue ||
                user.PasswordSetupTokenId.Value != tokenId)
            {
                return false;
            }

            // סיסמה ראשונית בלבד
            if (!string.IsNullOrEmpty(user.Password))
            {
                return false;
            }

            if (!user.PasswordSetupLinkSentAt.HasValue)
            {
                return false;
            }

            DateTime sentAt =
                user.PasswordSetupLinkSentAt.Value;

            if (sentAt > DateTime.UtcNow ||
                DateTime.UtcNow >=
                    sentAt.Add(SetupLinkLifetime))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.Password) ||
                dto.Password.Length < 8)
            {
                return false;
            }

            user.Password = _hasher.HashPassword(
                user,
                dto.Password);

            user.PasswordSetupTokenId = null;
            user.PasswordSetupLinkSentAt = null;

            _userRepository.Update(user);

            return true;
        }

        // ==========================================
        // מחיקה לוגית על ידי מנהל
        // ==========================================

        public bool SoftDeleteUser(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.Role == "Admin" ||
                user.IsDeleted)
            {
                return false;
            }

            user.IsDeleted = true;

            user.PasswordSetupTokenId = null;
            user.PasswordSetupLinkSentAt = null;

            _userRepository.Update(user);

            return true;
        }

        // ==========================================
        // שחזור משתמש מחוק
        // ==========================================

        public bool RestoreUser(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.Role == "Admin" ||
                !user.IsDeleted)
            {
                return false;
            }

            // אין לשחזר אם המייל כבר בשימוש
            var existingEmail =
                FindActiveUserByEmail(user.Email);

            if (existingEmail != null &&
                existingEmail.UserId != user.UserId)
            {
                throw new ConflictException(
                    "לא ניתן לשחזר את המשתמש. " +
                    "כתובת המייל כבר שייכת לחשבון אחר.");
            }

            // אין לשחזר אם תעודת הזהות כבר בשימוש
            var existingId =
                FindActiveUserByIdNumber(user.Id);

            if (existingId != null &&
                existingId.UserId != user.UserId)
            {
                throw new ConflictException(
                    "לא ניתן לשחזר את המשתמש. " +
                    "תעודת הזהות כבר שייכת לחשבון אחר.");
            }

            user.IsDeleted = false;

            _userRepository.Update(user);

            return true;
        }

        // ==========================================
        // מנהל המפלגה
        // ==========================================

        public UserDto? GetPartyLeader()
        {
            var leader = _userRepository.GetPartyLeader();

            if (leader == null)
                return null;

            return new UserDto
            {
                UserId = leader.UserId,
                Name = leader.Name
            };
        }
    }
}
