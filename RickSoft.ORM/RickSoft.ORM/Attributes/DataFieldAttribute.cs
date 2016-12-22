using System;

namespace RickSoft.ORM.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataFieldAttribute : Attribute
    {
        public string Column { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public ColumnOption Options { get; private set; }

        public DataFieldAttribute(string column, ColumnOption options)
        {
            this.Column = column;
            this.Options = options;
        }

        public DataFieldAttribute(string column, bool isPrimaryKey = false)
        {
            this.Column = column;
            this.IsPrimaryKey = isPrimaryKey;
        }
    }
}