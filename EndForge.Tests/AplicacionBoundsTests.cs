using System.Drawing;
using System.Windows.Forms;

namespace EndForge.Tests;

public sealed class AplicacionBoundsTests {
    [Fact]
    public void AplicarBoundsSiCambian_MismosLimitesNoReasigna() {
        using Panel control = new() {
            Bounds = new Rectangle(12, 18, 640, 360)
        };

        bool cambio = frmPrincipal.AplicarBoundsSiCambian(
            control,
            new Rectangle(12, 18, 640, 360));

        Assert.False(cambio);
        Assert.Equal(new Rectangle(12, 18, 640, 360), control.Bounds);
    }

    [Fact]
    public void AplicarBoundsSiCambian_NuevosLimitesAsignaUnaVez() {
        using Panel control = new() {
            Bounds = new Rectangle(0, 0, 320, 180)
        };
        Rectangle destino = new(20, 24, 1280, 720);

        Assert.True(frmPrincipal.AplicarBoundsSiCambian(control, destino));
        Assert.False(frmPrincipal.AplicarBoundsSiCambian(control, destino));
        Assert.Equal(destino, control.Bounds);
    }
}
