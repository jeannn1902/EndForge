using System.Drawing;
using System.Windows.Forms;

namespace EndForge.Tests;

public sealed class RecalculoResizeTests {
    [Theory]
    [InlineData(FormWindowState.Normal, FormWindowState.Normal, false)]
    [InlineData(FormWindowState.Maximized, FormWindowState.Maximized, false)]
    [InlineData(FormWindowState.Normal, FormWindowState.Maximized, true)]
    [InlineData(FormWindowState.Maximized, FormWindowState.Normal, true)]
    [InlineData(FormWindowState.Minimized, FormWindowState.Normal, true)]
    [InlineData(FormWindowState.Minimized, FormWindowState.Maximized, true)]
    [InlineData(FormWindowState.Normal, FormWindowState.Minimized, false)]
    public void CambioEstadoVentana_DecideRecalculoInmediato(
        FormWindowState anterior,
        FormWindowState actual,
        bool esperado) {
        Assert.Equal(
            esperado,
            frmPrincipal.DebeRecalcularInmediatamentePorCambioEstadoVentana(
                anterior,
                actual));
    }

    [Theory]
    [InlineData(1280, 720, 96, 1280, 720, 96, true)]
    [InlineData(1280, 720, 120, 1280, 720, 96, false)]
    [InlineData(1280, 720, 144, 1280, 720, 120, false)]
    [InlineData(1920, 1080, 96, 1280, 720, 96, false)]
    public void FirmaGeometria_IncluyeTamanoYDpi(
        int anchoActual,
        int altoActual,
        int dpiActual,
        int anchoAnterior,
        int altoAnterior,
        int dpiAnterior,
        bool vigente) {
        Assert.Equal(
            vigente,
            frmPrincipal.GeometriaVistaSigueVigente(
                new Size(anchoActual, altoActual),
                dpiActual,
                new Size(anchoAnterior, altoAnterior),
                dpiAnterior));
    }
}
