using System.Collections.Generic;

namespace SharedLibrary.Enums
{
    /// <summary>
    /// List of simple data types, that can be used at attribute type.
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
            /// "date" represents date in YYYY-MM-DD format stored as list containing one string 
            /// Example: ["2019-02-21"]
            /// </summary>
            "date",
            /// <summary>
            /// "datetime" represents date and time in YYYY-MM-DDTHH:MM format stored as list containing one string 
            /// Example: ["2019-02-11T20:57"]
            /// </summary>
            "datetime",
            /// <summary>
            /// "email" represents an email address in x@y.z format stored as list containing one string 
            /// Example: ["example@email.com"]
            /// </summary>
            "email",
            /// <summary>
            /// "month" represents month in YYYY-MM format stored as list containing one string 
            /// Example: ["2019-02"]
            /// </summary>
            "month",
            /// <summary>
            /// "int" represents any signed or unsigned integer stored as list containing one string 
            /// Example: ["-1234"] or ["-2.34567890876543212e+113"] or even ["-2345678908765432125678909876543213456789087654321345678908765432123456789098765432123456789876543214567872123456787654321234567890876543212345678900987654321234567890"]
            /// </summary>
            "int",
            /// <summary>
            /// "float" represents any signed or unsigned floating point number stored as list containing one string 
            /// Example: ["-12.34"] or ["-1.23456789098765432e+173"] or even ["-123456789098765432134567812345678909876543213456781234567890987654321345678123456321345678.12345678909876543213456781234567890987654321345678123456789087654321345678"]
            /// </summary>
            "float",
            /// <summary>
            /// "year" represents any at most four-digit positive or negative number stored as list containing one string 
            /// Example: ["-42"] 
            /// </summary>
            "year",
            //TODO popis
            "tel",
            /// <summary>
            /// "string" represents any preferably short string stored as list containing the string as first element
            /// Example: ["Hello world!"]
            /// </summary>
            "string",
            /// <summary>
            /// "time" represents time in HH:MM format stored as list containing one string 
            /// Example: ["14:19"],
            /// </summary>
            "time",
            //TODO popis
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