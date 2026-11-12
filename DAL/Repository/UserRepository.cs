using DAL.Entities;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
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

        public User? GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == id);
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

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

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User? GetByIdNumber(string idNumber)
        {
            var allUsers = _context.Users.ToList();

            var matchingUsers = allUsers
                .Where(u => u.Id != null &&
                            u.Id.Trim() == idNumber.Trim())
                .ToList();

            return matchingUsers.FirstOrDefault();
        }
        //public User? GetByIdNumber(string idNumber)
        //{
        //    return _context.Users.FirstOrDefault(u => u.Id == idNumber);
        //}

        public User? GetByEmailAndPassword(string email, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
        }
        public User? GetOwned(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
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