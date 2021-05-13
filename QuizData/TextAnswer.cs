using System;
using System.Xml;
using VirtualFlashCards.Xml;

namespace VirtualFlashCards.QuizData
{
    public class TextAnswer : Answer
    {
        public const string TYPE = "text";

        public string Value { get; set; }
        public bool MatchCase { get; set; }

        public TextAnswer(XmlNode node)
        {
            Value = node.Attributes("value").Value;
            MatchCase = node.Attributes("matchCase").Value(false);
        }

        public TextAnswer(string value)
        {
            Value = value;
        }

        public TextAnswer(string value, bool matchCase)
        {
            Value = value;
            MatchCase = matchCase;
        }

        public override bool IsCorrect(string input)
        {
            if (MatchCase)
            {
                return string.Equals(Value, input, StringComparison.CurrentCulture);
            }
            else
            {
                return string.Equals(Value, input, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = base.ToXml(doc);
            node.Attributes("value").Value = Value;
            if (MatchCase)
                node.Attributes("matchCase").Value = MatchCase.ToString();
            return node;
        }
    }
}
