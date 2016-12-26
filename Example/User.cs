using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Model;

namespace Example
{
    [DataTable("user")]
    public class User : DatabaseObject
    {

        [DataField("user_id", true)]
        public int Id { get; set; }

        [DataField("username", ColumnOption.Unique | ColumnOption.NotNull)]
        public string Username { get; set; }

        [DataField("password", ColumnOption.NotNull)]
        public string Password { get; set; }

        [DataField("email", ColumnOption.NotNull)]
        public string Email { get; set; }

        [DataField("last_login")]
        public DateTime LastLogin { get; set; }

    }
}