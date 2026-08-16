using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class RegresionTerminalidadAdivinaNumeroTests {
    private const string CasoPrimerIntentoId =
        "adivina-numero-primer-intento";
    private const string CasoEntradaPosteriorId =
        "adivina-numero-entrada-posterior-oculta";

    private static readonly ComparadorSalidaService Comparador = new();

    [Fact]
    public void SalidaCanonica_Aprueba() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            caso.SalidaEsperada);

        Assert.True(resultado.EsCorrecta, resultado.Mensaje);
    }

    [Fact]
    public void SalidaCanonicaConIntentoInvalidoPosterior_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = AgregarLineaPosterior(
            caso.SalidaEsperada,
            "Intento inválido");

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void SalidaCanonicaConOtroEventoPosterior_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = AgregarLineaPosterior(
            caso.SalidaEsperada,
            "El número secreto es mayor");

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void SalidaCanonicaConResultadoContradictorioPosterior_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = AgregarLineaPosterior(
            caso.SalidaEsperada,
            "Resultado: Incorrecto");

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("Resultado no correcto")]
    [InlineData("No es el número secreto")]
    [InlineData("Nunca es correcto")]
    [InlineData("No fue correcto")]
    public void NegacionDelAcierto_NoSeInterpretaComoCorrecto(string salida) {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            $"{salida}{Environment.NewLine}Intentos: 1");

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void SalidaCanonicaConPistaNegadaPosterior_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = AgregarLineaPosterior(
            caso.SalidaEsperada,
            "No es mayor");

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void SalidaCanonicaConIntentoPosteriorExplicito_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = AgregarLineaPosterior(
            caso.SalidaEsperada,
            "Intento: 9");

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void PromptInterrogativoAntesDelAcierto_NoSeInterpretaComoEvento() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida =
            $"¿El número secreto es mayor o menor?{Environment.NewLine}" +
            caso.SalidaEsperada;

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.True(resultado.EsCorrecta, resultado.Mensaje);
    }

    [Fact]
    public void ResultadoContradictorioPosteriorEnLaMismaLinea_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = caso.SalidaEsperada.Replace(
            "Correcto",
            "Correcto! Resultado: Incorrecto",
            StringComparison.Ordinal);

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void IntentoPosteriorEnLaMismaLinea_Falla() {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = caso.SalidaEsperada.Replace(
            "Correcto",
            "Correcto!; Intento: 9",
            StringComparison.Ordinal);

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("Correcto Resultado: Incorrecto")]
    [InlineData("Correcto, Resultado: Incorrecto")]
    public void ResultadoEtiquetadoPosteriorSinSaltoDeLinea_Falla(
        string eventoTerminalYResultado) {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = caso.SalidaEsperada.Replace(
            "Correcto",
            eventoTerminalYResultado,
            StringComparison.Ordinal);

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("Correcto Resultado:Incorrecto")]
    [InlineData("Correcto Resultado : Incorrecto")]
    [InlineData("Correcto Resultado=Incorrecto")]
    [InlineData("Correcto Resultado = Incorrecto")]
    public void ResultadoEtiquetadoConSeparadorFlexible_Falla(
        string eventoTerminalYResultado) {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = caso.SalidaEsperada.Replace(
            "Correcto",
            eventoTerminalYResultado,
            StringComparison.Ordinal);

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("Correcto Intento: 9")]
    [InlineData("Correcto, Intento: 9")]
    public void IntentoEtiquetadoPosteriorSinSaltoDeLinea_Falla(
        string eventoTerminalEIntento) {
        CasoPrueba caso = ObtenerCaso(CasoPrimerIntentoId);
        string salida = caso.SalidaEsperada.Replace(
            "Correcto",
            eventoTerminalEIntento,
            StringComparison.Ordinal);

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Theory]
    [InlineData("El número secreto es mayor", "No es mayor")]
    [InlineData("El número secreto es menor", "No es menor")]
    public void NegacionDeUnaPista_NoSeInterpretaComoLaPistaEsperada(
        string pistaCanonica,
        string pistaNegada) {
        CasoPrueba caso = ObtenerCaso("adivina-numero-pistas-opuestas");
        string salida = caso.SalidaEsperada.Replace(
            pistaCanonica,
            pistaNegada,
            StringComparison.Ordinal);

        ResultadoComparacionSalida resultado = Comparador.Comparar(caso, salida);

        Assert.False(resultado.EsCorrecta);
    }

    [Fact]
    public void EntradaPosteriorAlAciertoSinProcesar_TerminaCorrectamente() {
        CasoPrueba caso = ObtenerCaso(CasoEntradaPosteriorId);

        ResultadoComparacionSalida resultado = Comparador.Comparar(
            caso,
            caso.SalidaEsperada);

        Assert.False(caso.EsVisible);
        Assert.True(resultado.EsCorrecta, resultado.Mensaje);
    }

    private static CasoPrueba ObtenerCaso(string casoId) {
        DefinicionEvaluacionPractica definicion =
            CanonicalEvaluationFactory.ObtenerDefinicion(
                CatalogoEvaluacionesService.AdivinaNumeroId);

        return Assert.Single(
            definicion.CasosPrueba,
            caso => string.Equals(
                caso.Id,
                casoId,
                StringComparison.Ordinal));
    }

    private static string AgregarLineaPosterior(
        string salidaCanonica,
        string lineaPosterior) {
        return $"{salidaCanonica}{Environment.NewLine}{lineaPosterior}";
    }
}
