using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Descriptors;

namespace RazorWebApp.Helpers
{
    public class ValidationHelper
    {
        /// <summary>
        /// This method validates and adds missimg attributes values to data dictionary received from the Razor pages.
        /// </summary>
        /// <param name="dataDictionary">Data dictionary to validate</param>
        /// <param name="attributes">List of attributes that are required to be in the dataDictioanry</param>
        public void ValidateDataDictionary(Dictionary<string, List<string>> dataDictionary, List<AttributeDescriptor> attributes)
        {
            foreach (var attribute in attributes)
            {
                // If nothing was selected in a select box, the key (attributeName) was removed and 
                // before sending new values to API, the key needs to be reentered with empty values
                if (!dataDictionary.Keys.Contains(attribute.Name))
                    dataDictionary.Add(attribute.Name, new List<string>());
                // If basic type field for attributeName is not filled in, dataDictionary[attributeName][0] == null
                // the database and backend does not accept null as value, so the whole dataDictionary[attributeName]
                // needs to be replaced by new List<string>
                if (dataDictionary[attribute.Name].Count == 1 && dataDictionary[attribute.Name][0] == null)
                    dataDictionary[attribute.Name] = new List<string>();
            }
        }
    }
}