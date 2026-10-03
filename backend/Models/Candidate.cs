namespace Curriculos.Api.Models;

public class Candidate
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? DesiredPosition { get; set; }
    public string? ProfessionalSummary { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
