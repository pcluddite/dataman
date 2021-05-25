﻿using System;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    internal partial class DeserializedXmlObject<T>
    {
        private static IDeserializedXmlObject CreateDeserializedNullObject(XName name)
        {
            Type deserializedXmlObject = typeof(DeserializedNullObject).MakeGenericType(typeof(T));
            return (IDeserializedXmlObject)Activator.CreateInstance(deserializedXmlObject, name);
        }

        private class DeserializedNullObject : DeserializedXmlObject<object>
        {
            public DeserializedNullObject(XmlSerializeAttribute attrib)
                : base(null, attrib.Name)
            {
            }

            public DeserializedNullObject(XName attrName)
                : base(null, attrName)
            {
            }

            public override XObject Serialize()
            {
                XElement element = new XElement(Name);
                element.SetAttributeValue("t", "null");
                return element;
            }
        }
    }
}