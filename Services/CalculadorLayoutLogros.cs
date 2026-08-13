namespace EndForge.Services;

/// <summary>
/// Rectangulo expresado en unidades logicas a 96 DPI.
/// </summary>
public readonly record struct RectanguloLayoutLogro(
    int X,
    int Y,
    int Ancho,
    int Alto) {
    public int Derecha => X + Ancho;

    public int Inferior => Y + Alto;
}

/// <summary>
/// Geometria inmutable de la lista de logros en unidades logicas a 96 DPI.
/// </summary>
public sealed record MedidasLayoutLogros(
    bool ModoAmplio,
    int AnchoViewport,
    int Margen,
    int AnchoContenido,
    int XContenido,
    int Columnas,
    int Filas,
    int Separacion,
    int AltoTarjeta,
    int AltoContenido,
    int AltoTotal,
    int CantidadLogros) {
    public RectanguloLayoutLogro ObtenerRectanguloTarjeta(int indice) {
        if (indice < 0 || indice >= CantidadLogros) {
            throw new ArgumentOutOfRangeException(nameof(indice));
        }

        int fila = indice / Columnas;
        int columna = indice % Columnas;
        int anchoBase = Math.Max(
            1,
            (AnchoContenido - Separacion * (Columnas - 1)) / Columnas);
        int xRelativo = columna * (anchoBase + Separacion);
        int ancho = columna == Columnas - 1
            ? Math.Max(1, AnchoContenido - xRelativo)
            : anchoBase;

        return new RectanguloLayoutLogro(
            XContenido + xRelativo,
            Margen + fila * (AltoTarjeta + Separacion),
            ancho,
            AltoTarjeta);
    }
}

/// <summary>
/// Calcula la distribucion de la vista Logros sin depender de WinForms ni del
/// escalado fisico del monitor.
/// </summary>
public static class CalculadorLayoutLogros {
    public const int AnchoMinimoModoAmplio = 820;
    public const int AnchoMaximoContenido = 1320;
    public const int MargenAmplio = 24;
    public const int MargenCompacto = 12;
    public const int SeparacionTarjetas = 12;
    public const int AltoTarjetaAmplia = 112;
    public const int AltoTarjetaCompacta = 124;

    public static bool DeterminarModoAmplio(int anchoViewportLogico) {
        return anchoViewportLogico >= AnchoMinimoModoAmplio;
    }

    public static MedidasLayoutLogros Calcular(
        int anchoViewportLogico,
        int cantidadLogros) {
        int anchoViewport = Math.Max(1, anchoViewportLogico);
        int cantidad = Math.Max(0, cantidadLogros);
        bool modoAmplio = DeterminarModoAmplio(anchoViewport);
        int margen = modoAmplio ? MargenAmplio : MargenCompacto;
        int anchoDisponible = Math.Max(1, anchoViewport - margen * 2);
        int anchoContenido = Math.Min(
            AnchoMaximoContenido,
            anchoDisponible);
        int xContenido = Math.Max(0, (anchoViewport - anchoContenido) / 2);
        int columnas = modoAmplio ? 2 : 1;
        int filas = cantidad == 0
            ? 0
            : (cantidad + columnas - 1) / columnas;
        int altoTarjeta = modoAmplio
            ? AltoTarjetaAmplia
            : AltoTarjetaCompacta;
        int altoContenido = filas == 0
            ? 0
            : filas * altoTarjeta + (filas - 1) * SeparacionTarjetas;
        int altoTotal = margen + altoContenido + margen;

        return new MedidasLayoutLogros(
            modoAmplio,
            anchoViewport,
            margen,
            anchoContenido,
            xContenido,
            columnas,
            filas,
            SeparacionTarjetas,
            altoTarjeta,
            altoContenido,
            altoTotal,
            cantidad);
    }
}
