namespace Curriculos.Api.Models;

/// <summary>
/// Limites e regras do cadastro, compartilhados entre validação (DTO) e mapeamento do banco.
/// O frontend repete os mesmos valores em src/validation.js.
/// </summary>
public static class CandidateRules
{
    public const int FullNameMaxLength = 150;
    public const int EmailMaxLength = 254;
    public const int PhoneMaxLength = 30;
    public const int DesiredPositionMaxLength = 150;
    public const int ProfessionalSummaryMaxLength = 2000;

    // Regra proporcional ao desafio: algo@dominio.tld, sem espaços.
    // A mesma expressão é usada no frontend para evitar divergência.
    public const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
}
