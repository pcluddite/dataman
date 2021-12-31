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
    internal interface IXObjectSerializer
    {
        XName ElementName { get; set; }
        XName ValueAttributeName { get; set; }

        object Deserialize(XObject content);
        XObject Serialize(object obj, XName name);
        bool UsesXAttribute { get; }
    }

    internal interface IXObjectSerializer<in T, out XContentType> : IXObjectSerializer
        where XContentType : XObject
    {
        XContentType Serialize(T obj, XName name);
    }

    internal interface IXObjectDeserializer<in XContentType, out T> : IXObjectSerializer
        where XContentType : XObject
    {
        T Deserialize(XContentType content);
    }

    internal abstract class XObjectSerializer<T, XContentType> : IXObjectSerializer<T, XContentType>, IXObjectDeserializer<XContentType, T>
        where XContentType : XObject
    {
        public XmlSerializer XmlSerializer { get; }

        public virtual XName ElementName { get; set; }

        public virtual XName ValueAttributeName { get; set; }

        public XObjectSerializer(XmlSerializer serializer)
        {
            XmlSerializer = serializer;
        }

        public abstract bool UsesXAttribute { get; }

        public abstract T Deserialize(XContentType content);

        public abstract XContentType Serialize(T obj, XName contentName);

        #region ISerializedXmlObject Members

        object IXObjectSerializer.Deserialize(XObject content)
        {
            return Deserialize((XContentType)content);
        }

        XObject IXObjectSerializer.Serialize(object obj, XName contentName)
        {
            return Serialize((T)obj, contentName);
        }

        #endregion
    }
}
