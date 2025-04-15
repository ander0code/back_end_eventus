using back_end.Core.Data;
using back_end.Core.Entities;
using System.Linq;

namespace back_end.Modules.Auth.Repositories
{
    public interface IUserRepository
    {
        User GetUserByUsername(string username);
    }

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public User GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }
    }
}