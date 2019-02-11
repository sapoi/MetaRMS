using System;
using System.Collections.Generic;
using SharedLibrary.Structures;

namespace SharedLibrary.Helpers
{
    public static class Logger
    {
        public static void LogToConsole(string log)
        {
            Console.WriteLine($"{DateTime.Now}:\t{log}");
        }
        public static void LogMessageToConsole(Message message, string language = "en")
        {
            Console.WriteLine($"{DateTime.Now}:\t{message.GetMessage()}");
        }
        public static void LogMessagesToConsole(List<Message> messages, string language = "en")
        {
            foreach (var message in messages)
            {
                LogMessageToConsole(message, language);
            }
        }
    }
}