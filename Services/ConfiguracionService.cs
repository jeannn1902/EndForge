using EndForge.Models;
namespace EndForge.Services;

public class ConfiguracionService {
    private readonly string carpetaDatos;
    private readonly string rutaConfig;
    internal string RutaRecientes { get; }

    public ConfiguracionService() {
        carpetaDatos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EndForge");
        rutaConfig = Path.Combine(carpetaDatos, "config.txt");
        RutaRecientes = Path.Combine(carpetaDatos, "recientes.txt");
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
        } catch (IOException) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.ErrorLecturaConfiguracion
            };
        }

        if (lineas.Length < 2) {
            return new ResultadoCargaConfiguracion {
                Estado = EstadoCargaConfiguracion.NoDisponible
            };
        }

        EstadoCargaConfiguracion estado = EstadoCargaConfiguracion.Cargada;

        try {
            if (!File.Exists(RutaRecientes)) {
                File.Create(RutaRecientes).Close();
            }
        } catch (UnauthorizedAccessException) {
            estado = EstadoCargaConfiguracion.ErrorPermisosRecientes;
        } catch (IOException) {
            estado = EstadoCargaConfiguracion.ErrorCreacionRecientes;
        }

        return new ResultadoCargaConfiguracion {
            Estado = estado,
            RutaBase = lineas[0],
            RutaPlantilla = lineas[1]
        };
    }

    public EstadoValidacionConfiguracion ValidarConfiguracion(string rutaBase, string rutaPlantilla) {
        if (!Directory.Exists(rutaBase) || !Directory.Exists(rutaPlantilla)) {
            return EstadoValidacionConfiguracion.RutasNoExistentes;
        }

        string[] soluciones = Directory.GetFiles(rutaPlantilla, "*.sln", SearchOption.TopDirectoryOnly);

        if (soluciones.Length == 0) {
            return EstadoValidacionConfiguracion.PlantillaSinSolucion;
        }

        string[] proyectos = Directory.GetFiles(rutaPlantilla, "*.vcxproj",SearchOption.AllDirectories);

        if (proyectos.Length == 0) {
            return EstadoValidacionConfiguracion.PlantillaSinProyectoCpp;
        }

        string[] cpp = Directory.GetFiles(rutaPlantilla, "*.cpp", SearchOption.AllDirectories);

        if (cpp.Length == 0) {
            return EstadoValidacionConfiguracion.PlantillaSinArchivosCpp;
        }

        return EstadoValidacionConfiguracion.Valida;
    }

    public void GuardarConfiguracion(string rutaBase, string rutaPlantilla) {
        string rutaConfigTemporal = Path.Combine(carpetaDatos, $".config-{Guid.NewGuid():N}.tmp");

        try {
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
