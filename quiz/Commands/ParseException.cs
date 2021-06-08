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

namespace Baxendale.Quiz.Commands
{
    internal abstract class ParseException : CommandException
    {
        private const int LINE_SIZE = 50;

        public string InvalidText { get; protected set; }
        public int Index { get; protected set; }

        protected ParseException(string text, int index, string message)
            : base(CreateMessage(text, index, message))
        {
            InvalidText = text;
            Index = index;
        }

        private static string CreateMessage(string text, int index, string message)
        {
            StringBuilder sb = new StringBuilder();
            if (text.Length > LINE_SIZE)
            {
                int endIndex = Math.Min(index + LINE_SIZE / 2, text.Length);
                int size = LINE_SIZE;

                int startIndex = endIndex - LINE_SIZE;

                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                else if (startIndex > 0)
                {
                    sb.Append("...");
                    index -= startIndex;
                    startIndex += 3;
                    size -= 3;
                }

                if (size + startIndex < text.Length)
                {
                    size -= 3;
                }

                sb.Append(text, startIndex, size);

                if (size + startIndex < text.Length)
                {
                    sb.Append("...");
                }
            }
            else
            {
                sb.Append(text);
            }
            sb.AppendLine();
            sb.AppendLine("^".PadLeft(index + 1, ' '));
            sb.Append(message);
            return sb.ToString();
        }
    }
}
