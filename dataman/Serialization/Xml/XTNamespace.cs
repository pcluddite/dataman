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
    public class XTNamespace : ISerializedNamespace, IEquatable<XTNamespace>, IEquatable<XNamespace>
    {
        private readonly XNamespace _namespace;

        public string NamespaceName
        {
            get
            {
                return _namespace?.NamespaceName;
            }
        }

        public XTNamespace(XNamespace @namespace)
        {
            _namespace = @namespace;
        }

        public XTNamespace(string namespaceName)
        {
            _namespace = XNamespace.Get(namespaceName);
        }

        public override string ToString()
        {
            return _namespace?.ToString() ?? "null";
        }

        public override int GetHashCode()
        {
            return _namespace?.GetHashCode() ?? 0;
        }

        #region IEquatable Members

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == (object)this) return true;
            if (obj is XTNamespace xtns) return this == xtns;
            if (obj is XNamespace ns) return Equals(ns);
            return false;
        }

        public bool Equals(XTNamespace other)
        {
            return this == other;
        }

        bool IEquatable<XNamespace>.Equals(XNamespace other)
        {
            return _namespace == other;
        }

        bool IEquatable<ISerializedNamespace>.Equals(ISerializedNamespace other)
        {
            return Equals(other as XTNamespace);
        }

        public static bool operator==(XTNamespace left, XTNamespace right)
        {
            if (left == (object)right) return true;
            if (left == (object)null) return false;
            return left._namespace == right?._namespace;
        }

        public static bool operator !=(XTNamespace left, XTNamespace right)
        {
            return !(left == right);
        }

        #endregion

        #region implicit operators

        public static implicit operator XNamespace(XTNamespace xtnamespace)
        {
            return xtnamespace?._namespace;
        }

        public static implicit operator XTNamespace(XNamespace @namespace)
        {
            return new XTNamespace(@namespace);
        }

        public static implicit operator XTNamespace(string namespaceName)
        {
            return new XTNamespace(namespaceName);
        }

        #endregion
    }
}
