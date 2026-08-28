using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CondicionalesEvaluacionTests {
    private readonly CatalogoEvaluacionesService catalogo = new();
    private readonly ComparadorSalidaService comparador = new();

    [Theory]
    [InlineData(
        "condicionales-calificacion-aprobatoria",
        "calificacion-superior-oculta",
        "Calificación inválida")]
    [InlineData(
        "condicionales-descuento-compra",
        "descuento-total-negativo-oculto",
        "Total inválido")]
    [InlineData(
        "condicionales-menu-operaciones",
        "menu-operaciones-opcion-invalida-oculta",
        "Opción inválida")]
    public void EntradaInvalida_NoExigeRepetirElValorRechazado(
        string practicaId,
        string casoId,
        string salida) {
        DefinicionEvaluacionPractica definicion = catalogo.ObtenerDefinicion(practicaId)!;
        CasoPrueba caso = definicion.CasosPrueba.Single(item => item.Id == casoId);

        ResultadoComparacionSalida resultado = comparador.Comparar(caso, salida);

        Assert.True(
            resultado.EsCorrecta,
            $"{resultado.Mensaje} | Incumplidas: {string.Join(", ", resultado.ReglasIncumplidas)}");
        Assert.True(
            resultado.CumpleEstructura,
            $"Incumplidas: {string.Join(", ", resultado.ReglasIncumplidas)}");
    }
}
