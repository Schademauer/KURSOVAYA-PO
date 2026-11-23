using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeKurs.DAL
{
    public class User
    {
        public int UserId { get; set; }
        public int UserTypeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Login { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public int BonusPoints { get; set; }
        public bool BonusPointsActive { get; set; }
        public DateOnly RegistrationDate { get; set; }
    }
}