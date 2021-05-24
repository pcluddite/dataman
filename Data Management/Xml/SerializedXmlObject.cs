using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal interface ISerializedXmlObject
    {
        object Deserialize();
    }

    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        public XElement Node { get; private set; }
        public XName AttributeName { get; private set; }
        public T DefaultValue { get; private set; }

        protected SerializedXmlObject(XElement node, XName attrName, T defaultValue)
        {
            Node = node;
            AttributeName = attrName;
            DefaultValue = defaultValue;
        }

        public abstract T Deserialize();

        public static ISerializedXmlObject CreateSerializedObject(XElement node)
        {
            return CreateSerializedObject(node, null, default(T));
        }

        public static ISerializedXmlObject CreateSerializedObject(XElement node, XmlSerializeAttribute attrib)
        {
            return CreateSerializedObject(node, attrib.Name, (T)attrib.Default);
        }

        public static ISerializedXmlObject CreateSerializedObject(XElement node, XName name, T defaultValue)
        {
            Type memberType = typeof(T);
            if (typeof(IXmlSerializable).IsAssignableFrom(memberType))
            {
                return CreateSerializedCustomObject(node, name, defaultValue);
            }
            else if (memberType.IsArray)
            {
                return CreateSerializedArray(node, name, defaultValue);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(ICollection<>)))
            {
                return CreateSerializedGenericCollection(node, name, defaultValue);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                return CreateSerializedCollection(node, name, defaultValue);
            }
            else if (typeof(object) == memberType)
            {
                XAttribute typeAttr = node.Attribute("t");
                if (typeAttr == null)
                    throw new UnregisteredTypeException(node.Name);
                if (typeAttr.Value == "null")
                    return CreateSerializedNullObject(node, name);
                Type foundType = Type.GetType(typeAttr.Value, true);
                return XmlSerializer.CreateSerializedObject(foundType, node, name, defaultValue);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return CreateSerializedConvertible(node, name, defaultValue);
            }
            throw new UnsupportedTypeException(typeof(T));
        }

        #region ISerializedXmlObject Members

        object ISerializedXmlObject.Deserialize()
        {
            return Deserialize();
        }

        #endregion
    }
}
