using AutoMapper;
using Backend.Models.Database;
using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services;

public sealed class FavoriteService : IFavoriteService
{
    private readonly IProcessService _processService;
    private readonly IUnitOfWork? _unitOfWork;
    private readonly IMapper _mapper;

    public FavoriteService(IProcessService processService, IServiceProvider serviceProvider, IMapper mapper)
    {
        _processService = processService;
        _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<FavoriteResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable<IReadOnlyList<FavoriteResponse>>();

        var favorites = await _unitOfWork.Favorites.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<FavoriteResponse>>.Ok(_mapper.Map<IReadOnlyList<FavoriteResponse>>(favorites));
    }

    public async Task<Result<FavoriteResponse>> CreateAsync(CreateFavoriteRequest request, CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable<FavoriteResponse>();

        var processId = request.ProcessId?.Trim() ?? string.Empty;
        if (!ConvocaValueParser.IsSafeProcessId(processId))
            return Result<FavoriteResponse>.Fail("El identificador del proceso no es válido.", StatusCodes.Status400BadRequest);

        if (request.Notes is not null && request.Notes.Trim().Length > 500)
            return Result<FavoriteResponse>.Fail("La nota no puede superar 500 caracteres.", StatusCodes.Status400BadRequest);

        var existing = await _unitOfWork.Favorites.GetByProcessIdAsync(processId, cancellationToken);
        if (existing is not null)
            return Result<FavoriteResponse>.Fail("Este proceso ya está en favoritos.", StatusCodes.Status409Conflict);

        var processResult = await _processService.GetByIdAsync(processId, cancellationToken);
        if (!processResult.IsSuccess || processResult.Value is null)
            return Result<FavoriteResponse>.Fail(processResult.Error ?? "No se encontró el proceso.", processResult.StatusCode);

        var process = processResult.Value;
        var favorite = new Favorite
        {
            FavoriteId = Guid.NewGuid(),
            ProcessId = process.ProcessId,
            ProcedureName = process.ProcedureName,
            EntityName = process.EntityName,
            Status = process.Status,
            Modality = process.Modality,
            BasePrice = process.BasePrice,
            PublishedOn = process.PublishedOn,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedOn = DateTime.UtcNow.AddHours(-5)
        };

        await _unitOfWork.Favorites.AddAsync(favorite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<FavoriteResponse>.Ok(_mapper.Map<FavoriteResponse>(favorite), StatusCodes.Status201Created);
    }

    public async Task<Result<bool>> DeleteAsync(string processId, CancellationToken cancellationToken)
    {
        if (_unitOfWork is null)
            return Unavailable<bool>();

        if (!ConvocaValueParser.IsSafeProcessId(processId))
            return Result<bool>.Fail("El identificador del proceso no es válido.", StatusCodes.Status400BadRequest);

        var favorite = await _unitOfWork.Favorites.GetByProcessIdAsync(processId, cancellationToken);
        if (favorite is null)
            return Result<bool>.Fail("El proceso no está en favoritos.", StatusCodes.Status404NotFound);

        _unitOfWork.Favorites.Delete(favorite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }

    private static Result<T> Unavailable<T>()
    {
        return Result<T>.Fail(
            "La base de Azure no está configurada. Define ConnectionStrings:ConvocaDb.",
            StatusCodes.Status503ServiceUnavailable);
    }
}
