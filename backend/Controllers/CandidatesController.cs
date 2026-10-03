using Curriculos.Api.Data;
using Curriculos.Api.Dtos;
using Curriculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Controllers;

[ApiController]
[Route("api/candidates")]
public class CandidatesController(AppDbContext db) : ControllerBase
{
    // As validações do CandidateRequest são aplicadas automaticamente pelo [ApiController]:
    // se falharem, a API responde 400 com ValidationProblemDetails antes de entrar na action.
    [HttpPost]
    [ProducesResponseType<CandidateResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CandidateResponse>> Create(CandidateRequest request)
    {
        var candidate = new Candidate
        {
            FullName = request.FullName!.Trim(),
            Email = request.Email!.Trim(),
            Phone = Clean(request.Phone),
            DesiredPosition = Clean(request.DesiredPosition),
            ProfessionalSummary = Clean(request.ProfessionalSummary),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = candidate.Id }, CandidateResponse.From(candidate));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CandidateListItem>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<CandidateListItem>> List() =>
        await db.Candidates
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Select(c => new CandidateListItem(c.Id, c.FullName, c.Email, c.DesiredPosition, c.CreatedAt))
            .ToListAsync();

    [HttpGet("{id:int}")]
    [ProducesResponseType<CandidateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CandidateResponse>> GetById(int id)
    {
        var candidate = await db.Candidates.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        return candidate is null
            ? Problem(title: "Candidato não encontrado.", detail: "Candidato não encontrado.", statusCode: StatusCodes.Status404NotFound)
            : CandidateResponse.From(candidate);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
