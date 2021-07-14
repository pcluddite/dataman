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
using System.Reflection;
using System.Xml.Linq;
using Baxendale.Data.Collections;
using Baxendale.Data.Collections.Concurrent;
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    public sealed class XmlSerializer
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

        public T Deserialize<T>(XElement content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            if (serializer.UsesXAttribute)
                throw new UnsupportedTypeException(typeof(T));
            try
            {
                return serializer.Deserialize(content);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public object Deserialize(Type t, XElement content)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            if (serializer.UsesXAttribute)
                throw new UnsupportedTypeException(t);
            try
            {
                return serializer.Deserialize(content);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public T Deserialize<T>(XAttribute content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            if (!serializer.UsesXAttribute)
                throw new UnsupportedTypeException(typeof(T));
            try
            {
                return serializer.Deserialize(content);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public object Deserialize(Type t, XAttribute content)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            if (!serializer.UsesXAttribute)
                throw new UnsupportedTypeException(t);
            try
            {
                return serializer.Deserialize(content);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        internal T Deserialize<T>(XElement content, XName name)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (content == null)
                throw new UnsupportedTypeException(typeof(T));
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            if (serializer.UsesXAttribute)
            {
                XAttribute attribute = content.Attribute(name);
                if (attribute == null)
                    throw new XObjectNotFoundException(name.ToString());
                return Deserialize<T>(attribute);
            }
            content = content.Element(name);
            if (content == null)
                throw new XObjectNotFoundException(name.ToString());
            return Deserialize<T>(content);
        }

        internal object Deserialize(Type t, XElement content, XName name)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (name == null) throw new ArgumentNullException(nameof(name));
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            if (serializer.UsesXAttribute)
            {
                XAttribute attribute = content.Attribute(name);
                if (attribute == null)
                    throw new XObjectNotFoundException(name.ToString());
                return Deserialize(t, attribute);
            }
            content = content.Element(name);
            if (content == null)
                throw new XObjectNotFoundException(name.ToString());
            return Deserialize(t, content);
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

        public XElement Serialize<T>(T obj)
        {
            XName contentName = GetSerializedTypeName(typeof(T));
            if (contentName == null)
                throw new UnregisteredTypeException(typeof(T));
            return Serialize(obj, contentName);
        }

        public XElement Serialize<T>(T obj, XName name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return Serialize(CreateSerializerObject(obj), obj, name, ElementName);
        }

        internal XElement Serialize<T>(T obj, XName defaultElementName, XName defaultAttributeName)
        {
            IXmlObjectSerializer<T> serializer = CreateSerializerObject(obj);
            XName contentName = GetSerializedTypeName(typeof(T));
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Serialize(serializer, obj, contentName, defaultElementName);
        }

        internal XElement Serialize<T>(IXmlObjectSerializer<T> serializer, T obj, XName contentName, XName defaultElementName)
        {
            XObject content;
            try
            {
                content = serializer.Serialize(obj, contentName);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(defaultElementName ?? ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        public XElement Serialize(Type t, object obj)
        {
            XName contentName = GetSerializedTypeName(t);
            if (contentName == null)
                throw new UnregisteredTypeException(t);
            return Serialize(t, obj, contentName);
        }

        public XElement Serialize(Type t, object obj, XName name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return Serialize(t, CreateSerializerObject(t, obj), obj, name, ElementName);
        }

        internal XElement Serialize(Type t, object obj, XName defaultElementName, XName defaultAttributeName)
        {
            IXmlObjectSerializer serializer = CreateSerializerObject(t, obj);
            XName contentName = GetSerializedTypeName(t);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Serialize(t, serializer, obj, contentName, defaultElementName);
        }

        internal XElement Serialize(Type t, IXmlObjectSerializer serializer, object obj, XName contentName, XName defaultElementName)
        {
            XObject content;
            try
            {
                content = serializer.Serialize(obj, contentName);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(defaultElementName ?? ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
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

        internal IXmlObjectSerializer CreateSerializerObject(Type t, object obj)
        {
            if (obj == null)
                return (IXmlObjectSerializer)Activator.CreateInstance(typeof(XmlNullSerializer<>).MakeGenericType(t), this);
            return CreateSerializerObject(t);
        }

        internal IXmlObjectSerializer CreateSerializerObject(Type t)
        {
            LockingDictionary<Type, IXmlObjectSerializer> cache = _cache;
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);

            if (cache == null)
                return (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this);

            IXmlObjectSerializer serializer;
            if (!cache.TryGetValue(t, out serializer))
                cache.Add(t, serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this));

            return serializer;
        }

        internal IXmlObjectSerializer<T> CreateSerializerObject<T>(T obj)
        {
            if (obj == null)
                return new XmlNullSerializer<T>(this);
            return CreateSerializerObject<T>();
        }

        internal IXmlObjectSerializer<T> CreateSerializerObject<T>()
        { 
            LockingDictionary<Type, IXmlObjectSerializer> cache = _cache;
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));

            if (cache == null)
                return (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType, this);

            IXmlObjectSerializer serializer;
            if (!cache.TryGetValue(typeof(T), out serializer))
                cache.Add(typeof(T), serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this));

            return (IXmlObjectSerializer<T>)serializer;
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
