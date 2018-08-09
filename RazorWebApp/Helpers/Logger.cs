using System;

namespace RazorWebApp.Helpers
{
    public static class Logger
    {
        public static void Log(DateTime dateTime, string log)
        {
            Console.WriteLine($"{dateTime}:\t{log}");
        }
    }
}