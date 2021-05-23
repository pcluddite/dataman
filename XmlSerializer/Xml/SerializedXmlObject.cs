using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Reflection;
using System.Collections.Generic;

namespace Baxendale.DataManagement.Xml
{
    internal interface ISerializedXmlObject
    {
        object Deserialize();
        Array DeserializeArray(int rank);
        object DeserializeGenericCollection(Type collectionType);
        object DeserializeCollection(Type collectionType);
    }

    internal class SerializedXmlObject<T> : ISerializedXmlObject
    {
        public XElement Node { get; private set; }
        public XName AttributeName { get; private set; }
        public T DefaultValue { get; private set; }

        public SerializedXmlObject(XElement node)
            : this(node, node.Name, default(T))
        {
        }

        public SerializedXmlObject(XElement node, XmlSerializeAttribute attrib)
            : this(node, attrib.Name, (T)attrib.Default)
        {
        }

        public SerializedXmlObject(XElement node, XName attrName, T defaultValue)
        {
            Node = node;
            AttributeName = attrName;
            DefaultValue = defaultValue;
        }

        public T Deserialize()
        {
            Type memberType = typeof(T);
            if (typeof(IXmlSerializable).IsAssignableFrom(memberType))
            {
                return DeserializeCustomObject();
            }
            else
            {
                return DeserializeFrameworkObject();
            }
        }

        public T DeserializeCustomObject()
        {
            T obj = Activator.CreateInstance<T>();
            foreach (MemberInfo member in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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

                ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializerObject(member.GetReturnType(), Node, attrib);
                member.SetValue(obj, xmlObj.Deserialize());
            }
            return obj;
        }

        public T DeserializeFrameworkObject()
        {
            Type memberType = typeof(T);
            if (memberType.IsArray)
            {
                XElement arrNode = Node.Element(AttributeName);
                if (arrNode == null)
                    return DefaultValue;
                ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializerObject(memberType.GetElementType(), arrNode);
                return (T)((object)xmlObj.DeserializeArray(memberType.GetArrayRank()));
            }
            else if (memberType.IsSubClassOfGeneric(typeof(ICollection<>)))
            {
                XElement collectionNode = Node.Element(AttributeName);
                if (collectionNode == null)
                    return DefaultValue;
                ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializerObject(memberType.GetGenericArguments()[0], collectionNode);
                return (T)xmlObj.DeserializeGenericCollection(memberType);
            }
            
            if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                XElement collectionNode = Node.Element(AttributeName);
                if (collectionNode == null)
                    return DefaultValue;
                ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializerObject(typeof(object), collectionNode);
                return (T)xmlObj.DeserializeCollection(memberType);
            }
            else if (typeof(object) == memberType)
            {
                XAttribute typeAttr = Node.Attribute("t");
                if (typeAttr == null)
                    throw new UnregisteredTypeException(Node.Name);
                Type foundType = Type.GetType(typeAttr.Value, true);
                ISerializedXmlObject xmlObj = XmlSerializer.CreateSerializerObject(foundType, Node, AttributeName,  DefaultValue);
                return (T)xmlObj.Deserialize();
            }
            else if (Node.Attribute(AttributeName) == null)
            {
                return DefaultValue;
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return (T)Convert.ChangeType(Node.Attribute(AttributeName).Value, memberType);
            }
            throw new UnsupportedTypeException(typeof(T));
        }

        public Array DeserializeArray(int rank)
        {
            DynamicArray<T> arr = new DynamicArray<T>(new int[rank]);
            int[] indices = arr.DecrementIndex(arr.LowerBound);
            foreach (XElement child in Node.Elements("a"))
            {
                XAttribute indexAttribute = child.Attribute("i");
                if (indexAttribute == null)
                {
                    indices = arr.IncrementIndex(indices);
                }
                else
                {
                    SetIndices(indices, indexAttribute.Value);
                }
                arr[indices] = new SerializedXmlObject<T>(child, "v", default(T)).Deserialize();
            }
            return arr.ToArray();
        }

        private void SetIndices(int[] indices, string value)
        {
            int start = 0;
            int stop = 0;
            int dim = 0;

            for (; dim < indices.Length; ++dim)
                indices[dim] = 0;

            for (dim = 0; stop <= value.Length; ++stop)
            {
                if (stop == value.Length || value[stop] == ',')
                {
                     indices[dim++] = int.Parse(value.Substring(start, stop - start));
                     start = stop + 1;
                }
            }
        }

        public ICollection<T> DeserializeGenericCollection(Type collectionType)
        {
            ICollection<T> collection = (ICollection<T>)Activator.CreateInstance(collectionType);
            if (collection.IsReadOnly)
                throw new UnsupportedTypeException(typeof(ICollection<T>));
            foreach (XElement child in Node.Elements("a"))
            {
                collection.Add(new SerializedXmlObject<T>(child, "v", default(T)).Deserialize());
            }
            return collection;
        }

        public ICollection DeserializeCollection(Type collectionType)
        {
            ICollection collection = (ICollection)Activator.CreateInstance(collectionType);
            if (collection.IsReadOnly() == true)
                throw new UnsupportedTypeException(collectionType);
            foreach (XElement child in Node.Elements("a"))
            {
                collection.Add(new SerializedXmlObject<object>(child, "v", null).Deserialize());
            }
            return collection;
        }

        #region ISerializedXmlObject Members

        object ISerializedXmlObject.Deserialize()
        {
            return Deserialize();
        }

        object ISerializedXmlObject.DeserializeGenericCollection(Type collectionType)
        {
            return DeserializeGenericCollection(collectionType);
        }

        object ISerializedXmlObject.DeserializeCollection(Type collectionType)
        {
            return DeserializeCollection(collectionType);
        }

        #endregion
    }
}
