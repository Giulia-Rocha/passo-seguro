import { request, setSession, currentSession } from "../src/api";
jest.mock("expo-secure-store", () => ({
  setItemAsync: jest.fn(),
  deleteItemAsync: jest.fn(),
  getItemAsync: jest.fn(),
}));
const fetchMock = jest.fn();
const session = {
  accessToken: "old-access",
  refreshToken: "old-refresh",
  user: {
    id: "fictional-id",
    name: "Fictício",
    email: "user@example.test",
    role: "Citizen" as const,
  },
};
const response = (status: number, body: unknown) => ({
  status,
  ok: status >= 200 && status < 300,
  json: async () => body,
});
beforeEach(async () => {
  global.fetch = fetchMock;
  fetchMock.mockReset();
  await setSession(null);
});
test("reports an unavailable API as a recoverable network error", async () => {
  fetchMock.mockRejectedValue(new Error("network"));
  await expect(request("/occurrences")).rejects.toMatchObject({ status: 0 });
});
test("renews expired access before repeating the request", async () => {
  await setSession(session);
  fetchMock
    .mockResolvedValueOnce(response(401, {}))
    .mockResolvedValueOnce(
      response(200, {
        ...session,
        accessToken: "new-access",
        refreshToken: "new-refresh",
      }),
    )
    .mockResolvedValueOnce(response(200, { items: [] }));
  await expect(request("/occurrences")).resolves.toEqual({ items: [] });
  expect(fetchMock).toHaveBeenCalledTimes(3);
  expect(currentSession()?.accessToken).toBe("new-access");
});
test("clears local session when refresh has expired", async () => {
  await setSession(session);
  fetchMock
    .mockResolvedValueOnce(response(401, {}))
    .mockResolvedValueOnce(response(401, {}));
  await expect(request("/occurrences")).rejects.toMatchObject({ status: 401 });
  expect(currentSession()).toBeNull();
});
test("keeps session when refresh fails due to network loss", async () => {
  await setSession(session);
  fetchMock
    .mockResolvedValueOnce(response(401, {}))
    .mockRejectedValueOnce(new Error("offline"));
  await expect(request("/occurrences")).rejects.toMatchObject({ status: 0 });
  expect(currentSession()?.user.id).toBe(session.user.id);
});
test("surfaces domain message", async () => {
  fetchMock.mockResolvedValue(
    response(422, { title: "Selecione um ponto dentro de Bela Vista." }),
  );
  await expect(request("/occurrences")).rejects.toThrow("Bela Vista");
});
