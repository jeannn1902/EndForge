using EndForge.Models;
using System.Security;
namespace EndForge.Services;

public class ConfiguracionService {
    private readonly SeleccionSolucionesService seleccionSolucionesService;
    private readonly string carpetaDatos;
    private readonly string rutaConfig;
    internal string RutaRecientes { get; }

    public ConfiguracionService()
        : this(new SeleccionSolucionesService()) {
    }

    public ConfiguracionService(SeleccionSolucionesService seleccionSolucionesService)
        : this(
            seleccionSolucionesService,
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "EndForge")) {
    }

    internal ConfiguracionService(
        SeleccionSolucionesService seleccionSolucionesService,
        string carpetaDatos) {
        this.seleccionSolucionesService = seleccionSolucionesService;
        this.carpetaDatos = Path.GetFullPath(carpetaDatos);
        rutaConfig = Path.Combine(this.carpetaDatos, "config.txt");
        RutaRecientes = Path.Combine(this.carpetaDatos, "recientes.txt");
    }

    public ResultadoCargaConfiguracion CargarConfiguracion() {
        string[] lineas;

        try {
            if (!Directory.Exists(carpetaDatos)) {
                Directory.CreateDirectory(carpetaDatos);
            }

            lineas = File.ReadAllLines(rutaConfig);
        } catch (FileNotFoundException) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.NoDisponible
            };
        } catch (UnauthorizedAccessException) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.ErrorPermisosConfiguracion
            };
        } catch (SecurityException) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.ErrorPermisosConfiguracion
            };
        } catch (IOException) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.ErrorLecturaConfiguracion
            };
        } catch (Exception) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.ErrorLecturaConfiguracion
            };
        }

        if (lineas.Length < 2) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.NoDisponible
            };
        }

        return new ResultadoCargaConfiguracion {
            Estado = EstadoCargaConfiguracion.Cargada,
            RutaBase = lineas[0],
            RutaPlantilla = lineas[1]
        };
    }

    public EstadoValidacionConfiguracion ValidarConfiguracion(string rutaBase, string rutaPlantilla) {
        return ValidarConfiguracionDetallada(rutaBase, rutaPlantilla).Estado;
    }

    public ResultadoValidacionConfiguracion ValidarConfiguracionDetallada(
        string rutaBase,
        string rutaPlantilla) {
        if (!Directory.Exists(rutaBase) || !Directory.Exists(rutaPlantilla)) {
            return CrearResultadoValidacion(EstadoValidacionConfiguracion.RutasNoExistentes);
        }

        try {
            string rutaBaseNormalizada = Path.GetFullPath(rutaBase);

            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                rutaBaseNormalizada,
                rutaBaseNormalizada)) {
                return CrearResultadoValidacion(
                    EstadoValidacionConfiguracion.RutaBaseNoSegura);
            }

            string rutaPlantillaNormalizada = Path.GetFullPath(rutaPlantilla);
            ResultadoSeleccionSolucionCompatible seleccion =
                seleccionSolucionesService.SeleccionarSolucionParaPlantilla(
                    rutaPlantillaNormalizada
                );

            if (seleccion.Estado == EstadoSeleccionSolucionCompatible.Exitosa) {
                return CrearResultadoValidacion(
                    EstadoValidacionConfiguracion.Valida,
                    seleccion.RutaRelativaSolucion
                );
            }

            EstadoValidacionConfiguracion estado = seleccion.Estado switch {
                EstadoSeleccionSolucionCompatible.SinSoluciones =>
                    EstadoValidacionConfiguracion.PlantillaSinSolucion,
                EstadoSeleccionSolucionCompatible.SinSolucionMarcada =>
                    EstadoValidacionConfiguracion.PlantillaSolucionSinMarcador,
                EstadoSeleccionSolucionCompatible.CarpetaInexistente =>
                    EstadoValidacionConfiguracion.RutasNoExistentes,
                EstadoSeleccionSolucionCompatible.NingunaCompatible =>
                    MapearIncompatibilidad(seleccion.MotivoIncompatibilidad),
                _ => EstadoValidacionConfiguracion.ErrorLecturaPlantilla
            };

            return CrearResultadoValidacion(estado);
        } catch (UnauthorizedAccessException) {
            return CrearResultadoValidacion(EstadoValidacionConfiguracion.ErrorLecturaPlantilla);
        } catch (SecurityException) {
            return CrearResultadoValidacion(EstadoValidacionConfiguracion.ErrorLecturaPlantilla);
        } catch (IOException) {
            return CrearResultadoValidacion(EstadoValidacionConfiguracion.ErrorLecturaPlantilla);
        } catch (Exception) {
            return CrearResultadoValidacion(EstadoValidacionConfiguracion.ErrorLecturaPlantilla);
        }
    }

    private static EstadoValidacionConfiguracion MapearIncompatibilidad(
        MotivoIncompatibilidadSolucion motivo) {
        return motivo switch {
            MotivoIncompatibilidadSolucion.SolucionSinProyectoCpp =>
                EstadoValidacionConfiguracion.PlantillaSolucionSinReferenciaMarcador,
            MotivoIncompatibilidadSolucion.ProyectoInexistente or
            MotivoIncompatibilidadSolucion.ProyectoFueraDeRaiz =>
                EstadoValidacionConfiguracion.PlantillaProyectoReferenciadoNoDisponible,
            MotivoIncompatibilidadSolucion.ProyectoSinMarcador =>
                EstadoValidacionConfiguracion.PlantillaProyectoSinMarcador,
            MotivoIncompatibilidadSolucion.ProyectoSinClCompile or
            MotivoIncompatibilidadSolucion.ClCompileSinMarcador =>
                EstadoValidacionConfiguracion.PlantillaProyectoSinReferenciaMarcador,
            MotivoIncompatibilidadSolucion.ClCompileInexistente or
            MotivoIncompatibilidadSolucion.ClCompileFueraDeRaiz =>
                EstadoValidacionConfiguracion.PlantillaSinArchivosCpp,
            MotivoIncompatibilidadSolucion.ProyectoXmlInvalido or
            MotivoIncompatibilidadSolucion.FiltersXmlInvalido or
            MotivoIncompatibilidadSolucion.FiltersIncoherente =>
                EstadoValidacionConfiguracion.PlantillaProyectoXmlInvalido,
            _ => EstadoValidacionConfiguracion.ErrorLecturaPlantilla
        };
    }

    private static ResultadoValidacionConfiguracion CrearResultadoValidacion(
        EstadoValidacionConfiguracion estado,
        string rutaRelativaSolucion = "") {
        return new ResultadoValidacionConfiguracion {
            Estado = estado,
            RutaRelativaSolucion = rutaRelativaSolucion
        };
    }

    public void GuardarConfiguracion(string rutaBase, string rutaPlantilla) {
        string rutaConfigTemporal = Path.Combine(carpetaDatos, $".config-{Guid.NewGuid():N}.tmp");

        try {
            Directory.CreateDirectory(carpetaDatos);
            File.WriteAllLines(rutaConfigTemporal, new string[] {
                rutaBase, rutaPlantilla
            });

            if (File.Exists(rutaConfig)) {
                File.Replace(rutaConfigTemporal, rutaConfig, null);
            } else {
                File.Move(rutaConfigTemporal, rutaConfig);
            }
        } catch (Exception) {
            try {
                if (File.Exists(rutaConfigTemporal)) {
                    File.Delete(rutaConfigTemporal);
                }
            } catch (Exception) {
                // Evita ocultar el error original del guardado.
            }

            throw;
        }
    }
}
