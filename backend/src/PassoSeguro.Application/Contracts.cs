using System.ComponentModel.DataAnnotations;
using PassoSeguro.Domain;

namespace PassoSeguro.Application;

public sealed record RegisterRequest([Required, StringLength(80, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(200)] string Email, [Required, StringLength(100, MinimumLength = 8)] string Password);
public sealed record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public sealed record RefreshRequest([Required] string RefreshToken);
public sealed record UserResponse(Guid Id, string Name, string Email, string Role);
public sealed record AuthResponse(string AccessToken, string RefreshToken, UserResponse User);
public sealed record OccurrenceRequest([Range(1, int.MaxValue)] int CategoryId,
    [Required, StringLength(2000, MinimumLength = 10)] string Description, [Range(1, 5)] int Difficulty,
    [Range(-90, 90)] double Latitude, [Range(-180, 180)] double Longitude, [StringLength(300)] string? Address = null);
public sealed record StatusRequest(OccurrenceStatus Status);
public sealed record CategoryResponse(int Id, string Name);
public sealed record HistoryResponse(OccurrenceStatus Status, DateTimeOffset CreatedAt);
public sealed record OccurrenceResponse(Guid Id, Guid AuthorId, int CategoryId, string Category, string Description,
    int Difficulty, double Latitude, double Longitude, DateTimeOffset CreatedAt, OccurrenceStatus Status,
    PriorityResult Priority, bool HasPhoto, IReadOnlyList<HistoryResponse> History, string? Address = null);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CountResponse(string Label, int Count);
public sealed record ConcentrationResponse(double Latitude, double Longitude, int Count);
public sealed record IndicatorsResponse(int Total, IReadOnlyList<CountResponse> Categories, IReadOnlyList<CountResponse> Statuses,
    IReadOnlyList<CountResponse> Priorities, IReadOnlyList<CountResponse> Timeline, IReadOnlyList<ConcentrationResponse> Concentrations);
public sealed class OccurrenceFilter
{
    public int? CategoryId { get; set; }
    public OccurrenceStatus? Status { get; set; }
    [Range(1, 5)] public int? Difficulty { get; set; }
    public string? Priority { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public bool Mine { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 30;
    [RegularExpression("^(recent|priority)$")] public string Sort { get; set; } = "recent";
}
public interface ITerritory
{
    bool Contains(double latitude, double longitude);
    object GeoJson { get; }
}
public interface IOccurrenceStore
{
    Task<List<Occurrence>> List(CancellationToken ct);
    Task<Occurrence?> Find(Guid id, CancellationToken ct);
    Task<Occurrence?> FindRequest(Guid author, Guid key, CancellationToken ct);
    Task<bool> CategoryActive(int id, CancellationToken ct);
    Task<List<Category>> Categories(CancellationToken ct);
    void Add(Occurrence occurrence);
    void Remove(Occurrence occurrence);
    Task Save(CancellationToken ct);
}
public sealed class MissingException() : Exception("Registro não encontrado.");
public sealed class ForbiddenException() : Exception("Você não tem permissão para esta operação.");
public sealed class ConflictException(string message) : Exception(message);
