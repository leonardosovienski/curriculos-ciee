using Curriculos.Api.Models;

namespace Curriculos.Api.Dtos;

public record CandidateResponse(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    string? DesiredPosition,
    string? ProfessionalSummary,
    DateTimeOffset CreatedAt)
{
    public static CandidateResponse From(Candidate c) =>
        new(c.Id, c.FullName, c.Email, c.Phone, c.DesiredPosition, c.ProfessionalSummary, c.CreatedAt);
}

public record CandidateListItem(
    int Id,
    string FullName,
    string Email,
    string? DesiredPosition,
    DateTimeOffset CreatedAt);
