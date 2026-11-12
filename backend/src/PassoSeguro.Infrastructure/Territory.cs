using System.Reflection;
using System.Text.Json;
using PassoSeguro.Application;

namespace PassoSeguro.Infrastructure;

public sealed class Territory : ITerritory
{
    private readonly JsonElement data;
    public Territory()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PassoSeguro.Infrastructure.Data.bela-vista.geojson")
            ?? throw new InvalidOperationException("Limite territorial ausente.");
        using var json = JsonDocument.Parse(stream); data = json.RootElement.Clone();
        if (data.GetProperty("features").GetArrayLength() != 1) throw new InvalidOperationException("Limite territorial inválido.");
    }
    public object GeoJson => data;
    public bool Contains(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || !double.IsFinite(longitude)) return false;
        var geometry = data.GetProperty("features")[0].GetProperty("geometry");
        var coordinates = geometry.GetProperty("coordinates");
        return geometry.GetProperty("type").GetString() == "MultiPolygon"
            ? coordinates.EnumerateArray().Any(p => InPolygon(p, longitude, latitude)) : InPolygon(coordinates, longitude, latitude);
    }
    private static bool InPolygon(JsonElement rings, double x, double y) => InRing(rings[0], x, y) && !rings.EnumerateArray().Skip(1).Any(r => InRing(r, x, y));
    private static bool InRing(JsonElement ring, double x, double y)
    {
        var inside = false; var points = ring.EnumerateArray().Select(p => (X: p[0].GetDouble(), Y: p[1].GetDouble())).ToArray();
        for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
        {
            var a = points[i]; var b = points[j];
            var cross = (x - a.X) * (b.Y - a.Y) - (y - a.Y) * (b.X - a.X);
            if (Math.Abs(cross) < 1e-12 && x >= Math.Min(a.X, b.X) && x <= Math.Max(a.X, b.X) && y >= Math.Min(a.Y, b.Y) && y <= Math.Max(a.Y, b.Y)) return true;
            if ((a.Y > y) != (b.Y > y) && x < (b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X) inside = !inside;
        }
        return inside;
    }
}
