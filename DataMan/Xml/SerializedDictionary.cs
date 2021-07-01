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
        private static ISerializedXmlObject CreateSerializedDictionary(XElement node, T defaultValue)
        {
            Type dictionaryType = typeof(T).GetGenericBaseType(typeof(IDictionary<,>));
            if (dictionaryType == null)
                throw new UnsupportedTypeException(typeof(T));
            Type[] generics = dictionaryType.GetGenericArguments();
            Type serializedXmlObject = typeof(SerializedDictionary<,>).MakeGenericType(dictionaryType, generics[0], generics[1]);
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, defaultValue);
        }

        private class SerializedDictionary<TKey, TValue> : SerializedXmlObject<IDictionary<TKey, TValue>>
        {
            public SerializedDictionary(XElement node, IDictionary<TKey, TValue> defaultValue)
                : base(node, node.Name, defaultValue)
            {
            }

            public override IDictionary<TKey, TValue> Deserialize()
            {
                XElement node = Node;
                if (node == null)
                    return DefaultValue;
                IDictionary<TKey, TValue> dictionary = (IDictionary<TKey, TValue>)Activator.CreateInstance(typeof(T));
                foreach (XElement element in node.Elements(XmlSerializer.ElementName))
                {
                    dictionary.Add(XmlSerializer.Deserialize<TKey>(element, XmlSerializer.KeyAttributeName), XmlSerializer.Deserialize<TValue>(element, XmlSerializer.ValueAttributeName));
                }
                return dictionary;
            }
        }
    }
}
