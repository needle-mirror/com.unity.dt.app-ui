using System;

namespace Unity.AppUI.UI
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    class EnumNameAttribute : Attribute
    {
        public Type enumType { get; }
        
        public string Name { get; set; }
        
        public EnumNameAttribute(string Name, Type enumType)
        {
            this.enumType = enumType;
        }
    }
}
