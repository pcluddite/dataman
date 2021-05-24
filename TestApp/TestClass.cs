using System.Collections.Generic;
using System.Text;
using Baxendale.DataManagement.Collections;
using Baxendale.DataManagement.Xml;

namespace TestApp
{
    class TestClass : IXmlSerializableObject
    {
        private string foo = "test123";
        private int seven = 7;

        [XmlSerialize(Default = new int[0])]
        private int[] intArray;

        private int[,,] multiIntArray;

        [XmlSerialize(Name = "list")]
        public List<string> testList;

        public TestClass()
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("foo = ").AppendLine(foo);
            sb.Append("seven = ").Append(seven).AppendLine();
            sb.Append("intArray = ");
            if (intArray == null)
            {
                sb.AppendLine("null");
            }
            else 
            {
                sb.Append("{ ");
                sb.Append(intArray.ToString(", "));
                sb.AppendLine(" }");
            }
            sb.Append("testList = ");
            if (testList == null)
            {
                sb.AppendLine("null");
            }
            else
            {
                sb.Append("{ ");
                sb.Append(testList.ToString(", "));
                sb.AppendLine(" }");
            }
            sb.Append("multiIntArray = ");
            if (testList == null)
            {
                sb.AppendLine("null");
            }
            else
            {
                sb.Append("{ ");
                sb.Append(multiIntArray.ArrayToString(", "));
                sb.AppendLine(" }");
            }
            return sb.ToString();
        }
    }
}
