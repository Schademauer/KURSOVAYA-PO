using CoffeeKurs.DAL;
using CoffeeKurs.BLL.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.BLL.Services
{
    public class AuthService
    {
        // Метод возвращает UserDto, если вход успешен, и null, если нет
        public UserDto? Login(string login, string password)
        {
            using var context = new AppDbContext();

            // Ищем пользователя по логину и паролю
            // (В реальном проекте пароль нужно хэшировать, но для курсовой пока так)
            var user = context.Users
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == password && u.IsActive);

            if (user == null) return null;

            // Преобразуем сущность БД (User) в DTO для интерфейса
            return new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName ?? string.Empty,
                Login = user.Login,
                BonusPoints = user.BonusPoints,
                IsAdmin = user.UserTypeId == 1 // 1 - это Administrator (из таблицы Users_type)
            };
        }
    }
}