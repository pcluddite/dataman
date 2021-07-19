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
    public class UnsupportedTypeException : SerializerException
    {
        public Type UnsupportedType { get; private set; }

        public UnsupportedTypeException(Type type)
            : this(type, null)
        {
        }

        public UnsupportedTypeException(string typeName)
            : this(typeName, null)
        {
        }
        public UnsupportedTypeException(Type type, string message)
            : this(type.Name, message)
        {
            UnsupportedType = type;
        }

        public UnsupportedTypeException(string typeName, string message)
            : base(typeName + " is unsupported for deserialization." + (message == null ? "" : " " + message))
        {
        }
    }
}
