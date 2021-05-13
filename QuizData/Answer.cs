using System;
using System.Reflection;
using System.Text;
using System.Xml;
using VirtualFlashCards.Xml;

namespace VirtualFlashCards.QuizData
{
    public abstract class Answer
    {
        public abstract bool IsCorrect(string input);

        public virtual XmlElement ToXml(XmlDocument doc)
        {
            XmlElement elem = doc.CreateElement("answer");
            elem.SetAttribute("type", GetAnswerType(GetType()));
            return elem;
        }

        public static Answer FromXml(XmlNode node)
        {
            if (node.Name != "answer")
                throw new ArgumentException("Cannot convert node to Answer class because node is not an Answer");
            string ansTypeName = node.Attributes("type").Value;
            Type t = Type.GetType(GetAnswerClassNameFromType(ansTypeName));
            if (t == null)
                throw new ArgumentException("Encountered unknown answer type '" + ansTypeName + "'. The document may not be supported by this version of Flash Cards");
            return (Answer)Activator.CreateInstance(t, node);
        }

        private static string GetAnswerType(Type ansType)
        {
            if (!ansType.IsAssignableFrom(typeof(Answer)))
                throw new ArgumentException("GetAnswerType() was not passed a type of Answer");
            FieldInfo typeField = ansType.GetField("TYPE");
            if (typeField == null || !typeof(string).IsAssignableFrom(typeField.FieldType) || !typeField.IsLiteral)
                throw new ArgumentException("The Answer object must have a constant field named TYPE");
            return (string)typeField.GetValue(null);
        }

        private static string GetAnswerClassNameFromType(string ansTypeName)
        {
            StringBuilder sb = new StringBuilder(typeof(Answer).Namespace.Length + ansTypeName.Length + "Answer".Length + 1);
            sb.Append(typeof(Answer).Namespace);
            sb.Append('.');
            sb.Append(char.ToUpper(ansTypeName[0]));
            for (int i = 1; i < ansTypeName.Length; ++i)
            {
                sb.Append(char.ToLower(ansTypeName[i]));
            }
            sb.Append("Answer");
            return sb.ToString();
        }
    }
}
