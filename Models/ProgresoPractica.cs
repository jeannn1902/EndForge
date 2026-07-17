namespace EndForge.Models;

public sealed class ProgresoPractica {
    public string PracticaId { get; set; } = "";

    public EstadoPracticaCurso Estado { get; set; } = EstadoPracticaCurso.Pendiente;

    public string RutaProyecto { get; set; } = "";

    public DateTimeOffset FechaCreacion { get; set; }

    public DateTimeOffset? FechaActualizacion { get; set; }

    public DateTimeOffset? FechaFinalizacion { get; set; }
}
