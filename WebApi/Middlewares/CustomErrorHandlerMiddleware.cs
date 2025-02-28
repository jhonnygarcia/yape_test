using Application._Base;
using System.Net;
using System.Text.Json;

namespace WebApi.Middlewares
{
    public class CustomErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict);
            }
            catch (NotFoundException ex)
            {
                await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code)
        {
            string errorMessage = string.Empty;

            switch (exception)
            {
                case BusinessException businessException:
                    errorMessage = businessException.Message;
                    break;
                case NotFoundException businessException:
                    errorMessage = businessException.Message;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = errorMessage
            }));
        }
    }
}