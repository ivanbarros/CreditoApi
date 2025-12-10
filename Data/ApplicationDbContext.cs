using CreditoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditoAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Credito> Creditos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Credito>(entity =>
            {
                entity.ToTable("credito");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                // Configure decimal precision
                entity.Property(e => e.ValorIssqn).HasColumnType("decimal(15,2)");
                entity.Property(e => e.Aliquota).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ValorFaturado).HasColumnType("decimal(15,2)");
                entity.Property(e => e.ValorDeducao).HasColumnType("decimal(15,2)");
                entity.Property(e => e.BaseCalculo).HasColumnType("decimal(15,2)");
                
                entity.HasIndex(e => e.NumeroCredito).HasDatabaseName("idx_numero_credito");
                entity.HasIndex(e => e.NumeroNfse).HasDatabaseName("idx_numero_nfse");
            });
        }
    }
}
