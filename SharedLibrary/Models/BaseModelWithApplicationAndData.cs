using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedLibrary.Models
{
    public interface BaseModelWithApplicationAndData : BaseModel
    {
        // long Id { get; set; }
        long ApplicationId { get; set; }
        ApplicationModel Application { get; set; }
        string Data { get; set; }
        Dictionary<string, List<object>> DataDictionary { get; }
    }
}
