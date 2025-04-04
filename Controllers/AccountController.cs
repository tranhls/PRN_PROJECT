﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using PRN222_Assm.Models;
using Microsoft.AspNetCore.Identity;
using PRN222_Assm.Helper;

namespace PRN222_Assm.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private PrnassmContext _context;
        private readonly SendMail _sendMail;

        public AccountController(PrnassmContext context, IConfiguration configuration,SendMail sendMail)
        {
            _context = context;
            _configuration = configuration;
            _sendMail = sendMail;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {

            //var user = _context.Accounts.FirstOrDefault(u => u.Email == email && u.Password == password);

            bool isPass = false;
            var user = _context.Accounts.FirstOrDefault(u => u.Email == email);

            try
            {
                isPass = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch(Exception e)
            {
                isPass = false;
            }

            if (user == null || isPass == false)
            {
                ViewData["Error"] = "Invalid email or password.";
                return View();
            }



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

            bool isPass = false;
            bool isMatchPre = false;
            try
            {
                isPass = BCrypt.Net.BCrypt.Verify(OldPassword, user.Password);
            }
            catch(Exception e)
            {
                isPass = false;
            }

            if (isPass == false) {
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

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            user.Password = hashedPassword;
            _context.Update(user);
            _context.SaveChanges();
            ViewData["SuccessMessage"] = "Password changed successfully.";
            return View();
        }


        public IActionResult ForgotPassword(string email = "")
        {

            if (string.IsNullOrEmpty(email))
            {
                return View();
            }
            
            var user = _context.Accounts.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewData["Error"] = "Email not found";
                return View();
            }
            string encodedEmail = email;
            string confirmLink = Url.Action("NewPassword", "Account", new { email = email }, Request.Scheme);
            string subject = "Forgot password!!";
            string body = $"Please click the following link to get your password: <a href='{confirmLink}'>Confirm Password</a>";
            _sendMail.SendVerificationEmail(email, subject, body);
            ViewData["Success"] = "Send Mail Successfully, Please check your email";
            return View();
        }

        public IActionResult NewPassword( string email)
        {
            var user = _context.Accounts.FirstOrDefault(u => u.Email == email);
            ViewBag.user = user;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(string id,string NewPassword, string ConfirmPassword)
        {

            var user = _context.Accounts.FirstOrDefault(u => u.Id == int.Parse(id));
            ViewBag.user = user;
            if (ConfirmPassword != NewPassword)
            {
                ViewData["Error"] = "Password not match";
                return View();
            }

            user.Password = NewPassword;
            _context.Update(user);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            ViewData["SuccessMessage"] = "Password changed successfully.";
            return View();
        }
    }
}
