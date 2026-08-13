using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CalculadorLayoutFranjaMotivacionInicioTests {
    public static TheoryData<int, int> AnchosYDpi => new() {
        { 288, 96 },
        { 293, 96 },
        { 513, 96 },
        { 536, 96 },
        { 720, 96 },
        { 288, 120 },
        { 293, 120 },
        { 513, 120 },
        { 536, 120 },
        { 720, 120 },
        { 288, 144 },
        { 293, 144 },
        { 513, 144 },
        { 536, 144 },
        { 720, 144 }
    };

    [Theory]
    [MemberData(nameof(AnchosYDpi))]
    public void Calcular_MantieneTodoElContenidoDentroDelPanel(
        int anchoLogico,
        int dpi) {
        MedidasFranjaMotivacionInicio medidas = Calcular(anchoLogico, dpi);

        Assert.Equal(Escalar(anchoLogico, dpi), medidas.AnchoPanel);
        Assert.Equal(Escalar(248, dpi), medidas.AltoPanelRequerido);
        Assert.Equal(dpi, medidas.Dpi);

        foreach (RectanguloLayoutInicio rectangulo in ObtenerRectangulos(
                     medidas)) {
            Assert.True(rectangulo.X >= 0);
            Assert.True(rectangulo.Y >= 0);
            Assert.True(rectangulo.Ancho > 0);
            Assert.True(rectangulo.Alto > 0);
            Assert.True(rectangulo.Derecha <= medidas.AnchoPanel);
            Assert.True(rectangulo.Inferior <= medidas.AltoPanelRequerido);
        }
    }

    [Theory]
    [MemberData(nameof(AnchosYDpi))]
    public void Calcular_ConservaSeparacionesVerticalesMinimas(
        int anchoLogico,
        int dpi) {
        MedidasFranjaMotivacionInicio medidas = Calcular(anchoLogico, dpi);

        Assert.True(
            medidas.TituloRacha.Y - medidas.Separador.Inferior >=
                Escalar(5, dpi));
        Assert.True(
            medidas.ValorRacha.Y - medidas.TituloRacha.Inferior >=
                Escalar(2, dpi));
        Assert.True(
            medidas.DetalleRacha.Y - medidas.ValorRacha.Inferior >=
                Escalar(3, dpi));
        Assert.Equal(medidas.TituloRacha.Y, medidas.TituloLogros.Y);
        Assert.Equal(medidas.ValorRacha.Y, medidas.ValorLogros.Y);
        Assert.Equal(medidas.DetalleRacha.Y, medidas.BotonVerLogros.Y);
    }

    [Theory]
    [MemberData(nameof(AnchosYDpi))]
    public void Calcular_DejaElBotonCompletoYConMargenInferiorSeguro(
        int anchoLogico,
        int dpi) {
        MedidasFranjaMotivacionInicio medidas = Calcular(anchoLogico, dpi);

        Assert.True(
            medidas.BotonVerLogros.Y - medidas.ValorLogros.Inferior >=
                Escalar(3, dpi));
        Assert.True(
            medidas.AltoPanelRequerido - medidas.BotonVerLogros.Inferior >=
                Escalar(6, dpi));
        Assert.True(
            medidas.AltoPanelRequerido - medidas.DetalleRacha.Inferior >=
                Escalar(18, dpi));
    }

    [Theory]
    [MemberData(nameof(AnchosYDpi))]
    public void Calcular_ApilaLogrosUsandoElAnchoCompletoDeSuColumna(
        int anchoLogico,
        int dpi) {
        MedidasFranjaMotivacionInicio medidas = Calcular(anchoLogico, dpi);

        Assert.Equal(medidas.TituloLogros.X, medidas.ValorLogros.X);
        Assert.Equal(medidas.TituloLogros.X, medidas.BotonVerLogros.X);
        Assert.Equal(medidas.TituloLogros.Ancho, medidas.ValorLogros.Ancho);
        Assert.Equal(
            medidas.TituloLogros.Ancho,
            medidas.BotonVerLogros.Ancho);
        Assert.True(medidas.ValorLogros.Y >= medidas.TituloLogros.Inferior);
        Assert.Equal(
            medidas.Separador.Derecha,
            medidas.TituloLogros.Derecha);
    }

    [Theory]
    [MemberData(nameof(AnchosYDpi))]
    public void Calcular_ColumnasCubrenElInteriorSinSolaparse(
        int anchoLogico,
        int dpi) {
        MedidasFranjaMotivacionInicio medidas = Calcular(anchoLogico, dpi);
        int separacion = Escalar(8, dpi);

        Assert.Equal(
            medidas.TituloRacha.Derecha + separacion,
            medidas.TituloLogros.X);
        Assert.Equal(medidas.Separador.X, medidas.TituloRacha.X);
        Assert.Equal(medidas.Separador.Derecha,
            medidas.TituloLogros.Derecha);
        Assert.True(medidas.TituloRacha.Derecha <= medidas.TituloLogros.X);
    }

    [Theory]
    [MemberData(nameof(AnchosYDpi))]
    public void Calcular_MismasEntradasProduceGeometriaIdempotente(
        int anchoLogico,
        int dpi) {
        int anchoFisico = Escalar(anchoLogico, dpi);

        MedidasFranjaMotivacionInicio inicial =
            CalculadorLayoutFranjaMotivacionInicio.Calcular(anchoFisico, dpi);
        MedidasFranjaMotivacionInicio repetida =
            CalculadorLayoutFranjaMotivacionInicio.Calcular(anchoFisico, dpi);

        Assert.Equal(inicial, repetida);
    }

    [Theory]
    [InlineData(288)]
    [InlineData(293)]
    [InlineData(513)]
    [InlineData(536)]
    [InlineData(720)]
    public void Calcular_CicloDpiEsReversible(int anchoLogico) {
        MedidasFranjaMotivacionInicio inicial = Calcular(anchoLogico, 96);
        _ = Calcular(anchoLogico, 144);
        MedidasFranjaMotivacionInicio restaurada = Calcular(anchoLogico, 96);

        Assert.Equal(inicial, restaurada);
    }

    [Theory]
    [InlineData(0, 96)]
    [InlineData(-1, 96)]
    [InlineData(288, 0)]
    [InlineData(288, -1)]
    public void Calcular_RechazaEntradasInvalidas(int ancho, int dpi) {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CalculadorLayoutFranjaMotivacionInicio.Calcular(ancho, dpi));
    }

    private static MedidasFranjaMotivacionInicio Calcular(
        int anchoLogico,
        int dpi) {
        return CalculadorLayoutFranjaMotivacionInicio.Calcular(
            Escalar(anchoLogico, dpi),
            dpi);
    }

    private static int Escalar(int valor, int dpi) {
        return Math.Max(1, (int)Math.Round(valor * dpi / 96D));
    }

    private static IReadOnlyList<RectanguloLayoutInicio> ObtenerRectangulos(
        MedidasFranjaMotivacionInicio medidas) {
        return new[] {
            medidas.Separador,
            medidas.TituloRacha,
            medidas.ValorRacha,
            medidas.DetalleRacha,
            medidas.TituloLogros,
            medidas.ValorLogros,
            medidas.BotonVerLogros
        };
    }
}
