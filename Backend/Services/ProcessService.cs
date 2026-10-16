using System.Text.Json;
using AutoMapper;
using Backend.Clients;
using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Models.DTOs.Responses.Pagination;
using Backend.Services.Interfaces;

namespace Backend.Services;

public sealed class ProcessService : IProcessService
{
    private readonly IConvocaApiClient _convocaApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessService> _logger;
    private readonly IProfileService? _profileService;

    public ProcessService(
        IConvocaApiClient convocaApiClient,
        IMapper mapper,
        ILogger<ProcessService> logger,
        IProfileService? profileService = null)
    {
        _convocaApiClient = convocaApiClient;
        _mapper = mapper;
        _logger = logger;
        _profileService = profileService;
    }

    public async Task<Result<PaginationResponse<ProcessResponse>>> SearchAsync(
        ProcessSearchRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCommon(request) ?? ValidatePaging(request);
        if (validationError is not null)
            return Result<PaginationResponse<ProcessResponse>>.Fail(validationError, StatusCodes.Status400BadRequest);

        try
        {
            var (profileKeyword, profilePhrases) = await ApplyProfileAsync(request, cancellationToken);
            var criteria = ToCriteria(request) with { ProfilePhrases = profilePhrases };
            var where = ConvocaQueryBuilder.BuildWhere(criteria);
            var offset = (request.PageNumber - 1) * request.PageSize;
            var records = await _convocaApiClient.SearchAsync(
                where,
                ConvocaQueryBuilder.BuildFullText(criteria.Keyword),
                request.PageSize + 1,
                offset,
                compact: true,
                cancellationToken);

            var hasMore = records.Count > request.PageSize;
            var page = hasMore ? records.Take(request.PageSize).ToList() : records.ToList();
            var items = _mapper.Map<List<ProcessResponse>>(page);

            return Result<PaginationResponse<ProcessResponse>>.Ok(new PaginationResponse<ProcessResponse>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = items.Count == 0 ? 0 : offset + items.Count,
                TotalPages = hasMore ? request.PageNumber + 1 : request.PageNumber,
                HasMore = hasMore,
                ProfileKeyword = profileKeyword
            });
        }
        catch (Exception exception) when (IsUpstreamFailure(exception, cancellationToken))
        {
            _logger.LogError(exception, "convocaApp no pudo consultar los procesos");
            return Result<PaginationResponse<ProcessResponse>>.Fail(
                "convocaApp no pudo consultar los procesos de contratación.",
                StatusCodes.Status502BadGateway);
        }
    }

    public async Task<Result<ProcessResponse>> GetByIdAsync(string processId, CancellationToken cancellationToken)
    {
        if (!ConvocaValueParser.IsSafeProcessId(processId))
        {
            return Result<ProcessResponse>.Fail(
                "El identificador del proceso no es válido.",
                StatusCodes.Status400BadRequest);
        }

        try
        {
            var records = await _convocaApiClient.SearchAsync(
                ConvocaQueryBuilder.BuildProcessIdWhere(processId),
                fullText: null,
                limit: 1,
                offset: 0,
                compact: false,
                cancellationToken);

            var record = records.FirstOrDefault();
            if (record is null)
            {
                return Result<ProcessResponse>.Fail(
                    "No se encontró el proceso publicado.",
                    StatusCodes.Status404NotFound);
            }

            return Result<ProcessResponse>.Ok(_mapper.Map<ProcessResponse>(record));
        }
        catch (Exception exception) when (IsUpstreamFailure(exception, cancellationToken))
        {
            _logger.LogError(exception, "Error consultando el proceso {ProcessId}", processId);
            return Result<ProcessResponse>.Fail(
                "convocaApp no pudo consultar este proceso.",
                StatusCodes.Status502BadGateway);
        }
    }

    public async Task<Result<ProcessExportFile>> ExportAsync(
        ProcessSearchRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCommon(request);
        if (validationError is not null)
            return Result<ProcessExportFile>.Fail(validationError, StatusCodes.Status400BadRequest);

        try
        {
            var (_, profilePhrases) = await ApplyProfileAsync(request, cancellationToken);
            var criteria = ToCriteria(request) with { ProfilePhrases = profilePhrases };
            var where = ConvocaQueryBuilder.BuildWhere(criteria);
            var recordsTask = _convocaApiClient.SearchAsync(
                where,
                ConvocaQueryBuilder.BuildFullText(criteria.Keyword),
                ConvocaQueryBuilder.MaxExportSize,
                0,
                compact: false,
                cancellationToken);
            var countTask = _convocaApiClient.CountAsync(where, cancellationToken);
            await Task.WhenAll(recordsTask, countTask);

            var items = _mapper.Map<List<ProcessResponse>>(await recordsTask);
            var totalRecords = await countTask;

            return Result<ProcessExportFile>.Ok(new ProcessExportFile(
                ProcessCsvFormatter.Format(items),
                "convoca-processes.csv",
                totalRecords > items.Count,
                items.Count));
        }
        catch (Exception exception) when (IsUpstreamFailure(exception, cancellationToken))
        {
            _logger.LogError(exception, "convocaApp no pudo exportar los procesos");
            return Result<ProcessExportFile>.Fail(
                "convocaApp no pudo exportar los procesos.",
                StatusCodes.Status502BadGateway);
        }
    }

    private async Task<(string? Display, IReadOnlyList<string> Phrases)> ApplyProfileAsync(
        ProcessSearchRequest request,
        CancellationToken cancellationToken)
    {
        if (_profileService is null)
            return (null, []);

        var profile = await _profileService.GetAsync(cancellationToken);
        if (!profile.IsSuccess || profile.Value is null || !profile.Value.UseForSearch)
            return (null, []);

        var phrases = ProfileSearchText.Phrases(profile.Value.Title, profile.Value.Experience, profile.Value.Keywords);
        if (phrases.Count == 0)
            return (null, []);

        request.Keyword = null;
        return (ProfileSearchText.Build(profile.Value.Title, profile.Value.Experience, profile.Value.Keywords), phrases);
    }

    internal static ProcessSearchCriteria ToCriteria(ProcessSearchRequest request)
    {
        return new ProcessSearchCriteria
        {
            Keyword = request.Keyword?.Trim(),
            EntityName = request.EntityName?.Trim(),
            EntityNit = request.EntityNit?.Trim(),
            Department = request.Department?.Trim(),
            City = request.City?.Trim(),
            Modality = request.Modality?.Trim(),
            Phase = request.Phase?.Trim(),
            Status = request.Status?.Trim(),
            ContractType = request.ContractType?.Trim(),
            PublishedFrom = request.PublishedFrom,
            PublishedTo = request.PublishedTo,
            MinBasePrice = request.MinBasePrice,
            MaxBasePrice = request.MaxBasePrice,
            PublishedOnly = request.PublishedOnly ?? true,
            OpenForOffer = request.OpenForOffer ?? false,
            OffersDueFrom = request.OpenForOffer == true
                ? DateTime.UtcNow.AddHours(-5).Date
                : null
        };
    }

    private static string? ValidateCommon(ProcessSearchRequest request)
    {
        var textError = ValidateText(request.Keyword, "La palabra clave")
            ?? ValidateText(request.EntityName, "La entidad")
            ?? ValidateText(request.EntityNit, "El NIT")
            ?? ValidateText(request.Department, "El departamento")
            ?? ValidateText(request.City, "La ciudad")
            ?? ValidateText(request.Modality, "La modalidad")
            ?? ValidateText(request.Phase, "La fase")
            ?? ValidateText(request.Status, "El estado")
            ?? ValidateText(request.ContractType, "El tipo de contrato");

        if (textError is not null)
            return textError;

        if (request.PublishedFrom is not null &&
            request.PublishedTo is not null &&
            request.PublishedFrom.Value.Date > request.PublishedTo.Value.Date)
            return "La fecha inicial debe ser anterior o igual a la fecha final.";

        if (request.MinBasePrice is < 0 || request.MaxBasePrice is < 0)
            return "El precio base no puede ser negativo.";

        if (request.MinBasePrice is not null &&
            request.MaxBasePrice is not null &&
            request.MinBasePrice > request.MaxBasePrice)
            return "El precio mínimo debe ser menor o igual al precio máximo.";

        return null;
    }

    private static string? ValidatePaging(ProcessSearchRequest request)
    {
        if (request.PageNumber < 1)
            return "El número de página debe ser mayor a cero.";

        if (request.PageSize is < 1 or > ConvocaQueryBuilder.MaxPageSize)
            return $"El tamaño de página debe estar entre 1 y {ConvocaQueryBuilder.MaxPageSize}.";

        return null;
    }

    private static string? ValidateText(string? value, string label)
    {
        if (value is { Length: > ConvocaQueryBuilder.MaxTextLength })
            return $"{label} no puede superar {ConvocaQueryBuilder.MaxTextLength} caracteres.";

        return null;
    }

    private static bool IsUpstreamFailure(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
            return false;

        return exception is HttpRequestException or ConvocaUnavailableException or TaskCanceledException or JsonException;
    }
}
