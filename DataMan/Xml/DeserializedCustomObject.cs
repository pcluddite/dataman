//
//    DataMan - Supplemental library for managing data types and handling serialization
//    Copyright (C) 2021 Timothy Baxendale
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
//    USA
//
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedCustomObject(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedCustomObject<>).MakeGenericType(typeof(T), typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedCustomObject<V> : DeserializedXmlObject<V>
            where V : IXmlSerializableObject
        {
            public MethodInfo ToXmlMethod
            {
                get
                {
                    MethodInfo method;
                    try
                    {
                        method = typeof(V).GetMethod("ToXml", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(XName) }, null);
                    }
                    catch (AmbiguousMatchException)
                    {
                        try
                        {
                            method = typeof(V).GetMethod("ToXml", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(XName) }, null);
                        }
                        catch (AmbiguousMatchException)
                        {
                            method = null;
                        }
                    }
                    if (method == null)
                        return null;
                    if (!typeof(XObject).IsAssignableFrom(method.ReturnType))
                        return null;
                    return method;
                }
            }

            public DeserializedCustomObject(V obj, XName name)
                : base(obj, name)
            {
            }

            public override XObject Serialize()
            {
                MethodInfo toXmlMethod = ToXmlMethod;
                if (toXmlMethod == null)
                    return SerializeDefault();
                return (XObject)toXmlMethod.Invoke(DeserializedObject, new object[] { Name });
            }

            private XObject SerializeDefault()
            {
                XElement element = new XElement(Name);
                foreach (MemberInfo member in typeof(V).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (member.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length > 0)
                        continue; // skip anything compiler generated
                    if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                        continue; // skip anything that's not a field or property
                    if (member.GetCustomAttributes(typeof(XmlDoNotSerializeAttribute), true).Length > 0)
                        continue; // skip fields tagged with this attribute

                    Type memberType = member.GetReturnType();
                    XmlSerializeAttribute attrib = member.GetCustomAttribute<XmlSerializeAttribute>(inherit: true);
                    if (attrib == null)
                    {
                        if (attrib.Name == null)
                            attrib.Name = member.Name;
                    }
                    else
                    {
                        attrib = new XmlSerializeAttribute() { Name = member.Name };
                    }

                    object obj;
                    if (member.MemberType == MemberTypes.Field)
                    {
                        obj = ((FieldInfo)member).GetValue(DeserializedObject);
                    }
                    else
                    {
                        obj = ((PropertyInfo)member).GetGetMethod(nonPublic: true).Invoke(DeserializedObject, new object[0]);
                    }
                    
                    if (member.GetCustomAttribute<XmlSerializeNonDefaultAttribute>() == null || obj != memberType.CreateDefault())
                    {
                        IDeserializedXmlObject xmlObj = XmlSerializer.CreateDeserializedObject(memberType, obj, attrib);
                        element.Add(xmlObj.Serialize());
                    }
                }
                return element;
            }
        }
    }
}
