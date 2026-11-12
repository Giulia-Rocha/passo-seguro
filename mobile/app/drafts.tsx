import { useCallback, useState } from "react";
import { Alert, Text } from "react-native";
import { router, useFocusEffect } from "expo-router";
import { storage } from "../src/storage";
import { useSession } from "../src/session";
import { Draft } from "../src/types";
import { Body, Button, Card, Screen, styles } from "../src/ui";
export default function Drafts() {
  const { session } = useSession();
  const owner = session!.user.id;
  const [drafts, setDrafts] = useState<Draft[]>([]);
  const [error, setError] = useState("");
  const load = useCallback(() => {
    void storage
      .drafts(owner)
      .then(setDrafts)
      .catch((e) => setError(e.message));
  }, [owner]);
  useFocusEffect(load);
  return (
    <Screen title="Seus rascunhos">
      <Body>
        Registros salvos neste aparelho. O envio acontece somente quando você
        confirmar.
      </Body>
      {!!error && <Text style={styles.error}>{error}</Text>}
      {drafts.length === 0 && (
        <Card>
          <Body>Nenhum rascunho salvo.</Body>
        </Card>
      )}
      {drafts.map((d) => (
        <Card key={d.key}>
          <Body>{d.input.description || "Rascunho sem descrição"}</Body>
          <Text style={styles.muted}>
            {new Date(d.updatedAt).toLocaleString("pt-BR")}
            {d.occurrenceId ? " · ocorrência salva, foto pendente" : ""}
          </Text>
          <Button
            title="Continuar rascunho"
            onPress={() =>
              router.push({ pathname: "/record", params: { draftKey: d.key } })
            }
          />
          <Button
            title="Excluir rascunho"
            secondary
            onPress={() =>
              Alert.alert(
                "Excluir rascunho?",
                "O registro local será removido.",
                [
                  { text: "Cancelar", style: "cancel" },
                  {
                    text: "Excluir",
                    style: "destructive",
                    onPress: () => {
                      void storage.deleteDraft(owner, d.key).then(load);
                    },
                  },
                ],
              )
            }
          />
        </Card>
      ))}
    </Screen>
  );
}
