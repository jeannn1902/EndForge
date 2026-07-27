namespace EndForge.Models;

public enum ModoComparacionArchivoPrueba {
    TextoExacto,
    Estructurado
}

public enum EstadoArchivoPrueba {
    Disponible,
    Ausente,
    RutaInvalida,
    TipoInvalido,
    PuntoDeReanalisis,
    ContenidoExcesivo,
    ErrorLectura
}

public sealed class ArchivoEntradaPrueba {
    public string RutaRelativa { get; init; } = "";

    public string Contenido { get; init; } = "";
}

public sealed class ArchivoEsperadoPrueba {
    public string RutaRelativa { get; init; } = "";

    public string ContenidoEsperado { get; init; } = "";

    public ModoComparacionArchivoPrueba ModoComparacion { get; init; } =
        ModoComparacionArchivoPrueba.TextoExacto;

    public bool PermitirUnSaltoLineaFinal { get; init; }

    public IReadOnlyList<ReglaCadenaEsperada> CadenasEsperadas { get; init; } =
        Array.Empty<ReglaCadenaEsperada>();

    public IReadOnlyList<ReglaTablaEsperada> TablasEsperadas { get; init; } =
        Array.Empty<ReglaTablaEsperada>();

    public IReadOnlyList<ReglaBloquesRegistroEsperados>
        BloquesRegistroEsperados { get; init; } =
            Array.Empty<ReglaBloquesRegistroEsperados>();
}

public sealed class ReglaSalidaExactaPrueba {
    public string ValorEsperado { get; init; } = "";

    public bool PermitirUnSaltoLineaFinal { get; init; } = true;
}

public sealed class ResultadoArchivoPrueba {
    public string RutaRelativa { get; init; } = "";

    public EstadoArchivoPrueba Estado { get; init; }

    public string ContenidoObtenido { get; init; } = "";

    public Exception? Error { get; init; }

    public bool Disponible => Estado == EstadoArchivoPrueba.Disponible;
}
