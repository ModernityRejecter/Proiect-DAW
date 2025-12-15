using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proiect.Data;
using Proiect.Models;

namespace Proiect.Controllers
{
    public class ProductsController(ApplicationDbContext context, UserManager<ApplicationUser>
        userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly ApplicationDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        public IActionResult Index()
        {
            return View();
        }

        // in lucru
        //public IActionResult New()
        //{
        //    Product product = new Product();
        //    product.Categ = GetAllCategories();
        //    return View(product);
        //}

        //[HttpPost]
        //public IActionResult New(Product product)
        //{
        //    product.UserId = _userManager.GetUserId();
        //    return View();
        //}

    }
}
