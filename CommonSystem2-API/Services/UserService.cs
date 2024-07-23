using CommonSystem2_API.DatabaseContext;
using CommonSystem2_API.DataModel;
using CommonSystem2_API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommonSystem2_API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateUser(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password && u.IsActive == true);
            return user;
        }

        public async Task<User?> AddUser(UserModel usr)
        {
            var userData = await _context.Users.FirstOrDefaultAsync(u => u.Username == usr.Username);
            if (userData != null)
                return userData;
            var user = new User
            {
                Username = usr.Username,
                Password = usr.Password,
                Name = usr.Name,
                Organization = usr.Organization,
                Role = usr.Role,    
                IsActive = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUser(User user)
        {
            var userData = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (userData != null)
            {
                userData.Name = user.Name;
                userData.IsActive = user.IsActive;
                userData.Role = user.Role;
                userData.Organization = user.Organization;
                _context.Entry(userData).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return userData;
        }


        public async Task<User?> GetUser(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user;
        }

        public async Task<User?> ActivateUser(Guid id)
        {
            var userData = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if(userData != null)
            {
                userData.IsActive = true;
                _context.Entry(userData).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }            
            return userData;
        }

        public async Task<List<User>?> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }
    }
}
