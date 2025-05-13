using Application.Abstractions;
using Application.DTOs.Requests.MedicalrecordRequest;
using Application.DTOs.Responses.MedicalrecordResponse;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : BaseApiController
    {
        private readonly IMedicalRecordsService medicalRecordsService;

        public MedicalRecordsController(IMedicalRecordsService medicalRecordsService)
        {
            this.medicalRecordsService = medicalRecordsService;
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("Request received to get all medical records");
            var result = await medicalRecordsService.GetAllAsync();
            return HandleResult(result);
        }

        [Authorize(Roles = "Doctor,Patient")]
        [HttpGet("patient/{id}")]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPatientId(string id)
        {
            Log.Information("Request received for patient medical records: {PatientId}", id);
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Patient") && userIdFromToken != id)
            {
                return StatusCode(403, new
                {
                    Error = "Forbidden",
                    Message = "Patients can only access their own records"
                });
            }
            var result = await medicalRecordsService.GetByPatientIdAsync(id);

            return HandleResult(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("add")]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordRequest request)
        {
            Log.Information("Request received to create medical record for appointment {AppointmentID}", request.AppointmentId);

            var result = await medicalRecordsService.CreateAsync(request);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : HandleResult(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            Log.Information("Request received for medical record ID: {MedicalRecordId}", id);

            var result = await medicalRecordsService.GetByIdAsync(id);

            return HandleResult(result);
        }
    }
}
