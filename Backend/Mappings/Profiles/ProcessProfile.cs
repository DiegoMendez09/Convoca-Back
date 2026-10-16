using AutoMapper;
using Backend.Models.DTOs.Responses;
using Backend.Models.External;

namespace Backend.Mappings.Profiles;

public class ProcessProfile : Profile
{
    public ProcessProfile()
    {
        CreateMap<ConvocaProcessRecord, ProcessResponse>()
            .ForMember(dest => dest.ProcessId, opt => opt.MapFrom(src => src.ProcessId ?? string.Empty))
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Reference)))
            .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.EntityName) ?? string.Empty))
            .ForMember(dest => dest.EntityNit, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.EntityNit)))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Department)))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.City)))
            .ForMember(dest => dest.ProcedureName, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.ProcedureName) ?? string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Description)))
            .ForMember(dest => dest.Phase, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Phase)))
            .ForMember(dest => dest.PublishedOn, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseDate(src.PublishedOn)))
            .ForMember(dest => dest.LastPublishedOn, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseDate(src.LastPublishedOn)))
            .ForMember(dest => dest.OffersDueOn, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseDate(src.OffersDueOn)))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseDecimal(src.BasePrice)))
            .ForMember(dest => dest.Modality, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Modality)))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseInt(src.Duration)))
            .ForMember(dest => dest.DurationUnit, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.DurationUnit)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Status)))
            .ForMember(dest => dest.Awarded, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.Awarded)))
            .ForMember(dest => dest.AwardedOn, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseDate(src.AwardedOn)))
            .ForMember(dest => dest.AwardAmount, opt => opt.MapFrom(src => Services.ConvocaValueParser.ParseDecimal(src.AwardAmount)))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.SupplierName)))
            .ForMember(dest => dest.SupplierNit, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.SupplierNit)))
            .ForMember(dest => dest.UnspscCode, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.UnspscCode)))
            .ForMember(dest => dest.OpeningStatus, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.OpeningStatus)))
            .ForMember(dest => dest.ContractType, opt => opt.MapFrom(src => Services.ConvocaValueParser.Clean(src.ContractType)))
            .ForMember(dest => dest.ProcessUrl, opt => opt.MapFrom(src => Services.ConvocaValueParser.ResolvePublicUrl(src.ProcessId, src.ProcessUrl)));
    }
}
