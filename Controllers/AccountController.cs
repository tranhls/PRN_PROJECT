using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using PRN222_Assm.Models;
using Microsoft.AspNetCore.Identity;

namespace PRN222_Assm.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private PrnassmContext _context;

        public AccountController(PrnassmContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {

            var user = _context.Accounts.FirstOrDefault(u => u.Email == email && u.Password == password);
            

            if (user != null)
            {
                var role = user.Role == 1 ? "Student"
                          : user.Role == 2 ? "Teacher"
                          : "Admin";
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
              
                if (user.Role == 1)
                {
                    return RedirectToAction("Index", "Student");
                }
                else if (user.Role == 2)
                {
                    return RedirectToAction("MyClass", "Class");
                }
                else
                {
                    return RedirectToAction("Home", "Admin");
                }
                
            }
            else
            {
                ViewData["Error"] = "Invalid email or password.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string OldPassword, string NewPassword,string ConfirmPassword)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Accounts.FirstOrDefault(u => u.Email == email);

            if (user.Password != OldPassword) {
                ViewData["Error"] = "Old password is incorrect";
                return View();
            }

            if (OldPassword == NewPassword)
            {
                ViewData["Error"] = "New password must be different from old password"; ;
                return View();
            }

           if (ConfirmPassword != NewPassword)
           {
                ViewData["Error"] = "Password not match";
                return View();
           }

            user.Password = NewPassword;
            _context.Update(user);
            _context.SaveChanges();
            ViewData["SuccessMessage"] = "Password changed successfully.";
            return View();
        }
    }
}
