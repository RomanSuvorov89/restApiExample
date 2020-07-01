using Microsoft.AspNetCore.Http;
using System;

namespace CloudDefender.Models.Requests
{
    public class UpdateFileRequest
    {
        public Guid FolderId { get; set; }
        public Guid FileId { get; set; }
        public IFormFile UpdatedFile { get; set; }
    }
}
