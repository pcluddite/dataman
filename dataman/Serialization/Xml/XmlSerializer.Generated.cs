//
//    DataMan - Supplemental library for managing data types and handling serialization
//    Copyright (C) 2021 Timothy Baxendale
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
//    USA
//
using System;
using System.Xml.Linq;
using Baxendale.Serialization;

namespace Baxendale.Data.Xml
{
    public sealed partial class XmlSerializer
    {
        public XElement Serialize<T>(T obj)
        {
            XName contentName = SerializableTypes.GetXName(typeof(T));
            if (contentName == null)
                throw new UnregisteredTypeException(typeof(T));
            return Serialize(obj, contentName);
        }

        public XElement Serialize<T>(T obj, XName name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return Serialize(CreateSerializerObject(obj), obj, name, ElementName);
        }

        internal XElement Serialize<T>(T obj, XName defaultElementName, XName defaultAttributeName)
        {
            IXObjectSerializer serializer = CreateSerializerObject(obj);
            XName contentName = SerializableTypes.GetXName(typeof(T));
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Serialize(serializer, obj, contentName, defaultElementName);
        }

        internal XElement Serialize<T>(IXObjectSerializer serializer, T obj, XName contentName, XName defaultElementName)
        {
            XObject content;
#if !DEBUG
            try
            {
#endif
                content = serializer.Serialize(obj, contentName);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                throw new XmlSerializationException(new XElement(contentName), ex.GetBaseException());
            }
#endif
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(defaultElementName ?? ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        internal IXObjectSerializer CreateSerializerObject<T>(T obj)
        {
            if (obj == null)
                return new XmlNullSerializer<T>(this);
            Type runtimeType = obj.GetType();
            if (runtimeType == typeof(T))
                return CreateSerializerObject<T>();
            // runtime type does not match object type; this is a subclass or inherits an interface
            return CreateSerializerObject(runtimeType);
        }

        internal IXObjectSerializer CreateSerializerObject<T>()
        { 
            IXObjectSerializer serializer = SerializableTypes.GetCustomSerializer<T>();
            if (serializer != null)
                return serializer;

            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));

            return (IXObjectSerializer)Activator.CreateInstance(serializerType, this);
        }

        public T Deserialize<T>(XElement content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXObjectSerializer serializer = CreateSerializerObject<T>();
            if (serializer.UsesXAttribute)
                throw new UnsupportedTypeException(typeof(T));
#if !DEBUG
            try
            {
#endif
                return (T)serializer.Deserialize(content);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
#endif
        }

        public T Deserialize<T>(XAttribute content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXObjectSerializer serializer = CreateSerializerObject<T>();
            if (!serializer.UsesXAttribute)
                throw new UnsupportedTypeException(typeof(T));
#if !DEBUG
            try
            {
#endif
                return (T)serializer.Deserialize(content);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
#endif
        }

        internal T Deserialize<T>(XElement content, XName defaultElementName, XName defaultAttributeName)
        {
            Type runtimeType = typeof(T);
            if (typeof(T).IsAbstract || typeof(T).IsInterface)
                runtimeType = SerializableTypes.GetTypeFromXElement(content);
            IXObjectSerializer serializer = CreateSerializerObject(runtimeType);
            XName contentName = SerializableTypes.GetXName(typeof(T));
            if (contentName == null && typeof(T) != runtimeType)
                contentName = SerializableTypes.GetXName(runtimeType);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Deserialize<T>(serializer, content, contentName);
        }

        internal T Deserialize<T>(IXObjectSerializer serializer, XElement content, XName contentName)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (serializer.UsesXAttribute)
            {
                if (contentName == null)
                    contentName = ValueAttributeName;
                XAttribute attribute = content.Attribute(contentName);
                if (attribute == null)
                    throw new XObjectNotFoundException(content, contentName);
                return (T)serializer.Deserialize(attribute);
            }
#if !DEBUG
            try
            {
#endif
                return (T)serializer.Deserialize(content);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
#endif
        }

        public XElement Serialize(Type t, object obj)
        {
            XName contentName = SerializableTypes.GetXName(t);
            if (contentName == null)
                throw new UnregisteredTypeException(t);
            return Serialize(t, obj, contentName);
        }

        public XElement Serialize(Type t, object obj, XName name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return Serialize(t, CreateSerializerObject(t, obj), obj, name, ElementName);
        }

        internal XElement Serialize(Type t, object obj, XName defaultElementName, XName defaultAttributeName)
        {
            IXObjectSerializer serializer = CreateSerializerObject(obj);
            XName contentName = SerializableTypes.GetXName(t);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Serialize(t, serializer, obj, contentName, defaultElementName);
        }

        internal XElement Serialize(Type t, IXObjectSerializer serializer, object obj, XName contentName, XName defaultElementName)
        {
            XObject content;
#if !DEBUG
            try
            {
#endif
                content = serializer.Serialize(obj, contentName);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                throw new XmlSerializationException(new XElement(contentName), ex.GetBaseException());
            }
#endif
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(defaultElementName ?? ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        internal IXObjectSerializer CreateSerializerObject(Type t, object obj)
        {
            if (obj == null)
                return (IXObjectSerializer)Activator.CreateInstance(typeof(XmlNullSerializer<>).MakeGenericType(t), this);
            return CreateSerializerObject(t);
        }

        internal IXObjectSerializer CreateSerializerObject(Type t)
        { 
            IXObjectSerializer serializer = SerializableTypes.GetCustomSerializer(t);
            if (serializer != null)
                return serializer;

            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);

            return (IXObjectSerializer)Activator.CreateInstance(serializerType, this);
        }

        public object Deserialize(Type t, XElement content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXObjectSerializer serializer = CreateSerializerObject(t);
            if (serializer.UsesXAttribute)
                throw new UnsupportedTypeException(t);
#if !DEBUG
            try
            {
#endif
                return serializer.Deserialize(content);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
#endif
        }

        public object Deserialize(Type t, XAttribute content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXObjectSerializer serializer = CreateSerializerObject(t);
            if (!serializer.UsesXAttribute)
                throw new UnsupportedTypeException(t);
#if !DEBUG
            try
            {
#endif
                return serializer.Deserialize(content);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
#endif
        }

        internal object Deserialize(Type t, XElement content, XName defaultElementName, XName defaultAttributeName)
        {
            Type runtimeType = t;
            if (t.IsAbstract || t.IsInterface)
                runtimeType = SerializableTypes.GetTypeFromXElement(content);
            IXObjectSerializer serializer = CreateSerializerObject(runtimeType);
            XName contentName = SerializableTypes.GetXName(t);
            if (contentName == null && t != runtimeType)
                contentName = SerializableTypes.GetXName(runtimeType);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Deserialize(t, serializer, content, contentName);
        }

        internal object Deserialize(Type t, IXObjectSerializer serializer, XElement content, XName contentName)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (serializer.UsesXAttribute)
            {
                if (contentName == null)
                    contentName = ValueAttributeName;
                XAttribute attribute = content.Attribute(contentName);
                if (attribute == null)
                    throw new XObjectNotFoundException(content, contentName);
                return serializer.Deserialize(attribute);
            }
#if !DEBUG
            try
            {
#endif
                return serializer.Deserialize(content);
#if !DEBUG
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
#endif
        }
    }
}
