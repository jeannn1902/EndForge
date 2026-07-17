namespace EndForge.Controls;

internal sealed class TarjetaCursoInteractiva : Panel {
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
            OnClick(EventArgs.Empty);
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
}
