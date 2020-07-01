using CloudDefender.Helpers;
using CloudDefender.Models.Requests;
using CloudDefender.Services;
using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = DataAccess.Models.File;

namespace CloudDefender.Controllers
{
    [Route("files")]
    public class FileController : ApiControllerBase
    {
        private readonly FolderService _folderService;
        private readonly DbSet<File> _fileRepo;
        private readonly DbSet<Folder> _folderRepo;
        public FileController(FolderService folderService, DataDbContext db) : base(db)
        {
            _folderService = folderService;
            _fileRepo = _db.Set<File>();
            _folderRepo = _db.Set<Folder>();
        }

        /// <summary>
        /// Скачивает файл
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <response code="200">Файл в бинарном формате</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(Guid fileId)
        {
            var file = _fileRepo.FirstOrDefault(f => f.Id == fileId);
            if (file == null) return BadRequest("Файл не найден!");

            var memory = new MemoryStream();
            using (FileStream stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(file.FilePath), Path.GetFileName(file.FilePath));
        }

        /// <summary>
        /// Удаляет фаил из локального хранилища
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <response code="204">Операции произведены успешно</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpDelete("{fileId}")]
        public IActionResult DeleteFile(Guid fileId)
        {
            var file = _fileRepo.FirstOrDefault(f => f.Id == fileId);
            if (file == null) return NoContent();

            _fileRepo.Remove(file);
            _db.SaveChanges();

            return NoContent();
        }

        /// <summary>
        /// Загружает файл в облако
        /// </summary>
        /// <param name="request">Объект с параметрами для загрузки файла</param>
        /// <response code="201">Возвращает идентификатор файла</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(Guid), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpPost]
        public IActionResult CreateFile([FromForm]InsertFileRequest request)
        {
            if (request.File == null) return BadRequest("Отсутствует файл в запросе");

            var folder = _folderRepo.FirstOrDefault(f => f.Id == request.FolderId);
            if (folder == null) return BadRequest("Указанная папка не существует");

            if (_fileRepo.Any(f => f.FileName == request.File.FileName && f.Folder.Id == request.FolderId)) return BadRequest("В данной папке уже существует файл с таким именем");

            var newFile = new File
            {
                FileName = request.File.FileName,
                Folder = folder,
                FilePath = DiskHelper.SaveToPC(request.File, _folderService.GetFullPath(request.FolderId))
            };

            _fileRepo.Add(newFile);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetFile), routeValues: new { fileId = newFile.Id }, newFile.Id);
        }

        /// <summary>
        /// Метод для обновления файла.
        /// </summary>
        /// <param name="request">Объект с параметрами для обновления файла</param>
        /// <response code="204">Операция выполнена успешно</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpPut]
        public IActionResult UpdateFile([FromForm]UpdateFileRequest request)
        {
            if (request.UpdatedFile == null) return BadRequest("Отсутствует файл в запросе");

            var folder = _folderRepo.FirstOrDefault(f => f.Id == request.FolderId);
            if (folder == null) return BadRequest("Указанная папка не существует");

            if (!_fileRepo.Any(f => f.FileName == request.UpdatedFile.FileName && f.Folder.Id == request.FolderId)) return BadRequest("В данной папке не существует файл с таким именем");

            var file = _fileRepo.FirstOrDefault(x => x.Id == request.FileId);
            if (file == null) return BadRequest("Не найден файл для обновления");

            file.FilePath = DiskHelper.ReplaceFile(request.UpdatedFile, file.FileName, _folderService.GetFullPath(request.FolderId));
            file.FileName = request.UpdatedFile.FileName;

            _fileRepo.Update(file);
            _db.SaveChanges();

            return NoContent();
        }

        [NonAction]
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        [NonAction]
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},  
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
    }
}
