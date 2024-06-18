using System;

namespace Content.Code.Utility
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class TypeHintedAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeHintedPrefixAttribute : Attribute
    {
        public string prefix;

        public TypeHintedPrefixAttribute (string prefix)
        {
            this.prefix = prefix;
        }
    }
}