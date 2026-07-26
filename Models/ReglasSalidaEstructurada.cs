namespace EndForge.Models;

public enum TipoValorEstructurado {
    Numerico,
    Textual,
    Booleano
}

public enum PoliticaEspaciosCadena {
    Exactos,
    RecortarExtremos,
    ColapsarInternos
}

public enum OrigenCadenaEsperada {
    LineaCompleta,
    DespuesDeEtiqueta
}

public enum ModoRegionColeccion {
    MismaLineaTrasEtiqueta,
    BloqueHastaLineaVacia,
    BloqueHastaEtiquetaFin
}

public sealed class ValorEstructuradoEsperado {
    public string Nombre { get; init; } = "";

    public TipoValorEstructurado Tipo { get; init; }

    public double ValorNumerico { get; init; }

    public string ValorTextual { get; init; } = "";

    public bool ValorBooleano { get; init; }

    public IReadOnlyList<string> AlternativasTextuales { get; init; } =
        Array.Empty<string>();

    public double ToleranciaNumerica { get; init; } = 0.01D;

    public bool DistinguirMayusculas { get; init; }

    public bool DistinguirAcentos { get; init; }

    public PoliticaEspaciosCadena PoliticaEspacios { get; init; } =
        PoliticaEspaciosCadena.RecortarExtremos;

    public IReadOnlyList<string> RepresentacionesVerdaderas { get; init; } =
        Array.AsReadOnly(new[] { "si", "sí", "true", "verdadero", "1" });

    public IReadOnlyList<string> RepresentacionesFalsas { get; init; } =
        Array.AsReadOnly(new[] { "no", "false", "falso", "0" });
}

public sealed class ReglaColeccionEsperada {
    public string Nombre { get; init; } = "";

    public TipoValorEstructurado TipoElementos { get; init; }

    public IReadOnlyList<ValorEstructuradoEsperado> ElementosEsperados { get; init; } =
        Array.Empty<ValorEstructuradoEsperado>();

    public IReadOnlyList<string> EtiquetasInicio { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> EtiquetasFin { get; init; } =
        Array.Empty<string>();

    public ModoRegionColeccion Region { get; init; } =
        ModoRegionColeccion.MismaLineaTrasEtiqueta;

    public bool RequerirEtiqueta { get; init; } = true;

    public bool OrdenObligatorio { get; init; } = true;

    public int? CantidadExacta { get; init; }

    public bool PermitirDuplicados { get; init; }

    public bool PermitirElementosAdicionales { get; init; }

    public double ToleranciaNumerica { get; init; } = 0.01D;

    public bool DistinguirMayusculas { get; init; }

    public bool ConsumirAparicionesUnaVez { get; init; } = true;

    public bool Obligatoria { get; init; } = true;

    public string MensajeError { get; init; } = "";

    public IReadOnlyList<string> Separadores { get; init; } =
        Array.AsReadOnly(new[] { " ", "\t", "\r", "\n", ",", ";" });
}

public sealed class ReglaCadenaEsperada {
    public string Nombre { get; init; } = "";

    public string ValorEsperado { get; init; } = "";

    public IReadOnlyList<string> AlternativasValidas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();

    public OrigenCadenaEsperada Origen { get; init; } =
        OrigenCadenaEsperada.DespuesDeEtiqueta;

    public bool DistinguirMayusculas { get; init; } = true;

    public bool DistinguirAcentos { get; init; } = true;

    public PoliticaEspaciosCadena PoliticaEspacios { get; init; } =
        PoliticaEspaciosCadena.Exactos;

    public bool PermitirTextoAdicional { get; init; }

    public bool Obligatoria { get; init; } = true;

    public string MensajeError { get; init; } = "";
}

public sealed class ReglaTablaEsperada {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<string> EtiquetasInicio { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> EtiquetasFin { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<FilaTablaEsperada> FilasEsperadas { get; init; } =
        Array.Empty<FilaTablaEsperada>();

    public int? CantidadFilasExacta { get; init; }

    public int? CantidadColumnasExacta { get; init; }

    public bool OrdenFilasObligatorio { get; init; } = true;

    public bool PermitirFilasAdicionales { get; init; }

    public bool PermitirFilasDuplicadas { get; init; }

    public bool PermitirTextoNeutralEntreFilas { get; init; } = true;

    public IReadOnlyList<string> SeparadoresColumnas { get; init; } =
        Array.AsReadOnly(new[] { "\t", "|", ";", "," });

    public bool Obligatoria { get; init; } = true;

    public string MensajeError { get; init; } = "";
}

public sealed class FilaTablaEsperada {
    public string Nombre { get; init; } = "";

    public string Clave { get; init; } = "";

    public IReadOnlyList<string> ClavesAlternativas { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<CeldaTablaEsperada> Celdas { get; init; } =
        Array.Empty<CeldaTablaEsperada>();
}

public sealed class CeldaTablaEsperada {
    public string Nombre { get; init; } = "";

    public int Posicion { get; init; }

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();

    public ValorEstructuradoEsperado Valor { get; init; } = new();
}

public sealed class ReglaMatrizEsperada {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<string> EtiquetasInicio { get; init; } =
        Array.Empty<string>();

    public bool RequerirEtiqueta { get; init; }

    public int FilasEsperadas { get; init; }

    public int ColumnasEsperadas { get; init; }

    public TipoValorEstructurado TipoElementos { get; init; }

    public IReadOnlyList<IReadOnlyList<double>> ValoresNumericosEsperados {
        get;
        init;
    } = Array.Empty<IReadOnlyList<double>>();

    public IReadOnlyList<IReadOnlyList<string>> ValoresTextualesEsperados {
        get;
        init;
    } = Array.Empty<IReadOnlyList<string>>();

    public double ToleranciaNumerica { get; init; } = 0.01D;

    public bool DistinguirMayusculas { get; init; }

    public bool DistinguirAcentos { get; init; }

    public PoliticaEspaciosCadena PoliticaEspacios { get; init; } =
        PoliticaEspaciosCadena.RecortarExtremos;

    public IReadOnlyList<string> SeparadoresColumnas { get; init; } =
        Array.AsReadOnly(new[] { " ", "\t", ",", ";" });

    public bool PermitirElementosAdicionales { get; init; }

    public bool PermitirTextoNeutralExterno { get; init; } = true;

    public bool Obligatoria { get; init; } = true;

    public string MensajeError { get; init; } = "";
}

public sealed class ReglaBloquesRegistroEsperados {
    public string Nombre { get; init; } = "";

    public string NombreCampoClave { get; init; } = "";

    public IReadOnlyList<string> EtiquetasClave { get; init; } =
        Array.Empty<string>();

    public TipoValorEstructurado TipoClave { get; init; }

    public IReadOnlyList<RegistroEsperado> RegistrosEsperados { get; init; } =
        Array.Empty<RegistroEsperado>();

    public bool OrdenRegistrosObligatorio { get; init; }

    public bool PermitirRegistrosAdicionales { get; init; }

    public bool PermitirRegistrosDuplicados { get; init; }

    public bool PermitirTextoNeutralEntreBloques { get; init; } = true;

    public bool Obligatoria { get; init; } = true;

    public string MensajeError { get; init; } = "";
}

public sealed class RegistroEsperado {
    public string Nombre { get; init; } = "";

    public ValorEstructuradoEsperado Clave { get; init; } = new();

    public IReadOnlyList<CampoRegistroEsperado> Campos { get; init; } =
        Array.Empty<CampoRegistroEsperado>();
}

public sealed class CampoRegistroEsperado {
    public string Nombre { get; init; } = "";

    public IReadOnlyList<string> EtiquetasAlternativas { get; init; } =
        Array.Empty<string>();

    public ValorEstructuradoEsperado Valor { get; init; } = new();

    public bool Obligatorio { get; init; } = true;
}
