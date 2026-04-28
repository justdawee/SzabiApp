namespace SzabiApp.Backend.Common;

public sealed class DemoSeederOptions
{
    public const string SectionName = "DemoMode";

    /// <summary>
    /// When true, the demo data seeder runs once on startup (only if the database is empty).
    /// Set via appsettings.json (DemoMode:Enabled) or env var DemoMode__Enabled=true.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Apply EF Core migrations automatically on startup before seeding.
    /// Useful for docker-compose / presentation setups where migrations weren't run by hand.
    /// </summary>
    public bool ApplyMigrations { get; init; } = true;

    /// <summary>
    /// Default password assigned to all seeded demo users (hashed with the same hasher as production).
    /// </summary>
    public string DefaultPassword { get; init; } = "Demo123!";

    /// <summary>
    /// Total number of seeded employee users (excluding admin/managers).
    /// </summary>
    public int EmployeeCount { get; init; } = 35;

    /// <summary>
    /// Total number of seeded manager users.
    /// </summary>
    public int ManagerCount { get; init; } = 4;
}
