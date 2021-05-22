using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualFlashCards.QuizData;

namespace VirtualFlashCards.Forms
{
    internal class AnswerTypeSelection
    {
        public static readonly AnswerTypeSelection Text = new AnswerTypeSelection("Text", typeof(TextAnswer));
        public static readonly AnswerTypeSelection MultipleChoice = new AnswerTypeSelection("Multiple choice", typeof(SelectAnswer));
        public static readonly AnswerTypeSelection SelectAny = new AnswerTypeSelection("Select all that apply", typeof(MultiAnswer));
        public static readonly AnswerTypeSelection Combo = new AnswerTypeSelection("Combo Box", typeof(ComboAnswer));

        public string Name { get; private set; }
        public Type AnswerType { get; private set; }

        private AnswerTypeSelection(string name, Type answerType)
        {
            Name = name;
            AnswerType = answerType;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
