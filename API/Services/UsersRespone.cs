using API.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace API.Services
{
	public class UsersRespone: IUsersService
	{
		private readonly IConfiguration _configuration;

		public UsersRespone(IConfiguration configuration)
		{
			_configuration = configuration;
		}
        public UserLoginDto GetUserInfo(string username, string password)
        {
            using (IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var query = "SELECT * FROM Users WHERE username = @Username AND password = @Password";
                var users = connection.Query<UserLoginDto>(query, new { Username = username, Password = password }).ToList();

                if (users.Count == 0)
                {
                    throw new InvalidOperationException("Không tìm thấy người dùng.");
                }
                if (users.Count > 1)
                {
                    throw new InvalidOperationException("Tìm thấy nhiều người dùng có cùng thông tin đăng nhập.");
                }

                return users.Single(); // Chỉ trả về một người dùng duy nhất
            }
        }

        public IEnumerable<Users> GetUsers()
		{
			// Kết nối database
			using (IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				// Gọi stored procedure GETALLUSERS để lấy tất cả người dùng
				var items = connection.Query<Users>("GETALLUSERS", commandType: CommandType.StoredProcedure);

				return items;
			}
		}

        public Users AddUser(Users user)
        {
            using (IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                // Sử dụng parameter để truyền tham số cho DynamicParameters 
                parameters.Add("@hoVaTen", user.hoVaTen);
                parameters.Add("@soDienThoai", user.soDienThoai);
                // Chuyển đổi ngày tháng thành định dạng yyyy-MM-dd
                parameters.Add("@ngaySinh", user.ngaySinh.ToString("yyyy-MM-dd"));
                parameters.Add("@gioiTinh", user.gioiTinh);
                parameters.Add("@tinhThanh", user.tinhThanh);
                parameters.Add("@username", user.username);
                parameters.Add("@password", user.password);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                // Gọi stored procedure AddUser để thêm người dùng
                connection.Execute("AddUser", parameters, commandType: CommandType.StoredProcedure);

                // Lấy giá trị của UserId được trả về từ stored procedure
                var userId = parameters.Get<int>("@UserId");
                user.Id = userId;
                return user;
            }
        }


        public Users UpdateUser(int id, Users user)
		{
			using (IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();

				var parameters = new DynamicParameters();
				parameters.Add("@Id", id);
				parameters.Add("@hoVaTen", user.hoVaTen);
				parameters.Add("@soDienThoai", user.soDienThoai);
				parameters.Add("@ngaySinh", user.ngaySinh);
				parameters.Add("@gioiTinh", user.gioiTinh);
				parameters.Add("@tinhThanh", user.tinhThanh);
				parameters.Add("@Username", user.username);
				parameters.Add("@Password", user.password);
				parameters.Add("@RowsAffected", dbType: DbType.Int32, direction: ParameterDirection.Output);
				// Gọi stored procedure UpdateUser
				connection.Execute("UpdateUser", parameters, commandType: CommandType.StoredProcedure);

				var rowsAffected = parameters.Get<int>("@RowsAffected");

				if (rowsAffected == 0)
				{
					return null;
				}

				user.Id = id;
				return user;
			}
		}

		public Users DeleteUser(int id)
		{
			using (IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();

				var parameters = new DynamicParameters();
				parameters.Add("@UserId", id);
				var user = connection.QuerySingleOrDefault<Users>(
					"DeleteUser",
					parameters,
					commandType: CommandType.StoredProcedure
				);

				return user;
			}
		}

		public Users GetUserID(int id)
		{
			using (IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
			{
				connection.Open();
				// Sử dụng stored procedure để lấy người dùng theo Id
				var user = connection.QuerySingleOrDefault<Users>("GetUserWithId", new { UserId = id }, commandType: CommandType.StoredProcedure);

				return user;
			}
		}
	
}
}
