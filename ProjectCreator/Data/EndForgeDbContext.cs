using Microsoft.EntityFrameworkCore;
using EndForge.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace EndForge.Data
{
    public class EndForgeDbContext : DbContext
    {
        public EndForgeDbContext()
        {
        }

        public EndForgeDbContext(DbContextOptions<EndForgeDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;

        public DbSet<ProgresoUsuarioDb> ProgresoUsuarioDbs { get; set; } = null!;

        public DbSet<EvaluacionDb> EvaluacionDbs { get; set; } = null!;

        public DbSet<ConcesionXPDb> ConcesionXPDbs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // La configuración se inyecta externamente (DI/fábrica de tiempo de diseño).
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasMany(u => u.Progresos).WithOne(p => p.Usuario).HasForeignKey(p => p.UsuarioId);
                b.HasMany(u => u.Evaluaciones).WithOne(e => e.Usuario).HasForeignKey(e => e.UsuarioId);
                b.HasMany(u => u.ConcesionesXP).WithOne(c => c.Usuario).HasForeignKey(c => c.UsuarioId);
            });
        }
    }
}
