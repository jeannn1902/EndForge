using EndForge.Models;

namespace EndForge.Services;

public sealed class CalculadoraNivelService {
    private const decimal FactorNivel = 75m;

    public ResumenNivel Calcular(long xpTotal) {
        if (xpTotal < 0) {
            throw new ArgumentOutOfRangeException(
                nameof(xpTotal),
                "El XP total no puede ser negativo.");
        }

        decimal xp = xpTotal;
        long inferior = 1;
        long superior = 2;

        while (CalcularXpRequerido(superior) <= xp) {
            superior = checked(superior * 2);
        }

        while (inferior + 1 < superior) {
            long medio = inferior + ((superior - inferior) / 2);

            if (CalcularXpRequerido(medio) <= xp) {
                inferior = medio;
            } else {
                superior = medio;
            }
        }

        decimal minimo = CalcularXpRequerido(inferior);
        decimal siguiente = CalcularXpRequerido(inferior + 1);
        decimal dentroNivel = xp - minimo;
        decimal amplitudNivel = siguiente - minimo;
        decimal restante = siguiente - xp;
        decimal porcentaje = amplitudNivel == 0
            ? 0
            : decimal.Clamp(dentroNivel * 100m / amplitudNivel, 0m, 100m);

        return new ResumenNivel(
            xpTotal,
            inferior,
            minimo,
            siguiente,
            dentroNivel,
            restante,
            porcentaje);
    }

    public decimal CalcularXpRequerido(long nivel) {
        if (nivel < 1) {
            throw new ArgumentOutOfRangeException(
                nameof(nivel),
                "El nivel debe ser mayor o igual que uno.");
        }

        try {
            return checked(FactorNivel * (nivel - 1m) * nivel);
        } catch (OverflowException ex) {
            throw new ArgumentOutOfRangeException(
                nameof(nivel),
                nivel,
                $"El nivel excede el rango numérico admitido: {ex.Message}");
        }
    }
}
