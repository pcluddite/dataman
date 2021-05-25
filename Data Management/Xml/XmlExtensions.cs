using System;
using System.Xml;
using System.Xml.Linq;

namespace Baxendale.DataManagement.Xml
{
    public static class XmlExtensions
    {
        public static T Value<T>(this XAttribute attr) where T : IConvertible
        {
            return (T)System.Convert.ChangeType(attr.Value, typeof(T));
        }

        public static T Value<T>(this XAttribute attr, T @default) where T : IConvertible
        {
            if (attr == null)
                return @default;
            T value;
            if (attr.TryValue(out value))
                return value;
            return @default;
        }

        public static bool TryValue<T>(this XAttribute attr, out T value) where T : IConvertible
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
