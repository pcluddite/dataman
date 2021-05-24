using System;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedConvertible(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedArray<>).MakeGenericType(typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedConvertible<V> : DeserializedXmlObject<V>
            where V : IConvertible
        {
            public DeserializedConvertible(V obj, XmlSerializeAttribute attrib)
                : base(obj, attrib.Name)
            {
            }

            public DeserializedConvertible(V obj, XName attrName)
                : base(obj, attrName)
            {
            }

            public override XObject Serialize()
            {
                return new XAttribute(Name, DeserializedObject);
            }
        }
    }
}
