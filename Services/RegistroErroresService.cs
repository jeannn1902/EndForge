using System.Text;
using System.Diagnostics;

namespace EndForge.Services;

internal enum OrigenRegistroError {
    InicioAplicacion,
    Interfaz,
    DominioAplicacion,
    TareaNoObservada
}

internal sealed class RegistroErroresService {
    internal const int CantidadMaximaArchivosPredeterminada = 5;
    internal const long TamanoMaximoArchivoPredeterminadoBytes = 64 * 1024;

    private const int CantidadMaximaTiposExcepcion = 16;
    private const int CantidadMaximaMetodosStack = 12;
    private const string PrefijoArchivo = "endforge-error-";
    private readonly int cantidadMaximaArchivos;
    private readonly long tamanoMaximoArchivoBytes;
    private readonly object sincronizacion = new();

    internal string RutaDirectorioLogs { get; }

    internal RegistroErroresService()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EndForge",
                "Logs"),
            CantidadMaximaArchivosPredeterminada,
            TamanoMaximoArchivoPredeterminadoBytes) {
    }

    internal RegistroErroresService(
        string rutaDirectorioLogs,
        int cantidadMaximaArchivos = CantidadMaximaArchivosPredeterminada,
        long tamanoMaximoArchivoBytes = TamanoMaximoArchivoPredeterminadoBytes) {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDirectorioLogs);

        if (cantidadMaximaArchivos <= 0) {
            throw new ArgumentOutOfRangeException(nameof(cantidadMaximaArchivos));
        }

        if (tamanoMaximoArchivoBytes <= 0) {
            throw new ArgumentOutOfRangeException(nameof(tamanoMaximoArchivoBytes));
        }

        RutaDirectorioLogs = Path.GetFullPath(rutaDirectorioLogs);
        this.cantidadMaximaArchivos = cantidadMaximaArchivos;
        this.tamanoMaximoArchivoBytes = tamanoMaximoArchivoBytes;
    }

    internal bool Registrar(
        Exception error,
        OrigenRegistroError origen,
        bool esTerminante) {
        ArgumentNullException.ThrowIfNull(error);

        string? rutaTemporal = null;

        try {
            lock (sincronizacion) {
                Directory.CreateDirectory(RutaDirectorioLogs);

                string identificador =
                    $"{DateTime.UtcNow.Ticks:D19}-{Guid.NewGuid():N}";
                string rutaFinal = Path.Combine(
                    RutaDirectorioLogs,
                    $"{PrefijoArchivo}{identificador}.log");
                rutaTemporal = Path.Combine(
                    RutaDirectorioLogs,
                    $".{PrefijoArchivo}{identificador}.tmp");
                byte[] contenido = CrearContenidoAcotado(error, origen, esTerminante);

                File.WriteAllBytes(rutaTemporal, contenido);
                File.Move(rutaTemporal, rutaFinal);
                rutaTemporal = null;

                EliminarArchivosExcedentes();
                return true;
            }
        } catch (Exception errorRegistro)
            when (!EsExcepcionCritica(errorRegistro)) {
            return false;
        } finally {
            IntentarEliminarTemporal(rutaTemporal);
        }
    }

    internal static bool EsExcepcionCritica(Exception error) {
        ArgumentNullException.ThrowIfNull(error);

        HashSet<Exception> revisadas = new(ReferenceEqualityComparer.Instance);
        Stack<Exception> pendientes = new();
        pendientes.Push(error);

        while (pendientes.Count > 0) {
            Exception actual = pendientes.Pop();

            if (!revisadas.Add(actual)) {
                continue;
            }

            if (actual is OutOfMemoryException or
                StackOverflowException or
                AccessViolationException or
                BadImageFormatException or
                AppDomainUnloadedException or
                System.Runtime.InteropServices.SEHException) {
                return true;
            }

            if (actual is AggregateException agregada) {
                foreach (Exception interna in agregada.InnerExceptions) {
                    pendientes.Push(interna);
                }
            } else if (actual.InnerException is not null) {
                pendientes.Push(actual.InnerException);
            }
        }

        return false;
    }

    private byte[] CrearContenidoAcotado(
        Exception error,
        OrigenRegistroError origen,
        bool esTerminante) {
        StringBuilder contenido = new();
        contenido.AppendLine("Aplicacion: EndForge");
        contenido.Append("Fecha UTC: ");
        contenido.AppendLine(DateTimeOffset.UtcNow.ToString("O"));
        contenido.Append("Origen: ");
        contenido.AppendLine(origen.ToString());
        contenido.Append("Terminante: ");
        contenido.AppendLine(esTerminante.ToString());
        contenido.AppendLine("Tipos de excepcion:");

        foreach ((string Tipo, int HResult) detalle in ObtenerDetallesExcepcion(error)) {
            contenido.Append("- ");
            contenido.Append(detalle.Tipo);
            contenido.Append(" | HResult: 0x");
            contenido.AppendLine(detalle.HResult.ToString("X8"));
        }

        string[] metodosStack = ObtenerMetodosStack(error);

        if (metodosStack.Length > 0) {
            contenido.AppendLine("Stack administrado (sin rutas ni argumentos):");

            foreach (string metodo in metodosStack) {
                contenido.Append("- ");
                contenido.AppendLine(metodo);
            }
        }

        byte[] bytes = Encoding.UTF8.GetBytes(contenido.ToString());

        if (bytes.LongLength <= tamanoMaximoArchivoBytes) {
            return bytes;
        }

        int longitud = (int)Math.Min(tamanoMaximoArchivoBytes, int.MaxValue);
        return bytes.AsSpan(0, longitud).ToArray();
    }

    private static IEnumerable<(string Tipo, int HResult)> ObtenerDetallesExcepcion(
        Exception error) {
        HashSet<Exception> revisadas = new(ReferenceEqualityComparer.Instance);
        Stack<Exception> pendientes = new();
        pendientes.Push(error);
        int cantidad = 0;

        while (pendientes.Count > 0 && cantidad < CantidadMaximaTiposExcepcion) {
            Exception actual = pendientes.Pop();

            if (!revisadas.Add(actual)) {
                continue;
            }

            yield return (
                actual.GetType().FullName ?? actual.GetType().Name,
                actual.HResult);
            cantidad++;

            if (actual is AggregateException agregada) {
                for (int indice = agregada.InnerExceptions.Count - 1; indice >= 0; indice--) {
                    pendientes.Push(agregada.InnerExceptions[indice]);
                }
            } else if (actual.InnerException is not null) {
                pendientes.Push(actual.InnerException);
            }
        }
    }

    private static string[] ObtenerMetodosStack(Exception error) {
        StackFrame[]? marcos = new StackTrace(error, fNeedFileInfo: false).GetFrames();

        if (marcos is null || marcos.Length == 0) {
            return [];
        }

        return marcos
            .Select(marco => marco.GetMethod())
            .Where(metodo => metodo is not null)
            .Select(metodo => {
                Type? tipo = metodo!.DeclaringType;
                string nombreTipo = tipo?.FullName ?? tipo?.Name ?? "TipoDesconocido";
                return $"{nombreTipo}.{metodo.Name}";
            })
            .Distinct(StringComparer.Ordinal)
            .Take(CantidadMaximaMetodosStack)
            .ToArray();
    }

    private void EliminarArchivosExcedentes() {
        FileInfo[] archivos = new DirectoryInfo(RutaDirectorioLogs)
            .GetFiles($"{PrefijoArchivo}*.log", SearchOption.TopDirectoryOnly)
            .OrderByDescending(archivo => archivo.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (FileInfo archivo in archivos.Skip(cantidadMaximaArchivos)) {
            archivo.Delete();
        }
    }

    private static void IntentarEliminarTemporal(string? rutaTemporal) {
        if (string.IsNullOrEmpty(rutaTemporal)) {
            return;
        }

        try {
            if (File.Exists(rutaTemporal)) {
                File.Delete(rutaTemporal);
            }
        } catch (Exception errorLimpieza)
            when (!EsExcepcionCritica(errorLimpieza)) {
            // El registro es best-effort y nunca debe provocar un segundo error.
        }
    }
}
