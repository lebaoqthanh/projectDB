using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignments = new HashSet<Assignment>();
        }

        public uint CatId { get; set; }
        public uint ClassId { get; set; }
        public string CatName { get; set; } = null!;
        public uint Weight { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
