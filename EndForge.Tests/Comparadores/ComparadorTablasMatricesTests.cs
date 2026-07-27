using EndForge.Models;

namespace EndForge.Tests.Comparadores;

public sealed class ComparadorTablasMatricesTests {
    [Fact]
    public void Tabla_AceptaNombresCompuestosNegativosYComaDecimal() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            tablas: new[] { CrearTablaPersonas() });
        string salida = """
            Tabla:
            A: Ana María López|9,5
            B: José Pérez|-2
            Fin:
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.True(resultado.EsCorrecta);
        ResultadoTablaComparada tabla = Assert.Single(resultado.Tablas);
        Assert.Equal(2, tabla.CantidadEncontrada);
        Assert.All(tabla.Filas, fila => Assert.True(fila.Coincide));
    }

    [Fact]
    public void Tabla_FilaFaltante_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            tablas: new[] { CrearTablaPersonas() });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Tabla:\nA: Ana María López|9.5\nFin:");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            "Persona B",
            Assert.Single(resultado.Tablas).FilasFaltantes);
    }

    [Fact]
    public void Tabla_FilaAdicional_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            tablas: new[] { CrearTablaPersonas() });
        string salida = """
            Tabla:
            A: Ana María López|9.5
            B: José Pérez|-2
            C: Laura Díaz|8
            Fin:
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            Assert.Single(resultado.Tablas).FilasAdicionales,
            fila => fila.StartsWith("C:", StringComparison.Ordinal));
    }

    [Fact]
    public void Tabla_OrdenIncorrecto_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            tablas: new[] { CrearTablaPersonas() });
        string salida = """
            Tabla:
            B: José Pérez|-2
            A: Ana María López|9.5
            Fin:
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.Tablas).OrdenCorrecto);
    }

    [Fact]
    public void Tabla_ClaveDuplicada_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            tablas: new[] { CrearTablaPersonas() });
        string salida = """
            Tabla:
            A: Ana María López|9.5
            A: Ana María López|9.5
            B: José Pérez|-2
            Fin:
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            "Persona A",
            Assert.Single(resultado.Tablas).FilasDuplicadas);
    }

    [Fact]
    public void Tabla_CeldaIncorrecta_IndicaPrimeraFila() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            tablas: new[] { CrearTablaPersonas() });
        string salida = """
            Tabla:
            A: Ana María López|9.5
            B: José Pérez|-3
            Fin:
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        ResultadoTablaComparada tabla = Assert.Single(resultado.Tablas);
        Assert.NotNull(tabla.PrimeraFilaIncorrecta);
        Assert.Contains(
            tabla.Filas.SelectMany(fila => fila.Celdas),
            celda => celda.Nombre == "Promedio" && !celda.Coincide);
    }

    [Fact]
    public void MatrizUnoPorUno_ConCero_Aprueba() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(
            new[] { new[] { 0D } });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Matriz:\n0");

        Assert.True(resultado.EsCorrecta);
        ResultadoMatrizComparada matriz = Assert.Single(resultado.Matrices);
        Assert.Equal(1, matriz.FilasEncontradas);
        Assert.Equal(1, matriz.ColumnasEncontradas);
    }

    [Fact]
    public void Matriz_AceptaNegativosYComaDecimal() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(new[] {
            new[] { 0D, -2.5D },
            new[] { 3.25D, 4D }
        });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });
        string salida = """
            Matriz:
            0;-2,5
            3,25;4
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.True(resultado.EsCorrecta);
    }

    [Fact]
    public void Matriz_ListaPlana_SeRechazaPorDimensiones() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(new[] {
            new[] { 1D, 2D },
            new[] { 3D, 4D }
        });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Matriz:\n1;2;3;4");

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.Matrices).DimensionesCorrectas);
    }

    [Fact]
    public void Matriz_RectangularTranspuesta_SeDetecta() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(new[] {
            new[] { 1D, 2D, 3D },
            new[] { 4D, 5D, 6D }
        });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });
        string salida = """
            Matriz:
            1;4
            2;5
            3;6
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.True(Assert.Single(resultado.Matrices).EsTranspuesta);
    }

    [Fact]
    public void Matriz_FilaIncompleta_SeReporta() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(new[] {
            new[] { 1D, 2D },
            new[] { 3D, 4D }
        });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Matriz:\n1;2\n3");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains("Fila 2", Assert.Single(resultado.Matrices).FilasIncompletas);
    }

    [Fact]
    public void Matriz_ElementoAdicional_SeRechaza() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(new[] {
            new[] { 1D, 2D },
            new[] { 3D, 4D }
        });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Matriz:\n1;2;99\n3;4");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains("[1,3]", Assert.Single(resultado.Matrices).ElementosAdicionales);
    }

    [Fact]
    public void Matriz_ValorEnPosicionIncorrecta_SeRechaza() {
        ReglaMatrizEsperada regla = CrearMatrizNumerica(new[] {
            new[] { 1D, 2D },
            new[] { 3D, 4D }
        });
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            matrices: new[] { regla });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Matriz:\n1;3\n2;4");

        Assert.False(resultado.EsCorrecta);
        Assert.Equal(
            "[1,2]",
            Assert.Single(resultado.Matrices).PrimeraCeldaIncorrecta);
    }

    private static ReglaTablaEsperada CrearTablaPersonas() {
        return new ReglaTablaEsperada {
            Nombre = "Personas",
            EtiquetasInicio = new[] { "Tabla" },
            EtiquetasFin = new[] { "Fin" },
            FilasEsperadas = new[] {
                CrearFilaPersona("A", "Ana María López", 9.5D),
                CrearFilaPersona("B", "José Pérez", -2D)
            },
            CantidadFilasExacta = 2,
            CantidadColumnasExacta = 2,
            OrdenFilasObligatorio = true,
            PermitirFilasAdicionales = false,
            PermitirFilasDuplicadas = false,
            SeparadoresColumnas = new[] { "|" }
        };
    }

    private static FilaTablaEsperada CrearFilaPersona(
        string clave,
        string nombre,
        double promedio) {
        return new FilaTablaEsperada {
            Nombre = $"Persona {clave}",
            Clave = clave,
            Celdas = new[] {
                new CeldaTablaEsperada {
                    Nombre = "Nombre",
                    Posicion = 0,
                    Valor = ComparadorTestFactory.Texto(
                        nombre,
                        distinguirMayusculas: false,
                        distinguirAcentos: false,
                        PoliticaEspaciosCadena.ColapsarInternos)
                },
                new CeldaTablaEsperada {
                    Nombre = "Promedio",
                    Posicion = 1,
                    Valor = ComparadorTestFactory.Numero(promedio)
                }
            }
        };
    }

    private static ReglaMatrizEsperada CrearMatrizNumerica(
        IReadOnlyList<IReadOnlyList<double>> valores) {
        return new ReglaMatrizEsperada {
            Nombre = "Matriz numérica",
            EtiquetasInicio = new[] { "Matriz" },
            RequerirEtiqueta = true,
            FilasEsperadas = valores.Count,
            ColumnasEsperadas = valores.Count == 0 ? 0 : valores[0].Count,
            TipoElementos = TipoValorEstructurado.Numerico,
            ValoresNumericosEsperados = valores,
            ToleranciaNumerica = 0.001D,
            SeparadoresColumnas = new[] { ";" },
            PermitirElementosAdicionales = false
        };
    }
}
