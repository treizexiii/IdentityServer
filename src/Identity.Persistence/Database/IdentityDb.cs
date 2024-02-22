using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tools.TransactionsManager;

namespace Identity.Persistence.Database;

public class IdentityDb : DbContext, IDbContext
{
    public IdentityDb(DbContextOptions<IdentityDb> options)
        : base(options)
    {
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await Database.BeginTransactionAsync();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(b =>
        {
            b.HasIndex(u => u.UserName).IsUnique();
            b.HasIndex(u => u.NormalizedUserName).IsUnique();
        });
        builder.Entity<Role>(b =>
        {
            b.HasIndex(r => r.Name).IsUnique();
            b.HasIndex(r => r.NormalizedName).IsUnique();
            b.HasData(RolesList.GetRoles());
        });
        builder.Entity<RoleClaims>(b => { });
        builder.Entity<UserRole>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });
        });
        builder.Entity<UserClaim>(b => { });
        builder.Entity<UserToken>(b =>
        {
            b.HasIndex(ut => new { ut.UserId, ut.LoginProvider, ut.Name, ut.DeletedAt });
        });
        builder.Entity<UserLogin>(b =>
        {
            b.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
        });

        builder.Entity<App>(b =>
        {
            b.HasIndex(a => a.Key).IsUnique();
            b.HasIndex(a => a.NormalizedName).IsUnique();
        });
        builder.Entity<UserApp>(b =>
        {
            b.HasKey(ua => new { ua.UserId, ua.AppId });
        });

        base.OnModelCreating(builder);
    }

    public Task SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }

    public IDbContextTransaction? CurrentTransaction => Database.CurrentTransaction;

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<RoleClaims> RoleClaims { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<UserLogin> UserLogins { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<UserClaim> UserClaims { get; set; } = null!;
    public DbSet<UserApp> UserApps { get; set; } = null!;
    public DbSet<App> Apps { get; set; } = null!;
}