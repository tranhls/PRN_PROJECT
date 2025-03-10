using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PRN222_Assm.Models;
using System;
using System.IO;
using System.Security.Claims;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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

        public IActionResult MyClass(string search, int subjectSelect, int semesterSelect)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var account = _context.Accounts.FirstOrDefault(u => u.Email == userEmail);

            var myClass = _context.Classes.Where(c => c.Tearcher == account.Id)
                                            .Include(c => c.SubjectNavigation)
                                            .Include(c => c.Semester)
                                            .Include(c => c.StudentClasses)
                                            .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                myClass = myClass.Where(c => c.Name.ToLower().Contains(search.ToLower()) || c.Code.ToLower().Contains(search.ToLower())).ToList();
            }

            if (subjectSelect != 0)
            {
                myClass = myClass.Where(c => c.Subject == subjectSelect).ToList();
            }
            if (semesterSelect != 0)
            {
                myClass = myClass.Where(c => c.SemesterId == semesterSelect).ToList();
            }

            var subject = _context.Subjects.ToList();

            var semester = _context.Semmesters.ToList();

            ViewBag.Semesters = semester;
            ViewBag.Subjects = subject;
            ViewBag.MyClass = myClass;
            ViewBag.SubjectSelect = subjectSelect;
            ViewBag.SemesterSelect = semesterSelect;
            return View();
        }

        public IActionResult Detail(int id)
        {

            var students = _context.StudentClasses.Where(sc => sc.ClassId == id)
                                                    .Include(s => s.Student)
                                                    .ToList();
            var myClass = _context.Classes.Include(c => c.SubjectNavigation)
                                           .Include(c => c.Semester)
                                           .Include(c => c.SubjectNavigation)
                                           .FirstOrDefault(c => c.Id == id);
            var allStudent = _context.Accounts
                .Where(s => s.Role == 1 && !_context.StudentClasses.Any(cs => cs.StudentId == s.Id && cs.ClassId == id))
                .OrderByDescending(s => s.Name)
                .ToList();


            ViewBag.AllStudent = allStudent;
            ViewBag.Students = students;
            ViewBag.MyClass = myClass;
            return View();
        }

        public IActionResult GradeScore(int id)
        {

            var studentClass = _context.StudentClasses.Where(sc => sc.ClassId == id)
                                                       .Include(sc => sc.Student)
                                                       .Include(s => s.Score)
                                                       .ToList();
            var students = _context.StudentClasses.Where(sc => sc.ClassId == id)
                                                    .Include(s => s.Student)
                                                    .ToList();

            var myClass = _context.Classes.Include(c => c.SubjectNavigation)
                                           .Include(c => c.Semester)
                                           .Include(c => c.SubjectNavigation)
                                           .FirstOrDefault(c => c.Id == id);



            ViewBag.MyClass = myClass;
            ViewBag.Students = students;
            ViewBag.studentClass = studentClass;
            return View();
        }

        [HttpPost]
        public IActionResult GradeScore(List<StudentScoreViewModel> students, int classId)
        {
            foreach (var sc in students)
            {
                var studentClass = _context.StudentClasses.Include(s => s.Score).FirstOrDefault(s => s.StudentId == sc.Id && s.ClassId == classId);
                if (studentClass == null)
                {

                    Score score = new Score
                    {
                        Assignment = sc.Assignment,
                        Final = sc.Final,
                        Quiz = sc.Quiz,
                        Pt1 = sc.Pt1,
                        Pt2 = sc.Pt2
                    };
                    studentClass.Score = score;
                }
                else
                {
                    studentClass.Score.Assignment = sc.Assignment;
                    studentClass.Score.Final = sc.Final;
                    studentClass.Score.Quiz = sc.Quiz;
                    studentClass.Score.Pt1 = sc.Pt1;
                    studentClass.Score.Pt2 = sc.Pt2;
                }

                if (studentClass.Score.Final != null)
                {
                    double total = ((double)studentClass.Score.Final * 4 + (double)studentClass.Score.Assignment * 2 + (double)studentClass.Score.Quiz * 2 + (double)studentClass.Score.Pt1 + (double)studentClass.Score.Pt2)/ 10;
                    studentClass.Total = total;
                }

                var res = _context.StudentClasses.Update(studentClass);

                _context.SaveChanges();
            }

            return RedirectToAction("GradeScore", "Class", new { id = classId });
        }



        [HttpGet]
        public IActionResult ExportToPdf(int classId)
        {
            var myClass = _context.Classes
                .Include(c => c.SubjectNavigation)
                .Include(c => c.Semester)
                .FirstOrDefault(c => c.Id == classId);

            if (myClass == null)
            {
                return NotFound("Class not found.");
            }

            var studentClasses = _context.StudentClasses
                .Where(sc => sc.ClassId == classId)
                .Include(sc => sc.Student)
                .Include(sc => sc.Score)
                .ToList();

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph($"Scores for {myClass.Name} ({myClass.SubjectNavigation.Name}) - Semester {myClass.Semester.Name}")
                    .SetFontSize(18)
                    .SetBold());

                var table = new Table(8);
                table.AddHeaderCell("No");
                table.AddHeaderCell("Student Name");
                table.AddHeaderCell("PT1");
                table.AddHeaderCell("PT2");
                table.AddHeaderCell("Quiz");
                table.AddHeaderCell("Assignment");
                table.AddHeaderCell("Final");
                table.AddHeaderCell("Total");

                int index = 1;
                foreach (var studentClass in studentClasses)
                {
                    var studentName = studentClass.Student?.Name ?? "Unknown";
                    var score = studentClass.Score;
                    var total = studentClass.Total?.ToString("F2") ?? "N/A";

                    table.AddCell(index.ToString());
                    table.AddCell(studentName);
                    table.AddCell(score?.Pt1?.ToString() ?? "N/A");
                    table.AddCell(score?.Pt2?.ToString() ?? "N/A");
                    table.AddCell(score?.Quiz?.ToString() ?? "N/A");
                    table.AddCell(score?.Assignment?.ToString() ?? "N/A");
                    table.AddCell(score?.Final?.ToString() ?? "N/A");
                    table.AddCell(total);

                    index++;
                }

                document.Add(table);
                document.Close();

                var pdfBytes = memoryStream.ToArray();
                return File(pdfBytes, "application/pdf", $"Class_{myClass.Name.Trim()}_Scores_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        public IActionResult ExportToExcel(int classId)
        {
            var myClass = _context.Classes.FirstOrDefault(c => c.Id == classId);
            if (myClass == null)
            {
                return NotFound("Không tìm thấy lớp học.");
            }

            var studentClasses = _context.StudentClasses
                .Where(sc => sc.ClassId == classId)
                .Include(sc => sc.Student)
                .Include(sc => sc.Score)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Grade");
                worksheet.Cells[1, 1].LoadFromArrays(new[] { new[] { "STT", "Name", "PT1", "PT2", "Quiz", "Assignment", "Final", "Total" } });

                int row = 2;
                int index = 1;
                foreach (var sc in studentClasses)
                {
                    worksheet.Cells[row, 1].Value = index++;
                    worksheet.Cells[row, 2].Value = sc.Student?.Name ?? "N/A";
                    worksheet.Cells[row, 3].Value = sc.Score?.Pt1;
                    worksheet.Cells[row, 4].Value = sc.Score?.Pt2;
                    worksheet.Cells[row, 5].Value = sc.Score?.Quiz;
                    worksheet.Cells[row, 6].Value = sc.Score?.Assignment;
                    worksheet.Cells[row, 7].Value = sc.Score?.Final;
                    worksheet.Cells[row, 8].Value = sc.Total;
                    row++;
                }

                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Class_{myClass.Name.Trim()}_Grade.xlsx");
            }
        }
    }
    public class StudentScoreViewModel
    {
        public int Id { get; set; }
        public double? Pt1 { get; set; }
        public double? Pt2 { get; set; }
        public double? Quiz { get; set; }
        public double? Assignment { get; set; }
        public double? Final { get; set; }
    }
}
