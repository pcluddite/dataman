using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace VirtualFlashCards.QuizData
{
    public class ComboAnswer : SelectAnswer
    {
        public new const string TYPE = "combo";
        
        protected ComboAnswer()
            : base()
        {
        }

        protected ComboAnswer(XElement node)
            : base(node)
        {
        }

        public override XElement ToXml(XName name)
        {
            return base.ToXml(name);
        }

        public override bool IsCorrect(Control control)
        {
            ComboBox comboBox = (ComboBox)control;
            if (comboBox.SelectedItem == null)
                return false;
            return OptionDictionary[(string)comboBox.SelectedItem];
        }

        public override Answer CloneWithNewInput(Control control)
        {
            ComboBox comboBox = (ComboBox)control;
            ComboAnswer answer = new ComboAnswer();
            foreach (string s in comboBox.Items.Cast<string>())
            {
                answer.OptionDictionary[s] = false;
            }
            if (comboBox.SelectedItem != null)
                answer.OptionDictionary[(string)comboBox.SelectedItem] = true;
            return answer;
        }

        public override Control CreateFormControl(Font font)
        {
            ComboBox comboBox = new ComboBox()
            {
                Name = "comboBoxAnswer",
                Font = font,
                DropDownStyle = ComboBoxStyle.DropDown,
                Text = "Select One..."
            };
            foreach (string opt in OptionsRandomized)
            {
                comboBox.Items.Add(opt);
            }
            return comboBox;
        }

        public override bool Equals(SelectAnswer other)
        {
            return Equals(other as ComboAnswer);
        }

        public virtual bool Equals(ComboAnswer other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if ((object)other == null)
                return false;
            if (other.OptionDictionary.Count != OptionDictionary.Count)
                return false;
            return OptionDictionary.Equals(other.OptionDictionary);
        }
    }
}
