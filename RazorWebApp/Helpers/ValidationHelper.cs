using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Descriptors;

namespace RazorWebApp.Helpers
{
    public class ValidationHelper
    {
        // frontend
        public void ValidateValueList(Dictionary<string, List<string>> valueList, List<AttributeDescriptor> attributes)
        {
            foreach (var attribute in attributes)
            {
                // if nothing was selected in a select box, the key (attributeName) was removed and 
                // before sending new values to API, the key needs to be reentered with empty values
                if (!valueList.Keys.Contains(attribute.Name))
                    valueList.Add(attribute.Name, new List<string>());
                // if simple type field for attributeName is not filled in, ValueList[attributeName][0] == null
                // the database and backend does not accept null as value, so the whole ValueList[attributeName]
                // needs to be replaced by new List<string>
                if (valueList[attribute.Name].Count == 1 && valueList[attribute.Name][0] == null)
                    valueList[attribute.Name] = new List<string>();
            }
        }
    }
}