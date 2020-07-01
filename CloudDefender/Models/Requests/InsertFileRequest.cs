using Microsoft.AspNetCore.Http;
using System;

namespace CloudDefender.Models.Requests
{
    public class InsertFileRequest
    {
        public Guid FolderId { get; set; }
        public IFormFile File { get; set; }
    }
}
