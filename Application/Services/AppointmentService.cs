using Application.Abstractions;
using Application.DTOs.Requests.AppointmentRequests;
using Application.DTOs.Responses.AppointmentResponses;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBillingRepository _billingRepository;
        private readonly IRepository<Appointment> _appointmentRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<Patient> _PatientRepository;

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper,IBillingRepository billingRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _billingRepository = billingRepository;
            _appointmentRepository = unitOfWork.GetRepository<Appointment>();
            _userRepository = unitOfWork.GetRepository<AppUser>();
            _PatientRepository = unitOfWork.GetRepository<Patient>();
        }

        public async Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request)
        {
            // Validate doctor exists
            var doctor = await _userRepository.GetByIdAsync(request.DoctorId);
            if (doctor == null || doctor.Discriminator != "Doctor")
                return Result<AppointmentResponse>.Failure("Doctor not found");

            // Validate patient exists
            var patient = await _PatientRepository.GetByIdAsync(request.PatientId);
            if (patient == null || patient.Discriminator != "Patient")
                return Result<AppointmentResponse>.Failure("Patient not found");

            // Check for overlapping appointments
            // Replace the problematic LINQ with a query that can be translated by EF Core
            var overlappingAppointments = await _appointmentRepository.FindAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.DateTime.Date == request.DateTime.Date &&
                a.Status != AppointmentStatus.Cancelled);

            var hasOverlap = overlappingAppointments.Any(a =>
                Math.Abs((a.DateTime - request.DateTime).TotalHours) < 1);

            if (hasOverlap)
                return Result<AppointmentResponse>.Failure("Doctor has a conflicting appointment at this time");

            var appointment = _mapper.Map<Appointment>(request);
            appointment.Status = AppointmentStatus.Scheduled;
            appointment.StatusUpdatedOn = DateTime.UtcNow;

            await _appointmentRepository.AddAsync(appointment);
            await _unitOfWork.CommitAsync();
            var billing = new Billing
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                InvoiceDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending
            };
            await _billingRepository.AddAsync(billing);
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<AppointmentResponse>(appointment);
            response.DoctorName = $"{doctor.FullName}";
            response.PatientName = $"{patient.FullName}";

            return Result<AppointmentResponse>.Success(response, "Created Done  Successfully");
        }

        public async Task<Result<AppointmentResponse>> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return Result<AppointmentResponse>.Failure("Appointment not found");

            var response = _mapper.Map<AppointmentResponse>(appointment);

            // Load names
            var doctor = await _userRepository.GetByIdAsync(appointment.DoctorId);
            var patient = await _userRepository.GetByIdAsync(appointment.PatientId);

            response.DoctorName = doctor != null ? $"{doctor.FullName}" : "Unknown";
            response.PatientName = patient != null ? $"{patient.FullName}" : "Unknown";
            response.PatientId = appointment.PatientId;

            return Result<AppointmentResponse>.Success(response, "Operation Done Successfully");
        }

        public async Task<Result<AppointmentListResponse>> GetAppointmentsByPatientIdAsync(string patientId)
        {
            var appointments = await _appointmentRepository.FindAsync(a => a.PatientId == patientId);
            var response = new AppointmentListResponse
            {
                Appointments = _mapper.Map<List<AppointmentResponse>>(appointments),
                TotalCount = appointments.Count()
            };

            // Load doctor names
            var doctorIds = appointments.Select(a => a.DoctorId).Distinct();
            var doctors = await _userRepository.FindAsync(u => doctorIds.Contains(u.Id));

            foreach (var appointment in response.Appointments)
            {
                // Find the original appointment to get DoctorId
                var originalAppointment = appointments.FirstOrDefault(a => a.Id == int.Parse(appointment.Id));
                if (originalAppointment != null)
                {
                    var doctor = doctors.FirstOrDefault(d => d.Id == originalAppointment.DoctorId);
                    appointment.DoctorName = doctor != null ? $"{doctor.FullName}" : "Unknown";
                }
                else
                {
                    appointment.DoctorName = "Unknown";
                }
            }

            return Result<AppointmentListResponse>.Success(response, "Operation Done Successfully");
        }

        public async Task<Result<AppointmentListResponse>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            var appointments = await _appointmentRepository
                .FindAsync(
                    predicate: a => a.DoctorId == doctorId,
                    include: q => (IIncludableQueryable<Appointment, object>)q.Include(a => a.Patient.FullName)
                .Select(a => new
                {
                    Appointment = a,
                    PatientName = a.Patient.FullName
                })
                .ToListAsync());

            if (appointments == null || !appointments.Any())
                return Result<AppointmentListResponse>.Failure("No appointments found for this doctor");

            var response = new AppointmentListResponse
            {
                Appointments = _mapper.Map<List<AppointmentResponse>>(appointments),
                TotalCount = appointments.Count()
            };

            return Result<AppointmentListResponse>.Success(response, "Opertational Done Successfully");
        }

        public async Task<Result<AppointmentResponse>> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return Result<AppointmentResponse>.Failure("Appointment not found");

            // Check for overlapping appointments if datetime changes
            if (request.DateTime.HasValue && request.DateTime.Value != appointment.DateTime)
            {
                var hasOverlap = await _appointmentRepository.ExistsAsync(a =>
                    a.Id != id &&
                    a.DoctorId == appointment.DoctorId &&
                    a.DateTime.Date == request.DateTime.Value.Date &&
                    Math.Abs((a.DateTime - request.DateTime.Value).TotalHours) < 1 &&
                    a.Status != AppointmentStatus.Cancelled);

                if (hasOverlap)
                    return Result<AppointmentResponse>.Failure("Doctor has a conflicting appointment at the new time");
            }

            _mapper.Map(request, appointment);
            _appointmentRepository.Update(appointment);
            await _unitOfWork.CommitAsync();

            var response = _mapper.Map<AppointmentResponse>(appointment);

            // Load names
            var doctor = await _userRepository.GetByIdAsync(appointment.DoctorId);
            var patient = await _userRepository.GetByIdAsync(appointment.PatientId);

            response.DoctorName = doctor != null ? $"{doctor.FullName}" : "Unknown";
            response.PatientName = patient != null ? $"{patient.FullName}" : "Unknown";

            return Result<AppointmentResponse>.Success(response, "Update Done Successfully");
        }

        public async Task<Result<bool>> CancelAppointmentAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return Result<bool>.Failure("Appointment not found");

            if (!appointment.IsCancellable())
                return Result<bool>.Failure("Appointment cannot be cancelled");

            appointment.UpdateStatus(AppointmentStatus.Cancelled);
            _appointmentRepository.Update(appointment);
            await _unitOfWork.CommitAsync();

            return Result<bool>.Success(true, "Cancelation Done Successfully");
        }

        public async Task<Result<bool>> RescheduleAppointmentAsync(int id, DateTime newDateTime)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return Result<bool>.Failure("Appointment not found");

            // Check for overlapping appointments
            var hasOverlap = await _appointmentRepository.ExistsAsync(a =>
                a.Id != id &&
                a.DoctorId == appointment.DoctorId &&
                a.DateTime.Date == newDateTime.Date &&
                Math.Abs((a.DateTime - newDateTime).TotalHours) < 1 &&
                a.Status != AppointmentStatus.Cancelled);

            if (hasOverlap)
                return Result<bool>.Failure("Doctor has a conflicting appointment at the new time");

            appointment.DateTime = newDateTime;
            _appointmentRepository.Update(appointment);
            await _unitOfWork.CommitAsync();

            return Result<bool>.Success(true, "Operation Completed Successfully");
        }

        public async Task<Result<IEnumerable<AppointmentListResponse>>> GetAllAsync()
        {
            // Step 1: Get all appointments
            var appointments = await _appointmentRepository.GetAllAsync();

            // Step 2: Map to AppointmentResponse list
            var appointmentResponses = _mapper.Map<List<AppointmentResponse>>(appointments);

            // Step 3: Gather all unique doctor and patient IDs
            var doctorIds = appointments.Select(a => a.DoctorId).Distinct().ToList();
            var patientIds = appointments.Select(a => a.PatientId).Distinct().ToList();

            // Step 4: Get all doctors and patients
            var doctors = await _userRepository.FindAsync(u => doctorIds.Contains(u.Id));
            var patients = await _userRepository.FindAsync(u => patientIds.Contains(u.Id));

            // Step 5: Set DoctorName and PatientName for each response
            foreach (var response in appointmentResponses)
            {
                var originalAppointment = appointments.FirstOrDefault(a => a.Id == int.Parse(response.Id));
                if (originalAppointment != null)
                {
                    var doctor = doctors.FirstOrDefault(d => d.Id == originalAppointment.DoctorId);
                    var patient = patients.FirstOrDefault(p => p.Id == originalAppointment.PatientId);
                    response.DoctorName = doctor != null ? doctor.FullName : "Unknown";
                    response.PatientName = patient != null ? patient.FullName : "Unknown";
                }
                else
                {
                    response.DoctorName = "Unknown";
                    response.PatientName = "Unknown";
                }
            }

            // Step 6: Wrap in AppointmentListResponse
            var listResponse = new AppointmentListResponse
            {
                Appointments = appointmentResponses,
                TotalCount = appointmentResponses.Count
            };

            // Step 7: Return result
            return Result<IEnumerable<AppointmentListResponse>>.Success(
                new List<AppointmentListResponse> { listResponse },
                "Operation Done Successfully"
            );
        }
    }
}