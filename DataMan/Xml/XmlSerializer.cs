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
using System.Reflection;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Xml
{
    public static class XmlSerializer
    {
        public static readonly XNamespace ReservedNamespace = XNamespace.Get("baxml");

        private static readonly ISet<XName> ReservedNames = new HashSet<XName>(new[] { ReservedNamespace + "a", ReservedNamespace + "v" });
        private static readonly OneToManyBidictionary<Type, XName> SerializableTypes = new OneToManyBidictionary<Type, XName>();

        private static readonly object _object = new object();

        public static void RegisterType<T>(XName name) where T : IXmlSerializableObject
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ReservedNames.Contains(name)) throw new ArgumentException($"'{name}' is a reserved name and cannot be registered");
            SerializableTypes[typeof(T)] = name;
        }

        internal static XName GetSerializedTypeName(Type type)
        {
            return SerializableTypes.GetValuesByKey(type).FirstOrDefault();
        }

        internal static Type GetSerializedType(XName name)
        {
            return SerializableTypes.GetKeysByValue(name).FirstOrDefault();
        }

        public static object Deserialize(XElement node)
        {
            if (node == null)
                throw new NullReferenceException();
            Type t;
            if (!SerializableTypes.TryGetKey(node.Name, out t))
                throw new UnregisteredTypeException(node.Name);
            try
            {
                return CreateSerializedObject(t, node).Deserialize();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public static T Deserialize<T>(XElement node)
        {
            return Deserialize<T>(node, null);
        }

        public static T Deserialize<T>(XElement node, XName name)
        {
            if (node == null)
                throw new NullReferenceException();
            try
            {
                return (T)SerializedXmlObject<T>.CreateSerializedObject(node, name, default(T)).Deserialize();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public static XObject Serialize<T>(T o, XName name)
        {
            try
            {
                return CreateDeserializedObject(typeof(T), o, name).Serialize();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }

        }

        public static XObject Serialize(Type t, object o, XName name)
        {
            try
            {
                return CreateDeserializedObject(t, o, name).Serialize();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        public static XObject Serialize<T>(T o)
        {
            XName name = GetSerializedTypeName(typeof(T));
            if (name == null)
                throw new UnregisteredTypeException(typeof(T).Name);
            return Serialize(o, name);
        }

        internal static ISerializedXmlObject CreateSerializedObject(Type t, XElement node)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = serializedXmlObject.GetMethod(nameof(SerializedXmlObject<object>.CreateSerializedObject), new Type[] { typeof(XElement) });
            return (ISerializedXmlObject)factoryMethod.Invoke(null, new object[] { node });
        }

        internal static ISerializedXmlObject CreateSerializedObject(Type t, XElement node, XmlSerializeAttribute attrib)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = serializedXmlObject.GetMethod(nameof(SerializedXmlObject<object>.CreateSerializedObject), new Type[] { typeof(XElement), typeof(XmlSerializeAttribute) });
            return (ISerializedXmlObject)factoryMethod.Invoke(null, new object[] { node, attrib });
        }

        internal static ISerializedXmlObject CreateSerializedObject(Type t, XElement node, XName name, object defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = serializedXmlObject.GetMethod(nameof(SerializedXmlObject<object>.CreateSerializedObject), new Type[] { typeof(XElement), typeof(XName), t });
            return (ISerializedXmlObject)factoryMethod.Invoke(null, new object[] { node, name, defaultValue });
        }

        internal static IDeserializedXmlObject CreateDeserializedObject(Type t, object obj, XmlSerializeAttribute attrib)
        {
            Type deserializedXmlObject = typeof(DeserializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = deserializedXmlObject.GetMethod(nameof(DeserializedXmlObject<object>.CreateDeserializedObject), new Type[] { t, typeof(XmlSerializeAttribute) });
            return (IDeserializedXmlObject)factoryMethod.Invoke(null, new object[] { obj, attrib });
        }

        internal static IDeserializedXmlObject CreateDeserializedObject(Type t, object obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = deserializedXmlObject.GetMethod(nameof(DeserializedXmlObject<object>.CreateDeserializedObject), new Type[] { t, typeof(XName) });
            return (IDeserializedXmlObject)factoryMethod.Invoke(null, new object[] { obj, name });
        }
    }
}
