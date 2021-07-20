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
using System.IO;
using System.Xml.Linq;
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    public delegate XAttribute ToXAttribute<in T>(XmlSerializer serializer, T obj, XName name);
    public delegate T FromXAttribute<out T>(XmlSerializer serializer, XAttribute content);

    public delegate XElement ToXElement<in T>(XmlSerializer serializer, T obj, XName name);
    public delegate T FromXElement<out T>(XmlSerializer serializer, XElement content);

    public sealed partial class XmlSerializer
    {
        public static readonly XNamespace ReservedNamespace = "https://github.com/pcluddite/dataman";
        public static readonly XName ReservedNamespaceName = XNamespace.Xmlns + "baxml";

        public static XmlSerializer Default { get; } = new XmlSerializer();

        internal static readonly XName ElementName = ReservedNamespace + "a";
        internal static readonly XName KeyAttributeName = ReservedNamespace + "k";
        internal static readonly XName ValueAttributeName = ReservedNamespace + "v";
        internal static readonly XName IndexAttributeName = ReservedNamespace + "i";
        internal static readonly XName TypeAttributeName = ReservedNamespace + "t";

        private readonly XSerializerCustomTypesMap SerializableTypes;

        public XmlSerializer()
        {
            SerializableTypes = new XSerializerCustomTypesMap(this);
        }

        public void RegisterType<T>(XName name) where T : IXmlSerializableObject
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Namespace == ReservedNamespace) throw new ArgumentException($"'{name}' is a reserved name and cannot be registered");
            SerializableTypes.Add<T>(name);
        }

        public void RegisterType<T>(XName name, ToXElement<T> toXmlDelegate, FromXElement<T> fromXmlDelegate)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (toXmlDelegate == null) throw new ArgumentNullException(nameof(toXmlDelegate));
            if (fromXmlDelegate == null) throw new ArgumentNullException(nameof(fromXmlDelegate));
            if (name.Namespace == ReservedNamespace) throw new ArgumentException($"'{name}' is a reserved name and cannot be registered");
            SerializableTypes.Add(name, toXmlDelegate, fromXmlDelegate);
        }

        public void RegisterType<T>(XName name, ToXAttribute<T> toXmlDelegate, FromXAttribute<T> fromXmlDelegate)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (toXmlDelegate == null) throw new ArgumentNullException(nameof(toXmlDelegate));
            if (fromXmlDelegate == null) throw new ArgumentNullException(nameof(fromXmlDelegate));
            if (name.Namespace == ReservedNamespace) throw new ArgumentException($"'{name}' is a reserved name and cannot be registered");
            SerializableTypes.Add(name, toXmlDelegate, fromXmlDelegate);
        }

        public object Deserialize(XElement node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return Deserialize(SerializableTypes.GetTypeFromXElement(node), node);
        }

        public T Load<T>(string path) where T : IXmlSerializableObject
        {
            return Load<T>(XDocument.Load(path));
        }

        public T Load<T>(Stream stream) where T : IXmlSerializableObject
        {
            return Load<T>(XDocument.Load(stream));
        }

        public T Load<T>(XDocument document) where T : IXmlSerializableObject
        {
            return Deserialize<T>(document.Root);
        }

        public XDocument Save<T>(T o) where T : IXmlSerializableObject
        {
            XElement root = Serialize(o);
            root.SetAttributeValue(ReservedNamespaceName, ReservedNamespace.NamespaceName);
            XDocument doc = new XDocument();
            doc.Add(root);
            return doc;
        }

        public void Save<T>(T o, Stream stream) where T : IXmlSerializableObject
        {
            Save(o).Save(stream);
        }

        public void Save<T>(T o, string path) where T : IXmlSerializableObject
        {
            Save(o).Save(path);
        }

        internal Type GetTypeFromXElement(XElement content)
        {
            return SerializableTypes.GetTypeFromXElement(content);
        }

        internal static Type GetObjectSerializerType(Type memberType)
        {
            if (typeof(IXmlSerializableObject).IsAssignableFrom(memberType))
            {
                return typeof(XmlCustomObjectSerializer<>).MakeGenericType(memberType);
            }
            else if (memberType.IsArray)
            {
                return typeof(XmlArraySerializer<,>).MakeGenericType(memberType, memberType.GetElementType());
            }

            Type dictionaryType = memberType.GetGenericInterfaceDefinition(typeof(IDictionary<,>));
            if (dictionaryType != null)
            {
                Type[] generics = dictionaryType.GetGenericArguments();
                return typeof(XmlDictionarySerializer<,,>).MakeGenericType(dictionaryType, generics[0], generics[1]);
            }

            Type collectionType = memberType.GetGenericInterfaceDefinition(typeof(ICollection<>));
            if (collectionType != null)
            {
                return typeof(XmlGenericCollectionSerializer<,>).MakeGenericType(memberType, collectionType.GetGenericArguments()[0]);
            }
            
            if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                return typeof(XmlCollectionSerializer<>).MakeGenericType(memberType);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return typeof(XmlConvertibleSerializer<>).MakeGenericType(memberType);
            }
            return null;
        }
    }
}
