import { validateOccurrence } from "../src/validation";
const valid = {
  categoryId: 1,
  description: "Calçada fictícia com obstáculo",
  difficulty: 3,
  latitude: -23.557,
  longitude: -46.646,
};
test("accepts a complete occurrence", () =>
  expect(validateOccurrence(valid)).toBe(""));
test.each([0, 6, 1.5])("rejects invalid difficulty %s", (difficulty) =>
  expect(validateOccurrence({ ...valid, difficulty })).not.toBe(""),
);
test("rejects missing category", () =>
  expect(validateOccurrence({ ...valid, categoryId: 0 })).toContain(
    "categoria",
  ));
test("rejects whitespace description", () =>
  expect(
    validateOccurrence({ ...valid, description: "           " }),
  ).toContain("Descreva"));
test.each([NaN, Infinity, 91])("rejects invalid latitude %s", (latitude) =>
  expect(validateOccurrence({ ...valid, latitude })).toContain("localização"),
);
