using System;
using System.Collections.Generic;
using SharedLibrary.Enums;
using SharedLibrary.Structures;

namespace SharedLibrary.Helpers
{
    public static class MessageHepler
    {
        public static Message Create1007()
        {
            return new Message(MessageTypeEnum.Error, 1007, new List<string>());
        }
    }
}