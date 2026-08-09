using EndForge.Services;

namespace EndForge.Tests;

public sealed class CalculadoraRachaServiceTests {
    private static readonly DateTimeOffset InstanteBase =
        new(2026, 8, 8, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly HoyBase = new(2026, 8, 8);

    [Fact]
    public void Calcular_SinDias_DevuelveResumenVacio() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(Array.Empty<DateOnly>(), TimeZoneInfo.Utc);

        Assert.Equal(0, resultado.RachaActual);
        Assert.Equal(0, resultado.MejorRachaHistorica);
        Assert.Null(resultado.UltimoDiaEstudio);
    }

    [Fact]
    public void Calcular_ActividadesRepetidasElMismoDia_CuentanUnaVez() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { HoyBase, HoyBase, HoyBase },
            TimeZoneInfo.Utc);

        Assert.Equal(1, resultado.RachaActual);
        Assert.Equal(1, resultado.MejorRachaHistorica);
        Assert.Equal(HoyBase, resultado.UltimoDiaEstudio);
    }

    [Fact]
    public void Calcular_DiasConsecutivos_CalculaRachaCompleta() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { HoyBase.AddDays(-2), HoyBase.AddDays(-1), HoyBase },
            TimeZoneInfo.Utc);

        Assert.Equal(3, resultado.RachaActual);
        Assert.Equal(3, resultado.MejorRachaHistorica);
        Assert.Equal(HoyBase, resultado.UltimoDiaEstudio);
    }

    [Fact]
    public void Calcular_ConSalto_ReiniciaLaRachaTerminal() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { HoyBase.AddDays(-5), HoyBase.AddDays(-4), HoyBase },
            TimeZoneInfo.Utc);

        Assert.Equal(1, resultado.RachaActual);
        Assert.Equal(2, resultado.MejorRachaHistorica);
    }

    [Fact]
    public void Calcular_ConMejorRachaPrevia_ConservaElMaximo() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] {
                HoyBase.AddDays(-10),
                HoyBase.AddDays(-9),
                HoyBase.AddDays(-8),
                HoyBase.AddDays(-1)
            },
            TimeZoneInfo.Utc);

        Assert.Equal(1, resultado.RachaActual);
        Assert.Equal(3, resultado.MejorRachaHistorica);
    }

    [Fact]
    public void Calcular_DiasFueraDeOrden_LosNormalizaSinAlterarElResultado() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] {
                HoyBase,
                HoyBase.AddDays(-2),
                HoyBase.AddDays(-1),
                HoyBase.AddDays(-1)
            },
            TimeZoneInfo.Utc);

        Assert.Equal(3, resultado.RachaActual);
        Assert.Equal(3, resultado.MejorRachaHistorica);
        Assert.Equal(HoyBase, resultado.UltimoDiaEstudio);
    }

    [Fact]
    public void Calcular_UltimaActividadAyer_ConservaLaRachaActual() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { HoyBase.AddDays(-2), HoyBase.AddDays(-1) },
            TimeZoneInfo.Utc);

        Assert.Equal(2, resultado.RachaActual);
        Assert.Equal(2, resultado.MejorRachaHistorica);
    }

    [Fact]
    public void Calcular_UltimaActividadAnteriorAAyer_DevuelveRachaActualCero() {
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { HoyBase.AddDays(-4), HoyBase.AddDays(-3) },
            TimeZoneInfo.Utc);

        Assert.Equal(0, resultado.RachaActual);
        Assert.Equal(2, resultado.MejorRachaHistorica);
        Assert.Equal(HoyBase.AddDays(-3), resultado.UltimoDiaEstudio);
    }

    [Fact]
    public void Calcular_InstanteExplicito_UsaLaFechaDeLaZonaNoUtc() {
        TimeZoneInfo zona = TimeZoneInfo.CreateCustomTimeZone(
            "UTC+02-Pruebas",
            TimeSpan.FromHours(2),
            "UTC+02 Pruebas",
            "UTC+02 Pruebas");
        var servicio = CrearServicio();
        var instanteUtc = new DateTimeOffset(
            2026,
            1,
            1,
            23,
            30,
            0,
            TimeSpan.Zero);

        var resultado = servicio.Calcular(
            new[] { new DateOnly(2026, 1, 2) },
            zona,
            instanteUtc);

        Assert.Equal(1, resultado.RachaActual);
        Assert.Equal(new DateOnly(2026, 1, 2), resultado.UltimoDiaEstudio);
    }

    [Fact]
    public void Calcular_EnTransicionDst_UsaElDiaLocalReal() {
        TimeZoneInfo zona = CrearZonaConHorarioDeVerano();
        var servicio = CrearServicio();
        var instanteUtc = new DateTimeOffset(
            2026,
            11,
            1,
            3,
            30,
            0,
            TimeSpan.Zero);

        var resultado = servicio.Calcular(
            new[] { new DateOnly(2026, 11, 1) },
            zona,
            instanteUtc);

        Assert.Equal(0, resultado.RachaActual);
        Assert.Equal(1, resultado.MejorRachaHistorica);
    }

    [Fact]
    public void Calcular_InicioDstDeVeintitresHoras_UsaDiasConsecutivos() {
        TimeZoneInfo zona = CrearZonaConHorarioDeVerano();
        DateTime utcDiaAnterior = TimeZoneInfo.ConvertTimeToUtc(
            new DateTime(2026, 3, 7, 12, 0, 0, DateTimeKind.Unspecified),
            zona);
        DateTime utcDiaActual = TimeZoneInfo.ConvertTimeToUtc(
            new DateTime(2026, 3, 8, 12, 0, 0, DateTimeKind.Unspecified),
            zona);
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { new DateOnly(2026, 3, 7), new DateOnly(2026, 3, 8) },
            zona,
            new DateTimeOffset(utcDiaActual, TimeSpan.Zero));

        Assert.Equal(TimeSpan.FromHours(23), utcDiaActual - utcDiaAnterior);
        Assert.Equal(2, resultado.RachaActual);
        Assert.Equal(2, resultado.MejorRachaHistorica);
    }

    [Fact]
    public void Calcular_FinDstDeVeinticincoHoras_UsaDiasConsecutivos() {
        TimeZoneInfo zona = CrearZonaConHorarioDeVerano();
        DateTime utcDiaAnterior = TimeZoneInfo.ConvertTimeToUtc(
            new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Unspecified),
            zona);
        DateTime utcDiaActual = TimeZoneInfo.ConvertTimeToUtc(
            new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Unspecified),
            zona);
        var servicio = CrearServicio();

        var resultado = servicio.Calcular(
            new[] { new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 1) },
            zona,
            new DateTimeOffset(utcDiaActual, TimeSpan.Zero));

        Assert.Equal(TimeSpan.FromHours(25), utcDiaActual - utcDiaAnterior);
        Assert.Equal(2, resultado.RachaActual);
        Assert.Equal(2, resultado.MejorRachaHistorica);
    }

    [Fact]
    public void Calcular_ArgumentosNulos_SeRechazan() {
        var servicio = CrearServicio();

        Assert.Throws<ArgumentNullException>(
            () => servicio.Calcular(null!, TimeZoneInfo.Utc));
        Assert.Throws<ArgumentNullException>(
            () => servicio.Calcular(Array.Empty<DateOnly>(), null!));
    }

    private static CalculadoraRachaService CrearServicio() {
        return new CalculadoraRachaService(new TimeProviderFijo(InstanteBase));
    }

    private static TimeZoneInfo CrearZonaConHorarioDeVerano() {
        TimeZoneInfo.TransitionTime inicio =
            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                new DateTime(1, 1, 1, 2, 0, 0),
                3,
                2,
                DayOfWeek.Sunday);
        TimeZoneInfo.TransitionTime fin =
            TimeZoneInfo.TransitionTime.CreateFloatingDateRule(
                new DateTime(1, 1, 1, 2, 0, 0),
                11,
                1,
                DayOfWeek.Sunday);
        TimeZoneInfo.AdjustmentRule regla =
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                new DateTime(2020, 1, 1),
                new DateTime(2030, 12, 31),
                TimeSpan.FromHours(1),
                inicio,
                fin);

        return TimeZoneInfo.CreateCustomTimeZone(
            "Zona-DST-Pruebas",
            TimeSpan.FromHours(-5),
            "Zona DST Pruebas",
            "Zona estandar Pruebas",
            "Zona verano Pruebas",
            new[] { regla });
    }

    private sealed class TimeProviderFijo : TimeProvider {
        private readonly DateTimeOffset ahora;

        public TimeProviderFijo(DateTimeOffset ahora) {
            this.ahora = ahora;
        }

        public override DateTimeOffset GetUtcNow() {
            return ahora;
        }
    }
}
