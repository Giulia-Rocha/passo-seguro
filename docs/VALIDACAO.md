# Registro de validação

## Situação consolidada

| Item | Situação | Origem da confirmação |
|---|---|---|
| Implementação de Mobile, SOA e C# | Implementada, com referências na matriz de requisitos | Código e testes registrados |
| Testes de domínio | 21 aprovados | Execução local registrada |
| Testes de integração da API | 14 aprovados com PostgreSQL 16 | Execução local registrada |
| Testes mobile | 33 aprovados | Execução local registrada |
| TypeScript, lint e bundle iOS | Verificados nas execuções registradas | Ferramentas locais |
| Ficha, integrantes/RMs e repositório | Presentes no README | Arquivo verificado |
| Prints das telas e GIF | Presentes no README e em docs/evidencias | Arquivos verificados |
| Vídeo de até 3 minutos, jornada completa e salvamento/recuperação | Concluído | Confirmação da equipe |
| Publicação do vídeo no YouTube como não listado | Concluída conforme relato da equipe; endereço a conciliar com o README local | Confirmação da equipe |
| VoiceOver, fontes ampliadas e legibilidade no iPhone | Conferidos | Confirmação da equipe |
| Acesso dos professores e envio nas três disciplinas | Concluídos | Confirmação da equipe |

As confirmações da equipe registram os itens declarados como concluídos nesta revisão. Não representam novas execuções físicas pelo agente nem uma auditoria independente das submissões.

### Endereço do vídeo

O README local ainda aponta para um arquivo do Google Drive. A equipe informou que a publicação exigida no YouTube está concluída; falta informar o endereço correspondente para atualizar esse vínculo documental. O link não foi substituído por uma URL presumida.

## Evidências e resultados históricos

As seções abaixo preservam o histórico das verificações. Contagens menores correspondem a etapas anteriores de desenvolvimento, sem substituir os resultados consolidados acima. Os relatórios visuais iniciais de 11/11 permanecem como evidências daquelas execuções; os prints atuais do app estão vinculados no README.

## Atualização de 13/11/2026

- Textos da interface sem “fictício” ou “protótipo acadêmico”. Cadastro com nome, e-mail, senha e confirmação; sessão iniciada com a resposta da API. Testes de tela cobrem cadastro, senha divergente e conta duplicada; teste HTTP verifica que a conta criada é cidadã e consegue fazer login.
- Limpeza do prefixo de demonstração apenas nos registros de seed, com teste que comprova a preservação das descrições criadas por usuários. Origem dos dados iniciais permanece documentada.
- Nesta atualização, **33 testes mobile e 14 testes de integração passaram**, incluindo cadastro com início de sessão, login público e preservação das descrições dos usuários. TypeScript e lint aprovados.

- Pendência de `mobile/tsconfig.json` validada com TypeScript e commitada separadamente.
- Swagger corrigido: criação documenta 201; exclusões, logout e envio de foto documentam 204 sem corpo; health documenta 503 com exemplo de indisponibilidade. Fotos são respostas binárias JPEG/PNG. Exemplos completos de autenticação, indicadores, paginação, histórico, prioridade e erros, incluindo endereço nos exemplos de ocorrência.
- **12 testes de integração passaram** após a correção do contrato, incluindo uma API de teste com conexão indisponível para verificar o 503 real e sua documentação.

- Tema escuro com ciano/violeta, transições com redução de movimento e filtros horizontais em todas as consultas e seleções do formulário.
- Andamento destacado em badges e histórico em linha do tempo. Cores definidas pela equipe: Aberta em roxo, Em análise em amarelo e Resolvida em verde, com fundos escuros correspondentes e identificação por texto/ícone.
- Endereço opcional persistido em domínio, DTOs e PostgreSQL, com migração aditiva. A busca por endereço e o GPS usam expo-location do SDK 54, sem novos serviços pagos.
- **29 testes mobile, 21 testes de domínio e 9 testes de API aprovados**. Os novos cenários cobrem filtros acessíveis, permissão negada, GPS desativado, endereço não encontrado, resposta atrasada de filtros e persistência/edição de endereço.
- TypeScript e lint verificados após a atualização. Os testes automatizados de busca de endereço/GPS e consultas estão registrados; não foi relatada separadamente nesta revisão uma bateria física específica de GPS, câmera e funcionamento offline.
- As capturas e o relatório visual abaixo são evidências da entrega inicial de 11/11; não representam screenshots do novo tema no aparelho.

## Execuções reais — 11/11/2026 (São Paulo)

- Backend compilado em .NET 10, com pacotes NuGet restaurados.
- 20 testes unitários de domínio aprovados.
- 8 testes de integração aprovados com PostgreSQL 16 em container isolado.
- Migrações e seed executados em banco vazio durante a integração.
- Ambiente Docker de demonstração iniciado com banco persistente.
- Smoke HTTP: health 200, criação 201, chave idempotente, atualização recuperada após reiniciar banco e API, exclusão 204. Resultado em `evidencias/api-smoke.json`.
- 18 testes de mobile aprovados, incluindo renovação/expiração de sessão e indisponibilidade de rede.
- Pacote Expo confirmado em 54.0.0. A recomendação automática de patch mais recente foi excluída para preservar o requisito da equipe.
- Bundle iOS gerado com sucesso após os patches de dependências e ajustes do formulário.
- TypeScript validado sem erros. Lint configurado explicitamente para `app`, `src` e `__tests__`, sem depender das pastas vazias do template.
- Expo Doctor: 17/17 verificações aprovadas com a exceção intencional do patch de Expo registrada em package.json.
- Captura real do Swagger: `evidencias/swagger.png`.
- Relatório gerado dos resultados TRX/Jest reais: `evidencias/test-results.html`; captura visual em `evidencias/tests.png`.

## Limitações conhecidas

- A auditoria npm final registra **30 alertas: 8 baixos, 21 moderados e 1 alto**, em `npm-audit.json`. Foram aplicados patches de decode-uri-component e PostCSS sem mudar Expo 54.0.0. O alerta alto restante é `image-size@1.2.1`, usado pelo Metro para assets; a versão corrigida 2.x altera a API esperada pelo Metro deste SDK. Não foi aplicado upgrade forçado que comprometeria a compatibilidade.
- Esta entrega local não equivale a uma publicação comercial auditada. Use assets de origem conhecida no projeto; não processe imagens externas não confiáveis com a ferramenta de build antiga.
- O ambiente instalado usa Node 23.9.0 e emite avisos de engine em dependências de desenvolvimento. O ambiente reproduzível recomendado e configurado na CI é Node 22 LTS.
- OpenAPI usa a propriedade `Example`, funcional em OAS 3.0, marcada como obsoleta na biblioteca instalada; uma compilação limpa pode emitir CS0618.
- A CI está configurada e o repositório foi publicado. Os resultados dos workflows no GitHub não foram consultados nesta revisão; os resultados apresentados acima são das execuções locais registradas.

## Documentação de referência

- [README e materiais de apresentação](../README.md).
- [Matriz de rastreabilidade](REQUISITOS.md).
- [Resultados e evidências técnicas](evidencias/).
