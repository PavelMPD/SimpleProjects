using System;

namespace Diary.Models
{
    public class Task
    {
        public Int64? Id { get; set; }
        public Int64? WorkDayId { get; set; }
        public String Caption { get; set; }
        public String Description { get; set; }
        public Int32 TaskStatusId { get; set; }
    }
}