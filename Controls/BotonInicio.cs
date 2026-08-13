namespace EndForge.Controls;

internal sealed class BotonInicio : Button {
    private bool cursorEncima;

    public Color ColorBordeNormal { get; set; } = Color.FromArgb(91, 72, 116);

    public Color ColorBordeHover { get; set; } = Color.FromArgb(174, 108, 232);

    public Color ColorBordeFoco { get; set; } = Color.FromArgb(220, 176, 255);

    public BotonInicio() {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable,
            true);
        DoubleBuffered = true;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        TabStop = true;
    }

    protected override void OnMouseEnter(EventArgs e) {
        cursorEncima = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e) {
        cursorEncima = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        Invalidate();
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e) {
        Invalidate();
        base.OnLostFocus(e);
    }

    protected override void OnEnabledChanged(EventArgs e) {
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);

        if (ClientSize.Width <= 2 || ClientSize.Height <= 2) {
            return;
        }

        Color color = Focused
            ? ColorBordeFoco
            : cursorEncima && Enabled
                ? ColorBordeHover
                : ColorBordeNormal;
        float grosor = Focused ? 2F : 1F;
        Rectangle contorno = new(
            1,
            1,
            ClientSize.Width - 3,
            ClientSize.Height - 3);
        using Pen lapiz = new(color, grosor);
        e.Graphics.DrawRectangle(lapiz, contorno);
    }
}
