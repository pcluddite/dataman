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
using Baxendale.DataManagement.Collections.NonGenerics;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedCollection(XElement node, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedCollection<>).MakeGenericType(typeof(T), typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, defaultValue);
        }

        private class SerializedCollection<CollectionType> : SerializedXmlObject<CollectionType>
            where CollectionType : ICollection
        {
            public SerializedCollection(XElement node, CollectionType defaultValue)
                : base(node, node.Name, defaultValue)
            {
            }

            public override CollectionType Deserialize()
            {
                XElement node = Node;
                if (node == null)
                    return DefaultValue;

                CollectionType collection = (CollectionType)Activator.CreateInstance(typeof(CollectionType));
                if (collection.IsReadOnly() == true)
                    throw new UnsupportedTypeException(typeof(CollectionType));
                foreach (XElement child in node.Elements(XmlSerializer.ElementName))
                {
                    XAttribute typeAttribute = child.Attribute(XmlSerializer.TypeAttributeName);
                    if (typeAttribute == null)
                        throw new UnregisteredTypeException(child.Name.ToString());
                    Type itemType = Type.GetType(typeAttribute.Value, true);
                    collection.Add(XmlSerializer.CreateSerializedObject(itemType, child, XmlSerializer.ValueAttributeName, itemType.CreateDefault()));
                }
                return collection;
            }
        }
    }
}
