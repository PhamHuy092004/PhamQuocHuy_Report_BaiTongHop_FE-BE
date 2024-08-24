using API.Model;

namespace API.Services
{
	public interface IUsersService
	{
		IEnumerable<Users> GetUsers();
		public Users AddUser(Users users);
		Users GetUserID(int id);
		Users UpdateUser(int id, Users user);
		Users DeleteUser(int id);
        UserLoginDto GetUserInfo(string username, string password);


    }
}
