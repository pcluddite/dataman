using System;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedCustomObject(XElement node, XName name, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedCustomObject<>).MakeGenericType(typeof(T), typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, name, defaultValue);
        }

        private class SerializedCustomObject<V> : SerializedXmlObject<V>
            where V : IXmlSerializable
        {
            public SerializedCustomObject(XElement node, XName name, V defaultValue)
                : base(node, name, defaultValue)
            {
            }

            public override V Deserialize()
            {
                XElement node = AttributeName == null ? Node : Node.Element(AttributeName);
                if (node == null)
                    return DefaultValue;

                V obj = Activator.CreateInstance<V>();
                foreach (MemberInfo member in typeof(V).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                        continue; // skip anything that's not a field or property
                    if (member.GetCustomAttributes(typeof(XmlDoNotSerializeAttribute), true).Length > 0)
                        continue; // skip fields tagged with this attribute

                    Type memberType = member.GetReturnType();
                    object[] attribs = member.GetCustomAttributes(typeof(XmlSerializeAttribute), true);
                    XmlSerializeAttribute attrib = null;
                    if (attribs.Length > 0)
                    {
                        attrib = (XmlSerializeAttribute)attribs[attribs.Length - 1];
                        if (attrib.Name == null)
                            attrib.Name = member.Name;
                        if (attrib.Default == null)
                            attrib.Default = memberType.CreateDefault();
                    }
                    else
                    {
                        attrib = new XmlSerializeAttribute() { Name = member.Name, Default = memberType.CreateDefault() };
                    }

                    ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializedObject(member.GetReturnType(), node, attrib);
                    member.SetValue(obj, xmlObj.Deserialize());
                }
                return obj;
            }
        }
    }
}
