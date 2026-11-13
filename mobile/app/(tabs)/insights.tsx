import { FilterCarousel } from "../../src/FilterCarousel";
import { AnimatedContent } from "../../src/motion";
import { useState } from "react";
import { Text } from "react-native";
import { Count, Indicators, statusLabels, Status } from "../../src/types";
import { useResource } from "../../src/useResource";
import { Body, Card, Feedback, Screen, styles } from "../../src/ui";
function Counts({ title, items }: { title: string; items: Count[] }) {
  return (
    <Card>
      <Text accessibilityRole="header" style={styles.subtitle}>
        {title}
      </Text>
      {items.length === 0 ? (
        <Body>Nenhum dado neste período.</Body>
      ) : (
        items.map((x) => (
          <Body key={x.label}>
            {statusLabels[x.label as Status] ?? x.label}: {x.count}
          </Body>
        ))
      )}
    </Card>
  );
}
export default function Insights() {
  const [days, setDays] = useState(30);
  const [from] = useState(() => new Date());
  const since = new Date(from.getTime() - days * 86400000).toISOString();
  const resource = useResource<Indicators>(
    `/indicators?from=${encodeURIComponent(since)}`,
  );
  return (
    <Screen title="Acessibilidade em números">
      <Body>
        Acompanhe a distribuição e a evolução dos registros de Bela Vista.
      </Body>
      <FilterCarousel
        label="Período"
        value={String(days)}
        options={[7, 30, 90, 365].map((d) => ({
          value: String(d),
          label: `${d} dias`,
        }))}
        onChange={(value) => setDays(Number(value))}
      />
      <Feedback {...resource} />
      {resource.data && (
        <AnimatedContent key={days}>
          <Card>
            <Text style={styles.stat}>{resource.data.total}</Text>
            <Body>ocorrências no período</Body>
          </Card>
          <Counts
            title="Categorias mais frequentes"
            items={resource.data.categories}
          />
          <Counts title="Andamento" items={resource.data.statuses} />
          <Counts title="Prioridades" items={resource.data.priorities} />
          <Counts
            title="Evolução diária (UTC)"
            items={resource.data.timeline}
          />
          <Card>
            <Text style={styles.subtitle}>Concentração regional</Text>
            <Body>Células aproximadas de 200 metros.</Body>
            {resource.data.concentrations.map((x, i) => (
              <Body key={i}>
                {x.latitude.toFixed(4)}, {x.longitude.toFixed(4)}: {x.count}{" "}
                registros
              </Body>
            ))}
            {resource.data.concentrations.length === 0 && (
              <Body>Nenhum registro neste período.</Body>
            )}
          </Card>
        </AnimatedContent>
      )}
    </Screen>
  );
}
