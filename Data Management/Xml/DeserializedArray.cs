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
            Type deserializedXmlObject = typeof(DeserializedArray<>).MakeGenericType(typeof(T).GetElementType());
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, obj, name);
        }

        private class DeserializedArray<ElementType> : DeserializedXmlObject<Array>
        {
            public DeserializedArray(Array arr, XmlSerializeAttribute attrib)
                : base(arr, attrib.Name)
            {
            }

            public DeserializedArray(Array arr, XName attrName)
                : base(arr, attrName)
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
