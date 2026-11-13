import { Pressable, ScrollView, Text, View, StyleSheet } from "react-native";
import { theme } from "./theme";
import { styles } from "./ui";

export interface FilterOption {
  value: string;
  label: string;
}
export function FilterCarousel({
  label,
  options,
  value,
  onChange,
  disabled = false,
}: {
  label: string;
  options: FilterOption[];
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}) {
  return (
    <View style={local.container}>
      <Text style={styles.muted}>{label} · deslize para ver as opções</Text>
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator
        directionalLockEnabled
        contentContainerStyle={local.options}
        accessibilityLabel={`Opções de ${label}`}
      >
        {options.map((option) => (
          <Pressable
            key={option.value}
            accessibilityRole="button"
            accessibilityLabel={`${label}: ${option.label}`}
            accessibilityState={{ selected: value === option.value, disabled }}
            disabled={disabled}
            onPress={() => onChange(option.value)}
            style={({ pressed }) => [
              local.chip,
              value === option.value && local.selected,
              pressed && local.pressed,
              disabled && local.disabled,
            ]}
          >
            <Text
              style={[local.text, value === option.value && local.selectedText]}
            >
              {value === option.value ? "✓ " : ""}
              {option.label}
            </Text>
          </Pressable>
        ))}
      </ScrollView>
    </View>
  );
}
const local = StyleSheet.create({
  container: { gap: theme.spacing.sm },
  options: {
    gap: theme.spacing.sm,
    paddingBottom: theme.spacing.sm,
    alignItems: "center",
  },
  chip: {
    minHeight: theme.touch,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    borderRadius: theme.radius.sm,
    borderWidth: theme.dimensions.border,
    borderColor: theme.colors.border,
    backgroundColor: theme.colors.background,
    justifyContent: "center",
  },
  selected: {
    borderColor: theme.colors.primary,
    backgroundColor: theme.colors.accent,
  },
  text: { color: theme.colors.muted, fontSize: theme.font.body },
  selectedText: { color: theme.colors.primary, fontWeight: "700" },
  pressed: { backgroundColor: theme.colors.pressed },
  disabled: { opacity: theme.opacity.disabled },
});
