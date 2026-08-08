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
}

internal sealed class DocumentoMotivacion {
    public int Version { get; set; } = 1;

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

public sealed record ResumenMotivacion(
    EstadoDisponibilidadMotivacion Estado,
    long? XpTotal,
    ResumenNivel? Nivel,
    string ZonaHorariaEstudio,
    DateTimeOffset? UltimoInstanteUtcAceptado,
    IReadOnlyList<AdvertenciaMotivacion> Advertencias,
    Exception? Error);

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
    Exception? Error);
