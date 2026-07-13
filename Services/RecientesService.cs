// =============================
// Gestión de proyectos recientes
// =============================
using System.IO;
using System.Collections.Generic;
using System.Linq;
namespace EndForge.Services;

public class RecientesService {
    private readonly string rutaRecientes;

    public RecientesService(string rutaRecientes) {
        this.rutaRecientes = rutaRecientes;
    }

    public bool ExisteArchivoRecientes() {
        return File.Exists(rutaRecientes);
    }

    public string[] LeerProyectosRecientes() {
        return File.ReadAllLines(rutaRecientes);
    }

    public void GuardarProyectoReciente(string rutaProyecto) {
        List<string> recientes = new List<string>();

        if (File.Exists(rutaRecientes)) {
            recientes = File.ReadAllLines(rutaRecientes).ToList();
        }

        string nombreProyecto = Path.GetFileName(rutaProyecto);
        string registro = nombreProyecto + "|" + rutaProyecto;
        recientes.RemoveAll(x => x.EndsWith("|" + rutaProyecto));
        recientes.Insert(0, registro);

        if (recientes.Count > 10) {
            recientes = recientes.Take(10).ToList();
        }

        string carpetaRecientes = Path.GetDirectoryName(rutaRecientes)
            ?? throw new IOException("No se pudo determinar la carpeta de recientes.");
        string rutaRecientesTemporal = Path.Combine(carpetaRecientes, $".recientes-{Guid.NewGuid():N}.tmp");

        try {
            File.WriteAllLines(rutaRecientesTemporal, recientes);

            if (File.Exists(rutaRecientes)) {
                File.Replace(rutaRecientesTemporal, rutaRecientes, null);
            } else {
                File.Move(rutaRecientesTemporal, rutaRecientes);
            }
        } catch (Exception) {
            try {
                if (File.Exists(rutaRecientesTemporal)) {
                    File.Delete(rutaRecientesTemporal);
                }
            } catch (Exception) {
                // Evita ocultar el error original del guardado.
            }

            throw;
        }
    }
}
