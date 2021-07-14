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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.Data.Collections;
using Baxendale.Data.Collections.Concurrent;
using Baxendale.Data.Reflection;

namespace Baxendale.Data.Xml
{
    public sealed partial class XmlSerializer
    {
        public XElement Serialize<T>(T obj)
        {
            XName contentName = GetSerializedTypeName(typeof(T));
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
            XName contentName = GetSerializedTypeName(typeof(T));
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
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
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
            LockingDictionary<Type, IXmlObjectSerializer> cache = _cache;
            Type serializerType = GetObjectSerializerType(typeof(T));
            if (serializerType == null)
                throw new UnsupportedTypeException(typeof(T));

            if (cache == null)
                return (IXmlObjectSerializer<T>)Activator.CreateInstance(serializerType, this);

            IXmlObjectSerializer serializer;
            if (!cache.TryGetValue(typeof(T), out serializer))
                cache.Add(typeof(T), serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this));

            return (IXmlObjectSerializer<T>)serializer;
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
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
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
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        internal T Deserialize<T>(XElement content, XName defaultElementName, XName defaultAttributeName)
        {
            IXmlObjectSerializer<T> serializer = CreateSerializerObject<T>();
            XName contentName = GetSerializedTypeName(typeof(T));
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Deserialize<T>(serializer, content, contentName);
        }

        internal T Deserialize<T>(IXmlObjectSerializer<T> serializer, XElement content, XName contentName)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (serializer.UsesXAttribute)
            {
                XAttribute attribute = content.Attribute(contentName ?? ValueAttributeName);
                if (attribute == null)
                    throw new XObjectNotFoundException(contentName.ToString());
                return serializer.Deserialize(attribute);
            }
            if (contentName != null)
                content = content.Element(contentName);
            return serializer.Deserialize(content);
        }

        public XElement Serialize(Type t, object obj)
        {
            XName contentName = GetSerializedTypeName(t);
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
            XName contentName = GetSerializedTypeName(t);
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
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
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
            LockingDictionary<Type, IXmlObjectSerializer> cache = _cache;
            Type serializerType = GetObjectSerializerType(t);
            if (serializerType == null)
                throw new UnsupportedTypeException(t);

            if (cache == null)
                return (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this);

            IXmlObjectSerializer serializer;
            if (!cache.TryGetValue(t, out serializer))
                cache.Add(t, serializer = (IXmlObjectSerializer)Activator.CreateInstance(serializerType, this));

            return (IXmlObjectSerializer)serializer;
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
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
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
            catch (TargetInvocationException ex)
            {
                throw ex.GetBaseException();
            }
        }

        internal object Deserialize(Type t, XElement content, XName defaultElementName, XName defaultAttributeName)
        {
            IXmlObjectSerializer serializer = CreateSerializerObject(t);
            XName contentName = GetSerializedTypeName(t);
            if (contentName == null)
                contentName = serializer.UsesXAttribute ? defaultAttributeName : defaultElementName;
            return Deserialize(t, serializer, content, contentName);
        }

        internal object Deserialize(Type t, IXmlObjectSerializer serializer, XElement content, XName contentName)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (serializer.UsesXAttribute)
            {
                XAttribute attribute = content.Attribute(contentName ?? ValueAttributeName);
                if (attribute == null)
                    throw new XObjectNotFoundException(contentName.ToString());
                return serializer.Deserialize(attribute);
            }
            if (contentName != null)
                content = content.Element(contentName);
            return serializer.Deserialize(content);
        }
    }
}
