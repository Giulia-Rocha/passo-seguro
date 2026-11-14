# Matriz de rastreabilidade

Esta matriz relaciona os requisitos das três disciplinas às evidências do projeto. As execuções automatizadas possuem registros locais; a conclusão do vídeo, da revisão de acessibilidade no iPhone e da entrega foi confirmada pela equipe. O detalhamento está em [VALIDACAO.md](VALIDACAO.md).

Os materiais de apresentação estão no README: ficha da solução, integrantes/RMs, repositório, capturas das telas e vídeo. A equipe definiu o Google Drive como plataforma definitiva do vídeo e confirmou que o link está correto. O documento de Mobile especifica YouTube não listado; o uso do Drive é uma alternativa escolhida pela equipe, cuja aceitação cabe ao professor.

## SOA — 22 subitens

| ID | Requisito | Evidência |
|---|---|---|
| D1 | Entidades do negócio | Domain/Model.cs: Occurrence, Citizen, Category, StatusEntry |
| D2 | Quatro camadas | Projetos Domain, Application, Infrastructure e Api |
| D3 | Duas regras no domínio | Occurrence.Edit/ChangeStatus e PriorityCalculator; DomainTests |
| D4 | DTOs e validação | Application/Contracts.cs; ApiController e OccurrenceService |
| D5 | Relacionamento | Ocorrência → autor/categoria e ocorrência → histórico |
| B1 | Banco relacional real | compose.yaml: postgres:16-alpine; integração em container real |
| B2 | Variáveis e exemplo | .env.example, .gitignore, setup.ps1; segredos locais aleatórios |
| B3 | CRUD persistido | OccurrencesController; CrudActuallyPersistsAndReturnsContract; evidencias/api-smoke.json |
| B4 | FK persistida | InitialDomain migration e AppDbContext |
| B5 | Consulta extra | Filtros/ordenação/paginação em /occurrences; teste mine+difficulty+pageSize |
| M1 | Migrações sem auto-DDL | MigrateAsync no comando explícito --migrate; sem EnsureCreated |
| M2 | Duas migrações | InitialDomain, AddStatusTimelineIndex, ExplicitHistoryIdentifiers e AddOccurrenceAddress |
| M3 | Seed versionado | DemoSeed.Apply, dados fictícios dentro do polígono |
| M4 | Banco do zero | README e scripts/setup.ps1; integração cria banco descartável |
| O1 | OpenAPI acessível | /swagger/v1/swagger.json; teste IndicatorsHealthAndOpenApiAreAvailable |
| O2 | Resumos e tags | EndpointSummary/Tags em todas as ações |
| O3 | Schemas/exemplos | Contracts.cs e ApiExamples |
| O4 | Erros documentados | ApiDocumentation e ProblemDetails; testes de erros e contrato, incluindo saúde indisponível com resposta 503 |
| O5 | URL e print no README | URL documentada; captura real em docs/evidencias/swagger.png incluída no README |
| T1 | Testes unitários | DomainTests: 21 casos aprovados na execução registrada |
| T2 | HTTP status e corpo | ApiTests: 14 cenários aprovados com PostgreSQL 16, incluindo cadastro e contrato OpenAPI |
| T3 | Comando e print | npm run test:all; resultados TRX/Jest; captura real do relatório em docs/evidencias/tests.png |

## Mobile

| Critério | Implementação | Validação |
|---|---|---|
| Telas/navegação (2,5) | Expo 54.0.0, TypeScript, Expo Router, acesso, início, explorar, minhas, indicadores, perfil, registro, detalhe e rascunhos | Tipos/bundle aprovados; capturas reais no README e jornada no vídeo confirmada pela equipe |
| Consumo (2,0) | src/api.ts, useResource e Feedback; carregamento, erro, vazio, retry e renovação | Testes de componentes e HTTP; jornada integrada apresentada nas evidências da equipe |
| Banco (2,0) | PostgreSQL via API; cache e rascunhos SQLite em camada própria | Persistência real verificada nos testes; vídeo com salvamento e recuperação confirmado pela equipe |
| Acessibilidade (1,5) | Tema, textos legíveis, rótulos, botões acessíveis e lista alternativa ao mapa | Testes de componentes; revisão de VoiceOver, fontes ampliadas e legibilidade no iPhone confirmada pela equipe |
| Aderência (1,0) | Registro, dificuldade, consulta, mapa, histórico e indicadores de Bela Vista | Fluxos HTTP verificados; jornada principal e conteúdo do vídeo confirmados pela equipe |
| Documentação (1,0) | Ficha, integrantes/RMs, execução, justificativa do banco, fontes, prints e vídeo no README | Materiais presentes; vídeo no Drive por decisão da equipe, com aceitação dessa alternativa a confirmar com o professor |

Os testes mobile totalizam **33 casos aprovados** na última execução registrada. Os relatórios visuais iniciais são identificados como evidências históricas em VALIDACAO.md.

## C#

| Critério | Evidência |
|---|---|
| POO (3,0) | Encapsulamento em Occurrence/StatusEntry; enums, exceções e regras testadas |
| Web API (2,5) | Controllers REST, DTOs, autenticação/autorização e OpenAPI |
| EF Core + LINQ (1,5) | AppDbContext, Include/Where/OrderBy, migrações, FK e PostgreSQL real |
| Async/Await (1,0) | Casos de uso, controllers, IO e consultas assíncronas com CancellationToken |
| Qualidade (2,0) | Camadas, DI/interfaces, testes, CI, README e histórico Conventional Commits |

A equipe confirmou a disponibilização do repositório aos professores e o envio nas três disciplinas. A avaliação e a pontuação cabem aos professores.
