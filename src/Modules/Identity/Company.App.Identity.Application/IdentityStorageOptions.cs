namespace Company.App.Identity.Application;

public sealed class IdentityStorageOptions
{
    public const string SectionName = "Identity:Storage";

    public string ConnectionString { get; set; } = "Data Source=sadathems-identity.db";
}
