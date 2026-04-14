using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class EnrollmentGrade
    {
        public uint ClassId { get; set; }
        public string Uid { get; set; } = null!;
        public string? Grade { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual Student UidNavigation { get; set; } = null!;
    }
}
