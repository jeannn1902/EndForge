namespace EndForge.Models;

public sealed class ResultadoComparacionSalida {
    public bool EsCorrecta { get; init; }

    public bool CumpleEstructura { get; init; }

    public bool EsSalidaLegible { get; init; }

    public IReadOnlyList<string> TokensFaltantes { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> GruposAlternativosFaltantes { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<ResultadoValorNumericoComparado> ValoresNumericos { get; init; } =
        Array.Empty<ResultadoValorNumericoComparado>();

    public IReadOnlyList<ResultadoValorBooleanoComparado> ValoresBooleanos { get; init; } =
        Array.Empty<ResultadoValorBooleanoComparado>();

    public IReadOnlyList<ResultadoValorTextualComparado> ValoresTextuales { get; init; } =
        Array.Empty<ResultadoValorTextualComparado>();

    public IReadOnlyList<string> ReglasCumplidas { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ReglasIncumplidas { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> ContradiccionesDetectadas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> EtiquetasAlternativasReconocidas { get; init; } =
        Array.Empty<string>();

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoValorNumericoComparado {
    public string Nombre { get; init; } = "";

    public double ValorEsperado { get; init; }

    public double? ValorObtenido { get; init; }

    public double Tolerancia { get; init; }

    public bool EsOpcional { get; init; }

    public bool DebeEstarAusente { get; init; }

    public bool Coincide { get; init; }

    public string EtiquetaEncontrada { get; init; } = "";

    public bool TieneContradiccion { get; init; }

    public bool UsoEtiquetaAlternativa { get; init; }

    public IReadOnlyList<double> ValoresEncontrados { get; init; } = Array.Empty<double>();
}

public sealed class ResultadoValorBooleanoComparado {
    public string Nombre { get; init; } = "";

    public bool ValorEsperado { get; init; }

    public bool? ValorObtenido { get; init; }

    public bool Coincide { get; init; }

    public string EtiquetaEncontrada { get; init; } = "";

    public string RepresentacionEncontrada { get; init; } = "";

    public bool TieneContradiccion { get; init; }

    public bool UsoEtiquetaAlternativa { get; init; }

    public IReadOnlyList<bool> ValoresEncontrados { get; init; } = Array.Empty<bool>();
}

public sealed class ResultadoValorTextualComparado {
    public string Nombre { get; init; } = "";

    public string ValorEsperado { get; init; } = "";

    public bool EsOpcional { get; init; }

    public string ValorObtenido { get; init; } = "";

    public bool Coincide { get; init; }

    public string EtiquetaEncontrada { get; init; } = "";

    public string RepresentacionEncontrada { get; init; } = "";

    public bool TieneContradiccion { get; init; }

    public bool UsoEtiquetaAlternativa { get; init; }

    public IReadOnlyList<string> ValoresEncontrados { get; init; } = Array.Empty<string>();
}
