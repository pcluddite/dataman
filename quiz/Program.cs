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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baxendale.Quiz.Commands;

namespace Baxendale.Quiz
{
    internal class Program
    {
        public const int EXIT_SUCCESS = 0;
        public const int EXIT_ERROR_GENERIC = 1;

        public const string PROMPT = "$> ";

        public static bool PendingExit { get; set; }

        private static IDictionary<string, Command> Commands;

        public static int Main(string[] args)
        {
            Console.Title = "Quiz";
            if (args.Length == 0)
            {
                ShowIntro();
                return InteractiveMode();
            }
            return EXIT_SUCCESS;
        }

        public static void ShowIntro()
        {
            Console.WriteLine("Quiz [Version 4.0], Copyright (C) 2009-2021 Timothy Baxendale");
            Console.WriteLine("This software comes with ABSOLUTELY NO WARRANTY; for details type 'show w'.");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions; type 'show c' for details.");
            Console.WriteLine();
        }

        public static int InteractiveMode()
        {
            Commands = LoadCommands();
            while (!PendingExit)
            {
                Console.Write(PROMPT);
                Input input;
                try
                {
                    input = Console.ReadLine();
                    Command cmd;
                    if (!Commands.TryGetValue(input.Name, out cmd))
                        throw new CommandException("'" + input.Name + "' is not a valid command");
                    string error;
                    if ((error = cmd.GetErrorMessage(input)) != null)
                        throw new CommandException(error);
                    cmd.Execute(input);
                }
                catch (CommandException ex)
                {
                    ConsoleColor color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = color;
                }
                Console.WriteLine();
            }
            return EXIT_SUCCESS;
        }

        private static IDictionary<string, Command> LoadCommands()
        {
            IDictionary<string, Command> commands = new Dictionary<string, Command>(StringComparer.CurrentCultureIgnoreCase);
            foreach (Command cmd in Command.FindCommands())
            {
                commands[cmd.Name] = cmd;
            }
            return commands;
        }
    }
}
