using CommonSystem2_API.DataModel;
using CommonSystem2_API.Models;

namespace CommonSystem2_API.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateUser(string username, string password);
        Task<User?> AddUser(UserModel user);
        Task<User?> UpdateUser(User user);
        Task<User?> GetUser(string username);
        Task<User?> ActivateUser(Guid id);
        Task<List<User>?> GetUsers();
    }
}
