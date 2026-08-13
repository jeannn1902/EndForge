using EndForge.Models;

namespace EndForge.Services;

/// <summary>
/// Calcula las alturas lógicas del dashboard sin depender de controles
/// WinForms ni de la resolución real del equipo.
/// </summary>
public static class CalculadorLayoutInicio {
    public const int MaximoActividadesVisibles = 3;
    public const int AnchoMinimoModoAmplio = 820;

    public static bool DeterminarModoAmplio(int anchoContenidoLogico) {
        return anchoContenidoLogico >= AnchoMinimoModoAmplio;
    }

    internal static RectanguloLayoutInicio EscalarRectanguloFisico(
        RectanguloLayoutInicio rectangulo,
        int anchoReal,
        int anchoLogico,
        int dpi) {
        if (anchoReal <= 0) {
            throw new ArgumentOutOfRangeException(nameof(anchoReal));
        }

        if (anchoLogico <= 0) {
            throw new ArgumentOutOfRangeException(nameof(anchoLogico));
        }

        if (dpi <= 0) {
            throw new ArgumentOutOfRangeException(nameof(dpi));
        }

        int x = rectangulo.X == 0
            ? 0
            : EscalarMedida(rectangulo.X, dpi);
        int y = rectangulo.Y == 0
            ? 0
            : EscalarMedida(rectangulo.Y, dpi);
        int derecha = rectangulo.Derecha >= anchoLogico
            ? anchoReal
            : EscalarMedida(rectangulo.Derecha, dpi);

        return new RectanguloLayoutInicio(
            x,
            y,
            Math.Max(1, derecha - x),
            EscalarMedida(rectangulo.Alto, dpi));
    }

    public static MedidasLayoutInicio Calcular(
        bool modoAmplio,
        int anchoContenidoLogico,
        int altoViewportLogico,
        int cantidadActividades) {
        int anchoContenido = Math.Max(1, anchoContenidoLogico);
        _ = Math.Max(0, altoViewportLogico);

        int actividades = Math.Clamp(
            cantidadActividades,
            0,
            MaximoActividadesVisibles);
        const int separacion = 12;
        const int altoContinuacionCompacto = 220;
        const int altoProgreso =
            CalculadorLayoutFranjaMotivacionInicio.AltoPanelLogico;
        int altoContinuacion = modoAmplio
            ? altoProgreso
            : altoContinuacionCompacto;
        int altoActividad = actividades switch {
            0 => 96,
            _ => 100 + (actividades - 1) * 54
        };

        RectanguloLayoutInicio continuacion;
        RectanguloLayoutInicio progreso;
        RectanguloLayoutInicio actividad;
        int altoFilaPrincipal;

        if (modoAmplio) {
            int anchoContinuacion = Math.Max(
                520,
                (int)Math.Round((anchoContenido - separacion) * 0.64D));
            anchoContinuacion = Math.Min(
                Math.Max(1, anchoContenido - separacion - 280),
                anchoContinuacion);
            int anchoSecundario = Math.Max(
                1,
                anchoContenido - anchoContinuacion - separacion);
            int xSecundario = anchoContinuacion + separacion;

            continuacion = new RectanguloLayoutInicio(
                0,
                0,
                anchoContinuacion,
                altoContinuacion);
            progreso = new RectanguloLayoutInicio(
                xSecundario,
                0,
                anchoSecundario,
                altoProgreso);
            int yActividad = Math.Max(
                continuacion.Inferior,
                progreso.Inferior) + separacion;
            actividad = new RectanguloLayoutInicio(
                0,
                yActividad,
                anchoContenido,
                altoActividad);
            altoFilaPrincipal = actividad.Inferior;
        } else {
            continuacion = new RectanguloLayoutInicio(
                0,
                0,
                anchoContenido,
                altoContinuacion);
            progreso = new RectanguloLayoutInicio(
                0,
                continuacion.Inferior + separacion,
                anchoContenido,
                altoProgreso);
            actividad = new RectanguloLayoutInicio(
                0,
                progreso.Inferior + separacion,
                anchoContenido,
                altoActividad);
            altoFilaPrincipal = actividad.Inferior;
        }

        int columnasMetricas = modoAmplio ? 4 : 2;
        int filasMetricas = modoAmplio ? 1 : 2;
        const int altoTarjetaMetrica = 104;
        int altoMetricas = altoTarjetaMetrica * filasMetricas +
            separacion * (filasMetricas - 1);

        int altoRecomendacion = modoAmplio ? 184 : 216;
        RectanguloLayoutInicio recomendacion = new(
            0,
            0,
            anchoContenido,
            altoRecomendacion);
        int altoFilaInferior = altoRecomendacion;

        return new MedidasLayoutInicio(
            modoAmplio,
            anchoContenido,
            separacion,
            AltoEncabezado: 78,
            altoContinuacion,
            altoProgreso,
            altoFilaPrincipal,
            columnasMetricas,
            filasMetricas,
            altoTarjetaMetrica,
            altoMetricas,
            altoRecomendacion,
            altoActividad,
            altoFilaInferior,
            continuacion,
            progreso,
            actividad,
            recomendacion);
    }

    private static int EscalarMedida(int valor, int dpi) {
        return Math.Max(1, (int)Math.Round(valor * dpi / 96D));
    }
}
