using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Administrator
    {
        public string Uid { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly Dob { get; set; }
    }
}
