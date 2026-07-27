using EndForge.Models;
using System.Text;

namespace EndForge.Services;

public sealed class EjecucionPruebasService {
    // Seguridad: esta ejecución local controla tiempo, salida y procesos propios,
    // pero no es un sandbox. Una futura evaluación en Azure deberá ejecutar cada
    // intento en un contenedor aislado, efímero y sin acceso a datos de EndForge.
    private const int LimiteCaracteresSalida = 64 * 1024;
    private const int LimiteBytesArchivo = 256 * 1024;
    private static readonly TimeSpan TiempoTecnicoMaximo = TimeSpan.FromSeconds(5);
    private static readonly UTF8Encoding Utf8SinBomEstricto =
        new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public async Task<ResultadoEjecucionPruebaCpp> EjecutarCasoAsync(
        SesionCompilacionCpp sesion,
        string entrada,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(sesion);
        entrada ??= "";

        if (!sesion.IntentarObtenerContextoEjecucion(
            out string rutaEjecutable,
            out string rutaPractica)) {
            return new ResultadoEjecucionPruebaCpp {
                Estado = EstadoEjecucionPruebaCpp.SesionNoDisponible
            };
        }

        if (!File.Exists(rutaEjecutable)) {
            return new ResultadoEjecucionPruebaCpp {
                Estado = EstadoEjecucionPruebaCpp.EjecutableInexistente
            };
        }

        if (!Directory.Exists(rutaPractica)) {
            return new ResultadoEjecucionPruebaCpp {
                Estado = EstadoEjecucionPruebaCpp.DirectorioTrabajoInexistente
            };
        }

        return await EjecutarEnDirectorioAsync(
            rutaEjecutable,
            rutaPractica,
            entrada,
            cancellationToken
        ).ConfigureAwait(false);
    }

    public async Task<ResultadoEjecucionCasoPruebaCpp> EjecutarCasoAsync(
        SesionCompilacionCpp sesion,
        CasoPrueba caso,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(sesion);
        ArgumentNullException.ThrowIfNull(caso);

        if (caso.ArchivosEntrada.Count == 0 &&
            caso.ArchivosEsperados.Count == 0) {
            ResultadoEjecucionPruebaCpp ejecucionAnterior =
                await EjecutarCasoAsync(
                    sesion,
                    caso.Entrada,
                    cancellationToken
                ).ConfigureAwait(false);
            return new ResultadoEjecucionCasoPruebaCpp {
                Ejecucion = ejecucionAnterior
            };
        }

        if (!sesion.IntentarObtenerContextoEjecucion(
            out string rutaEjecutable,
            out _)) {
            return CrearResultadoCasoConError(
                EstadoEjecucionPruebaCpp.SesionNoDisponible);
        }

        if (!File.Exists(rutaEjecutable)) {
            return CrearResultadoCasoConError(
                EstadoEjecucionPruebaCpp.EjecutableInexistente);
        }

        string directorioCaso = "";

        try {
            ValidarConfiguracionArchivos(caso);
            directorioCaso = CrearDirectorioCaso(sesion.DirectorioArtefactos);
            PrepararArchivosEntrada(directorioCaso, caso.ArchivosEntrada);

            ResultadoEjecucionPruebaCpp ejecucion =
                await EjecutarEnDirectorioAsync(
                    rutaEjecutable,
                    directorioCaso,
                    caso.Entrada,
                    cancellationToken
                ).ConfigureAwait(false);
            IReadOnlyList<ResultadoArchivoPrueba> archivos =
                CapturarArchivosEsperados(
                    directorioCaso,
                    caso.ArchivosEsperados);

            return new ResultadoEjecucionCasoPruebaCpp {
                Ejecucion = ejecucion,
                Archivos = archivos
            };
        } catch (OperationCanceledException) {
            return CrearResultadoCasoConError(
                EstadoEjecucionPruebaCpp.Cancelada);
        } catch (Exception ex) {
            return CrearResultadoCasoConError(
                EstadoEjecucionPruebaCpp.ErrorInfraestructura,
                ex);
        } finally {
            if (!string.IsNullOrEmpty(directorioCaso)) {
                EliminarDirectorioCaso(
                    sesion.DirectorioArtefactos,
                    directorioCaso);
            }
        }
    }

    private static async Task<ResultadoEjecucionPruebaCpp>
        EjecutarEnDirectorioAsync(
            string rutaEjecutable,
            string directorioTrabajo,
            string entrada,
            CancellationToken cancellationToken) {
        ResultadoProcesoControladoCpp proceso =
            await ProcesoControladoCpp.EjecutarAsync(
                new SolicitudProcesoControladoCpp {
                    Archivo = rutaEjecutable,
                    DirectorioTrabajo = directorioTrabajo,
                    Entrada = entrada,
                    Argumentos = Array.Empty<string>(),
                    TiempoMaximo = TiempoTecnicoMaximo,
                    LimiteCaracteresSalida = LimiteCaracteresSalida
                },
                cancellationToken
            ).ConfigureAwait(false);

        EstadoEjecucionPruebaCpp estado = proceso.Estado switch {
            EstadoProcesoControladoCpp.Exitosa when proceso.CodigoSalida == 0 =>
                EstadoEjecucionPruebaCpp.Exitosa,
            EstadoProcesoControladoCpp.Exitosa =>
                EstadoEjecucionPruebaCpp.CodigoSalidaNoCero,
            EstadoProcesoControladoCpp.Cancelada =>
                EstadoEjecucionPruebaCpp.Cancelada,
            EstadoProcesoControladoCpp.TiempoExcedido =>
                EstadoEjecucionPruebaCpp.TiempoTecnicoExcedido,
            EstadoProcesoControladoCpp.SalidaExcesiva =>
                EstadoEjecucionPruebaCpp.SalidaExcesiva,
            EstadoProcesoControladoCpp.ErrorInicio =>
                EstadoEjecucionPruebaCpp.ErrorInicio,
            EstadoProcesoControladoCpp.ErrorEjecucion =>
                EstadoEjecucionPruebaCpp.ErrorInfraestructura,
            _ => EstadoEjecucionPruebaCpp.ErrorInfraestructura
        };

        return new ResultadoEjecucionPruebaCpp {
            Estado = estado,
            SalidaEstandar = proceso.SalidaEstandar,
            SalidaError = proceso.SalidaError,
            CodigoSalida = proceso.CodigoSalida,
            SalidaTruncada = proceso.SalidaTruncada,
            Duracion = proceso.Duracion,
            Error = proceso.Error
        };
    }

    private static void ValidarConfiguracionArchivos(CasoPrueba caso) {
        ValidarRutasUnicas(
            caso.ArchivosEntrada.Select(archivo => archivo.RutaRelativa),
            "entrada");
        ValidarRutasUnicas(
            caso.ArchivosEsperados.Select(archivo => archivo.RutaRelativa),
            "resultado");

        if (caso.ArchivosEntrada.Any(archivo =>
            Encoding.UTF8.GetByteCount(archivo.Contenido) >
                LimiteBytesArchivo)) {
            throw new InvalidOperationException(
                "Un archivo de entrada supera el límite permitido.");
        }

        foreach (ArchivoEsperadoPrueba archivo in caso.ArchivosEsperados) {
            bool comparacionEstructurada =
                archivo.CadenasEsperadas.Count > 0 ||
                archivo.TablasEsperadas.Count > 0 ||
                archivo.BloquesRegistroEsperados.Count > 0;

            if (archivo.ModoComparacion ==
                    ModoComparacionArchivoPrueba.Estructurado &&
                !comparacionEstructurada) {
                throw new InvalidOperationException(
                    "La comparación estructurada de un archivo no contiene reglas.");
            }
        }
    }

    private static void ValidarRutasUnicas(
        IEnumerable<string> rutas,
        string tipo) {
        string[] normalizadas = rutas
            .Select(NormalizarRutaRelativaConfigurada)
            .ToArray();

        if (normalizadas
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != normalizadas.Length) {
            throw new InvalidOperationException(
                $"El caso contiene rutas de {tipo} duplicadas.");
        }
    }

    private static string CrearDirectorioCaso(string directorioArtefactos) {
        string raiz = Path.GetFullPath(directorioArtefactos);

        if (!Directory.Exists(raiz) ||
            File.GetAttributes(raiz).HasFlag(FileAttributes.ReparsePoint)) {
            throw new InvalidOperationException(
                "La sesión de evaluación no ofrece un directorio seguro.");
        }

        while (true) {
            string directorio = Path.Combine(
                raiz,
                $"caso-archivos-{Guid.NewGuid():N}");

            if (Directory.Exists(directorio) || File.Exists(directorio)) {
                continue;
            }

            Directory.CreateDirectory(directorio);

            try {
                if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(
                        raiz,
                        directorio) ||
                    File.GetAttributes(directorio).HasFlag(
                        FileAttributes.ReparsePoint)) {
                    throw new InvalidOperationException(
                        "No fue posible crear el directorio aislado del caso.");
                }
            } catch (Exception) {
                EliminarDirectorioCaso(raiz, directorio);
                throw;
            }

            return directorio;
        }
    }

    private static void PrepararArchivosEntrada(
        string directorioCaso,
        IReadOnlyList<ArchivoEntradaPrueba> archivos) {
        foreach (ArchivoEntradaPrueba archivo in archivos) {
            string ruta = ResolverRutaRelativaSegura(
                directorioCaso,
                archivo.RutaRelativa);
            string? directorio = Path.GetDirectoryName(ruta);

            if (string.IsNullOrEmpty(directorio)) {
                throw new InvalidOperationException(
                    "La ruta del archivo de entrada no tiene un directorio válido.");
            }

            Directory.CreateDirectory(directorio);

            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                directorioCaso,
                directorio)) {
                throw new InvalidOperationException(
                    "La ruta del archivo de entrada contiene un punto de reanálisis.");
            }

            File.WriteAllText(ruta, archivo.Contenido, Utf8SinBomEstricto);
        }
    }

    private static IReadOnlyList<ResultadoArchivoPrueba>
        CapturarArchivosEsperados(
            string directorioCaso,
            IReadOnlyList<ArchivoEsperadoPrueba> esperados) {
        return esperados
            .Select(esperado => CapturarArchivoEsperado(
                directorioCaso,
                esperado))
            .ToArray();
    }

    private static ResultadoArchivoPrueba CapturarArchivoEsperado(
        string directorioCaso,
        ArchivoEsperadoPrueba esperado) {
        string ruta;

        try {
            ruta = ResolverRutaRelativaSegura(
                directorioCaso,
                esperado.RutaRelativa);
        } catch (Exception ex) {
            return new ResultadoArchivoPrueba {
                RutaRelativa = esperado.RutaRelativa,
                Estado = EstadoArchivoPrueba.RutaInvalida,
                Error = ex
            };
        }

        try {
            if (!File.Exists(ruta)) {
                return new ResultadoArchivoPrueba {
                    RutaRelativa = esperado.RutaRelativa,
                    Estado = Directory.Exists(ruta)
                        ? EstadoArchivoPrueba.TipoInvalido
                        : EstadoArchivoPrueba.Ausente
                };
            }

            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                    directorioCaso,
                    ruta) ||
                File.GetAttributes(ruta).HasFlag(
                    FileAttributes.ReparsePoint)) {
                return new ResultadoArchivoPrueba {
                    RutaRelativa = esperado.RutaRelativa,
                    Estado = EstadoArchivoPrueba.PuntoDeReanalisis
                };
            }

            FileInfo informacion = new(ruta);

            if (informacion.Length > LimiteBytesArchivo) {
                return new ResultadoArchivoPrueba {
                    RutaRelativa = esperado.RutaRelativa,
                    Estado = EstadoArchivoPrueba.ContenidoExcesivo
                };
            }

            return new ResultadoArchivoPrueba {
                RutaRelativa = esperado.RutaRelativa,
                Estado = EstadoArchivoPrueba.Disponible,
                ContenidoObtenido = File.ReadAllText(
                    ruta,
                    Utf8SinBomEstricto)
            };
        } catch (Exception ex) {
            return new ResultadoArchivoPrueba {
                RutaRelativa = esperado.RutaRelativa,
                Estado = EstadoArchivoPrueba.ErrorLectura,
                Error = ex
            };
        }
    }

    private static string ResolverRutaRelativaSegura(
        string directorioCaso,
        string rutaRelativa) {
        string normalizada = NormalizarRutaRelativaConfigurada(rutaRelativa);
        string completa = Path.GetFullPath(normalizada, directorioCaso);

        if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(
                directorioCaso,
                completa) ||
            string.Equals(
                Path.GetFullPath(directorioCaso),
                completa,
                StringComparison.OrdinalIgnoreCase)) {
            throw new InvalidOperationException(
                "La ruta configurada sale del directorio aislado.");
        }

        return completa;
    }

    private static string NormalizarRutaRelativaConfigurada(
        string rutaRelativa) {
        if (string.IsNullOrWhiteSpace(rutaRelativa) ||
            Path.IsPathRooted(rutaRelativa) ||
            rutaRelativa.Contains(':') ||
            rutaRelativa.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            throw new InvalidOperationException(
                "Las rutas de prueba deben ser relativas y seguras.");
        }

        string[] segmentos = rutaRelativa.Split(
            new[] {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            },
            StringSplitOptions.None);

        if (segmentos.Any(segmento =>
            string.IsNullOrWhiteSpace(segmento) ||
            segmento is "." or ".." ||
            segmento.EndsWith(' ') ||
            segmento.EndsWith('.'))) {
            throw new InvalidOperationException(
                "La ruta de prueba contiene segmentos no permitidos.");
        }

        return string.Join(Path.DirectorySeparatorChar, segmentos);
    }

    private static ResultadoEjecucionCasoPruebaCpp CrearResultadoCasoConError(
        EstadoEjecucionPruebaCpp estado,
        Exception? error = null) {
        return new ResultadoEjecucionCasoPruebaCpp {
            Ejecucion = new ResultadoEjecucionPruebaCpp {
                Estado = estado,
                Error = error
            }
        };
    }

    private static void EliminarDirectorioCaso(
        string directorioArtefactos,
        string directorioCaso) {
        try {
            string raiz = Path.GetFullPath(directorioArtefactos);
            string caso = Path.GetFullPath(directorioCaso);

            if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(raiz, caso) ||
                !Path.GetFileName(caso).StartsWith(
                    "caso-archivos-",
                    StringComparison.Ordinal) ||
                !Directory.Exists(caso)) {
                return;
            }

            EliminarArbolCasoSinSeguirEnlaces(caso);
        } catch (Exception) {
            // La limpieza se intenta siempre y nunca oculta el resultado.
        }
    }

    private static void EliminarArbolCasoSinSeguirEnlaces(string directorio) {
        if (File.GetAttributes(directorio).HasFlag(
            FileAttributes.ReparsePoint)) {
            Directory.Delete(directorio, recursive: false);
            return;
        }

        foreach (string entrada in Directory.EnumerateFileSystemEntries(
            directorio,
            "*",
            SearchOption.TopDirectoryOnly)) {
            FileAttributes atributos = File.GetAttributes(entrada);

            if (atributos.HasFlag(FileAttributes.Directory) &&
                !atributos.HasFlag(FileAttributes.ReparsePoint)) {
                EliminarArbolCasoSinSeguirEnlaces(entrada);
            } else if (atributos.HasFlag(FileAttributes.Directory)) {
                Directory.Delete(entrada, recursive: false);
            } else {
                if (atributos.HasFlag(FileAttributes.ReadOnly)) {
                    File.SetAttributes(
                        entrada,
                        atributos & ~FileAttributes.ReadOnly);
                }

                File.Delete(entrada);
            }
        }

        Directory.Delete(directorio, recursive: false);
    }
}

public sealed class ResultadoEjecucionCasoPruebaCpp {
    public ResultadoEjecucionPruebaCpp Ejecucion { get; init; } = new();

    public IReadOnlyList<ResultadoArchivoPrueba> Archivos { get; init; } =
        Array.Empty<ResultadoArchivoPrueba>();
}
