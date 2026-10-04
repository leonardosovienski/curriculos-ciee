using Curriculos.Api.Services;

namespace Curriculos.Api.Tests;

public class ResumeTextParserTests
{
    [Fact]
    public void Finds_email_phone_and_name_in_typical_header()
    {
        var text = """
            Mariana Exemplo Souza
            Desenvolvedora de Sistemas
            mariana.souza@example.com | (41) 98765-4321 | linkedin.com/in/mariana
            """;

        var result = ResumeTextParser.Parse(text);

        Assert.Equal("Mariana Exemplo Souza", result.FullName);
        Assert.Equal("mariana.souza@example.com", result.Email);
        Assert.Equal("(41) 98765-4321", result.Phone);
    }

    [Fact]
    public void Missing_fields_return_null()
    {
        var result = ResumeTextParser.Parse("Experiência\nDesenvolvimento de APIs em .NET");

        Assert.Null(result.FullName);
        Assert.Null(result.Email);
        Assert.Null(result.Phone);
    }

    [Fact]
    public void Email_is_returned_as_found()
    {
        var result = ResumeTextParser.Parse("Contato: Mariana.Souza@Example.com");

        Assert.Equal("Mariana.Souza@Example.com", result.Email);
    }

    [Theory]
    [InlineData("(41) 98765-4321", "(41) 98765-4321")]
    [InlineData("41 98765-4321", "(41) 98765-4321")]
    [InlineData("+55 (41) 98765-4321", "(41) 98765-4321")]
    [InlineData("+55 41 98765 4321", "(41) 98765-4321")]
    [InlineData("+5541987654321", "(41) 98765-4321")]
    [InlineData("(41) 3333-4444", "(41) 3333-4444")]
    [InlineData("Celular: 41987654321", "(41) 98765-4321")]
    public void Recognizes_brazilian_phone_formats(string line, string expected)
    {
        Assert.Equal(expected, ResumeTextParser.Parse(line).Phone);
    }

    [Theory]
    [InlineData("CPF: 123.456.789-09")]
    [InlineData("CPF 12345678909")]
    [InlineData("12345678909")]
    [InlineData("CEP 80000-000")]
    [InlineData("Período: 03/2024 – 02/2025")]
    [InlineData("(41) 88765-4321")]
    public void Does_not_confuse_other_numbers_with_phone(string line)
    {
        Assert.Null(ResumeTextParser.Parse(line).Phone);
    }

    [Fact]
    public void Skips_section_titles_and_job_titles_when_looking_for_name()
    {
        var text = """
            Currículo
            Desenvolvedora de Sistemas
            Mariana Exemplo Souza
            """;

        Assert.Equal("Mariana Exemplo Souza", ResumeTextParser.Parse(text).FullName);
    }

    [Fact]
    public void Accepts_lowercase_particles_in_names()
    {
        Assert.Equal("João Pedro da Silva dos Santos", ResumeTextParser.Parse("João Pedro da Silva dos Santos").FullName);
    }

    [Fact]
    public void Name_in_uppercase_is_kept_as_found()
    {
        Assert.Equal("MARIANA EXEMPLO SOUZA", ResumeTextParser.Parse("MARIANA EXEMPLO SOUZA").FullName);
    }

    [Theory]
    [InlineData("Mariana")]                                   // uma palavra só: confiança baixa
    [InlineData("Rua das Araucárias, 123 – Curitiba/PR")]     // endereço
    [InlineData("linkedin.com/in/mariana-souza")]
    [InlineData("Desenvolvimento de APIs em .NET")]          // frase comum, não nome
    public void Returns_null_name_when_confidence_is_low(string line)
    {
        Assert.Null(ResumeTextParser.Parse(line).FullName);
    }
}
