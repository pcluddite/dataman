﻿//
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

namespace Baxendale.DataManagement.Xml
{
    internal class XmlDictionarySerializer<DictionaryType, TKey, TValue> : XmlObjectSerializer<DictionaryType, XElement>
        where DictionaryType : IDictionary<TKey, TValue>
    {
        public override bool UsesXAttribute => false;

        public XmlDictionarySerializer()
        {
        }

        public override DictionaryType Deserialize(XElement content)
        {
            DictionaryType dictionary = (DictionaryType)Activator.CreateInstance(typeof(DictionaryType));
            foreach (XElement child in content.Elements(XmlSerializer.ElementName))
            {
                TKey key = XmlSerializer.Deserialize<TKey>(child, XmlSerializer.KeyAttributeName);
                TValue value = XmlSerializer.Deserialize<TValue>(child, XmlSerializer.ValueAttributeName);
                dictionary.Add(key, value);
            }
            return dictionary;
        }

        public override XElement Serialize(DictionaryType obj, XName name)
        {
            XElement element = new XElement(name);
            foreach (KeyValuePair<TKey, TValue> item in obj)
            {
                XElement a = new XElement(XmlSerializer.ElementName);
                a.Add(XmlSerializer.Serialize(item.Key, XmlSerializer.KeyAttributeName));
                a.Add(XmlSerializer.Serialize(item.Value, XmlSerializer.ValueAttributeName));
                element.Add(a);
            }
            return element;
        }
    }
}
