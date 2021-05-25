using System;

namespace Baxendale.DataManagement.Xml
{
    public class UnsupportedTypeException : XmlSerializationException
    {
        public Type UnsupportedType { get; private set; }

        public UnsupportedTypeException(Type type)
            : this(type, null)
        {
        }

        public UnsupportedTypeException(string typeName)
            : this(typeName, null)
        {
        }
        public UnsupportedTypeException(Type type, string message)
            : this(type.Name, message)
        {
            UnsupportedType = type;
        }

        public UnsupportedTypeException(string typeName, string message)
            : base(typeName + " is unsupported for deserialization." + (message == null ? "" : " " + message))
        {
        }
    }
}
