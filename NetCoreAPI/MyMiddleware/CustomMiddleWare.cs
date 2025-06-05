namespace NetCoreAPI.MyMiddleware
{
    public class CustomMiddleWare
    {
        public RequestDelegate _next { get; set; }
        public CustomMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            
            context.Response.Headers.Add("Hackedby" , "DucAnh");
            await _next(context);
            //await context.Response.WriteAsync("Hello");
        }
    }
}
