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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            CustomClassAttribute = typeof(V).GetClassAttribute();
        }

        public override V Deserialize(XElement content)
        {
            MethodInfo fromXml = DeserializeMethod;
            if (fromXml == null)
            {
                if (typeof(V).IsValueType)
                    throw new UnsupportedTypeException(typeof(V), "Value types need an explicit deserialization method defined");
                return DefaultDeserialize(content);
            }
            return (V)fromXml.Invoke(null, new object[] { content });
        }

        private V DefaultDeserialize(XElement content)
        {
            V obj = Activator.CreateInstance<V>();
            HashSet<FieldInfo> backingFields = new HashSet<FieldInfo>();
            foreach (PropertyInfo propertyInfo in typeof(V).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (propertyInfo.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                    continue; // skip anything compiler generated
                if (propertyInfo.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                    continue; // skip properties tagged with this attribute
                if (!CustomClassAttribute.AllProperties && propertyInfo.GetCustomAttribute<XmlSerializablePropertyAttribute>(inherit: true) == null)
                    continue; // skip properties that do not have this attribute if not serializing all properties

                XmlSerializablePropertyAttribute attrib = propertyInfo.GetPropertyAttribute();

                if (attrib.BackingField == null)
                {
                    if (propertyInfo.SetMethod == null)
                        throw new ReadOnlyFieldException(propertyInfo);
                    propertyInfo.SetValue(obj, XmlSerializer.Deserialize(propertyInfo, content));
                }
                else
                {
                    FieldInfo backingField = typeof(V).GetField(attrib.BackingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (backingField == null)
                        throw new FieldNotFoundException(typeof(V), attrib.BackingField);
                    if (backingField.IsLiteral || backingField.IsInitOnly)
                        throw new ReadOnlyFieldException(backingField);
                    backingField.SetValue(obj, XmlSerializer.Deserialize(backingField, content));
                    backingFields.Add(backingField);
                }
            }

            foreach (FieldInfo fieldInfo in typeof(V).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                    continue; // skip anything compiler generated
                if (fieldInfo.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                    continue; // skip fields tagged with this attribute
                if (!CustomClassAttribute.AllFields && fieldInfo.GetCustomAttribute<XmlSerializablePropertyAttribute>(inherit: true) == null)
                    continue; // skip properties that do not have this attribute if not serializing all properties
                if (backingFields.Contains(fieldInfo))
                    continue; // skip fields that act as backing fields
                if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
                    throw new ReadOnlyFieldException(fieldInfo);
                fieldInfo.SetValue(obj, XmlSerializer.Deserialize(fieldInfo, content));
            }

            return obj;
        }

        public override XElement Serialize(V obj, XName name)
        {
            MethodInfo toXmlMethod = SerializeMethod;
            if (toXmlMethod == null)
                return DefaultSerialize(obj, name);
            return (XElement)toXmlMethod.Invoke(obj, new object[] { name });
        }

        private XElement DefaultSerialize(V obj, XName name)
        {
            XElement content = new XElement(name);
            HashSet<FieldInfo> backingFields = new HashSet<FieldInfo>();
            foreach (PropertyInfo propertyInfo in typeof(V).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (propertyInfo.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                    continue; // skip anything compiler generated
                if (propertyInfo.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                    continue; // skip properties tagged with this attribute
                if (!CustomClassAttribute.AllProperties && propertyInfo.GetCustomAttribute<XmlSerializablePropertyAttribute>(inherit: true) == null)
                    continue; // skip properties that do not have this attribute if not serializing all properties

                XmlSerializablePropertyAttribute attrib = propertyInfo.GetPropertyAttribute();
                if (CustomClassAttribute.OverrideMemberOptions)
                    attrib.SerializeDefault = CustomClassAttribute.SerializeDefault;

                if (attrib.BackingField == null)
                {
                    if (propertyInfo.SetMethod == null)
                        throw new ReadOnlyFieldException(propertyInfo);
                    XObject propertyContent = XmlSerializer.Serialize(propertyInfo, obj);
                    if (propertyContent != null)
                        content.Add(propertyContent);
                }
                else
                {
                    FieldInfo backingField = typeof(V).GetField(attrib.BackingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (backingField == null)
                        throw new FieldNotFoundException(typeof(V), attrib.BackingField);
                    if (backingField.IsLiteral || backingField.IsInitOnly)
                        throw new ReadOnlyFieldException(backingField);
                    XObject fieldContent = XmlSerializer.Serialize(backingField, obj);
                    if (fieldContent != null)
                        content.Add(fieldContent);
                    backingFields.Add(backingField);
                }
            }

            foreach (FieldInfo fieldInfo in typeof(V).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                    continue; // skip anything compiler generated
                if (fieldInfo.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                    continue; // skip fields tagged with this attribute
                if (!CustomClassAttribute.AllFields && fieldInfo.GetCustomAttribute<XmlSerializablePropertyAttribute>(inherit: true) == null)
                    continue; // skip properties that do not have this attribute if not serializing all properties
                if (backingFields.Contains(fieldInfo))
                    continue; // skip fields that act as backing fields
                if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly)
                    throw new ReadOnlyFieldException(fieldInfo);
                XmlSerializableFieldAttribute attrib = fieldInfo.GetFieldAttribute();
                if (CustomClassAttribute.OverrideMemberOptions)
                    attrib.SerializeDefault = CustomClassAttribute.SerializeDefault;
                XObject fieldContent = XmlSerializer.Serialize(fieldInfo, obj, serializeDefault: attrib.SerializeDefault);
                if (fieldContent != null)
                    content.Add(fieldContent);
            }

            return content;
        }
    }
}
