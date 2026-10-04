using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Curriculos.Api.Dtos;

namespace Curriculos.Api.Tests;

public class ResumesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private Task<HttpResponseMessage> Upload(byte[] content, string fileName = "curriculo.pdf")
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(file, "file", fileName);
        return _client.PostAsync("/api/resumes/parse", form);
    }

    private static async Task<string?> Detail(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("detail").GetString();
    }

    [Fact]
    public async Task Fictitious_resume_returns_extracted_fields()
    {
        var response = await Upload(PdfTextExtractorTests.Sample("curriculo-ficticio.pdf"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ResumeParseResponse>();
        Assert.Equal(new ResumeParseResponse("Mariana Exemplo Souza", "mariana.souza@example.com", "(41) 98765-4321"), result);
    }

    [Fact]
    public async Task Missing_file_returns_400()
    {
        // Formulário válido, mas sem o campo "file" (ex.: usuário enviou sem escolher arquivo).
        var form = new MultipartFormDataContent { { new StringContent("sem arquivo"), "observacao" } };
        var response = await _client.PostAsync("/api/resumes/parse", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Selecione um arquivo PDF.", await Detail(response));
    }

    [Fact]
    public async Task Text_file_renamed_to_pdf_returns_400()
    {
        var response = await Upload(PdfTextExtractorTests.Sample("nao-e-um-pdf.pdf"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("O arquivo enviado não é um PDF válido.", await Detail(response));
    }

    [Fact]
    public async Task File_slightly_above_5_mb_returns_413_with_clear_message()
    {
        var content = new byte[5 * 1024 * 1024 + 1];
        Encoding.ASCII.GetBytes("%PDF-1.7").CopyTo(content, 0);

        var response = await Upload(content);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("O PDF deve possuir no máximo 5 MB.", await Detail(response));
    }

    [Theory]
    [InlineData("curriculo-escaneado.pdf")]
    [InlineData(null)]
    public async Task Unreadable_pdf_returns_422(string? sample)
    {
        var content = sample is null
            ? Encoding.ASCII.GetBytes("%PDF-1.7\nestrutura corrompida")
            : PdfTextExtractorTests.Sample(sample);

        var response = await Upload(content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.StartsWith("Não foi possível extrair informações do currículo.", await Detail(response));
    }
}
