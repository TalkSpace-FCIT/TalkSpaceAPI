using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.AppointmentRequests
{
    public class CreateAppointmentRequest
    {
        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        public string VisitType { get; set; } 

        [Required]
        public string DoctorId { get; set; }

        public string PatientId { get; set; }

        public string? Notes { get; set; }
    }
}
