// using System.Collections.Generic;
// using SharedLibrary.Enums;

// namespace SharedLibrary.Helpers
// {
//     class MessageTextHelper
//     {
//         private MessageTextHelper() 
//         { 
//             // Load files with error codes to the dictionary
//             translationsStructure = new Dictionary<LanguageEnum, Dictionary<int, string>>();
//             translationsStructure[LanguageEnum.En] = new Dictionary<int, string>();
//             translationsStructure[LanguageEnum.Cs] = new Dictionary<int, string>();
//             translationsStructure[LanguageEnum.Sk] = new Dictionary<int, string>();

//             foreach (var language in translationsStructure.Keys)
//             {
                
//             }
//         }

//         private Dictionary<LanguageEnum, Dictionary<int, string>> translationsStructure;

//         public static readonly MessageTextHelper instance = new MessageTextHelper();

//         public string GetMessage(int code, LanguageEnum language)
//         {
//             return translationsStructure[language][code];
//         }

//     }
// }