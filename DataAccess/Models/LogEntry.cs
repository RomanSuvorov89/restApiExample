using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class LogEntry : BaseModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string OperationName { get; set; }
    }
}
