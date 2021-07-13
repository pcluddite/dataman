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
using System.Collections.Generic;
using System.Xml.Linq;

namespace Baxendale.Data.Xml
{
    internal class XmlGenericCollectionSerializer<CollectionType, ItemType> : XmlObjectSerializer<CollectionType, XElement>
        where CollectionType : ICollection<ItemType>, new()
    {
        public override bool UsesXAttribute => false;

        public XmlGenericCollectionSerializer(XmlSerializer serializer)
            : base(serializer)
        {
        }

        public override CollectionType Deserialize(XElement content)
        {
            CollectionType collection = Activator.CreateInstance<CollectionType>();
            if (collection.IsReadOnly)
                throw new UnsupportedTypeException(typeof(CollectionType));
            foreach (XElement child in content.Elements())
            {
                Type t = XmlSerializer.GetSerializedType(child.Name.ToString());
                if (t == null)
                {
                    collection.Add(XmlSerializer.Deserialize<ItemType>(child, ValueAttributeName));
                }
                else
                {
                    collection.Add(XmlSerializer.Deserialize<ItemType>(child));
                }
            }
            return collection;
        }

        public override XElement Serialize(CollectionType obj, XName name)
        {
            XElement element = new XElement(name);
            foreach (ItemType item in obj)
            {
                element.Add(XmlSerializer.Serialize(item, ElementName, ValueAttributeName));
            }
            return element;
        }
    }
}
