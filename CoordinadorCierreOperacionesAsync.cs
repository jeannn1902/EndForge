namespace EndForge;

internal readonly record struct DecisionCierreOperacionesAsync(
    bool PermitirCierre,
    bool DebeEsperar,
    Task? FinalizacionPendiente);

internal sealed class CoordinadorCierreOperacionesAsync {
    private readonly object sincronizacion = new();
    private bool cierreSolicitado;
    private bool esperaIniciada;
    private bool cierreAutorizado;
    private bool reintentoProgramado;
    private Task? finalizacionPendiente;

    public bool CierreSolicitado {
        get {
            lock (sincronizacion) {
                return cierreSolicitado;
            }
        }
    }

    public bool PuedeActualizarInterfaz => !CierreSolicitado;

    public DecisionCierreOperacionesAsync SolicitarCierre(
        params Task?[] operaciones) {
        ArgumentNullException.ThrowIfNull(operaciones);

        lock (sincronizacion) {
            cierreSolicitado = true;

            if (cierreAutorizado) {
                return new DecisionCierreOperacionesAsync(
                    PermitirCierre: true,
                    DebeEsperar: false,
                    FinalizacionPendiente: null);
            }

            Task[] pendientes = operaciones
                .Where(operacion =>
                    operacion is { IsCompleted: false })
                .Cast<Task>()
                .Distinct()
                .ToArray();

            if (esperaIniciada) {
                return new DecisionCierreOperacionesAsync(
                    PermitirCierre: false,
                    DebeEsperar: false,
                    FinalizacionPendiente: null);
            }

            if (pendientes.Length == 0) {
                return new DecisionCierreOperacionesAsync(
                    PermitirCierre: true,
                    DebeEsperar: false,
                    FinalizacionPendiente: null);
            }

            esperaIniciada = true;
            finalizacionPendiente = Task.WhenAll(pendientes);

            return new DecisionCierreOperacionesAsync(
                PermitirCierre: false,
                DebeEsperar: true,
                FinalizacionPendiente: finalizacionPendiente);
        }
    }

    public bool IntentarAutorizarReintento() {
        lock (sincronizacion) {
            if (!esperaIniciada ||
                finalizacionPendiente is not { IsCompleted: true }) {
                return false;
            }

            esperaIniciada = false;
            cierreAutorizado = true;
            finalizacionPendiente = null;

            if (reintentoProgramado) {
                return false;
            }

            reintentoProgramado = true;
            return true;
        }
    }
}
