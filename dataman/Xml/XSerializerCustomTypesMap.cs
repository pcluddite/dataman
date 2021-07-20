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
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Baxendale.Data.Collections;

namespace Baxendale.Data.Xml
{
    internal class XSerializerCustomTypesMap : IDictionary<Type, XName>
    {
        private readonly OneToManyBidictionary<Type, XmlCustomTypeMetaData> _types = new OneToManyBidictionary<Type, XmlCustomTypeMetaData>();
        private readonly object _locker = new object();

        public XmlSerializer XmlSerializer { get; }

        public XName this[Type key]
        {
            get
            {
                lock (_locker) return _types[key].Name;
            }
            set
            {
                lock (_locker)
                {
                    XmlCustomTypeMetaData meta = _types[key];
                    _types[key] = new XmlCustomTypeMetaData(key, value, meta.ObjectSerializer);
                }
            }
        }

        public ICollection<Type> Keys => _types.Keys;

        public IEnumerable<XName> Values
        {
            get
            {
                foreach (XmlCustomTypeMetaData meta in _types.Values)
                    yield return meta.Name;
            }
        }

        public int Count
        {
            get
            {
                lock (_locker) return _types.Count;
            }
        }

        public XSerializerCustomTypesMap(XmlSerializer serializer)
        {
            XmlSerializer = serializer;
        }

        public void Add<T>(XName name)
            where T : IXmlSerializableObject
        {
            lock (_locker) _types.Add(typeof(T), XmlCustomTypeMetaData.CreateMetaData<T>(XmlSerializer, name));
        }

        public void Add<T>(XName name, ToXElement<T> toXmlDelegate, FromXElement<T> fromXmlDelegate)
        {
            lock (_locker) _types.Add(typeof(T), XmlCustomTypeMetaData.CreateMetaData(XmlSerializer, name, toXmlDelegate, fromXmlDelegate));
        }

        public void Add<T>(XName name, ToXAttribute<T> toXmlDelegate, FromXAttribute<T> fromXmlDelegate)
        {
            lock (_locker) _types.Add(typeof(T), XmlCustomTypeMetaData.CreateMetaData(XmlSerializer, name, toXmlDelegate, fromXmlDelegate));
        }

        public XName GetXName(Type type)
        {
            lock (_locker)
            {
                XmlCustomTypeMetaData meta;
                if (_types.TryGetValue(type, out meta))
                    return meta.Name;
                return null;
            }
        }

        public Type GetTypeFromXName(XName name)
        {
            lock (_locker)
            {
                Type type;
                if (_types.TryGetKey(new XmlCustomTypeMetaData(name), out type))
                    return type;
                return null;
            }
        }

        public Type GetTypeFromXElement(XElement node)
        {
            if (node.Name == XmlSerializer.ElementName)
            {
                string attrValue = node.Attribute(XmlSerializer.TypeAttributeName)?.Value;
                if (attrValue == null)
                    throw new UnregisteredTypeException(node.Name.ToString());
                if (attrValue == "null")
                    return typeof(object);
                return Type.GetType(attrValue, throwOnError: true);
            }

            Type t = GetTypeFromXName(node.Name);
            if (t == null)
                throw new UnregisteredTypeException(node.Name.ToString());
            return t;
        }

        public IXObjectSerializer<T> GetCustomSerializer<T>()
        {
            lock (_locker)
            {
                XmlCustomTypeMetaData meta;
                if (_types.TryGetValue(typeof(T), out meta))
                    return (IXObjectSerializer<T>)meta.ObjectSerializer;
                return null;
            }
        }

        public IXObjectSerializer GetCustomSerializer(Type t)
        {
            lock (_locker)
            {
                XmlCustomTypeMetaData meta;
                if (_types.TryGetValue(t, out meta))
                    return meta.ObjectSerializer;
                return null;
            }
        }

        public void Clear()
        {
            lock (_locker) _types.Clear();
        }

        public bool ContainsKey(Type key)
        {
            lock (_locker) return _types.ContainsKey(key);
        }

        public bool Remove(Type key)
        {
            lock (_locker) return _types.RemoveByKey(key);
        }

        public IEnumerator<KeyValuePair<Type, XName>> GetEnumerator()
        {
            foreach (var kv in _types)
                yield return new KeyValuePair<Type, XName>(kv.Key, kv.Value.Name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDictionary

        void IDictionary<Type, XName>.Add(Type t, XName name)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (name == null) throw new ArgumentNullException(nameof(name));

            MethodInfo factory = GetType().GetMethod(nameof(Add), new[] { typeof(Type) });
            factory.MakeGenericMethod(t).Invoke(null, new object[] { name });
        }

        ICollection<XName> IDictionary<Type, XName>.Values
        {
            get
            {
                lock (_locker) return Values.ToList();
            }
        }

        bool IDictionary<Type, XName>.TryGetValue(Type key, out XName value)
        {
            lock (_locker)
            {
                XmlCustomTypeMetaData meta;
                value = null;
                if (!_types.TryGetValue(key, out meta))
                    return false;
                value = meta.Name;
                return true;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<Type, XName>>

        bool ICollection<KeyValuePair<Type, XName>>.IsReadOnly => false;

        void ICollection<KeyValuePair<Type, XName>>.Add(KeyValuePair<Type, XName> item)
        {
            ((IDictionary<Type, XName>)this).Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<Type, XName>>.Contains(KeyValuePair<Type, XName> item)
        {
            lock (_locker)
            {
                IEnumerable<XmlCustomTypeMetaData> metas;
                if (!_types.TryGetValues(item.Key, out metas))
                    return false;
                foreach (XmlCustomTypeMetaData meta in metas)
                {
                    if (meta.Name == item.Value)
                        return true;
                }
                return false;
            }
        }

        void ICollection<KeyValuePair<Type, XName>>.CopyTo(KeyValuePair<Type, XName>[] array, int arrayIndex)
        {
            lock (_locker)
            {
                foreach (var kv in this)
                    array[arrayIndex++] = kv;
            }
        }

        bool ICollection<KeyValuePair<Type, XName>>.Remove(KeyValuePair<Type, XName> item)
        {
            lock (_locker)
            {
                IEnumerable<XmlCustomTypeMetaData> metas;
                if (!_types.TryGetValues(item.Key, out metas))
                    return false;
                foreach (XmlCustomTypeMetaData meta in metas)
                {
                    if (meta.Name == item.Value)
                        return _types.RemoveByValue(meta);
                }
                return false;
            }
        }

        #endregion
    }
}
