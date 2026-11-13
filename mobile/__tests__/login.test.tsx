import { fireEvent, render, waitFor } from "@testing-library/react-native";
import Login from "../app/login";
import { request, setSession } from "../src/api";

jest.mock(
  "react-native-safe-area-context",
  () => jest.requireActual("react-native-safe-area-context/jest/mock").default,
);
jest.mock("../src/api", () => ({ request: jest.fn(), setSession: jest.fn() }));

beforeEach(() => jest.clearAllMocks());

function fillRegistration(
  view: ReturnType<typeof render>,
  confirmation = "Senha-teste-123!",
) {
  fireEvent.press(view.getByRole("button", { name: "Criar conta" }));
  fireEvent.changeText(view.getByLabelText("Nome"), " Pessoa de teste ");
  fireEvent.changeText(view.getByLabelText("E-mail"), " PESSOA@EXAMPLE.TEST ");
  fireEvent.changeText(view.getByLabelText("Senha"), "Senha-teste-123!");
  fireEvent.changeText(view.getByLabelText("Confirmar senha"), confirmation);
}

test("account creation sends normalized data and starts the returned session", async () => {
  const session = {
    accessToken: "test-access",
    refreshToken: "test-refresh",
    user: {
      id: "test-user",
      name: "Pessoa de teste",
      email: "pessoa@example.test",
      role: "Citizen",
    },
  };
  jest.mocked(request).mockResolvedValueOnce(session);
  jest.mocked(setSession).mockResolvedValue(undefined);
  const view = render(<Login />);
  fillRegistration(view);
  fireEvent.press(view.getByRole("button", { name: "Criar minha conta" }));
  await waitFor(() => expect(setSession).toHaveBeenCalledWith(session));
  expect(request).toHaveBeenCalledTimes(1);
  expect(request).toHaveBeenCalledWith("/auth/register", {
    method: "POST",
    body: JSON.stringify({
      name: "Pessoa de teste",
      email: "pessoa@example.test",
      password: "Senha-teste-123!",
    }),
  });
});

test("mismatched passwords do not create an account", () => {
  const view = render(<Login />);
  fillRegistration(view, "Outra-senha-123!");
  fireEvent.press(view.getByRole("button", { name: "Criar minha conta" }));
  expect(view.getByText(/As senhas não conferem/)).toBeTruthy();
  expect(request).not.toHaveBeenCalled();
});

test("duplicate account errors remain visible without starting a session", async () => {
  jest
    .mocked(request)
    .mockRejectedValueOnce(new Error("Este e-mail já está cadastrado."));
  const view = render(<Login />);
  fillRegistration(view);
  fireEvent.press(view.getByRole("button", { name: "Criar minha conta" }));
  await waitFor(() =>
    expect(view.getByText("Este e-mail já está cadastrado.")).toBeTruthy(),
  );
  expect(setSession).not.toHaveBeenCalled();
  expect(
    view.getByRole("button", { name: "Criar minha conta" }),
  ).not.toBeDisabled();
});

test("entry screen offers account creation with neutral labels", () => {
  const view = render(<Login />);
  expect(view.getByRole("button", { name: "Criar conta" })).toBeTruthy();
  expect(view.getByLabelText("E-mail")).toBeTruthy();
  expect(view.queryByText(/fictíci|protótipo|acadêmico/i)).toBeNull();
});
