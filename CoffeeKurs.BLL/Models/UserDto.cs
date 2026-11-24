using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeKurs.BLL.Models
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Login { get; set; }
        public int BonusPoints { get; set; }
        public bool IsAdmin { get; set; }
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }
}