using Application.Abstractions;
using Application.DTOs.Requests.MedicalrecordRequest;
using Application.DTOs.Responses.MedicalrecordResponse;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Results;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Services
{
    public class MedicalRecordService : IMedicalRecordsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<MedicalRecord> _medicalRecordRepository;
        private readonly IRepository<Doctor> _doctorRepository;
        private readonly IRepository<Patient> _patientRepository;
        private readonly IRepository<Appointment> _appointmentRepository;

        public MedicalRecordService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _medicalRecordRepository = unitOfWork.GetRepository<MedicalRecord>();
            _doctorRepository = unitOfWork.GetRepository<Doctor>();
            _patientRepository = unitOfWork.GetRepository<Patient>();
            _appointmentRepository = unitOfWork.GetRepository<Appointment>();
        }

        public async Task<Result<IEnumerable<MedicalRecordResponse>>> GetAllAsync()
        {
            try
            {
                Log.Information("Fetching all medical records");

                var records = await _medicalRecordRepository.GetAllAsync(
                    include: q => ((IQueryable<MedicalRecord>)q)
                                  .Include(mr => mr.Patient)
                                  .Include(mr => mr.Appointment));

                var response = await Task.WhenAll(records.Select(record => MapToResponseWithDetails(record)));
                Log.Debug("Successfully retrieved {RecordCount} medical records", response.Count());
                return Result<IEnumerable<MedicalRecordResponse>>.Success(response, message: "All medical records retreived successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all medical records");
                return Result<IEnumerable<MedicalRecordResponse>>.Failure(
                    "An error occurred while fetching medical records",
                    ErrorSource.Database);
            }
        }

        public async Task<Result<MedicalRecordResponse>> GetByIdAsync(int id)
        {
            try
            {
                Log.Information("Fetching medical record with ID: {MedicalRecordId}", id);

                var record = await _medicalRecordRepository.GetByIdAsync(id);
                if (record == null)
                {
                    Log.Warning("Medical record not found with ID: {MedicalRecordId}", id);
                    return Result<MedicalRecordResponse>.Failure("Medical record not found", ErrorSource.TalkSpaceAPI);
                }

                var response = await MapToResponseWithDetails(record);

                Log.Debug("Successfully retrieved medical record with ID: {MedicalRecordId}", id);
                return Result<MedicalRecordResponse>.Success(response, message: $"Medical record with id: {id} retreived successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching medical record with ID: {MedicalRecordId}", id);
                return Result<MedicalRecordResponse>.Failure("An error occurred while fetching the medical record", ErrorSource.Database);
            }
        }

        // Update the problematic code block in the GetByPatientIdAsync method
        public async Task<Result<IEnumerable<MedicalRecordResponse>>> GetByPatientIdAsync(string patientId)
        {
            try
            {
                Log.Information("Fetching medical records for patient ID: {PatientId}", patientId);

                var records = await _medicalRecordRepository.FindAsync(
                    mr => mr.PatientId == patientId,
                    include: q => ((IQueryable<MedicalRecord>)q)
                                  .Include(mr => mr.Doctor)
                                  .Include(mr => mr.Patient)
                                  .Include(mr => mr.Appointment));


                var response = await Task.WhenAll(records.Select(record => MapToResponseWithDetails(record)));

                Log.Debug("Found {RecordCount} records for patient ID: {PatientId}",
                    response.Length, patientId);

                return Result<IEnumerable<MedicalRecordResponse>>.Success(response, message: $"medical record by patientid: {patientId} retreived successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching medical records for patient ID: {PatientId}", patientId);
                return Result<IEnumerable<MedicalRecordResponse>>.Failure(
                    "An error occurred while fetching medical records",
                    ErrorSource.Database);
            }
        }

        public async Task<Result<MedicalRecordResponse>> CreateAsync(CreateMedicalRecordRequest request)
        {
            try
            {
                Log.Information("Creating new medical record for appointment ID: {AppointmentId}",
                    request.AppointmentId);

                var medicalRecord = _mapper.Map<MedicalRecord>(request);

                await _medicalRecordRepository.AddAsync(medicalRecord);
                await _unitOfWork.CommitAsync();

                // Get the full response with details
                var response = await MapToResponseWithDetails(medicalRecord);

                Log.Information("Successfully created medical record with ID: {MedicalRecordId}",
                    medicalRecord.Id);

                return Result<MedicalRecordResponse>.Success(response, message: "Medical record created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating medical record for appointment ID: {AppointmentId}",
                    request.AppointmentId);
                return Result<MedicalRecordResponse>.Failure(
                    "An error occurred while creating the medical record",
                    ErrorSource.Database);
            }
        }


        private async Task<MedicalRecordResponse> MapToResponseWithDetails(MedicalRecord record)
        {
            // Get related entities
            var doctor = await _doctorRepository.GetByIdAsync(record.DoctorId);
            var patient = await _patientRepository.GetByIdAsync(record.PatientId);
            var appointment = await _appointmentRepository.GetByIdAsync(record.AppointmentId);

            return new MedicalRecordResponse(
                Id: record.Id,
                VisitDate: record.VisitDate,
                Notes: record.Notes,
                Prescriptions: record.Prescriptions,
                AppointmentId: record.AppointmentId,
                AppointmentDate: appointment?.DateTime ?? DateTime.MinValue,
                PatientName: patient?.FullName ?? "Unknown",
                DoctorName: doctor?.FullName ?? "Unknown",
                CreatedAt: record.CreatedAt,
                UpdatedAt: record.UpdatedAt
            );
        }
    }
}