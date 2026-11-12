# Passo Seguro — Mobile

React Native + TypeScript + Expo **54.0.0**, Expo Router e SQLite.

Consulte a [documentação principal](../README.md) para ficha da CitizenTech, contas demonstrativas, API, banco, fontes e evidências.

```powershell
npm ci
# Configure EXPO_PUBLIC_API_URL em .env com o IP Wi-Fi do computador.
npx expo start
```

Testes: `npm test`. Verificações: `npm run typecheck` e `npm run lint`.

O app consome a API real. A API e o PostgreSQL devem estar ativos em Docker. Não coloque credenciais do banco nas variáveis `EXPO_PUBLIC_*`.
