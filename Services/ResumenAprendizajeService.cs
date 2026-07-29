using EndForge.Models;

namespace EndForge.Services;

public sealed class ResumenAprendizajeService {
    public const int CalificacionAprobatoria = 70;

    private readonly Func<IReadOnlyList<GradoCurso>> cargarCatalogo;
    private readonly Func<ResultadoCargaProgreso> cargarProgreso;
    private readonly Func<ResultadoCargaHistorialEvaluaciones> cargarHistorial;
    private readonly RecomendacionAprendizajeService recomendacionService;

    public ResumenAprendizajeService()
        : this(
            new GradosService(new CursoService()),
            new ProgresoCursoService(),
            new HistorialEvaluacionesService(),
            new RecomendacionAprendizajeService()) {
    }

    public ResumenAprendizajeService(
        GradosService gradosService,
        ProgresoCursoService progresoService,
        HistorialEvaluacionesService historialService,
        RecomendacionAprendizajeService recomendacionService)
        : this(
            () => (gradosService ??
                throw new ArgumentNullException(nameof(gradosService)))
                .CargarGrados(null),
            () => (progresoService ??
                throw new ArgumentNullException(nameof(progresoService)))
                .CargarProgreso(),
            () => (historialService ??
                throw new ArgumentNullException(nameof(historialService)))
                .CargarHistorial(),
            recomendacionService ??
                throw new ArgumentNullException(nameof(recomendacionService))) {
    }

    internal ResumenAprendizajeService(
        Func<IReadOnlyList<GradoCurso>> cargarCatalogo,
        Func<ResultadoCargaProgreso> cargarProgreso,
        Func<ResultadoCargaHistorialEvaluaciones> cargarHistorial,
        RecomendacionAprendizajeService recomendacionService) {
        this.cargarCatalogo = cargarCatalogo ??
            throw new ArgumentNullException(nameof(cargarCatalogo));
        this.cargarProgreso = cargarProgreso ??
            throw new ArgumentNullException(nameof(cargarProgreso));
        this.cargarHistorial = cargarHistorial ??
            throw new ArgumentNullException(nameof(cargarHistorial));
        this.recomendacionService = recomendacionService ??
            throw new ArgumentNullException(nameof(recomendacionService));
    }

    public Task<ResumenInicio> CrearResumenAsync(
        CancellationToken cancellationToken = default) {
        return Task.Run(
            () => CrearResumen(cancellationToken),
            cancellationToken);
    }

    public ResumenInicio CrearResumen(
        CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<GradoCurso> grados = cargarCatalogo();
        CatalogoAprendizajeSnapshot catalogo =
            CatalogoAprendizajeSnapshot.Crear(grados, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        ResultadoCargaProgreso cargaProgreso = CargarProgresoSeguro(
            cargarProgreso,
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        ResultadoCargaHistorialEvaluaciones cargaHistorial =
            CargarHistorialSeguro(cargarHistorial, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        EstadoFuenteDatosAprendizaje estadoProgreso =
            ObtenerEstadoFuente(cargaProgreso);
        EstadoFuenteDatosAprendizaje estadoHistorial =
            ObtenerEstadoFuente(cargaHistorial);
        bool progresoUtilizable =
            estadoProgreso != EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible;
        bool historialUtilizable =
            estadoHistorial != EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible;
        IReadOnlyDictionary<string, ProgresoPractica> progreso =
            DatosAprendizajeNormalizados.CrearProgreso(
                catalogo,
                progresoUtilizable
                    ? cargaProgreso.Progreso.Practicas
                    : Array.Empty<ProgresoPractica>(),
                out int progresoHuerfano,
                cancellationToken);
        IReadOnlyDictionary<string, HistorialPractica> historial =
            DatosAprendizajeNormalizados.CrearHistorial(
                catalogo,
                historialUtilizable
                    ? cargaHistorial.Historial.Practicas
                    : Array.Empty<HistorialPractica>(),
                out int historialHuerfano,
                cancellationToken);

        bool datosParciales =
            estadoProgreso is EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada or
                EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible ||
            estadoHistorial is EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada or
                EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible ||
            progresoHuerfano > 0 ||
            historialHuerfano > 0;
        ResumenProgresoGlobal resumenProgreso = CrearResumenProgreso(
            catalogo,
            progreso,
            progresoUtilizable,
            estadoProgreso != EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
            cancellationToken);
        ResumenEvaluacionesGlobal resumenEvaluaciones = CrearResumenEvaluaciones(
            historial,
            historialUtilizable);
        ActividadAprendizaje? ultimaActividad = ObtenerUltimaActividad(
            catalogo,
            progreso,
            historial,
            cancellationToken);
        ResultadoContinuidadAprendizaje continuidad =
            recomendacionService.Resolver(
                catalogo,
                progreso,
                historial,
                progresoUtilizable,
                datosParciales,
                cancellationToken);
        EstadoDisponibilidadDatos estado = ObtenerEstadoGlobal(
            estadoProgreso,
            estadoHistorial,
            progresoHuerfano,
            historialHuerfano,
            progreso,
            historial);

        cancellationToken.ThrowIfCancellationRequested();
        return new ResumenInicio(
            estado,
            resumenProgreso,
            resumenEvaluaciones,
            ultimaActividad,
            continuidad.Continuacion,
            continuidad.Recomendacion,
            new EstadoFuenteAprendizaje(
                estadoProgreso,
                cargaProgreso.Estado.ToString(),
                cargaProgreso.RegistrosInvalidos,
                progresoHuerfano,
                cargaProgreso.Error),
            new EstadoFuenteAprendizaje(
                estadoHistorial,
                cargaHistorial.Estado.ToString(),
                cargaHistorial.RegistrosInvalidos,
                historialHuerfano,
                cargaHistorial.Error));
    }

    private static ResultadoCargaProgreso CargarProgresoSeguro(
        Func<ResultadoCargaProgreso> cargar,
        CancellationToken cancellationToken) {
        try {
            return cargar();
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoCargaProgreso {
                Estado = EstadoCargaProgreso.ErrorIo,
                Error = ex
            };
        }
    }

    private static ResultadoCargaHistorialEvaluaciones CargarHistorialSeguro(
        Func<ResultadoCargaHistorialEvaluaciones> cargar,
        CancellationToken cancellationToken) {
        try {
            return cargar();
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoCargaHistorialEvaluaciones {
                Estado = EstadoCargaHistorialEvaluaciones.ErrorIo,
                Error = ex
            };
        }
    }

    private static EstadoFuenteDatosAprendizaje ObtenerEstadoFuente(
        ResultadoCargaProgreso carga) {
        return carga.Estado switch {
            EstadoCargaProgreso.Exitosa =>
                EstadoFuenteDatosAprendizaje.Disponible,
            EstadoCargaProgreso.ArchivoInexistente or
            EstadoCargaProgreso.ArchivoVacio =>
                EstadoFuenteDatosAprendizaje.SinDatos,
            EstadoCargaProgreso.ContenidoInvalido
                when carga.Progreso.Practicas.Count > 0 =>
                EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
            _ => EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible
        };
    }

    private static EstadoFuenteDatosAprendizaje ObtenerEstadoFuente(
        ResultadoCargaHistorialEvaluaciones carga) {
        return carga.Estado switch {
            EstadoCargaHistorialEvaluaciones.Exitosa =>
                EstadoFuenteDatosAprendizaje.Disponible,
            EstadoCargaHistorialEvaluaciones.ArchivoInexistente or
            EstadoCargaHistorialEvaluaciones.ArchivoVacio =>
                EstadoFuenteDatosAprendizaje.SinDatos,
            EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido
                when carga.Historial.Practicas.Count > 0 =>
                EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
            _ => EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible
        };
    }

    private static ResumenProgresoGlobal CrearResumenProgreso(
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        bool fuenteUtilizable,
        bool fuenteCompleta,
        CancellationToken cancellationToken) {
        List<ResumenProgresoGrado> grados = new(catalogo.Grados.Count);
        int realizadasGlobales = 0;
        int enProgresoGlobales = 0;
        int temasCompletados = 0;
        int gradosCompletados = 0;

        foreach (GradoCurso grado in catalogo.Grados) {
            cancellationToken.ThrowIfCancellationRequested();
            TemaCatalogoAprendizaje[] temasGrado = catalogo.Temas
                .Where(item => item.Grado.Id.Equals(
                    grado.Id,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            List<ResumenProgresoTema> resumenTemas = new(temasGrado.Length);
            int realizadasGrado = 0;
            int enProgresoGrado = 0;
            int publicadasGrado = 0;

            foreach (TemaCatalogoAprendizaje tema in temasGrado) {
                int publicadas = tema.Practicas.Count;
                int realizadas = fuenteUtilizable
                    ? tema.Practicas.Count(item =>
                        ObtenerEstado(item, progreso) == EstadoPracticaCurso.Realizada)
                    : 0;
                int enProgreso = fuenteUtilizable
                    ? tema.Practicas.Count(item =>
                        ObtenerEstado(item, progreso) == EstadoPracticaCurso.EnProgreso)
                    : 0;
                int? pendientes = fuenteUtilizable && fuenteCompleta
                    ? publicadas - realizadas - enProgreso
                    : null;
                bool? completado = fuenteUtilizable && fuenteCompleta
                    ? publicadas > 0 && realizadas == publicadas
                    : null;

                if (completado == true) {
                    temasCompletados++;
                }

                publicadasGrado += publicadas;
                realizadasGrado += realizadas;
                enProgresoGrado += enProgreso;
                resumenTemas.Add(new ResumenProgresoTema(
                    grado.Id,
                    tema.Tema.Id,
                    tema.Tema.Numero,
                    tema.Tema.Nombre,
                    publicadas,
                    fuenteUtilizable ? realizadas : null,
                    fuenteUtilizable ? enProgreso : null,
                    pendientes,
                    fuenteUtilizable && fuenteCompleta
                        ? CalcularPorcentaje(realizadas, publicadas)
                        : null,
                    completado));
            }

            bool? gradoCompletado = fuenteUtilizable && fuenteCompleta
                ? publicadasGrado > 0 && realizadasGrado == publicadasGrado
                : null;

            if (gradoCompletado == true) {
                gradosCompletados++;
            }

            realizadasGlobales += realizadasGrado;
            enProgresoGlobales += enProgresoGrado;
            grados.Add(new ResumenProgresoGrado(
                grado.Id,
                grado.Numero,
                grado.Nombre,
                publicadasGrado,
                fuenteUtilizable ? realizadasGrado : null,
                fuenteUtilizable ? enProgresoGrado : null,
                fuenteUtilizable && fuenteCompleta
                    ? publicadasGrado - realizadasGrado - enProgresoGrado
                    : null,
                fuenteUtilizable && fuenteCompleta
                    ? CalcularPorcentaje(realizadasGrado, publicadasGrado)
                    : null,
                gradoCompletado,
                Array.AsReadOnly(resumenTemas.ToArray())));
        }

        int totalPracticas = catalogo.Practicas.Count;
        return new ResumenProgresoGlobal(
            totalPracticas,
            fuenteUtilizable ? realizadasGlobales : null,
            fuenteUtilizable ? enProgresoGlobales : null,
            fuenteUtilizable && fuenteCompleta
                ? totalPracticas - realizadasGlobales - enProgresoGlobales
                : null,
            fuenteUtilizable && fuenteCompleta
                ? CalcularPorcentaje(realizadasGlobales, totalPracticas)
                : null,
            catalogo.Temas.Count,
            fuenteUtilizable && fuenteCompleta ? temasCompletados : null,
            catalogo.Grados.Count,
            fuenteUtilizable && fuenteCompleta ? gradosCompletados : null,
            Array.AsReadOnly(grados.ToArray()));
    }

    private static ResumenEvaluacionesGlobal CrearResumenEvaluaciones(
        IReadOnlyDictionary<string, HistorialPractica> historial,
        bool fuenteUtilizable) {
        if (!fuenteUtilizable) {
            return new ResumenEvaluacionesGlobal(null, null, null, null, null);
        }

        HistorialPractica[] evaluaciones = historial.Values.ToArray();
        int[] mejores = evaluaciones
            .Where(item => item.MejorCalificacion.HasValue)
            .Select(item => item.MejorCalificacion!.Value)
            .ToArray();

        return new ResumenEvaluacionesGlobal(
            evaluaciones.Sum(item => (long)Math.Max(0, item.TotalIntentos)),
            evaluaciones.Count(item =>
                item.TotalIntentos > 0 || item.MejorCalificacion.HasValue),
            evaluaciones.Count(item =>
                item.MejorCalificacion >= CalificacionAprobatoria),
            mejores.Length == 0
                ? null
                : (int)Math.Round(
                    mejores.Average(),
                    MidpointRounding.AwayFromZero),
            mejores.Length == 0 ? null : mejores.Max());
    }

    private static ActividadAprendizaje? ObtenerUltimaActividad(
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        IReadOnlyDictionary<string, HistorialPractica> historial,
        CancellationToken cancellationToken) {
        List<CandidatoActividad> actividades = new();

        foreach (PracticaCatalogoAprendizaje item in catalogo.Practicas) {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset? fechaProgreso =
                progreso.TryGetValue(item.Practica.Id, out ProgresoPractica? registro)
                    ? DatosAprendizajeNormalizados.ObtenerFechaActividad(registro)
                    : null;
            DateTimeOffset? fechaHistorial =
                historial.TryGetValue(item.Practica.Id, out HistorialPractica? evaluacion)
                    ? evaluacion.FechaUltimoIntento
                    : null;
            DateTimeOffset? fecha = DatosAprendizajeNormalizados.ObtenerMasReciente(
                fechaProgreso,
                fechaHistorial);

            if (!fecha.HasValue) {
                continue;
            }

            FuenteActividadAprendizaje fuente =
                fechaProgreso.HasValue &&
                fechaHistorial.HasValue &&
                fechaProgreso.Value == fechaHistorial.Value
                    ? FuenteActividadAprendizaje.Ambas
                    : fechaHistorial.HasValue &&
                      (!fechaProgreso.HasValue ||
                       fechaHistorial.Value > fechaProgreso.Value)
                        ? FuenteActividadAprendizaje.HistorialEvaluaciones
                        : FuenteActividadAprendizaje.Progreso;
            actividades.Add(new CandidatoActividad(item, fecha.Value, fuente));
        }

        CandidatoActividad? actividad = actividades
            .OrderByDescending(item => item.Fecha)
            .ThenBy(item => item.Practica.Indice)
            .FirstOrDefault();

        return actividad is null
            ? null
            : new ActividadAprendizaje(
                actividad.Fecha,
                actividad.Practica.CrearReferencia(),
                actividad.Fuente,
                EsAproximada: true);
    }

    private static EstadoDisponibilidadDatos ObtenerEstadoGlobal(
        EstadoFuenteDatosAprendizaje estadoProgreso,
        EstadoFuenteDatosAprendizaje estadoHistorial,
        int progresoHuerfano,
        int historialHuerfano,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        IReadOnlyDictionary<string, HistorialPractica> historial) {
        bool progresoNoDisponible =
            estadoProgreso == EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible;
        bool historialNoDisponible =
            estadoHistorial == EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible;

        if (progresoNoDisponible && historialNoDisponible) {
            return EstadoDisponibilidadDatos.DatosTemporalmenteNoDisponibles;
        }

        if (progresoNoDisponible ||
            historialNoDisponible ||
            estadoProgreso == EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada ||
            estadoHistorial == EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada ||
            progresoHuerfano > 0 ||
            historialHuerfano > 0) {
            return EstadoDisponibilidadDatos.DatosParcialmenteRecuperados;
        }

        bool existeActividad = progreso.Count > 0 ||
            historial.Values.Any(item =>
                item.TotalIntentos > 0 ||
                item.MejorCalificacion.HasValue ||
                item.FechaUltimoIntento.HasValue);
        return existeActividad
            ? EstadoDisponibilidadDatos.DatosDisponibles
            : EstadoDisponibilidadDatos.SinActividad;
    }

    private static int CalcularPorcentaje(int valor, int total) {
        return total <= 0
            ? 0
            : Math.Clamp(
                (int)Math.Round(
                    valor * 100D / total,
                    MidpointRounding.AwayFromZero),
                0,
                100);
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

    private sealed record CandidatoActividad(
        PracticaCatalogoAprendizaje Practica,
        DateTimeOffset Fecha,
        FuenteActividadAprendizaje Fuente);
}
