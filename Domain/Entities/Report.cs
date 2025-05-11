using Domain.Enums;

namespace Domain.Entities
{
    public class Report:BaseEntity
    {
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public ReportType Type { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Content { get; set; } = null!;

        public string DoctorId { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}
