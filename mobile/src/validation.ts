import { OccurrenceInput } from "./types";
export function validateOccurrence(input: OccurrenceInput) {
  if ((input.address?.length ?? 0) > 300)
    return "O endereço deve ter no máximo 300 caracteres.";
  if (!Number.isInteger(input.categoryId) || input.categoryId < 1)
    return "Escolha uma categoria.";
  if (
    input.description.trim().length < 10 ||
    input.description.trim().length > 2000
  )
    return "Descreva o problema com 10 a 2000 caracteres.";
  if (
    !Number.isInteger(input.difficulty) ||
    input.difficulty < 1 ||
    input.difficulty > 5
  )
    return "Selecione uma dificuldade de 1 a 5.";
  if (
    !Number.isFinite(input.latitude) ||
    !Number.isFinite(input.longitude) ||
    Math.abs(input.latitude) > 90 ||
    Math.abs(input.longitude) > 180
  )
    return "Informe uma localização válida.";
  return "";
}
