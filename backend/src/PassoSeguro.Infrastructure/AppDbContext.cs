using Microsoft.EntityFrameworkCore;
using PassoSeguro.Application;
using PassoSeguro.Domain;

namespace PassoSeguro.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IOccurrenceStore
{
    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Category> CategorySet => Set<Category>();
    public DbSet<Occurrence> Occurrences => Set<Occurrence>();
    public DbSet<RefreshSession> Sessions => Set<RefreshSession>();
    public DbSet<OccurrencePhoto> Photos => Set<OccurrencePhoto>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Citizen>().HasIndex(x=>x.Email).IsUnique();
        b.Entity<Citizen>().Property(x=>x.Email).HasMaxLength(200);
        b.Entity<Citizen>().Property(x=>x.Name).HasMaxLength(80);
        b.Entity<Occurrence>().HasOne(x=>x.Author).WithMany().HasForeignKey(x=>x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Occurrence>().HasOne(x=>x.Category).WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Occurrence>().HasMany(x=>x.History).WithOne().HasForeignKey(x=>x.OccurrenceId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<StatusEntry>().HasOne(x=>x.Actor).WithMany().HasForeignKey(x=>x.ActorId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Occurrence>().Property(x=>x.Description).HasMaxLength(2000);
        b.Entity<Occurrence>().HasIndex(x=>new { x.AuthorId, x.RequestKey }).IsUnique();
        b.Entity<Occurrence>().HasIndex(x=>new { x.CategoryId, x.Status, x.CreatedAt });
        b.Entity<Occurrence>().Property(x=>x.Version).IsRowVersion();
        b.Entity<RefreshSession>().Property(x=>x.Version).IsRowVersion();
        b.Entity<RefreshSession>().HasIndex(x=>x.TokenHash).IsUnique();
        b.Entity<RefreshSession>().HasOne(x=>x.Citizen).WithMany().HasForeignKey(x=>x.CitizenId);
        b.Entity<OccurrencePhoto>().HasKey(x=>x.OccurrenceId);
        b.Entity<OccurrencePhoto>().HasOne(x=>x.Occurrence).WithOne(x=>x.Photo).HasForeignKey<OccurrencePhoto>(x=>x.OccurrenceId).OnDelete(DeleteBehavior.Cascade);
    }
    private IQueryable<Occurrence> Full => Occurrences.Include(x=>x.Category).Include(x=>x.History).Include(x=>x.Photo);
    public Task<List<Occurrence>> List(CancellationToken ct) => Full.OrderByDescending(x=>x.CreatedAt).ToListAsync(ct);
    public Task<Occurrence?> Find(Guid id, CancellationToken ct) => Full.FirstOrDefaultAsync(x=>x.Id == id,ct);
    public Task<Occurrence?> FindRequest(Guid author, Guid key, CancellationToken ct) => Full.FirstOrDefaultAsync(x=>x.AuthorId == author && x.RequestKey == key,ct);
    public Task<bool> CategoryActive(int id, CancellationToken ct) => CategorySet.AnyAsync(x=>x.Id == id && x.Active,ct);
    public Task<List<Category>> Categories(CancellationToken ct) => CategorySet.Where(x=>x.Active).OrderBy(x=>x.Id).ToListAsync(ct);
    void IOccurrenceStore.Add(Occurrence occurrence) => Occurrences.Add(occurrence);
    void IOccurrenceStore.Remove(Occurrence occurrence) => Occurrences.Remove(occurrence);
    public async Task Save(CancellationToken ct) => await SaveChangesAsync(ct);
}
