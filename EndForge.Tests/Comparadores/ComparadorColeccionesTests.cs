using EndForge.Models;

namespace EndForge.Tests.Comparadores;

public sealed class ComparadorColeccionesTests {
    [Fact]
    public void ColeccionNumerica_AceptaCeroNegativosYComaDecimal() {
        ReglaColeccionEsperada regla = new() {
            Nombre = "Valores",
            TipoElementos = TipoValorEstructurado.Numerico,
            EtiquetasInicio = new[] { "Valores" },
            ElementosEsperados = new[] {
                ComparadorTestFactory.Numero(0D),
                ComparadorTestFactory.Numero(-2.5D),
                ComparadorTestFactory.Numero(3.25D)
            },
            CantidadExacta = 3,
            OrdenObligatorio = true,
            PermitirDuplicados = false,
            PermitirElementosAdicionales = false,
            Separadores = new[] { ";" },
            ToleranciaNumerica = 0.001D
        };
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Valores: 0; -2,5; 3,250");

        Assert.True(resultado.EsCorrecta);
        ResultadoColeccionComparada coleccion =
            Assert.Single(resultado.Colecciones);
        Assert.Equal(new[] { "0", "-2.5", "3.25" }, coleccion.ElementosEncontrados);
    }

    [Fact]
    public void ColeccionVacia_ConCantidadCero_Aprueba() {
        ReglaColeccionEsperada regla = new() {
            Nombre = "Valores",
            TipoElementos = TipoValorEstructurado.Numerico,
            EtiquetasInicio = new[] { "Valores" },
            ElementosEsperados = Array.Empty<ValorEstructuradoEsperado>(),
            CantidadExacta = 0
        };
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Valores:");

        Assert.True(resultado.EsCorrecta);
        ResultadoColeccionComparada coleccion =
            Assert.Single(resultado.Colecciones);
        Assert.Equal(0, coleccion.CantidadEncontrada);
        Assert.True(coleccion.CantidadCorrecta);
    }

    [Fact]
    public void ColeccionTextual_AdmiteNombresCompuestosYAcentosFlexibles() {
        ReglaColeccionEsperada regla = new() {
            Nombre = "Nombres",
            TipoElementos = TipoValorEstructurado.Textual,
            EtiquetasInicio = new[] { "Nombres" },
            ElementosEsperados = new[] {
                ComparadorTestFactory.Texto("Ana María"),
                ComparadorTestFactory.Texto("José Pérez")
            },
            CantidadExacta = 2,
            OrdenObligatorio = true,
            Separadores = new[] { ";" },
            DistinguirMayusculas = false
        };
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "NOMBRES: ana maria; JOSÉ PÉREZ");

        Assert.True(resultado.EsCorrecta);
    }

    [Fact]
    public void ColeccionBooleanos_AceptaRepresentacionesEquivalentes() {
        ReglaColeccionEsperada regla = new() {
            Nombre = "Estados",
            TipoElementos = TipoValorEstructurado.Booleano,
            EtiquetasInicio = new[] { "Estados" },
            ElementosEsperados = new[] {
                ComparadorTestFactory.Booleano(true),
                ComparadorTestFactory.Booleano(false),
                ComparadorTestFactory.Booleano(true)
            },
            CantidadExacta = 3,
            OrdenObligatorio = true,
            PermitirDuplicados = true,
            Separadores = new[] { ";" }
        };
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Estados: sí; 0; TRUE");

        Assert.True(resultado.EsCorrecta);
    }

    [Fact]
    public void Coleccion_OrdenIncorrecto_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { CrearColeccionNumerica(1D, 2D, 3D) });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Valores: 1 3 2");

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.Colecciones).OrdenCorrecto);
    }

    [Theory]
    [InlineData("Valores: 1 2", "3")]
    [InlineData("Valores: 1 2 3 4", "4")]
    public void Coleccion_ElementoFaltanteOAdicional_SeRechaza(
        string salida,
        string elemento) {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { CrearColeccionNumerica(1D, 2D, 3D) });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        ResultadoColeccionComparada coleccion =
            Assert.Single(resultado.Colecciones);
        Assert.True(
            coleccion.ElementosFaltantes.Contains(elemento) ||
            coleccion.ElementosAdicionales.Contains(elemento));
    }

    [Fact]
    public void Coleccion_DuplicadoNoPermitido_SeReporta() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { CrearColeccionNumerica(1D, 2D, 3D) });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Valores: 1 2 2 3");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            "2",
            Assert.Single(resultado.Colecciones).DuplicadosInesperados);
    }

    [Fact]
    public void Coleccion_MismaEtiquetaConResultadosDiferentes_EsContradiccion() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            colecciones: new[] { CrearColeccionNumerica(1D, 2D) });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Valores: 1 2\nValores: 1 3");

        Assert.False(resultado.EsCorrecta);
        ResultadoColeccionComparada coleccion =
            Assert.Single(resultado.Colecciones);
        Assert.True(coleccion.TieneContradiccion);
        Assert.Contains("Valores", resultado.ContradiccionesDetectadas);
    }

    private static ReglaColeccionEsperada CrearColeccionNumerica(
        params double[] valores) {
        return new ReglaColeccionEsperada {
            Nombre = "Valores",
            TipoElementos = TipoValorEstructurado.Numerico,
            EtiquetasInicio = new[] { "Valores" },
            ElementosEsperados = valores
                .Select(valor => ComparadorTestFactory.Numero(valor))
                .ToArray(),
            CantidadExacta = valores.Length,
            OrdenObligatorio = true,
            PermitirDuplicados = false,
            PermitirElementosAdicionales = false
        };
    }
}
