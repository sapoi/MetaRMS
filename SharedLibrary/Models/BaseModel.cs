using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedLibrary.Models
{
    public interface BaseModel
    {
        long Id { get; set; }
    }
}
