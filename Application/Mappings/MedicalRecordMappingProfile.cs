using AutoMapper;
using Domain.Entities;
using Application.DTOs.Requests.MedicalrecordRequest;
using Application.DTOs.Responses.MedicalrecordResponse;

namespace Application.Mappings
{
    public class MedicalRecordMappingProfile : Profile
    {
        public MedicalRecordMappingProfile()
        {
            // Medical Record -> MedicalRecordResponse
            CreateMap<MedicalRecord, MedicalRecordResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.VisitDate, opt => opt.MapFrom(src => src.VisitDate))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.Prescriptions, opt => opt.MapFrom(src => src.Prescriptions))
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.FullName))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.Appointment.DateTime))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FullName));

            // CreateMedicalRecordRequest -> MedicalRecord
            CreateMap<CreateMedicalRecordRequest, MedicalRecord>()
                .ForMember(dest => dest.VisitDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.Prescriptions, opt => opt.MapFrom(src => src.Prescriptions))
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.DoctorId))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId));
        }
    }
}
