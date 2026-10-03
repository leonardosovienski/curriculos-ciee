using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Curriculos.Api.Dtos;

namespace Curriculos.Api.Tests;

public class CandidatesApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CandidatesApiTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Created_candidate_appears_in_list_and_details()
    {
        var response = await _client.PostAsJsonAsync("/api/candidates", new
        {
            fullName = "  Mariana Exemplo Souza  ",
            email = "mariana.souza@example.com",
            phone = "(41) 98765-4321",
            desiredPosition = "   ",
            professionalSummary = "Experiência com .NET e React."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CandidateResponse>();
        Assert.NotNull(created);
        Assert.Equal("Mariana Exemplo Souza", created.FullName);   // trim aplicado
        Assert.Null(created.DesiredPosition);                       // opcional vazio vira null
        Assert.Equal($"/api/candidates/{created.Id}", response.Headers.Location?.AbsolutePath);

        var list = await _client.GetFromJsonAsync<List<CandidateListItem>>("/api/candidates");
        Assert.Contains(list!, c => c.Id == created.Id);

        var details = await _client.GetFromJsonAsync<CandidateResponse>($"/api/candidates/{created.Id}");
        Assert.Equal(created, details);
    }

    [Fact]
    public async Task Invalid_candidate_returns_400_with_errors_per_field()
    {
        var response = await _client.PostAsJsonAsync("/api/candidates", new
        {
            fullName = "   ",
            email = "mariana@example"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = problem.RootElement.GetProperty("errors")
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value[0].GetString(), StringComparer.OrdinalIgnoreCase);

        Assert.Equal("Informe o nome completo.", errors["fullName"]);
        Assert.Equal("Informe um e-mail válido.", errors["email"]);
    }

    [Fact]
    public async Task Unknown_candidate_returns_404_problem_details()
    {
        var response = await _client.GetAsync("/api/candidates/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Duplicate_email_is_allowed()
    {
        var body = new { fullName = "Pessoa Repetida", email = "repetida@example.com" };

        var first = await _client.PostAsJsonAsync("/api/candidates", body);
        var second = await _client.PostAsJsonAsync("/api/candidates", body);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }

    [Fact]
    public async Task List_returns_most_recent_first()
    {
        var older = await (await _client.PostAsJsonAsync("/api/candidates",
            new { fullName = "Primeiro Cadastro", email = "primeiro@example.com" })).Content.ReadFromJsonAsync<CandidateResponse>();
        var newer = await (await _client.PostAsJsonAsync("/api/candidates",
            new { fullName = "Segundo Cadastro", email = "segundo@example.com" })).Content.ReadFromJsonAsync<CandidateResponse>();

        var list = await _client.GetFromJsonAsync<List<CandidateListItem>>("/api/candidates");
        var ids = list!.Select(c => c.Id).ToList();

        Assert.True(ids.IndexOf(newer!.Id) < ids.IndexOf(older!.Id));
    }
}
