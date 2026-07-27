using EndForge.Models;

namespace EndForge.Tests.Comparadores;

public sealed class ComparadorCadenasTests {
    [Fact]
    public void CadenaVacia_ComoLineaCompleta_ApruebaSalidaVacia() {
        ReglaCadenaEsperada regla = new() {
            Nombre = "Cadena vacía",
            ValorEsperado = string.Empty,
            Origen = OrigenCadenaEsperada.LineaCompleta,
            DistinguirMayusculas = true,
            DistinguirAcentos = true,
            PoliticaEspacios = PoliticaEspaciosCadena.Exactos
        };
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, string.Empty);

        Assert.True(resultado.EsCorrecta);
        Assert.True(resultado.EsSalidaLegible);
        Assert.Equal(string.Empty, Assert.Single(resultado.Cadenas).ValoresEncontrados[0]);
    }

    [Fact]
    public void Cadena_RecortaExtremosYColapsaEspaciosInternos() {
        ReglaCadenaEsperada regla = CrearCadena(
            "Ana María López",
            mayusculas: false,
            acentos: false,
            PoliticaEspaciosCadena.ColapsarInternos);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Nombre:   ANA   MARIA   LÓPEZ   ");

        Assert.True(resultado.EsCorrecta);
    }

    [Fact]
    public void Cadena_Exacta_DetectaMayusculasIncorrectas() {
        ReglaCadenaEsperada regla = CrearCadena(
            "EndForge",
            mayusculas: true,
            acentos: true,
            PoliticaEspaciosCadena.Exactos);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Nombre: endforge");

        Assert.False(resultado.EsCorrecta);
        ResultadoCadenaComparada cadena = Assert.Single(resultado.Cadenas);
        Assert.False(cadena.CoincideMayusculas);
    }

    [Fact]
    public void Cadena_Exacta_DetectaAcentoAusente() {
        ReglaCadenaEsperada regla = CrearCadena(
            "Programación",
            mayusculas: false,
            acentos: true,
            PoliticaEspaciosCadena.Exactos);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Nombre: Programacion");

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.Cadenas).CoincideAcentos);
    }

    [Fact]
    public void Cadena_Exacta_DetectaEspaciosInternosDiferentes() {
        ReglaCadenaEsperada regla = CrearCadena(
            "Ana María",
            mayusculas: true,
            acentos: true,
            PoliticaEspaciosCadena.Exactos);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Nombre: Ana  María");

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.Cadenas).CoincideEspacios);
    }

    [Theory]
    [InlineData("Nombre: Ana")]
    [InlineData("Nombre: Ana María López adicional")]
    public void Cadena_TruncadaOExtendida_SeRechaza(string salida) {
        ReglaCadenaEsperada regla = CrearCadena(
            "Ana María López",
            mayusculas: true,
            acentos: true,
            PoliticaEspaciosCadena.Exactos);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.Cadenas).Coincide);
    }

    [Fact]
    public void Cadena_DosValoresDiferentesParaMismaEtiqueta_EsContradiccion() {
        ReglaCadenaEsperada regla = CrearCadena(
            "Ana María",
            mayusculas: false,
            acentos: false,
            PoliticaEspaciosCadena.ColapsarInternos);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Nombre: Ana María\nNombre: Laura");

        Assert.False(resultado.EsCorrecta);
        Assert.True(Assert.Single(resultado.Cadenas).TieneContradiccion);
        Assert.Contains("Nombre", resultado.ContradiccionesDetectadas);
    }

    [Fact]
    public void Cadena_LineaCompleta_RechazaUnaSegundaLineaNoPermitida() {
        ReglaCadenaEsperada regla = new() {
            Nombre = "Salida exacta",
            ValorEsperado = "Hola",
            Origen = OrigenCadenaEsperada.LineaCompleta,
            PermitirTextoAdicional = false
        };
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            cadenas: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Hola\nTexto extra");

        Assert.False(resultado.EsCorrecta);
        Assert.True(Assert.Single(resultado.Cadenas).TieneTextoAdicional);
    }

    private static ReglaCadenaEsperada CrearCadena(
        string valor,
        bool mayusculas,
        bool acentos,
        PoliticaEspaciosCadena espacios) {
        return new ReglaCadenaEsperada {
            Nombre = "Nombre",
            ValorEsperado = valor,
            EtiquetasAlternativas = new[] { "Nombre", "Nombre completo" },
            Origen = OrigenCadenaEsperada.DespuesDeEtiqueta,
            DistinguirMayusculas = mayusculas,
            DistinguirAcentos = acentos,
            PoliticaEspacios = espacios,
            PermitirTextoAdicional = false
        };
    }
}
