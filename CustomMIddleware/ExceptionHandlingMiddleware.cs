using Newtonsoft.Json;
using System.Net;

namespace CRUD_API_Assignment.CustomMIddleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                _next!(context!);

            }
            catch (Exception ex)
            {

                await HandleExceptionAsync(context, ex);
            }
            
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception (you can customize this part)
            Console.WriteLine($"Exception: {exception.Message}");

            // Set the response status code
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Return a JSON response with the exception details
            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(result);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}

