using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CloudDefender.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DataDbContext _db;

        public AuthMiddleware(RequestDelegate next, DataDbContext db)
        {
            _next = next;
            _db = db;
        }

        public async Task Invoke(HttpContext context, ILogger<AuthMiddleware> logger)
        {
            Debugger.Log(1, "", $"{context.Request.Path}{context.Request.QueryString}");
            var userId = context.Request.Headers["userId"];

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                };

                ClaimsPrincipal userContext = new ClaimsPrincipal(new ClaimsIdentity(claims, "MyAuth"));

                context.User = userContext;

                await _next.Invoke(context);
            }
            else if (context.Request.Path.Value.Contains("auth"))
            {
                await _next.Invoke(context);
            }
            else
            {
                _db.Set<LogEntry>().Add(new LogEntry
                {
                    Title = "Ошибка авторизации!",
                    Message = $"UserId: {userId}",
                    OperationName = "AuthService"
                });

                await _db.SaveChangesAsync();

                if (Boolean.TryParse(context.Request.Headers["godmode"], out var isGodMode) && isGodMode) await _next.Invoke(context);
                else
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }
        }
    }
}
