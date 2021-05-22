using System;
using System.Xml;

namespace Baxendale.DataManagement.Extensions
{
    internal static class XmlExtensions
    {
        public static XmlAttribute Attributes(this XmlNode node, string name)
        {
            XmlAttribute attr = node.Attributes[name];
            if (attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(name);
                attr.Value = "";
                node.Attributes.Append(attr);
            }
            return attr;
        }

        public static T Value<T>(this XmlAttribute attr) where T : IConvertible
        {
            return (T)System.Convert.ChangeType(attr.Value, typeof(T));
        }

        public static T Value<T>(this XmlAttribute attr, T @default) where T : IConvertible
        {
            T value;
            if (attr.TryValue(out value))
                return value;
            return @default;
        }

        public static bool TryValue<T>(this XmlAttribute attr, out T value) where T : IConvertible
        {
            try
            {
                value = attr.Value<T>();
                return true;
            }
            catch (Exception ex)
            {
                if (!(ex is InvalidCastException || ex is FormatException))
                    throw;
                value = default(T);
                return false;
                
            }
        }
    }
}
