using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedGenericCollection(XElement node, XName name, T defaultValue)
        {
            Type collectionType = typeof(T).GetGenericBaseType(typeof(ICollection<>));
            if (collectionType == null)
                throw new UnsupportedTypeException(typeof(T));

            Type serializedXmlObject = typeof(SerializedGenericCollection<>).MakeGenericType(collectionType, collectionType.GetGenericArguments()[0]);
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, name, defaultValue);
        }

        private class SerializedGenericCollection<ItemType> : SerializedXmlObject<ICollection<ItemType>>
        {
            public SerializedGenericCollection(XElement node, XName name, ICollection<ItemType> defaultValue)
                : base(node, name, defaultValue)
            {
            }

            public override ICollection<ItemType> Deserialize()
            {
                XElement node = AttributeName == null ? Node : Node.Element(AttributeName);
                if (node == null)
                    return DefaultValue;

                ICollection<ItemType> collection = (ICollection<ItemType>)Activator.CreateInstance(typeof(T));
                if (collection.IsReadOnly)
                    throw new UnsupportedTypeException(typeof(T));
                foreach (XElement child in node.Elements("a"))
                {
                    collection.Add((ItemType)XmlSerializer.CreateSerializedObject(typeof(ItemType), child, "v", default(ItemType)).Deserialize());
                }
                return collection;
            }
        }
    }
}
