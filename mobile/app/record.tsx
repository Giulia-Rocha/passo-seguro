import { useEffect, useRef, useState } from "react";
import { Alert, Image, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";
import MapView, { Marker } from "react-native-maps";
import * as Crypto from "expo-crypto";
import * as Location from "expo-location";
import * as ImagePicker from "expo-image-picker";
import { File, Paths } from "expo-file-system";
import { ApiError, request, uploadPhoto } from "../src/api";
import { storage } from "../src/storage";
import { useSession } from "../src/session";
import { Category, Draft, Occurrence, OccurrenceInput } from "../src/types";
import { useResource } from "../src/useResource";
import { validateOccurrence } from "../src/validation";
import { Body, Button, Card, Feedback, Field, Screen, styles } from "../src/ui";
const empty: OccurrenceInput = {
  categoryId: 0,
  description: "",
  difficulty: 3,
  latitude: -23.557,
  longitude: -46.646,
};
export default function Record() {
  const { id, draftKey } = useLocalSearchParams<{
    id?: string;
    draftKey?: string;
  }>();
  const { session } = useSession();
  const owner = session!.user.id;
  const [input, setInput] = useState<OccurrenceInput>(empty);
  const [latitudeText, setLatitudeText] = useState(String(empty.latitude));
  const [longitudeText, setLongitudeText] = useState(String(empty.longitude));
  const [pending, setPending] = useState(false);
  const [photo, setPhoto] = useState<string>();
  const [key] = useState(() => draftKey ?? Crypto.randomUUID());
  const [createdId, setCreatedId] = useState<string>();
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [ready, setReady] = useState(!id && !draftKey);
  const lock = useRef(false);
  const map = useRef<MapView>(null);
  const categories = useResource<Category[]>("/categories");
  const locked = !!createdId || pending;
  function update<K extends keyof OccurrenceInput>(
    field: K,
    value: OccurrenceInput[K],
  ) {
    if (!locked) {
      setInput((v) => ({ ...v, [field]: value }));
      if (field === "latitude") setLatitudeText(String(value));
      if (field === "longitude") setLongitudeText(String(value));
    }
  }
  useEffect(() => {
    let active = true;
    (async () => {
      try {
        if (draftKey) {
          const d = (await storage.drafts(owner)).find(
            (x) => x.key === draftKey,
          );
          if (!d) throw new Error("Rascunho não encontrado.");
          if (active) {
            setInput(d.input);
            setLatitudeText(String(d.input.latitude));
            setLongitudeText(String(d.input.longitude));
            setPending(d.pending ?? false);
            setPhoto(d.photo);
            setCreatedId(d.occurrenceId);
          }
        } else if (id) {
          const o = await request<Occurrence>(`/occurrences/${id}`);
          if (active) {
            setInput(o);
            setLatitudeText(String(o.latitude));
            setLongitudeText(String(o.longitude));
          }
        }
      } catch (e) {
        if (active) setError((e as Error).message);
      } finally {
        if (active) setReady(true);
      }
    })();
    return () => {
      active = false;
    };
  }, [draftKey, id, owner]);
  async function saveDraft(occurrenceId = createdId, pendingAttempt = pending) {
    const d: Draft = {
      key,
      input,
      photo,
      occurrenceId,
      pending: pendingAttempt,
      updatedAt: new Date().toISOString(),
    };
    await storage.saveDraft(owner, d);
  }
  async function locate() {
    try {
      const permission = await Location.requestForegroundPermissionsAsync();
      if (!permission.granted) {
        setError(
          "Localização não autorizada. Selecione um ponto no mapa ou informe as coordenadas.",
        );
        return;
      }
      const position = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
      });
      update("latitude", position.coords.latitude);
      update("longitude", position.coords.longitude);
      map.current?.animateToRegion({
        ...position.coords,
        latitudeDelta: 0.01,
        longitudeDelta: 0.01,
      });
    } catch {
      setError(
        "Não foi possível obter sua localização. Selecione um ponto no mapa.",
      );
    }
  }
  async function pick(camera: boolean) {
    try {
      if (camera) {
        const permission = await ImagePicker.requestCameraPermissionsAsync();
        if (!permission.granted) {
          setError(
            "Câmera não autorizada. Você pode escolher uma foto da galeria.",
          );
          return;
        }
      }
      const result = camera
        ? await ImagePicker.launchCameraAsync({
            mediaTypes: ["images"],
            quality: 0.7,
            allowsEditing: true,
          })
        : await ImagePicker.launchImageLibraryAsync({
            mediaTypes: ["images"],
            quality: 0.7,
            allowsEditing: true,
          });
      if (!result.canceled) {
        const asset = result.assets[0];
        if ((asset.fileSize ?? 0) > 5 * 1024 * 1024) {
          setError("Escolha uma foto de até 5 MB.");
          return;
        }
        const destination = new File(
          Paths.document,
          `${key}-${Crypto.randomUUID()}.jpg`,
        );
        new File(asset.uri).copy(destination);
        setPhoto(destination.uri);
      }
    } catch (e) {
      setError(`Não foi possível selecionar a foto: ${(e as Error).message}`);
    }
  }
  async function submit() {
    if (lock.current) return;
    const validation = validateOccurrence(input);
    if (validation) {
      setError(validation);
      return;
    }
    lock.current = true;
    setBusy(true);
    setError("");
    let persisted = createdId;
    try {
      if (!id) {
        setPending(true);
        await saveDraft(createdId, true);
      }
      const result = id
        ? await request<Occurrence>(`/occurrences/${id}`, {
            method: "PUT",
            body: JSON.stringify(input),
          })
        : createdId
          ? { id: createdId }
          : await request<Occurrence>("/occurrences", {
              method: "POST",
              headers: { "Idempotency-Key": key },
              body: JSON.stringify(input),
            });
      persisted = result.id;
      if (!id) {
        setCreatedId(result.id);
        await saveDraft(result.id, true);
      }
      if (photo) await uploadPhoto(result.id, photo);
      if (!id) await storage.deleteDraft(owner, key);
      router.replace({ pathname: "/occurrence", params: { id: result.id } });
    } catch (e) {
      const message = (e as Error).message;
      setError(
        persisted
          ? `Ocorrência salva. A foto não foi concluída. Tente enviar novamente. ${message}`
          : message,
      );
      if (!id && e instanceof ApiError) {
        if ([400, 422].includes(e.status) && !persisted) {
          setPending(false);
          await saveDraft(undefined, false);
        } else await saveDraft(persisted, true);
      }
    } finally {
      lock.current = false;
      setBusy(false);
    }
  }
  return (
    <Screen title={id ? "Editar ocorrência" : "Registrar obstáculo"}>
      <Body>
        Descreva uma dificuldade de circulação em Bela Vista. Use somente dados
        fictícios.
      </Body>
      {!ready ? (
        <Body>Carregando formulário…</Body>
      ) : (
        <>
          <Feedback {...categories} />
          {pending && !createdId && (
            <Card>
              <Body>
                O envio anterior precisa ser confirmado. Tente salvar novamente
                com os mesmos dados para evitar duplica??o.
              </Body>
            </Card>
          )}
          {!!createdId && (
            <Card>
              <Body>
                A ocorrência já foi salva. Falta concluir a foto; os dados do
                registro estão preservados.
              </Body>
            </Card>
          )}
          <Text style={styles.subtitle}>Categoria</Text>
          <View style={styles.row}>
            {categories.data?.map((c) => (
              <Button
                key={c.id}
                title={c.name}
                secondary={input.categoryId !== c.id}
                disabled={busy || locked}
                onPress={() => update("categoryId", c.id)}
              />
            ))}
          </View>
          <Field
            label="Descrição do obstáculo"
            value={input.description}
            onChangeText={(v) => update("description", v)}
            editable={!busy && !locked}
            multiline
            maxLength={2000}
          />
          <Text style={styles.subtitle}>Dificuldade de circulação</Text>
          <Body>1: pequena dificuldade · 5: impede a passagem</Body>
          <View style={styles.row}>
            {[1, 2, 3, 4, 5].map((n) => (
              <Button
                key={n}
                title={`${n}`}
                secondary={input.difficulty !== n}
                disabled={busy || locked}
                onPress={() => update("difficulty", n)}
              />
            ))}
          </View>
          <Text style={styles.subtitle}>Localização</Text>
          <Body>
            Toque no mapa para marcar o obstáculo ou informe as coordenadas
            abaixo.
          </Body>
          <MapView
            ref={map}
            style={styles.map}
            accessibilityLabel="Selecione a localização do obstáculo. Também pode usar os campos latitude e longitude."
            initialRegion={{
              ...empty,
              latitudeDelta: 0.02,
              longitudeDelta: 0.02,
            }}
            onPress={(event) => {
              update("latitude", event.nativeEvent.coordinate.latitude);
              update("longitude", event.nativeEvent.coordinate.longitude);
            }}
          >
            <Marker
              coordinate={{
                latitude: input.latitude || empty.latitude,
                longitude: input.longitude || empty.longitude,
              }}
            />
          </MapView>
          <Button
            title="Usar minha localização"
            secondary
            disabled={busy || locked}
            onPress={locate}
          />
          <Field
            label="Latitude"
            value={latitudeText}
            keyboardType="numbers-and-punctuation"
            editable={!busy && !locked}
            onChangeText={(v) => {
              setLatitudeText(v);
              setInput((current) => ({
                ...current,
                latitude: v.trim() ? Number(v.replace(",", ".")) : NaN,
              }));
            }}
          />
          <Field
            label="Longitude"
            value={longitudeText}
            keyboardType="numbers-and-punctuation"
            editable={!busy && !locked}
            onChangeText={(v) => {
              setLongitudeText(v);
              setInput((current) => ({
                ...current,
                longitude: v.trim() ? Number(v.replace(",", ".")) : NaN,
              }));
            }}
          />
          <Text style={styles.subtitle}>Foto opcional</Text>
          {photo && (
            <Image
              source={{ uri: photo }}
              accessibilityLabel="Foto selecionada para esta ocorrência"
              style={styles.map}
            />
          )}
          <Button
            title="Escolher foto"
            secondary
            disabled={busy}
            onPress={() => pick(false)}
          />
          <Button
            title="Tirar foto"
            secondary
            disabled={busy}
            onPress={() => pick(true)}
          />
          {photo && (
            <Button
              title="Retirar foto selecionada"
              secondary
              disabled={busy}
              onPress={() => setPhoto(undefined)}
            />
          )}
          {!!error && (
            <Text accessibilityRole="alert" style={styles.error}>
              {error}
            </Text>
          )}
          <Button
            title={
              busy
                ? "Salvando…"
                : createdId
                  ? "Concluir envio"
                  : "Salvar ocorrência"
            }
            disabled={busy}
            onPress={submit}
          />
          {!id && (
            <Button
              title="Salvar rascunho neste aparelho"
              secondary
              disabled={busy}
              onPress={() => {
                void saveDraft()
                  .then(() =>
                    Alert.alert(
                      "Rascunho salvo",
                      "Você pode retomá-lo na tela de rascunhos.",
                    ),
                  )
                  .catch((e) => setError(e.message));
              }}
            />
          )}
        </>
      )}
    </Screen>
  );
}
