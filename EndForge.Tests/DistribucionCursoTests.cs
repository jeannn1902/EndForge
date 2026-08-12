namespace EndForge.Tests;

public sealed class DistribucionCursoTests {
    [Theory]
    [InlineData(false, false, false, false, false)]
    [InlineData(true, false, false, false, false)]
    [InlineData(true, true, false, false, true)]
    [InlineData(true, false, true, false, true)]
    [InlineData(true, false, false, true, true)]
    [InlineData(true, true, true, true, true)]
    public void DebeRecalcularDistribucionCurso_SoloParaCursoActivo(
        bool inicializado,
        bool distribucionCurso,
        bool modoInmersivo,
        bool vistaActiva,
        bool esperado) {
        Assert.Equal(
            esperado,
            frmPrincipal.DebeRecalcularDistribucionCurso(
                inicializado,
                distribucionCurso,
                modoInmersivo,
                vistaActiva));
    }
}
