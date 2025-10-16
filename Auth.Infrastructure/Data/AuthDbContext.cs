using Auth.Infrastructure.Identity;
using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Auth.Infrastructure.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

        public DbSet<UserSession> UserSessions { get; set; } = default!;
        // IUnitOfWork implementation (if you keep it)
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Optionally: update UpdatedAt on ApplicationUser or other audit fields here
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use dedicated schema
            builder.HasDefaultSchema("auth");

            // Rename Identity tables if you want custom names (optional)
            builder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("AspNetUsers", "auth");
                b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
                b.Property(u => u.FullName).HasMaxLength(200);
                b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("AspNetRoles", "auth");
                b.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
            });

            // Remap other identity tables
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", "auth");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", "auth");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", "auth");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", "auth");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", "auth");

            // RefreshToken relation
            builder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens", "auth");
                b.HasKey(r => r.Id);
                b.Property(r => r.Token).IsRequired().HasMaxLength(450).IsUnicode(false);
                b.HasOne(r => r.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Additional indexes
            builder.Entity<RefreshToken>().HasIndex(r => r.Token).IsUnique(false);

            builder.Entity<UserSession>(b =>
            {
                b.ToTable("UserSessions", "auth");
                b.HasKey(s => s.Id);
                b.Property(s => s.RefreshToken).IsRequired().HasMaxLength(450);
                b.HasOne(s => s.User)
                 .WithMany(u => u.Sessions)
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Property(s => s.ExpiresAt).IsRequired();
            });
        }
    }
}
