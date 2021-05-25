using System;

namespace Baxendale.DataManagement.Xml
{
    public abstract class XmlSerializationException : Exception
    {
        protected XmlSerializationException(string message)
            : base(message)
        {
        }
    }
}
