using DAL.Entities;
using System.Collections.Generic;

namespace DAL.IRepository
{
    public interface IUserRepository
    {
        User? GetById(int id);
        List<User> GetAll();
        void Add(User user);
        void Delete(string idNumber);
        void Update(User user);
        User? GetByEmail(string email);
        User? GetByEmailAndPassword(string email, string password);
        public User? GetByIdNumber(string idNumber);
    }
}