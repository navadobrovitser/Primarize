using BLL.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            // 1. רישום השגיאה ללוג
            _logger.LogError(ex, "Unhandled exception at {Path}", context.HttpContext.Request.Path);

            // 2. קביעת קוד ה-HTTP לפי סוג החריגה שנזרקה ב-BLL
            int status = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                BusinessRuleException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            // 3. בשגיאות שרת כלליות (500) לא חושפים פרטים פנימיים למשתמש
            string message = status == 500
                ? "An unexpected error occurred. Please try again later."
                : ex.Message;

            // 4. החזרת תשובה במבנה ProblemDetails תקני
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = status,
                Title = message
            })
            {
                StatusCode = status
            };

            // 5. סימון שהשגיאה טופלה
            context.ExceptionHandled = true;
        }
    }
}