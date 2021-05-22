using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Baxendale.DataManagement.Extensions;

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
            GroupBox group = (GroupBox)control;
            foreach (CheckBox check in group.Controls.Cast<CheckBox>())
            {
                if ((check.Checked && !OptionDictionary[check.Text])
                     || (!check.Checked && OptionDictionary[check.Text]))
                {
                    return false;
                }
            }
            return true;
        }

        public override Answer CloneWithNewInput(Control control)
        {
            GroupBox group = (GroupBox)control;
            MultiAnswer answer = new MultiAnswer();
            foreach (CheckBox check in group.Controls.Cast<CheckBox>())
            {
                answer.OptionDictionary[check.Text] = check.Checked;
            }
            return answer;
        }

        public override Control CreateFormControl(Font font)
        {
            GroupBox group = new GroupBox()
            {
                Name = "grpMulti",
                Text = "Select All That Apply",
                Font = font
            };
            int i = 0;
            foreach (string option in OptionsRandomized)
            {
                CheckBox checkAnswer = new CheckBox();
                checkAnswer.Name = "checkAnswer" + i++;
                checkAnswer.Text = option;
                checkAnswer.Location = new Point(10, i * (checkAnswer.Height + 5));
                checkAnswer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                group.Controls.Add(checkAnswer);
            }
            if (group.Controls.Count > 0)
            {
                Control lastControl = group.Controls[group.Controls.Count - 1];
                group.Height = lastControl.Height + lastControl.Location.Y + 10;
            }
            return group;
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
