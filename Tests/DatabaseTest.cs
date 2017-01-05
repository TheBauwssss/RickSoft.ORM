using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RickSoft.ORM.Engine;
using RickSoft.ORM.Engine.Controller;
using Tests.DataModel;
using Tests.Tools;

namespace Tests
{
    [TestClass]
    public class DatabaseTest
    {
        [TestInitialize]
        public void SetupDatabase()
        {
            Database.CreateInstance(new DatabaseConfig());
            DatabaseBuilder.Create(new User(), true);
        }

        [TestCleanup]
        public void DestroyDatabase()
        {
            
        }

        [TestMethod]
        public void TestModelStorage()
        {
            List<User> users = new List<User>();

            for (int i = 0; i < 5; i++)
            {
                User u = new User();
                u.Email = $"{i}@domain.com";
                u.Username = $"User{i}";
                u.LastLogin = DateTime.Now.AddDays(-i);
                u.LastLogin = u.LastLogin.Round(TimeSpan.FromSeconds(1));
                u.Password = $"Password{i}";
                Database.Insert(ref u);

                Assert.AreNotEqual(u.Id, 0); //check if id was updated

                users.Add(u);
            }

            List<User> retrieved = Database.GetAll<User>();

            Assert.AreEqual(users.Count, retrieved.Count);

            foreach (User ret in retrieved)
            {
                bool match = false;

                foreach (User original in users)
                {
                    if (ret.Equals(original))
                    {
                        match = true;
                        break;
                    }

                    
                }
                Assert.IsTrue(match);
            }

            User temp = Database.Get<User>(o => o.Id == 2);
            Assert.IsNotNull(temp);
            Assert.AreEqual(temp.Id, 2);

            int userId = 3;
            temp = Database.Get<User>(o => o.Id == userId);
            Assert.IsNotNull(temp);
            Assert.AreEqual(temp.Id, 3);

        }

        [TestMethod]
        public void Benchmark()
        {
            //reset db
            DatabaseBuilder.Create(new User(), true);

            int num = 250;

            int percent = 0;

            Stopwatch s = new Stopwatch();
            s.Start();
            for (int i = 0; i < num; i++)
            {
                User u = new User();
                u.Email = $"{i}@domain.com";
                u.Username = $"User{i}";
                u.LastLogin = DateTime.Now.AddDays(-i);
                u.LastLogin = u.LastLogin.Round(TimeSpan.FromSeconds(1));
                u.Password = $"Password{i}";
                Database.Insert(ref u);

                if ((100.0/num)*i%10 > percent)
                {
                    percent++;
                    //Console.WriteLine($"Process is currently at {percent*10}%");
                }
            }
            s.Stop();

            double speed = s.ElapsedMilliseconds/num;

            Console.WriteLine("Benchmarking single insertions...");
            Console.WriteLine($"Benchmark complete, {num} insertions in {s.ElapsedMilliseconds}ms, avg {speed}ms per insertion");

            DatabaseBuilder.Create(new User(), true);

            num = 1000;

            s.Restart();
            List<User> users = new List<User>();
            for (int i = 0; i < num; i++)
            {
                User u = new User();
                u.Email = $"{i}@domain.com";
                u.Username = $"User{i}";
                u.LastLogin = DateTime.Now.AddDays(-5);
                u.LastLogin = u.LastLogin.Round(TimeSpan.FromSeconds(1));
                u.Password = $"Password{i}";
                
                users.Add(u);
            }

            Console.WriteLine("Benchmarking bulk insertion...");
            Database.InsertAll(ref users);
            s.Stop();
            speed = s.ElapsedMilliseconds/num;
            Console.WriteLine($"Benchmark complete, {num} insertions in {s.ElapsedMilliseconds}ms, avg {speed}ms per insertion");


            


        }
    }
}
