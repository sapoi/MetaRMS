using System.Collections.Generic;
using SharedLibrary.Enums;

namespace SharedLibrary.Structures
{
    /// <summary>
    /// Messages is the main communication class used for sending massages from server to clients.
    /// </summary>
    public class Message
        {
            /// <summary>
            /// MessageType property.
            /// </summary>
            /// <value>MessageType represents a type of message from MessageTypeEnum.</value>
            public MessageTypeEnum MessageType { get; }
            /// <summary>
            /// MessageCode property.
            /// </summary>
            /// <value>MessageCode represents a 4 digit code representing message text.</value>
            public int MessageCode { get; }
            /// <summary>
            /// Placeholders property.
            /// </summary>
            /// <value>Placehorders contain a list of strings used in GetMessage method.</value>
            public List<string> Placeholders { get; }
            // /// <summary>
            // /// Language property.
            // /// </summary>
            // /// <value>Language property represents a language of the message from LanguageEnum</value>
            // public LanguageEnum Language { get; }
            /// <summary>
            /// AttributeName property.
            /// </summary>
            /// <value>AttributeName property represents name of attribute, to which the message belongs, if such attribute exists.</value>
            public string AttributeName { get; }
            /// <summary>
            /// Massage constructor.
            /// </summary>
            /// <param name="messageType">Type of the message from the MessageTypeEnum enum.</param>
            /// <param name="messageCode">Code of the message.</param>
            /// <param name="placeholders">List of strings used for message placeholders.</param>
            /// <param name="language">Language of the message from the LanguageEnum enum.</param>
            /// <param name="attributeName">Name of attribute the message belongs to.</param>
            public Message(MessageTypeEnum messageType, int messageCode, List<string> placeholders, /* LanguageEnum language = LanguageEnum.En, */ string attributeName = null)
            {
                MessageType = messageType;
                MessageCode = messageCode;
                Placeholders = placeholders;
                // Language = language;
                AttributeName = attributeName;
            }
            /// <summary>
            /// GetMessage method creates a single string with replaced placeholders from message.
            /// </summary>
            /// <returns>String representing the message.</returns>
            public string GetMessage()
            {
                //TODO preklady
                string message = "";
                switch (MessageType)
                {
                    case MessageTypeEnum.Error:
                        message = "ERROR: ";
                        break;
                    case MessageTypeEnum.Warning:
                        message = "WARNING: ";
                        break;
                    case MessageTypeEnum.Info:
                        message = "INFO: ";
                        break;
                    default:
                        break;
                }
                if (AttributeName != null)
                    message += AttributeName + " - ";
                message += MessageCode;
                for (int i = 0; i < Placeholders.Count; i++)
                {
                    message.Replace($"{{{i}}}", Placeholders[i]);
                }
                return message;
            }
        }
}