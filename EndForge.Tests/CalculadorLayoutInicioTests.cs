using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CalculadorLayoutInicioTests {
    private const int AnchoAmplio = 1200;
    private const int AnchoCompacto = 720;

    [Theory]
    [InlineData(819, false)]
    [InlineData(820, true)]
    [InlineData(840, true)]
    public void AnchoRestaurado_EligeModoSegunLegibilidadDeLasColumnas(
        int ancho,
        bool esperado) {
        Assert.Equal(
            esperado,
            CalculadorLayoutInicio.DeterminarModoAmplio(ancho));
    }

    [Fact]
    public void VistaAmplia_UsaContinuacionComoColumnaPrincipal() {
        var layout = CalcularAmplio(1);

        Assert.True(layout.ModoAmplio);
        Assert.Equal(0, layout.Continuacion.X);
        Assert.Equal(0, layout.Continuacion.Y);
        Assert.True(layout.Continuacion.Ancho > layout.Progreso.Ancho);
        Assert.Equal(220, layout.Continuacion.Alto);
    }

    [Fact]
    public void VistaAmplia_ApilaProgresoYActividadEnColumnaSecundaria() {
        var layout = CalcularAmplio(1);

        Assert.Equal(layout.Progreso.X, layout.Actividad.X);
        Assert.Equal(layout.Progreso.Ancho, layout.Actividad.Ancho);
        Assert.Equal(
            layout.Progreso.Inferior + layout.Separacion,
            layout.Actividad.Y);
        Assert.Equal(layout.Actividad.Inferior, layout.AltoFilaPrincipal);
    }

    [Fact]
    public void VistaAmplia_NoEstiraContinuacionParaIgualarColumnaSecundaria() {
        var layout = CalcularAmplio(3);

        Assert.Equal(220, layout.Continuacion.Alto);
        Assert.True(layout.AltoFilaPrincipal > layout.Continuacion.Alto);
    }

    [Theory]
    [InlineData(0, 96)]
    [InlineData(1, 100)]
    [InlineData(2, 154)]
    [InlineData(3, 208)]
    public void Actividad_UsaAlturaSegunElementosReales(
        int cantidad,
        int altoEsperado) {
        var layout = CalcularAmplio(cantidad);

        Assert.Equal(altoEsperado, layout.AltoActividad);
        Assert.Equal(altoEsperado, layout.Actividad.Alto);
    }

    [Fact]
    public void Actividad_LimitaElLayoutALasTresFilasDisponibles() {
        var tres = CalcularAmplio(3);
        var exceso = CalcularAmplio(20);

        Assert.Equal(tres.AltoActividad, exceso.AltoActividad);
        Assert.Equal(tres.Actividad, exceso.Actividad);
        Assert.Equal(tres.AltoFilaPrincipal, exceso.AltoFilaPrincipal);
    }

    [Fact]
    public void Metricas_ConservanAlturaUniformeEnAmbosModos() {
        var amplio = CalcularAmplio(1);
        var compacto = CalcularCompacto(1);

        Assert.Equal(4, amplio.ColumnasMetricas);
        Assert.Equal(1, amplio.FilasMetricas);
        Assert.Equal(2, compacto.ColumnasMetricas);
        Assert.Equal(2, compacto.FilasMetricas);
        Assert.Equal(104, amplio.AltoTarjetaMetrica);
        Assert.Equal(amplio.AltoTarjetaMetrica, compacto.AltoTarjetaMetrica);
    }

    [Fact]
    public void NivelCompacto_NoAlteraComposicionExteriorDelDashboard() {
        var amplio = CalcularAmplio(1);
        var compacto = CalcularCompacto(1);

        Assert.Equal(
            CalculadorLayoutFranjaMotivacionInicio.AltoPanelLogico,
            amplio.AltoProgreso);
        Assert.Equal(
            CalculadorLayoutFranjaMotivacionInicio.AltoPanelLogico,
            compacto.AltoProgreso);
        Assert.Equal(4, amplio.ColumnasMetricas);
        Assert.Equal(2, compacto.ColumnasMetricas);
    }

    [Fact]
    public void ProgresoAmpliado_ConservaAnchosYAlturasDeLasDemasTarjetas() {
        var layout = CalcularAmplio(1);

        Assert.Equal(new Models.RectanguloLayoutInicio(0, 0, 760, 220),
            layout.Continuacion);
        Assert.Equal(new Models.RectanguloLayoutInicio(772, 0, 428, 248),
            layout.Progreso);
        Assert.Equal(new Models.RectanguloLayoutInicio(772, 260, 428, 100),
            layout.Actividad);
        Assert.Equal(104, layout.AltoTarjetaMetrica);
        Assert.Equal(184, layout.AltoRecomendacion);
        Assert.Equal(1200, layout.Recomendacion.Ancho);
    }

    [Fact]
    public void Recomendacion_OcupaTodoElAnchoInferior() {
        var amplio = CalcularAmplio(1);
        var compacto = CalcularCompacto(1);

        Assert.Equal(0, amplio.Recomendacion.X);
        Assert.Equal(AnchoAmplio, amplio.Recomendacion.Ancho);
        Assert.Equal(amplio.AnchoContenido, amplio.Recomendacion.Derecha);
        Assert.Equal(AnchoCompacto, compacto.Recomendacion.Ancho);
        Assert.Equal(compacto.AltoRecomendacion, compacto.AltoFilaInferior);
    }

    [Fact]
    public void Tarjetas_NoSeEstiranConElAltoDelViewport() {
        var reducido = CalculadorLayoutInicio.Calcular(
            true,
            AnchoAmplio,
            altoViewportLogico: 520,
            cantidadActividades: 1);
        var alto = CalculadorLayoutInicio.Calcular(
            true,
            AnchoAmplio,
            altoViewportLogico: 1400,
            cantidadActividades: 1);

        Assert.Equal(reducido, alto);
    }

    [Fact]
    public void Compacto_ApilaContinuacionProgresoYActividadEnEseOrden() {
        var layout = CalcularCompacto(1);

        Assert.False(layout.ModoAmplio);
        Assert.Equal(0, layout.Continuacion.X);
        Assert.Equal(layout.Continuacion.Inferior + layout.Separacion,
            layout.Progreso.Y);
        Assert.Equal(layout.Progreso.Inferior + layout.Separacion,
            layout.Actividad.Y);
        Assert.Equal(layout.Actividad.Inferior, layout.AltoFilaPrincipal);
        Assert.All(
            new[] { layout.Continuacion, layout.Progreso, layout.Actividad },
            rectangulo => Assert.Equal(AnchoCompacto, rectangulo.Ancho));
    }

    [Fact]
    public void Compacto_UbicaMetricasAntesDeRecomendacionSinCombinarTarjetas() {
        var layout = CalcularCompacto(1);

        Assert.Equal(
            layout.AltoTarjetaMetrica * 2 + layout.Separacion,
            layout.AltoMetricas);
        Assert.Equal(layout.AltoRecomendacion, layout.AltoFilaInferior);
        Assert.Equal(0, layout.Recomendacion.Y);
    }

    [Fact]
    public void CambioAmplioCompactoAmplio_EsReversible() {
        var amplioInicial = CalcularAmplio(3);
        var compacto = CalcularCompacto(3);
        var amplioFinal = CalcularAmplio(3);

        Assert.NotEqual(amplioInicial.Continuacion, compacto.Continuacion);
        Assert.NotEqual(amplioInicial.Actividad, compacto.Actividad);
        Assert.Equal(amplioInicial, amplioFinal);
    }

    [Fact]
    public void RecalculoConMismasEntradas_NoAcumulaOffsets() {
        var inicial = CalcularAmplio(2);
        var segundo = CalcularAmplio(2);
        var tercero = CalcularAmplio(2);

        Assert.Equal(inicial, segundo);
        Assert.Equal(segundo, tercero);
    }

    [Theory]
    [InlineData(720, false)]
    [InlineData(819, false)]
    [InlineData(820, true)]
    [InlineData(1200, true)]
    [InlineData(1500, true)]
    public void ProgresoYActividad_UsanElBordeInferiorRealEnTodosLosModos(
        int ancho,
        bool modoAmplio) {
        MedidasLayoutInicio layout = CalculadorLayoutInicio.Calcular(
            modoAmplio,
            ancho,
            altoViewportLogico: 900,
            cantidadActividades: 3);

        Assert.Equal(
            layout.Progreso.Inferior + layout.Separacion,
            layout.Actividad.Y);
        Assert.True(layout.Progreso.Inferior < layout.Actividad.Y);
        Assert.True(layout.Actividad.Inferior <= layout.AltoFilaPrincipal);
    }

    [Theory]
    [InlineData(720, false, 96)]
    [InlineData(819, false, 120)]
    [InlineData(820, true, 144)]
    [InlineData(1200, true, 120)]
    [InlineData(1500, true, 144)]
    public void ProgresoYActividad_ConservanSeparacionTrasEscalarDpi(
        int ancho,
        bool modoAmplio,
        int dpi) {
        MedidasLayoutInicio layout = CalculadorLayoutInicio.Calcular(
            modoAmplio,
            ancho,
            altoViewportLogico: 900,
            cantidadActividades: 2);
        int anchoReal = Escalar(ancho, dpi);
        RectanguloLayoutInicio progreso =
            CalculadorLayoutInicio.EscalarRectanguloFisico(
                layout.Progreso,
                anchoReal,
                ancho,
                dpi);
        RectanguloLayoutInicio actividad =
            CalculadorLayoutInicio.EscalarRectanguloFisico(
                layout.Actividad,
                anchoReal,
                ancho,
                dpi);

        Assert.True(
            progreso.Inferior + Escalar(layout.Separacion, dpi) <=
                actividad.Y);
    }

    [Fact]
    public void InicioLogrosInicio_RepetidoConservaLaMismaGeometria() {
        MedidasLayoutInicio geometriaInicial = CalcularAmplio(3);

        for (int ciclo = 0; ciclo < 10; ciclo++) {
            MedidasLayoutInicio geometriaLogros =
                CalculadorLayoutInicio.Calcular(
                    modoAmplio: false,
                    anchoContenidoLogico: AnchoCompacto,
                    altoViewportLogico: 720,
                    cantidadActividades: 1);
            MedidasLayoutInicio geometriaRestaurada = CalcularAmplio(3);

            Assert.NotEqual(geometriaInicial, geometriaLogros);
            Assert.Equal(geometriaInicial, geometriaRestaurada);
            Assert.Equal(
                geometriaRestaurada.Progreso.Inferior +
                    geometriaRestaurada.Separacion,
                geometriaRestaurada.Actividad.Y);
        }
    }

    [Fact]
    public void CambioSoloDeViewport_NoAlteraBoundsLogicos() {
        var reducido = CalculadorLayoutInicio.Calcular(
            true,
            AnchoAmplio,
            altoViewportLogico: 480,
            cantidadActividades: 2);
        var extendido = CalculadorLayoutInicio.Calcular(
            true,
            AnchoAmplio,
            altoViewportLogico: 1200,
            cantidadActividades: 2);

        Assert.Equal(reducido.Continuacion, extendido.Continuacion);
        Assert.Equal(reducido.Progreso, extendido.Progreso);
        Assert.Equal(reducido.Actividad, extendido.Actividad);
        Assert.Equal(reducido.Recomendacion, extendido.Recomendacion);
    }

    private static Models.MedidasLayoutInicio CalcularAmplio(
        int cantidadActividades) {
        return CalculadorLayoutInicio.Calcular(
            modoAmplio: true,
            anchoContenidoLogico: AnchoAmplio,
            altoViewportLogico: 900,
            cantidadActividades);
    }

    private static Models.MedidasLayoutInicio CalcularCompacto(
        int cantidadActividades) {
        return CalculadorLayoutInicio.Calcular(
            modoAmplio: false,
            anchoContenidoLogico: AnchoCompacto,
            altoViewportLogico: 720,
            cantidadActividades);
    }

    private static int Escalar(int valor, int dpi) {
        return Math.Max(1, (int)Math.Round(valor * dpi / 96D));
    }
}
