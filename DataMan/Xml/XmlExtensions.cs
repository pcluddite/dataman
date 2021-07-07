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

        internal static XmlSerializableClassAttribute GetClassAttribute(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            XmlSerializableClassAttribute attr = type.GetCustomAttributes<XmlSerializableClassAttribute>(inherit: true).FirstOrDefault();
            return attr ?? new XmlSerializableClassAttribute() { OverrideMemberOptions = false };
        }

        internal static XmlSerializableFieldAttribute GetFieldAttribute(this FieldInfo field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            XmlSerializableFieldAttribute attr = field.GetCustomAttributes<XmlSerializableFieldAttribute>(inherit: true).FirstOrDefault();
            return attr ?? new XmlSerializableFieldAttribute() { Name = field.Name, Default = field.FieldType.CreateDefault() };
        }

        internal static XmlSerializablePropertyAttribute GetPropertyAttribute(this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            XmlSerializablePropertyAttribute attr = property.GetCustomAttributes<XmlSerializablePropertyAttribute>(inherit: true).FirstOrDefault();
            return attr ?? new XmlSerializablePropertyAttribute() { Name = property.Name, Default = property.PropertyType.CreateDefault() };
        }


        internal static XmlSerializableMemberAttribute GetMemberAttribute(this MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (member.MemberType == MemberTypes.Field)
                return GetFieldAttribute((FieldInfo)member);
            if (member.MemberType == MemberTypes.Property)
                return GetPropertyAttribute((PropertyInfo)member);
            throw new ArgumentException();
        }
    }
}
