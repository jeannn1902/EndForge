using System.Drawing;

namespace EndForge.Services;

internal static class CalculadorGeometriaNavegacionCurso {
    internal static Rectangle CalcularPanelPrincipalConMenu(
        Rectangle areaCliente,
        int limiteSuperior,
        int limiteIzquierdo,
        int margen) {
        int izquierdaDisponible = Math.Clamp(
            limiteIzquierdo,
            areaCliente.Left,
            areaCliente.Right);
        int superiorDisponible = Math.Clamp(
            limiteSuperior,
            areaCliente.Top,
            areaCliente.Bottom);
        int anchoDisponible = Math.Max(
            1,
            areaCliente.Right - izquierdaDisponible);
        int altoDisponible = Math.Max(
            1,
            areaCliente.Bottom - superiorDisponible);
        int margenHorizontal = Math.Min(
            Math.Max(0, margen),
            Math.Max(0, (anchoDisponible - 1) / 2));
        int margenVertical = Math.Min(
            Math.Max(0, margen),
            Math.Max(0, (altoDisponible - 1) / 2));

        return new Rectangle(
            izquierdaDisponible + margenHorizontal,
            superiorDisponible + margenVertical,
            Math.Max(1, anchoDisponible - margenHorizontal * 2),
            Math.Max(1, altoDisponible - margenVertical * 2));
    }

    internal static int CalcularAnchoContenidoCurricular(
        int anchoVista,
        int margenHorizontal,
        int anchoMaximo) {
        int anchoDisponible = Math.Max(
            1,
            anchoVista - Math.Max(0, margenHorizontal) * 2);

        return Math.Min(anchoDisponible, Math.Max(1, anchoMaximo));
    }
}
