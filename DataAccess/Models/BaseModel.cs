using DataAccess.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccess.Models
{
    public class BaseModel
    {
        [Key]
        public Guid Id { get; internal set; }
        public DateTime CreatedOn { get; internal set; }
        public DateTime ModifiedOn { get; internal set; }
        public StateCode StateCode { get; internal set; }
    }
}
