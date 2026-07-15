namespace EndForge.Models;
public enum EstadoLecturaRecientes {
    Exitosa,
    ArchivoInexistente,
    PermisosInsuficientes,
    ErrorIo,
    ContenidoInvalido
}

public sealed class ResultadoLecturaRecientes {
    public EstadoLecturaRecientes Estado { get; init; }
    public IReadOnlyList<ProyectoReciente> Proyectos { get; init; } = Array.Empty<ProyectoReciente>();
    public int RegistrosInvalidos { get; init; }
    public Exception? Error { get; init; }
    public bool DatosDisponibles => Estado == EstadoLecturaRecientes.Exitosa || Estado == EstadoLecturaRecientes.ContenidoInvalido;
}

public enum EstadoEscrituraRecientes {
    Exitosa,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoEscrituraRecientes {
    public EstadoEscrituraRecientes Estado { get; init; }
    public int RegistrosInvalidosIgnorados { get; init; }
    public Exception? Error { get; init; }
    public bool EsExitosa => Estado == EstadoEscrituraRecientes.Exitosa;
}
