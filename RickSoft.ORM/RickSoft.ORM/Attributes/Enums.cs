using System;

namespace RickSoft.ORM.Engine.Attributes
{
   
    public enum OrderingMode
    {
        Descending = 0,
        Ascending = 1
    }

    public enum OptionType
    {
        OrderBy = 0
    }

    public enum DoNotDuplicateField
    {
        Field = 0,
        Key = 1
    }

    [Flags]
    public enum ColumnOption
    {
        None = 0,
        Unique = 1,
        NotNull = 2
    }
    
}
