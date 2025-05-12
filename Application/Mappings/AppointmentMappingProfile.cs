using Application.DTOs.Requests.AppointmentRequests;
using Application.DTOs.Responses.AppointmentResponses;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class AppointmentMappingProfile : Profile
    {
        public AppointmentMappingProfile()
        {
            // Request -> Entity
            CreateMap<CreateAppointmentRequest, Appointment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => AppointmentStatus.Scheduled))
                .ForMember(dest => dest.StatusUpdatedOn, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecord, opt => opt.Ignore());

            CreateMap<UpdateAppointmentRequest, Appointment>()
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecord, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Entity -> Response
            CreateMap<Appointment, AppointmentResponse>()
                .ForMember(dest => dest.VisitType,
                    opt => opt.MapFrom(src => src.VisitType.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            // Removed StatusHistory mapping as AppointmentResponse does not have StatusHistory

            CreateMap<Appointment, AppointmentResponse>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
           .ForMember(dest => dest.VisitType, opt => opt.MapFrom(src => src.VisitType.ToString()))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
           .ForMember(dest => dest.DoctorName, opt => opt.Ignore())
           .ForMember(dest => dest.PatientName, opt => opt.Ignore());

            CreateMap<Appointment, AppointmentListResponse>();
        }
    }
}
