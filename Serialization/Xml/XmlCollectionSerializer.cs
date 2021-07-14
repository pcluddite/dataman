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
using System.Collections;
using System.Xml.Linq;
using Baxendale.Data.Collections.NonGenerics;

namespace Baxendale.Data.Xml
{
    internal class XmlCollectionSerializer<CollectionType> : XmlObjectSerializer<CollectionType, XElement>
        where CollectionType : ICollection
    {
        public override bool UsesXAttribute => false;

        public XmlCollectionSerializer(XmlSerializer serializer)
            : base(serializer)
        {
        }

        public override CollectionType Deserialize(XElement content)
        {
            CollectionType collection = (CollectionType)Activator.CreateInstance(typeof(CollectionType));
            if (collection.IsReadOnly() == true)
                throw new UnsupportedTypeException(typeof(CollectionType));
            foreach (XElement child in content.Elements())
            {
                XAttribute typeAttribute = child.Attribute(XmlSerializer.TypeAttributeName);
                if (typeAttribute == null)
                    throw new UnregisteredTypeException(child.Name.ToString());
                Type itemType = Type.GetType(typeAttribute.Value, true);
                collection.Add(XmlSerializer.Deserialize(itemType, child, null, ValueAttributeName));
            }
            return collection;
        }

        public override XElement Serialize(CollectionType obj, XName name)
        {
            XElement element = new XElement(name);
            foreach (object item in obj)
            {
                Type itemType = item?.GetType();
                XElement a = XmlSerializer.Serialize(itemType, item, ElementName, ValueAttributeName);
                if (itemType != null)
                    a.SetAttributeValue(XmlSerializer.TypeAttributeName, itemType.FullName ?? "null");
                element.Add(a);
            }
            return element;
        }
    }
}
