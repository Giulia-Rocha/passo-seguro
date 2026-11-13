import { Alert, Text } from "react-native";
import { router } from "expo-router";
import { useSession } from "../../src/session";
import { Body, Button, Card, Screen, styles } from "../../src/ui";
export default function Profile() {
  const { session, logout } = useSession();
  return (
    <Screen title="Seu perfil">
      <Card>
        <Text style={styles.subtitle}>{session?.user.name}</Text>
        <Body>{session?.user.email}</Body>
        <Body>{session?.user.role === "Manager" ? "Gestor" : "Cidadão"}</Body>
      </Card>
      <Button
        title="Meus rascunhos"
        secondary
        onPress={() => router.push("/drafts")}
      />
      <Card>
        <Body>
          Passo Seguro conecta pessoas e informações sobre acessibilidade
          urbana.
        </Body>
        <Body>
          O mapa usa o limite público de Bela Vista disponibilizado pelo
          GeoSampa.
        </Body>
        <Body>
          A prioridade é uma estimativa do projeto. Não representa decisão
          oficial nem garantia de atendimento.
        </Body>
      </Card>
      <Button
        title="Sair da conta"
        onPress={() =>
          Alert.alert(
            "Sair da conta?",
            "O cache e os rascunhos desta conta serão apagados deste aparelho.",
            [
              { text: "Cancelar", style: "cancel" },
              {
                text: "Sair",
                style: "destructive",
                onPress: () => {
                  void logout().catch((e) =>
                    Alert.alert("Não foi possível sair", e.message),
                  );
                },
              },
            ],
          )
        }
      />
    </Screen>
  );
}
