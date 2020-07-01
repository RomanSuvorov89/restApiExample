using DataAccess.Models.Enum;
using System;

namespace CloudDefender.Models.Requests
{
    public class ShareFolderRequest
    {
        public Guid FolderId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// По умолчанию Read\Update
        /// </summary>
        public int AccessLevel { get; set; }
    }
}
