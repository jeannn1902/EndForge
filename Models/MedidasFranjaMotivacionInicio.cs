namespace EndForge.Models;

/// <summary>
/// Geometria fisica, ya escalada al DPI indicado, de la franja de racha y
/// logros que se muestra dentro de la tarjeta Progreso de Inicio.
/// </summary>
public sealed record MedidasFranjaMotivacionInicio(
    int AnchoPanel,
    int AltoPanelRequerido,
    int Dpi,
    RectanguloLayoutInicio Separador,
    RectanguloLayoutInicio TituloRacha,
    RectanguloLayoutInicio ValorRacha,
    RectanguloLayoutInicio DetalleRacha,
    RectanguloLayoutInicio TituloLogros,
    RectanguloLayoutInicio ValorLogros,
    RectanguloLayoutInicio BotonVerLogros);
