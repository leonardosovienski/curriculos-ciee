using Curriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Candidate> Candidates => Set<Candidate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var candidate = modelBuilder.Entity<Candidate>();

        candidate.ToTable("Candidates");
        candidate.HasKey(c => c.Id);

        candidate.Property(c => c.FullName).IsRequired().HasMaxLength(CandidateRules.FullNameMaxLength);
        candidate.Property(c => c.Email).IsRequired().HasMaxLength(CandidateRules.EmailMaxLength);
        candidate.Property(c => c.Phone).HasMaxLength(CandidateRules.PhoneMaxLength);
        candidate.Property(c => c.DesiredPosition).HasMaxLength(CandidateRules.DesiredPositionMaxLength);
        candidate.Property(c => c.ProfessionalSummary).HasMaxLength(CandidateRules.ProfessionalSummaryMaxLength);
        candidate.Property(c => c.CreatedAt).IsRequired();

        // E-mail duplicado é permitido de propósito: o enunciado não define unicidade.
    }
}
