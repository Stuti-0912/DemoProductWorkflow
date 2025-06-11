using DemoProductworkflow.Data;
using DemoProductworkflow.Interfaces;
using DemoProductworkflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoProductworkflow.Controllers
{
    [Authorize(Roles = "User")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.CreatedById = user?.Id; // ✅ Check if null
            model.CreatedAt = DateTime.UtcNow;

            // 🔎 TEMP: log it to check
            Console.WriteLine("User Id from controller: " + user?.Id);
            Console.WriteLine("Model.CreatedById before save: " + model.CreatedById);

            if (ModelState.IsValid)
            {
                _context.Products.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
            //return View(model);
        }
    }
}
