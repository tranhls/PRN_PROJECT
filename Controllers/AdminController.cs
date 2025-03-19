using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN222_Assm.Models;

namespace PRN222_Assm.Controllers
{
    public class AdminController : Controller
    {
        private readonly PrnassmContext _contextDAO;
        public AdminController(PrnassmContext context)
        {
            _contextDAO = context;
        }
        public IActionResult Home()
        {
            return View();
        }

        public IActionResult ManageStudent()
        {
            var listStudent = _contextDAO.Accounts
       .Where(st => st.Role == 1)
       .Select(st => new
       {
           st.Id,
           st.Name,
           st.Dob,
           st.Email,
           st.Phone,
           st.Address,
           st.Code,
           Classes = st.StudentClasses.Select(sc => sc.Class).ToList()
       })
       .ToList();
            ViewBag.listStudent = listStudent;
            return View();
        }

        public IActionResult AddStudent()
        {
            var listClass = _contextDAO.Classes.ToList();
            ViewBag.listClass = listClass;
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(string Name, DateOnly Dob, string Phone,string Email, string Password ,string Address, string Code)
        {
            var student = _contextDAO.Accounts.FirstOrDefault(st => st.Code == Code);
            if (student != null)
            {
                ViewBag.CodeExited = "Code is exited please input different code";
                return View();
            }
            var studentEmail = _contextDAO.Accounts.FirstOrDefault(st => st.Email == Email);
            if (studentEmail != null)
            {
                ViewBag.EmailExit = "Email is exited please input different Email";
                return View();
            }

            var studentPhone = _contextDAO.Accounts.FirstOrDefault(st => st.Phone == Phone);
            if (studentPhone != null)
            {
                ViewBag.PhoneExit = "Phone is exited please input different Phone";
                return View();
            }
            Account ac = new Account
            {
                Name = Name,
                Dob = Dob,
                Phone = Phone,
                Email = Email,
                Password = Password,
                Address = Address,
                Code = Code,
                Role = 1
            };
            _contextDAO.Accounts.Add(ac);
            _contextDAO.SaveChanges();
            return RedirectToAction("ManageStudent");
        }

        public IActionResult EditStudent(int Id)
        {
            var student = _contextDAO.Accounts.Include(st => st.StudentClasses).FirstOrDefault(st => st.Id == Id);
            ViewBag.getInfor = student;
            var listClass = _contextDAO.Classes.ToList();
            ViewBag.listClass = listClass;
            return View();
        }

        public IActionResult SearchStudent(string Search)
        {
            var listStudent = _contextDAO.Accounts
                .Where(st => st.Role == 1 && st.Name.Contains(Search))
                .Select(st => new
                {
                    st.Id,
                    st.Name,
                    st.Dob,
                    st.Email,
                    st.Phone,
                    st.Address,
                    st.Code,
                    Classes = st.StudentClasses.Select(sc => sc.Class).ToList()
                })
                .ToList();

            ViewBag.listStudent = listStudent;
            return View("ManageStudent");
        }


        [HttpPost]
        public IActionResult EditStudent(int Id, string Code, string Name, DateOnly Dob, string Phone, string Address)
        {
            var student = _contextDAO.Accounts.FirstOrDefault(st => st.Id == Id);
            if (student == null)
            {
                return NotFound();
            }
            student.Code = Code;
            student.Name = Name;
            student.Dob = Dob;
            student.Phone = Phone;
            student.Address = Address;

            _contextDAO.Accounts.Update(student);
            _contextDAO.SaveChanges();

            return RedirectToAction("ManageStudent");
        }

        public IActionResult RemoveStudent(int accountId)
        {
            try
            {
                var student = _contextDAO.Accounts.FirstOrDefault(st => st.Id == accountId);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student not found.";
                    return RedirectToAction("ManageStudent");
                }

                _contextDAO.Accounts.Remove(student);
                _contextDAO.SaveChanges();

                TempData["SuccessMessage"] = "Student removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Delete fail because student is exited in a class ";
            }

            return RedirectToAction("ManageStudent");
        }

        public IActionResult AddStudentInClass()
        {
            var students = _contextDAO.Accounts.Where(ac => ac.Role == 1);
            var Classes = _contextDAO.Classes.ToList();
            ViewBag.Students = students;
            ViewBag.Classes = Classes;
            return View();
        }

        [HttpPost]
        public IActionResult AddStudentInClass(int classId, int[] studentId)
        {
            foreach (var student in studentId)
            {
                StudentClass sc = new StudentClass
                {
                    StudentId = student,
                    ClassId = classId
                };
                _contextDAO.StudentClasses.Add(sc);
                _contextDAO.SaveChanges();
            }
            return RedirectToAction("ManageClass");
        }


        public IActionResult ManageClass()
        {
            var StudentClasses = _contextDAO.StudentClasses.Include(sc => sc.Student).Include(sc => sc.Class)
                .ToList();
            var classes = _contextDAO.Classes.Include(sc => sc.StudentClasses)
                                            .Include(sc => sc.SubjectNavigation)
                                            .Include(sc => sc.Semester)
                                            .ToList();
            ViewBag.StudentClasses = StudentClasses;
            ViewBag.Classes = classes;
            return View();
        }

        public IActionResult SearchClass(string Search)
        {
            var StudentClasses = _contextDAO.StudentClasses.Include(sc => sc.Student).Include(sc => sc.Class)
                .Where(sc => sc.Class.Code.Contains(Search))
                .ToList();
            ViewBag.StudentClasses = StudentClasses;
            return View("ManageClass");
        }

        public IActionResult AddClass()
        {
            var semester  = _contextDAO.Semmesters.ToList();
            var tearcher = _contextDAO.Accounts.Where(tc => tc.Role == 2).ToList();
            var subjects = _contextDAO.Subjects.ToList();

            ViewBag.Subjects = subjects;
            ViewBag.Semesters = semester;
            ViewBag.Teachers = tearcher;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClass(Class classObj)
        {
            if (ModelState.IsValid)
            {
                _contextDAO.Classes.Add(classObj);
                _contextDAO.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Subjects = _contextDAO.Subjects.ToList();
            ViewBag.Semesters = _contextDAO.Semmesters.ToList();
            ViewBag.Teachers = _contextDAO.Accounts.Where(tc => tc.Role == 2).ToList();
            return RedirectToAction("ManageClass");
        }

        public IActionResult ManageSubjects()
        {
            var subjects = _contextDAO.Subjects
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Code,
                    s.Category,
                    CategoryName = s.CategoryNavigation != null ? s.CategoryNavigation.Name : "N/A",
                })
                .ToList();

            ViewBag.Subjects = subjects;
            return View();
        }

        public IActionResult AddSubject()
        {
            var Categories = _contextDAO.Categories.ToList();
            ViewBag.Categories = Categories;
            return View();
        }

        [HttpPost]
        public IActionResult AddSubject(string Name, string Code, int Category)
        {
            Subject su = new Subject
            {
                Name = Name,
                Code = Code,
                Category = Category
            };
            _contextDAO.Subjects.Add(su);
            _contextDAO.SaveChanges();
            var subjects = _contextDAO.Subjects
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Code,
                    s.Category,
                    CategoryName = s.CategoryNavigation != null ? s.CategoryNavigation.Name : "N/A",
                })
                .ToList();

            ViewBag.Subjects = subjects;
            return View("ManageSubjects");
        }



        public IActionResult SearchSubjects(string Search)
        {
            var subjects = _contextDAO.Subjects.Where(su => su.Name.Contains(Search))
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Code,
                    s.Category,
                    CategoryName = s.CategoryNavigation != null ? s.CategoryNavigation.Name : "N/A",
                })
                .ToList();

            ViewBag.Subjects = subjects;
            return View("ManageSubjects");
        }

        public IActionResult EditSubject(int Id)
        {
            var subject = _contextDAO.Subjects.FirstOrDefault(su => su.Id == Id);
            var Categories = _contextDAO.Categories.ToList();
            ViewBag.Categories = Categories;
            ViewBag.subject = subject;
            return View();
        }

        [HttpPost]
        public IActionResult EditSubject(int Id, string Name, string Code, int Category)
        {
            var sub = _contextDAO.Subjects.FirstOrDefault(su => su.Id == Id);
            sub.Name = Name;
            sub.Code = Code;
            sub.Category = Category;
            _contextDAO.SaveChanges();
            return RedirectToAction("ManageSubjects");
        }

        public IActionResult RemoveSubject(int subId)
        {
            try
            {
                var sub = _contextDAO.Subjects.FirstOrDefault(su => su.Id == subId);
                _contextDAO.Subjects.Remove(sub);
                _contextDAO.SaveChanges();
                TempData["SuccessMessage"] = "Student removed successfully.";
                return RedirectToAction("ManageSubjects");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Delete fail because subjects is exited in a class ";
            }

            return RedirectToAction("ManageSubjects");
        }

        public IActionResult ManageTeacher()
        {
            var listTeachers = _contextDAO.Accounts.Where(tc => tc.Role == 2).ToList();
            ViewBag.listTeachers = listTeachers;
            return View();
        }

        public IActionResult EditTeacher(int Id)
        {
            var teacher = _contextDAO.Accounts.FirstOrDefault(tc => tc.Id == Id);
            ViewBag.teacher = teacher;
            return View();
        }

        [HttpPost]
        public IActionResult EditTeacher(int Id, string Code, string Name, DateOnly Dob, string Email, string Phone, string Address)
        {
            var teacher = _contextDAO.Accounts.FirstOrDefault(tc => tc.Id == Id);
            teacher.Code = Code;
            teacher.Name = Name;
            teacher.Dob = Dob;
            teacher.Email = Email;
            teacher.Address = Address;
            teacher.Phone = Phone;
            _contextDAO.SaveChanges();
            return RedirectToAction("ManageTeacher");
        }

        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddTeacher(string Name, string Code, string Email, string Phone, DateOnly Dob, string Address)
        {
            var teacher = new Account
            {
                Name = Name,
                Email = Email,
                Code = Code,
                Phone = Phone,
                Dob = Dob,
                Address = Address,
                Role = 2
            };
            _contextDAO.Accounts.Add(teacher);
            _contextDAO.SaveChanges();
            var listTeachers = _contextDAO.Accounts.Where(tc => tc.Role == 2).ToList();
            ViewBag.listTeachers = listTeachers;
            return View("ManageTeacher");
        }

        public IActionResult RemoveTeacher(int accountId)
        {
            var teacher = _contextDAO.Accounts.FirstOrDefault(tc => tc.Id == accountId);
            if (teacher == null)
            {
                return NotFound();
            }
            _contextDAO.Accounts.Remove(teacher);
            _contextDAO.SaveChanges();
            var listTeachers = _contextDAO.Accounts.Where(tc => tc.Role == 2).ToList();
            ViewBag.listTeachers = listTeachers;
            return View("ManageTeacher");

        }

    }
}
