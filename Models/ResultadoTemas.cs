namespace EndForge.Models;

public enum EstadoCargaTemas {
    Exitosa,
    RutaInexistente,
    PermisosInsuficientes,
    ErrorIo
}

public sealed class ResultadoCargaTemas {
    public EstadoCargaTemas Estado { get; init; }

    public IReadOnlyList<string> Temas { get; init; } =
        Array.Empty<string>();

    public Exception? Error { get; init; }

    public bool EsExitosa => Estado == EstadoCargaTemas.Exitosa;
}

public enum EstadoNumeracionPractica {
    Exitosa,
    TemaInexistente,
    PermisosInsuficientes,
    ErrorIo,
    LimiteAlcanzado
}

public sealed class ResultadoNumeracionPractica {
    public EstadoNumeracionPractica Estado { get; init; }

    public int? Numero { get; init; }

    public Exception? Error { get; init; }

    public bool EsExitosa =>
        Estado == EstadoNumeracionPractica.Exitosa &&
        Numero.HasValue;
}
