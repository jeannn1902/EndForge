using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace EndForge.Controls;

internal readonly record struct ResultadoRuedaDesplazamiento(
    int PosicionDestino,
    int AcumuladoRestante);

internal sealed class EstadoDesplazamientoSincrono {
    public int PosicionActual { get; private set; }

    public int MaximoActual { get; private set; }

    public bool AplicacionEnCurso { get; private set; }

    public int OperacionesPendientes => 0;

    public void Restablecer(int posicion, int maximo) {
        MaximoActual = Math.Max(0, maximo);
        PosicionActual = Limitar(posicion, MaximoActual);
    }

    public bool AjustarMaximo(int maximo) {
        if (AplicacionEnCurso) {
            return false;
        }

        MaximoActual = Math.Max(0, maximo);
        int nuevaPosicion = Limitar(PosicionActual, MaximoActual);

        if (nuevaPosicion == PosicionActual) {
            return false;
        }

        PosicionActual = nuevaPosicion;
        return true;
    }

    public bool IntentarMover(
        int destino,
        int maximo,
        Action<int, int> aplicar) {
        ArgumentNullException.ThrowIfNull(aplicar);

        if (AplicacionEnCurso) {
            return false;
        }

        MaximoActual = Math.Max(0, maximo);
        int nuevaPosicion = Limitar(destino, MaximoActual);

        if (nuevaPosicion == PosicionActual) {
            return false;
        }

        int posicionAnterior = PosicionActual;
        PosicionActual = nuevaPosicion;
        AplicacionEnCurso = true;

        try {
            aplicar(posicionAnterior, nuevaPosicion);
            return true;
        } finally {
            AplicacionEnCurso = false;
        }
    }

    private static int Limitar(int posicion, int maximo) {
        return Math.Clamp(posicion, 0, Math.Max(0, maximo));
    }
}

internal sealed class PanelDesplazableSinBarras : Panel {
    private const int DeltaRueda = 120;
    private const int PasoLinea = 40;
    private const int AnchoReservaBarra = 16;
    private const int AnchoPistaBarra = 8;
    private const int MargenVerticalBarra = 4;
    private const int AltoMinimoIndicador = 28;
    private const int MargenRepintadoDesplazamiento = 2;
    private const int DuracionSuavizadoRuedaMs = 750;
    private const int IntervaloAnimacionMs = 15;

    private sealed class ContenedorFlujoDobleBuffer : FlowLayoutPanel {
        public ContenedorFlujoDobleBuffer() {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            DoubleBuffered = true;
        }
    }

    private static readonly Color ColorPistaBarra = Color.FromArgb(31, 25, 45);
    private static readonly Color ColorIndicadorBarra = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorIndicadorBarraHover = Color.FromArgb(174, 108, 232);
    private readonly SolidBrush pincelFondoContenido = new(Color.FromArgb(18, 14, 27));
    private readonly SolidBrush pincelPistaBarra = new(ColorPistaBarra);
    private readonly SolidBrush pincelIndicadorBarra = new(ColorIndicadorBarra);
    private readonly SolidBrush pincelIndicadorBarraHover = new(ColorIndicadorBarraHover);
    private readonly Pen lapizBordeFoco = new(Color.FromArgb(202, 151, 247), 1F) {
        DashStyle = DashStyle.Dot
    };

    private readonly EstadoDesplazamientoSincrono estadoDesplazamiento = new();
    private int acumuladoRueda;
    private bool actualizandoContenido;
    private bool barraVisible;
    private bool indicadorHover;
    private bool arrastrandoIndicador;
    private int desfaseArrastreIndicador;
    private bool eventosContenidoConectados;
    private readonly HashSet<Control> controlesRegistrados = new();
    private readonly System.Windows.Forms.Timer temporizadorAnimacion = new() {
        Interval = IntervaloAnimacionMs
    };
    private readonly Stopwatch relojAnimacion = new();
    private int posicionInicioAnimacion;
    private int posicionDestinoAnimacion;

    public FlowLayoutPanel Contenido { get; }

    public bool MostrarBordeFoco { get; set; } = true;

    internal int PosicionDesplazamientoActual =>
        estadoDesplazamiento.PosicionActual;

    internal int OperacionesDesplazamientoPendientes =>
        estadoDesplazamiento.OperacionesPendientes;

    public Color ColorFondoContenido {
        get => Contenido.BackColor;
        set {
            Contenido.BackColor = value;
            pincelFondoContenido.Color = value;
            Contenido.Invalidate(true);
            InvalidarBarra();
        }
    }

    public PanelDesplazableSinBarras() {
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable,
            true);

        DoubleBuffered = true;
        TabStop = true;
        BackColor = Color.FromArgb(116, 67, 163);
        Padding = new Padding(1);

        Contenido = new ContenedorFlujoDobleBuffer {
            AutoScroll = false,
            BackColor = Color.FromArgb(18, 14, 27),
            FlowDirection = FlowDirection.TopDown,
            Location = new Point(Padding.Left, Padding.Top),
            Margin = Padding.Empty,
            TabStop = false,
            WrapContents = false
        };

        Controls.Add(Contenido);
        ConectarEventosContenido();
        temporizadorAnimacion.Tick += TemporizadorAnimacion_Tick;
    }

    public void ActualizarContenido(bool volverAlInicio) {
        if (actualizandoContenido ||
            IsDisposed ||
            !ReferenceEquals(Contenido.Parent, this)) {
            return;
        }

        actualizandoContenido = true;

        try {
            if (volverAlInicio) {
                estadoDesplazamiento.Restablecer(0, int.MaxValue);
                acumuladoRueda = 0;
            }

            int anchoDisponible = Math.Max(
                1,
                ClientSize.Width - Padding.Horizontal - AnchoReservaBarra);
            int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
            Contenido.SetBounds(
                Padding.Left,
                Padding.Top - estadoDesplazamiento.PosicionActual,
                anchoDisponible,
                Math.Max(1, Contenido.Height));
            Contenido.PerformLayout();

            int altoContenidoReal = Math.Max(
                Contenido.Padding.Vertical,
                Contenido.GetPreferredSize(new Size(anchoDisponible, 0)).Height);
            barraVisible = altoContenidoReal > altoDisponible;
            int altoContenido = Math.Max(altoDisponible, altoContenidoReal);
            int maximo = CalcularMaximoDesplazamiento(
                altoContenidoReal,
                altoDisponible);
            estadoDesplazamiento.AjustarMaximo(maximo);

            if (!barraVisible) {
                indicadorHover = false;
                arrastrandoIndicador = false;
                desfaseArrastreIndicador = 0;
                Cursor = Cursors.Default;

                if (Capture) {
                    Capture = false;
                }
            }

            Contenido.SetBounds(
                Padding.Left,
                Padding.Top - estadoDesplazamiento.PosicionActual,
                anchoDisponible,
                altoContenido);
            Contenido.PerformLayout();
            Invalidate();
        } finally {
            actualizandoContenido = false;
        }
    }

    public void TransferirContenidoA(Control nuevoContenedor) {
        ArgumentNullException.ThrowIfNull(nuevoContenedor);

        if (ReferenceEquals(Contenido.Parent, nuevoContenedor)) {
            return;
        }

        DesconectarEventosContenido();
        CancelarInteraccion();
        estadoDesplazamiento.Restablecer(0, int.MaxValue);
        barraVisible = false;
        nuevoContenedor.Controls.Add(Contenido);
        Invalidate();
    }

    public void RestaurarContenido() {
        if (ReferenceEquals(Contenido.Parent, this)) {
            return;
        }

        CancelarInteraccion();
        Controls.Add(Contenido);
        ConectarEventosContenido();
        ActualizarContenido(volverAlInicio: false);
    }

    public void CancelarInteraccion() {
        bool requiereRepintado = indicadorHover || arrastrandoIndicador;

        DetenerAnimacion();
        acumuladoRueda = 0;
        indicadorHover = false;
        arrastrandoIndicador = false;
        desfaseArrastreIndicador = 0;
        Cursor = Cursors.Default;

        if (Capture) {
            Capture = false;
        }

        if (requiereRepintado) {
            InvalidarBarra();
        }
    }

    public void IrAlInicio() {
        acumuladoRueda = 0;
        MoverA(0);
    }

    public void IrAlFinal() {
        MoverA(int.MaxValue);
    }

    public void AsegurarVisible(Control control) {
        if (!control.Visible || !EsDescendienteDe(control, Contenido)) {
            return;
        }

        Rectangle limitesControl = Contenido.RectangleToClient(
            control.RectangleToScreen(control.ClientRectangle));
        int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
        int limiteSuperior = ObtenerDesplazamientoSolicitado();
        int limiteInferior = limiteSuperior + altoDisponible;

        if (limitesControl.Top < limiteSuperior) {
            MoverA(limitesControl.Top);
        } else if (limitesControl.Bottom > limiteInferior) {
            MoverA(limitesControl.Bottom - altoDisponible);
        }
    }

    private static bool EsDescendienteDe(Control control, Control posibleAncestro) {
        for (Control? actual = control.Parent; actual is not null; actual = actual.Parent) {
            if (ReferenceEquals(actual, posibleAncestro)) {
                return true;
            }
        }

        return false;
    }

    protected override void OnResize(EventArgs e) {
        CancelarInteraccion();
        base.OnResize(e);
        ActualizarContenido(volverAlInicio: false);
    }

    protected override void OnVisibleChanged(EventArgs e) {
        if (!Visible) {
            CancelarInteraccion();
        }

        base.OnVisibleChanged(e);

        if (Visible) {
            ActualizarContenido(volverAlInicio: false);
        }
    }

    protected override void OnParentChanged(EventArgs e) {
        CancelarInteraccion();
        base.OnParentChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
        base.OnPaint(e);

        Rectangle viewport = ObtenerRectanguloViewport();

        if (!viewport.IsEmpty) {
            e.Graphics.FillRectangle(pincelFondoContenido, viewport);
        }

        if (MostrarBordeFoco && Focused && ClientSize.Width > 1 && ClientSize.Height > 1) {
            e.Graphics.DrawRectangle(
                lapizBordeFoco,
                new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1));
        }

        Rectangle reservaBarra = ObtenerRectanguloReservaBarra();

        if (reservaBarra.IsEmpty) {
            return;
        }

        e.Graphics.FillRectangle(pincelFondoContenido, reservaBarra);

        if (!barraVisible) {
            return;
        }

        Rectangle pista = ObtenerRectanguloPista();
        Rectangle indicador = ObtenerRectanguloIndicador();

        if (pista.IsEmpty || indicador.IsEmpty) {
            return;
        }

        SmoothingMode suavizadoAnterior = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        RellenarRectanguloRedondeado(e.Graphics, pincelPistaBarra, pista);
        RellenarRectanguloRedondeado(
            e.Graphics,
            indicadorHover || arrastrandoIndicador
                ? pincelIndicadorBarraHover
                : pincelIndicadorBarra,
            indicador);

        e.Graphics.SmoothingMode = suavizadoAnterior;
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e) {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left || !barraVisible) {
            return;
        }

        Rectangle indicador = ObtenerRectanguloIndicador();
        Rectangle pista = ObtenerRectanguloPista();

        if (indicador.Contains(e.Location)) {
            IniciarArrastreIndicador(e.Y - indicador.Top);
            return;
        }

        if (pista.Contains(e.Location)) {
            IniciarArrastreIndicador(indicador.Height / 2);
            MoverIndicadorDesdeMouse(e.Y);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);

        if (arrastrandoIndicador) {
            MoverIndicadorDesdeMouse(e.Y);
            return;
        }

        bool nuevoHover = barraVisible && ObtenerRectanguloIndicador().Contains(e.Location);

        if (nuevoHover == indicadorHover) {
            return;
        }

        indicadorHover = nuevoHover;
        Cursor = indicadorHover ? Cursors.Hand : Cursors.Default;
        InvalidarBarra();
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);

        if (e.Button == MouseButtons.Left) {
            FinalizarArrastreIndicador(e.Location);
        }
    }

    protected override void OnMouseLeave(EventArgs e) {
        base.OnMouseLeave(e);

        if (!arrastrandoIndicador && indicadorHover) {
            indicadorHover = false;
            Cursor = Cursors.Default;
            InvalidarBarra();
        }
    }

    protected override void OnMouseCaptureChanged(EventArgs e) {
        base.OnMouseCaptureChanged(e);

        if (arrastrandoIndicador && !Capture) {
            CancelarInteraccion();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        if (e is HandledMouseEventArgs { Handled: true }) {
            return;
        }

        ProcesarRueda(e);

        if (e is HandledMouseEventArgs handled) {
            handled.Handled = true;
        }

        base.OnMouseWheel(e);
    }

    protected override bool IsInputKey(Keys keyData) {
        return (keyData & Keys.KeyCode) switch {
            Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End => true,
            _ => base.IsInputKey(keyData)
        };
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        int pagina = Math.Max(PasoLinea, ClientSize.Height - PasoLinea);
        int posicionActual = ObtenerDesplazamientoSolicitado();
        int? destino = e.KeyCode switch {
            Keys.Up => posicionActual - PasoLinea,
            Keys.Down => posicionActual + PasoLinea,
            Keys.PageUp => posicionActual - pagina,
            Keys.PageDown => posicionActual + pagina,
            Keys.Home => 0,
            Keys.End => int.MaxValue,
            _ => null
        };

        if (destino.HasValue) {
            MoverA(destino.Value);
            e.Handled = true;
            e.SuppressKeyPress = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void ProcesarRueda(MouseEventArgs e) {
        int lineas = SystemInformation.MouseWheelScrollLines;
        ResultadoRuedaDesplazamiento resultado = CalcularDestinoRueda(
            ObtenerDesplazamientoSolicitado(),
            acumuladoRueda,
            e.Delta,
            lineas,
            ClientSize.Height,
            ObtenerMaximoDesplazamiento());
        acumuladoRueda = resultado.AcumuladoRestante;
        MoverA(resultado.PosicionDestino, animar: true);
    }

    private void MoverA(int destino, bool animar = false) {
        int maximo = ObtenerMaximoDesplazamiento();

        destino = Math.Clamp(destino, 0, maximo);

        if (animar) {
            IniciarAnimacion(destino, maximo);
            return;
        }

        DetenerAnimacion();
        Rectangle indicadorAnterior = ObtenerRectanguloIndicador();

        estadoDesplazamiento.IntentarMover(
            destino,
            maximo,
            (_, posicionNueva) =>
                AplicarDesplazamiento(indicadorAnterior, posicionNueva));
    }

    private void IniciarAnimacion(int destino, int maximo) {
        int posicionActual = estadoDesplazamiento.PosicionActual;

        if (!temporizadorAnimacion.Enabled && posicionActual == destino) {
            return;
        }

        if (temporizadorAnimacion.Enabled && posicionDestinoAnimacion == destino) {
            return;
        }

        posicionInicioAnimacion = posicionActual;
        posicionDestinoAnimacion = destino;
        relojAnimacion.Restart();
        temporizadorAnimacion.Start();
    }

    private void TemporizadorAnimacion_Tick(object? sender, EventArgs e) {
        double progreso = Math.Clamp(
            relojAnimacion.Elapsed.TotalMilliseconds / DuracionSuavizadoRuedaMs,
            0D,
            1D);
        double progresoSuavizado = 1D - Math.Pow(1D - progreso, 3D);
        int posicion = (int)Math.Round(
            posicionInicioAnimacion +
            (posicionDestinoAnimacion - posicionInicioAnimacion) *
            progresoSuavizado);
        Rectangle indicadorAnterior = ObtenerRectanguloIndicador();

        estadoDesplazamiento.Restablecer(posicion, ObtenerMaximoDesplazamiento());
        AplicarDesplazamiento(indicadorAnterior, estadoDesplazamiento.PosicionActual);

        if (progreso >= 1D) {
            DetenerAnimacion();
        }
    }

    private void DetenerAnimacion() {
        if (!temporizadorAnimacion.Enabled && !relojAnimacion.IsRunning) {
            return;
        }

        temporizadorAnimacion.Stop();
        relojAnimacion.Stop();
        estadoDesplazamiento.Restablecer(
            posicionDestinoAnimacion,
            ObtenerMaximoDesplazamiento());
    }

    private int ObtenerDesplazamientoSolicitado() {
        return estadoDesplazamiento.PosicionActual;
    }

    private void AplicarDesplazamiento(
        Rectangle indicadorAnterior,
        int posicionNueva) {
        SuspendLayout();

        try {
            Contenido.Top = Padding.Top - posicionNueva;
        } finally {
            ResumeLayout(performLayout: false);
        }

        Rectangle indicadorNuevo = ObtenerRectanguloIndicador();
        InvalidarCambioIndicador(indicadorAnterior, indicadorNuevo);
    }

    private void IniciarArrastreIndicador(int desfase) {
        Rectangle indicador = ObtenerRectanguloIndicador();

        if (indicador.IsEmpty) {
            return;
        }

        if (CanFocus) {
            Focus();
        }

        arrastrandoIndicador = true;
        indicadorHover = true;
        desfaseArrastreIndicador = Math.Clamp(desfase, 0, indicador.Height);
        Cursor = Cursors.Hand;
        Capture = true;
        InvalidarBarra();
    }

    private void MoverIndicadorDesdeMouse(int posicionY) {
        MoverA(CalcularDesplazamientoDesdeMouse(posicionY));
    }

    private int CalcularDesplazamientoDesdeMouse(int posicionY) {
        Rectangle pista = ObtenerRectanguloPista();
        Rectangle indicador = ObtenerRectanguloIndicador();
        int recorrido = pista.Height - indicador.Height;
        int maximo = ObtenerMaximoDesplazamiento();
        return CalcularDestinoIndicador(
            posicionY,
            desfaseArrastreIndicador,
            pista.Top,
            recorrido,
            maximo);
    }

    internal static ResultadoRuedaDesplazamiento CalcularDestinoRueda(
        int posicionActual,
        int acumuladoActual,
        int delta,
        int lineas,
        int altoViewport,
        int maximo) {
        int limite = Math.Max(0, maximo);

        if (lineas == 0) {
            return new ResultadoRuedaDesplazamiento(
                Math.Clamp(posicionActual, 0, limite),
                acumuladoActual);
        }

        int nuevoAcumulado = acumuladoActual + delta;
        int pasos = nuevoAcumulado / DeltaRueda;
        nuevoAcumulado %= DeltaRueda;

        if (pasos == 0) {
            return new ResultadoRuedaDesplazamiento(
                Math.Clamp(posicionActual, 0, limite),
                nuevoAcumulado);
        }

        int distancia = lineas < 0
            ? Math.Max(PasoLinea, altoViewport - PasoLinea)
            : lineas * PasoLinea;
        long destino = (long)posicionActual - (long)pasos * distancia;
        return new ResultadoRuedaDesplazamiento(
            (int)Math.Clamp(destino, 0L, limite),
            nuevoAcumulado);
    }

    internal static int CalcularDestinoIndicador(
        int posicionY,
        int desfase,
        int inicioPista,
        int recorrido,
        int maximo) {
        if (recorrido <= 0 || maximo <= 0) {
            return 0;
        }

        int posicionIndicador = Math.Clamp(
            posicionY - desfase - inicioPista,
            0,
            recorrido);
        return (int)Math.Round(posicionIndicador * maximo / (double)recorrido);
    }

    private void FinalizarArrastreIndicador(Point ubicacionMouse) {
        if (!arrastrandoIndicador) {
            return;
        }

        int destinoFinal = CalcularDesplazamientoDesdeMouse(ubicacionMouse.Y);
        MoverA(destinoFinal);
        arrastrandoIndicador = false;
        desfaseArrastreIndicador = 0;
        indicadorHover = barraVisible && ObtenerRectanguloIndicador().Contains(ubicacionMouse);
        Cursor = indicadorHover ? Cursors.Hand : Cursors.Default;

        if (Capture) {
            Capture = false;
        }

        InvalidarBarra();
    }

    private int ObtenerMaximoDesplazamiento() {
        int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
        return CalcularMaximoDesplazamiento(Contenido.Height, altoDisponible);
    }

    private static int CalcularMaximoDesplazamiento(
        int altoContenido,
        int altoViewport) {
        return Math.Max(0, altoContenido - altoViewport);
    }

    private Rectangle ObtenerRectanguloViewport() {
        int ancho = Math.Max(
            0,
            ClientSize.Width - Padding.Horizontal - AnchoReservaBarra);
        int alto = Math.Max(0, ClientSize.Height - Padding.Vertical);

        return ancho > 0 && alto > 0
            ? new Rectangle(Padding.Left, Padding.Top, ancho, alto)
            : Rectangle.Empty;
    }

    private void InvalidarCambioIndicador(
        Rectangle indicadorAnterior,
        Rectangle indicadorNuevo) {
        Rectangle region = indicadorAnterior.IsEmpty
            ? indicadorNuevo
            : indicadorNuevo.IsEmpty
                ? indicadorAnterior
                : Rectangle.Union(indicadorAnterior, indicadorNuevo);

        if (region.IsEmpty) {
            return;
        }

        region.Inflate(
            MargenRepintadoDesplazamiento,
            MargenRepintadoDesplazamiento);
        region = Rectangle.Intersect(region, ObtenerRectanguloReservaBarra());

        if (!region.IsEmpty) {
            Invalidate(region);
        }
    }

    private Rectangle ObtenerRectanguloReservaBarra() {
        int limiteDerecho = Math.Max(Padding.Left, ClientSize.Width - Padding.Right);
        int x = Math.Max(Padding.Left, limiteDerecho - AnchoReservaBarra);
        int alto = Math.Max(0, ClientSize.Height - Padding.Vertical);

        return alto > 0 && limiteDerecho > x
            ? new Rectangle(x, Padding.Top, limiteDerecho - x, alto)
            : Rectangle.Empty;
    }

    private Rectangle ObtenerRectanguloPista() {
        Rectangle reserva = ObtenerRectanguloReservaBarra();
        int alto = reserva.Height - MargenVerticalBarra * 2;

        if (reserva.IsEmpty || alto <= 0) {
            return Rectangle.Empty;
        }

        int ancho = Math.Min(AnchoPistaBarra, reserva.Width);
        int x = reserva.Left + (reserva.Width - ancho) / 2;
        return new Rectangle(x, reserva.Top + MargenVerticalBarra, ancho, alto);
    }

    private Rectangle ObtenerRectanguloIndicador() {
        if (!barraVisible) {
            return Rectangle.Empty;
        }

        Rectangle pista = ObtenerRectanguloPista();
        int altoDisponible = Math.Max(1, ClientSize.Height - Padding.Vertical);
        int altoContenido = Math.Max(altoDisponible, Contenido.Height);

        if (pista.IsEmpty || altoContenido <= altoDisponible) {
            return Rectangle.Empty;
        }

        int altoMinimo = Math.Min(AltoMinimoIndicador, pista.Height);
        int altoIndicador = Math.Clamp(
            (int)Math.Round(pista.Height * altoDisponible / (double)altoContenido),
            altoMinimo,
            pista.Height);
        int recorrido = pista.Height - altoIndicador;
        int maximo = ObtenerMaximoDesplazamiento();
        int y = pista.Top;

        if (recorrido > 0 && maximo > 0) {
            y += (int)Math.Round(
                recorrido * estadoDesplazamiento.PosicionActual /
                (double)maximo);
        }

        return new Rectangle(pista.Left, y, pista.Width, altoIndicador);
    }

    private void InvalidarBarra() {
        Rectangle reserva = ObtenerRectanguloReservaBarra();

        if (!reserva.IsEmpty) {
            Invalidate(reserva);
        }
    }

    private static void RellenarRectanguloRedondeado(
        Graphics graphics,
        Brush brush,
        Rectangle rectangulo) {
        int diametro = Math.Min(rectangulo.Width, rectangulo.Height);

        if (diametro <= 1) {
            graphics.FillRectangle(brush, rectangulo);
            return;
        }

        using GraphicsPath trazado = new();
        trazado.AddArc(rectangulo.Left, rectangulo.Top, diametro, diametro, 180, 90);
        trazado.AddArc(rectangulo.Right - diametro, rectangulo.Top, diametro, diametro, 270, 90);
        trazado.AddArc(
            rectangulo.Right - diametro,
            rectangulo.Bottom - diametro,
            diametro,
            diametro,
            0,
            90);
        trazado.AddArc(rectangulo.Left, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
        trazado.CloseFigure();
        graphics.FillPath(brush, trazado);
    }

    private void Contenido_Layout(object? sender, LayoutEventArgs e) {
        if (!actualizandoContenido) {
            ActualizarContenido(volverAlInicio: false);
        }
    }

    private void RegistrarArbol(Control control) {
        if (!controlesRegistrados.Add(control)) {
            return;
        }

        control.MouseEnter += Control_MouseEnter;
        control.MouseWheel += Control_MouseWheel;
        control.ControlAdded += Control_ControlAdded;
        control.ControlRemoved += Control_ControlRemoved;

        foreach (Control hijo in control.Controls) {
            RegistrarArbol(hijo);
        }
    }

    private void DesregistrarArbol(Control control) {
        foreach (Control hijo in control.Controls) {
            DesregistrarArbol(hijo);
        }

        if (!controlesRegistrados.Remove(control)) {
            return;
        }

        control.MouseEnter -= Control_MouseEnter;
        control.MouseWheel -= Control_MouseWheel;
        control.ControlAdded -= Control_ControlAdded;
        control.ControlRemoved -= Control_ControlRemoved;
    }

    private void Control_MouseEnter(object? sender, EventArgs e) {
        if (TabStop && CanFocus && !ContainsFocus) {
            Focus();
        }
    }

    private void Control_MouseWheel(object? sender, MouseEventArgs e) {
        if (e is HandledMouseEventArgs { Handled: true }) {
            return;
        }

        ProcesarRueda(e);

        if (e is HandledMouseEventArgs handled) {
            handled.Handled = true;
        }
    }

    private void Control_ControlAdded(object? sender, ControlEventArgs e) {
        if (e.Control is not null) {
            RegistrarArbol(e.Control);
        }
    }

    private void Control_ControlRemoved(object? sender, ControlEventArgs e) {
        if (e.Control is not null) {
            DesregistrarArbol(e.Control);
        }
    }

    private void ConectarEventosContenido() {
        if (eventosContenidoConectados) {
            return;
        }

        RegistrarArbol(Contenido);
        Contenido.Layout += Contenido_Layout;
        eventosContenidoConectados = true;
    }

    private void DesconectarEventosContenido() {
        if (!eventosContenidoConectados) {
            return;
        }

        Contenido.Layout -= Contenido_Layout;
        DesregistrarArbol(Contenido);
        eventosContenidoConectados = false;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            CancelarInteraccion();
            DesconectarEventosContenido();
            temporizadorAnimacion.Dispose();
            pincelFondoContenido.Dispose();
            pincelPistaBarra.Dispose();
            pincelIndicadorBarra.Dispose();
            pincelIndicadorBarraHover.Dispose();
            lapizBordeFoco.Dispose();
        }

        base.Dispose(disposing);
    }
}
