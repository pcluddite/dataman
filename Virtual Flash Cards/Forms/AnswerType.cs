//
//    Virtual Flash Cards
//    Copyright (C) 2009-2021 Timothy Baxendale
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
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
