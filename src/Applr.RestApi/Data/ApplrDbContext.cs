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

    public DbSet<ProfileField> ProfileFields => Set<ProfileField>();

    public DbSet<ProfileFieldPattern> ProfileFieldPatterns => Set<ProfileFieldPattern>();

    public DbSet<ProfileValue> ProfileValues => Set<ProfileValue>();

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

        // profile_values is keyed on (user, field, rank), not a surrogate id --
        // one answer per rank, several ranks per field, enforced by the
        // database rather than by remembering to check first. Composite keys
        // can't be expressed with [Key] annotations, so this has to be Fluent API.
        modelBuilder.Entity<ProfileValue>()
            .HasKey(v => new { v.UserId, v.FieldKey, v.ValueRank });

        // Mirrors the UNIQUE from the migration SQL. domain is an empty
        // string rather than NULL for global patterns precisely so this
        // catches duplicates: NULL never equals NULL, so a nullable column
        // would let two identical global rows both through.
        modelBuilder.Entity<ProfileFieldPattern>()
            .HasIndex(p => new { p.FieldKey, p.Pattern, p.Domain })
            .IsUnique();

        // Ordering is the matching mechanism, so two fields sharing a slot
        // would make which pattern claims an input non-deterministic.
        modelBuilder.Entity<ProfileField>()
            .HasIndex(f => f.MatchOrder)
            .IsUnique();

        // created_on / updated_on are the database's to fill (DEFAULT
        // CURRENT_TIMESTAMP, ON UPDATE for updated_on). Without this EF sends
        // the CLR default, 0001-01-01, and overrides the column default.
        // Marked store-generated, EF leaves the column out of the INSERT when
        // no value was set; an explicit value (JobSyncService) is still sent.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.FindProperty("CreatedOn") is not null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedOn")
                    .ValueGeneratedOnAdd();
            }

            if (entityType.FindProperty("UpdatedOn") is not null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedOn")
                    .ValueGeneratedOnAddOrUpdate();
            }
        }
    }
}
