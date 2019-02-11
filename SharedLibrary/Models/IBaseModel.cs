using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedLibrary.Models
{
    /// <summary>
    /// BaseModel is a interface for all models.
    /// </summary>
    public interface IBaseModel
    {
        /// <summary>
        /// Id property.
        /// </summary>
        /// <value>Id is a unique identificator in the database.</value>
        long Id { get; set; }
    }
}
