using System;
using System.Collections;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedCollection(XElement node, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedCollection<>).MakeGenericType(typeof(T), typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, defaultValue);
        }

        private class SerializedCollection<CollectionType> : SerializedXmlObject<CollectionType>
            where CollectionType : ICollection
        {
            public SerializedCollection(XElement node, CollectionType defaultValue)
                : base(node, node.Name, defaultValue)
            {
            }

            public override CollectionType Deserialize()
            {
                XElement node = Node;
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
