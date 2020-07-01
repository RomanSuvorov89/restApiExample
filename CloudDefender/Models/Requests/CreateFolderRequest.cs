using System;

namespace CloudDefender.Models.Requests
{
    public class CreateFolderRequest
    {
        public Guid ParentFolderId { get; set; }
        public string NewFolderName { get; set; }
    }
}
