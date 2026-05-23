using BuildingBlocks.Core;

namespace Company.App.Identity.Domain;

public sealed class RoleGroup : Entity
{
    public RoleGroup(string name)
    {
        Rename(name);
    }

    public string Name { get; private set; } = string.Empty;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role group name is required.", nameof(name));
        }

        Name = name.Trim();
    }
}
