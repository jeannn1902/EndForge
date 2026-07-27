using EndForge.Models;

namespace EndForge.Tests.Comparadores;

public sealed class ComparadorRegistrosTests {
    [Fact]
    public void Registros_AceptanNombresCompuestosBooleanosYComaDecimal() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, SalidaCorrecta());

        Assert.True(resultado.EsCorrecta);
        ResultadoBloquesRegistroComparados registros =
            Assert.Single(resultado.BloquesRegistro);
        Assert.Equal(2, registros.CantidadEncontrada);
        Assert.All(registros.Registros, registro => Assert.True(registro.Coincide));
    }

    [Fact]
    public void Registros_CamposMezcladosEntreIds_SeRechazan() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });
        string salida = """
            ID: 1
            Nombre: José Pérez
            Promedio: 9,5
            Activo: sí
            ID: 2
            Nombre: Ana María López
            Promedio: -1,25
            Activo: no
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            Assert.Single(resultado.BloquesRegistro).Registros
                .SelectMany(registro => registro.Campos),
            campo => campo.Nombre == "Nombre" && !campo.Coincide);
    }

    [Fact]
    public void Registros_RegistroFaltante_SeReporta() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });
        string salida = """
            ID: 1
            Nombre: Ana María López
            Promedio: 9.5
            Activo: sí
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            "Estudiante 2",
            Assert.Single(resultado.BloquesRegistro).RegistrosFaltantes);
    }

    [Fact]
    public void Registros_RegistroAdicional_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });
        string salida = SalidaCorrecta() + """

            ID: 3
            Nombre: Laura Díaz
            Promedio: 0
            Activo: sí
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            "3",
            Assert.Single(resultado.BloquesRegistro).RegistrosAdicionales);
    }

    [Fact]
    public void Registros_IdDuplicado_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });
        string salida = """
            ID: 1
            Nombre: Ana María López
            Promedio: 9.5
            Activo: sí
            ID: 1
            Nombre: Ana María López
            Promedio: 9.5
            Activo: sí
            ID: 2
            Nombre: José Pérez
            Promedio: -1.25
            Activo: no
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.Contains(
            "1",
            Assert.Single(resultado.BloquesRegistro).RegistrosDuplicados);
    }

    [Fact]
    public void Registros_OrdenIncorrecto_SeRechaza() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });
        string salida = """
            ID: 2
            Nombre: José Pérez
            Promedio: -1.25
            Activo: no
            ID: 1
            Nombre: Ana María López
            Promedio: 9.5
            Activo: sí
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.BloquesRegistro).OrdenCorrecto);
    }

    [Fact]
    public void Registros_CampoRepetidoConValoresDiferentes_EsContradiccion() {
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { CrearRegistros() });
        string salida = """
            ID: 1
            Nombre: Ana María López
            Nombre: Laura Díaz
            Promedio: 9.5
            Activo: sí
            ID: 2
            Nombre: José Pérez
            Promedio: -1.25
            Activo: no
            """;

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        ResultadoBloquesRegistroComparados registros =
            Assert.Single(resultado.BloquesRegistro);
        Assert.True(registros.TieneContradiccion);
        Assert.Contains("Estudiantes", resultado.ContradiccionesDetectadas);
    }

    [Fact]
    public void Registros_TextoFueraDeCamposCuandoNoEstaPermitido_SeRechaza() {
        ReglaBloquesRegistroEsperados regla = CrearRegistros(
            permitirTextoNeutral: false);
        CasoPrueba caso = ComparadorTestFactory.CasoMixto(
            registros: new[] { regla });
        string salida = "Resumen no reconocido\n" + SalidaCorrecta();

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.True(Assert.Single(resultado.BloquesRegistro).TieneContradiccion);
    }

    private static ReglaBloquesRegistroEsperados CrearRegistros(
        bool permitirTextoNeutral = true) {
        return new ReglaBloquesRegistroEsperados {
            Nombre = "Estudiantes",
            NombreCampoClave = "ID",
            EtiquetasClave = new[] { "ID", "Identificador" },
            TipoClave = TipoValorEstructurado.Numerico,
            RegistrosEsperados = new[] {
                CrearRegistro(
                    "Estudiante 1",
                    1D,
                    "Ana María López",
                    9.5D,
                    activo: true),
                CrearRegistro(
                    "Estudiante 2",
                    2D,
                    "José Pérez",
                    -1.25D,
                    activo: false)
            },
            OrdenRegistrosObligatorio = true,
            PermitirRegistrosAdicionales = false,
            PermitirRegistrosDuplicados = false,
            PermitirTextoNeutralEntreBloques = permitirTextoNeutral
        };
    }

    private static RegistroEsperado CrearRegistro(
        string nombreRegistro,
        double id,
        string nombre,
        double promedio,
        bool activo) {
        return new RegistroEsperado {
            Nombre = nombreRegistro,
            Clave = ComparadorTestFactory.Numero(id),
            Campos = new[] {
                new CampoRegistroEsperado {
                    Nombre = "Nombre",
                    EtiquetasAlternativas = new[] {
                        "Nombre",
                        "Nombre completo"
                    },
                    Valor = ComparadorTestFactory.Texto(
                        nombre,
                        distinguirMayusculas: false,
                        distinguirAcentos: false,
                        PoliticaEspaciosCadena.ColapsarInternos)
                },
                new CampoRegistroEsperado {
                    Nombre = "Promedio",
                    EtiquetasAlternativas = new[] { "Promedio", "Calificación" },
                    Valor = ComparadorTestFactory.Numero(promedio, 0.001D)
                },
                new CampoRegistroEsperado {
                    Nombre = "Activo",
                    EtiquetasAlternativas = new[] { "Activo", "Es activo" },
                    Valor = ComparadorTestFactory.Booleano(activo)
                }
            }
        };
    }

    private static string SalidaCorrecta() {
        return """
            ID: 1
            Nombre: ANA MARIA LÓPEZ
            Promedio: 9,5
            Activo: verdadero
            ID: 2
            Nombre: José Pérez
            Promedio: -1,25
            Activo: 0
            """;
    }
}
