namespace EndForge.Models;

public sealed class IntentoPractica {
    public string Id { get; init; } = "";

    public string PracticaId { get; init; } = "";

    public DateTimeOffset Fecha { get; init; }

    public int Calificacion { get; init; }

    public bool Compilo { get; init; }

    public int PruebasSuperadas { get; init; }

    public int PruebasTotales { get; init; }

    public string ResultadoGeneral { get; init; } = "";

    public bool EjecucionFinalizada { get; init; }

    public int PuntosObtenidos { get; init; }

    public int PuntosMaximos { get; init; } = 100;

    public int PuntosCompilacion { get; init; }

    public int PuntosValidacion { get; init; }

    public int PuntosClaridad { get; init; }

    public string RutaProyecto { get; init; } = "";

    public IReadOnlyList<ResultadoCasoPrueba> Resultados { get; init; } =
        Array.Empty<ResultadoCasoPrueba>();

    public IReadOnlyList<string> Retroalimentacion { get; init; } =
        Array.Empty<string>();
}
