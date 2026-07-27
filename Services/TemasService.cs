using EndForge.Models;
using System.Globalization;
using System.Security;

namespace EndForge.Services;

public sealed class TemasService {
    private readonly Func<string, string[]> obtenerDirectorios;

    public TemasService()
        : this(Directory.GetDirectories) {
    }

    internal TemasService(Func<string, string[]> obtenerDirectorios) {
        this.obtenerDirectorios = obtenerDirectorios ??
            throw new ArgumentNullException(nameof(obtenerDirectorios));
    }

    public IReadOnlyList<string> CargarTemas(string rutaBase) {
        return CargarTemasDetallado(rutaBase).Temas;
    }

    public ResultadoCargaTemas CargarTemasDetallado(string rutaBase) {
        if (string.IsNullOrWhiteSpace(rutaBase)) {
            return CrearResultadoCarga(EstadoCargaTemas.RutaInexistente);
        }

        try {
            string rutaBaseNormalizada = Path.GetFullPath(rutaBase);
            string[] temas = obtenerDirectorios(rutaBaseNormalizada)
                .OrderBy(carpeta => carpeta, StringComparer.OrdinalIgnoreCase)
                .ThenBy(carpeta => carpeta, StringComparer.Ordinal)
                .Where(carpeta =>
                    EsDirectorioTemaSeguro(
                        rutaBaseNormalizada,
                        carpeta))
                .Select(Path.GetFileName)
                .Where(nombreCarpeta =>
                    !string.IsNullOrEmpty(nombreCarpeta) &&
                    !nombreCarpeta.StartsWith(".", StringComparison.Ordinal) &&
                    EsNombreTemaValido(nombreCarpeta))
                .Select(nombreCarpeta => nombreCarpeta!)
                .ToArray();

            return CrearResultadoCarga(
                EstadoCargaTemas.Exitosa,
                temas);
        } catch (DirectoryNotFoundException ex) {
            return CrearResultadoCarga(
                EstadoCargaTemas.RutaInexistente,
                error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoCarga(
                EstadoCargaTemas.PermisosInsuficientes,
                error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoCarga(
                EstadoCargaTemas.PermisosInsuficientes,
                error: ex);
        } catch (IOException ex) {
            return CrearResultadoCarga(
                EstadoCargaTemas.ErrorIo,
                error: ex);
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException) {
            return CrearResultadoCarga(
                EstadoCargaTemas.RutaInexistente,
                error: ex);
        }
    }

    public ResultadoNumeracionPractica ObtenerSiguienteNumero(
        string rutaBase,
        string temaSeleccionado) {
        if (!IntentarObtenerRutaTema(
            rutaBase,
            temaSeleccionado,
            out string rutaTema)) {
            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.TemaInexistente);
        }

        try {
            int mayorNumero = 0;

            foreach (string carpeta in obtenerDirectorios(rutaTema)) {
                string nombreCarpeta = Path.GetFileName(carpeta);

                if (!IntentarObtenerNumeroPractica(
                    nombreCarpeta,
                    out int numero)) {
                    continue;
                }

                mayorNumero = Math.Max(mayorNumero, numero);
            }

            if (mayorNumero == int.MaxValue) {
                return CrearResultadoNumeracion(
                    EstadoNumeracionPractica.LimiteAlcanzado);
            }

            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.Exitosa,
                mayorNumero + 1);
        } catch (DirectoryNotFoundException ex) {
            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.TemaInexistente,
                error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.PermisosInsuficientes,
                error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.PermisosInsuficientes,
                error: ex);
        } catch (IOException ex) {
            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.ErrorIo,
                error: ex);
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException) {
            return CrearResultadoNumeracion(
                EstadoNumeracionPractica.TemaInexistente,
                error: ex);
        }
    }

    public bool ExisteTema(string rutaBase, string temaSeleccionado) {
        if (!IntentarObtenerRutaTema(
            rutaBase,
            temaSeleccionado,
            out string rutaTema)) {
            return false;
        }

        return Directory.Exists(rutaTema);
    }

    internal bool IntentarObtenerRutaTemaSeguraParaCreacion(
        string rutaBase,
        string temaSeleccionado,
        out string rutaTema) {
        return IntentarObtenerRutaTema(
            rutaBase,
            temaSeleccionado,
            out rutaTema);
    }

    private static bool IntentarObtenerRutaTema(
        string rutaBase,
        string temaSeleccionado,
        out string rutaTema) {
        rutaTema = "";

        if (string.IsNullOrWhiteSpace(rutaBase) ||
            string.IsNullOrWhiteSpace(temaSeleccionado)) {
            return false;
        }

        try {
            if (Path.IsPathRooted(temaSeleccionado)) {
                return false;
            }

            string[] segmentos = temaSeleccionado.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.None);

            if (segmentos.Length == 0 ||
                segmentos.Any(segmento => !EsSegmentoRutaValido(segmento)) ||
                !EsNombreTemaValido(segmentos[^1])) {
                return false;
            }

            string rutaBaseNormalizada = Path.GetFullPath(rutaBase);
            string rutaTemaNormalizada = Path.GetFullPath(
                Path.Combine(rutaBaseNormalizada, temaSeleccionado));
            string rutaRelativa = Path.GetRelativePath(
                rutaBaseNormalizada,
                rutaTemaNormalizada);

            if (rutaRelativa.Equals(".", StringComparison.Ordinal) ||
                Path.IsPathRooted(rutaRelativa) ||
                rutaRelativa.Equals("..", StringComparison.Ordinal) ||
                rutaRelativa.StartsWith(
                    $"..{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal) ||
                rutaRelativa.StartsWith(
                    $"..{Path.AltDirectorySeparatorChar}",
                    StringComparison.Ordinal)) {
                return false;
            }

            if (!DirectorioTemporalEvaluacionCpp
                .EsRutaSinPuntosDeReanalisis(
                    rutaBaseNormalizada,
                    rutaTemaNormalizada)) {
                return false;
            }

            rutaTema = rutaTemaNormalizada;
            return true;
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or
                PathTooLongException) {
            return false;
        }
    }

    private static bool EsSegmentoRutaValido(string segmento) {
        return !string.IsNullOrWhiteSpace(segmento) &&
            !segmento.Equals(".", StringComparison.Ordinal) &&
            !segmento.Equals("..", StringComparison.Ordinal) &&
            !segmento.EndsWith(".", StringComparison.Ordinal) &&
            !segmento.EndsWith(" ", StringComparison.Ordinal) &&
            segmento.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    private static bool EsDirectorioTemaSeguro(
        string rutaBase,
        string rutaCandidata) {
        string rutaNormalizada = Path.GetFullPath(rutaCandidata);
        string relativa = Path.GetRelativePath(
            rutaBase,
            rutaNormalizada);

        return !Path.IsPathRooted(relativa) &&
            !relativa.Equals(".", StringComparison.Ordinal) &&
            !relativa.Equals("..", StringComparison.Ordinal) &&
            relativa.IndexOfAny([
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar]) < 0 &&
            !File.GetAttributes(rutaNormalizada)
                .HasFlag(FileAttributes.ReparsePoint);
    }

    private static bool EsNombreTemaValido(string nombreCarpeta) {
        int separador = nombreCarpeta.IndexOf('_');

        return separador > 0 &&
            separador < nombreCarpeta.Length - 1 &&
            int.TryParse(
                nombreCarpeta.AsSpan(0, separador),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int numero) &&
            numero > 0;
    }

    private static bool IntentarObtenerNumeroPractica(
        string nombreCarpeta,
        out int numero) {
        numero = 0;
        int separador = nombreCarpeta.IndexOf('_');

        return separador > 0 &&
            separador < nombreCarpeta.Length - 1 &&
            int.TryParse(
                nombreCarpeta.AsSpan(0, separador),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out numero) &&
            numero > 0;
    }

    private static ResultadoCargaTemas CrearResultadoCarga(
        EstadoCargaTemas estado,
        IReadOnlyList<string>? temas = null,
        Exception? error = null) {
        return new ResultadoCargaTemas {
            Estado = estado,
            Temas = temas ?? Array.Empty<string>(),
            Error = error
        };
    }

    private static ResultadoNumeracionPractica CrearResultadoNumeracion(
        EstadoNumeracionPractica estado,
        int? numero = null,
        Exception? error = null) {
        return new ResultadoNumeracionPractica {
            Estado = estado,
            Numero = numero,
            Error = error
        };
    }
}
