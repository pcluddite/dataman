using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    public class UnregisteredTypeException : XmlSerializationException
    {
        public XName ElementName { get; private set; }

        public UnregisteredTypeException(XName elementName)
            : base("<" + elementName + "> was not registered for deserialization and the type of object is not known")
        {
            ElementName = elementName;
        }
    }
}
