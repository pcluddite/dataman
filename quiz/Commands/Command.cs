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
using System.Collections.Generic;
using System.Reflection;

namespace Baxendale.Quiz.Commands
{
    internal abstract class Command
    {
        public abstract string Name { get; }

        public abstract int RequiredArguments { get; }

        public abstract void Execute(Input input);

        public virtual string GetErrorMessage(Input input)
        {
            if (input.ArgumentCount != RequiredArguments)
                return Name + " expects " + RequiredArguments + " argument(s)";
            return null;
        }

        public static IList<Command> FindCommands()
        {
            IList<Command> cmds = new List<Command>();
            foreach (Type type in Assembly.GetAssembly(typeof(Command)).GetTypes())
            {
                if (type.Namespace == typeof(Command).Namespace && type.Name.StartsWith("Cmd"))
                {
                    cmds.Add((Command)Activator.CreateInstance(type));
                }
            }
            return cmds;
        }
    }
}
