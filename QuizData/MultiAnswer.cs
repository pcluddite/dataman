using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using VirtualFlashCards.Xml;

namespace VirtualFlashCards.QuizData
{
    public class MultiAnswer : Answer
    {
        public const string TYPE = "multi";

        private List<MultiAnswerOption> choices = new List<MultiAnswerOption>();

        public IEnumerable<string> Choices
        {
            get
            {
                foreach (MultiAnswerOption choice in choices)
                {
                    yield return choice.Text;
                }
            }
        }

        public IEnumerable<string> CorrectChoices
        {
            get
            {
                foreach (MultiAnswerOption choice in choices)
                {
                    if (choice.IsCorrect)
                        yield return choice.Text;
                }
            }
        }

        private MultiAnswer()
        {
        }

        protected MultiAnswer(XmlNode node)
        {
            foreach (XmlNode optionNode in node.SelectNodes("option"))
            {
                choices.Add(new MultiAnswerOption(optionNode));
            }
        }

        public override bool IsCorrect(string input)
        {
            foreach (string option in CorrectChoices)
            {
                if (input == option)
                    return true;
            }
            return false;
        }

        public override Answer CloneWithNewInput(params string[] input)
        {
            if (input == null)
                throw new ArgumentNullException();
            MultiAnswer answer = new MultiAnswer();
            foreach (MultiAnswerOption option in answer.choices)
            {
                bool correct = Array.IndexOf(input, option.Text) > -1;
                answer.choices.Add(new MultiAnswerOption(option.Text, correct));
            }
            return answer;
        }

        public override bool Equals(Answer other)
        {
            return Equals(other as MultiAnswer);
        }

        public virtual bool Equals(MultiAnswer other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if ((object)other == null)
                return false;
            if (other.choices.Count != choices.Count)
                return false;
            for (int idx = 0; idx < choices.Count; ++idx)
            {
                if (other.choices[idx] != choices[idx])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return choices.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (MultiAnswerOption option in choices)
            {
                sb.Append(option.IsCorrect ? "[*] " : "[ ] ");
                sb.AppendLine(option.Text);
            }
            return sb.ToString();
        }

        private struct MultiAnswerOption
        {
            public readonly bool IsCorrect;
            public readonly string Text;

            public MultiAnswerOption(string text, bool correct)
            {
                Text = text;
                IsCorrect = correct;
            }

            public MultiAnswerOption(XmlNode node)
            {
                if (node.Attributes["text"] == null)
                    throw new XmlException("Multi answer option had no text attribute");
                Text = node.Attributes["text"].Value;
                IsCorrect = node.Attributes("value").Value(false);
            }

            public override string ToString()
            {
                return Text;
            }

            public XmlElement ToXml(XmlDocument doc)
            {
                XmlElement node = doc.CreateElement("option");
                node.Attributes("text").Value = Text;
                if (IsCorrect)
                    node.Attributes("value").Value = IsCorrect.ToString();
                return node;
            }

            public override bool Equals(object obj)
            {
                MultiAnswerOption? other = obj as MultiAnswerOption?;
                if (other == null)
                    return false;
                return Equals(other.Value);
            }

            public bool Equals(MultiAnswerOption other)
            {
                return Text == other.Text && IsCorrect == other.IsCorrect;
            }

            public override int GetHashCode()
            {
                return Text.GetHashCode() ^ IsCorrect.GetHashCode();
            }

            public static bool operator ==(MultiAnswerOption left, MultiAnswerOption right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MultiAnswerOption left, MultiAnswerOption right)
            {
                return !left.Equals(right);
            }
        }
    }
}
