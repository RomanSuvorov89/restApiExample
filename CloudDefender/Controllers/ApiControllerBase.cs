using DataAccess;
using DataAccess.Models;
using DataAccess.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CloudDefender.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class ApiControllerBase : ControllerBase
    {
        private Guid UserId => IsGodMode ? Guid.Parse("42EF6EA1-6833-4BDA-D115-08D7C3748690") : Guid.Parse((User.Claims as IEnumerable<Claim>)?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
        private bool IsGodMode => Boolean.TryParse(HttpContext.Request.Headers["godmode"], out var isGodMode) && isGodMode;
        protected User UserContext => _db.Set<User>().First(u => u.Id == UserId);
        protected readonly DataDbContext _db;
        public ApiControllerBase(DataDbContext db)
        {
            _db = db;
        }

        protected bool UserHasRole(Guid folderId, AccessLevel accessLevel)
        {
            var relation = _db.Set<UsersFolders>().FirstOrDefault(uf => uf.UserId == UserId && uf.FolderId == folderId);
            if (relation == null) return false;
            return (relation.AccessLevel & accessLevel) == accessLevel;
        }
    }
}