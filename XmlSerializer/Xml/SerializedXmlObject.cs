using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal interface ISerializedXmlObject
    {
        object Deserialize();
        object Deserialize(XmlSerializeAttribute attr);
        object DeserializeArray(XName name, int rank, Delegate defaultValue);
    }

    internal class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private XElement node;

        public SerializedXmlObject(XElement node)
        {
            this.node = node;
        }

        public object Deserialize()
        {
            T obj = Activator.CreateInstance<T>();
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
                    if (attr.Name == null)
                        attr.Name = member.Name;
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
            return Deserialize(node, attr.Name, defaultValue);
        }

        public static T Deserialize(XElement node, XName attrName, Func<T> defaultValue)
        {
            Type memberType = typeof(T);
            if (memberType.IsArray)
            {
                ISerializedXmlObject xmlObj = node.CreateSerializerObject(memberType.GetElementType());
                return (T)xmlObj.DeserializeArray(attrName, memberType.GetArrayRank(), defaultValue);
            }
            else if (node.Attribute(attrName) == null)
            {
                return defaultValue.Invoke();
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

        public object DeserializeArray(XName name, int rank, Delegate defaultValue)
        {
            XElement arrNode = node.Element(name);
            if (arrNode == null)
                return defaultValue.DynamicInvoke();
            DynamicArray<T> arr = new DynamicArray<T>(new int[rank]);
            int[] indices = arr.LowerBound;
            foreach (XElement child in arrNode.Elements(XmlSerializer.SpecialNS + "a"))
            {
                XAttribute indexAttribute = child.Attribute(XmlSerializer.SpecialNS + "i");
                if (indexAttribute == null)
                {
                    indices = arr.IncrementIndex(indices);
                }
                else
                {
                    SetIndices(indices, indexAttribute.Value);
                }
                arr[indices] = Deserialize(child, XmlSerializer.SpecialNS + "v", () => default(T));
            }
            return arr.ToArray();
        }

        private void SetIndices(int[] indices, string value)
        {
            int start = 0;
            int stop = 0;
            int dim = 0;
            for (; stop <= value.Length; ++stop)
            {
                if (stop == value.Length || value[stop] == ',')
                {
                     indices[dim++] = int.Parse(value.Substring(start, stop - start));
                     start = stop + 1;
                }
            }
        }
    }
}
