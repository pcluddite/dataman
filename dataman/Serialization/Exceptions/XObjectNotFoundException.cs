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

using System.Xml.Linq;

namespace Baxendale.Serialization
{
    internal class XObjectNotFoundException : SerializationException
    {
        public string Name { get; }

        public XObjectNotFoundException(XObject source, XName name)
            : this(source, name?.ToString())
        {
        }

        public XObjectNotFoundException(XObject source, string name)
            : base(source, $"{name} was not found in the current element")
        {
            Name = name;
        }
    }
}