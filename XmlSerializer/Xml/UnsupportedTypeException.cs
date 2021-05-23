using System;

namespace Baxendale.DataManagement.Xml
{
    public class UnsupportedTypeException : XmlSerializationException
    {
        public Type UnsupportedType { get; private set; }

        public UnsupportedTypeException(Type type)
            : base(type.Name + " is unsupported for deserialization")
        {
            UnsupportedType = type;
        }
    }
}
