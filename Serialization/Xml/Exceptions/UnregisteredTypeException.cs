﻿//
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
    public class UnregisteredTypeException : SerializerException
    {
        public string ElementName { get; private set; }
        public Type UnregisteredType { get; private set; }

        public UnregisteredTypeException(string elementName)
            : base($"<{elementName}> was not registered for deserialization and the type of object is not known")
        {
            ElementName = elementName;
        }

        public UnregisteredTypeException(Type type)
            : base($"{type.FullName} was not registered for serialization and no XML name is associated with it")
        {
            UnregisteredType = type;
        }
    }
}
