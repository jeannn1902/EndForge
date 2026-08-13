namespace EndForge.Tests;

public sealed class PoliticaFocoTarjetaInicioTests {
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void ResaltadoDeTarjeta_DependeDeLaPoliticaYDelFocoDescendiente(
        bool resaltarFocoContenido,
        bool contieneFoco,
        bool esperado) {
        Assert.Equal(
            esperado,
            frmPrincipal.DebeResaltarFocoTarjetaCurso(
                resaltarFocoContenido,
                contieneFoco));
    }

    [Fact]
    public void TarjetaNoInteractivaDeInicio_NoPareceSeleccionadaPorFocoDelBoton() {
        Assert.False(
            frmPrincipal.DebeResaltarFocoTarjetaCurso(
                resaltarFocoContenido: false,
                contieneFoco: true));
    }

    [Fact]
    public void TarjetasExistentesDelCurso_ConservanSuIndicadorDeFoco() {
        Assert.True(
            frmPrincipal.DebeResaltarFocoTarjetaCurso(
                resaltarFocoContenido: true,
                contieneFoco: true));
    }

    [Fact]
    public void BotonInicio_ConservaTabStopYActivacion() {
        using EndForge.Controls.BotonInicio boton = new();
        int activaciones = 0;
        boton.Click += (_, _) => activaciones++;

        Assert.True(boton.TabStop);

        boton.PerformClick();

        Assert.Equal(1, activaciones);
    }
}
