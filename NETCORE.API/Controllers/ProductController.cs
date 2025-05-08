using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Netcore.IRepository;

namespace NETCORE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public ProductController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpPost("Product_GetAll")]
        public async Task<IActionResult> Product_GetAll()
        {
            try
            {
                var result = await _accountRepository.Products_GetAll();
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
    