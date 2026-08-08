using EndForge.Models;
using EndForge.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EndForge.Tests;

public sealed class MotivacionServiceTests {
    private const string PracticaId = "practica-prueba";
    private static readonly DateTimeOffset FechaBase =
        new(2026, 7, 31, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void UsuarioNuevo_CreaDocumentoVersionadoConNivelUno() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(0, resultado.XpConcedido);
        Assert.Equal(0, resultado.XpTotalResultante);
        Assert.Equal(1, resultado.Resumen.Nivel!.NivelActual);
        Assert.True(File.Exists(servicio.RutaMotivacion));
        using JsonDocument documento = JsonDocument.Parse(
            File.ReadAllText(servicio.RutaMotivacion));
        Assert.Equal(1, documento.RootElement.GetProperty("Version").GetInt32());
        Assert.Equal(
            entorno.Reloj.LocalTimeZone.Id,
            documento.RootElement.GetProperty("ZonaHorariaEstudio").GetString());
    }

    [Fact]
    public void VinculoPersistido_SeConcedeUnaVezInclusoTrasReinicio() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.EnProgreso, @"C:\Practicas\01"));

        ResultadoProcesamientoMotivacion primera =
            servicio.ProcesarVinculoPractica(PracticaId);
        ResultadoProcesamientoMotivacion repetida =
            servicio.ProcesarVinculoPractica(PracticaId);
        ResultadoProcesamientoMotivacion reinicio = entorno
            .CrearServicio()
            .ProcesarVinculoPractica(PracticaId);

        Assert.Equal(10, primera.XpConcedido);
        Assert.Equal(EstadoProcesamientoMotivacion.YaAplicada, repetida.Estado);
        Assert.Equal(0, repetida.XpConcedido);
        Assert.Equal(EstadoProcesamientoMotivacion.YaAplicada, reinicio.Estado);
        Assert.Equal(10, reinicio.XpTotalResultante);
    }

    [Fact]
    public void PracticaRealizada_ConcedePracticaTemaYGradoSinRevocarAlVolverAPendiente() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.Realizada));

        ResultadoProcesamientoMotivacion realizada =
            servicio.ProcesarPracticaRealizada(PracticaId);
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.Pendiente));
        ResultadoProcesamientoMotivacion pendiente =
            servicio.ProcesarPracticaRealizada(PracticaId);

        Assert.Equal(300, realizada.XpConcedido);
        Assert.Equal(300, realizada.XpTotalResultante);
        Assert.Equal(0, pendiente.XpConcedido);
        Assert.Equal(300, servicio.ObtenerResumenMotivacion().XpTotal);
    }

    [Fact]
    public void ProgresoPersistido_ConcedeVinculoPracticaTemaYGradoEnUnaOperacion() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.Realizada,
                    @"C:\Practicas\01")
            }
        };

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(PracticaId, snapshot);

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(310, resultado.XpConcedido);
        Assert.Equal(310, resultado.XpTotalResultante);
        Assert.Equal(4, resultado.ClavesProcesadas.Count);
    }

    [Fact]
    public void ProgresoPersistido_NoReleeUnaVersionPosteriorRevertida() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        int cargasAntes = entorno.CargasProgreso;
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.Realizada,
                    @"C:\Practicas\01")
            }
        };
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.Pendiente));

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(PracticaId, snapshot);

        Assert.Equal(310, resultado.XpConcedido);
        Assert.Equal(cargasAntes, entorno.CargasProgreso);
    }

    [Fact]
    public void ProgresoPersistido_RepetidoEsIdempotente() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.Realizada,
                    @"C:\Practicas\01")
            }
        };

        servicio.ProcesarProgresoPersistido(PracticaId, snapshot);
        ResultadoProcesamientoMotivacion repetida =
            servicio.ProcesarProgresoPersistido(PracticaId, snapshot);

        Assert.Equal(EstadoProcesamientoMotivacion.YaAplicada, repetida.Estado);
        Assert.Equal(0, repetida.XpConcedido);
        Assert.Equal(310, repetida.XpTotalResultante);
    }

    [Fact]
    public void PrimerVinculoPersistido_SeClasificaComoTiempoReal() {
        using EntornoPrueba entorno = new();
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.EnProgreso,
                    @"C:\Practicas\01")
            }
        };
        entorno.Progreso = CrearCargaProgreso(snapshot.Practicas.ToArray());
        MotivacionService servicio = entorno.CrearServicio();

        servicio.ProcesarProgresoPersistido(
            PracticaId,
            snapshot,
            vinculoPersistidoAhora: true,
            realizadaPersistidaAhora: false);

        IReadOnlyDictionary<string, bool> importadas =
            LeerEstadoImportacion(servicio.RutaMotivacion);
        Assert.False(importadas[$"practica:{PracticaId}:vinculada"]);
    }

    [Fact]
    public void PrimeraRealizacion_NoReclasificaElVinculoHistorico() {
        using EntornoPrueba entorno = new();
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.Realizada,
                    @"C:\Practicas\01")
            }
        };
        entorno.Progreso = CrearCargaProgreso(snapshot.Practicas.ToArray());
        MotivacionService servicio = entorno.CrearServicio();

        servicio.ProcesarProgresoPersistido(
            PracticaId,
            snapshot,
            vinculoPersistidoAhora: false,
            realizadaPersistidaAhora: true);

        IReadOnlyDictionary<string, bool> importadas =
            LeerEstadoImportacion(servicio.RutaMotivacion);
        Assert.True(importadas[$"practica:{PracticaId}:vinculada"]);
        Assert.False(importadas[$"practica:{PracticaId}:realizada"]);
        Assert.False(importadas["tema:grado-prueba:tema-prueba:completado"]);
        Assert.False(importadas["grado:grado-prueba:completado"]);
    }

    [Fact]
    public void Evaluaciones_ConcedenAprobacionMejoraLimitadaYPerfecta() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Historial = CrearCargaHistorial(CrearHistorial(60));

        ResultadoProcesamientoMotivacion primera =
            servicio.ProcesarEvaluacionPersistida(PracticaId);
        entorno.Historial = CrearCargaHistorial(CrearHistorial(72, 60, 72));
        ResultadoProcesamientoMotivacion aprobada =
            servicio.ProcesarEvaluacionPersistida(PracticaId);
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(90, 60, 72, 90));
        ResultadoProcesamientoMotivacion mejora =
            servicio.ProcesarEvaluacionPersistida(PracticaId);
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(80, 60, 72, 90, 80));
        ResultadoProcesamientoMotivacion baja =
            servicio.ProcesarEvaluacionPersistida(PracticaId);
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(100, 60, 72, 90, 100));
        ResultadoProcesamientoMotivacion perfecta =
            servicio.ProcesarEvaluacionPersistida(PracticaId);
        ResultadoProcesamientoMotivacion perfectaRepetida =
            servicio.ProcesarEvaluacionPersistida(PracticaId);

        Assert.Equal(0, primera.XpConcedido);
        Assert.Equal(52, aprobada.XpConcedido);
        Assert.Equal(13, mejora.XpConcedido);
        Assert.Equal(0, baja.XpConcedido);
        Assert.Equal(25, perfecta.XpConcedido);
        Assert.Equal(0, perfectaRepetida.XpConcedido);
        Assert.Equal(90, perfecta.XpTotalResultante);
    }

    [Fact]
    public void EvaluacionTrasFalloMotivacional_RecuperaMejoraDemostrable() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(72, 60, 72));

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(PracticaId);

        Assert.Equal(52, resultado.XpConcedido);
        Assert.Equal(52, resultado.XpTotalResultante);
    }

    [Fact]
    public void EvaluacionPersistida_UsaElHistorialPublicadoAunqueLuegoSeElimine() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        HistorialPractica snapshot = CrearHistorial(100, 60, 100);
        entorno.Historial = CrearCargaHistorial();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(PracticaId, snapshot);

        Assert.Equal(90, resultado.XpConcedido);
        Assert.Equal(90, resultado.XpTotalResultante);
    }

    [Fact]
    public void PrimeraEvaluacion_ReclasificaSoloLosHitosCausadosPorElIntento() {
        using EntornoPrueba entorno = new();
        HistorialPractica historial = CrearHistorial(100, 50, 60, 100);
        IntentoPractica intentoActual = historial.Intentos[^1];
        entorno.Historial = CrearCargaHistorial(historial);
        MotivacionService servicio = entorno.CrearServicio();

        servicio.ProcesarEvaluacionPersistida(
            PracticaId,
            historial,
            intentoActual);

        IReadOnlyDictionary<string, bool> importadas =
            LeerEstadoImportacion(servicio.RutaMotivacion);
        Assert.False(importadas[$"practica:{PracticaId}:aprobada"]);
        Assert.False(importadas[$"practica:{PracticaId}:perfecta"]);

        for (int tramo = 1; tramo <= 10; tramo++) {
            Assert.True(importadas[
                $"practica:{PracticaId}:mejora:{tramo:D2}"]);
        }

        for (int tramo = 11; tramo <= 25; tramo++) {
            Assert.False(importadas[
                $"practica:{PracticaId}:mejora:{tramo:D2}"]);
        }
    }

    [Fact]
    public void EvaluacionInferior_NoReclasificaHitosHistoricos() {
        using EntornoPrueba entorno = new();
        HistorialPractica historial = CrearHistorial(100, 100, 80);
        IntentoPractica intentoActual = historial.Intentos[^1];
        entorno.Historial = CrearCargaHistorial(historial);
        MotivacionService servicio = entorno.CrearServicio();

        servicio.ProcesarEvaluacionPersistida(
            PracticaId,
            historial,
            intentoActual);

        IReadOnlyDictionary<string, bool> importadas =
            LeerEstadoImportacion(servicio.RutaMotivacion);
        Assert.True(importadas[$"practica:{PracticaId}:aprobada"]);
        Assert.True(importadas[$"practica:{PracticaId}:perfecta"]);
    }

    [Fact]
    public void PrimerDocumento_IntentoExactoConRelojAtrasado_ReportaSoloXpActual() {
        using EntornoPrueba entorno = new();
        IntentoPractica anterior = CrearIntento(
            "intento-anterior",
            FechaBase,
            60);
        IntentoPractica actual = CrearIntento(
            "intento-actual",
            FechaBase.AddMinutes(-1),
            72);
        HistorialPractica historial = CrearHistorialConIntentoActual(
            actual,
            anterior);
        entorno.Historial = CrearCargaHistorial(historial);
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion primera =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historial,
                actual);
        ResultadoProcesamientoMotivacion repetida =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historial,
                actual);

        Assert.Equal(52, primera.XpConcedido);
        Assert.Equal(52, primera.XpTotalResultante);
        Assert.Equal(1, primera.NivelAnterior);
        Assert.Equal(1, primera.NivelNuevo);
        Assert.False(primera.SubioNivel);
        Assert.Equal(0, repetida.XpConcedido);
        Assert.Equal(52, repetida.XpTotalResultante);
    }

    [Fact]
    public void DocumentoExistente_IntentoInferiorNoReclasificaBacklogHistorico() {
        using EntornoPrueba entorno = new();
        HistorialPractica backlog = CrearHistorial(100, 60, 100);
        entorno.Historial = CrearCargaHistorial(backlog);
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        IntentoPractica actual = CrearIntento(
            "intento-actual",
            FechaBase.AddMinutes(2),
            80);
        HistorialPractica historial = CrearHistorialConIntentoActual(
            actual,
            backlog.Intentos.ToArray());

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historial,
                actual);

        Assert.Equal(0, resultado.XpConcedido);
        IReadOnlyDictionary<string, bool> importadas =
            LeerEstadoImportacion(servicio.RutaMotivacion);
        Assert.True(importadas[$"practica:{PracticaId}:aprobada"]);
        Assert.True(importadas[$"practica:{PracticaId}:perfecta"]);

        for (int tramo = 1; tramo <= 25; tramo++) {
            Assert.True(importadas[
                $"practica:{PracticaId}:mejora:{tramo:D2}"]);
        }
    }

    [Fact]
    public void PrimeraRealizacionConVinculoHistorico_ReportaSoloTrescientosXp() {
        using EntornoPrueba entorno = new();
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.Realizada,
                    @"C:\Practicas\01")
            }
        };
        entorno.Progreso = CrearCargaProgreso(snapshot.Practicas.ToArray());
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(
                PracticaId,
                snapshot,
                vinculoPersistidoAhora: false,
                realizadaPersistidaAhora: true);

        Assert.Equal(300, resultado.XpConcedido);
        Assert.Equal(310, resultado.XpTotalResultante);
        Assert.Equal(1, resultado.NivelAnterior);
        Assert.Equal(2, resultado.NivelNuevo);
        Assert.True(resultado.SubioNivel);
    }

    [Fact]
    public void IntentoExacto_CruzaSetentaYCienConCronologiaAmbigua() {
        using EntornoPrueba entorno = new();
        IntentoPractica primero = CrearIntento(
            "intento-primero",
            FechaBase,
            60);
        IntentoPractica segundo = CrearIntento(
            "intento-segundo",
            FechaBase,
            65);
        IntentoPractica actual = CrearIntento(
            "intento-actual",
            FechaBase,
            100);
        HistorialPractica historial = CrearHistorialConIntentoActual(
            actual,
            primero,
            segundo);
        entorno.Historial = CrearCargaHistorial(historial);
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historial,
                actual);

        Assert.Equal(90, resultado.XpConcedido);
        Assert.Equal(90, resultado.XpTotalResultante);
        IReadOnlyDictionary<string, bool> importadas =
            LeerEstadoImportacion(servicio.RutaMotivacion);
        Assert.False(importadas[$"practica:{PracticaId}:aprobada"]);
        Assert.False(importadas[$"practica:{PracticaId}:perfecta"]);

        for (int tramo = 1; tramo <= 25; tramo++) {
            Assert.False(importadas[
                $"practica:{PracticaId}:mejora:{tramo:D2}"]);
        }
    }

    [Fact]
    public void MigracionAmbigua_TransicionExactaPosteriorNoPierdeMejoraDemostrable() {
        using EntornoPrueba entorno = new();
        IntentoPractica mejorHistorico = CrearIntento(
            "intento-mejor-historico",
            FechaBase,
            90);
        IntentoPractica ultimoConRelojAtrasado = CrearIntento(
            "intento-ultimo-reloj-atrasado",
            FechaBase.AddHours(-1),
            60);
        entorno.Historial = CrearCargaHistorial(new HistorialPractica {
            PracticaId = PracticaId,
            TotalIntentos = 2,
            MejorCalificacion = 90,
            UltimaCalificacion = 60,
            FechaUltimoIntento = ultimoConRelojAtrasado.Fecha,
            Intentos = new[] { mejorHistorico, ultimoConRelojAtrasado }
        });
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion migracion =
            servicio.ReconciliarEstadoActual();
        IntentoPractica actual = CrearIntento(
            "intento-actual",
            FechaBase.AddHours(1),
            100);
        HistorialPractica historialActualizado =
            CrearHistorialConIntentoActual(
                actual,
                mejorHistorico,
                ultimoConRelojAtrasado);

        ResultadoProcesamientoMotivacion actualizacion =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historialActualizado,
                actual);

        Assert.Equal(40, migracion.XpConcedido);
        Assert.Equal(35, actualizacion.XpConcedido);
        Assert.Equal(75, actualizacion.XpTotalResultante);
    }

    [Theory]
    [InlineData(51, 60, 72, 52, 52)]
    [InlineData(100, 90, 100, 35, 75)]
    public void IntentoExacto_DespuesDelLimiteDeDetalle_ConservaLaTransicion(
        int totalIntentos,
        int mejorAnterior,
        int calificacionActual,
        int xpOperacionEsperado,
        int xpTotalEsperado) {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        IntentoPractica actual = CrearIntento(
            $"intento-{totalIntentos}",
            FechaBase.AddHours(2),
            calificacionActual);
        IntentoPractica[] retenidosAnteriores = Enumerable
            .Range(1, 49)
            .Select(indice => CrearIntento(
                $"intento-{indice}",
                FechaBase.AddMinutes(indice),
                mejorAnterior))
            .ToArray();
        HistorialPractica historial = new() {
            PracticaId = PracticaId,
            TotalIntentos = totalIntentos,
            MejorCalificacion = calificacionActual,
            UltimaCalificacion = calificacionActual,
            FechaUltimoIntento = actual.Fecha,
            Intentos = retenidosAnteriores.Append(actual).ToArray()
        };
        TransicionEvaluacionPersistida transicion = new() {
            PracticaId = PracticaId,
            IntentoId = actual.Id,
            CalificacionIntento = actual.Calificacion,
            MejorCalificacionAnterior = mejorAnterior,
            UltimaCalificacionAnterior = mejorAnterior,
            FechaUltimoIntentoAnterior = FechaBase.AddMinutes(49),
            MejorCalificacionPosterior = calificacionActual,
            TotalIntentos = totalIntentos,
            IntentoPublicado = true
        };

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historial,
                transicion);

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(xpOperacionEsperado, resultado.XpConcedido);
        Assert.Equal(xpTotalEsperado, resultado.XpTotalResultante);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(51)]
    [InlineData(100)]
    public void GuardarIntento_ExponeTransicionSinDependerDelDetalleRetenido(
        int totalIntentos) {
        using EntornoPrueba entorno = new();
        HistorialEvaluacionesService historialService = new(entorno.Carpeta);

        for (int indice = 1; indice < totalIntentos; indice++) {
            ResultadoEscrituraHistorialEvaluaciones anterior =
                historialService.GuardarIntento(CrearIntento(
                    $"anterior-{indice}",
                    FechaBase.AddMinutes(indice),
                    60));
            Assert.True(anterior.EsExitosa);
        }

        IntentoPractica intentoActual = CrearIntento(
            $"actual-{totalIntentos}",
            FechaBase.AddMinutes(totalIntentos),
            72);
        ResultadoEscrituraHistorialEvaluaciones guardado =
            historialService.GuardarIntento(intentoActual);
        TransicionEvaluacionPersistida transicion =
            Assert.IsType<TransicionEvaluacionPersistida>(
                guardado.TransicionPersistida);

        Assert.True(guardado.EsExitosa);
        Assert.True(transicion.IntentoPublicado);
        Assert.Equal(totalIntentos, transicion.TotalIntentos);
        Assert.Equal(intentoActual.Id, transicion.IntentoId);
        Assert.Equal(60, transicion.MejorCalificacionAnterior);
        Assert.Equal(72, transicion.MejorCalificacionPosterior);
        Assert.Equal(Math.Min(totalIntentos, 50),
            guardado.HistorialActualizado!.Intentos.Count);

        entorno.Historial = historialService.CargarHistorial();
        MotivacionService motivacion = entorno.CrearServicio();
        ResultadoProcesamientoMotivacion resultado =
            motivacion.ProcesarEvaluacionPersistida(
                PracticaId,
                guardado.HistorialActualizado,
                transicion);
        ResultadoProcesamientoMotivacion reconciliacion =
            motivacion.ReconciliarEstadoActual();

        Assert.Equal(52, resultado.XpConcedido);
        Assert.Equal(52, resultado.XpTotalResultante);
        Assert.Equal(0, reconciliacion.XpConcedido);
        Assert.Equal(52, reconciliacion.XpTotalResultante);
    }

    [Fact]
    public void IntentoCincuentaYUno_PerfectaYMejoraSeProcesanDesdeLaTransicion() {
        using EntornoPrueba entorno = new();
        HistorialEvaluacionesService historialService = new(entorno.Carpeta);

        for (int indice = 1; indice <= 50; indice++) {
            Assert.True(historialService.GuardarIntento(CrearIntento(
                $"anterior-{indice}",
                FechaBase.AddMinutes(indice),
                90)).EsExitosa);
        }

        ResultadoEscrituraHistorialEvaluaciones guardado =
            historialService.GuardarIntento(CrearIntento(
                "actual-51-perfecta",
                FechaBase.AddMinutes(51),
                100));
        entorno.Historial = historialService.CargarHistorial();
        MotivacionService motivacion = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            motivacion.ProcesarEvaluacionPersistida(
                PracticaId,
                guardado.HistorialActualizado!,
                guardado.TransicionPersistida!);

        Assert.Equal(35, resultado.XpConcedido);
        Assert.Equal(75, resultado.XpTotalResultante);
        Assert.Contains(
            $"practica:{PracticaId}:perfecta",
            resultado.ClavesProcesadas);
    }

    [Fact]
    public void HitoImportadoExistente_NoSeConvierteEnActualAlRepetirLaOperacion() {
        using EntornoPrueba entorno = new();
        ProgresoCurso snapshot = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(
                    EstadoPracticaCurso.Realizada,
                    @"C:\Practicas\01")
            }
        };
        entorno.Progreso = CrearCargaProgreso(snapshot.Practicas.ToArray());
        MotivacionService servicio = entorno.CrearServicio();
        ResultadoProcesamientoMotivacion migracion =
            servicio.ReconciliarEstadoActual();

        ResultadoProcesamientoMotivacion repetida =
            servicio.ProcesarProgresoPersistido(
                PracticaId,
                snapshot,
                vinculoPersistidoAhora: true,
                realizadaPersistidaAhora: true);

        Assert.Equal(310, migracion.XpConcedido);
        Assert.Equal(EstadoProcesamientoMotivacion.YaAplicada, repetida.Estado);
        Assert.Equal(0, repetida.XpConcedido);
        Assert.All(
            LeerEstadoImportacion(servicio.RutaMotivacion).Values,
            Assert.True);
    }

    [Fact]
    public void RealizadaPendienteRealizada_NoDuplicaConcesiones() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso realizada = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(EstadoPracticaCurso.Realizada)
            }
        };
        ProgresoCurso pendiente = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgreso(EstadoPracticaCurso.Pendiente)
            }
        };

        ResultadoProcesamientoMotivacion primera =
            servicio.ProcesarProgresoPersistido(PracticaId, realizada);
        servicio.ProcesarProgresoPersistido(PracticaId, pendiente);
        ResultadoProcesamientoMotivacion segunda =
            servicio.ProcesarProgresoPersistido(PracticaId, realizada);

        Assert.Equal(300, primera.XpConcedido);
        Assert.Equal(0, segunda.XpConcedido);
        Assert.Equal(300, segunda.XpTotalResultante);
    }

    [Fact]
    public void CatalogoMultinivel_CompletaSoloElTemaYGradoQueCorresponden() {
        using EntornoPrueba entorno = new() {
            Catalogo = CrearCatalogoMultinivel()
        };
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        ProgresoCurso progreso = new() {
            Practicas = new List<ProgresoPractica> {
                CrearProgresoPara("g1-t1-p1", EstadoPracticaCurso.Realizada),
                CrearProgresoPara("g1-t1-p2", EstadoPracticaCurso.Pendiente),
                CrearProgresoPara("g1-t2-p1", EstadoPracticaCurso.Pendiente),
                CrearProgresoPara("g2-t1-p1", EstadoPracticaCurso.Pendiente)
            }
        };

        ResultadoProcesamientoMotivacion primera =
            servicio.ProcesarProgresoPersistido("g1-t1-p1", progreso);
        progreso.Practicas[1] = CrearProgresoPara(
            "g1-t1-p2",
            EstadoPracticaCurso.Realizada);
        ResultadoProcesamientoMotivacion temaUno =
            servicio.ProcesarProgresoPersistido("g1-t1-p2", progreso);
        progreso.Practicas[2] = CrearProgresoPara(
            "g1-t2-p1",
            EstadoPracticaCurso.Realizada);
        ResultadoProcesamientoMotivacion gradoUno =
            servicio.ProcesarProgresoPersistido("g1-t2-p1", progreso);
        progreso.Practicas[3] = CrearProgresoPara(
            "g2-t1-p1",
            EstadoPracticaCurso.Realizada);
        ResultadoProcesamientoMotivacion gradoDos =
            servicio.ProcesarProgresoPersistido("g2-t1-p1", progreso);

        Assert.Equal(25, primera.XpConcedido);
        Assert.Equal(100, temaUno.XpConcedido);
        Assert.Equal(300, gradoUno.XpConcedido);
        Assert.Equal(300, gradoDos.XpConcedido);
        Assert.Equal(725, gradoDos.XpTotalResultante);
    }

    [Fact]
    public void EvaluacionExacta_SesentaYNueveASetenta_ConcedeAprobacionYMejora() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        IntentoPractica anterior = CrearIntento(
            "intento-69",
            FechaBase,
            69);
        IntentoPractica actual = CrearIntento(
            "intento-70",
            FechaBase.AddMinutes(1),
            70);
        HistorialPractica historial =
            CrearHistorialConIntentoActual(actual, anterior);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historial,
                actual);

        Assert.Equal(41, resultado.XpConcedido);
        Assert.Equal(41, resultado.XpTotalResultante);
    }

    [Fact]
    public void EvaluacionExacta_NoventaYNueveACien_ConcedePerfectaYMejora() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        IntentoPractica anterior = CrearIntento(
            "intento-99",
            FechaBase,
            99);
        HistorialPractica historialAnterior =
            CrearHistorialConIntentoActual(anterior);
        servicio.ProcesarEvaluacionPersistida(
            PracticaId,
            historialAnterior,
            anterior);
        IntentoPractica actual = CrearIntento(
            "intento-100",
            FechaBase.AddMinutes(1),
            100);
        HistorialPractica historialActual =
            CrearHistorialConIntentoActual(actual, anterior);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                PracticaId,
                historialActual,
                actual);

        Assert.Equal(26, resultado.XpConcedido);
        Assert.Equal(66, resultado.XpTotalResultante);
    }

    [Fact]
    public void HistorialEliminadoDespuesDeConceder_NoRevocaXp() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        HistorialPractica historial = CrearHistorial(100, 60, 100);

        servicio.ProcesarEvaluacionPersistida(PracticaId, historial);
        entorno.Historial = CrearCargaHistorial();
        ResultadoProcesamientoMotivacion reconciliada =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(0, reconciliada.XpConcedido);
        Assert.Equal(90, reconciliada.XpTotalResultante);
    }

    [Fact]
    public void ReconciliacionRepetida_EsIdempotente() {
        using EntornoPrueba entorno = new();
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(
                EstadoPracticaCurso.Realizada,
                @"C:\Practicas\01"));
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(100, 60, 100));
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion primera =
            servicio.ReconciliarEstadoActual();
        ResultadoProcesamientoMotivacion segunda =
            servicio.ReconciliarEstadoActual();
        ResultadoProcesamientoMotivacion tercera =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(400, primera.XpConcedido);
        Assert.Equal(0, segunda.XpConcedido);
        Assert.Equal(0, tercera.XpConcedido);
        Assert.Equal(400, tercera.XpTotalResultante);
    }

    [Fact]
    public void Migracion_ImportaSoloHechosDemostrablesYReportaXpConcedido() {
        using EntornoPrueba entorno = new();
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.Realizada, @"C:\Practicas\01"));
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(100, 60, 100));
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(400, resultado.XpConcedido);
        Assert.Equal(400, resultado.XpTotalResultante);
        using JsonDocument documento = JsonDocument.Parse(
            File.ReadAllText(servicio.RutaMotivacion));
        JsonElement concesiones = documento.RootElement.GetProperty("ConcesionesXP");
        Assert.Equal(31, concesiones.GetArrayLength());
        Assert.All(
            concesiones.EnumerateArray(),
            item => Assert.True(item.GetProperty("EsImportada").GetBoolean()));
    }

    [Fact]
    public void Migracion_HistorialTruncadoNoInventaXpDeMejora() {
        using EntornoPrueba entorno = new();
        HistorialPractica historial = CrearHistorial(90, 60, 90);
        entorno.Historial = CrearCargaHistorial(new HistorialPractica {
            PracticaId = historial.PracticaId,
            TotalIntentos = 3,
            MejorCalificacion = historial.MejorCalificacion,
            UltimaCalificacion = historial.UltimaCalificacion,
            FechaUltimoIntento = historial.FechaUltimoIntento,
            Intentos = historial.Intentos
        });

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(40, resultado.XpConcedido);
        Assert.Contains(
            AdvertenciaMotivacion.MejoraHistoricaNoDemostrable,
            resultado.Resumen.Advertencias);
    }

    [Fact]
    public void Migracion_RetrocesoDeRelojNoInventaXpDeMejora() {
        using EntornoPrueba entorno = new();
        IntentoPractica primero = new() {
            Id = "intento-primero",
            PracticaId = PracticaId,
            Fecha = FechaBase,
            Calificacion = 90,
            ResultadoGeneral = "Resultado",
            PuntosMaximos = 100,
            RutaProyecto = @"C:\Practicas\01"
        };
        IntentoPractica posteriorConRelojAtrasado = new() {
            Id = "intento-posterior",
            PracticaId = PracticaId,
            Fecha = FechaBase.AddHours(-1),
            Calificacion = 60,
            ResultadoGeneral = "Resultado",
            PuntosMaximos = 100,
            RutaProyecto = @"C:\Practicas\01"
        };
        entorno.Historial = CrearCargaHistorial(new HistorialPractica {
            PracticaId = PracticaId,
            TotalIntentos = 2,
            MejorCalificacion = 90,
            UltimaCalificacion = 60,
            FechaUltimoIntento = posteriorConRelojAtrasado.Fecha,
            Intentos = new[] { primero, posteriorConRelojAtrasado }
        });

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(40, resultado.XpConcedido);
        Assert.DoesNotContain(
            resultado.ClavesProcesadas,
            clave => clave.Contains(":mejora:", StringComparison.Ordinal));
        Assert.Contains(
            AdvertenciaMotivacion.MejoraHistoricaNoDemostrable,
            resultado.Resumen.Advertencias);
    }

    [Fact]
    public void Migracion_ProgresoParcialNoOcultaMejoraConHistorialCompleto() {
        using EntornoPrueba entorno = new();
        entorno.Progreso = CrearCargaProgreso(new ProgresoPractica {
            PracticaId = "practica-huerfana",
            Estado = EstadoPracticaCurso.Realizada,
            FechaCreacion = FechaBase,
            FechaActualizacion = FechaBase,
            FechaFinalizacion = FechaBase
        });
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(90, 60, 90));

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(65, resultado.XpConcedido);
        Assert.Contains(
            resultado.ClavesProcesadas,
            clave => clave.Contains(":mejora:", StringComparison.Ordinal));
        Assert.Contains(
            AdvertenciaMotivacion.DatosAcademicosParciales,
            resultado.Resumen.Advertencias);
    }

    [Fact]
    public async Task DosInstanciasConcurrentes_NoDuplicanLaMismaConcesion() {
        using EntornoPrueba entorno = new();
        entorno.CrearServicio().ReconciliarEstadoActual();
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.EnProgreso, @"C:\Practicas\01"));
        MotivacionService primera = entorno.CrearServicio();
        MotivacionService segunda = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion[] resultados = await Task.WhenAll(
            Task.Run(() => primera.ProcesarVinculoPractica(PracticaId)),
            Task.Run(() => segunda.ProcesarVinculoPractica(PracticaId)));

        Assert.Equal(10, resultados.Sum(item => item.XpConcedido));
        Assert.Equal(10, primera.ObtenerResumenMotivacion().XpTotal);
    }

    [Fact]
    public void FuenteAcademicaNoDisponible_NoConcedeNiPresentaCeroFalso() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Progreso = new ResultadoCargaProgreso {
            Estado = EstadoCargaProgreso.ErrorIo,
            Error = new IOException("progreso.json bloqueado")
        };

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarVinculoPractica(PracticaId);

        Assert.Equal(
            EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
            resultado.Estado);
        Assert.Equal(0, resultado.XpConcedido);
        Assert.Equal(0, resultado.Resumen.XpTotal);
        Assert.NotNull(resultado.Error);
    }

    [Fact]
    public void ReconciliacionParcial_ConservaErrorYLuegoReparaLoPendiente() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        IOException errorHistorial = new("historial-evaluaciones.json bloqueado");
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(
                EstadoPracticaCurso.Realizada,
                @"C:\Practicas\01"));
        entorno.Historial = new ResultadoCargaHistorialEvaluaciones {
            Estado = EstadoCargaHistorialEvaluaciones.ErrorIo,
            Error = errorHistorial
        };

        ResultadoProcesamientoMotivacion parcial =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, parcial.Estado);
        Assert.Equal(310, parcial.XpConcedido);
        Assert.Equal(310, parcial.XpTotalResultante);
        Assert.Same(errorHistorial, parcial.Error);
        Assert.Contains(
            AdvertenciaMotivacion.DatosAcademicosParciales,
            parcial.Resumen.Advertencias);

        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(100, 60, 100));
        ResultadoProcesamientoMotivacion reparada =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, reparada.Estado);
        Assert.Equal(90, reparada.XpConcedido);
        Assert.Equal(400, reparada.XpTotalResultante);
        Assert.Null(reparada.Error);
    }

    [Fact]
    public void ZonaHorariaPersistidaInexistente_ConservaXpYDevuelveAdvertencia() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        servicio.ProcesarProgresoPersistido(
            PracticaId,
            new ProgresoCurso {
                Practicas = new List<ProgresoPractica> {
                    CrearProgreso(
                        EstadoPracticaCurso.EnProgreso,
                        @"C:\Practicas\01")
                }
            });
        string original = File.ReadAllText(servicio.RutaMotivacion);
        JsonObject documento = JsonNode.Parse(original)!.AsObject();
        documento["ZonaHorariaEstudio"] = "EndForge/Zona-Inexistente";
        string alterado = documento.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true
        });
        File.WriteAllText(servicio.RutaMotivacion, alterado);

        ResumenMotivacion resumen = servicio.ObtenerResumenMotivacion();

        Assert.Equal(10, resumen.XpTotal);
        Assert.Contains(
            AdvertenciaMotivacion.ZonaHorariaNoDisponible,
            resumen.Advertencias);
        Assert.Equal(alterado, File.ReadAllText(servicio.RutaMotivacion));
    }

    [Fact]
    public void RetrocesoDeReloj_NoRevocaNiBloqueaRecompensasYAdvierte() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        servicio.ProcesarProgresoPersistido(
            PracticaId,
            new ProgresoCurso {
                Practicas = new List<ProgresoPractica> {
                    CrearProgreso(
                        EstadoPracticaCurso.EnProgreso,
                        @"C:\Practicas\01")
                }
            });
        entorno.Reloj.Avanzar(TimeSpan.FromHours(-1));

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarProgresoPersistido(
                PracticaId,
                new ProgresoCurso {
                    Practicas = new List<ProgresoPractica> {
                        CrearProgreso(
                            EstadoPracticaCurso.Realizada,
                            @"C:\Practicas\01")
                    }
                });

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(300, resultado.XpConcedido);
        Assert.Equal(310, resultado.XpTotalResultante);
        Assert.Equal(FechaBase, resultado.Resumen.UltimoInstanteUtcAceptado);
        Assert.Contains(
            AdvertenciaMotivacion.RetrocesoRelojDetectado,
            resultado.Resumen.Advertencias);
    }

    [Fact]
    public void MigracionConFuenteNoDisponible_NoCreaDocumentoParcial() {
        using EntornoPrueba entorno = new() {
            Progreso = new ResultadoCargaProgreso {
                Estado = EstadoCargaProgreso.PermisosInsuficientes,
                Error = new UnauthorizedAccessException()
            }
        };
        MotivacionService servicio = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
            resultado.Estado);
        Assert.False(File.Exists(servicio.RutaMotivacion));
        Assert.Null(resultado.Resumen.XpTotal);
    }

    [Fact]
    public void DocumentoCorrupto_NoSeReiniciaNiSeSobrescribe() {
        using EntornoPrueba entorno = new();
        string ruta = Path.Combine(entorno.Carpeta, "motivacion.json");
        const string contenido = "{ contenido roto";
        File.WriteAllText(ruta, contenido);
        MotivacionService servicio = entorno.CrearServicio();

        ResumenMotivacion resumen = servicio.ObtenerResumenMotivacion();

        Assert.Equal(EstadoDisponibilidadMotivacion.NoDisponible, resumen.Estado);
        Assert.Null(resumen.XpTotal);
        Assert.Equal(contenido, File.ReadAllText(ruta));
    }

    [Fact]
    public void DocumentoExcesivo_SeRechazaSinModificarlo() {
        using EntornoPrueba entorno = new();
        string ruta = Path.Combine(entorno.Carpeta, "motivacion.json");
        const long longitud = (16L * 1024L * 1024L) + 1L;
        using (FileStream archivo = new(
            ruta,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None)) {
            archivo.SetLength(longitud);
        }
        MotivacionService servicio = entorno.CrearServicio();

        ResumenMotivacion resumen = servicio.ObtenerResumenMotivacion();

        Assert.Equal(EstadoDisponibilidadMotivacion.NoDisponible, resumen.Estado);
        Assert.IsType<InvalidDataException>(resumen.Error);
        Assert.Equal(longitud, new FileInfo(ruta).Length);
    }

    [Theory]
    [InlineData("xp-negativo")]
    [InlineData("id-vacio")]
    [InlineData("fecha-invalida")]
    public void ConcesionInvalida_SeRechazaSinSobrescribir(string alteracion) {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        servicio.ProcesarProgresoPersistido(
            PracticaId,
            new ProgresoCurso {
                Practicas = new List<ProgresoPractica> {
                    CrearProgreso(
                        EstadoPracticaCurso.EnProgreso,
                        @"C:\Practicas\01")
                }
            });
        JsonObject documento = JsonNode
            .Parse(File.ReadAllText(servicio.RutaMotivacion))!
            .AsObject();
        JsonObject concesion = documento["ConcesionesXP"]!
            .AsArray()[0]!
            .AsObject();

        switch (alteracion) {
            case "xp-negativo":
                concesion["CantidadXP"] = -10;
                break;
            case "id-vacio":
                concesion["PracticaId"] = "";
                break;
            case "fecha-invalida":
                concesion["FechaUtc"] = "0001-01-01T00:00:00+00:00";
                break;
            default:
                throw new InvalidOperationException("Alteración de prueba desconocida.");
        }

        string alterado = documento.ToJsonString(new JsonSerializerOptions {
            WriteIndented = true
        });
        File.WriteAllText(servicio.RutaMotivacion, alterado);

        ResumenMotivacion resumen = servicio.ObtenerResumenMotivacion();

        Assert.Equal(EstadoDisponibilidadMotivacion.NoDisponible, resumen.Estado);
        Assert.NotNull(resumen.Error);
        Assert.Equal(alterado, File.ReadAllText(servicio.RutaMotivacion));
    }

    [Fact]
    public void VersionFutura_NoSeSobrescribe() {
        using EntornoPrueba entorno = new();
        string ruta = Path.Combine(entorno.Carpeta, "motivacion.json");
        const string contenido = "{\"Version\":99}";
        File.WriteAllText(ruta, contenido);

        ResultadoProcesamientoMotivacion resultado = entorno
            .CrearServicio()
            .ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.VersionIncompatible,
            resultado.Estado);
        Assert.Equal(contenido, File.ReadAllText(ruta));
    }

    [Fact]
    public void TramosDeMejoraNoContiguos_SeRechazanSinReiniciarArchivo() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Historial = CrearCargaHistorial(
            CrearHistorial(62, 60, 62));
        servicio.ProcesarEvaluacionPersistida(PracticaId);
        string original = File.ReadAllText(servicio.RutaMotivacion);
        string alterado = original.Replace(
            $"practica:{PracticaId}:mejora:02",
            $"practica:{PracticaId}:mejora:03",
            StringComparison.Ordinal);
        File.WriteAllText(servicio.RutaMotivacion, alterado);

        ResumenMotivacion resumen = servicio.ObtenerResumenMotivacion();

        Assert.Equal(EstadoDisponibilidadMotivacion.NoDisponible, resumen.Estado);
        Assert.Equal(alterado, File.ReadAllText(servicio.RutaMotivacion));
    }

    [Fact]
    public void FalloAlReemplazar_ConservaDocumentoAnteriorYLimpiaTemporal() {
        using EntornoPrueba entorno = new();
        MotivacionService inicial = entorno.CrearServicio();
        inicial.ReconciliarEstadoActual();
        string anterior = File.ReadAllText(inicial.RutaMotivacion);
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.EnProgreso, @"C:\Practicas\01"));
        MotivacionService conFallo = entorno.CrearServicio(
            new SistemaArchivosConFalloReemplazo());

        ResultadoProcesamientoMotivacion resultado =
            conFallo.ProcesarVinculoPractica(PracticaId);

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.Equal(anterior, File.ReadAllText(inicial.RutaMotivacion));
        Assert.Empty(Directory.EnumerateFiles(
            entorno.Carpeta,
            ".motivacion-*.tmp"));
    }

    [Fact]
    public void FalloAlCrearDirectorio_NoPublicaDocumento() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio(
            new SistemaArchivosConFalloCreacionDirectorio());

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.False(File.Exists(servicio.RutaMotivacion));
        Assert.Empty(Directory.EnumerateFiles(
            entorno.Carpeta,
            ".motivacion-*.tmp"));
    }

    [Fact]
    public void FalloAlMoverDocumentoInicial_NoPublicaYLimpiaTemporal() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio(
            new SistemaArchivosConFalloMovimiento());

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.False(File.Exists(servicio.RutaMotivacion));
        Assert.Empty(Directory.EnumerateFiles(
            entorno.Carpeta,
            ".motivacion-*.tmp"));
    }

    [Fact]
    public void DocumentoConMasDeCincuentaMilConcesiones_SeRechazaSinSobrescribir() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        JsonObject documento = JsonNode.Parse(
            File.ReadAllText(servicio.RutaMotivacion))!.AsObject();
        JsonArray concesiones = new();

        for (int indice = 0; indice <= 50_000; indice++) {
            string practicaId = $"p-{indice}";
            concesiones.Add(new JsonObject {
                ["Clave"] = $"practica:{practicaId}:vinculada",
                ["CantidadXP"] = 10,
                ["FechaUtc"] = FechaBase,
                ["Tipo"] = "PracticaVinculada",
                ["PracticaId"] = practicaId,
                ["TemaId"] = null,
                ["GradoId"] = null,
                ["EsImportada"] = true
            });
        }

        documento["ConcesionesXP"] = concesiones;
        string contenido = documento.ToJsonString();
        File.WriteAllText(servicio.RutaMotivacion, contenido);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.Equal(contenido, File.ReadAllText(servicio.RutaMotivacion));
    }

    [Fact]
    public void MutexAbandonado_SeRecuperaSinDuplicarConcesiones() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        string rutaNormalizada = Path.GetFullPath(servicio.RutaMotivacion)
            .ToUpperInvariant();
        string nombreMutex = @"Global\EndForge.Motivacion." +
            Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(rutaNormalizada)));
        using ManualResetEventSlim adquirido = new(initialState: false);
        Thread hilo = new(() => {
            using Mutex mutex = new(initiallyOwned: false, nombreMutex);
            mutex.WaitOne();
            adquirido.Set();
        });
        hilo.Start();
        Assert.True(adquirido.Wait(TimeSpan.FromSeconds(5)));
        Assert.True(hilo.Join(TimeSpan.FromSeconds(5)));

        ResultadoProcesamientoMotivacion primera =
            servicio.ReconciliarEstadoActual();
        ResultadoProcesamientoMotivacion segunda =
            servicio.ReconciliarEstadoActual();

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, primera.Estado);
        Assert.Equal(0, segunda.XpConcedido);
        Assert.Equal(primera.XpTotalResultante, segunda.XpTotalResultante);
    }

    [Fact]
    public void NuevaConcesion_UsaInstanteActualYNoLaEjecucionAnterior() {
        using EntornoPrueba entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        entorno.Reloj.Avanzar(TimeSpan.FromHours(2));
        entorno.Progreso = CrearCargaProgreso(
            CrearProgreso(EstadoPracticaCurso.EnProgreso, @"C:\Practicas\01"));

        servicio.ProcesarVinculoPractica(PracticaId);

        using JsonDocument documento = JsonDocument.Parse(
            File.ReadAllText(servicio.RutaMotivacion));
        DateTimeOffset fecha = documento.RootElement
            .GetProperty("ConcesionesXP")[0]
            .GetProperty("FechaUtc")
            .GetDateTimeOffset();
        Assert.Equal(FechaBase.AddHours(2), fecha);
    }

    [Fact]
    public void PropiedadesJsonDuplicadas_SeRechazanSinSobrescribir() {
        using EntornoPrueba entorno = new();
        string ruta = Path.Combine(entorno.Carpeta, "motivacion.json");
        const string contenido = "{\"Version\":1,\"version\":1}";
        File.WriteAllText(ruta, contenido);

        ResumenMotivacion resumen = entorno
            .CrearServicio()
            .ObtenerResumenMotivacion();

        Assert.Equal(EstadoDisponibilidadMotivacion.NoDisponible, resumen.Estado);
        Assert.Equal(contenido, File.ReadAllText(ruta));
    }

    [Fact]
    public void TemporalCompletoSinDestino_SeRecuperaTrasInterrupcion() {
        using EntornoPrueba entorno = new();
        MotivacionService inicial = entorno.CrearServicio();
        inicial.ReconciliarEstadoActual();
        string temporal = Path.Combine(
            entorno.Carpeta,
            ".motivacion-recuperable.tmp");
        File.Move(inicial.RutaMotivacion, temporal);

        ResumenMotivacion resumen = entorno
            .CrearServicio()
            .ObtenerResumenMotivacion();

        Assert.Equal(0, resumen.XpTotal);
        Assert.True(File.Exists(inicial.RutaMotivacion));
        Assert.False(File.Exists(temporal));
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

    private static IReadOnlyDictionary<string, bool> LeerEstadoImportacion(
        string ruta) {
        using JsonDocument documento = JsonDocument.Parse(
            File.ReadAllText(ruta));
        return documento.RootElement
            .GetProperty("ConcesionesXP")
            .EnumerateArray()
            .ToDictionary(
                item => item.GetProperty("Clave").GetString()!,
                item => item.GetProperty("EsImportada").GetBoolean(),
                StringComparer.OrdinalIgnoreCase);
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

    private static ProgresoPractica CrearProgreso(
        EstadoPracticaCurso estado,
        string ruta = "") {
        return new ProgresoPractica {
            PracticaId = PracticaId,
            Estado = estado,
            RutaProyecto = ruta,
            FechaCreacion = FechaBase.AddHours(-1),
            FechaActualizacion = FechaBase,
            FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
                ? FechaBase
                : null
        };
    }

    private static ProgresoPractica CrearProgresoPara(
        string practicaId,
        EstadoPracticaCurso estado) {
        return new ProgresoPractica {
            PracticaId = practicaId,
            Estado = estado,
            FechaCreacion = FechaBase.AddHours(-1),
            FechaActualizacion = FechaBase,
            FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
                ? FechaBase
                : null
        };
    }

    private static HistorialPractica CrearHistorial(
        int mejor,
        params int[] calificaciones) {
        int[] valores = calificaciones.Length == 0
            ? new[] { mejor }
            : calificaciones;
        IntentoPractica[] intentos = valores
            .Select((calificacion, indice) => new IntentoPractica {
                Id = $"intento-{indice + 1}",
                PracticaId = PracticaId,
                Fecha = FechaBase.AddMinutes(indice),
                Calificacion = calificacion,
                ResultadoGeneral = "Resultado",
                PuntosMaximos = 100,
                RutaProyecto = @"C:\Practicas\01"
            })
            .ToArray();
        return new HistorialPractica {
            PracticaId = PracticaId,
            TotalIntentos = intentos.Length,
            MejorCalificacion = mejor,
            UltimaCalificacion = intentos[^1].Calificacion,
            FechaUltimoIntento = intentos[^1].Fecha,
            Intentos = intentos
        };
    }

    private static IntentoPractica CrearIntento(
        string id,
        DateTimeOffset fecha,
        int calificacion) {
        return new IntentoPractica {
            Id = id,
            PracticaId = PracticaId,
            Fecha = fecha,
            Calificacion = calificacion,
            ResultadoGeneral = "Resultado",
            PuntosMaximos = 100,
            RutaProyecto = @"C:\Practicas\01"
        };
    }

    private static HistorialPractica CrearHistorialConIntentoActual(
        IntentoPractica intentoActual,
        params IntentoPractica[] anteriores) {
        IntentoPractica[] intentos = anteriores
            .Append(intentoActual)
            .ToArray();
        return new HistorialPractica {
            PracticaId = PracticaId,
            TotalIntentos = intentos.Length,
            MejorCalificacion = intentos.Max(item => item.Calificacion),
            UltimaCalificacion = intentoActual.Calificacion,
            FechaUltimoIntento = intentoActual.Fecha,
            Intentos = intentos
        };
    }

    private sealed class EntornoPrueba : IDisposable {
        public EntornoPrueba() {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-Motivacion-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
        }

        public string Carpeta { get; }

        public TimeProviderFijo Reloj { get; } = new(FechaBase);

        public ResultadoCargaProgreso Progreso { get; set; } =
            CrearCargaProgreso();

        public ResultadoCargaHistorialEvaluaciones Historial { get; set; } =
            CrearCargaHistorial();

        public IReadOnlyList<GradoCurso> Catalogo { get; set; } =
            CrearCatalogo();

        public int CargasProgreso { get; private set; }

        public MotivacionService CrearServicio(
            ISistemaArchivosMotivacion? archivos = null) {
            return new MotivacionService(
                Carpeta,
                Reloj,
                () => Catalogo,
                () => {
                    CargasProgreso++;
                    return Progreso;
                },
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
        PracticaCurso practica = new() {
            Id = PracticaId,
            TemaId = "tema-prueba",
            Numero = 1,
            Nombre = "Práctica"
        };
        TemaCurso tema = new() {
            Id = "tema-prueba",
            Numero = 1,
            Nombre = "Tema",
            Practicas = new[] { practica }
        };
        return new[] {
            new GradoCurso {
                Id = "grado-prueba",
                Numero = 1,
                Nombre = "Grado",
                EsContenidoDisponible = true,
                Temas = new[] { tema }
            }
        };
    }

    private static IReadOnlyList<GradoCurso> CrearCatalogoMultinivel() {
        return new[] {
            new GradoCurso {
                Id = "grado-uno",
                Numero = 1,
                Nombre = "Grado uno",
                EsContenidoDisponible = true,
                Temas = new[] {
                    CrearTemaCatalogo(
                        "g1-t1",
                        1,
                        "g1-t1-p1",
                        "g1-t1-p2"),
                    CrearTemaCatalogo(
                        "g1-t2",
                        2,
                        "g1-t2-p1")
                }
            },
            new GradoCurso {
                Id = "grado-dos",
                Numero = 2,
                Nombre = "Grado dos",
                EsContenidoDisponible = true,
                Temas = new[] {
                    CrearTemaCatalogo(
                        "g2-t1",
                        1,
                        "g2-t1-p1")
                }
            }
        };
    }

    private static TemaCurso CrearTemaCatalogo(
        string temaId,
        int numero,
        params string[] practicas) {
        return new TemaCurso {
            Id = temaId,
            Numero = numero,
            Nombre = temaId,
            Practicas = practicas
                .Select((id, indice) => new PracticaCurso {
                    Id = id,
                    TemaId = temaId,
                    Numero = indice + 1,
                    Nombre = id
                })
                .ToArray()
        };
    }

    private sealed class TimeProviderFijo : TimeProvider {
        private DateTimeOffset ahora;

        public TimeProviderFijo(DateTimeOffset ahora) {
            this.ahora = ahora;
        }

        public override DateTimeOffset GetUtcNow() => ahora;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public void Avanzar(TimeSpan intervalo) {
            ahora = ahora.Add(intervalo);
        }
    }

    private sealed class SistemaArchivosConFalloReemplazo :
        ISistemaArchivosMotivacion {
        private readonly SistemaArchivosMotivacion real = new();

        public bool ArchivoExiste(string ruta) => real.ArchivoExiste(ruta);
        public long ObtenerLongitud(string ruta) => real.ObtenerLongitud(ruta);
        public string LeerTodoTexto(string ruta) => real.LeerTodoTexto(ruta);
        public void CrearDirectorio(string ruta) => real.CrearDirectorio(ruta);
        public void EscribirTodoTextoDurable(string ruta, string contenido) =>
            real.EscribirTodoTextoDurable(ruta, contenido);
        public void Reemplazar(string origen, string destino) =>
            throw new IOException("Fallo de reemplazo simulado.");
        public void Mover(string origen, string destino) =>
            real.Mover(origen, destino);
        public void Eliminar(string ruta) => real.Eliminar(ruta);
        public IEnumerable<string> EnumerarArchivos(
            string carpeta,
            string patron) => real.EnumerarArchivos(carpeta, patron);
        public DateTime ObtenerUltimaEscrituraUtc(string ruta) =>
            real.ObtenerUltimaEscrituraUtc(ruta);
    }

    private sealed class SistemaArchivosConFalloCreacionDirectorio :
        SistemaArchivosDelegado {
        public override void CrearDirectorio(string ruta) =>
            throw new UnauthorizedAccessException(
                "Fallo de creación de directorio simulado.");
    }

    private sealed class SistemaArchivosConFalloMovimiento :
        SistemaArchivosDelegado {
        public override void Mover(string origen, string destino) =>
            throw new IOException("Fallo de movimiento inicial simulado.");
    }

    private abstract class SistemaArchivosDelegado :
        ISistemaArchivosMotivacion {
        private readonly SistemaArchivosMotivacion real = new();

        public virtual bool ArchivoExiste(string ruta) => real.ArchivoExiste(ruta);
        public virtual long ObtenerLongitud(string ruta) => real.ObtenerLongitud(ruta);
        public virtual string LeerTodoTexto(string ruta) => real.LeerTodoTexto(ruta);
        public virtual void CrearDirectorio(string ruta) => real.CrearDirectorio(ruta);
        public virtual void EscribirTodoTextoDurable(
            string ruta,
            string contenido) => real.EscribirTodoTextoDurable(ruta, contenido);
        public virtual void Reemplazar(string origen, string destino) =>
            real.Reemplazar(origen, destino);
        public virtual void Mover(string origen, string destino) =>
            real.Mover(origen, destino);
        public virtual void Eliminar(string ruta) => real.Eliminar(ruta);
        public virtual IEnumerable<string> EnumerarArchivos(
            string carpeta,
            string patron) => real.EnumerarArchivos(carpeta, patron);
        public virtual DateTime ObtenerUltimaEscrituraUtc(string ruta) =>
            real.ObtenerUltimaEscrituraUtc(ruta);
    }
}
