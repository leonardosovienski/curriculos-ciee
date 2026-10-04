using System.ComponentModel.DataAnnotations;
using Curriculos.Api.Models;

namespace Curriculos.Api.Dtos;

public class CandidateRequest
{
    // Os valores são normalizados (trim) já na desserialização, para que as validações
    // abaixo avaliem o mesmo valor que será gravado. Ex.: " ana@example.com " é aceito.
    private string? _fullName;
    private string? _email;
    private string? _phone;
    private string? _desiredPosition;
    private string? _professionalSummary;

    [Required(ErrorMessage = "Informe o nome completo.")]
    [MaxLength(CandidateRules.FullNameMaxLength, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    public string? FullName { get => _fullName; set => _fullName = value?.Trim(); }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [MaxLength(CandidateRules.EmailMaxLength, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    [RegularExpression(CandidateRules.EmailPattern, ErrorMessage = "Informe um e-mail válido.")]
    public string? Email { get => _email; set => _email = value?.Trim(); }

    [MaxLength(CandidateRules.PhoneMaxLength, ErrorMessage = "O telefone deve ter no máximo 30 caracteres.")]
    public string? Phone { get => _phone; set => _phone = value?.Trim(); }

    [MaxLength(CandidateRules.DesiredPositionMaxLength, ErrorMessage = "A área ou cargo deve ter no máximo 150 caracteres.")]
    public string? DesiredPosition { get => _desiredPosition; set => _desiredPosition = value?.Trim(); }

    [MaxLength(CandidateRules.ProfessionalSummaryMaxLength, ErrorMessage = "O resumo deve ter no máximo 2000 caracteres.")]
    public string? ProfessionalSummary { get => _professionalSummary; set => _professionalSummary = value?.Trim(); }
}
