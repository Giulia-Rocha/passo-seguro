namespace PassoSeguro.Domain;

public sealed class RuleException(string message) : Exception(message);
public enum OccurrenceStatus { Open, InReview, Resolved }
public sealed class Citizen
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Citizen";
}
public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
}
public sealed class RefreshSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CitizenId { get; set; }
    public Citizen Citizen { get; set; } = null!;
    public string TokenHash { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public uint Version { get; set; }
}
public sealed class Occurrence
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AuthorId { get; private set; }
    public Citizen Author { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string Description { get; private set; } = "";
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public int Difficulty { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public OccurrenceStatus Status { get; private set; } = OccurrenceStatus.Open;
    public Guid RequestKey { get; private set; }
    public string RequestFingerprint { get; private set; } = "";
    public uint Version { get; set; }
    public List<StatusEntry> History { get; private set; } = [];
    public OccurrencePhoto? Photo { get; private set; }
    private Occurrence() { }
    public static Occurrence Create(Guid author, int category, string description, int difficulty,
        double latitude, double longitude, DateTimeOffset now, Guid requestKey, string fingerprint)
    {
        if (author == Guid.Empty || requestKey == Guid.Empty) throw new RuleException("Autor e chave de envio são obrigatórios.");
        var value = new Occurrence { AuthorId = author, CreatedAt = now, RequestKey = requestKey, RequestFingerprint = fingerprint };
        value.SetDetails(category, description, difficulty, latitude, longitude);
        value.History.Add(new StatusEntry(value.Id, author, OccurrenceStatus.Open, now));
        return value;
    }
    public void Edit(Guid actor, int category, string description, int difficulty, double latitude, double longitude)
    {
        EnsureEditable(actor);
        SetDetails(category, description, difficulty, latitude, longitude);
    }
    private void SetDetails(int category, string description, int difficulty, double latitude, double longitude)
    {
        if (category <= 0) throw new RuleException("Escolha uma categoria.");
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length is < 10 or > 2000)
            throw new RuleException("Descreva a situação com 10 a 2000 caracteres.");
        if (difficulty is < 1 or > 5) throw new RuleException("A dificuldade deve estar entre 1 e 5.");
        if (!double.IsFinite(latitude) || !double.IsFinite(longitude) || latitude is < -90 or > 90 || longitude is < -180 or > 180)
            throw new RuleException("Informe coordenadas válidas.");
        CategoryId = category; Description = description.Trim(); Difficulty = difficulty; Latitude = latitude; Longitude = longitude;
    }
    public void EnsureEditable(Guid actor)
    {
        if (actor != AuthorId) throw new RuleException("Somente o autor pode alterar esta ocorrência.");
        if (Status != OccurrenceStatus.Open) throw new RuleException("A ocorrência só pode ser editada ou excluída enquanto aberta.");
    }
    public void ChangeStatus(bool isManager, Guid actor, OccurrenceStatus next, DateTimeOffset now)
    {
        if (!isManager) throw new RuleException("Somente gestores podem alterar o andamento.");
        if (!((Status == OccurrenceStatus.Open && next == OccurrenceStatus.InReview) ||
              (Status == OccurrenceStatus.InReview && next is OccurrenceStatus.Open or OccurrenceStatus.Resolved)))
            throw new RuleException("Transição de status não permitida.");
        Status = next; History.Add(new StatusEntry(Id, actor, next, now));
    }
}
public sealed class StatusEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OccurrenceId { get; private set; }
    public Guid ActorId { get; private set; }
    public Citizen Actor { get; private set; } = null!;
    public OccurrenceStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    private StatusEntry() { }
    public StatusEntry(Guid occurrenceId, Guid actorId, OccurrenceStatus status, DateTimeOffset now)
    { OccurrenceId = occurrenceId; ActorId = actorId; Status = status; CreatedAt = now; }
}
public sealed class OccurrencePhoto
{
    public Guid OccurrenceId { get; set; }
    public Occurrence Occurrence { get; set; } = null!;
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = "";
}
public sealed record PriorityResult(int Score, string Level, int SeverityPoints, int RecurrencePoints, int AgePoints);
public static class PriorityCalculator
{
    public static PriorityResult Calculate(Occurrence value, IEnumerable<Occurrence> all, DateTimeOffset now)
    {
        if (value.Status == OccurrenceStatus.Resolved) return new(0, "Resolvida", 0, 0, 0);
        var similar = all.Count(x => x.Id != value.Id && x.CategoryId == value.CategoryId &&
            x.Status != OccurrenceStatus.Resolved && x.CreatedAt >= now.AddDays(-30) && x.CreatedAt <= now &&
            Distance(value.Latitude, value.Longitude, x.Latitude, x.Longitude) <= 200);
        var severity = value.Difficulty * 12;
        var recurrence = Math.Min(similar * 5, 25);
        var age = Math.Clamp((int)(now - value.CreatedAt).TotalDays, 0, 15);
        var score = severity + recurrence + age;
        return new(score, score < 35 ? "Baixa" : score < 60 ? "Média" : score < 80 ? "Alta" : "Crítica", severity, recurrence, age);
    }
    public static double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        const double rad = Math.PI / 180;
        var a = Math.Pow(Math.Sin((lat2-lat1)*rad/2),2) + Math.Cos(lat1*rad)*Math.Cos(lat2*rad)*Math.Pow(Math.Sin((lon2-lon1)*rad/2),2);
        return 6371000 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
}
