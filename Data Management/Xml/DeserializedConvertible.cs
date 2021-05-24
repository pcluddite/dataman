using System;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedConvertible(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedArray<>).MakeGenericType(typeof(T), typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedConvertible<V> : DeserializedXmlObject<V>
            where V : IConvertible
        {
            public DeserializedConvertible(V obj, XName name)
                : base(obj, name)
            {
            }

            public override XObject Serialize()
            {
                return new XAttribute(Name, DeserializedObject);
            }
        }
    }
}
