using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PhamQuocHuy_Report_BaiTongHop_FE_BE.Models;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace PhamQuocHuy_Report_BaiTongHop_FE_BE.Controllers
{
	public class UsersController : Controller
	{
        private readonly string _apiUrl = "http://localhost:18389/api/Users";
        private readonly string apiUrl = "http://localhost:18389/api/Users";
        private readonly HttpClient _httpClient;
        private readonly TokenValidationParameters _tokenValidationParameters;
        public UsersController(HttpClient httpClient, TokenValidationParameters tokenValidationParameters)
        {
            _httpClient = httpClient;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public IActionResult Index()
		{
			return View();
		}

        [HttpGet("Users/DetailsUsers/{id}")]
        public async Task<IActionResult> DetailsUsers(int id)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Bạn cần phải đăng nhập để truy cập trang này.";
                return RedirectToAction("Login", "Users");
            }

            Users user;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy",
                        Culture = new System.Globalization.CultureInfo("en-GB")
                    };

                    user = JsonConvert.DeserializeObject<Users>(apiResponse, settings);
                }
            }
            return View(user);
        }

        [HttpGet("Users/UpdateAccount/{id}")]
        public async Task<IActionResult> UpdateAccount(int id)
        {
        
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Bạn cần phải đăng nhập để truy cập trang này.";
                return RedirectToAction("Login", "Users");
            }

            Users user;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{apiUrl}/{id}"))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy",
                        Culture = new System.Globalization.CultureInfo("en-GB")
                    };

                    user = JsonConvert.DeserializeObject<Users>(apiResponse, settings);
                }
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAccount(Users user, int id)
        {
            if (ModelState.IsValid)
            {
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        var response = await httpClient.PutAsJsonAsync($"{apiUrl}/{id}", user);

                        if (response.IsSuccessStatusCode)
                        {
                            ViewBag.Success = "Cập nhật thành công";
                            return RedirectToAction("DetailsUsers", new { id = user.Id });
                        }
                        else
                        {
                            var errorResponse = await response.Content.ReadAsStringAsync();
                            ViewBag.Error = $"Cập nhật thất bại: {errorResponse}";
                            ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = $"Đã xảy ra lỗi: {ex.Message}";
                        ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                    }
                }
            }
            else
            {
                ViewBag.Error = "Cập nhật thất bại do xảy ra lỗi trong dữ liệu.";
            }
            return View(user);
        }
        public async Task<IActionResult> List()
        {
            List<Users> usersList = new List<Users>();

            try
            {
                // Lấy token từ session
                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["Message"] = "Bạn cần phải đăng nhập để truy cập trang này.";
                    return RedirectToAction("Login");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        var settings = new JsonSerializerSettings
                        {
                            DateFormatString = "dd/MM/yyyy",
                            Culture = new System.Globalization.CultureInfo("en-GB")
                        };

                        usersList = JsonConvert.DeserializeObject<List<Users>>(apiResponse, settings);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ViewBag.Message = "Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ViewBag.Message = $"Lỗi khi gọi API: {response.StatusCode}";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Message = $"Lỗi khi gọi API: {ex.Message}";
            }
            catch (JsonSerializationException ex)
            {
                ViewBag.Message = $"Lỗi khi chuyển đổi dữ liệu: {ex.Message}";
            }

            return View(usersList);
        }

        public IActionResult CreateAccount()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Bạn cần phải đăng nhập để truy cập trang này.";
                return RedirectToAction("Login", "Users");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(Users user, string repass)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (user.password != repass)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận mật khẩu không khớp.");
                return View(user);
            }

            try
            {
                HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync(_apiUrl, user);

                if (responseMessage.IsSuccessStatusCode)
                {
                   
                    ViewBag.Message = "Tạo tài khoản thành công!";
                    return RedirectToAction("List");
                }
                else
                {
                    string errorResponse = await responseMessage.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Tạo tài khoản thất bại: {errorResponse}");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Lỗi khi gọi API: {ex.Message}");
            }

            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync($"{apiUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Đã xóa thành công User.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "Không tìm thấy User.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa User.";
                }
            }

            return RedirectToAction("List");
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto loginModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Dữ liệu đăng nhập không hợp lệ.";
                return View(loginModel);
            }

            var response = await _httpClient.PostAsJsonAsync("http://localhost:18389/api/Jwt/GenerateToken", loginModel);

            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadAsStringAsync();
                ViewBag.Token = token;
                ViewBag.Message = "Đăng nhập thành công! Token được tạo.";
                HttpContext.Session.SetString("JWTToken", token);
                return View();
            }
            else
            {
                ViewBag.Error = "Đăng nhập thất bại. Vui lòng kiểm tra lại tên đăng nhập hoặc mật khẩu.";
                return View(loginModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ValidationMessage = "Token không được để trống.";
                return View("Login");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);
                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    HttpContext.Session.SetString("Token", token);
                    return RedirectToAction("List");
                }
                else
                {
                    ViewBag.ValidationMessage = "Token không hợp lệ.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ValidationMessage = $"Token không hợp lệ. Lỗi: {ex.Message}";
                return View("Login");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWTToken");
            HttpContext.Session.Remove("Token");
            return RedirectToAction("Login");
        }


    }
}
