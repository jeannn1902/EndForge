namespace EndForge.Models;

public enum EstadoVistaPreviaPractica {
    Vacia,
    NumeracionNoDisponible,
    Completa
}

public sealed class ResultadoVistaPreviaPractica {
    public EstadoVistaPreviaPractica Estado { get; init; }

    public string NombreFinal { get; init; } = "";

    public EstadoNumeracionPractica? EstadoNumeracion { get; init; }

    public Exception? Error { get; init; }
}
