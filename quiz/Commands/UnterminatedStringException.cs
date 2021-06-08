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

namespace Baxendale.Quiz.Commands
{
    internal class UnterminatedStringException : ParseException
    {
        public UnterminatedStringException(string text, int startIndex, char closeChar)
            : base(text, startIndex, CreateMessage(text, startIndex, closeChar))
        {
        }

        private static string CreateMessage(string text, int startIndex, char closeChar)
        {
            return "A string was started with [" + text[startIndex] + "] but was never closed.";
        }
    }
}
