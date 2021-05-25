using System;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;
using System.Runtime.CompilerServices;

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
            where V : IXmlSerializableObject
        {
            public MethodInfo FromXmlMethod
            {
                get
                {
                    MethodInfo method;
                    try
                    {
                        method = typeof(V).GetMethod("FromXml", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(XElement), typeof(XName) }, null);
                    }
                    catch (AmbiguousMatchException)
                    {
                        try
                        {
                            method = typeof(V).GetMethod("FromXml", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(XElement), typeof(XName) }, null);
                        }
                        catch (AmbiguousMatchException)
                        {
                            method = null;
                        }
                    }
                    if (method == null)
                        return null;
                    if (!typeof(V).IsAssignableFrom(method.ReturnType))
                        return null;
                    return method;
                }
            }

            public SerializedCustomObject(XElement node, XName name, V defaultValue)
                : base(node, name, defaultValue)
            {
            }

            public override V Deserialize()
            {
                MethodInfo fromXml = FromXmlMethod;
                if (fromXml == null)
                {
                    if (typeof(V).IsValueType)
                        throw new UnsupportedTypeException(typeof(V), "Value types need an explicit deserialization method defined");
                    return DefaultDeserialize();
                }
                return (V)fromXml.Invoke(null, new object[] { Node, Name });
            }

            private V DefaultDeserialize()
            {
                XElement node = Name == null ? Node : Node.Element(Name);
                if (node == null)
                    return DefaultValue;

                V obj = Activator.CreateInstance<V>();
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
