using AutoMapper;
using Backend.Models.Database;
using Backend.Models.DTOs.Responses;

namespace Backend.Mappings.Profiles;

public sealed class WorkspaceProfile : Profile
{
    public WorkspaceProfile()
    {
        CreateMap<Favorite, FavoriteResponse>();
    }
}
