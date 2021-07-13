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
using System.Reflection;
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    internal abstract class XmlSerializableMember<TMemberType, TAttribType>
        where TMemberType : MemberInfo
        where TAttribType : XmlSerializableMemberAttribute, new()
    {
        public TMemberType Member { get; }
        public TAttribType Attribute { get; }

        public string Name => Attribute.Name;
        public bool SerializeDefault => Attribute.SerializeDefault;
        public object Default => Attribute.Default;
        public bool HasAttribute { get; }

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
                    Default = memberType.GetReturnType().CreateDefault()
                };
            }
            Attribute = attrib;
            if (xmlSerializableClassAttribute?.OverrideMemberOptions == true)
                Attribute.SerializeDefault = xmlSerializableClassAttribute.SerializeDefault;
            Member = memberType;
        }
    }

    internal class XmlSerializableField : XmlSerializableMember<FieldInfo, XmlSerializableFieldAttribute>
    {
        public XmlSerializableField(FieldInfo field)
            : base(field)
        {
        }

        public XmlSerializableField(FieldInfo field, XmlSerializableClassAttribute classAttribute)
            : base(field, classAttribute)
        {
        }
    }

    internal class XmlSerializableProperty : XmlSerializableMember<PropertyInfo, XmlSerializablePropertyAttribute>
    {
        public string BackingField => Attribute.BackingField;

        public XmlSerializableProperty(PropertyInfo property)
            : base(property)
        {
        }

        public XmlSerializableProperty(PropertyInfo property, XmlSerializableClassAttribute classAttribute)
            : base(property, classAttribute)
        {
        }
    }
}
