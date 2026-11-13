# Registro de apoio de IA

## 2026-11-13 — Cadastro e textos do aplicativo

Solicitação: retirar termos fictícios do app, habilitar criar conta e remover “protótipo acadêmico” da tela inicial.

Alterações: rótulos e textos neutros nas telas, ação Criar conta com confirmação de senha, normalização de nome/e-mail e bloqueio de envios duplicados. Cadastro conectado ao endpoint público existente; testes validam criação, início de sessão e erros. A limpeza do prefixo dos dados iniciais se restringe a registros identificados como seed; descrições de usuários são preservadas. A origem simulada dos dados continua documentada no README.

## 2026-11-13 — Pendência mobile e correção do Swagger

Solicitação: “commita uma pendencia que tem em mobile e corrige o swagger”.

Pendência identificada: simplificação dos includes de `mobile/tsconfig.json`; validada e commitada separadamente. Correção do OpenAPI: status reais 201/204/503, ausência de corpo em 204, fotos binárias, exemplos completos de DTOs/erros e exemplos por operação. Testes de contrato HTTP verificam documentação e comportamento real, incluindo falha isolada de conexão na verificação de saúde. A API Docker foi reconstruída para servir o contrato atualizado.

## 2026-11-13 — Cores de andamento

Definição do usuário: “em aberto - roxo, em analise - amarelo, resolvida - verde”. Aplicação no tema central, badges e linha do tempo, preservando rótulos e ícones acessíveis.

## 2026-11-13 — Melhorias de experiência e localização

Solicitação: suavizar transições de páginas/filtros; apresentar filtros em carrosséis horizontais; melhorar acompanhamento e informar status antes de escolher suas cores; adotar tema escuro cyberpunk; adicionar endereço e localização atual ao registro.

Implementação: tema central escuro, navegação e resultados animados com redução de movimento, filtros compartilhados, badges/linha do tempo neutros, endereço opcional com migração, busca nativa e GPS com mensagens e carregamento. Testes de filtros, concorrência de consultas, localização e persistência. As cores individuais de status ficam pendentes de definição do usuário. A alteração preexistente em mobile/tsconfig.json foi preservada.

## 2026-11-11 — Planejamento e implementação

Solicitação: desenvolver o Passo Seguro conforme os documentos CitizenTech, Mobile, SOA e C#, em monorepo com Expo SDK 54.0.0, API C#, PostgreSQL 16 em Docker, testes e commits por feature.

Decisões confirmadas pela equipe: Bela Vista; cadastro e login com cidadão e gestor; priorização determinística por gravidade, recorrência e tempo; SQLite para consultas e rascunhos; API e banco locais em Docker; IA e IoT fora desta entrega.

A IA auxilia a implementação e revisão. A equipe deve estudar e conseguir explicar os contratos, regras e testes. Evidências só devem ser marcadas como verificadas após execução real.

## 2026-11-11 — Autorização de execução

Prompt da equipe: “pode começar a implementação”.

Trabalho realizado: quatro camadas .NET, regras e testes de domínio, autenticação, CRUD, fotos e acompanhamento, migrações EF Core, seed fictício e polígono GeoSampa, indicadores, app Expo Router, SQLite/SecureStore, rascunhos e idempotência, testes mobile/API, CI e documentação.

Revisão guiada por resultados reais: corrigir pontos do seed fora do território, rastreamento EF de novos históricos, coordenadas digitadas, repetição após resposta perdida e pastas de lint do template. Ferramentas executadas: .NET build/test/EF Migrations, PostgreSQL 16 em Docker, Jest, TypeScript, ESLint, Expo Doctor, exportação iOS e smoke HTTP com reinício de containers. Não foram realizados testes físicos nem fabricadas evidências do iPhone.
