using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Department
    {
        public Department()
        {
            Courses = new HashSet<Course>();
            Professors = new HashSet<Professor>();
            Students = new HashSet<Student>();
        }

        public uint DeptId { get; set; }
        public string DeptName { get; set; } = null!;
        public string Subject { get; set; } = null!;

        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<Professor> Professors { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
