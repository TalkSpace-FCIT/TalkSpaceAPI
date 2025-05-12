using Application.Abstractions;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace TalkSpace.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class MedicalRecordsController : BaseApiController
    {
        private readonly IMedicalRecordsService medicalRecordsService;

        public MedicalRecordsController(IMedicalRecordsService medicalRecordsService)
        {
            this.medicalRecordsService = medicalRecordsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("Request received to get all medical records");
            var result = await medicalRecordsService.GetAllAsync();
            return HandleResult(result);
        }

        [HttpGet("patient/{id}")]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPatientId(string id)
        {
            Log.Information("Request received for patient medical records: {PatientId}", id);

            var result = await medicalRecordsService.GetByPatientIdAsync(id);

            return HandleResult(result);
        }

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
