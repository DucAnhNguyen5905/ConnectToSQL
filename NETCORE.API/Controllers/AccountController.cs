using DataAccess.Netcore.IRepository;
using DataAccess.Netcore.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Netcore.DO;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
        public AccountController (IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        [HttpPost("Account_GetAll")]
        public async Task<IActionResult> Account_GetAll()
        {
            try
            {
                var result = await _unitOfWork._accountRepository.Accounts_GetAll();
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
        public async Task<IActionResult> Account_Delete([FromBody] AccountDeleteRequest requestData)
        {
            try
            {
                var result = await _unitOfWork._accountRepository.Account_Delete(requestData);
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
                var result = await _unitOfWork._accountRepository.Account_Update(requestData);
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
            catch (Exception )
            {
                throw ;
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
