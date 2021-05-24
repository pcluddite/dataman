using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedGenericCollection(T obj, XName name)
        {
            Type collectionType = typeof(T).GetGenericBaseType(typeof(ICollection<>));
            if (collectionType == null)
                throw new UnsupportedTypeException(typeof(T));

            Type deserializedXmlObject = typeof(DeserializedGenericCollection<>).MakeGenericType(collectionType.GetGenericArguments()[0]);
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedGenericCollection<ItemType> : DeserializedXmlObject<ICollection<ItemType>>
        {
            public DeserializedGenericCollection(ICollection<ItemType> obj, XmlSerializeAttribute attrib)
                : base(obj, attrib.Name)
            {
            }

            public DeserializedGenericCollection(ICollection<ItemType> obj, XName attrName)
                : base(obj, attrName)
            {
            }

            public override XObject Serialize()
            {
                if (DeserializedObject.IsReadOnly)
                    throw new UnsupportedTypeException(typeof(ICollection<ItemType>));

                XElement element = new XElement(Name);
                foreach (ItemType item in DeserializedObject)
                {
                    XElement a = new XElement("a");
                    IDeserializedXmlObject xobj = XmlSerializer.CreateDeserializedObject(typeof(ItemType), item, "v");
                    a.Add(xobj.Serialize());
                    element.Add(a);
                }
                return element;
            }
        }
    }
}
