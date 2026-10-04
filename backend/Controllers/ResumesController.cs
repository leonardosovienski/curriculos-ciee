using Curriculos.Api.Dtos;
using Curriculos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Curriculos.Api.Controllers;

[ApiController]
[Route("api/resumes")]
public class ResumesController(ILogger<ResumesController> logger) : ControllerBase
{
    private const string NotPdfMessage = "O arquivo enviado não é um PDF válido.";
    private const string TooLargeMessage = "O PDF deve possuir no máximo 5 MB.";
    private const string UnreadableMessage =
        "Não foi possível extrair informações do currículo. Você pode preencher o formulário manualmente.";

    // Lê o PDF, tenta identificar nome, e-mail e telefone e devolve o resultado.
    // Não grava nada: o cadastro continua sendo feito por POST /api/candidates, com o mesmo formulário.
    //
    // O limite da requisição (6 MB) fica um pouco acima do limite funcional (5 MB) para que arquivos
    // levemente maiores cheguem à validação abaixo e recebam a mensagem clara. O padrão do servidor
    // (~28,6 MB) é reduzido aqui apenas como proteção.
    [HttpPost("parse")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType<ResumeParseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ResumeParseResponse>> Parse(IFormFile? file)
    {
        var content = file is null ? null : await ReadAllBytesAsync(file);

        switch (PdfFileValidator.Validate(file?.FileName, content))
        {
            case PdfValidationError.Missing:
                return Problem(title: "Arquivo ausente", detail: "Selecione um arquivo PDF.", statusCode: StatusCodes.Status400BadRequest);
            case PdfValidationError.TooLarge:
                return Problem(title: "Arquivo muito grande", detail: TooLargeMessage, statusCode: StatusCodes.Status413PayloadTooLarge);
            case PdfValidationError.NotPdf:
                logger.LogInformation("Arquivo rejeitado: {FileName} ({ContentType})", file?.FileName, file?.ContentType);
                return Problem(title: "Arquivo inválido", detail: NotPdfMessage, statusCode: StatusCodes.Status400BadRequest);
        }

        string text;
        try
        {
            text = PdfTextExtractor.ExtractText(content!);
        }
        catch (PdfReadException ex)
        {
            logger.LogWarning(ex, "Falha ao ler o PDF {FileName}", file!.FileName);
            return Unreadable();
        }

        // PDF sem camada de texto (ex.: escaneado): não há o que extrair sem OCR.
        if (string.IsNullOrWhiteSpace(text))
        {
            return Unreadable();
        }

        var result = ResumeTextParser.Parse(text);
        return new ResumeParseResponse(result.FullName, result.Email, result.Phone);
    }

    private ObjectResult Unreadable() =>
        Problem(title: "PDF não processável", detail: UnreadableMessage, statusCode: StatusCodes.Status422UnprocessableEntity);

    private static async Task<byte[]> ReadAllBytesAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        return stream.ToArray();
    }
}
