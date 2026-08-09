using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class MotivacionLogrosV2Tests {
    private static readonly DateTimeOffset FechaBase =
        new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly string[] TodosLosLogros = {
        CatalogoLogrosService.PrimeraPracticaVinculadaId,
        CatalogoLogrosService.PrimeraPracticaRealizadaId,
        CatalogoLogrosService.PrimeraEvaluacionAprobadaId,
        CatalogoLogrosService.PrimeraEvaluacionPerfectaId,
        CatalogoLogrosService.PrimerTemaCompletadoId,
        CatalogoLogrosService.PrimerGradoCompletadoId,
        CatalogoLogrosService.CincoPracticasRealizadasId,
        CatalogoLogrosService.DiezPracticasRealizadasId,
        CatalogoLogrosService.VeinticincoPracticasRealizadasId,
        CatalogoLogrosService.GradoFundamentosCompletoId,
        CatalogoLogrosService.GradoJuniorCompletoId,
        CatalogoLogrosService.CincoPracticasAprobadasId,
        CatalogoLogrosService.DiezPracticasAprobadasId,
        CatalogoLogrosService.CincoPracticasPerfectasId
    };

    [Fact]
    public void ReconciliacionCompleta_ImportaLosCatorceLogrosSinNotificarlos() {
        using EntornoLogros entorno = new(CrearCatalogo(15, 15));
        entorno.EstablecerProgresoRealizado(entorno.Practicas.Count);
        entorno.EstablecerHistorial(entorno.Practicas.Count, 100);

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Empty(resultado.LogrosNuevos);
        Assert.Equal(
            TodosLosLogros.OrderBy(id => id, StringComparer.OrdinalIgnoreCase),
            resultado.Resumen.LogrosDesbloqueados
                .Select(logro => logro.LogroId)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));
        Assert.All(resultado.Resumen.LogrosDesbloqueados, logro =>
            Assert.True(logro.EsImportado));
    }

    [Theory]
    [InlineData(4, CatalogoLogrosService.CincoPracticasRealizadasId)]
    [InlineData(9, CatalogoLogrosService.DiezPracticasRealizadasId)]
    [InlineData(24, CatalogoLogrosService.VeinticincoPracticasRealizadasId)]
    public void PracticasRealizadas_CruzarFronteraDesbloqueaLogroActual(
        int realizadasAnteriores,
        string logroEsperado) {
        using EntornoLogros entorno = new(CrearCatalogo(15, 15));
        entorno.EstablecerProgresoRealizado(realizadasAnteriores);
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso progreso = entorno.CrearProgresoRealizado(
            realizadasAnteriores + 1,
            actualDesde: realizadasAnteriores);
        ProgresoPractica actual = progreso.Practicas[realizadasAnteriores];
        entorno.Progreso = CrearCargaProgreso(progreso);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(
                actual.PracticaId,
                progreso,
                CrearTransicionProgreso(actual));

        LogroDesbloqueado logro = Assert.Single(
            resultado.LogrosNuevos,
            item => item.LogroId == logroEsperado);
        Assert.False(logro.EsImportado);
    }

    [Fact]
    public void QuintaRealizada_UsaDeltaAcademicoSinContarSuConcesionActual() {
        using EntornoLogros entorno = new(CrearCatalogo(15, 0));
        entorno.EstablecerProgresoRealizado(4);
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso progreso = entorno.CrearProgresoRealizado(
            5,
            actualDesde: 4);
        ProgresoPractica actual = progreso.Practicas[4];
        entorno.Progreso = CrearCargaProgreso(progreso);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(
                actual.PracticaId,
                progreso,
                CrearTransicionProgreso(actual));

        Assert.Equal(35, resultado.XpConcedido);
        Assert.Equal(175, resultado.XpTotalResultante);
        LogroDesbloqueado logro = Assert.Single(
            resultado.LogrosNuevos,
            item => item.LogroId ==
                CatalogoLogrosService.CincoPracticasRealizadasId);
        Assert.False(logro.EsImportado);
    }

    [Theory]
    [InlineData(4, CatalogoLogrosService.CincoPracticasAprobadasId)]
    [InlineData(9, CatalogoLogrosService.DiezPracticasAprobadasId)]
    public void PracticasAprobadas_CruzarFronteraDesbloqueaLogroActual(
        int aprobadasAnteriores,
        string logroEsperado) {
        using EntornoLogros entorno = new(CrearCatalogo(15, 15));
        entorno.EstablecerHistorial(aprobadasAnteriores, 80);
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        string practicaId = entorno.Practicas[aprobadasAnteriores].Id;
        HistorialPractica historial = CrearHistorial(practicaId, 80, 100);
        entorno.AgregarHistorial(historial);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                practicaId,
                historial,
                CrearTransicionEvaluacion(historial));

        LogroDesbloqueado logro = Assert.Single(
            resultado.LogrosNuevos,
            item => item.LogroId == logroEsperado);
        Assert.False(logro.EsImportado);
    }

    [Fact]
    public void PracticasPerfectas_CuatroACinco_DesbloqueaLogrosCorrespondientes() {
        using EntornoLogros entorno = new(CrearCatalogo(15, 15));
        entorno.EstablecerHistorial(4, 100);
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        string practicaId = entorno.Practicas[4].Id;
        HistorialPractica historial = CrearHistorial(practicaId, 100, 100);
        entorno.AgregarHistorial(historial);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                practicaId,
                historial,
                CrearTransicionEvaluacion(historial));

        Assert.Contains(resultado.LogrosNuevos, logro =>
            logro.LogroId == CatalogoLogrosService.CincoPracticasPerfectasId &&
            !logro.EsImportado);
        Assert.Contains(resultado.LogrosNuevos, logro =>
            logro.LogroId == CatalogoLogrosService.CincoPracticasAprobadasId &&
            !logro.EsImportado);
    }

    [Fact]
    public void LogroHistoricoYLogroActual_ConservanAtribucionDiferente() {
        using EntornoLogros historico = new(CrearCatalogo(1, 0));
        historico.EstablecerProgresoRealizado(1);

        ResultadoProcesamientoMotivacion importado = historico
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Empty(importado.LogrosNuevos);
        Assert.True(ObtenerLogro(
            importado.Resumen,
            CatalogoLogrosService.PrimeraPracticaRealizadaId).EsImportado);

        using EntornoLogros actual = new(CrearCatalogo(1, 0));
        MotivacionService servicioActual = actual.CrearServicio();
        servicioActual.ReconciliarEstadoActual();
        ProgresoCurso progreso = actual.CrearProgresoRealizado(1, actualDesde: 0);
        actual.Progreso = CrearCargaProgreso(progreso);

        ResultadoProcesamientoMotivacion nuevo =
            servicioActual.ProcesarProgresoPersistido(
                actual.Practicas[0].Id,
                progreso,
                CrearTransicionProgreso(progreso.Practicas[0]));

        Assert.False(ObtenerLogro(
            nuevo.Resumen,
            CatalogoLogrosService.PrimeraPracticaRealizadaId).EsImportado);
        Assert.Contains(nuevo.LogrosNuevos, logro =>
            logro.LogroId ==
                CatalogoLogrosService.PrimeraPracticaRealizadaId);
    }

    [Fact]
    public void PracticaUnicaRealizada_DesbloqueaVariosLogrosEnUnaOperacion() {
        using EntornoLogros entorno = new(CrearCatalogo(1, 0));
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso progreso = entorno.CrearProgresoRealizado(1, actualDesde: 0);
        entorno.Progreso = CrearCargaProgreso(progreso);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(
                entorno.Practicas[0].Id,
                progreso,
                CrearTransicionProgreso(progreso.Practicas[0]));

        Assert.Equal(
            new[] {
                CatalogoLogrosService.GradoFundamentosCompletoId,
                CatalogoLogrosService.PrimerGradoCompletadoId,
                CatalogoLogrosService.PrimerTemaCompletadoId,
                CatalogoLogrosService.PrimeraPracticaRealizadaId,
                CatalogoLogrosService.PrimeraPracticaVinculadaId
            }.OrderBy(id => id, StringComparer.OrdinalIgnoreCase),
            resultado.LogrosNuevos
                .Select(logro => logro.LogroId)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));
        Assert.All(resultado.LogrosNuevos, logro =>
            Assert.False(logro.EsImportado));
        Assert.Equal(310, resultado.XpConcedido);
    }

    [Fact]
    public void MismaTransicionRepetida_NoDuplicaNiVuelveANotificarLogros() {
        using EntornoLogros entorno = new(CrearCatalogo(1, 0));
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso progreso = entorno.CrearProgresoRealizado(1, actualDesde: 0);
        TransicionProgresoPersistida transicion =
            CrearTransicionProgreso(progreso.Practicas[0]);
        entorno.Progreso = CrearCargaProgreso(progreso);

        ResultadoProcesamientoMotivacion primera =
            servicio.ProcesarProgresoPersistido(
                entorno.Practicas[0].Id,
                progreso,
                transicion);
        ResultadoProcesamientoMotivacion repetida =
            servicio.ProcesarProgresoPersistido(
                entorno.Practicas[0].Id,
                progreso,
                transicion);

        Assert.NotEmpty(primera.LogrosNuevos);
        Assert.Empty(repetida.LogrosNuevos);
        Assert.Equal(
            primera.Resumen.LogrosDesbloqueados.Count,
            repetida.Resumen.LogrosDesbloqueados.Count);
        Assert.Equal(
            repetida.Resumen.LogrosDesbloqueados.Count,
            repetida.Resumen.LogrosDesbloqueados
                .Select(logro => logro.LogroId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
    }

    [Fact]
    public void APIsLegadas_DescubrimientosHistoricosNoSeNotificanComoNuevos() {
        using EntornoLogros vinculo = new(CrearCatalogo(1, 0));
        MotivacionService servicioVinculo = vinculo.CrearServicio();
        servicioVinculo.ReconciliarEstadoActual();
        vinculo.EstablecerProgresoVinculado(1);
        ResultadoProcesamientoMotivacion resultadoVinculo =
            servicioVinculo.ProcesarVinculoPractica(vinculo.Practicas[0].Id);

        using EntornoLogros realizada = new(CrearCatalogo(1, 0));
        MotivacionService servicioRealizada = realizada.CrearServicio();
        servicioRealizada.ReconciliarEstadoActual();
        realizada.EstablecerProgresoRealizado(1);
        ResultadoProcesamientoMotivacion resultadoRealizada =
            servicioRealizada.ProcesarPracticaRealizada(realizada.Practicas[0].Id);

        using EntornoLogros snapshot = new(CrearCatalogo(1, 0));
        MotivacionService servicioSnapshot = snapshot.CrearServicio();
        servicioSnapshot.ReconciliarEstadoActual();
        ProgresoCurso progreso = snapshot.CrearProgresoRealizado(1);
        snapshot.Progreso = CrearCargaProgreso(progreso);
        ResultadoProcesamientoMotivacion resultadoSnapshot =
            servicioSnapshot.ProcesarProgresoPersistido(
                snapshot.Practicas[0].Id,
                progreso);

        using EntornoLogros evaluacion = new(CrearCatalogo(1, 0));
        MotivacionService servicioEvaluacion = evaluacion.CrearServicio();
        servicioEvaluacion.ReconciliarEstadoActual();
        evaluacion.EstablecerHistorial(1, 100);
        ResultadoProcesamientoMotivacion resultadoEvaluacion =
            servicioEvaluacion.ProcesarEvaluacionPersistida(
                evaluacion.Practicas[0].Id);

        AssertDescubrimientoImportado(resultadoVinculo);
        AssertDescubrimientoImportado(resultadoRealizada);
        AssertDescubrimientoImportado(resultadoSnapshot);
        AssertDescubrimientoImportado(resultadoEvaluacion);
    }

    [Fact]
    public void BackfillHistoricoDuranteEvaluacion_NoApareceEnLogrosNuevos() {
        using EntornoLogros entorno = new(CrearCatalogo(1, 0));
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        string practicaId = entorno.Practicas[0].Id;
        IntentoPractica anterior = CrearIntento(
            practicaId,
            "intento-anterior",
            80,
            FechaBase.AddMinutes(-1));
        IntentoPractica actual = CrearIntento(
            practicaId,
            "intento-actual",
            50,
            FechaBase);
        HistorialPractica historial = CrearHistorial(
            practicaId,
            anterior,
            actual);
        TransicionEvaluacionPersistida transicion = new() {
            PracticaId = practicaId,
            IntentoId = actual.Id,
            FechaIntento = actual.Fecha,
            CalificacionIntento = actual.Calificacion,
            MejorCalificacionAnterior = anterior.Calificacion,
            UltimaCalificacionAnterior = anterior.Calificacion,
            FechaUltimoIntentoAnterior = anterior.Fecha,
            MejorCalificacionPosterior = anterior.Calificacion,
            TotalIntentos = 2,
            IntentoPublicado = true
        };
        entorno.Historial = CrearCargaHistorial(historial);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                practicaId,
                historial,
                transicion);

        Assert.DoesNotContain(resultado.LogrosNuevos, logro =>
            logro.LogroId ==
                CatalogoLogrosService.PrimeraEvaluacionAprobadaId);
        Assert.True(ObtenerLogro(
            resultado.Resumen,
            CatalogoLogrosService.PrimeraEvaluacionAprobadaId).EsImportado);
    }

    [Fact]
    public async Task DosInstancias_MismoLogro_SePersisteYNotificaUnaSolaVez() {
        using EntornoLogros entorno = new(CrearCatalogo(1, 0));
        entorno.CrearServicio().ReconciliarEstadoActual();
        ProgresoCurso progreso = entorno.CrearProgresoRealizado(1, actualDesde: 0);
        TransicionProgresoPersistida transicion =
            CrearTransicionProgreso(progreso.Practicas[0]);
        entorno.Progreso = CrearCargaProgreso(progreso);
        MotivacionService primera = entorno.CrearServicio();
        MotivacionService segunda = entorno.CrearServicio();
        using ManualResetEventSlim inicio = new(false);

        Task<ResultadoProcesamientoMotivacion> tareaUno = Task.Run(() => {
            inicio.Wait();
            return primera.ProcesarProgresoPersistido(
                entorno.Practicas[0].Id,
                progreso,
                transicion);
        });
        Task<ResultadoProcesamientoMotivacion> tareaDos = Task.Run(() => {
            inicio.Wait();
            return segunda.ProcesarProgresoPersistido(
                entorno.Practicas[0].Id,
                progreso,
                transicion);
        });
        inicio.Set();
        ResultadoProcesamientoMotivacion[] resultados =
            await Task.WhenAll(tareaUno, tareaDos);

        Assert.Equal(1, resultados
            .SelectMany(resultado => resultado.LogrosNuevos)
            .Count(logro => logro.LogroId ==
                CatalogoLogrosService.PrimeraPracticaRealizadaId));
        ResumenMotivacion resumen = entorno
            .CrearServicio()
            .ReconciliarEstadoActual()
            .Resumen;
        Assert.Equal(1, resumen.LogrosDesbloqueados.Count(logro =>
            logro.LogroId ==
                CatalogoLogrosService.PrimeraPracticaRealizadaId));
    }

    [Fact]
    public async Task DosInstancias_LogrosDiferentes_ConservaAmbos() {
        using EntornoLogros entorno = new(CrearCatalogo(2, 0));
        entorno.CrearServicio().ReconciliarEstadoActual();
        ProgresoCurso progreso = entorno.CrearProgresoRealizado(1, actualDesde: 0);
        entorno.Progreso = CrearCargaProgreso(progreso);
        string evaluadaId = entorno.Practicas[1].Id;
        HistorialPractica historial = CrearHistorial(evaluadaId, 80, 100);
        entorno.Historial = CrearCargaHistorial(historial);
        MotivacionService primera = entorno.CrearServicio();
        MotivacionService segunda = entorno.CrearServicio();
        using ManualResetEventSlim inicio = new(false);

        Task<ResultadoProcesamientoMotivacion> progresoTask = Task.Run(() => {
            inicio.Wait();
            return primera.ProcesarProgresoPersistido(
                entorno.Practicas[0].Id,
                progreso,
                CrearTransicionProgreso(progreso.Practicas[0]));
        });
        Task<ResultadoProcesamientoMotivacion> evaluacionTask = Task.Run(() => {
            inicio.Wait();
            return segunda.ProcesarEvaluacionPersistida(
                evaluadaId,
                historial,
                CrearTransicionEvaluacion(historial));
        });
        inicio.Set();
        ResultadoProcesamientoMotivacion[] resultados =
            await Task.WhenAll(progresoTask, evaluacionTask);

        string[] nuevos = resultados
            .SelectMany(resultado => resultado.LogrosNuevos)
            .Select(logro => logro.LogroId)
            .ToArray();
        Assert.Contains(
            CatalogoLogrosService.PrimeraPracticaRealizadaId,
            nuevos);
        Assert.Contains(
            CatalogoLogrosService.PrimeraEvaluacionAprobadaId,
            nuevos);
        ResumenMotivacion resumen = entorno
            .CrearServicio()
            .ReconciliarEstadoActual()
            .Resumen;
        Assert.Equal(1, resumen.LogrosDesbloqueados.Count(logro =>
            logro.LogroId ==
                CatalogoLogrosService.PrimeraPracticaRealizadaId));
        Assert.Equal(1, resumen.LogrosDesbloqueados.Count(logro =>
            logro.LogroId ==
                CatalogoLogrosService.PrimeraEvaluacionAprobadaId));
    }

    private static void AssertDescubrimientoImportado(
        ResultadoProcesamientoMotivacion resultado) {
        Assert.Empty(resultado.LogrosNuevos);
        Assert.All(resultado.Resumen.LogrosDesbloqueados, logro =>
            Assert.True(logro.EsImportado));
    }

    private static LogroDesbloqueado ObtenerLogro(
        ResumenMotivacion resumen,
        string logroId) {
        return Assert.Single(resumen.LogrosDesbloqueados, logro =>
            logro.LogroId.Equals(logroId, StringComparison.OrdinalIgnoreCase));
    }

    private static TransicionProgresoPersistida CrearTransicionProgreso(
        ProgresoPractica actual) {
        return new TransicionProgresoPersistida {
            PracticaId = actual.PracticaId,
            ProgresoAnterior = null,
            ProgresoFinal = actual,
            PracticaCreada = true,
            VinculoPersistidoAhora = !string.IsNullOrWhiteSpace(
                actual.RutaProyecto),
            RealizadaPersistidaAhora =
                actual.Estado == EstadoPracticaCurso.Realizada
        };
    }

    private static TransicionEvaluacionPersistida CrearTransicionEvaluacion(
        HistorialPractica historial) {
        IntentoPractica intento = Assert.Single(historial.Intentos);
        return new TransicionEvaluacionPersistida {
            PracticaId = historial.PracticaId,
            IntentoId = intento.Id,
            FechaIntento = intento.Fecha,
            CalificacionIntento = intento.Calificacion,
            MejorCalificacionPosterior = intento.Calificacion,
            TotalIntentos = 1,
            IntentoPublicado = true
        };
    }

    private static ResultadoCargaProgreso CrearCargaProgreso(
        ProgresoCurso progreso) {
        return new ResultadoCargaProgreso {
            Estado = EstadoCargaProgreso.Exitosa,
            Progreso = progreso
        };
    }

    private static ResultadoCargaHistorialEvaluaciones CrearCargaHistorial(
        params HistorialPractica[] historiales) {
        return new ResultadoCargaHistorialEvaluaciones {
            Estado = EstadoCargaHistorialEvaluaciones.Exitosa,
            Historial = new HistorialEvaluaciones {
                Practicas = historiales
            }
        };
    }

    private static HistorialPractica CrearHistorial(
        string practicaId,
        int calificacion,
        int indice) {
        IntentoPractica intento = CrearIntento(
            practicaId,
            $"intento-{practicaId}-{indice}",
            calificacion,
            FechaBase.AddMinutes(-indice));
        return CrearHistorial(practicaId, intento);
    }

    private static HistorialPractica CrearHistorial(
        string practicaId,
        params IntentoPractica[] intentos) {
        IntentoPractica ultimo = intentos[^1];
        return new HistorialPractica {
            PracticaId = practicaId,
            TotalIntentos = intentos.Length,
            MejorCalificacion = intentos.Max(item => item.Calificacion),
            UltimaCalificacion = ultimo.Calificacion,
            FechaUltimoIntento = ultimo.Fecha,
            Intentos = intentos
        };
    }

    private static IntentoPractica CrearIntento(
        string practicaId,
        string intentoId,
        int calificacion,
        DateTimeOffset fecha) {
        return new IntentoPractica {
            Id = intentoId,
            PracticaId = practicaId,
            Fecha = fecha,
            Calificacion = calificacion,
            ResultadoGeneral = "Resultado",
            PuntosMaximos = 100,
            RutaProyecto = $@"C:\Practicas\{practicaId}"
        };
    }

    private static IReadOnlyList<GradoCurso> CrearCatalogo(
        int practicasGradoUno,
        int practicasGradoDos) {
        List<GradoCurso> grados = new();

        if (practicasGradoUno > 0) {
            grados.Add(CrearGrado(
                GradosService.GradoFundamentosId,
                1,
                practicasGradoUno));
        }

        if (practicasGradoDos > 0) {
            grados.Add(CrearGrado(
                GradosService.GradoJuniorId,
                2,
                practicasGradoDos));
        }

        return grados;
    }

    private static GradoCurso CrearGrado(
        string gradoId,
        int numero,
        int cantidadPracticas) {
        string temaId = $"tema-{gradoId}";
        PracticaCurso[] practicas = Enumerable
            .Range(1, cantidadPracticas)
            .Select(indice => new PracticaCurso {
                Id = $"{gradoId}-p{indice:D2}",
                TemaId = temaId,
                Numero = indice,
                Nombre = $"Practica {indice}"
            })
            .ToArray();
        TemaCurso tema = new() {
            Id = temaId,
            Numero = 1,
            Nombre = "Tema",
            Practicas = practicas
        };
        return new GradoCurso {
            Id = gradoId,
            Numero = numero,
            Nombre = $"Grado {numero}",
            EsContenidoDisponible = true,
            Temas = new[] { tema }
        };
    }

    private sealed class EntornoLogros : IDisposable {
        private readonly TimeProviderFijo reloj = new(FechaBase);

        public EntornoLogros(IReadOnlyList<GradoCurso> catalogo) {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-LogrosV2-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
            Catalogo = catalogo;
            Practicas = catalogo
                .SelectMany(grado => grado.Temas)
                .SelectMany(tema => tema.Practicas)
                .ToArray();
        }

        public string Carpeta { get; }

        public IReadOnlyList<GradoCurso> Catalogo { get; }

        public IReadOnlyList<PracticaCurso> Practicas { get; }

        public ResultadoCargaProgreso Progreso { get; set; } =
            CrearCargaProgreso(new ProgresoCurso());

        public ResultadoCargaHistorialEvaluaciones Historial { get; set; } =
            CrearCargaHistorial();

        public MotivacionService CrearServicio() {
            return new MotivacionService(
                Carpeta,
                reloj,
                () => Catalogo,
                () => Progreso,
                () => Historial);
        }

        public void EstablecerProgresoRealizado(int cantidad) {
            Progreso = CrearCargaProgreso(CrearProgresoRealizado(cantidad));
        }

        public void EstablecerProgresoVinculado(int cantidad) {
            ProgresoCurso progreso = new() {
                Practicas = Practicas
                    .Take(cantidad)
                    .Select((practica, indice) => CrearProgresoPractica(
                        practica.Id,
                        EstadoPracticaCurso.EnProgreso,
                        indice,
                        esActual: false))
                    .ToList()
            };
            Progreso = CrearCargaProgreso(progreso);
        }

        public ProgresoCurso CrearProgresoRealizado(
            int cantidad,
            int actualDesde = int.MaxValue) {
            return new ProgresoCurso {
                Practicas = Practicas
                    .Take(cantidad)
                    .Select((practica, indice) => CrearProgresoPractica(
                        practica.Id,
                        EstadoPracticaCurso.Realizada,
                        indice,
                        esActual: indice >= actualDesde))
                    .ToList()
            };
        }

        public void EstablecerHistorial(int cantidad, int calificacion) {
            Historial = CrearCargaHistorial(Practicas
                .Take(cantidad)
                .Select((practica, indice) => CrearHistorial(
                    practica.Id,
                    calificacion,
                    indice + 1))
                .ToArray());
        }

        public void AgregarHistorial(HistorialPractica historial) {
            Historial = CrearCargaHistorial(Historial.Historial.Practicas
                .Where(item => !item.PracticaId.Equals(
                    historial.PracticaId,
                    StringComparison.OrdinalIgnoreCase))
                .Append(historial)
                .ToArray());
        }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }

        private static ProgresoPractica CrearProgresoPractica(
            string practicaId,
            EstadoPracticaCurso estado,
            int indice,
            bool esActual) {
            DateTimeOffset fecha = esActual
                ? FechaBase
                : FechaBase.AddDays(-2).AddMinutes(-indice);
            return new ProgresoPractica {
                PracticaId = practicaId,
                Estado = estado,
                RutaProyecto = $@"C:\Practicas\{practicaId}",
                FechaCreacion = fecha,
                FechaActualizacion = fecha,
                FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
                    ? fecha
                    : null
            };
        }
    }

    private sealed class TimeProviderFijo : TimeProvider {
        private readonly DateTimeOffset ahora;

        public TimeProviderFijo(DateTimeOffset ahora) {
            this.ahora = ahora;
        }

        public override DateTimeOffset GetUtcNow() => ahora;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
