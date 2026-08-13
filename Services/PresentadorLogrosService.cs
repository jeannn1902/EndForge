using EndForge.Models;
using System.Globalization;

namespace EndForge.Services;

public sealed class PresentadorLogrosService {
    private const string TextoNoDisponible = "Temporalmente no disponible";
    private const string TextoVersionIncompatible =
        "Disponible con una versión compatible";
    private readonly CatalogoLogrosService catalogo;
    private readonly IReadOnlyList<MetadatoVisualLogro> metadatos;

    public PresentadorLogrosService()
        : this(new CatalogoLogrosService()) {
    }

    public PresentadorLogrosService(CatalogoLogrosService catalogo) {
        this.catalogo = catalogo ??
            throw new ArgumentNullException(nameof(catalogo));
        metadatos = CrearMetadatosVisuales();
        ValidarCoberturaCatalogo();
    }

    public PresentacionLogros Crear(
        ResumenInicio resumenInicio,
        ResumenMotivacion motivacion) {
        ArgumentNullException.ThrowIfNull(resumenInicio);
        ArgumentNullException.ThrowIfNull(motivacion);

        bool disponible = EsDisponible(motivacion);
        Dictionary<string, LogroDesbloqueado> desbloqueados = disponible
            ? ObtenerLogrosConocidos(motivacion)
            : new Dictionary<string, LogroDesbloqueado>(
                StringComparer.OrdinalIgnoreCase);
        TimeZoneInfo? zona = disponible
            ? IntentarResolverZonaHoraria(motivacion)
            : null;
        PresentacionLogro[] logros = metadatos
            .Select((metadato, indice) => CrearLogro(
                metadato,
                indice + 1,
                resumenInicio,
                desbloqueados,
                zona,
                disponible))
            .ToArray();
        PresentacionSeccionLogros[] secciones = logros
            .GroupBy(logro => logro.Seccion)
            .Select((grupo, indice) => new PresentacionSeccionLogros(
                grupo.Key,
                ObtenerTituloSeccion(grupo.Key),
                indice + 1,
                Array.AsReadOnly(grupo.ToArray())))
            .ToArray();
        EstadoPresentacionLogros estado = motivacion.Estado switch {
            EstadoDisponibilidadMotivacion.VersionIncompatible =>
                EstadoPresentacionLogros.VersionIncompatible,
            EstadoDisponibilidadMotivacion.NoDisponible =>
                EstadoPresentacionLogros.NoDisponible,
            EstadoDisponibilidadMotivacion.SinActividad =>
                EstadoPresentacionLogros.SinActividad,
            _ => EstadoPresentacionLogros.Disponible
        };
        int? cantidad = disponible ? desbloqueados.Count : null;

        return new PresentacionLogros(
            estado,
            cantidad,
            metadatos.Count,
            cantidad.HasValue
                ? $"{cantidad.Value} de {metadatos.Count} logros desbloqueados"
                : "Logros no disponibles",
            estado switch {
                EstadoPresentacionLogros.NoDisponible =>
                    "Los logros no están disponibles temporalmente.",
                EstadoPresentacionLogros.VersionIncompatible =>
                    "Los logros estarán disponibles con una versión compatible de EndForge.",
                EstadoPresentacionLogros.SinActividad =>
                    "Completa actividades del curso para reconocer tus primeros logros.",
                _ => string.Empty
            },
            Array.AsReadOnly(secciones),
            Array.AsReadOnly(logros));
    }

    public PresentacionMotivacionInicio CrearPresentacionInicio(
        ResumenMotivacion motivacion) {
        ArgumentNullException.ThrowIfNull(motivacion);

        if (motivacion.Estado ==
            EstadoDisponibilidadMotivacion.VersionIncompatible) {
            return CrearPresentacionInicioNoDisponible(
                EstadoMetricaMotivacionalInicio.VersionIncompatible,
                TextoVersionIncompatible);
        }

        if (!EsDisponible(motivacion)) {
            return CrearPresentacionInicioNoDisponible(
                EstadoMetricaMotivacionalInicio.NoDisponible,
                TextoNoDisponible);
        }

        Dictionary<string, LogroDesbloqueado> desbloqueados =
            ObtenerLogrosConocidos(motivacion);
        LogrosInicioPresentable logros = new(
            desbloqueados.Count == 0
                ? EstadoMetricaMotivacionalInicio.SinDatos
                : EstadoMetricaMotivacionalInicio.Disponible,
            desbloqueados.Count,
            metadatos.Count,
            $"{desbloqueados.Count} / {metadatos.Count}",
            "Logros reconocidos",
            $"{desbloqueados.Count} de {metadatos.Count} logros desbloqueados.");
        RachaInicioPresentable racha = CrearRacha(motivacion);
        return new PresentacionMotivacionInicio(racha, logros);
    }

    public PresentacionMotivacionInicio CrearPresentacionInicioNoDisponible() {
        return CrearPresentacionInicioNoDisponible(
            EstadoMetricaMotivacionalInicio.NoDisponible,
            TextoNoDisponible);
    }

    private PresentacionMotivacionInicio CrearPresentacionInicioNoDisponible(
        EstadoMetricaMotivacionalInicio estado,
        string detalle) {
        return new PresentacionMotivacionInicio(
            new RachaInicioPresentable(
                estado,
                null,
                null,
                "—",
                detalle,
                estado == EstadoMetricaMotivacionalInicio.VersionIncompatible
                    ? "La racha de estudio estará disponible con una versión compatible de EndForge."
                    : "La racha de estudio no está disponible temporalmente."),
            new LogrosInicioPresentable(
                estado,
                null,
                metadatos.Count,
                "—",
                detalle,
                estado == EstadoMetricaMotivacionalInicio.VersionIncompatible
                    ? "Los logros estarán disponibles con una versión compatible de EndForge."
                    : "Los logros no están disponibles temporalmente."));
    }

    private RachaInicioPresentable CrearRacha(ResumenMotivacion motivacion) {
        if (IntentarResolverZonaHoraria(motivacion) is null) {
            return new RachaInicioPresentable(
                EstadoMetricaMotivacionalInicio.ZonaHorariaNoDisponible,
                null,
                motivacion.Racha.MejorRachaHistorica,
                "—",
                "Zona horaria no disponible",
                "La racha actual no se pudo calcular con la zona horaria de estudio guardada.");
        }

        int actual = Math.Max(0, motivacion.Racha.RachaActual);
        int mejor = Math.Max(actual, motivacion.Racha.MejorRachaHistorica);
        string textoValor = actual switch {
            0 when mejor > 0 => "Empieza una nueva racha",
            0 => "0 días",
            1 => "1 día de estudio",
            _ => $"{actual} días de racha"
        };
        string detalle;

        if (actual == 0 && mejor == 0) {
            detalle = "Empieza una nueva racha";
        } else if (actual == 0) {
            detalle = $"Mejor racha: {mejor} días";
        } else if (actual == 1) {
            detalle = "Día de estudio registrado";
        } else {
            detalle = $"Mejor racha: {mejor} días";
        }

        return new RachaInicioPresentable(
            actual == 0 && mejor == 0
                ? EstadoMetricaMotivacionalInicio.SinDatos
                : EstadoMetricaMotivacionalInicio.Disponible,
            actual,
            mejor,
            textoValor,
            detalle,
            $"Racha actual: {textoValor}. {detalle}.");
    }

    private PresentacionLogro CrearLogro(
        MetadatoVisualLogro metadato,
        int orden,
        ResumenInicio resumenInicio,
        IReadOnlyDictionary<string, LogroDesbloqueado> desbloqueados,
        TimeZoneInfo? zona,
        bool disponible) {
        LogroDesbloqueado? reconocimiento = null;
        bool desbloqueado = disponible && desbloqueados.TryGetValue(
            metadato.Id,
            out reconocimiento);
        EstadoLogroPresentable estado = !disponible
            ? EstadoLogroPresentable.EstadoNoDisponible
            : desbloqueado
                ? EstadoLogroPresentable.Desbloqueado
                : EstadoLogroPresentable.Pendiente;
        DateOnly? fechaLocal = null;
        string textoFecha = string.Empty;

        if (desbloqueado &&
            reconocimiento is not null &&
            !reconocimiento.EsImportado &&
            zona is not null) {
            DateTimeOffset fechaConvertida = TimeZoneInfo.ConvertTime(
                reconocimiento.FechaReconocimientoUtc,
                zona);
            fechaLocal = DateOnly.FromDateTime(fechaConvertida.DateTime);
            textoFecha = $"Reconocido el {FormatearFecha(fechaLocal.Value)}";
        }

        (int? progresoActual, int? progresoObjetivo, string textoProgreso) =
            ObtenerProgreso(
                metadato.Id,
                resumenInicio,
                disponible,
                desbloqueado);
        string textoEstado = estado switch {
            EstadoLogroPresentable.Desbloqueado => "DESBLOQUEADO",
            EstadoLogroPresentable.Pendiente => "PENDIENTE",
            _ => "ESTADO NO DISPONIBLE"
        };
        string descripcionAccesible =
            $"{metadato.Descripcion} Estado: {textoEstado}.";

        if (!string.IsNullOrWhiteSpace(textoProgreso)) {
            descripcionAccesible += $" Progreso: {textoProgreso}.";
        }

        if (!string.IsNullOrWhiteSpace(textoFecha)) {
            descripcionAccesible += $" {textoFecha}.";
        }

        return new PresentacionLogro(
            metadato.Id,
            metadato.Nombre,
            metadato.Descripcion,
            metadato.Seccion,
            ObtenerTituloSeccion(metadato.Seccion),
            orden,
            estado,
            textoEstado,
            reconocimiento?.EsImportado ?? false,
            fechaLocal,
            textoFecha,
            progresoActual,
            progresoObjetivo,
            textoProgreso,
            descripcionAccesible);
    }

    private static (int? Actual, int? Objetivo, string Texto) ObtenerProgreso(
        string logroId,
        ResumenInicio resumen,
        bool motivacionDisponible,
        bool desbloqueado) {
        if (!motivacionDisponible) {
            return (null, null, string.Empty);
        }

        int? actual;
        int objetivo;
        string unidad;

        if (logroId.Equals(
                CatalogoLogrosService.CincoPracticasRealizadasId,
                StringComparison.OrdinalIgnoreCase)) {
            actual = ObtenerPracticasRealizadasConfiables(resumen);
            objetivo = 5;
            unidad = "prácticas";
        } else if (logroId.Equals(
                CatalogoLogrosService.DiezPracticasRealizadasId,
                StringComparison.OrdinalIgnoreCase)) {
            actual = ObtenerPracticasRealizadasConfiables(resumen);
            objetivo = 10;
            unidad = "prácticas";
        } else if (logroId.Equals(
                CatalogoLogrosService.VeinticincoPracticasRealizadasId,
                StringComparison.OrdinalIgnoreCase)) {
            actual = ObtenerPracticasRealizadasConfiables(resumen);
            objetivo = 25;
            unidad = "prácticas";
        } else if (logroId.Equals(
                CatalogoLogrosService.CincoPracticasAprobadasId,
                StringComparison.OrdinalIgnoreCase)) {
            actual = ObtenerPracticasAprobadasConfiables(resumen);
            objetivo = 5;
            unidad = "prÃ¡cticas aprobadas";
        } else if (logroId.Equals(
                CatalogoLogrosService.DiezPracticasAprobadasId,
                StringComparison.OrdinalIgnoreCase)) {
            actual = ObtenerPracticasAprobadasConfiables(resumen);
            objetivo = 10;
            unidad = "prÃ¡cticas aprobadas";
        } else {
            return (null, null, string.Empty);
        }

        if (!actual.HasValue) {
            return (null, null, string.Empty);
        }

        int valor = Math.Clamp(actual.Value, 0, objetivo);

        if (desbloqueado) {
            valor = objetivo;
        }

        return (valor, objetivo, $"{valor} / {objetivo} {unidad}");
    }

    private static int? ObtenerPracticasRealizadasConfiables(
        ResumenInicio resumen) {
        return EsFuenteConfiable(resumen.FuenteProgreso.Estado)
            ? resumen.Progreso.PracticasRealizadas
            : null;
    }

    private static int? ObtenerPracticasAprobadasConfiables(
        ResumenInicio resumen) {
        return EsFuenteConfiable(resumen.FuenteHistorial.Estado)
            ? resumen.Evaluaciones.PracticasConEvaluacionAprobada
            : null;
    }

    private static bool EsFuenteConfiable(
        EstadoFuenteDatosAprendizaje estado) {
        return estado is EstadoFuenteDatosAprendizaje.Disponible or
            EstadoFuenteDatosAprendizaje.SinDatos;
    }

    private Dictionary<string, LogroDesbloqueado> ObtenerLogrosConocidos(
        ResumenMotivacion motivacion) {
        Dictionary<string, LogroDesbloqueado> resultado =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (LogroDesbloqueado logro in motivacion.LogrosDesbloqueados) {
            if (logro is not null &&
                catalogo.EsLogroConocido(logro.LogroId)) {
                resultado.TryAdd(logro.LogroId, logro);
            }
        }

        return resultado;
    }

    private static TimeZoneInfo? IntentarResolverZonaHoraria(
        ResumenMotivacion motivacion) {
        if (motivacion.Advertencias.Contains(
                AdvertenciaMotivacion.ZonaHorariaNoDisponible) ||
            string.IsNullOrWhiteSpace(motivacion.ZonaHorariaEstudio)) {
            return null;
        }

        try {
            return TimeZoneInfo.FindSystemTimeZoneById(
                motivacion.ZonaHorariaEstudio);
        } catch (Exception ex) when (ex is TimeZoneNotFoundException or
            InvalidTimeZoneException) {
            return null;
        }
    }

    private static string FormatearFecha(DateOnly fecha) {
        return fecha.ToString(
                "d MMM yyyy",
                CultureInfo.GetCultureInfo("es-MX"))
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .ToLower(CultureInfo.GetCultureInfo("es-MX"));
    }

    private static bool EsDisponible(ResumenMotivacion motivacion) {
        return motivacion.Estado is EstadoDisponibilidadMotivacion.Disponible or
            EstadoDisponibilidadMotivacion.SinActividad;
    }

    private void ValidarCoberturaCatalogo() {
        string[] catalogoIds = catalogo.CargarDefiniciones()
            .Select(definicion => definicion.Id)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        string[] metadatosIds = metadatos
            .Select(metadato => metadato.Id)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!catalogoIds.SequenceEqual(
                metadatosIds,
                StringComparer.OrdinalIgnoreCase)) {
            throw new InvalidOperationException(
                "Los metadatos visuales no cubren exactamente el catálogo de logros.");
        }
    }

    private static string ObtenerTituloSeccion(
        SeccionLogroPresentable seccion) {
        return seccion switch {
            SeccionLogroPresentable.PrimerosPasos => "PRIMEROS PASOS",
            SeccionLogroPresentable.Progreso => "PROGRESO",
            SeccionLogroPresentable.Evaluaciones => "EVALUACIONES",
            _ => "TEMAS Y GRADOS"
        };
    }

    private static IReadOnlyList<MetadatoVisualLogro> CrearMetadatosVisuales() {
        return Array.AsReadOnly(new[] {
            new MetadatoVisualLogro(
                CatalogoLogrosService.PrimeraPracticaVinculadaId,
                "Primera práctica vinculada",
                "Vincula una práctica de EndForge con un proyecto de C++.",
                SeccionLogroPresentable.PrimerosPasos),
            new MetadatoVisualLogro(
                CatalogoLogrosService.PrimeraPracticaRealizadaId,
                "Primera práctica completada",
                "Completa tu primera práctica del curso.",
                SeccionLogroPresentable.PrimerosPasos),
            new MetadatoVisualLogro(
                CatalogoLogrosService.CincoPracticasRealizadasId,
                "Cinco prácticas completadas",
                "Completa cinco prácticas distintas.",
                SeccionLogroPresentable.Progreso),
            new MetadatoVisualLogro(
                CatalogoLogrosService.DiezPracticasRealizadasId,
                "Diez prácticas completadas",
                "Completa diez prácticas distintas.",
                SeccionLogroPresentable.Progreso),
            new MetadatoVisualLogro(
                CatalogoLogrosService.VeinticincoPracticasRealizadasId,
                "Veinticinco prácticas completadas",
                "Completa veinticinco prácticas distintas.",
                SeccionLogroPresentable.Progreso),
            new MetadatoVisualLogro(
                CatalogoLogrosService.PrimeraEvaluacionAprobadaId,
                "Primera evaluación aprobada",
                "Aprueba la evaluación de una práctica por primera vez.",
                SeccionLogroPresentable.Evaluaciones),
            new MetadatoVisualLogro(
                CatalogoLogrosService.PrimeraEvaluacionPerfectaId,
                "Primera calificación perfecta",
                "Obtén 100 puntos en una práctica.",
                SeccionLogroPresentable.Evaluaciones),
            new MetadatoVisualLogro(
                CatalogoLogrosService.CincoPracticasAprobadasId,
                "Cinco evaluaciones aprobadas",
                "Aprueba evaluaciones en cinco prácticas distintas.",
                SeccionLogroPresentable.Evaluaciones),
            new MetadatoVisualLogro(
                CatalogoLogrosService.DiezPracticasAprobadasId,
                "Diez evaluaciones aprobadas",
                "Aprueba evaluaciones en diez prácticas distintas.",
                SeccionLogroPresentable.Evaluaciones),
            new MetadatoVisualLogro(
                CatalogoLogrosService.CincoPracticasPerfectasId,
                "Cinco resultados perfectos",
                "Obtén 100 puntos en cinco prácticas distintas.",
                SeccionLogroPresentable.Evaluaciones),
            new MetadatoVisualLogro(
                CatalogoLogrosService.PrimerTemaCompletadoId,
                "Primer tema completado",
                "Completa todas las prácticas de un tema.",
                SeccionLogroPresentable.TemasYGrados),
            new MetadatoVisualLogro(
                CatalogoLogrosService.PrimerGradoCompletadoId,
                "Primer grado completado",
                "Completa todos los temas de un grado.",
                SeccionLogroPresentable.TemasYGrados),
            new MetadatoVisualLogro(
                CatalogoLogrosService.GradoFundamentosCompletoId,
                "Fundamentos de C++ completados",
                "Completa el Grado 1: Fundamentos de C++.",
                SeccionLogroPresentable.TemasYGrados),
            new MetadatoVisualLogro(
                CatalogoLogrosService.GradoJuniorCompletoId,
                "C++ Junior completado",
                "Completa el Grado 2: C++ Junior.",
                SeccionLogroPresentable.TemasYGrados)
        });
    }

    private sealed record MetadatoVisualLogro(
        string Id,
        string Nombre,
        string Descripcion,
        SeccionLogroPresentable Seccion);
}
