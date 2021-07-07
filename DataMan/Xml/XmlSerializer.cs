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
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    public static class XmlSerializer
    {
        public static readonly XNamespace ReservedNamespace = "https://github.com/pcluddite/dataman";
        public static readonly XName ReservedNamespaceName = XNamespace.Xmlns + "baxml";

        internal static readonly XName ElementName = ReservedNamespace + "a";
        internal static readonly XName KeyAttributeName = ReservedNamespace + "k";
        internal static readonly XName ValueAttributeName = ReservedNamespace + "v";
        internal static readonly XName IndexAttributeName = ReservedNamespace + "i";
        internal static readonly XName TypeAttributeName = ReservedNamespace + "t";

        private static readonly OneToManyBidictionary<Type, XName> SerializableTypes = new OneToManyBidictionary<Type, XName>();

        public static void RegisterType<T>(XName name) where T : IXmlSerializableObject
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.Namespace == ReservedNamespace) throw new ArgumentException($"'{name}' is a reserved name and cannot be registered");
            SerializableTypes[typeof(T)] = name;
        }

        internal static XName GetSerializedTypeName(Type type)
        {
            XName name;
            if (SerializableTypes.TryGetValue(type, out name))
                return name;
            return null;
        }

        internal static Type GetSerializedType(XName name)
        {
            Type type;
            if (SerializableTypes.TryGetKey(name, out type))
                return type;
            return null;
        }

        public static object Deserialize(XElement node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return Deserialize(GetTypeFromNode(node), node);
        }

        public static T Deserialize<T>(XElement content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));
            IXmlObjectSerializer<T> serializer = (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType);
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

        public static object Deserialize(Type t, XElement content)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (content == null) throw new ArgumentNullException(nameof(content));
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
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

        public static T Deserialize<T>(XAttribute content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));
            IXmlObjectSerializer<T> serializer = (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType);
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

        public static object Deserialize(Type t, XAttribute content)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (content == null) throw new ArgumentNullException(nameof(content));
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
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

        internal static T Deserialize<T>(XElement content, XName name)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (name == null) throw new ArgumentNullException(nameof(name));
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (content == null)
                throw new UnsupportedTypeException(typeof(T));
            IXmlObjectSerializer<T> serializer = (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType);
            if (serializer.UsesXAttribute)
            {
                XAttribute attribute = content.Attribute(name);
                if (attribute == null)
                    throw new XObjectNotFound(name.ToString());
                return Deserialize<T>(attribute);
            }
            content = content.Element(name);
            if (content == null)
                throw new XObjectNotFound(name.ToString());
            return Deserialize<T>(content);
        }

        internal static object Deserialize(Type t, XElement content, XName name)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (name == null) throw new ArgumentNullException(nameof(name));
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
            if (serializer.UsesXAttribute)
            {
                XAttribute attribute = content.Attribute(name);
                if (attribute == null)
                    throw new XObjectNotFound(name.ToString());
                return Deserialize(t, attribute);
            }
            content = content.Element(name);
            if (content == null)
                throw new XObjectNotFound(name.ToString());
            return Deserialize(t, content);
        }

        internal static object Deserialize(MemberInfo member, XElement content)
        {
            Type memberType = member.GetReturnType();
            XmlSerializableMemberAttribute attribute = member.GetMemberAttribute();
            return Deserialize(memberType, content, attribute.Name);
        }

        private static Type GetTypeFromNode(XElement node)
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

        public static T Load<T>(string path) where T : IXmlSerializableObject
        {
            return Load<T>(XDocument.Load(path));
        }

        public static T Load<T>(Stream stream) where T : IXmlSerializableObject
        {
            return Load<T>(XDocument.Load(stream));
        }

        public static T Load<T>(XDocument document) where T : IXmlSerializableObject
        {
            return Deserialize<T>(document.Root);
        }

        public static XElement Serialize<T>(T obj)
        {
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));
            IXmlObjectSerializer<T> serializer = (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType);
            XName name = GetSerializedTypeName(typeof(T));
            if (name == null)
                name = serializer.UsesXAttribute ? ValueAttributeName : ElementName;
            return Serialize(serializer, obj, name);
        }

        public static XElement Serialize<T>(T obj, XName name)
        {
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));
            IXmlObjectSerializer<T> serializer = (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType);
            return Serialize(serializer, obj, name);
        }

        private static XElement Serialize<T>(IXmlObjectSerializer<T> serializer, T obj, XName name)
        {
            XObject content;
            try
            {
                content = serializer.Serialize(obj, name);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        internal static XElement Serialize(Type t, object obj)
        {
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
            XName name = GetSerializedTypeName(t);
            if (name == null)
                name = serializer.UsesXAttribute ? ValueAttributeName : ElementName;
            return Serialize(serializer, obj, name);
        }

        internal static XElement Serialize(Type t, object obj, XName name)
        {
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
            return Serialize(serializer, obj, name);
        }

        private static XElement Serialize(IXmlObjectSerializer serializer, object obj, XName name)
        {
            XObject content;
            try
            {
                content = serializer.Serialize(obj, name);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        internal static XObject Serialize(MemberInfo member, object instance, bool serializeDefault)
        {
            Type memberType = member.GetReturnType();
            Type serializerType = GetObjectSerializerType(memberType);
            if (serializerType == null)
                throw new UnsupportedTypeException(memberType);
            IXmlObjectSerializer serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType);
            XmlSerializableMemberAttribute memberAttribute = member.GetMemberAttribute();
            object value = member.GetValue(instance);
            if (!serializeDefault && memberAttribute.Default == value)
                return null;
            return serializer.Serialize(value, memberAttribute.Name);
        }

        public static XDocument Save<T>(T o) where T : IXmlSerializableObject
        {
            XElement root = Serialize(o);
            root.SetAttributeValue(ReservedNamespaceName, ReservedNamespace.NamespaceName);
            XDocument doc = new XDocument();
            doc.Add(root);
            return doc;
        }

        public static void Save<T>(T o, Stream stream) where T : IXmlSerializableObject
        {
            Save(o).Save(stream);
        }

        public static void Save<T>(T o, string path) where T : IXmlSerializableObject
        {
            Save(o).Save(path);
        }

        internal static Type GetObjectSerializerType(Type memberType)
        {
            if (memberType == null)
            {
                return typeof(XmlNullSerializer<>).MakeGenericType(memberType);
            }
            else if (typeof(IXmlSerializableObject).IsAssignableFrom(memberType))
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
    }
}
