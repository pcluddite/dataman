using System;
using System.Collections;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedCollection(XElement node, XName name, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedCollection<>).MakeGenericType(typeof(T), typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, name, defaultValue);
        }

        private class SerializedCollection<CollectionType> : SerializedXmlObject<CollectionType>
            where CollectionType : ICollection
        {
            public SerializedCollection(XElement node, XName attrName, CollectionType defaultValue)
                : base(node, attrName, defaultValue)
            {
            }

            public override CollectionType Deserialize()
            {
                XElement node = AttributeName == null ? Node : Node.Element(AttributeName);
                if (node == null)
                    return DefaultValue;

                CollectionType collection = (CollectionType)Activator.CreateInstance(typeof(CollectionType));
                if (collection.IsReadOnly() == true)
                    throw new UnsupportedTypeException(typeof(CollectionType));
                foreach (XElement child in node.Elements("a"))
                {
                    XAttribute typeAttribute = child.Attribute("t");
                    if (typeAttribute == null)
                        throw new UnregisteredTypeException(child.Name);
                    Type itemType = Type.GetType(typeAttribute.Value, true);
                    collection.Add(XmlSerializer.CreateSerializedObject(itemType, child, "v", itemType.CreateDefault()));
                }
                return collection;
            }
        }
    }
}
