using EndForge.Models;
using EndForge.Services;
using System.Globalization;
using System.Text.Json;

namespace EndForge.Tests;

public sealed class MotivacionActividadV2Tests {
    private static readonly DateTimeOffset FechaProceso =
        new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ProgresoExacto_RegistraElDiaPublicadoUnaSolaVez() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        DateTimeOffset fechaActividad = FechaProceso.AddDays(-1);
        (ProgresoCurso progreso, TransicionProgresoPersistida transicion) =
            CrearProgresoExacto("practica-a", fechaActividad);

        ResultadoProcesamientoMotivacion primero =
            servicio.ProcesarProgresoPersistido(
                "practica-a",
                progreso,
                transicion);
        ResultadoProcesamientoMotivacion repetido =
            servicio.ProcesarProgresoPersistido(
                "practica-a",
                progreso,
                transicion);

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, primero.Estado);
        Assert.NotEqual(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            repetido.Estado);
        Assert.Equal(
            new[] { DateOnly.FromDateTime(fechaActividad.UtcDateTime) },
            LeerDiasActividad(servicio.RutaMotivacion));
        LogroDesbloqueado logro = Assert.Single(
            primero.LogrosNuevos,
            item => item.LogroId ==
                CatalogoLogrosService.PrimeraPracticaVinculadaId);
        Assert.Equal(fechaActividad, logro.FechaReconocimientoUtc);
    }

    [Fact]
    public void EvaluacionExactaSinXp_RegistraActividad() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        DateTimeOffset fechaActividad = FechaProceso.AddDays(-2);
        (HistorialPractica historial, TransicionEvaluacionPersistida transicion) =
            CrearEvaluacionExacta("practica-a", fechaActividad, 50);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                "practica-a",
                historial,
                transicion);

        Assert.Equal(0, resultado.XpConcedido);
        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(
            new[] { DateOnly.FromDateTime(fechaActividad.UtcDateTime) },
            LeerDiasActividad(servicio.RutaMotivacion));
    }

    [Fact]
    public void EvaluacionProcesadaDespuesDeMedianoche_UsaFechaIntento() {
        DateTimeOffset fechaIntento =
            new(2026, 8, 8, 23, 50, 0, TimeSpan.Zero);
        using EntornoActividad entorno = new(
            new DateTimeOffset(2026, 8, 9, 8, 0, 0, TimeSpan.Zero));
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        (HistorialPractica historial, TransicionEvaluacionPersistida transicion) =
            CrearEvaluacionExacta("practica-a", fechaIntento, 40);

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarEvaluacionPersistida(
                "practica-a",
                historial,
                transicion);

        Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, resultado.Estado);
        Assert.Equal(
            new[] { new DateOnly(2026, 8, 8) },
            LeerDiasActividad(servicio.RutaMotivacion));
        Assert.Equal(new DateOnly(2026, 8, 8), resultado.Resumen.Racha.UltimoDiaEstudio);
    }

    [Fact]
    public void EventosFueraDeOrdenYRelojAtrasado_ConservanDiasAceptados() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        DateTimeOffset fechaReciente = FechaProceso.AddDays(-2);
        DateTimeOffset fechaAnterior = FechaProceso.AddDays(-4);
        (ProgresoCurso reciente, TransicionProgresoPersistida transicionReciente) =
            CrearProgresoExacto("practica-a", fechaReciente);
        (ProgresoCurso anterior, TransicionProgresoPersistida transicionAnterior) =
            CrearProgresoExacto("practica-b", fechaAnterior);

        servicio.ProcesarProgresoPersistido(
            "practica-a",
            reciente,
            transicionReciente);
        servicio.ProcesarProgresoPersistido(
            "practica-b",
            anterior,
            transicionAnterior);

        entorno.Reloj.AhoraUtc = FechaProceso.AddDays(-5);
        (ProgresoCurso atrasado, TransicionProgresoPersistida transicionAtrasada) =
            CrearProgresoExacto("practica-c", FechaProceso.AddDays(-5));
        ResultadoProcesamientoMotivacion resultadoAtrasado =
            servicio.ProcesarProgresoPersistido(
                "practica-c",
                atrasado,
                transicionAtrasada);

        Assert.Contains(
            AdvertenciaMotivacion.RetrocesoRelojDetectado,
            resultadoAtrasado.Resumen.Advertencias);
        Assert.Equal(
            new[] {
                DateOnly.FromDateTime(FechaProceso.AddDays(-5).UtcDateTime),
                DateOnly.FromDateTime(fechaAnterior.UtcDateTime),
                DateOnly.FromDateTime(fechaReciente.UtcDateTime)
            },
            LeerDiasActividad(servicio.RutaMotivacion));
    }

    [Fact]
    public void EvidenciaInvalidaOPracticaDesconocida_NoMutaActividad() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        DateTimeOffset fecha = FechaProceso.AddDays(-1);
        (HistorialPractica historial, TransicionEvaluacionPersistida valida) =
            CrearEvaluacionExacta("practica-a", fecha, 80);
        TransicionEvaluacionPersistida invalida = CopiarTransicion(
            valida,
            fechaIntento: fecha.AddDays(-1));

        ResultadoProcesamientoMotivacion resultadoInvalido =
            servicio.ProcesarEvaluacionPersistida(
                "practica-a",
                historial,
                invalida);
        (HistorialPractica historialDesconocido,
            TransicionEvaluacionPersistida transicionDesconocida) =
            CrearEvaluacionExacta("practica-desconocida", fecha, 80);
        ResultadoProcesamientoMotivacion resultadoDesconocido =
            servicio.ProcesarEvaluacionPersistida(
                "practica-desconocida",
                historialDesconocido,
                transicionDesconocida);

        Assert.Equal(
            EstadoProcesamientoMotivacion.DatosMotivacionalesNoDisponibles,
            resultadoInvalido.Estado);
        Assert.NotEqual(
            EstadoProcesamientoMotivacion.Aplicada,
            resultadoDesconocido.Estado);
        Assert.Empty(LeerDiasActividad(servicio.RutaMotivacion));
    }

    [Fact]
    public async Task DosInstancias_MismoDiaMantienenUnaSolaEntrada() {
        using EntornoActividad entorno = new();
        MotivacionService inicial = entorno.CrearServicio();
        inicial.ReconciliarEstadoActual();
        DateTimeOffset fecha = FechaProceso.AddDays(-1);
        (ProgresoCurso progresoA, TransicionProgresoPersistida transicionA) =
            CrearProgresoExacto("practica-a", fecha.AddHours(1));
        (ProgresoCurso progresoB, TransicionProgresoPersistida transicionB) =
            CrearProgresoExacto("practica-b", fecha.AddHours(8));
        MotivacionService instanciaA = entorno.CrearServicio();
        MotivacionService instanciaB = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion[] resultados = await Task.WhenAll(
            Task.Run(() => instanciaA.ProcesarProgresoPersistido(
                "practica-a",
                progresoA,
                transicionA)),
            Task.Run(() => instanciaB.ProcesarProgresoPersistido(
                "practica-b",
                progresoB,
                transicionB)));

        Assert.All(resultados, resultado => Assert.NotEqual(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado));
        Assert.Equal(
            new[] { DateOnly.FromDateTime(fecha.UtcDateTime) },
            LeerDiasActividad(inicial.RutaMotivacion));
    }

    [Fact]
    public async Task DosInstancias_DiasDistintosConservanAmbasEntradas() {
        using EntornoActividad entorno = new();
        MotivacionService inicial = entorno.CrearServicio();
        inicial.ReconciliarEstadoActual();
        DateTimeOffset fechaA = FechaProceso.AddDays(-3);
        DateTimeOffset fechaB = FechaProceso.AddDays(-1);
        (ProgresoCurso progresoA, TransicionProgresoPersistida transicionA) =
            CrearProgresoExacto("practica-a", fechaA);
        (ProgresoCurso progresoB, TransicionProgresoPersistida transicionB) =
            CrearProgresoExacto("practica-b", fechaB);
        MotivacionService instanciaA = entorno.CrearServicio();
        MotivacionService instanciaB = entorno.CrearServicio();

        ResultadoProcesamientoMotivacion[] resultados = await Task.WhenAll(
            Task.Run(() => instanciaA.ProcesarProgresoPersistido(
                "practica-a",
                progresoA,
                transicionA)),
            Task.Run(() => instanciaB.ProcesarProgresoPersistido(
                "practica-b",
                progresoB,
                transicionB)));

        Assert.All(resultados, resultado => Assert.NotEqual(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado));
        Assert.Equal(
            new[] {
                DateOnly.FromDateTime(fechaA.UtcDateTime),
                DateOnly.FromDateTime(fechaB.UtcDateTime)
            },
            LeerDiasActividad(inicial.RutaMotivacion));
    }

    [Fact]
    public void ApiLegadoSinTransicion_NoInventaDiaDeActividad() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        (ProgresoCurso progreso, _) = CrearProgresoExacto(
            "practica-a",
            FechaProceso.AddDays(-1));
        entorno.Progreso = new ResultadoCargaProgreso {
            Estado = EstadoCargaProgreso.Exitosa,
            Progreso = progreso
        };

        ResultadoProcesamientoMotivacion resultado =
            servicio.ProcesarVinculoPractica("practica-a");

        Assert.NotEqual(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.Empty(LeerDiasActividad(servicio.RutaMotivacion));
    }

    [Fact]
    public void AbrirYReconciliarSinHechos_NoCreaActividad() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();

        servicio.ObtenerResumenMotivacion();
        servicio.ObtenerResumenMotivacion();

        Assert.Empty(LeerDiasActividad(servicio.RutaMotivacion));
    }

    [Fact]
    public void RelojAtrasado_NoConvierteEnFuturaLaUltimaActividadAceptada() {
        using EntornoActividad entorno = new();
        MotivacionService servicio = entorno.CrearServicio();
        servicio.ReconciliarEstadoActual();
        (ProgresoCurso progreso, TransicionProgresoPersistida transicion) =
            CrearProgresoExacto("practica-a", FechaProceso);
        servicio.ProcesarProgresoPersistido(
            "practica-a",
            progreso,
            transicion);
        entorno.Reloj.AhoraUtc = FechaProceso.AddDays(-5);

        ResumenMotivacion resumen = servicio.ObtenerResumenMotivacion();

        Assert.Equal(1, resumen.Racha.RachaActual);
        Assert.Equal(new DateOnly(2026, 8, 20), resumen.Racha.UltimoDiaEstudio);
        Assert.Contains(
            AdvertenciaMotivacion.RetrocesoRelojDetectado,
            resumen.Advertencias);
    }

    private static (ProgresoCurso, TransicionProgresoPersistida)
        CrearProgresoExacto(string practicaId, DateTimeOffset fecha) {
        ProgresoPractica final = new() {
            PracticaId = practicaId,
            Estado = EstadoPracticaCurso.EnProgreso,
            RutaProyecto = $@"C:\Practicas\{practicaId}",
            FechaCreacion = fecha,
            FechaActualizacion = fecha
        };
        ProgresoCurso progreso = new() {
            Practicas = new List<ProgresoPractica> { final }
        };
        TransicionProgresoPersistida transicion = new() {
            PracticaId = practicaId,
            ProgresoFinal = final,
            PracticaCreada = true,
            VinculoPersistidoAhora = true,
            RealizadaPersistidaAhora = false
        };
        return (progreso, transicion);
    }

    private static (HistorialPractica, TransicionEvaluacionPersistida)
        CrearEvaluacionExacta(
            string practicaId,
            DateTimeOffset fecha,
            int calificacion) {
        IntentoPractica intento = new() {
            Id = $"intento-{practicaId}",
            PracticaId = practicaId,
            Fecha = fecha,
            Calificacion = calificacion,
            ResultadoGeneral = "Resultado",
            PuntosMaximos = 100,
            RutaProyecto = $@"C:\Practicas\{practicaId}"
        };
        HistorialPractica historial = new() {
            PracticaId = practicaId,
            TotalIntentos = 1,
            MejorCalificacion = calificacion,
            UltimaCalificacion = calificacion,
            FechaUltimoIntento = fecha,
            Intentos = new[] { intento }
        };
        TransicionEvaluacionPersistida transicion = new() {
            PracticaId = practicaId,
            IntentoId = intento.Id,
            FechaIntento = fecha,
            CalificacionIntento = calificacion,
            MejorCalificacionPosterior = calificacion,
            TotalIntentos = 1,
            IntentoPublicado = true
        };
        return (historial, transicion);
    }

    private static TransicionEvaluacionPersistida CopiarTransicion(
        TransicionEvaluacionPersistida origen,
        DateTimeOffset fechaIntento) {
        return new TransicionEvaluacionPersistida {
            PracticaId = origen.PracticaId,
            IntentoId = origen.IntentoId,
            FechaIntento = fechaIntento,
            CalificacionIntento = origen.CalificacionIntento,
            MejorCalificacionAnterior = origen.MejorCalificacionAnterior,
            UltimaCalificacionAnterior = origen.UltimaCalificacionAnterior,
            FechaUltimoIntentoAnterior = origen.FechaUltimoIntentoAnterior,
            MejorCalificacionPosterior = origen.MejorCalificacionPosterior,
            TotalIntentos = origen.TotalIntentos,
            IntentoPublicado = origen.IntentoPublicado
        };
    }

    private static DateOnly[] LeerDiasActividad(string ruta) {
        using JsonDocument documento = JsonDocument.Parse(File.ReadAllText(ruta));
        return documento.RootElement
            .GetProperty("DiasActividadAcademica")
            .EnumerateArray()
            .Select(item => DateOnly.Parse(
                item.GetString()!,
                CultureInfo.InvariantCulture))
            .OrderBy(item => item)
            .ToArray();
    }

    private static IReadOnlyList<GradoCurso> CrearCatalogo() {
        PracticaCurso[] practicas = new[] {
            "practica-a",
            "practica-b",
            "practica-c",
            "practica-d"
        }.Select((id, indice) => new PracticaCurso {
            Id = id,
            TemaId = "tema-prueba",
            Numero = indice + 1,
            Nombre = id
        }).ToArray();
        TemaCurso tema = new() {
            Id = "tema-prueba",
            Numero = 1,
            Nombre = "Tema",
            Practicas = practicas
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

    private sealed class EntornoActividad : IDisposable {
        public EntornoActividad(DateTimeOffset? ahoraUtc = null) {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-Motivacion-Actividad-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
            Reloj = new TimeProviderMutable(ahoraUtc ?? FechaProceso);
        }

        public string Carpeta { get; }

        public TimeProviderMutable Reloj { get; }

        public ResultadoCargaProgreso Progreso { get; set; } = new() {
            Estado = EstadoCargaProgreso.Exitosa,
            Progreso = new ProgresoCurso()
        };

        public ResultadoCargaHistorialEvaluaciones Historial { get; set; } = new() {
            Estado = EstadoCargaHistorialEvaluaciones.Exitosa,
            Historial = new HistorialEvaluaciones()
        };

        public MotivacionService CrearServicio() {
            return new MotivacionService(
                Carpeta,
                Reloj,
                CrearCatalogo,
                () => Progreso,
                () => Historial);
        }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }
    }

    private sealed class TimeProviderMutable : TimeProvider {
        public TimeProviderMutable(DateTimeOffset ahoraUtc) {
            AhoraUtc = ahoraUtc;
        }

        public DateTimeOffset AhoraUtc { get; set; }

        public override DateTimeOffset GetUtcNow() => AhoraUtc;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
