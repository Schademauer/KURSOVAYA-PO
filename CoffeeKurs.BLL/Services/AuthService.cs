using CoffeeKurs.DAL;
using CoffeeKurs.BLL.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.BLL.Services
{
    public class AuthService
    {

        public UserDto? Login(string login, string password)
        {
            using var context = new AppDbContext();

            var user = context.Users
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == password && u.IsActive);

            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName ?? string.Empty,
                Login = user.Login,
                BonusPoints = user.BonusPoints,
                IsAdmin = user.UserTypeId == 1 
            };
        }
        public UserDto? GetUserById(int userId)
        {
            using var context = new AppDbContext();
            var user = context.Users.Find(userId);
            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName ?? string.Empty,
                Login = user.Login,
                BonusPoints = user.BonusPoints,
                IsAdmin = user.UserTypeId == 1
            };
        }
    }
}