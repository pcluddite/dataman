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
                ThrowUnknownTag(node.Name);
            return node.CreateSerializerObject(t).Deserialize();
        }

        public static T Deserialize<T>(XElement node)
        {
            if (node == null)
                throw new NullReferenceException();
            return (T)node.CreateSerializerObject(typeof(T)).Deserialize();
        }

        internal static ISerializedXmlObject CreateSerializerObject(this XElement node, Type t)
        {
            Type serializedXmlObject = typeof(SerializedXmlObject<>).MakeGenericType(t);
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node);
        }

        private static void ThrowUnknownTag(XName name)
        {
            ThrowUnknownTag(name.ToString());
        }

        private static void ThrowUnknownTag(string tagName)
        {
            throw new XmlException("<" + tagName + "> tag is not recognized as a valid element");
        }
    }
}
