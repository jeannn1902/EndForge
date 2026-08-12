using System.Drawing;

namespace EndForge.Tests;

public sealed class FondoEndForgeTests {
    [Fact]
    public void Cache_MismoTamanoSeReutilizaSinReconstruir() {
        using Bitmap origen = new(16, 12);
        using FondoEndForge fondo = new() {
            Size = new Size(320, 180),
            ImagenFondo = origen
        };

        Assert.True(fondo.ActualizarCacheImagenFondo());
        Assert.False(fondo.ActualizarCacheImagenFondo());
        Assert.True(fondo.TieneImagenFondoAjustada);
        Assert.Equal(new Size(320, 180), fondo.TamanoImagenFondoAjustada);
        Assert.Equal(1, fondo.ReconstruccionesImagenFondo);
    }

    [Fact]
    public void Cache_CambioDeTamanoReemplazaLaUnicaRepresentacion() {
        using Bitmap origen = new(16, 12);
        using FondoEndForge fondo = new() {
            Size = new Size(320, 180),
            ImagenFondo = origen
        };
        fondo.ActualizarCacheImagenFondo();

        fondo.Size = new Size(640, 360);

        Assert.True(fondo.ActualizarCacheImagenFondo());
        Assert.Equal(new Size(640, 360), fondo.TamanoImagenFondoAjustada);
        Assert.Equal(2, fondo.ReconstruccionesImagenFondo);
    }

    [Fact]
    public void Cache_CambioDeOrigenInvalidaLaRepresentacionAnterior() {
        using Bitmap origenInicial = new(16, 12);
        using Bitmap origenNuevo = new(24, 18);
        using FondoEndForge fondo = new() {
            Size = new Size(320, 180),
            ImagenFondo = origenInicial
        };
        fondo.ActualizarCacheImagenFondo();

        fondo.ImagenFondo = origenNuevo;

        Assert.False(fondo.TieneImagenFondoAjustada);
        Assert.True(fondo.ActualizarCacheImagenFondo());
        Assert.Equal(2, fondo.ReconstruccionesImagenFondo);
    }

    [Fact]
    public void Dispose_LiberaLaRepresentacionAjustada() {
        using Bitmap origen = new(16, 12);
        FondoEndForge fondo = new() {
            Size = new Size(320, 180),
            ImagenFondo = origen
        };
        fondo.ActualizarCacheImagenFondo();

        fondo.Dispose();

        Assert.False(fondo.TieneImagenFondoAjustada);
        Assert.Equal(Size.Empty, fondo.TamanoImagenFondoAjustada);
    }

    [Fact]
    public void Cache_AlRetirarElOrigenLiberaLaUnicaRepresentacion() {
        using Bitmap origen = new(16, 12);
        using FondoEndForge fondo = new() {
            Size = new Size(320, 180),
            ImagenFondo = origen
        };
        fondo.ActualizarCacheImagenFondo();

        fondo.ImagenFondo = null;

        Assert.False(fondo.TieneImagenFondoAjustada);
        Assert.Equal(Size.Empty, fondo.TamanoImagenFondoAjustada);
        Assert.False(fondo.ActualizarCacheImagenFondo());
    }

    [Fact]
    public void Cache_EscaladoConservaBordesCompletamenteOpacos() {
        using Bitmap origen = new(2, 2);
        using (Graphics graphics = Graphics.FromImage(origen)) {
            graphics.Clear(Color.FromArgb(255, 72, 30, 112));
        }

        using FondoEndForge fondo = new() {
            Size = new Size(320, 180),
            ImagenFondo = origen
        };

        Assert.True(fondo.ActualizarCacheImagenFondo());
        Assert.True(fondo.BordesCacheSonOpacos());
    }
}
