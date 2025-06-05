using DataAccessNetcore.DO;
using DataAccessNetcore.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpPost("Account_GetAll")]
        public async Task<IActionResult> Account_GetAll()
        {
            try
            {
                var result = await _accountRepository.Accounts_GetAll();
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

        [HttpPost("Account_Delete")]
        public async Task<IActionResult> AccountDelete([FromBody] AccountDelete_RequestData requestData)
        {
            try
            {

                var result = await _accountRepository.Account_Delete(requestData);
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
