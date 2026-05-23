using Company.App.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Company.App.Identity.Infrastructure;

public sealed class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<RoleGroup> RoleGroups => Set<RoleGroup>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RoleGroup>(entity =>
        {
            entity.HasKey(roleGroup => roleGroup.Id);
            entity.Property(roleGroup => roleGroup.Name)
                .HasMaxLength(128)
                .IsRequired();
        });
    }
}
