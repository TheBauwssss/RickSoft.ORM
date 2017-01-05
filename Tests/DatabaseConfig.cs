using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RickSoft.ORM.Engine.Model;

namespace Tests
{
    internal class DatabaseConfig : DbConfig
    {
        public override string Server
        {
            get { return "server2.rick-soft.com"; }
        }

        public override int Port
        {
            get { return 3306; }
        }

        public override string Database
        {
            get { return "orm_test"; }
        }

        public override string Username
        {
            get { return "orm_test"; }
        }

        public override string Password
        {
            get { return "?dTf5bwRRhkkN5UFPD!EsY6HAs#R36Um"; }
        }

        public override bool SafeMode
        {
            get { return false; }
        }
    }
}
