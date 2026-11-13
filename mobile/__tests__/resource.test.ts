import { act, renderHook, waitFor } from "@testing-library/react-native";
import { useResource } from "../src/useResource";
import { request } from "../src/api";
jest.mock("expo-router", () => ({
  useFocusEffect: (effect: () => void) =>
    jest
      .requireActual<typeof import("react")>("react")
      .useEffect(effect, [effect]),
}));
jest.mock("../src/session", () => ({
  useSession: () => ({ session: { user: { id: "test" } } }),
}));
jest.mock("../src/api", () => ({
  request: jest.fn(),
  ApiError: class extends Error {},
}));
jest.mock("../src/storage", () => ({
  storage: { cache: jest.fn().mockResolvedValue(undefined), read: jest.fn() },
}));
test("rapid filter changes keep the latest response even when an older query finishes last", async () => {
  let resolveFirst!: (value: string[]) => void;
  let resolveSecond!: (value: string[]) => void;
  jest
    .mocked(request)
    .mockReturnValueOnce(
      new Promise((resolve) => {
        resolveFirst = resolve;
      }),
    )
    .mockReturnValueOnce(
      new Promise((resolve) => {
        resolveSecond = resolve;
      }),
    );
  const hook = renderHook<
    ReturnType<typeof useResource<string[]>>,
    { path: string }
  >(({ path }) => useResource<string[]>(path), {
    initialProps: { path: "/occurrences?status=Open" },
  });
  hook.rerender({ path: "/occurrences?status=Resolved" });
  await act(async () => resolveSecond(["resolved"]));
  await waitFor(() => expect(hook.result.current.data).toEqual(["resolved"]));
  await act(async () => resolveFirst(["old-open"]));
  expect(hook.result.current.data).toEqual(["resolved"]);
  expect(hook.result.current.loading).toBe(false);
});
