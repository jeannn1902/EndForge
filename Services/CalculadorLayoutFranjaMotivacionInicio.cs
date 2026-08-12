using EndForge.Models;

namespace EndForge.Services;

/// <summary>
/// Calcula los limites fisicos de la franja motivacional de Inicio sin
/// depender de WinForms ni del estado anterior de los controles.
/// </summary>
public static class CalculadorLayoutFranjaMotivacionInicio {
    public const int DpiBase = 96;
    public const int AltoPanelLogico = 248;

    private const int MargenHorizontalLogico = 18;
    private const int SeparacionColumnasLogica = 8;
    private const int YSeparadorLogico = 161;
    private const int AltoSeparadorLogico = 1;
    private const int YTitulosLogico = 168;
    private const int AltoTitulosLogico = 14;
    private const int YValoresLogico = 185;
    private const int AltoValoresLogico = 19;
    private const int YFilaInferiorLogico = 208;
    private const int AltoDetalleRachaLogico = 16;
    private const int AltoBotonLogico = 32;

    public static MedidasFranjaMotivacionInicio Calcular(
        int anchoPanelFisico,
        int dpi) {
        if (anchoPanelFisico <= 0) {
            throw new ArgumentOutOfRangeException(nameof(anchoPanelFisico));
        }

        if (dpi <= 0) {
            throw new ArgumentOutOfRangeException(nameof(dpi));
        }

        int margen = Escalar(MargenHorizontalLogico, dpi);
        int separacionColumnas = Escalar(SeparacionColumnasLogica, dpi);
        int anchoInterior = anchoPanelFisico - margen * 2;

        if (anchoInterior <= separacionColumnas + 1) {
            throw new ArgumentOutOfRangeException(
                nameof(anchoPanelFisico),
                "El panel no tiene ancho suficiente para las dos columnas.");
        }

        int anchoDisponibleColumnas = anchoInterior - separacionColumnas;
        int anchoRacha = Math.Max(1, anchoDisponibleColumnas * 3 / 5);
        int anchoLogros = anchoDisponibleColumnas - anchoRacha;
        int xLogros = margen + anchoRacha + separacionColumnas;

        int ySeparador = Escalar(YSeparadorLogico, dpi);
        int altoSeparador = Escalar(AltoSeparadorLogico, dpi);
        int yTitulos = Escalar(YTitulosLogico, dpi);
        int altoTitulos = Escalar(AltoTitulosLogico, dpi);
        int yValores = Escalar(YValoresLogico, dpi);
        int altoValores = Escalar(AltoValoresLogico, dpi);
        int yFilaInferior = Escalar(YFilaInferiorLogico, dpi);

        return new MedidasFranjaMotivacionInicio(
            anchoPanelFisico,
            Escalar(AltoPanelLogico, dpi),
            dpi,
            new RectanguloLayoutInicio(
                margen,
                ySeparador,
                anchoInterior,
                altoSeparador),
            new RectanguloLayoutInicio(
                margen,
                yTitulos,
                anchoRacha,
                altoTitulos),
            new RectanguloLayoutInicio(
                margen,
                yValores,
                anchoRacha,
                altoValores),
            new RectanguloLayoutInicio(
                margen,
                yFilaInferior,
                anchoRacha,
                Escalar(AltoDetalleRachaLogico, dpi)),
            new RectanguloLayoutInicio(
                xLogros,
                yTitulos,
                anchoLogros,
                altoTitulos),
            new RectanguloLayoutInicio(
                xLogros,
                yValores,
                anchoLogros,
                altoValores),
            new RectanguloLayoutInicio(
                xLogros,
                yFilaInferior,
                anchoLogros,
                Escalar(AltoBotonLogico, dpi)));
    }

    internal static int Escalar(int valorLogico, int dpi) {
        return Math.Max(
            1,
            (int)Math.Round(valorLogico * dpi / (double)DpiBase));
    }
}
