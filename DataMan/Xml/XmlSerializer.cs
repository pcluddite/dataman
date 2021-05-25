﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    public static class XmlSerializer
    {
        private static readonly IDictionary<string, Type> SerializableTypes = new Dictionary<string, Type>();

        public static void RegisterType<T>(string name) where T : IXmlSerializableObject
        {
            SerializableTypes[name] = typeof(T);
        }

        public static void RegisterType<T>(XName name) where T : IXmlSerializableObject
        {
            SerializableTypes[name.ToString()] = typeof(T);
        }

        public static object Deserialize(XElement node)
        {
            if (node == null)
                throw new NullReferenceException();
            Type t = SerializableTypes[node.Name.ToString()];
            if (t == null)
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
            if (node == null)
                throw new NullReferenceException();
            try
            {
                return (T)SerializedXmlObject<T>.CreateSerializedObject(node).Deserialize();
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

        internal static ISerializedXmlObject CreateSerializedObject(Type t, XElement node)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = serializedXmlObject.GetMethod("CreateSerializedObject", new Type[] { typeof(XElement) });
            return (ISerializedXmlObject)factoryMethod.Invoke(null, new object[] { node });
        }

        internal static ISerializedXmlObject CreateSerializedObject(Type t, XElement node, XmlSerializeAttribute attrib)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = serializedXmlObject.GetMethod("CreateSerializedObject", new Type[] { typeof(XElement), typeof(XmlSerializeAttribute) });
            return (ISerializedXmlObject)factoryMethod.Invoke(null, new object[] { node, attrib });
        }

        internal static ISerializedXmlObject CreateSerializedObject(Type t, XElement node, XName name, object defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = serializedXmlObject.GetMethod("CreateSerializedObject", new Type[] { typeof(XElement), typeof(XName), t });
            return (ISerializedXmlObject)factoryMethod.Invoke(null, new object[] { node, name, defaultValue });
        }

        internal static IDeserializedXmlObject CreateDeserializedObject(Type t, object obj, XmlSerializeAttribute attrib)
        {
            Type deserializedXmlObject = typeof(DeserializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = deserializedXmlObject.GetMethod("CreateDeserializedObject", new Type[] { t, typeof(XmlSerializeAttribute) });
            return (IDeserializedXmlObject)factoryMethod.Invoke(null, new object[] { obj, attrib });
        }

        internal static IDeserializedXmlObject CreateDeserializedObject(Type t, object obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedXmlObject<>).MakeGenericType(t);
            MethodInfo factoryMethod = deserializedXmlObject.GetMethod("CreateDeserializedObject", new Type[] { t, typeof(XName) });
            return (IDeserializedXmlObject)factoryMethod.Invoke(null, new object[] { obj, name });
        }
    }
}