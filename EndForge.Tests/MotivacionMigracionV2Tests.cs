using EndForge.Models;
using EndForge.Services;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EndForge.Tests;

public sealed class MotivacionMigracionV2Tests {
    private const string PracticaUno = "migracion-practica-uno";
    private const string PracticaDos = "migracion-practica-dos";
    private static readonly DateTimeOffset FechaBase =
        new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions OpcionesJson =
        CrearOpcionesJson();

    [Fact]
    public void MigracionV1_PreservaIntegramenteEstadoAnteriorYXp() {
        using EntornoMigracion entorno = new();
        DocumentoV1Prueba anterior = CrearDocumentoV1(conConcesiones: true);
        entorno.EscribirVersion1(anterior);
        entorno.Progreso = CrearProgresoNoDisponible();
        entorno.Historial = CrearHistorialNoDisponible();

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(76, resultado.XpTotalResultante);
        JsonObject migrado = entorno.LeerDocumento();
        Assert.Equal(2, migrado["Version"]!.GetValue<int>());
        Assert.Equal(anterior.ZonaHorariaEstudio,
            migrado["ZonaHorariaEstudio"]!.GetValue<string>());
        Assert.Equal(anterior.UltimoInstanteUtcAceptado,
            migrado["UltimoInstanteUtcAceptado"]!.GetValue<DateTimeOffset>());

        ConcesionXP[] concesiones = migrado["ConcesionesXP"]!
            .Deserialize<ConcesionXP[]>(OpcionesJson)!;
        Assert.Equal(anterior.ConcesionesXP.Count, concesiones.Length);

        for (int indice = 0; indice < concesiones.Length; indice++) {
            ConcesionXP esperada = anterior.ConcesionesXP[indice];
            ConcesionXP actual = concesiones[indice];
            Assert.Equal(esperada.Clave, actual.Clave);
            Assert.Equal(esperada.CantidadXP, actual.CantidadXP);
            Assert.Equal(esperada.FechaUtc, actual.FechaUtc);
            Assert.Equal(esperada.Tipo, actual.Tipo);
            Assert.Equal(esperada.PracticaId, actual.PracticaId);
            Assert.Equal(esperada.TemaId, actual.TemaId);
            Assert.Equal(esperada.GradoId, actual.GradoId);
            Assert.Equal(esperada.EsImportada, actual.EsImportada);
        }

        Assert.Equal(
            anterior.MejorCalificacionReconocidaPorPractica,
            migrado["MejorCalificacionReconocidaPorPractica"]!
                .Deserialize<Dictionary<string, int>>()!);
        Assert.Equal(
            anterior.XPMejoraConcedidoPorPractica,
            migrado["XPMejoraConcedidoPorPractica"]!
                .Deserialize<Dictionary<string, int>>()!);
        JsonObject metadatos = migrado["MetadatosMigracion"]!.AsObject();
        Assert.Equal(anterior.MetadatosMigracion.VersionMigracion,
            metadatos["VersionMigracion"]!.GetValue<int>());
        Assert.Equal(anterior.MetadatosMigracion.MigracionInicialCompletada,
            metadatos["MigracionInicialCompletada"]!.GetValue<bool>());
        Assert.Equal(anterior.MetadatosMigracion.FechaMigracionUtc,
            metadatos["FechaMigracionUtc"]!.GetValue<DateTimeOffset>());
        Assert.Equal(anterior.MetadatosMigracion.ProgresoProcesado,
            metadatos["ProgresoProcesado"]!.GetValue<bool>());
        Assert.Equal(anterior.MetadatosMigracion.HistorialProcesado,
            metadatos["HistorialProcesado"]!.GetValue<bool>());
        Assert.Equal(anterior.MetadatosMigracion.MejorasHistoricasReconocidas,
            metadatos["MejorasHistoricasReconocidas"]!.GetValue<int>());
        Assert.Equal(anterior.MetadatosMigracion.MejorasHistoricasOmitidas,
            metadatos["MejorasHistoricasOmitidas"]!.GetValue<int>());
        Assert.Equal(anterior.MetadatosMigracion.UltimaReconciliacionUtc,
            metadatos["UltimaReconciliacionUtc"]!.GetValue<DateTimeOffset>());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void MigracionV1_ConFuentesNoDisponibles_SeCompletaDespues(
        bool progresoNoDisponible,
        bool historialNoDisponible) {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1());
        entorno.Progreso = progresoNoDisponible
            ? CrearProgresoNoDisponible()
            : CrearCargaProgreso();
        entorno.Historial = historialNoDisponible
            ? CrearHistorialNoDisponible()
            : CrearCargaHistorial();
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion migracion =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, migracion.Estado);
        JsonObject primeraVersion = entorno.LeerDocumento();
        Assert.Equal(2, primeraVersion["Version"]!.GetValue<int>());
        Assert.True(primeraVersion["MetadatosMigracion"]!
            ["HistoriaActividadParcial"]!.GetValue<bool>());

        DateTimeOffset fechaFinalizacion = FechaBase.AddDays(-3);
        DateTimeOffset fechaIntento = FechaBase.AddDays(-1);
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(
                PracticaUno,
                EstadoPracticaCurso.Realizada,
                fechaFinalizacion));
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(PracticaUno, fechaIntento));

        servicio.ReconciliarEstadoActual();

        JsonObject completado = entorno.LeerDocumento();
        JsonObject metadatos = completado["MetadatosMigracion"]!.AsObject();
        Assert.True(metadatos["LogrosHistoricosProcesados"]!.GetValue<bool>());
        Assert.True(metadatos["ActividadHistoricaProcesada"]!.GetValue<bool>());
        Assert.False(metadatos["HistoriaActividadParcial"]!.GetValue<bool>());
        Assert.Equal(
            new[] {
                DateOnly.FromDateTime(fechaFinalizacion.UtcDateTime),
                DateOnly.FromDateTime(fechaIntento.UtcDateTime)
            },
            LeerDias(completado));
    }

    [Fact]
    public void MigracionV1_HistoriaParcial_ImportaSoloIntentosRecuperablesYAdvierte() {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1());
        DateTimeOffset fechaRetenida = FechaBase.AddDays(-2);
        entorno.Historial = new ResultadoCargaHistorialEvaluaciones {
            Estado =
                EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido,
            Historial = new HistorialEvaluaciones {
                Practicas = new[] {
                    CrearHistorial(
                        PracticaUno,
                        fechaRetenida,
                        totalIntentos: 3)
                }
            },
            RegistrosInvalidos = 1,
            Error = new InvalidDataException("Historial parcial simulado.")
        };

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Contains(
            AdvertenciaMotivacion.DatosAcademicosParciales,
            resultado.Resumen.Advertencias);
        JsonObject documento = entorno.LeerDocumento();
        Assert.True(documento["MetadatosMigracion"]!
            ["HistoriaActividadParcial"]!.GetValue<bool>());
        Assert.Equal(
            new[] { DateOnly.FromDateTime(fechaRetenida.UtcDateTime) },
            LeerDias(documento));
    }

    [Fact]
    public void MigracionV1_HistoriaInexistente_NoInventaDias() {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1());
        entorno.Historial = new ResultadoCargaHistorialEvaluaciones {
            Estado = EstadoCargaHistorialEvaluaciones.ArchivoInexistente,
            Historial = new HistorialEvaluaciones()
        };

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Empty(LeerDias(entorno.LeerDocumento()));
        Assert.Equal(0, resultado.Resumen.Racha.RachaActual);
        Assert.Equal(0, resultado.Resumen.Racha.MejorRachaHistorica);
        Assert.Null(resultado.Resumen.Racha.UltimoDiaEstudio);
    }

    [Fact]
    public void MigracionV1_Repetida_NoDuplicaNiReescribeElDocumento() {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1(conConcesiones: true));
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        string despuesDeMigrar = File.ReadAllText(servicio.RutaMotivacion);

        ResultadoProcesamientoMotivacion repetida =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(76, repetida.XpTotalResultante);
        Assert.Equal(
            despuesDeMigrar,
            File.ReadAllText(servicio.RutaMotivacion));
        JsonObject documento = entorno.LeerDocumento();
        Assert.Equal(
            documento["LogrosDesbloqueados"]!.AsArray().Count,
            documento["LogrosDesbloqueados"]!.AsArray()
                .Select(item => item!["LogroId"]!.GetValue<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
    }

    [Theory]
    [InlineData("LogrosDesbloqueados")]
    [InlineData("DiasActividadAcademica")]
    [InlineData("MetadatosMigracion.MigracionVersion2Completada")]
    [InlineData("MetadatosMigracion.ActividadHistoricaProcesada")]
    public void DocumentoV2_SinCampoObligatorio_SeRechazaSinSobrescribir(
        string rutaPropiedad) {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1());
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        JsonObject documento = entorno.LeerDocumento();
        EliminarPropiedad(documento, rutaPropiedad);
        string alterado = documento.ToJsonString(OpcionesJson);
        File.WriteAllText(servicio.RutaMotivacion, alterado);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.Equal(alterado, File.ReadAllText(servicio.RutaMotivacion));
    }

    [Fact]
    public void DocumentoV1_SinCampoObligatorioDeConcesion_NoSeMigra() {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1(conConcesiones: true));
        JsonObject documento = entorno.LeerDocumento();
        JsonObject concesion = documento["ConcesionesXP"]!
            .AsArray()[0]!
            .AsObject();
        Assert.True(concesion.Remove("EsImportada"));
        string alterado = documento.ToJsonString(OpcionesJson);
        File.WriteAllText(entorno.RutaMotivacion, alterado);

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.Equal(alterado, File.ReadAllText(entorno.RutaMotivacion));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void RecuperacionTemporal_VersionUnoOVersionDos_PublicaDocumentoCompleto(
        int versionTemporal) {
        using EntornoMigracion entorno = new();
        DocumentoV1Prueba versionUno = CrearDocumentoV1(conConcesiones: true);
        string contenidoTemporal;

        if (versionTemporal == 1) {
            contenidoTemporal = JsonSerializer.Serialize(
                versionUno,
                OpcionesJson);
        } else {
            entorno.EscribirVersion1(versionUno);
            entorno.CrearServicio().ReconciliarEstadoActual();
            contenidoTemporal = File.ReadAllText(entorno.RutaMotivacion);
            File.Delete(entorno.RutaMotivacion);
        }

        string temporal = Path.Combine(
            entorno.Carpeta,
            $".motivacion-recuperacion-v{versionTemporal}.tmp");
        File.WriteAllText(temporal, contenidoTemporal);

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(76, resultado.XpTotalResultante);
        Assert.True(File.Exists(entorno.RutaMotivacion));
        Assert.False(File.Exists(temporal));
        Assert.Equal(2, entorno.LeerDocumento()["Version"]!.GetValue<int>());
    }

    [Fact]
    public void MigracionConTransicion_RealizaUnaSolaPublicacionAtomica() {
        using EntornoMigracion entorno = new();
        string versionUno = entorno.EscribirVersion1(CrearDocumentoV1());
        ProgresoPractica anterior = CrearProgreso(
            PracticaUno,
            EstadoPracticaCurso.Pendiente,
            fechaFinalizacion: null,
            fechaCreacion: FechaBase.AddDays(-5),
            fechaActualizacion: FechaBase.AddDays(-4));
        ProgresoPractica final = CopiarProgreso(anterior);
        final.Estado = EstadoPracticaCurso.EnProgreso;
        final.RutaProyecto = @"C:\Practicas\Migracion";
        final.FechaActualizacion = FechaBase.AddHours(-2);
        ProgresoCurso progreso = new() {
            Practicas = new List<ProgresoPractica> { final }
        };
        entorno.Progreso = CrearCargaProgreso(final);
        TransicionProgresoPersistida transicion = new() {
            PracticaId = PracticaUno,
            ProgresoAnterior = anterior,
            ProgresoFinal = final,
            PracticaCreada = false,
            VinculoPersistidoAhora = true,
            RealizadaPersistidaAhora = false
        };
        SistemaArchivosFallaEnReemplazoNumero archivos = new(2);

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio(archivos)
            .ProcesarProgresoPersistido(PracticaUno, progreso, transicion);

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(1, archivos.IntentosReemplazo);
        Assert.NotEqual(versionUno, File.ReadAllText(entorno.RutaMotivacion));
        JsonObject documento = entorno.LeerDocumento();
        Assert.Equal(2, documento["Version"]!.GetValue<int>());
        Assert.Contains(
            DateOnly.FromDateTime(final.FechaActualizacion!.Value.UtcDateTime),
            LeerDias(documento));
        Assert.Contains(
            documento["ConcesionesXP"]!.AsArray(),
            item => item!["Clave"]!.GetValue<string>() ==
                $"practica:{PracticaUno}:vinculada");
    }

    [Fact]
    public void MigracionV1_FalloDelPrimerReplace_ConservaOriginalIntacto() {
        using EntornoMigracion entorno = new();
        string original = entorno.EscribirVersion1(
            CrearDocumentoV1(conConcesiones: true));
        SistemaArchivosFallaEnReemplazoNumero archivos = new(1);

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio(archivos)
            .ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.Equal(1, archivos.IntentosReemplazo);
        Assert.Equal(original, File.ReadAllText(entorno.RutaMotivacion));
        Assert.Empty(Directory.EnumerateFiles(
            entorno.Carpeta,
            ".motivacion-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    [Fact]
    public void MigracionV1_ZonaInvalida_ConservaArchivoXpYZona() {
        using EntornoMigracion entorno = new();
        const string zonaInexistente = "EndForge/Zona-Inexistente";
        entorno.EscribirVersion1(CrearDocumentoV1(
            conConcesiones: true,
            zonaHoraria: zonaInexistente));
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(76, resultado.XpTotalResultante);
        Assert.Contains(
            AdvertenciaMotivacion.ZonaHorariaNoDisponible,
            resultado.Resumen.Advertencias);
        JsonObject documento = entorno.LeerDocumento();
        Assert.Equal(2, documento["Version"]!.GetValue<int>());
        Assert.Equal(
            zonaInexistente,
            documento["ZonaHorariaEstudio"]!.GetValue<string>());
        string contenido = File.ReadAllText(servicio.RutaMotivacion);

        servicio.ReconciliarEstadoActual();

        Assert.Equal(contenido, File.ReadAllText(servicio.RutaMotivacion));
    }

    [Fact]
    public void MigracionV1_DiasHistoricos_UsaSoloFinalizacionEIntentosRetenidos() {
        using EntornoMigracion entorno = new();
        entorno.EscribirVersion1(CrearDocumentoV1());
        DateTimeOffset creacionRealizada = FechaBase.AddDays(-20);
        DateTimeOffset actualizacionRealizada = FechaBase.AddDays(-10);
        DateTimeOffset finalizacion = FechaBase.AddDays(-8);
        DateTimeOffset creacionVinculada = FechaBase.AddDays(-18);
        DateTimeOffset actualizacionVinculada = FechaBase.AddDays(-6);
        DateTimeOffset intentoRetenido = FechaBase.AddDays(-4);
        ProgresoPractica realizada = CrearProgreso(
            PracticaUno,
            EstadoPracticaCurso.Realizada,
            finalizacion,
            creacionRealizada,
            actualizacionRealizada);
        ProgresoPractica vinculada = CrearProgreso(
            PracticaDos,
            EstadoPracticaCurso.EnProgreso,
            fechaFinalizacion: null,
            fechaCreacion: creacionVinculada,
            fechaActualizacion: actualizacionVinculada,
            ruta: @"C:\Practicas\Dos");
        entorno.Progreso = CrearCargaProgreso(realizada, vinculada);
        entorno.Historial = new ResultadoCargaHistorialEvaluaciones {
            Estado =
                EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido,
            Historial = new HistorialEvaluaciones {
                Practicas = new[] {
                    CrearHistorial(
                        PracticaUno,
                        intentoRetenido,
                        totalIntentos: 7)
                }
            },
            RegistrosInvalidos = 1
        };

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        DateOnly[] dias = LeerDias(entorno.LeerDocumento());
        Assert.Equal(
            new[] {
                DateOnly.FromDateTime(finalizacion.UtcDateTime),
                DateOnly.FromDateTime(intentoRetenido.UtcDateTime)
            },
            dias);
        Assert.DoesNotContain(
            DateOnly.FromDateTime(creacionRealizada.UtcDateTime),
            dias);
        Assert.DoesNotContain(
            DateOnly.FromDateTime(actualizacionRealizada.UtcDateTime),
            dias);
        Assert.DoesNotContain(
            DateOnly.FromDateTime(creacionVinculada.UtcDateTime),
            dias);
        Assert.DoesNotContain(
            DateOnly.FromDateTime(actualizacionVinculada.UtcDateTime),
            dias);
        Assert.Contains(
            AdvertenciaMotivacion.DatosAcademicosParciales,
            resultado.Resumen.Advertencias);
    }

    private static DocumentoV1Prueba CrearDocumentoV1(
        bool conConcesiones = false,
        string zonaHoraria = "UTC") {
        List<ConcesionXP> concesiones = conConcesiones
            ? new List<ConcesionXP> {
                new() {
                    Clave = $"practica:{PracticaUno}:vinculada",
                    CantidadXP = 10,
                    FechaUtc = FechaBase.AddDays(-20),
                    Tipo = TipoConcesionXP.PracticaVinculada,
                    PracticaId = PracticaUno,
                    EsImportada = false
                },
                new() {
                    Clave = $"practica:{PracticaUno}:realizada",
                    CantidadXP = 25,
                    FechaUtc = FechaBase.AddDays(-19),
                    Tipo = TipoConcesionXP.PracticaRealizada,
                    PracticaId = PracticaUno,
                    EsImportada = true
                },
                new() {
                    Clave = $"practica:{PracticaUno}:aprobada",
                    CantidadXP = 40,
                    FechaUtc = FechaBase.AddDays(-18),
                    Tipo = TipoConcesionXP.EvaluacionAprobada,
                    PracticaId = PracticaUno,
                    EsImportada = false
                },
                new() {
                    Clave = $"practica:{PracticaUno}:mejora:01",
                    CantidadXP = 1,
                    FechaUtc = FechaBase.AddDays(-17),
                    Tipo = TipoConcesionXP.MejoraCalificacion,
                    PracticaId = PracticaUno,
                    EsImportada = true
                }
            }
            : new List<ConcesionXP>();
        return new DocumentoV1Prueba {
            ZonaHorariaEstudio = zonaHoraria,
            ConcesionesXP = concesiones,
            MejorCalificacionReconocidaPorPractica = conConcesiones
                ? new Dictionary<string, int> { [PracticaUno] = 85 }
                : new Dictionary<string, int>(),
            XPMejoraConcedidoPorPractica = conConcesiones
                ? new Dictionary<string, int> { [PracticaUno] = 1 }
                : new Dictionary<string, int>(),
            UltimoInstanteUtcAceptado = FechaBase.AddDays(-1),
            MetadatosMigracion = new MetadatosV1Prueba {
                VersionMigracion = 1,
                MigracionInicialCompletada = true,
                FechaMigracionUtc = FechaBase.AddDays(-30),
                ProgresoProcesado = true,
                HistorialProcesado = false,
                MejorasHistoricasReconocidas = conConcesiones ? 1 : 0,
                MejorasHistoricasOmitidas = 2,
                UltimaReconciliacionUtc = FechaBase.AddDays(-2)
            }
        };
    }

    private static ResultadoCargaProgreso CrearCargaProgreso(
        params ProgresoPractica[] practicas) {
        return new ResultadoCargaProgreso {
            Estado = EstadoCargaProgreso.Exitosa,
            Progreso = new ProgresoCurso {
                Practicas = practicas.ToList()
            }
        };
    }

    private static ResultadoCargaProgreso CrearProgresoNoDisponible() {
        return new ResultadoCargaProgreso {
            Estado = EstadoCargaProgreso.PermisosInsuficientes,
            Error = new UnauthorizedAccessException(
                "Progreso no disponible simulado.")
        };
    }

    private static ResultadoCargaHistorialEvaluaciones CrearCargaHistorial(
        params HistorialPractica[] practicas) {
        return new ResultadoCargaHistorialEvaluaciones {
            Estado = EstadoCargaHistorialEvaluaciones.Exitosa,
            Historial = new HistorialEvaluaciones {
                Practicas = practicas
            }
        };
    }

    private static ResultadoCargaHistorialEvaluaciones
        CrearHistorialNoDisponible() {
        return new ResultadoCargaHistorialEvaluaciones {
            Estado = EstadoCargaHistorialEvaluaciones.ErrorIo,
            Error = new IOException("Historial no disponible simulado.")
        };
    }

    private static ProgresoPractica CrearProgreso(
        string practicaId,
        EstadoPracticaCurso estado,
        DateTimeOffset? fechaFinalizacion,
        DateTimeOffset? fechaCreacion = null,
        DateTimeOffset? fechaActualizacion = null,
        string ruta = "") {
        return new ProgresoPractica {
            PracticaId = practicaId,
            Estado = estado,
            RutaProyecto = ruta,
            FechaCreacion = fechaCreacion ?? FechaBase.AddDays(-10),
            FechaActualizacion = fechaActualizacion ?? FechaBase.AddDays(-5),
            FechaFinalizacion = fechaFinalizacion
        };
    }

    private static HistorialPractica CrearHistorial(
        string practicaId,
        DateTimeOffset fechaIntento,
        int totalIntentos = 1) {
        IntentoPractica intento = new() {
            Id = $"intento-{practicaId}",
            PracticaId = practicaId,
            Fecha = fechaIntento,
            Calificacion = 80,
            ResultadoGeneral = "Aprobada",
            PuntosMaximos = 100,
            RutaProyecto = @"C:\Practicas\Migracion"
        };
        return new HistorialPractica {
            PracticaId = practicaId,
            TotalIntentos = totalIntentos,
            MejorCalificacion = 80,
            UltimaCalificacion = 80,
            FechaUltimoIntento = fechaIntento,
            Intentos = new[] { intento }
        };
    }

    private static ProgresoPractica CopiarProgreso(ProgresoPractica origen) {
        return new ProgresoPractica {
            PracticaId = origen.PracticaId,
            Estado = origen.Estado,
            RutaProyecto = origen.RutaProyecto,
            FechaCreacion = origen.FechaCreacion,
            FechaActualizacion = origen.FechaActualizacion,
            FechaFinalizacion = origen.FechaFinalizacion
        };
    }

    private static DateOnly[] LeerDias(JsonObject documento) {
        return documento["DiasActividadAcademica"]!
            .AsArray()
            .Select(item => DateOnly.Parse(
                item!.GetValue<string>(),
                CultureInfo.InvariantCulture))
            .ToArray();
    }

    private static void EliminarPropiedad(
        JsonObject documento,
        string rutaPropiedad) {
        string[] partes = rutaPropiedad.Split('.');
        JsonObject contenedor = documento;

        for (int indice = 0; indice < partes.Length - 1; indice++) {
            contenedor = contenedor[partes[indice]]!.AsObject();
        }

        Assert.True(contenedor.Remove(partes[^1]));
    }

    private static JsonSerializerOptions CrearOpcionesJson() {
        JsonSerializerOptions opciones = new() {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        opciones.Converters.Add(new JsonStringEnumConverter());
        return opciones;
    }

    private sealed class EntornoMigracion : IDisposable {
        public EntornoMigracion() {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-MigracionV2-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
        }

        public string Carpeta { get; }

        public string RutaMotivacion => Path.Combine(Carpeta, "motivacion.json");

        public ResultadoCargaProgreso Progreso { get; set; } =
            CrearCargaProgreso();

        public ResultadoCargaHistorialEvaluaciones Historial { get; set; } =
            CrearCargaHistorial();

        public string EscribirVersion1(DocumentoV1Prueba documento) {
            string contenido = JsonSerializer.Serialize(documento, OpcionesJson);
            File.WriteAllText(RutaMotivacion, contenido);
            return contenido;
        }

        public JsonObject LeerDocumento() {
            return JsonNode.Parse(File.ReadAllText(RutaMotivacion))!.AsObject();
        }

        public MotivacionService CrearServicio(
            ISistemaArchivosMotivacion? archivos = null) {
            return new MotivacionService(
                Carpeta,
                new TimeProviderFijo(FechaBase),
                CrearCatalogo,
                () => Progreso,
                () => Historial,
                archivos);
        }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }
    }

    private static IReadOnlyList<GradoCurso> CrearCatalogo() {
        PracticaCurso[] practicas = new[] { PracticaUno, PracticaDos }
            .Select((id, indice) => new PracticaCurso {
                Id = id,
                TemaId = "tema-migracion",
                Numero = indice + 1,
                Nombre = id
            })
            .ToArray();
        TemaCurso tema = new() {
            Id = "tema-migracion",
            Numero = 1,
            Nombre = "Tema migracion",
            Practicas = practicas
        };
        return new[] {
            new GradoCurso {
                Id = "grado-migracion",
                Numero = 1,
                Nombre = "Grado migracion",
                EsContenidoDisponible = true,
                Temas = new[] { tema }
            }
        };
    }

    private sealed class TimeProviderFijo : TimeProvider {
        private readonly DateTimeOffset ahora;

        public TimeProviderFijo(DateTimeOffset ahora) {
            this.ahora = ahora;
        }

        public override DateTimeOffset GetUtcNow() => ahora;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class SistemaArchivosFallaEnReemplazoNumero :
        ISistemaArchivosMotivacion {
        private readonly SistemaArchivosMotivacion real = new();
        private readonly int intentoConFallo;

        public SistemaArchivosFallaEnReemplazoNumero(int intentoConFallo) {
            this.intentoConFallo = intentoConFallo;
        }

        public int IntentosReemplazo { get; private set; }

        public bool ArchivoExiste(string ruta) => real.ArchivoExiste(ruta);
        public long ObtenerLongitud(string ruta) => real.ObtenerLongitud(ruta);
        public string LeerTodoTexto(string ruta) => real.LeerTodoTexto(ruta);
        public void CrearDirectorio(string ruta) => real.CrearDirectorio(ruta);
        public void EscribirTodoTextoDurable(string ruta, string contenido) =>
            real.EscribirTodoTextoDurable(ruta, contenido);

        public void Reemplazar(string origen, string destino) {
            IntentosReemplazo++;

            if (IntentosReemplazo == intentoConFallo) {
                throw new IOException(
                    $"Fallo simulado en Replace {IntentosReemplazo}.");
            }

            real.Reemplazar(origen, destino);
        }

        public void Mover(string origen, string destino) =>
            real.Mover(origen, destino);
        public void Eliminar(string ruta) => real.Eliminar(ruta);
        public IEnumerable<string> EnumerarArchivos(
            string carpeta,
            string patron) => real.EnumerarArchivos(carpeta, patron);
        public DateTime ObtenerUltimaEscrituraUtc(string ruta) =>
            real.ObtenerUltimaEscrituraUtc(ruta);
    }

    private sealed class DocumentoV1Prueba {
        public int Version { get; set; } = 1;

        public string ZonaHorariaEstudio { get; set; } = "UTC";

        public List<ConcesionXP> ConcesionesXP { get; set; } = new();

        public Dictionary<string, int> MejorCalificacionReconocidaPorPractica {
            get;
            set;
        } = new();

        public Dictionary<string, int> XPMejoraConcedidoPorPractica {
            get;
            set;
        } = new();

        public DateTimeOffset UltimoInstanteUtcAceptado { get; set; }

        public MetadatosV1Prueba MetadatosMigracion { get; set; } = new();
    }

    private sealed class MetadatosV1Prueba {
        public int VersionMigracion { get; set; }

        public bool MigracionInicialCompletada { get; set; }

        public DateTimeOffset FechaMigracionUtc { get; set; }

        public bool ProgresoProcesado { get; set; }

        public bool HistorialProcesado { get; set; }

        public int MejorasHistoricasReconocidas { get; set; }

        public int MejorasHistoricasOmitidas { get; set; }

        public DateTimeOffset? UltimaReconciliacionUtc { get; set; }
    }
}
