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
using System.Runtime.InteropServices;
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    internal interface IXmlSerializableMember
    {
        MemberInfo Member { get; }
        XmlSerializableMemberAttribute Attribute { get; }
        Type MemberType { get; }
        string Name { get; }
        bool SerializeDefault { get; }
        object Default { get; }
        bool HasAttribute { get;}

        void SetValue(object instance, object value);
        object GetValue(object instance);
    }

    internal abstract class XmlSerializableMember<TMemberType, TAttribType> : IXmlSerializableMember
        where TMemberType : MemberInfo
        where TAttribType : XmlSerializableMemberAttribute, new()
    {
        public TMemberType Member { get; }
        public TAttribType Attribute { get; }

        public string Name => Attribute.Name;
        public bool SerializeDefault => Attribute.SerializeDefault;
        public object Default => Attribute.Default;
        public bool HasAttribute { get; }

        public abstract Type MemberType { get; }

        protected XmlSerializableMember(TMemberType member)
            : this(member, member.DeclaringType.GetXmlSerializableClassAttribute())
        {
            Member = member;
        }

        protected XmlSerializableMember(TMemberType memberType, XmlSerializableClassAttribute xmlSerializableClassAttribute)
        {
            TAttribType attrib = (TAttribType)memberType.GetXmlSerializableMemberAttribute();
            HasAttribute = attrib != null;
            if (attrib == null)
            {
                HasAttribute = false;
                attrib = new TAttribType()
                {
                    Name = memberType.Name,
                    Default = memberType.GetReturnType().CreateDefault(),
                };
            }
            Attribute = attrib;
            if (xmlSerializableClassAttribute?.OverrideMemberOptions == true)
                Attribute.SerializeDefault = xmlSerializableClassAttribute.SerializeDefault;
            Member = memberType;
        }

        MemberInfo IXmlSerializableMember.Member
        {
            get
            {
                return Member;
            }
        }

        XmlSerializableMemberAttribute IXmlSerializableMember.Attribute
        {
            get
            {
                return Attribute;
            }
        }

        public abstract void SetValue(object instance, object value);
        public abstract object GetValue(object instance);
    }

    internal class XmlSerializableField : XmlSerializableMember<FieldInfo, XmlSerializableFieldAttribute>
    {
        public override Type MemberType => Member.FieldType;

        public XmlSerializableField(FieldInfo field)
            : base(field)
        {
        }

        public XmlSerializableField(FieldInfo field, XmlSerializableClassAttribute classAttribute)
            : base(field, classAttribute)
        {
        }

        public override void SetValue(object instance, object value)
        {
            if (Member.IsLiteral || Member.IsInitOnly)
                throw new ReadOnlyFieldException(Member);
            if (value == null && Default == null && MemberType.IsValueType)
                Attribute.Default = Activator.CreateInstance(MemberType);
            Member.SetValue(instance, value);
        }

        public override object GetValue(object instance)
        {
            object value = Member.GetValue(instance);
            if (value == null && SerializeDefault && Default == null && MemberType.IsValueType)
                return Activator.CreateInstance(MemberType);
            return value;
        }
    }

    internal class XmlSerializableProperty : XmlSerializableMember<PropertyInfo, XmlSerializablePropertyAttribute>
    {
        public override Type MemberType
        {
            get
            {
                FieldInfo backingField = BackingField;
                if (backingField == null)
                    return Member.PropertyType;
                return backingField.FieldType;
            }
        }

        public FieldInfo BackingField
        {
            get
            {
                if (Attribute.BackingField == null)
                    return null;
                FieldInfo backingField = Member.DeclaringType.GetField(Attribute.BackingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (backingField == null)
                    throw new FieldNotFoundException(Member.DeclaringType, Attribute.BackingField);
                if (backingField.IsLiteral || backingField.IsInitOnly)
                    throw new ReadOnlyFieldException(backingField);
                return backingField;
            }
        }

        public XmlSerializableProperty(PropertyInfo property)
            : base(property)
        {
        }

        public XmlSerializableProperty(PropertyInfo property, XmlSerializableClassAttribute classAttribute)
            : base(property, classAttribute)
        {
        }

        public override object GetValue(object instance)
        {
            FieldInfo backingField = BackingField;
            if (backingField == null)
            {
                if (Member.SetMethod == null)
                    throw new ReadOnlyFieldException(Member);
                return Member.GetValue(instance);
            }
            if (backingField.IsLiteral || backingField.IsInitOnly)
                throw new ReadOnlyFieldException(Member);
            object value = backingField.GetValue(instance);
            if (value == null && SerializeDefault && Default == null && MemberType.IsValueType)
                return Activator.CreateInstance(MemberType);
            return value;
        }

        public override void SetValue(object instance, object value)
        {
            FieldInfo backingField = BackingField;
            if (value == null && SerializeDefault && Default == null && MemberType.IsValueType)
                value = Activator.CreateInstance(MemberType);
            if (backingField == null)
            {
                if (Member.SetMethod == null)
                    throw new ReadOnlyFieldException(Member);
                Member.SetValue(instance, value);
            }
            else
            {
                if (backingField.IsLiteral || backingField.IsInitOnly)
                    throw new ReadOnlyFieldException(Member);
                backingField.SetValue(instance, value);
            }
        }
    }
}
