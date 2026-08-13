using EndForge.Models;

namespace EndForge.Services;

public sealed class RecomendacionAprendizajeService {
    private readonly Func<string, bool> existeDirectorio;

    public RecomendacionAprendizajeService()
        : this(Directory.Exists) {
    }

    internal RecomendacionAprendizajeService(Func<string, bool> existeDirectorio) {
        this.existeDirectorio = existeDirectorio ??
            throw new ArgumentNullException(nameof(existeDirectorio));
    }

    public ResultadoContinuidadAprendizaje Resolver(
        IReadOnlyList<GradoCurso> grados,
        ProgresoCurso progreso,
        HistorialEvaluaciones historial,
        bool datosParciales = false,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(grados);
        ArgumentNullException.ThrowIfNull(progreso);
        ArgumentNullException.ThrowIfNull(historial);

        CatalogoAprendizajeSnapshot catalogo =
            CatalogoAprendizajeSnapshot.Crear(grados, cancellationToken);
        IReadOnlyDictionary<string, ProgresoPractica> progresoConocido =
            DatosAprendizajeNormalizados.CrearProgreso(
                catalogo,
                progreso.Practicas,
                out _,
                cancellationToken);
        IReadOnlyDictionary<string, HistorialPractica> historialConocido =
            DatosAprendizajeNormalizados.CrearHistorial(
                catalogo,
                historial.Practicas,
                out _,
                cancellationToken);

        return Resolver(
            catalogo,
            progresoConocido,
            historialConocido,
            progresoDisponible: true,
            datosParciales,
            cancellationToken);
    }

    internal ResultadoContinuidadAprendizaje Resolver(
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        IReadOnlyDictionary<string, HistorialPractica> historial,
        bool progresoDisponible,
        bool datosParciales,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(catalogo);
        ArgumentNullException.ThrowIfNull(progreso);
        ArgumentNullException.ThrowIfNull(historial);
        cancellationToken.ThrowIfCancellationRequested();

        if (catalogo.Practicas.Count == 0) {
            return new ResultadoContinuidadAprendizaje(
                CrearContinuacionSinContenido(datosParciales),
                CrearRecomendacionSinContenido(datosParciales));
        }

        if (!progresoDisponible) {
            return new ResultadoContinuidadAprendizaje(
                new ContinuacionAprendizaje(
                    EstadoContinuacionAprendizaje.DatosNoDisponibles,
                    null,
                    null,
                    EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                    null,
                    true),
                new RecomendacionAprendizaje(
                    EstadoRecomendacionAprendizaje.DatosNoDisponibles,
                    null,
                    null,
                    true));
        }

        RecomendacionAprendizaje recomendacion = CrearRecomendacion(
            catalogo,
            progreso,
            historial,
            datosParciales,
            cancellationToken);
        ContinuacionAprendizaje continuacion = CrearContinuacion(
            catalogo,
            progreso,
            historial,
            recomendacion,
            datosParciales,
            cancellationToken);

        return new ResultadoContinuidadAprendizaje(continuacion, recomendacion);
    }

    private RecomendacionAprendizaje CrearRecomendacion(
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        IReadOnlyDictionary<string, HistorialPractica> historial,
        bool datosParciales,
        CancellationToken cancellationToken) {
        bool rutaCompletada = catalogo.Practicas.All(item =>
            ObtenerEstado(item, progreso) == EstadoPracticaCurso.Realizada);

        if (rutaCompletada) {
            return new RecomendacionAprendizaje(
                EstadoRecomendacionAprendizaje.RutaCompletada,
                null,
                null,
                datosParciales);
        }

        TemaCatalogoAprendizaje[] temasConProgreso = catalogo.Temas
            .Where(tema => TemaTieneProgreso(tema, progreso, historial))
            .ToArray();

        if (temasConProgreso.Length == 0) {
            return CrearRecomendacionDisponible(
                catalogo.Practicas[0],
                MotivoRecomendacionAprendizaje.PrimeraPractica,
                datosParciales);
        }

        TemaCatalogoAprendizaje? temaAvanzadoConPendientes = temasConProgreso
            .Where(tema => ObtenerPrimeraPendiente(tema, progreso) is not null)
            .OrderByDescending(tema => tema.Indice)
            .FirstOrDefault();

        if (temaAvanzadoConPendientes is not null) {
            PracticaCatalogoAprendizaje pendiente =
                ObtenerPrimeraPendiente(temaAvanzadoConPendientes, progreso)!;
            return CrearRecomendacionDisponible(
                pendiente,
                MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso,
                datosParciales);
        }

        TemaCatalogoAprendizaje ultimoTemaIniciado = temasConProgreso
            .OrderByDescending(tema => tema.Indice)
            .First();
        TemaCatalogoAprendizaje? temaPosterior = catalogo.Temas
            .Where(tema => tema.Indice > ultimoTemaIniciado.Indice)
            .FirstOrDefault(tema => ObtenerPrimeraPendiente(tema, progreso) is not null);

        if (temaPosterior is not null) {
            PracticaCatalogoAprendizaje pendiente =
                ObtenerPrimeraPendiente(temaPosterior, progreso)!;
            MotivoRecomendacionAprendizaje motivo =
                temaPosterior.Grado.Id.Equals(
                    ultimoTemaIniciado.Grado.Id,
                    StringComparison.OrdinalIgnoreCase)
                    ? MotivoRecomendacionAprendizaje.SiguienteTema
                    : MotivoRecomendacionAprendizaje.SiguienteGrado;
            return CrearRecomendacionDisponible(pendiente, motivo, datosParciales);
        }

        TemaCatalogoAprendizaje? temaPendienteAnterior = catalogo.Temas
            .Where(tema => ObtenerPrimeraPendiente(tema, progreso) is not null)
            .OrderByDescending(tema => tema.Indice)
            .FirstOrDefault();

        if (temaPendienteAnterior is not null) {
            return CrearRecomendacionDisponible(
                ObtenerPrimeraPendiente(temaPendienteAnterior, progreso)!,
                MotivoRecomendacionAprendizaje.PendienteAnterior,
                datosParciales);
        }

        PracticaCatalogoAprendizaje? practicaEnProgreso = catalogo.Practicas
            .FirstOrDefault(item =>
                ObtenerEstado(item, progreso) == EstadoPracticaCurso.EnProgreso);

        if (practicaEnProgreso is not null) {
            return CrearRecomendacionDisponible(
                practicaEnProgreso,
                MotivoRecomendacionAprendizaje.RetomarPracticaEnProgreso,
                datosParciales);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new RecomendacionAprendizaje(
            EstadoRecomendacionAprendizaje.SinPracticasPendientes,
            null,
            null,
            datosParciales);
    }

    private ContinuacionAprendizaje CrearContinuacion(
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        IReadOnlyDictionary<string, HistorialPractica> historial,
        RecomendacionAprendizaje recomendacion,
        bool datosParciales,
        CancellationToken cancellationToken) {
        List<CandidatoContinuacion> candidatos = new();

        foreach (PracticaCatalogoAprendizaje item in catalogo.Practicas) {
            cancellationToken.ThrowIfCancellationRequested();

            if (!progreso.TryGetValue(item.Practica.Id, out ProgresoPractica? registro) ||
                registro.Estado != EstadoPracticaCurso.EnProgreso) {
                continue;
            }

            DateTimeOffset? fechaActividad = DatosAprendizajeNormalizados.ObtenerMasReciente(
                DatosAprendizajeNormalizados.ObtenerFechaActividad(registro),
                historial.TryGetValue(item.Practica.Id, out HistorialPractica? historialPractica)
                    ? historialPractica.FechaUltimoIntento
                    : null);
            EstadoRutaProyectoAprendizaje estadoRuta =
                ObtenerEstadoRuta(registro.RutaProyecto);
            candidatos.Add(new CandidatoContinuacion(
                item,
                registro.RutaProyecto,
                estadoRuta,
                fechaActividad));
        }

        CandidatoContinuacion? candidato = candidatos
            .OrderByDescending(item =>
                item.EstadoRuta == EstadoRutaProyectoAprendizaje.Disponible)
            .ThenByDescending(item => item.FechaActividad)
            .ThenBy(item => item.Practica.Indice)
            .ThenBy(item => item.Practica.Practica.Id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Practica.Practica.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        if (candidato is not null) {
            return new ContinuacionAprendizaje(
                EstadoContinuacionAprendizaje.PracticaEnProgreso,
                candidato.Practica.CrearReferencia(),
                string.IsNullOrWhiteSpace(candidato.RutaProyecto)
                    ? null
                    : candidato.RutaProyecto,
                candidato.EstadoRuta,
                candidato.FechaActividad,
                datosParciales);
        }

        return recomendacion.Estado switch {
            EstadoRecomendacionAprendizaje.Disponible =>
                new ContinuacionAprendizaje(
                    EstadoContinuacionAprendizaje.BasadaEnRecomendacion,
                    recomendacion.Practica,
                    null,
                    EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                    null,
                    datosParciales),
            EstadoRecomendacionAprendizaje.RutaCompletada =>
                new ContinuacionAprendizaje(
                    EstadoContinuacionAprendizaje.RutaCompletada,
                    null,
                    null,
                    EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                    null,
                    datosParciales),
            EstadoRecomendacionAprendizaje.SinContenidoDisponible =>
                CrearContinuacionSinContenido(datosParciales),
            _ =>
                new ContinuacionAprendizaje(
                    EstadoContinuacionAprendizaje.DatosNoDisponibles,
                    null,
                    null,
                    EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                    null,
                    true)
        };
    }

    private EstadoRutaProyectoAprendizaje ObtenerEstadoRuta(string? rutaProyecto) {
        if (string.IsNullOrWhiteSpace(rutaProyecto)) {
            return EstadoRutaProyectoAprendizaje.SinRutaVinculada;
        }

        try {
            return Path.IsPathFullyQualified(rutaProyecto) &&
                existeDirectorio(rutaProyecto)
                ? EstadoRutaProyectoAprendizaje.Disponible
                : EstadoRutaProyectoAprendizaje.NoDisponible;
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return EstadoRutaProyectoAprendizaje.NoDisponible;
        }
    }

    private static bool TemaTieneProgreso(
        TemaCatalogoAprendizaje tema,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        IReadOnlyDictionary<string, HistorialPractica> historial) {
        return tema.Practicas.Any(item =>
            progreso.TryGetValue(item.Practica.Id, out ProgresoPractica? registro) &&
            (registro.Estado != EstadoPracticaCurso.Pendiente ||
             !string.IsNullOrWhiteSpace(registro.RutaProyecto)) ||
            historial.TryGetValue(item.Practica.Id, out HistorialPractica? evaluaciones) &&
            evaluaciones.TotalIntentos > 0);
    }

    private static PracticaCatalogoAprendizaje? ObtenerPrimeraPendiente(
        TemaCatalogoAprendizaje tema,
        IReadOnlyDictionary<string, ProgresoPractica> progreso) {
        return tema.Practicas.FirstOrDefault(item =>
            ObtenerEstado(item, progreso) == EstadoPracticaCurso.Pendiente);
    }

    private static EstadoPracticaCurso ObtenerEstado(
        PracticaCatalogoAprendizaje practica,
        IReadOnlyDictionary<string, ProgresoPractica> progreso) {
        return progreso.TryGetValue(
            practica.Practica.Id,
            out ProgresoPractica? registro)
            ? registro.Estado
            : EstadoPracticaCurso.Pendiente;
    }

    private static RecomendacionAprendizaje CrearRecomendacionDisponible(
        PracticaCatalogoAprendizaje practica,
        MotivoRecomendacionAprendizaje motivo,
        bool datosParciales) {
        return new RecomendacionAprendizaje(
            EstadoRecomendacionAprendizaje.Disponible,
            practica.CrearReferencia(),
            motivo,
            datosParciales);
    }

    private static RecomendacionAprendizaje CrearRecomendacionSinContenido(
        bool datosParciales) {
        return new RecomendacionAprendizaje(
            EstadoRecomendacionAprendizaje.SinContenidoDisponible,
            null,
            null,
            datosParciales);
    }

    private static ContinuacionAprendizaje CrearContinuacionSinContenido(
        bool datosParciales) {
        return new ContinuacionAprendizaje(
            EstadoContinuacionAprendizaje.SinContenidoDisponible,
            null,
            null,
            EstadoRutaProyectoAprendizaje.SinRutaVinculada,
            null,
            datosParciales);
    }

    private sealed record CandidatoContinuacion(
        PracticaCatalogoAprendizaje Practica,
        string RutaProyecto,
        EstadoRutaProyectoAprendizaje EstadoRuta,
        DateTimeOffset? FechaActividad);
}
