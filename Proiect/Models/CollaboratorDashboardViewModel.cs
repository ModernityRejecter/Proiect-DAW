namespace Proiect.Models
{
    public class CollaboratorDashboardViewModel
    {
        // Carduri statistici (echivalentul TotalUsers, TotalProducts de la admin)
        public int TotalProposals { get; set; }
        public int ApprovedProducts { get; set; }
        public int PendingProposals { get; set; }
        public int RejectedProposals { get; set; }

        public List<ProductProposal> RecentProposals { get; set; } = new List<ProductProposal>();
    }
}
