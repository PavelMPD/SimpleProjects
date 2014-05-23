using System;
using System.Collections.Generic;

namespace MyDiary.Model
{
    public class WorkDay : Entity
    {
        public DateTime Date { get; set; }
        public String Comment { get; set; }

        public User User { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }
}