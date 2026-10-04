using System.Text;
using Curriculos.Api.Services;

namespace Curriculos.Api.Tests;

public class PdfFileValidatorTests
{
    private static byte[] Pdf(int size = 100)
    {
        var bytes = new byte[size];
        Encoding.ASCII.GetBytes("%PDF-1.7").CopyTo(bytes, 0);
        return bytes;
    }

    [Fact]
    public void Valid_pdf_passes()
    {
        Assert.Null(PdfFileValidator.Validate("curriculo.pdf", Pdf()));
    }

    [Fact]
    public void Extension_is_case_insensitive()
    {
        Assert.Null(PdfFileValidator.Validate("CURRICULO.PDF", Pdf()));
    }

    [Fact]
    public void Missing_or_empty_file_is_rejected()
    {
        Assert.Equal(PdfValidationError.Missing, PdfFileValidator.Validate("curriculo.pdf", null));
        Assert.Equal(PdfValidationError.Missing, PdfFileValidator.Validate("curriculo.pdf", []));
    }

    [Fact]
    public void File_larger_than_5_mb_is_rejected()
    {
        var justAtLimit = Pdf((int)PdfFileValidator.MaxSizeBytes);
        var overLimit = Pdf((int)PdfFileValidator.MaxSizeBytes + 1);

        Assert.Null(PdfFileValidator.Validate("curriculo.pdf", justAtLimit));
        Assert.Equal(PdfValidationError.TooLarge, PdfFileValidator.Validate("curriculo.pdf", overLimit));
    }

    [Theory]
    [InlineData("curriculo.docx")]
    [InlineData("curriculo.txt")]
    [InlineData("curriculo")]
    public void Non_pdf_extension_is_rejected(string fileName)
    {
        Assert.Equal(PdfValidationError.NotPdf, PdfFileValidator.Validate(fileName, Pdf()));
    }

    [Fact]
    public void Pdf_extension_without_pdf_signature_is_rejected()
    {
        var text = Encoding.UTF8.GetBytes("isto é um arquivo de texto renomeado");

        Assert.Equal(PdfValidationError.NotPdf, PdfFileValidator.Validate("falso.pdf", text));
    }
}
