using Application.Abstractions;
using Application.DTOs.Requests.AppointmentRequests;
using Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Serilog;
using System.Security.Claims;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    public class AppointmentController : BaseApiController
    {
        private readonly IAppointmentService appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            this.appointmentService = appointmentService;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest appointmentRequest)
        {

            var res = await appointmentService.CreateAppointmentAsync(appointmentRequest);

            return HandleResult(res); 
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAppointments()
        {
          
            var res = await appointmentService.GetAllAsync();

            return HandleResult(res);
        }
        [HttpGet("GetBypatient/{id}")]
        public async Task<IActionResult> GetAppointmentByPatientId(string id)
        {
            
            var res = await appointmentService.GetAppointmentsByPatientIdAsync(id);
            return HandleResult(res);
        }
        [HttpGet("GetBydoctor/{id}")]
        public async Task<IActionResult> GetAppointmentByDoctorId(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != id)
            {
                return StatusCode(403, new
                {
                    Error = "Forbidden",
                    Message = "Patients can only access their own records"
                });
            }
            var res = await appointmentService.GetAppointmentsByDoctorIdAsync(id);
            return HandleResult(res);
        }
        [HttpGet("GetByID/{id}")]
        public async Task<IActionResult> GetAppointmentById(string id)
        {
            
            var res = await appointmentService.GetAppointmentByIdAsync(id);
            return HandleResult(res);
        }


    }
}
