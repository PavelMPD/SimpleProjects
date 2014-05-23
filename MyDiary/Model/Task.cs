using System;
using MyDiary.Model.Enums;

namespace MyDiary.Model
{
    public class Task : Entity
    {
        public String Caption { get; set; }
        public String Description { get; set; }
        public TaskStatus TaskStatusId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        /// <summary>
        /// Потраченное время на задачу
        /// </summary>
        public Int32 SpendTime { get; set; }

        public WorkDay WorkDay { get; set; }
    }
}