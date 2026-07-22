using System.Drawing.Drawing2D;

namespace EndForge.Controls;

internal sealed class TarjetaCursoInteractiva : Panel {
    private bool estaEnHover;

    public event EventHandler? ActivadaPorTeclado;

    public bool EstaEnHover {
        get => estaEnHover;
        set {
            if (estaEnHover == value) {
                return;
            }

            estaEnHover = value;
            Invalidate();
        }
    }

    public int RadioEsquinas { get; set; } = 16;

    public Color ColorBordeNormal { get; set; } = Color.FromArgb(116, 67, 163);

    public Color ColorBordeHover { get; set; } = Color.FromArgb(174, 108, 232);

    public Color ColorBordeFoco { get; set; } = Color.FromArgb(202, 151, 247);

    public TarjetaCursoInteractiva() {
        SetStyle(
            ControlStyles.Selectable |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);

        TabStop = true;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        if (e.Button == MouseButtons.Left && CanFocus) {
            Focus();
        }

        base.OnMouseDown(e);
    }

    protected override bool IsInputKey(Keys keyData) {
        return (keyData & Keys.KeyCode) switch {
            Keys.Up or Keys.Down or Keys.Enter or Keys.Space => true,
            _ => base.IsInputKey(keyData)
        };
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        if (e.KeyCode is Keys.Enter or Keys.Space) {
            ActivadaPorTeclado?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        if (e.KeyCode is Keys.Up or Keys.Down && MoverFoco(e.KeyCode == Keys.Down)) {
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnEnter(EventArgs e) {
        base.OnEnter(e);
        Invalidate();
        AsegurarVisibilidadEnContenedores();
    }

    protected override void OnLeave(EventArgs e) {
        base.OnLeave(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);

        if (ClientSize.Width <= 3 || ClientSize.Height <= 3) {
            return;
        }

        float escalaDpi = Math.Max(1F, DeviceDpi / 96F);
        Rectangle contornoExterior = new(1, 1, Width - 3, Height - 3);
        SmoothingMode suavizadoAnterior = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        if (ContainsFocus) {
            DibujarBordeResaltado(
                e.Graphics,
                contornoExterior,
                ColorBordeFoco,
                escalaDpi,
                anchoPrincipal: 2.05F,
                alfaResplandor: 58,
                alfaInterior: 90);
        } else if (EstaEnHover) {
            DibujarBordeResaltado(
                e.Graphics,
                contornoExterior,
                ColorBordeHover,
                escalaDpi,
                anchoPrincipal: 1.55F,
                alfaResplandor: 46,
                alfaInterior: 68);
        } else {
            using GraphicsPath contorno = CrearContornoRedondeado(
                contornoExterior,
                RadioEsquinas);
            using Pen borde = new(ColorBordeNormal, 1.2F * escalaDpi);
            e.Graphics.DrawPath(borde, contorno);
        }

        e.Graphics.SmoothingMode = suavizadoAnterior;
    }

    private void DibujarBordeResaltado(
        Graphics graphics,
        Rectangle contornoExterior,
        Color color,
        float escalaDpi,
        float anchoPrincipal,
        int alfaResplandor,
        int alfaInterior) {
        using GraphicsPath exterior = CrearContornoRedondeado(
            contornoExterior,
            RadioEsquinas);
        using Pen resplandor = new(
            Color.FromArgb(alfaResplandor, color),
            3.2F * escalaDpi);
        using Pen bordePrincipal = new(
            Color.FromArgb(235, color),
            anchoPrincipal * escalaDpi);
        graphics.DrawPath(resplandor, exterior);
        graphics.DrawPath(bordePrincipal, exterior);

        int margenInterior = Math.Max(3, (int)Math.Round(3F * escalaDpi));
        Rectangle contornoInterior = new(
            margenInterior,
            margenInterior,
            Math.Max(1, ClientSize.Width - margenInterior * 2 - 1),
            Math.Max(1, ClientSize.Height - margenInterior * 2 - 1));
        int radioInterior = Math.Max(1, RadioEsquinas - margenInterior + 1);

        using GraphicsPath interior = CrearContornoRedondeado(
            contornoInterior,
            radioInterior);
        using Pen bordeInterior = new(
            Color.FromArgb(alfaInterior, color),
            0.8F * escalaDpi);
        graphics.DrawPath(bordeInterior, interior);
    }

    private bool MoverFoco(bool haciaAdelante) {
        if (Parent is null) {
            return false;
        }

        TarjetaCursoInteractiva[] tarjetas = Parent.Controls
            .OfType<TarjetaCursoInteractiva>()
            .Where(tarjeta => tarjeta.Visible && tarjeta.Enabled && tarjeta.TabStop)
            .OrderBy(tarjeta => tarjeta.TabIndex)
            .ToArray();
        int indice = Array.IndexOf(tarjetas, this);

        if (indice < 0) {
            return false;
        }

        int siguiente = haciaAdelante ? indice + 1 : indice - 1;

        if (siguiente < 0 || siguiente >= tarjetas.Length) {
            return false;
        }

        tarjetas[siguiente].Select();
        return true;
    }

    private void AsegurarVisibilidadEnContenedores() {
        for (Control? actual = Parent; actual is not null; actual = actual.Parent) {
            if (actual is PanelDesplazableSinBarras desplazamiento) {
                desplazamiento.AsegurarVisible(this);
            }
        }
    }

    private static GraphicsPath CrearContornoRedondeado(Rectangle limites, int radio) {
        int radioSeguro = Math.Max(
            1,
            Math.Min(radio, Math.Min(limites.Width, limites.Height) / 2));
        int diametro = radioSeguro * 2;
        GraphicsPath contorno = new();
        contorno.AddArc(limites.Left, limites.Top, diametro, diametro, 180, 90);
        contorno.AddArc(limites.Right - diametro, limites.Top, diametro, diametro, 270, 90);
        contorno.AddArc(
            limites.Right - diametro,
            limites.Bottom - diametro,
            diametro,
            diametro,
            0,
            90);
        contorno.AddArc(limites.Left, limites.Bottom - diametro, diametro, diametro, 90, 90);
        contorno.CloseFigure();
        return contorno;
    }
}
