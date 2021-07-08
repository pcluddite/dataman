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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal class XmlCustomObjectSerializer<V> : XmlObjectSerializer<V, XElement>
        where V : IXmlSerializableObject
    {
        public XmlSerializableClassAttribute CustomClassAttribute { get; }
        public override bool UsesXAttribute => false;

        public string DeserializeMethodName
        {
            get
            {
                return CustomClassAttribute.DeserializeMethodName ?? "FromXml";
            }
        }

        public string SerializeMethodName
        {
            get
            {
                return CustomClassAttribute.SerializeMethodName ?? "ToXml";
            }
        }

        public MethodInfo DeserializeMethod
        {
            get
            {
                IEnumerable<MethodInfo> fromMethods = from method in typeof(V).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                      let parameters = method.GetParameters()
                                                      where DeserializeMethodName == method.Name
                                                            && parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(XElement))
                                                            && method.ReturnType.IsAssignableFrom(typeof(V))
                                                      select method;
                return fromMethods.OrderBy(x => x, new MethodInfoComparer()).FirstOrDefault();
            }
        }

        public MethodInfo SerializeMethod
        {
            get
            {
                IEnumerable<MethodInfo> toMethods = from method in typeof(V).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                    let parameters = method.GetParameters()
                                                    where SerializeMethodName == method.Name
                                                           && parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(XName))
                                                           && typeof(XObject).IsAssignableFrom(method.ReturnType)
                                                    select method;
                return toMethods.OrderBy(x => x, new MethodInfoComparer()).FirstOrDefault();
            }
        }

        public XmlCustomObjectSerializer()
        {
            CustomClassAttribute = typeof(V).GetXmlSerializableClassAttribute() ?? new XmlSerializableClassAttribute()
            {
                OverrideMemberOptions = false
            };
        }

        public override V Deserialize(XElement content)
        {
            MethodInfo fromXml = DeserializeMethod;
            if (fromXml != null)
                return (V)fromXml.Invoke(null, new object[] { content });

            if (typeof(V).IsValueType)
                throw new UnsupportedTypeException(typeof(V), "Value types need an explicit deserialization method defined");

            V obj = Activator.CreateInstance<V>();
            HashSet<FieldInfo> backingFields = new HashSet<FieldInfo>();
            DeserializeProperties(obj, content, backingFields);
            DeserializeFields(obj, content, backingFields);
            return obj;
        }

        private void DeserializeProperties(V obj, XElement content, HashSet<FieldInfo> backingFields)
        {
            foreach (XmlSerializableProperty property in typeof(V).GetSerializableProperties())
            {
                PropertyInfo propertyInfo = property.Member;

                if (property.BackingField == null)
                {
                    if (propertyInfo.SetMethod == null)
                        throw new ReadOnlyFieldException(propertyInfo);
                    propertyInfo.SetValue(obj, XmlSerializer.Deserialize(propertyInfo, content));
                }
                else
                {
                    FieldInfo backingField = typeof(V).GetField(property.BackingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (backingField == null)
                        throw new FieldNotFoundException(typeof(V), property.BackingField);
                    if (backingField.IsLiteral || backingField.IsInitOnly)
                        throw new ReadOnlyFieldException(backingField);
                    backingField.SetValue(obj, XmlSerializer.Deserialize(backingField, content));
                    backingFields.Add(backingField);
                }
            }
        }

        private void DeserializeFields(V obj, XElement content, HashSet<FieldInfo> backingFields)
        {
            foreach (XmlSerializableField field in typeof(V).GetSerializableFields(CustomClassAttribute, backingFields))
            {
                FieldInfo fieldInfo = field.Member;
                if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
                    throw new ReadOnlyFieldException(fieldInfo);
                fieldInfo.SetValue(obj, XmlSerializer.Deserialize(fieldInfo, content));
            }
        }

        public override XElement Serialize(V obj, XName name)
        {
            MethodInfo toXmlMethod = SerializeMethod;
            if (toXmlMethod != null)
                return (XElement)toXmlMethod.Invoke(obj, new object[] { name });
            XElement content = new XElement(name);
            HashSet<FieldInfo> backingFields = new HashSet<FieldInfo>();
            SerializeProperties(obj, content, backingFields);
            SerializeFields(obj, content, backingFields);
            return content;
        }

        private void SerializeProperties(V obj, XElement content, HashSet<FieldInfo> backingFields)
        {
            foreach (XmlSerializableProperty property in typeof(V).GetSerializableProperties())
            {
                PropertyInfo propertyInfo = property.Member;

                if (property.BackingField == null)
                {
                    if (propertyInfo.SetMethod == null)
                        throw new ReadOnlyFieldException(propertyInfo);

                    XObject propertyContent = SerializeMember(obj, propertyInfo, property.Attribute);
                    if (propertyContent != null)
                        content.Add(propertyContent);
                }
                else
                {
                    FieldInfo backingField = typeof(V).GetField(property.BackingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (backingField == null)
                        throw new FieldNotFoundException(typeof(V), property.BackingField);

                    if (backingField.IsLiteral || backingField.IsInitOnly)
                        throw new ReadOnlyFieldException(backingField);

                    XObject fieldContent = SerializeMember(obj, backingField, property.Attribute);
                    if (fieldContent != null)
                        content.Add(fieldContent);

                    backingFields.Add(backingField);
                }
            }
        }

        private void SerializeFields(V obj, XElement content, HashSet<FieldInfo> backingFields)
        {
            foreach (XmlSerializableField field in typeof(V).GetSerializableFields(CustomClassAttribute, backingFields))
            {
                FieldInfo fieldInfo = field.Member;

                if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
                    throw new ReadOnlyFieldException(fieldInfo);

                XObject fieldContent = SerializeMember(obj, fieldInfo, field.Attribute);
                if (fieldContent != null)
                    content.Add(fieldContent);
            }
        }

        private static XObject SerializeMember(V obj, MemberInfo member, XmlSerializableMemberAttribute memberAttribute)
        {
            Type memberType = member.GetReturnType();
            Type serializerType = XmlSerializer.GetObjectSerializerType(memberType);
            if (serializerType == null)
                throw new UnsupportedTypeException(memberType);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
            object value = member.GetValue(obj);
            if (memberType.IsValueType && memberAttribute.Default == null)
                memberAttribute.Default = Activator.CreateInstance(memberType);
            if (!memberAttribute.SerializeDefault && memberAttribute.Default == value)
                return null;
            return serializer.Serialize(value, memberAttribute.Name);
        }
    }
}
