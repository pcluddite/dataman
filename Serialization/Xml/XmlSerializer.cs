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
using Baxendale.Data.Collections;
using Baxendale.Data.Collections.Concurrent;
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    public sealed partial class XmlSerializer
    {
        public static readonly XNamespace ReservedNamespace = "https://github.com/pcluddite/dataman";
        public static readonly XName ReservedNamespaceName = XNamespace.Xmlns + "baxml";

        internal static readonly XName ElementName = ReservedNamespace + "a";
        internal static readonly XName KeyAttributeName = ReservedNamespace + "k";
        internal static readonly XName ValueAttributeName = ReservedNamespace + "v";
        internal static readonly XName IndexAttributeName = ReservedNamespace + "i";
        internal static readonly XName TypeAttributeName = ReservedNamespace + "t";

        private readonly OneToManyBidictionary<Type, XName> SerializableTypes = new OneToManyBidictionary<Type, XName>();

        public static XmlSerializer Default { get; } = new XmlSerializer(cache: false);

        private LockingDictionary<Type, IXmlObjectSerializer> _cache;

        public bool CacheTypes
        {
            get
            {
                return _cache != null;
            }
            set
            {
                if (value && _cache == null)
                {
                    _cache = new LockingDictionary<Type, IXmlObjectSerializer>();
                }
                else if (!value && _cache != null)
                {
                    _cache.Clear();
                    _cache = null;
                }
            }
        }

        public XmlSerializer()
            : this(cache: true)
        {
        }

        public XmlSerializer(bool cache)
        {
            CacheTypes = cache;
        }

        public void RegisterType<T>(XName name) where T : IXmlSerializableObject
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Namespace == ReservedNamespace) throw new ArgumentException($"'{name}' is a reserved name and cannot be registered");
            SerializableTypes[typeof(T)] = name;
        }

        internal XName GetSerializedTypeName(Type type)
        {
            XName name;
            if (SerializableTypes.TryGetValue(type, out name))
                return name;
            return null;
        }

        internal Type GetSerializedType(XName name)
        {
            Type type;
            if (SerializableTypes.TryGetKey(name, out type))
                return type;
            return null;
        }

        public object Deserialize(XElement node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return Deserialize(GetTypeFromNode(node), node);
        }

        private Type GetTypeFromNode(XElement node)
        {
            if (node.Name == ElementName)
            {
                string attrValue = node.Attribute(TypeAttributeName)?.Value;
                if (attrValue == null)
                    throw new UnregisteredTypeException(node.Name.ToString());
                if (attrValue == "null")
                    return typeof(object);
                return Type.GetType(attrValue, throwOnError: true);
            }

            Type t;
            if (!SerializableTypes.TryGetKey(node.Name, out t))
                throw new UnregisteredTypeException(node.Name.ToString());
            return t;
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
            else if (memberType.IsSubClassOfGeneric(typeof(IDictionary<,>)))
            {
                Type dictionaryType = memberType.GetGenericBaseType(typeof(IDictionary<,>));
                if (dictionaryType == null)
                    throw new UnsupportedTypeException(memberType);
                Type[] generics = dictionaryType.GetGenericArguments();
                return typeof(XmlDictionarySerializer<,,>).MakeGenericType(dictionaryType, generics[0], generics[1]);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(ICollection<>)))
            {
                Type collectionType = memberType.GetGenericBaseType(typeof(ICollection<>));
                if (collectionType == null)
                    throw new UnsupportedTypeException(memberType);
                return typeof(XmlGenericCollectionSerializer<,>).MakeGenericType(collectionType, collectionType.GetGenericArguments()[0]);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                return typeof(XmlCollectionSerializer<>).MakeGenericType(memberType);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return typeof(XmlConvertibleSerializer<>).MakeGenericType(memberType);
            }
            return null;
        }

        public void ClearCache()
        {
            _cache?.Clear();
        }
    }
}
