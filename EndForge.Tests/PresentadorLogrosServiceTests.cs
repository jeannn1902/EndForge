using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class PresentadorLogrosServiceTests {
    private static readonly DateTimeOffset FechaReconocimiento =
        new(2026, 8, 9, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Crear_SinActividad_MuestraCeroDeCatorceYTodosPendientes() {
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(),
            CrearMotivacion(EstadoDisponibilidadMotivacion.SinActividad));

        Assert.Equal(EstadoPresentacionLogros.SinActividad, presentacion.Estado);
        Assert.Equal(0, presentacion.LogrosDesbloqueados);
        Assert.Equal(14, presentacion.TotalLogros);
        Assert.Equal("0 de 14 logros desbloqueados", presentacion.TextoResumen);
        Assert.All(presentacion.Logros, logro =>
            Assert.Equal(EstadoLogroPresentable.Pendiente, logro.Estado));
    }

    [Fact]
    public void Crear_TodosDesbloqueados_MuestraCatorceDeCatorce() {
        LogroDesbloqueado[] reconocidos = new CatalogoLogrosService()
            .CargarDefiniciones()
            .Select(definicion => CrearLogro(definicion.Id))
            .ToArray();
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(realizadas: 60, aprobadas: 60),
            CrearMotivacion(logros: reconocidos));

        Assert.Equal(14, presentacion.LogrosDesbloqueados);
        Assert.Equal("14 de 14 logros desbloqueados", presentacion.TextoResumen);
        Assert.All(presentacion.Logros, logro =>
            Assert.Equal(EstadoLogroPresentable.Desbloqueado, logro.Estado));
    }

    [Theory]
    [InlineData(
        EstadoDisponibilidadMotivacion.NoDisponible,
        EstadoPresentacionLogros.NoDisponible)]
    [InlineData(
        EstadoDisponibilidadMotivacion.VersionIncompatible,
        EstadoPresentacionLogros.VersionIncompatible)]
    public void Crear_NoDisponible_NoInventaEstadoNiProgreso(
        EstadoDisponibilidadMotivacion estado,
        EstadoPresentacionLogros esperado) {
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(realizadas: 12, aprobadas: 8),
            CrearMotivacion(estado));

        Assert.Equal(esperado, presentacion.Estado);
        Assert.Null(presentacion.LogrosDesbloqueados);
        Assert.Equal("Logros no disponibles", presentacion.TextoResumen);
        Assert.All(presentacion.Logros, logro => {
            Assert.Equal(
                EstadoLogroPresentable.EstadoNoDisponible,
                logro.Estado);
            Assert.Null(logro.ProgresoActual);
            Assert.Null(logro.ProgresoObjetivo);
        });
    }

    [Fact]
    public void Crear_ExponeCatorceLogrosEnCuatroSeccionesYOrdenEstable() {
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(),
            CrearMotivacion());

        Assert.Equal(14, presentacion.Logros.Count);
        Assert.Equal(4, presentacion.Secciones.Count);
        Assert.Equal(new[] { 2, 3, 5, 4 },
            presentacion.Secciones.Select(seccion => seccion.Logros.Count));
        Assert.Equal(
            new[] {
                SeccionLogroPresentable.PrimerosPasos,
                SeccionLogroPresentable.Progreso,
                SeccionLogroPresentable.Evaluaciones,
                SeccionLogroPresentable.TemasYGrados
            },
            presentacion.Secciones.Select(seccion => seccion.Seccion));
        Assert.Equal(
            new[] {
                "PRIMEROS PASOS",
                "PROGRESO",
                "EVALUACIONES",
                "TEMAS Y GRADOS"
            },
            presentacion.Secciones.Select(seccion => seccion.Titulo));
        Assert.Equal(
            new[] { 1, 2, 3, 4 },
            presentacion.Secciones.Select(seccion => seccion.Orden));
        Assert.Equal(
            Enumerable.Range(1, 14),
            presentacion.Logros.Select(logro => logro.Orden));
        Assert.Equal(
            new[] {
                CatalogoLogrosService.PrimeraPracticaVinculadaId,
                CatalogoLogrosService.PrimeraPracticaRealizadaId,
                CatalogoLogrosService.CincoPracticasRealizadasId,
                CatalogoLogrosService.DiezPracticasRealizadasId,
                CatalogoLogrosService.VeinticincoPracticasRealizadasId,
                CatalogoLogrosService.PrimeraEvaluacionAprobadaId,
                CatalogoLogrosService.PrimeraEvaluacionPerfectaId,
                CatalogoLogrosService.CincoPracticasAprobadasId,
                CatalogoLogrosService.DiezPracticasAprobadasId,
                CatalogoLogrosService.CincoPracticasPerfectasId,
                CatalogoLogrosService.PrimerTemaCompletadoId,
                CatalogoLogrosService.PrimerGradoCompletadoId,
                CatalogoLogrosService.GradoFundamentosCompletoId,
                CatalogoLogrosService.GradoJuniorCompletoId
            },
            presentacion.Logros.Select(logro => logro.Id));
        Assert.Equal(
            new[] {
                "Primera práctica vinculada",
                "Primera práctica completada",
                "Cinco prácticas completadas",
                "Diez prácticas completadas",
                "Veinticinco prácticas completadas",
                "Primera evaluación aprobada",
                "Primera calificación perfecta",
                "Cinco evaluaciones aprobadas",
                "Diez evaluaciones aprobadas",
                "Cinco resultados perfectos",
                "Primer tema completado",
                "Primer grado completado",
                "Fundamentos de C++ completados",
                "C++ Junior completado"
            },
            presentacion.Logros.Select(logro => logro.Nombre));
    }

    [Theory]
    [InlineData(
        "logro:evaluacion:primera-aprobada",
        "Aprueba la evaluación de una práctica por primera vez.")]
    [InlineData(
        "logro:evaluacion:primera-perfecta",
        "Obtén 100 puntos en una práctica.")]
    [InlineData(
        "logro:practicas:realizadas:5",
        "Completa cinco prácticas distintas.")]
    [InlineData(
        "logro:practicas:realizadas:10",
        "Completa diez prácticas distintas.")]
    [InlineData(
        "logro:practicas:realizadas:25",
        "Completa veinticinco prácticas distintas.")]
    [InlineData(
        "logro:evaluaciones:aprobadas:5",
        "Aprueba evaluaciones en cinco prácticas distintas.")]
    [InlineData(
        "logro:evaluaciones:aprobadas:10",
        "Aprueba evaluaciones en diez prácticas distintas.")]
    [InlineData(
        "logro:evaluaciones:perfectas:5",
        "Obtén 100 puntos en cinco prácticas distintas.")]
    public void Crear_UsaDescripcionVisibleEsperada(
        string logroId,
        string descripcion) {
        PresentacionLogro logro = new PresentadorLogrosService()
            .Crear(CrearResumenInicio(), CrearMotivacion())
            .Logros
            .Single(item => item.Id == logroId);

        Assert.Equal(descripcion, logro.Descripcion);
    }

    [Fact]
    public void Crear_ConDatosConfiables_ExponeSoloCincoProgresosParciales() {
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(realizadas: 3, aprobadas: 4),
            CrearMotivacion());
        PresentacionLogro[] conProgreso = presentacion.Logros
            .Where(logro => logro.ProgresoActual.HasValue)
            .ToArray();

        Assert.Equal(5, conProgreso.Length);
        AssertProgreso(
            presentacion,
            CatalogoLogrosService.CincoPracticasRealizadasId,
            3,
            5,
            "3 / 5 prácticas");
        AssertProgreso(
            presentacion,
            CatalogoLogrosService.DiezPracticasRealizadasId,
            3,
            10,
            "3 / 10 prácticas");
        AssertProgreso(
            presentacion,
            CatalogoLogrosService.VeinticincoPracticasRealizadasId,
            3,
            25,
            "3 / 25 prácticas");
        AssertProgreso(
            presentacion,
            CatalogoLogrosService.CincoPracticasAprobadasId,
            4,
            5,
            "4 / 5 prÃ¡cticas aprobadas");
        AssertProgreso(
            presentacion,
            CatalogoLogrosService.DiezPracticasAprobadasId,
            4,
            10,
            "4 / 10 prÃ¡cticas aprobadas");
        Assert.All(
            presentacion.Logros.Except(conProgreso),
            logro => {
                Assert.Null(logro.ProgresoActual);
                Assert.Null(logro.ProgresoObjetivo);
                Assert.Empty(logro.TextoProgreso);
            });
    }

    [Fact]
    public void Crear_ConFuentesParciales_NoExponeProgresoIncierto() {
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(
                realizadas: 3,
                aprobadas: 4,
                estadoProgreso:
                    EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
                estadoHistorial:
                    EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada),
            CrearMotivacion());

        Assert.All(presentacion.Logros, logro => {
            Assert.Null(logro.ProgresoActual);
            Assert.Null(logro.ProgresoObjetivo);
            Assert.Empty(logro.TextoProgreso);
        });
    }

    [Fact]
    public void Crear_LogroDesbloqueadoFuerzaProgresoCompletoSinContradecirEstado() {
        ResumenMotivacion motivacion = CrearMotivacion(logros: new[] {
            CrearLogro(CatalogoLogrosService.CincoPracticasRealizadasId)
        });
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(realizadas: 4),
            motivacion);

        AssertProgreso(
            presentacion,
            CatalogoLogrosService.CincoPracticasRealizadasId,
            5,
            5,
            "5 / 5 prácticas");
    }

    [Fact]
    public void Crear_LogroActual_ConvierteYPresentaFechaSimple() {
        ResumenMotivacion motivacion = CrearMotivacion(logros: new[] {
            CrearLogro(CatalogoLogrosService.PrimeraPracticaRealizadaId)
        });

        PresentacionLogro logro = new PresentadorLogrosService()
            .Crear(CrearResumenInicio(), motivacion)
            .Logros
            .Single(item => item.Id ==
                CatalogoLogrosService.PrimeraPracticaRealizadaId);

        Assert.Equal(EstadoLogroPresentable.Desbloqueado, logro.Estado);
        Assert.Equal(new DateOnly(2026, 8, 9), logro.FechaReconocimientoLocal);
        Assert.Equal("Reconocido el 9 ago 2026", logro.TextoFecha);
    }

    [Fact]
    public void Crear_LogroImportado_SeDesbloqueaSinFingirFechaHistorica() {
        ResumenMotivacion motivacion = CrearMotivacion(logros: new[] {
            CrearLogro(
                CatalogoLogrosService.PrimeraPracticaRealizadaId,
                esImportado: true)
        });

        PresentacionLogro logro = new PresentadorLogrosService()
            .Crear(CrearResumenInicio(), motivacion)
            .Logros
            .Single(item => item.Id ==
                CatalogoLogrosService.PrimeraPracticaRealizadaId);

        Assert.Equal(EstadoLogroPresentable.Desbloqueado, logro.Estado);
        Assert.True(logro.EsImportado);
        Assert.Null(logro.FechaReconocimientoLocal);
        Assert.Empty(logro.TextoFecha);
    }

    [Fact]
    public void Crear_IdDesconocido_NoAumentaContadorNiLista() {
        ResumenMotivacion motivacion = CrearMotivacion(logros: new[] {
            CrearLogro(CatalogoLogrosService.PrimeraPracticaRealizadaId),
            CrearLogro("logro:futuro:no-conocido")
        });

        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(),
            motivacion);

        Assert.Equal(1, presentacion.LogrosDesbloqueados);
        Assert.Equal(14, presentacion.TotalLogros);
        Assert.Equal("1 de 14 logros desbloqueados", presentacion.TextoResumen);
        Assert.DoesNotContain(
            presentacion.Logros,
            logro => logro.Id == "logro:futuro:no-conocido");
    }

    [Fact]
    public void Crear_ZonaInvalida_OcultaFechaPeroConservaLogros() {
        ResumenMotivacion motivacion = CrearMotivacion(
            logros: new[] {
                CrearLogro(CatalogoLogrosService.PrimeraPracticaRealizadaId)
            },
            zona: "EndForge/Zona-Inexistente",
            advertencias: new[] {
                AdvertenciaMotivacion.ZonaHorariaNoDisponible
            });
        PresentadorLogrosService presentador = new();

        PresentacionLogros logros = presentador.Crear(
            CrearResumenInicio(),
            motivacion);
        PresentacionMotivacionInicio inicio =
            presentador.CrearPresentacionInicio(motivacion);

        Assert.Equal(1, logros.LogrosDesbloqueados);
        Assert.Empty(logros.Logros.Single(item => item.Estado ==
            EstadoLogroPresentable.Desbloqueado).TextoFecha);
        Assert.Equal(
            EstadoMetricaMotivacionalInicio.ZonaHorariaNoDisponible,
            inicio.Racha.Estado);
        Assert.Equal("—", inicio.Racha.TextoValor);
        Assert.Equal("1 / 14", inicio.Logros.TextoValor);
    }

    [Theory]
    [InlineData(0, 0, "0 días", "Empieza una nueva racha")]
    [InlineData(1, 1, "1 día de estudio", "Día de estudio registrado")]
    [InlineData(4, 8, "4 días de racha", "Mejor racha: 8 días")]
    [InlineData(0, 8, "Empieza una nueva racha", "Mejor racha: 8 días")]
    public void CrearPresentacionInicio_UsaTextosDeRachaEsperados(
        int actual,
        int mejor,
        string valor,
        string detalle) {
        ResumenMotivacion motivacion = CrearMotivacion(
            racha: new ResumenRacha(actual, mejor, null));

        RachaInicioPresentable racha = new PresentadorLogrosService()
            .CrearPresentacionInicio(motivacion)
            .Racha;

        Assert.Equal(valor, racha.TextoValor);
        Assert.Equal(detalle, racha.TextoDetalle);
        Assert.Equal(actual, racha.RachaActual);
        Assert.Equal(mejor, racha.MejorRacha);
        Assert.Equal(
            actual == 0 && mejor == 0
                ? EstadoMetricaMotivacionalInicio.SinDatos
                : EstadoMetricaMotivacionalInicio.Disponible,
            racha.Estado);
    }

    [Theory]
    [InlineData(
        EstadoDisponibilidadMotivacion.NoDisponible,
        EstadoMetricaMotivacionalInicio.NoDisponible,
        "Temporalmente no disponible")]
    [InlineData(
        EstadoDisponibilidadMotivacion.VersionIncompatible,
        EstadoMetricaMotivacionalInicio.VersionIncompatible,
        "Disponible con una versión compatible")]
    public void CrearPresentacionInicio_EstadoNoDisponible_NoInventaCeros(
        EstadoDisponibilidadMotivacion estado,
        EstadoMetricaMotivacionalInicio estadoEsperado,
        string detalle) {
        PresentacionMotivacionInicio presentacion =
            new PresentadorLogrosService().CrearPresentacionInicio(
                CrearMotivacion(estado));

        Assert.Equal(estadoEsperado, presentacion.Racha.Estado);
        Assert.Equal(estadoEsperado, presentacion.Logros.Estado);
        Assert.Null(presentacion.Racha.RachaActual);
        Assert.Null(presentacion.Logros.LogrosDesbloqueados);
        Assert.Equal("—", presentacion.Racha.TextoValor);
        Assert.Equal("—", presentacion.Logros.TextoValor);
        Assert.Equal(detalle, presentacion.Racha.TextoDetalle);
    }

    [Fact]
    public void Crear_ColeccionesExpuestasSonDeSoloLectura() {
        PresentacionLogros presentacion = new PresentadorLogrosService().Crear(
            CrearResumenInicio(),
            CrearMotivacion());
        IList<PresentacionLogro> logros = Assert.IsAssignableFrom<
            IList<PresentacionLogro>>(presentacion.Logros);
        IList<PresentacionSeccionLogros> secciones = Assert.IsAssignableFrom<
            IList<PresentacionSeccionLogros>>(presentacion.Secciones);
        IList<PresentacionLogro> logrosSeccion = Assert.IsAssignableFrom<
            IList<PresentacionLogro>>(presentacion.Secciones[0].Logros);

        Assert.Throws<NotSupportedException>(() =>
            logros.Add(presentacion.Logros[0]));
        Assert.Throws<NotSupportedException>(() =>
            secciones.Add(presentacion.Secciones[0]));
        Assert.Throws<NotSupportedException>(() =>
            logrosSeccion.Add(presentacion.Logros[0]));
    }

    private static void AssertProgreso(
        PresentacionLogros presentacion,
        string logroId,
        int actual,
        int objetivo,
        string texto) {
        PresentacionLogro logro = presentacion.Logros.Single(item =>
            item.Id == logroId);
        Assert.Equal(actual, logro.ProgresoActual);
        Assert.Equal(objetivo, logro.ProgresoObjetivo);
        Assert.Equal(texto, logro.TextoProgreso);
    }

    private static LogroDesbloqueado CrearLogro(
        string id,
        bool esImportado = false) {
        return new LogroDesbloqueado {
            LogroId = id,
            FechaReconocimientoUtc = FechaReconocimiento,
            EsImportado = esImportado
        };
    }

    private static ResumenMotivacion CrearMotivacion(
        EstadoDisponibilidadMotivacion estado =
            EstadoDisponibilidadMotivacion.Disponible,
        ResumenRacha? racha = null,
        IReadOnlyList<LogroDesbloqueado>? logros = null,
        string zona = "UTC",
        IReadOnlyList<AdvertenciaMotivacion>? advertencias = null) {
        return new ResumenMotivacion(
            estado,
            estado is EstadoDisponibilidadMotivacion.NoDisponible or
                EstadoDisponibilidadMotivacion.VersionIncompatible
                ? null
                : 0,
            estado is EstadoDisponibilidadMotivacion.NoDisponible or
                EstadoDisponibilidadMotivacion.VersionIncompatible
                ? null
                : new CalculadoraNivelService().Calcular(0),
            zona,
            null,
            advertencias ?? Array.Empty<AdvertenciaMotivacion>(),
            null) {
            Racha = racha ?? new ResumenRacha(0, 0, null),
            LogrosDesbloqueados = logros ?? Array.Empty<LogroDesbloqueado>()
        };
    }

    private static ResumenInicio CrearResumenInicio(
        int? realizadas = 0,
        int? aprobadas = 0,
        EstadoFuenteDatosAprendizaje estadoProgreso =
            EstadoFuenteDatosAprendizaje.Disponible,
        EstadoFuenteDatosAprendizaje estadoHistorial =
            EstadoFuenteDatosAprendizaje.Disponible) {
        EstadoFuenteAprendizaje fuenteProgreso = new(
            estadoProgreso,
            "Prueba",
            0,
            0,
            null);
        EstadoFuenteAprendizaje fuenteHistorial = new(
            estadoHistorial,
            "Prueba",
            0,
            0,
            null);
        return new ResumenInicio(
            EstadoDisponibilidadDatos.DatosDisponibles,
            new ResumenProgresoGlobal(
                60,
                realizadas,
                0,
                realizadas.HasValue ? 60 - realizadas.Value : null,
                realizadas.HasValue
                    ? (int)Math.Round(realizadas.Value * 100m / 60m)
                    : null,
                9,
                0,
                2,
                0,
                Array.Empty<ResumenProgresoGrado>()),
            new ResumenEvaluacionesGlobal(
                aprobadas,
                aprobadas,
                aprobadas,
                null,
                null),
            null,
            new ContinuacionAprendizaje(
                EstadoContinuacionAprendizaje.SinContenidoDisponible,
                null,
                null,
                EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                null,
                false),
            new RecomendacionAprendizaje(
                EstadoRecomendacionAprendizaje.SinContenidoDisponible,
                null,
                null,
                false),
            fuenteProgreso,
            fuenteHistorial);
    }
}
