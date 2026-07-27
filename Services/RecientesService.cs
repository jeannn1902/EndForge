using EndForge.Models;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace EndForge.Services;

public class RecientesService {
    private const int LimiteRecientes = 10;
    private static readonly TimeSpan TiempoEsperaMutex =
        TimeSpan.FromSeconds(15);
    private readonly string rutaRecientes;
    private readonly SeleccionSolucionesService seleccionSolucionesService;

    public RecientesService(string rutaRecientes)
        : this(rutaRecientes, new SeleccionSolucionesService()) {
    }

    internal RecientesService(
        string rutaRecientes,
        SeleccionSolucionesService seleccionSolucionesService) {
        this.rutaRecientes = rutaRecientes;
        this.seleccionSolucionesService = seleccionSolucionesService;
    }

    public bool ExisteArchivoRecientes() {
        return File.Exists(rutaRecientes);
    }

    public ResultadoLecturaRecientes LeerProyectosRecientes() {
        return LeerProyectosRecientesInterno();
    }

    public ResultadoEscrituraRecientes GuardarProyectoReciente(
        string rutaProyecto) {
        if (!IntentarNormalizarRutaProyecto(
            rutaProyecto,
            out string rutaProyectoNormalizada)) {
            return new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.RutaProyectoInvalida
            };
        }

        if (!ProyectoEstaDisponible(rutaProyectoNormalizada)) {
            return new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.ProyectoNoDisponible
            };
        }

        string? carpetaRecientes;

        try {
            carpetaRecientes = Path.GetDirectoryName(
                Path.GetFullPath(rutaRecientes));
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or
                PathTooLongException) {
            return CrearErrorEscritura(
                EstadoEscrituraRecientes.ErrorIo,
                ex);
        }

        if (string.IsNullOrWhiteSpace(carpetaRecientes)) {
            return CrearErrorEscritura(
                EstadoEscrituraRecientes.ErrorIo,
                new IOException(
                    "No se pudo determinar la carpeta de recientes."));
        }

        string? rutaRecientesTemporal = null;
        Mutex? mutex = null;
        bool mutexAdquirido = false;

        try {
            mutex = new Mutex(
                initiallyOwned: false,
                CrearNombreMutex(rutaRecientes));

            try {
                mutexAdquirido = mutex.WaitOne(TiempoEsperaMutex);
            } catch (AbandonedMutexException) {
                mutexAdquirido = true;
            }

            if (!mutexAdquirido) {
                return CrearErrorEscritura(
                    EstadoEscrituraRecientes.ArchivoBloqueado,
                    new TimeoutException(
                        "No se pudo obtener acceso exclusivo a recientes."));
            }

            Directory.CreateDirectory(carpetaRecientes);

            if (!ProyectoEstaDisponible(rutaProyectoNormalizada)) {
                return new ResultadoEscrituraRecientes {
                    Estado =
                        EstadoEscrituraRecientes.ProyectoNoDisponible
                };
            }

            ResultadoLecturaRecientes lectura =
                LeerProyectosRecientesInterno();

            if (lectura.Estado ==
                EstadoLecturaRecientes.PermisosInsuficientes) {
                return new ResultadoEscrituraRecientes {
                    Estado =
                        EstadoEscrituraRecientes.PermisosInsuficientes,
                    Error = lectura.Error
                };
            }

            if (lectura.Estado == EstadoLecturaRecientes.ErrorIo) {
                return new ResultadoEscrituraRecientes {
                    Estado = EstadoEscrituraRecientes.ErrorIo,
                    Error = lectura.Error
                };
            }

            List<ProyectoReciente> recientes = lectura.Proyectos
                .Select(proyecto => new ProyectoReciente {
                    Nombre = proyecto.Nombre,
                    Ruta = proyecto.Ruta
                })
                .ToList();

            recientes.RemoveAll(proyecto => proyecto.Ruta.Equals(
                rutaProyectoNormalizada,
                StringComparison.OrdinalIgnoreCase));

            recientes.Insert(0, new ProyectoReciente {
                Nombre = Path.GetFileName(rutaProyectoNormalizada),
                Ruta = rutaProyectoNormalizada
            });

            string[] contenido = recientes
                .Take(LimiteRecientes)
                .Select(proyecto =>
                    $"{proyecto.Nombre}|{proyecto.Ruta}")
                .ToArray();
            rutaRecientesTemporal = Path.Combine(
                carpetaRecientes,
                $".recientes-{Guid.NewGuid():N}.tmp");
            File.WriteAllLines(rutaRecientesTemporal, contenido);
            PublicarArchivoAtomico(
                rutaRecientesTemporal,
                rutaRecientes);
            rutaRecientesTemporal = null;

            return new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.Exitosa,
                RegistrosInvalidosIgnorados =
                    lectura.RegistrosInvalidos,
                RegistrosNoDisponiblesIgnorados =
                    lectura.RegistrosNoDisponibles
            };
        } catch (UnauthorizedAccessException ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(
                EstadoEscrituraRecientes.PermisosInsuficientes,
                ex);
        } catch (SecurityException ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(
                EstadoEscrituraRecientes.PermisosInsuficientes,
                ex);
        } catch (IOException ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(
                EstadoEscrituraRecientes.ErrorIo,
                ex);
        } catch (Exception ex) {
            LimpiarTemporal(rutaRecientesTemporal);
            return CrearErrorEscritura(
                EstadoEscrituraRecientes.ErrorIo,
                ex);
        } finally {
            if (mutexAdquirido) {
                try {
                    mutex?.ReleaseMutex();
                } catch (ApplicationException) {
                    // El resultado de la operación tiene prioridad.
                }
            }

            mutex?.Dispose();
        }
    }

    private ResultadoLecturaRecientes LeerProyectosRecientesInterno() {
        string[] lineas;

        try {
            lineas = File.ReadAllLines(rutaRecientes);
        } catch (FileNotFoundException ex) {
            return CrearResultadoLectura(
                EstadoLecturaRecientes.ArchivoInexistente,
                error: ex);
        } catch (DirectoryNotFoundException ex) {
            return CrearResultadoLectura(
                EstadoLecturaRecientes.ArchivoInexistente,
                error: ex);
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoLectura(
                EstadoLecturaRecientes.PermisosInsuficientes,
                error: ex);
        } catch (SecurityException ex) {
            return CrearResultadoLectura(
                EstadoLecturaRecientes.PermisosInsuficientes,
                error: ex);
        } catch (IOException ex) {
            return CrearResultadoLectura(
                EstadoLecturaRecientes.ErrorIo,
                error: ex);
        } catch (Exception ex) {
            return CrearResultadoLectura(
                EstadoLecturaRecientes.ErrorIo,
                error: ex);
        }

        List<ProyectoReciente> proyectos = new();
        HashSet<string> rutasAgregadas =
            new(StringComparer.OrdinalIgnoreCase);
        int registrosInvalidos = 0;
        int registrosNoDisponibles = 0;

        foreach (string linea in lineas) {
            if (!IntentarCrearProyectoReciente(
                linea,
                out ProyectoReciente proyecto)) {
                registrosInvalidos++;
                continue;
            }

            if (!ProyectoEstaDisponible(proyecto.Ruta)) {
                registrosNoDisponibles++;
                continue;
            }

            if (!rutasAgregadas.Add(proyecto.Ruta)) {
                continue;
            }

            if (proyectos.Count < LimiteRecientes) {
                proyectos.Add(proyecto);
            }
        }

        EstadoLecturaRecientes estado =
            registrosInvalidos > 0 || registrosNoDisponibles > 0
                ? EstadoLecturaRecientes.ContenidoInvalido
                : EstadoLecturaRecientes.Exitosa;

        return CrearResultadoLectura(
            estado,
            proyectos,
            registrosInvalidos,
            registrosNoDisponibles);
    }

    private bool ProyectoEstaDisponible(string rutaProyecto) {
        try {
            ResultadoSeleccionSolucionCompatible seleccion =
                seleccionSolucionesService.SeleccionarSolucionParaPractica(
                    rutaProyecto,
                    rutaRelativaEsperada: null,
                    usarSeleccionGuardada: true);
            return seleccion.Estado ==
                EstadoSeleccionSolucionCompatible.Exitosa;
        } catch (Exception) {
            return false;
        }
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
                !IntentarNormalizarRutaProyecto(
                    ruta,
                    out string rutaNormalizada)) {
                return false;
            }

            proyecto = new ProyectoReciente {
                Nombre = Path.GetFileName(rutaNormalizada),
                Ruta = rutaNormalizada
            };
            return true;
        } catch (Exception) {
            return false;
        }
    }

    private static bool IntentarNormalizarRutaProyecto(
        string? ruta,
        out string rutaNormalizada) {
        rutaNormalizada = "";

        if (string.IsNullOrWhiteSpace(ruta) ||
            !Path.IsPathFullyQualified(ruta)) {
            return false;
        }

        try {
            string rutaCompleta = Path.GetFullPath(ruta);
            string? raiz = Path.GetPathRoot(rutaCompleta);

            if (!string.IsNullOrEmpty(raiz) &&
                !rutaCompleta.Equals(
                    raiz,
                    StringComparison.OrdinalIgnoreCase)) {
                rutaCompleta = rutaCompleta.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar);
            }

            if (string.IsNullOrWhiteSpace(
                Path.GetFileName(rutaCompleta))) {
                return false;
            }

            rutaNormalizada = rutaCompleta;
            return true;
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or
                PathTooLongException) {
            return false;
        }
    }

    private static string CrearNombreMutex(string rutaArchivo) {
        string rutaNormalizada;

        try {
            rutaNormalizada = Path
                .GetFullPath(rutaArchivo)
                .ToUpperInvariant();
        } catch (Exception) {
            rutaNormalizada = rutaArchivo.ToUpperInvariant();
        }

        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(rutaNormalizada));
        return $@"Local\EndForge-Recientes-{Convert.ToHexString(hash)}";
    }

    private static void PublicarArchivoAtomico(
        string rutaTemporal,
        string rutaDestino) {
        if (File.Exists(rutaDestino)) {
            try {
                File.Replace(rutaTemporal, rutaDestino, null);
                return;
            } catch (FileNotFoundException) when (
                !File.Exists(rutaDestino)) {
                // El archivo pudo eliminarse después de comprobarlo.
            }
        }

        File.Move(rutaTemporal, rutaDestino);
    }

    private static ResultadoLecturaRecientes CrearResultadoLectura(
        EstadoLecturaRecientes estado,
        IReadOnlyList<ProyectoReciente>? proyectos = null,
        int registrosInvalidos = 0,
        int registrosNoDisponibles = 0,
        Exception? error = null) {
        return new ResultadoLecturaRecientes {
            Estado = estado,
            Proyectos =
                proyectos ?? Array.Empty<ProyectoReciente>(),
            RegistrosInvalidos = registrosInvalidos,
            RegistrosNoDisponibles = registrosNoDisponibles,
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

    private static void LimpiarTemporal(string? rutaTemporal) {
        if (string.IsNullOrWhiteSpace(rutaTemporal)) {
            return;
        }

        try {
            if (File.Exists(rutaTemporal)) {
                File.Delete(rutaTemporal);
            }
        } catch (Exception) {
            // El error original de escritura tiene prioridad.
        }
    }
}
