namespace EndForge.Models;

public sealed class GuiaPractica {
    public string QueVasAConstruir { get; init; } = "";

    public IReadOnlyList<DatoGuiaPractica> DatosNecesarios { get; init; } =
        Array.Empty<DatoGuiaPractica>();

    public IReadOnlyList<ConceptoGuiaPractica> ExplicacionesConceptos { get; init; } =
        Array.Empty<ConceptoGuiaPractica>();

    public IReadOnlyList<string> PasosSugeridos { get; init; } = Array.Empty<string>();

    public string AdvertenciaEvaluacion { get; init; } = "";

    public HerramientaGuiaPractica? HerramientaUtil { get; init; }

    public EjemploEjecucionPractica? EjemploEjecucion { get; init; }

    public IReadOnlyList<string> ErroresComunes { get; init; } = Array.Empty<string>();
}

public sealed class DatoGuiaPractica {
    public string Nombre { get; init; } = "";

    public string Tipo { get; init; } = "";

    public string Descripcion { get; init; } = "";

    public string Ejemplo { get; init; } = "";
}

public sealed class ConceptoGuiaPractica {
    public string Nombre { get; init; } = "";

    public string Explicacion { get; init; } = "";

    public string Fragmento { get; init; } = "";
}

public sealed class HerramientaGuiaPractica {
    public string Nombre { get; init; } = "";

    public string Descripcion { get; init; } = "";

    public string ParaQueSirve { get; init; } = "";

    public string Codigo { get; init; } = "";

    public string AclaracionOpcional { get; init; } = "";
}

public sealed class EjemploEjecucionPractica {
    public string Entrada { get; init; } = "";

    public string SalidaEsperada { get; init; } = "";
}
