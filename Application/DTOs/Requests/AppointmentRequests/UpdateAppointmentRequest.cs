using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.AppointmentRequests
{
    public class UpdateAppointmentRequest
    {
        public DateTime? DateTime { get; set; }
        public string? VisitType { get; set; }
        public string? Status { get; set; } 
        public string? Notes { get; set; }
    }
}
