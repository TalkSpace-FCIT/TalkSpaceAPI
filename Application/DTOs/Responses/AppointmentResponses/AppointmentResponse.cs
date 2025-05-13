using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.AppointmentResponses
{
    public class AppointmentResponse
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }
        public string VisitType { get; set; }
        public string Status { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string PatientId { get; set; }
        public DateTime StatusUpdatedOn { get; set; }
    }
}
