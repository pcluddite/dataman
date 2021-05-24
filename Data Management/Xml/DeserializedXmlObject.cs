using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Baxendale.DataManagement.Reflection;

namespace Baxendale.DataManagement.Xml
{
    internal interface IDeserializedXmlObject
    {
        XObject Serialize();
    }

    internal abstract partial class DeserializedXmlObject<T> : IDeserializedXmlObject
    {
        public virtual T DeserializedObject { get; protected set; }
        public virtual XName Name { get; protected set; }

        protected DeserializedXmlObject(T obj, XmlSerializeAttribute attrib)
            : this(obj, attrib.Name)
        {
        }

        protected DeserializedXmlObject(T obj, XName attrName)
        {
            DeserializedObject = obj;
            Name = attrName;
        }

        public abstract XObject Serialize();

        public static IDeserializedXmlObject CreateDeserializedObject(T obj, XName name)
        {
            Type memberType = typeof(T);
            if (typeof(IXmlSerializable).IsAssignableFrom(memberType))
            {
                return CreateDeserializedCustomObject(obj, name);
            }
            else if (memberType.IsArray)
            {
                return CreateDeserializedArrayObject(obj, name);
            }
            else if (memberType.IsSubClassOfGeneric(typeof(ICollection<>)))
            {
                return CreateDeserializedGenericCollection(obj, name);
            }
            else if (typeof(ICollection).IsAssignableFrom(memberType))
            {
                return CreateDeserializedCollection(obj, name);
            }
            else if (typeof(IConvertible).IsAssignableFrom(memberType))
            {
                return CreateDeserializedConvertible(obj, name);
            }
            throw new UnsupportedTypeException(typeof(T));
        }
    }
}
