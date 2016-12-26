using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickSoft.ORM.Engine.Model
{
    public abstract class DbConfig
    {
        public abstract string Server { get; }
        public abstract int Port { get; }
        public abstract string Database { get; }
        public abstract string Username { get; }
        public abstract string Password { get; }
        public abstract bool SafeMode { get; }

        public string ConnectionString
        {
            get { return $"SERVER={Server};PORT={Port};DATABASE={Database};UID={Username};PASSWORD={Password};"; }
        }
    }
}
