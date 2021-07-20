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

namespace Baxendale.Data.Xml
{
    internal struct XmlCustomTypeMetaData : IEquatable<XmlCustomTypeMetaData>
    {
        private Lazy<IXObjectSerializer> _objSerializer;

        public Type Type { get; }
        public IXObjectSerializer ObjectSerializer => _objSerializer.Value;
        public XName Name { get; }

        public XmlCustomTypeMetaData(Type t, XName name, IXObjectSerializer objSerializer)
        {
            Type = t;
            _objSerializer = new Lazy<IXObjectSerializer>(() => objSerializer);
            Name = name;
        }

        public XmlCustomTypeMetaData(XName name)
        {
            Type = null;
            _objSerializer = null;
            Name = name;
        }

        private XmlCustomTypeMetaData(Type t, XName name, Lazy<IXObjectSerializer> objSerializer)
        {
            Type = t;
            _objSerializer = objSerializer;
            Name = name;
        }

        public static XmlCustomTypeMetaData CreateMetaData<T>(XmlSerializer serializer, XName name)
            where T : IXmlSerializableObject
        {
            Lazy<IXObjectSerializer> objSerializer = new Lazy<IXObjectSerializer>(() => new XmlCustomObjectSerializer<T>(serializer));
            return new XmlCustomTypeMetaData(typeof(T), name, objSerializer);
        }

        public static XmlCustomTypeMetaData CreateMetaData<T>(XmlSerializer serializer, XName name, ToXElement<T> toXml, FromXElement<T> fromXml)
        {
            Lazy<IXObjectSerializer> objSerializer = new Lazy<IXObjectSerializer>(() => new XmlCustomXElementSerializer<T>(serializer, toXml, fromXml));
            return new XmlCustomTypeMetaData(typeof(T), name, objSerializer);
        }

        public static XmlCustomTypeMetaData CreateMetaData<T>(XmlSerializer serializer, XName name, ToXAttribute<T> toXml, FromXAttribute<T> fromXml)
        {
            Lazy<IXObjectSerializer> objSerializer = new Lazy<IXObjectSerializer>(() => new XmlCustomXAttributeSerializer<T>(serializer, toXml, fromXml));
            return new XmlCustomTypeMetaData(typeof(T), name, objSerializer);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            XmlCustomTypeMetaData? xtype = obj as XmlCustomTypeMetaData?;
            if (obj == null) return false;
            return Equals(xtype);
        }

        public bool Equals(XmlCustomTypeMetaData other)
        {
            return other.Name == Name;
        }
    }
}
