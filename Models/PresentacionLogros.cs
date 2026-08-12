namespace EndForge.Models;

public enum EstadoMetricaMotivacionalInicio {
    Disponible,
    SinDatos,
    NoDisponible,
    VersionIncompatible,
    ZonaHorariaNoDisponible
}

public sealed record RachaInicioPresentable(
    EstadoMetricaMotivacionalInicio Estado,
    int? RachaActual,
    int? MejorRacha,
    string TextoValor,
    string TextoDetalle,
    string DescripcionAccesible);

public sealed record LogrosInicioPresentable(
    EstadoMetricaMotivacionalInicio Estado,
    int? LogrosDesbloqueados,
    int TotalLogros,
    string TextoValor,
    string TextoDetalle,
    string DescripcionAccesible);

public sealed record PresentacionMotivacionInicio(
    RachaInicioPresentable Racha,
    LogrosInicioPresentable Logros);

public enum EstadoPresentacionLogros {
    Disponible,
    SinActividad,
    NoDisponible,
    VersionIncompatible
}

public enum EstadoLogroPresentable {
    Desbloqueado,
    Pendiente,
    EstadoNoDisponible
}

public enum SeccionLogroPresentable {
    PrimerosPasos,
    Progreso,
    Evaluaciones,
    TemasYGrados
}

public sealed record PresentacionLogro(
    string Id,
    string Nombre,
    string Descripcion,
    SeccionLogroPresentable Seccion,
    string TituloSeccion,
    int Orden,
    EstadoLogroPresentable Estado,
    string TextoEstado,
    bool EsImportado,
    DateOnly? FechaReconocimientoLocal,
    string TextoFecha,
    int? ProgresoActual,
    int? ProgresoObjetivo,
    string TextoProgreso,
    string DescripcionAccesible);

public sealed record PresentacionSeccionLogros(
    SeccionLogroPresentable Seccion,
    string Titulo,
    int Orden,
    IReadOnlyList<PresentacionLogro> Logros);

public sealed record PresentacionLogros(
    EstadoPresentacionLogros Estado,
    int? LogrosDesbloqueados,
    int TotalLogros,
    string TextoResumen,
    string MensajeDisponibilidad,
    IReadOnlyList<PresentacionSeccionLogros> Secciones,
    IReadOnlyList<PresentacionLogro> Logros);
