using Microsoft.AspNetCore.Builder;

namespace NetCoreAPI.MyMiddleware
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseMyMiddleWare(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomMiddleWare>();
        }
    }
}
