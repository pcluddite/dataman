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
using System.Collections.Generic;
using System.Reflection;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Reflection
{
    public class MethodInfoComparer : IComparer<MethodInfo>
    {
        public virtual IComparer<string> NameComparer { get; }
        public virtual IComparer<Type> DeclaredTypeComparer { get; }
        public virtual IComparer<MethodAttributes> AttributeComparer { get; }
        public virtual IComparer<Type> ReturnTypeComparer { get; }
        public virtual IComparer<ParameterInfo[]> ParameterComparer { get; }

        public MethodInfoComparer()
            : this(null, null, null, null, null)
        {
        }

        public MethodInfoComparer(IComparer<string> nameComparer, IComparer<Type> declaredTypeComparer, IComparer<MethodAttributes> attributeComparer, IComparer<Type> returnTypeComparer, IComparer<ParameterInfo[]> parameterComparer)
        {
            NameComparer = nameComparer ?? Comparer<string>.Default;
            DeclaredTypeComparer = declaredTypeComparer ?? TypeComparer.Default;
            AttributeComparer = attributeComparer ?? Comparer<MethodAttributes>.Create(CompareAttributes);
            ReturnTypeComparer = returnTypeComparer ?? TypeComparer.Default;
            ParameterComparer = parameterComparer ?? Comparer<ParameterInfo[]>.Create((a, b) => a.Length.CompareTo(b.Length));
        }

        public virtual int Compare(MethodInfo x, MethodInfo y)
        {
            CompositeComparer<MethodInfo> comparer = new CompositeComparer<MethodInfo>
            {
                { NameComparer, m => m.Name },
                { DeclaredTypeComparer, m => m.DeclaringType },
                { AttributeComparer, m => m.Attributes },
                { ReturnTypeComparer, m => m.ReturnType },
                { ParameterComparer, m => m.GetParameters() }
            };
            return comparer.Compare(x, y);
        }

        private static int CompareAttributes(MethodAttributes x, MethodAttributes y)
        {
            if (x == y)
                return 0;

            if (x.HasFlag(MethodAttributes.Public) != y.HasFlag(MethodAttributes.Public))
                return x.HasFlag(MethodAttributes.Public) ? -1 : 1;

            if (x.HasFlag(MethodAttributes.Static) != y.HasFlag(MethodAttributes.Static))
                return x.HasFlag(MethodAttributes.Static) ? -1 : 1;

            if (x.HasFlag(MethodAttributes.Final) != y.HasFlag(MethodAttributes.Final))
                return x.HasFlag(MethodAttributes.Final) ? -1 : 1;

            if (x.HasFlag(MethodAttributes.Abstract) != y.HasFlag(MethodAttributes.Abstract))
                return y.HasFlag(MethodAttributes.Abstract) ? -1 : 1;

            if (x.HasFlag(MethodAttributes.Assembly) != y.HasFlag(MethodAttributes.Assembly))
                return x.HasFlag(MethodAttributes.Assembly) ? -1 : 1;

            if (x.HasFlag(MethodAttributes.Family) != y.HasFlag(MethodAttributes.Family))
                return x.HasFlag(MethodAttributes.Family) ? -1 : 1;

            return x.CompareTo(y);
        }
    }
}
