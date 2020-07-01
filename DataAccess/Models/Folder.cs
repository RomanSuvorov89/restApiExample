using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Folder : BaseModel
    {
        public string FolderName { get; set; }

        public Guid ParentFolderId { get; set; }
        //public virtual Folder ParentFolder { get; set; }

        public ICollection<File> Files { get; set; }
    }
}
