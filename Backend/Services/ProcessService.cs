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
    private readonly ISecopApiClient _secopApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessService> _logger;

    public ProcessService(ISecopApiClient secopApiClient, IMapper mapper, ILogger<ProcessService> logger)
    {
        _secopApiClient = secopApiClient;
        _mapper = mapper;
        _logger = logger;
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
            var criteria = ToCriteria(request);
            var where = SecopQueryBuilder.BuildWhere(criteria);
            var offset = (request.PageNumber - 1) * request.PageSize;

            var recordsTask = _secopApiClient.SearchAsync(where, request.PageSize, offset, cancellationToken);
            var countTask = _secopApiClient.CountAsync(where, cancellationToken);
            await Task.WhenAll(recordsTask, countTask);

            var items = _mapper.Map<List<ProcessResponse>>(await recordsTask);
            var totalRecords = await countTask;
            var totalPages = request.PageSize == 0
                ? 0
                : (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return Result<PaginationResponse<ProcessResponse>>.Ok(new PaginationResponse<ProcessResponse>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            });
        }
        catch (Exception exception) when (IsUpstreamFailure(exception, cancellationToken))
        {
            _logger.LogError(exception, "Error consultando procesos de SECOP II");
            return Result<PaginationResponse<ProcessResponse>>.Fail(
                "No fue posible consultar los procesos de SECOP II.",
                StatusCodes.Status502BadGateway);
        }
    }

    public async Task<Result<ProcessResponse>> GetByIdAsync(string processId, CancellationToken cancellationToken)
    {
        if (!SecopValueParser.IsSafeProcessId(processId))
        {
            return Result<ProcessResponse>.Fail(
                "El identificador del proceso no es válido.",
                StatusCodes.Status400BadRequest);
        }

        try
        {
            var records = await _secopApiClient.SearchAsync(
                SecopQueryBuilder.BuildProcessIdWhere(processId),
                limit: 1,
                offset: 0,
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
                "No fue posible consultar el proceso en SECOP II.",
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
            var where = SecopQueryBuilder.BuildWhere(ToCriteria(request));
            var recordsTask = _secopApiClient.SearchAsync(where, SecopQueryBuilder.MaxExportSize, 0, cancellationToken);
            var countTask = _secopApiClient.CountAsync(where, cancellationToken);
            await Task.WhenAll(recordsTask, countTask);

            var items = _mapper.Map<List<ProcessResponse>>(await recordsTask);
            var totalRecords = await countTask;

            return Result<ProcessExportFile>.Ok(new ProcessExportFile(
                ProcessCsvFormatter.Format(items),
                "procesos-secop-ii.csv",
                totalRecords > items.Count,
                items.Count));
        }
        catch (Exception exception) when (IsUpstreamFailure(exception, cancellationToken))
        {
            _logger.LogError(exception, "Error exportando procesos de SECOP II");
            return Result<ProcessExportFile>.Fail(
                "No fue posible exportar los procesos de SECOP II.",
                StatusCodes.Status502BadGateway);
        }
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
            OpenForOffer = request.OpenForOffer ?? false
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

        if (request.PageSize is < 1 or > SecopQueryBuilder.MaxPageSize)
            return $"El tamaño de página debe estar entre 1 y {SecopQueryBuilder.MaxPageSize}.";

        return null;
    }

    private static string? ValidateText(string? value, string label)
    {
        if (value is { Length: > SecopQueryBuilder.MaxTextLength })
            return $"{label} no puede superar {SecopQueryBuilder.MaxTextLength} caracteres.";

        return null;
    }

    private static bool IsUpstreamFailure(Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
            return false;

        return exception is HttpRequestException or SecopUnavailableException or TaskCanceledException or JsonException;
    }
}
