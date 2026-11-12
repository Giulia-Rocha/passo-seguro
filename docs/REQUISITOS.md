# Matriz de rastreabilidade

Implementado significa que existe código; verificado indica execução registrada. A validação no iPhone e os materiais de submissão são tratados separadamente em VALIDACAO.md.

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
| M2 | Duas migrações | InitialDomain + AddStatusTimelineIndex; terceira registra mapeamento correto do histórico |
| M3 | Seed versionado | DemoSeed.Apply, dados fictícios dentro do polígono |
| M4 | Banco do zero | README e scripts/setup.ps1; integração cria banco descartável |
| O1 | OpenAPI acessível | /swagger/v1/swagger.json; teste IndicatorsHealthAndOpenApiAreAvailable |
| O2 | Resumos e tags | EndpointSummary/Tags em todas as ações |
| O3 | Schemas/exemplos | Contracts.cs e ApiExamples |
| O4 | Erros documentados | ApiDocumentation; ProblemDetails; testes 400/401/403/404/409/415/422 |
| O5 | URL e print no README | URL documentada; captura real em docs/evidencias/swagger.png incluída no README |
| T1 | Testes unitários | DomainTests: 20 casos executados |
| T2 | HTTP status e corpo | ApiTests: 8 cenários executados em PostgreSQL 16 |
| T3 | Comando e print | npm run test:all; resultados TRX/Jest; captura real do relatório em docs/evidencias/tests.png |

## Mobile

| Critério | Implementação | Validação |
|---|---|---|
| Telas/navegação (2,5) | Expo 54.0.0, TypeScript, Expo Router, acesso, início, explorar, minhas, indicadores, perfil, registro, detalhe, rascunhos | Tipos/bundle e testes; navegação física pendente |
| Consumo (2,0) | src/api.ts, useResource, Feedback; carregamento, erro, vazio, retry e renovação | Testes de componentes/HTTP; integração física pendente |
| Banco (2,0) | PostgreSQL via API; cache e rascunhos SQLite em camada própria | Backend real verificado; SQLite nativo requer iPhone |
| Acessibilidade (1,5) | Tema, textos legíveis, rótulos, botões acessíveis, lista alternativa ao mapa | Testes de componentes; VoiceOver/fonte ampliada pendentes |
| Aderência (1,0) | Registro, dificuldade, consulta, mapa, histórico e indicadores de Bela Vista | Fluxos HTTP verificados; jornada no aparelho pendente |
| Documentação (1,0) | Ficha, setup, fontes, banco e roteiro | Integrantes, prints físicos, vídeo e URL final pendentes |

## C#

| Critério | Evidência |
|---|---|
| POO (3,0) | Encapsulamento em Occurrence/StatusEntry; enums, exceções e regras testadas |
| Web API (2,5) | Controllers REST, DTOs, autenticação/autorização e OpenAPI |
| EF Core + LINQ (1,5) | AppDbContext, Include/Where/OrderBy, migrações, FK e PostgreSQL real |
| Async/Await (1,0) | Casos de uso, controllers, IO e consultas assíncronas com CancellationToken |
| Qualidade (2,0) | Camadas, DI/interfaces, testes, CI, README e histórico Conventional Commits |

A pontuação cabe aos professores. Esta matriz não garante nota nem substitui as evidências físicas e materiais exigidos.
