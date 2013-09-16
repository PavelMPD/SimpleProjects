using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Diary.Models
{
    public class WorkDay
    {
        public Int64? Id { get; set; }
        public DateTime Date { get; set; }
        public String UserName { get; set; }
        public String Comment { get; set; }
    }
}