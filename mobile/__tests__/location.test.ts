import * as Location from "expo-location";
import { currentCoordinates, findAddress } from "../src/location";
jest.mock("expo-location", () => ({
  requestForegroundPermissionsAsync: jest.fn(),
  hasServicesEnabledAsync: jest.fn(),
  getCurrentPositionAsync: jest.fn(),
  geocodeAsync: jest.fn(),
  Accuracy: { High: 4 },
}));

beforeEach(() => {
  jest.clearAllMocks();
  jest
    .mocked(Location.requestForegroundPermissionsAsync)
    .mockResolvedValue({
      granted: true,
    } as Location.LocationPermissionResponse);
  jest.mocked(Location.hasServicesEnabledAsync).mockResolvedValue(true);
});
test("current GPS coordinates become the occurrence point", async () => {
  jest
    .mocked(Location.getCurrentPositionAsync)
    .mockResolvedValue({
      coords: { latitude: -23.557, longitude: -46.646 },
    } as Location.LocationObject);
  await expect(currentCoordinates()).resolves.toEqual({
    latitude: -23.557,
    longitude: -46.646,
  });
});
test("denied permission does not request a GPS fix", async () => {
  jest
    .mocked(Location.requestForegroundPermissionsAsync)
    .mockResolvedValue({
      granted: false,
    } as Location.LocationPermissionResponse);
  await expect(currentCoordinates()).rejects.toThrow("Permissão");
  expect(Location.getCurrentPositionAsync).not.toHaveBeenCalled();
});
test("disabled GPS has actionable feedback", async () => {
  jest.mocked(Location.hasServicesEnabledAsync).mockResolvedValue(false);
  await expect(currentCoordinates()).rejects.toThrow("Ative os serviços");
});
test("address search is contextualized to the prototype region", async () => {
  jest
    .mocked(Location.geocodeAsync)
    .mockResolvedValue([{ latitude: -23.557, longitude: -46.646 }]);
  await expect(findAddress("Rua Treze de Maio, 100")).resolves.toHaveLength(1);
  expect(Location.geocodeAsync).toHaveBeenCalledWith(
    "Rua Treze de Maio, 100, Bela Vista, São Paulo, Brasil",
  );
});
test("unknown address does not silently replace coordinates", async () => {
  jest.mocked(Location.geocodeAsync).mockResolvedValue([]);
  await expect(findAddress("Rua inexistente, 100")).rejects.toThrow(
    "Endereço não encontrado",
  );
});
