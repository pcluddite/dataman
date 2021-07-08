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
using System.Xml.Linq;
using System.Reflection;
using System.Linq;
using Baxendale.DataManagement.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Baxendale.DataManagement.Collections;
using System.Runtime.InteropServices;

namespace Baxendale.DataManagement.Xml
{
    public static class XmlExtensions
    {
        public static T Value<T>(this XAttribute attr) where T : IConvertible
        {
            return (T)Convert.ChangeType(attr.Value, typeof(T));
        }

        public static T Value<T>(this XAttribute attr, T @default) where T : IConvertible
        {
            if (attr == null)
                return @default;
            T value;
            if (attr.TryValue(out value))
                return value;
            return @default;
        }

        public static bool TryValue<T>(this XAttribute attr, out T value) where T : IConvertible
        {
            try
            {
                value = attr.Value<T>();
                return true;
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidCastException || ex is FormatException))
                    throw;
                value = default(T);
                return false;
            }
        }

        internal static XmlSerializableClassAttribute GetXmlSerializableClassAttribute(this Type type)
        {
            return type.GetCustomAttributes<XmlSerializableClassAttribute>(inherit: true).FirstOrDefault();
        }

        internal static XmlSerializableFieldAttribute GetXmlSerializableFieldAttribute(this FieldInfo field)
        {
            return field.GetCustomAttributes<XmlSerializableFieldAttribute>(inherit: true).FirstOrDefault();
        }

        internal static XmlSerializablePropertyAttribute GetXmlSerializablePropertyAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttributes<XmlSerializablePropertyAttribute>(inherit: true).FirstOrDefault();
        }

        internal static XmlSerializableMemberAttribute GetXmlSerializableMemberAttribute(this MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (member.MemberType == MemberTypes.Field)
                return GetXmlSerializableFieldAttribute((FieldInfo)member);
            if (member.MemberType == MemberTypes.Property)
                return GetXmlSerializablePropertyAttribute((PropertyInfo)member);
            throw new ArgumentException();
        }

        internal static IEnumerable<XmlSerializableField> GetSerializableFields(this Type type)
        {
            return GetSerializableFields(type, type.GetXmlSerializableClassAttribute(), Collections<FieldInfo>.EmptyCollection);
        }

        internal static IEnumerable<XmlSerializableField> GetSerializableFields(this Type type, XmlSerializableClassAttribute classAttribute)
        {
            return GetSerializableFields(type, classAttribute, Collections<FieldInfo>.EmptyCollection);
        }

        internal static IEnumerable<XmlSerializableField> GetSerializableFields(this Type type, ICollection<FieldInfo> excluded)
        {
            return GetSerializableFields(type, type.GetXmlSerializableClassAttribute(), excluded);
        }

        internal static IEnumerable<XmlSerializableField> GetSerializableFields(this Type type, XmlSerializableClassAttribute classAttribute, ICollection<FieldInfo> excluded)
        {
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                    continue; // skip anything compiler generated
                if (fieldInfo.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                    continue; // skip fields tagged with this attribute
                XmlSerializableField field = new XmlSerializableField(fieldInfo, classAttribute);
                if (!field.HasAttribute && classAttribute?.AllFields == false)
                    continue; // skip fields that do not have this attribute if not serializing all fields
                yield return field;
            }
        }

        internal static IEnumerable<XmlSerializableProperty> GetSerializableProperties(this Type type)
        {
            return GetSerializableProperties(type, type.GetXmlSerializableClassAttribute());
        }

        internal static IEnumerable<XmlSerializableProperty> GetSerializableProperties(this Type type, XmlSerializableClassAttribute classAttribute)
        {
            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (propertyInfo.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                    continue; // skip anything compiler generated
                if (propertyInfo.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                    continue; // skip properties tagged with this attribute
                XmlSerializableProperty prop = new XmlSerializableProperty(propertyInfo, classAttribute);
                if (!prop.HasAttribute && classAttribute?.AllProperties == false)
                    continue; // skip properties that do not have this attribute if not serializing all properties
                yield return prop;
            }
        }
    }
}
