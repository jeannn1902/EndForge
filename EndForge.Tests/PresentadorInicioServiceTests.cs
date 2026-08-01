using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class PresentadorInicioServiceTests {
    [Theory]
    [InlineData(4, "Buenas noches")]
    [InlineData(5, "Buenos días")]
    [InlineData(11, "Buenos días")]
    [InlineData(12, "Buenas tardes")]
    [InlineData(18, "Buenas tardes")]
    [InlineData(19, "Buenas noches")]
    public void Saludo_UsaHoraLocalDelTimeProvider(
        int hora,
        string esperado) {
        PresentadorInicioService presentador = new(
            new TimeProviderFijo(
                new DateTimeOffset(2026, 7, 30, hora, 0, 0, TimeSpan.Zero)));

        PresentacionInicio presentacion =
            presentador.Crear(CrearResumen());

        Assert.Equal(esperado, presentacion.Encabezado.Saludo);
    }

    [Fact]
    public void UsuarioNuevo_MuestraInicioHonestoSinEvaluaciones() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen());

        Assert.Equal(
            "Empieza tu ruta de aprendizaje",
            presentacion.Continuacion.Titulo);
        Assert.Equal(
            "Explorar práctica",
            presentacion.Continuacion.AccionPrincipal?.Texto);
        Assert.Equal("0 de 60", presentacion.Progreso.PracticasRealizadas.Texto);
        Assert.Equal(0, presentacion.Progreso.ValorBarra);
        Assert.All(
            presentacion.Metricas
                .Where(item =>
                    item.Tipo != TipoMetricaInicio.PracticasEnProgreso),
            item => Assert.Equal("Sin evaluaciones", item.Dato.Texto));
    }

    [Fact]
    public void UsuarioNuevo_DistingueLaSiguientePracticaSinCambiarLaRazon() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen());

        Assert.Equal(
            "SIGUIENTE PRÁCTICA",
            presentacion.Recomendacion?.TituloSeccion);
        Assert.Equal(
            "Empieza tu ruta de aprendizaje.",
            presentacion.Recomendacion?.Razon);
        Assert.Equal(
            "Ver siguiente práctica",
            presentacion.Recomendacion?.Accion.Texto);
        Assert.Contains(
            "siguiente práctica",
            presentacion.Recomendacion?.Accion.AccessibleName ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "siguiente práctica recomendada",
            presentacion.Recomendacion?.Accion.AccessibleDescription ??
                string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ContinuarYSiguientePractica_ConservanDestinosDistintos() {
        ReferenciaPracticaAprendizaje actual = CrearReferencia(
            "condicionales-mayor-de-edad",
            1,
            "Mayor de edad");
        ReferenciaPracticaAprendizaje siguiente = CrearReferencia(
            "condicionales-clasificar-numero",
            2,
            "Número positivo, negativo o cero");
        ContinuacionAprendizaje continuacion = new(
            EstadoContinuacionAprendizaje.PracticaEnProgreso,
            actual,
            null,
            EstadoRutaProyectoAprendizaje.SinRutaVinculada,
            null,
            false);
        RecomendacionAprendizaje recomendacion = new(
            EstadoRecomendacionAprendizaje.Disponible,
            siguiente,
            MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso,
            false);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosDisponibles,
                estadoProgreso: EstadoFuenteDatosAprendizaje.Disponible,
                enProgreso: 1,
                continuacion: continuacion,
                recomendacion: recomendacion));

        Assert.Equal(
            TipoAccionInicio.ContinuarPractica,
            presentacion.Continuacion.AccionPrincipal?.Tipo);
        Assert.Equal(
            actual.PracticaId,
            presentacion.Continuacion.AccionPrincipal?.Practica?.PracticaId);
        Assert.Equal(
            TipoAccionInicio.VerPractica,
            presentacion.Recomendacion?.Accion.Tipo);
        Assert.Equal(
            siguiente.PracticaId,
            presentacion.Recomendacion?.Accion.Practica?.PracticaId);
        Assert.NotEqual(
            presentacion.Continuacion.AccionPrincipal?.Practica?.PracticaId,
            presentacion.Recomendacion?.Accion.Practica?.PracticaId);
    }

    [Fact]
    public void RecomendacionDistinta_MencionaLaPracticaActualDinamicamente() {
        ReferenciaPracticaAprendizaje actual = CrearReferencia(
            "condicionales-mayor-de-edad",
            1,
            "Mayor de edad");
        ReferenciaPracticaAprendizaje siguiente = CrearReferencia(
            "condicionales-clasificar-numero",
            2,
            "Número positivo, negativo o cero");
        ContinuacionAprendizaje continuacion = new(
            EstadoContinuacionAprendizaje.PracticaEnProgreso,
            actual,
            null,
            EstadoRutaProyectoAprendizaje.SinRutaVinculada,
            null,
            false);
        RecomendacionAprendizaje recomendacion = new(
            EstadoRecomendacionAprendizaje.Disponible,
            siguiente,
            MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso,
            false);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosDisponibles,
                estadoProgreso: EstadoFuenteDatosAprendizaje.Disponible,
                enProgreso: 1,
                continuacion: continuacion,
                recomendacion: recomendacion));

        Assert.Equal(
            "Termina “Mayor de edad” y continúa con esta práctica.",
            presentacion.Recomendacion?.Razon);
        Assert.DoesNotContain(
            siguiente.NombrePractica,
            presentacion.Recomendacion?.Razon ?? string.Empty,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ProgresoParcial_NoPresentaCerosNiPorcentajeInventados() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
                estadoProgreso:
                    EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
                realizadas: 0,
                enProgreso: 0,
                porcentaje: null));

        Assert.Equal(
            "Información parcial",
            presentacion.Progreso.PracticasRealizadas.Texto);
        Assert.Equal(
            "Información parcial",
            presentacion.Progreso.Porcentaje.Texto);
        Assert.Null(presentacion.Progreso.ValorBarra);
        Assert.Equal(
            "Información parcial",
            ObtenerMetrica(
                presentacion,
                TipoMetricaInicio.PracticasEnProgreso).Dato.Texto);
    }

    [Fact]
    public void RutaCompleta_OfreceRutaYEstadisticas() {
        ContinuacionAprendizaje continuacion = new(
            EstadoContinuacionAprendizaje.RutaCompletada,
            null,
            null,
            EstadoRutaProyectoAprendizaje.SinRutaVinculada,
            null,
            false);
        RecomendacionAprendizaje recomendacion = new(
            EstadoRecomendacionAprendizaje.RutaCompletada,
            null,
            null,
            false);
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosDisponibles,
                estadoProgreso: EstadoFuenteDatosAprendizaje.Disponible,
                realizadas: 60,
                enProgreso: 0,
                porcentaje: 100,
                temasCompletados: 9,
                gradosCompletados: 2,
                continuacion: continuacion,
                recomendacion: recomendacion));

        Assert.Equal("Ruta completada", presentacion.Continuacion.Titulo);
        Assert.Equal(
            TipoAccionInicio.VerRutaAprendizaje,
            presentacion.Continuacion.AccionPrincipal?.Tipo);
        Assert.Contains(
            presentacion.Continuacion.AccionesSecundarias,
            item => item.Tipo == TipoAccionInicio.VerEstadisticas);
        Assert.Null(presentacion.Recomendacion);
        Assert.DoesNotContain(
            presentacion.Continuacion.AccionesSecundarias,
            item => item.Tipo == TipoAccionInicio.VerPractica);
    }

    [Fact]
    public void HistorialNoDisponible_MuestraNoDisponibleYConservaProgreso() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
                estadoProgreso: EstadoFuenteDatosAprendizaje.Disponible,
                estadoHistorial:
                    EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
                realizadas: 12,
                enProgreso: 2,
                porcentaje: 20,
                intentos: null,
                practicasEvaluadas: null,
                aprobadas: null,
                promedio: null,
                mejor: null));

        Assert.Equal(
            "12 de 60",
            presentacion.Progreso.PracticasRealizadas.Texto);
        Assert.Equal(
            "No disponible",
            ObtenerMetrica(
                presentacion,
                TipoMetricaInicio.EvaluacionesAprobadas).Dato.Texto);
        Assert.NotNull(presentacion.BandaDatos);
        Assert.Contains(
            "historial de evaluaciones",
            presentacion.BandaDatos!.Mensaje,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HistorialParcialSinValores_NoAfirmaQueNoHuboEvaluaciones() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
                estadoHistorial:
                    EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada,
                intentos: 0,
                practicasEvaluadas: 0,
                aprobadas: 0,
                promedio: null,
                mejor: null));

        Assert.All(
            presentacion.Metricas
                .Where(item =>
                    item.Tipo != TipoMetricaInicio.PracticasEnProgreso),
            item => Assert.Equal("Información parcial", item.Dato.Texto));
    }

    [Fact]
    public void ProgresoNoDisponible_NoPresentaCeroComoDatoReal() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosParcialmenteRecuperados,
                estadoProgreso:
                    EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
                estadoHistorial: EstadoFuenteDatosAprendizaje.Disponible,
                realizadas: null,
                enProgreso: null,
                porcentaje: null,
                temasCompletados: null,
                gradosCompletados: null,
                intentos: 3,
                practicasEvaluadas: 1,
                aprobadas: 1,
                promedio: 80,
                mejor: 80));

        Assert.Equal(
            "No disponible",
            presentacion.Progreso.PracticasRealizadas.Texto);
        Assert.Equal(
            "No disponible",
            ObtenerMetrica(
                presentacion,
                TipoMetricaInicio.PracticasEnProgreso).Dato.Texto);
        Assert.Null(presentacion.Progreso.ValorBarra);
    }

    [Fact]
    public void AmbasFuentesNoDisponibles_CreaBandaRecuperable() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado:
                    EstadoDisponibilidadDatos.DatosTemporalmenteNoDisponibles,
                estadoProgreso:
                    EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
                estadoHistorial:
                    EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible,
                realizadas: null,
                enProgreso: null,
                porcentaje: null,
                temasCompletados: null,
                gradosCompletados: null,
                intentos: null,
                practicasEvaluadas: null,
                aprobadas: null,
                promedio: null,
                mejor: null,
                continuacion: new ContinuacionAprendizaje(
                    EstadoContinuacionAprendizaje.DatosNoDisponibles,
                    null,
                    null,
                    EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                    null,
                    true),
                recomendacion: new RecomendacionAprendizaje(
                    EstadoRecomendacionAprendizaje.DatosNoDisponibles,
                    null,
                    null,
                    true)));

        Assert.NotNull(presentacion.BandaDatos);
        Assert.Equal(
            TipoAccionInicio.Reintentar,
            presentacion.BandaDatos!.AccionReintentar.Tipo);
        Assert.Equal(
            EstadoDatoInicio.NoDisponible,
            presentacion.Continuacion.Estado);
    }

    [Theory]
    [InlineData(
        MotivoRecomendacionAprendizaje.PrimeraPractica,
        "Empieza tu ruta de aprendizaje.")]
    [InlineData(
        MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso,
        "Siguiente práctica de tu tema actual.")]
    [InlineData(
        MotivoRecomendacionAprendizaje.SiguienteTema,
        "Avanza al siguiente tema.")]
    [InlineData(
        MotivoRecomendacionAprendizaje.SiguienteGrado,
        "Avanza al siguiente grado.")]
    [InlineData(
        MotivoRecomendacionAprendizaje.PendienteAnterior,
        "Retoma una práctica pendiente de tu ruta.")]
    [InlineData(
        MotivoRecomendacionAprendizaje.RetomarPracticaEnProgreso,
        "Continúa una práctica que ya comenzaste.")]
    public void Recomendacion_TraduceSoloElMotivoDemostrable(
        MotivoRecomendacionAprendizaje motivo,
        string razonEsperada) {
        RecomendacionAprendizaje recomendacion = new(
            EstadoRecomendacionAprendizaje.Disponible,
            CrearReferencia(),
            motivo,
            false);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(recomendacion: recomendacion));

        Assert.Equal(razonEsperada, presentacion.Recomendacion?.Razon);
    }

    [Fact]
    public void ContinuacionConRuta_ConservaRutaParaRevalidarlaAlActivar() {
        const string ruta = @"C:\Practicas\01_Datos";
        ContinuacionAprendizaje continuacion = new(
            EstadoContinuacionAprendizaje.PracticaEnProgreso,
            CrearReferencia(),
            ruta,
            EstadoRutaProyectoAprendizaje.Disponible,
            new DateTimeOffset(2026, 7, 30, 8, 0, 0, TimeSpan.Zero),
            false);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosDisponibles,
                estadoProgreso: EstadoFuenteDatosAprendizaje.Disponible,
                continuacion: continuacion));

        Assert.Equal(
            TipoAccionInicio.ContinuarPractica,
            presentacion.Continuacion.AccionPrincipal?.Tipo);
        Assert.Equal(
            ruta,
            presentacion.Continuacion.AccionPrincipal?.RutaProyecto);
        Assert.Equal(
            "Proyecto disponible.",
            presentacion.Continuacion.TextoRuta);
    }

    [Fact]
    public void ContinuacionConRutaAusente_ConservaPracticaYAdvierte() {
        ContinuacionAprendizaje continuacion = new(
            EstadoContinuacionAprendizaje.PracticaEnProgreso,
            CrearReferencia(),
            @"C:\Ruta\Ausente",
            EstadoRutaProyectoAprendizaje.NoDisponible,
            null,
            false);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(
                estado: EstadoDisponibilidadDatos.DatosDisponibles,
                estadoProgreso: EstadoFuenteDatosAprendizaje.Disponible,
                continuacion: continuacion));

        Assert.Equal(
            "La carpeta vinculada ya no está disponible.",
            presentacion.Continuacion.TextoRuta);
        Assert.NotNull(presentacion.Continuacion.Practica);
        Assert.Equal(
            TipoAccionInicio.ContinuarPractica,
            presentacion.Continuacion.AccionPrincipal?.Tipo);
    }

    [Theory]
    [InlineData(
        FuenteActividadAprendizaje.Progreso,
        "Actividad registrada en Variables.")]
    [InlineData(
        FuenteActividadAprendizaje.Ambas,
        "Actividad registrada en Variables.")]
    [InlineData(
        FuenteActividadAprendizaje.HistorialEvaluaciones,
        "Evaluación registrada en Variables.")]
    public void Actividad_UsaDescripcionDemostrable(
        FuenteActividadAprendizaje fuente,
        string textoEsperado) {
        ActividadAprendizaje actividad = new(
            new DateTimeOffset(2026, 7, 30, 7, 0, 0, TimeSpan.Zero),
            CrearReferencia(),
            fuente,
            true);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen(actividad: actividad));

        ActividadInicioPresentable item =
            Assert.Single(presentacion.Actividades);
        Assert.True(presentacion.Actividades.Count <= 3);
        Assert.Equal(textoEsperado, item.Texto);
        Assert.Equal("Hoy", item.TextoFecha);
        Assert.DoesNotContain(
            "abri",
            item.Texto,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Recomendacion_IncluyeDificultadDuracionYAccionAccesible() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumen());

        Assert.Equal("Inicial", presentacion.Recomendacion?.Dificultad);
        Assert.Equal("25 min", presentacion.Recomendacion?.DuracionEstimada);
        Assert.False(
            string.IsNullOrWhiteSpace(
                presentacion.Recomendacion?.Accion.AccessibleName));
        Assert.False(
            string.IsNullOrWhiteSpace(
                presentacion.Recomendacion?.Accion.AccessibleDescription));
    }

    [Fact]
    public void SnapshotReal_ConservaDificultadYDuracionDelCatalogo() {
        ResumenAprendizajeService servicio = new(
            () => new GradosService(new CursoService()).CargarGrados(null),
            () => new ResultadoCargaProgreso {
                Estado = EstadoCargaProgreso.ArchivoInexistente
            },
            () => new ResultadoCargaHistorialEvaluaciones {
                Estado = EstadoCargaHistorialEvaluaciones.ArchivoInexistente
            },
            new RecomendacionAprendizajeService(_ => false));

        ResumenInicio resumen = servicio.CrearResumen();

        Assert.False(
            string.IsNullOrWhiteSpace(
                resumen.Recomendacion.Practica?.Dificultad));
        Assert.False(
            string.IsNullOrWhiteSpace(
                resumen.Recomendacion.Practica?.DuracionEstimada));
    }

    [Fact]
    public void EstadosDeCarga_NoExponenElErrorTecnico() {
        EstadoCargaInicioPresentable cargando =
            PresentadorInicioService.CrearEstadoCargando();
        EstadoCargaInicioPresentable error =
            PresentadorInicioService.CrearEstadoErrorRecuperable();

        Assert.Equal(EstadoCargaInicio.Cargando, cargando.Estado);
        Assert.True(cargando.MostrarIndicador);
        Assert.Equal(EstadoCargaInicio.ErrorRecuperable, error.Estado);
        Assert.True(error.MostrarReintentar);
        Assert.DoesNotContain(
            @"C:\",
            error.Mensaje,
            StringComparison.OrdinalIgnoreCase);
    }

    private static MetricaInicioPresentable ObtenerMetrica(
        PresentacionInicio presentacion,
        TipoMetricaInicio tipo) {
        return Assert.Single(
            presentacion.Metricas,
            item => item.Tipo == tipo);
    }

    private static PresentadorInicioService CrearPresentador() {
        return new PresentadorInicioService(
            new TimeProviderFijo(
                new DateTimeOffset(
                    2026,
                    7,
                    30,
                    10,
                    0,
                    0,
                    TimeSpan.Zero)));
    }

    private static ResumenInicio CrearResumen(
        EstadoDisponibilidadDatos estado =
            EstadoDisponibilidadDatos.SinActividad,
        EstadoFuenteDatosAprendizaje estadoProgreso =
            EstadoFuenteDatosAprendizaje.SinDatos,
        EstadoFuenteDatosAprendizaje estadoHistorial =
            EstadoFuenteDatosAprendizaje.SinDatos,
        int? realizadas = 0,
        int? enProgreso = 0,
        int? porcentaje = 0,
        int? temasCompletados = 0,
        int? gradosCompletados = 0,
        long? intentos = 0,
        int? practicasEvaluadas = 0,
        int? aprobadas = 0,
        int? promedio = null,
        int? mejor = null,
        ContinuacionAprendizaje? continuacion = null,
        RecomendacionAprendizaje? recomendacion = null,
        ActividadAprendizaje? actividad = null) {
        ReferenciaPracticaAprendizaje practica = CrearReferencia();
        RecomendacionAprendizaje recomendacionFinal =
            recomendacion ?? new RecomendacionAprendizaje(
                EstadoRecomendacionAprendizaje.Disponible,
                practica,
                MotivoRecomendacionAprendizaje.PrimeraPractica,
                false);
        ContinuacionAprendizaje continuacionFinal =
            continuacion ?? new ContinuacionAprendizaje(
                EstadoContinuacionAprendizaje.BasadaEnRecomendacion,
                recomendacionFinal.Practica,
                null,
                EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                null,
                false);

        return new ResumenInicio(
            estado,
            new ResumenProgresoGlobal(
                60,
                realizadas,
                enProgreso,
                realizadas.HasValue && enProgreso.HasValue
                    ? 60 - realizadas.Value - enProgreso.Value
                    : null,
                porcentaje,
                9,
                temasCompletados,
                2,
                gradosCompletados,
                Array.Empty<ResumenProgresoGrado>()),
            new ResumenEvaluacionesGlobal(
                intentos,
                practicasEvaluadas,
                aprobadas,
                promedio,
                mejor),
            actividad,
            continuacionFinal,
            recomendacionFinal,
            new EstadoFuenteAprendizaje(
                estadoProgreso,
                estadoProgreso.ToString(),
                0,
                0,
                null),
            new EstadoFuenteAprendizaje(
                estadoHistorial,
                estadoHistorial.ToString(),
                0,
                0,
                null));
    }

    private static ReferenciaPracticaAprendizaje CrearReferencia(
        string practicaId = "variables-datos-personales",
        int numeroPractica = 1,
        string nombrePractica = "Datos personales") {
        return new ReferenciaPracticaAprendizaje(
            GradosService.GradoFundamentosId,
            1,
            "Fundamentos de C++",
            "variables",
            1,
            "Variables",
            practicaId,
            numeroPractica,
            nombrePractica) {
            Dificultad = "Inicial",
            DuracionEstimada = "25 min"
        };
    }

    private sealed class TimeProviderFijo : TimeProvider {
        private readonly DateTimeOffset ahora;

        public TimeProviderFijo(DateTimeOffset ahora) {
            this.ahora = ahora;
        }

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow() {
            return ahora;
        }
    }
}
