﻿//
//    DataMan - Supplemental library for managing data types and handling serialization
//    Copyright (C) 2021-2022 Timothy Baxendale
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

namespace Baxendale.Serialization
{
    public class SerializationException : Exception
    {
        public ISerializedObject SourceObject { get; }

        public SerializationException(ISerializedObject source)
            : this(source, $"An exception occurred when processing {source?.BaseUri}")
        {
        }

        public SerializationException(ISerializedObject source, Exception innerException)
            : this(source, $"An exception occurred when processing {source?.BaseUri} because of the following: {innerException?.Message}")
        {
        }

        public SerializationException(ISerializedObject source, string message)
            : base(message)
        {
            SourceObject = source;
        }

        public SerializationException(ISerializedObject source, string message, Exception innerException)
            : base(message, innerException)
        {
            SourceObject = source;
        }
    }
}
