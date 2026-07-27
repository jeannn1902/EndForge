using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CatalogoGradosTests {
    [Fact]
    public void CargarGrados_ConservaOrdenMetasYDisponibilidad() {
        CursoService gradoUno = CatalogoTestHelper.CrearCursoGradoUno();
        GradosService servicio = new(gradoUno);

        IReadOnlyList<GradoCurso> grados = servicio.CargarGrados(null);

        Assert.Equal(5, grados.Count);
        Assert.Equal(
            new[] {
                GradosService.GradoFundamentosId,
                GradosService.GradoJuniorId,
                "grado-3",
                "grado-4",
                "grado-5"
            },
            grados.Select(grado => grado.Id));
        Assert.Equal(
            new[] { 1, 2, 3, 4, 5 },
            grados.Select(grado => grado.Numero));

        GradoCurso gradoFundamentos = grados[0];
        Assert.True(gradoFundamentos.EsContenidoDisponible);
        Assert.Equal(20, gradoFundamentos.CantidadPracticasDisponibles);
        Assert.Equal(
            GradosService.MetaCurricularPracticasGradoFundamentos,
            gradoFundamentos.CantidadPracticasPlaneadas);

        GradoCurso gradoJunior = grados[1];
        Assert.True(gradoJunior.EsContenidoDisponible);
        Assert.Equal(40, gradoJunior.CantidadPracticasDisponibles);
        Assert.Equal(
            GradosService.MetaCurricularPracticasGradoJunior,
            gradoJunior.CantidadPracticasPlaneadas);

        Assert.All(
            grados.Skip(2),
            grado => {
                Assert.False(grado.EsContenidoDisponible);
                Assert.Equal(EstadoGradoCurso.Proximamente, grado.Estado);
            });
    }

    [Fact]
    public void ObtenerCurso_ExponeLosDosCatalogosPublicados() {
        CursoService gradoUno = CatalogoTestHelper.CrearCursoGradoUno();
        GradosService servicio = new(gradoUno);

        CursoService? catalogoUno = servicio.ObtenerCurso(
            GradosService.GradoFundamentosId);
        CursoService? catalogoDos = servicio.ObtenerCurso(
            GradosService.GradoJuniorId);

        Assert.Same(gradoUno, catalogoUno);
        Assert.NotNull(catalogoDos);
        Assert.Equal(1, catalogoUno!.NumeroGrado);
        Assert.Equal(2, catalogoDos!.NumeroGrado);
        Assert.Equal(
            CursoService.TotalPracticasPlaneadasGradoFundamentos,
            catalogoUno.TotalPracticasPlaneadas);
        Assert.Equal(
            CursoService.TotalPracticasPlaneadasGradoJunior,
            catalogoDos.TotalPracticasPlaneadas);
        Assert.Null(servicio.ObtenerCurso("grado-inexistente"));
    }
}
