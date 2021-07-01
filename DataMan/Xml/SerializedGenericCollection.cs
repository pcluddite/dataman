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
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedGenericCollection(XElement node, T defaultValue)
        {
            Type collectionType = typeof(T).GetGenericBaseType(typeof(ICollection<>));
            if (collectionType == null)
                throw new UnsupportedTypeException(typeof(T));

            Type serializedXmlObject = typeof(SerializedGenericCollection<>).MakeGenericType(collectionType, collectionType.GetGenericArguments()[0]);
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, defaultValue);
        }

        private class SerializedGenericCollection<ItemType> : SerializedXmlObject<ICollection<ItemType>>
        {
            public SerializedGenericCollection(XElement node, ICollection<ItemType> defaultValue)
                : base(node, node.Name, defaultValue)
            {
            }

            public override ICollection<ItemType> Deserialize()
            {
                XElement node = Node;
                if (node == null)
                    return DefaultValue;

                ICollection<ItemType> collection = (ICollection<ItemType>)Activator.CreateInstance(typeof(T));
                if (collection.IsReadOnly)
                    throw new UnsupportedTypeException(typeof(T));
                foreach (XElement child in node.Elements())
                {
                    Type t = XmlSerializer.GetSerializedType(child.Name.ToString());
                    if (t == null)
                    {
                        collection.Add(XmlSerializer.Deserialize<ItemType>(child, XmlSerializer.ValueAttributeName));
                    }
                    else
                    {
                        collection.Add(XmlSerializer.Deserialize<ItemType>(child));
                    }
                }
                return collection;
            }
        }
    }
}
