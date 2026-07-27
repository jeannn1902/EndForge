using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CatalogoPracticasTests {
    [Fact]
    public void GradoUno_ConservaTemasIdsGuiasYEvaluaciones() {
        CursoService curso = CatalogoTestHelper.CrearCursoGradoUno();
        CatalogoEvaluacionesService evaluaciones = new();
        IReadOnlyList<TemaCurso> temas = curso.CargarTemas();
        PracticaCurso[] practicas = CatalogoTestHelper.CargarPracticas(curso);

        Assert.Equal(4, temas.Count);
        Assert.Equal(
            new[] { "variables", "condicionales", "ciclos", "funciones" },
            temas.Select(tema => tema.Id));
        Assert.Equal(
            new[] { "Variables", "Condicionales", "Ciclos", "Funciones" },
            temas.Select(tema => tema.Nombre));
        Assert.Equal(
            new[] {
                "01_Variables",
                "02_Condicionales",
                "03_Ciclos",
                "04_Funciones"
            },
            temas.Select(tema => tema.NombreCarpeta));
        Assert.All(temas, tema => Assert.Equal(5, tema.Practicas.Count));

        Assert.Equal(20, curso.TotalPracticasPlaneadas);
        Assert.Equal(20, curso.TotalPracticasDisponibles);
        Assert.Equal(
            CatalogoTestHelper.IdsPracticasGradoUno,
            practicas.Select(practica => practica.Id));
        Assert.All(practicas, practica => {
            Assert.NotNull(practica.Guia);
            Assert.True(evaluaciones.EsPracticaEvaluable(practica.Id));
        });
    }

    [Fact]
    public void GradoDos_ConservaOrdenDistribucionMetaGuiasYEvaluaciones() {
        CursoService curso = CatalogoTestHelper.CrearCursoGradoDos();
        CatalogoEvaluacionesService evaluaciones = new();
        IReadOnlyList<TemaCurso> temas = curso.CargarTemas();
        PracticaCurso[] practicas = CatalogoTestHelper.CargarPracticas(curso);

        Assert.Equal(5, temas.Count);
        Assert.Equal(
            new[] {
                CursoService.TemaArreglosGradoJuniorId,
                CursoService.TemaCadenasGradoJuniorId,
                CursoService.TemaMatricesGradoJuniorId,
                CursoService.TemaEstructurasGradoJuniorId,
                CursoService.TemaArchivosGradoJuniorId
            },
            temas.Select(tema => tema.Id));
        Assert.Equal(
            new[] {
                "Arreglos",
                "Cadenas",
                "Matrices",
                "Estructuras",
                "Archivos"
            },
            temas.Select(tema => tema.Nombre));
        Assert.Equal(
            new[] {
                "01_Arreglos",
                "02_Cadenas",
                "03_Matrices",
                "04_Estructuras",
                "05_Archivos"
            },
            temas.Select(tema => tema.NombreCarpeta));
        Assert.Equal(
            new[] { 10, 8, 8, 8, 6 },
            temas.Select(tema => tema.Practicas.Count));

        Assert.Equal(40, curso.TotalPracticasPlaneadas);
        Assert.Equal(40, curso.TotalPracticasDisponibles);
        Assert.Equal(
            CatalogoTestHelper.IdsPracticasGradoDos,
            practicas.Select(practica => practica.Id));
        Assert.All(practicas, practica => {
            Assert.NotNull(practica.Guia);
            Assert.True(evaluaciones.EsPracticaEvaluable(practica.Id));
        });
    }

    [Fact]
    public void CatalogoGlobal_TieneOrdenEIdsUnicosSinEvaluacionesHuerfanas() {
        CursoService gradoUno = CatalogoTestHelper.CrearCursoGradoUno();
        CursoService gradoDos = CatalogoTestHelper.CrearCursoGradoDos();
        CursoService[] cursos = { gradoUno, gradoDos };
        TemaCurso[] temas = cursos
            .SelectMany(curso => curso.CargarTemas())
            .ToArray();
        PracticaCurso[] practicas = temas
            .SelectMany(tema => tema.Practicas)
            .ToArray();
        DefinicionEvaluacionPractica[] definiciones =
            new CatalogoEvaluacionesService()
                .CargarDefiniciones()
                .ToArray();

        Assert.Equal(9, temas.Length);
        Assert.Equal(
            temas.Length,
            temas.Select(tema => tema.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.Equal(60, practicas.Length);
        Assert.Equal(
            practicas.Length,
            practicas.Select(practica => practica.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.Equal(60, definiciones.Length);
        Assert.Equal(
            definiciones.Length,
            definiciones.Select(definicion => definicion.PracticaId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());

        HashSet<string> idsPracticas = practicas
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> idsEvaluaciones = definiciones
            .Select(definicion => definicion.PracticaId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Empty(idsPracticas.Except(idsEvaluaciones));
        Assert.Empty(idsEvaluaciones.Except(idsPracticas));

        foreach (CursoService curso in cursos) {
            TemaCurso[] temasCurso = curso.CargarTemas().ToArray();
            Assert.Equal(
                Enumerable.Range(1, temasCurso.Length),
                temasCurso.Select(tema => tema.Numero));

            foreach (TemaCurso tema in temasCurso) {
                Assert.False(tema.EsProximamente);
                Assert.Equal(
                    Enumerable.Range(1, tema.Practicas.Count),
                    tema.Practicas.Select(practica => practica.Numero));
                Assert.All(
                    tema.Practicas,
                    practica => Assert.Equal(tema.Id, practica.TemaId));
            }
        }
    }
}
