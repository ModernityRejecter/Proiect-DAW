using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Proiect.Models
{
    public class ProposalFeedback
    {
        public int Id { get; set; }

        [Required(ErrorMessage =  "Message is required")]
        [StringLength(1000, ErrorMessage = "Message must be at most 1000 characters long")]
        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        //--------------------------------------------------------
        
        public int UserId { get; set; }

        public virtual ApplicationUser User { get; set;}

        //--------------------------------------------------------
        
        public int ProposalId { get; set; }

        public virtual ProductProposal Proposal { get; set; }

        //--------------------------------------------------------
    }
}
