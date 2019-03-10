using System.Collections.Generic;
using System.Text;
using SharedLibrary.Enums;
using SharedLibrary.Helpers;
using SharedLibrary.StaticFiles;

namespace SharedLibrary.Structures
{
    /// <summary>
    /// Messages is the main communication class used for sending messages from server to clients.
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
        /// <param name="attributeName">Name of attribute the message belongs to.</param>
        public Message(MessageTypeEnum messageType, int messageCode, List<string> placeholders, string attributeName = null)
        {
            MessageType = messageType;
            MessageCode = messageCode;
            Placeholders = placeholders;
            AttributeName = attributeName;
        }
        /// <summary>
        /// GetMessage method creates a single string with replaced placeholders from message.
        /// </summary>
        /// <param name="language">Language of the message from the LanguageEnum enum.</param>
        /// <returns>String representing the message.</returns>
        public string GetMessage(LanguageEnum language = LanguageEnum.En)
        {
            // Build a new message
            StringBuilder sb = new StringBuilder();
            // Add attribute, if present
            if (AttributeName != null)
                sb.Append(AttributeName + " - ");
            // Try to get message text
            if (MessagesEn.Messages.ContainsKey(MessageCode))
                sb.Append(MessagesEn.Messages[MessageCode]);
            else
            {
                sb.Append(MessageCode.ToString());
                Logger.LogToConsole($"Unknown message code {MessageCode}");
            }
            
            // Replace placeholders
            for (int i = 0; i < Placeholders.Count; i++)
            {
                sb.Replace($"{{{i}}}", Placeholders[i]);
            }
            return sb.ToString();
        }
    }
}