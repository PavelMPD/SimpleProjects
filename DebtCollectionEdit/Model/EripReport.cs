using System;
using System.ComponentModel.DataAnnotations.Schema;

using DebtCollection.Model.Enums;

namespace DebtCollection.Model
{
    [Table("EripReports")]
    public class EripReport : Entity
    {
        public DateTime GenerationDate { get; set; }

        public RunWayType RunWay { get; set; }

        public bool Failed { get; set; }
    }
}
