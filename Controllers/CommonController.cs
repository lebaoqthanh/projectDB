using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var dps = db.Departments.Select(c=> new
            {
                name=c.DeptName,
                subject=c.Subject,
                
            }).ToList();
            return Json(dps);
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalog = (from d in db.Departments
                select new
                {
                    subject = d.Subject,
                    dname = d.DeptName,
                    courses = (from c in db.Courses
                        where c.DeptId == d.DeptId
                        orderby c.Number
                        select new
                        {
                            number = c.Number,
                            cname = c.CourseName
                        }).ToList()
                }).ToList();

            return Json(catalog);
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            if (string.IsNullOrWhiteSpace(subject) || number <= 0)
            {
                return Json(new List<object>());
            }

            subject = subject.Trim();

            var classes = (from cl in db.Classes
                join co in db.Courses on cl.CourseId equals co.CourseId
                join d in db.Departments on co.DeptId equals d.DeptId
                join p in db.Professors on cl.ProfessorUid equals p.Uid
                where d.Subject == subject && co.Number == number
                select new
                {
                    season = cl.SemesterSeason,
                    year = cl.SemesterYear,
                    location = cl.Location,
                    start = cl.StartTime.ToString("HH:mm:ss"),
                    end = cl.EndTime.ToString("HH:mm:ss"),
                    fname = p.FirstName,
                    lname = p.LastName,
                }).ToList();
            return Json(classes);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            if (!HasValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(asgname))
            {
                return Content("Assignment not found.");
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();
            asgname = asgname.Trim();

            var assignment = (from a in db.Assignments
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                join cl in db.Classes on ac.ClassId equals cl.ClassId
                join co in db.Courses on cl.CourseId equals co.CourseId
                join d in db.Departments on co.DeptId equals d.DeptId
                where ac.CatName == category
                    && a.AssignmentName == asgname
                    && cl.SemesterSeason == season
                    && cl.SemesterYear == year
                    && co.Number == num
                    && d.Subject == subject
                select a).FirstOrDefault();

            if (assignment == null)
            {
                return Content("Assignment not found.");
            }

            return Content(assignment.Contents, "text/html");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            if (!HasValidClassKey(subject, num, season, year) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(asgname) ||
                string.IsNullOrWhiteSpace(uid))
            {
                return Content("");
            }

            subject = subject.Trim();
            season = season.Trim();
            category = category.Trim();
            asgname = asgname.Trim();
            uid = uid.Trim();

            var sub = (from s in db.Submissions
                join a in db.Assignments on s.AssignmentId equals a.AssignmentId
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                join cl in db.Classes on ac.ClassId equals cl.ClassId
                join co in db.Courses on cl.CourseId equals co.CourseId
                join d in db.Departments on co.DeptId equals d.DeptId
                where d.Subject == subject
                    && co.Number == num
                    && cl.SemesterSeason == season
                    && cl.SemesterYear == year
                    && ac.CatName == category
                    && a.AssignmentName == asgname
                    && s.Uid == uid
                select s).FirstOrDefault();
            if (sub == null)
            {
                return Content("");
            }
            return Content(sub.Contents, "text/html");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Json(new { success = false });
            }

            uid = uid.Trim();

            var admin = db.Administrators.FirstOrDefault(a => a.Uid == uid);
            if (admin != null)
            {
                return Json(new
                {
                    fname = admin.FirstName,
                    lname = admin.LastName,
                    uid = admin.Uid
                });
            }

            // 2. Check if the user is a Professor
            var prof = (from p in db.Professors
                join d in db.Departments on p.DeptId equals d.DeptId
                where p.Uid == uid
                select new
                {
                    fname = p.FirstName,
                    lname = p.LastName,
                    uid = p.Uid,
                    department = d.DeptName
                }).FirstOrDefault();

            if (prof != null)
            {
                return Json(prof);
            }

            // 3. Check if the user is a Student
            var student = (from s in db.Students
                join d in db.Departments on s.Major equals d.DeptId
                where s.Uid == uid
                select new
                {
                    fname = s.FirstName,
                    lname = s.LastName,
                    uid = s.Uid,
                    department = d.DeptName
                }).FirstOrDefault();

            if (student != null)
            {
                return Json(student);
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/

        private static bool HasValidClassKey(string subject, int num, string season, int year)
        {
            return !string.IsNullOrWhiteSpace(subject)
                && num > 0
                && !string.IsNullOrWhiteSpace(season)
                && year > 0;
        }
    }
}
