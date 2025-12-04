using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MarkShop.Data.ShopSbS.Data; // Check your namespace matches AppDbContext
using MarkShop.Models;

namespace MarkShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Find the user in the database
            // Note: In a real app, you should HASH the password, not store it plain!
            var user = _context.Customers.FirstOrDefault(c => c.Email == email && c.Password == password);

            if (user != null)
            {
                // 2. Create "Claims" (ID card details) for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("CustomerId", user.Id.ToString()) // IMPORTANT: Store the ID to use later
                };

                // 3. Create the Identity and Sign In
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}