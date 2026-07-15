using EndForge.Models;
using System.Diagnostics;

namespace EndForge.Services;

public sealed class AperturaPracticasService {
    private readonly SeleccionSolucionesService seleccionSolucionesService;

    public AperturaPracticasService()
        : this(new SeleccionSolucionesService()) {
    }

    public AperturaPracticasService(SeleccionSolucionesService seleccionSolucionesService) {
        this.seleccionSolucionesService = seleccionSolucionesService;
    }

    public ResultadoAperturaPractica AbrirPractica(
        string rutaProyecto,
        Action? antesDeAbrir = null) {
        return AbrirPracticaInterna(
            rutaProyecto,
            null,
            usarSeleccionGuardada: true,
            antesDeAbrir
        );
    }

    public ResultadoAperturaPractica AbrirPractica(
        string rutaProyecto,
        string rutaRelativaSolucionEsperada,
        Action? antesDeAbrir = null) {
        return AbrirPracticaInterna(
            rutaProyecto,
            rutaRelativaSolucionEsperada,
            usarSeleccionGuardada: false,
            antesDeAbrir
        );
    }

    private ResultadoAperturaPractica AbrirPracticaInterna(
        string rutaProyecto,
        string? rutaRelativaSolucionEsperada,
        bool usarSeleccionGuardada,
        Action? antesDeAbrir) {
        if (!Directory.Exists(rutaProyecto)) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.CarpetaInexistente,
                Error = new DirectoryNotFoundException("La carpeta de esta práctica ya no existe.")
            };
        }

        string? rutaSolucion;

        try {
            if (usarSeleccionGuardada) {
                rutaRelativaSolucionEsperada =
                    seleccionSolucionesService.LeerSolucionSeleccionada(rutaProyecto);
            }

            // Regla determinista: nombre ordinal sin distinguir mayúsculas,
            // con orden ordinal como desempate.
            if (rutaRelativaSolucionEsperada == null) {
                rutaSolucion = seleccionSolucionesService
                    .ObtenerSolucionesOrdenadas(rutaProyecto)
                    .FirstOrDefault();
            } else if (
                Path.GetExtension(rutaRelativaSolucionEsperada).Equals(".sln", StringComparison.OrdinalIgnoreCase) &&
                seleccionSolucionesService.IntentarResolverRutaRelativa(
                    rutaProyecto,
                    rutaRelativaSolucionEsperada,
                    out string rutaEsperada) &&
                File.Exists(rutaEsperada)) {
                rutaSolucion = rutaEsperada;
            } else {
                rutaSolucion = null;
            }
        } catch (Exception ex) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.ErrorApertura,
                Error = ex
            };
        }

        if (rutaSolucion == null) {
            string mensaje = rutaRelativaSolucionEsperada == null
                ? "No se encontró ningún archivo .sln en la práctica."
                : "No se encontró la solución esperada de la práctica.";

            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.SolucionInexistente,
                Error = new FileNotFoundException(mensaje)
            };
        }

        try {
            antesDeAbrir?.Invoke();

            Process.Start(new ProcessStartInfo {
                FileName = rutaSolucion,
                UseShellExecute = true
            });

            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.Exitosa,
                RutaSolucion = rutaSolucion
            };
        } catch (Exception ex) {
            return new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.ErrorApertura,
                RutaSolucion = rutaSolucion,
                Error = ex
            };
        }
    }
}
