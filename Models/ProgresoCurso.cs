namespace EndForge.Models;

public sealed class ProgresoCurso {
    public List<ProgresoPractica> Practicas { get; set; } = new();
}

public enum EstadoCargaProgreso {
    Exitosa,
    ArchivoInexistente,
    ArchivoVacio,
    ContenidoInvalido,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoCargaProgreso {
    public EstadoCargaProgreso Estado { get; init; }

    public ProgresoCurso Progreso { get; init; } = new();

    public int RegistrosInvalidos { get; init; }

    public Exception? Error { get; init; }

    public bool DatosDisponibles =>
        Estado == EstadoCargaProgreso.Exitosa ||
        Estado == EstadoCargaProgreso.ArchivoInexistente ||
        Estado == EstadoCargaProgreso.ArchivoVacio ||
        Estado == EstadoCargaProgreso.ContenidoInvalido;
}

public enum EstadoEscrituraProgreso {
    Exitosa,
    ContenidoInvalido,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoEscrituraProgreso {
    public EstadoEscrituraProgreso Estado { get; init; }

    public ProgresoCurso? ProgresoPersistido { get; init; }

    public TransicionProgresoPersistida? TransicionPersistida { get; init; }

    public int RegistrosInvalidosIgnorados { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa => Estado == EstadoEscrituraProgreso.Exitosa;
}

public sealed class TransicionProgresoPersistida {
    public string PracticaId { get; init; } = "";

    public ProgresoPractica? ProgresoAnterior { get; init; }

    public ProgresoPractica ProgresoFinal { get; init; } = new();

    public bool PracticaCreada { get; init; }

    public bool VinculoPersistidoAhora { get; init; }

    public bool RealizadaPersistidaAhora { get; init; }
}
