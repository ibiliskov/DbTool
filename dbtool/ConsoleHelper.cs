using System;

namespace dbtool
{
    public static class ConsoleHelper
    {
        public static void WriteLine(ConsoleColor consoleColor, string message)
        {
            var initialColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ForegroundColor = initialColor;
        }

        public static void WriteError(string message)
        {
            WriteLine(ConsoleColor.Red, message);
        }

        public static void WriteSuccess(string message)
        {
            WriteLine(ConsoleColor.Green, message);
        }

        public static void WriteWarning(string message)
        {
            WriteLine(ConsoleColor.Yellow, message);
        }
    }
}
