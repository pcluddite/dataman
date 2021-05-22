using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Extensions;

namespace Baxendale.DataManagement.Serialization.Xml
{
    internal interface ISerializedXmlObject
    {
        object Deserialize();
        object Deserialize(XmlSerializeAttribute attr);
        object DeserializeArray(int rank);
    }

    internal class SerializedXmlObject<T> : ISerializedXmlObject
        where T : new()
    {
        private XElement node;

        public SerializedXmlObject(XElement node)
        {
            this.node = node;
        }

        public object Deserialize()
        {
            T obj = new T();
            foreach (MemberInfo member in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                    continue; // skip anything that's not a field or property
                if (member.GetCustomAttributes(typeof(XmlDoNotSerializeAttribute), true).Length > 0)
                    continue; // skip fields tagged with this attribute

                object[] attribs = member.GetCustomAttributes(typeof(XmlSerializeAttribute), true);
                XmlSerializeAttribute attr = null;
                if (attribs.Length > 0)
                {
                    attr = (XmlSerializeAttribute)attribs[attribs.Length - 1];
                }
                else
                {
                    attr = new XmlSerializeAttribute() { Name = member.Name };
                }

                ISerializedXmlObject xmlObj = node.CreateSerializerObject(member.GetReturnType());
                member.SetValue(obj, xmlObj.Deserialize(attr));
            }

            return obj;
        }

        public object Deserialize(XmlSerializeAttribute attr)
        {
            Func<T> defaultValue = () => attr.DefaultValue == null ? default(T) : (T)attr.DefaultValue;
            return Deserialize(attr.Name, defaultValue);
        }

        public T Deserialize(XName attrName, Func<T> defaultValue)
        {
            if (node.Attribute(attrName) == null)
                return defaultValue.Invoke();
            Type memberType = typeof(T);
            if (memberType.IsArray)
            {
                ISerializedXmlObject xmlObj = node.CreateSerializerObject(memberType.GetElementType());
                return (T)xmlObj.DeserializeArray(memberType.GetArrayRank());
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return (T)Convert.ChangeType(node.Attribute(attrName).Value, memberType);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {

            }
            throw new XmlException(typeof(T) + " is unsupported");
        }

        public object DeserializeArray(int rank)
        {
            if (rank == 1)
                return DeserializeSingleDimArray();
            return DeserializeMultiDimArray(rank);
        }

        public Array DeserializeMultiDimArray(int rank)
        {
            DynamicArray<T> arr = new DynamicArray<T>();
            
            return arr.ToArray();
        }

        private T[] DeserializeSingleDimArray()
        {
            List<T> elements = new List<T>();
            foreach (XElement child in node.Elements("{q}a"))
            {
                object o = child.Deserialize();
                if (o == null)
                    elements.Add(default(T));
                if (o.GetType().IsAssignableFrom(typeof(T)))
                    elements.Add((T)o);
            }
            return elements.ToArray();
        }
    }
}
