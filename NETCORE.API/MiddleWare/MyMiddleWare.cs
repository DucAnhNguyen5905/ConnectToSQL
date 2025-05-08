namespace NETCORE.API.MiddleWare
{
    public class MyMiddleWare
    {
        public RequestDelegate _next { get; set; }

        public MyMiddleWare (RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);
        }
    }
}
