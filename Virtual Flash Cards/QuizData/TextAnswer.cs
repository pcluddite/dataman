using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Baxendale.DataManagement.Xml;

namespace VirtualFlashCards.QuizData
{
    public class TextAnswer : Answer
    {
        public const string TYPE = "text";

        public string Value { get; set; }
        public bool MatchCase { get; set; }

        protected TextAnswer(XmlNode node)
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

        public override bool IsCorrect(Control control)
        {
            TextBox txtAnswer = (TextBox)control;
            if (MatchCase)
            {
                return string.Equals(Value, txtAnswer.Text, StringComparison.CurrentCulture);
            }
            else
            {
                return string.Equals(Value, txtAnswer.Text, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public override Answer CloneWithNewInput(Control control)
        {
            return new TextAnswer(((TextBox)control).Text, MatchCase);
        }

        public override Control CreateFormControl(Font font)
        {
            return new TextBox()
            {
                Name = "txtAnswer",
                Font = font
            };
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement node = base.ToXml(doc);
            node.Attributes("value").Value = Value;
            if (MatchCase)
                node.Attributes("matchCase").Value = MatchCase.ToString();
            return node;
        }

        public override bool Equals(Answer other)
        {
            return Equals(other as TextAnswer);
        }

        public virtual bool Equals(TextAnswer other)
        {
            if (ReferenceEquals(other, this))
                return true;
            if ((object)other == null)
                return false;
            return Value == other.Value && MatchCase == other.MatchCase;
        }

        public override int GetHashCode()
        {
            return (Value == null ? 0 : Value.GetHashCode()) ^ MatchCase.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
