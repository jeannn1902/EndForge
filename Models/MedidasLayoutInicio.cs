namespace EndForge.Models;

/// <summary>
/// Rectángulo en unidades lógicas a 96 DPI. Permite comprobar la
/// composición de Inicio sin depender de controles WinForms.
/// </summary>
public readonly record struct RectanguloLayoutInicio(
    int X,
    int Y,
    int Ancho,
    int Alto) {
    public int Derecha => X + Ancho;
    public int Inferior => Y + Alto;
}

/// <summary>
/// Medidas en unidades lógicas a 96 DPI para distribuir Inicio.
/// </summary>
public sealed record MedidasLayoutInicio(
    bool ModoAmplio,
    int AnchoContenido,
    int Separacion,
    int AltoEncabezado,
    int AltoContinuacion,
    int AltoProgreso,
    int AltoFilaPrincipal,
    int ColumnasMetricas,
    int FilasMetricas,
    int AltoTarjetaMetrica,
    int AltoMetricas,
    int AltoRecomendacion,
    int AltoActividad,
    int AltoFilaInferior,
    RectanguloLayoutInicio Continuacion,
    RectanguloLayoutInicio Progreso,
    RectanguloLayoutInicio Actividad,
    RectanguloLayoutInicio Recomendacion);
