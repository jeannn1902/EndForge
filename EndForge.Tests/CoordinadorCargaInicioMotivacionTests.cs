using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CoordinadorCargaInicioMotivacionTests {
    [Fact]
    public void RecargaProgramada_CierreSolicitadoAntesDelCallback_NoActualizaInicio() {
        bool cierreSolicitado = false;
        int actualizaciones = 0;
        Action callbackProgramado = () => {
            if (frmPrincipal.PuedeEjecutarRecargaInicioProgramada(
                    cierreSolicitado,
                    puedeActualizarInterfazInicio: true)) {
                actualizaciones++;
            }
        };

        Assert.True(frmPrincipal.PuedeEjecutarRecargaInicioProgramada(
            cierreSolicitado,
            puedeActualizarInterfazInicio: true));

        cierreSolicitado = true;
        callbackProgramado();

        Assert.Equal(0, actualizaciones);
    }

    [Fact]
    public void ReconciliacionParcial_ConservaXpPersistidoYPropagaAdvertencia() {
        IOException error = new("historial-evaluaciones.json bloqueado");
        ResumenMotivacion resumenParcial = CrearResumenMotivacion(
            EstadoDisponibilidadMotivacion.Disponible,
            xp: 10);
        ResultadoProcesamientoMotivacion resultado = new(
            EstadoProcesamientoMotivacion.Aplicada,
            10,
            10,
            1,
            1,
            false,
            new[] { "practica:variables-datos-personales:vinculada" },
            resumenParcial,
            error);

        ResumenMotivacion preparado =
            frmPrincipal.PrepararResumenMotivacionInicio(resultado);

        Assert.Equal(
            EstadoDisponibilidadMotivacion.Disponible,
            preparado.Estado);
        Assert.Equal(10, preparado.XpTotal);
        Assert.NotNull(preparado.Nivel);
        Assert.Same(error, preparado.Error);
    }

    [Fact]
    public async Task Recarga_CargaMotivacionUnaVezYLaIncluyeEnPresentacion() {
        int cargasMotivacion = 0;
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            _ => {
                cargasMotivacion++;
                return Task.FromResult<ResumenMotivacion?>(
                    CrearMotivacionDisponible());
            });

        ResultadoCargaInicio resultado = await coordinador.RecargarAsync();

        Assert.Equal(1, cargasMotivacion);
        Assert.Equal(EstadoResultadoCargaInicio.Completada, resultado.Estado);
        Assert.Equal("Nivel 2", resultado.Presentacion?.Nivel.TextoNivel);
        Assert.Equal("150 XP", resultado.Presentacion?.Nivel.TextoXpTotal);
        Assert.Null(resultado.AdvertenciaMotivacion);
    }

    [Fact]
    public async Task ErrorMotivacional_NoOcultaDatosAcademicos() {
        IOException error = new("Motivación bloqueada");
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            _ => Task.FromException<ResumenMotivacion?>(error));

        ResultadoCargaInicio resultado = await coordinador.RecargarAsync();

        Assert.Equal(EstadoResultadoCargaInicio.Completada, resultado.Estado);
        Assert.Equal(
            EstadoNivelInicio.NoDisponible,
            resultado.Presentacion?.Nivel.Estado);
        Assert.Equal(
            "0 de 60",
            resultado.Presentacion?.Progreso.PracticasRealizadas.Texto);
        Assert.Same(error, resultado.AdvertenciaMotivacion);
    }

    [Fact]
    public async Task AdvertenciaConXpPersistido_ConservaNivelYSePropaga() {
        IOException error = new("historial-evaluaciones.json bloqueado");
        ResumenMotivacion motivacion = CrearResumenMotivacion(
            EstadoDisponibilidadMotivacion.Disponible,
            xp: 150) with {
            Error = error
        };
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            _ => Task.FromResult<ResumenMotivacion?>(motivacion));

        ResultadoCargaInicio resultado = await coordinador.RecargarAsync();

        Assert.Equal(EstadoResultadoCargaInicio.Completada, resultado.Estado);
        Assert.Equal(
            EstadoNivelInicio.Disponible,
            resultado.Presentacion?.Nivel.Estado);
        Assert.Equal("Nivel 2", resultado.Presentacion?.Nivel.TextoNivel);
        Assert.Equal("150 XP", resultado.Presentacion?.Nivel.TextoXpTotal);
        Assert.Same(error, resultado.AdvertenciaMotivacion);
    }

    [Fact]
    public async Task VersionIncompatible_NoFallaCargaDeInicio() {
        ResumenMotivacion incompatible = new(
            EstadoDisponibilidadMotivacion.VersionIncompatible,
            null,
            null,
            string.Empty,
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            null);
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            _ => Task.FromResult<ResumenMotivacion?>(incompatible));

        ResultadoCargaInicio resultado = await coordinador.RecargarAsync();

        Assert.Equal(EstadoResultadoCargaInicio.Completada, resultado.Estado);
        Assert.Equal(
            EstadoNivelInicio.VersionIncompatible,
            resultado.Presentacion?.Nivel.Estado);
        Assert.Equal(4, resultado.Presentacion?.Metricas.Count);
    }

    [Fact]
    public async Task CancelacionDuranteMotivacion_NoPublicaPresentacionParcial() {
        TaskCompletionSource<bool> iniciada = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            async cancellationToken => {
                iniciada.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return CrearMotivacionDisponible();
            });
        using CancellationTokenSource cancelacion = new();

        Task<ResultadoCargaInicio> carga = coordinador.RecargarAsync(
            cancelacion.Token);
        await iniciada.Task;
        cancelacion.Cancel();
        ResultadoCargaInicio resultado = await carga;

        Assert.Equal(EstadoResultadoCargaInicio.Cancelada, resultado.Estado);
        Assert.Null(resultado.Presentacion);
    }

    private static CoordinadorCargaInicio CrearCoordinador(
        Func<CancellationToken, Task<ResumenMotivacion?>> cargarMotivacion) {
        return new CoordinadorCargaInicio(
            _ => Task.FromResult(CrearResumenAcademico()),
            cargarMotivacion,
            new PresentadorInicioService(new TimeProviderFijo()));
    }

    private static ResumenMotivacion CrearMotivacionDisponible() {
        return CrearResumenMotivacion(
            EstadoDisponibilidadMotivacion.Disponible,
            150);
    }

    private static ResumenMotivacion CrearResumenMotivacion(
        EstadoDisponibilidadMotivacion estado,
        long xp) {
        return new ResumenMotivacion(
            estado,
            xp,
            new CalculadoraNivelService().Calcular(xp),
            "America/Mexico_City",
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            null);
    }

    private static ResumenInicio CrearResumenAcademico() {
        EstadoFuenteAprendizaje fuente = new(
            EstadoFuenteDatosAprendizaje.SinDatos,
            "Prueba",
            0,
            0,
            null);
        return new ResumenInicio(
            EstadoDisponibilidadDatos.SinActividad,
            new ResumenProgresoGlobal(
                60,
                0,
                0,
                60,
                0,
                9,
                0,
                2,
                0,
                Array.Empty<ResumenProgresoGrado>()),
            new ResumenEvaluacionesGlobal(0, 0, 0, null, null),
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
            fuente,
            fuente);
    }

    private sealed class TimeProviderFijo : TimeProvider {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow() {
            return new DateTimeOffset(
                2026,
                7,
                31,
                10,
                0,
                0,
                TimeSpan.Zero);
        }
    }
}
