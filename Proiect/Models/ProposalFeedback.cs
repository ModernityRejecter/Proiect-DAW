using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Proiect.Models
{
    public class ProposalFeedback
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage =  "Message is required")]
        [StringLength(400, ErrorMessage = "Message must be at most 400 characters long")]
        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        //--------------------------------------------------------
        
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set;}

        //--------------------------------------------------------
        
        public int ProposalId { get; set; }

        public virtual ProductProposal Proposal { get; set; }

        //--------------------------------------------------------
    }
}
