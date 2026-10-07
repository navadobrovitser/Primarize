using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.IRepository
{
    public interface IUserRepository
    {
        User? GetById(int id);              // 1. return object by id
        List<User> GetAll();                // 2. return list
        void Add(User user);                // 3. add
        void Delete(int id);                // 4. delete
        void Update(User user);             // 5. update

        // extra functions
        User? GetByEmail(string email);
        User? GetByEmailAndPassword(string email, string password);
    }
}
