import { Text, View, StyleSheet } from "react-native";
import { Status, statusLabels, Occurrence } from "./types";
import { theme } from "./theme";
import { styles } from "./ui";

const icons: Record<Status, string> = {
  Open: "○",
  InReview: "◉",
  Resolved: "✓",
};
const descriptions: Record<Status, string> = {
  Open: "Registro recebido. Aguarda análise do gestor.",
  InReview: "O gestor iniciou a análise desta ocorrência.",
  Resolved: "Ocorrência marcada como resolvida pelo gestor do protótipo.",
};
export function StatusBadge({ status }: { status: Status }) {
  const color = theme.colors.status[status];
  return (
    <View style={[local.badge, { backgroundColor: color.background, borderColor: color.foreground }]}>
      <Text
        accessibilityLabel={`Andamento: ${statusLabels[status]}`}
        style={[local.label, { color: color.foreground }]}
      >
        {icons[status]} {statusLabels[status]}
      </Text>
    </View>
  );
}
export function StatusProgress({ occurrence }: { occurrence: Occurrence }) {
  return (
    <View style={{ gap: theme.spacing.md }}>
      <Text style={styles.subtitle}>Andamento da ocorrência</Text>
      <StatusBadge status={occurrence.status} />
      <Text style={styles.text}>{descriptions[occurrence.status]}</Text>
      {occurrence.history.map((entry, index) => (
        <View key={`${entry.createdAt}-${index}`} style={[local.entry, { borderLeftColor: theme.colors.status[entry.status].foreground }]}>
          <Text style={[local.marker, { color: theme.colors.status[entry.status].foreground }]}>{icons[entry.status]}</Text>
          <View style={local.detail}>
            <Text style={[styles.text, { color: theme.colors.status[entry.status].foreground }]}>
              {statusLabels[entry.status]}
              {index === occurrence.history.length - 1
                ? " · atualização mais recente"
                : ""}
            </Text>
            <Text style={styles.muted}>
              {new Date(entry.createdAt).toLocaleString("pt-BR")}
            </Text>
          </View>
        </View>
      ))}
    </View>
  );
}
const local = StyleSheet.create({
  badge: {
    alignSelf: "flex-start",
    borderWidth: theme.dimensions.border,
    borderRadius: theme.radius.sm,
    padding: theme.spacing.sm,
  },
  label: {
    fontSize: theme.font.body,
    fontWeight: "600",
  },
  entry: {
    flexDirection: "row",
    gap: theme.spacing.md,
    borderLeftWidth: theme.dimensions.border,
    borderLeftColor: theme.colors.border,
    paddingLeft: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
  },
  detail: { flex: 1, gap: theme.spacing.xs },
  marker: { fontSize: theme.font.subtitle },
});
