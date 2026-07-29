using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class ResumenAprendizajeServiceTests {
    private static readonly DateTimeOffset FechaBase =
        new(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CatalogoPublicado_CuentaSesentaPracticasNueveTemasYDosGrados() {
        ResumenInicio resumen = CrearServicio().CrearResumen();

        Assert.Equal(60, resumen.Progreso.TotalPracticasPublicadas);
        Assert.Equal(9, resumen.Progreso.TotalTemas);
        Assert.Equal(2, resumen.Progreso.TotalGrados);
        Assert.Equal(2, resumen.Progreso.Grados.Count);
        Assert.Equal(9, resumen.Progreso.Grados.Sum(grado => grado.Temas.Count));
        Assert.Equal(
            60,
            resumen.Progreso.Grados.Sum(grado => grado.TotalPracticasPublicadas));
        Assert.Equal(
            new[] {
                GradosService.GradoFundamentosId,
                GradosService.GradoJuniorId
            },
            resumen.Progreso.Grados.Select(grado => grado.GradoId));
    }

    [Fact]
    public void CatalogoConIdDuplicadoSinDistinguirMayusculas_SeRechazaClaramente() {
        PracticaCurso primera = new() {
            Id = "practica-duplicada",
            TemaId = "tema-prueba",
            Numero = 1,
            Nombre = "Primera"
        };
        PracticaCurso segunda = new() {
            Id = "PRACTICA-DUPLICADA",
            TemaId = "tema-prueba",
            Numero = 2,
            Nombre = "Segunda"
        };
        TemaCurso tema = new() {
            Id = "tema-prueba",
            Numero = 1,
            Nombre = "Tema",
            Practicas = Array.AsReadOnly(new[] { primera, segunda })
        };
        GradoCurso grado = new() {
            Id = "grado-prueba",
            Numero = 1,
            Nombre = "Grado",
            EsContenidoDisponible = true,
            Temas = Array.AsReadOnly(new[] { tema })
        };
        ResumenAprendizajeService servicio = CrearServicio(
            () => Array.AsReadOnly(new[] { grado }),
            () => CrearCargaProgreso(),
            () => CrearCargaHistorial());

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => servicio.CrearResumen());

        Assert.Contains("duplicado", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UsuarioNuevo_ConservaTotalesYDevuelveSinActividad() {
        ResumenInicio resumen = CrearServicio(
            progreso: new ResultadoCargaProgreso {
                Estado = EstadoCargaProgreso.ArchivoInexistente
            },
            historial: new ResultadoCargaHistorialEvaluaciones {
                Estado = EstadoCargaHistorialEvaluaciones.ArchivoInexistente
            }).CrearResumen();

        Assert.Equal(EstadoDisponibilidadDatos.SinActividad, resumen.Estado);
        Assert.Equal(0, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(0, resumen.Progreso.PracticasEnProgreso);
        Assert.Equal(60, resumen.Progreso.PracticasPendientes);
        Assert.Equal(0, resumen.Progreso.PorcentajeGlobal);
        Assert.Equal(0, resumen.Progreso.TemasCompletados);
        Assert.Equal(0, resumen.Progreso.GradosCompletados);
        Assert.Equal(0L, resumen.Evaluaciones.TotalEvaluacionesRealizadas);
        Assert.Equal(0, resumen.Evaluaciones.PracticasEvaluadas);
        Assert.Null(resumen.UltimaActividad);
        Assert.Equal(
            EstadoFuenteDatosAprendizaje.SinDatos,
            resumen.FuenteProgreso.Estado);
        Assert.Equal(
            EstadoFuenteDatosAprendizaje.SinDatos,
            resumen.FuenteHistorial.Estado);
    }

    [Fact]
    public void IdHuerfanoDeProgreso_NoAlteraContadores() {
        ProgresoPractica conocida = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            EstadoPracticaCurso.Realizada);
        ProgresoPractica huerfana = CrearProgreso(
            "practica-que-no-existe",
            EstadoPracticaCurso.Realizada);

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(conocida, huerfana)).CrearResumen();

        Assert.Equal(1, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(59, resumen.Progreso.PracticasPendientes);
        Assert.Equal(1, resumen.FuenteProgreso.RegistrosHuerfanos);
        Assert.Equal(
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
            resumen.Estado);
    }

    [Fact]
    public void IdHuerfanoDeHistorial_NoAlteraMetricas() {
        HistorialPractica conocida = CrearHistorial(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            totalIntentos: 3,
            mejorCalificacion: 80);
        HistorialPractica huerfana = CrearHistorial(
            "practica-que-no-existe",
            totalIntentos: 100,
            mejorCalificacion: 100);

        ResumenInicio resumen = CrearServicio(
            historial: CrearCargaHistorial(conocida, huerfana)).CrearResumen();

        Assert.Equal(3L, resumen.Evaluaciones.TotalEvaluacionesRealizadas);
        Assert.Equal(1, resumen.Evaluaciones.PracticasEvaluadas);
        Assert.Equal(80, resumen.Evaluaciones.MejorCalificacionGlobal);
        Assert.Equal(1, resumen.FuenteHistorial.RegistrosHuerfanos);
        Assert.Equal(
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
            resumen.Estado);
    }

    [Fact]
    public void ProgresoParcialDeGradoUno_CuentaEstadosCorrectamente() {
        ProgresoPractica realizada = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            EstadoPracticaCurso.Realizada);
        ProgresoPractica enProgreso = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoUno[1],
            EstadoPracticaCurso.EnProgreso,
            FechaBase.AddDays(1));

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(realizada, enProgreso)).CrearResumen();
        ResumenProgresoGrado gradoUno = ObtenerGrado(
            resumen,
            GradosService.GradoFundamentosId);
        ResumenProgresoGrado gradoDos = ObtenerGrado(
            resumen,
            GradosService.GradoJuniorId);

        Assert.Equal(1, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(1, resumen.Progreso.PracticasEnProgreso);
        Assert.Equal(58, resumen.Progreso.PracticasPendientes);
        Assert.Equal(1, gradoUno.PracticasRealizadas);
        Assert.Equal(1, gradoUno.PracticasEnProgreso);
        Assert.Equal(18, gradoUno.PracticasPendientes);
        Assert.Equal(0, gradoDos.PracticasRealizadas);
        Assert.Equal(0, gradoDos.PracticasEnProgreso);
        Assert.Equal(40, gradoDos.PracticasPendientes);
    }

    [Fact]
    public void ProgresoParcialDeGradoDos_CuentaEstadosCorrectamente() {
        ProgresoPractica realizada = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoDos[0],
            EstadoPracticaCurso.Realizada);
        ProgresoPractica enProgreso = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoDos[1],
            EstadoPracticaCurso.EnProgreso,
            FechaBase.AddDays(1));

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(realizada, enProgreso)).CrearResumen();
        ResumenProgresoGrado gradoUno = ObtenerGrado(
            resumen,
            GradosService.GradoFundamentosId);
        ResumenProgresoGrado gradoDos = ObtenerGrado(
            resumen,
            GradosService.GradoJuniorId);

        Assert.Equal(1, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(1, resumen.Progreso.PracticasEnProgreso);
        Assert.Equal(0, gradoUno.PracticasRealizadas);
        Assert.Equal(20, gradoUno.PracticasPendientes);
        Assert.Equal(1, gradoDos.PracticasRealizadas);
        Assert.Equal(1, gradoDos.PracticasEnProgreso);
        Assert.Equal(38, gradoDos.PracticasPendientes);
    }

    [Fact]
    public void ProgresoCombinado_AgregaAmbosGradosSinDuplicar() {
        ProgresoPractica[] progreso = {
            CrearProgreso(
                CatalogoTestHelper.IdsPracticasGradoUno[0],
                EstadoPracticaCurso.Realizada),
            CrearProgreso(
                CatalogoTestHelper.IdsPracticasGradoUno[1],
                EstadoPracticaCurso.EnProgreso),
            CrearProgreso(
                CatalogoTestHelper.IdsPracticasGradoDos[0],
                EstadoPracticaCurso.Realizada),
            CrearProgreso(
                CatalogoTestHelper.IdsPracticasGradoDos[1],
                EstadoPracticaCurso.Realizada)
        };

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(progreso)).CrearResumen();

        Assert.Equal(2, resumen.Progreso.Grados.Count);
        Assert.Equal(3, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(1, resumen.Progreso.PracticasEnProgreso);
        Assert.Equal(56, resumen.Progreso.PracticasPendientes);
        Assert.Equal(
            3,
            resumen.Progreso.Grados.Sum(grado => grado.PracticasRealizadas));
        Assert.Equal(
            1,
            resumen.Progreso.Grados.Sum(grado => grado.PracticasEnProgreso));
    }

    [Fact]
    public void PorcentajeGlobal_UsaSesentaPublicadasYRedondeoActual() {
        ProgresoPractica realizada = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            EstadoPracticaCurso.Realizada);

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(realizada)).CrearResumen();

        Assert.Equal(2, resumen.Progreso.PorcentajeGlobal);
        Assert.Equal(
            5,
            ObtenerGrado(resumen, GradosService.GradoFundamentosId).Porcentaje);
        Assert.Equal(
            0,
            ObtenerGrado(resumen, GradosService.GradoJuniorId).Porcentaje);
    }

    [Fact]
    public void Tema_SeCompletaSoloConTodasSusPracticasRealizadas() {
        ProgresoPractica[] progreso = CatalogoTestHelper.IdsPracticasGradoUno
            .Take(5)
            .Select(id => CrearProgreso(id, EstadoPracticaCurso.Realizada))
            .ToArray();

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(progreso)).CrearResumen();
        ResumenProgresoTema variables = ObtenerGrado(
                resumen,
                GradosService.GradoFundamentosId)
            .Temas.Single(tema => tema.TemaId == "variables");
        ResumenProgresoTema condicionales = ObtenerGrado(
                resumen,
                GradosService.GradoFundamentosId)
            .Temas.Single(tema => tema.TemaId == "condicionales");

        Assert.Equal(1, resumen.Progreso.TemasCompletados);
        Assert.True(variables.Completado);
        Assert.Equal(100, variables.Porcentaje);
        Assert.False(condicionales.Completado);
        Assert.Equal(0, condicionales.Porcentaje);
    }

    [Fact]
    public void Grado_SeCompletaSoloConTodasSusPracticasRealizadas() {
        ProgresoPractica[] progreso = CatalogoTestHelper.IdsPracticasGradoUno
            .Select(id => CrearProgreso(id, EstadoPracticaCurso.Realizada))
            .ToArray();

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(progreso)).CrearResumen();
        ResumenProgresoGrado gradoUno = ObtenerGrado(
            resumen,
            GradosService.GradoFundamentosId);
        ResumenProgresoGrado gradoDos = ObtenerGrado(
            resumen,
            GradosService.GradoJuniorId);

        Assert.Equal(1, resumen.Progreso.GradosCompletados);
        Assert.Equal(4, resumen.Progreso.TemasCompletados);
        Assert.True(gradoUno.Completado);
        Assert.Equal(100, gradoUno.Porcentaje);
        Assert.False(gradoDos.Completado);
        Assert.Equal(33, resumen.Progreso.PorcentajeGlobal);
    }

    [Fact]
    public void EvaluacionesRealizadas_SumanTotalIntentosDePracticasConocidas() {
        HistorialPractica[] historial = {
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[0],
                totalIntentos: 3,
                mejorCalificacion: 75),
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoDos[0],
                totalIntentos: 5,
                mejorCalificacion: 85)
        };

        ResumenInicio resumen = CrearServicio(
            historial: CrearCargaHistorial(historial)).CrearResumen();

        Assert.Equal(8L, resumen.Evaluaciones.TotalEvaluacionesRealizadas);
        Assert.Equal(2, resumen.Evaluaciones.PracticasEvaluadas);
    }

    [Fact]
    public void Promedio_UsaMejorCalificacionExcluyeNullYRedondeaAwayFromZero() {
        HistorialPractica[] historial = {
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[0],
                totalIntentos: 2,
                mejorCalificacion: 80),
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[1],
                totalIntentos: 1,
                mejorCalificacion: 81),
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[2],
                totalIntentos: 1,
                mejorCalificacion: null)
        };

        ResumenInicio resumen = CrearServicio(
            historial: CrearCargaHistorial(historial)).CrearResumen();

        Assert.Equal(81, resumen.Evaluaciones.PromedioMejoresCalificaciones);
        Assert.Equal(81, resumen.Evaluaciones.MejorCalificacionGlobal);
        Assert.Equal(3, resumen.Evaluaciones.PracticasEvaluadas);
    }

    [Fact]
    public void EvaluacionesAprobadas_UsanUmbralInclusivoDeSetenta() {
        HistorialPractica[] historial = {
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[0],
                totalIntentos: 1,
                mejorCalificacion: 69),
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[1],
                totalIntentos: 1,
                mejorCalificacion: 70),
            CrearHistorial(
                CatalogoTestHelper.IdsPracticasGradoUno[2],
                totalIntentos: 1,
                mejorCalificacion: 100)
        };

        ResumenInicio resumen = CrearServicio(
            historial: CrearCargaHistorial(historial)).CrearResumen();

        Assert.Equal(2, resumen.Evaluaciones.EvaluacionesAprobadas);
        Assert.Equal(100, resumen.Evaluaciones.MejorCalificacionGlobal);
    }

    [Fact]
    public void UltimaActividad_UsaLaFechaMasRecienteEntreProgresoEHistorial() {
        string practicaProgreso = CatalogoTestHelper.IdsPracticasGradoUno[0];
        string practicaHistorial = CatalogoTestHelper.IdsPracticasGradoDos[0];
        DateTimeOffset fechaProgreso = FechaBase.AddDays(2);
        DateTimeOffset fechaHistorial = FechaBase.AddDays(4);
        ProgresoPractica progreso = CrearProgreso(
            practicaProgreso,
            EstadoPracticaCurso.EnProgreso,
            fechaProgreso);
        HistorialPractica historial = CrearHistorial(
            practicaHistorial,
            totalIntentos: 1,
            mejorCalificacion: 80,
            fechaUltimoIntento: fechaHistorial);

        ResumenInicio resumen = CrearServicio(
            progreso: CrearCargaProgreso(progreso),
            historial: CrearCargaHistorial(historial)).CrearResumen();

        Assert.NotNull(resumen.UltimaActividad);
        Assert.Equal(fechaHistorial, resumen.UltimaActividad!.Fecha);
        Assert.Equal(
            practicaHistorial,
            resumen.UltimaActividad.Practica.PracticaId);
        Assert.Equal(
            FuenteActividadAprendizaje.HistorialEvaluaciones,
            resumen.UltimaActividad.Fuente);
        Assert.True(resumen.UltimaActividad.EsAproximada);
    }

    [Fact]
    public void ContenidoRecuperableConInvalidos_ConservaDatosYMarcaParcial() {
        ProgresoPractica conocida = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            EstadoPracticaCurso.Realizada);
        ResultadoCargaProgreso carga = new() {
            Estado = EstadoCargaProgreso.ContenidoInvalido,
            Progreso = new ProgresoCurso {
                Practicas = new List<ProgresoPractica> { conocida }
            },
            RegistrosInvalidos = 2,
            Error = new InvalidDataException("Dos registros no son recuperables.")
        };

        ResumenInicio resumen = CrearServicio(progreso: carga).CrearResumen();

        Assert.Equal(
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
            resumen.Estado);
        Assert.Equal(
            EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
            resumen.FuenteProgreso.Estado);
        Assert.Equal(2, resumen.FuenteProgreso.RegistrosInvalidos);
        Assert.Equal(1, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(0, resumen.Progreso.PracticasEnProgreso);
        Assert.Null(resumen.Progreso.PracticasPendientes);
        Assert.Null(resumen.Progreso.PorcentajeGlobal);
        Assert.Null(resumen.Progreso.TemasCompletados);
        Assert.Null(resumen.Progreso.GradosCompletados);
    }

    [Fact]
    public void ProgresoNoDisponible_ConservaHistorialSinExponerCerosFalsos() {
        ResultadoCargaProgreso progreso = new() {
            Estado = EstadoCargaProgreso.ErrorIo,
            Error = new IOException("progreso.json bloqueado")
        };
        HistorialPractica historial = CrearHistorial(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            totalIntentos: 3,
            mejorCalificacion: 90);

        ResumenInicio resumen = CrearServicio(
            progreso,
            CrearCargaHistorial(historial)).CrearResumen();

        Assert.Equal(
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
            resumen.Estado);
        Assert.Equal(
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
            resumen.FuenteProgreso.Estado);
        Assert.Null(resumen.Progreso.PracticasRealizadas);
        Assert.Null(resumen.Progreso.PracticasEnProgreso);
        Assert.Null(resumen.Progreso.PracticasPendientes);
        Assert.Null(resumen.Progreso.PorcentajeGlobal);
        Assert.Equal(3L, resumen.Evaluaciones.TotalEvaluacionesRealizadas);
        Assert.Equal(90, resumen.Evaluaciones.MejorCalificacionGlobal);
    }

    [Fact]
    public void HistorialNoDisponible_ConservaProgresoSinExponerCerosFalsos() {
        ProgresoPractica progreso = CrearProgreso(
            CatalogoTestHelper.IdsPracticasGradoUno[0],
            EstadoPracticaCurso.Realizada);
        ResultadoCargaHistorialEvaluaciones historial = new() {
            Estado = EstadoCargaHistorialEvaluaciones.VersionNoCompatible,
            Error = new InvalidDataException("Versión futura.")
        };

        ResumenInicio resumen = CrearServicio(
            CrearCargaProgreso(progreso),
            historial).CrearResumen();

        Assert.Equal(
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
            resumen.Estado);
        Assert.Equal(1, resumen.Progreso.PracticasRealizadas);
        Assert.Equal(59, resumen.Progreso.PracticasPendientes);
        Assert.Equal(
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
            resumen.FuenteHistorial.Estado);
        Assert.Null(resumen.Evaluaciones.TotalEvaluacionesRealizadas);
        Assert.Null(resumen.Evaluaciones.PracticasEvaluadas);
        Assert.Null(resumen.Evaluaciones.EvaluacionesAprobadas);
        Assert.Null(resumen.Evaluaciones.PromedioMejoresCalificaciones);
        Assert.Null(resumen.Evaluaciones.MejorCalificacionGlobal);
    }

    [Fact]
    public void AmbasFuentesNoDisponibles_NoPresentanResumenComoProgresoCero() {
        ResumenInicio resumen = CrearServicio(
            progreso: new ResultadoCargaProgreso {
                Estado = EstadoCargaProgreso.PermisosInsuficientes,
                Error = new UnauthorizedAccessException()
            },
            historial: new ResultadoCargaHistorialEvaluaciones {
                Estado = EstadoCargaHistorialEvaluaciones.ErrorIo,
                Error = new IOException()
            }).CrearResumen();

        Assert.Equal(
            EstadoDisponibilidadDatos.DatosTemporalmenteNoDisponibles,
            resumen.Estado);
        Assert.Equal(60, resumen.Progreso.TotalPracticasPublicadas);
        Assert.Equal(9, resumen.Progreso.TotalTemas);
        Assert.Equal(2, resumen.Progreso.TotalGrados);
        Assert.Null(resumen.Progreso.PracticasRealizadas);
        Assert.Null(resumen.Progreso.PracticasEnProgreso);
        Assert.Null(resumen.Progreso.PracticasPendientes);
        Assert.Null(resumen.Progreso.PorcentajeGlobal);
        Assert.Null(resumen.Evaluaciones.TotalEvaluacionesRealizadas);
        Assert.Null(resumen.Evaluaciones.PracticasEvaluadas);
        Assert.Null(resumen.UltimaActividad);
    }

    [Fact]
    public void ExcepcionDeUnaFuente_SeEstructuraComoParcialYNoComoCero() {
        IOException error = new("Fallo inesperado de lectura.");
        ResumenAprendizajeService servicio = CrearServicio(
            CargarCatalogo,
            () => throw error,
            () => CrearCargaHistorial());

        ResumenInicio resumen = servicio.CrearResumen();

        Assert.Equal(
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
            resumen.Estado);
        Assert.Equal(
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
            resumen.FuenteProgreso.Estado);
        Assert.Same(error, resumen.FuenteProgreso.Error);
        Assert.Null(resumen.Progreso.PracticasRealizadas);
        Assert.Equal(0L, resumen.Evaluaciones.TotalEvaluacionesRealizadas);
    }

    [Fact]
    public async Task CancelacionDuranteCarga_NoDevuelveSnapshotParcial() {
        using ManualResetEventSlim cargaIniciada = new(initialState: false);
        using ManualResetEventSlim permitirFinalizacion = new(initialState: false);
        using CancellationTokenSource cancelacion = new();
        int cargasHistorial = 0;
        ResumenAprendizajeService servicio = CrearServicio(
            CargarCatalogo,
            () => {
                cargaIniciada.Set();
                permitirFinalizacion.Wait(TimeSpan.FromSeconds(5));
                return CrearCargaProgreso();
            },
            () => {
                Interlocked.Increment(ref cargasHistorial);
                return CrearCargaHistorial();
            });

        Task<ResumenInicio> operacion = servicio.CrearResumenAsync(
            cancelacion.Token);

        try {
            Assert.True(cargaIniciada.Wait(TimeSpan.FromSeconds(5)));
            cancelacion.Cancel();
        } finally {
            permitirFinalizacion.Set();
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await operacion);
        Assert.Equal(0, Volatile.Read(ref cargasHistorial));
    }

    [Fact]
    public void CrearResumen_CargaCatalogoProgresoEHistorialUnaSolaVez() {
        int cargasCatalogo = 0;
        int cargasProgreso = 0;
        int cargasHistorial = 0;
        ResumenAprendizajeService servicio = CrearServicio(
            () => {
                Interlocked.Increment(ref cargasCatalogo);
                return CargarCatalogo();
            },
            () => {
                Interlocked.Increment(ref cargasProgreso);
                return CrearCargaProgreso();
            },
            () => {
                Interlocked.Increment(ref cargasHistorial);
                return CrearCargaHistorial();
            });

        ResumenInicio resumen = servicio.CrearResumen();

        Assert.NotNull(resumen);
        Assert.Equal(1, cargasCatalogo);
        Assert.Equal(1, cargasProgreso);
        Assert.Equal(1, cargasHistorial);
    }

    private static ResumenAprendizajeService CrearServicio(
        ResultadoCargaProgreso? progreso = null,
        ResultadoCargaHistorialEvaluaciones? historial = null) {
        ResultadoCargaProgreso cargaProgreso =
            progreso ?? CrearCargaProgreso();
        ResultadoCargaHistorialEvaluaciones cargaHistorial =
            historial ?? CrearCargaHistorial();

        return CrearServicio(
            CargarCatalogo,
            () => cargaProgreso,
            () => cargaHistorial);
    }

    private static ResumenAprendizajeService CrearServicio(
        Func<IReadOnlyList<GradoCurso>> cargarCatalogo,
        Func<ResultadoCargaProgreso> cargarProgreso,
        Func<ResultadoCargaHistorialEvaluaciones> cargarHistorial) {
        return new ResumenAprendizajeService(
            cargarCatalogo,
            cargarProgreso,
            cargarHistorial,
            new RecomendacionAprendizajeService(_ => false));
    }

    private static IReadOnlyList<GradoCurso> CargarCatalogo() {
        return new GradosService(new CursoService()).CargarGrados(null);
    }

    private static ResultadoCargaProgreso CrearCargaProgreso(
        params ProgresoPractica[] practicas) {
        return new ResultadoCargaProgreso {
            Estado = EstadoCargaProgreso.Exitosa,
            Progreso = new ProgresoCurso {
                Practicas = practicas.ToList()
            }
        };
    }

    private static ResultadoCargaHistorialEvaluaciones CrearCargaHistorial(
        params HistorialPractica[] practicas) {
        return new ResultadoCargaHistorialEvaluaciones {
            Estado = EstadoCargaHistorialEvaluaciones.Exitosa,
            Historial = new HistorialEvaluaciones {
                Practicas = Array.AsReadOnly(practicas)
            }
        };
    }

    private static ProgresoPractica CrearProgreso(
        string practicaId,
        EstadoPracticaCurso estado,
        DateTimeOffset? fecha = null) {
        DateTimeOffset fechaRegistro = fecha ?? FechaBase;

        return new ProgresoPractica {
            PracticaId = practicaId,
            Estado = estado,
            FechaCreacion = fechaRegistro.AddHours(-1),
            FechaActualizacion = fechaRegistro,
            FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
                ? fechaRegistro
                : null
        };
    }

    private static HistorialPractica CrearHistorial(
        string practicaId,
        int totalIntentos,
        int? mejorCalificacion,
        DateTimeOffset? fechaUltimoIntento = null) {
        return new HistorialPractica {
            PracticaId = practicaId,
            TotalIntentos = totalIntentos,
            MejorCalificacion = mejorCalificacion,
            UltimaCalificacion = mejorCalificacion,
            FechaUltimoIntento = fechaUltimoIntento ?? FechaBase
        };
    }

    private static ResumenProgresoGrado ObtenerGrado(
        ResumenInicio resumen,
        string gradoId) {
        return resumen.Progreso.Grados.Single(grado =>
            grado.GradoId.Equals(gradoId, StringComparison.OrdinalIgnoreCase));
    }
}
