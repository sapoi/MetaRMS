using System;

namespace SharedLibrary.Helpers
{
    public static class Logger
    {
        public static void LogToConsole(string log)
        {
            Console.WriteLine($"{DateTime.Now}:\t{log}");
        }
    }
}