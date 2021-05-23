using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    public static class XmlSerializer
    {
        private static readonly IDictionary<string, Type> SerializableTypes = new Dictionary<string, Type>();

        public static void RegisterType<T>(string name) where T : IXmlSerializable, new()
        {
            SerializableTypes[name] = typeof(T);
        }

        public static void RegisterType<T>(XName name) where T : IXmlSerializable, new()
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
            return CreateSerializerObject(t, node).Deserialize();
        }

        public static T Deserialize<T>(XElement node)
        {
            if (node == null)
                throw new NullReferenceException();
            return new SerializedXmlObject<T>(node).Deserialize();
        }

        internal static ISerializedXmlObject CreateSerializerObject(Type t, XElement node)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node);
        }

        internal static ISerializedXmlObject CreateSerializerObject(Type t, XElement node, XmlSerializeAttribute attrib)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, attrib);
        }
    }
}
