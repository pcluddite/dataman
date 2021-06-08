//
//    Quiz
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
using System.Text;
using System.Xml.Linq;

namespace Baxendale.Quiz.Objects
{
    public class SelectAnswer : MultiAnswer, IEquatable<SelectAnswer>
    {
        public new const string TYPE = "select";

        protected SelectAnswer()
            : base()
        {
        }

        protected SelectAnswer(MultiAnswer other)
            : base(other)
        {
        }

        protected SelectAnswer(SelectAnswer other)
            : this((MultiAnswer)other)
        {
        }

        protected SelectAnswer(XElement node)
            : base(node)
        {
        }

        public override XElement ToXml(XName name)
        {
            return base.ToXml(name);
        }

        public override Answer CloneWithNewInput(string input)
        {
            SelectAnswer answer = new SelectAnswer();
            foreach (var option in OptionDictionary)
                answer.OptionDictionary[option.Key] = new MultiAnswerOption(option.Value.Text, false);
            if (input != null)
                answer.OptionDictionary[input].IsCorrect = true;
            return answer;
        }

        public override string GetPrompt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Choose one:");
            sb.AppendLine();
            foreach (var option in OptionDictionary)
                sb.AppendLine(option.Value.ToString(option.Key));
            return sb.ToString();
        }

        public override bool IsCorrect(string input)
        {
            return OptionDictionary[input].IsCorrect;
        }

        public override bool Equals(MultiAnswer other)
        {
            return Equals(other as SelectAnswer);
        }

        public virtual bool Equals(SelectAnswer other)
        {
            if ((object)other == null)
                return false;
            if ((object)this == other)
                return true;
            return OptionDictionary.Equals(other.OptionDictionary);
        }
    }
}
