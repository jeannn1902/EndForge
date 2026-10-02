using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EndForge.Data
{
    // Design-time factory to allow 'dotnet ef' tools to create the DbContext.
    public class EndForgeDbContextFactory : IDesignTimeDbContextFactory<EndForgeDbContext>
    {
        public EndForgeDbContext CreateDbContext(string[] args)
        {
            // Try read connection from environment variable first, then appsettings.json near the project, then fallback to LocalDB.
            string? connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Look for appsettings.json in the project directory (where the tools run)
                var basePath = Directory.GetCurrentDirectory();
                var config = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                connectionString = config.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=EndForgeCloudDev;Trusted_Connection=True;Connection Timeout=3;";
            }

            var optionsBuilder = new DbContextOptionsBuilder<EndForgeDbContext>();
            optionsBuilder.UseSqlServer(connectionString, b => b.EnableRetryOnFailure());

            return new EndForgeDbContext(optionsBuilder.Options);
        }
    }
}
