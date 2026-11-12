import { useState } from "react";
import { Text, View } from "react-native";
import { router } from "expo-router";
import MapView, { Marker } from "react-native-maps";
import { Category, Occurrence, Page, Status, statusLabels } from "./types";
import { useResource } from "./useResource";
import { Body, Button, Card, Feedback, Screen, styles } from "./ui";
export function OccurrenceList({ mine = false }: { mine?: boolean }) {
  const [category, setCategory] = useState<number>();
  const [status, setStatus] = useState<Status>();
  const [priority, setPriority] = useState("");
  const [difficulty, setDifficulty] = useState<number>();
  const [sort, setSort] = useState("recent");
  const [page, setPage] = useState(1);
  const [map, setMap] = useState(false);
  const categories = useResource<Category[]>("/categories");
  const query = `?mine=${mine}&page=${page}&pageSize=30&sort=${sort}${category ? `&categoryId=${category}` : ""}${status ? `&status=${status}` : ""}${priority ? `&priority=${encodeURIComponent(priority)}` : ""}${difficulty ? `&difficulty=${difficulty}` : ""}`;
  const resource = useResource<Page<Occurrence>>(`/occurrences${query}`);
  function choose(action: () => void) {
    setPage(1);
    action();
  }
  return (
    <Screen title={mine ? "Minhas ocorrências" : "Explore o território"}>
      <Body>
        {mine
          ? "Acompanhe os registros que você criou."
          : "Conheça as barreiras de acessibilidade em Bela Vista."}
      </Body>
      <Button
        title="Registrar obstáculo"
        onPress={() => router.push("/record")}
      />
      <Card>
        <Text style={styles.subtitle}>Filtrar consulta</Text>
        <Text style={styles.muted}>Categoria</Text>
        <View style={styles.row}>
          <Button
            title="Todas"
            secondary={!category}
            onPress={() => choose(() => setCategory(undefined))}
          />
          {categories.data?.map((c) => (
            <Button
              key={c.id}
              title={c.name}
              secondary={category !== c.id}
              onPress={() => choose(() => setCategory(c.id))}
            />
          ))}
        </View>
        <Text style={styles.muted}>Andamento</Text>
        <View style={styles.row}>
          <Button
            title="Todos"
            secondary={!!status}
            onPress={() => choose(() => setStatus(undefined))}
          />
          {(Object.keys(statusLabels) as Status[]).map((s) => (
            <Button
              key={s}
              title={statusLabels[s]}
              secondary={status !== s}
              onPress={() => choose(() => setStatus(s))}
            />
          ))}
        </View>
        <Text style={styles.muted}>Prioridade</Text>
        <View style={styles.row}>
          {["", "Baixa", "Média", "Alta", "Crítica"].map((p) => (
            <Button
              key={p}
              title={p || "Todas as prioridades"}
              secondary={priority !== p}
              onPress={() => choose(() => setPriority(p))}
            />
          ))}
        </View>
        <Text style={styles.muted}>Dificuldade</Text>
        <View style={styles.row}>
          {[undefined, 1, 2, 3, 4, 5].map((d) => (
            <Button
              key={d ?? "all"}
              title={d ? `${d}` : "Todas as dificuldades"}
              secondary={difficulty !== d}
              onPress={() => choose(() => setDifficulty(d))}
            />
          ))}
        </View>
        <Button
          title={
            sort === "recent"
              ? "Ordenar por prioridade"
              : "Ordenar por mais recentes"
          }
          secondary
          onPress={() =>
            choose(() => setSort(sort === "recent" ? "priority" : "recent"))
          }
        />
      </Card>
      <Feedback {...resource} />
      {resource.data && (
        <>
          <Text style={styles.muted}>
            {resource.data.total} resultados · página {page}
          </Text>
          <Button
            title={map ? "Mostrar somente lista" : "Mostrar mapa desta página"}
            secondary
            onPress={() => setMap(!map)}
          />
          {map && (
            <MapView
              accessibilityLabel="Mapa de ocorrências. A mesma consulta está disponível na lista abaixo."
              style={styles.map}
              initialRegion={{
                latitude: -23.557,
                longitude: -46.646,
                latitudeDelta: 0.025,
                longitudeDelta: 0.025,
              }}
            >
              {resource.data.items.map((o) => (
                <Marker
                  key={o.id}
                  coordinate={{ latitude: o.latitude, longitude: o.longitude }}
                  title={o.category}
                  description={o.priority.level}
                  onCalloutPress={() =>
                    router.push({
                      pathname: "/occurrence",
                      params: { id: o.id },
                    })
                  }
                />
              ))}
            </MapView>
          )}
          {resource.data.items.length === 0 && (
            <Card>
              <Body>Nenhuma ocorrência encontrada para estes filtros.</Body>
            </Card>
          )}
          {resource.data.items.map((o) => (
            <Card key={o.id}>
              <Text style={styles.subtitle}>{o.category}</Text>
              <Text style={styles.pill}>
                {o.priority.level} · {statusLabels[o.status]}
              </Text>
              <Body>{o.description}</Body>
              <Text style={styles.muted}>
                {new Date(o.createdAt).toLocaleDateString("pt-BR")} ·
                Dificuldade {o.difficulty}/5
              </Text>
              <Button
                title="Ver detalhes"
                secondary
                onPress={() =>
                  router.push({ pathname: "/occurrence", params: { id: o.id } })
                }
              />
            </Card>
          ))}
          <View style={styles.row}>
            <Button
              title="Página anterior"
              secondary
              disabled={page === 1}
              onPress={() => setPage(page - 1)}
            />
            <Button
              title="Próxima página"
              secondary
              disabled={page * 30 >= resource.data.total}
              onPress={() => setPage(page + 1)}
            />
          </View>
        </>
      )}
    </Screen>
  );
}
