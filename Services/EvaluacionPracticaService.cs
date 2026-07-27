using EndForge.Models;

namespace EndForge.Services;

public sealed class EvaluacionPracticaService {
    private readonly CatalogoEvaluacionesService catalogoEvaluacionesService;
    private readonly CompilacionCppService compilacionCppService;
    private readonly EjecucionPruebasService ejecucionPruebasService;
    private readonly ComparadorSalidaService comparadorSalidaService;

    public EvaluacionPracticaService()
        : this(
            new CatalogoEvaluacionesService(),
            new CompilacionCppService(),
            new EjecucionPruebasService(),
            new ComparadorSalidaService()) {
    }

    public EvaluacionPracticaService(
        CatalogoEvaluacionesService catalogoEvaluacionesService,
        CompilacionCppService compilacionCppService,
        EjecucionPruebasService ejecucionPruebasService,
        ComparadorSalidaService comparadorSalidaService) {
        this.catalogoEvaluacionesService =
            catalogoEvaluacionesService ?? throw new ArgumentNullException(
                nameof(catalogoEvaluacionesService));
        this.compilacionCppService =
            compilacionCppService ?? throw new ArgumentNullException(
                nameof(compilacionCppService));
        this.ejecucionPruebasService =
            ejecucionPruebasService ?? throw new ArgumentNullException(
                nameof(ejecucionPruebasService));
        this.comparadorSalidaService =
            comparadorSalidaService ?? throw new ArgumentNullException(
                nameof(comparadorSalidaService));
    }

    public async Task<ResultadoProcesoEvaluacionPractica> EvaluarAsync(
        SolicitudEvaluacionPractica solicitud,
        IProgress<ProgresoEvaluacionPractica>? progreso = null,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (cancellationToken.IsCancellationRequested) {
            return CrearCancelacion();
        }

        DefinicionEvaluacionPractica? definicion =
            catalogoEvaluacionesService.ObtenerDefinicion(solicitud.PracticaId);

        if (definicion is null) {
            return new ResultadoProcesoEvaluacionPractica {
                Estado = EstadoProcesoEvaluacionPractica.PracticaNoEvaluable,
                Mensaje = "Evaluación automática próximamente para esta práctica."
            };
        }

        Informar(
            progreso,
            EtapaEvaluacionPractica.Preparando,
            "Preparando evaluación…",
            0,
            definicion.CasosPrueba.Count);

        if (string.IsNullOrWhiteSpace(solicitud.RutaProyecto)) {
            return new ResultadoProcesoEvaluacionPractica {
                Estado = EstadoProcesoEvaluacionPractica.ProyectoNoDisponible,
                Mensaje = "No se encontró la ruta del proyecto que se desea evaluar."
            };
        }

        Informar(
            progreso,
            EtapaEvaluacionPractica.Compilando,
            "Compilando…",
            0,
            definicion.CasosPrueba.Count);

        ResultadoCompilacionCpp compilacion;

        try {
            compilacion = await compilacionCppService.CompilarAsync(
                new SolicitudCompilacionCpp {
                    RutaPractica = solicitud.RutaProyecto,
                    Configuracion = "Debug",
                    Plataforma = "x64"
                },
                cancellationToken
            ).ConfigureAwait(false);
        } catch (OperationCanceledException) {
            return CrearCancelacion();
        } catch (Exception ex) {
            return CrearErrorInfraestructura(
                "No fue posible iniciar la compilación de la práctica.",
                ex);
        }

        if (compilacion.Estado == EstadoCompilacionCpp.Cancelada ||
            cancellationToken.IsCancellationRequested) {
            compilacion.Sesion?.Dispose();
            return CrearCancelacion();
        }

        if (compilacion.Estado == EstadoCompilacionCpp.ErrorCompilacion) {
            compilacion.Sesion?.Dispose();
            return CrearEvaluacionConErrorCompilacion(
                definicion,
                solicitud.RutaProyecto,
                compilacion);
        }

        if (!compilacion.EsExitosa || compilacion.Sesion is null) {
            compilacion.Sesion?.Dispose();
            return ClasificarErrorCompilacion(compilacion);
        }

        using SesionCompilacionCpp sesion = compilacion.Sesion;
        List<CasoEvaluado> casosEvaluados = new(definicion.CasosPrueba.Count);

        try {
            for (int indice = 0; indice < definicion.CasosPrueba.Count; indice++) {
                if (cancellationToken.IsCancellationRequested) {
                    return CrearCancelacion();
                }

                CasoPrueba caso = definicion.CasosPrueba[indice];
                Informar(
                    progreso,
                    EtapaEvaluacionPractica.EjecutandoPruebas,
                    $"Ejecutando prueba {indice + 1} de {definicion.CasosPrueba.Count}…",
                    indice,
                    definicion.CasosPrueba.Count);

                ResultadoEjecucionCasoPruebaCpp ejecucionCaso =
                    await ejecucionPruebasService.EjecutarCasoAsync(
                        sesion,
                        caso,
                        cancellationToken
                    ).ConfigureAwait(false);
                ResultadoEjecucionPruebaCpp ejecucion =
                    ejecucionCaso.Ejecucion;

                if (ejecucion.Estado == EstadoEjecucionPruebaCpp.Cancelada ||
                    cancellationToken.IsCancellationRequested) {
                    return CrearCancelacion();
                }

                if (EsErrorInfraestructura(ejecucion.Estado)) {
                    return CrearErrorInfraestructura(
                        ObtenerMensajeErrorEjecucion(ejecucion.Estado),
                        ejecucion.Error);
                }

                casosEvaluados.Add(EvaluarCaso(caso, ejecucionCaso));
            }
        } catch (OperationCanceledException) {
            return CrearCancelacion();
        } catch (Exception ex) {
            return CrearErrorInfraestructura(
                "No fue posible completar la ejecución de las pruebas.",
                ex);
        }

        if (cancellationToken.IsCancellationRequested) {
            return CrearCancelacion();
        }

        Informar(
            progreso,
            EtapaEvaluacionPractica.GenerandoResultado,
            "Generando resultado…",
            definicion.CasosPrueba.Count,
            definicion.CasosPrueba.Count);

        ResultadoEvaluacion resultado = CrearResultadoFinal(
            definicion,
            solicitud.RutaProyecto,
            casosEvaluados);

        return new ResultadoProcesoEvaluacionPractica {
            Estado = EstadoProcesoEvaluacionPractica.Finalizada,
            Resultado = resultado,
            Mensaje = "La evaluación terminó correctamente."
        };
    }

    internal CasoEvaluado EvaluarCaso(
        CasoPrueba caso,
        ResultadoEjecucionCasoPruebaCpp ejecucionCaso) {
        ResultadoEjecucionPruebaCpp ejecucion = ejecucionCaso.Ejecucion;

        if (ejecucion.Estado == EstadoEjecucionPruebaCpp.Exitosa) {
            ResultadoComparacionSalida comparacion =
                CompararResultadoCaso(
                    caso,
                    ejecucion.SalidaEstandar,
                    ejecucionCaso.Archivos);

            return new CasoEvaluado(
                new ResultadoCasoPrueba {
                    CasoPruebaId = caso.Id,
                    Entrada = caso.Entrada,
                    SalidaEsperada = caso.SalidaEsperada,
                    SalidaObtenida = ejecucion.SalidaEstandar,
                    Aprobado = comparacion.EsCorrecta,
                    PuntosObtenidos = comparacion.EsCorrecta
                        ? caso.Puntos
                        : 0,
                    PuntosMaximos = caso.Puntos,
                    Mensaje = comparacion.Mensaje,
                    EsVisible = caso.EsVisible,
                    EjecucionFinalizada = true
                },
                comparacion);
        }

        string mensaje = ejecucion.Estado switch {
            EstadoEjecucionPruebaCpp.TiempoTecnicoExcedido =>
                "El programa no terminó la ejecución. Revisa si algún ciclo o condición puede continuar indefinidamente.",
            EstadoEjecucionPruebaCpp.SalidaExcesiva =>
                "El programa produjo demasiada salida. Revisa si está mostrando información de forma repetitiva.",
            EstadoEjecucionPruebaCpp.CodigoSalidaNoCero =>
                "El programa terminó con un error durante este caso. Revisa el resultado y vuelve a intentarlo.",
            _ => "La prueba no pudo completarse correctamente."
        };

        return new CasoEvaluado(
            new ResultadoCasoPrueba {
                CasoPruebaId = caso.Id,
                Entrada = caso.Entrada,
                SalidaEsperada = caso.SalidaEsperada,
                SalidaObtenida = ejecucion.SalidaEstandar,
                Aprobado = false,
                PuntosObtenidos = 0,
                PuntosMaximos = caso.Puntos,
                Mensaje = mensaje,
                EsVisible = caso.EsVisible,
                EjecucionFinalizada = ejecucion.EjecucionFinalizada
            },
            null);
    }

    internal ResultadoComparacionSalida CompararResultadoCaso(
        CasoPrueba caso,
        string salida,
        IReadOnlyList<ResultadoArchivoPrueba> archivosObtenidos) {
        ArgumentNullException.ThrowIfNull(caso);
        ArgumentNullException.ThrowIfNull(archivosObtenidos);

        ResultadoComparacionSalida comparacion =
            caso.SalidaExactaEsperada is null
                ? TieneReglasDeSalida(caso)
                    ? comparadorSalidaService.Comparar(caso, salida)
                    : CrearComparacionDeSalidaNoRequerida()
                : CompararSalidaExacta(caso, salida);
        ResultadoComparacionArchivos archivos = CompararArchivos(
            caso,
            archivosObtenidos);

        return CombinarComparacionConArchivos(
            caso,
            comparacion,
            archivos);
    }

    private static bool TieneReglasDeSalida(CasoPrueba caso) {
        return caso.TokensObligatorios.Count > 0 ||
            caso.GruposTokensAlternativos.Count > 0 ||
            caso.ValoresNumericosEsperados.Count > 0 ||
            caso.ValoresBooleanosEsperados.Count > 0 ||
            caso.ValoresTextualesEsperados.Count > 0 ||
            caso.SecuenciasEsperadas.Count > 0 ||
            caso.SecuenciasCompuestasEsperadas.Count > 0 ||
            caso.ColeccionesEsperadas.Count > 0 ||
            caso.CadenasEsperadas.Count > 0 ||
            caso.TablasEsperadas.Count > 0 ||
            caso.MatricesEsperadas.Count > 0 ||
            caso.BloquesRegistroEsperados.Count > 0;
    }

    private static ResultadoComparacionSalida
        CrearComparacionDeSalidaNoRequerida() {
        return new ResultadoComparacionSalida {
            EsCorrecta = true,
            CumpleEstructura = true,
            EsSalidaLegible = true,
            ReglasCumplidas = Array.AsReadOnly(new[] {
                "La práctica no requiere una salida de consola específica."
            })
        };
    }

    private ResultadoComparacionArchivos CompararArchivos(
        CasoPrueba caso,
        IReadOnlyList<ResultadoArchivoPrueba> archivosObtenidos) {
        if (caso.ArchivosEsperados.Count == 0) {
            return ResultadoComparacionArchivos.Correcta;
        }

        foreach (ArchivoEsperadoPrueba esperado in caso.ArchivosEsperados) {
            ResultadoArchivoPrueba? obtenido = archivosObtenidos
                .FirstOrDefault(archivo => archivo.RutaRelativa.Equals(
                    esperado.RutaRelativa,
                    StringComparison.OrdinalIgnoreCase));

            if (obtenido is null || !obtenido.Disponible) {
                return new ResultadoComparacionArchivos(
                    false,
                    CrearMensajeArchivoNoDisponible(esperado, obtenido));
            }

            bool coincide;
            string mensaje;

            if (esperado.ModoComparacion ==
                ModoComparacionArchivoPrueba.TextoExacto) {
                coincide = CoincidenTextosExactos(
                    esperado.ContenidoEsperado,
                    obtenido.ContenidoObtenido,
                    esperado.PermitirUnSaltoLineaFinal);
                mensaje = coincide
                    ? string.Empty
                    : $"El contenido de {esperado.RutaRelativa} no coincide exactamente.";
            } else {
                CasoPrueba comparacionContenido = new() {
                    Id = $"{caso.Id}-contenido-archivo",
                    Nombre = esperado.RutaRelativa,
                    Puntos = 1,
                    ComparacionFlexible = true,
                    ModoComparacion = ModoComparacionCaso.Valores,
                    CadenasEsperadas = esperado.CadenasEsperadas,
                    TablasEsperadas = esperado.TablasEsperadas,
                    BloquesRegistroEsperados =
                        esperado.BloquesRegistroEsperados
                };
                ResultadoComparacionSalida resultadoContenido =
                    comparadorSalidaService.Comparar(
                        comparacionContenido,
                        obtenido.ContenidoObtenido);
                coincide = resultadoContenido.EsCorrecta;
                mensaje = coincide
                    ? string.Empty
                    : resultadoContenido.Mensaje;
            }

            if (!coincide) {
                return new ResultadoComparacionArchivos(false, mensaje);
            }
        }

        return ResultadoComparacionArchivos.Correcta;
    }

    private static ResultadoComparacionSalida CompararSalidaExacta(
        CasoPrueba caso,
        string salida) {
        ReglaSalidaExactaPrueba regla = caso.SalidaExactaEsperada!;
        bool coincide = CoincidenTextosExactos(
            regla.ValorEsperado,
            salida,
            regla.PermitirUnSaltoLineaFinal);
        string nombreRegla = "Contenido completo de la salida";

        return new ResultadoComparacionSalida {
            EsCorrecta = coincide,
            CumpleEstructura = coincide,
            EsSalidaLegible = coincide ||
                !string.IsNullOrWhiteSpace(salida),
            ReglasCumplidas = coincide
                ? Array.AsReadOnly(new[] { nombreRegla })
                : Array.Empty<string>(),
            ReglasIncumplidas = coincide
                ? Array.Empty<string>()
                : Array.AsReadOnly(new[] { nombreRegla }),
            Mensaje = coincide
                ? "La salida coincide con el contenido completo del archivo."
                : caso.EsVisible
                    ? "La salida debe conservar exactamente todas las líneas y espacios del archivo."
                    : "La salida del caso oculto no coincide."
        };
    }

    private static bool CoincidenTextosExactos(
        string esperado,
        string obtenido,
        bool permitirUnSaltoFinal) {
        string esperadoNormalizado = NormalizarSaltosLinea(esperado);
        string obtenidoNormalizado = NormalizarSaltosLinea(obtenido);

        if (permitirUnSaltoFinal) {
            esperadoNormalizado = QuitarUnSaltoFinal(
                esperadoNormalizado);
            obtenidoNormalizado = QuitarUnSaltoFinal(
                obtenidoNormalizado);
        }

        return esperadoNormalizado.Equals(
            obtenidoNormalizado,
            StringComparison.Ordinal);
    }

    private static string NormalizarSaltosLinea(string texto) {
        return texto
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }

    private static string QuitarUnSaltoFinal(string texto) {
        return texto.EndsWith('\n')
            ? texto[..^1]
            : texto;
    }

    private static string CrearMensajeArchivoNoDisponible(
        ArchivoEsperadoPrueba esperado,
        ResultadoArchivoPrueba? obtenido) {
        return obtenido?.Estado switch {
            EstadoArchivoPrueba.TipoInvalido =>
                $"{esperado.RutaRelativa} debe ser un archivo regular.",
            EstadoArchivoPrueba.PuntoDeReanalisis =>
                $"{esperado.RutaRelativa} no puede ser un enlace o punto de reanálisis.",
            EstadoArchivoPrueba.ContenidoExcesivo =>
                $"{esperado.RutaRelativa} supera el tamaño permitido.",
            EstadoArchivoPrueba.ErrorLectura =>
                $"No fue posible leer {esperado.RutaRelativa}.",
            _ => $"No se creó el archivo esperado {esperado.RutaRelativa}."
        };
    }

    private static ResultadoComparacionSalida CombinarComparacionConArchivos(
        CasoPrueba caso,
        ResultadoComparacionSalida salida,
        ResultadoComparacionArchivos archivos) {
        if (archivos.Coincide) {
            if (caso.ArchivosEsperados.Count == 0) {
                return salida;
            }

            return new ResultadoComparacionSalida {
                EsCorrecta = salida.EsCorrecta,
                CumpleEstructura = salida.CumpleEstructura,
                EsSalidaLegible = salida.EsSalidaLegible ||
                    !TieneReglasDeSalida(caso),
                TokensFaltantes = salida.TokensFaltantes,
                GruposAlternativosFaltantes =
                    salida.GruposAlternativosFaltantes,
                ValoresNumericos = salida.ValoresNumericos,
                ValoresBooleanos = salida.ValoresBooleanos,
                ValoresTextuales = salida.ValoresTextuales,
                Secuencias = salida.Secuencias,
                SecuenciasCompuestas = salida.SecuenciasCompuestas,
                Colecciones = salida.Colecciones,
                Cadenas = salida.Cadenas,
                Tablas = salida.Tablas,
                Matrices = salida.Matrices,
                BloquesRegistro = salida.BloquesRegistro,
                ReglasCumplidas = salida.ReglasCumplidas
                    .Concat(caso.ArchivosEsperados.Select(archivo =>
                        $"Archivo correcto: {archivo.RutaRelativa}"))
                    .ToArray(),
                ReglasIncumplidas = salida.ReglasIncumplidas,
                ContradiccionesDetectadas =
                    salida.ContradiccionesDetectadas,
                EtiquetasAlternativasReconocidas =
                    salida.EtiquetasAlternativasReconocidas,
                Mensaje = salida.Mensaje
            };
        }

        string reglaArchivo = caso.EsVisible
            ? archivos.Mensaje
            : "Un archivo del caso oculto no coincide.";

        return new ResultadoComparacionSalida {
            EsCorrecta = false,
            CumpleEstructura = false,
            EsSalidaLegible = TieneReglasDeSalida(caso) &&
                salida.EsSalidaLegible,
            TokensFaltantes = salida.TokensFaltantes,
            GruposAlternativosFaltantes =
                salida.GruposAlternativosFaltantes,
            ValoresNumericos = salida.ValoresNumericos,
            ValoresBooleanos = salida.ValoresBooleanos,
            ValoresTextuales = salida.ValoresTextuales,
            Secuencias = salida.Secuencias,
            SecuenciasCompuestas = salida.SecuenciasCompuestas,
            Colecciones = salida.Colecciones,
            Cadenas = salida.Cadenas,
            Tablas = salida.Tablas,
            Matrices = salida.Matrices,
            BloquesRegistro = salida.BloquesRegistro,
            ReglasCumplidas = salida.ReglasCumplidas,
            ReglasIncumplidas = salida.ReglasIncumplidas
                .Concat(new[] { reglaArchivo })
                .ToArray(),
            ContradiccionesDetectadas =
                salida.ContradiccionesDetectadas,
            EtiquetasAlternativasReconocidas =
                salida.EtiquetasAlternativasReconocidas,
            Mensaje = reglaArchivo
        };
    }

    private static ResultadoEvaluacion CrearResultadoFinal(
        DefinicionEvaluacionPractica definicion,
        string rutaProyecto,
        IReadOnlyList<CasoEvaluado> casosEvaluados) {
        int puntosCompilacion = ObtenerPuntosCriterio(
            definicion,
            TipoCriterioEvaluacion.Compilacion);
        int puntosCasos = casosEvaluados.Sum(caso => caso.Resultado.PuntosObtenidos);
        bool cumpleValidacion = casosEvaluados.Count == definicion.CasosPrueba.Count &&
            casosEvaluados.All(caso => caso.Comparacion?.CumpleEstructura == true);
        bool cumpleClaridad = casosEvaluados.Count == definicion.CasosPrueba.Count &&
            casosEvaluados.All(caso => caso.Comparacion?.EsSalidaLegible == true);
        int puntosValidacion = cumpleValidacion
            ? ObtenerPuntosCriterio(definicion, TipoCriterioEvaluacion.Validacion)
            : 0;
        int puntosClaridad = cumpleClaridad
            ? ObtenerPuntosCriterio(definicion, TipoCriterioEvaluacion.CalidadBasica)
            : 0;
        int puntosObtenidos =
            puntosCompilacion + puntosCasos + puntosValidacion + puntosClaridad;
        int calificacion = CalcularCalificacion(
            puntosObtenidos,
            definicion.PuntosMaximos);
        int pruebasSuperadas = casosEvaluados.Count(caso => caso.Resultado.Aprobado);
        string resultadoGeneral = ObtenerResultadoGeneral(calificacion);

        return new ResultadoEvaluacion {
            PracticaId = definicion.PracticaId,
            Fecha = DateTimeOffset.Now,
            Compilo = true,
            EjecucionFinalizada = casosEvaluados.All(caso =>
                caso.Resultado.EjecucionFinalizada),
            Calificacion = calificacion,
            PuntosObtenidos = puntosObtenidos,
            PuntosMaximos = definicion.PuntosMaximos,
            PruebasSuperadas = pruebasSuperadas,
            PruebasTotales = definicion.CasosPrueba.Count,
            PuntosCompilacion = puntosCompilacion,
            PuntosValidacion = puntosValidacion,
            PuntosClaridad = puntosClaridad,
            ResultadoGeneral = resultadoGeneral,
            Resultados = casosEvaluados.Select(caso => caso.Resultado).ToArray(),
            Retroalimentacion = CrearRetroalimentacion(
                calificacion,
                casosEvaluados),
            RutaProyecto = rutaProyecto
        };
    }

    private static ResultadoProcesoEvaluacionPractica CrearEvaluacionConErrorCompilacion(
        DefinicionEvaluacionPractica definicion,
        string rutaProyecto,
        ResultadoCompilacionCpp compilacion) {
        string mensajeDetalle = ExtraerDetalleCompilacion(compilacion);
        List<string> retroalimentacion = new() {
            "Tu proyecto todavía necesita algunos ajustes antes de ejecutar las pruebas.",
            "Revisa los errores de compilación, guarda los cambios y vuelve a evaluar."
        };

        if (!string.IsNullOrWhiteSpace(mensajeDetalle)) {
            retroalimentacion.Add(mensajeDetalle);
        }

        ResultadoEvaluacion resultado = new() {
            PracticaId = definicion.PracticaId,
            Fecha = DateTimeOffset.Now,
            Compilo = false,
            EjecucionFinalizada = false,
            Calificacion = 0,
            PuntosObtenidos = 0,
            PuntosMaximos = definicion.PuntosMaximos,
            PruebasSuperadas = 0,
            PruebasTotales = definicion.CasosPrueba.Count,
            ResultadoGeneral = ObtenerResultadoGeneral(0),
            Retroalimentacion = retroalimentacion.AsReadOnly(),
            RutaProyecto = rutaProyecto
        };

        return new ResultadoProcesoEvaluacionPractica {
            Estado = EstadoProcesoEvaluacionPractica.Finalizada,
            Resultado = resultado,
            Mensaje = "La práctica no compiló. Puedes corregirla y evaluar de nuevo."
        };
    }

    private static ResultadoProcesoEvaluacionPractica ClasificarErrorCompilacion(
        ResultadoCompilacionCpp compilacion) {
        return compilacion.Estado switch {
            EstadoCompilacionCpp.ProyectoNoDisponible =>
                new ResultadoProcesoEvaluacionPractica {
                    Estado = EstadoProcesoEvaluacionPractica.ProyectoNoDisponible,
                    Mensaje = ObtenerMensajeProyectoNoDisponible(
                        compilacion.ResolucionProyecto),
                    Error = compilacion.Error
                },
            EstadoCompilacionCpp.MsBuildNoDisponible or
            EstadoCompilacionCpp.EntornoCompilacionNoDisponible =>
                new ResultadoProcesoEvaluacionPractica {
                    Estado = EstadoProcesoEvaluacionPractica.EntornoNoDisponible,
                    Mensaje = "No está disponible el entorno de compilación de C++. Verifica la instalación de Visual Studio y sus herramientas de C++.",
                    Error = compilacion.Error
                },
            _ => CrearErrorInfraestructura(
                ObtenerMensajeErrorCompilacion(compilacion.Estado),
                compilacion.Error)
        };
    }

    private static bool EsErrorInfraestructura(EstadoEjecucionPruebaCpp estado) {
        return estado is
            EstadoEjecucionPruebaCpp.SesionNoDisponible or
            EstadoEjecucionPruebaCpp.EjecutableInexistente or
            EstadoEjecucionPruebaCpp.DirectorioTrabajoInexistente or
            EstadoEjecucionPruebaCpp.ErrorInicio or
            EstadoEjecucionPruebaCpp.ErrorInfraestructura;
    }

    private static int ObtenerPuntosCriterio(
        DefinicionEvaluacionPractica definicion,
        TipoCriterioEvaluacion tipo) {
        return definicion.Criterios
            .Where(criterio => criterio.Tipo == tipo)
            .Sum(criterio => criterio.PuntosMaximos);
    }

    private static int CalcularCalificacion(int puntosObtenidos, int puntosMaximos) {
        return puntosMaximos <= 0
            ? 0
            : Math.Clamp(
                (int)Math.Round(puntosObtenidos * 100D / puntosMaximos),
                0,
                100);
    }

    private static string ObtenerResultadoGeneral(int calificacion) {
        return calificacion switch {
            100 => "Dominada",
            >= 90 => "Excelente",
            >= 80 => "Buen trabajo",
            >= 70 => "Aprobada",
            _ => "Necesita revisión"
        };
    }

    private static IReadOnlyList<string> CrearRetroalimentacion(
        int calificacion,
        IReadOnlyList<CasoEvaluado> casosEvaluados) {
        List<string> mensajes = new() {
            calificacion switch {
                100 => "Dominaste todos los criterios de esta práctica.",
                >= 90 => "La práctica funciona correctamente. Revisa cualquier detalle pendiente para alcanzar el dominio completo.",
                >= 70 => "Buen avance. Revisa los casos que no pasaron.",
                _ => "Tu proyecto todavía necesita algunos ajustes."
            }
        };

        foreach (CasoEvaluado caso in casosEvaluados.Where(caso => !caso.Resultado.Aprobado)) {
            mensajes.Add(caso.Resultado.Mensaje);
        }

        return mensajes
            .Where(mensaje => !string.IsNullOrWhiteSpace(mensaje))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string ExtraerDetalleCompilacion(ResultadoCompilacionCpp compilacion) {
        string detalle = string.IsNullOrWhiteSpace(compilacion.SalidaError)
            ? compilacion.SalidaEstandar
            : compilacion.SalidaError;

        if (string.IsNullOrWhiteSpace(detalle)) {
            return string.Empty;
        }

        string primeraLinea = detalle
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(linea => linea.Trim())
            .FirstOrDefault(linea => linea.Length > 0) ?? string.Empty;

        const int longitudMaxima = 300;
        return primeraLinea.Length <= longitudMaxima
            ? primeraLinea
            : primeraLinea[..longitudMaxima] + "…";
    }

    private static string ObtenerMensajeProyectoNoDisponible(
        ResultadoResolucionProyectoEvaluacionCpp? resolucion) {
        return resolucion?.Estado switch {
            EstadoResolucionProyectoEvaluacionCpp.CarpetaInexistente =>
                "La carpeta de la práctica ya no existe.",
            EstadoResolucionProyectoEvaluacionCpp.SolucionInexistente =>
                "No se encontró la solución esperada de la práctica.",
            EstadoResolucionProyectoEvaluacionCpp.ProyectoInexistente =>
                "La solución referencia un proyecto de C++ que ya no existe.",
            EstadoResolucionProyectoEvaluacionCpp.MarcadorIlegible or
            EstadoResolucionProyectoEvaluacionCpp.MarcadorInvalido =>
                "No se pudo leer la solución seleccionada para esta práctica.",
            _ => "No fue posible localizar de forma segura el proyecto de C++ que se desea evaluar."
        };
    }

    private static string ObtenerMensajeErrorCompilacion(EstadoCompilacionCpp estado) {
        return estado switch {
            EstadoCompilacionCpp.TiempoExcedido =>
                "La compilación no terminó dentro del límite técnico de protección.",
            EstadoCompilacionCpp.SalidaExcesiva =>
                "La compilación produjo demasiada información y se detuvo de forma segura.",
            EstadoCompilacionCpp.EjecutableNoGenerado =>
                "La compilación terminó, pero no se encontró el ejecutable esperado.",
            EstadoCompilacionCpp.EjecutableAmbiguo =>
                "Se generó más de un ejecutable y no fue posible elegir uno de forma segura.",
            EstadoCompilacionCpp.EjecutableFueraDeDirectorioTemporal =>
                "El ejecutable generado no pertenece al directorio seguro de esta evaluación.",
            _ => "No fue posible completar la compilación por un error del entorno de evaluación."
        };
    }

    private static string ObtenerMensajeErrorEjecucion(EstadoEjecucionPruebaCpp estado) {
        return estado switch {
            EstadoEjecucionPruebaCpp.SesionNoDisponible =>
                "La sesión temporal de compilación ya no está disponible.",
            EstadoEjecucionPruebaCpp.EjecutableInexistente =>
                "El ejecutable de la práctica ya no está disponible.",
            EstadoEjecucionPruebaCpp.DirectorioTrabajoInexistente =>
                "La carpeta de la práctica ya no está disponible.",
            _ => "No fue posible ejecutar la prueba por un error del entorno de evaluación."
        };
    }

    private static ResultadoProcesoEvaluacionPractica CrearCancelacion() {
        return new ResultadoProcesoEvaluacionPractica {
            Estado = EstadoProcesoEvaluacionPractica.Cancelada,
            Mensaje = "Evaluación cancelada"
        };
    }

    private static ResultadoProcesoEvaluacionPractica CrearErrorInfraestructura(
        string mensaje,
        Exception? error) {
        return new ResultadoProcesoEvaluacionPractica {
            Estado = EstadoProcesoEvaluacionPractica.ErrorInfraestructura,
            Mensaje = mensaje,
            Error = error
        };
    }

    private static void Informar(
        IProgress<ProgresoEvaluacionPractica>? progreso,
        EtapaEvaluacionPractica etapa,
        string mensaje,
        int casosCompletados,
        int casosTotales) {
        progreso?.Report(new ProgresoEvaluacionPractica {
            Etapa = etapa,
            Mensaje = mensaje,
            CasosCompletados = casosCompletados,
            CasosTotales = casosTotales
        });
    }

    internal sealed record CasoEvaluado(
        ResultadoCasoPrueba Resultado,
        ResultadoComparacionSalida? Comparacion);

    private sealed record ResultadoComparacionArchivos(
        bool Coincide,
        string Mensaje) {
        public static ResultadoComparacionArchivos Correcta { get; } =
            new(true, string.Empty);
    }
}
