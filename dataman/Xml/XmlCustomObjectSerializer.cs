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
    internal class XmlCustomObjectSerializer<V> : XObjectSerializer<V, XObject>
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

        public XmlCustomObjectSerializer(XmlSerializer serializer)
            : base(serializer)
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
                    throw new XmlSerializationException(content, $"Cannot deserialize {typeof(V).Name} from given XObject type.");
                return DefaultDeserialize((XElement)content);
            }
            if (deserializeMethod.GetParameters().Length == 1)
                return (V)deserializeMethod.Invoke(null, new object[] { content });
            return (V)deserializeMethod.Invoke(null, new object[] { XmlSerializer, content });
        }

        private V DefaultDeserialize(XElement content)
        {
            V obj = (V)Activator.CreateInstance(typeof(V), nonPublic: true);
            HashSet<FieldInfo> backingFields = new HashSet<FieldInfo>();

            foreach (XmlSerializableProperty property in typeof(V).GetSerializableProperties(content, CustomClassAttribute))
            {
                FieldInfo backingField = property.BackingField;
                if (backingField != null)
                    backingFields.Add(backingField);
                property.SetValue(obj, DeserializeMember(property, content));
            }

            foreach (XmlSerializableField field in typeof(V).GetSerializableFields(content, CustomClassAttribute, backingFields))
            {
                field.SetValue(obj, DeserializeMember(field, content));
            }

            return obj;
        }

        private object DeserializeMember(IXmlSerializableMember member, XElement content)
        {
            IXObjectSerializer serializer = XmlSerializer.CreateSerializerObject(member.MemberType);
            serializer.ElementName = member.ElementName;
            serializer.ValueAttributeName = member.AttributeName;

            XObject memberContent;
            if (serializer.UsesXAttribute)
            {
                memberContent = content.Attribute(member.Name);
            }
            else
            {
                memberContent = content.Element(member.Name);
            }

            object value = null;
            if (memberContent != null)
                value = serializer.Deserialize(memberContent);
            if (value == null)
            {
                if (member.Default == null && member.MemberType.IsValueType)
                    member.Attribute.Default = Activator.CreateInstance(member.MemberType);
                value = member.Default;
            }
            return value;
        }

        public override XObject Serialize(V obj, XName name)
        {
            MethodInfo serializeMethod = SerializeMethod ?? FindSerializeMethod<XElement>(SerializeMethodName);
            if (serializeMethod == null)
                return DefaultSerialize(obj, name);
            if (serializeMethod.GetParameters().Length == 1)
                return (XObject)serializeMethod.Invoke(obj, new object[] { name });
            return (XObject)serializeMethod.Invoke(obj, new object[] { XmlSerializer, name });
        }

        private XElement DefaultSerialize(V obj, XName name)
        {
            XElement content = new XElement(name);
            HashSet<FieldInfo> backingFields = new HashSet<FieldInfo>();

            foreach (XmlSerializableProperty property in typeof(V).GetSerializableProperties(content, CustomClassAttribute))
            {
                FieldInfo backingField = property.BackingField;
                if (backingField != null)
                    backingFields.Add(backingField);
                content.Add(SerializeMember(obj, property));
            }

            foreach (XmlSerializableField field in typeof(V).GetSerializableFields(content, CustomClassAttribute, backingFields))
            {
                XObject fieldContent = SerializeMember(obj, field);
                if (fieldContent != null)
                    content.Add(fieldContent);
            }

            return content;
        }

        private XObject SerializeMember(V obj, IXmlSerializableMember member)
        {
            IXObjectSerializer serializer = XmlSerializer.CreateSerializerObject(member.MemberType);
            serializer.ElementName = member.ElementName;
            serializer.ValueAttributeName = member.AttributeName;
            return serializer.Serialize(member.GetValue(obj), member.Name);
        }

        private static MethodInfo FindDeserializeMethod<XType>(string methodName)
            where XType : XObject
        {
            IEnumerable<MethodInfo> fromMethods = from method in typeof(V).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                  let parameters = method.GetParameters()
                                                  where methodName == method.Name
                                                        && ((parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(XType))) || (parameters.Length == 2 && parameters[0].ParameterType == typeof(XmlSerializer) && parameters[1].ParameterType.IsAssignableFrom(typeof(XType))))
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
                                                       && ((parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(XName))) || (parameters.Length == 2 && parameters[0].ParameterType == typeof(XmlSerializer) && parameters[1].ParameterType.IsAssignableFrom(typeof(XName))))
                                                       && typeof(XType).IsAssignableFrom(method.ReturnType)
                                                select method;
            return toMethods.OrderBy(x => x, new MethodInfoComparer()).FirstOrDefault();
        }
    }
}