using System.Text;

namespace Curriculos.Api.Services;

public enum PdfValidationError
{
    Missing,
    TooLarge,
    NotPdf
}

/// <summary>
/// Validação do arquivo enviado para importação.
/// Extensão e assinatura são verificadas no próprio conteúdo; o Content-Type
/// informado pelo navegador não é usado para decidir, pois é controlado pelo cliente.
/// </summary>
public static class PdfFileValidator
{
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    // Pela especificação, o cabeçalho "%PDF-" pode ser precedido por alguns bytes;
    // leitores comuns aceitam o cabeçalho dentro do primeiro 1 KB.
    private const int SignatureSearchWindow = 1024;
    private static readonly byte[] Signature = Encoding.ASCII.GetBytes("%PDF-");

    public static PdfValidationError? Validate(string? fileName, byte[]? content)
    {
        if (content is null || content.Length == 0)
            return PdfValidationError.Missing;

        if (content.Length > MaxSizeBytes)
            return PdfValidationError.TooLarge;

        if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            return PdfValidationError.NotPdf;

        if (!HasPdfSignature(content))
            return PdfValidationError.NotPdf;

        return null;
    }

    private static bool HasPdfSignature(byte[] content)
    {
        var window = content.AsSpan(0, Math.Min(content.Length, SignatureSearchWindow));
        return window.IndexOf(Signature) >= 0;
    }
}
