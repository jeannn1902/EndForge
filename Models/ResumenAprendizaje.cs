namespace EndForge.Models;

public enum EstadoDisponibilidadDatos {
    DatosDisponibles,
    SinActividad,
    DatosParcialmenteRecuperados,
    DatosTemporalmenteNoDisponibles
}

public enum EstadoFuenteDatosAprendizaje {
    Disponible,
    SinDatos,
    ParcialmenteRecuperada,
    TemporalmenteNoDisponible
}

public enum EstadoRutaProyectoAprendizaje {
    SinRutaVinculada,
    Disponible,
    NoDisponible
}

public enum EstadoContinuacionAprendizaje {
    PracticaEnProgreso,
    BasadaEnRecomendacion,
    RutaCompletada,
    DatosNoDisponibles,
    SinContenidoDisponible
}

public enum EstadoRecomendacionAprendizaje {
    Disponible,
    RutaCompletada,
    SinPracticasPendientes,
    DatosNoDisponibles,
    SinContenidoDisponible
}

public enum MotivoRecomendacionAprendizaje {
    PrimeraPractica,
    TemaAvanzadoConProgreso,
    SiguienteTema,
    SiguienteGrado,
    PendienteAnterior,
    RetomarPracticaEnProgreso
}

public enum FuenteActividadAprendizaje {
    Progreso,
    HistorialEvaluaciones,
    Ambas
}

public sealed record ReferenciaPracticaAprendizaje(
    string GradoId,
    int NumeroGrado,
    string NombreGrado,
    string TemaId,
    int NumeroTema,
    string NombreTema,
    string PracticaId,
    int NumeroPractica,
    string NombrePractica) {
    public string Dificultad { get; init; } = "";

    public string DuracionEstimada { get; init; } = "";
}

public sealed record ResumenProgresoTema(
    string GradoId,
    string TemaId,
    int Numero,
    string Nombre,
    int TotalPracticasPublicadas,
    int? PracticasRealizadas,
    int? PracticasEnProgreso,
    int? PracticasPendientes,
    int? Porcentaje,
    bool? Completado);

public sealed record ResumenProgresoGrado(
    string GradoId,
    int Numero,
    string Nombre,
    int TotalPracticasPublicadas,
    int? PracticasRealizadas,
    int? PracticasEnProgreso,
    int? PracticasPendientes,
    int? Porcentaje,
    bool? Completado,
    IReadOnlyList<ResumenProgresoTema> Temas);

public sealed record ResumenProgresoGlobal(
    int TotalPracticasPublicadas,
    int? PracticasRealizadas,
    int? PracticasEnProgreso,
    int? PracticasPendientes,
    int? PorcentajeGlobal,
    int TotalTemas,
    int? TemasCompletados,
    int TotalGrados,
    int? GradosCompletados,
    IReadOnlyList<ResumenProgresoGrado> Grados);

public sealed record ResumenEvaluacionesGlobal(
    long? TotalEvaluacionesRealizadas,
    int? PracticasEvaluadas,
    int? PracticasConEvaluacionAprobada,
    int? PromedioMejoresCalificaciones,
    int? MejorCalificacionGlobal) {
    public int? EvaluacionesAprobadas => PracticasConEvaluacionAprobada;
}

public sealed record ActividadAprendizaje(
    DateTimeOffset Fecha,
    ReferenciaPracticaAprendizaje Practica,
    FuenteActividadAprendizaje Fuente,
    bool EsAproximada);

public sealed record RecomendacionAprendizaje(
    EstadoRecomendacionAprendizaje Estado,
    ReferenciaPracticaAprendizaje? Practica,
    MotivoRecomendacionAprendizaje? Motivo,
    bool BasadaEnDatosParciales);

public sealed record ContinuacionAprendizaje(
    EstadoContinuacionAprendizaje Estado,
    ReferenciaPracticaAprendizaje? Practica,
    string? RutaProyecto,
    EstadoRutaProyectoAprendizaje EstadoRuta,
    DateTimeOffset? FechaActividad,
    bool BasadaEnDatosParciales);

public sealed record EstadoFuenteAprendizaje(
    EstadoFuenteDatosAprendizaje Estado,
    string EstadoOrigen,
    int RegistrosInvalidos,
    int RegistrosHuerfanos,
    Exception? Error);

public sealed record ResumenInicio(
    EstadoDisponibilidadDatos Estado,
    ResumenProgresoGlobal Progreso,
    ResumenEvaluacionesGlobal Evaluaciones,
    ActividadAprendizaje? UltimaActividad,
    ContinuacionAprendizaje Continuacion,
    RecomendacionAprendizaje Recomendacion,
    EstadoFuenteAprendizaje FuenteProgreso,
    EstadoFuenteAprendizaje FuenteHistorial);

public sealed record ResultadoContinuidadAprendizaje(
    ContinuacionAprendizaje Continuacion,
    RecomendacionAprendizaje Recomendacion);
