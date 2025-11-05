using Core.Domain.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Identity.Domain.Entities;

namespace Authentication.Infrastructure.Data
{
    // فقط User-centric
    public class IdentityDbContext
          : IdentityDbContext<ApplicationUser , ApplicationRole, Guid>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await base.SaveChangesAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("auth");
            builder.ApplyConfiguration(new OutboxMessageConfiguration("identity"));

            // User
            builder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("AspNetUsers", "identity");
                b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
                b.Property(u => u.FullName).HasMaxLength(200);
                b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
                b.HasIndex(u => u.FkPersonId).IsUnique();
                b.Property(u => u.FkPersonId).IsRequired();
            });

            // جداول مرتبط با User (IdentityUserContext این‌ها را پشتیبانی می‌کند)
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", "identity");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", "identity");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", "identity");

            // RefreshToken
            builder.Entity<RefreshToken>(b =>
            {
                b.ToTable("RefreshTokens", "identity");
                b.HasKey(r => r.Id);
                b.Property(r => r.Token).IsRequired().HasMaxLength(450).IsUnicode(false);
                b.HasOne(r => r.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // UserSession
            builder.Entity<UserSession>(b =>
            {
                b.ToTable("UserSessions", "identity");
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
