namespace EndForge.Models;

public enum ModoComparacionCaso {
    Valores,
    Texto,
    Mixto
}

public sealed class CasoPrueba {
    public string Id { get; init; } = "";

    public string Nombre { get; init; } = "";

    public string Entrada { get; init; } = "";

    public string SalidaEsperada { get; init; } = "";

    public bool EsVisible { get; init; } = true;

    public int Puntos { get; init; }

    // Se conserva para compatibilidad con definiciones creadas antes de que
    // existieran modos explícitos. Las comparaciones nuevas usan ModoComparacion.
    public bool ComparacionFlexible { get; init; }

    public ModoComparacionCaso ModoComparacion { get; init; } =
        ModoComparacionCaso.Mixto;

    public string Descripcion { get; init; } = "";

    public IReadOnlyList<string> TokensObligatorios { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<GrupoTokensEsperados> GruposTokensAlternativos { get; init; } =
        Array.Empty<GrupoTokensEsperados>();

    public IReadOnlyList<ValorNumericoEsperado> ValoresNumericosEsperados { get; init; } =
        Array.Empty<ValorNumericoEsperado>();

    public IReadOnlyList<ValorBooleanoEsperado> ValoresBooleanosEsperados { get; init; } =
        Array.Empty<ValorBooleanoEsperado>();
}

public sealed class GrupoTokensEsperados {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<string> Alternativas { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> EtiquetasAsociadas { get; init; } =
        Array.Empty<string>();
}

public sealed class ValorNumericoEsperado {
    public string Nombre { get; init; } = "";

    public double Valor { get; init; }

    public double Tolerancia { get; init; } = 0.01D;

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();
}

public sealed class ValorBooleanoEsperado {
    public string Nombre { get; init; } = "";

    public bool Valor { get; init; }

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> RepresentacionesVerdaderas { get; init; } =
        Array.AsReadOnly(new[] { "si", "sí", "true", "verdadero", "1" });

    public IReadOnlyList<string> RepresentacionesFalsas { get; init; } =
        Array.AsReadOnly(new[] { "no", "false", "falso", "0" });
}
