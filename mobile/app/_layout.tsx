import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { SessionProvider, useSession } from "../src/session";
import { theme } from "../src/theme";
import { ActivityIndicator } from "react-native";
function Navigation() {
  const { session, ready } = useSession();
  if (!ready)
    return <ActivityIndicator accessibilityLabel="Abrindo sua sessão" />;
  return (
    <>
      <StatusBar style="dark" />
      <Stack
        screenOptions={{
          headerStyle: { backgroundColor: theme.colors.background },
          headerTintColor: theme.colors.text,
          headerBackTitle: "Voltar",
        }}
      >
        <Stack.Protected guard={!session}>
          <Stack.Screen name="login" options={{ title: "Bem-vindo" }} />
        </Stack.Protected>
        <Stack.Protected guard={!!session}>
          <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
          <Stack.Screen name="occurrence" options={{ title: "Ocorrência" }} />
          <Stack.Screen name="record" options={{ title: "Registrar" }} />
          <Stack.Screen name="drafts" options={{ title: "Rascunhos" }} />
        </Stack.Protected>
      </Stack>
    </>
  );
}
export default function Layout() {
  return (
    <SessionProvider>
      <Navigation />
    </SessionProvider>
  );
}
