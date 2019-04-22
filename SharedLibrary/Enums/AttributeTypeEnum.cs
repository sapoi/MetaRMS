using System.Collections.Generic;

namespace SharedLibrary.Enums
{
    /// <summary>
    /// List of basic data types, that can be used at attribute type.
    /// </summary>
    public static class AttributeType
    {
        public static readonly List<string> Types = new List<string> {
            /// <summary>
            /// "color" represents hexadecimal color stored as list containing one string 
            /// Example: ["#ff00e6"]
            /// </summary>
            "color",
            /// <summary>
            /// "date" represents date in yyyy-MM-dd format stored as list containing one string 
            /// Example: ["2019-02-21"]
            /// </summary>
            "date",
            /// <summary>
            /// "datetime" represents date and time in yyyy-MM-ddThh:mm format stored as list containing one string 
            /// Example: ["2019-02-11T20:57"]
            /// </summary>
            "datetime",
            /// <summary>
            /// "email" represents an email address in x@y.z format stored as list containing one string 
            /// Example: ["example@email.com"]
            /// </summary>
            "email",
            /// <summary>
            /// "month" represents month in yyyy-MM format stored as list containing one string 
            /// Example: ["2019-02"]
            /// </summary>
            "month",
            /// <summary>
            /// "int" represents signed or unsigned integer between –2,147,483,648 and 2,147,483,647 
            /// corresponding to C# int stored as list containing one string 
            /// Example: ["-1234"]
            /// </summary>
            "int",
            /// <summary>
            /// "float" represents signed or unsigned floating point number
            /// corresponding to C# float stored as list containing one string 
            /// Example: ["-12.34"]
            /// </summary>
            "float",
            /// <summary>
            /// "year" represents any at most four-digit positive or negative number stored as list containing one string 
            /// Example: ["-42"] 
            /// </summary>
            "year",
            /// <summary>
            /// "phone" represents any phone number with legal characters (numbers, "+", "(", ")", ".", "-" and ",") 
            /// stored as list containing the string as first element
            /// Example: ["+123 (456)-789"]
            /// </summary>
            "phone",
            /// <summary>
            /// "string" represents any preferably short string stored as list containing the string as first element
            /// Example: ["Hello world!"]
            /// </summary>
            "string",
            /// <summary>
            /// "time" represents time in hh:mm format stored as list containing one string 
            /// Example: ["14:19"]
            /// </summary>
            "time",
            /// <summary>
            /// "url" represents absloute or relative url stored as list containing one string 
            /// Example: ["www.example.com"]
            /// </summary>
            "url",
            /// <summary>
            /// "username" represents a unique username in UsersDatasetDescriptor stored as list containing one string
            /// Example: ["admin"]
            /// </summary>
            "username",
            /// <summary>
            /// "password" represents a string stored as list containing the string as first element
            /// Example: ["secret_password"]
            /// </summary>
            "password",
            /// <summary>
            /// "bool" represents a boolean value stored as list containing one string
            /// Example: ["0"] or ["1"]
            /// </summary>
            "bool",
            /// <summary>
            /// "text" represents any preferably long string as list containing the string as first element
            /// Example: ["Hello,\r\nI am multiline text.\r\nLorem Ipsum ..."]
            /// </summary>
            "text"
        };
    }
}