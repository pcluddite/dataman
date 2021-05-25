using System;

namespace Baxendale.DataManagement.Xml
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class XmlSerializeAttribute : Attribute
    {
        public string Name { get; set; }
        public object Default { get; set; }

        public XmlSerializeAttribute()
        {
        }
    }
}
