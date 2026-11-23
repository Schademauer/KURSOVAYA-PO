using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeKurs.DAL
{
    public class Order
    {
        public int OrderId { get; set; }
        public int? ClientId { get; set; }
        public int? StatusId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Comment { get; set; }
        public TimeOnly CreatedTime { get; set; }
        public DateOnly CreatedDate { get; set; }
        public int BonusPointsUsed { get; set; }
        public int BonusPointsEarned { get; set; }
        public decimal FinalAmount { get; set; }

        // Связи
        public User? Client { get; set; }
        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}