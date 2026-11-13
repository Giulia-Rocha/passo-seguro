import * as Location from "expo-location";
import { Platform } from "react-native";

export interface Coordinates {
  latitude: number;
  longitude: number;
}
async function withTimeout<T>(operation: Promise<T>): Promise<T> {
  let timer: ReturnType<typeof setTimeout> | undefined;
  try {
    return await Promise.race([
      operation,
      new Promise<never>((_, reject) => {
        timer = setTimeout(
          () =>
            reject(
              new Error(
                "A localização demorou para responder. Tente novamente ou marque o ponto no mapa.",
              ),
            ),
          20000,
        );
      }),
    ]);
  } finally {
    if (timer) clearTimeout(timer);
  }
}
async function requestPermission() {
  const permission = await Location.requestForegroundPermissionsAsync();
  if (!permission.granted)
    throw new Error(
      "Permissão de localização negada. Você pode marcar o ponto no mapa ou habilitar a permissão nos Ajustes.",
    );
}
export async function currentCoordinates(): Promise<Coordinates> {
  await requestPermission();
  if (!(await Location.hasServicesEnabledAsync()))
    throw new Error(
      "Ative os serviços de localização do aparelho e tente novamente.",
    );
  const position = await withTimeout(
    Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.High }),
  );
  return {
    latitude: position.coords.latitude,
    longitude: position.coords.longitude,
  };
}
export async function findAddress(address: string): Promise<Coordinates[]> {
  if (address.trim().length < 5)
    throw new Error("Informe rua e número para localizar o endereço.");
  if (Platform.OS === "android") await requestPermission();
  const query = `${address.trim()}, Bela Vista, São Paulo, Brasil`;
  const results = await withTimeout(Location.geocodeAsync(query));
  if (!results.length)
    throw new Error(
      "Endereço não encontrado. Confira rua e número ou selecione o ponto no mapa.",
    );
  return results;
}
