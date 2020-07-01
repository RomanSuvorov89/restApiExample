using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class User : BaseModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
