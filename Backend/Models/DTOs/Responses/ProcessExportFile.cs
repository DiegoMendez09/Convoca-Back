namespace Backend.Models.DTOs.Responses;

public sealed record ProcessExportFile(byte[] Content, string FileName, bool Truncated, int ExportedCount);
