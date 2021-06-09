using System;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;
using System.Runtime.CompilerServices;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedCustomObject(XElement node, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedCustomObject<>).MakeGenericType(typeof(T), typeof(T));
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, defaultValue);
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

            public SerializedCustomObject(XElement node, V defaultValue)
                : base(node, node.Name, defaultValue)
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
                XElement node = Node;
                if (node == null)
                    return DefaultValue;
                V obj = Activator.CreateInstance<V>();
                foreach (MemberInfo member in typeof(V).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (member.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) != null)
                        continue; // skip anything compiler generated
                    if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                        continue; // skip anything that's not a field or property
                    if (member.GetCustomAttribute<XmlDoNotSerializeAttribute>(inherit: true) != null)
                        continue; // skip fields tagged with this attribute
                    Type memberType = member.GetReturnType();
                    XmlSerializeAttribute attrib = member.GetCustomAttribute<XmlSerializeAttribute>(inherit: true);
                    if (attrib == null)
                    {
                        attrib = new XmlSerializeAttribute() { Name = member.Name, Default = memberType.CreateDefault() };
                    }
                    else
                    {
                        if (attrib.Name == null)
                            attrib.Name = member.Name;
                        if (attrib.Default == null)
                            attrib.Default = memberType.CreateDefault();
                    }

                    ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializedObject(memberType, node, attrib);
                    member.SetValue(obj, xmlObj.Deserialize());
                }
                return obj;
            }
        }
    }
}
