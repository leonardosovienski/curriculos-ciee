# Relatório de desenvolvimento

Este documento registra como a solução foi planejada e construída: decisões, alternativas descartadas, uso de IA, validação, dificuldades e limitações.

## 1. Planejamento

O desafio foi tratado em quatro etapas: **pesquisar → validar → decidir → estruturar**, antes de escrever qualquer código.

O princípio central veio do próprio enunciado: *preferir uma solução simples, funcional e que eu consiga compreender e evoluir*. Para cada tecnologia ou padrão, a pergunta foi: **isso aumenta a chance de a solução atender ao desafio ou só a deixa mais sofisticada?** Se a resposta era "só mais sofisticada", ficou de fora.

Ordem de prioridade usada nas decisões: funcionalidade → confiabilidade → facilidade de execução → documentação → testes relevantes → UX → sofisticação.

O trabalho foi dividido em três marcos, cada um com um critério objetivo de "pronto":

| Marco | Escopo | Critério de pronto |
|---|---|---|
| **M1** | Banco, migrations, API de candidatos, frontend com cadastro manual, listagem e detalhes | A partir de um banco vazio: cadastro manual salva, aparece na lista, detalhes abrem por URL e sobrevivem ao F5, erros de validação do backend aparecem no formulário |
| **M2** | Upload de PDF, validação do arquivo, extração, parser, preenchimento do mesmo formulário | Currículo fictício preenche nome, e-mail e telefone; PDF inválido, escaneado ou corrompido mostra mensagem e não bloqueia o cadastro manual |
| **M3** | Documentação, revisão e entrega | Testes passando, README validado em clone limpo, este relatório, repositório acessível |

O PDF ficou deliberadamente fora do M1: só depois de o fluxo manual funcionar de ponta a ponta o parser foi construído em cima dele.

## 2. Arquitetura

```text
React + Vite ──fetch("/api")──► proxy do Vite ──► ASP.NET Core Web API ──► EF Core ──► SQL Server (Docker)
```

- **Três peças, sem camadas extras.** O `CandidatesController` usa o `AppDbContext` diretamente: o CRUD é trivial e um *service* ou *repository* não teria responsabilidade própria.
- **Serviços só onde há responsabilidade real**, no fluxo de PDF: `PdfFileValidator` (regras do arquivo), `PdfTextExtractor` (I/O com o PdfPig) e `ResumeTextParser` (função pura sobre texto). Separar extração de parsing permite testar todas as heurísticas sem precisar de um PDF para cada caso.
- **O endpoint de parse não grava nada.** Ele devolve os dados encontrados; o cadastro continua sendo um único `POST /api/candidates`, com as mesmas validações nos dois fluxos.
- **Um formulário só.** A importação apenas preenche o estado do mesmo componente `CandidateForm`, e só em campos vazios, para nunca apagar o que a pessoa digitou.

## 3. Decisões técnicas

| Decisão | Escolha | Alternativas consideradas | Por que não |
|---|---|---|---|
| Runtime | .NET 10 (LTS) | .NET 8, .NET 9 | Ambos perdem suporte em 10/11/2026; começar neles seria escolher uma versão prestes a expirar |
| Backend | Controllers + DataAnnotations | Minimal APIs, FluentValidation | `[ApiController]` já devolve `400` com erros por campo; FluentValidation seria uma dependência a mais para cinco campos |
| Persistência | EF Core direto no controller | Repository, Unit of Work | O `DbContext` já cumpre esse papel; a camada seria artificial |
| Schema | Migrations do EF aplicadas na inicialização | `schema.sql`, comando manual | Fonte única do schema e nenhum passo extra para quem avalia. Em produção, migrations iriam por pipeline |
| Banco local | SQL Server 2022 em Docker, porta 14330 | LocalDB, SQL Server instalado, tudo em Docker | LocalDB é só Windows; instalação manual é frágil; containerizar tudo exigiria Dockerfiles sem ganho. A porta 14330 evita conflito com um SQL Server já instalado |
| Prontidão do banco | Healthcheck + `--wait` + retry de 10×3 s na migration | Só retry global (`EnableRetryOnFailure`), Polly | Retry global atrasa todas as respostas de erro; Polly seria dependência a mais |
| Identificador | `int IDENTITY` | GUID | URLs legíveis (`/candidates/7`) |
| Data de cadastro | `DateTimeOffset` definido pelo backend | `DateTime` | Com `DateTime`, o valor volta sem fuso e o navegador mostra a hora errada |
| E-mail duplicado | Permitido | Índice único | O enunciado não define unicidade; seria uma regra inventada |
| Validação de e-mail | Mesma regex simples no backend e no frontend | `[EmailAddress]`, regex completa da RFC | `[EmailAddress]` aceita `teste@teste`; a RFC completa é desproporcional; a mesma regex dos dois lados evita divergência |
| Erros | ProblemDetails nativo | Formato próprio | Padrão da plataforma, sem código de infraestrutura |
| Integração em dev | Proxy do Vite para `/api` | CORS, URL da API em variável | Nenhuma configuração de CORS e URLs relativas no frontend |
| Biblioteca de PDF | PdfPig 0.1.16 (Apache 2.0) | iText, IronPDF, PDFsharp | iText é AGPL/comercial; IronPDF é comercial; PDFsharp é voltado à geração, não à extração |
| Parser | Regras determinísticas | OCR, IA, NLP | Fora do escopo e do prazo; regras são explicáveis e testáveis |
| Status HTTP do arquivo | `400` inválido, `413` grande, `422` ilegível | `415` | `415` trata do tipo da requisição (que é `multipart`, correto), não do conteúdo do arquivo |
| Limite da requisição | 6 MB no endpoint de parse | Padrão do servidor (~28,6 MB) | O padrão já deixaria o arquivo chegar à validação; o limite de 6 MB é uma **redução** protetiva, ainda acima dos 5 MB para que a mensagem clara apareça |
| Frontend | React + Vite + JavaScript, `fetch`, estado do React | TypeScript, Axios, React Hook Form, Zod | Cinco campos e quatro endpoints não justificam as dependências |
| Rotas | React Router (2 rotas) | Troca de tela por estado | A tela de detalhes tem URL própria e funciona com F5 |
| Configuração | Senha fictícia de ambiente local versionada como padrão | `user-secrets` obrigatório | O avaliador executa sem nenhum passo de configuração. Não há credencial real no repositório, e os valores podem ser sobrescritos por `.env` e variável de ambiente |

### Decisões de escopo

- Sem edição, exclusão, autenticação ou paginação: não foram solicitadas.
- O PDF e o texto extraído não são armazenados: servem apenas para preencher o formulário.

## 4. Uso de IA

### Modelos utilizados

Usei IA como apoio para transformar o enunciado em um plano, implementar a solução e revisar os resultados. O Claude ajudou principalmente na construção do código e dos testes; o ChatGPT, na idealização e revisão das escolhas. Minha participação foi definir e revisar o escopo, executar a aplicação localmente, observar os resultados e levar os problemas encontrados de volta para correção. A implementação foi assistida por IA, e essa participação está detalhada abaixo.

- **Claude (Anthropic)**, no claude.ai, foi o assistente principal: pesquisa e validação de versões, licenças e comportamento dos frameworks; plano técnico; implementação do código e dos testes a partir do plano aprovado; e diagnóstico dos problemas na execução local.
- **ChatGPT (OpenAI)**, usado para revisar o plano e o M1 de forma independente. As revisões foram trazidas de volta ao Claude para comparação, e cada divergência foi decidida com evidência.
- **Codex (OpenAI)**, usado na revisão final dos requisitos, testes locais, conferência da persistência no SQL Server e correção da interface em telas pequenas. A participação incluiu leitura do código e execução de ferramentas, não apenas sugestões em conversa.

Os identificadores exatos dos modelos das sessões anteriores não foram registrados neste documento. Os nomes acima identificam as ferramentas utilizadas; não representam uma afirmação sobre versões específicas dos modelos.

### Como a IA foi usada

1. **Pesquisa e plano.** Um prompt estruturado definiu papel, prazo, regras de decisão, o enunciado como fonte da verdade, hipóteses a validar e o formato da resposta. Trecho:

   > "Seu objetivo é me ajudar a escolher a solução com maior probabilidade de aprovação dentro do prazo disponível, e não a solução tecnicamente mais sofisticada. [...] Para qualquer tecnologia, padrão, biblioteca ou melhoria, pergunte: isso aumenta materialmente minha chance de aprovação neste desafio ou apenas deixa a solução mais sofisticada?"

2. **Revisão cruzada.** Planos e entregas foram comparados entre os dois assistentes com prompts curtos ("analisa"), e as divergências foram resolvidas uma a uma.
3. **Implementação.** Com o plano congelado, o Claude implementou M1 e M2 num ambiente com .NET 10 e Node, rodando os testes e verificando a interface num navegador automatizado antes de cada entrega.
4. **Validação local.** Eu executei tudo na minha máquina (Windows, Docker Desktop, SQL Server real) seguindo o README, testei os fluxos no navegador e reportei os resultados e erros.

### Respostas aproveitadas

- A estrutura geral (três peças, quatro endpoints, uma tabela) e a divisão em marcos.
- A escolha do PdfPig com `ContentOrderTextExtractor`, confirmada na documentação oficial da biblioteca.
- A estratégia de prontidão do banco (healthcheck, `--wait`, retry) e a porta 14330.
- Os testes de API com WebApplicationFactory, que verificam o contrato HTTP sem precisar de SQL Server.

### Respostas corrigidas

- **Limite HTTP.** A hipótese inicial era aumentar o limite do servidor para que arquivos pouco acima de 5 MB chegassem à validação. A pesquisa mostrou que o padrão do Kestrel já é ~28,6 MB, então o ajuste correto foi o oposto: reduzir para 6 MB como proteção.
- **Versão do .NET.** O plano partia do .NET 8; a verificação do ciclo de suporte levou ao .NET 10.
- **Datas de suporte.** Uma revisão indicou 11/11/2026 para o fim do .NET 8/9; a checagem nas fontes confirmou 10/11/2026 (as datas caem na *Patch Tuesday*).
- **Heurística de nome.** Durante os testes, a frase "Desenvolvimento de APIs em .NET" foi aceita como nome. A regra passou a exigir inicial maiúscula em cada palavra (exceto partículas como "da" e "dos"), com um teste para o caso.
- **Trim antes da validação.** Uma revisão apontou que o backend validava o valor antes do *trim*, então `" ana@example.com "` era rejeitado pela API. O *trim* passou a ocorrer na desserialização, com teste.
- **Autor dos commits.** O primeiro comando sugerido para trocar o autor (`--reset-author`) também redefiniria as datas dos commits; foi substituído por `--author`, que preserva as datas reais.

### Respostas adaptadas

- **Parser.** O plano inicial processava só as duas primeiras páginas, descartava textos com menos de ~20 letras, convertia nomes em CAIXA ALTA para Title Case e o e-mail para minúsculas. Após revisão, o texto passou a ser extraído de todas as páginas, apenas PDFs sem texto geram erro, e os valores são devolvidos como encontrados: o parser extrai, não reescreve.
- **Telefone.** Sequências só de dígitos passaram a exigir um rótulo ("Telefone", "Celular") na mesma linha, para não confundir com CPF.
- **React Router.** Chegou a ser removido do plano em favor de troca de tela por estado e voltou, porque a tela de detalhes com URL própria (e funcionando com F5) tem valor visível para quem avalia.

### Respostas descartadas

- Camadas e padrões sem responsabilidade concreta: Clean Architecture, Repository, MediatR, `CandidateService`, AutoMapper.
- Bibliotecas de frontend sem necessidade: Axios, React Hook Form, Zod/Yup.
- `user-secrets` e `.env` obrigatórios para o backend (mais passos e mais chance de erro na execução).
- Mover o manifest do `dotnet-ef` para `.config/`: o SDK encontra o arquivo na raiz, o que foi confirmado com `dotnet tool restore` num clone limpo.
- OCR, IA ou serviços externos no parser.

### Histórico de commits

Os commits seguem a ordem real de construção (M1, depois M2, depois documentação), mas partes do M1 foram implementadas em blocos com assistência de IA e depois separadas em commits lógicos, por isso alguns commits têm horários muito próximos. As datas não foram alteradas.

## 5. Validação e testes

**Automatizados** (62 testes, `dotnet test`, sem SQL Server):

- Validação do cadastro: obrigatoriedade, formato de e-mail, tamanhos e *trim*.
- Validação do arquivo: ausente, limite de 5 MB, extensão e assinatura `%PDF-`.
- Parser: e-mail, formatos de telefone, números que não são telefone (CPF, CEP, datas), heurística de nome, campos ausentes.
- Extração: currículo fictício de ponta a ponta, PDF escaneado sem texto e PDF corrompido.
- API (WebApplicationFactory): cadastro, listagem, detalhes, `400`, `404`, e-mail duplicado e importação com `200`, `400`, `413` e `422`. O banco é substituído pelo provider InMemory **apenas nesses testes**.

**Manuais**, na minha máquina, com SQL Server real:

- Banco criado do zero pela migration; dados preservados ao reiniciar a API e ao atualizar o projeto.
- Cadastro manual, validações no formulário, e-mail duplicado, detalhes com F5 e candidato inexistente.
- Importação do currículo fictício (nome, e-mail e telefone preenchidos, corrigidos e salvos).
- PDF escaneado mostrando o aviso e o cadastro manual seguindo normalmente.

## 6. Dificuldades

- **Disco cheio no ambiente local.** Na primeira execução, o download da imagem do SQL Server falhou com `read-only file system`. A causa era o disco C: com menos de 1 MB livre, o que deixou o disco virtual do Docker em modo somente leitura. Resolvido liberando espaço e reiniciando o WSL e o Docker Desktop. Ficou registrado no *troubleshooting* do README.
- **Verificação da interface sem o backend real no ambiente da IA.** O ambiente onde o código foi gerado não permitia subir a API .NET atrás do Vite por limite de memória. Até o M1, a interface foi verificada com um servidor simulado fiel ao contrato da API, que por sua vez é coberto pelos testes de integração. No M2, a verificação já usou o parser .NET real. A validação com SQL Server foi feita na minha máquina.
- **Detalhes de framework descobertos na prática:** o ASP.NET devolve as chaves de erro de validação em PascalCase (`FullName`), normalizadas no frontend; o React Router 8 exige Node 22.22 ou superior; o template de testes do .NET 10 ainda usa xUnit 2.9.3.

## 7. Limitações

- O parser é heurístico e não cobre todos os layouts. As limitações detalhadas estão no README (seção "Limitações do parser"): sem OCR, colunas podem desordenar o texto, nomes em imagem ou fora das primeiras linhas não são encontrados, telefones estrangeiros não são reconhecidos.
- Sem edição, exclusão, busca ou paginação na listagem.
- A configuração é voltada a execução local. Em produção seriam necessários: credenciais por cofre de segredos, migrations por pipeline, usuário do banco sem privilégios de administrador e o frontend servido como build estático.

## 8. Melhorias futuras

- OCR opcional para PDFs escaneados.
- Extração de área de interesse e resumo profissional.
- Busca e paginação na listagem; edição e exclusão, se o negócio pedir.
- Testes de integração contra SQL Server real (por exemplo, com Testcontainers).
- Testes de frontend para o fluxo de importação.

## 9. Tempo aproximado

| Etapa | Tempo |
|---|---|
| Pesquisa, validação e plano técnico (incluindo revisões cruzadas) | ~6 h |
| Implementação assistida do M1 e do M2 | ~3 h |
| Execução local, testes manuais e resolução de problemas de ambiente | ~3 h |
| Documentação e revisão final | ~1 h |
| **Total** | **~13 h** |

## 10. Revisão final assistida em 04/10/2026

A revisão partiu de pedidos para testar a aplicação local, comparar a entrega com o enunciado e corrigir os pontos encontrados. As verificações e a alteração abaixo foram executadas pelo Codex no ambiente local, complementando a validação manual descrita anteriormente.

- Cadastro manual pelo navegador, validação de campos obrigatórios e e-mail inválido, limpeza do formulário, listagem e detalhes após recarregar a página.
- Consulta direta ao SQL Server confirmou o registro fictício `Teste QA Codex`, criado pelo navegador, e a migration `20261003232314_InitialCreate` aplicada. Isso confirma a persistência desse fluxo; não equivale a um teste de recuperação após reiniciar o banco.
- Execução local de `dotnet test --nologo -m:1`: 62 testes aprovados, nenhum reprovado ou ignorado. Os testes de API continuam usando o provider InMemory.
- Execução local de `npm run build`: concluída com sucesso.
- Correção de responsividade: o painel não podia encolher até a largura disponível e o bloco de importação mantinha o botão ao lado do texto. A correção permite encolher os painéis e empilha o bloco de importação em telas pequenas. Conferência no navegador em larguras de 320 e 390 px, sem transbordamento horizontal da página.

A validação da leitura de PDF nesta revisão foi feita pelos testes automatizados, que incluem extração do arquivo fictício e erros de arquivo inválido, tamanho e leitura. O fluxo completo de upload e correção no navegador ainda não foi repetido nesta revisão. Não foi acrescentada uma estimativa de tempo desta etapa ao total anterior, pois ela não foi medida.
