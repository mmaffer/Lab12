using Microsoft.EntityFrameworkCore;
using Sem12.Models;

namespace Sem12.Data;

public class GymnasioContext(DbContextOptions<GymnasioContext> options) : DbContext(options)
{
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Membresia> Membresias => Set<Membresia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Socio>(entity =>
        {
            entity.ToTable("Socios");
            entity.HasIndex(p => p.Dni).IsUnique().HasDatabaseName("UQ_Socios_Dni");
            entity.HasIndex(p => p.Email).IsUnique().HasDatabaseName("UQ_Socios_Email");
        });
        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.ToTable("Membresias");
            entity.Property(m => m.Precio).HasPrecision(7, 2);
            entity.HasOne(m => m.Socio).WithMany(p => p.Membresias)
                .HasForeignKey(m => m.IdSocio).OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Membresias_Socios");
        });
    }
}
