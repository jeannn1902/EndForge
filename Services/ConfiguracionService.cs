using EndForge.Models;
using System.Xml;
using System.Xml.Linq;
namespace EndForge.Services;

public class ConfiguracionService {
    private const string MarcadorPlantilla = "00_Plantilla";
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

        try {
            string rutaPlantillaNormalizada = Path.GetFullPath(rutaPlantilla);
            string[] soluciones = BuscarArchivosPorExtension(
                rutaPlantillaNormalizada,
                ".sln",
                SearchOption.TopDirectoryOnly
            )
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(Path.GetFileName, StringComparer.Ordinal)
                .ToArray();

            if (soluciones.Length == 0) {
                return EstadoValidacionConfiguracion.PlantillaSinSolucion;
            }

            string[] solucionesConMarcador = soluciones
                .Where(NombreContieneMarcador)
                .ToArray();

            if (solucionesConMarcador.Length == 0) {
                return EstadoValidacionConfiguracion.PlantillaSolucionSinMarcador;
            }

            string[] proyectos = BuscarArchivosPorExtension(
                rutaPlantillaNormalizada,
                ".vcxproj",
                SearchOption.AllDirectories
            );

            if (proyectos.Length == 0) {
                return EstadoValidacionConfiguracion.PlantillaSinProyectoCpp;
            }

            string[] cpp = BuscarArchivosPorExtension(
                rutaPlantillaNormalizada,
                ".cpp",
                SearchOption.AllDirectories
            );

            if (cpp.Length == 0) {
                return EstadoValidacionConfiguracion.PlantillaSinArchivosCpp;
            }

            bool existeReferenciaProyecto = false;
            bool existeProyectoReferenciado = false;
            bool existeProyectoReferenciadoConMarcador = false;
            bool existeProyectoReferenciadoXmlValido = false;

            foreach (string solucion in solucionesConMarcador) {
                string[] referenciasProyecto = ExtraerReferenciasProyecto(solucion);

                foreach (string referenciaProyecto in referenciasProyecto) {
                    existeReferenciaProyecto = true;

                    if (!IntentarResolverReferenciaProyecto(
                        rutaPlantillaNormalizada,
                        solucion,
                        referenciaProyecto,
                        out string rutaProyecto)) {
                        continue;
                    }

                    if (!File.Exists(rutaProyecto)) {
                        continue;
                    }

                    existeProyectoReferenciado = true;

                    if (!NombreContieneMarcador(rutaProyecto)) {
                        continue;
                    }

                    existeProyectoReferenciadoConMarcador = true;

                    try {
                        bool contieneReferenciaMarcada = ProyectoContieneReferenciaMarcada(rutaProyecto);
                        existeProyectoReferenciadoXmlValido = true;

                        if (contieneReferenciaMarcada) {
                            return EstadoValidacionConfiguracion.Valida;
                        }
                    } catch (XmlException) {
                        // Se revisan los demás proyectos referenciados antes de rechazar la plantilla.
                    }
                }
            }

            if (!existeReferenciaProyecto) {
                return EstadoValidacionConfiguracion.PlantillaSolucionSinReferenciaMarcador;
            }

            if (!existeProyectoReferenciado) {
                return EstadoValidacionConfiguracion.PlantillaProyectoReferenciadoNoDisponible;
            }

            if (!existeProyectoReferenciadoConMarcador) {
                return EstadoValidacionConfiguracion.PlantillaProyectoSinMarcador;
            }

            if (!existeProyectoReferenciadoXmlValido) {
                return EstadoValidacionConfiguracion.PlantillaProyectoXmlInvalido;
            }

            return EstadoValidacionConfiguracion.PlantillaProyectoSinReferenciaMarcador;
        } catch (UnauthorizedAccessException) {
            return EstadoValidacionConfiguracion.ErrorLecturaPlantilla;
        } catch (IOException) {
            return EstadoValidacionConfiguracion.ErrorLecturaPlantilla;
        }
    }

    private static string[] BuscarArchivosPorExtension(
        string ruta,
        string extension,
        SearchOption opcionBusqueda) {
        return Directory
            .EnumerateFiles(ruta, "*", opcionBusqueda)
            .Where(archivo => Path.GetExtension(archivo).Equals(extension, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static bool NombreContieneMarcador(string rutaArchivo) {
        return Path
            .GetFileNameWithoutExtension(rutaArchivo)
            .Contains(MarcadorPlantilla, StringComparison.Ordinal);
    }

    private static string[] ExtraerReferenciasProyecto(string rutaSolucion) {
        List<string> referencias = new();

        foreach (string linea in File.ReadLines(rutaSolucion)) {
            string[] campos = linea.Split('"');

            if (campos.Length <= 5 ||
                !linea.TrimStart().StartsWith("Project(", StringComparison.Ordinal)) {
                continue;
            }

            string referencia = campos[5];

            if (Path.GetExtension(referencia).Equals(".vcxproj", StringComparison.OrdinalIgnoreCase)) {
                referencias.Add(referencia);
            }
        }

        return referencias
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(referencia => referencia, StringComparer.OrdinalIgnoreCase)
            .ThenBy(referencia => referencia, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IntentarResolverReferenciaProyecto(
        string rutaPlantilla,
        string rutaSolucion,
        string referenciaProyecto,
        out string rutaProyecto) {
        rutaProyecto = "";

        if (string.IsNullOrWhiteSpace(referenciaProyecto) || Path.IsPathRooted(referenciaProyecto)) {
            return false;
        }

        try {
            string carpetaSolucion = Path.GetDirectoryName(rutaSolucion)!;
            string rutaNormalizada = Path.GetFullPath(referenciaProyecto, carpetaSolucion);
            string rutaRelativa = Path.GetRelativePath(rutaPlantilla, rutaNormalizada);

            if (Path.IsPathRooted(rutaRelativa) ||
                rutaRelativa.Equals("..", StringComparison.Ordinal) ||
                rutaRelativa.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                rutaRelativa.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal)) {
                return false;
            }

            rutaProyecto = rutaNormalizada;
            return true;
        } catch (ArgumentException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        } catch (PathTooLongException) {
            return false;
        }
    }

    private static bool ProyectoContieneReferenciaMarcada(string rutaProyecto) {
        XDocument proyecto = XDocument.Load(rutaProyecto, LoadOptions.PreserveWhitespace);

        return proyecto
                .Descendants()
                .Attributes()
                .Any(atributo => atributo.Value.Contains(MarcadorPlantilla, StringComparison.Ordinal)) ||
            proyecto
                .DescendantNodes()
                .OfType<XText>()
                .Any(texto => texto.Value.Contains(MarcadorPlantilla, StringComparison.Ordinal));
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
