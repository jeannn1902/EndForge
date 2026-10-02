using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EndForge.Data;
using EndForge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EndForge.Repositories
{
    public class ProgresoRepository : IProgresoRepository
    {
        private readonly EndForgeDbContext _db;
        private readonly ILogger<ProgresoRepository>? _logger;
        private readonly string _jsonPath;

        public ProgresoRepository(EndForgeDbContext db, ILogger<ProgresoRepository>? logger = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "EndForge");
            Directory.CreateDirectory(dir);
            _jsonPath = Path.Combine(dir, "progreso_backup.json");
        }

        public async Task<IEnumerable<ProgresoUsuarioDb>> GetByUsuarioAsync(int usuarioId)
        {
            // Intentar leer desde la base de datos (primario)
            try
            {
                var fromDb = await _db.ProgresoUsuarioDbs
                    .AsNoTracking()
                    .Where(p => p.UsuarioId == usuarioId)
                    .ToListAsync();

                if (fromDb != null && fromDb.Count > 0)
                {
                    return fromDb;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Fallo al leer progreso desde la base de datos, usando fallback JSON");
            }

            // Fallback a JSON local
            try
            {
                if (!File.Exists(_jsonPath))
                {
                    return Enumerable.Empty<ProgresoUsuarioDb>();
                }

                var text = await File.ReadAllTextAsync(_jsonPath);
                var list = JsonSerializer.Deserialize<List<ProgresoUsuarioDb>>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return list?.Where(p => p.UsuarioId == usuarioId) ?? Enumerable.Empty<ProgresoUsuarioDb>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error leyendo fallback JSON de progreso");
                return Enumerable.Empty<ProgresoUsuarioDb>();
            }
        }

        public async Task SaveAsync(ProgresoUsuarioDb progreso)
        {
            if (progreso == null) throw new ArgumentNullException(nameof(progreso));

            // Intento principal: persistir en la base de datos
            try
            {
                if (progreso.Id == 0)
                {
                    await _db.ProgresoUsuarioDbs.AddAsync(progreso);
                }
                else
                {
                    _db.ProgresoUsuarioDbs.Update(progreso);
                }

                await _db.SaveChangesAsync();
                // También actualizar el respaldo local para evitar divergencias en caso de desconexión
                await SaveToJsonBackupAsync(progreso);
                return;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Fallo al guardar en DB, guardando en backup JSON");
            }

            // Si falló la DB, guardar en JSON como respaldo
            try
            {
                await SaveToJsonBackupAsync(progreso);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error guardando el backup JSON de progreso");
                throw; // rethrow to inform caller if even backup fails
            }
        }

        public async Task MigrateIfNeededAsync()
        {
            try
            {
                // Aplica migraciones pendientes; si la BD no está disponible, la llamada lanzará excepción
                await _db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "No se pudieron aplicar migraciones en este momento");
            }
        }

        private async Task SaveToJsonBackupAsync(ProgresoUsuarioDb progreso)
        {
            List<ProgresoUsuarioDb> list;

            if (File.Exists(_jsonPath))
            {
                var text = await File.ReadAllTextAsync(_jsonPath);
                list = JsonSerializer.Deserialize<List<ProgresoUsuarioDb>>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ProgresoUsuarioDb>();
            }
            else
            {
                list = new List<ProgresoUsuarioDb>();
            }

            // Si existe, intentar reemplazar por Id o añadir
            var existing = list.FirstOrDefault(p => p.Id == progreso.Id && p.UsuarioId == progreso.UsuarioId);
            if (existing != null)
            {
                list.Remove(existing);
            }

            // Asegurar que el objeto guardado tenga un Id único en el backup si no tiene
            if (progreso.Id == 0)
            {
                var maxId = list.Count == 0 ? 0 : list.Max(p => p.Id);
                progreso.Id = maxId + 1;
            }

            list.Add(progreso);

            var outText = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_jsonPath, outText);
        }
    }
}
