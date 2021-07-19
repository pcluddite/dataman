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
using System.Reflection;

namespace Baxendale.Data.Collections.NonGenerics
{
    public static class NonGenericExtensions
    {
        public static void Add(this ICollection collection, object item)
        {
            Type collectionType = collection.GetType();
            MethodInfo addMethods = collectionType.GetMethod(nameof(ArrayList.Add), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(object) }, null);
            addMethods.Invoke(collection, new object[] { item });
        }

        public static bool? IsReadOnly(this ICollection collection)
        {
            Type collectionType = collection.GetType();
            try
            {
                PropertyInfo readOnlyProperty = collectionType.GetProperty(nameof(ArrayList.IsReadOnly), BindingFlags.Instance | BindingFlags.Public, null, typeof(bool), new Type[0], null);
                if (readOnlyProperty == null)
                    return null;
                MethodInfo getMethod = readOnlyProperty.GetGetMethod(false);
                if (getMethod == null)
                    return null;
                return (bool)getMethod.Invoke(collection, new object[0]);
            }
            catch (AmbiguousMatchException)
            {
                return null;
            }
        }
    }
}
