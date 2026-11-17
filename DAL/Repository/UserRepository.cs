
using DAL.Entities;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // שליפה לפי UserId - כולל משתמשים מחוקים
        public User? GetById(int id)
        {
            return _context.Users
                .FirstOrDefault(u => u.UserId == id);
        }

        // כל המשתמשים, כולל היסטוריים
        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // שימור ההתנהגות הקיימת של Delete
        public void Delete(string idNumber)
        {
            var user = GetByIdNumber(idNumber);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // רק משתמשים שלא נמחקו
        public User? GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            string normalizedEmail =
                email.Trim().ToLowerInvariant();

            return _context.Users
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    u.Email != null &&
                    u.Email.Trim().ToLower() == normalizedEmail);
        }

        // רק משתמשים שלא נמחקו
        public User? GetByIdNumber(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return null;

            string normalizedId = idNumber.Trim();

            return _context.Users
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    u.Id != null &&
                    u.Id.Trim() == normalizedId);
        }

        // נשמרת החתימה הקיימת
        public User? GetByEmailAndPassword(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            string normalizedEmail =
                email.Trim().ToLowerInvariant();

            return _context.Users
                .FirstOrDefault(u =>
                    !u.IsDeleted &&
                    u.Email.Trim().ToLower() == normalizedEmail &&
                    u.Password == password);
        }

        public User? GetOwned(int userId)
        {
            return _context.Users
                .FirstOrDefault(u =>
                    u.UserId == userId &&
                    !u.IsDeleted);
        }

        public User? GetPartyLeader()
        {
            return _context.Users
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.Role == "Admin" &&
                    !u.IsDeleted);
        }
    }
}
