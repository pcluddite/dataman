using System.Text;
using Baxendale.DataManagement.Xml;

namespace TestApp
{
    class TestClass : IXmlSerializable
    {
        private string foo = "test123";
        private int seven = 7;

        [XmlSerialize(Default = new int[0])]
        private int[] intArray;

        private int[,,] multiIntArray;

        public TestClass()
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ ");
            sb.Append("foo = ").Append(foo).Append(", ");
            sb.Append("seven = ").Append(seven).Append(", ");
            if (intArray == null)
            {
                sb.Append("null");
            }
            else if (intArray.Length > 0)
            {
                sb.Append("{ ");
                sb.Append(intArray[0]);
                for (int i = 1; i < intArray.Length; ++i)
                {
                    sb.Append(", ").Append(intArray[i]);
                }
                sb.Append(" }");
            }
            sb.Append(" }");
            return sb.ToString();
        }
    }
}
