using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedCustomObject(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedCustomObject<>).MakeGenericType(typeof(T), typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedCustomObject<V> : DeserializedXmlObject<V>
            where V : IXmlSerializableObject
        {
            public MethodInfo ToXmlMethod
            {
                get
                {
                    MethodInfo method;
                    try
                    {
                        method = typeof(V).GetMethod("ToXml", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(XName) }, null);
                    }
                    catch (AmbiguousMatchException)
                    {
                        try
                        {
                            method = typeof(V).GetMethod("ToXml", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(XName) }, null);
                        }
                        catch (AmbiguousMatchException)
                        {
                            method = null;
                        }
                    }
                    if (method == null)
                        return null;
                    if (!typeof(XObject).IsAssignableFrom(method.ReturnType))
                        return null;
                    return method;
                }
            }

            public DeserializedCustomObject(V obj, XName name)
                : base(obj, name)
            {
            }

            public override XObject Serialize()
            {
                MethodInfo toXmlMethod = ToXmlMethod;
                if (toXmlMethod == null)
                    return SerializeDefault();
                return (XObject)toXmlMethod.Invoke(DeserializedObject, new object[] { Name });
            }

            private XObject SerializeDefault()
            {
                XElement element = new XElement(Name);
                foreach (MemberInfo member in typeof(V).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (member.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length > 0)
                        continue; // skip anything compiler generated
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
