using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Report:BaseEntity
    {
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public ReportType Type { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Content { get; set; } // Could store JSON/HTML or serialized data

        // Foreign Key
        public int? ReceptionistId { get; set; }

        // Navigation Properties
        // Uncomment when adding their classes
        // public Receptionist GeneratedBy { get; set; } // Aggregation with Receptionist
    }
}
