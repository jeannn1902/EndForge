using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class MayorDeEdadEvaluacionFlexibleTests {
    private const string PracticaId = "condicionales-mayor-de-edad";

    private readonly DefinicionEvaluacionPractica definicion =
        new CatalogoEvaluacionesService().ObtenerDefinicion(PracticaId)
        ?? throw new InvalidOperationException(
            $"No existe la evaluacion '{PracticaId}'.");
    private readonly ComparadorSalidaService comparador = new();
    private readonly EvaluacionPracticaService evaluador = new();

    [Theory]
    [InlineData(
        "mayor-edad-limite-adulto",
        "Edad: 18\nClasificacion: Mayor de edad")]
    [InlineData(
        "mayor-edad-menor",
        "Edad: 17\nClasificacion: Menor de edad")]
    [InlineData(
        "mayor-edad-negativa",
        "Edad: -1\nClasificacion: Edad invalida")]
    [InlineData(
        "mayor-edad-superior-oculto",
        "Edad: 121\nClasificacion: Edad invalida")]
    public void FormatoCanonico_ApruebaLosCuatroCasos(
        string casoId,
        string salida) {
        AssertSalidaCorrecta(casoId, salida);
    }

    [Theory]
    [InlineData("mayor-edad-limite-adulto", "Mayor de edad.")]
    [InlineData("mayor-edad-menor", "Menor de edad.")]
    [InlineData("mayor-edad-negativa", "Edad invalida.")]
    [InlineData("mayor-edad-superior-oculto", "Fuera de rango.")]
    public void CategoriaSemanticaSinEtiqueta_Aprueba(
        string casoId,
        string salida) {
        AssertSalidaCorrecta(casoId, salida);
    }

    [Theory]
    [InlineData(
        "mayor-edad-limite-adulto",
        "La edad es 18.\nLa persona es mayor de edad.")]
    [InlineData(
        "mayor-edad-menor",
        "La edad es 17.\nLa persona es menor de edad.")]
    [InlineData(
        "mayor-edad-negativa",
        "La edad es -1.\nLa edad es invalida.")]
    [InlineData(
        "mayor-edad-superior-oculto",
        "La edad es 121.\nEl valor esta fuera de rango.")]
    public void ConectoresNaturales_Aprueban(
        string casoId,
        string salida) {
        AssertSalidaCorrecta(casoId, salida);
    }

    [Fact]
    public void CajaAcentosEspaciosYPuntuacion_NoCambianLaCategoria() {
        AssertSalidaCorrecta(
            "mayor-edad-limite-adulto",
            "  eDaD   :   18  \n  cLaSiFiCaCiÓn : ADULTO!  ");
    }

    [Fact]
    public void PromptInocuoYRespuestaSeparada_Aprueban() {
        AssertSalidaCorrecta(
            "mayor-edad-limite-adulto",
            "Ingrese su edad:\nMayor de edad.");
    }

    [Fact]
    public void PreguntaConCategoriaPeroSinRespuesta_NoAprueba() {
        AssertSalidaIncorrecta(
            "mayor-edad-limite-adulto",
            "¿Es mayor de edad?");
    }

    [Fact]
    public void PreguntaDeAdultoYRespuestaNegada_ClasificanComoMenor() {
        AssertSalidaCorrecta(
            "mayor-edad-menor",
            "¿Es mayor de edad?\nNo es mayor de edad.");
    }

    [Fact]
    public void NegacionDeAdulto_ApruebaMenorYRechazaMayor() {
        const string salida = "No es mayor de edad.";

        AssertSalidaCorrecta("mayor-edad-menor", salida);
        AssertSalidaIncorrecta("mayor-edad-limite-adulto", salida);
    }

    [Fact]
    public void PalabraMayorAislada_NoEsUnaClasificacionSuficiente() {
        AssertSalidaIncorrecta("mayor-edad-limite-adulto", "Mayor");
    }

    [Fact]
    public void CategoriasIncompatibles_SeRechazan() {
        ResultadoComparacionSalida resultado = Comparar(
            "mayor-edad-limite-adulto",
            "Edad: 18\nMayor de edad.\nMenor de edad.");

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
    }

    [Fact]
    public void CategoriaCorrectaEIncorrectaEtiquetadas_SeMarcanComoContradiccion() {
        ResultadoComparacionSalida resultado = Comparar(
            "mayor-edad-limite-adulto",
            "Edad: 18\nClasificacion: Mayor de edad\nResultado: Menor de edad");

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
        Assert.Contains(
            "Clasificación",
            resultado.ContradiccionesDetectadas,
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EdadEsOpcionalCuandoLaCategoriaEsClara() {
        AssertSalidaCorrecta(
            "mayor-edad-limite-adulto",
            "La persona es mayor de edad.");
    }

    [Fact]
    public void EdadCorrectaSiSeMuestra_Aprueba() {
        AssertSalidaCorrecta(
            "mayor-edad-limite-adulto",
            "Edad: 18\nMayor de edad.");
    }

    [Theory]
    [InlineData("Edad: 17\nMayor de edad.")]
    [InlineData("La edad es 17.\nLa persona es mayor de edad.")]
    [InlineData("17.\nMayor de edad.")]
    public void EdadIncorrectaSiSeMuestra_SeRechaza(string salida) {
        AssertSalidaIncorrecta("mayor-edad-limite-adulto", salida);
    }

    [Fact]
    public void CategoriaIncorrecta_SeRechaza() {
        AssertSalidaIncorrecta(
            "mayor-edad-limite-adulto",
            "Edad: 18\nMenor de edad.");
    }

    [Fact]
    public void SalidaVacia_SeRechazaComoNoEstructurada() {
        ResultadoComparacionSalida resultado = Comparar(
            "mayor-edad-limite-adulto",
            string.Empty);

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
        Assert.False(resultado.EsSalidaLegible);
    }

    [Fact]
    public void AsociacionTextualSinPoliticaExplicita_ConservaEtiquetaObligatoria() {
        CasoPrueba caso = new() {
            ModoComparacion = ModoComparacionCaso.Texto,
            ValoresTextualesEsperados = Array.AsReadOnly(new[] {
                new ValorTextualEsperado {
                    Nombre = "Clasificación",
                    Valor = "Mayor de edad",
                    Opciones = Array.AsReadOnly(new[] {
                        new OpcionValorTextual {
                            Valor = "Mayor de edad",
                            Alternativas = Array.AsReadOnly(new[] {
                                "mayor de edad"
                            })
                        }
                    })
                }
            })
        };

        ResultadoComparacionSalida resultado = comparador.Comparar(
            caso,
            "Mayor de edad.");

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
    }

    [Fact]
    public void AsociacionNumericaSinPoliticaExplicita_ConservaEtiquetaObligatoria() {
        CasoPrueba caso = new() {
            ModoComparacion = ModoComparacionCaso.Valores,
            ValoresNumericosEsperados = Array.AsReadOnly(new[] {
                new ValorNumericoEsperado {
                    Nombre = "Edad",
                    Valor = 18
                }
            })
        };

        ResultadoComparacionSalida resultado = comparador.Comparar(caso, "18");

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
    }

    [Fact]
    public void CategoriaEtiquetadaYOtraSinEtiqueta_SeMarcanComoContradiccion() {
        ResultadoComparacionSalida resultado = Comparar(
            "mayor-edad-limite-adulto",
            "Clasificacion: Mayor de edad\nMenor de edad.");

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
        Assert.Contains(
            "Clasificación",
            resultado.ContradiccionesDetectadas,
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void CategoriaEtiquetadaConOtraTrasPuntuacion_SeRechaza() {
        AssertSalidaIncorrecta(
            "mayor-edad-limite-adulto",
            "Clasificacion: Adulto! Menor de edad.");
    }

    [Theory]
    [InlineData("No creo que la persona es mayor de edad.")]
    [InlineData("La persona no siempre es mayor de edad.")]
    public void ConectorConNegacionPrevia_NoAfirmaLaCategoriaMayor(string salida) {
        AssertSalidaIncorrecta("mayor-edad-limite-adulto", salida);
    }

    [Fact]
    public void EdadEtiquetadaYNaturalContradictorias_SeRechazan() {
        AssertSalidaIncorrecta(
            "mayor-edad-limite-adulto",
            "Edad: 18\nLa edad es 17.\nMayor de edad.");
    }

    [Fact]
    public void EdadEtiquetadaYLineaNumericaContradictorias_SeRechazan() {
        AssertSalidaIncorrecta(
            "mayor-edad-limite-adulto",
            "Edad: 18\n17\nMayor de edad.");
    }

    [Fact]
    public void NumeroInocuoNoSeInterpretaComoLaEdad() {
        AssertSalidaCorrecta(
            "mayor-edad-limite-adulto",
            "Ejercicio 2026\nLa persona es mayor de edad.");
    }

    [Theory]
    [InlineData("mayor-edad-limite-adulto", "La persona es mayor de edad.")]
    [InlineData("mayor-edad-menor", "No es mayor de edad.")]
    [InlineData("mayor-edad-negativa", "Edad invalida.")]
    [InlineData("mayor-edad-superior-oculto", "Fuera de rango.")]
    public void EvaluadorReal_OtorgaLosPuntosDelCasoALaCategoriaCorrecta(
        string casoId,
        string salida) {
        CasoPrueba caso = ObtenerCaso(casoId);
        EvaluacionPracticaService.CasoEvaluado evaluado = evaluador.EvaluarCaso(
            caso,
            new ResultadoEjecucionCasoPruebaCpp {
                Ejecucion = new ResultadoEjecucionPruebaCpp {
                    Estado = EstadoEjecucionPruebaCpp.Exitosa,
                    SalidaEstandar = salida,
                    CodigoSalida = 0
                }
            });

        Assert.True(evaluado.Resultado.Aprobado);
        Assert.Equal(caso.Puntos, evaluado.Resultado.PuntosObtenidos);
        Assert.NotNull(evaluado.Comparacion);
        Assert.True(evaluado.Comparacion.CumpleEstructura);
    }

    private void AssertSalidaCorrecta(string casoId, string salida) {
        ResultadoComparacionSalida resultado = Comparar(casoId, salida);

        Assert.True(resultado.EsCorrecta);
        Assert.True(resultado.CumpleEstructura);
        Assert.True(resultado.EsSalidaLegible);
    }

    private void AssertSalidaIncorrecta(string casoId, string salida) {
        ResultadoComparacionSalida resultado = Comparar(casoId, salida);

        Assert.False(resultado.EsCorrecta);
        Assert.False(resultado.CumpleEstructura);
    }

    private ResultadoComparacionSalida Comparar(string casoId, string salida) {
        return comparador.Comparar(ObtenerCaso(casoId), salida);
    }

    private CasoPrueba ObtenerCaso(string casoId) {
        return Assert.Single(definicion.CasosPrueba, caso =>
            caso.Id.Equals(casoId, StringComparison.Ordinal));
    }
}
