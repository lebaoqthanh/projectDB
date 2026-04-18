using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            if (!IsValidClassKey(subject, num, season, year))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();
            season = season.Trim();

            var students = db.EnrollmentGrades
                .Where(e =>
                    e.Class.Course.Dept.Subject == subject &&
                    e.Class.Course.Number == num &&
                    e.Class.SemesterSeason == season &&
                    e.Class.SemesterYear == year)
                .Select(e => new
                {
                    fname = e.UidNavigation.FirstName,
                    lname = e.UidNavigation.LastName,
                    uid = e.UidNavigation.Uid,
                    dob = e.UidNavigation.Dob,
                    grade = e.Grade ?? "--"
                })
                .ToList();

            return Json(students);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (!IsValidClassKey(subject, num, season, year))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category?.Trim();

            var categories = db.AssignmentCategories
                .Where(c => c.Class.Course.Dept.Subject == subject
                            && c.Class.Course.Number == num
                            && c.Class.SemesterSeason == season
                            && c.Class.SemesterYear == year);

            if (!string.IsNullOrEmpty(category))
            {
                categories = categories.Where(c => c.CatName == category);
            }

            var result = categories
                .SelectMany(c => c.Assignments.Select(a => new
                {
                    aname = a.AssignmentName,
                    cname = a.Cat.CatName,
                    due = a.DueDate,
                    submissions = a.Submissions.Count()
                }))
                .ToList();

            return Json(result);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            if (!IsValidClassKey(subject, num, season, year))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();
            season = season.Trim();

            var categories = db.AssignmentCategories.Where(c =>
                c.Class.Course.Dept.Subject == subject &&
                c.Class.Course.Number == num &&
                c.Class.SemesterSeason == season &&
                c.Class.SemesterYear == year).Select(c => new
            {
                name = c.CatName,
                weight = c.Weight
            }).ToList();
            return Json(categories);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            if (!IsValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                catweight <= 0)
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();

            var courseClass = db.Classes.FirstOrDefault(c =>
                c.Course.Dept.Subject == subject &&
                c.Course.Number == num &&
                c.SemesterSeason == season &&
                c.SemesterYear == year);

            if (courseClass == null)
            {
                return Json(new { success = false });
            }

            var exists = db.AssignmentCategories.Any(c =>
                c.ClassId == courseClass.ClassId &&
                c.CatName == category);

            if (exists)
            {
                return Json(new { success = false });
            }

            var assignmentCategory = new AssignmentCategory
            {
                ClassId = courseClass.ClassId,
                CatName = category,
                Weight = (uint)catweight
            };

            db.AssignmentCategories.Add(assignmentCategory);
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            if (!IsValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(asgname) ||
                string.IsNullOrWhiteSpace(asgcontents) ||
                asgpoints <= 0)
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();
            asgname = asgname.Trim();
            asgcontents = asgcontents.Trim();

            var assignmentCategory = db.AssignmentCategories.FirstOrDefault(c =>
                c.Class.Course.Dept.Subject == subject &&
                c.Class.Course.Number == num &&
                c.Class.SemesterSeason == season &&
                c.Class.SemesterYear == year &&
                c.CatName == category);

            if (assignmentCategory == null)
            {
                return Json(new { success = false });
            }

            var exists = db.Assignments.Any(a =>
                a.CatId == assignmentCategory.CatId &&
                a.AssignmentName == asgname);

            if (exists)
            {
                return Json(new { success = false });
            }

            var assignment = new Assignment
            {
                CatId = assignmentCategory.CatId,
                AssignmentName = asgname,
                MaxPoints = (uint)asgpoints,
                DueDate = asgdue,
                Contents = asgcontents
            };

            db.Assignments.Add(assignment);
            db.SaveChanges();

            UpdateGradesForClass(assignmentCategory.ClassId);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            if (!IsValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(asgname))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();
            asgname = asgname.Trim();

            var assignment = db.Assignments.FirstOrDefault(c =>
                c.Cat.CatName == category &&
                c.AssignmentName == asgname &&
                c.Cat.Class.Course.Dept.Subject == subject &&
                c.Cat.Class.Course.Number == num &&
                c.Cat.Class.SemesterSeason == season &&
                c.Cat.Class.SemesterYear == year
            );

            if (assignment == null)
            {
                return Json(new List<object>());
            }

            var submissions = db.Submissions
                .Where(s => s.AssignmentId == assignment.AssignmentId)
                .Select(s => new
                {
                    fname = s.UidNavigation.FirstName,
                    lname = s.UidNavigation.LastName,
                    uid = s.Uid,
                    time = s.SubmissionTime,
                    score = s.Score
                }).ToList();

            return Json(submissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            if (!IsValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(asgname) ||
                string.IsNullOrWhiteSpace(uid) ||
                score < 0)
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();
            asgname = asgname.Trim();
            uid = uid.Trim();

            var submissionData = db.Submissions
                .Where(c =>
                    c.Assignment.Cat.Class.Course.Dept.Subject == subject &&
                    c.Assignment.Cat.Class.Course.Number == num &&
                    c.Assignment.Cat.Class.SemesterSeason == season &&
                    c.Assignment.Cat.Class.SemesterYear == year &&
                    c.Assignment.Cat.CatName == category &&
                    c.Assignment.AssignmentName == asgname &&
                    c.Uid == uid)
                .Select(c => new
                {
                    Submission = c,
                    MaxPoints = c.Assignment.MaxPoints,
                    ClassId = c.Assignment.Cat.ClassId
                })
                .FirstOrDefault();

            if (submissionData == null)
            {
                return Json(new { success = false });
            }

            if (submissionData.MaxPoints < score)
            {
                return Json(new { success = false });
            }

            submissionData.Submission.Score = (uint) score;
            UpdateStudentGrade(submissionData.ClassId, uid);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Json(new List<object>());
            }

            uid = uid.Trim();

            var result = db.Classes
                .Where(cl => cl.ProfessorUid == uid)
                .Select(cl => new
                {
                    subject = cl.Course.Dept.Subject,
                    number = cl.Course.Number,
                    name = cl.Course.CourseName,
                    season = cl.SemesterSeason,
                    year = cl.SemesterYear
                }).ToList();

            return Json(result);
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

        private void UpdateGradesForClass(uint classId)
        {
            var enrollments = db.EnrollmentGrades
                .Where(e => e.ClassId == classId)
                .ToList();

            foreach (var enrollment in enrollments)
            {
                enrollment.Grade = CalculateLetterGrade(classId, enrollment.Uid);
            }
        }

        private void UpdateStudentGrade(uint classId, string uid)
        {
            var enrollment = db.EnrollmentGrades.FirstOrDefault(e =>
                e.ClassId == classId &&
                e.Uid == uid);

            if (enrollment == null)
            {
                return;
            }

            enrollment.Grade = CalculateLetterGrade(classId, uid);
        }

        private string CalculateLetterGrade(uint classId, string uid)
        {
            var categories = db.AssignmentCategories
                .Where(c => c.ClassId == classId)
                .Select(c => new
                {
                    c.CatId,
                    c.Weight,
                    Assignments = c.Assignments.Select(a => new
                    {
                        a.AssignmentId,
                        a.MaxPoints
                    }).ToList()
                })
                .ToList();

            var gradedCategories = categories
                .Where(c => c.Assignments.Count > 0)
                .ToList();

            if (gradedCategories.Count == 0)
            {
                return "--";
            }

            double weightedTotal = 0.0;
            double totalWeight = 0.0;

            foreach (var category in gradedCategories)
            {
                var assignmentIds = category.Assignments.Select(a => a.AssignmentId).ToList();
                var maxPoints = category.Assignments.Sum(a => (double)a.MaxPoints);

                if (maxPoints <= 0.0)
                {
                    continue;
                }

                var earnedPoints = db.Submissions
                    .Where(s => s.Uid == uid && assignmentIds.Contains(s.AssignmentId))
                    .Sum(s => (double?)s.Score) ?? 0.0;

                var categoryPercent = earnedPoints / maxPoints;
                weightedTotal += categoryPercent * category.Weight;
                totalWeight += category.Weight;
            }

            if (totalWeight <= 0.0)
            {
                return "--";
            }

            var classPercent = weightedTotal * (100.0 / totalWeight);
            return PercentageToLetterGrade(classPercent);
        }

        private static string PercentageToLetterGrade(double percentage)
        {
            if (percentage >= 93.0) return "A";
            if (percentage >= 90.0) return "A-";
            if (percentage >= 87.0) return "B+";
            if (percentage >= 83.0) return "B";
            if (percentage >= 80.0) return "B-";
            if (percentage >= 77.0) return "C+";
            if (percentage >= 73.0) return "C";
            if (percentage >= 70.0) return "C-";
            if (percentage >= 67.0) return "D+";
            if (percentage >= 63.0) return "D";
            if (percentage >= 60.0) return "D-";
            return "E";
        }
    }
}
