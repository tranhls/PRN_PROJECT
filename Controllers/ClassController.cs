using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_Assm.Models;
using System.Security.Claims;

namespace PRN222_Assm.Controllers
{
    public class ClassController : Controller
    {

        private PrnassmContext _context;

        public ClassController(PrnassmContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyClass()
        {
            var  userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var account = _context.Accounts.FirstOrDefault(u => u.Email == userEmail);

            var myClass = _context.Classes.Where(c => c.Tearcher == account.Id)
                                            .Include(c => c.SubjectNavigation)
                                            .Include(c => c.Semester)
                                            .Include(c => c.StudentClasses)
                                            .ToList();

            var subject = _context.Subjects.ToList();

            var semester = _context.Semmesters.ToList();

            ViewBag.Semesters = semester;
            ViewBag.Subjects = subject;
            ViewBag.MyClass = myClass;
            return View();
        }

        public IActionResult Detail(int id) {

            var students = _context.StudentClasses.Where(sc => sc.ClassId == id)
                                                    .Include(s => s.Student)
                                                    .ToList();
            var myClass = _context.Classes.Include(c => c.SubjectNavigation)
                                           .Include(c => c.Semester)
                                           .Include(c => c.SubjectNavigation)
                                           .FirstOrDefault(c => c.Id == id);
                       
            

            ViewBag.Students = students;
            ViewBag.MyClass = myClass;
            return View();
        }
    }
}
