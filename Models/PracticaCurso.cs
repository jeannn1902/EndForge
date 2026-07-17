namespace EndForge.Models;

public sealed class PracticaCurso {
    public string Id { get; init; } = "";

    public string TemaId { get; init; } = "";

    public int Numero { get; init; }

    public string Nombre { get; init; } = "";

    public string Objetivo { get; init; } = "";

    public string Descripcion { get; init; } = "";

    public IReadOnlyList<string> Conceptos { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Instrucciones { get; init; } = Array.Empty<string>();

    public string ResultadoEsperado { get; init; } = "";

    public string Dificultad { get; init; } = "";

    public string DuracionEstimada { get; init; } = "";

    public IReadOnlyList<string> RequisitosPrevios { get; init; } = Array.Empty<string>();

    public EstadoPracticaCurso EstadoInicial { get; init; } = EstadoPracticaCurso.Pendiente;

    public string NombreProyecto { get; init; } = "";
}
