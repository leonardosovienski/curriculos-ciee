using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Curriculos.Api.Services;

/// <summary>Falha ao abrir ou ler o PDF (corrompido, protegido por senha, estrutura inválida).</summary>
public class PdfReadException(string message, Exception innerException) : Exception(message, innerException);

public static class PdfTextExtractor
{
    /// <summary>
    /// Extrai o texto de todas as páginas, em ordem de leitura aproximada.
    /// Usa ContentOrderTextExtractor, recomendado pelo PdfPig em vez de page.Text,
    /// que segue a ordem interna do arquivo e costuma embaralhar o texto.
    /// Um PDF sem camada de texto (escaneado) retorna string vazia.
    /// </summary>
    public static string ExtractText(byte[] pdf)
    {
        try
        {
            using var document = PdfDocument.Open(pdf);
            var text = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                text.AppendLine(ContentOrderTextExtractor.GetText(page));
            }

            return text.ToString();
        }
        catch (Exception ex)
        {
            throw new PdfReadException("Não foi possível ler o conteúdo do PDF.", ex);
        }
    }
}
