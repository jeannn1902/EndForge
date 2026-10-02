using System.Collections.Generic;
using System.Threading.Tasks;
using EndForge.Models;

namespace EndForge.Repositories
{
    public interface IProgresoRepository
    {
        Task<IEnumerable<ProgresoUsuarioDb>> GetByUsuarioAsync(int usuarioId);

        Task SaveAsync(ProgresoUsuarioDb progreso);

        /// <summary>
        /// Aplica migraciones pendientes o prepara el esquema si es necesario.
        /// </summary>
        Task MigrateIfNeededAsync();
    }
}
