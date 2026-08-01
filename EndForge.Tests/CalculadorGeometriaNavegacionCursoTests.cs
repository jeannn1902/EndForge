using System.Drawing;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CalculadorGeometriaNavegacionCursoTests {
    [Fact]
    public void CalcularPanelPrincipalConMenu_UsaAreaActualYReservaMenu() {
        Rectangle resultado =
            CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
                new Rectangle(0, 0, 1600, 900),
                limiteSuperior: 36,
                limiteIzquierdo: 205,
                margen: 24);

        Assert.Equal(new Rectangle(229, 60, 1347, 816), resultado);
    }

    [Fact]
    public void CalcularPanelPrincipalConMenu_UnAreaAnteriorNoContaminaCurso() {
        _ = CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
            new Rectangle(0, 0, 1200, 800),
            limiteSuperior: 36,
            limiteIzquierdo: 205,
            margen: 24);

        Rectangle resultado =
            CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
                new Rectangle(0, 0, 1920, 1080),
                limiteSuperior: 36,
                limiteIzquierdo: 205,
                margen: 24);

        Assert.Equal(new Rectangle(229, 60, 1667, 996), resultado);
    }

    [Fact]
    public void CalcularPanelPrincipalConMenu_RecorridosRepetidosSonIdempotentes() {
        Rectangle area = new(0, 0, 1440, 900);

        Rectangle primero =
            CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
                area,
                limiteSuperior: 36,
                limiteIzquierdo: 205,
                margen: 24);
        Rectangle segundo =
            CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
                area,
                limiteSuperior: 36,
                limiteIzquierdo: 205,
                margen: 24);

        Assert.Equal(primero, segundo);
    }

    [Fact]
    public void CalcularPanelPrincipalConMenu_AreasCompactasConservanLimitesValidos() {
        Rectangle resultado =
            CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
                new Rectangle(0, 0, 220, 80),
                limiteSuperior: 36,
                limiteIzquierdo: 205,
                margen: 24);

        Assert.Equal(new Rectangle(212, 57, 1, 2), resultado);
    }

    [Fact]
    public void CalcularAnchoContenidoCurricular_RestauradoConservaLimiteActual() {
        int resultado =
            CalculadorGeometriaNavegacionCurso.CalcularAnchoContenidoCurricular(
                anchoVista: 947,
                margenHorizontal: 16,
                anchoMaximo: 1180);

        Assert.Equal(915, resultado);
    }

    [Fact]
    public void CalcularAnchoContenidoCurricular_MaximizadoUsaAnchoAmplio() {
        int resultado =
            CalculadorGeometriaNavegacionCurso.CalcularAnchoContenidoCurricular(
                anchoVista: 1667,
                margenHorizontal: 0,
                anchoMaximo: 1500);

        Assert.Equal(1500, resultado);
    }
}
