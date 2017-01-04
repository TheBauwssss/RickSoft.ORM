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

            List<User> results = Database.Get<User>(u => u.Email == "nep@nepper.com");
            

            Console.ReadKey();
        }
    }
}
