# Arquivos de exemplo

Todos os dados são fictícios.

| Arquivo | Uso | Resultado esperado na importação |
|---|---|---|
| `curriculo-ficticio.pdf` | Currículo com camada de texto | Preenche nome, e-mail e telefone |
| `curriculo-escaneado.pdf` | Mesmo currículo salvo apenas como imagem (sem texto) | Aviso de que não foi possível extrair informações; formulário continua disponível |
| `nao-e-um-pdf.pdf` | Arquivo de texto renomeado para `.pdf` | "O arquivo enviado não é um PDF válido." |

Para testar o limite de 5 MB, use qualquer PDF maior que isso (não incluído no repositório para não aumentar o tamanho do clone).
