using System;

namespace RickSoft.ORM.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DoNotDuplicateAttribute : Attribute
    {
        public DoNotDuplicateField FieldType { get; private set; }

        public DoNotDuplicateAttribute(DoNotDuplicateField fieldType)
        {
            this.FieldType = fieldType;
        }
    }
}

