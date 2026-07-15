using EndForge.Models;
using System.Security;
namespace EndForge.Services;

public class RecientesService {
    private const int LimiteRecientes = 10;
    private readonly string rutaRecientes;
    public RecientesService(string rutaRecientes) {
        this.rutaRecientes = rutaRecientes;
    }

    public bool ExisteArchivoRecientes() {
        return File.Exists(rutaRecientes);
    }

    public ResultadoLecturaRecientes LeerProyectosRecientes() {
        string[] lineas;

        try {
            lineas = File.ReadAllLines(rutaRecientes);
        } catch (FileNotFoundException ex) {
            return CrearResultadoLectura(EstadoLecturaRecientes.ArchivoInexistente, error: ex);
        } catch (DirectoryNotFoundException ex) {
            return CrearResultadoLectura(EstadoLecturaRecientes.ArchivoInexistente, error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoLectura(EstadoLecturaRecientes.PermisosInsuficientes, error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoLectura(EstadoLecturaRecientes.PermisosInsuficientes, error: ex);
        } catch (IOException ex) {
            return CrearResultadoLectura(EstadoLecturaRecientes.ErrorIo, error: ex);
        } catch (Exception ex) {
            return CrearResultadoLectura(EstadoLecturaRecientes.ErrorIo, error: ex);
        }

        List<ProyectoReciente> proyectos = new();
        HashSet<string> rutasAgregadas = new(StringComparer.OrdinalIgnoreCase);
        int registrosInvalidos = 0;

        foreach (string linea in lineas) {
            if (!IntentarCrearProyectoReciente(linea, out ProyectoReciente proyecto)) {
                registrosInvalidos++;
                continue;
            }

            if (!rutasAgregadas.Add(proyecto.Ruta)) {
                continue;
            }

            if (proyectos.Count < LimiteRecientes) {
                proyectos.Add(proyecto);
            }
        }

        EstadoLecturaRecientes estado = registrosInvalidos > 0 ? EstadoLecturaRecientes.ContenidoInvalido : EstadoLecturaRecientes.Exitosa;
        return CrearResultadoLectura(estado, proyectos, registrosInvalidos);
    }

    public ResultadoEscrituraRecientes GuardarProyectoReciente(string rutaProyecto) {
        ResultadoLecturaRecientes lectura = LeerProyectosRecientes();

        if (lectura.Estado == EstadoLecturaRecientes.PermisosInsuficientes) {
            return new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.PermisosInsuficientes, Error = lectura.Error};
        }

        if (lectura.Estado == EstadoLecturaRecientes.ErrorIo) {
            return new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.ErrorIo, Error = lectura.Error};
        }

        string? rutaRecientesTemporal = null;

        try {
            List<ProyectoReciente> recientes = lectura.Proyectos
                .Select(proyecto => new ProyectoReciente {
                    Nombre = proyecto.Nombre, Ruta = proyecto.Ruta}).ToList();

            recientes.RemoveAll(proyecto => proyecto.Ruta.Equals(rutaProyecto, StringComparison.OrdinalIgnoreCase));

            recientes.Insert(0, new ProyectoReciente {
                Nombre = Path.GetFileName(rutaProyecto), Ruta = rutaProyecto});

            string[] contenido = recientes.Take(LimiteRecientes).Select(proyecto => $"{proyecto.Nombre}|{proyecto.Ruta}").ToArray();
            string carpetaRecientes = Path.GetDirectoryName(rutaRecientes) ?? throw new IOException("No se pudo determinar la carpeta de recientes.");
            rutaRecientesTemporal = Path.Combine(carpetaRecientes, $".recientes-{Guid.NewGuid():N}.tmp");
            File.WriteAllLines(rutaRecientesTemporal, contenido);

            if (File.Exists(rutaRecientes)) {
                File.Replace(rutaRecientesTemporal, rutaRecientes, null);
            } else {
                File.Move(rutaRecientesTemporal, rutaRecientes);
            }

            return new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.Exitosa, RegistrosInvalidosIgnorados = lectura.RegistrosInvalidos};
        } catch (UnauthorizedAccessException ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(EstadoEscrituraRecientes.PermisosInsuficientes, ex);
        } catch (SecurityException ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(EstadoEscrituraRecientes.PermisosInsuficientes, ex);
        } catch (IOException ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(EstadoEscrituraRecientes.ErrorIo, ex);
        } catch (Exception ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(EstadoEscrituraRecientes.ErrorIo, ex);
        }
    }

    private static ResultadoLecturaRecientes CrearResultadoLectura(
        EstadoLecturaRecientes estado,
        IReadOnlyList<ProyectoReciente>? proyectos = null,
        int registrosInvalidos = 0,
        Exception? error = null) {
        return new ResultadoLecturaRecientes {
            Estado = estado,
            Proyectos = proyectos ?? Array.Empty<ProyectoReciente>(),
            RegistrosInvalidos = registrosInvalidos,
            Error = error
        };
    }

    private static ResultadoEscrituraRecientes CrearErrorEscritura(
        EstadoEscrituraRecientes estado,
        Exception error) {
        return new ResultadoEscrituraRecientes {
            Estado = estado,
            Error = error
        };
    }

    private static bool IntentarCrearProyectoReciente(
        string linea,
        out ProyectoReciente proyecto) {
        proyecto = new ProyectoReciente();

        try {
            int separador = linea.IndexOf('|');

            if (separador <= 0 ||
                separador == linea.Length - 1 ||
                linea.IndexOf('|', separador + 1) >= 0) {
                return false;
            }

            string nombre = linea[..separador];
            string ruta = linea[(separador + 1)..];

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(ruta) ||
                !Path.IsPathFullyQualified(ruta)) {
                return false;
            }

            Path.GetFullPath(ruta);

            proyecto = new ProyectoReciente {
                Nombre = nombre,
                Ruta = ruta
            };
            return true;
        } catch (Exception) {
            return false;
        }
    }

    private static void LimpiarTemporal(string? rutaTemporal) {
        if (string.IsNullOrWhiteSpace(rutaTemporal)) {
            return;
        }

        try {
            if (File.Exists(rutaTemporal)) {
                File.Delete(rutaTemporal);
            }
        } catch (Exception) {
            // El error original de escritura tiene prioridad sobre la limpieza.
        }
    }
}
