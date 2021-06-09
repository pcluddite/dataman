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
using System.Collections.Generic;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal interface ISerializedXmlObject
    {
        object Deserialize();
    }

    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        public XElement Node { get; private set; }
        public XName Name { get; private set; }
        public T DefaultValue { get; private set; }

        protected SerializedXmlObject(XElement node, XName attrName, T defaultValue)
        {
            Node = node;
            Name = attrName;
            DefaultValue = defaultValue;
        }

        public abstract T Deserialize();

        public static ISerializedXmlObject CreateSerializedObject(XElement node)
        {
            return CreateSerializedObject(node, null, default(T));
        }

        public static ISerializedXmlObject CreateSerializedObject(XElement node, XmlSerializeAttribute attrib)
        {
            return CreateSerializedObject(node, attrib.Name, (T)attrib.Default);
        }

        public static ISerializedXmlObject CreateSerializedObject(XElement node, XName name, T defaultValue)
        {
            Type memberType = typeof(T);
            if (typeof(IXmlSerializableObject).IsAssignableFrom(memberType))
            {
                if (name != null) node = node.Element(name);
                return CreateSerializedCustomObject(node.Element(name), defaultValue);
            }
            else if (memberType.IsArray)
            {
                if (name != null) node = node.Element(name);
                return CreateSerializedArray(node, defaultValue);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(IDictionary<,>)))
            {
                if (name != null) node = node.Element(name);
                return CreateSerializedDictionary(node.Element(name), defaultValue);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(ICollection<>)))
            {
                if (name != null) node = node.Element(name);
                return CreateSerializedGenericCollection(node.Element(name), defaultValue);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                if (name != null) node = node.Element(name);
                return CreateSerializedCollection(node.Element(name), defaultValue);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return CreateSerializedConvertible(node, name, defaultValue);
            }
            else if (typeof(object) == memberType)
            {
                XAttribute typeAttr = node.Attribute("t");
                if (typeAttr?.Value == "null")
                    return CreateSerializedNullObject(node, name);
                Type foundType = Type.GetType(typeAttr?.Value, throwOnError: false);
                if (foundType == null)
                    throw new UnregisteredTypeException(node.Name);
                return XmlSerializer.CreateSerializedObject(foundType, node, name, defaultValue);
            }
            throw new UnsupportedTypeException(typeof(T));
        }

        #region ISerializedXmlObject Members

        object ISerializedXmlObject.Deserialize()
        {
            return Deserialize();
        }

        #endregion
    }
}
