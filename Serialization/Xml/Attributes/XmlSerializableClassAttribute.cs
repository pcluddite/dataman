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

namespace Baxendale.Data.Xml
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class XmlSerializableClassAttribute : Attribute
    {
        /// <summary>
        /// The method to use for deserializing. This needs to be public and static and take the parameters XElement
        /// with a return type of the current class. If this is null, FromXml(XElement) will be used.
        /// Example: <code>public static MyObject FromXml(XElement element)</code>
        /// </summary>
        public string DeserializeMethodName { get; set; }

        /// <summary>
        /// The method to use for serializing. This needs to be an instance method and take an XName object and return an object
        /// that derives from XObject
        /// Example: <code>public XElement ToXml(XName name)</code>
        /// </summary>
        public string SerializeMethodName { get; set; }

        /// <summary>
        /// Serialize all members and not just the ones marked with the <code>XmlSerializableFieldAttribute</code> or <code>XmlSerializablePropertyAttribute</code>
        /// Setting this will override settings for <code>AllProperties</code> and <code>AllFields</code>
        /// This is false by default.
        /// </summary>
        public bool AllMembers
        {
            get
            {
                return AllFields && AllProperties;
            }
            set
            {
                AllFields = value;
                AllProperties = value;
            }
        }

        /// <summary>
        /// Serialize all properties and not just the ones marked with the <code>XmlSerializablePropertyAttribute</code>
        /// This is false by default.
        /// </summary>
        public bool AllProperties { get; set; } = false;

        /// <summary>
        /// Serialize all fields and not just the ones marked with the <code>XmlSerializableFieldAttribute</code>
        /// This is false by default.
        /// </summary>
        public bool AllFields { get; set; } = false;

        /// <summary>
        /// Serialize all members whether or not their value is equal to <code>default(T)</code> where <code>T</code> is the member type.
        /// This is true by default.
        /// </summary>
        public bool SerializeDefault { get; set; } = true;

        /// <summary>
        /// When true, overrides any option that conflicts with an attribute option applied to a member.
        /// This is true by default.
        /// </summary>
        public bool OverrideMemberOptions { get; set; } = true;

        public XmlSerializableClassAttribute()
        {
        }
    }
}
