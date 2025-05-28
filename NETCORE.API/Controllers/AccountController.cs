using DataAccess.Netcore.IRepository;
using DataAccess.Netcore.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Netcore.DO;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using NetCore.API.Filter;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using StackExchange.Redis;

namespace NETCORE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        //private readonly IAccountRepository _accountRepository;

        //public AccountController(IAccountRepository accountRepository)
        //{
        //   _accountRepository = accountRepository;
        //}

        private IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        public AccountController(IUnitOfWork unitOfWork, IConfiguration configuration, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _cache = cache;
        }
        [HttpPost("Account_GetAll")]
        public async Task<IActionResult> Account_GetAll()
        {
            var result = new List<Account>();
            try
            {
                //var result = await _accountRepository.Acccounts_GetAll();
                //Lần đầu thì thì vào database để lấy dữ liệu

                // kiểm tra trong caching 
                var cacheKey = "GetAll_KeyCaching";
                var cacheData = await _cache.GetStringAsync(cacheKey);

                if (cacheData != null)
                {
                    // nếu trong caching có dữ liệu thì lấy luôn
                    result = JsonConvert.DeserializeObject<List<Account>>(cacheData);
                    return Ok(result);
                }

                // nếu trong caceh không có thì vào db để lấy dữ liệu
                result = await _unitOfWork._accountGenericRepository.GetAll();

                // set dữ liệu vào cache
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1))
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(1));

                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(result), options);

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

        [HttpPost("Account_Update")]
        public async Task<IActionResult> Account_Update([FromBody] AccountUpdateRequest requestData)
        {
            try
            {
                if (requestData == null || requestData.AccountID <= 0)
                {
                    return BadRequest("Invalid account data.");
                }

                // Cập nhật vào database
                var existingAccount = await _unitOfWork._accountGenericRepository.GetById(requestData.AccountID);
                if (existingAccount == null)
                {
                    return NotFound("Account not found.");
                }

                // Gán lại dữ liệu
                existingAccount.AccountID = requestData.AccountID;
                existingAccount.Address = requestData.Address;
                // Thêm các trường cần cập nhật tại đây

                await _unitOfWork._accountGenericRepository.Update(existingAccount);
                await _unitOfWork.CompleteAsync(); // Lưu thay đổi

                // --- CẬP NHẬT CACHE ---
                var cacheKey = "Update_KeyCaching";
                var allAccounts = await _unitOfWork._accountGenericRepository.GetAll();
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1))
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(1));

                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(allAccounts), cacheOptions);

                return Ok(existingAccount);
            }
            catch (Exception ex)
            {
                // Log lỗi 
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }


        [HttpPost("Account_Insert")]
        public async Task<IActionResult> Account_Insert([FromBody] AccountInsertRequest requestData)
        {
            var result = new Account();
            try
            {
                var cacheKey = "Insert_KeyCaching";

                // Map dữ liệu từ requestData sang Account
                result.AccountID = requestData.AccountID;
                result.UserName = requestData.UserName;
                result.Password = requestData.Password;
                result.Address = requestData.Address;
                result.Fullname = requestData.Fullname;
                result.Isadmin = requestData.Isadmin;

                // Insert vào DB
                await _unitOfWork._accountGenericRepository.Add(result);

                // Ghi dữ liệu vào Redis Cache
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1))
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(1));

                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(result), options);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }

        [HttpPost("Account_Delete")]
        public async Task<IActionResult> Account_Delete([FromBody] AccountDeleteRequest requestData)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (requestData == null || requestData.AccountID <= 0)
                {
                    return BadRequest("Invalid data !.");
                }

                // Kiểm tra xem tài khoản có tồn tại không
                var account = await _unitOfWork._accountGenericRepository.GetById(requestData.AccountID);
                if (account == null)
                {
                    return NotFound($"Can not found any account with ID: {requestData.AccountID}");
                }

                // Xóa tài khoản
                _unitOfWork._accountGenericRepository.Delete(requestData.AccountID);
                _unitOfWork.SaveChanges(); // Hoặc await SaveChangesAsync() nếu có hỗ trợ async

                // Xóa cache nếu có
                var cacheKey = "Delete_KeyCaching";
                await _cache.RemoveAsync(cacheKey);

                return Ok(new
                {
                    success = true,
                    message = "Delete Successful !."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error.");
            }
        }





        [HttpPost("Account_Login")]
        public async Task<IActionResult> Account_Login(AcountLoginRequest requestData)
        {
            var result = new LoginReturnData();
            try
            {
                //Check login
                var resultLogin = await _unitOfWork._accountRepository.Account_Login(requestData);
                if (resultLogin == null)
                {
                    result.ResponseCode = -1;
                    result.ResponseMessage = " Tai khoan khong ton tai !";
                    return Ok(result);
                }
                // tạo claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, resultLogin.UserName),
                    new Claim(ClaimTypes.PrimarySid, resultLogin.AccountID.ToString()),
                    new Claim(ClaimTypes.GivenName, resultLogin.Fullname.ToString())
                };

                // tạo token
                var newToken = CreateToken(claims);

                var token = new JwtSecurityTokenHandler().WriteToken(newToken);

                //Trao kết quả cho client
                result.ResponseCode = 1;
                result.ResponseMessage = " Dang nhap thanh cong !";
                result.token = token;
                result.UserName = resultLogin.UserName;
                result.AccountID = resultLogin.AccountID;
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        






        
    }
}
