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
        
        protected DeserializedXmlObject(T obj, XName name)
        {
            DeserializedObject = obj;
            Name = name;
        }

        public abstract XObject Serialize();

        public static IDeserializedXmlObject CreateDeserializedObject(T obj, XmlSerializeAttribute attrib)
        {
            return CreateDeserializedObject(obj, attrib.Name);
        }

        public static IDeserializedXmlObject CreateDeserializedObject(T obj, XName name)
        {
            Type memberType = typeof(T);
            if (!memberType.IsValueType && ((object)obj) == null)
            {
                return CreateDeserializedNullObject(name);
            }
            else if (typeof(IXmlSerializableObject).IsAssignableFrom(memberType))
            {
                return CreateDeserializedCustomObject(obj, name);
            }
            else if (memberType.IsArray)
            {
                return CreateDeserializedArray(obj, name);
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
