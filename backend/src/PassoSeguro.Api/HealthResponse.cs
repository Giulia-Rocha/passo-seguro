namespace PassoSeguro.Api;

/// <summary>Contrato da verificação de saúde; Database só é informado quando disponível.</summary>
public sealed record HealthResponse(string Status, string? Database = null);
