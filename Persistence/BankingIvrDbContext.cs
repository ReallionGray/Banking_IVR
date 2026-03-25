using Banking_IVR.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking_IVR.Persistence;

public class BankingIvrDbContext(DbContextOptions<BankingIvrDbContext> options) : DbContext(options)
{
    public DbSet<UssdSetting> UssdSettings => Set<UssdSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UssdSetting>();
        entity.ToTable("USSD_Setting");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.MSISDN).HasMaxLength(32).IsRequired();
        entity.Property(x => x.Language).HasMaxLength(16).IsRequired();
        entity.Property(x => x.Status).IsRequired();
        entity.HasIndex(x => x.MSISDN).IsUnique();
    }
}
