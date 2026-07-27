using EndForge.Models;
using System.Diagnostics;

namespace EndForge.Services;

public sealed class AperturaPracticasService {
    private readonly SeleccionSolucionesService seleccionSolucionesService;
    private readonly Action<ProcessStartInfo> lanzarProceso;

    public AperturaPracticasService()
        : this(
            new SeleccionSolucionesService(),
            LanzarProcesoDelSistema) {
    }

    public AperturaPracticasService(
        SeleccionSolucionesService seleccionSolucionesService)
        : this(
            seleccionSolucionesService,
            LanzarProcesoDelSistema) {
    }

    internal AperturaPracticasService(
        SeleccionSolucionesService seleccionSolucionesService,
        Action<ProcessStartInfo> lanzarProceso) {
        this.seleccionSolucionesService = seleccionSolucionesService;
        this.lanzarProceso = lanzarProceso;
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

        ResultadoSeleccionSolucionCompatible seleccion =
            seleccionSolucionesService.SeleccionarSolucionParaPractica(
                rutaProyecto,
                rutaRelativaSolucionEsperada,
                usarSeleccionGuardada
            );

        if (seleccion.Estado != EstadoSeleccionSolucionCompatible.Exitosa) {
            EstadoAperturaPractica estado = seleccion.Estado switch {
                EstadoSeleccionSolucionCompatible.CarpetaInexistente =>
                    EstadoAperturaPractica.CarpetaInexistente,
                EstadoSeleccionSolucionCompatible.SinSoluciones or
                EstadoSeleccionSolucionCompatible.SolucionInexistente =>
                    EstadoAperturaPractica.SolucionInexistente,
                EstadoSeleccionSolucionCompatible.Ambigua =>
                    EstadoAperturaPractica.SolucionAmbigua,
                EstadoSeleccionSolucionCompatible.NingunaCompatible or
                EstadoSeleccionSolucionCompatible.MarcadorInvalido or
                EstadoSeleccionSolucionCompatible.SolucionFueraDeRaiz =>
                    EstadoAperturaPractica.SolucionIncompatible,
                _ => EstadoAperturaPractica.ErrorApertura
            };
            string mensaje = estado switch {
                EstadoAperturaPractica.SolucionInexistente =>
                    "No se encontró la solución esperada de la práctica.",
                EstadoAperturaPractica.SolucionAmbigua =>
                    "La práctica contiene varias soluciones compatibles y no tiene una selección guardada.",
                EstadoAperturaPractica.SolucionIncompatible =>
                    "La solución seleccionada no es compatible o referencia archivos no disponibles.",
                _ => "No se pudo determinar la solución que debe abrirse."
            };

            return new ResultadoAperturaPractica {
                Estado = estado,
                Error = seleccion.Error ?? new InvalidDataException(mensaje)
            };
        }

        string rutaSolucion = seleccion.RutaSolucion;

        try {
            antesDeAbrir?.Invoke();

            string? rutaCpp = DirectorioTemporalEvaluacionCpp
                .EnumerarArchivosSinPuntosDeReanalisis(
                    Path.GetDirectoryName(rutaSolucion) ?? rutaProyecto)
                .Where(ruta => Path
                    .GetExtension(ruta)
                    .Equals(".cpp", StringComparison.OrdinalIgnoreCase))
                .OrderBy(ruta => ruta, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            string rutaDevenv =
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe";

            if (rutaCpp is not null && File.Exists(rutaDevenv)) {
                ProcessStartInfo inicioVisualStudio = new() {
                    FileName = rutaDevenv,
                    UseShellExecute = false
                };

                inicioVisualStudio.ArgumentList.Add(rutaSolucion);
                inicioVisualStudio.ArgumentList.Add(rutaCpp);

                lanzarProceso(inicioVisualStudio);
            } else {
                lanzarProceso(new ProcessStartInfo {
                    FileName = rutaSolucion,
                    UseShellExecute = true
                });
            }

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

    private static void LanzarProcesoDelSistema(ProcessStartInfo inicio) {
        Process.Start(inicio);
    }
}
