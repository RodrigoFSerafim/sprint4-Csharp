using BetControlAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BetControlAPI.Data
{
    /// <summary>
    /// Contexto do Entity Framework para o banco de dados da aplicação
    /// Define as entidades e seus relacionamentos
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Inicializa o contexto com as opções de configuração
        /// </summary>
        /// <param name="options">Opções de configuração do DbContext</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Tabela de usuários do sistema
        /// </summary>
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        
        /// <summary>
        /// Tabela de apostas realizadas pelos usuários
        /// </summary>
        public DbSet<Aposta> Apostas => Set<Aposta>();
        
        /// <summary>
        /// Tabela de limites mensais de apostas por usuário
        /// </summary>
        public DbSet<Limite> Limites => Set<Limite>();

        /// <summary>
        /// Configuração do modelo de dados e relacionamentos
        /// </summary>
        /// <param name="modelBuilder">Builder para configuração do modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Email do usuário deve ser único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Relacionamento Usuario -> Apostas (1:N)
            // Ao deletar usuário, deleta suas apostas (Cascade)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Apostas)
                .WithOne(a => a.Usuario!)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento Usuario -> Limites (1:N)
            // Ao deletar usuário, deleta seus limites (Cascade)
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Limites)
                .WithOne(l => l.Usuario!)
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único composto: um usuário só pode ter um limite por mês
            modelBuilder.Entity<Limite>()
                .HasIndex(l => new { l.UsuarioId, l.MesReferencia })
                .IsUnique();
        }
    }
}


