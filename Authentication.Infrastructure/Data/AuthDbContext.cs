using Authentication.Domain.Entities;
using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Infrastructure.Data
{
    // فقط User-centric tables در این DbContext
    public class AuthDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;

        // IUnitOfWork implementation
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // اسکیمای Authentication
            builder.HasDefaultSchema("auth");
            builder.ApplyConfiguration(new OutboxMessageConfiguration("auth"));

            // User
            builder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("AspNetUsers", "auth");
                b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
                b.Property(u => u.FullName).HasMaxLength(200);
                b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
                b.HasIndex(u => u.FkPersonId).IsUnique();
                b.Property(u => u.FkPersonId).IsRequired();
            });

            // جداول مرتبط با User
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", "auth");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", "auth");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", "auth");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", "auth");

            // RefreshToken
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
            builder.Entity<RefreshToken>().HasIndex(r => r.Token).IsUnique(false);

            // UserSession
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
