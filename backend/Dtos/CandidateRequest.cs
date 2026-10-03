using System.ComponentModel.DataAnnotations;
using Curriculos.Api.Models;

namespace Curriculos.Api.Dtos;

public class CandidateRequest
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [MaxLength(CandidateRules.FullNameMaxLength, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [MaxLength(CandidateRules.EmailMaxLength, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    [RegularExpression(CandidateRules.EmailPattern, ErrorMessage = "Informe um e-mail válido.")]
    public string? Email { get; set; }

    [MaxLength(CandidateRules.PhoneMaxLength, ErrorMessage = "O telefone deve ter no máximo 30 caracteres.")]
    public string? Phone { get; set; }

    [MaxLength(CandidateRules.DesiredPositionMaxLength, ErrorMessage = "A área ou cargo deve ter no máximo 150 caracteres.")]
    public string? DesiredPosition { get; set; }

    [MaxLength(CandidateRules.ProfessionalSummaryMaxLength, ErrorMessage = "O resumo deve ter no máximo 2000 caracteres.")]
    public string? ProfessionalSummary { get; set; }
}
