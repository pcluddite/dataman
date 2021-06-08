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
using System.Xml.Linq;

namespace Baxendale.Quiz.Objects
{
    public class ComboAnswer : SelectAnswer, IEquatable<ComboAnswer>
    {
        public new const string TYPE = "combo";
        
        protected ComboAnswer()
            : base()
        {
        }

        protected ComboAnswer(SelectAnswer other)
            : base(other)
        {
        }

        protected ComboAnswer(ComboAnswer other)
            : this((SelectAnswer)other)
        {
        }

        protected ComboAnswer(XElement node)
            : base(node)
        {
        }

        public override Answer CloneWithNewInput(string input)
        {
            return new ComboAnswer((SelectAnswer)base.CloneWithNewInput(input));
        }

        public override bool Equals(SelectAnswer other)
        {
            return Equals(other as ComboAnswer);
        }

        public virtual bool Equals(ComboAnswer other)
        {
            if ((object)other == null)
                return false;
            if ((object)this == other)
                return true;
            return OptionDictionary.Equals(other.OptionDictionary);
        }
    }
}
