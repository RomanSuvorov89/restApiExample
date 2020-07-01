using CloudDefender.Models.Responses;
using DataAccess;
using DataAccess.Models;
using DataAccess.Models.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudDefender.Services
{
    public class FolderService
    {
        private readonly DataDbContext _db;
        public FolderService(DataDbContext db)
        {
            _db = db;
        }

        public string GetFullPath(Guid currentFolderId)
        {
            if (currentFolderId == Guid.Empty)
                return string.Empty;

            var currentFolder = _db.Set<Folder>().FirstOrDefault(f => f.Id == currentFolderId);

            return GetFullPath(currentFolder.ParentFolderId) + @"\" + currentFolder.FolderName;
        }

        public List<Folder> GetSubFolders(Guid currentFolderId, Guid userId)
        {
            var folders = _db.Set<Folder>().Where(f => f.ParentFolderId == currentFolderId).ToList();

            foreach(var folder in folders.ToList())
            {
                var folderUser = _db.Set<UsersFolders>().FirstOrDefault(uf => uf.FolderId == folder.Id && uf.UserId == userId);
                if (folderUser == null || (folderUser.AccessLevel & AccessLevel.Read) != AccessLevel.Read) folders.Remove(folder);
            }

            return folders;
        }

        public IEnumerable<Folder> GetChildFolders(Guid currentFolderId)
        {
            var subFolders = new List<Folder>();
            var currentFolder = _db.Set<Folder>().Include(folder => folder.Files).FirstOrDefault(f => f.Id == currentFolderId);

            foreach (var folder in _db.Set<Folder>().Where(f => f.ParentFolderId == currentFolderId).Include(folder => folder.Files))
            {
                subFolders.AddRange(GetChildFolders(folder.Id));
            }

            subFolders.Add(currentFolder);

            return subFolders;
        }

        public List<FileDescription> GetFiles(Guid folderId)
        {
            var files = _db.Set<File>().Where(f => f.Folder.Id == folderId).ToList();
            return files.Select(f => new FileDescription { FileId = f.Id, FileName = f.FileName }).ToList();
        }
    }
}
