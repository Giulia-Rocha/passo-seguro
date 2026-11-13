import { router } from "expo-router";
import { Text } from "react-native";
import { useSession } from "../../src/session";
import { useResource } from "../../src/useResource";
import { Indicators } from "../../src/types";
import { Body, Button, Card, Feedback, Screen, styles } from "../../src/ui";
export default function Home() {
  const { session } = useSession();
  const resource = useResource<Indicators>("/indicators");
  return (
    <Screen title="Cada passo importa">
      <Text style={styles.muted}>BELA VISTA · SÃO PAULO</Text>
      <Body>Olá, {session?.user.name}. O que você encontrou no caminho?</Body>
      <Button
        title="Registrar um obstáculo"
        onPress={() => router.push("/record")}
      />
      <Button
        title="Continuar um rascunho"
        secondary
        onPress={() => router.push("/drafts")}
      />
      <Feedback {...resource} />
      {resource.data && (
        <Card>
          <Text style={styles.stat}>{resource.data.total}</Text>
          <Body>ocorrências registradas no território</Body>
          <Text style={styles.muted}>
            Acompanhe os obstáculos e as condições de circulação em Bela Vista.
          </Text>
        </Card>
      )}
      <Card>
        <Text style={styles.subtitle}>Uma cidade para todas as pessoas</Text>
        <Body>
          Consulte o mapa e a lista de obstáculos. Registre o problema com uma
          descrição clara e acompanhe seu andamento.
        </Body>
        <Button
          title="Explorar ocorrências"
          secondary
          onPress={() => router.navigate("/(tabs)/explore")}
        />
      </Card>
      <Text style={styles.muted}>
        Não há garantia de resolução. A prioridade é uma estimativa
        computacional do projeto.
      </Text>
    </Screen>
  );
}
