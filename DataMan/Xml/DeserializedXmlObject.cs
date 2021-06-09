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
    internal interface IDeserializedXmlObject
    {
        XObject Serialize();
    }

    internal abstract partial class DeserializedXmlObject<T> : IDeserializedXmlObject
    {
        public virtual T DeserializedObject { get; protected set; }
        public virtual XName Name { get; protected set; }
        
        protected DeserializedXmlObject(T obj, XName name)
        {
            DeserializedObject = obj;
            Name = name;
        }

        public abstract XObject Serialize();

        public static IDeserializedXmlObject CreateDeserializedObject(T obj, XmlSerializeAttribute attrib)
        {
            return CreateDeserializedObject(obj, attrib.Name);
        }

        public static IDeserializedXmlObject CreateDeserializedObject(T obj, XName name)
        {
            Type memberType = typeof(T);
            if (!memberType.IsValueType && ((object)obj) == null)
            {
                return CreateDeserializedNullObject(name);
            }
            else if (typeof(IXmlSerializableObject).IsAssignableFrom(memberType))
            {
                return CreateDeserializedCustomObject(obj, name);
            }
            else if (memberType.IsArray)
            {
                return CreateDeserializedArray(obj, name);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(IDictionary<,>)))
            {
                return CreateDeserializedDictionary(obj, name);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(ICollection<>)))
            {
                return CreateDeserializedGenericCollection(obj, name);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                return CreateDeserializedCollection(obj, name);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return CreateDeserializedConvertible(obj, name);
            }
            throw new UnsupportedTypeException(typeof(T));
        }
    }
}
