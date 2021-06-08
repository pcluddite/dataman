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
using System.Globalization;

namespace Baxendale.Quiz.Objects
{
    public partial class MultiAnswer
    {
        protected struct MultiAnswerKey : IEquatable<MultiAnswerKey>
        {
            public readonly string Key;

            public MultiAnswerKey(string key)
            {
                Key = key.ToLower(CultureInfo.CurrentCulture);
            }

            public override int GetHashCode()
            {
                return Key == null ? 0 : Key.GetHashCode();
            }

            public override string ToString()
            {
                return Key;
            }

            #region implicit operators

            public static implicit operator MultiAnswerKey(string key)
            {
                return new MultiAnswerKey(key);
            }

            public static implicit operator string(MultiAnswerKey key)
            {
                return key.Key;
            }

            public static implicit operator MultiAnswerKey(char key)
            {
                return new MultiAnswerKey(key.ToString());
            }

            #endregion

            #region explicit operators

            public static explicit operator char(MultiAnswerKey key)
            {
                if (string.IsNullOrEmpty(key))
                    return '\0';
                return key.Key[0];
            }

            #endregion

            #region IEquatable<MultiAnswerKey> Members

            public override bool Equals(object obj)
            {
                MultiAnswerKey? other = obj as MultiAnswerKey?;
                if (other == null)
                    return false;
                return Equals(other.Value);
            }

            public bool Equals(MultiAnswerKey other)
            {
                return this.Key == other.Key;
            }

            public static bool operator ==(MultiAnswerKey left, MultiAnswerKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MultiAnswerKey left, MultiAnswerKey right)
            {
                return !left.Equals(right);
            }

            #endregion
        }
    }
}
