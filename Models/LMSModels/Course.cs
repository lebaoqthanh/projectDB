using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public uint CourseId { get; set; }
        public uint DeptId { get; set; }
        public int Number { get; set; }
        public string CourseName { get; set; } = null!;

        public virtual Department Dept { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
