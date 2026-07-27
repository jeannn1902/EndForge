using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests.Comparadores;

public sealed class ComparadorValoresTests {
    [Fact]
    public void Comparar_CasoNulo_LanzaArgumentNullException() {
        ComparadorSalidaService comparador = new();

        Assert.Throws<ArgumentNullException>(() =>
            comparador.Comparar(null!, "salida"));
    }

    [Fact]
    public void Texto_AceptaMayusculasAcentosYEspaciosEquivalentes() {
        CasoPrueba caso = new() {
            ModoComparacion = ModoComparacionCaso.Texto,
            TokensObligatorios = new[] { "clasificación válida" }
        };

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "  CLASIFICACION   VÁLIDA  ");

        Assert.True(resultado.EsCorrecta);
        Assert.Empty(resultado.TokensFaltantes);
    }

    [Fact]
    public void Texto_SinTokenObligatorio_IndicaReglaIncumplida() {
        CasoPrueba caso = new() {
            ModoComparacion = ModoComparacionCaso.Texto,
            TokensObligatorios = new[] { "resultado" }
        };

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "sin datos");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains("resultado", resultado.TokensFaltantes);
        Assert.NotEmpty(resultado.ReglasIncumplidas);
    }

    [Theory]
    [InlineData("Temperatura: -2.50")]
    [InlineData("Temperatura: -2,50")]
    [InlineData("TEMPERATURA = -2,49")]
    public void ValorNumerico_AceptaNegativosComaDecimalYTolerancia(
        string salida) {
        CasoPrueba caso = CasoConNumero(-2.5D, 0.011D);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, salida);

        Assert.True(resultado.EsCorrecta);
        ResultadoValorNumericoComparado valor =
            Assert.Single(resultado.ValoresNumericos);
        Assert.True(valor.Coincide);
        Assert.InRange(valor.ValorObtenido!.Value, -2.511D, -2.489D);
    }

    [Fact]
    public void ValorNumerico_CeroEsComparadoSinTratarloComoAusente() {
        CasoPrueba caso = CasoConNumero(0D);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Temperatura: 0");

        Assert.True(resultado.EsCorrecta);
        Assert.Equal(0D, Assert.Single(resultado.ValoresNumericos).ValorObtenido);
    }

    [Fact]
    public void ValorNumerico_FueraDeTolerancia_SeRechaza() {
        CasoPrueba caso = CasoConNumero(10D, 0.01D);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Temperatura: 10.02");

        Assert.False(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.ValoresNumericos).Coincide);
    }

    [Fact]
    public void ValorNumerico_ConDosValoresDiferentes_SeMarcaContradiccion() {
        CasoPrueba caso = CasoConNumero(51D);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Temperatura: 999\nTemperatura: 51");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains("Temperatura", resultado.ContradiccionesDetectadas);
        Assert.True(Assert.Single(resultado.ValoresNumericos).TieneContradiccion);
    }

    [Fact]
    public void ValorNumerico_RepetidoDeFormaEquivalente_NoEsContradiccion() {
        CasoPrueba caso = CasoConNumero(2.5D);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Temperatura: 2.5\nTemperatura: 2,50");

        Assert.True(resultado.EsCorrecta);
        Assert.Empty(resultado.ContradiccionesDetectadas);
    }

    [Theory]
    [InlineData("sí")]
    [InlineData("SI")]
    [InlineData("true")]
    [InlineData("verdadero")]
    [InlineData("1")]
    public void BooleanoVerdadero_AceptaRepresentacionesConfiguradas(
        string representacion) {
        CasoPrueba caso = CasoConBooleano(true);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                $"Es estudiante: {representacion}");

        Assert.True(resultado.EsCorrecta);
        Assert.True(Assert.Single(resultado.ValoresBooleanos).ValorObtenido);
    }

    [Theory]
    [InlineData("no")]
    [InlineData("FALSE")]
    [InlineData("falso")]
    [InlineData("0")]
    public void BooleanoFalso_AceptaRepresentacionesConfiguradas(
        string representacion) {
        CasoPrueba caso = CasoConBooleano(false);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                $"Es estudiante: {representacion}");

        Assert.True(resultado.EsCorrecta);
        Assert.False(Assert.Single(resultado.ValoresBooleanos).ValorObtenido);
    }

    [Fact]
    public void Booleano_UsaEtiquetaAlternativaYLaReporta() {
        CasoPrueba caso = CasoConBooleano(true);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Alumno: sí");

        Assert.True(resultado.EsCorrecta);
        Assert.Contains(
            resultado.EtiquetasAlternativasReconocidas,
            etiqueta => etiqueta.Contains("Alumno", StringComparison.Ordinal));
    }

    [Fact]
    public void Booleano_RepresentacionDesconocida_SeRechaza() {
        CasoPrueba caso = CasoConBooleano(true);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "Es estudiante: quizá");

        Assert.False(resultado.EsCorrecta);
        Assert.Null(Assert.Single(resultado.ValoresBooleanos).ValorObtenido);
    }

    [Fact]
    public void Booleano_VerdaderoYFalsoParaMismoCampo_EsContradiccion() {
        CasoPrueba caso = CasoConBooleano(true);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(
                caso,
                "Es estudiante: sí\nAlumno: no");

        Assert.False(resultado.EsCorrecta);
        Assert.Contains("Estudiante", resultado.ContradiccionesDetectadas);
    }

    [Fact]
    public void ValorTextual_AceptaAlternativaSinEtiquetaYNormalizaAcentos() {
        CasoPrueba caso = new() {
            ModoComparacion = ModoComparacionCaso.Texto,
            ValoresTextualesEsperados = new[] {
                new ValorTextualEsperado {
                    Nombre = "Clasificación",
                    Valor = "Positivo",
                    PermitirSinEtiqueta = true,
                    Opciones = new[] {
                        new OpcionValorTextual {
                            Valor = "Positivo",
                            Alternativas = new[] { "Número positivo" }
                        }
                    }
                }
            }
        };

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, "NUMERO POSITIVO");

        Assert.True(resultado.EsCorrecta);
        Assert.True(Assert.Single(resultado.ValoresTextuales).Coincide);
    }

    [Fact]
    public void SalidaNula_SeTrataComoVaciaSinLanzar() {
        CasoPrueba caso = CasoConNumero(1D);

        ResultadoComparacionSalida resultado =
            ComparadorTestFactory.Comparar(caso, null);

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.EsSalidaLegible);
    }

    private static CasoPrueba CasoConNumero(
        double valor,
        double tolerancia = 0.01D) {
        return new CasoPrueba {
            ModoComparacion = ModoComparacionCaso.Valores,
            ValoresNumericosEsperados = new[] {
                new ValorNumericoEsperado {
                    Nombre = "Temperatura",
                    Valor = valor,
                    Tolerancia = tolerancia,
                    EtiquetasAlternativas = new[] {
                        "Temperatura",
                        "Valor térmico"
                    }
                }
            }
        };
    }

    private static CasoPrueba CasoConBooleano(bool valor) {
        return new CasoPrueba {
            ModoComparacion = ModoComparacionCaso.Valores,
            ValoresBooleanosEsperados = new[] {
                new ValorBooleanoEsperado {
                    Nombre = "Estudiante",
                    Valor = valor,
                    EtiquetasAlternativas = new[] {
                        "Es estudiante",
                        "Alumno"
                    }
                }
            }
        };
    }
}
