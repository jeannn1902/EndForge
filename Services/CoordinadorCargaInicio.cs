using EndForge.Models;

namespace EndForge.Services;

public sealed class CoordinadorCargaInicio {
    private readonly Func<CancellationToken, Task<ResumenInicio>> cargarResumen;
    private readonly Func<CancellationToken, Task<ResumenMotivacion?>>
        cargarMotivacion;
    private readonly PresentadorInicioService presentador;
    private readonly PresentadorLogrosService presentadorLogros;
    private readonly SemaphoreSlim cargaExclusiva = new(1, 1);
    private readonly object sincronizacion = new();
    private CancellationTokenSource? cancelacionSolicitudActual;
    private long generacionActual;
    private bool cargaEnCurso;
    private bool cerrado;

    public CoordinadorCargaInicio(
        ResumenAprendizajeService resumenService,
        PresentadorInicioService presentador)
        : this(
            (resumenService ??
                throw new ArgumentNullException(nameof(resumenService)))
                .CrearResumenAsync,
            CargarMotivacionNoDisponibleAsync,
            presentador) {
    }

    public CoordinadorCargaInicio(
        Func<CancellationToken, Task<ResumenInicio>> cargarResumen,
        PresentadorInicioService presentador)
        : this(
            cargarResumen,
            CargarMotivacionNoDisponibleAsync,
            presentador) {
    }

    public CoordinadorCargaInicio(
        Func<CancellationToken, Task<ResumenInicio>> cargarResumen,
        Func<CancellationToken, Task<ResumenMotivacion?>> cargarMotivacion,
        PresentadorInicioService presentador) {
        this.cargarResumen = cargarResumen ??
            throw new ArgumentNullException(nameof(cargarResumen));
        this.cargarMotivacion = cargarMotivacion ??
            throw new ArgumentNullException(nameof(cargarMotivacion));
        this.presentador = presentador ??
            throw new ArgumentNullException(nameof(presentador));
        presentadorLogros = new PresentadorLogrosService();
    }

    public CoordinadorCargaInicio(
        Func<CancellationToken, Task<ResumenInicio>> cargarResumen,
        Func<CancellationToken, Task<ResumenMotivacion?>> cargarMotivacion,
        PresentadorInicioService presentador,
        PresentadorLogrosService presentadorLogros) {
        this.cargarResumen = cargarResumen ??
            throw new ArgumentNullException(nameof(cargarResumen));
        this.cargarMotivacion = cargarMotivacion ??
            throw new ArgumentNullException(nameof(cargarMotivacion));
        this.presentador = presentador ??
            throw new ArgumentNullException(nameof(presentador));
        this.presentadorLogros = presentadorLogros ??
            throw new ArgumentNullException(nameof(presentadorLogros));
    }

    public bool CargaEnCurso {
        get {
            lock (sincronizacion) {
                return cargaEnCurso;
            }
        }
    }

    public Task<ResultadoCargaInicio> RecargarAsync(
        CancellationToken cancellationToken = default) {
        CancellationTokenSource? cancelacionAnterior;
        CancellationTokenSource cancelacionActual;
        long generacion;

        lock (sincronizacion) {
            if (cerrado) {
                return Task.FromResult(new ResultadoCargaInicio(
                    generacionActual,
                    EstadoResultadoCargaInicio.Cerrada,
                    null,
                    null));
            }

            generacion = ++generacionActual;
            cancelacionActual =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);
            cancelacionAnterior = cancelacionSolicitudActual;
            cancelacionSolicitudActual = cancelacionActual;
            cargaEnCurso = true;
        }

        IntentarCancelar(cancelacionAnterior);
        return EjecutarCargaAsync(generacion, cancelacionActual);
    }

    public bool PuedeAplicar(ResultadoCargaInicio resultado) {
        ArgumentNullException.ThrowIfNull(resultado);

        lock (sincronizacion) {
            return !cerrado &&
                resultado.Generacion == generacionActual &&
                resultado.Estado is (
                    EstadoResultadoCargaInicio.Completada or
                    EstadoResultadoCargaInicio.ErrorRecuperable);
        }
    }

    public void Cerrar() {
        CancellationTokenSource? cancelacion;

        lock (sincronizacion) {
            if (cerrado) {
                return;
            }

            cerrado = true;
            generacionActual++;
            cargaEnCurso = false;
            cancelacion = cancelacionSolicitudActual;
            cancelacionSolicitudActual = null;
        }

        IntentarCancelar(cancelacion);
    }

    private async Task<ResultadoCargaInicio> EjecutarCargaAsync(
        long generacion,
        CancellationTokenSource origenCancelacion) {
        bool exclusividadAdquirida = false;
        CancellationToken cancellationToken = origenCancelacion.Token;

        try {
            await cargaExclusiva.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            exclusividadAdquirida = true;
            cancellationToken.ThrowIfCancellationRequested();

            ResumenInicio resumen = await cargarResumen(cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            ResumenMotivacion? motivacion = null;
            Exception? advertenciaMotivacion = null;

            try {
                motivacion = await cargarMotivacion(cancellationToken)
                    .ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                advertenciaMotivacion = motivacion?.Error;
            } catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested) {
                throw;
            } catch (Exception ex)
                when (!RegistroErroresService.EsExcepcionCritica(ex)) {
                advertenciaMotivacion = ex;
            }

            cancellationToken.ThrowIfCancellationRequested();
            PresentacionInicio presentacion = motivacion is null
                ? presentador.Crear(resumen)
                : presentador.Crear(resumen, motivacion);
            PresentacionLogros presentacionLogros = presentadorLogros.Crear(
                resumen,
                motivacion ?? CrearMotivacionNoDisponible());
            cancellationToken.ThrowIfCancellationRequested();

            lock (sincronizacion) {
                if (cerrado) {
                    return CrearResultado(
                        generacion,
                        EstadoResultadoCargaInicio.Cerrada);
                }

                if (generacion != generacionActual) {
                    return CrearResultado(
                        generacion,
                        EstadoResultadoCargaInicio.Obsoleta);
                }
            }

            return new ResultadoCargaInicio(
                generacion,
                EstadoResultadoCargaInicio.Completada,
                presentacion,
                null) {
                AdvertenciaMotivacion = advertenciaMotivacion,
                Logros = presentacionLogros
            };
        } catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested) {
            lock (sincronizacion) {
                if (cerrado) {
                    return CrearResultado(
                        generacion,
                        EstadoResultadoCargaInicio.Cerrada);
                }

                return CrearResultado(
                    generacion,
                    generacion == generacionActual
                        ? EstadoResultadoCargaInicio.Cancelada
                        : EstadoResultadoCargaInicio.Obsoleta);
            }
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            lock (sincronizacion) {
                if (cerrado) {
                    return CrearResultado(
                        generacion,
                        EstadoResultadoCargaInicio.Cerrada);
                }

                if (generacion != generacionActual) {
                    return CrearResultado(
                        generacion,
                        EstadoResultadoCargaInicio.Obsoleta);
                }
            }

            return new ResultadoCargaInicio(
                generacion,
                EstadoResultadoCargaInicio.ErrorRecuperable,
                null,
                ex);
        } finally {
            if (exclusividadAdquirida) {
                cargaExclusiva.Release();
            }

            lock (sincronizacion) {
                if (ReferenceEquals(
                    cancelacionSolicitudActual,
                    origenCancelacion)) {
                    cancelacionSolicitudActual = null;
                    cargaEnCurso = false;
                }
            }

            origenCancelacion.Dispose();
        }
    }

    private static ResultadoCargaInicio CrearResultado(
        long generacion,
        EstadoResultadoCargaInicio estado) {
        return new ResultadoCargaInicio(
            generacion,
            estado,
            null,
            null);
    }

    private static Task<ResumenMotivacion?> CargarMotivacionNoDisponibleAsync(
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<ResumenMotivacion?>(null);
    }

    private static ResumenMotivacion CrearMotivacionNoDisponible() {
        return new ResumenMotivacion(
            EstadoDisponibilidadMotivacion.NoDisponible,
            null,
            null,
            string.Empty,
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            null);
    }

    private static void IntentarCancelar(
        CancellationTokenSource? cancelacion) {
        if (cancelacion is null) {
            return;
        }

        try {
            cancelacion.Cancel();
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            // Una devolución de llamada de cancelación no debe impedir
            // que la generación más reciente controle el resultado visible.
        }
    }
}
