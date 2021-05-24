using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedCollection(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedArrayObject<>).MakeGenericType(typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedCollection<V> : DeserializedXmlObject<ICollection>
        {
            public DeserializedCollection(ICollection obj, XmlSerializeAttribute attrib)
                : base(obj, attrib.Name)
            {
            }

            public DeserializedCollection(ICollection obj, XName attrName)
                : base(obj, attrName)
            {
            }

            public override XObject Serialize()
            {
                if (DeserializedObject.IsReadOnly() == true)
                    throw new UnsupportedTypeException(typeof(ICollection<V>));

                XElement element = new XElement(Name);
                foreach (V item in DeserializedObject)
                {
                    XElement a = new XElement("a");
                    IDeserializedXmlObject xobj = XmlSerializer.CreateDeserializedObject(item.GetType(), item, "v");
                    a.SetAttributeValue("t", item.GetType().FullName);
                    a.Add(xobj.Serialize());
                    element.Add(a);
                }
                return element;
            }
        }
    }
}
