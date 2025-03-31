using iText.StyledXmlParser.Node;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_Assm.Models;
using System.Security.Claims;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace PRN222_Assm.Controllers
{
    public class StudentController : Controller
    {
        private PrnassmContext _context;

        public StudentController(PrnassmContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search)
        {
            var email  = User.FindFirst(ClaimTypes.Email)?.Value;

            var account = _context.Accounts.FirstOrDefault(u => u.Email == email);

            var myClass = _context.StudentClasses.Where(sc => sc.StudentId == account.Id)
                                                .Include(sc => sc.Class)
                                                .Include(sc => sc.Class.SubjectNavigation)
                                                .Include(sc => sc.Class.Semester)
                                                .Include(sc => sc.Class.TearcherNavigation)
                                                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                myClass = myClass.Where(c => c.Class.Name.ToLower().Contains(search.ToLower()) || c.Class.Code.ToLower().Contains(search.ToLower())).ToList();
            }

            ViewBag.MyClass = myClass;
            return View();
        }


        public IActionResult MyGrade(int classId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var account = _context.Accounts.FirstOrDefault(u => u.Email == email);

            var studentClasses = _context.StudentClasses.Include(sc => sc.Score).FirstOrDefault(sc => sc.StudentId == account.Id && sc.ClassId == classId);
            var myClass = _context.Classes.Include(c => c.SubjectNavigation)
                                           .Include(c => c.Semester)
                                            .Include(c => c.SubjectNavigation)
                                           .FirstOrDefault(c => c.Id == classId);

           ViewBag.Account = account;
            ViewBag.StudentClasses = studentClasses;
            ViewBag.MyClass = myClass;
            return View();
        }

        public IActionResult MyAttendance(int classId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var account = _context.Accounts.FirstOrDefault(u => u.Email == email);

            List<Attendance> attendances = _context.Attendances
                .Where(sc => sc.Class.ClassId == classId && sc.Class.StudentId == account.Id).ToList();

            var days = Enumerable.Range(1, 15).Select(day => attendances.FirstOrDefault(a => a.Day == day) ?? new Attendance { Day = day, isPresent = false }).ToList();

            var myClass = _context.Classes.Include(c => c.SubjectNavigation)
                                           .Include(c => c.Semester)
                                            .Include(c => c.SubjectNavigation)
                                           .FirstOrDefault(c => c.Id == classId);
            
            ViewBag.ClassName = myClass;

            ViewBag.Account = account;
            ViewBag.MyAttendance = days;
            return View();
        }

        public IActionResult Detail(int classId)
        {

            return View();
        }
    }
}
