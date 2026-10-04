namespace Curriculos.Api.Dtos;

/// <summary>Dados encontrados no PDF. Campos não identificados vêm null.</summary>
public record ResumeParseResponse(string? FullName, string? Email, string? Phone);
