using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PassoSeguro.Domain;
using System.Text.RegularExpressions;
namespace PassoSeguro.Infrastructure;

public static class DemoSeed
{
    public static async Task Apply(AppDbContext db, string password, CancellationToken ct = default)
    {
        if (password.Length < 8) throw new InvalidOperationException("Demo:Password deve ter ao menos 8 caracteres.");
        if (await db.Citizens.AnyAsync(ct))
        {
            // Repair only fictitious seed timestamps from the first development run.
            var samples = await db.Occurrences.Include(x => x.History).Where(x => x.RequestFingerprint == "seed").ToListAsync(ct);
            foreach (var sample in samples)
            {
                var description = Regex.Replace(sample.Description, @"^Dado fictício de demonstração \d+: ", "");
                if (description != sample.Description)
                    db.Entry(sample).Property(x => x.Description).CurrentValue = description;
                var firstTransition = sample.History.Where(x => x.Status != OccurrenceStatus.Open).Min(x => (DateTimeOffset?)x.CreatedAt);
                if (firstTransition.HasValue && firstTransition.Value < sample.CreatedAt)
                {
                    var created = firstTransition.Value.AddHours(-1);
                    db.Entry(sample).Property(x => x.CreatedAt).CurrentValue = created;
                    foreach (var start in sample.History.Where(x => x.Status == OccurrenceStatus.Open))
                        db.Entry(start).Property(x => x.CreatedAt).CurrentValue = created;
                }
            }
            await db.SaveChangesAsync(ct);
            return;
        }
        var citizen = new Citizen { Name = "Cidadã demonstração", Email = "cidadao@passoseguro.test" };
        var manager = new Citizen { Name = "Gestora demonstração", Email = "gestor@passoseguro.test", Role = "Manager" };
        var hasher = new PasswordHasher<Citizen>();
        citizen.PasswordHash = hasher.HashPassword(citizen, password); manager.PasswordHash = hasher.HashPassword(manager, password);
        db.Citizens.AddRange(citizen, manager);
        string[] names = ["Calçada danificada", "Rampa bloqueada", "Ausência de rampa", "Obstáculo na passagem", "Travessia sem acessibilidade"];
        for (var i = 0; i < names.Length; i++) db.CategorySet.Add(new Category { Id = i + 1, Name = names[i] });
        var territory = new Territory();
        for (var i = 0; i < 15; i++)
        {
            var lat = -23.5590 + (i % 5) * .0003; var lon = -46.6470 + (i / 5) * .0003;
            if (!territory.Contains(lat, lon)) throw new InvalidOperationException("Seed fora do território.");
            var occurrence = Occurrence.Create(citizen.Id, i % 5 + 1, "Passagem com dificuldade de acesso.", i % 5 + 1,
                lat, lon, DateTimeOffset.UtcNow.AddDays(-i - 1), Guid.NewGuid(), "seed");
            if (i % 3 == 0) occurrence.ChangeStatus(true, manager.Id, OccurrenceStatus.InReview, DateTimeOffset.UtcNow.AddHours(-2));
            if (i % 6 == 0) occurrence.ChangeStatus(true, manager.Id, OccurrenceStatus.Resolved, DateTimeOffset.UtcNow.AddHours(-1));
            db.Occurrences.Add(occurrence);
        }
        await db.SaveChangesAsync(ct);
    }
}
