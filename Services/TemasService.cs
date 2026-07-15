using System.Security;

namespace EndForge.Services;
public sealed class TemasService {

    public IReadOnlyList<string> CargarTemas(string rutaBase) {
        if (!Directory.Exists(rutaBase)) {
            return Array.Empty<string>();
        }

        try {
            return Directory
                .GetDirectories(rutaBase)
                .OrderBy(carpeta => carpeta)
                .Select(Path.GetFileName)
                .Where(nombreCarpeta =>
                    !string.IsNullOrEmpty(nombreCarpeta) &&
                    !nombreCarpeta.StartsWith(".") &&
                    EsNombreTemaValido(nombreCarpeta))
                .ToArray()!;
        } catch (UnauthorizedAccessException) {
            return Array.Empty<string>();
        } catch (SecurityException) {
            return Array.Empty<string>();
        } catch (IOException) {
            return Array.Empty<string>();
        }
    }

    public int ObtenerSiguienteNumero(string rutaBase, string temaSeleccionado) {
        string rutaTema = Path.Combine(rutaBase, temaSeleccionado);

        if (!Directory.Exists(rutaTema)) {
            return 1;
        }

        int mayorNumero = 0;

        foreach (string carpeta in Directory.GetDirectories(rutaTema)) {
            string nombreCarpeta = Path.GetFileName(carpeta);
            string[] partes = nombreCarpeta.Split('_');

            if (partes.Length > 0 && int.TryParse(partes[0], out int numero)) {
                mayorNumero = Math.Max(mayorNumero, numero);
            }
        }

        return mayorNumero + 1;
    }

    public bool ExisteTema(string rutaBase, string temaSeleccionado) {
        if (string.IsNullOrWhiteSpace(rutaBase) || string.IsNullOrWhiteSpace(temaSeleccionado)) {
            return false;
        }

        return Directory.Exists(Path.Combine(rutaBase, temaSeleccionado));
    }

    private static bool EsNombreTemaValido(string nombreCarpeta) {
        string[] partes = nombreCarpeta.Split('_');

        return partes.Length >= 2 && int.TryParse(partes[0], out _);
    }
}
