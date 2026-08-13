using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CoordinadorCargaInicioTests {
    [Fact]
    public async Task Recarga_MantieneEstadoDeCargaHastaCompletar() {
        TaskCompletionSource<bool> inicio = CrearSenal();
        TaskCompletionSource<bool> continuar = CrearSenal();
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            async cancellationToken => {
                inicio.TrySetResult(true);
                await continuar.Task.WaitAsync(cancellationToken);
                return CrearResumen(10);
            });

        Task<ResultadoCargaInicio> operacion = coordinador.RecargarAsync();
        await inicio.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(coordinador.CargaEnCurso);

        continuar.TrySetResult(true);
        ResultadoCargaInicio resultado =
            await operacion.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(
            EstadoResultadoCargaInicio.Completada,
            resultado.Estado);
        Assert.NotNull(resultado.Presentacion);
        Assert.True(coordinador.PuedeAplicar(resultado));
        Assert.False(coordinador.CargaEnCurso);
    }

    [Fact]
    public async Task RecargasSolapadas_EjecutanComoMaximoUnaCargaFisica() {
        object sincronizacion = new();
        int llamadas = 0;
        int activas = 0;
        int maximoActivas = 0;
        TaskCompletionSource<bool> primeraIniciada = CrearSenal();
        TaskCompletionSource<bool> segundaIniciada = CrearSenal();
        TaskCompletionSource<bool> liberarPrimera = CrearSenal();
        TaskCompletionSource<bool> liberarSegunda = CrearSenal();
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            async _ => {
                int llamada;

                lock (sincronizacion) {
                    llamada = ++llamadas;
                    activas++;
                    maximoActivas = Math.Max(maximoActivas, activas);
                }

                try {
                    if (llamada == 1) {
                        primeraIniciada.TrySetResult(true);
                        await liberarPrimera.Task;
                        return CrearResumen(5);
                    }

                    segundaIniciada.TrySetResult(true);
                    await liberarSegunda.Task;
                    return CrearResumen(15);
                } finally {
                    lock (sincronizacion) {
                        activas--;
                    }
                }
            });

        Task<ResultadoCargaInicio> primera = coordinador.RecargarAsync();
        await primeraIniciada.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Task<ResultadoCargaInicio> segunda = coordinador.RecargarAsync();

        Assert.False(segundaIniciada.Task.IsCompleted);

        liberarPrimera.TrySetResult(true);
        ResultadoCargaInicio resultadoPrimero =
            await primera.WaitAsync(TimeSpan.FromSeconds(5));
        await segundaIniciada.Task.WaitAsync(TimeSpan.FromSeconds(5));
        liberarSegunda.TrySetResult(true);
        ResultadoCargaInicio resultadoSegundo =
            await segunda.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(1, maximoActivas);
        Assert.Equal(
            EstadoResultadoCargaInicio.Obsoleta,
            resultadoPrimero.Estado);
        Assert.False(coordinador.PuedeAplicar(resultadoPrimero));
        Assert.Equal(
            EstadoResultadoCargaInicio.Completada,
            resultadoSegundo.Estado);
        Assert.True(coordinador.PuedeAplicar(resultadoSegundo));
        Assert.Equal(
            "15 %",
            resultadoSegundo.Presentacion?.Progreso.Porcentaje.Texto);
    }

    [Fact]
    public async Task TresReintentos_OmitenLaSolicitudIntermediaCancelada() {
        int llamadas = 0;
        TaskCompletionSource<bool> primeraIniciada = CrearSenal();
        TaskCompletionSource<bool> ultimaIniciada = CrearSenal();
        TaskCompletionSource<bool> liberarPrimera = CrearSenal();
        TaskCompletionSource<bool> liberarUltima = CrearSenal();
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            async _ => {
                int llamada = Interlocked.Increment(ref llamadas);

                if (llamada == 1) {
                    primeraIniciada.TrySetResult(true);
                    await liberarPrimera.Task;
                    return CrearResumen(10);
                }

                ultimaIniciada.TrySetResult(true);
                await liberarUltima.Task;
                return CrearResumen(30);
            });

        Task<ResultadoCargaInicio> primera = coordinador.RecargarAsync();
        await primeraIniciada.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Task<ResultadoCargaInicio> intermedia = coordinador.RecargarAsync();
        Task<ResultadoCargaInicio> ultima = coordinador.RecargarAsync();

        liberarPrimera.TrySetResult(true);
        await ultimaIniciada.Task.WaitAsync(TimeSpan.FromSeconds(5));
        liberarUltima.TrySetResult(true);

        ResultadoCargaInicio[] resultados = await Task.WhenAll(
            primera,
            intermedia,
            ultima).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(2, llamadas);
        Assert.Equal(
            EstadoResultadoCargaInicio.Obsoleta,
            resultados[0].Estado);
        Assert.Equal(
            EstadoResultadoCargaInicio.Obsoleta,
            resultados[1].Estado);
        Assert.Equal(
            EstadoResultadoCargaInicio.Completada,
            resultados[2].Estado);
        Assert.True(coordinador.PuedeAplicar(resultados[2]));
    }

    [Fact]
    public async Task CancelacionExterna_NoSeConvierteEnErrorRecuperable() {
        TaskCompletionSource<bool> inicio = CrearSenal();
        using CancellationTokenSource cancelacion = new();
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            async cancellationToken => {
                inicio.TrySetResult(true);
                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    cancellationToken);
                return CrearResumen(0);
            });

        Task<ResultadoCargaInicio> operacion =
            coordinador.RecargarAsync(cancelacion.Token);
        await inicio.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancelacion.Cancel();

        ResultadoCargaInicio resultado =
            await operacion.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(
            EstadoResultadoCargaInicio.Cancelada,
            resultado.Estado);
        Assert.Null(resultado.Error);
        Assert.False(coordinador.PuedeAplicar(resultado));
    }

    [Fact]
    public async Task CierreDuranteCarga_ImpideAplicarSnapshotPosterior() {
        TaskCompletionSource<bool> inicio = CrearSenal();
        TaskCompletionSource<bool> continuar = CrearSenal();
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            async _ => {
                inicio.TrySetResult(true);
                await continuar.Task;
                return CrearResumen(40);
            });

        Task<ResultadoCargaInicio> operacion = coordinador.RecargarAsync();
        await inicio.Task.WaitAsync(TimeSpan.FromSeconds(5));
        coordinador.Cerrar();
        coordinador.Cerrar();
        continuar.TrySetResult(true);

        ResultadoCargaInicio resultado =
            await operacion.WaitAsync(TimeSpan.FromSeconds(5));
        ResultadoCargaInicio recargaTrasCierre =
            await coordinador.RecargarAsync();

        Assert.Equal(
            EstadoResultadoCargaInicio.Cerrada,
            resultado.Estado);
        Assert.Equal(
            EstadoResultadoCargaInicio.Cerrada,
            recargaTrasCierre.Estado);
        Assert.False(coordinador.PuedeAplicar(resultado));
        Assert.False(coordinador.CargaEnCurso);
    }

    [Fact]
    public async Task ErrorNoCritico_DevuelveEstadoRecuperableSinPresentacionFalsa() {
        IOException error = new("Ruta técnica privada");
        CoordinadorCargaInicio coordinador = CrearCoordinador(
            _ => Task.FromException<ResumenInicio>(error));

        ResultadoCargaInicio resultado =
            await coordinador.RecargarAsync();

        Assert.Equal(
            EstadoResultadoCargaInicio.ErrorRecuperable,
            resultado.Estado);
        Assert.Same(error, resultado.Error);
        Assert.Null(resultado.Presentacion);
        Assert.True(coordinador.PuedeAplicar(resultado));
    }

    [Fact]
    public void EstadosPresentables_DistinguenCargaYErrorRecuperable() {
        EstadoCargaInicioPresentable carga =
            PresentadorInicioService.CrearEstadoCargando();
        EstadoCargaInicioPresentable error =
            PresentadorInicioService.CrearEstadoErrorRecuperable();

        Assert.Equal(EstadoCargaInicio.Cargando, carga.Estado);
        Assert.True(carga.MostrarIndicador);
        Assert.False(carga.MostrarReintentar);
        Assert.Equal(EstadoCargaInicio.ErrorRecuperable, error.Estado);
        Assert.False(error.MostrarIndicador);
        Assert.True(error.MostrarReintentar);
    }

    private static CoordinadorCargaInicio CrearCoordinador(
        Func<CancellationToken, Task<ResumenInicio>> cargar) {
        return new CoordinadorCargaInicio(
            cargar,
            new PresentadorInicioService(
                new TimeProviderFijo(
                    new DateTimeOffset(
                        2026,
                        7,
                        30,
                        10,
                        0,
                        0,
                        TimeSpan.Zero))));
    }

    private static ResumenInicio CrearResumen(int porcentaje) {
        ReferenciaPracticaAprendizaje practica =
            new(
                GradosService.GradoFundamentosId,
                1,
                "Fundamentos de C++",
                "variables",
                1,
                "Variables",
                "variables-datos-personales",
                1,
                "Datos personales") {
                Dificultad = "Inicial",
                DuracionEstimada = "25 min"
            };
        RecomendacionAprendizaje recomendacion = new(
            EstadoRecomendacionAprendizaje.Disponible,
            practica,
            MotivoRecomendacionAprendizaje.PrimeraPractica,
            false);

        return new ResumenInicio(
            porcentaje == 0
                ? EstadoDisponibilidadDatos.SinActividad
                : EstadoDisponibilidadDatos.DatosDisponibles,
            new ResumenProgresoGlobal(
                60,
                porcentaje * 60 / 100,
                0,
                60 - porcentaje * 60 / 100,
                porcentaje,
                9,
                0,
                2,
                0,
                Array.Empty<ResumenProgresoGrado>()),
            new ResumenEvaluacionesGlobal(0, 0, 0, null, null),
            null,
            new ContinuacionAprendizaje(
                EstadoContinuacionAprendizaje.BasadaEnRecomendacion,
                practica,
                null,
                EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                null,
                false),
            recomendacion,
            new EstadoFuenteAprendizaje(
                porcentaje == 0
                    ? EstadoFuenteDatosAprendizaje.SinDatos
                    : EstadoFuenteDatosAprendizaje.Disponible,
                "Prueba",
                0,
                0,
                null),
            new EstadoFuenteAprendizaje(
                EstadoFuenteDatosAprendizaje.SinDatos,
                "Prueba",
                0,
                0,
                null));
    }

    private static TaskCompletionSource<bool> CrearSenal() {
        return new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
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
