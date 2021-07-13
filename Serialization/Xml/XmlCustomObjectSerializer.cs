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
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    internal class XmlCustomObjectSerializer<V> : XmlObjectSerializer<V, XObject>
        where V : IXmlSerializableObject
    {
        public MethodInfo DeserializeMethod { get; }
        public MethodInfo SerializeMethod { get; }
        public XmlSerializableClassAttribute CustomClassAttribute { get; }

        public override bool UsesXAttribute { get; }

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

        public XmlCustomObjectSerializer()
        {
            CustomClassAttribute = typeof(V).GetXmlSerializableClassAttribute() ?? new XmlSerializableClassAttribute()
            {
                OverrideMemberOptions = false
            };
            DeserializeMethod = FindDeserializeMethod<XAttribute>(DeserializeMethodName);
            SerializeMethod = FindSerializeMethod<XAttribute>(SerializeMethodName);
            if (DeserializeMethod != null && SerializeMethod != null)
            {
                UsesXAttribute = true;
            }
            else if (DeserializeMethod != null || SerializeMethod != null)
            {
                throw new UnsupportedTypeException(typeof(V), "A custom XAttribute class must define both a serializer and deserializer");
            }
        }

        public override V Deserialize(XObject content)
        {
            MethodInfo deserializeMethod = DeserializeMethod ?? FindDeserializeMethod<XElement>(DeserializeMethodName);
            if (deserializeMethod == null)
            {
                XElement xelementContent = content as XElement;
                if (xelementContent == null)
                    throw new UnsupportedTypeException(typeof(V), "Cannot deserialize from given XObject type.");
                return DefaultDeserialize((XElement)content);
            }
            return (V)deserializeMethod.Invoke(null, new[] { content });
        }

        private V DefaultDeserialize(XElement content)
        {
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

        public override XObject Serialize(V obj, XName name)
        {
            MethodInfo serializeMethod = DeserializeMethod ?? FindSerializeMethod<XElement>(SerializeMethodName);
            if (serializeMethod == null)
                return DefaultSerialize(obj, name);
            return (XObject)serializeMethod.Invoke(null, new[] { name });
        }

        private XElement DefaultSerialize(V obj, XName name)
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

        private static MethodInfo FindDeserializeMethod<XType>(string methodName)
            where XType : XObject
        {
            IEnumerable<MethodInfo> fromMethods = from method in typeof(V).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                  let parameters = method.GetParameters()
                                                  where methodName == method.Name
                                                        && parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(XType))
                                                        && method.ReturnType.IsAssignableFrom(typeof(V))
                                                  select method;
            return fromMethods.OrderBy(x => x, new MethodInfoComparer()).FirstOrDefault();
        }

        private static MethodInfo FindSerializeMethod<XType>(string methodName)
            where XType : XObject
        {
            IEnumerable<MethodInfo> toMethods = from method in typeof(V).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                let parameters = method.GetParameters()
                                                where methodName == method.Name
                                                       && parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(XName))
                                                       && typeof(XType).IsAssignableFrom(method.ReturnType)
                                                select method;
            return toMethods.OrderBy(x => x, new MethodInfoComparer()).FirstOrDefault();
        }
    }
}