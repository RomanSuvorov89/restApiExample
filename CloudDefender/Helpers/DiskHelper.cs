using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CloudDefender.Helpers
{
    public static class DiskHelper
    {
        private static readonly string RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CloudDefender";
        public static void CreateFolder(string path, string newFolderName = null)
        {
            var subPath = string.IsNullOrWhiteSpace(path) ? string.Empty : $@"\{path}";
            newFolderName = string.IsNullOrWhiteSpace(newFolderName) ? string.Empty : $@"\{newFolderName}";
            var fullPath = Path.Combine($@"{RootFolder}{subPath}{newFolderName}");
            Directory.CreateDirectory(fullPath);
        }

        public static string SaveToPC(this IFormFile file, string path)
        {
            var filePath = Path.Combine(RootFolder + path, file.FileName);

            if (!Directory.Exists(RootFolder + path)) CreateFolder(RootFolder + path);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return filePath;
        }

        public static string ReplaceFile(this IFormFile newFile, string oldName, string path)
        {
            File.Delete(Path.Combine(RootFolder + path, oldName));
            return SaveToPC(newFile, path);
        }
    }
}
