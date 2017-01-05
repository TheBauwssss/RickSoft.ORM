using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RickSoft.ORM.Engine.Model
{
    public class SqlWhereCondition
    {
        public SqlOperator Operator { get; set; }
        public string Column { get; set; }
        public object Value { get; set; }
        
        public void BindValue(ref MySqlCommand command)
        {
            command.Parameters.AddWithValue("@" + Column, Value);
        }

        public string GenerateWhereClause()
        {
            string where = "";

            if (Operator.IsBoolean)
                where = Column;
            else
               where = $"{Column} {Operator.MainOperator} @{Column}";

            if (Operator.Invert)
                where = $"NOT {where}";

            return where;
        }
    }
}
