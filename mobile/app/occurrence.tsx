import { useState } from "react";
import { StatusBadge, StatusProgress } from "../src/OccurrenceStatus";
import { Alert, Image, Text } from "react-native";
import { router, useLocalSearchParams } from "expo-router";
import { API_URL, currentSession, request } from "../src/api";
import { useSession } from "../src/session";
import { Occurrence, Status } from "../src/types";
import { useResource } from "../src/useResource";
import { Body, Button, Card, Feedback, Screen, styles } from "../src/ui";
export default function Detail() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const resource = useResource<Occurrence>(`/occurrences/${id}`);
  const { session } = useSession();
  const [busy, setBusy] = useState(false);
  const [photoError, setPhotoError] = useState(false);
  async function status(next: Status) {
    setBusy(true);
    try {
      await request(`/occurrences/${id}/status`, {
        method: "PATCH",
        body: JSON.stringify({ status: next }),
      });
      await resource.reload();
    } catch (e) {
      Alert.alert("Não foi possível atualizar", (e as Error).message);
    } finally {
      setBusy(false);
    }
  }
  function remove() {
    Alert.alert(
      "Excluir ocorrência?",
      "Esta ação remove o registro e sua foto.",
      [
        { text: "Cancelar", style: "cancel" },
        {
          text: "Excluir",
          style: "destructive",
          onPress: async () => {
            setBusy(true);
            try {
              await request(`/occurrences/${id}`, { method: "DELETE" });
              router.back();
            } catch (e) {
              Alert.alert("Não foi possível excluir", (e as Error).message);
            } finally {
              setBusy(false);
            }
          },
        },
      ],
    );
  }
  const o = resource.data;
  const offline = !!resource.cachedAt;
  return (
    <Screen title="Detalhes da ocorrência">
      <Feedback {...resource} />
      {o && (
        <>
          <Card>
            <Text style={styles.subtitle}>{o.category}</Text>
            <StatusBadge status={o.status} />
            <Body>{o.description}</Body>
            {!!o.address && <Body>Endereço: {o.address}</Body>}
            <Body>Dificuldade {o.difficulty}/5</Body>
            <Body>{new Date(o.createdAt).toLocaleString("pt-BR")}</Body>
            <Text style={styles.muted}>
              Local: {o.latitude.toFixed(6)}, {o.longitude.toFixed(6)}
            </Text>
          </Card>
          <Card>
            <Text style={styles.subtitle}>Prioridade {o.priority.level}</Text>
            <Body>
              {o.priority.score} pontos: gravidade {o.priority.severityPoints},
              recorrência {o.priority.recurrencePoints}, tempo{" "}
              {o.priority.agePoints}.
            </Body>
            <Text style={styles.muted}>
              Estimativa acadêmica, sem caráter oficial.
            </Text>
          </Card>
          {o.hasPhoto && !offline && (
            <Card>
              <Image
                accessibilityLabel="Fotografia do obstáculo enviada pelo autor"
                source={{
                  uri: `${API_URL}/occurrences/${id}/photo`,
                  headers: {
                    Authorization: `Bearer ${currentSession()?.accessToken}`,
                  },
                }}
                style={styles.map}
                resizeMode="contain"
                onError={() => setPhotoError(true)}
              />
              {photoError && (
                <Body>
                  Não foi possível carregar a foto. Atualize a consulta para
                  tentar novamente.
                </Body>
              )}
            </Card>
          )}
          <Card>
            <StatusProgress occurrence={o} />
          </Card>
          {o.authorId === session?.user.id && o.status === "Open" && (
            <>
              <Button
                title="Editar ocorrência"
                disabled={busy || offline}
                onPress={() =>
                  router.push({ pathname: "/record", params: { id } })
                }
              />
              <Button
                title="Excluir ocorrência"
                secondary
                disabled={busy || offline}
                onPress={remove}
              />
              {o.hasPhoto && (
                <Button
                  title="Remover foto"
                  secondary
                  disabled={busy || offline}
                  onPress={() =>
                    Alert.alert("Remover foto?", "A ocorrência será mantida.", [
                      { text: "Cancelar", style: "cancel" },
                      {
                        text: "Remover",
                        onPress: async () => {
                          try {
                            await request(`/occurrences/${id}/photo`, {
                              method: "DELETE",
                            });
                            await resource.reload();
                          } catch (e) {
                            Alert.alert("Erro", (e as Error).message);
                          }
                        },
                      },
                    ])
                  }
                />
              )}
            </>
          )}
          {session?.user.role === "Manager" && o.status === "Open" && (
            <Button
              title="Iniciar análise"
              disabled={busy || offline}
              onPress={() => status("InReview")}
            />
          )}
          {session?.user.role === "Manager" && o.status === "InReview" && (
            <>
              <Button
                title="Marcar como resolvida"
                disabled={busy || offline}
                onPress={() => status("Resolved")}
              />
              <Button
                title="Voltar para aberta"
                secondary
                disabled={busy || offline}
                onPress={() => status("Open")}
              />
            </>
          )}
        </>
      )}
    </Screen>
  );
}
