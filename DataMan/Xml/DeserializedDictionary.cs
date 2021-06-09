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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedDictionary(T obj, XName name)
        {
            Type dictionaryType = typeof(T).GetGenericBaseType(typeof(IDictionary<,>));
            if (dictionaryType == null)
                throw new UnsupportedTypeException(typeof(T));
            Type[] generics = dictionaryType.GetGenericArguments();
            Type deserializedXmlObject = typeof(DeserializedDictionary<,>).MakeGenericType(dictionaryType, generics[0], generics[1]);
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedDictionary<TKey, TValue> : DeserializedXmlObject<IDictionary<TKey, TValue>>
        {
            public DeserializedDictionary(IDictionary<TKey, TValue> obj, XName name)
                : base(obj, name)
            {
            }

            public override XObject Serialize()
            {
                XElement element = new XElement(Name);
                foreach (KeyValuePair<TKey, TValue> item in DeserializedObject)
                {
                    XElement a = new XElement("a");
                    a.Add(XmlSerializer.Serialize(item.Key, "key"));
                    a.Add(XmlSerializer.Serialize(item.Value, "value"));
                    element.Add(a);
                }
                return element;
            }
        }
    }
}
