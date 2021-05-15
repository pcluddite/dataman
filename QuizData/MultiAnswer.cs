using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using VirtualFlashCards.Xml;
using System.Linq;

namespace VirtualFlashCards.QuizData
{
    public class MultiAnswer : Answer
    {
        public const string TYPE = "multi";

        protected Dictionary<string, bool> OptionDictionary = new Dictionary<string, bool>();

        public virtual IEnumerable<string> Options
        {
            get
            {
                return OptionDictionary.Keys;
            }
        }

        public virtual IEnumerable<string> CorrectOptions
        {
            get
            {
                foreach (KeyValuePair<string, bool> choice in OptionDictionary)
                {
                    if (choice.Value)
                        yield return choice.Key;
                }
            }
        }

        public virtual IEnumerable<string> OptionsRandomized
        {
            get
            {
                List<string> options = new List<string>(Options);
                Random r = new Random();
                while (options.Count > 0)
                {
                    int idx = r.Next(0, options.Count);
                    yield return options[idx];
                    options.RemoveAt(idx);
                }
            }
        }

        protected MultiAnswer()
        {
        }

        protected MultiAnswer(XmlNode node)
        {
            foreach (XmlNode optionNode in node.SelectNodes("option"))
            {
                MultiAnswerOption option = new MultiAnswerOption(optionNode);
                OptionDictionary.Add(option.Text, option.IsCorrect);
            }
        }

        public override bool IsCorrect(Control control)
        {
            CheckedListBox checkedListBox = (CheckedListBox)control;
            HashSet<string> selectedOptions = new HashSet<string>();
            foreach (object checkedItem in checkedListBox.CheckedItems)
            {
                if (!OptionDictionary[(string)checkedItem])
                    return false;
                selectedOptions.Add((string)checkedItem);
            }
            foreach (string correct in CorrectOptions)
            {
                if (!selectedOptions.Contains(correct))
                    return false;
            }
            return true;
        }

        public override Answer CloneWithNewInput(Control control)
        {
            CheckedListBox checkedListBox = (CheckedListBox)control;
            MultiAnswer answer = new MultiAnswer();
            answer.OptionDictionary = new Dictionary<string, bool>(OptionDictionary);
            foreach(string option in OptionDictionary.Keys)
            {
                answer.OptionDictionary[option] = false;
            }
            foreach (object checkedItem in checkedListBox.CheckedItems)
            {
                answer.OptionDictionary[(string)checkedItem] = true;
            }
            return answer;
        }

        public override Control CreateFormControl()
        {
            CheckedListBox checkedListBox = new CheckedListBox() 
            {
                Height = 25 * OptionDictionary.Count,
                CheckOnClick = true
            };
            foreach (string option in OptionsRandomized)
            {
                checkedListBox.Items.Add(option);
            }
            return checkedListBox;
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            XmlElement elem = base.ToXml(doc);
            foreach (KeyValuePair<string, bool> option in OptionDictionary)
            {
                elem.AppendChild(new MultiAnswerOption(option.Key, option.Value).ToXml(doc));
            }
            return elem;
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
            if (other.OptionDictionary.Count != OptionDictionary.Count)
                return false;
            return OptionDictionary.Equals(other.OptionDictionary);
        }

        public override int GetHashCode()
        {
            return OptionDictionary.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, bool> option in OptionDictionary)
            {
                sb.Append(option.Value ? "[*] " : "[ ] ");
                sb.AppendLine(option.Key);
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
