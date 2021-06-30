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
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedGenericCollection(T obj, XName name)
        {
            Type collectionType = typeof(T).GetGenericBaseType(typeof(ICollection<>));
            if (collectionType == null)
                throw new UnsupportedTypeException(typeof(T));

            Type deserializedXmlObject = typeof(DeserializedGenericCollection<>).MakeGenericType(typeof(T), collectionType.GetGenericArguments()[0]);
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedGenericCollection<ItemType> : DeserializedXmlObject<ICollection<ItemType>>
        {
            public DeserializedGenericCollection(ICollection<ItemType> obj, XName name)
                : base(obj, name)
            {
            }

            public override XObject Serialize()
            {
                if (DeserializedObject.IsReadOnly)
                    throw new UnsupportedTypeException(typeof(T));

                XElement element = new XElement(Name);
                foreach (ItemType item in DeserializedObject)
                {
                    XObject serializedItem = XmlSerializer.Serialize(item, "v");
                    if (serializedItem is XElement)
                    {
                        ((XElement)serializedItem).Name = XmlSerializer.GetSerializedTypeName(item.GetType()) ?? "a";
                        element.Add(serializedItem);
                    }
                    else
                    {
                        XElement a = new XElement("a");
                        a.Add(serializedItem);
                        element.Add(a);
                    }
                }
                return element;
            }
        }
    }
}
