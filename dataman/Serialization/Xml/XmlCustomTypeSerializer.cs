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
using System.Xml.Linq;

namespace Baxendale.Data.Xml
{
    internal class XmlCustomXElementSerializer<T> : XObjectSerializer<T, XElement>
    {
        public override bool UsesXAttribute => false;

        public ToXElement<T> ToXml { get; }
        public FromXElement<T> FromXml { get; }

        public XmlCustomXElementSerializer(XmlSerializer serializer, ToXElement<T> toXml, FromXElement<T> fromXml)
            : base(serializer)
        {
            ToXml = toXml;
            FromXml = fromXml;
        }

        public override T Deserialize(XElement content)
        {
            return FromXml(XmlSerializer, content);
        }

        public override XElement Serialize(T obj, XName contentName)
        {
            return ToXml(XmlSerializer, obj, contentName);
        }
    }

    internal class XmlCustomXAttributeSerializer<T> : XObjectSerializer<T, XAttribute>
    {
        public override bool UsesXAttribute => true;

        public ToXAttribute<T> ToXml { get; }
        public FromXAttribute<T> FromXml { get; }

        public XmlCustomXAttributeSerializer(XmlSerializer serializer, ToXAttribute<T> toXml, FromXAttribute<T> fromXml)
            : base(serializer)
        {
            ToXml = toXml;
            FromXml = fromXml;
        }

        public override T Deserialize(XAttribute content)
        {
            return FromXml(XmlSerializer, content);
        }

        public override XAttribute Serialize(T obj, XName contentName)
        {
            return ToXml(XmlSerializer, obj, contentName);
        }
    }
}
