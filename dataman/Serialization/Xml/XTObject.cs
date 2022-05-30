//
//    DataMan - Supplemental library for managing data types and handling serialization
//    Copyright (C) 2021-2022 Timothy Baxendale
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
using System.Xml;
using System.Xml.Linq;

namespace Baxendale.Serialization.Xml
{
    public abstract class XTObject : ISerializedObject
    {
        public abstract string BaseUri { get; }
        public abstract XmlNodeType NodeType { get; }
    }

    public abstract class XTObject<T> : XTObject
        where T : XObject
    {
        protected T _object;

        public override string BaseUri => _object?.BaseUri;
        public override XmlNodeType NodeType => _object?.NodeType ?? XmlNodeType.None;

        protected XTObject(T xobject)
        {
            _object = xobject;
        }
    }

    public class XTElement : XTObject<XElement>
    {
        public XElement Element => _object;

        public XTElement(XElement xelement)
            : base(xelement)
        {
        }

        public static implicit operator XTElement(XElement element)
        {
            return new XTElement(element);
        }

        public static implicit operator XElement(XTElement element)
        {
            return element?.Element;
        }
    }

    public class XTAttribute : XTObject<XAttribute>
    {
        public XAttribute Attribute => _object;

        public XTAttribute(XAttribute xattribute)
            : base(xattribute)
        {
        }

        public static implicit operator XTAttribute(XAttribute attribute)
        {
            return new XTAttribute(attribute);
        }

        public static implicit operator XAttribute(XTAttribute attribute)
        {
            return attribute?.Attribute;
        }
    }
}
