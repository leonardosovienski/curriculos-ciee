using System.ComponentModel.DataAnnotations;
using Curriculos.Api.Dtos;

namespace Curriculos.Api.Tests;

/// <summary>
/// Regras autoritativas do cadastro (as mesmas usadas no fluxo manual e no fluxo com PDF).
/// </summary>
public class CandidateValidationTests
{
    private static List<ValidationResult> Validate(CandidateRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    private static CandidateRequest Valid() => new()
    {
        FullName = "Mariana Exemplo Souza",
        Email = "mariana.souza@example.com"
    };

    [Fact]
    public void Valid_request_with_only_required_fields_passes()
    {
        Assert.Empty(Validate(Valid()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Full_name_is_required(string? fullName)
    {
        var request = Valid();
        request.FullName = fullName;

        var error = Assert.Single(Validate(request));
        Assert.Contains(nameof(CandidateRequest.FullName), error.MemberNames);
        Assert.Equal("Informe o nome completo.", error.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Email_is_required(string? email)
    {
        var request = Valid();
        request.Email = email;

        var error = Assert.Single(Validate(request));
        Assert.Contains(nameof(CandidateRequest.Email), error.MemberNames);
        Assert.Equal("Informe o e-mail.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("mariana")]
    [InlineData("mariana@example")]
    [InlineData("@example.com")]
    [InlineData("mariana souza@example.com")]
    [InlineData("mariana@@example.com")]
    public void Invalid_email_format_is_rejected(string email)
    {
        var request = Valid();
        request.Email = email;

        var error = Assert.Single(Validate(request));
        Assert.Equal("Informe um e-mail válido.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("mariana@example.com")]
    [InlineData("mariana.souza+vagas@empresa.com.br")]
    [InlineData("M.Souza@Example.org")]
    public void Common_email_formats_are_accepted(string email)
    {
        var request = Valid();
        request.Email = email;

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void Fields_longer_than_limits_are_rejected()
    {
        var request = Valid();
        request.FullName = new string('a', 151);
        request.ProfessionalSummary = new string('a', 2001);

        var errors = Validate(request);

        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void Values_are_trimmed_before_validation()
    {
        var request = Valid();
        request.Email = "  mariana.souza@example.com  ";
        request.FullName = "  Mariana Exemplo Souza ";

        Assert.Empty(Validate(request));
        Assert.Equal("mariana.souza@example.com", request.Email);
        Assert.Equal("Mariana Exemplo Souza", request.FullName);
    }
}
