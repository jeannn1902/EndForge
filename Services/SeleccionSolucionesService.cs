using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EndForge.Services;

internal enum EstadoSeleccionSolucionCompatible {
    Exitosa,
    CarpetaInexistente,
    SinSoluciones,
    SinSolucionMarcada,
    MarcadorIlegible,
    MarcadorInvalido,
    SolucionInexistente,
    SolucionFueraDeRaiz,
    NingunaCompatible,
    Ambigua,
    ErrorLectura
}

internal enum MotivoIncompatibilidadSolucion {
    Ninguno,
    SolucionSinProyectoCpp,
    ProyectoInexistente,
    ProyectoFueraDeRaiz,
    ProyectoXmlInvalido,
    ProyectoSinMarcador,
    ProyectoSinClCompile,
    ClCompileSinMarcador,
    ClCompileInexistente,
    ClCompileFueraDeRaiz,
    FiltersXmlInvalido,
    FiltersIncoherente
}

internal sealed class ResultadoSeleccionSolucionCompatible {
    public EstadoSeleccionSolucionCompatible Estado { get; init; }

    public MotivoIncompatibilidadSolucion MotivoIncompatibilidad { get; init; }

    public string RutaSolucion { get; init; } = "";

    public string RutaRelativaSolucion { get; init; } = "";

    public bool UsaSeleccionGuardada { get; init; }

    public Exception? Error { get; init; }
}

public sealed class SeleccionSolucionesService {
    public const string MarcadorPlantilla = "00_Plantilla";
    private const string NombreArchivoSeleccion = ".endforge-solution";
    private const int CaracteresMaximosSeleccion = 4096;
    private const long BytesMaximosSolucion = 2 * 1024 * 1024;
    private const long BytesMaximosXmlProyecto = 8 * 1024 * 1024;
    private const long CaracteresMaximosXmlProyecto = 8 * 1024 * 1024;

    public string[] ObtenerSolucionesOrdenadas(string rutaCarpeta) {
        return Directory
            .EnumerateFiles(rutaCarpeta, "*", SearchOption.TopDirectoryOnly)
            .Where(archivo => Path.GetExtension(archivo).Equals(".sln", StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(Path.GetFileName, StringComparer.Ordinal)
            .ToArray();
    }

    public string ObtenerRutaRelativa(string rutaRaiz, string rutaSolucion) {
        return Path.GetRelativePath(Path.GetFullPath(rutaRaiz), Path.GetFullPath(rutaSolucion));
    }

    public string TransformarRutaRelativa(string rutaRelativa, string nombreProyecto) {
        if (Path.IsPathRooted(rutaRelativa)) {
            throw new ArgumentException("La ruta de la solución debe ser relativa.", nameof(rutaRelativa));
        }

        return rutaRelativa.Replace(MarcadorPlantilla, nombreProyecto, StringComparison.Ordinal);
    }

    public void GuardarSolucionSeleccionada(string rutaCarpeta, string rutaRelativaSolucion) {
        if (!Path.GetExtension(rutaRelativaSolucion).Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
            !IntentarResolverRutaRelativa(rutaCarpeta, rutaRelativaSolucion, out string rutaSolucion) ||
            !File.Exists(rutaSolucion)) {
            throw new FileNotFoundException("No se encontró la solución esperada de la práctica.");
        }

        File.WriteAllText(
            Path.Combine(rutaCarpeta, NombreArchivoSeleccion),
            rutaRelativaSolucion
        );
    }

    public string? LeerSolucionSeleccionada(string rutaCarpeta) {
        string rutaSeleccion = Path.Combine(rutaCarpeta, NombreArchivoSeleccion);

        try {
            return LeerTextoAcotado(
                rutaSeleccion,
                CaracteresMaximosSeleccion
            ).Trim();
        } catch (FileNotFoundException) {
            return null;
        } catch (DirectoryNotFoundException) {
            return null;
        }
    }

    internal ResultadoSeleccionSolucionCompatible SeleccionarSolucionParaPlantilla(
        string rutaPlantilla) {
        if (!Directory.Exists(rutaPlantilla)) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.CarpetaInexistente
            );
        }

        try {
            string raiz = Path.GetFullPath(rutaPlantilla);
            string[] soluciones = ObtenerSolucionesOrdenadas(raiz);

            if (soluciones.Length == 0) {
                return CrearResultadoSeleccion(
                    EstadoSeleccionSolucionCompatible.SinSoluciones
                );
            }

            string[] solucionesMarcadas = soluciones
                .Where(RutaContieneMarcador)
                .ToArray();

            if (solucionesMarcadas.Length == 0) {
                return CrearResultadoSeleccion(
                    EstadoSeleccionSolucionCompatible.SinSolucionMarcada
                );
            }

            List<(string Ruta, ResultadoAnalisisSolucion Analisis)> analisis = solucionesMarcadas
                .Select(solucion => (
                    solucion,
                    AnalizarSolucion(raiz, solucion, exigirMarcadoresPlantilla: true)))
                .ToList();
            List<(string Ruta, ResultadoAnalisisSolucion Analisis)> compatibles =
                analisis
                    .Where(resultado => resultado.Analisis.EsCompatible)
                    .ToList();

            if (compatibles.Count > 0) {
                ResultadoAnalisisSolucion analisisXmlTransformable =
                    AnalizarXmlTransformableDePlantilla(raiz);

                if (!analisisXmlTransformable.EsCompatible) {
                    return CrearResultadoSeleccion(
                        EstadoSeleccionSolucionCompatible.NingunaCompatible,
                        analisisXmlTransformable.Motivo,
                        error: analisisXmlTransformable.Error
                    );
                }

                return CrearResultadoSeleccionExitosa(
                    raiz,
                    compatibles[0].Ruta
                );
            }

            ResultadoAnalisisSolucion primerError = analisis[0].Analisis;
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.NingunaCompatible,
                primerError.Motivo,
                error: primerError.Error
            );
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.ErrorLectura,
                error: ex
            );
        } catch (IOException ex) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.ErrorLectura,
                error: ex
            );
        } catch (Exception ex) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.ErrorLectura,
                error: ex
            );
        }
    }

    private static ResultadoAnalisisSolucion
        AnalizarXmlTransformableDePlantilla(string rutaRaiz) {
        foreach (string rutaXml in
            DirectorioTemporalEvaluacionCpp
                .EnumerarArchivosSinPuntosDeReanalisis(rutaRaiz)
                .Where(ruta => {
                    string extension = Path.GetExtension(ruta);
                    return extension.Equals(
                            ".vcxproj",
                            StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals(
                            ".filters",
                            StringComparison.OrdinalIgnoreCase);
                })) {
            try {
                CargarXmlSeguro(rutaXml);
            } catch (Exception ex) when (
                ex is XmlException or IOException or
                    UnauthorizedAccessException) {
                MotivoIncompatibilidadSolucion motivo =
                    Path.GetExtension(rutaXml).Equals(
                        ".filters",
                        StringComparison.OrdinalIgnoreCase)
                    ? MotivoIncompatibilidadSolucion.FiltersXmlInvalido
                    : MotivoIncompatibilidadSolucion.ProyectoXmlInvalido;

                return ResultadoAnalisisSolucion.Incompatible(
                    motivo,
                    ex);
            }
        }

        return ResultadoAnalisisSolucion.Compatible();
    }

    internal ResultadoSeleccionSolucionCompatible SeleccionarSolucionParaPractica(
        string rutaPractica,
        string? rutaRelativaEsperada,
        bool usarSeleccionGuardada) {
        if (!Directory.Exists(rutaPractica)) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.CarpetaInexistente
            );
        }

        string raiz;

        try {
            raiz = Path.GetFullPath(rutaPractica);
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or PathTooLongException) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.CarpetaInexistente,
                error: ex
            );
        }

        bool usaSeleccionGuardada = false;

        if (usarSeleccionGuardada) {
            string rutaMarcador = Path.Combine(raiz, NombreArchivoSeleccion);

            try {
                File.GetAttributes(rutaMarcador);
                usaSeleccionGuardada = true;
                rutaRelativaEsperada = LeerTextoAcotado(
                    rutaMarcador,
                    CaracteresMaximosSeleccion
                ).Trim();
            } catch (FileNotFoundException) {
                rutaRelativaEsperada = null;
            } catch (DirectoryNotFoundException) {
                rutaRelativaEsperada = null;
            } catch (Exception ex) when (
                ex is IOException or UnauthorizedAccessException or DecoderFallbackException) {
                return CrearResultadoSeleccion(
                    EstadoSeleccionSolucionCompatible.MarcadorIlegible,
                    usaSeleccionGuardada: true,
                    error: ex
                );
            }
        }

        try {
            if (rutaRelativaEsperada is not null) {
                if (string.IsNullOrWhiteSpace(rutaRelativaEsperada) ||
                    rutaRelativaEsperada.Contains('\r', StringComparison.Ordinal) ||
                    rutaRelativaEsperada.Contains('\n', StringComparison.Ordinal) ||
                    Path.IsPathRooted(rutaRelativaEsperada) ||
                    !Path.GetExtension(rutaRelativaEsperada).Equals(
                        ".sln",
                        StringComparison.OrdinalIgnoreCase)) {
                    return CrearResultadoSeleccion(
                        EstadoSeleccionSolucionCompatible.MarcadorInvalido,
                        usaSeleccionGuardada: usaSeleccionGuardada
                    );
                }

                if (!IntentarResolverRutaRelativa(
                    raiz,
                    rutaRelativaEsperada,
                    out string rutaEsperada)) {
                    return CrearResultadoSeleccion(
                        EstadoSeleccionSolucionCompatible.SolucionFueraDeRaiz,
                        usaSeleccionGuardada: usaSeleccionGuardada
                    );
                }

                if (!File.Exists(rutaEsperada)) {
                    return CrearResultadoSeleccion(
                        EstadoSeleccionSolucionCompatible.SolucionInexistente,
                        usaSeleccionGuardada: usaSeleccionGuardada
                    );
                }

                ResultadoAnalisisSolucion analisisEsperado = AnalizarSolucion(
                    raiz,
                    rutaEsperada,
                    exigirMarcadoresPlantilla: false
                );

                if (!analisisEsperado.EsCompatible) {
                    return CrearResultadoSeleccion(
                        EstadoSeleccionSolucionCompatible.NingunaCompatible,
                        analisisEsperado.Motivo,
                        usaSeleccionGuardada,
                        analisisEsperado.Error
                    );
                }

                return CrearResultadoSeleccionExitosa(
                    raiz,
                    rutaEsperada,
                    usaSeleccionGuardada
                );
            }

            string[] soluciones = ObtenerSolucionesOrdenadas(raiz);

            if (soluciones.Length == 0) {
                return CrearResultadoSeleccion(
                    EstadoSeleccionSolucionCompatible.SinSoluciones
                );
            }

            List<(string Ruta, ResultadoAnalisisSolucion Analisis)> compatibles =
                soluciones
                    .Select(solucion => (
                        solucion,
                        AnalizarSolucion(
                            raiz,
                            solucion,
                            exigirMarcadoresPlantilla: false)))
                    .Where(resultado => resultado.Item2.EsCompatible)
                    .ToList();

            if (compatibles.Count == 0) {
                ResultadoAnalisisSolucion primerError = AnalizarSolucion(
                    raiz,
                    soluciones[0],
                    exigirMarcadoresPlantilla: false
                );
                return CrearResultadoSeleccion(
                    EstadoSeleccionSolucionCompatible.NingunaCompatible,
                    primerError.Motivo,
                    error: primerError.Error
                );
            }

            if (compatibles.Count > 1) {
                return CrearResultadoSeleccion(
                    EstadoSeleccionSolucionCompatible.Ambigua
                );
            }

            return CrearResultadoSeleccionExitosa(
                raiz,
                compatibles[0].Ruta
            );
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.ErrorLectura,
                usaSeleccionGuardada: usaSeleccionGuardada,
                error: ex
            );
        } catch (IOException ex) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.ErrorLectura,
                usaSeleccionGuardada: usaSeleccionGuardada,
                error: ex
            );
        } catch (Exception ex) {
            return CrearResultadoSeleccion(
                EstadoSeleccionSolucionCompatible.ErrorLectura,
                usaSeleccionGuardada: usaSeleccionGuardada,
                error: ex
            );
        }
    }

    public ResultadoResolucionProyectoEvaluacionCpp ResolverProyectoParaEvaluacion(
        string rutaPractica) {
        if (!Directory.Exists(rutaPractica)) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.CarpetaInexistente
            );
        }

        string rutaPracticaNormalizada;

        try {
            rutaPracticaNormalizada = Path.GetFullPath(rutaPractica);
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or PathTooLongException) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.CarpetaInexistente,
                error: ex
            );
        }

        try {
            ResultadoSeleccionEvaluacionCpp seleccion =
                ResolverSolucionParaEvaluacion(rutaPracticaNormalizada);

            if (seleccion.Estado != EstadoResolucionProyectoEvaluacionCpp.Exitosa) {
                return CrearResultadoResolucion(
                    seleccion.Estado,
                    usaSeleccionGuardada: seleccion.UsaSeleccionGuardada,
                    error: seleccion.Error
                );
            }

            ResultadoProyectoReferenciadoCpp proyecto = ResolverProyectoCppReferenciado(
                rutaPracticaNormalizada,
                seleccion.RutaSolucion
            );

            return CrearResultadoResolucion(
                proyecto.Estado,
                seleccion.RutaSolucion,
                proyecto.RutaProyectoCpp,
                seleccion.UsaSeleccionGuardada,
                proyecto.Error
            );
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                error: ex
            );
        } catch (IOException ex) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                error: ex
            );
        } catch (Exception ex) {
            return CrearResultadoResolucion(
                EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                error: ex
            );
        }
    }

    public bool IntentarResolverRutaRelativa(
        string rutaRaiz,
        string rutaRelativa,
        out string rutaCompleta) {
        rutaCompleta = "";

        if (string.IsNullOrWhiteSpace(rutaRelativa) || Path.IsPathRooted(rutaRelativa)) {
            return false;
        }

        try {
            string raizNormalizada = Path.GetFullPath(rutaRaiz);
            string rutaNormalizada = Path.GetFullPath(rutaRelativa, raizNormalizada);
            string rutaRelativaNormalizada = Path.GetRelativePath(raizNormalizada, rutaNormalizada);

            if (Path.IsPathRooted(rutaRelativaNormalizada) ||
                rutaRelativaNormalizada.Equals("..", StringComparison.Ordinal) ||
                rutaRelativaNormalizada.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                rutaRelativaNormalizada.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal)) {
                return false;
            }

            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                raizNormalizada,
                rutaNormalizada)) {
                return false;
            }

            rutaCompleta = rutaNormalizada;
            return true;
        } catch (ArgumentException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        } catch (PathTooLongException) {
            return false;
        }
    }

    private ResultadoSeleccionEvaluacionCpp ResolverSolucionParaEvaluacion(
        string rutaPractica) {
        ResultadoSeleccionSolucionCompatible seleccion =
            SeleccionarSolucionParaPractica(
                rutaPractica,
                rutaRelativaEsperada: null,
                usarSeleccionGuardada: true
            );

        EstadoResolucionProyectoEvaluacionCpp estado = seleccion.Estado switch {
            EstadoSeleccionSolucionCompatible.Exitosa =>
                EstadoResolucionProyectoEvaluacionCpp.Exitosa,
            EstadoSeleccionSolucionCompatible.CarpetaInexistente =>
                EstadoResolucionProyectoEvaluacionCpp.CarpetaInexistente,
            EstadoSeleccionSolucionCompatible.MarcadorIlegible =>
                EstadoResolucionProyectoEvaluacionCpp.MarcadorIlegible,
            EstadoSeleccionSolucionCompatible.MarcadorInvalido =>
                EstadoResolucionProyectoEvaluacionCpp.MarcadorInvalido,
            EstadoSeleccionSolucionCompatible.SinSoluciones or
            EstadoSeleccionSolucionCompatible.SolucionInexistente =>
                EstadoResolucionProyectoEvaluacionCpp.SolucionInexistente,
            EstadoSeleccionSolucionCompatible.SolucionFueraDeRaiz =>
                EstadoResolucionProyectoEvaluacionCpp.SolucionFueraDePractica,
            EstadoSeleccionSolucionCompatible.Ambigua =>
                EstadoResolucionProyectoEvaluacionCpp.SolucionAmbigua,
            EstadoSeleccionSolucionCompatible.NingunaCompatible =>
                MapearIncompatibilidadEvaluacion(seleccion.MotivoIncompatibilidad),
            _ => EstadoResolucionProyectoEvaluacionCpp.ErrorLectura
        };

        return new ResultadoSeleccionEvaluacionCpp {
            Estado = estado,
            RutaSolucion = seleccion.RutaSolucion,
            UsaSeleccionGuardada = seleccion.UsaSeleccionGuardada,
            Error = seleccion.Error
        };
    }

    private ResultadoAnalisisSolucion AnalizarSolucion(
        string rutaRaiz,
        string rutaSolucion,
        bool exigirMarcadoresPlantilla) {
        try {
            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                rutaRaiz,
                rutaSolucion)) {
                return ResultadoAnalisisSolucion.Incompatible(
                    MotivoIncompatibilidadSolucion.ProyectoFueraDeRaiz
                );
            }

            if (new FileInfo(rutaSolucion).Length > BytesMaximosSolucion) {
                return ResultadoAnalisisSolucion.Incompatible(
                    MotivoIncompatibilidadSolucion.ProyectoXmlInvalido,
                    new InvalidDataException(
                        "El archivo de solución excede el tamaño admitido.")
                );
            }

            string[] referenciasProyecto =
                ExtraerReferenciasProyectoCpp(rutaSolucion);

            if (referenciasProyecto.Length == 0) {
                return ResultadoAnalisisSolucion.Incompatible(
                    MotivoIncompatibilidadSolucion.SolucionSinProyectoCpp
                );
            }

            string directorioSolucion = Path.GetDirectoryName(rutaSolucion)!;
            bool existeProyectoMarcado = false;
            bool existeClCompileMarcadoEnProyectoMarcado = false;
            bool existeCppReferenciado = false;

            foreach (string referenciaProyecto in referenciasProyecto) {
                if (!IntentarResolverRutaDesdeDirectorio(
                    rutaRaiz,
                    directorioSolucion,
                    referenciaProyecto,
                    out string rutaProyecto)) {
                    return ResultadoAnalisisSolucion.Incompatible(
                        MotivoIncompatibilidadSolucion.ProyectoFueraDeRaiz
                    );
                }

                if (!File.Exists(rutaProyecto)) {
                    return ResultadoAnalisisSolucion.Incompatible(
                        MotivoIncompatibilidadSolucion.ProyectoInexistente
                    );
                }

                bool proyectoMarcado = RutaContieneMarcador(rutaProyecto);
                ResultadoAnalisisProyecto analisisProyecto = AnalizarProyecto(
                    rutaRaiz,
                    rutaProyecto
                );

                if (!analisisProyecto.EsCompatible) {
                    return ResultadoAnalisisSolucion.Incompatible(
                        analisisProyecto.Motivo,
                        analisisProyecto.Error
                    );
                }

                existeCppReferenciado |= analisisProyecto.TieneCppReferenciado;

                if (proyectoMarcado) {
                    existeProyectoMarcado = true;
                    existeClCompileMarcadoEnProyectoMarcado |=
                        analisisProyecto.TieneClCompileMarcado;
                }
            }

            if (!existeCppReferenciado) {
                return ResultadoAnalisisSolucion.Incompatible(
                    MotivoIncompatibilidadSolucion.ProyectoSinClCompile
                );
            }

            if (exigirMarcadoresPlantilla && !existeProyectoMarcado) {
                return ResultadoAnalisisSolucion.Incompatible(
                    MotivoIncompatibilidadSolucion.ProyectoSinMarcador
                );
            }

            if (exigirMarcadoresPlantilla &&
                !existeClCompileMarcadoEnProyectoMarcado) {
                return ResultadoAnalisisSolucion.Incompatible(
                    MotivoIncompatibilidadSolucion.ClCompileSinMarcador
                );
            }

            return ResultadoAnalisisSolucion.Compatible();
        } catch (UnauthorizedAccessException ex) {
            return ResultadoAnalisisSolucion.Incompatible(
                MotivoIncompatibilidadSolucion.ProyectoXmlInvalido,
                ex
            );
        } catch (IOException ex) {
            return ResultadoAnalisisSolucion.Incompatible(
                MotivoIncompatibilidadSolucion.ProyectoXmlInvalido,
                ex
            );
        } catch (Exception ex) {
            return ResultadoAnalisisSolucion.Incompatible(
                MotivoIncompatibilidadSolucion.ProyectoXmlInvalido,
                ex
            );
        }
    }

    private ResultadoAnalisisProyecto AnalizarProyecto(
        string rutaRaiz,
        string rutaProyecto) {
        XDocument proyecto;

        try {
            proyecto = CargarXmlSeguro(rutaProyecto);
        } catch (Exception ex) when (
            ex is XmlException or IOException or UnauthorizedAccessException) {
            return ResultadoAnalisisProyecto.Incompatible(
                MotivoIncompatibilidadSolucion.ProyectoXmlInvalido,
                ex
            );
        }

        string directorioProyecto = Path.GetDirectoryName(rutaProyecto)!;
        string[] referenciasClCompile = ExtraerReferenciasClCompile(proyecto);
        HashSet<string> rutasClCompile = new(StringComparer.OrdinalIgnoreCase);
        bool tieneCpp = false;
        bool tieneMarcador = false;

        foreach (string referencia in referenciasClCompile) {
            if (!IntentarResolverRutaDesdeDirectorio(
                rutaRaiz,
                directorioProyecto,
                referencia,
                out string rutaClCompile)) {
                return ResultadoAnalisisProyecto.Incompatible(
                    MotivoIncompatibilidadSolucion.ClCompileFueraDeRaiz
                );
            }

            if (!File.Exists(rutaClCompile)) {
                return ResultadoAnalisisProyecto.Incompatible(
                    MotivoIncompatibilidadSolucion.ClCompileInexistente
                );
            }

            rutasClCompile.Add(Path.GetFullPath(rutaClCompile));
            tieneCpp |= Path.GetExtension(rutaClCompile).Equals(
                ".cpp",
                StringComparison.OrdinalIgnoreCase);
            tieneMarcador |= referencia.Contains(
                MarcadorPlantilla,
                StringComparison.Ordinal);
        }

        string? rutaFilters = BuscarArchivoFilters(rutaProyecto);

        if (rutaFilters is not null) {
            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                rutaRaiz,
                rutaFilters)) {
                return ResultadoAnalisisProyecto.Incompatible(
                    MotivoIncompatibilidadSolucion.FiltersIncoherente
                );
            }

            XDocument filters;

            try {
                filters = CargarXmlSeguro(rutaFilters);
            } catch (Exception ex) when (
                ex is XmlException or IOException or UnauthorizedAccessException) {
                return ResultadoAnalisisProyecto.Incompatible(
                    MotivoIncompatibilidadSolucion.FiltersXmlInvalido,
                    ex
                );
            }

            foreach (string referenciaFilter in ExtraerReferenciasClCompile(filters)) {
                if (!IntentarResolverRutaDesdeDirectorio(
                    rutaRaiz,
                    directorioProyecto,
                    referenciaFilter,
                    out string rutaFilter) ||
                    !rutasClCompile.Contains(Path.GetFullPath(rutaFilter))) {
                    return ResultadoAnalisisProyecto.Incompatible(
                        MotivoIncompatibilidadSolucion.FiltersIncoherente
                    );
                }
            }
        }

        return ResultadoAnalisisProyecto.Compatible(
            tieneCpp,
            tieneMarcador
        );
    }

    private static string[] ExtraerReferenciasClCompile(XDocument documento) {
        return documento
            .Descendants()
            .Where(elemento => elemento.Name.LocalName.Equals(
                "ClCompile",
                StringComparison.Ordinal))
            .Select(elemento => elemento
                .Attributes()
                .FirstOrDefault(atributo => atributo.Name.LocalName.Equals(
                    "Include",
                    StringComparison.Ordinal))
                ?.Value)
            .Where(referencia => !string.IsNullOrWhiteSpace(referencia))
            .Select(referencia => referencia!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(referencia => referencia, StringComparer.OrdinalIgnoreCase)
            .ThenBy(referencia => referencia, StringComparer.Ordinal)
            .ToArray();
    }

    private static string? BuscarArchivoFilters(string rutaProyecto) {
        string directorio = Path.GetDirectoryName(rutaProyecto)!;
        string nombreEsperado = Path.GetFileName(rutaProyecto) + ".filters";

        return Directory
            .EnumerateFiles(directorio, "*", SearchOption.TopDirectoryOnly)
            .Where(ruta => Path
                .GetFileName(ruta)
                .Equals(nombreEsperado, StringComparison.OrdinalIgnoreCase))
            .OrderBy(ruta => ruta, StringComparer.OrdinalIgnoreCase)
            .ThenBy(ruta => ruta, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static XDocument CargarXmlSeguro(string rutaXml) {
        if (new FileInfo(rutaXml).Length > BytesMaximosXmlProyecto) {
            throw new InvalidDataException(
                "El archivo XML excede el tamaÃ±o admitido.");
        }

        XmlReaderSettings configuracionXml = new() {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument =
                CaracteresMaximosXmlProyecto
        };

        using XmlReader lector = XmlReader.Create(rutaXml, configuracionXml);
        return XDocument.Load(lector, LoadOptions.PreserveWhitespace);
    }

    private static bool IntentarResolverRutaDesdeDirectorio(
        string rutaRaiz,
        string directorioBase,
        string rutaRelativa,
        out string rutaCompleta) {
        rutaCompleta = "";

        if (string.IsNullOrWhiteSpace(rutaRelativa) ||
            Path.IsPathRooted(rutaRelativa)) {
            return false;
        }

        try {
            string raiz = Path.GetFullPath(rutaRaiz);
            string candidata = Path.GetFullPath(rutaRelativa, directorioBase);

            if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(raiz, candidata) ||
                !DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                    raiz,
                    candidata)) {
                return false;
            }

            rutaCompleta = candidata;
            return true;
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or PathTooLongException) {
            return false;
        }
    }

    private static bool RutaContieneMarcador(string ruta) {
        return Path
            .GetFileNameWithoutExtension(ruta)
            .Contains(MarcadorPlantilla, StringComparison.Ordinal);
    }

    private static EstadoResolucionProyectoEvaluacionCpp
        MapearIncompatibilidadEvaluacion(
            MotivoIncompatibilidadSolucion motivo) {
        return motivo switch {
            MotivoIncompatibilidadSolucion.SolucionSinProyectoCpp =>
                EstadoResolucionProyectoEvaluacionCpp.SolucionSinProyectoCpp,
            MotivoIncompatibilidadSolucion.ProyectoInexistente =>
                EstadoResolucionProyectoEvaluacionCpp.ProyectoInexistente,
            MotivoIncompatibilidadSolucion.ProyectoFueraDeRaiz or
            MotivoIncompatibilidadSolucion.ClCompileFueraDeRaiz =>
                EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica,
            _ => EstadoResolucionProyectoEvaluacionCpp.ProyectoInvalido
        };
    }

    private static ResultadoSeleccionSolucionCompatible
        CrearResultadoSeleccionExitosa(
            string rutaRaiz,
            string rutaSolucion,
            bool usaSeleccionGuardada = false) {
        return new ResultadoSeleccionSolucionCompatible {
            Estado = EstadoSeleccionSolucionCompatible.Exitosa,
            RutaSolucion = Path.GetFullPath(rutaSolucion),
            RutaRelativaSolucion = Path.GetRelativePath(
                Path.GetFullPath(rutaRaiz),
                Path.GetFullPath(rutaSolucion)),
            UsaSeleccionGuardada = usaSeleccionGuardada
        };
    }

    private static ResultadoSeleccionSolucionCompatible CrearResultadoSeleccion(
        EstadoSeleccionSolucionCompatible estado,
        MotivoIncompatibilidadSolucion motivo =
            MotivoIncompatibilidadSolucion.Ninguno,
        bool usaSeleccionGuardada = false,
        Exception? error = null) {
        return new ResultadoSeleccionSolucionCompatible {
            Estado = estado,
            MotivoIncompatibilidad = motivo,
            UsaSeleccionGuardada = usaSeleccionGuardada,
            Error = error
        };
    }

    private ResultadoProyectoReferenciadoCpp ResolverProyectoCppReferenciado(
        string rutaPractica,
        string rutaSolucion) {
        if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
            rutaPractica,
            rutaSolucion)) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionFueraDePractica
            };
        }

        if (new FileInfo(rutaSolucion).Length > BytesMaximosSolucion) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoInvalido,
                Error = new InvalidDataException(
                    "El archivo de solución excede el tamaño admitido para evaluación."
                )
            };
        }

        string directorioSolucion = Path.GetDirectoryName(rutaSolucion)!;
        string[] referencias = ExtraerReferenciasProyectoCpp(rutaSolucion);

        if (referencias.Length == 0) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.SolucionSinProyectoCpp
            };
        }

        List<string> proyectos = new();

        foreach (string referencia in referencias) {
            string rutaProyecto;

            try {
                rutaProyecto = Path.GetFullPath(referencia, directorioSolucion);
            } catch (Exception ex) when (
                ex is ArgumentException or NotSupportedException or PathTooLongException) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica,
                    Error = ex
                };
            }

            if (!DirectorioTemporalEvaluacionCpp.EstaDentroDe(rutaPractica, rutaProyecto)) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica
                };
            }

            if (!DirectorioTemporalEvaluacionCpp.EsRutaSinPuntosDeReanalisis(
                rutaPractica,
                rutaProyecto)) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoFueraDePractica
                };
            }

            if (!File.Exists(rutaProyecto)) {
                return new ResultadoProyectoReferenciadoCpp {
                    Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoInexistente
                };
            }

            proyectos.Add(rutaProyecto);
        }

        string[] proyectosAplicacion;

        try {
            proyectosAplicacion = proyectos
                .Where(EsProyectoAplicacion)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(Path.GetFileName, StringComparer.Ordinal)
                .ToArray();
        } catch (XmlException ex) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoInvalido,
                Error = ex
            };
        }

        if (proyectosAplicacion.Length == 0) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoNoEjecutable
            };
        }

        if (proyectosAplicacion.Length == 1) {
            return new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.Exitosa,
                RutaProyectoCpp = proyectosAplicacion[0]
            };
        }

        string nombreSolucion = Path.GetFileNameWithoutExtension(rutaSolucion);
        string[] coincidencias = proyectosAplicacion
            .Where(proyecto => Path
                .GetFileNameWithoutExtension(proyecto)
                .Equals(nombreSolucion, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return coincidencias.Length == 1
            ? new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.Exitosa,
                RutaProyectoCpp = coincidencias[0]
            }
            : new ResultadoProyectoReferenciadoCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ProyectoAmbiguo
            };
    }

    private static string[] ExtraerReferenciasProyectoCpp(string rutaSolucion) {
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

    private static bool EsProyectoAplicacion(string rutaProyecto) {
        XmlReaderSettings configuracionXml = new() {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using XmlReader lector = XmlReader.Create(rutaProyecto, configuracionXml);
        XDocument proyecto = XDocument.Load(lector, LoadOptions.None);

        return proyecto
            .Descendants()
            .Where(elemento => elemento.Name.LocalName.Equals(
                "ConfigurationType",
                StringComparison.Ordinal
            ))
            .Any(elemento => elemento.Value.Trim().Equals(
                "Application",
                StringComparison.OrdinalIgnoreCase
            ));
    }

    private static string LeerTextoAcotado(string rutaArchivo, int caracteresMaximos) {
        using FileStream flujo = new(
            rutaArchivo,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        using StreamReader lector = new(
            flujo,
            new UTF8Encoding(false, true),
            detectEncodingFromByteOrderMarks: true
        );

        char[] contenido = new char[caracteresMaximos + 1];
        int total = 0;

        while (total < contenido.Length) {
            int leidos = lector.Read(contenido, total, contenido.Length - total);

            if (leidos == 0) {
                break;
            }

            total += leidos;
        }

        if (total > caracteresMaximos || lector.Peek() >= 0) {
            throw new InvalidDataException(
                "El archivo de selección de solución excede el tamaño admitido."
            );
        }

        return new string(contenido, 0, total);
    }

    private static ResultadoResolucionProyectoEvaluacionCpp CrearResultadoResolucion(
        EstadoResolucionProyectoEvaluacionCpp estado,
        string rutaSolucion = "",
        string rutaProyectoCpp = "",
        bool usaSeleccionGuardada = false,
        Exception? error = null) {
        return new ResultadoResolucionProyectoEvaluacionCpp {
            Estado = estado,
            RutaSolucion = rutaSolucion,
            RutaProyectoCpp = rutaProyectoCpp,
            UsaSeleccionGuardada = usaSeleccionGuardada,
            Error = error
        };
    }

    private sealed class ResultadoAnalisisSolucion {
        public bool EsCompatible { get; init; }

        public MotivoIncompatibilidadSolucion Motivo { get; init; }

        public Exception? Error { get; init; }

        public static ResultadoAnalisisSolucion Compatible() {
            return new ResultadoAnalisisSolucion {
                EsCompatible = true
            };
        }

        public static ResultadoAnalisisSolucion Incompatible(
            MotivoIncompatibilidadSolucion motivo,
            Exception? error = null) {
            return new ResultadoAnalisisSolucion {
                Motivo = motivo,
                Error = error
            };
        }
    }

    private sealed class ResultadoAnalisisProyecto {
        public bool EsCompatible { get; init; }

        public bool TieneCppReferenciado { get; init; }

        public bool TieneClCompileMarcado { get; init; }

        public MotivoIncompatibilidadSolucion Motivo { get; init; }

        public Exception? Error { get; init; }

        public static ResultadoAnalisisProyecto Compatible(
            bool tieneCppReferenciado,
            bool tieneClCompileMarcado) {
            return new ResultadoAnalisisProyecto {
                EsCompatible = true,
                TieneCppReferenciado = tieneCppReferenciado,
                TieneClCompileMarcado = tieneClCompileMarcado
            };
        }

        public static ResultadoAnalisisProyecto Incompatible(
            MotivoIncompatibilidadSolucion motivo,
            Exception? error = null) {
            return new ResultadoAnalisisProyecto {
                Motivo = motivo,
                Error = error
            };
        }
    }

    private sealed class ResultadoSeleccionEvaluacionCpp {
        public EstadoResolucionProyectoEvaluacionCpp Estado { get; init; }

        public string RutaSolucion { get; init; } = "";

        public bool UsaSeleccionGuardada { get; init; }

        public Exception? Error { get; init; }
    }

    private sealed class ResultadoProyectoReferenciadoCpp {
        public EstadoResolucionProyectoEvaluacionCpp Estado { get; init; }

        public string RutaProyectoCpp { get; init; } = "";

        public Exception? Error { get; init; }
    }
}
