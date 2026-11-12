# Registro de apoio de IA

## 2026-11-11 — Planejamento e implementação

Solicitação: desenvolver o Passo Seguro conforme os documentos CitizenTech, Mobile, SOA e C#, em monorepo com Expo SDK 54.0.0, API C#, PostgreSQL 16 em Docker, testes e commits por feature.

Decisões confirmadas pela equipe: Bela Vista; cadastro e login com cidadão e gestor; priorização determinística por gravidade, recorrência e tempo; SQLite para consultas e rascunhos; API e banco locais em Docker; IA e IoT fora desta entrega.

A IA auxilia a implementação e revisão. A equipe deve estudar e conseguir explicar os contratos, regras e testes. Evidências só devem ser marcadas como verificadas após execução real.

## 2026-11-11 — Autorização de execução

Prompt da equipe: “pode começar a implementação”.

Trabalho realizado: quatro camadas .NET, regras e testes de domínio, autenticação, CRUD, fotos e acompanhamento, migrações EF Core, seed fictício e polígono GeoSampa, indicadores, app Expo Router, SQLite/SecureStore, rascunhos e idempotência, testes mobile/API, CI e documentação.

Revisão guiada por resultados reais: corrigir pontos do seed fora do território, rastreamento EF de novos históricos, coordenadas digitadas, repetição após resposta perdida e pastas de lint do template. Ferramentas executadas: .NET build/test/EF Migrations, PostgreSQL 16 em Docker, Jest, TypeScript, ESLint, Expo Doctor, exportação iOS e smoke HTTP com reinício de containers. Não foram realizados testes físicos nem fabricadas evidências do iPhone.
