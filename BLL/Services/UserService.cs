
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
        // שליפת משתמשים
        // ==========================================

        public IEnumerable<UserDto> GetAllUsers()
        {
            var users = _userRepository.GetAll();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public UserDto GetUserById(string id)
        {
            var user = _userRepository.GetByIdNumber(id);

            if (user == null)
                throw new NotFoundException("המשתמש לא נמצא.");

            return _mapper.Map<UserDto>(user);
        }

        // ==========================================
        // עדכון ומחיקת משתמש
        // ==========================================

        public void UpdateUser(
            int currentUserId,
            RegisterUserDto model)
        {
            var user = _userRepository.GetOwned(currentUserId);

            if (user == null)
                throw new NotFoundException("המשתמש לא נמצא.");

            _mapper.Map(model, user);
            _userRepository.Update(user);
        }

        public void DeleteUser(int currentUserId)
        {
            var user = _userRepository.GetOwned(currentUserId);

            if (user == null)
                throw new NotFoundException("המשתמש לא נמצא.");

            if (user.Role == "Admin")
                throw new BusinessRuleException(
                    "לא ניתן להסיר חשבון מנהל.");

            if (user.IsDeleted)
                throw new BusinessRuleException(
                    "המשתמש כבר הוסר מהמערכת.");

            user.IsDeleted = true;

            _userRepository.Update(user);
        }

        // ==========================================
        // הרשמת משתמש חדש
        // ==========================================

        public bool Register(RegisterUserDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var idNumber = dto.Id.Trim();

            if (_userRepository.GetByEmail(email) != null)
            {
                throw new ConflictException(
                    "כתובת האימייל כבר רשומה במערכת.");
            }

            if (_userRepository.GetByIdNumber(idNumber) != null)
            {
                throw new ConflictException(
                    "תעודת הזהות כבר רשומה במערכת.");
            }

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
                Password = string.Empty,
                PasswordSetupLinkSentAt = null,
                PasswordSetupTokenId = null
            };

            _userRepository.Add(user);

            return true;
        }

        // ==========================================
        // התחברות
        // ==========================================

        public AuthResponseDto Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = _userRepository.GetByEmail(email);

            if (user == null ||
     !user.IsApproved ||
     user.IsDeleted ||
     string.IsNullOrEmpty(user.Password))
            {
                throw new BusinessRuleException(
                    "אימייל או סיסמה שגויים.");
            }
            var result = _hasher.VerifyHashedPassword(
                user,
                user.Password,
                dto.Password);

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
            // מזהה ייחודי לכל קישור
            Guid tokenId = Guid.NewGuid();

            // יצירת טוקן אישי ל-24 שעות
            string token = _passwordSetupTokens.GenerateToken(
                user.UserId,
                user.Email,
                tokenId);

            string frontendUrl =
                _configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Frontend:BaseUrl is missing");

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

            _emailService.SendEmail(
                user.Email,
                subject,
                body);

            // שמירת פרטי הקישור האחרון
            user.PasswordSetupTokenId = tokenId;
            user.PasswordSetupLinkSentAt = DateTime.UtcNow;
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

            // תאריך תחילת החברות הוא יום אישור המנהל
            user.PartyJoinDate = DateTime.UtcNow;

            // הפעלת החברות
            user.IsApproved = true;

            // שמירת האישור במסד הנתונים הקיים
            _userRepository.Update(user);

            // שליחת קישור להגדרת סיסמה
            // אם השליחה תיכשל, החברות עדיין מאושרת
            // וניתן יהיה לשלוח קישור מחדש
            SendPasswordSetupLink(user);

            return true;
        }

        // ==========================================
        // שליחה חוזרת לאחר 24 שעות
        // ==========================================

        public bool ResendPasswordSetupLink(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null ||
                user.Role == "Admin" ||
                !user.IsApproved || user.IsDeleted ||
                !string.IsNullOrEmpty(user.Password))
            {
                return false;
            }

            if (!user.PasswordSetupLinkSentAt.HasValue)
                return false;

            DateTime expiresAt =
                user.PasswordSetupLinkSentAt.Value
                    .Add(SetupLinkLifetime);

            if (DateTime.UtcNow < expiresAt)
                return false;

            SendPasswordSetupLink(user);

            _userRepository.Update(user);

            return true;
        }

        // ==========================================
        // מחיקת בקשת הרשמה
        // ==========================================

        public bool DeleteRegistrationRequest(int userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null || user.Role == "Admin")
                return false;

            if (!string.IsNullOrEmpty(user.Password))
                return false;

            _userRepository.Delete(user.Id);

            return true;
        }

        // ==========================================
        // הגדרת סיסמה
        // ==========================================

        public bool SetPassword(SetPasswordDto dto)
        {
            // בדיקת תקינות הטוקן
            bool valid = _passwordSetupTokens.TryValidateToken(
                dto.Token,
                out int userId,
                out string email,
                out Guid tokenId);

            if (!valid)
                return false;

            var user = _userRepository.GetById(userId);

            if (user == null)
                return false;
            if (user.IsDeleted)
                return false;

            // בדיקת התאמה בין הטוקן למשתמש
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

            if (!user.IsApproved)
                return false;

            // מניעת שימוש חוזר
            if (!string.IsNullOrEmpty(user.Password))
                return false;

            if (!user.PasswordSetupLinkSentAt.HasValue)
                return false;

            // בדיקת תוקף 24 שעות
            if (DateTime.UtcNow >=
                user.PasswordSetupLinkSentAt.Value
                    .Add(SetupLinkLifetime))
            {
                return false;
            }

            // בדיקת סיסמה
            if (string.IsNullOrWhiteSpace(dto.Password) ||
                dto.Password.Length < 8)
            {
                return false;
            }

            // הצפנת הסיסמה באמצעות Hash
            user.Password = _hasher.HashPassword(
                user,
                dto.Password);

            // ביטול הקישור לאחר שימוש
            user.PasswordSetupTokenId = null;
            user.PasswordSetupLinkSentAt = null;

            _userRepository.Update(user);

            return true;
        }
        // ==========================================
        // מחיקה לוגית של משתמש על ידי מנהל
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
            _userRepository.Update(user);

            return true;
        }

        // ==========================================
        // שחזור משתמש שהוסר
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

            user.IsDeleted = false;
            _userRepository.Update(user);

            return true;
        }

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

