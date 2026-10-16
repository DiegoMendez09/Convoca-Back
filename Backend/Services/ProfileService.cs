using Backend.Models.Database;
using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Repositories.Implementations;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IUnitOfWork? _unitOfWork;

    public ProfileService(IServiceProvider serviceProvider)
    {
        _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
    }

    public async Task<Result<ProfileResponse>> GetAsync(CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable();

        var profile = await _unitOfWork.Profiles.GetAsync(cancellationToken);
        return Result<ProfileResponse>.Ok(ToResponse(profile));
    }

    public async Task<Result<ProfileResponse>> SaveAsync(SaveProfileRequest request, CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable();

        var title = (request.Title ?? string.Empty).Trim();
        var experience = (request.Experience ?? string.Empty).Trim();
        var keywords = (request.Keywords ?? string.Empty).Trim();

        if (title.Length > 200)
            return Result<ProfileResponse>.Fail("El título no puede superar 200 caracteres.", StatusCodes.Status400BadRequest);

        if (experience.Length > 2000)
            return Result<ProfileResponse>.Fail("La experiencia no puede superar 2000 caracteres.", StatusCodes.Status400BadRequest);

        if (keywords.Length > 300)
            return Result<ProfileResponse>.Fail("Las palabras clave no pueden superar 300 caracteres.", StatusCodes.Status400BadRequest);

        var profile = await _unitOfWork.Profiles.GetAsync(cancellationToken);
        if (profile is null)
        {
            profile = new ProfessionalProfile { ProfileId = ProfileRepository.SingleProfileId };
            await _unitOfWork.Profiles.AddAsync(profile, cancellationToken);
        }

        profile.Title = title;
        profile.Experience = experience;
        profile.Keywords = keywords;
        profile.UseForSearch = request.UseForSearch;
        profile.UpdatedOn = DateTime.UtcNow.AddHours(-5);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProfileResponse>.Ok(ToResponse(profile));
    }

    private static Result<ProfileResponse> Unavailable()
    {
        return Result<ProfileResponse>.Fail(
            "La base de Azure no está configurada. El perfil no se puede guardar.",
            StatusCodes.Status503ServiceUnavailable);
    }

    private static ProfileResponse ToResponse(ProfessionalProfile? profile)
    {
        if (profile is null)
        {
            return new ProfileResponse();
        }

        return new ProfileResponse
        {
            Title = profile.Title,
            Experience = profile.Experience,
            Keywords = profile.Keywords,
            UseForSearch = profile.UseForSearch,
            SearchText = ProfileSearchText.Build(profile.Title, profile.Experience, profile.Keywords),
            UpdatedOn = profile.UpdatedOn == default ? null : profile.UpdatedOn
        };
    }
}
