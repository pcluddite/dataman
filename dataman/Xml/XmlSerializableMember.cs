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
using System.Xml.Linq;
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
        bool IsReadOnly { get; }
        XObject Source { get; }

        string ElementName { get; }
        string AttributeName { get; }

        void SetValue(object instance, object value);
        object GetValue(object instance);
    }

    internal abstract class XmlSerializableMember<TMemberType, TAttribType> : IXmlSerializableMember
        where TMemberType : MemberInfo
        where TAttribType : XmlSerializableMemberAttribute, new()
    {
        public TMemberType Member { get; }
        public TAttribType Attribute { get; }
        public XObject Source { get; }

        public string Name => Attribute.Name ?? Member.Name;
        public string ElementName => Attribute.ElementName;
        public string AttributeName => Attribute.AttributeName;
        public bool SerializeDefault => Attribute.SerializeDefault;
        public object Default => Attribute.Default;
        public bool HasAttribute { get; }
        
        public abstract Type MemberType { get; }
        public abstract bool IsReadOnly { get; }

        protected XmlSerializableMember(XObject source, TMemberType member)
            : this(source, member, member.DeclaringType.GetXmlSerializableClassAttribute())
        {
        }

        protected XmlSerializableMember(XObject source, TMemberType member, XmlSerializableClassAttribute xmlSerializableClassAttribute)
        {
            Source = source;
            TAttribType attrib = (TAttribType)member.GetXmlSerializableMemberAttribute();
            HasAttribute = attrib != null;
            if (attrib == null)
            {
                HasAttribute = false;
                attrib = new TAttribType()
                {
                    Name = member.Name,
                    Default = member.GetReturnType().CreateDefault(),
                };
            }
            Attribute = attrib;
            if (xmlSerializableClassAttribute?.OverrideMemberOptions == true)
                Attribute.SerializeDefault = xmlSerializableClassAttribute.SerializeDefault;
            Member = member;
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

        public override bool IsReadOnly => Member.IsLiteral || Member.IsInitOnly;

        public XmlSerializableField(XObject source, FieldInfo field)
            : base(source, field)
        {
        }

        public XmlSerializableField(XObject source, FieldInfo field, XmlSerializableClassAttribute classAttribute)
            : base(source, field, classAttribute)
        {
        }

        public override void SetValue(object instance, object value)
        {
            if (IsReadOnly) throw new ReadOnlyMemberException(Source, Member);
            if (value == null && Default == null && MemberType.IsValueType)
                Attribute.Default = Activator.CreateInstance(MemberType);
            Member.SetValue(instance, value);
        }

        public override object GetValue(object instance)
        {
            if (IsReadOnly) throw new ReadOnlyMemberException(Source, Member);
            object value = Member.GetValue(instance);
            if (value == null && SerializeDefault && Default == null && MemberType.IsValueType)
                return Activator.CreateInstance(MemberType);
            return value;
        }
    }

    internal class XmlSerializableProperty : XmlSerializableMember<PropertyInfo, XmlSerializablePropertyAttribute>, IXmlSerializableMember
    {
        private readonly Lazy<FieldInfo> _backingField;

        public override Type MemberType
        {
            get
            {
                if (BackingField == null)
                    return Member.PropertyType;
                return BackingField.FieldType;
            }
        }

        public FieldInfo BackingField => _backingField.Value;

        public override bool IsReadOnly
        {
            get
            {
                if (BackingField == null)
                    return !Member.CanWrite;
                return BackingField.IsLiteral || BackingField.IsInitOnly;
            }
        }

        public XmlSerializableProperty(XObject source, PropertyInfo property)
            : base(source, property)
        {
            _backingField = new Lazy<FieldInfo>(FindBackingFieldField);
        }

        public XmlSerializableProperty(XObject source, PropertyInfo property, XmlSerializableClassAttribute classAttribute)
            : base(source, property, classAttribute)
        {
            _backingField = new Lazy<FieldInfo>(FindBackingFieldField);
        }

        public override object GetValue(object instance)
        {
            if (BackingField == null)
            {
                if (IsReadOnly) throw new ReadOnlyMemberException(Source, Member);
                return Member.GetValue(instance);
            }

            if (IsReadOnly) throw new ReadOnlyMemberException(Source, BackingField);
            object value = BackingField.GetValue(instance);
            if (value == null && SerializeDefault && Default == null && MemberType.IsValueType)
                return Activator.CreateInstance(MemberType);

            return value;
        }

        public override void SetValue(object instance, object value)
        {
            if (value == null && SerializeDefault && Default == null && MemberType.IsValueType)
                value = Activator.CreateInstance(MemberType);
            if (BackingField == null)
            {
                if (IsReadOnly) throw new ReadOnlyMemberException(Source, Member);
                Member.SetValue(instance, value);
            }
            else
            {
                if (IsReadOnly) throw new ReadOnlyMemberException(Source, BackingField);
                BackingField.SetValue(instance, value);
            }
        }

        private FieldInfo FindBackingFieldField()
        {
            if (Attribute.BackingField == null)
                return null;
            FieldInfo field = Member.DeclaringType.GetField(Attribute.BackingField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new FieldNotFoundException(Source, MemberType, Name);
            return field;
        }

        MemberInfo IXmlSerializableMember.Member
        {
            get
            {
                return (MemberInfo)BackingField ?? Member;
            }
        }
    }
}
