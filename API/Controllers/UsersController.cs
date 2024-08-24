using API.Data;
using API.Model;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public readonly ApplicationDbContext _context;
        public readonly IUsersService _usersService;
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration, IUsersService usersService, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
            _usersService = usersService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<IEnumerable<Users>> GetUsers()
        {
            var users = _usersService.GetUsers();

            if (users == null || !users.Any())
            {
                return NoContent();
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public ActionResult<Users> GetUserById(int id)
        {
            var user = _usersService.GetUserID(id);

            if (user == null)
            {
                return NoContent();
            }
            return Ok(user);
        }


        [HttpPost]
        public IActionResult Register([FromBody] Users user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.password) || user.password != user.repass)
            {
                return BadRequest("Mật khẩu và xác nhận mật khẩu không khớp hoặc thông tin yêu cầu không hợp lệ.");
            }

            try
            {
                var createdUser = _usersService.AddUser(user);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi xảy ra khi tạo tài khoản.");
            }
        }


        [HttpPut("{id}")]
        public ActionResult<Users> UpdateUser(int id, [FromBody] Users user)
        {
            if (id != user.Id)
            {
                return BadRequest("ID không khớp với thông tin người dùng.");
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, ModelState);
            }
            var updatedUser = _usersService.UpdateUser(id, user);
            if (updatedUser == null)
            {
                return NotFound("Người dùng không tìm thấy.");
            }

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _usersService.GetUserID(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            _usersService.DeleteUser(id);
            return Ok(new { message = "Đã xóa thành công User.", user });
        }
        



    }
}
