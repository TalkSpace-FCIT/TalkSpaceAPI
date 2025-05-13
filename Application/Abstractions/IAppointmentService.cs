using Application.DTOs.Requests.AppointmentRequests;
using Application.DTOs.Responses.AppointmentResponses;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Results;
using Application.DTOs.Responses;

namespace Application.Abstractions
{
    public interface IAppointmentService
    {   
        Task<Result<AppointmentResponse>> CreateAppointmentAsync(CreateAppointmentRequest request);
        Task <Result<AppointmentResponse>> GetAppointmentByIdAsync(int id);
        Task<Result<AppointmentListResponse>> GetAppointmentsByPatientIdAsync(string patientId);
        Task<Result<AppointmentListResponse>> GetAppointmentsByDoctorIdAsync(string doctorId);
        Task<Result<AppointmentResponse>> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request);
        Task<Result<bool>> CancelAppointmentAsync(int id);
        Task<Result<bool>> RescheduleAppointmentAsync(int id, DateTime newDateTime);
        Task<Result<IEnumerable<AppointmentListResponse>>> GetAllAsync();

    }
}
