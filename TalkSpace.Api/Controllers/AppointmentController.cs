using Application.Abstractions;
using Application.DTOs.Requests.AppointmentRequests;
using Application.DTOs.Responses.AppointmentResponses;
using Domain.Entities;
using Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Serilog;
using System.Security.Claims;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Roles ="Patient,Doctor")] 
    public class AppointmentController : BaseApiController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var result = await _appointmentService.CreateAppointmentAsync(request);
            return HandleResult(result);
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _appointmentService.GetAllAsync();
            return HandleResult(result);
        }

        [HttpGet("patient/{patientId}")] 
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByPatientId(string patientId)
        {
            // Patients can only view their own appointments
            if (User.IsInRole("Patient") && User.FindFirstValue(ClaimTypes.NameIdentifier) != patientId)
            {
                return Forbid();
            }

            var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
            return HandleResult(result);
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Doctor")] 
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByDoctorId(string doctorId)
        {
            // Doctors can only view their own appointments
            if (User.IsInRole("Doctor") && User.FindFirstValue(ClaimTypes.NameIdentifier) != doctorId)
            {
                return Forbid();
            }

            var result = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
            return HandleResult(result);
        }

        [HttpGet("{appointmentId}")]
        [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int appointmentId)
        {
            var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (!result.IsSuccess || result.Value == null)
            {
                return HandleResult(result);
            }
            // Only allow the patient who owns the appointment to view it
            if (User.IsInRole("Patient") )
            {
                var currentuserid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(result.Value.PatientId != currentuserid)
                {
                    return Forbid();
                }

            }

            return HandleResult(result);
        }
    }
}