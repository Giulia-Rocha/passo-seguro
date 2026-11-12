# Plano aprovado

1. Fundação: monorepo, Expo 54, quatro camadas .NET 10, configuração e rastreabilidade.
2. Persistência: PostgreSQL 16, duas migrações, seed fictício reproduzível.
3. Acesso: cadastro, JWT, refresh revogável, cidadão e gestor.
4. Ocorrências: CRUD, localização, categorias, foto e histórico de status.
5. Consulta: mapa, filtros, paginação e prioridade explicável.
6. Indicadores: categorias, períodos, concentrações e prioridades.
7. Offline e acessibilidade: SQLite, rascunhos, envio manual idempotente e VoiceOver.
8. Validação: testes unitários e integração, CI, documentação e evidências reais.

Cada feature recebe commit Conventional Commits. IA, IoT, integração oficial, roteirização e hospedagem pública estão fora deste escopo. Não usar dados pessoais reais.

## Regras de prioridade

Dificuldade (1–5) × 12 + min(semelhantes × 5, 25) + min(dias completos em aberto, 15).
Semelhantes: mesma categoria, até 200 m, últimos 30 dias, não resolvidas, excluindo a própria.
Baixa <35; média <60; alta <80; crítica >=80. Resolvidas não integram fila de prioridade.

## Aceite

Jornada real no app e API; CRUD persistido após reinício; testes de regras, HTTP e mobile; duas migrações; OpenAPI completa; fontes documentadas; README com ficha, integrantes, prints e vídeo até 3 minutos. Testes físicos e artefatos externos não executados são pendências explícitas.
