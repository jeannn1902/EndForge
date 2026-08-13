using EndForge.Services;

namespace EndForge.Tests;

public sealed class CalculadorLayoutLogrosTests {
    [Theory]
    [InlineData(819, false)]
    [InlineData(820, true)]
    [InlineData(1320, true)]
    public void AnchoLogico_DeterminaModoEnElLimiteEsperado(
        int ancho,
        bool esperado) {
        Assert.Equal(
            esperado,
            CalculadorLayoutLogros.DeterminarModoAmplio(ancho));
    }

    [Fact]
    public void Amplio_DistribuyeCatorceLogrosEnDosColumnas() {
        MedidasLayoutLogros layout = CalculadorLayoutLogros.Calcular(
            anchoViewportLogico: 1200,
            cantidadLogros: 14);

        Assert.True(layout.ModoAmplio);
        Assert.Equal(24, layout.Margen);
        Assert.Equal(1152, layout.AnchoContenido);
        Assert.Equal(24, layout.XContenido);
        Assert.Equal(2, layout.Columnas);
        Assert.Equal(7, layout.Filas);
        Assert.Equal(112, layout.AltoTarjeta);
        Assert.Equal(856, layout.AltoContenido);
        Assert.Equal(904, layout.AltoTotal);
        Assert.Equal(
            new RectanguloLayoutLogro(24, 24, 570, 112),
            layout.ObtenerRectanguloTarjeta(0));
        Assert.Equal(
            new RectanguloLayoutLogro(606, 24, 570, 112),
            layout.ObtenerRectanguloTarjeta(1));
        Assert.Equal(
            new RectanguloLayoutLogro(606, 768, 570, 112),
            layout.ObtenerRectanguloTarjeta(13));
    }

    [Fact]
    public void Compacto_DistribuyeCatorceLogrosEnUnaColumna() {
        MedidasLayoutLogros layout = CalculadorLayoutLogros.Calcular(
            anchoViewportLogico: 720,
            cantidadLogros: 14);

        Assert.False(layout.ModoAmplio);
        Assert.Equal(12, layout.Margen);
        Assert.Equal(696, layout.AnchoContenido);
        Assert.Equal(12, layout.XContenido);
        Assert.Equal(1, layout.Columnas);
        Assert.Equal(14, layout.Filas);
        Assert.Equal(124, layout.AltoTarjeta);
        Assert.Equal(1892, layout.AltoContenido);
        Assert.Equal(1916, layout.AltoTotal);
        Assert.Equal(
            new RectanguloLayoutLogro(12, 12, 696, 124),
            layout.ObtenerRectanguloTarjeta(0));
        Assert.Equal(
            new RectanguloLayoutLogro(12, 1780, 696, 124),
            layout.ObtenerRectanguloTarjeta(13));
    }

    [Fact]
    public void ViewportAmplio_LimitaYCentraContenidoA1320() {
        MedidasLayoutLogros layout = CalculadorLayoutLogros.Calcular(
            anchoViewportLogico: 1600,
            cantidadLogros: 2);

        Assert.Equal(1320, layout.AnchoContenido);
        Assert.Equal(140, layout.XContenido);
        Assert.Equal(140, layout.AnchoViewport -
            layout.ObtenerRectanguloTarjeta(1).Derecha);
    }

    [Fact]
    public void AnchoImpar_EntregaElRemanenteALaUltimaColumnaSinDesbordar() {
        MedidasLayoutLogros layout = CalculadorLayoutLogros.Calcular(
            anchoViewportLogico: 1001,
            cantidadLogros: 2);
        RectanguloLayoutLogro izquierda =
            layout.ObtenerRectanguloTarjeta(0);
        RectanguloLayoutLogro derecha =
            layout.ObtenerRectanguloTarjeta(1);

        Assert.Equal(470, izquierda.Ancho);
        Assert.Equal(471, derecha.Ancho);
        Assert.Equal(layout.Separacion, derecha.X - izquierda.Derecha);
        Assert.Equal(layout.XContenido + layout.AnchoContenido, derecha.Derecha);
    }

    [Fact]
    public void CantidadImpar_NoInventaUnaTarjetaParaCompletarLaFila() {
        MedidasLayoutLogros layout = CalculadorLayoutLogros.Calcular(
            anchoViewportLogico: 1200,
            cantidadLogros: 13);

        Assert.Equal(7, layout.Filas);
        Assert.Equal(13, layout.CantidadLogros);
        Assert.Equal(24, layout.ObtenerRectanguloTarjeta(12).X);
        Assert.Throws<ArgumentOutOfRangeException>(
            () => layout.ObtenerRectanguloTarjeta(13));
    }

    [Fact]
    public void RecalculoConMismasEntradas_EsIdempotente() {
        MedidasLayoutLogros primero =
            CalculadorLayoutLogros.Calcular(1200, 14);
        MedidasLayoutLogros segundo =
            CalculadorLayoutLogros.Calcular(1200, 14);

        Assert.Equal(primero, segundo);

        for (int indice = 0; indice < primero.CantidadLogros; indice++) {
            Assert.Equal(
                primero.ObtenerRectanguloTarjeta(indice),
                segundo.ObtenerRectanguloTarjeta(indice));
        }
    }

    [Theory]
    [InlineData(96)]
    [InlineData(120)]
    [InlineData(144)]
    public void DpiFisicoConvertidoAUnidadesLogicas_ConservaLaGeometria(
        int dpi) {
        const int anchoLogicoEsperado = 1200;
        int anchoFisico = (int)Math.Round(
            anchoLogicoEsperado * dpi / 96D);
        int anchoLogico = (int)Math.Round(anchoFisico * 96D / dpi);
        MedidasLayoutLogros referencia =
            CalculadorLayoutLogros.Calcular(anchoLogicoEsperado, 14);
        MedidasLayoutLogros actual =
            CalculadorLayoutLogros.Calcular(anchoLogico, 14);

        Assert.Equal(anchoLogicoEsperado, anchoLogico);
        Assert.Equal(referencia, actual);
        Assert.Equal(
            referencia.ObtenerRectanguloTarjeta(13),
            actual.ObtenerRectanguloTarjeta(13));
    }

    [Fact]
    public void SinLogros_ConservaMargenesSinCrearFilas() {
        MedidasLayoutLogros layout =
            CalculadorLayoutLogros.Calcular(1200, 0);

        Assert.Equal(0, layout.CantidadLogros);
        Assert.Equal(0, layout.Filas);
        Assert.Equal(0, layout.AltoContenido);
        Assert.Equal(48, layout.AltoTotal);
        Assert.Throws<ArgumentOutOfRangeException>(
            () => layout.ObtenerRectanguloTarjeta(0));
    }
}
