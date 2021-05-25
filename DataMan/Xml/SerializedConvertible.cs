using System;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedConvertible(XElement node, XName name, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedConvertible<>).MakeGenericType(typeof(T), typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, name, defaultValue);
        }

        private class SerializedConvertible<V> : SerializedXmlObject<V>
            where V : IConvertible
        {
            public SerializedConvertible(XElement node, XName name, V defaultValue)
                : base(node, name, defaultValue)
            {
            }

            public override V Deserialize()
            {
                XAttribute attr = Node.Attribute(Name);
                if (attr == null)
                    return DefaultValue;
                return (V)Convert.ChangeType(attr.Value, typeof(V));
            }
        }
    }
}
