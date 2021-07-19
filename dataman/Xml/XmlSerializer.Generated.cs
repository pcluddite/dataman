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
using Baxendale.Data.Collections.Concurrent;

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
            IXmlObjectSerializer<T> serializer = CreateSerializerObject(obj);
            XName contentName = SerializableTypes.GetXName(typeof(T));
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Serialize(serializer, obj, contentName, defaultElementName);
        }

        internal XElement Serialize<T>(IXmlObjectSerializer<T> serializer, T obj, XName contentName, XName defaultElementName)
        {
            XObject content;
            try
            {
                content = serializer.Serialize(obj, contentName);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                throw new XmlSerializationException(new XElement(contentName), ex.GetBaseException());
            }
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(defaultElementName ?? ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        internal IXmlObjectSerializer<T> CreateSerializerObject<T>(T obj)
        {
            if (obj == null)
                return new XmlNullSerializer<T>(this);
            return CreateSerializerObject<T>();
        }

        internal IXmlObjectSerializer<T> CreateSerializerObject<T>()
        { 
            IXmlObjectSerializer<T> serializer = SerializableTypes.GetCustomSerializer<T>();
            if (serializer != null)
                return serializer;

            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));

            return (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType, this);
        }

        public T Deserialize<T>(XElement content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            if (serializer.UsesXAttribute)
                throw new UnsupportedTypeException(typeof(T));
            try
            {
                return serializer.Deserialize(content);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
        }

        public T Deserialize<T>(XAttribute content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            if (!serializer.UsesXAttribute)
                throw new UnsupportedTypeException(typeof(T));
            try
            {
                return serializer.Deserialize(content);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
        }

        internal T Deserialize<T>(XElement content, XName defaultElementName, XName defaultAttributeName)
        {
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            XName contentName = SerializableTypes.GetXName(typeof(T));
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Deserialize<T>(serializer, content, contentName);
        }

        internal T Deserialize<T>(IXmlObjectSerializer<T> serializer, XElement content, XName contentName)
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
            if (contentName != null)
                content = content.Element(contentName);
            try
            {
                return serializer.Deserialize(content);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
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
            IXmlObjectSerializer serializer = CreateSerializerObject(obj);
            XName contentName = SerializableTypes.GetXName(t);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Serialize(t, serializer, obj, contentName, defaultElementName);
        }

        internal XElement Serialize(Type t, IXmlObjectSerializer serializer, object obj, XName contentName, XName defaultElementName)
        {
            XObject content;
            try
            {
                content = serializer.Serialize(obj, contentName);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                throw new XmlSerializationException(new XElement(contentName), ex.GetBaseException());
            }
            if (serializer.UsesXAttribute)
            {
                XElement element = new XElement(defaultElementName ?? ElementName);
                element.Add(content);
                return element;
            }
            return (XElement)content;
        }

        internal IXmlObjectSerializer CreateSerializerObject(Type t, object obj)
        {
            if (obj == null)
                return (IXmlObjectSerializer)Activator.CreateInstance(typeof(XmlNullSerializer<>).MakeGenericType(t), this);
            return CreateSerializerObject(t);
        }

        internal IXmlObjectSerializer CreateSerializerObject(Type t)
        { 
            IXmlObjectSerializer serializer = SerializableTypes.GetCustomSerializer(t);
            if (serializer != null)
                return serializer;

            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);

            return (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this);
        }

        public object Deserialize(Type t, XElement content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            if (serializer.UsesXAttribute)
                throw new UnsupportedTypeException(t);
            try
            {
                return serializer.Deserialize(content);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
        }

        public object Deserialize(Type t, XAttribute content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            if (!serializer.UsesXAttribute)
                throw new UnsupportedTypeException(t);
            try
            {
                return serializer.Deserialize(content);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
        }

        internal object Deserialize(Type t, XElement content, XName defaultElementName, XName defaultAttributeName)
        {
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            XName contentName = SerializableTypes.GetXName(t);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Deserialize(t, serializer, content, contentName);
        }

        internal object Deserialize(Type t, IXmlObjectSerializer serializer, XElement content, XName contentName)
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
            if (contentName != null)
                content = content.Element(contentName);
            try
            {
                return serializer.Deserialize(content);
            }
            catch (Exception ex) when(!(ex is XmlSerializationException))
            {
                if (ex.GetBaseException() is XmlSerializationException)
                    throw ex.GetBaseException();
                throw new XmlSerializationException(content, ex.GetBaseException());
            }
        }
    }
}
