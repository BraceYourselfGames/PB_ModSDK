using System;


namespace YamlDotNet.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YamlForce : Attribute
    {
    }
}
