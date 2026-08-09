using EndForge.Models;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EndForge.Services;

public sealed class MotivacionService {
    private const int VersionAnterior = 1;
    private const int VersionActual = 2;
    private const int VersionMigracionActual = 1;
    private const int MaximoConcesiones = 50_000;
    private const int MaximoEstadosPorPractica = 10_000;
    private const int MaximoLogros = 512;
    private const int MaximoDiasActividad = 36_600;
    private const int MaximoLongitudIdentificador = 256;
    private const int MaximoLongitudClave = 1_024;
    private const long MaximoBytesArchivo = 16L * 1024L * 1024L;
    private static readonly TimeSpan EsperaMutex = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ToleranciaRetrocesoReloj =
        TimeSpan.FromMinutes(5);

    private readonly string carpetaDatos;
    private readonly string nombreMutex;
    private readonly TimeProvider reloj;
    private readonly Func<IReadOnlyList<GradoCurso>> cargarCatalogo;
    private readonly Func<ResultadoCargaProgreso> cargarProgreso;
    private readonly Func<ResultadoCargaHistorialEvaluaciones> cargarHistorial;
    private readonly CalculadoraNivelService calculadoraNivel;
    private readonly CalculadoraRachaService calculadoraRacha;
    private readonly CatalogoLogrosService catalogoLogros;
    private readonly ISistemaArchivosMotivacion archivos;
    private readonly JsonSerializerOptions opcionesJson;

    public string RutaMotivacion { get; }

    public MotivacionService()
        : this(
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "EndForge"),
            TimeProvider.System) {
    }

    internal MotivacionService(string carpetaDatos, TimeProvider reloj)
        : this(carpetaDatos, reloj, CrearDependencias(carpetaDatos)) {
    }

    private MotivacionService(
        string carpetaDatos,
        TimeProvider reloj,
        DependenciasPredeterminadas dependencias)
        : this(
            carpetaDatos,
            reloj,
            dependencias.CargarCatalogo,
            dependencias.CargarProgreso,
            dependencias.CargarHistorial) {
    }

    internal MotivacionService(
        string carpetaDatos,
        TimeProvider reloj,
        Func<IReadOnlyList<GradoCurso>> cargarCatalogo,
        Func<ResultadoCargaProgreso> cargarProgreso,
        Func<ResultadoCargaHistorialEvaluaciones> cargarHistorial,
        ISistemaArchivosMotivacion? archivos = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(carpetaDatos);
        this.carpetaDatos = Path.GetFullPath(carpetaDatos);
        this.reloj = reloj ?? throw new ArgumentNullException(nameof(reloj));
        this.cargarCatalogo = cargarCatalogo ??
            throw new ArgumentNullException(nameof(cargarCatalogo));
        this.cargarProgreso = cargarProgreso ??
            throw new ArgumentNullException(nameof(cargarProgreso));
        this.cargarHistorial = cargarHistorial ??
            throw new ArgumentNullException(nameof(cargarHistorial));
        this.archivos = archivos ?? new SistemaArchivosMotivacion();
        calculadoraNivel = new CalculadoraNivelService();
        calculadoraRacha = new CalculadoraRachaService(reloj);
        catalogoLogros = new CatalogoLogrosService();
        RutaMotivacion = Path.Combine(this.carpetaDatos, "motivacion.json");
        nombreMutex = CrearNombreMutex(RutaMotivacion);
        opcionesJson = CrearOpcionesJson();
    }

    public ResultadoProcesamientoMotivacion ProcesarVinculoPractica(
        string practicaId) {
        return EjecutarOperacion(
            TipoOperacionMotivacion.VinculoPractica,
            practicaId,
            evidencia: null);
    }

    public ResultadoProcesamientoMotivacion ProcesarPracticaRealizada(
        string practicaId) {
        return EjecutarOperacion(
            TipoOperacionMotivacion.PracticaRealizada,
            practicaId,
            evidencia: null);
    }

    public ResultadoProcesamientoMotivacion ProcesarProgresoPersistido(
        string practicaId,
        ProgresoCurso progresoPersistido) {
        ArgumentNullException.ThrowIfNull(progresoPersistido);
        ProgresoPractica? practica = progresoPersistido.Practicas
            .FirstOrDefault(item => item.PracticaId.Equals(
                practicaId,
                StringComparison.OrdinalIgnoreCase));
        return ProcesarProgresoPersistido(
            practicaId,
            progresoPersistido,
            vinculoPersistidoAhora:
                !string.IsNullOrWhiteSpace(practica?.RutaProyecto),
            realizadaPersistidaAhora:
                practica?.Estado == EstadoPracticaCurso.Realizada);
    }

    internal ResultadoProcesamientoMotivacion ProcesarProgresoPersistido(
        string practicaId,
        ProgresoCurso progresoPersistido,
        bool vinculoPersistidoAhora,
        bool realizadaPersistidaAhora) {
        ArgumentNullException.ThrowIfNull(progresoPersistido);
        return EjecutarOperacion(
            TipoOperacionMotivacion.ProgresoPersistido,
            practicaId,
            new EvidenciaOperacion(
                CopiarProgreso(progresoPersistido),
                null,
                null,
                vinculoPersistidoAhora,
                realizadaPersistidaAhora));
    }

    internal ResultadoProcesamientoMotivacion ProcesarProgresoPersistido(
        string practicaId,
        ProgresoCurso progresoPersistido,
        TransicionProgresoPersistida transicionPersistida) {
        ArgumentNullException.ThrowIfNull(progresoPersistido);
        ArgumentNullException.ThrowIfNull(transicionPersistida);
        return EjecutarOperacion(
            TipoOperacionMotivacion.ProgresoPersistido,
            practicaId,
            new EvidenciaOperacion(
                CopiarProgreso(progresoPersistido),
                null,
                null,
                false,
                false,
                CopiarTransicionProgreso(transicionPersistida),
                null));
    }

    public ResultadoProcesamientoMotivacion ProcesarEvaluacionPersistida(
        string practicaId) {
        return EjecutarOperacion(
            TipoOperacionMotivacion.EvaluacionPersistida,
            practicaId,
            evidencia: null);
    }

    internal ResultadoProcesamientoMotivacion ProcesarEvaluacionPersistida(
        string practicaId,
        HistorialPractica historialPersistido) {
        ArgumentNullException.ThrowIfNull(historialPersistido);
        return EjecutarOperacion(
            TipoOperacionMotivacion.EvaluacionPersistida,
            practicaId,
            new EvidenciaOperacion(
                null,
                CopiarHistorial(historialPersistido),
                null,
                false,
                false));
    }

    internal ResultadoProcesamientoMotivacion ProcesarEvaluacionPersistida(
        string practicaId,
        HistorialPractica historialPersistido,
        IntentoPractica intentoPersistido) {
        ArgumentNullException.ThrowIfNull(historialPersistido);
        ArgumentNullException.ThrowIfNull(intentoPersistido);
        TransicionEvaluacionPersistida? transicion =
            IntentarCrearTransicionEvaluacionCompatibilidad(
                practicaId,
                historialPersistido,
                intentoPersistido,
                out TransicionEvaluacionPersistida resultadoTransicion)
                ? resultadoTransicion
                : null;
        return EjecutarOperacion(
            TipoOperacionMotivacion.EvaluacionPersistida,
            practicaId,
            new EvidenciaOperacion(
                null,
                CopiarHistorial(historialPersistido),
                CopiarIntento(intentoPersistido),
                false,
                false,
                null,
                transicion));
    }

    internal ResultadoProcesamientoMotivacion ProcesarEvaluacionPersistida(
        string practicaId,
        HistorialPractica historialPersistido,
        TransicionEvaluacionPersistida transicionPersistida) {
        ArgumentNullException.ThrowIfNull(historialPersistido);
        ArgumentNullException.ThrowIfNull(transicionPersistida);
        return EjecutarOperacion(
            TipoOperacionMotivacion.EvaluacionPersistida,
            practicaId,
            new EvidenciaOperacion(
                null,
                CopiarHistorial(historialPersistido),
                null,
                false,
                false,
                null,
                CopiarTransicionEvaluacion(transicionPersistida)));
    }

    public ResultadoProcesamientoMotivacion ReconciliarEstadoActual() {
        return EjecutarOperacion(
            TipoOperacionMotivacion.Reconciliacion,
            practicaId: null,
            evidencia: null);
    }

    public ResumenMotivacion ObtenerResumenMotivacion() {
        return ReconciliarEstadoActual().Resumen;
    }

    private ResultadoProcesamientoMotivacion EjecutarOperacion(
        TipoOperacionMotivacion operacion,
        string? practicaId,
        EvidenciaOperacion? evidencia) {
        string? identificador = practicaId?.Trim();

        if (operacion != TipoOperacionMotivacion.Reconciliacion &&
            !EsIdentificadorValido(identificador)) {
            return CrearResultadoNoDisponible(
                EstadoProcesamientoMotivacion.SinRecompensa,
                new ArgumentException(
                    "El identificador de práctica no es válido.",
                    nameof(practicaId)));
        }

        if (evidencia?.TransicionEvaluacion is not null &&
            (evidencia.Historial is null ||
                !EsTransicionEvaluacionValida(
                    identificador!,
                    evidencia.Historial,
                    evidencia.TransicionEvaluacion))) {
            return CrearResultadoNoDisponible(
                EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
                new InvalidDataException(
                    "La transición de evaluación no representa una publicación válida."));
        }

        if (evidencia?.TransicionProgreso is not null) {
            ProgresoPractica? progreso = evidencia.Progreso?.Practicas
                .FirstOrDefault(item => item.PracticaId.Equals(
                    identificador!,
                    StringComparison.OrdinalIgnoreCase));

            if (progreso is null ||
                !EsTransicionProgresoValida(
                    identificador!,
                    progreso,
                    evidencia.TransicionProgreso)) {
                return CrearResultadoNoDisponible(
                    EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
                    new InvalidDataException(
                        "La transición de progreso no representa una publicación válida."));
            }
        }

        Mutex? mutex = null;
        bool adquirido = false;

        try {
            mutex = new Mutex(initiallyOwned: false, nombreMutex);

            try {
                adquirido = mutex.WaitOne(EsperaMutex);
            } catch (AbandonedMutexException) {
                adquirido = true;
            }

            if (!adquirido) {
                return CrearResultadoNoDisponible(
                    EstadoProcesamientoMotivacion.ErrorRecuperable,
                    new TimeoutException(
                        "Otra instancia de EndForge está actualizando motivacion.json."));
            }

            archivos.CrearDirectorio(carpetaDatos);
            RecuperarInterrupcionSinBloqueo();
            ResultadoCargaDocumento carga = CargarDocumentoSinBloqueo(
                RutaMotivacion);

            if (carga.Estado == EstadoCargaDocumento.VersionIncompatible) {
                return CrearResultadoNoDisponible(
                    EstadoProcesamientoMotivacion.VersionIncompatible,
                    carga.Error,
                    versionIncompatible: true);
            }

            if (carga.Estado is EstadoCargaDocumento.ContenidoInvalido or
                EstadoCargaDocumento.PermisosInsuficientes or
                EstadoCargaDocumento.ErrorIo) {
                return CrearResultadoNoDisponible(
                    EstadoProcesamientoMotivacion.ErrorRecuperable,
                    carga.Error);
            }

            ContextoMutacion contexto = new();
            DocumentoMotivacion documento;
            bool documentoNuevo = carga.Estado == EstadoCargaDocumento.ArchivoInexistente;
            bool migradoDesdeVersion1 =
                carga.Estado == EstadoCargaDocumento.RequiereMigracion;

            if (migradoDesdeVersion1) {
                documento = ConvertirDocumentoVersion1(
                    carga.DocumentoVersion1!,
                    ObtenerAhoraUtc());
                long xpVersion1 = CalcularXpTotal(documento);
                CompletarMigracionVersion2(
                    documento,
                    contexto,
                    identificador,
                    evidencia);

                if (CalcularXpTotal(documento) != xpVersion1) {
                    throw new InvalidDataException(
                        "La migracion Version 1 a Version 2 intento modificar el XP.");
                }

                if (operacion == TipoOperacionMotivacion.Reconciliacion) {
                    GuardarDocumentoSinBloqueo(
                        documento,
                        ordenarConcesiones: false);
                    ResumenMotivacion resumenMigrado = CrearResumen(
                        documento,
                        contexto.Advertencias);
                    long nivelMigrado = resumenMigrado.Nivel!.NivelActual;
                    return new ResultadoProcesamientoMotivacion(
                        EstadoProcesamientoMotivacion.Aplicada,
                        0,
                        xpVersion1,
                        nivelMigrado,
                        nivelMigrado,
                        false,
                        Array.Empty<string>(),
                        resumenMigrado,
                        contexto.ErrorFuente);
                }

                contexto.FuenteNoDisponible = false;
                contexto.InstanteConcesionUtc = null;
                contexto.InstanteReconocimientoLogrosUtc = null;
            } else if (documentoNuevo) {
                ResultadoCreacionDocumento migracion = CrearDocumentoMigrado(
                    contexto,
                    identificador,
                    evidencia);

                if (migracion.Documento is null) {
                    return CrearResultadoNoDisponible(
                        migracion.Estado,
                        migracion.Error);
                }

                documento = migracion.Documento;
                contexto.HuboCambio = true;
            } else {
                documento = carga.Documento!;
            }

            contexto.ReportarSoloConcesionesOperacionActual =
                operacion != TipoOperacionMotivacion.Reconciliacion &&
                (documentoNuevo ||
                    operacion == TipoOperacionMotivacion.ProgresoPersistido ||
                    evidencia?.Intento is not null ||
                    evidencia?.TransicionEvaluacion is not null);

            PrepararInstanteOperacion(documento, contexto);
            ImportarLogrosDesdeConcesiones(
                documento,
                contexto,
                esImportado: true);
            long xpAntesDeAplicar = documentoNuevo
                ? 0
                : CalcularXpTotal(documento);
            EstadoAplicacionOperacion estadoAplicacion = AplicarOperacion(
                documento,
                contexto,
                operacion,
                identificador,
                evidencia);
            ProcesarVersion2OperacionActual(
                documento,
                contexto,
                operacion,
                identificador,
                evidencia);
            ReclasificarConcesionesCreadasEnOperacionActual(documento, contexto);

            if (contexto.FuenteNoDisponible && !contexto.HuboCambio) {
                ResumenMotivacion resumenDisponible =
                    CrearResumen(documento, contexto.Advertencias);
                long xpBaseDisponible =
                    contexto.ReportarSoloConcesionesOperacionActual
                        ? resumenDisponible.XpTotal ?? 0
                        : xpAntesDeAplicar;
                long nivelBaseDisponible = calculadoraNivel
                    .Calcular(xpBaseDisponible)
                    .NivelActual;
                IReadOnlyList<string> clavesDisponibles =
                    ObtenerClavesResultado(contexto);
                return new ResultadoProcesamientoMotivacion(
                    EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
                    0,
                    resumenDisponible.XpTotal,
                    nivelBaseDisponible,
                    resumenDisponible.Nivel?.NivelActual,
                    false,
                    clavesDisponibles,
                    resumenDisponible,
                    contexto.ErrorFuente);
            }

            if (contexto.HuboCambio) {
                ActualizarInstanteAceptado(documento, contexto);
                documento.MetadatosMigracion.UltimaReconciliacionUtc =
                    documento.UltimoInstanteUtcAceptado;
                GuardarDocumentoSinBloqueo(
                    documento,
                    ordenarConcesiones: !migradoDesdeVersion1);
            }

            long xpActual = CalcularXpTotal(documento);
            ResumenMotivacion resumen = CrearResumen(documento, contexto.Advertencias);
            long nivelActual = resumen.Nivel!.NivelActual;
            long xpConcedido = contexto.ReportarSoloConcesionesOperacionActual
                ? CalcularXpConcedidoOperacionActual(documento, contexto)
                : checked(xpActual - xpAntesDeAplicar);
            long xpBaseOperacion = checked(xpActual - xpConcedido);
            long nivelAnterior = calculadoraNivel
                .Calcular(xpBaseOperacion)
                .NivelActual;
            IReadOnlyList<string> clavesResultado =
                ObtenerClavesResultado(contexto);
            EstadoProcesamientoMotivacion estado = xpConcedido > 0 ||
                documentoNuevo ||
                contexto.HuboCambioVersion2OperacionActual ||
                estadoAplicacion == EstadoAplicacionOperacion.Aplicada
                ? EstadoProcesamientoMotivacion.Aplicada
                : estadoAplicacion == EstadoAplicacionOperacion.YaAplicada
                    ? EstadoProcesamientoMotivacion.YaAplicada
                    : EstadoProcesamientoMotivacion.SinRecompensa;

            return new ResultadoProcesamientoMotivacion(
                estado,
                xpConcedido,
                xpActual,
                nivelAnterior,
                nivelActual,
                nivelActual > nivelAnterior,
                clavesResultado,
                resumen,
                contexto.ErrorFuente) {
                LogrosNuevos = contexto.LogrosNuevos
                    .Select(CopiarLogro)
                    .ToArray()
            };
        } catch (UnauthorizedAccessException ex) {
            return CrearResultadoNoDisponible(
                EstadoProcesamientoMotivacion.ErrorRecuperable,
                ex);
        } catch (SecurityException ex) {
            return CrearResultadoNoDisponible(
                EstadoProcesamientoMotivacion.ErrorRecuperable,
                ex);
        } catch (IOException ex) {
            return CrearResultadoNoDisponible(
                EstadoProcesamientoMotivacion.ErrorRecuperable,
                ex);
        } catch (Exception ex) when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return CrearResultadoNoDisponible(
                EstadoProcesamientoMotivacion.ErrorRecuperable,
                ex);
        } finally {
            if (adquirido && mutex is not null) {
                try {
                    mutex.ReleaseMutex();
                } catch (Exception) {
                    // El resultado de la operación tiene prioridad.
                }
            }

            mutex?.Dispose();
        }
    }

    private ResultadoCreacionDocumento CrearDocumentoMigrado(
        ContextoMutacion contexto,
        string? practicaId,
        EvidenciaOperacion? evidencia) {
        ResultadoFuentes fuentes = CargarFuentesAcademicas();

        if (!fuentes.Disponibles) {
            return new ResultadoCreacionDocumento(
                null,
                EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
                fuentes.Error);
        }

        fuentes = PrepararFuentesHistoricasParaOperacionActual(
            fuentes,
            practicaId,
            evidencia);

        DateTimeOffset ahora = ObtenerAhoraUtc();
        DocumentoMotivacion documento = new() {
            Version = VersionActual,
            ZonaHorariaEstudio = reloj.LocalTimeZone.Id,
            UltimoInstanteUtcAceptado = ahora,
            MetadatosMigracion = new MetadatosMigracionMotivacion {
                VersionMigracion = VersionMigracionActual,
                MigracionInicialCompletada = true,
                FechaMigracionUtc = ahora,
                ProgresoProcesado = true,
                HistorialProcesado = true,
                UltimaReconciliacionUtc = ahora,
                MigracionVersion2Completada = true,
                FechaMigracionVersion2Utc = ahora,
                LogrosHistoricosProcesados = true,
                ActividadHistoricaProcesada = true,
                HistoriaActividadParcial = false
            }
        };
        contexto.InstanteConcesionUtc = ahora;

        AplicarReconciliacionCompleta(
            documento,
            fuentes,
            contexto,
            esImportada: true,
            esMigracionInicial: true);
        return new ResultadoCreacionDocumento(
            documento,
            EstadoProcesamientoMotivacion.Aplicada,
            null);
    }

    private void CompletarMigracionVersion2(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string? practicaId,
        EvidenciaOperacion? evidencia) {
        ResultadoFuentes fuentes = PrepararFuentesHistoricasParaOperacionActual(
            CargarFuentesAcademicas(),
            practicaId,
            evidencia);
        contexto.InstanteReconocimientoLogrosUtc =
            documento.MetadatosMigracion.FechaMigracionVersion2Utc;
        ReconciliarDatosVersion2Historicos(documento, fuentes, contexto);
        contexto.HuboCambio = true;
    }

    private void ReconciliarDatosVersion2Historicos(
        DocumentoMotivacion documento,
        ResultadoFuentes fuentes,
        ContextoMutacion contexto) {
        ImportarLogrosDesdeFuentes(documento, fuentes, contexto);
        ImportarDiasHistoricos(documento, fuentes, contexto);

        bool fuentesCompletas = fuentes.Catalogo is not null &&
            fuentes.ProgresoDisponible &&
            fuentes.HistorialDisponible;
        bool historiaParcial = !fuentesCompletas ||
            fuentes.DatosParciales ||
            fuentes.Progreso.Values.Any(item =>
                !string.IsNullOrWhiteSpace(item.RutaProyecto)) ||
            fuentes.Historial.Values.Any(item =>
                item.TotalIntentos > item.Intentos.Count);
        MetadatosMigracionMotivacion metadatos = documento.MetadatosMigracion;
        bool logrosProcesados =
            metadatos.LogrosHistoricosProcesados || fuentesCompletas;
        bool actividadProcesada =
            metadatos.ActividadHistoricaProcesada || fuentesCompletas;
        if (metadatos.LogrosHistoricosProcesados != logrosProcesados ||
            metadatos.ActividadHistoricaProcesada != actividadProcesada ||
            metadatos.HistoriaActividadParcial != historiaParcial) {
            metadatos.LogrosHistoricosProcesados = logrosProcesados;
            metadatos.ActividadHistoricaProcesada = actividadProcesada;
            metadatos.HistoriaActividadParcial = historiaParcial;
            contexto.HuboCambio = true;
        }

        if (!fuentesCompletas) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente ??= fuentes.Error;
        }

        if (!fuentesCompletas || fuentes.DatosParciales) {
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.DatosAcademicosParciales);
        }
    }

    private void ImportarLogrosDesdeFuentes(
        DocumentoMotivacion documento,
        ResultadoFuentes fuentes,
        ContextoMutacion contexto) {
        HechosLogros hechos = CrearHechosLogros(documento, fuentes);
        ReconocerLogrosCumplidos(
            documento,
            contexto,
            hechos,
            esImportado: true);
    }

    private void ImportarLogrosDesdeConcesiones(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        bool esImportado,
        IReadOnlySet<string>? clavesExcluidas = null) {
        ReconocerLogrosCumplidos(
            documento,
            contexto,
            CrearHechosLogros(
                documento,
                CrearFuentesSinDatosAcademicos(),
                clavesExcluidas),
            esImportado);
    }

    private static ResultadoFuentes CrearFuentesSinDatosAcademicos() {
        return new ResultadoFuentes(
            null,
            new Dictionary<string, ProgresoPractica>(),
            new Dictionary<string, HistorialPractica>(),
            false,
            false,
            false,
            false,
            null);
    }

    private HechosLogros CrearHechosLogros(
        DocumentoMotivacion documento,
        ResultadoFuentes fuentes,
        IReadOnlySet<string>? clavesExcluidas = null) {
        HashSet<string> vinculadas = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> realizadas = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> aprobadas = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> perfectas = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> temas = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> grados = new(StringComparer.OrdinalIgnoreCase);

        foreach (ConcesionXP concesion in documento.ConcesionesXP) {
            if (clavesExcluidas?.Contains(concesion.Clave) == true) {
                continue;
            }

            switch (concesion.Tipo) {
                case TipoConcesionXP.PracticaVinculada:
                    vinculadas.Add(concesion.PracticaId!);
                    break;
                case TipoConcesionXP.PracticaRealizada:
                    realizadas.Add(concesion.PracticaId!);
                    break;
                case TipoConcesionXP.EvaluacionAprobada:
                    aprobadas.Add(concesion.PracticaId!);
                    break;
                case TipoConcesionXP.EvaluacionPerfecta:
                    perfectas.Add(concesion.PracticaId!);
                    break;
                case TipoConcesionXP.TemaCompletado:
                    temas.Add($"{concesion.GradoId}:{concesion.TemaId}");
                    break;
                case TipoConcesionXP.GradoCompletado:
                    grados.Add(concesion.GradoId!);
                    break;
            }
        }

        CatalogoAprendizajeSnapshot? catalogo = fuentes.Catalogo;

        if (catalogo is not null && fuentes.ProgresoDisponible) {
            foreach ((string id, ProgresoPractica progreso) in fuentes.Progreso) {
                if (!catalogo.PracticasPorId.ContainsKey(id)) {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progreso.RutaProyecto)) {
                    vinculadas.Add(id);
                }

                if (progreso.Estado == EstadoPracticaCurso.Realizada) {
                    realizadas.Add(id);
                }
            }

            foreach (TemaCatalogoAprendizaje tema in catalogo.Temas) {
                if (tema.Practicas.Count > 0 && tema.Practicas.All(item =>
                        fuentes.Progreso.TryGetValue(
                            item.Practica.Id,
                            out ProgresoPractica? progreso) &&
                        progreso.Estado == EstadoPracticaCurso.Realizada)) {
                    temas.Add($"{tema.Grado.Id}:{tema.Tema.Id}");
                }
            }

            foreach (GradoCurso grado in catalogo.Grados) {
                PracticaCatalogoAprendizaje[] practicas = catalogo.Practicas
                    .Where(item => item.Grado.Id.Equals(
                        grado.Id,
                        StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (practicas.Length > 0 && practicas.All(item =>
                        fuentes.Progreso.TryGetValue(
                            item.Practica.Id,
                            out ProgresoPractica? progreso) &&
                        progreso.Estado == EstadoPracticaCurso.Realizada)) {
                    grados.Add(grado.Id);
                }
            }
        }

        if (catalogo is not null && fuentes.HistorialDisponible) {
            foreach ((string id, HistorialPractica historial) in
                fuentes.Historial) {
                if (!catalogo.PracticasPorId.ContainsKey(id) ||
                    !historial.MejorCalificacion.HasValue) {
                    continue;
                }

                if (historial.MejorCalificacion.Value >= 70) {
                    aprobadas.Add(id);
                }

                if (historial.MejorCalificacion.Value == 100) {
                    perfectas.Add(id);
                }
            }
        }

        return new HechosLogros(
            vinculadas,
            realizadas,
            aprobadas,
            perfectas,
            temas,
            grados);
    }

    private void ReconocerLogrosCumplidos(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        HechosLogros hechos,
        bool esImportado) {
        foreach (DefinicionLogro definicion in catalogoLogros.CargarDefiniciones()) {
            bool cumplido = definicion.Criterio switch {
                CriterioLogro.PracticasVinculadasDistintas =>
                    hechos.PracticasVinculadas.Count >= definicion.Umbral,
                CriterioLogro.PracticasRealizadasDistintas =>
                    hechos.PracticasRealizadas.Count >= definicion.Umbral,
                CriterioLogro.PracticasAprobadasDistintas =>
                    hechos.PracticasAprobadas.Count >= definicion.Umbral,
                CriterioLogro.PracticasPerfectasDistintas =>
                    hechos.PracticasPerfectas.Count >= definicion.Umbral,
                CriterioLogro.TemasCompletadosDistintos =>
                    hechos.TemasCompletados.Count >= definicion.Umbral,
                CriterioLogro.GradosCompletadosDistintos =>
                    hechos.GradosCompletados.Count >= definicion.Umbral,
                CriterioLogro.GradoEspecificoCompletado =>
                    hechos.GradosCompletados.Contains(definicion.GradoId!),
                _ => false
            };

            if (cumplido) {
                AgregarLogro(
                    documento,
                    contexto,
                    definicion.Id,
                    esImportado);
            }
        }
    }

    private static bool AgregarLogro(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string logroId,
        bool esImportado) {
        if (documento.LogrosDesbloqueados.Any(item => item.LogroId.Equals(
                logroId,
                StringComparison.OrdinalIgnoreCase))) {
            return false;
        }

        LogroDesbloqueado logro = new() {
            LogroId = logroId,
            FechaReconocimientoUtc =
                contexto.InstanteReconocimientoLogrosUtc ??
                contexto.InstanteConcesionUtc ??
                documento.UltimoInstanteUtcAceptado,
            EsImportado = esImportado
        };
        documento.LogrosDesbloqueados.Add(logro);
        contexto.HuboCambio = true;

        if (!esImportado) {
            contexto.HuboCambioVersion2OperacionActual = true;
            contexto.LogrosNuevos.Add(CopiarLogro(logro));
        }

        return true;
    }

    private void ImportarDiasHistoricos(
        DocumentoMotivacion documento,
        ResultadoFuentes fuentes,
        ContextoMutacion contexto) {
        if (!IntentarResolverZonaHoraria(documento, contexto, out TimeZoneInfo? zona)) {
            return;
        }

        DateTimeOffset limiteUtc = ObtenerAhoraUtc() + ToleranciaRetrocesoReloj;
        List<DateTimeOffset> instantes = new();

        if (fuentes.ProgresoDisponible) {
            instantes.AddRange(fuentes.Progreso.Values
                .Where(item =>
                    item.Estado == EstadoPracticaCurso.Realizada &&
                    item.FechaFinalizacion.HasValue)
                .Select(item => item.FechaFinalizacion!.Value));
        }

        if (fuentes.HistorialDisponible) {
            instantes.AddRange(fuentes.Historial.Values
                .SelectMany(item => item.Intentos)
                .Select(item => item.Fecha));
        }

        foreach (DateTimeOffset instante in instantes) {
            if (instante == default || instante.ToUniversalTime() > limiteUtc) {
                contexto.Advertencias.Add(
                    AdvertenciaMotivacion.DatosAcademicosParciales);
                continue;
            }

            DateOnly dia = ObtenerDiaAcademico(instante, zona!);

            if (!documento.DiasActividadAcademica.Contains(dia)) {
                documento.DiasActividadAcademica.Add(dia);
                contexto.HuboCambio = true;
            }
        }
    }

    private void ProcesarVersion2OperacionActual(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        TipoOperacionMotivacion operacion,
        string? practicaId,
        EvidenciaOperacion? evidencia) {
        DateTimeOffset? instanteActividad = operacion switch {
            TipoOperacionMotivacion.ProgresoPersistido
                when evidencia?.TransicionProgreso is not null &&
                    (evidencia.TransicionProgreso.VinculoPersistidoAhora ||
                     evidencia.TransicionProgreso.RealizadaPersistidaAhora) =>
                evidencia.TransicionProgreso.ProgresoFinal.FechaActualizacion,
            TipoOperacionMotivacion.EvaluacionPersistida
                when evidencia?.TransicionEvaluacion is not null =>
                evidencia.TransicionEvaluacion.FechaIntento,
            _ => null
        };

        if (contexto.OperacionAcademicaActualConfirmada &&
            instanteActividad.HasValue &&
            RegistrarDiaActividadActual(
                documento,
                contexto,
                instanteActividad.Value)) {
            DateTimeOffset instanteActividadUtc =
                instanteActividad.Value.ToUniversalTime();
            contexto.InstanteReconocimientoLogrosUtc = instanteActividadUtc;

            if (!contexto.InstanteConcesionUtc.HasValue ||
                instanteActividadUtc > contexto.InstanteConcesionUtc.Value) {
                contexto.InstanteConcesionUtc = instanteActividadUtc;
            }
        }

        if (operacion == TipoOperacionMotivacion.Reconciliacion) {
            return;
        }

        if (!contexto.OperacionAcademicaActualConfirmada ||
            evidencia is null ||
            string.IsNullOrWhiteSpace(practicaId)) {
            ImportarLogrosDesdeConcesiones(
                documento,
                contexto,
                esImportado: true);
            return;
        }

        HechosLogros hechosAnteriores = CrearHechosLogros(
            documento,
            CrearFuentesSinDatosAcademicos(),
            contexto.ClavesConcedidasOperacionActual);
        ReconocerLogrosCumplidos(
            documento,
            contexto,
            hechosAnteriores,
            esImportado: true);
        HechosLogros hechosPosteriores = CrearHechosLogrosOperacionActual(
            hechosAnteriores,
            contexto.CatalogoOperacionActual,
            operacion,
            practicaId,
            evidencia);
        ReconocerLogrosCumplidos(
            documento,
            contexto,
            hechosPosteriores,
            esImportado: false);
    }

    private HechosLogros CrearHechosLogrosOperacionActual(
        HechosLogros hechosAnteriores,
        CatalogoAprendizajeSnapshot? catalogo,
        TipoOperacionMotivacion operacion,
        string practicaId,
        EvidenciaOperacion evidencia) {
        HashSet<string> vinculadas = new(
            hechosAnteriores.PracticasVinculadas,
            StringComparer.OrdinalIgnoreCase);
        HashSet<string> realizadas = new(
            hechosAnteriores.PracticasRealizadas,
            StringComparer.OrdinalIgnoreCase);
        HashSet<string> aprobadas = new(
            hechosAnteriores.PracticasAprobadas,
            StringComparer.OrdinalIgnoreCase);
        HashSet<string> perfectas = new(
            hechosAnteriores.PracticasPerfectas,
            StringComparer.OrdinalIgnoreCase);
        HashSet<string> temas = new(
            hechosAnteriores.TemasCompletados,
            StringComparer.OrdinalIgnoreCase);
        HashSet<string> grados = new(
            hechosAnteriores.GradosCompletados,
            StringComparer.OrdinalIgnoreCase);

        if (operacion == TipoOperacionMotivacion.ProgresoPersistido &&
            evidencia.TransicionProgreso is not null) {
            if (evidencia.TransicionProgreso.VinculoPersistidoAhora) {
                vinculadas.Add(practicaId);
            }

            if (evidencia.TransicionProgreso.RealizadaPersistidaAhora) {
                realizadas.Add(practicaId);
                AgregarContenidoCompletadoDesdeRealizadas(
                    catalogo,
                    realizadas,
                    temas,
                    grados);
            }
        }

        if (operacion == TipoOperacionMotivacion.EvaluacionPersistida &&
            evidencia.TransicionEvaluacion is not null) {
            TransicionEvaluacionPersistida transicion =
                evidencia.TransicionEvaluacion;

            if (transicion.CalificacionIntento >= 70 &&
                (!transicion.MejorCalificacionAnterior.HasValue ||
                 transicion.MejorCalificacionAnterior.Value < 70)) {
                aprobadas.Add(practicaId);
            }

            if (transicion.CalificacionIntento == 100 &&
                (!transicion.MejorCalificacionAnterior.HasValue ||
                 transicion.MejorCalificacionAnterior.Value < 100)) {
                perfectas.Add(practicaId);
            }
        }

        return new HechosLogros(
            vinculadas,
            realizadas,
            aprobadas,
            perfectas,
            temas,
            grados);
    }

    private static void AgregarContenidoCompletadoDesdeRealizadas(
        CatalogoAprendizajeSnapshot? catalogo,
        IReadOnlySet<string> realizadas,
        HashSet<string> temas,
        HashSet<string> grados) {
        if (catalogo is null) {
            return;
        }

        foreach (TemaCatalogoAprendizaje tema in catalogo.Temas) {
            if (tema.Practicas.Count > 0 && tema.Practicas.All(item =>
                    realizadas.Contains(item.Practica.Id))) {
                temas.Add($"{tema.Grado.Id}:{tema.Tema.Id}");
            }
        }

        foreach (GradoCurso grado in catalogo.Grados) {
            PracticaCatalogoAprendizaje[] practicas = catalogo.Practicas
                .Where(item => item.Grado.Id.Equals(
                    grado.Id,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (practicas.Length > 0 && practicas.All(item =>
                    realizadas.Contains(item.Practica.Id))) {
                grados.Add(grado.Id);
            }
        }
    }

    private bool RegistrarDiaActividadActual(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        DateTimeOffset instante) {
        if (!IntentarResolverZonaHoraria(documento, contexto, out TimeZoneInfo? zona)) {
            return false;
        }

        DateTimeOffset instanteUtc = instante.ToUniversalTime();
        DateTimeOffset ahoraUtc = ObtenerAhoraUtc();

        if (instante == default ||
            instanteUtc > ahoraUtc + ToleranciaRetrocesoReloj) {
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.DatosAcademicosParciales);
            return false;
        }

        DateOnly dia = ObtenerDiaAcademico(instanteUtc, zona!);

        if (!documento.DiasActividadAcademica.Contains(dia)) {
            documento.DiasActividadAcademica.Add(dia);
            contexto.HuboCambio = true;
            contexto.HuboCambioVersion2OperacionActual = true;
        }

        return true;
    }

    private static DateOnly ObtenerDiaAcademico(
        DateTimeOffset instante,
        TimeZoneInfo zona) {
        return DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(instante.ToUniversalTime(), zona).DateTime);
    }

    private static bool IntentarResolverZonaHoraria(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        out TimeZoneInfo? zona) {
        try {
            zona = TimeZoneInfo.FindSystemTimeZoneById(
                documento.ZonaHorariaEstudio);
            return true;
        } catch (Exception ex) when (ex is TimeZoneNotFoundException or
            InvalidTimeZoneException) {
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.ZonaHorariaNoDisponible);
            zona = null;
            return false;
        }
    }

    private static LogroDesbloqueado CopiarLogro(LogroDesbloqueado logro) {
        return new LogroDesbloqueado {
            LogroId = logro.LogroId,
            FechaReconocimientoUtc = logro.FechaReconocimientoUtc,
            EsImportado = logro.EsImportado
        };
    }

    private static ResultadoFuentes PrepararFuentesHistoricasParaOperacionActual(
        ResultadoFuentes fuentes,
        string? practicaId,
        EvidenciaOperacion? evidencia) {
        if (fuentes.Catalogo is null ||
            string.IsNullOrWhiteSpace(practicaId) ||
            evidencia is null) {
            return fuentes;
        }

        if (evidencia.TransicionProgreso is not null) {
            Dictionary<string, ProgresoPractica> progresoAnterior =
                new(fuentes.Progreso, StringComparer.OrdinalIgnoreCase);
            progresoAnterior.Remove(practicaId);

            if (evidencia.TransicionProgreso.ProgresoAnterior is not null) {
                progresoAnterior[practicaId] = CopiarProgresoPractica(
                    evidencia.TransicionProgreso.ProgresoAnterior);
            }

            fuentes = fuentes with { Progreso = progresoAnterior };
        }

        if (evidencia.TransicionEvaluacion is null ||
            evidencia.Historial is null) {
            return fuentes;
        }

        Dictionary<string, HistorialPractica> historialAnterior =
            new(fuentes.Historial, StringComparer.OrdinalIgnoreCase);
        historialAnterior.Remove(practicaId);
        HistorialPractica? practicaAnterior = CrearHistorialAnterior(
            practicaId,
            evidencia.Historial,
            evidencia.TransicionEvaluacion);

        if (practicaAnterior is not null) {
            historialAnterior[practicaId] = practicaAnterior;
        }

        return fuentes with { Historial = historialAnterior };
    }

    private EstadoAplicacionOperacion AplicarOperacion(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        TipoOperacionMotivacion operacion,
        string? practicaId,
        EvidenciaOperacion? evidencia) {
        return operacion switch {
            TipoOperacionMotivacion.VinculoPractica =>
                AplicarVinculoPersistido(
                    documento,
                    contexto,
                    practicaId!),
            TipoOperacionMotivacion.PracticaRealizada =>
                AplicarPracticaRealizadaPersistida(
                    documento,
                    contexto,
                    practicaId!),
            TipoOperacionMotivacion.ProgresoPersistido =>
                AplicarProgresoPersistido(
                    documento,
                    contexto,
                    practicaId!,
                    evidencia!.Progreso!,
                    evidencia.VinculoPersistidoAhora,
                    evidencia.RealizadaPersistidaAhora,
                    evidencia.TransicionProgreso),
            TipoOperacionMotivacion.EvaluacionPersistida =>
                AplicarEvaluacionPersistida(
                    documento,
                    contexto,
                    practicaId!,
                    evidencia?.Historial,
                    evidencia?.Intento,
                    evidencia?.TransicionEvaluacion),
            TipoOperacionMotivacion.Reconciliacion =>
                AplicarReconciliacion(documento, contexto),
            _ => EstadoAplicacionOperacion.SinRecompensa
        };
    }

    private EstadoAplicacionOperacion AplicarVinculoPersistido(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId) {
        ResultadoFuentes fuentes = CargarFuentesAcademicas(
            cargarSoloProgreso: true);

        if (!fuentes.ProgresoDisponible || fuentes.Catalogo is null) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = fuentes.Error;
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        if (!fuentes.Catalogo.PracticasPorId.ContainsKey(practicaId) ||
            !fuentes.Progreso.TryGetValue(practicaId, out ProgresoPractica? progreso) ||
            string.IsNullOrWhiteSpace(progreso.RutaProyecto)) {
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        return Conceder(
            documento,
            contexto,
            TipoConcesionXP.PracticaVinculada,
            10,
            practicaId: practicaId,
            esImportada: false,
            esHitoOperacionActual: true)
            ? EstadoAplicacionOperacion.Aplicada
            : EstadoAplicacionOperacion.YaAplicada;
    }

    private EstadoAplicacionOperacion AplicarPracticaRealizadaPersistida(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId) {
        ResultadoFuentes fuentes = CargarFuentesAcademicas(
            cargarSoloProgreso: true);

        if (!fuentes.ProgresoDisponible || fuentes.Catalogo is null) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = fuentes.Error;
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        if (!fuentes.Catalogo.PracticasPorId.ContainsKey(practicaId) ||
            !fuentes.Progreso.TryGetValue(practicaId, out ProgresoPractica? progreso) ||
            progreso.Estado != EstadoPracticaCurso.Realizada) {
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        bool aplicada = Conceder(
            documento,
            contexto,
            TipoConcesionXP.PracticaRealizada,
            25,
            practicaId: practicaId,
            esImportada: false,
            esHitoOperacionActual: true);
        aplicada |= ConcederContenidoCompletadoParaPractica(
            documento,
            fuentes.Catalogo,
            fuentes.Progreso,
            contexto,
            practicaId);
        return aplicada
            ? EstadoAplicacionOperacion.Aplicada
            : EstadoAplicacionOperacion.YaAplicada;
    }

    private EstadoAplicacionOperacion AplicarProgresoPersistido(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId,
        ProgresoCurso progresoPersistido,
        bool vinculoPersistidoAhora,
        bool realizadaPersistidaAhora,
        TransicionProgresoPersistida? transicionPersistida) {
        ResultadoFuentes fuentes = CrearFuentesDesdeProgreso(
            progresoPersistido);

        if (!fuentes.ProgresoDisponible || fuentes.Catalogo is null) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = fuentes.Error;
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        if (!fuentes.Catalogo.PracticasPorId.ContainsKey(practicaId) ||
            !fuentes.Progreso.TryGetValue(
                practicaId,
                out ProgresoPractica? progreso)) {
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        if (transicionPersistida is not null) {
            if (!EsTransicionProgresoValida(
                    practicaId,
                    progreso,
                    transicionPersistida)) {
                contexto.FuenteNoDisponible = true;
                contexto.ErrorFuente = new InvalidDataException(
                    "La transición de progreso publicada no coincide con el snapshot persistido.");
                return EstadoAplicacionOperacion.SinRecompensa;
            }

            vinculoPersistidoAhora = transicionPersistida.VinculoPersistidoAhora;
            realizadaPersistidaAhora = transicionPersistida.RealizadaPersistidaAhora;
            contexto.CatalogoOperacionActual = fuentes.Catalogo;
            contexto.OperacionAcademicaActualConfirmada = true;
        }

        bool aplicada = false;
        bool habiaConcesionExistente = contexto.HuboConcesionYaExistente;

        if (vinculoPersistidoAhora &&
            !string.IsNullOrWhiteSpace(progreso.RutaProyecto)) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.PracticaVinculada,
                10,
                practicaId: practicaId,
                esImportada: false,
                esHitoOperacionActual: true);
        }

        if (realizadaPersistidaAhora &&
            progreso.Estado == EstadoPracticaCurso.Realizada) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.PracticaRealizada,
                25,
                practicaId: practicaId,
                esImportada: false,
                esHitoOperacionActual: true);
            aplicada |= ConcederContenidoCompletadoParaPractica(
                documento,
                fuentes.Catalogo,
                fuentes.Progreso,
                contexto,
                practicaId);
        }

        return aplicada
            ? EstadoAplicacionOperacion.Aplicada
            : contexto.HuboConcesionYaExistente != habiaConcesionExistente
                ? EstadoAplicacionOperacion.YaAplicada
                : EstadoAplicacionOperacion.SinRecompensa;
    }

    private EstadoAplicacionOperacion AplicarEvaluacionPersistida(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId,
        HistorialPractica? evidenciaPersistida,
        IntentoPractica? intentoPersistido,
        TransicionEvaluacionPersistida? transicionPersistida) {
        ResultadoFuentes fuentes = evidenciaPersistida is null
            ? CargarFuentesAcademicas(cargarSoloHistorial: true)
            : CrearFuentesDesdeHistorial(evidenciaPersistida);

        if (!fuentes.HistorialDisponible || fuentes.Catalogo is null) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = fuentes.Error;
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        if (!fuentes.Catalogo.PracticasPorId.ContainsKey(practicaId) ||
            !fuentes.Historial.TryGetValue(practicaId, out HistorialPractica? historial) ||
            !historial.MejorCalificacion.HasValue) {
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        if (transicionPersistida is not null) {
            return AplicarEvaluacionConTransicionExacta(
                documento,
                contexto,
                practicaId,
                historial,
                transicionPersistida);
        }

        if (intentoPersistido is not null) {
            return AplicarEvaluacionConIntentoExacto(
                documento,
                contexto,
                practicaId,
                historial,
                intentoPersistido);
        }

        bool aplicada = ProcesarHistorialPractica(
            documento,
            historial,
            contexto,
            esImportado: false,
            esMigracionInicial: false);
        return aplicada
            ? EstadoAplicacionOperacion.Aplicada
            : contexto.ClavesProcesadas.Count > 0
                ? EstadoAplicacionOperacion.YaAplicada
                : EstadoAplicacionOperacion.SinRecompensa;
    }

    private EstadoAplicacionOperacion AplicarReconciliacion(
        DocumentoMotivacion documento,
        ContextoMutacion contexto) {
        ResultadoFuentes fuentes = CargarFuentesAcademicas();

        if (!fuentes.ProgresoDisponible && !fuentes.HistorialDisponible) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = fuentes.Error;
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        int cambiosAntes = contexto.ClavesProcesadas.Count;
        AplicarReconciliacionCompleta(
            documento,
            fuentes,
            contexto,
            esImportada: true,
            esMigracionInicial: false);
        return contexto.ClavesProcesadas.Count > cambiosAntes || contexto.HuboCambio
            ? EstadoAplicacionOperacion.Aplicada
            : EstadoAplicacionOperacion.YaAplicada;
    }

    private void AplicarReconciliacionCompleta(
        DocumentoMotivacion documento,
        ResultadoFuentes fuentes,
        ContextoMutacion contexto,
        bool esImportada,
        bool esMigracionInicial) {
        if (fuentes.Catalogo is null) {
            return;
        }

        if (fuentes.DatosParciales) {
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.DatosAcademicosParciales);
        }

        if (!fuentes.ProgresoDisponible || !fuentes.HistorialDisponible) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente ??= fuentes.Error;
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.DatosAcademicosParciales);
        }

        if (fuentes.ProgresoDisponible) {
            foreach (PracticaCatalogoAprendizaje practica in
                fuentes.Catalogo.Practicas) {
                if (!fuentes.Progreso.TryGetValue(
                        practica.Practica.Id,
                        out ProgresoPractica? progreso)) {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(progreso.RutaProyecto)) {
                    Conceder(
                        documento,
                        contexto,
                        TipoConcesionXP.PracticaVinculada,
                        10,
                        practicaId: practica.Practica.Id,
                        esImportada: esImportada);
                }

                if (progreso.Estado == EstadoPracticaCurso.Realizada) {
                    Conceder(
                        documento,
                        contexto,
                        TipoConcesionXP.PracticaRealizada,
                        25,
                        practicaId: practica.Practica.Id,
                        esImportada: esImportada);
                }
            }

            ConcederContenidoCompletado(
                documento,
                fuentes.Catalogo,
                fuentes.Progreso,
                contexto,
                esImportada);
        }

        if (fuentes.HistorialDisponible) {
            foreach (HistorialPractica historial in fuentes.Historial.Values) {
                ProcesarHistorialPractica(
                    documento,
                    historial,
                    contexto,
                    esImportada,
                    esMigracionInicial,
                    permitirMejoraHistorica: !fuentes.HistorialParcial);
            }
        }

        ReconciliarDatosVersion2Historicos(documento, fuentes, contexto);
    }

    private bool ProcesarHistorialPractica(
        DocumentoMotivacion documento,
        HistorialPractica historial,
        ContextoMutacion contexto,
        bool esImportado,
        bool esMigracionInicial,
        bool permitirMejoraHistorica = true) {
        if (!historial.MejorCalificacion.HasValue) {
            return false;
        }

        string practicaId = historial.PracticaId;
        int mejor = historial.MejorCalificacion.Value;
        bool aplicada = false;

        if (mejor >= 70) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.EvaluacionAprobada,
                40,
                practicaId: practicaId,
                esImportada: esImportado);
        }

        if (mejor == 100) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.EvaluacionPerfecta,
                25,
                practicaId: practicaId,
                esImportada: esImportado);
        }

        bool reconocida = documento.MejorCalificacionReconocidaPorPractica
            .TryGetValue(practicaId, out int mejorReconocida);
        int xpMejoraConcedido = documento.XPMejoraConcedidoPorPractica
            .GetValueOrDefault(practicaId);

        if (!permitirMejoraHistorica) {
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.MejoraHistoricaNoDemostrable);

            if (esMigracionInicial) {
                documento.MetadatosMigracion.MejorasHistoricasOmitidas++;
            }

            return aplicada;
        }

        if (!reconocida) {
            int mejoraDemostrable = 0;
            bool demostrable = IntentarCalcularMejoraHistorica(
                historial,
                out mejoraDemostrable);

            documento.MejorCalificacionReconocidaPorPractica[practicaId] = mejor;
            documento.XPMejoraConcedidoPorPractica[practicaId] = 0;
            contexto.HuboCambio = true;
            mejorReconocida = mejor;

            if (demostrable && mejoraDemostrable > 0) {
                int cantidad = Math.Min(25, mejoraDemostrable);
                aplicada |= ConcederMejora(
                    documento,
                    contexto,
                    practicaId,
                    cantidad,
                    esImportado);

                if (esMigracionInicial) {
                    documento.MetadatosMigracion.MejorasHistoricasReconocidas++;
                }
            } else if (esImportado && historial.TotalIntentos > 1) {
                contexto.Advertencias.Add(
                    AdvertenciaMotivacion.MejoraHistoricaNoDemostrable);

                if (esMigracionInicial) {
                    documento.MetadatosMigracion.MejorasHistoricasOmitidas++;
                }
            }

            return aplicada;
        }

        if (mejor <= mejorReconocida || xpMejoraConcedido >= 25) {
            return aplicada;
        }

        int mejora = Math.Min(
            mejor - mejorReconocida,
            25 - xpMejoraConcedido);
        aplicada |= ConcederMejora(
            documento,
            contexto,
            practicaId,
            mejora,
            esImportado);
        documento.MejorCalificacionReconocidaPorPractica[practicaId] = mejor;
        contexto.HuboCambio = true;
        return aplicada;
    }

    private bool ConcederMejora(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId,
        int cantidad,
        bool esImportada,
        bool esHitoOperacionActual = false) {
        int concedido = documento.XPMejoraConcedidoPorPractica
            .GetValueOrDefault(practicaId);
        int limite = Math.Min(25, checked(concedido + cantidad));
        bool aplicada = false;

        for (int tramo = concedido + 1; tramo <= limite; tramo++) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.MejoraCalificacion,
                1,
                practicaId: practicaId,
                esImportada: esImportada,
                tramoMejora: tramo,
                esHitoOperacionActual: esHitoOperacionActual);
        }

        documento.XPMejoraConcedidoPorPractica[practicaId] = limite;
        contexto.HuboCambio |= limite != concedido;
        return aplicada;
    }

    private bool ConcederContenidoCompletado(
        DocumentoMotivacion documento,
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        ContextoMutacion contexto,
        bool esImportado) {
        bool aplicada = false;

        foreach (TemaCatalogoAprendizaje tema in catalogo.Temas) {
            bool completo = tema.Practicas.Count > 0 &&
                tema.Practicas.All(item =>
                    progreso.TryGetValue(item.Practica.Id, out ProgresoPractica? registro) &&
                    registro.Estado == EstadoPracticaCurso.Realizada);

            if (completo) {
                aplicada |= Conceder(
                    documento,
                    contexto,
                    TipoConcesionXP.TemaCompletado,
                    75,
                    temaId: tema.Tema.Id,
                    gradoId: tema.Grado.Id,
                    esImportada: esImportado);
            }
        }

        foreach (GradoCurso grado in catalogo.Grados) {
            PracticaCatalogoAprendizaje[] practicas = catalogo.Practicas
                .Where(item => item.Grado.Id.Equals(
                    grado.Id,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            bool completo = practicas.Length > 0 && practicas.All(item =>
                progreso.TryGetValue(item.Practica.Id, out ProgresoPractica? registro) &&
                registro.Estado == EstadoPracticaCurso.Realizada);

            if (completo) {
                aplicada |= Conceder(
                    documento,
                    contexto,
                    TipoConcesionXP.GradoCompletado,
                    200,
                    gradoId: grado.Id,
                    esImportada: esImportado);
            }
        }

        return aplicada;
    }

    private bool ConcederContenidoCompletadoParaPractica(
        DocumentoMotivacion documento,
        CatalogoAprendizajeSnapshot catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> progreso,
        ContextoMutacion contexto,
        string practicaId) {
        if (!catalogo.PracticasPorId.TryGetValue(
                practicaId,
                out PracticaCatalogoAprendizaje? practica)) {
            return false;
        }

        bool aplicada = false;
        PracticaCatalogoAprendizaje[] practicasTema = catalogo.Practicas
            .Where(item => item.Grado.Id.Equals(
                    practica.Grado.Id,
                    StringComparison.OrdinalIgnoreCase) &&
                item.Tema.Id.Equals(
                    practica.Tema.Id,
                    StringComparison.OrdinalIgnoreCase))
            .ToArray();
        bool temaCompleto = practicasTema.Length > 0 &&
            practicasTema.All(item => progreso.TryGetValue(
                    item.Practica.Id,
                    out ProgresoPractica? registro) &&
                registro.Estado == EstadoPracticaCurso.Realizada);

        if (temaCompleto) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.TemaCompletado,
                75,
                temaId: practica.Tema.Id,
                gradoId: practica.Grado.Id,
                esImportada: false,
                esHitoOperacionActual: true);
        }

        PracticaCatalogoAprendizaje[] practicasGrado = catalogo.Practicas
            .Where(item => item.Grado.Id.Equals(
                practica.Grado.Id,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        bool gradoCompleto = practicasGrado.Length > 0 &&
            practicasGrado.All(item => progreso.TryGetValue(
                    item.Practica.Id,
                    out ProgresoPractica? registro) &&
                registro.Estado == EstadoPracticaCurso.Realizada);

        if (gradoCompleto) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.GradoCompletado,
                200,
                gradoId: practica.Grado.Id,
                esImportada: false,
                esHitoOperacionActual: true);
        }

        return aplicada;
    }

    private bool Conceder(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        TipoConcesionXP tipo,
        int cantidad,
        string? practicaId = null,
        string? temaId = null,
        string? gradoId = null,
        bool esImportada = false,
        int? tramoMejora = null,
        bool esHitoOperacionActual = false) {
        string clave = CrearClave(
            tipo,
            practicaId,
            temaId,
            gradoId,
            tramoMejora);
        if (documento.ConcesionesXP.Any(item => item.Clave.Equals(
                clave,
                StringComparison.OrdinalIgnoreCase))) {
            contexto.HuboConcesionYaExistente = true;

            if (esHitoOperacionActual &&
                contexto.ClavesCreadasEnOperacion.Contains(clave)) {
                contexto.ClavesHitosOperacionActual.Add(clave);
            }

            return false;
        }

        contexto.ClavesProcesadas.Add(clave);
        contexto.ClavesCreadasEnOperacion.Add(clave);

        if (esHitoOperacionActual) {
            contexto.ClavesHitosOperacionActual.Add(clave);
        }

        documento.ConcesionesXP.Add(new ConcesionXP {
            Clave = clave,
            CantidadXP = cantidad,
            FechaUtc = contexto.InstanteConcesionUtc ??
                documento.UltimoInstanteUtcAceptado,
            Tipo = tipo,
            PracticaId = NormalizarOpcional(practicaId),
            TemaId = NormalizarOpcional(temaId),
            GradoId = NormalizarOpcional(gradoId),
            EsImportada = esImportada
        });

        if (esHitoOperacionActual && !esImportada) {
            contexto.ClavesConcedidasOperacionActual.Add(clave);
        }

        contexto.HuboCambio = true;
        return true;
    }

    private EstadoAplicacionOperacion AplicarEvaluacionConIntentoExacto(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId,
        HistorialPractica historial,
        IntentoPractica intentoActual) {
        if (!IntentarCrearTransicionEvaluacionCompatibilidad(
                practicaId,
                historial,
                intentoActual,
                out TransicionEvaluacionPersistida transicion)) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = new InvalidDataException(
                "El historial persistido no permite identificar de forma " +
                "inequívoca el intento recién guardado.");
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        return AplicarEvaluacionConTransicionExacta(
            documento,
            contexto,
            practicaId,
            historial,
            transicion);
    }

    private EstadoAplicacionOperacion AplicarEvaluacionConTransicionExacta(
        DocumentoMotivacion documento,
        ContextoMutacion contexto,
        string practicaId,
        HistorialPractica historial,
        TransicionEvaluacionPersistida transicion) {
        if (!EsTransicionEvaluacionValida(
                practicaId,
                historial,
                transicion)) {
            contexto.FuenteNoDisponible = true;
            contexto.ErrorFuente = new InvalidDataException(
                "La transición de evaluación publicada no coincide con el historial persistido.");
            return EstadoAplicacionOperacion.SinRecompensa;
        }

        contexto.OperacionAcademicaActualConfirmada = true;

        bool aplicada = false;
        bool habiaConcesionExistente = contexto.HuboConcesionYaExistente;
        int? mejorAnterior = transicion.MejorCalificacionAnterior;
        HistorialPractica? historialAnterior = CrearHistorialAnterior(
            practicaId,
            historial,
            transicion);

        if (historialAnterior is not null) {
            aplicada |= ProcesarHistorialPractica(
                documento,
                historialAnterior,
                contexto,
                esImportado: true,
                esMigracionInicial: false);
        }

        if (transicion.CalificacionIntento >= 70 &&
            (!mejorAnterior.HasValue || mejorAnterior.Value < 70)) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.EvaluacionAprobada,
                40,
                practicaId: practicaId,
                esImportada: false,
                esHitoOperacionActual: true);
        }

        if (transicion.CalificacionIntento == 100 &&
            (!mejorAnterior.HasValue || mejorAnterior.Value < 100)) {
            aplicada |= Conceder(
                documento,
                contexto,
                TipoConcesionXP.EvaluacionPerfecta,
                25,
                practicaId: practicaId,
                esImportada: false,
                esHitoOperacionActual: true);
        }

        int mejorReconocidaAntes = documento.MejorCalificacionReconocidaPorPractica
            .GetValueOrDefault(practicaId, -1);

        if (mejorAnterior.HasValue &&
            transicion.MejorCalificacionPosterior > mejorReconocidaAntes) {
            int xpConcedido = documento.XPMejoraConcedidoPorPractica
                .GetValueOrDefault(practicaId);
            int baseDemostrable = Math.Max(
                mejorAnterior.Value,
                mejorReconocidaAntes);
            int mejoraDemostrable = checked(
                transicion.MejorCalificacionPosterior - baseDemostrable);
            int cantidad = Math.Min(
                mejoraDemostrable,
                Math.Max(0, 25 - xpConcedido));

            if (cantidad > 0) {
                aplicada |= ConcederMejora(
                    documento,
                    contexto,
                    practicaId,
                    cantidad,
                    esImportada: false,
                    esHitoOperacionActual: true);
            }
        }

        int mejorReconocida = documento.MejorCalificacionReconocidaPorPractica
            .GetValueOrDefault(practicaId, -1);
        int mejorActual = transicion.MejorCalificacionPosterior;

        if (mejorActual > mejorReconocida) {
            documento.MejorCalificacionReconocidaPorPractica[practicaId] =
                mejorActual;
            contexto.HuboCambio = true;
        }

        return aplicada
            ? EstadoAplicacionOperacion.Aplicada
            : contexto.HuboConcesionYaExistente != habiaConcesionExistente
                ? EstadoAplicacionOperacion.YaAplicada
                : EstadoAplicacionOperacion.SinRecompensa;
    }

    private static bool IntentarCrearTransicionEvaluacionCompatibilidad(
        string practicaId,
        HistorialPractica historial,
        IntentoPractica intentoActual,
        out TransicionEvaluacionPersistida transicion) {
        transicion = new TransicionEvaluacionPersistida();
        IReadOnlyList<IntentoPractica> intentos = historial.Intentos;

        if (intentos is null ||
            historial.TotalIntentos != intentos.Count ||
            intentos.Count == 0 ||
            string.IsNullOrWhiteSpace(intentoActual.Id) ||
            !historial.PracticaId.Equals(
                practicaId,
                StringComparison.OrdinalIgnoreCase) ||
            !intentoActual.PracticaId.Equals(
                practicaId,
                StringComparison.OrdinalIgnoreCase) ||
            intentoActual.Calificacion is < 0 or > 100 ||
            intentos.Any(item =>
                item is null ||
                string.IsNullOrWhiteSpace(item.Id) ||
                item.Calificacion is < 0 or > 100) ||
            intentos.Select(item => item.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != intentos.Count ||
            !historial.MejorCalificacion.HasValue ||
            historial.MejorCalificacion.Value !=
                intentos.Max(item => item.Calificacion) ||
            historial.UltimaCalificacion != intentoActual.Calificacion ||
            historial.FechaUltimoIntento != intentoActual.Fecha) {
            return false;
        }

        IntentoPractica[] coincidencias = intentos
            .Where(item => item.Id.Equals(
                intentoActual.Id,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (coincidencias.Length != 1 ||
            coincidencias[0].Calificacion != intentoActual.Calificacion ||
            coincidencias[0].Fecha != intentoActual.Fecha) {
            return false;
        }

        IntentoPractica[] anteriores = intentos
            .Where(item => !item.Id.Equals(
                intentoActual.Id,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        int? mejorAnterior = anteriores.Length == 0
            ? null
            : anteriores.Max(item => item.Calificacion);
        IntentoPractica? ultimoAnterior = anteriores
            .OrderByDescending(item => item.Fecha)
            .ThenByDescending(item => item.Id, StringComparer.Ordinal)
            .FirstOrDefault();
        transicion = new TransicionEvaluacionPersistida {
            PracticaId = practicaId,
            IntentoId = intentoActual.Id,
            FechaIntento = intentoActual.Fecha,
            CalificacionIntento = intentoActual.Calificacion,
            MejorCalificacionAnterior = mejorAnterior,
            UltimaCalificacionAnterior = ultimoAnterior?.Calificacion,
            FechaUltimoIntentoAnterior = ultimoAnterior?.Fecha,
            MejorCalificacionPosterior = historial.MejorCalificacion.Value,
            TotalIntentos = historial.TotalIntentos,
            IntentoPublicado = true
        };
        return true;
    }

    private static HistorialPractica? CrearHistorialAnterior(
        string practicaId,
        HistorialPractica historial,
        TransicionEvaluacionPersistida transicion) {
        int totalAnterior = transicion.TotalIntentos - 1;

        if (totalAnterior <= 0 ||
            !transicion.MejorCalificacionAnterior.HasValue) {
            return null;
        }

        IntentoPractica[] anteriores = historial.Intentos
            .Where(item => !item.Id.Equals(
                transicion.IntentoId,
                StringComparison.OrdinalIgnoreCase))
            .Select(CopiarIntento)
            .ToArray();
        return new HistorialPractica {
            PracticaId = practicaId,
            TotalIntentos = totalAnterior,
            MejorCalificacion = transicion.MejorCalificacionAnterior,
            UltimaCalificacion = transicion.UltimaCalificacionAnterior,
            FechaUltimoIntento = transicion.FechaUltimoIntentoAnterior,
            Intentos = anteriores
        };
    }

    private static bool EsTransicionEvaluacionValida(
        string practicaId,
        HistorialPractica historial,
        TransicionEvaluacionPersistida transicion) {
        if (!transicion.IntentoPublicado ||
            string.IsNullOrWhiteSpace(transicion.IntentoId) ||
            transicion.FechaIntento == default ||
            transicion.CalificacionIntento is < 0 or > 100 ||
            transicion.MejorCalificacionPosterior is < 0 or > 100 ||
            transicion.TotalIntentos <= 0 ||
            !practicaId.Equals(
                transicion.PracticaId,
                StringComparison.OrdinalIgnoreCase) ||
            !practicaId.Equals(
                historial.PracticaId,
                StringComparison.OrdinalIgnoreCase) ||
            historial.TotalIntentos != transicion.TotalIntentos ||
            historial.MejorCalificacion != transicion.MejorCalificacionPosterior ||
            historial.UltimaCalificacion != transicion.CalificacionIntento ||
            historial.FechaUltimoIntento != transicion.FechaIntento) {
            return false;
        }

        IntentoPractica[] coincidencias = historial.Intentos
            .Where(item => item.Id.Equals(
                transicion.IntentoId,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (coincidencias.Length != 1 ||
            coincidencias[0].Fecha != transicion.FechaIntento ||
            coincidencias[0].Calificacion != transicion.CalificacionIntento) {
            return false;
        }

        if (transicion.MejorCalificacionAnterior is not int mejorAnterior) {
            return transicion.TotalIntentos == 1 &&
                transicion.MejorCalificacionPosterior ==
                    transicion.CalificacionIntento &&
                !transicion.UltimaCalificacionAnterior.HasValue &&
                !transicion.FechaUltimoIntentoAnterior.HasValue;
        }

        return transicion.TotalIntentos >= 2 &&
            mejorAnterior is >= 0 and <= 100 &&
            transicion.MejorCalificacionPosterior == Math.Max(
                mejorAnterior,
                transicion.CalificacionIntento) &&
            transicion.UltimaCalificacionAnterior.HasValue ==
                transicion.FechaUltimoIntentoAnterior.HasValue;
    }

    private static void ReclasificarConcesionesCreadasEnOperacionActual(
        DocumentoMotivacion documento,
        ContextoMutacion contexto) {
        if (contexto.ClavesHitosOperacionActual.Count == 0) {
            return;
        }

        foreach (ConcesionXP concesion in documento.ConcesionesXP.Where(item =>
            item.EsImportada &&
            contexto.ClavesCreadasEnOperacion.Contains(item.Clave) &&
            contexto.ClavesHitosOperacionActual.Contains(item.Clave))) {
            concesion.EsImportada = false;
            contexto.ClavesConcedidasOperacionActual.Add(concesion.Clave);
            contexto.HuboCambio = true;
        }
    }

    private static long CalcularXpConcedidoOperacionActual(
        DocumentoMotivacion documento,
        ContextoMutacion contexto) {
        long resultado = 0;

        foreach (ConcesionXP concesion in documento.ConcesionesXP.Where(item =>
            contexto.ClavesConcedidasOperacionActual.Contains(item.Clave))) {
            resultado = checked(resultado + concesion.CantidadXP);
        }

        return resultado;
    }

    private static IReadOnlyList<string> ObtenerClavesResultado(
        ContextoMutacion contexto) {
        return contexto.ReportarSoloConcesionesOperacionActual
            ? contexto.ClavesConcedidasOperacionActual
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item, StringComparer.Ordinal)
                .ToArray()
            : contexto.ClavesProcesadas.AsReadOnly();
    }

    private static ProgresoCurso CopiarProgreso(ProgresoCurso progreso) {
        return new ProgresoCurso {
            Practicas = progreso.Practicas
                .Select(item => new ProgresoPractica {
                    PracticaId = item.PracticaId,
                    Estado = item.Estado,
                    RutaProyecto = item.RutaProyecto,
                    FechaCreacion = item.FechaCreacion,
                    FechaActualizacion = item.FechaActualizacion,
                    FechaFinalizacion = item.FechaFinalizacion
                })
                .ToList()
        };
    }

    private static HistorialPractica CopiarHistorial(
        HistorialPractica historial) {
        return new HistorialPractica {
            PracticaId = historial.PracticaId,
            TotalIntentos = historial.TotalIntentos,
            MejorCalificacion = historial.MejorCalificacion,
            UltimaCalificacion = historial.UltimaCalificacion,
            FechaUltimoIntento = historial.FechaUltimoIntento,
            Intentos = historial.Intentos.ToArray()
        };
    }

    private static IntentoPractica CopiarIntento(IntentoPractica intento) {
        return new IntentoPractica {
            Id = intento.Id,
            PracticaId = intento.PracticaId,
            Fecha = intento.Fecha,
            Calificacion = intento.Calificacion,
            Compilo = intento.Compilo,
            PruebasSuperadas = intento.PruebasSuperadas,
            PruebasTotales = intento.PruebasTotales,
            ResultadoGeneral = intento.ResultadoGeneral,
            EjecucionFinalizada = intento.EjecucionFinalizada,
            PuntosObtenidos = intento.PuntosObtenidos,
            PuntosMaximos = intento.PuntosMaximos,
            RutaProyecto = intento.RutaProyecto,
            Resultados = intento.Resultados.ToArray(),
            Retroalimentacion = intento.Retroalimentacion.ToArray()
        };
    }

    private static TransicionEvaluacionPersistida CopiarTransicionEvaluacion(
        TransicionEvaluacionPersistida transicion) {
        return new TransicionEvaluacionPersistida {
            PracticaId = transicion.PracticaId,
            IntentoId = transicion.IntentoId,
            FechaIntento = transicion.FechaIntento,
            CalificacionIntento = transicion.CalificacionIntento,
            MejorCalificacionAnterior = transicion.MejorCalificacionAnterior,
            UltimaCalificacionAnterior = transicion.UltimaCalificacionAnterior,
            FechaUltimoIntentoAnterior = transicion.FechaUltimoIntentoAnterior,
            MejorCalificacionPosterior = transicion.MejorCalificacionPosterior,
            TotalIntentos = transicion.TotalIntentos,
            IntentoPublicado = transicion.IntentoPublicado
        };
    }

    private static TransicionProgresoPersistida CopiarTransicionProgreso(
        TransicionProgresoPersistida transicion) {
        return new TransicionProgresoPersistida {
            PracticaId = transicion.PracticaId,
            ProgresoAnterior = transicion.ProgresoAnterior is null
                ? null
                : CopiarProgresoPractica(transicion.ProgresoAnterior),
            ProgresoFinal = CopiarProgresoPractica(transicion.ProgresoFinal),
            PracticaCreada = transicion.PracticaCreada,
            VinculoPersistidoAhora = transicion.VinculoPersistidoAhora,
            RealizadaPersistidaAhora = transicion.RealizadaPersistidaAhora
        };
    }

    private static ProgresoPractica CopiarProgresoPractica(
        ProgresoPractica practica) {
        return new ProgresoPractica {
            PracticaId = practica.PracticaId,
            Estado = practica.Estado,
            RutaProyecto = practica.RutaProyecto,
            FechaCreacion = practica.FechaCreacion,
            FechaActualizacion = practica.FechaActualizacion,
            FechaFinalizacion = practica.FechaFinalizacion
        };
    }

    private static bool EsTransicionProgresoValida(
        string practicaId,
        ProgresoPractica progresoPersistido,
        TransicionProgresoPersistida transicion) {
        ProgresoPractica final = transicion.ProgresoFinal;

        if (!practicaId.Equals(
                transicion.PracticaId,
                StringComparison.OrdinalIgnoreCase) ||
            !practicaId.Equals(
                final.PracticaId,
                StringComparison.OrdinalIgnoreCase) ||
            !PracticasEquivalentes(progresoPersistido, final) ||
            transicion.PracticaCreada !=
                (transicion.ProgresoAnterior is null)) {
            return false;
        }

        bool vinculoPersistidoAhora =
            string.IsNullOrWhiteSpace(transicion.ProgresoAnterior?.RutaProyecto) &&
            !string.IsNullOrWhiteSpace(final.RutaProyecto);
        bool realizadaPersistidaAhora =
            transicion.ProgresoAnterior?.Estado != EstadoPracticaCurso.Realizada &&
            final.Estado == EstadoPracticaCurso.Realizada;
        return transicion.VinculoPersistidoAhora == vinculoPersistidoAhora &&
            transicion.RealizadaPersistidaAhora == realizadaPersistidaAhora &&
            (!vinculoPersistidoAhora && !realizadaPersistidaAhora ||
                final.FechaActualizacion.HasValue &&
                final.FechaActualizacion.Value != default);
    }

    private static bool PracticasEquivalentes(
        ProgresoPractica izquierda,
        ProgresoPractica derecha) {
        return izquierda.PracticaId.Equals(
                derecha.PracticaId,
                StringComparison.OrdinalIgnoreCase) &&
            izquierda.Estado == derecha.Estado &&
            string.Equals(
                izquierda.RutaProyecto,
                derecha.RutaProyecto,
                StringComparison.OrdinalIgnoreCase) &&
            izquierda.FechaCreacion == derecha.FechaCreacion &&
            izquierda.FechaActualizacion == derecha.FechaActualizacion &&
            izquierda.FechaFinalizacion == derecha.FechaFinalizacion;
    }

    private ResultadoFuentes CargarFuentesAcademicas(
        bool cargarSoloProgreso = false,
        bool cargarSoloHistorial = false) {
        try {
            CatalogoAprendizajeSnapshot catalogo =
                CatalogoAprendizajeSnapshot.Crear(cargarCatalogo());
            ResultadoCargaProgreso? progreso = cargarSoloHistorial
                ? null
                : cargarProgreso();
            ResultadoCargaHistorialEvaluaciones? historial = cargarSoloProgreso
                ? null
                : cargarHistorial();
            bool progresoDisponible = progreso is not null &&
                EsProgresoUtilizable(progreso);
            bool historialDisponible = historial is not null &&
                EsHistorialUtilizable(historial);
            int progresoHuerfano = 0;
            int historialHuerfano = 0;
            IReadOnlyDictionary<string, ProgresoPractica> progresoNormalizado =
                progresoDisponible
                    ? DatosAprendizajeNormalizados.CrearProgreso(
                        catalogo,
                        progreso!.Progreso.Practicas,
                        out progresoHuerfano)
                    : new Dictionary<string, ProgresoPractica>();
            IReadOnlyDictionary<string, HistorialPractica> historialNormalizado =
                historialDisponible
                    ? CrearHistorialConIntentos(
                        catalogo,
                        historial!.Historial.Practicas,
                        out historialHuerfano)
                    : new Dictionary<string, HistorialPractica>();
            bool progresoParcial =
                progreso?.Estado == EstadoCargaProgreso.ContenidoInvalido ||
                progresoDisponible && progresoHuerfano > 0;
            bool historialParcial =
                historial?.Estado ==
                    EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido ||
                historialDisponible && historialHuerfano > 0;
            Exception? error = progreso is not null && !progresoDisponible
                ? progreso.Error ?? new InvalidDataException(
                    "progreso.json no contiene registros recuperables.")
                : historial is not null && !historialDisponible
                    ? historial.Error ?? new InvalidDataException(
                        "historial-evaluaciones.json no contiene registros recuperables.")
                    : null;

            return new ResultadoFuentes(
                catalogo,
                progresoNormalizado,
                historialNormalizado,
                cargarSoloHistorial || progresoDisponible,
                cargarSoloProgreso || historialDisponible,
                progresoParcial,
                historialParcial,
                error);
        } catch (Exception ex) when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoFuentes(
                null,
                new Dictionary<string, ProgresoPractica>(),
                new Dictionary<string, HistorialPractica>(),
                false,
                false,
                false,
                false,
                ex);
        }
    }

    private ResultadoFuentes CrearFuentesDesdeProgreso(
        ProgresoCurso progresoPersistido) {
        try {
            CatalogoAprendizajeSnapshot catalogo =
                CatalogoAprendizajeSnapshot.Crear(cargarCatalogo());
            IReadOnlyDictionary<string, ProgresoPractica> progreso =
                DatosAprendizajeNormalizados.CrearProgreso(
                    catalogo,
                    progresoPersistido.Practicas,
                    out int registrosHuerfanos);
            return new ResultadoFuentes(
                catalogo,
                progreso,
                new Dictionary<string, HistorialPractica>(),
                true,
                true,
                registrosHuerfanos > 0,
                false,
                null);
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoFuentes(
                null,
                new Dictionary<string, ProgresoPractica>(),
                new Dictionary<string, HistorialPractica>(),
                false,
                true,
                false,
                false,
                ex);
        }
    }

    private ResultadoFuentes CrearFuentesDesdeHistorial(
        HistorialPractica historialPersistido) {
        try {
            CatalogoAprendizajeSnapshot catalogo =
                CatalogoAprendizajeSnapshot.Crear(cargarCatalogo());
            IReadOnlyDictionary<string, HistorialPractica> historial =
                CrearHistorialConIntentos(
                    catalogo,
                    new[] { historialPersistido },
                    out int registrosHuerfanos);
            return new ResultadoFuentes(
                catalogo,
                new Dictionary<string, ProgresoPractica>(),
                historial,
                true,
                true,
                false,
                registrosHuerfanos > 0,
                null);
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoFuentes(
                null,
                new Dictionary<string, ProgresoPractica>(),
                new Dictionary<string, HistorialPractica>(),
                true,
                false,
                false,
                false,
                ex);
        }
    }

    private static bool EsProgresoUtilizable(ResultadoCargaProgreso resultado) {
        return resultado.Estado switch {
            EstadoCargaProgreso.Exitosa or
            EstadoCargaProgreso.ArchivoInexistente or
            EstadoCargaProgreso.ArchivoVacio => true,
            EstadoCargaProgreso.ContenidoInvalido =>
                resultado.Progreso.Practicas.Count > 0,
            _ => false
        };
    }

    private static bool EsHistorialUtilizable(
        ResultadoCargaHistorialEvaluaciones resultado) {
        return resultado.Estado switch {
            EstadoCargaHistorialEvaluaciones.Exitosa or
            EstadoCargaHistorialEvaluaciones.ArchivoInexistente or
            EstadoCargaHistorialEvaluaciones.ArchivoVacio => true,
            EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido =>
                resultado.Historial.Practicas.Count > 0,
            _ => false
        };
    }

    private static IReadOnlyDictionary<string, HistorialPractica>
        CrearHistorialConIntentos(
            CatalogoAprendizajeSnapshot catalogo,
            IEnumerable<HistorialPractica> historial,
            out int huerfanos) {
        Dictionary<string, HistorialPractica> resultado =
            new(StringComparer.OrdinalIgnoreCase);
        huerfanos = 0;

        foreach (HistorialPractica item in historial) {
            if (item is null ||
                string.IsNullOrWhiteSpace(item.PracticaId) ||
                !catalogo.PracticasPorId.ContainsKey(item.PracticaId)) {
                huerfanos++;
                continue;
            }

            if (!resultado.TryAdd(item.PracticaId, item)) {
                huerfanos++;
            }
        }

        return resultado;
    }

    private static bool IntentarCalcularMejoraHistorica(
        HistorialPractica historial,
        out int mejora) {
        mejora = 0;
        IReadOnlyList<IntentoPractica> intentos = historial.Intentos;

        if (historial.TotalIntentos <= 0 ||
            historial.TotalIntentos != intentos.Count ||
            intentos.Count == 0 ||
            !historial.MejorCalificacion.HasValue ||
            !historial.UltimaCalificacion.HasValue ||
            !historial.FechaUltimoIntento.HasValue ||
            intentos.Select(item => item.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != intentos.Count ||
            intentos.Select(item => item.Fecha)
                .Distinct()
                .Count() != intentos.Count) {
            return false;
        }

        DateTimeOffset fechaPrimera = intentos.Min(item => item.Fecha);
        DateTimeOffset fechaUltima = intentos.Max(item => item.Fecha);
        IntentoPractica[] primeros = intentos
            .Where(item => item.Fecha == fechaPrimera)
            .ToArray();
        IntentoPractica[] ultimos = intentos
            .Where(item => item.Fecha == fechaUltima)
            .ToArray();

        if (primeros.Length != 1 ||
            ultimos.Length != 1 ||
            historial.FechaUltimoIntento.Value != fechaUltima ||
            historial.UltimaCalificacion.Value != ultimos[0].Calificacion ||
            intentos.Max(item => item.Calificacion) !=
                historial.MejorCalificacion.Value) {
            return false;
        }

        mejora = Math.Max(
            0,
            historial.MejorCalificacion.Value - primeros[0].Calificacion);
        return true;
    }

    private void PrepararInstanteOperacion(
        DocumentoMotivacion documento,
        ContextoMutacion contexto) {
        DateTimeOffset ahora = ObtenerAhoraUtc();

        if (ahora + ToleranciaRetrocesoReloj <
            documento.UltimoInstanteUtcAceptado) {
            contexto.Advertencias.Add(
                AdvertenciaMotivacion.RetrocesoRelojDetectado);
        }

        contexto.InstanteConcesionUtc = ahora >
            documento.UltimoInstanteUtcAceptado
                ? ahora
                : documento.UltimoInstanteUtcAceptado;
    }

    private static void ActualizarInstanteAceptado(
        DocumentoMotivacion documento,
        ContextoMutacion contexto) {
        DateTimeOffset instante = contexto.InstanteConcesionUtc ??
            documento.UltimoInstanteUtcAceptado;
        documento.UltimoInstanteUtcAceptado = instante;

        foreach (ConcesionXP concesion in documento.ConcesionesXP.Where(item =>
            item.FechaUtc == default)) {
            concesion.FechaUtc = instante;
        }
    }

    private DateTimeOffset ObtenerAhoraUtc() {
        return reloj.GetUtcNow().ToUniversalTime();
    }

    private ResumenMotivacion CrearResumen(
        DocumentoMotivacion documento,
        IEnumerable<AdvertenciaMotivacion>? advertencias = null) {
        HashSet<AdvertenciaMotivacion> todas = advertencias is null
            ? new HashSet<AdvertenciaMotivacion>()
            : new HashSet<AdvertenciaMotivacion>(advertencias);

        ResumenRacha racha;

        try {
            TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById(
                documento.ZonaHorariaEstudio);
            DateTimeOffset referenciaUtc = ObtenerAhoraUtc();

            if (documento.UltimoInstanteUtcAceptado > referenciaUtc) {
                referenciaUtc = documento.UltimoInstanteUtcAceptado;
            }

            racha = calculadoraRacha.Calcular(
                documento.DiasActividadAcademica,
                zona,
                referenciaUtc);
        } catch (Exception ex) when (ex is TimeZoneNotFoundException or
            InvalidTimeZoneException) {
            todas.Add(AdvertenciaMotivacion.ZonaHorariaNoDisponible);
            racha = CrearResumenRachaSinZona(
                documento.DiasActividadAcademica);
        }

        long xpTotal = CalcularXpTotal(documento);
        return new ResumenMotivacion(
            xpTotal == 0 &&
                documento.LogrosDesbloqueados.Count == 0 &&
                documento.DiasActividadAcademica.Count == 0
                ? EstadoDisponibilidadMotivacion.SinActividad
                : EstadoDisponibilidadMotivacion.Disponible,
            xpTotal,
            calculadoraNivel.Calcular(xpTotal),
            documento.ZonaHorariaEstudio,
            documento.UltimoInstanteUtcAceptado,
            todas.OrderBy(item => item).ToArray(),
            null) {
            Racha = racha,
            LogrosDesbloqueados = documento.LogrosDesbloqueados
                .Select(CopiarLogro)
                .ToArray()
        };
    }

    private static ResumenRacha CrearResumenRachaSinZona(
        IEnumerable<DateOnly> dias) {
        DateOnly[] ordenados = dias
            .Distinct()
            .OrderBy(item => item.DayNumber)
            .ToArray();

        if (ordenados.Length == 0) {
            return new ResumenRacha(0, 0, null);
        }

        int actual = 0;
        int mejor = 0;
        DateOnly? anterior = null;

        foreach (DateOnly dia in ordenados) {
            actual = anterior.HasValue &&
                dia.DayNumber == anterior.Value.DayNumber + 1
                    ? actual + 1
                    : 1;
            mejor = Math.Max(mejor, actual);
            anterior = dia;
        }

        return new ResumenRacha(0, mejor, ordenados[^1]);
    }

    private ResultadoProcesamientoMotivacion CrearResultadoNoDisponible(
        EstadoProcesamientoMotivacion estado,
        Exception? error,
        bool versionIncompatible = false) {
        ResumenMotivacion resumen = new(
            versionIncompatible
                ? EstadoDisponibilidadMotivacion.VersionIncompatible
                : EstadoDisponibilidadMotivacion.NoDisponible,
            null,
            null,
            string.Empty,
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            error);
        return new ResultadoProcesamientoMotivacion(
            estado,
            0,
            null,
            null,
            null,
            false,
            Array.Empty<string>(),
            resumen,
            error);
    }

    private ResultadoCargaDocumento CargarDocumentoSinBloqueo(string ruta) {
        if (!archivos.ArchivoExiste(ruta)) {
            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.ArchivoInexistente,
                null,
                null);
        }

        try {
            if (archivos.ObtenerLongitud(ruta) > MaximoBytesArchivo) {
                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.ContenidoInvalido,
                    null,
                    new InvalidDataException(
                        "motivacion.json supera el tamaño máximo admitido."));
            }

            string contenido = archivos.LeerTodoTexto(ruta);

            if (string.IsNullOrWhiteSpace(contenido)) {
                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.ContenidoInvalido,
                    null,
                    new InvalidDataException("motivacion.json está vacío."));
            }

            using JsonDocument json = JsonDocument.Parse(contenido);

            if (TienePropiedadesDuplicadas(json.RootElement)) {
                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.ContenidoInvalido,
                    null,
                    new InvalidDataException(
                        "motivacion.json contiene propiedades duplicadas."));
            }

            if (!IntentarLeerVersion(json.RootElement, out int version)) {
                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.ContenidoInvalido,
                    null,
                    new InvalidDataException(
                        "motivacion.json no contiene una versión válida."));
            }

            if (version > VersionActual || version < VersionAnterior) {
                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.VersionIncompatible,
                    null,
                    new InvalidDataException(
                        $"La versión {version} de motivacion.json no es compatible."));
            }

            if (version == VersionAnterior) {
                DocumentoMotivacionVersion1? anterior =
                    JsonSerializer.Deserialize<DocumentoMotivacionVersion1>(
                        contenido,
                        opcionesJson);
                Exception? errorAnterior = null;

                if (!ContienePropiedadesObligatoriasVersion1(json.RootElement) ||
                    anterior is null ||
                    !IntentarValidarDocumentoVersion1(
                        anterior,
                        out errorAnterior)) {
                    return new ResultadoCargaDocumento(
                        EstadoCargaDocumento.ContenidoInvalido,
                        null,
                        errorAnterior ?? new InvalidDataException(
                            "motivacion.json Version 1 contiene datos invÃ¡lidos."));
                }

                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.RequiereMigracion,
                    null,
                    null,
                    anterior);
            }

            DocumentoMotivacion? documento =
                JsonSerializer.Deserialize<DocumentoMotivacion>(
                    contenido,
                    opcionesJson);
            Exception? error = null;

            if (!ContienePropiedadesObligatoriasVersion2(json.RootElement) ||
                documento is null ||
                !IntentarNormalizarYValidarDocumento(
                    documento,
                    out error)) {
                return new ResultadoCargaDocumento(
                    EstadoCargaDocumento.ContenidoInvalido,
                    null,
                    error ?? new InvalidDataException(
                        "motivacion.json contiene datos inválidos."));
            }

            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.Exitosa,
                documento,
                null);
        } catch (UnauthorizedAccessException ex) {
            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.PermisosInsuficientes,
                null,
                ex);
        } catch (SecurityException ex) {
            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.PermisosInsuficientes,
                null,
                ex);
        } catch (JsonException ex) {
            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.ContenidoInvalido,
                null,
                ex);
        } catch (IOException ex) {
            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.ErrorIo,
                null,
                ex);
        } catch (Exception ex) when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoCargaDocumento(
                EstadoCargaDocumento.ContenidoInvalido,
                null,
                ex);
        }
    }

    private void GuardarDocumentoSinBloqueo(
        DocumentoMotivacion documento,
        bool ordenarConcesiones = true) {
        if (ordenarConcesiones) {
            documento.ConcesionesXP = documento.ConcesionesXP
                .OrderBy(item => item.Clave, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.Clave, StringComparer.Ordinal)
                .ToList();
        }

        documento.LogrosDesbloqueados = documento.LogrosDesbloqueados
            .OrderBy(item => item.LogroId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.LogroId, StringComparer.Ordinal)
            .ToList();
        documento.DiasActividadAcademica = documento.DiasActividadAcademica
            .Distinct()
            .OrderBy(item => item.DayNumber)
            .ToList();

        if (!IntentarNormalizarYValidarDocumento(documento, out Exception? error)) {
            throw error ?? new InvalidDataException(
                "El documento motivacional no es válido.");
        }

        string contenido = JsonSerializer.Serialize(documento, opcionesJson);

        if (Encoding.UTF8.GetByteCount(contenido) > MaximoBytesArchivo) {
            throw new InvalidDataException(
                "El documento motivacional supera el tamaño máximo admitido.");
        }

        string temporal = Path.Combine(
            carpetaDatos,
            $".motivacion-{Guid.NewGuid():N}.tmp");

        try {
            archivos.EscribirTodoTextoDurable(temporal, contenido);

            if (archivos.ArchivoExiste(RutaMotivacion)) {
                try {
                    archivos.Reemplazar(temporal, RutaMotivacion);
                    temporal = string.Empty;
                } catch (FileNotFoundException) when (
                    !archivos.ArchivoExiste(RutaMotivacion)) {
                    archivos.Mover(temporal, RutaMotivacion);
                    temporal = string.Empty;
                }
            } else {
                archivos.Mover(temporal, RutaMotivacion);
                temporal = string.Empty;
            }
        } finally {
            LimpiarTemporal(temporal);
        }
    }

    private void RecuperarInterrupcionSinBloqueo() {
        string[] temporales = archivos.EnumerarArchivos(
                carpetaDatos,
                ".motivacion-*.tmp")
            .OrderByDescending(archivos.ObtenerUltimaEscrituraUtc)
            .ThenBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (temporales.Length == 0) {
            return;
        }

        if (archivos.ArchivoExiste(RutaMotivacion)) {
            foreach (string temporal in temporales) {
                LimpiarTemporal(temporal);
            }

            return;
        }

        string? recuperable = temporales.FirstOrDefault(temporal => {
            EstadoCargaDocumento estado =
                CargarDocumentoSinBloqueo(temporal).Estado;
            return estado is EstadoCargaDocumento.Exitosa or
                EstadoCargaDocumento.RequiereMigracion or
                EstadoCargaDocumento.VersionIncompatible;
        });

        if (recuperable is not null) {
            archivos.Mover(recuperable, RutaMotivacion);
        }

        foreach (string temporal in temporales.Where(item =>
            !item.Equals(recuperable, StringComparison.OrdinalIgnoreCase))) {
            LimpiarTemporal(temporal);
        }
    }

    private void LimpiarTemporal(string? ruta) {
        if (string.IsNullOrWhiteSpace(ruta)) {
            return;
        }

        try {
            if (archivos.ArchivoExiste(ruta)) {
                archivos.Eliminar(ruta);
            }
        } catch (Exception) {
            // La limpieza nunca debe ocultar el resultado principal.
        }
    }

    private static bool ContienePropiedadesObligatoriasVersion2(
        JsonElement raiz) {
        if (!ContienePropiedades(
                raiz,
                nameof(DocumentoMotivacion.Version),
                nameof(DocumentoMotivacion.ZonaHorariaEstudio),
                nameof(DocumentoMotivacion.ConcesionesXP),
                nameof(DocumentoMotivacion.MejorCalificacionReconocidaPorPractica),
                nameof(DocumentoMotivacion.XPMejoraConcedidoPorPractica),
                nameof(DocumentoMotivacion.UltimoInstanteUtcAceptado),
                nameof(DocumentoMotivacion.MetadatosMigracion),
                nameof(DocumentoMotivacion.LogrosDesbloqueados),
                nameof(DocumentoMotivacion.DiasActividadAcademica))) {
            return false;
        }

        if (!ColeccionContieneObjetosConPropiedades(
                raiz,
                nameof(DocumentoMotivacion.ConcesionesXP),
                nameof(ConcesionXP.Clave),
                nameof(ConcesionXP.CantidadXP),
                nameof(ConcesionXP.FechaUtc),
                nameof(ConcesionXP.Tipo),
                nameof(ConcesionXP.PracticaId),
                nameof(ConcesionXP.TemaId),
                nameof(ConcesionXP.GradoId),
                nameof(ConcesionXP.EsImportada)) ||
            !ColeccionContieneObjetosConPropiedades(
                raiz,
                nameof(DocumentoMotivacion.LogrosDesbloqueados),
                nameof(LogroDesbloqueado.LogroId),
                nameof(LogroDesbloqueado.FechaReconocimientoUtc),
                nameof(LogroDesbloqueado.EsImportado))) {
            return false;
        }

        JsonElement metadatos = raiz.EnumerateObject()
            .First(item => item.Name.Equals(
                nameof(DocumentoMotivacion.MetadatosMigracion),
                StringComparison.OrdinalIgnoreCase))
            .Value;
        return ContienePropiedades(
            metadatos,
            nameof(MetadatosMigracionMotivacion.VersionMigracion),
            nameof(MetadatosMigracionMotivacion.MigracionInicialCompletada),
            nameof(MetadatosMigracionMotivacion.FechaMigracionUtc),
            nameof(MetadatosMigracionMotivacion.ProgresoProcesado),
            nameof(MetadatosMigracionMotivacion.HistorialProcesado),
            nameof(MetadatosMigracionMotivacion.MejorasHistoricasReconocidas),
            nameof(MetadatosMigracionMotivacion.MejorasHistoricasOmitidas),
            nameof(MetadatosMigracionMotivacion.UltimaReconciliacionUtc),
            nameof(MetadatosMigracionMotivacion.MigracionVersion2Completada),
            nameof(MetadatosMigracionMotivacion.FechaMigracionVersion2Utc),
            nameof(MetadatosMigracionMotivacion.LogrosHistoricosProcesados),
            nameof(MetadatosMigracionMotivacion.ActividadHistoricaProcesada),
            nameof(MetadatosMigracionMotivacion.HistoriaActividadParcial));
    }

    private static bool ContienePropiedadesObligatoriasVersion1(
        JsonElement raiz) {
        if (!ContienePropiedades(
                raiz,
                nameof(DocumentoMotivacionVersion1.Version),
                nameof(DocumentoMotivacionVersion1.ZonaHorariaEstudio),
                nameof(DocumentoMotivacionVersion1.ConcesionesXP),
                nameof(DocumentoMotivacionVersion1.MejorCalificacionReconocidaPorPractica),
                nameof(DocumentoMotivacionVersion1.XPMejoraConcedidoPorPractica),
                nameof(DocumentoMotivacionVersion1.UltimoInstanteUtcAceptado),
                nameof(DocumentoMotivacionVersion1.MetadatosMigracion))) {
            return false;
        }

        if (!ColeccionContieneObjetosConPropiedades(
                raiz,
                nameof(DocumentoMotivacionVersion1.ConcesionesXP),
                nameof(ConcesionXP.Clave),
                nameof(ConcesionXP.CantidadXP),
                nameof(ConcesionXP.FechaUtc),
                nameof(ConcesionXP.Tipo),
                nameof(ConcesionXP.PracticaId),
                nameof(ConcesionXP.TemaId),
                nameof(ConcesionXP.GradoId),
                nameof(ConcesionXP.EsImportada))) {
            return false;
        }

        JsonElement metadatos = raiz.EnumerateObject()
            .First(item => item.Name.Equals(
                nameof(DocumentoMotivacionVersion1.MetadatosMigracion),
                StringComparison.OrdinalIgnoreCase))
            .Value;
        return ContienePropiedades(
            metadatos,
            nameof(MetadatosMigracionMotivacionVersion1.VersionMigracion),
            nameof(MetadatosMigracionMotivacionVersion1.MigracionInicialCompletada),
            nameof(MetadatosMigracionMotivacionVersion1.FechaMigracionUtc),
            nameof(MetadatosMigracionMotivacionVersion1.ProgresoProcesado),
            nameof(MetadatosMigracionMotivacionVersion1.HistorialProcesado),
            nameof(MetadatosMigracionMotivacionVersion1.MejorasHistoricasReconocidas),
            nameof(MetadatosMigracionMotivacionVersion1.MejorasHistoricasOmitidas),
            nameof(MetadatosMigracionMotivacionVersion1.UltimaReconciliacionUtc));
    }

    private static bool ColeccionContieneObjetosConPropiedades(
        JsonElement raiz,
        string nombreColeccion,
        params string[] propiedades) {
        JsonElement coleccion = raiz.EnumerateObject()
            .First(item => item.Name.Equals(
                nombreColeccion,
                StringComparison.OrdinalIgnoreCase))
            .Value;
        return coleccion.ValueKind == JsonValueKind.Array &&
            coleccion.EnumerateArray().All(item =>
                ContienePropiedades(item, propiedades));
    }

    private static bool ContienePropiedades(
        JsonElement elemento,
        params string[] nombres) {
        if (elemento.ValueKind != JsonValueKind.Object) {
            return false;
        }

        HashSet<string> presentes = elemento.EnumerateObject()
            .Select(item => item.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return nombres.All(presentes.Contains);
    }

    private static bool IntentarValidarDocumentoVersion1(
        DocumentoMotivacionVersion1 documento,
        out Exception? error) {
        error = null;

        try {
            MetadatosMigracionMotivacionVersion1? metadatos =
                documento.MetadatosMigracion;

            if (documento.Version != VersionAnterior ||
                string.IsNullOrWhiteSpace(documento.ZonaHorariaEstudio) ||
                documento.ConcesionesXP is null ||
                documento.MejorCalificacionReconocidaPorPractica is null ||
                documento.XPMejoraConcedidoPorPractica is null ||
                !documento.UltimoInstanteUtcAceptado.HasValue ||
                metadatos is null ||
                !metadatos.VersionMigracion.HasValue ||
                !metadatos.MigracionInicialCompletada.HasValue ||
                !metadatos.FechaMigracionUtc.HasValue ||
                !metadatos.ProgresoProcesado.HasValue ||
                !metadatos.HistorialProcesado.HasValue ||
                !metadatos.MejorasHistoricasReconocidas.HasValue ||
                !metadatos.MejorasHistoricasOmitidas.HasValue) {
                throw new InvalidDataException(
                    "motivacion.json Version 1 no contiene todos sus campos obligatorios.");
            }

            DocumentoMotivacion copia = ConvertirDocumentoVersion1(
                documento,
                documento.UltimoInstanteUtcAceptado.Value);

            if (!IntentarNormalizarYValidarDocumento(copia, out error)) {
                throw error ?? new InvalidDataException(
                    "motivacion.json Version 1 contiene datos invalidos.");
            }

            return true;
        } catch (Exception ex) when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            error = ex;
            return false;
        }
    }

    private static DocumentoMotivacion ConvertirDocumentoVersion1(
        DocumentoMotivacionVersion1 anterior,
        DateTimeOffset fechaMigracionVersion2Utc) {
        MetadatosMigracionMotivacionVersion1 metadatos =
            anterior.MetadatosMigracion!;
        return new DocumentoMotivacion {
            Version = VersionActual,
            ZonaHorariaEstudio = anterior.ZonaHorariaEstudio!,
            ConcesionesXP = anterior.ConcesionesXP!
                .Select(CopiarConcesion)
                .ToList(),
            MejorCalificacionReconocidaPorPractica = new Dictionary<string, int>(
                anterior.MejorCalificacionReconocidaPorPractica!,
                StringComparer.OrdinalIgnoreCase),
            XPMejoraConcedidoPorPractica = new Dictionary<string, int>(
                anterior.XPMejoraConcedidoPorPractica!,
                StringComparer.OrdinalIgnoreCase),
            UltimoInstanteUtcAceptado =
                anterior.UltimoInstanteUtcAceptado!.Value,
            MetadatosMigracion = new MetadatosMigracionMotivacion {
                VersionMigracion = metadatos.VersionMigracion!.Value,
                MigracionInicialCompletada =
                    metadatos.MigracionInicialCompletada!.Value,
                FechaMigracionUtc = metadatos.FechaMigracionUtc!.Value,
                ProgresoProcesado = metadatos.ProgresoProcesado!.Value,
                HistorialProcesado = metadatos.HistorialProcesado!.Value,
                MejorasHistoricasReconocidas =
                    metadatos.MejorasHistoricasReconocidas!.Value,
                MejorasHistoricasOmitidas =
                    metadatos.MejorasHistoricasOmitidas!.Value,
                UltimaReconciliacionUtc = metadatos.UltimaReconciliacionUtc,
                MigracionVersion2Completada = true,
                FechaMigracionVersion2Utc = fechaMigracionVersion2Utc,
                LogrosHistoricosProcesados = false,
                ActividadHistoricaProcesada = false,
                HistoriaActividadParcial = true
            },
            LogrosDesbloqueados = new List<LogroDesbloqueado>(),
            DiasActividadAcademica = new List<DateOnly>()
        };
    }

    private static ConcesionXP CopiarConcesion(ConcesionXP concesion) {
        return new ConcesionXP {
            Clave = concesion.Clave,
            CantidadXP = concesion.CantidadXP,
            FechaUtc = concesion.FechaUtc,
            Tipo = concesion.Tipo,
            PracticaId = concesion.PracticaId,
            TemaId = concesion.TemaId,
            GradoId = concesion.GradoId,
            EsImportada = concesion.EsImportada
        };
    }

    private static bool EsIdentificadorLogroValido(string? logroId) {
        return !string.IsNullOrWhiteSpace(logroId) &&
            logroId.Length <= MaximoLongitudIdentificador &&
            logroId.Equals(logroId.Trim(), StringComparison.Ordinal) &&
            logroId.StartsWith("logro:", StringComparison.OrdinalIgnoreCase) &&
            !logroId.Any(char.IsControl);
    }

    private static void ValidarDiasNoFuturos(DocumentoMotivacion documento) {
        if (documento.DiasActividadAcademica.Count == 0) {
            return;
        }

        try {
            TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById(
                documento.ZonaHorariaEstudio);
            DateTimeOffset limiteUtc =
                documento.MetadatosMigracion.FechaMigracionVersion2Utc!.Value >
                    documento.UltimoInstanteUtcAceptado
                    ? documento.MetadatosMigracion.FechaMigracionVersion2Utc.Value
                    : documento.UltimoInstanteUtcAceptado;
            DateOnly limite = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime(limiteUtc, zona).DateTime);

            if (documento.DiasActividadAcademica[^1] > limite) {
                throw new InvalidDataException(
                    "motivacion.json contiene actividad academica futura.");
            }
        } catch (Exception ex) when (ex is TimeZoneNotFoundException or
            InvalidTimeZoneException) {
            // Una zona no disponible se informa en el resumen sin destruir datos.
        }
    }

    private static bool IntentarNormalizarYValidarDocumento(
        DocumentoMotivacion documento,
        out Exception? error) {
        error = null;

        try {
            if (documento.Version != VersionActual ||
                string.IsNullOrWhiteSpace(documento.ZonaHorariaEstudio) ||
                documento.ConcesionesXP is null ||
                documento.MejorCalificacionReconocidaPorPractica is null ||
                documento.XPMejoraConcedidoPorPractica is null ||
                documento.MetadatosMigracion is null ||
                documento.LogrosDesbloqueados is null ||
                documento.DiasActividadAcademica is null ||
                documento.ConcesionesXP.Count > MaximoConcesiones ||
                documento.LogrosDesbloqueados.Count > MaximoLogros ||
                documento.DiasActividadAcademica.Count > MaximoDiasActividad ||
                documento.MejorCalificacionReconocidaPorPractica.Count >
                    MaximoEstadosPorPractica ||
                documento.XPMejoraConcedidoPorPractica.Count >
                    MaximoEstadosPorPractica ||
                !EsFechaUtcValida(documento.UltimoInstanteUtcAceptado) ||
                !documento.MetadatosMigracion.MigracionInicialCompletada ||
                documento.MetadatosMigracion.VersionMigracion !=
                    VersionMigracionActual ||
                !documento.MetadatosMigracion.MigracionVersion2Completada ||
                !documento.MetadatosMigracion.FechaMigracionVersion2Utc.HasValue ||
                !EsFechaUtcValida(
                    documento.MetadatosMigracion.FechaMigracionVersion2Utc.Value) ||
                !documento.MetadatosMigracion.ActividadHistoricaProcesada &&
                    !documento.MetadatosMigracion.HistoriaActividadParcial ||
                !EsFechaUtcValida(
                    documento.MetadatosMigracion.FechaMigracionUtc) ||
                documento.MetadatosMigracion.MejorasHistoricasReconocidas < 0 ||
                documento.MetadatosMigracion.MejorasHistoricasOmitidas < 0 ||
                documento.MetadatosMigracion.UltimaReconciliacionUtc.HasValue &&
                (!EsFechaUtcValida(
                    documento.MetadatosMigracion.UltimaReconciliacionUtc.Value) ||
                 documento.MetadatosMigracion.UltimaReconciliacionUtc.Value >
                    documento.UltimoInstanteUtcAceptado)) {
                throw new InvalidDataException(
                    "El encabezado de motivacion.json no es válido.");
            }

            documento.ZonaHorariaEstudio = documento.ZonaHorariaEstudio.Trim();
            documento.MejorCalificacionReconocidaPorPractica =
                CopiarDiccionarioValidado(
                    documento.MejorCalificacionReconocidaPorPractica,
                    valor => valor is >= 0 and <= 100,
                    "mejor calificación");
            documento.XPMejoraConcedidoPorPractica =
                CopiarDiccionarioValidado(
                    documento.XPMejoraConcedidoPorPractica,
                    valor => valor is >= 0 and <= 25,
                    "XP de mejora");
            HashSet<string> claves = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> logros = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<int>> tramosMejoraPorPractica =
                new(StringComparer.OrdinalIgnoreCase);
            long total = 0;

            foreach (ConcesionXP concesion in documento.ConcesionesXP) {
                ValidarConcesion(concesion);

                if (!claves.Add(concesion.Clave)) {
                    throw new InvalidDataException(
                        "motivacion.json contiene claves de concesión duplicadas.");
                }

                if (concesion.FechaUtc > documento.UltimoInstanteUtcAceptado) {
                    throw new InvalidDataException(
                        "Una concesión tiene una fecha posterior al instante aceptado.");
                }

                total = checked(total + concesion.CantidadXP);

                if (concesion.Tipo == TipoConcesionXP.MejoraCalificacion) {
                    if (!tramosMejoraPorPractica.TryGetValue(
                            concesion.PracticaId!,
                            out List<int>? tramos)) {
                        tramos = new List<int>();
                        tramosMejoraPorPractica.Add(
                            concesion.PracticaId!,
                            tramos);
                    }

                    tramos.Add(IntentarLeerTramoMejora(concesion.Clave)!.Value);
                }
            }

            foreach ((string id, List<int> tramos) in
                tramosMejoraPorPractica) {
                int[] ordenados = tramos.OrderBy(item => item).ToArray();

                if (!ordenados.SequenceEqual(
                    Enumerable.Range(1, ordenados.Length))) {
                    throw new InvalidDataException(
                        "Los tramos de mejora no forman una secuencia contigua.");
                }

                if (!documento.XPMejoraConcedidoPorPractica.TryGetValue(
                        id,
                        out int registrada) ||
                    registrada != ordenados.Length) {
                    throw new InvalidDataException(
                        "El resumen de XP por mejora no coincide con sus concesiones.");
                }
            }

            foreach ((string id, int cantidad) in
                documento.XPMejoraConcedidoPorPractica) {
                int cantidadConcesiones = tramosMejoraPorPractica.TryGetValue(
                        id,
                        out List<int>? tramos)
                    ? tramos.Count
                    : 0;

                if (cantidad != cantidadConcesiones) {
                    throw new InvalidDataException(
                        "El resumen de XP por mejora contiene un total inconsistente.");
                }
            }

            DateTimeOffset limiteReconocimiento =
                documento.MetadatosMigracion.FechaMigracionVersion2Utc.Value >
                    documento.UltimoInstanteUtcAceptado
                    ? documento.MetadatosMigracion.FechaMigracionVersion2Utc.Value
                    : documento.UltimoInstanteUtcAceptado;

            foreach (LogroDesbloqueado logro in documento.LogrosDesbloqueados) {
                if (logro is null ||
                    !EsIdentificadorLogroValido(logro.LogroId) ||
                    !EsFechaUtcValida(logro.FechaReconocimientoUtc) ||
                    logro.FechaReconocimientoUtc > limiteReconocimiento ||
                    !logros.Add(logro.LogroId)) {
                    throw new InvalidDataException(
                        "motivacion.json contiene un logro invÃ¡lido o duplicado.");
                }
            }

            DateOnly? anterior = null;

            foreach (DateOnly dia in documento.DiasActividadAcademica) {
                if (dia == default ||
                    anterior.HasValue && dia.DayNumber <= anterior.Value.DayNumber) {
                    throw new InvalidDataException(
                        "Los dÃ­as de actividad deben ser vÃ¡lidos, Ãºnicos y ordenados.");
                }

                anterior = dia;
            }

            ValidarDiasNoFuturos(documento);

            _ = total;
            return true;
        } catch (Exception ex) when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            error = ex;
            return false;
        }
    }

    private static Dictionary<string, int> CopiarDiccionarioValidado(
        IEnumerable<KeyValuePair<string, int>> origen,
        Func<int, bool> validarValor,
        string descripcion) {
        Dictionary<string, int> copia = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string id, int valor) in origen) {
            if (!EsIdentificadorValido(id) ||
                !validarValor(valor) ||
                !copia.TryAdd(id.Trim(), valor)) {
                throw new InvalidDataException(
                    $"El estado de {descripcion} contiene datos inválidos o duplicados.");
            }
        }

        return copia;
    }

    private static void ValidarConcesion(ConcesionXP? concesion) {
        if (concesion is null ||
            string.IsNullOrWhiteSpace(concesion.Clave) ||
            concesion.Clave.Length > MaximoLongitudClave ||
            concesion.CantidadXP <= 0 ||
            !Enum.IsDefined(concesion.Tipo) ||
            !EsFechaUtcValida(concesion.FechaUtc)) {
            throw new InvalidDataException(
                "motivacion.json contiene una concesión inválida.");
        }

        concesion.Clave = concesion.Clave.Trim();
        concesion.PracticaId = NormalizarOpcional(concesion.PracticaId);
        concesion.TemaId = NormalizarOpcional(concesion.TemaId);
        concesion.GradoId = NormalizarOpcional(concesion.GradoId);
        bool idsValidos = concesion.Tipo switch {
            TipoConcesionXP.PracticaVinculada or
            TipoConcesionXP.PracticaRealizada or
            TipoConcesionXP.EvaluacionAprobada or
            TipoConcesionXP.MejoraCalificacion or
            TipoConcesionXP.EvaluacionPerfecta =>
                EsIdentificadorValido(concesion.PracticaId) &&
                concesion.TemaId is null &&
                concesion.GradoId is null,
            TipoConcesionXP.TemaCompletado =>
                concesion.PracticaId is null &&
                EsIdentificadorValido(concesion.TemaId) &&
                EsIdentificadorValido(concesion.GradoId),
            TipoConcesionXP.GradoCompletado =>
                concesion.PracticaId is null &&
                concesion.TemaId is null &&
                EsIdentificadorValido(concesion.GradoId),
            _ => false
        };

        if (!idsValidos) {
            throw new InvalidDataException(
                "Una concesión contiene identificadores que no corresponden a su tipo.");
        }

        int? tramo = concesion.Tipo == TipoConcesionXP.MejoraCalificacion
            ? IntentarLeerTramoMejora(concesion.Clave)
            : null;
        string claveEsperada = CrearClave(
            concesion.Tipo,
            concesion.PracticaId,
            concesion.TemaId,
            concesion.GradoId,
            tramo);
        int cantidadEsperada = concesion.Tipo switch {
            TipoConcesionXP.PracticaVinculada => 10,
            TipoConcesionXP.PracticaRealizada => 25,
            TipoConcesionXP.EvaluacionAprobada => 40,
            TipoConcesionXP.MejoraCalificacion => 1,
            TipoConcesionXP.EvaluacionPerfecta => 25,
            TipoConcesionXP.TemaCompletado => 75,
            TipoConcesionXP.GradoCompletado => 200,
            _ => throw new InvalidDataException("Tipo de concesión desconocido.")
        };

        if (concesion.CantidadXP != cantidadEsperada ||
            !concesion.Clave.Equals(
                claveEsperada,
                StringComparison.OrdinalIgnoreCase)) {
            throw new InvalidDataException(
                "Una concesión no coincide con su clave o cantidad esperada.");
        }
    }

    private static int? IntentarLeerTramoMejora(string clave) {
        int separador = clave.LastIndexOf(':');

        if (separador < 0 ||
            !int.TryParse(clave[(separador + 1)..], out int tramo) ||
            tramo is < 1 or > 25) {
            throw new InvalidDataException(
                "La concesión de mejora no contiene un tramo válido.");
        }

        return tramo;
    }

    private static string CrearClave(
        TipoConcesionXP tipo,
        string? practicaId,
        string? temaId,
        string? gradoId,
        int? tramoMejora) {
        string practica = NormalizarId(practicaId);
        string tema = NormalizarId(temaId);
        string grado = NormalizarId(gradoId);

        return tipo switch {
            TipoConcesionXP.PracticaVinculada when EsIdentificadorValido(practica) =>
                $"practica:{practica}:vinculada",
            TipoConcesionXP.PracticaRealizada when EsIdentificadorValido(practica) =>
                $"practica:{practica}:realizada",
            TipoConcesionXP.EvaluacionAprobada when EsIdentificadorValido(practica) =>
                $"practica:{practica}:aprobada",
            TipoConcesionXP.EvaluacionPerfecta when EsIdentificadorValido(practica) =>
                $"practica:{practica}:perfecta",
            TipoConcesionXP.MejoraCalificacion
                when EsIdentificadorValido(practica) &&
                    tramoMejora is >= 1 and <= 25 =>
                $"practica:{practica}:mejora:{tramoMejora.Value:D2}",
            TipoConcesionXP.TemaCompletado
                when EsIdentificadorValido(grado) &&
                    EsIdentificadorValido(tema) =>
                $"tema:{grado}:{tema}:completado",
            TipoConcesionXP.GradoCompletado when EsIdentificadorValido(grado) =>
                $"grado:{grado}:completado",
            _ => throw new InvalidDataException(
                "No se puede construir una clave motivacional válida.")
        };
    }

    private static string NormalizarId(string? identificador) {
        return identificador?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static string? NormalizarOpcional(string? identificador) {
        return string.IsNullOrWhiteSpace(identificador)
            ? null
            : identificador.Trim().ToLowerInvariant();
    }

    private static bool EsIdentificadorValido(string? identificador) {
        return !string.IsNullOrWhiteSpace(identificador) &&
            identificador.Length <= MaximoLongitudIdentificador &&
            !identificador.Any(char.IsControl);
    }

    private static bool EsFechaUtcValida(DateTimeOffset fecha) {
        return fecha != default && fecha.Offset == TimeSpan.Zero;
    }

    private static long CalcularXpTotal(DocumentoMotivacion documento) {
        long total = 0;

        foreach (ConcesionXP concesion in documento.ConcesionesXP) {
            total = checked(total + concesion.CantidadXP);
        }

        return total;
    }

    private static bool IntentarLeerVersion(JsonElement raiz, out int version) {
        version = 0;

        if (raiz.ValueKind != JsonValueKind.Object) {
            return false;
        }

        foreach (JsonProperty propiedad in raiz.EnumerateObject()) {
            if (propiedad.Name.Equals(
                    nameof(DocumentoMotivacion.Version),
                    StringComparison.OrdinalIgnoreCase)) {
                return propiedad.Value.ValueKind == JsonValueKind.Number &&
                    propiedad.Value.TryGetInt32(out version) &&
                    version > 0;
            }
        }

        return false;
    }

    private static bool TienePropiedadesDuplicadas(JsonElement elemento) {
        if (elemento.ValueKind == JsonValueKind.Array) {
            return elemento.EnumerateArray().Any(TienePropiedadesDuplicadas);
        }

        if (elemento.ValueKind != JsonValueKind.Object) {
            return false;
        }

        HashSet<string> nombres = new(StringComparer.OrdinalIgnoreCase);

        foreach (JsonProperty propiedad in elemento.EnumerateObject()) {
            if (!nombres.Add(propiedad.Name) ||
                TienePropiedadesDuplicadas(propiedad.Value)) {
                return true;
            }
        }

        return false;
    }

    private static JsonSerializerOptions CrearOpcionesJson() {
        JsonSerializerOptions opciones = new() {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        opciones.Converters.Add(new JsonStringEnumConverter());
        return opciones;
    }

    private static string CrearNombreMutex(string ruta) {
        string normalizada = Path.GetFullPath(ruta).ToUpperInvariant();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizada));
        return $@"Global\EndForge.Motivacion.{Convert.ToHexString(hash)}";
    }

    private static DependenciasPredeterminadas CrearDependencias(
        string carpetaDatos) {
        ProgresoCursoService progreso = new(carpetaDatos);
        HistorialEvaluacionesService historial = new(carpetaDatos);
        GradosService grados = new(new CursoService());
        return new DependenciasPredeterminadas(
            () => grados.CargarGrados(null),
            progreso.CargarProgreso,
            historial.CargarHistorial);
    }

    private sealed class ContextoMutacion {
        public bool HuboCambio { get; set; }

        public bool FuenteNoDisponible { get; set; }

        public bool HuboConcesionYaExistente { get; set; }

        public bool ReportarSoloConcesionesOperacionActual { get; set; }

        public bool HuboCambioVersion2OperacionActual { get; set; }

        public bool OperacionAcademicaActualConfirmada { get; set; }

        public DateTimeOffset? InstanteConcesionUtc { get; set; }

        public DateTimeOffset? InstanteReconocimientoLogrosUtc { get; set; }

        public CatalogoAprendizajeSnapshot? CatalogoOperacionActual { get; set; }

        public List<string> ClavesProcesadas { get; } = new();

        public HashSet<string> ClavesHitosOperacionActual { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> ClavesCreadasEnOperacion { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> ClavesConcedidasOperacionActual { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public HashSet<AdvertenciaMotivacion> Advertencias { get; } = new();

        public List<LogroDesbloqueado> LogrosNuevos { get; } = new();

        public Exception? ErrorFuente { get; set; }
    }

    private sealed record ResultadoFuentes(
        CatalogoAprendizajeSnapshot? Catalogo,
        IReadOnlyDictionary<string, ProgresoPractica> Progreso,
        IReadOnlyDictionary<string, HistorialPractica> Historial,
        bool ProgresoDisponible,
        bool HistorialDisponible,
        bool ProgresoParcial,
        bool HistorialParcial,
        Exception? Error) {
        public bool DatosParciales => ProgresoParcial || HistorialParcial;

        public bool Disponibles =>
            Catalogo is not null && ProgresoDisponible && HistorialDisponible;
    }

    private sealed record HechosLogros(
        IReadOnlySet<string> PracticasVinculadas,
        IReadOnlySet<string> PracticasRealizadas,
        IReadOnlySet<string> PracticasAprobadas,
        IReadOnlySet<string> PracticasPerfectas,
        IReadOnlySet<string> TemasCompletados,
        IReadOnlySet<string> GradosCompletados);

    private sealed record ResultadoCreacionDocumento(
        DocumentoMotivacion? Documento,
        EstadoProcesamientoMotivacion Estado,
        Exception? Error);

    private sealed record ResultadoCargaDocumento(
        EstadoCargaDocumento Estado,
        DocumentoMotivacion? Documento,
        Exception? Error,
        DocumentoMotivacionVersion1? DocumentoVersion1 = null);

    private sealed record DependenciasPredeterminadas(
        Func<IReadOnlyList<GradoCurso>> CargarCatalogo,
        Func<ResultadoCargaProgreso> CargarProgreso,
        Func<ResultadoCargaHistorialEvaluaciones> CargarHistorial);

    private sealed record EvidenciaOperacion(
        ProgresoCurso? Progreso,
        HistorialPractica? Historial,
        IntentoPractica? Intento,
        bool VinculoPersistidoAhora,
        bool RealizadaPersistidaAhora,
        TransicionProgresoPersistida? TransicionProgreso = null,
        TransicionEvaluacionPersistida? TransicionEvaluacion = null);

    private enum TipoOperacionMotivacion {
        VinculoPractica,
        PracticaRealizada,
        ProgresoPersistido,
        EvaluacionPersistida,
        Reconciliacion
    }

    private enum EstadoAplicacionOperacion {
        Aplicada,
        YaAplicada,
        SinRecompensa
    }

    private enum EstadoCargaDocumento {
        Exitosa,
        RequiereMigracion,
        ArchivoInexistente,
        ContenidoInvalido,
        VersionIncompatible,
        PermisosInsuficientes,
        ErrorIo
    }
}

internal interface ISistemaArchivosMotivacion {
    bool ArchivoExiste(string ruta);

    long ObtenerLongitud(string ruta);

    string LeerTodoTexto(string ruta);

    void CrearDirectorio(string ruta);

    void EscribirTodoTextoDurable(string ruta, string contenido);

    void Reemplazar(string origen, string destino);

    void Mover(string origen, string destino);

    void Eliminar(string ruta);

    IEnumerable<string> EnumerarArchivos(string carpeta, string patron);

    DateTime ObtenerUltimaEscrituraUtc(string ruta);
}

internal sealed class SistemaArchivosMotivacion : ISistemaArchivosMotivacion {
    public bool ArchivoExiste(string ruta) => File.Exists(ruta);

    public long ObtenerLongitud(string ruta) => new FileInfo(ruta).Length;

    public string LeerTodoTexto(string ruta) {
        using FileStream archivo = new(
            ruta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        using StreamReader lector = new(
            archivo,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        return lector.ReadToEnd();
    }

    public void CrearDirectorio(string ruta) => Directory.CreateDirectory(ruta);

    public void EscribirTodoTextoDurable(string ruta, string contenido) {
        using FileStream archivo = new(
            ruta,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4_096,
            FileOptions.WriteThrough);
        using StreamWriter escritor = new(
            archivo,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            bufferSize: 4_096,
            leaveOpen: true);
        escritor.Write(contenido);
        escritor.Flush();
        archivo.Flush(flushToDisk: true);
    }

    public void Reemplazar(string origen, string destino) {
        File.Replace(origen, destino, null);
    }

    public void Mover(string origen, string destino) {
        File.Move(origen, destino);
    }

    public void Eliminar(string ruta) => File.Delete(ruta);

    public IEnumerable<string> EnumerarArchivos(string carpeta, string patron) {
        return Directory.Exists(carpeta)
            ? Directory.EnumerateFiles(carpeta, patron, SearchOption.TopDirectoryOnly)
            : Array.Empty<string>();
    }

    public DateTime ObtenerUltimaEscrituraUtc(string ruta) {
        return File.GetLastWriteTimeUtc(ruta);
    }
}
