using System;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedNullObject(XElement node, XName name)
        {
            Type serializedXmlObject = typeof(SerializedCustomObject<>).MakeGenericType(typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, name);
        }

        private class SerializedNullObject<V> : SerializedXmlObject<V>
        {
            public SerializedNullObject(XElement node, XName name)
                : base(node, name, default(V))
            {
            }

            public override V Deserialize()
            {
                return default(V);
            }
        }
    }
}
