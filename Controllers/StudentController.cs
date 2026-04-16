using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Json(new List<object>());
            }

            uid = uid.Trim();

            var result = db.EnrollmentGrades
                .Where(e => e.Uid == uid)
                .Select(e => new
                {
                    subject = e.Class.Course.Dept.Subject,
                    number = e.Class.Course.Number,
                    name = e.Class.Course.CourseName,
                    season = e.Class.SemesterSeason,
                    year = e.Class.SemesterYear,
                    grade = e.Grade ?? "--"
                })
                .ToList();

            return Json(result);
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            if (!IsValidClassKey(subject, num, season, year) || string.IsNullOrWhiteSpace(uid))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();
            season = season.Trim();
            uid = uid.Trim();

            var result = db.Assignments
                .Where(a =>
                    a.Cat.Class.Course.Dept.Subject == subject &&
                    a.Cat.Class.Course.Number == num &&
                    a.Cat.Class.SemesterSeason == season &&
                    a.Cat.Class.SemesterYear == year &&
                    a.Cat.Class.EnrollmentGrades.Any(e => e.Uid == uid))
                .Select(a => new
                {
                    aname = a.AssignmentName,
                    cname = a.Cat.CatName,
                    due = a.DueDate,
                    score = a.Submissions
                        .Where(s => s.Uid == uid)
                        .Select(s => s.Score)
                        .FirstOrDefault()
                })
                .ToList();

            return Json(result);
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            if (!IsValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(asgname) ||
                string.IsNullOrWhiteSpace(uid) ||
                string.IsNullOrWhiteSpace(contents))
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();
            asgname = asgname.Trim();
            uid = uid.Trim();
            contents = contents.Trim();

            var assignment = db.Assignments.FirstOrDefault(a =>
                a.AssignmentName == asgname &&
                a.Cat.CatName == category &&
                a.Cat.Class.Course.Dept.Subject == subject &&
                a.Cat.Class.Course.Number == num &&
                a.Cat.Class.SemesterSeason == season &&
                a.Cat.Class.SemesterYear == year);

            if (assignment == null)
            {
                return Json(new { success = false });
            }

            var enrolled = db.EnrollmentGrades.Any(e =>
                e.ClassId == assignment.Cat.ClassId &&
                e.Uid == uid);

            if (!enrolled)
            {
                return Json(new { success = false });
            }

            var submission = db.Submissions.FirstOrDefault(s =>
                s.AssignmentId == assignment.AssignmentId &&
                s.Uid == uid);

            if (submission == null)
            {
                submission = new Submission
                {
                    AssignmentId = assignment.AssignmentId,
                    Uid = uid,
                    Contents = contents,
                    SubmissionTime = DateTime.Now,
                    Score = 0
                };

                db.Submissions.Add(submission);
            }
            else
            {
                submission.Contents = contents;
                submission.SubmissionTime = DateTime.Now;
            }

            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            if (!IsValidClassKey(subject, num, season, year) || string.IsNullOrWhiteSpace(uid))
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            season = season.Trim();
            uid = uid.Trim();

            var courseClass = db.Classes.FirstOrDefault(c =>
                c.Course.Dept.Subject == subject &&
                c.Course.Number == num &&
                c.SemesterSeason == season &&
                c.SemesterYear == year);

            if (courseClass == null)
            {
                return Json(new { success = false });
            }

            var alreadyEnrolled = db.EnrollmentGrades.Any(e =>
                e.ClassId == courseClass.ClassId &&
                e.Uid == uid);

            if (alreadyEnrolled)
            {
                return Json(new { success = false });
            }

            db.EnrollmentGrades.Add(new EnrollmentGrade
            {
                ClassId = courseClass.ClassId,
                Uid = uid,
                Grade = null
            });

            db.SaveChanges();

            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Json(new { gpa = 0.0 });
            }

            uid = uid.Trim();

            var grades = db.EnrollmentGrades
                .Where(e => e.Uid == uid && e.Grade != null && e.Grade != "--")
                .Select(e => e.Grade!)
                .ToList();

            var gradePoints = grades
                .Select(TryGradeToPoints)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToList();

            if (gradePoints.Count == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            var gpa = gradePoints.Average();

            return Json(new { gpa });
        }
                
        /*******End code to modify********/

        private static bool IsValidClassKey(string subject, int num, string season, int year)
        {
            if (string.IsNullOrWhiteSpace(subject) || num <= 0 || string.IsNullOrWhiteSpace(season) || year <= 0)
            {
                return false;
            }

            var trimmedSeason = season.Trim();
            return trimmedSeason == "Spring" || trimmedSeason == "Summer" || trimmedSeason == "Fall";
        }

        private static double? TryGradeToPoints(string grade)
        {
            return grade.Trim() switch
            {
                "A" => 4.0,
                "A-" => 3.7,
                "B+" => 3.3,
                "B" => 3.0,
                "B-" => 2.7,
                "C+" => 2.3,
                "C" => 2.0,
                "C-" => 1.7,
                "D+" => 1.3,
                "D" => 1.0,
                "D-" => 0.7,
                "E" => 0.0,
                _ => null
            };
        }

    }
}
