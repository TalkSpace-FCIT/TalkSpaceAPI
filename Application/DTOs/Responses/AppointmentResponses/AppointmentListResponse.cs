using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.AppointmentResponses
{
    public  class AppointmentListResponse
    {
        public List<AppointmentResponse> Appointments { get; set; }
        public int TotalCount { get; set; }
    }
}
