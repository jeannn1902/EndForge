namespace EndForge.Models;

public sealed class ResultadoColeccionComparada {
    public string Nombre { get; init; } = "";

    public string EtiquetaEncontrada { get; init; } = "";

    public bool RegionEncontrada { get; init; }

    public IReadOnlyList<string> ElementosEncontrados { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> ElementosFaltantes { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> ElementosAdicionales { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> DuplicadosInesperados { get; init; } =
        Array.Empty<string>();

    public int CantidadEsperada { get; init; }

    public int CantidadEncontrada { get; init; }

    public bool CantidadCorrecta { get; init; }

    public bool OrdenCorrecto { get; init; }

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoCadenaComparada {
    public string Nombre { get; init; } = "";

    public string ValorEsperado { get; init; } = "";

    public IReadOnlyList<string> ValoresEncontrados { get; init; } =
        Array.Empty<string>();

    public string EtiquetaEncontrada { get; init; } = "";

    public bool EtiquetaPresente { get; init; }

    public bool CoincideMayusculas { get; init; }

    public bool CoincideAcentos { get; init; }

    public bool CoincideEspacios { get; init; }

    public bool TieneTextoAdicional { get; init; }

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoTablaComparada {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<ResultadoFilaTablaComparada> Filas { get; init; } =
        Array.Empty<ResultadoFilaTablaComparada>();

    public int CantidadEsperada { get; init; }

    public int CantidadEncontrada { get; init; }

    public bool CantidadCorrecta { get; init; }

    public bool OrdenCorrecto { get; init; }

    public IReadOnlyList<string> FilasFaltantes { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> FilasDuplicadas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> FilasAdicionales { get; init; } =
        Array.Empty<string>();

    public int? PrimeraFilaIncorrecta { get; init; }

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoFilaTablaComparada {
    public string Nombre { get; init; } = "";

    public string ClaveEsperada { get; init; } = "";

    public string ClaveEncontrada { get; init; } = "";

    public int NumeroFila { get; init; }

    public IReadOnlyList<ResultadoCeldaTablaComparada> Celdas { get; init; } =
        Array.Empty<ResultadoCeldaTablaComparada>();

    public bool EsDuplicada { get; init; }

    public bool Coincide { get; init; }
}

public sealed class ResultadoCeldaTablaComparada {
    public string Nombre { get; init; } = "";

    public int Fila { get; init; }

    public int Columna { get; init; }

    public string ValorEsperado { get; init; } = "";

    public string ValorEncontrado { get; init; } = "";

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }
}

public sealed class ResultadoMatrizComparada {
    public string Nombre { get; init; } = "";

    public int FilasEsperadas { get; init; }

    public int ColumnasEsperadas { get; init; }

    public int FilasEncontradas { get; init; }

    public int ColumnasEncontradas { get; init; }

    public bool DimensionesCorrectas { get; init; }

    public bool EsTranspuesta { get; init; }

    public string PrimeraCeldaIncorrecta { get; init; } = "";

    public IReadOnlyList<string> ElementosAdicionales { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> FilasIncompletas { get; init; } =
        Array.Empty<string>();

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoBloquesRegistroComparados {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<ResultadoRegistroComparado> Registros { get; init; } =
        Array.Empty<ResultadoRegistroComparado>();

    public int CantidadEsperada { get; init; }

    public int CantidadEncontrada { get; init; }

    public bool OrdenCorrecto { get; init; }

    public IReadOnlyList<string> RegistrosFaltantes { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> RegistrosDuplicados { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> RegistrosAdicionales { get; init; } =
        Array.Empty<string>();

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }

    public string Mensaje { get; init; } = "";
}

public sealed class ResultadoRegistroComparado {
    public string Nombre { get; init; } = "";

    public string ClaveEsperada { get; init; } = "";

    public string ClaveEncontrada { get; init; } = "";

    public int NumeroBloque { get; init; }

    public IReadOnlyList<ResultadoCampoRegistroComparado> Campos { get; init; } =
        Array.Empty<ResultadoCampoRegistroComparado>();

    public bool EsDuplicado { get; init; }

    public bool Coincide { get; init; }
}

public sealed class ResultadoCampoRegistroComparado {
    public string Nombre { get; init; } = "";

    public string ValorEsperado { get; init; } = "";

    public IReadOnlyList<string> ValoresEncontrados { get; init; } =
        Array.Empty<string>();

    public string EtiquetaEncontrada { get; init; } = "";

    public bool EsObligatorio { get; init; }

    public bool TieneContradiccion { get; init; }

    public bool Coincide { get; init; }
}
