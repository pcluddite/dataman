using System;
using System.Collections;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedCollection(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedArray<>).MakeGenericType(typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedCollection : DeserializedXmlObject<ICollection>
        {
            public DeserializedCollection(ICollection obj, XName name)
                : base(obj, name)
            {
            }

            public override XObject Serialize()
            {
                if (DeserializedObject.IsReadOnly() == true)
                    throw new UnsupportedTypeException(typeof(T));

                XElement element = new XElement(Name);
                foreach (object item in DeserializedObject)
                {
                    XElement a;
                    if (item == null)
                    {
                        a = (XElement)CreateDeserializedNullObject("a").Serialize();
                    }
                    else
                    {
                        a = new XElement("a");
                        IDeserializedXmlObject xobj = XmlSerializer.CreateDeserializedObject(item.GetType(), item, "v");
                        a.SetAttributeValue("t", item.GetType().FullName);
                        a.Add(xobj.Serialize());
                    }
                    element.Add(a);
                }
                return element;
            }
        }
    }
}
