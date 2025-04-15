namespace back_end.Middleware
{
    public class ExampleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExampleMiddleware> _logger;

        public ExampleMiddleware(RequestDelegate next, ILogger<ExampleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Request received: {context.Request.Path}");
            
            // Puedes realizar operaciones antes de pasar la solicitud al siguiente middleware
            
            await _next(context);
            
            // Puedes realizar operaciones después de que el siguiente middleware haya procesado la solicitud
        }
    }

    // Método de extensión para facilitar el registro del middleware
    public static class ExampleMiddlewareExtensions
    {
        public static IApplicationBuilder UseExampleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExampleMiddleware>();
        }
    }
}