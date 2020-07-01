using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace CloudDefender.Filters
{
    public class ExceptionFilter : Attribute, IExceptionFilter
    {
        private readonly DataDbContext _db;
        public ExceptionFilter(DataDbContext db)
        {
            _db = db;
        }

        public void OnException(ExceptionContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var exceptionStack = context.Exception.StackTrace;
            var exceptionMessage = context.Exception.Message;
            var message = $"При выполнении {actionName} возникло исключение: \n {exceptionMessage}";

            _db.Set<LogEntry>().Add(new LogEntry
            {
                Title = "Произошла ошибка!",
                Message = $"{message} \n {exceptionStack}",
                OperationName = actionName
            });

            _db.SaveChanges();

            context.HttpContext.Response.StatusCode = 500;
            context.Result = new ObjectResult($"Message: {message} \nStackTrace: {exceptionStack}");
            context.ExceptionHandled = true;
        }
    }
}
