using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;
using System.Xml;
using System.Collections;

namespace VirtualFlashCards.Xml
{
    public static class XmlSerializer
    {
        private static readonly IDictionary<string, Type> SerializableTypes = new Dictionary<string, Type>();

        public static void RegisterType<T>(string name) where T : IXmlSerializable, new()
        {
            SerializableTypes[name] = typeof(T);
        }

        public static void RegisterType<T>(XName name) where T : IXmlSerializable, new()
        {
            SerializableTypes[name.ToString()] = typeof(T);
        }

        public static object Deserialize(this XElement node)
        {
            Type t = SerializableTypes[node.Name.ToString()];
            if (t == null)
                ThrowUnknownTag(node.Name);
            return Deserialize(node, t);
        }

        public static T Deserialize<T>(this XElement node) where T : IXmlSerializable, new()
        {
            Type t = SerializableTypes[node.Name.ToString()];
            if (t == null)
                ThrowUnknownTag(node.Name);
            object obj = node.Deserialize(typeof(T));
            if (obj == null)
                return default(T);
            return (T)obj;
        }

        private static object Deserialize(this XElement node, Type t)
        {
            object obj = Activator.CreateInstance(t);
            foreach (MemberInfo member in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (member.MemberType != MemberTypes.Field && member.MemberType != MemberTypes.Property)
                    continue; // skip anything that's not a field or property
                if (member.GetCustomAttributes(typeof(XmlDoNotSerializeAttribute), true).Length > 0)
                    continue; // skip fields tagged with this attribute
                
                string name = null;
                object defaultValue = null;

                object[] attribs = member.GetCustomAttributes(typeof(XmlSerializeAttribute), true);
                if (attribs.Length > 0)
                {
                    XmlSerializeAttribute attr = (XmlSerializeAttribute)attribs[attribs.Length - 1];
                    name = attr.Name;
                    defaultValue = attr.DefaultValue;
                }
                if (name == null)
                {
                    name = member.Name;
                }

                Type fieldType = member.GetReturnType();
                member.SetValue(obj, node.Deserialize(name, fieldType, () => defaultValue == null ? fieldType.CreateDefault() : defaultValue));
            }

            return obj;
        }

        private static object Deserialize(this XElement node, XName attrName, Type memberType, Func<object> defaultValue)
        {
            if (node.Attribute(attrName) == null)
                return defaultValue.Invoke();
            if (memberType.IsArray)
            {
                return node.DeserializeArray(attrName, memberType);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return Convert.ChangeType(node.Attribute(attrName).Value, memberType);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {

            }
            return null;
        }

        private static Array DeserializeArray(this XElement node, XName attrName, Type memberType)
        {
            Array array = (Array)Activator.CreateInstance(memberType);
            int rank = array.Rank;
            long[] indices = new long[rank];
            node.DeserializeArray(array, memberType.GetElementType(), indices, 0);
            return array;
        }

        private static void DeserializeArray(this XElement node, Array arr, Type memberType, long[] indices, int nestLevel)
        {
            foreach (XElement child in node.Elements("{q}a"))
            {
                if (indices[nestLevel] > arr.GetUpperBound(nestLevel))
                    throw new XmlException("There are more nodes than the size of the array dimension");
                if (nestLevel < arr.Rank)
                {
                    child.DeserializeArray(arr, memberType, indices, nestLevel + 1);
                }
                else
                {
                    arr.SetValue(child.Deserialize(memberType), indices);
                }
                ++indices[nestLevel];
            }
            //while (indices[0] < arr.GetLongLength(0))
            //{
            //    array.SetValue(obj, indices);
            //    ++indices[rank - 1];
            //    for (int dim = rank - 1; dim > 0; --dim)
            //    {
            //        if (indices[dim] == array.GetLongLength(dim))
            //        {
            //            for (int i = dim; i < rank; ++i)
            //                indices[i] = 0;
            //            ++indices[dim - 1];
            //        }
            //    }
            //}
        }

        private static void ThrowUnknownTag(XName name)
        {
            ThrowUnknownTag(name.ToString());
        }

        private static void ThrowUnknownTag(string tagName)
        {
            throw new XmlException("<" + tagName + "> tag is not recognized as a valid element");
        }
    }
}
