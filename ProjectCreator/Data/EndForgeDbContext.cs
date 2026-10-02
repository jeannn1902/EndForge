using Microsoft.EntityFrameworkCore;
using EndForge.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace EndForge.Data
{
    public class EndForgeDbContext : DbContext
    {

        public EndForgeDbContext(DbContextOptions<EndForgeDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;

        public DbSet<ProgresoUsuarioDb> ProgresoUsuarioDbs { get; set; } = null!;

        public DbSet<EvaluacionDb> EvaluacionDbs { get; set; } = null!;

        public DbSet<ConcesionXPDb> ConcesionXPDbs { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string? connectionString = null;
                try
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .Build();

                    connectionString = config.GetConnectionString("DefaultConnection");
                }
                catch
                {
                }

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = "Server=(localdb)\\mssqllocaldb;Database=EndForgeCloudDev;Trusted_Connection=True;Connection Timeout=3;";
                }
                else
                {
                    try
                    {
                        var csb = new SqlConnectionStringBuilder(connectionString)
                        {
                            ConnectTimeout = 3
                        };
                        connectionString = csb.ConnectionString;
                    }
                    catch
                    {
                    }
                }

                optionsBuilder.UseSqlServer(connectionString, b => b.EnableRetryOnFailure());
            }
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
