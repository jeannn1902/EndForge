using EndForge.Models;

namespace EndForge.Services;

public sealed class CalculadoraRachaService {
    private readonly TimeProvider reloj;

    public CalculadoraRachaService()
        : this(TimeProvider.System) {
    }

    public CalculadoraRachaService(TimeProvider reloj) {
        this.reloj = reloj ?? throw new ArgumentNullException(nameof(reloj));
    }

    public ResumenRacha Calcular(
        IEnumerable<DateOnly> dias,
        TimeZoneInfo zonaHoraria) {
        return Calcular(dias, zonaHoraria, reloj.GetUtcNow());
    }

    public ResumenRacha Calcular(
        IEnumerable<DateOnly> dias,
        TimeZoneInfo zonaHoraria,
        DateTimeOffset instanteUtc) {
        ArgumentNullException.ThrowIfNull(dias);
        ArgumentNullException.ThrowIfNull(zonaHoraria);

        var diasOrdenados = new SortedSet<DateOnly>(dias);
        if (diasOrdenados.Count == 0) {
            return new ResumenRacha(0, 0, null);
        }

        int mejorRacha = 0;
        int rachaTerminal = 0;
        DateOnly? diaAnterior = null;

        foreach (DateOnly dia in diasOrdenados) {
            rachaTerminal = diaAnterior is not null &&
                dia.DayNumber == diaAnterior.Value.DayNumber + 1
                ? rachaTerminal + 1
                : 1;

            mejorRacha = Math.Max(mejorRacha, rachaTerminal);
            diaAnterior = dia;
        }

        DateOnly ultimoDia = diasOrdenados.Max;
        DateOnly hoy = ObtenerDiaLocal(instanteUtc, zonaHoraria);
        int diasDesdeUltimaActividad = hoy.DayNumber - ultimoDia.DayNumber;
        int rachaActual = diasDesdeUltimaActividad is 0 or 1
            ? rachaTerminal
            : 0;

        return new ResumenRacha(rachaActual, mejorRacha, ultimoDia);
    }

    private static DateOnly ObtenerDiaLocal(
        DateTimeOffset instanteUtc,
        TimeZoneInfo zonaHoraria) {
        DateTimeOffset instanteLocal = TimeZoneInfo.ConvertTime(
            instanteUtc.ToUniversalTime(),
            zonaHoraria);

        return DateOnly.FromDateTime(instanteLocal.DateTime);
    }
}
