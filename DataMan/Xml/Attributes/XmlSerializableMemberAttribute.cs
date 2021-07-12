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

namespace Baxendale.DataManagement.Xml
{
    public abstract class XmlSerializableMemberAttribute : Attribute
    {
        /// <summary>
        /// The name that should appear in the XML. If this is null, the member name is used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether or not to serialize this member if the value is equal to the default. This will override any setting for the class.
        /// </summary>
        public bool SerializeDefault { get; set; } = true;

        /// <summary>
        /// The default value this should be set to if no value is found in the XML
        /// </summary>
        public object Default { get; set; }
    }

    public class XmlSerializableFieldAttribute : XmlSerializableMemberAttribute
    {
    }

    public class XmlSerializablePropertyAttribute : XmlSerializableMemberAttribute
    {
        /// <summary>
        /// The field that backs this property. If set, any XmlSerializable attributes for the specified
        /// field will be ignored.
        /// </summary>
        public string BackingField { get; set; }
    }
}
