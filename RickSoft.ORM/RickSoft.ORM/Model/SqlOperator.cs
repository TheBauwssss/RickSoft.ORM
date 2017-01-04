using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickSoft.ORM.Engine.Model
{
    public class SqlOperator
    {
        public string MainOperator { get; set; }
        public bool Invert { get; set; }
        public bool IsBoolean { get; set; }
    }
}
