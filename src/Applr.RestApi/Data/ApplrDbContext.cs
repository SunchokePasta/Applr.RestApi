using System.Text.Json;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Applr.RestApi.Data;

public sealed class ApplrDbContext(DbContextOptions<ApplrDbContext> options)
    : DbContext(options)
{
    public DbSet<RawJob> RawJobs => Set<RawJob>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // raw_cells is a MySQL `json` column holding a plain array of
        // strings. Data Annotations alone can't express "serialize this
        // as JSON", so this is Fluent API config RawJob needs.
        modelBuilder.Entity<RawJob>()
            .Property(r => r.RawCells)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null),
                new ValueComparer<List<string>?>(
                    (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                    v => v == null ? 0 : v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                    v => v == null ? null : v.ToList()));

        // Mirrors the UNIQUE constraints from the migration SQL -- kept
        // here too so the EF model matches the real schema, and so
        // SaveChangesAsync fails fast (DbUpdateException) on a duplicate
        // instead of relying purely on the DB round-trip to catch it.
        // No relationship config (.HasOne/.WithMany) on purpose: each
        // repository queries its own table only, nothing here joins.
        modelBuilder.Entity<Company>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Job>()
            .HasIndex(j => j.RawJobId)
            .IsUnique();
    }
}
