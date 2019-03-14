using System;
using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace SharedLibrary.Helpers
{
    /// <summary>
    /// Helper for easy creation of often used messages.
    /// </summary>
    public static class MessageHepler
    {
        /// <summary>
        /// Returns server error message.
        /// </summary>
        /// <returns>Message with code 1007</returns>
        public static Message Create1007()
        {
            return new Message(MessageTypeEnum.Error, 1007, new List<string>());
        }
    }
}