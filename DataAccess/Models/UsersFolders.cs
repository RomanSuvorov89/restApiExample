using DataAccess.Models.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class UsersFolders : BaseModel
    {
        public Guid UserId { get; set; }
        public Guid FolderId { get; set; }
        public AccessLevel AccessLevel { get; set; }

        public User User { get; set; }
        public Folder Folder { get; set; }
    }
}
