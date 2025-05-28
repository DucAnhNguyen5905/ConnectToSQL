using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataAccess.Netcore.EfCore;
using Microsoft.EntityFrameworkCore;

namespace NetCore.API.Filter
{
    public class CSharpCoBanAuthorizeAttribute : TypeFilterAttribute
    {
        public CSharpCoBanAuthorizeAttribute(string functionCode, string permission) : base(typeof(DemoAuthorizeActionFilter))
        {
            Arguments = new object[] { functionCode, permission };
        }
    }
    public class DemoAuthorizeActionFilter : IAsyncAuthorizationFilter
    {
        public readonly CSharpCoBanDbContext _dbcontext;
        private readonly string _functionCode;
        private readonly string _permission;
        public DemoAuthorizeActionFilter(CSharpCoBanDbContext dbcontext, string functionCode, string permission)
        {
            _dbcontext = dbcontext;
            _functionCode = functionCode;
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var identity = context.HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                var userId = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid)?.Value != null
                    ? Convert.ToInt32(userClaims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid)?.Value) : 0;

                if (userId == 0)
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    context.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    context.Result = new JsonResult(new
                    {
                        ReturnCode = System.Net.HttpStatusCode.Unauthorized,
                        ReturnMessage = "Vui lòng đăng nhập để thực hiện chức năng này "
                    });


                }

                // Laays functionID

                var function = _dbcontext.function.FirstOrDefault(x => x.FunctionCode == _functionCode);
                if (function == null || function.FunctionID <= 0)
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    context.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    context.Result = new JsonResult(new
                    {
                        ReturnCode = System.Net.HttpStatusCode.Unauthorized,
                        ReturnMessage = "Chức năng không tồn tại"
                    });

                    return;
                }


                // Check permission
                var list = _dbcontext.permission.ToList() ;

                var permission = _dbcontext.permission.FirstOrDefault(x => x.AccountID == userId
                && x.FunctionID == function.FunctionID);





                if (permission == null)
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    context.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    context.Result = new JsonResult(new
                    {
                        ReturnCode = System.Net.HttpStatusCode.Unauthorized,
                        ReturnMessage = "Bạn không có quyền thực hiện chức năng này"
                    });
                }

                if (_permission == "ISVIEWS" && permission != null && permission.IsView == 0)
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    context.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    context.Result = new JsonResult(new
                    {
                        ReturnCode = System.Net.HttpStatusCode.Unauthorized,
                        ReturnMessage = "Bạn không có quyền xem chức năng này"
                    });
                }

            }

            return ;
        }
    }
}