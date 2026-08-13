using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private readonly object sincronizacionColaMotivacion = new();
    private Task tareaMotivacionActiva = Task.CompletedTask;

    private Task EncolarProcesamientoMotivacion(
        Action operacion,
        bool permitirDuranteCierre = false) {
        ArgumentNullException.ThrowIfNull(operacion);

        lock (sincronizacionColaMotivacion) {
            if (coordinadorCierreOperaciones.CierreSolicitado &&
                !permitirDuranteCierre) {
                return Task.CompletedTask;
            }

            Task operacionAnterior = tareaMotivacionActiva;
            Task operacionActual = EjecutarProcesamientoMotivacionAsync(
                operacionAnterior,
                operacion);
            tareaMotivacionActiva = operacionActual;
            return operacionActual;
        }
    }

    private async Task EjecutarProcesamientoMotivacionAsync(
        Task operacionAnterior,
        Action operacion) {
        try {
            await operacionAnterior.ConfigureAwait(false);
            await Task.Run(operacion).ConfigureAwait(false);
        } catch (Exception ex)
            when (!Services.RegistroErroresService.EsExcepcionCritica(ex)) {
            Program.RegistrarErrorRecuperable(ex);
        } finally {
            ProgramarRecargaInicioTrasMotivacion();
        }
    }

    private Task ObtenerTareaMotivacionPendiente() {
        lock (sincronizacionColaMotivacion) {
            return tareaMotivacionActiva;
        }
    }

    private void RegistrarResultadoMotivacion(
        ResultadoProcesamientoMotivacion resultado) {
        RegistrarLogrosNuevosParaNotificacion(resultado.LogrosNuevos);

        if (resultado.Error is not null) {
            Program.RegistrarErrorRecuperable(resultado.Error);
            return;
        }

        if (resultado.Estado is
            EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles or
            EstadoProcesamientoMotivacion.ErrorRecuperable or
            EstadoProcesamientoMotivacion.VersionIncompatible) {
            Program.RegistrarErrorRecuperable(new InvalidOperationException(
                $"La operación motivacional terminó con estado {resultado.Estado}."));
        }
    }

    private void ProcesarMotivacionProgresoPersistido(
        string practicaId,
        ProgresoCurso progresoPersistido,
        bool vinculoPersistidoAhora,
        bool realizadaPersistidaAhora) {
        RegistrarResultadoMotivacion(
            motivacionService.ProcesarProgresoPersistido(
                practicaId,
                progresoPersistido,
                vinculoPersistidoAhora,
                realizadaPersistidaAhora));
    }

    private void ProgramarRecargaInicioTrasMotivacion() {
        try {
            if (coordinadorCierreOperaciones.CierreSolicitado ||
                IsDisposed ||
                Disposing ||
                !IsHandleCreated) {
                return;
            }

            ProgramarAccionInterfazSegura(() => {
                if (!PuedeEjecutarRecargaInicioProgramada(
                        coordinadorCierreOperaciones.CierreSolicitado,
                        PuedeActualizarInterfazInicio())) {
                    return;
                }

                MarcarInicioPendienteDeRecarga();
            });
        } catch (Exception ex)
            when (!Services.RegistroErroresService.EsExcepcionCritica(ex)) {
            Program.RegistrarErrorRecuperable(ex);
        }
    }

    internal static bool PuedeEjecutarRecargaInicioProgramada(
        bool cierreSolicitado,
        bool puedeActualizarInterfazInicio) {
        return !cierreSolicitado && puedeActualizarInterfazInicio;
    }
}
