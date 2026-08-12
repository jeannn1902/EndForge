using EndForge.Models;

namespace EndForge.Services;

/// <summary>
/// Conserva en memoria los logros nuevos pendientes de presentar. No decide
/// cuándo ni cómo se muestran; la interfaz puede consultarlos y consumirlos
/// únicamente cuando resulte oportuno.
/// </summary>
public sealed class CoordinadorNotificacionesLogros {
    private readonly object sincronizacion = new();
    private readonly HashSet<string> identificadoresObservados =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<LogroDesbloqueado> pendientes = new();
    private bool cerrado;

    public bool Cerrado {
        get {
            lock (sincronizacion) {
                return cerrado;
            }
        }
    }

    public int CantidadPendientes {
        get {
            lock (sincronizacion) {
                return cerrado ? 0 : pendientes.Count;
            }
        }
    }

    public bool Registrar(LogroDesbloqueado logro) {
        ArgumentNullException.ThrowIfNull(logro);

        lock (sincronizacion) {
            return RegistrarSinBloqueo(logro);
        }
    }

    public int Registrar(IEnumerable<LogroDesbloqueado> logros) {
        ArgumentNullException.ThrowIfNull(logros);
        LogroDesbloqueado[] lote = logros.ToArray();

        lock (sincronizacion) {
            if (cerrado) {
                return 0;
            }

            int agregados = 0;

            foreach (LogroDesbloqueado? logro in lote) {
                if (logro is null) {
                    continue;
                }

                if (RegistrarSinBloqueo(logro)) {
                    agregados++;
                }
            }

            return agregados;
        }
    }

    /// <summary>
    /// Devuelve una copia de los pendientes sin retirarlos. Esta operación
    /// permite diferir su presentación mientras Inicio u otra vista no estén
    /// disponibles.
    /// </summary>
    public IReadOnlyList<LogroDesbloqueado> ConsultarPendientes() {
        lock (sincronizacion) {
            return cerrado
                ? Array.Empty<LogroDesbloqueado>()
                : CopiarPendientes();
        }
    }

    public IReadOnlyList<LogroDesbloqueado> ConsumirPendientes() {
        lock (sincronizacion) {
            if (cerrado || pendientes.Count == 0) {
                return Array.Empty<LogroDesbloqueado>();
            }

            IReadOnlyList<LogroDesbloqueado> resultado = CopiarPendientes();
            pendientes.Clear();
            return resultado;
        }
    }

    /// <summary>
    /// Devuelve al principio de la cola un lote que ya se habÃ­a retirado para
    /// mostrarlo, pero cuya presentaciÃ³n se interrumpiÃ³ al abandonar Inicio o
    /// al aparecer un estado prioritario de carga. Los identificadores siguen
    /// marcados como observados para que una recarga no pueda duplicarlos.
    /// </summary>
    public int ReponerPendientesAlInicio(
        IEnumerable<LogroDesbloqueado> logros) {
        ArgumentNullException.ThrowIfNull(logros);
        LogroDesbloqueado[] lote = logros.ToArray();

        lock (sincronizacion) {
            if (cerrado || lote.Length == 0) {
                return 0;
            }

            HashSet<string> pendientesActuales = pendientes
                .Select(logro => logro.LogroId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<LogroDesbloqueado> restaurados = new(lote.Length);

            foreach (LogroDesbloqueado? logro in lote) {
                string identificador = logro?.LogroId?.Trim() ?? string.Empty;

                if (identificador.Length == 0 ||
                    logro!.EsImportado ||
                    !identificadoresObservados.Contains(identificador) ||
                    !pendientesActuales.Add(identificador)) {
                    continue;
                }

                restaurados.Add(Copiar(logro, identificador));
            }

            if (restaurados.Count > 0) {
                pendientes.InsertRange(0, restaurados);
            }

            return restaurados.Count;
        }
    }

    public void Cerrar() {
        lock (sincronizacion) {
            if (cerrado) {
                return;
            }

            cerrado = true;
            pendientes.Clear();
            identificadoresObservados.Clear();
        }
    }

    private bool RegistrarSinBloqueo(LogroDesbloqueado logro) {
        if (cerrado) {
            return false;
        }

        string identificador = logro.LogroId?.Trim() ?? string.Empty;
        if (identificador.Length == 0 ||
            !identificadoresObservados.Add(identificador)) {
            return false;
        }

        // Un logro importado se recuerda para impedir que una carga posterior
        // del mismo identificador lo convierta accidentalmente en "nuevo".
        if (logro.EsImportado) {
            return false;
        }

        pendientes.Add(Copiar(logro, identificador));
        return true;
    }

    private IReadOnlyList<LogroDesbloqueado> CopiarPendientes() {
        return Array.AsReadOnly(
            pendientes
                .Select(logro => Copiar(logro, logro.LogroId))
                .ToArray());
    }

    private static LogroDesbloqueado Copiar(
        LogroDesbloqueado logro,
        string identificador) {
        return new LogroDesbloqueado {
            LogroId = identificador,
            FechaReconocimientoUtc = logro.FechaReconocimientoUtc,
            EsImportado = logro.EsImportado
        };
    }
}
