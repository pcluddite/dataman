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
    internal interface IXmlObjectSerializer
    {
        object Deserialize(XObject content);
        XObject Serialize(object obj, XName name);
        bool UsesXAttribute { get; }
    }

    internal interface IXmlObjectSerializer<T> : IXmlObjectSerializer
    {
        new T Deserialize(XObject content);
        XObject Serialize(T obj, XName name);
    }

    internal abstract class XmlObjectSerializer<T, XContentType> : IXmlObjectSerializer<T>
        where XContentType : XObject
    {
        public XmlSerializer XmlSerializer { get; }

        public XmlObjectSerializer(XmlSerializer serializer)
        {
            XmlSerializer = serializer;
        }

        public abstract bool UsesXAttribute { get; }

        public abstract T Deserialize(XContentType content);

        public abstract XContentType Serialize(T obj, XName name);

        #region ISerializedXmlObject Members

        object IXmlObjectSerializer.Deserialize(XObject content)
        {
            return Deserialize((XContentType)content);
        }

        T IXmlObjectSerializer<T>.Deserialize(XObject content)
        {
            return Deserialize((XContentType)content);
        }

        XObject IXmlObjectSerializer.Serialize(object obj, XName name)
        {
            return Serialize((T)obj, name);
        }

        XObject IXmlObjectSerializer<T>.Serialize(T obj, XName name)
        {
            return Serialize(obj, name);
        }

        #endregion
    }
}
