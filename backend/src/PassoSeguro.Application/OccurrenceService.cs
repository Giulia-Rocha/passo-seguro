using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PassoSeguro.Domain;

namespace PassoSeguro.Application;

public sealed class OccurrenceService(IOccurrenceStore store, ITerritory territory, TimeProvider clock)
{
    public async Task<OccurrenceResponse> Create(Guid actor, Guid key, OccurrenceRequest request, CancellationToken ct)
    {
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request))));
        var previous = await store.FindRequest(actor, key, ct);
        if (previous != null)
        {
            if (previous.RequestFingerprint != fingerprint) throw new ConflictException("Esta chave de envio já foi usada com outros dados.");
            return Map(previous, await store.List(ct));
        }
        await Validate(request, ct);
        var value = Occurrence.Create(actor, request.CategoryId, request.Description, request.Difficulty,
            request.Latitude, request.Longitude, clock.GetUtcNow(), key, fingerprint);
        store.Add(value); await store.Save(ct);
        return Map(value, await store.List(ct));
    }
    public async Task<OccurrenceResponse> Edit(Guid id, Guid actor, OccurrenceRequest request, CancellationToken ct)
    {
        var value = await Required(id, ct);
        if (value.AuthorId != actor) throw new ForbiddenException();
        await Validate(request, ct);
        value.Edit(actor, request.CategoryId, request.Description, request.Difficulty, request.Latitude, request.Longitude);
        await store.Save(ct); return Map(value, await store.List(ct));
    }
    public async Task Delete(Guid id, Guid actor, CancellationToken ct)
    {
        var value = await Required(id, ct);
        if (value.AuthorId != actor) throw new ForbiddenException();
        value.EnsureEditable(actor); store.Remove(value); await store.Save(ct);
    }
    public async Task<OccurrenceResponse> ChangeStatus(Guid id, Guid actor, bool manager, OccurrenceStatus status, CancellationToken ct)
    {
        if (!manager) throw new ForbiddenException();
        var value = await Required(id, ct);
        value.ChangeStatus(manager, actor, status, clock.GetUtcNow()); await store.Save(ct);
        return Map(value, await store.List(ct));
    }
    public async Task<Occurrence> Required(Guid id, CancellationToken ct) => await store.Find(id, ct) ?? throw new MissingException();
    public async Task<OccurrenceResponse> Detail(Guid id, CancellationToken ct) => Map(await Required(id, ct), await store.List(ct));
    public async Task<PagedResponse<OccurrenceResponse>> List(OccurrenceFilter filter, Guid actor, CancellationToken ct)
    {
        var items = await Filter(filter, actor, ct);
        return new(items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(), items.Count, filter.Page, filter.PageSize);
    }
    public async Task<IndicatorsResponse> Indicators(OccurrenceFilter filter, Guid actor, CancellationToken ct)
    {
        var items = await Filter(filter, actor, ct);
        static List<CountResponse> Group(IEnumerable<string> values) => values.GroupBy(x => x).Select(x => new CountResponse(x.Key, x.Count())).OrderByDescending(x => x.Count).ToList();
        return new(items.Count, Group(items.Select(x=>x.Category)), Group(items.Select(x=>x.Status.ToString())),
            Group(items.Select(x=>x.Priority.Level)), items.GroupBy(x=>x.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd"))
                .OrderBy(x=>x.Key).Select(x=>new CountResponse(x.Key,x.Count())).ToList(),
            items.GroupBy(x=>new { Lat = Math.Floor(x.Latitude / .0018), Lon = Math.Floor(x.Longitude / .00196) })
                .Select(x=>new ConcentrationResponse((x.Key.Lat+.5)*.0018,(x.Key.Lon+.5)*.00196,x.Count())).ToList());
    }
    private async Task<List<OccurrenceResponse>> Filter(OccurrenceFilter filter, Guid actor, CancellationToken ct)
    {
        if (filter.From > filter.To) throw new RuleException("O início do período deve preceder o fim.");
        var all = await store.List(ct);
        var values = all.Where(x=>(!filter.CategoryId.HasValue || x.CategoryId == filter.CategoryId) &&
            (!filter.Status.HasValue || x.Status == filter.Status) && (!filter.Difficulty.HasValue || x.Difficulty == filter.Difficulty) &&
            (!filter.From.HasValue || x.CreatedAt >= filter.From) && (!filter.To.HasValue || x.CreatedAt <= filter.To) && (!filter.Mine || x.AuthorId == actor))
            .Select(x=>Map(x,all)).Where(x=>string.IsNullOrEmpty(filter.Priority) || x.Priority.Level == filter.Priority);
        return (filter.Sort == "priority" ? values.OrderByDescending(x=>x.Priority.Score).ThenBy(x=>x.CreatedAt) : values.OrderByDescending(x=>x.CreatedAt)).ToList();
    }
    private async Task Validate(OccurrenceRequest request, CancellationToken ct)
    {
        if (!territory.Contains(request.Latitude, request.Longitude)) throw new RuleException("Selecione um ponto dentro de Bela Vista.");
        if (!await store.CategoryActive(request.CategoryId, ct)) throw new RuleException("Categoria indisponível.");
    }
    private OccurrenceResponse Map(Occurrence value, List<Occurrence> all) => new(value.Id, value.AuthorId, value.CategoryId,
        value.Category?.Name ?? "", value.Description, value.Difficulty, value.Latitude, value.Longitude, value.CreatedAt, value.Status,
        PriorityCalculator.Calculate(value, all, clock.GetUtcNow()), value.Photo != null,
        value.History.OrderBy(x=>x.CreatedAt).Select(x=>new HistoryResponse(x.Status,x.CreatedAt)).ToList());
}
