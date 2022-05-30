//
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

namespace Baxendale.Serialization.Xml
{
    public class XTName : ISerializedName, IEquatable<XTName>, IEquatable<XName>
    {
        private readonly XName _name;

        public string LocalName => _name?.LocalName;
        public string NamespaceName => _name?.NamespaceName;
        public XTNamespace Namespace => _name?.Namespace;

        public XTName(XName xname)
        {
            _name = xname;
        }

        public XTName(string expandedName)
        {
            _name = XName.Get(expandedName);
        }

        public XTName(string localName, string namespaceName)
        {
            _name = XName.Get(localName, namespaceName);
        }

        ISerializedNamespace ISerializedName.Namespace
        {
            get
            {
                return Namespace;
            }
        }

        public bool Equals(string other)
        {
            throw new System.NotImplementedException();
        }

        bool IEquatable<ISerializedName>.Equals(ISerializedName other)
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is XTName) return Equals((XTName)obj);
            if (obj is XName) return Equals((XName)obj);
            return base.Equals(obj);
        }

        public bool Equals(XTName obj)
        {
            return this == obj;
        }

        public bool Equals(XName obj)
        {
            return _name == obj;
        }

        public static bool operator ==(XTName left, XTName right)
        {
            if (left == (object)right) return true;
            return left?._name == right?._name;
        }

        public static bool operator !=(XTName left, XTName right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return _name?.ToString() ?? "null";
        }

        public static implicit operator XTName(XName name)
        {
            return new XTName(name);
        }

        public static implicit operator XName(XTName name)
        {
            return name?._name;
        }
    }
}
