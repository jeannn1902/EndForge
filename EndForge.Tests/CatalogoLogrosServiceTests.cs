using EndForge.Services;

namespace EndForge.Tests;

public sealed class CatalogoLogrosServiceTests {
    [Fact]
    public void CargarDefiniciones_ExponeLosCatorceLogrosEnOrdenDeterminista() {
        CatalogoLogrosService servicio = new();

        IReadOnlyList<DefinicionLogro> definiciones =
            servicio.CargarDefiniciones();

        Assert.Equal(14, definiciones.Count);
        Assert.Equal(
            new[] {
                "logro:practica:primera-vinculada",
                "logro:practica:primera-realizada",
                "logro:evaluacion:primera-aprobada",
                "logro:evaluacion:primera-perfecta",
                "logro:tema:primero-completado",
                "logro:grado:primero-completado",
                "logro:practicas:realizadas:5",
                "logro:practicas:realizadas:10",
                "logro:practicas:realizadas:25",
                "logro:grado:grado-1-fundamentos-cpp:completo",
                "logro:grado:grado-2-cpp-junior:completo",
                "logro:evaluaciones:aprobadas:5",
                "logro:evaluaciones:aprobadas:10",
                "logro:evaluaciones:perfectas:5"
            },
            definiciones.Select(definicion => definicion.Id));
        Assert.Equal(
            definiciones.Count,
            definiciones.Select(definicion => definicion.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
    }

    [Fact]
    public void CargarDefiniciones_ExponeCriteriosYUmbralesEsperados() {
        CatalogoLogrosService servicio = new();

        IReadOnlyDictionary<string, DefinicionLogro> definiciones = servicio
            .CargarDefiniciones()
            .ToDictionary(
                definicion => definicion.Id,
                StringComparer.OrdinalIgnoreCase);

        AssertCriterio(
            definiciones,
            CatalogoLogrosService.PrimeraPracticaVinculadaId,
            CriterioLogro.PracticasVinculadasDistintas,
            1);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.PrimeraPracticaRealizadaId,
            CriterioLogro.PracticasRealizadasDistintas,
            1);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.PrimeraEvaluacionAprobadaId,
            CriterioLogro.PracticasAprobadasDistintas,
            1);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.PrimeraEvaluacionPerfectaId,
            CriterioLogro.PracticasPerfectasDistintas,
            1);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.PrimerTemaCompletadoId,
            CriterioLogro.TemasCompletadosDistintos,
            1);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.PrimerGradoCompletadoId,
            CriterioLogro.GradosCompletadosDistintos,
            1);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.CincoPracticasRealizadasId,
            CriterioLogro.PracticasRealizadasDistintas,
            5);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.DiezPracticasRealizadasId,
            CriterioLogro.PracticasRealizadasDistintas,
            10);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.VeinticincoPracticasRealizadasId,
            CriterioLogro.PracticasRealizadasDistintas,
            25);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.CincoPracticasAprobadasId,
            CriterioLogro.PracticasAprobadasDistintas,
            5);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.DiezPracticasAprobadasId,
            CriterioLogro.PracticasAprobadasDistintas,
            10);
        AssertCriterio(
            definiciones,
            CatalogoLogrosService.CincoPracticasPerfectasId,
            CriterioLogro.PracticasPerfectasDistintas,
            5);
    }

    [Fact]
    public void CargarDefiniciones_UsaLosIdentificadoresRealesDeGrado() {
        CatalogoLogrosService servicio = new();

        DefinicionLogro gradoUno = Assert.Single(
            servicio.CargarDefiniciones(),
            definicion => definicion.Id ==
                CatalogoLogrosService.GradoFundamentosCompletoId);
        DefinicionLogro gradoDos = Assert.Single(
            servicio.CargarDefiniciones(),
            definicion => definicion.Id ==
                CatalogoLogrosService.GradoJuniorCompletoId);

        Assert.Equal(GradosService.GradoFundamentosId, gradoUno.GradoId);
        Assert.Equal(GradosService.GradoJuniorId, gradoDos.GradoId);
        Assert.Equal(
            $"logro:grado:{GradosService.GradoFundamentosId}:completo",
            gradoUno.Id);
        Assert.Equal(
            $"logro:grado:{GradosService.GradoJuniorId}:completo",
            gradoDos.Id);
        Assert.Equal(CriterioLogro.GradoEspecificoCompletado, gradoUno.Criterio);
        Assert.Equal(CriterioLogro.GradoEspecificoCompletado, gradoDos.Criterio);
    }

    [Theory]
    [InlineData("logro:practica:primera-vinculada")]
    [InlineData("LOGRO:PRACTICA:PRIMERA-VINCULADA")]
    public void ObtenerDefinicion_EncuentraPorIdSinDistinguirMayusculas(
        string id) {
        CatalogoLogrosService servicio = new();

        DefinicionLogro? definicion = servicio.ObtenerDefinicion(id);

        Assert.NotNull(definicion);
        Assert.Equal(
            CatalogoLogrosService.PrimeraPracticaVinculadaId,
            definicion.Id);
        Assert.True(servicio.EsLogroConocido(id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" logro:practica:primera-vinculada ")]
    [InlineData("logro:desconocido")]
    public void ObtenerDefinicion_IdInvalidoONoRegistrado_DevuelveNull(string id) {
        CatalogoLogrosService servicio = new();

        Assert.Null(servicio.ObtenerDefinicion(id));
        Assert.False(servicio.EsLogroConocido(id));
    }

    private static void AssertCriterio(
        IReadOnlyDictionary<string, DefinicionLogro> definiciones,
        string id,
        CriterioLogro criterio,
        int umbral) {
        DefinicionLogro definicion = definiciones[id];

        Assert.Equal(criterio, definicion.Criterio);
        Assert.Equal(umbral, definicion.Umbral);
        Assert.Null(definicion.GradoId);
    }
}
