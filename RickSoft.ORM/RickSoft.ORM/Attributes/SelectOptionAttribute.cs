using System;

namespace RickSoft.ORM.Engine.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SelectOptionAttribute : Attribute
    {
        public OrderingMode Order { get; private set; }
        public OptionType Option { get; private set; }

        public SelectOptionAttribute(OptionType selectionType, OrderingMode mode)
        {
            this.Order = mode;
            this.Option = selectionType;
        }
    }
}
