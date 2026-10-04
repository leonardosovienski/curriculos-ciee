using System.Text;
using Curriculos.Api.Services;

namespace Curriculos.Api.Tests;

public class PdfTextExtractorTests
{
    internal static byte[] Sample(string fileName) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "samples", fileName));

    [Fact]
    public void Fictitious_resume_is_extracted_and_parsed()
    {
        var text = PdfTextExtractor.ExtractText(Sample("curriculo-ficticio.pdf"));
        var result = ResumeTextParser.Parse(text);

        Assert.Equal("Mariana Exemplo Souza", result.FullName);
        Assert.Equal("mariana.souza@example.com", result.Email);
        Assert.Equal("(41) 98765-4321", result.Phone);
    }

    [Fact]
    public void Scanned_pdf_without_text_layer_returns_empty_text()
    {
        var text = PdfTextExtractor.ExtractText(Sample("curriculo-escaneado.pdf"));

        Assert.True(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public void Corrupted_pdf_throws_controlled_exception()
    {
        var corrupted = Encoding.ASCII.GetBytes("%PDF-1.7\nconteúdo corrompido sem estrutura de PDF");

        Assert.Throws<PdfReadException>(() => PdfTextExtractor.ExtractText(corrupted));
    }
}
