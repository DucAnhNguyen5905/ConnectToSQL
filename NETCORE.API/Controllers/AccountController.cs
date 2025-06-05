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
using static DataAccess.Netcore.DO.AcountLoginRequest;

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


        [HttpPost("Account_LogOut")]
        [CSharpCoBanAuthorizeAttribute("Account_LogOut", "")]
        public async Task<ActionResult> Account_LogOut(Account_LogOutRequestData requestData)
        {
            try
            {
                // Lấy được AccountID từ Filter 
                var AccountID = UserManagerSession.AccountID;

                // Lấy DeviceID 
                // kết hợp User_Session_AccountID_DeviceID // User_Session_3_DEVICE_01
                var keyCache = "User_sessions_" + AccountID + "_" + requestData.DeviceID;

                // Xóa cái key User_Session_3_DEVICE_01 trong redis

                _cache.Remove(keyCache);

                return Ok(new { mes = "LogOut Thành công !" });
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [HttpPost("Account_Login")]
        public async Task<IActionResult> Account_Login(AcountLoginRequest requestData)
        {
            var result = new LoginReturnData();
            var listKey = new List<string>();
            try
            {
                // Bước 0 :

                // Kết nối đến Redis server (sửa lại nếu bạn dùng host/port khác)
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");

                // Lấy database (mặc định là DB 0)
                IDatabase db = redis.GetDatabase();

                // Lấy server object để có thể truy vấn key
                // Lưu ý: cần biết rõ endpoint để tạo đối tượng server
                var endpoints = redis.GetEndPoints();
                IServer server = redis.GetServer(endpoints[0]);

                // Lấy tất cả key (có thể thêm pattern nếu cần)
                foreach (var key in server.Keys())
                {
                    listKey.Add(key.ToString().Substring(0, 5));
                }



                // bước 1: CheckLogin 
                var resultLogin = await _unitOfWork._accountRepository.Account_Login(requestData);
                if (resultLogin == null)
                {
                    result.ResponseCode = -1;
                    result.ResponseMessage = "Tài khoản không tồn tại";
                    return Ok(result);
                }

                // Kiểm tra số lượng Sessison 

                var keySessions = "User_sessions_" + resultLogin.AccountID;

                //  var CountKey = listKey.FindAll(s => s.Equals(keySessions)).Count;
                var CountKey = 0;
                if (listKey.Count > 0)
                {
                    foreach (var item in listKey)
                    {
                        if (item == keySessions) { CountKey++; }
                    }
                }
                if (CountKey >= 2)
                {
                    result.ResponseCode = -21;
                    result.ResponseMessage = "Tài khoản của bạn chỉ được phép đăng nhập trên 02 thiết bị ";
                    return Ok(result);
                }

                // Bước 2 : Tạo claims 

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, resultLogin.UserName),
                    new Claim(ClaimTypes.PrimarySid, resultLogin.AccountID.ToString()),
                    new Claim(ClaimTypes.GivenName, resultLogin.Fullname.ToString())
                };

                // Bước 3 : Tạo token

                var newToken = CreateToken(claims);

                var token = new JwtSecurityTokenHandler().WriteToken(newToken);


                // bước 4: Lưu token vào database/Redis (nếu cần thiết)

                var user_sessions = new User_Sessions
                {
                    AccountID = resultLogin.AccountID,
                    Token = token,
                    DeviceID = requestData.DeviceID,
                    ExpriredTime = new JwtSecurityToken(token).ValidTo
                };

                var keyCaching = $"User_sessions_{resultLogin.AccountID}_{requestData.DeviceID}";
                var options_cache = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(30) // Thời gian hết hạn của token
                };

                var data_cache = JsonConvert.SerializeObject(user_sessions);

                await _cache.SetStringAsync(keyCaching, data_cache, options_cache);

                // Bước 4: Trao kết quả cho client
                result.ResponseCode = 1;
                result.ResponseMessage = "Login Successful";
                result.token = token;
                result.AccountID = resultLogin.AccountID;
                result.UserName = resultLogin.UserName;
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

        [HttpPost("Upload_Ava")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadFileInputDto input)
        {
            if (input.File == null || input.File.Length == 0)
                return BadRequest("File null");

            // Lưu vào thư mục Files (ngoài wwwroot)
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Files");

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, input.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await input.File.CopyToAsync(stream);
            }

            // Trả về URL để truy cập ảnh
            var fileUrl = $"{Request.Scheme}://{Request.Host}/Files/{input.File.FileName}";
            return Ok(new { message = "Upload successful!", url = fileUrl });
        }

        [HttpPost("Account_UploadImage")]
        public async Task<IActionResult> Account_UploadImage(AccountUploadImageRequest requestData)
        {
            var returnData = new ReturnData();
            try
            {
                string imgPath = string.Empty;
                if (requestData == null
                    || string.IsNullOrEmpty(requestData.Base64ImageString))
                {
                    returnData.ResponseCode = -1;
                    returnData.ResponseMessage = "Dữ liệu đầu vào không hợp lệ";
                    return Ok(returnData);
                }


                var path = "files"; //Path

                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string imageName = Guid.NewGuid().ToString() + ".png";


                //set the image path
                imgPath = Path.Combine(path, imageName);
                if (!requestData.Base64ImageString.Contains("data:image"))
                {
                    returnData.ResponseCode = -2;
                    returnData.ResponseMessage = "Dữ liệu đầu vào không hợp lệ";
                    return Ok(returnData);
                }

                requestData.Base64ImageString = requestData.Base64ImageString.Substring(requestData.Base64ImageString.LastIndexOf(',') + 1);
                byte[] imageBytes = Convert.FromBase64String(requestData.Base64ImageString);
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                image.Save(imgPath, System.Drawing.Imaging.ImageFormat.Png);

                returnData.ResponseCode = 1;
                returnData.ResponseMessage = imageName;

                return Ok(returnData);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file: {ex.Message}");
            }
        }








    }
}
