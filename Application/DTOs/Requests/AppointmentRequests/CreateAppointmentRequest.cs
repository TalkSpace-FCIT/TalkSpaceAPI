using Application.CustomValidation;
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
        [Required(ErrorMessage = "Appointment date and time is required")]
        [FutureDateAttribute(ErrorMessage = "Appointment date must be in the future")]

        public DateTime DateTime { get; set; }

        [Required(ErrorMessage = "Visit type is required")]
        [StringLength(20, ErrorMessage = "Visit type cannot exceed 20 characters")]
        [RegularExpression(@"^(Consultation|FollowUp|Emergency)$",
          ErrorMessage = "Visit type must be: Consultation, FollowUp, or Emergency")]
        public string VisitType { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        public string DoctorId { get; set; }
        [Required(ErrorMessage = "Patient ID is required")]
        public string PatientId { get; set; }
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}
