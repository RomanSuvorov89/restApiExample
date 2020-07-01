using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudDefender.Models.Responses
{
    public class GetFoldersResponse
    {
        public FolderDescription Folder { get; set; }
    }

    public class FolderDescription
    {
        public string FullPath { get; set; }
        public string FolderName { get; set; }
        public Guid FolderId { get; set; }
        public List<FileDescription> Files { get; set; }
        public List<ChildFolder> Folders { get; set; }
        public List<string> Owners { get; set; }
    }

    public class ChildFolder
    { 
        public Guid FolderId { get; set; }
        public string FolderName { get; set; }
    }

    public class FileDescription
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
    }
}
