using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace EndForge.Controls;

internal sealed class TextBoxMultilineaEndForge : Control {
    private const int EmGetFirstVisibleLine = 0x00CE;
    private const int EmGetLineCount = 0x00BA;
    private const int EmGetRect = 0x00B2;
    private const int EmLineScroll = 0x00B6;
    private const int EmSetUndoLimit = 0x0452;
    private const int EmStopGroupTyping = 0x0458;
    private const int SbVert = 1;
    private const int GwlStyle = -16;
    private const long WsVScroll = 0x00200000L;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpFrameChanged = 0x0020;

    private const int AnchoReservaBarra = 18;
    private const int AnchoPista = 8;
    private const int MargenVerticalPista = 6;
    private const int AltoMinimoIndicador = 24;
    private const int Borde = 1;

    private static readonly Color ColorBorde = Color.FromArgb(72, 58, 88);
    private static readonly Color ColorBordeFoco = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorPista = Color.FromArgb(31, 25, 45);
    private static readonly Color ColorIndicador = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorIndicadorHover = Color.FromArgb(174, 108, 232);
    private readonly Pen lapizBorde = new(ColorBorde);
    private readonly Pen lapizBordeFoco = new(ColorBordeFoco);
    private readonly SolidBrush pincelPista = new(ColorPista);
    private readonly SolidBrush pincelIndicador = new(ColorIndicador);
    private readonly SolidBrush pincelIndicadorHover = new(ColorIndicadorHover);

    private readonly ObservadorMensajesTextBox observadorMensajes;
    private readonly System.Windows.Forms.Timer timerCerrarGrupoEscritura;
    private bool barraVisible;
    private bool indicadorHover;
    private bool arrastrandoIndicador;
    private bool actualizandoEstado;
    private bool sincronizacionPendiente;
    private int desfaseArrastre;
    private int cantidadLineas = 1;
    private int lineasVisibles = 1;
    private int primeraLineaVisible;
    private int acumuladoRueda;
    private bool asignacionProgramatica;
    private bool cerrarGrupoTrasTecla;
#if DEBUG
    private int creacionesHandle;
    private IntPtr ultimoHandleObservado;
#endif

    public RichTextBox CampoTexto { get; }

    [AllowNull]
    public override string Text {
        get => CampoTexto.Text;
        set {
            string nuevoTexto = value ?? string.Empty;

            if (string.Equals(CampoTexto.Text, nuevoTexto, StringComparison.Ordinal)) {
                return;
            }

            RegistrarAsignacionProgramatica(nuevoTexto);
            DetenerTemporizadorAgrupacion();
            FinalizarGrupoEscritura("antes de asignar Text desde código");
            int inicioSeleccion = CampoTexto.SelectionStart;
            int longitudSeleccion = CampoTexto.SelectionLength;

            asignacionProgramatica = true;

            try {
                CampoTexto.Text = nuevoTexto;
                CampoTexto.SelectionStart = Math.Min(inicioSeleccion, nuevoTexto.Length);
                CampoTexto.SelectionLength = Math.Min(
                    longitudSeleccion,
                    nuevoTexto.Length - CampoTexto.SelectionStart);
            } finally {
                asignacionProgramatica = false;
                FinalizarGrupoEscritura("después de asignar Text desde código");
                RegistrarEstadoHistorial("asignación programática completada");
            }
        }
    }

    public bool ReadOnly {
        get => CampoTexto.ReadOnly;
        set => CampoTexto.ReadOnly = value;
    }

    public TextBoxMultilineaEndForge(RichTextBox campoTexto) {
        ArgumentNullException.ThrowIfNull(campoTexto);

        CampoTexto = campoTexto;
        observadorMensajes = new ObservadorMensajesTextBox(this);
        timerCerrarGrupoEscritura = new System.Windows.Forms.Timer {
            Interval = 550
        };
        timerCerrarGrupoEscritura.Tick += TimerCerrarGrupoEscritura_Tick;

        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.ContainerControl,
            true);

        DoubleBuffered = true;
        TabStop = false;
        BackColor = campoTexto.BackColor;
        ForeColor = campoTexto.ForeColor;

        campoTexto.Multiline = true;
        campoTexto.BorderStyle = BorderStyle.None;
        campoTexto.DetectUrls = false;
        campoTexto.ScrollBars = RichTextBoxScrollBars.None;
        campoTexto.ShortcutsEnabled = true;

        Controls.Add(campoTexto);

        campoTexto.HandleCreated += CampoTexto_HandleCreated;
        campoTexto.HandleDestroyed += CampoTexto_HandleDestroyed;
        campoTexto.TextChanged += CampoTexto_TextChanged;
        campoTexto.MouseWheel += CampoTexto_MouseWheel;
        campoTexto.KeyDown += CampoTexto_KeyDown;
        campoTexto.KeyUp += CampoTexto_KeyUp;
        campoTexto.FontChanged += CampoTexto_GeometriaChanged;
        campoTexto.SizeChanged += CampoTexto_GeometriaChanged;
        campoTexto.Enter += CampoTexto_FocoChanged;
        campoTexto.Leave += CampoTexto_FocoChanged;
        campoTexto.EnabledChanged += CampoTexto_EstadoChanged;

        if (campoTexto.IsHandleCreated) {
            observadorMensajes.Conectar(campoTexto.Handle);
            ConfigurarHistorialNativo();
            OcultarBarraNativa();
        }

        AplicarGeometriaInterna();
    }

    public new bool Focus() => CampoTexto.Focus();

    public void Clear() => Text = string.Empty;

    private int Escalar(int valor) {
        return Math.Max(1, (int)Math.Round(valor * DeviceDpi / 96D));
    }

    protected override void OnHandleCreated(EventArgs e) {
        base.OnHandleCreated(e);
        ProgramarSincronizacion();
    }

    protected override void OnLayout(LayoutEventArgs levent) {
        base.OnLayout(levent);
        AplicarGeometriaInterna();
        ProgramarSincronizacion();
    }

    protected override void OnPaint(PaintEventArgs e) {
        e.Graphics.Clear(BackColor);

        if (ClientSize.Width > 1 && ClientSize.Height > 1) {
            e.Graphics.DrawRectangle(
                CampoTexto.Focused ? lapizBordeFoco : lapizBorde,
                new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1));
        }

        base.OnPaint(e);

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

        RellenarRectanguloRedondeado(e.Graphics, pincelPista, pista);
        RellenarRectanguloRedondeado(
            e.Graphics,
            indicadorHover || arrastrandoIndicador
                ? pincelIndicadorHover
                : pincelIndicador,
            indicador);

        e.Graphics.SmoothingMode = suavizadoAnterior;
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left || !barraVisible || !CampoTexto.Enabled) {
            return;
        }

        Rectangle indicador = ObtenerRectanguloIndicador();
        Rectangle pista = ObtenerRectanguloPista();

        if (!indicador.Contains(e.Location) && !pista.Contains(e.Location)) {
            return;
        }

        CampoTexto.Focus();
        arrastrandoIndicador = true;
        indicadorHover = true;
        Capture = true;
        desfaseArrastre = indicador.Contains(e.Location)
            ? e.Y - indicador.Top
            : indicador.Height / 2;
        MoverIndicadorDesdeMouse(e.Y);
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
            FinalizarArrastre();
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
            arrastrandoIndicador = false;
            desfaseArrastre = 0;
            indicadorHover = false;
            Cursor = Cursors.Default;
            InvalidarBarra();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        if (e is HandledMouseEventArgs { Handled: true }) {
            return;
        }

        if (barraVisible && CampoTexto.Enabled) {
            ProcesarRueda(e.Delta);

            if (e is HandledMouseEventArgs handled) {
                handled.Handled = true;
            }

            return;
        }

        base.OnMouseWheel(e);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            CampoTexto.HandleCreated -= CampoTexto_HandleCreated;
            CampoTexto.HandleDestroyed -= CampoTexto_HandleDestroyed;
            CampoTexto.TextChanged -= CampoTexto_TextChanged;
            CampoTexto.MouseWheel -= CampoTexto_MouseWheel;
            CampoTexto.KeyDown -= CampoTexto_KeyDown;
            CampoTexto.KeyUp -= CampoTexto_KeyUp;
            CampoTexto.FontChanged -= CampoTexto_GeometriaChanged;
            CampoTexto.SizeChanged -= CampoTexto_GeometriaChanged;
            CampoTexto.Enter -= CampoTexto_FocoChanged;
            CampoTexto.Leave -= CampoTexto_FocoChanged;
            CampoTexto.EnabledChanged -= CampoTexto_EstadoChanged;
            timerCerrarGrupoEscritura.Stop();
            timerCerrarGrupoEscritura.Tick -= TimerCerrarGrupoEscritura_Tick;
            timerCerrarGrupoEscritura.Dispose();
            observadorMensajes.Dispose();
            lapizBorde.Dispose();
            lapizBordeFoco.Dispose();
            pincelPista.Dispose();
            pincelIndicador.Dispose();
            pincelIndicadorHover.Dispose();
        }

        base.Dispose(disposing);
    }

    private void CampoTexto_HandleCreated(object? sender, EventArgs e) {
#if DEBUG
        creacionesHandle++;
        bool recreado = ultimoHandleObservado != IntPtr.Zero &&
            ultimoHandleObservado != CampoTexto.Handle;
        ultimoHandleObservado = CampoTexto.Handle;
        Debug.WriteLine(
            $"[Objetivo] HandleCreated #{creacionesHandle}: 0x{CampoTexto.Handle.ToInt64():X}; " +
            $"recreado={recreado}.");
#endif
        observadorMensajes.Conectar(CampoTexto.Handle);
        ConfigurarHistorialNativo();
        OcultarBarraNativa();
        ProgramarSincronizacion();
    }

    private void CampoTexto_HandleDestroyed(object? sender, EventArgs e) {
        DetenerTemporizadorAgrupacion();
#if DEBUG
        Debug.WriteLine($"[Objetivo] HandleDestroyed: 0x{ultimoHandleObservado.ToInt64():X}.");
#endif
        observadorMensajes.Desconectar();
    }

    private void CampoTexto_TextChanged(object? sender, EventArgs e) {
        base.OnTextChanged(e);

        if (!asignacionProgramatica) {
            timerCerrarGrupoEscritura.Stop();
            timerCerrarGrupoEscritura.Start();
            RegistrarEstadoHistorial("TextChanged de usuario");
        }

        ProgramarSincronizacion();
    }

    private void CampoTexto_KeyDown(object? sender, KeyEventArgs e) {
        bool esAtajoEdicion = e.Control &&
            (e.KeyCode == Keys.V || e.KeyCode == Keys.X);
        bool eliminaSeleccion = CampoTexto.SelectionLength > 0 &&
            (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back);
        bool esHistorial = e.Control &&
            (e.KeyCode == Keys.Z || e.KeyCode == Keys.Y);

        if (e.KeyCode == Keys.Enter || esAtajoEdicion || eliminaSeleccion || esHistorial) {
            DetenerTemporizadorAgrupacion();
            FinalizarGrupoEscritura($"límite previo a {e.KeyCode}");
            cerrarGrupoTrasTecla = e.KeyCode == Keys.Enter ||
                esAtajoEdicion ||
                eliminaSeleccion;
        }

        if (esHistorial) {
            RegistrarEstadoHistorial($"antes de Ctrl+{e.KeyCode}");
        }
    }

    private void CampoTexto_KeyUp(object? sender, KeyEventArgs e) {
        if (cerrarGrupoTrasTecla) {
            cerrarGrupoTrasTecla = false;
            DetenerTemporizadorAgrupacion();
            FinalizarGrupoEscritura($"límite posterior a {e.KeyCode}");
        }

        if (e.Control && (e.KeyCode == Keys.Z || e.KeyCode == Keys.Y)) {
            RegistrarEstadoHistorial($"después de Ctrl+{e.KeyCode}");
        }
    }

    private void CampoTexto_MouseWheel(object? sender, MouseEventArgs e) {
        if (!barraVisible || !CampoTexto.Enabled ||
            e is HandledMouseEventArgs { Handled: true }) {
            return;
        }

        ProcesarRueda(e.Delta);

        if (e is HandledMouseEventArgs handled) {
            handled.Handled = true;
        }
    }

    private void CampoTexto_GeometriaChanged(object? sender, EventArgs e) {
        ProgramarSincronizacion();
    }

    private void CampoTexto_FocoChanged(object? sender, EventArgs e) {
        if (!CampoTexto.Focused) {
            DetenerTemporizadorAgrupacion();
            FinalizarGrupoEscritura("pérdida de foco");
        }

        Invalidate();
        ProgramarSincronizacion();
    }

    private void CampoTexto_EstadoChanged(object? sender, EventArgs e) {
        Cursor = Cursors.Default;
        Invalidate();
    }

    private void TimerCerrarGrupoEscritura_Tick(object? sender, EventArgs e) {
        timerCerrarGrupoEscritura.Stop();
        FinalizarGrupoEscritura("pausa de escritura");
        RegistrarEstadoHistorial("grupo cerrado por inactividad");
    }

    private void DetenerTemporizadorAgrupacion() {
        if (timerCerrarGrupoEscritura.Enabled) {
            timerCerrarGrupoEscritura.Stop();
        }
    }

    private void ConfigurarHistorialNativo() {
        if (!CampoTexto.IsHandleCreated || CampoTexto.IsDisposed) {
            return;
        }

        SendMessage(
            CampoTexto.Handle,
            EmSetUndoLimit,
            new IntPtr(100),
            IntPtr.Zero);
        FinalizarGrupoEscritura("configuración inicial del historial");
        RegistrarEstadoHistorial("EM_SETUNDOLIMIT=100");
    }

    private void FinalizarGrupoEscritura(string origen) {
        if (!CampoTexto.IsHandleCreated || CampoTexto.IsDisposed) {
            return;
        }

        SendMessage(
            CampoTexto.Handle,
            EmStopGroupTyping,
            IntPtr.Zero,
            IntPtr.Zero);
        RegistrarGrupoFinalizado(origen);
    }

    private void AntesDeComandoEdicionNativo(string comando) {
        DetenerTemporizadorAgrupacion();
        FinalizarGrupoEscritura($"antes de {comando}");
        RegistrarEstadoHistorial($"antes de {comando}");
    }

    private void DespuesDeComandoEdicionNativo(string comando) {
        DetenerTemporizadorAgrupacion();
        FinalizarGrupoEscritura($"después de {comando}");
        RegistrarEstadoHistorial($"después de {comando}");
    }

    [Conditional("DEBUG")]
    private void RegistrarGrupoFinalizado(string origen) {
        Debug.WriteLine($"[Objetivo] EM_STOPGROUPTYPING: {origen}.");
    }

    [Conditional("DEBUG")]
    private void RegistrarEstadoHistorial(string origen) {
        if (!CampoTexto.IsHandleCreated || CampoTexto.IsDisposed) {
            return;
        }

        VerificarHandleEstable(origen);
        Debug.WriteLine(
            $"[Objetivo] {origen}: Handle=0x{CampoTexto.Handle.ToInt64():X}, " +
            $"CanUndo={CampoTexto.CanUndo}, Undo='{CampoTexto.UndoActionName}', " +
            $"CanRedo={CampoTexto.CanRedo}, Redo='{CampoTexto.RedoActionName}'.");
    }

    [Conditional("DEBUG")]
    private void RegistrarAsignacionProgramatica(string nuevoTexto) {
        Debug.WriteLine(
            $"[Objetivo] Asignación externa de Text ({nuevoTexto.Length} caracteres).\n" +
            Environment.StackTrace);
    }

    [Conditional("DEBUG")]
    private void VerificarHandleEstable(string origen) {
#if DEBUG
        if (CampoTexto.IsHandleCreated &&
            ultimoHandleObservado != IntPtr.Zero &&
            CampoTexto.Handle != ultimoHandleObservado) {
            Debug.WriteLine(
                $"[Objetivo] ALERTA: Handle cambió durante {origen}: " +
                $"0x{ultimoHandleObservado.ToInt64():X} -> 0x{CampoTexto.Handle.ToInt64():X}.");
            ultimoHandleObservado = CampoTexto.Handle;
        }
#endif
    }

    private void AplicarGeometriaInterna() {
        int borde = Escalar(Borde);
        int margenIzquierdo = Escalar(7);
        int margenSuperior = Escalar(5);
        int margenDerecho = barraVisible
            ? Escalar(AnchoReservaBarra + 3)
            : Escalar(7);
        int margenInferior = Escalar(5);
        Rectangle areaTexto = Rectangle.FromLTRB(
            borde + margenIzquierdo,
            borde + margenSuperior,
            Math.Max(borde + margenIzquierdo + 1, ClientSize.Width - borde - margenDerecho),
            Math.Max(borde + margenSuperior + 1, ClientSize.Height - borde - margenInferior));

        if (CampoTexto.Bounds != areaTexto) {
            CampoTexto.Bounds = areaTexto;
            OcultarBarraNativa();
        }
    }

    private void ProgramarSincronizacion() {
        if (sincronizacionPendiente || IsDisposed || Disposing) {
            return;
        }

        if (!IsHandleCreated) {
            return;
        }

        sincronizacionPendiente = true;

        BeginInvoke((Action)(() => {
            sincronizacionPendiente = false;

            if (!IsDisposed && !Disposing) {
                ActualizarEstadoBarra();
            }
        }));
    }

    private void ActualizarEstadoBarra() {
        if (actualizandoEstado || !CampoTexto.IsHandleCreated || CampoTexto.IsDisposed) {
            return;
        }

        VerificarHandleEstable("sincronización de la barra personalizada");
        actualizandoEstado = true;

        try {
            int nuevasLineas = Math.Max(
                1,
                SendMessage(
                    CampoTexto.Handle,
                    EmGetLineCount,
                    IntPtr.Zero,
                    IntPtr.Zero).ToInt32());
            int altoLinea = Math.Max(1, CampoTexto.Font.Height);
            int altoFormato = ObtenerAltoAreaFormato();
            int nuevasLineasVisibles = Math.Max(1, altoFormato / altoLinea);
            bool nuevaBarraVisible = nuevasLineas > nuevasLineasVisibles;
            bool cambioVisibilidad = nuevaBarraVisible != barraVisible;

            cantidadLineas = nuevasLineas;
            lineasVisibles = nuevasLineasVisibles;
            barraVisible = nuevaBarraVisible;

            if (cambioVisibilidad) {
                indicadorHover = false;
                FinalizarArrastre();
                AplicarGeometriaInterna();
                ProgramarSincronizacion();
            }

            int maximo = ObtenerMaximaPrimeraLinea();
            primeraLineaVisible = Math.Clamp(ObtenerPrimeraLineaVisible(), 0, maximo);

            if (!barraVisible) {
                acumuladoRueda = 0;
            }

            OcultarBarraNativa();
            InvalidarBarra();
        } finally {
            actualizandoEstado = false;
        }
    }

    private int ObtenerPrimeraLineaVisible() {
        if (!CampoTexto.IsHandleCreated) {
            return 0;
        }

        return Math.Max(
            0,
            SendMessage(
                CampoTexto.Handle,
                EmGetFirstVisibleLine,
                IntPtr.Zero,
                IntPtr.Zero).ToInt32());
    }

    private int ObtenerAltoAreaFormato() {
        RectanguloNativo rectangulo = default;
        SendMessage(
            CampoTexto.Handle,
            EmGetRect,
            IntPtr.Zero,
            ref rectangulo);
        return Math.Max(1, rectangulo.Bottom - rectangulo.Top);
    }

    private void OcultarBarraNativa() {
        if (!CampoTexto.IsHandleCreated || CampoTexto.IsDisposed) {
            return;
        }

        IntPtr estiloActual = ObtenerWindowLongPtr(CampoTexto.Handle, GwlStyle);
        long estilo = estiloActual.ToInt64();

        if ((estilo & WsVScroll) != 0) {
            EstablecerWindowLongPtr(
                CampoTexto.Handle,
                GwlStyle,
                new IntPtr(estilo & ~WsVScroll));
            SetWindowPos(
                CampoTexto.Handle,
                IntPtr.Zero,
                0,
                0,
                0,
                0,
                SwpNoMove |
                SwpNoSize |
                SwpNoZOrder |
                SwpNoActivate |
                SwpFrameChanged);

#if DEBUG
            Debug.WriteLine(
                $"[Objetivo] Se eliminó WS_VSCROLL del Handle 0x{CampoTexto.Handle.ToInt64():X}.");
#endif
        }

        ShowScrollBar(CampoTexto.Handle, SbVert, false);
    }

    private int ObtenerMaximaPrimeraLinea() => Math.Max(0, cantidadLineas - lineasVisibles);

    private void MoverIndicadorDesdeMouse(int posicionY) {
        Rectangle pista = ObtenerRectanguloPista();
        Rectangle indicador = ObtenerRectanguloIndicador();
        int recorrido = pista.Height - indicador.Height;

        if (recorrido <= 0) {
            MoverALinea(0);
            return;
        }

        int parteSuperior = Math.Clamp(
            posicionY - desfaseArrastre,
            pista.Top,
            pista.Bottom - indicador.Height);
        double proporcion = (parteSuperior - pista.Top) / (double)recorrido;
        int destino = (int)Math.Round(proporcion * ObtenerMaximaPrimeraLinea());
        MoverALinea(destino);
    }

    private void ProcesarRueda(int delta) {
        int lineasPorPaso = SystemInformation.MouseWheelScrollLines;

        if (lineasPorPaso == 0 || delta == 0) {
            return;
        }

        acumuladoRueda += delta;
        int pasos = acumuladoRueda / 120;
        acumuladoRueda %= 120;

        if (pasos == 0) {
            return;
        }

        int distancia = lineasPorPaso < 0
            ? Math.Max(1, lineasVisibles - 1)
            : Math.Max(1, lineasPorPaso);
        MoverALinea(ObtenerPrimeraLineaVisible() - pasos * distancia);
    }

    private void MoverALinea(int destino) {
        if (!CampoTexto.IsHandleCreated) {
            return;
        }

        VerificarHandleEstable("desplazamiento del editor");
        int actual = ObtenerPrimeraLineaVisible();
        int nuevaLinea = Math.Clamp(destino, 0, ObtenerMaximaPrimeraLinea());
        int desplazamiento = nuevaLinea - actual;

        if (desplazamiento == 0) {
            return;
        }

        SendMessage(
            CampoTexto.Handle,
            EmLineScroll,
            IntPtr.Zero,
            new IntPtr(desplazamiento));
        primeraLineaVisible = Math.Clamp(ObtenerPrimeraLineaVisible(), 0, ObtenerMaximaPrimeraLinea());
        InvalidarBarra();
    }

    private Rectangle ObtenerRectanguloPista() {
        int borde = Escalar(Borde);
        int anchoReserva = Escalar(AnchoReservaBarra);
        int anchoPista = Escalar(AnchoPista);
        int margenVertical = Escalar(MargenVerticalPista);

        if (!barraVisible || ClientSize.Width <= anchoReserva || ClientSize.Height <= borde * 2) {
            return Rectangle.Empty;
        }

        int x = ClientSize.Width - borde - anchoReserva +
            Math.Max(0, (anchoReserva - anchoPista) / 2);
        int y = borde + margenVertical;
        int alto = ClientSize.Height - (borde + margenVertical) * 2;
        return alto > 0
            ? new Rectangle(x, y, anchoPista, alto)
            : Rectangle.Empty;
    }

    private Rectangle ObtenerRectanguloIndicador() {
        Rectangle pista = ObtenerRectanguloPista();
        int maximo = ObtenerMaximaPrimeraLinea();

        if (pista.IsEmpty || maximo <= 0) {
            return Rectangle.Empty;
        }

        double proporcionVisible = Math.Min(1D, lineasVisibles / (double)cantidadLineas);
        int altoIndicador = Math.Clamp(
            (int)Math.Round(pista.Height * proporcionVisible),
            Math.Min(Escalar(AltoMinimoIndicador), pista.Height),
            pista.Height);
        int recorrido = pista.Height - altoIndicador;
        int y = pista.Top + (int)Math.Round(
            recorrido * primeraLineaVisible / (double)maximo);
        return new Rectangle(pista.Left, y, pista.Width, altoIndicador);
    }

    private void FinalizarArrastre() {
        arrastrandoIndicador = false;
        desfaseArrastre = 0;

        if (Capture) {
            Capture = false;
        }

        Point posicion = PointToClient(Cursor.Position);
        indicadorHover = barraVisible && ObtenerRectanguloIndicador().Contains(posicion);
        Cursor = indicadorHover ? Cursors.Hand : Cursors.Default;
        InvalidarBarra();
    }

    private void InvalidarBarra() {
        int borde = Escalar(Borde);
        int anchoReserva = Escalar(AnchoReservaBarra);
        Rectangle reserva = new(
            Math.Max(0, ClientSize.Width - anchoReserva - borde),
            0,
            Math.Min(ClientSize.Width, anchoReserva + borde),
            ClientSize.Height);

        if (!reserva.IsEmpty) {
            Invalidate(reserva);
        }
    }

    private static void RellenarRectanguloRedondeado(
        Graphics graphics,
        Brush brush,
        Rectangle rectangulo) {
        int diametro = Math.Min(rectangulo.Width, rectangulo.Height);

        if (diametro <= 2) {
            graphics.FillRectangle(brush, rectangulo);
            return;
        }

        using GraphicsPath ruta = new();
        ruta.AddArc(rectangulo.Left, rectangulo.Top, diametro, diametro, 180, 90);
        ruta.AddArc(
            rectangulo.Right - diametro,
            rectangulo.Top,
            diametro,
            diametro,
            270,
            90);
        ruta.AddArc(
            rectangulo.Right - diametro,
            rectangulo.Bottom - diametro,
            diametro,
            diametro,
            0,
            90);
        ruta.AddArc(
            rectangulo.Left,
            rectangulo.Bottom - diametro,
            diametro,
            diametro,
            90,
            90);
        ruta.CloseFigure();
        graphics.FillPath(brush, ruta);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        ref RectanguloNativo lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowScrollBar(
        IntPtr hWnd,
        int wBar,
        [MarshalAs(UnmanagedType.Bool)] bool bShow);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(
        IntPtr hWnd,
        int nIndex,
        IntPtr dwNewLong);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    private static IntPtr ObtenerWindowLongPtr(IntPtr hWnd, int nIndex) {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    private static IntPtr EstablecerWindowLongPtr(
        IntPtr hWnd,
        int nIndex,
        IntPtr nuevoValor) {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, nuevoValor)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, nuevoValor.ToInt32()));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RectanguloNativo {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private sealed class ObservadorMensajesTextBox : NativeWindow, IDisposable {
        private const int WmSetText = 0x000C;
        private const int WmKeyDown = 0x0100;
        private const int WmKeyUp = 0x0101;
        private const int WmChar = 0x0102;
        private const int WmVScroll = 0x0115;
        private const int WmMouseMove = 0x0200;
        private const int WmLButtonDown = 0x0201;
        private const int WmLButtonUp = 0x0202;
        private const int WmCut = 0x0300;
        private const int WmPaste = 0x0302;
        private const int WmClear = 0x0303;
        private const int WmUndo = 0x0304;
        private const int EmScrollCaret = 0x00B7;
        private const int EmRedo = 0x0454;

        private TextBoxMultilineaEndForge? propietario;

        public ObservadorMensajesTextBox(TextBoxMultilineaEndForge propietario) {
            this.propietario = propietario;
        }

        public void Conectar(IntPtr handle) {
            if (Handle == handle) {
                return;
            }

            Desconectar();
            AssignHandle(handle);
        }

        public void Desconectar() {
            if (Handle != IntPtr.Zero) {
                ReleaseHandle();
            }
        }

        protected override void WndProc(ref Message m) {
            string? comandoEdicion = m.Msg switch {
                WmCut => "WM_CUT",
                WmPaste => "WM_PASTE",
                WmClear => "WM_CLEAR",
                WmUndo => "WM_UNDO",
                EmRedo => "EM_REDO",
                _ => null
            };

            if (comandoEdicion is not null) {
                propietario?.AntesDeComandoEdicionNativo(comandoEdicion);
            }

            base.WndProc(ref m);

            if (comandoEdicion is not null) {
                propietario?.DespuesDeComandoEdicionNativo(comandoEdicion);
            }

            bool sincronizar = m.Msg switch {
                WmSetText or WmKeyDown or WmKeyUp or WmChar or WmVScroll or
                WmLButtonDown or WmLButtonUp or WmCut or WmPaste or
                WmClear or WmUndo or EmRedo or EmScrollCaret => true,
                WmMouseMove => Control.MouseButtons.HasFlag(MouseButtons.Left),
                _ => false
            };

            if (sincronizar) {
                propietario?.ProgramarSincronizacion();
            }
        }

        public void Dispose() {
            Desconectar();
            propietario = null;
        }
    }
}
