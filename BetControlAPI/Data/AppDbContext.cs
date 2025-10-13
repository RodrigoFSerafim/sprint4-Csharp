using BetControlAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BetControlAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Aposta> Apostas => Set<Aposta>();
        public DbSet<Limite> Limites => Set<Limite>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Apostas)
                .WithOne(a => a.Usuario!)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Limites)
                .WithOne(l => l.Usuario!)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Limite>()
                .HasIndex(l => new { l.UsuarioId, l.MesReferencia })
                .IsUnique();
        }
    }
}


