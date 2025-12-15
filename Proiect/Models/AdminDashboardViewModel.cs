namespace Proiect.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingProposals { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }
}