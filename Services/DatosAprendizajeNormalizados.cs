using EndForge.Models;
using System.Collections.ObjectModel;

namespace EndForge.Services;

internal static class DatosAprendizajeNormalizados {
    public static IReadOnlyDictionary<string, ProgresoPractica> CrearProgreso(
        CatalogoAprendizajeSnapshot catalogo,
        IEnumerable<ProgresoPractica> progreso,
        out int registrosHuerfanos,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(progreso);

        registrosHuerfanos = 0;
        Dictionary<string, List<ProgresoPractica>> agrupado =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (ProgresoPractica item in progreso) {
            cancellationToken.ThrowIfCancellationRequested();

            if (item is null ||
                string.IsNullOrWhiteSpace(item.PracticaId) ||
                !catalogo.PracticasPorId.ContainsKey(item.PracticaId)) {
                registrosHuerfanos++;
                continue;
            }

            if (!agrupado.TryGetValue(item.PracticaId, out List<ProgresoPractica>? grupo)) {
                grupo = new List<ProgresoPractica>();
                agrupado.Add(item.PracticaId, grupo);
            }

            grupo.Add(item);
        }

        Dictionary<string, ProgresoPractica> normalizado =
            new(StringComparer.OrdinalIgnoreCase);

        foreach ((string practicaId, List<ProgresoPractica> registros) in agrupado) {
            cancellationToken.ThrowIfCancellationRequested();
            ProgresoPractica seleccionado = registros
                .OrderByDescending(ObtenerFechaActividad)
                .ThenByDescending(item => item.Estado)
                .ThenByDescending(item => !string.IsNullOrWhiteSpace(item.RutaProyecto))
                .ThenBy(item => item.RutaProyecto, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.RutaProyecto, StringComparer.Ordinal)
                .First();
            normalizado.Add(practicaId, Copiar(seleccionado));
        }

        return new ReadOnlyDictionary<string, ProgresoPractica>(normalizado);
    }

    public static IReadOnlyDictionary<string, HistorialPractica> CrearHistorial(
        CatalogoAprendizajeSnapshot catalogo,
        IEnumerable<HistorialPractica> historial,
        out int registrosHuerfanos,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(historial);

        registrosHuerfanos = 0;
        Dictionary<string, List<HistorialPractica>> agrupado =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (HistorialPractica item in historial) {
            cancellationToken.ThrowIfCancellationRequested();

            if (item is null ||
                string.IsNullOrWhiteSpace(item.PracticaId) ||
                !catalogo.PracticasPorId.ContainsKey(item.PracticaId)) {
                registrosHuerfanos++;
                continue;
            }

            if (!agrupado.TryGetValue(item.PracticaId, out List<HistorialPractica>? grupo)) {
                grupo = new List<HistorialPractica>();
                agrupado.Add(item.PracticaId, grupo);
            }

            grupo.Add(item);
        }

        Dictionary<string, HistorialPractica> normalizado =
            new(StringComparer.OrdinalIgnoreCase);

        foreach ((string practicaId, List<HistorialPractica> registros) in agrupado) {
            cancellationToken.ThrowIfCancellationRequested();
            int totalIntentos = registros.Max(item => Math.Max(0, item.TotalIntentos));
            int? mejorCalificacion = registros
                .Where(item => item.MejorCalificacion.HasValue)
                .Select(item => item.MejorCalificacion)
                .Max();
            HistorialPractica? masReciente = registros
                .Where(item => item.FechaUltimoIntento.HasValue)
                .OrderByDescending(item => item.FechaUltimoIntento)
                .ThenByDescending(item => item.UltimaCalificacion)
                .FirstOrDefault();
            normalizado.Add(practicaId, new HistorialPractica {
                PracticaId = practicaId,
                TotalIntentos = totalIntentos,
                MejorCalificacion = mejorCalificacion,
                UltimaCalificacion = masReciente?.UltimaCalificacion,
                FechaUltimoIntento = masReciente?.FechaUltimoIntento
            });
        }

        return new ReadOnlyDictionary<string, HistorialPractica>(normalizado);
    }

    public static DateTimeOffset? ObtenerFechaActividad(ProgresoPractica progreso) {
        ArgumentNullException.ThrowIfNull(progreso);
        DateTimeOffset? fecha = progreso.FechaCreacion == default
            ? null
            : progreso.FechaCreacion;

        fecha = ObtenerMasReciente(fecha, progreso.FechaActualizacion);
        return ObtenerMasReciente(fecha, progreso.FechaFinalizacion);
    }

    public static DateTimeOffset? ObtenerMasReciente(
        DateTimeOffset? primera,
        DateTimeOffset? segunda) {
        if (!primera.HasValue) {
            return segunda;
        }

        if (!segunda.HasValue) {
            return primera;
        }

        return primera.Value >= segunda.Value ? primera : segunda;
    }

    private static ProgresoPractica Copiar(ProgresoPractica origen) {
        return new ProgresoPractica {
            PracticaId = origen.PracticaId.Trim(),
            Estado = origen.Estado,
            RutaProyecto = origen.RutaProyecto?.Trim() ?? string.Empty,
            FechaCreacion = origen.FechaCreacion,
            FechaActualizacion = origen.FechaActualizacion,
            FechaFinalizacion = origen.FechaFinalizacion
        };
    }
}
