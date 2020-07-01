using System;

namespace DataAccess.Models
{
    public class File : BaseModel
    {
        public virtual Folder Folder { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
