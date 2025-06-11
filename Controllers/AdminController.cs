using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdWorkFlow.Models;
using ProdWorkFlow.Services;
using System.Security.Claims;

namespace ProdWorkFlow.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                // Generate a random temporary password
                var temporaryPassword = Guid.NewGuid().ToString("N").Substring(0, 8);
                
                // Set default values
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = true;
                user.EmailConfirmed = true;

                var (success, message) = await _userService.CreateUserAsync(user, temporaryPassword);

                if (success)
                {
                    TempData["SuccessMessage"] = "User created successfully. A welcome email has been sent.";
                    return RedirectToAction(nameof(Users));
                }

                ModelState.AddModelError("", message);
            }

            return View(user);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                var success = await _userService.UpdateUserAsync(user);
                if (success)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(Users));
                }

                ModelState.AddModelError("", "Error updating user.");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToAction(nameof(Users));
        }
    }
} 