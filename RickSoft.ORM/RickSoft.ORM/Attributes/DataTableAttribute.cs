using System;

namespace RickSoft.ORM.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTableAttribute : Attribute
    {
        public string TableName { get; private set; }
        public string PrimaryKeyColumn { get; private set; }
        
        public DataTableAttribute(string tableName)
        {
            this.TableName = tableName;
            this.PrimaryKeyColumn = tableName + "_id";
        }
        
        public DataTableAttribute(string tableName, string primaryKeyColumn = null)
        {
            this.TableName = tableName;
            this.PrimaryKeyColumn = primaryKeyColumn;
        }
    }
}