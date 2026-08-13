namespace EndForge.Models;

public enum TipoConcesionXP {
    PracticaVinculada,
    PracticaRealizada,
    EvaluacionAprobada,
    MejoraCalificacion,
    EvaluacionPerfecta,
    TemaCompletado,
    GradoCompletado
}

public sealed class ConcesionXP {
    public string Clave { get; set; } = string.Empty;

    public int CantidadXP { get; set; }

    public DateTimeOffset FechaUtc { get; set; }

    public TipoConcesionXP Tipo { get; set; }

    public string? PracticaId { get; set; }

    public string? TemaId { get; set; }

    public string? GradoId { get; set; }

    public bool EsImportada { get; set; }
}

public sealed class MetadatosMigracionMotivacion {
    public int VersionMigracion { get; set; } = 1;

    public bool MigracionInicialCompletada { get; set; }

    public DateTimeOffset FechaMigracionUtc { get; set; }

    public bool ProgresoProcesado { get; set; }

    public bool HistorialProcesado { get; set; }

    public int MejorasHistoricasReconocidas { get; set; }

    public int MejorasHistoricasOmitidas { get; set; }

    public DateTimeOffset? UltimaReconciliacionUtc { get; set; }

    public bool MigracionVersion2Completada { get; set; }

    public DateTimeOffset? FechaMigracionVersion2Utc { get; set; }

    public bool LogrosHistoricosProcesados { get; set; }

    public bool ActividadHistoricaProcesada { get; set; }

    public bool HistoriaActividadParcial { get; set; }
}

public sealed class LogroDesbloqueado {
    public string LogroId { get; set; } = string.Empty;

    public DateTimeOffset FechaReconocimientoUtc { get; set; }

    public bool EsImportado { get; set; }
}

internal sealed class DocumentoMotivacion {
    public int Version { get; set; } = 2;

    public string ZonaHorariaEstudio { get; set; } = string.Empty;

    public List<ConcesionXP> ConcesionesXP { get; set; } = new();

    public Dictionary<string, int> MejorCalificacionReconocidaPorPractica {
        get;
        set;
    } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, int> XPMejoraConcedidoPorPractica { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset UltimoInstanteUtcAceptado { get; set; }

    public MetadatosMigracionMotivacion MetadatosMigracion { get; set; } = new();

    public List<LogroDesbloqueado> LogrosDesbloqueados { get; set; } = new();

    public List<DateOnly> DiasActividadAcademica { get; set; } = new();
}

internal sealed class DocumentoMotivacionVersion1 {
    public int? Version { get; set; }

    public string? ZonaHorariaEstudio { get; set; }

    public List<ConcesionXP>? ConcesionesXP { get; set; }

    public Dictionary<string, int>? MejorCalificacionReconocidaPorPractica {
        get;
        set;
    }

    public Dictionary<string, int>? XPMejoraConcedidoPorPractica { get; set; }

    public DateTimeOffset? UltimoInstanteUtcAceptado { get; set; }

    public MetadatosMigracionMotivacionVersion1? MetadatosMigracion { get; set; }
}

internal sealed class MetadatosMigracionMotivacionVersion1 {
    public int? VersionMigracion { get; set; }

    public bool? MigracionInicialCompletada { get; set; }

    public DateTimeOffset? FechaMigracionUtc { get; set; }

    public bool? ProgresoProcesado { get; set; }

    public bool? HistorialProcesado { get; set; }

    public int? MejorasHistoricasReconocidas { get; set; }

    public int? MejorasHistoricasOmitidas { get; set; }

    public DateTimeOffset? UltimaReconciliacionUtc { get; set; }
}

public enum AdvertenciaMotivacion {
    ZonaHorariaNoDisponible,
    RetrocesoRelojDetectado,
    DatosAcademicosParciales,
    MejoraHistoricaNoDemostrable
}

public enum EstadoDisponibilidadMotivacion {
    Disponible,
    SinActividad,
    NoDisponible,
    VersionIncompatible
}

public sealed record ResumenNivel(
    long XpTotal,
    long NivelActual,
    decimal XpMinimoNivelActual,
    decimal XpRequeridoSiguienteNivel,
    decimal XpAcumuladoDentroNivel,
    decimal XpRestante,
    decimal PorcentajeNivel);

public sealed record ResumenRacha(
    int RachaActual,
    int MejorRachaHistorica,
    DateOnly? UltimoDiaEstudio);

public sealed record ResumenMotivacion(
    EstadoDisponibilidadMotivacion Estado,
    long? XpTotal,
    ResumenNivel? Nivel,
    string ZonaHorariaEstudio,
    DateTimeOffset? UltimoInstanteUtcAceptado,
    IReadOnlyList<AdvertenciaMotivacion> Advertencias,
    Exception? Error) {
    public ResumenRacha Racha { get; init; } = new(0, 0, null);

    public IReadOnlyList<LogroDesbloqueado> LogrosDesbloqueados { get; init; } =
        Array.Empty<LogroDesbloqueado>();
}

public enum EstadoProcesamientoMotivacion {
    Aplicada,
    YaAplicada,
    SinRecompensa,
    DatosMotivacionalesNoDisponibles,
    ErrorRecuperable,
    VersionIncompatible
}

public sealed record ResultadoProcesamientoMotivacion(
    EstadoProcesamientoMotivacion Estado,
    long XpConcedido,
    long? XpTotalResultante,
    long? NivelAnterior,
    long? NivelNuevo,
    bool SubioNivel,
    IReadOnlyList<string> ClavesProcesadas,
    ResumenMotivacion Resumen,
    Exception? Error) {
    public IReadOnlyList<LogroDesbloqueado> LogrosNuevos { get; init; } =
        Array.Empty<LogroDesbloqueado>();
}
