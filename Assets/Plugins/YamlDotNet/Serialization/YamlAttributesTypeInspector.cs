//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Applies the Yaml* attributes to another <see cref="ITypeInspector"/>.
    /// </summary>
    public sealed class YamlAttributesTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;
        private static Type stringType = typeof (string);

        public YamlAttributesTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return innerTypeDescriptor.GetProperties(type, container)
                .Where(p => p.GetCustomAttribute<YamlIgnoreAttribute>() == null)
                
                /*
                .Where(p =>
                {
                    if (p.GetCustomAttribute<YamlIgnoreIfDefaultAttribute> () == null)
                        return true;
                    
                    // If container is null, that means this method is being using on deserialization, where this attribute is irrelevant
                    if (container == null)
                        return true;

                    // Wrapping everything in a try/catch in case we hit a read exception
                    try
                    {
                        bool isValueType = p.Type.IsValueType;
                        var valueDescriptor = p.Read (container);
                        var valueType = valueDescriptor.Type;

                        if (valueType == stringType)
                        {
                            var valueCasted = (string)valueDescriptor.Value;
                            UnityEngine.Debug.Log ($"Node {p.Name} | Type: string | Value: {valueCasted}");
                            
                            if (string.IsNullOrEmpty (valueCasted))
                            {
                                UnityEngine.Debug.Log ($"Skipping value node {p.Name} with default value");
                                return false;
                            }
                            else
                                UnityEngine.Debug.Log ($"Value node {p.Name} has non-default value");
                        }
                        else
                        {
                            var valueCasted = Convert.ChangeType (valueDescriptor.Value, valueType);
                            UnityEngine.Debug.Log ($"Node {p.Name} | Non-object: {isValueType} | Type: {valueType} | Casted: {valueCasted}");
                    
                            if (isValueType)
                            {
                                if (valueCasted == default)
                                {
                                    UnityEngine.Debug.Log ($"Skipping value node {p.Name} with default value");
                                    return false;
                                }
                                else
                                    UnityEngine.Debug.Log ($"Value node {p.Name} has non-default value");
                            }
                            else
                            {
                                if (valueCasted == null)
                                {
                                    UnityEngine.Debug.Log ($"Skipping object node {p.Name} with null value");
                                    return false;
                                }
                                else
                                    UnityEngine.Debug.Log ($"Object node {p.Name} has non-null value");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogException (e);
                    }

                    return true;
                })
                */
                
                .Select(p =>
                {
                    var descriptor = new PropertyDescriptor(p);
                    
                    var member = p.GetCustomAttribute<YamlMemberAttribute>();
                    if (member != null)
                    {
                        if (member.SerializeAs != null)
                        {
                            descriptor.TypeOverride = member.SerializeAs;
                        }

                        descriptor.Order = member.Order;
                        descriptor.ScalarStyle = member.ScalarStyle;

                        if (member.Alias != null)
                        {
                            descriptor.Name = member.Alias;
                        }
                    }

                    return (IPropertyDescriptor)descriptor;
                })
                .OrderBy(p => p.Order);
        }
    }
}
