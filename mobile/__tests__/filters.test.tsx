import { fireEvent, render } from "@testing-library/react-native";
import { FilterCarousel } from "../src/FilterCarousel";
import { StatusBadge } from "../src/OccurrenceStatus";

const options = [
  { value: "", label: "Todos" },
  { value: "Open", label: "Aberta" },
  { value: "InReview", label: "Em análise" },
];
test("filters scroll horizontally and identify the selected option", () => {
  const onChange = jest.fn();
  const view = render(
    <FilterCarousel
      label="Andamento"
      options={options}
      value="Open"
      onChange={onChange}
    />,
  );
  expect(view.getByLabelText("Opções de Andamento").props.horizontal).toBe(
    true,
  );
  expect(
    view.getByRole("button", { name: "Andamento: Aberta" }).props
      .accessibilityState.selected,
  ).toBe(true);
  fireEvent.press(view.getByRole("button", { name: "Andamento: Em análise" }));
  expect(onChange).toHaveBeenCalledWith("InReview");
});
test("disabled filter options cannot change a pending record", () => {
  const onChange = jest.fn();
  const view = render(
    <FilterCarousel
      label="Andamento"
      options={options}
      value="Open"
      disabled
      onChange={onChange}
    />,
  );
  fireEvent.press(view.getByRole("button", { name: "Andamento: Em análise" }));
  expect(onChange).not.toHaveBeenCalled();
});
test.each(["Open", "InReview", "Resolved"] as const)(
  "status %s has a label independent of its color",
  (status) => {
    const view = render(<StatusBadge status={status} />);
    expect(view.getByLabelText(/Andamento:/)).toBeTruthy();
  },
);
