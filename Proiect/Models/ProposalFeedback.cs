using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Proiect.Models
{
    public class ProposalFeedback
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage =  "Este necesar să inserați un mesaj")]
        [StringLength(1000, ErrorMessage = "Lungimea mesajului trebuie să fie de maxim 1000 caractere")]
        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        //--------------------------------------------------------

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set;}

        //--------------------------------------------------------
        
        public int ProposalId { get; set; }

        public virtual ProductProposal Proposal { get; set; }

        //--------------------------------------------------------
    }
}
