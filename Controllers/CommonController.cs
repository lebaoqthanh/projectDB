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
                
            });
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
            var cat = db.Courses.Select(c => new {
                subject = c.Dept.Subject,
                dname = c.Dept.DeptName
            }).ToList();
            return Json(cat);
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
            var classes = db.Classes.Where(c => c.Course.Dept.Subject == subject && c.Course.Number == number).Select(c => new
            {
                season=c.SemesterSeason,
                year=c.SemesterYear,
                location=c.Location,
                start=c.StartTime,
                end=c.EndTime,
                fname=c.ProfessorU.FirstName,
                lname=c.ProfessorU.LastName,

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
        {   var assignment = db.Assignments
                .FirstOrDefault(a => 
                    a.Cat.CatName == category &&
                    a.AssignmentName == asgname &&
                    a.Cat.Class.SemesterSeason == season &&
                    a.Cat.Class.SemesterYear == year &&
                    a.Cat.Class.Course.Number == num &&
                    a.Cat.Class.Course.Dept.Subject == subject
                );

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
            var sub = db.Submissions.FirstOrDefault(s =>
                s.Assignment.Cat.Class.Course.Dept.Subject == subject &&
                s.Assignment.Cat.Class.Course.Number == num &&
                s.Assignment.Cat.Class.SemesterSeason==season &&
                s.Assignment.Cat.Class.SemesterYear==year &&
                s.Assignment.Cat.CatName==category&&
                s.Assignment.AssignmentName==asgname &&
                s.UidNavigation.Uid==uid);
            if (sub == null)
            {
                return Content("Submission not found.");
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
            var prof = db.Professors.FirstOrDefault(p => p.UId == uid);
            if (prof != null)
            {
                return Json(new
                {
                    fname = prof.FirstName,
                    lname = prof.LastName,
                    uid = prof.Uid,
                    department = prof.Dept.DeptName // Navigation property to Department
                });
            }

            // 3. Check if the user is a Student
            var student = db.Students.FirstOrDefault(s => s.UId == uid);
            if (student != null)
            {
                return Json(new
                {
                    fname = student.FirstName,
                    lname = student.LastName,
                    uid = student.Uid,
                    department = student.MajorNavigation.DeptName // Navigation property to Department
                });
            }

            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

