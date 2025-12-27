using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    public class ProposalFeedbacksController(ApplicationDbContext context, UserManager<ApplicationUser>
        userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        [Authorize(Roles = "Admin,Colaborator")]

        public IActionResult Index()
        {
            ICollection<ProposalFeedback> texts = new List<ProposalFeedback>();
            texts = db.ProposalFeedbacks.ToList();
            return View(texts);
        }


        //------------------------------------------------------------------------
        // metode interne
        //private ICollection<ProposalFeedback> GetAllTexts()
        //{


        //}

    }
}
