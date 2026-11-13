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

        public GlobalExceptionFilter(
            ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            // רישום השגיאה ללוג
            _logger.LogError(
                ex,
                "Exception at {Path}",
                context.HttpContext.Request.Path);

            // התאמת קוד HTTP לסוג השגיאה
            int status = ex switch
            {
                NotFoundException =>
                    StatusCodes.Status404NotFound,

                ConflictException =>
                    StatusCodes.Status409Conflict,

                BusinessRuleException =>
                    StatusCodes.Status400BadRequest,

                ArgumentException =>
                    StatusCodes.Status400BadRequest,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

            // לא חושפים פרטים פנימיים בשגיאות לא צפויות
            string message = status == 500
                ? "אירעה תקלה בשרת. נסי שוב מאוחר יותר."
                : ex.Message;

            // תשובת ProblemDetails עם שדה message נוסף
            var problem = new ProblemDetails
            {
                Status = status,
                Title = message
            };

            problem.Extensions["message"] = message;

            context.Result = new ObjectResult(problem)
            {
                StatusCode = status
            };

            // סימון שהחריגה טופלה
            context.ExceptionHandled = true;
        }
    }
}