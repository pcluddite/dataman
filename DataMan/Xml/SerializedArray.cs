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
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedArray(XElement node, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedArray<>).MakeGenericType(typeof(T), typeof(T).GetElementType());
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, defaultValue);
        }

        private class SerializedArray<ElementType> : SerializedXmlObject<Array>
        {
            public int Rank
            {
                get
                {
                    return typeof(T).GetArrayRank();
                }
            }

            public SerializedArray(XElement node, Array defaultValue)
                : base(node, node.Name, defaultValue)
            {
            }

            public override Array Deserialize()
            {
                XElement node = Node;
                if (node == null)
                    return DefaultValue;
                DynamicArray<ElementType> arr = new DynamicArray<ElementType>(new int[Rank]);
                int[] indices = arr.DecrementIndex(arr.LowerBound);
                foreach (XElement child in node.Elements("a"))
                {
                    XAttribute indexAttribute = child.Attribute("i");
                    if (indexAttribute == null)
                    {
                        indices = arr.IncrementIndex(indices);
                    }
                    else
                    {
                        SetIndices(indices, indexAttribute.Value);
                    }
                    arr[indices] = (ElementType)XmlSerializer.CreateSerializedObject(typeof(ElementType), child, "v", default(ElementType)).Deserialize();
                }
                return arr.ToArray();
            }

            private void SetIndices(int[] indices, string value)
            {
                int start = 0;
                int stop = 0;
                int dim = 0;

                for (; dim < indices.Length; ++dim)
                    indices[dim] = 0;

                for (dim = 0; stop <= value.Length; ++stop)
                {
                    if (stop == value.Length || value[stop] == ',')
                    {
                        indices[dim++] = int.Parse(value.Substring(start, stop - start));
                        start = stop + 1;
                    }
                }
            }
        }
    }
}
