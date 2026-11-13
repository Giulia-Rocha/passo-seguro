# Validação e pendências

## Atualização de 13/11/2026

- Tema escuro com ciano/violeta, transições com redução de movimento e filtros horizontais em todas as consultas e seleções do formulário.
- Andamento destacado em badges e histórico em linha do tempo. Cores específicas de Aberta, Em análise e Resolvida aguardam definição da equipe; apresentação neutra provisória.
- Endereço opcional persistido em domínio, DTOs e PostgreSQL, com migração aditiva. A busca por endereço e o GPS usam expo-location do SDK 54, sem novos serviços pagos.
- **29 testes mobile, 21 testes de domínio e 9 testes de API aprovados**. Os novos cenários cobrem filtros acessíveis, permissão negada, GPS desativado, endereço não encontrado, resposta atrasada de filtros e persistência/edição de endereço.
- TypeScript e lint verificados após a atualização. Busca de endereço/GPS e animações ainda precisam de validação física no iPhone.
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
- A CI está configurada, mas sua execução no GitHub depende da criação/publicação do repositório.

## Depende do aparelho/equipe

- Percorrer jornada no Expo Go do iPhone pela rede local.
- Verificar câmera/galeria, GPS e permissões negadas.
- Confirmar SQLite e rascunho após fechar/reabrir o app e ao perder conexão.
- Revisar VoiceOver, ampliação de fonte, contraste e alvos de toque no aparelho.
- Capturar prints reais das telas e anexar ao README.
- Informar nomes/RMs, URL do repositório e link não listado do vídeo até 3 minutos.
- Confirmar acesso do avaliador e enviar no Teams até 19/11/2026 às 23h55.

Não há simulação de testes físicos ou de screenshots do app. Esses itens permanecem pendentes até execução no dispositivo.
