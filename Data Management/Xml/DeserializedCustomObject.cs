using System;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedCustomObject(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedCustomObject<>).MakeGenericType(typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedCustomObject<V> : DeserializedXmlObject<V>
            where V : IXmlSerializableObject
        {
            public DeserializedCustomObject(V obj, XmlSerializeAttribute attrib)
                : base(obj, attrib.Name)
            {
            }

            public DeserializedCustomObject(V obj, XName attrName)
                : base(obj, attrName)
            {
            }

            public override XObject Serialize()
            {
                XElement element = new XElement(Name);
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
                    }
                    else
                    {
                        attrib = new XmlSerializeAttribute() { Name = member.Name };
                    }

                    IDeserializedXmlObject xmlObj = XmlSerializer.CreateDeserializedObject(member.GetReturnType(), DeserializedObject, attrib);
                    element.Add(xmlObj.Serialize());
                }
                return element;
            }
        }
    }
}
