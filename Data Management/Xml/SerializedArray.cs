using System;
using System.Xml.Linq;
using Baxendale.DataManagement.Collections;

namespace Baxendale.DataManagement.Xml
{
    internal abstract partial class SerializedXmlObject<T> : ISerializedXmlObject
    {
        private static ISerializedXmlObject CreateSerializedArray(XElement node, XName name, T defaultValue)
        {
            Type serializedXmlObject = typeof(SerializedArray<>).MakeGenericType(typeof(T), typeof(T).GetElementType());
            return (ISerializedXmlObject)Activator.CreateInstance(serializedXmlObject, node, name, defaultValue);
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

            public SerializedArray(XElement node, XName attrName, Array defaultValue)
                : base(node, attrName, defaultValue)
            {
            }

            public override Array Deserialize()
            {
                XElement node = AttributeName == null ? Node : Node.Element(AttributeName);
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
