namespace EndForge.Models;

public enum EstadoDatoInicio {
    Disponible,
    SinDatos,
    Parcial,
    NoDisponible
}

public enum TipoAccionInicio {
    ContinuarPractica,
    VerPractica,
    VerRutaAprendizaje,
    VerEstadisticas,
    Reintentar
}

public enum TipoMetricaInicio {
    EvaluacionesAprobadas,
    PromedioMejoresCalificaciones,
    MejorCalificacion,
    PracticasEnProgreso
}

public enum EstadoCargaInicio {
    Inactivo,
    Cargando,
    ErrorRecuperable
}

public enum EstadoNivelInicio {
    Disponible,
    NoDisponible,
    VersionIncompatible
}

public enum EstadoResultadoCargaInicio {
    Completada,
    ErrorRecuperable,
    Cancelada,
    Obsoleta,
    Cerrada
}

public sealed record EncabezadoInicioPresentable(
    string Saludo,
    string Subtitulo);

public sealed record AccionInicioPresentable(
    TipoAccionInicio Tipo,
    string Texto,
    string AccessibleName,
    string AccessibleDescription,
    ReferenciaPracticaAprendizaje? Practica = null,
    string? RutaProyecto = null);

public sealed record DatoInicioPresentable(
    EstadoDatoInicio Estado,
    string Texto,
    int? Valor,
    string Descripcion);

public sealed record ContinuacionInicioPresentable(
    EstadoDatoInicio Estado,
    string Titulo,
    ReferenciaPracticaAprendizaje? Practica,
    string TextoGrado,
    string TextoTema,
    string TextoPractica,
    string TextoEstado,
    string TextoRuta,
    bool BasadaEnDatosParciales,
    AccionInicioPresentable? AccionPrincipal,
    IReadOnlyList<AccionInicioPresentable> AccionesSecundarias);

public sealed record ProgresoInicioPresentable(
    DatoInicioPresentable PracticasRealizadas,
    DatoInicioPresentable Porcentaje,
    int? ValorBarra,
    DatoInicioPresentable TemasCompletados,
    DatoInicioPresentable GradosCompletados);

public sealed record PresentacionNivel(
    EstadoNivelInicio Estado,
    string TextoNivel,
    string TextoXpTotal,
    string TextoXpRestante,
    int? ValorBarra,
    string DescripcionAccesible);

public sealed record MetricaInicioPresentable(
    TipoMetricaInicio Tipo,
    string Titulo,
    DatoInicioPresentable Dato);

public sealed record RecomendacionInicioPresentable(
    string TituloSeccion,
    ReferenciaPracticaAprendizaje Practica,
    string TextoGrado,
    string TextoTema,
    string TextoPractica,
    string Dificultad,
    string DuracionEstimada,
    string Razon,
    bool BasadaEnDatosParciales,
    AccionInicioPresentable Accion);

public sealed record ActividadInicioPresentable(
    DateTimeOffset Fecha,
    string TextoFecha,
    string Texto,
    ReferenciaPracticaAprendizaje Practica,
    FuenteActividadAprendizaje Fuente,
    bool EsAproximada);

public sealed record BandaDatosInicioPresentable(
    EstadoDisponibilidadDatos Estado,
    string Titulo,
    string Mensaje,
    AccionInicioPresentable AccionReintentar);

public sealed record PresentacionInicio(
    EstadoDisponibilidadDatos EstadoDatos,
    EncabezadoInicioPresentable Encabezado,
    ContinuacionInicioPresentable Continuacion,
    ProgresoInicioPresentable Progreso,
    IReadOnlyList<MetricaInicioPresentable> Metricas,
    RecomendacionInicioPresentable? Recomendacion,
    IReadOnlyList<ActividadInicioPresentable> Actividades,
    BandaDatosInicioPresentable? BandaDatos) {
    public PresentacionNivel Nivel { get; init; } = new(
        EstadoNivelInicio.NoDisponible,
        "No disponible",
        string.Empty,
        "No pudimos cargar tu nivel y XP.",
        null,
        "El nivel y la experiencia no están disponibles temporalmente.");
    public PresentacionMotivacionInicio Motivacion { get; init; } = new(
        new RachaInicioPresentable(
            EstadoMetricaMotivacionalInicio.NoDisponible,
            null,
            null,
            "—",
            "Temporalmente no disponible",
            "La racha de estudio no está disponible temporalmente."),
        new LogrosInicioPresentable(
            EstadoMetricaMotivacionalInicio.NoDisponible,
            null,
            14,
            "—",
            "Temporalmente no disponible",
            "Los logros no están disponibles temporalmente."));
}

public sealed record EstadoCargaInicioPresentable(
    EstadoCargaInicio Estado,
    string Mensaje,
    bool MostrarIndicador,
    bool MostrarReintentar);

public sealed record ResultadoCargaInicio(
    long Generacion,
    EstadoResultadoCargaInicio Estado,
    PresentacionInicio? Presentacion,
    Exception? Error) {
    public Exception? AdvertenciaMotivacion { get; init; }

    public PresentacionLogros? Logros { get; init; }
}
