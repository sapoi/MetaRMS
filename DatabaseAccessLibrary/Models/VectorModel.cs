using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DatabaseAccessLibrary.Models
{
    [Table("Table")]
    public class VectorModel {
        public VectorModel(string itemName, string stringValue, DateTime? dateTimeValue, int? intValue,
                bool? boolValue, double? doubleValue, long? longValue){
            this.ItemName = itemName;
            this.StringValue = stringValue;
            this.DateTimeValue = dateTimeValue;
            this.IntValue = intValue;
            this.BoolValue = boolValue;
            this.DoubleValue = doubleValue;
            this.LongValue = longValue;
        }
        [Key]
        public int Id { get;  set; }
        [Required]
        public string ItemName { get;  set; }

        public string StringValue { get;  set; }
        public DateTime? DateTimeValue { get;  set; }
        public int? IntValue { get;  set; }
        public bool? BoolValue { get;  set; }
        public double? DoubleValue { get;  set; }
        public long? LongValue { get;  set; }
        // jak se tady dá reprezentovat libovolný enum -> jako string
        // public DEF? Def { get;  set; }

    }
}