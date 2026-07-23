namespace EndForge.Models;

public enum ModoComparacionCaso {
    Valores,
    Texto,
    Mixto,
    Secuencia
}

public enum TipoSecuenciaEsperada {
    Numerica,
    Textual
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

    public IReadOnlyList<ValorTextualEsperado> ValoresTextualesEsperados { get; init; } =
        Array.Empty<ValorTextualEsperado>();

    public IReadOnlyList<SecuenciaEsperada> SecuenciasEsperadas { get; init; } =
        Array.Empty<SecuenciaEsperada>();
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

    public bool EsOpcional { get; init; }

    public bool DebeEstarAusente { get; init; }

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<double> ValoresEquivalentes { get; init; } =
        Array.Empty<double>();
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

public sealed class ValorTextualEsperado {
    public string Nombre { get; init; } = "";

    public string Valor { get; init; } = "";

    public bool EsOpcional { get; init; }

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<OpcionValorTextual> Opciones { get; init; } =
        Array.Empty<OpcionValorTextual>();
}

public sealed class OpcionValorTextual {
    public string Valor { get; init; } = "";

    public IReadOnlyList<string> Alternativas { get; init; } = Array.Empty<string>();
}

public sealed class SecuenciaEsperada {
    public string Nombre { get; init; } = "";

    public TipoSecuenciaEsperada Tipo { get; init; }

    public IReadOnlyList<double> ValoresNumericosEsperados { get; init; } =
        Array.Empty<double>();

    public IReadOnlyList<ElementoTextualSecuenciaEsperado>
        AlternativasTextualesEsperadas { get; init; } =
            Array.Empty<ElementoTextualSecuenciaEsperado>();

    public bool OrdenObligatorio { get; init; } = true;

    public int? CantidadExacta { get; init; }

    public bool PermitirDuplicados { get; init; }

    public bool PermitirElementosAdicionales { get; init; }

    public double ToleranciaNumerica { get; init; } = 0.01D;

    public IReadOnlyList<string> SeparadoresPermitidos { get; init; } =
        Array.AsReadOnly(new[] { " ", "\t", "\r", "\n", ",", ";" });

    public bool PermitirTextoAdicional { get; init; } = true;
}

public sealed class ElementoTextualSecuenciaEsperado {
    public string Valor { get; init; } = "";

    public IReadOnlyList<string> Alternativas { get; init; } =
        Array.Empty<string>();
}
