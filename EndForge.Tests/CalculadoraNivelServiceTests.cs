using EndForge.Services;

namespace EndForge.Tests;

public sealed class CalculadoraNivelServiceTests {
    private readonly CalculadoraNivelService servicio = new();

    [Theory]
    [InlineData(0, 1, 0, 150)]
    [InlineData(149, 1, 0, 1)]
    [InlineData(150, 2, 150, 300)]
    [InlineData(151, 2, 150, 299)]
    [InlineData(449, 2, 150, 1)]
    [InlineData(450, 3, 450, 450)]
    [InlineData(900, 4, 900, 600)]
    [InlineData(8250, 11, 8250, 1650)]
    public void Calcular_RespetaUmbralesAcumulados(
        long xp,
        long nivel,
        long minimo,
        long restante) {
        var resultado = servicio.Calcular(xp);

        Assert.Equal(nivel, resultado.NivelActual);
        Assert.Equal(minimo, resultado.XpMinimoNivelActual);
        Assert.Equal(restante, resultado.XpRestante);
    }

    [Fact]
    public void Calcular_MitadDelPrimerNivel_DevuelveCincuentaPorCiento() {
        var resultado = servicio.Calcular(75);

        Assert.Equal(75, resultado.XpAcumuladoDentroNivel);
        Assert.Equal(50m, resultado.PorcentajeNivel);
    }

    [Fact]
    public void Calcular_ValorGrande_NoDesbordaNiImponeNivelMaximo() {
        var resultado = servicio.Calcular(long.MaxValue);

        Assert.True(resultado.NivelActual > 100_000_000);
        Assert.True(resultado.XpMinimoNivelActual <= long.MaxValue);
        Assert.True(resultado.XpRequeridoSiguienteNivel > long.MaxValue);
        Assert.InRange(resultado.PorcentajeNivel, 0m, 100m);
    }

    [Fact]
    public void Calcular_XpNegativo_SeRechaza() {
        Assert.Throws<ArgumentOutOfRangeException>(() => servicio.Calcular(-1));
    }

    [Fact]
    public void CalcularXpRequerido_NivelInvalido_SeRechaza() {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => servicio.CalcularXpRequerido(0));
    }
}
