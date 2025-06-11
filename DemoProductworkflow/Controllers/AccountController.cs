using DemoProductworkflow.Data;
using DemoProductworkflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace DemoProductworkflow.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }


        private async Task EnsureRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && user.IsActive)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("UserList");

                    if (await _userManager.IsInRoleAsync(user, "User"))
                        return RedirectToAction("Index", "Product");

                    // Optionally handle unknown roles
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }


        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(string email, string firstName, string lastName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                ModelState.AddModelError("", "Please fill all required fields.");
                return View();
            }

            await EnsureRolesAsync();

            var tempPassword = GenerateSecurePassword();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                // Add to email log
                /*  var emailBody = $"Dear {firstName} {lastName},<br/><br/>Your account has been created.<br/>" +
                                  $"Email: {email}<br/>Temporary Password: {tempPassword}<br/><br/>" +
                                  $"Please change your password after your first login.<br/><br/>Regards,<br/>Product Workflow Team";*/

                var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);

                var emailBody = $@"
                        Dear {firstName} {lastName},<br/><br/>
                        Your account has been created successfully.<br/><br/>
                        <b>Email:</b> {email}<br/>
                        <b>Temporary Password:</b> {tempPassword}<br/><br/>
                        Please click the link below to log in and change your password after your first login:<br/>
                        <a href='{loginUrl}'>Login to Product Workflow System</a><br/><br/>
                        Once logged in, you can start creating products.<br/><br/>
                        Regards,<br/>
                        <b>Product Workflow Team</b>";

                var emailLog = new EmailLog
                {
                    ToEmail = email,
                    Subject = "Welcome to Product Workflow System",
                    Body = emailBody,
                    Status = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmailLogs.Add(emailLog);
                await _context.SaveChangesAsync();

                try
                {
                    var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

                    using var smtpClient = new SmtpClient(emailSettings.SmtpServer, emailSettings.SmtpPort)
                    {
                        Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
                        EnableSsl = true
                    };

                    var mailMessage = new MailMessage(emailSettings.SenderEmail, email)
                    {
                        Subject = "Welcome to Product Workflow System",
                        Body = emailBody,
                        IsBodyHtml = true
                    };

                    await smtpClient.SendMailAsync(mailMessage);

                    // Update EmailLog status to Sent
                    emailLog.Status = 1;
                    _context.Update(emailLog);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email sending failed: " + ex.Message);
                    // Leave Status = 0 for retry
                }

                TempData["Success"] = "User created and welcome email sent.";
                return RedirectToAction("UserList");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        private string GenerateSecurePassword()
        {
            return "A1@" + Guid.NewGuid().ToString("N").Substring(0, 5);
        }
    }
}