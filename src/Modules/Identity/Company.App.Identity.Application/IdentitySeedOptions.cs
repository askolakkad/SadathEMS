namespace Company.App.Identity.Application;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "Identity:Seed";

    public string AdminRole { get; set; } = "Administrator";

    public string AdminEmail { get; set; } = "admin@sadathems.local";

    public string AdminPassword { get; set; } = "Admin@12345";
}
