import React from "react";
import { fireEvent, render } from "@testing-library/react-native";
import { Button, Feedback, Field } from "../src/ui";
test("accessible action invokes handler", () => {
  const onPress = jest.fn();
  const view = render(<Button title="Salvar ocorrência" onPress={onPress} />);
  fireEvent.press(view.getByRole("button", { name: "Salvar ocorrência" }));
  expect(onPress).toHaveBeenCalledTimes(1);
});
test("disabled action cannot be submitted twice", () => {
  const onPress = jest.fn();
  const view = render(<Button title="Salvando" disabled onPress={onPress} />);
  fireEvent.press(view.getByRole("button"));
  expect(onPress).not.toHaveBeenCalled();
});
test("error shows retry and cache age", () => {
  const reload = jest.fn();
  const view = render(
    <Feedback
      loading={false}
      error="Servidor indisponível"
      cachedAt="2026-11-11T12:00:00Z"
      reload={reload}
    />,
  );
  expect(view.getByText(/desatualizados/)).toBeTruthy();
  fireEvent.press(view.getByText("Tentar novamente"));
  expect(reload).toHaveBeenCalled();
});
test("field is identified for screen reader", () => {
  const view = render(<Field label="Descrição do obstáculo" />);
  expect(view.getByLabelText("Descrição do obstáculo")).toBeTruthy();
});
