namespace EndForge.Services;

public sealed class SeleccionSolucionesService {
    public const string MarcadorPlantilla = "00_Plantilla";
    private const string NombreArchivoSeleccion = ".endforge-solution";

    public string[] ObtenerSolucionesOrdenadas(string rutaCarpeta) {
        return Directory
            .EnumerateFiles(rutaCarpeta, "*", SearchOption.TopDirectoryOnly)
            .Where(archivo => Path.GetExtension(archivo).Equals(".sln", StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(Path.GetFileName, StringComparer.Ordinal)
            .ToArray();
    }

    public string ObtenerRutaRelativa(string rutaRaiz, string rutaSolucion) {
        return Path.GetRelativePath(Path.GetFullPath(rutaRaiz), Path.GetFullPath(rutaSolucion));
    }

    public string TransformarRutaRelativa(string rutaRelativa, string nombreProyecto) {
        if (Path.IsPathRooted(rutaRelativa)) {
            throw new ArgumentException("La ruta de la solución debe ser relativa.", nameof(rutaRelativa));
        }

        return rutaRelativa.Replace(MarcadorPlantilla, nombreProyecto, StringComparison.Ordinal);
    }

    public void GuardarSolucionSeleccionada(string rutaCarpeta, string rutaRelativaSolucion) {
        if (!Path.GetExtension(rutaRelativaSolucion).Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
            !IntentarResolverRutaRelativa(rutaCarpeta, rutaRelativaSolucion, out string rutaSolucion) ||
            !File.Exists(rutaSolucion)) {
            throw new FileNotFoundException("No se encontró la solución esperada de la práctica.");
        }

        File.WriteAllText(
            Path.Combine(rutaCarpeta, NombreArchivoSeleccion),
            rutaRelativaSolucion
        );
    }

    public string? LeerSolucionSeleccionada(string rutaCarpeta) {
        string rutaSeleccion = Path.Combine(rutaCarpeta, NombreArchivoSeleccion);
        return File.Exists(rutaSeleccion)
            ? File.ReadAllText(rutaSeleccion)
            : null;
    }

    public bool IntentarResolverRutaRelativa(
        string rutaRaiz,
        string rutaRelativa,
        out string rutaCompleta) {
        rutaCompleta = "";

        if (string.IsNullOrWhiteSpace(rutaRelativa) || Path.IsPathRooted(rutaRelativa)) {
            return false;
        }

        try {
            string raizNormalizada = Path.GetFullPath(rutaRaiz);
            string rutaNormalizada = Path.GetFullPath(rutaRelativa, raizNormalizada);
            string rutaRelativaNormalizada = Path.GetRelativePath(raizNormalizada, rutaNormalizada);

            if (Path.IsPathRooted(rutaRelativaNormalizada) ||
                rutaRelativaNormalizada.Equals("..", StringComparison.Ordinal) ||
                rutaRelativaNormalizada.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                rutaRelativaNormalizada.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal)) {
                return false;
            }

            rutaCompleta = rutaNormalizada;
            return true;
        } catch (ArgumentException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        } catch (PathTooLongException) {
            return false;
        }
    }
}
