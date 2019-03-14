using System;
using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// Class for logging.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs given string to the console.
        /// </summary>
        /// <param name="log">String to log.</param>
        public static void LogToConsole(string log)
        {
            Console.WriteLine($"{DateTime.Now}:\t{log}");
        }
        /// <summary>
        /// Logs given message in given language to the console
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="language">Language in which the message is logged</param>
        public static void LogMessageToConsole(Message message, LanguageEnum language = LanguageEnum.En)
        {
            Console.WriteLine($"{DateTime.Now}:\t{message.GetMessage(language)}");
        }
        /// <summary>
        /// Logs multiple messages to the console.
        /// </summary>
        /// <param name="messages">List of messages to log</param>
        /// <param name="language">Language in which the messages are logged</param>
        public static void LogMessagesToConsole(List<Message> messages, LanguageEnum language = LanguageEnum.En)
        {
            foreach (var message in messages)
                LogMessageToConsole(message, language);
        }
        /// <summary>
        /// Logs exception to the console
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public static void LogExceptionToConsole(Exception exception)
        {
            Console.WriteLine($"{DateTime.Now}:\t{exception.Message}\n\t{exception.StackTrace}");
        }
    }
}