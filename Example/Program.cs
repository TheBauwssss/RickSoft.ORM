using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RickSoft.ORM.Engine;
using RickSoft.ORM.Engine.Controller;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.CreateInstance(new DatabaseConfig());

            DatabaseBuilder.Create(new User(), true);

            var user = new User
            {
                Username = "User",
                Email = "nep@nepper.com",
                Password = "123456789",
                LastLogin = DateTime.Now.AddDays(10)
            };

            Database.Insert(ref user);

            user = new User
            {
                Username = "User2",
                Email = "nep2@nepper.com",
                Password = "13123213213",
                LastLogin = DateTime.Now.AddDays(4)
            };

            Database.Insert(ref user);

            User obj = Database.Get<User>(u => u.Email == "nep@nepper.com");

            int id = 1;

            obj = Database.Get<User>(u => u.Id == id);



            int num = 160;


            List<User> users = new List<User>();
            for (int i = 5; i < num; i++)
            {
                User u = new User();
                u.Email = $"{i}@domain.com";
                u.Username = $"User{i}";
                u.LastLogin = DateTime.Now.AddDays(-i);
                u.Password = $"Password{i}";
                
                users.Add(u);
            }

            Database.InsertAll(ref users);


            string sql = ";DROP user;--";

            var fdsfsdfs = Database.Get<User>(u => u.Username == sql);

            User moeder = new User();
            moeder.Username = sql;
            moeder.Email = sql;
            moeder.Password = sql;
            moeder.LastLogin = DateTime.Now;
            Database.Insert(ref moeder);

            Console.ReadKey();
        }
    }
}
