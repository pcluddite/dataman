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
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedArray(T obj, XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedArray<>).MakeGenericType(typeof(T), typeof(T).GetElementType());
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedArray<ElementType> : DeserializedXmlObject<Array>
        {
            public DeserializedArray(Array arr, XName name)
                : base(arr, name)
            {
            }

            public override XObject Serialize()
            {
                XElement element = new XElement(Name);

                int rank = DeserializedObject.Rank;
                int[] indices = new int[rank];

                foreach (ElementType item in DeserializedObject.Cast<ElementType>())
                {
                    XElement a = new XElement("a");

                    IDeserializedXmlObject xobj = XmlSerializer.CreateDeserializedObject(typeof(ElementType), item, "v");
                    a.Add(xobj.Serialize());

                    if (rank > 1)
                        a.SetAttributeValue("i", indices.ToString(','));

                    element.Add(a);

                    ++indices[rank - 1];
                    for (int dim = rank - 1; dim > 0; --dim)
                    {
                        if (indices[dim] >= DeserializedObject.GetLength(dim))
                        {
                            for (int i = dim; i < rank; ++i)
                                indices[i] = 0;
                            ++indices[dim - 1];
                        }
                    }
                }
                return element;
            }
        }
    }
}
