import { FilterCarousel } from "./FilterCarousel";
import { AnimatedContent } from "./motion";
import { StatusBadge } from "./OccurrenceStatus";
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
        <FilterCarousel
          label="Categoria"
          value={category?.toString() ?? ""}
          options={[
            { value: "", label: "Todas" },
            ...(categories.data ?? []).map((c) => ({
              value: String(c.id),
              label: c.name,
            })),
          ]}
          onChange={(value) =>
            choose(() => setCategory(value ? Number(value) : undefined))
          }
        />
        <FilterCarousel
          label="Andamento"
          value={status ?? ""}
          options={[
            { value: "", label: "Todos" },
            ...(Object.keys(statusLabels) as Status[]).map((value) => ({
              value,
              label: statusLabels[value],
            })),
          ]}
          onChange={(value) =>
            choose(() => setStatus(value ? (value as Status) : undefined))
          }
        />
        <FilterCarousel
          label="Prioridade"
          value={priority}
          options={["", "Baixa", "Média", "Alta", "Crítica"].map((value) => ({
            value,
            label: value || "Todas",
          }))}
          onChange={(value) => choose(() => setPriority(value))}
        />
        <FilterCarousel
          label="Dificuldade"
          value={difficulty?.toString() ?? ""}
          options={[
            { value: "", label: "Todas" },
            ...[1, 2, 3, 4, 5].map((value) => ({
              value: String(value),
              label: `${value} de 5`,
            })),
          ]}
          onChange={(value) =>
            choose(() => setDifficulty(value ? Number(value) : undefined))
          }
        />
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
        <AnimatedContent key={query}>
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
              userInterfaceStyle="dark"
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
              <Text style={styles.pill}>Prioridade {o.priority.level}</Text>
              <StatusBadge status={o.status} />
              <Body>{o.description}</Body>
              {!!o.address && <Text style={styles.muted}>{o.address}</Text>}
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
        </AnimatedContent>
      )}
    </Screen>
  );
}
