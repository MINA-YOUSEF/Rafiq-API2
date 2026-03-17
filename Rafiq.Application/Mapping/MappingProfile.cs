using AutoMapper;
using Rafiq.Application.DTOs.Appointments;
using Rafiq.Application.DTOs.Children;
using Rafiq.Application.DTOs.Exercises;
using Rafiq.Application.DTOs.MedicalReports;
using Rafiq.Application.DTOs.Media;
using Rafiq.Application.DTOs.Messages;
using Rafiq.Application.DTOs.ProgressReports;
using Rafiq.Application.DTOs.Sessions;
using Rafiq.Application.DTOs.TreatmentPlans;
using Rafiq.Domain.Entities;

namespace Rafiq.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Child, ChildDto>();
        CreateMap<Appointment, AppointmentDto>();
        CreateMap<MedicalReport, MedicalReportDto>();
        CreateMap<Media, MediaDto>();
        CreateMap<Exercise, ExerciseDto>()
            .ForMember(dest => dest.MediaUrl, opt => opt.MapFrom(src => src.Media.Url))
            .ForMember(dest => dest.MediaThumbnailUrl, opt => opt.MapFrom(src => src.Media.ThumbnailUrl));
        CreateMap<SessionResult, SessionResultDto>();

        CreateMap<Session, SessionDto>()
            .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.SessionResult));

        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SentAtUtc, opt => opt.MapFrom(src => src.CreatedAtUtc));

        CreateMap<ProgressReport, ProgressReportDto>();

        CreateMap<TreatmentPlanExercise, TreatmentPlanExerciseDto>()
            .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));

        CreateMap<TreatmentPlan, TreatmentPlanDto>()
            .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises));
    }
}
