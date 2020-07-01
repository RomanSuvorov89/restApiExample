using CloudDefender.Helpers;
using CloudDefender.Models.Requests;
using CloudDefender.Models.Responses;
using CloudDefender.Services;
using DataAccess;
using DataAccess.Models;
using DataAccess.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudDefender.Controllers
{
    [Route("folders")]
    public class FolderController : ApiControllerBase
    {
        private readonly FolderService _folderService;
        private readonly DbSet<Folder> _folderRepo;
        public FolderController(FolderService folderService, DataDbContext db) : base(db)
        {
            _folderService = folderService;
            _folderRepo = _db.Set<Folder>();
        }

        /// <summary>
        /// Выгружает информацию о папке
        /// </summary>
        /// <param name="folderId">Идентификатор папки</param>
        /// <response code="200">Объект с данными папки</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(GetFoldersResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpGet("{folderId}")]
        public IActionResult GetFolder(Guid folderId)
        {
            string folderName = "root";

            if (folderId != Guid.Empty)
            {
                var currentFolder = _folderRepo.Where(f => f.Id == folderId).FirstOrDefault();

                if (currentFolder == null) return BadRequest("Папка не найдена!");

                folderId = currentFolder.Id;
                folderName = currentFolder.FolderName;
            }

            if (folderId != Guid.Empty && !UserHasRole(folderId, AccessLevel.Read)) return BadRequest("Не достаточно уровня прав доступа!");

            var childFolders = _folderService.GetSubFolders(folderId, UserContext.Id);

            var response = new GetFoldersResponse
            {
                Folder = new FolderDescription
                {
                    FolderId = folderId,
                    FolderName = folderName,
                    Folders = childFolders.Select(f => new ChildFolder { FolderName = f.FolderName, FolderId = f.Id }).ToList(),
                    FullPath = _folderService.GetFullPath(folderId),
                    Files = _folderService.GetFiles(folderId),
                    Owners = _db.Set<UsersFolders>().Where(uf => uf.FolderId == folderId && uf.AccessLevel == AccessLevel.Owner).Select(uf => _db.Users.First(u => u.Id == uf.UserId).Username).ToList()
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Создаёт папку, привязывая её к пользователю
        /// </summary>
        /// <param name="request">Объект с параметрами для создания папки</param>
        /// <response code="201">Возвращает идентификатор папки</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(Guid), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpPost]
        public IActionResult CreateFolder(CreateFolderRequest request)
        {
            if (_folderRepo.Any(f => f.FolderName == request.NewFolderName && f.ParentFolderId == request.ParentFolderId))
                return BadRequest("В текущей папке уже существует папка с таким именем");

            if (!_folderRepo.Any(f => request.ParentFolderId == f.Id) && request.ParentFolderId != Guid.Empty)
                return BadRequest("Папки с указанным id не существует!");

            if (request.ParentFolderId != Guid.Empty && !UserHasRole(request.ParentFolderId, AccessLevel.Create)) return BadRequest("Не достаточно уровня прав доступа!");

            var newFolder = new Folder
            {
                ParentFolderId = request.ParentFolderId,
                FolderName = request.NewFolderName
            };

            DiskHelper.CreateFolder(_folderService.GetFullPath(request.ParentFolderId), request.NewFolderName);
            _db.Set<UsersFolders>().Add(new UsersFolders
            {
                AccessLevel = AccessLevel.Owner,
                User = UserContext,
                Folder = newFolder
            });
            _db.Set<Folder>().Add(newFolder);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetFolder), routeValues: new { folderId = newFolder.Id }, newFolder.Id);
        }

        /// <summary>
        /// Расшаривает папку другому пользователю
        /// </summary>
        /// <param name="request">Объект с параметрами для шары папки</param>
        /// <response code="204">Операция выполнена успешно</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpPatch]
        public IActionResult ShareFolder(ShareFolderRequest request)
        {
            if (!UserHasRole(request.FolderId, AccessLevel.Owner)) return BadRequest("Не достаточно уровня прав доступа!");

            var folder = _folderRepo.FirstOrDefault(f => f.Id == request.FolderId);
            if (folder == null) return BadRequest("Папки с таким id не существует!");

            var sharingUser = _db.Set<User>().FirstOrDefault(u => u.Username == request.Username || u.Email == request.Email);
            if (sharingUser == null) return BadRequest("Пользователя, которому вы пытаетесь дать доступ, не существует!");

            var accessLevel = ((AccessLevel)request.AccessLevel) == AccessLevel.None ? AccessLevel.Read | AccessLevel.Update : (AccessLevel)request.AccessLevel;
            var userFolder = _db.Set<UsersFolders>().FirstOrDefault(uf => uf.Folder == folder && sharingUser == uf.User);

            if (userFolder?.AccessLevel == accessLevel) return NoContent();

            if (userFolder != null)
            {
                userFolder.AccessLevel = accessLevel;
            }
            else
            {
                _db.Set<UsersFolders>().Add(new UsersFolders
                {
                    AccessLevel = accessLevel,
                    User = sharingUser,
                    Folder = folder,
                });
            }

            _db.SaveChanges();

            return NoContent();
        }

        /// <summary>
        /// Удаляет папку и все вложенные файлы
        /// </summary>
        /// <param name="folderId">Идентификатор папки</param>
        /// <response code="204">Операция выполнена успешно</response>
        /// <response code="400">Запрос составлен неверно</response>
        /// <response code="500">Произошла ошибка, обратитесь к администратору!</response>
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [HttpDelete("{folderId}")]
        public IActionResult DeleteFolder(Guid folderId)
        {
            if (!UserHasRole(folderId, AccessLevel.Delete)) return BadRequest("Не достаточно уровня прав доступа!");
            var childFolders = _folderService.GetChildFolders(folderId);
            var emptyFolders = _folderRepo.Where(f => !_db.Set<UsersFolders>().Any(uf => uf.FolderId == f.Id)).Include(folder => folder.Files);

            foreach(var folder in childFolders)
            {
                _db.Set<File>().RemoveRange(folder.Files);
                _db.Set<UsersFolders>().RemoveRange(_db.Set<UsersFolders>().Where(uf => uf.FolderId == folder.Id));
            }

            foreach (var folder in emptyFolders)
            {
                _db.Set<File>().RemoveRange(folder.Files);
                _db.Set<UsersFolders>().RemoveRange(_db.Set<UsersFolders>().Where(uf => uf.FolderId == folder.Id));
            }

            _folderRepo.RemoveRange(childFolders);
            _folderRepo.RemoveRange(emptyFolders);
            _db.SaveChanges();

            return NoContent();
        }
    }
}
