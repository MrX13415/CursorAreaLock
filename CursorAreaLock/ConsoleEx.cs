using System;
using System.Collections.Generic;
using System.Text;

namespace System.ConsoleExtentions
{
    public class ConsoleEx
    {
        public static ConsoleColor DefaultForegroundColor{ get; set; }
        public static ConsoleColor DefaultBackgroundColor{ get; set; }
       
        static ConsoleEx(){
            DefaultForegroundColor = Console.ForegroundColor;
            DefaultBackgroundColor = Console.BackgroundColor;
        }

        public static void ResetColors()
        {
            Console.ForegroundColor = DefaultForegroundColor;
            Console.BackgroundColor = DefaultBackgroundColor;
        }

        public static void Write(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            ResetColors();
        }

        public static void WriteKeyValue(string key, string value, ConsoleColor valueColor)
        {
            WriteKeyValue(key, value, valueColor, DefaultForegroundColor);
        }

        public static void WriteKeyValue(string key, string value, ConsoleColor valueColor, string seperator)
        {
            WriteKeyValue(key, value, valueColor, DefaultForegroundColor, seperator);
        }

        public static void WriteKeyValue(string key, string value, ConsoleColor valueColor, ConsoleColor keyColor)
        {
            _WriteKeyValue(key, value, valueColor, keyColor, ": ");
        }

        public static void WriteKeyValue(string key, string value, ConsoleColor valueColor, ConsoleColor keyColor, string seperator)
        {
            _WriteKeyValue(key, value, valueColor, keyColor, seperator);
        }

        private static string _WriteKeyValue(string key, string value, ConsoleColor valueColor, ConsoleColor keyColor, string template)
        {
            Console.ForegroundColor = keyColor;
            Console.Write(key);
            Console.Write(template);
            Console.ForegroundColor = valueColor;
            Console.Write(value);
            ResetColors();
            return key + template + value;
        }

        public static void WriteKeyValueLine(string key, string value, ConsoleColor valueColor)
        {
            WriteKeyValueLine(key, value, valueColor, DefaultForegroundColor, ": ");
        }

        public static void WriteKeyValueLine(string key, string value, ConsoleColor valueColor, string seperator)
        {
            WriteKeyValueLine(key, value, valueColor, DefaultForegroundColor, seperator);
        }

        public static void WriteKeyValueLine(string key, string value, ConsoleColor valueColor, ConsoleColor keyColor, string seperator)
        {
            string text = _WriteKeyValue(key, value, valueColor, keyColor, seperator);
            Console.WriteLine("".PadRight(Console.WindowWidth - text.Length - 1));
        }

        public static void WriteLine()
        {
            WriteLine("");
        }

        public static void WriteLine(String text)
        {
            WriteLine(text, DefaultForegroundColor);
        }

        public static void WriteLine(String text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text.PadRight(Console.WindowWidth - 1));
            ResetColors();
        }

        public static void WriteLineCenter(String text)
        {
            WriteLineCenter(text, DefaultForegroundColor);
        }

        public static void WriteLineCenter(String text, ConsoleColor color)
        {
            int lineWidth = Console.WindowWidth - text.Length - 1;
            Console.ForegroundColor = color;
            Console.WriteLine(String.Format("{0," + (lineWidth / 2 * -1) + "}{1," + (lineWidth / 2 * -1) + "}", "", text));
            ResetColors();
        }

        public static string CutText(string text, int lenght)
        {
            if (text.Length <= lenght) return text;
            return text.Substring(0, lenght - 3) + "...";
        }
    }
}
