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
using System.Linq;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Xml
{
    internal class XmlArraySerializer<ArrayType, ElementType> : XmlObjectSerializer<Array, XElement>
    {
        public int Rank
        {
            get
            {
                return typeof(ArrayType).GetArrayRank();
            }
        }

        public override bool UsesXAttribute => false;

        public XmlArraySerializer()
        {
        }

        public override Array Deserialize(XElement content)
        {
            DynamicArray<ElementType> arr = new DynamicArray<ElementType>(new int[Rank]);
            int[] indices = arr.DecrementIndex(arr.LowerBound);
            foreach (XElement child in content.Elements())
            {
                XAttribute indexAttribute = child.Attribute(XmlSerializer.IndexAttributeName);
                if (indexAttribute == null)
                {
                    indices = arr.IncrementIndex(indices);
                }
                else
                {
                    SetIndices(indices, indexAttribute.Value);
                }
                if (typeof(IConvertible).IsAssignableFrom(typeof(ElementType)))
                {
                    XAttribute valueAttribute = child.Attribute(XmlSerializer.ValueAttributeName);
                    arr[indices] = (valueAttribute == null) ? default(ElementType) : XmlSerializer.Deserialize<ElementType>(valueAttribute);
                }
                else
                {
                    arr[indices] = XmlSerializer.Deserialize<ElementType>(child);
                }
            }
            return arr.ToArray();
        }

        public override XElement Serialize(Array array, XName name)
        {
            XElement content = new XElement(name);

            int rank = Rank;
            int[] indices = new int[rank];

            foreach (ElementType item in array.Cast<ElementType>())
            {
                XElement itemContent = XmlSerializer.Serialize(item);
                
                if (rank > 1)
                    itemContent.SetAttributeValue(XmlSerializer.IndexAttributeName, indices.ToString(','));

                content.Add(itemContent);

                ++indices[rank - 1];
                for (int dim = rank - 1; dim > 0; --dim)
                {
                    if (indices[dim] >= array.GetLength(dim))
                    {
                        for (int i = dim; i < rank; ++i)
                            indices[i] = 0;
                        ++indices[dim - 1];
                    }
                }
            }
            return content;
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
