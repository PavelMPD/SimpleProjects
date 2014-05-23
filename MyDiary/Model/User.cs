using System;
using System.Collections.Generic;

namespace MyDiary.Model
{
    public class User : Entity
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String MiddleName { get; set; }
        public virtual ICollection<WorkDay> WorkDays { get; set; }
    }
}
