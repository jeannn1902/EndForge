using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class PresentadorInicioMotivacionTests {
    [Fact]
    public void SinXp_MuestraNivelUnoYProgresoDisponible() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumenAcademico(),
            CrearResumenMotivacion(
                EstadoDisponibilidadMotivacion.SinActividad,
                new ResumenNivel(0, 1, 0, 150, 0, 150, 0)));

        Assert.Equal(EstadoNivelInicio.Disponible, presentacion.Nivel.Estado);
        Assert.Equal("Nivel 1", presentacion.Nivel.TextoNivel);
        Assert.Equal("0 XP", presentacion.Nivel.TextoXpTotal);
        Assert.Equal(
            "150 XP para el siguiente nivel",
            presentacion.Nivel.TextoXpRestante);
        Assert.Equal(0, presentacion.Nivel.ValorBarra);
        Assert.Equal(4, presentacion.Metricas.Count);
        Assert.Equal("0 días", presentacion.Motivacion.Racha.TextoValor);
        Assert.Equal("0 / 14", presentacion.Motivacion.Logros.TextoValor);
    }

    [Fact]
    public void RachaYLogrosDisponibles_SeIntegranSinRecalcularNegocio() {
        ResumenMotivacion motivacion = CrearResumenMotivacion(
            EstadoDisponibilidadMotivacion.Disponible,
            new ResumenNivel(150, 2, 150, 450, 0, 300, 0)) with {
            Racha = new ResumenRacha(4, 8, new DateOnly(2026, 8, 9)),
            LogrosDesbloqueados = new[] {
                new LogroDesbloqueado {
                    LogroId =
                        CatalogoLogrosService.PrimeraPracticaVinculadaId,
                    FechaReconocimientoUtc = new DateTimeOffset(
                        2026,
                        8,
                        9,
                        12,
                        0,
                        0,
                        TimeSpan.Zero),
                    EsImportado = false
                }
            }
        };

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumenAcademico(),
            motivacion);

        Assert.Equal("4 días de racha", presentacion.Motivacion.Racha.TextoValor);
        Assert.Equal(
            "Mejor racha: 8 días",
            presentacion.Motivacion.Racha.TextoDetalle);
        Assert.Equal(4, presentacion.Motivacion.Racha.RachaActual);
        Assert.Equal(8, presentacion.Motivacion.Racha.MejorRacha);
        Assert.Equal("1 / 14", presentacion.Motivacion.Logros.TextoValor);
        Assert.Equal(1, presentacion.Motivacion.Logros.LogrosDesbloqueados);
    }

    [Fact]
    public void NivelIntermedio_UsaResumenCalculadoSinDuplicarFormula() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumenAcademico(),
            CrearResumenMotivacion(
                EstadoDisponibilidadMotivacion.Disponible,
                new ResumenNivel(
                    520,
                    3,
                    450,
                    900,
                    70,
                    380,
                    70m * 100m / 450m)));

        Assert.Equal("Nivel 3", presentacion.Nivel.TextoNivel);
        Assert.Equal("520 XP", presentacion.Nivel.TextoXpTotal);
        Assert.Equal(
            "380 XP para el siguiente nivel",
            presentacion.Nivel.TextoXpRestante);
        Assert.Equal(16, presentacion.Nivel.ValorBarra);
    }

    [Fact]
    public void MotivacionNoDisponible_NoMuestraCeroFalso() {
        ResumenMotivacion motivacion = new(
            EstadoDisponibilidadMotivacion.NoDisponible,
            null,
            null,
            string.Empty,
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            new IOException("Prueba"));

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumenAcademico(),
            motivacion);

        Assert.Equal(
            EstadoNivelInicio.NoDisponible,
            presentacion.Nivel.Estado);
        Assert.Equal("No disponible", presentacion.Nivel.TextoNivel);
        Assert.DoesNotContain("0 XP", presentacion.Nivel.TextoXpTotal);
        Assert.Null(presentacion.Nivel.ValorBarra);
        Assert.Equal(4, presentacion.Metricas.Count);
        Assert.Equal(
            EstadoMetricaMotivacionalInicio.NoDisponible,
            presentacion.Motivacion.Racha.Estado);
        Assert.Equal("—", presentacion.Motivacion.Logros.TextoValor);
    }

    [Fact]
    public void VersionIncompatible_ConservaDashboardAcademico() {
        ResumenMotivacion motivacion = new(
            EstadoDisponibilidadMotivacion.VersionIncompatible,
            null,
            null,
            string.Empty,
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            null);

        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumenAcademico(),
            motivacion);

        Assert.Equal(
            EstadoNivelInicio.VersionIncompatible,
            presentacion.Nivel.Estado);
        Assert.Equal("No disponible", presentacion.Nivel.TextoNivel);
        Assert.Contains(
            "otra versión",
            presentacion.Nivel.TextoXpRestante,
            StringComparison.OrdinalIgnoreCase);
        Assert.Null(presentacion.Nivel.ValorBarra);
        Assert.Equal("0 de 60", presentacion.Progreso.PracticasRealizadas.Texto);
        Assert.Equal(4, presentacion.Metricas.Count);
        Assert.Equal(
            EstadoMetricaMotivacionalInicio.VersionIncompatible,
            presentacion.Motivacion.Racha.Estado);
        Assert.Equal(
            "Disponible con una versión compatible",
            presentacion.Motivacion.Logros.TextoDetalle);
    }

    [Fact]
    public void CrearAnterior_ConservaCompatibilidadSinInventarXp() {
        PresentacionInicio presentacion = CrearPresentador().Crear(
            CrearResumenAcademico());

        Assert.Equal(
            EstadoNivelInicio.NoDisponible,
            presentacion.Nivel.Estado);
        Assert.Null(presentacion.Nivel.ValorBarra);
        Assert.Equal(4, presentacion.Metricas.Count);
        Assert.Equal(
            EstadoMetricaMotivacionalInicio.NoDisponible,
            presentacion.Motivacion.Logros.Estado);
    }

    private static ResumenMotivacion CrearResumenMotivacion(
        EstadoDisponibilidadMotivacion estado,
        ResumenNivel nivel) {
        return new ResumenMotivacion(
            estado,
            nivel.XpTotal,
            nivel,
            "America/Mexico_City",
            null,
            Array.Empty<AdvertenciaMotivacion>(),
            null);
    }

    private static PresentadorInicioService CrearPresentador() {
        return new PresentadorInicioService(new TimeProviderFijo());
    }

    private static ResumenInicio CrearResumenAcademico() {
        return new ResumenInicio(
            EstadoDisponibilidadDatos.SinActividad,
            new ResumenProgresoGlobal(
                60,
                0,
                0,
                60,
                0,
                9,
                0,
                2,
                0,
                Array.Empty<ResumenProgresoGrado>()),
            new ResumenEvaluacionesGlobal(0, 0, 0, null, null),
            null,
            new ContinuacionAprendizaje(
                EstadoContinuacionAprendizaje.SinContenidoDisponible,
                null,
                null,
                EstadoRutaProyectoAprendizaje.SinRutaVinculada,
                null,
                false),
            new RecomendacionAprendizaje(
                EstadoRecomendacionAprendizaje.SinContenidoDisponible,
                null,
                null,
                false),
            CrearFuenteSinDatos(),
            CrearFuenteSinDatos());
    }

    private static EstadoFuenteAprendizaje CrearFuenteSinDatos() {
        return new EstadoFuenteAprendizaje(
            EstadoFuenteDatosAprendizaje.SinDatos,
            "Prueba",
            0,
            0,
            null);
    }

    private sealed class TimeProviderFijo : TimeProvider {
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow() {
            return new DateTimeOffset(
                2026,
                7,
                31,
                10,
                0,
                0,
                TimeSpan.Zero);
        }
    }
}
