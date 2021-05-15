using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace VirtualFlashCards.QuizData
{
    public class SelectAnswer : MultiAnswer
    {
        public new const string TYPE = "select";

        public override IEnumerable<string> CorrectOptions
        {
            get
            {
                foreach (KeyValuePair<string, bool> option in OptionDictionary)
                {
                    if (option.Value)
                    {
                        return new string[] { option.Key };
                    }
                }
                return new string[0];
            }
        }

        protected SelectAnswer()
            : base()
        {
        }

        protected SelectAnswer(XmlNode node)
            : base(node)
        {
        }

        public override XmlElement ToXml(XmlDocument doc)
        {
            return base.ToXml(doc);
        }

        public override Answer CloneWithNewInput(Control control)
        {
            GroupBox group = (GroupBox)control;
            SelectAnswer answer = new SelectAnswer();
            foreach (RadioButton radio in group.Controls.Cast<RadioButton>())
            {
                answer.OptionDictionary[radio.Text] = radio.Checked;
            }
            return answer;
        }

        public override Control CreateFormControl(Font font)
        {
            GroupBox group = new GroupBox()
            {
                Name = "grpSelect",
                Text = "Select One",
                Font = font
            };
            int i = 0;
            foreach (string option in OptionsRandomized)
            {
                RadioButton radio = new RadioButton();
                radio.Name = "radioAnswer" + i++;
                radio.Text = option;
                radio.Location = new Point(10, i * (radio.Height + 5));
                radio.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                group.Controls.Add(radio);
            }
            if (group.Controls.Count > 0)
            {
                Control lastControl = group.Controls[group.Controls.Count - 1];
                group.Height = lastControl.Height + lastControl.Location.Y + 10;
            }
            return group;
        }

        public override bool IsCorrect(Control control)
        {
            GroupBox group = (GroupBox)control;
            foreach (RadioButton radio in group.Controls.Cast<RadioButton>())
            {
                if (radio.Checked)
                {
                    return OptionDictionary[radio.Text];
                }
            }
            return false;
        }
    }
}
