using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            name = name.Trim();

            var exists = db.Departments.Any(d => d.Subject == subject);
            if (exists)
            {
                return Json(new { success = false });
            }

            var department = new Department
            {
                Subject = subject,
                DeptName = name
            };

            db.Departments.Add(department);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();

            var result = db.Courses
                .Where(c => c.Dept.Subject== subject)
                .Select(c => new
                {
                    number = c.Number,
                    name = c.CourseName,
                })
                .ToList();
            
            
            return Json(result);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();

            var professors = db.Professors.Where(c => c.Dept.Subject == subject).Select(c => new
            {
                lname = c.LastName,
                fname = c.FirstName,
                uid = c.Uid
            }).ToList();
            
            return Json(professors);
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(name) || number <= 0)
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            name = name.Trim();

            var department = db.Departments.FirstOrDefault(d => d.Subject == subject);
            if (department == null)
            {
                return Json(new { success = false });
            }

            var exists = db.Courses.Any(c => c.DeptId == department.DeptId && c.Number == number);
            if (exists)
            {
                return Json(new { success = false });
            }

            var course = new Course
            {
                DeptId = department.DeptId,
                Number = number,
                CourseName = name
            };

            db.Courses.Add(course);
            db.SaveChanges();

            return Json(new { success = true });
            
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            if (!IsValidClassKey(subject, number, season, year) ||
                string.IsNullOrWhiteSpace(location) ||
                string.IsNullOrWhiteSpace(instructor))
            {
                return Json(new { success = false });
            }

            subject = subject.Trim();
            season = season.Trim();
            location = location.Trim();
            instructor = instructor.Trim();

            var course = db.Courses.FirstOrDefault(c =>
                c.Dept.Subject == subject &&
                c.Number == number);

            var professor = db.Professors.FirstOrDefault(p => p.Uid == instructor);

            if (course == null || professor == null)
            {
                return Json(new { success = false });
            }

            var startTime = TimeOnly.FromDateTime(start);
            var endTime = TimeOnly.FromDateTime(end);

            if (startTime >= endTime)
            {
                return Json(new { success = false });
            }

            var duplicateOffering = db.Classes.Any(c =>
                c.CourseId == course.CourseId &&
                c.SemesterSeason == season &&
                c.SemesterYear == year);

            if (duplicateOffering)
            {
                return Json(new { success = false });
            }

            var locationConflict = db.Classes.Any(c =>
                c.Location == location &&
                c.SemesterSeason == season &&
                c.SemesterYear == year &&
                c.StartTime < endTime &&
                startTime < c.EndTime);

            if (locationConflict)
            {
                return Json(new { success = false });
            }

            var newClass = new Class
            {
                CourseId = course.CourseId,
                SemesterSeason = season,
                SemesterYear = (uint)year,
                StartTime = startTime,
                EndTime = endTime,
                Location = location,
                ProfessorUid = instructor
            };

            db.Classes.Add(newClass);
            db.SaveChanges();
            
            
            return Json(new { success = true });
        }


        /*******End code to modify********/

        private static bool IsValidClassKey(string subject, int number, string season, int year)
        {
            if (string.IsNullOrWhiteSpace(subject) || number <= 0 || string.IsNullOrWhiteSpace(season) || year <= 0)
            {
                return false;
            }

            return season == "Spring" || season == "Summer" || season == "Fall";
        }
    }
}
