namespace EndForge.Models;

public sealed class SolicitudCreacionPractica {
    public string RutaPlantilla { get; init; } = "";

    public string RutaProyecto { get; init; } = "";

    public string RutaBaseConfiable { get; init; } = "";

    public string NombreProyecto { get; init; } = "";

    public string Tema { get; init; } = "";

    public string Objetivo { get; init; } = "";

    public string RutaRelativaSolucionEsperada { get; init; } = "";
}

public enum EstadoCreacionPractica {
    Exitosa,
    DestinoExistente,
    ErrorCreacion,
    ErrorApertura,
    CreadaAbiertaSinRegistroReciente
}

public sealed class ResultadoCreacionPractica {
    public EstadoCreacionPractica Estado { get; init; }

    public Exception? Error { get; init; }

    public Exception? ErrorSecundario { get; init; }

    public ResultadoEscrituraRecientes? RegistroReciente { get; init; }
}
