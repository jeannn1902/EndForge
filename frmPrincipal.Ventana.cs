using System.Runtime.InteropServices;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private enum DistribucionPanelPrincipal {
        Normal,
        Inicio,
        Curso,
        NuevaPractica,
        Estadisticas,
        Logros
    }

    private System.Windows.Forms.Timer timerRecalcularVista = new System.Windows.Forms.Timer();
    private bool recalculandoVista;
    private bool recalculoPendienteDuranteTransicion;
    private DistribucionPanelPrincipal distribucionPanelPrincipal;
    private FormWindowState ultimoEstadoVentana = FormWindowState.Normal;
    private Size ultimoTamanoVistaRecalculado;
    private int ultimoDpiVistaRecalculado;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    private void ActivarBarraTituloOscura() {
        if (Environment.OSVersion.Version.Major >= 10) {
            int usarModoOscuro = 1;
            DwmSetWindowAttribute(Handle, 20, ref usarModoOscuro, sizeof(int));
        }
    }

    private void ActivarDobleBuffer(Control control) {
        typeof(Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(control, true, null);
    }

    private void InvalidarFondoContinuo() {
        if (IsDisposed || !IsHandleCreated) {
            return;
        }

        fondoEndForge.Invalidate();
    }

    private void PosicionarBotonesBarraTitulo() {
        btnCerrar.Location = new Point(panelBarraTitulo.Width - btnCerrar.Width, 0);
        btnMaximizar.Location = new Point(btnCerrar.Left - btnMaximizar.Width, 0);
        btnMinimizar.Location = new Point(btnMaximizar.Left - btnMinimizar.Width, 0);
    }

    private void ActualizarBotonMaximizar() {
        if (WindowState == FormWindowState.Maximized) {
            btnMaximizar.Text = "❐";
        } else {
            btnMaximizar.Text = "□";
        }
    }

    private void BtnMinimizar_Click(object? sender, EventArgs e) {
        if (transicionandoDesdeBienvenida) {
            return;
        }

        WindowState = FormWindowState.Minimized;
    }

    private void BtnMaximizar_Click(object? sender, EventArgs e) {
        if (transicionandoDesdeBienvenida) {
            return;
        }

        if (WindowState == FormWindowState.Maximized) {
            WindowState = FormWindowState.Normal;
        } else {
            WindowState = FormWindowState.Maximized;
        }

        ActualizarBotonMaximizar();
        ActiveControl = null;
    }

    private void BtnCerrar_Click(object? sender, EventArgs e) {
        Close();
    }

    private void BtnCerrar_MouseEnter(object? sender, EventArgs e) {
        btnCerrar.BackColor = Color.FromArgb(190, 40, 40);
        btnCerrar.ForeColor = Color.White;
    }

    private void BtnCerrar_MouseLeave(object? sender, EventArgs e) {
        btnCerrar.BackColor = Color.FromArgb(20, 16, 30);
        btnCerrar.ForeColor = Color.White;
    }

    private void BtnVentana_MouseLeave(object? sender, EventArgs e) {
        Button? boton = sender as Button;

        if (boton != null) {
            boton.BackColor = Color.FromArgb(20, 16, 30);
            boton.ForeColor = Color.White;
        }
    }

    private void PanelBarraTitulo_MouseDown(object? sender, MouseEventArgs e) {
        if (!transicionandoDesdeBienvenida && e.Button == MouseButtons.Left) {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }
    }

    private void CentrarPanelPrincipal() {
        if (rutaAprendizajeInmersivaActiva) {
            Rectangle area = ClientRectangle;
            int margen = EscalarDiseno(24);
            int limiteSuperior = Math.Max(area.Top, panelBarraTitulo.Bottom);
            int ancho = Math.Max(1, area.Width - margen * 2);
            int alto = Math.Max(
                1,
                area.Bottom - limiteSuperior - margen * 2);

            AplicarBoundsSiCambian(
                panelPrincipal,
                new Rectangle(
                    area.Left + margen,
                    limiteSuperior + margen,
                    ancho,
                    alto));
            return;
        }

        if (evaluacionInmersivaAmpliaActiva) {
            int limiteSuperior = Math.Max(ClientRectangle.Top, panelBarraTitulo.Bottom);
            AplicarBoundsSiCambian(
                panelPrincipal,
                new Rectangle(
                    ClientRectangle.Left,
                    limiteSuperior,
                    Math.Max(1, ClientRectangle.Width),
                    Math.Max(1, ClientRectangle.Bottom - limiteSuperior)));
            return;
        }

        if (modoCursoInmersivo) {
            Rectangle area = ClientRectangle;
            int limiteSuperior = Math.Max(area.Top, panelBarraTitulo.Bottom);
            int margen = EscalarDiseno(24);
            int anchoDisponible = Math.Max(1, area.Width - margen * 2);
            int altoDisponible = Math.Max(
                1,
                area.Bottom - limiteSuperior - margen * 2);
            bool usarDistribucionAmplia =
                anchoDisponible >= EscalarDiseno(1200);

            if (usarDistribucionAmplia) {
                int margenInferior = EscalarDiseno(18);
                int altoAmplio = Math.Max(
                    1,
                    area.Bottom - limiteSuperior - margen - margenInferior);
                int anchoMinimo = Math.Min(
                    anchoDisponible,
                    EscalarDiseno(820));
                int anchoMaximo = Math.Min(
                    anchoDisponible,
                    EscalarDiseno(1050));
                int anchoReservandoPersonaje = Math.Max(
                    1,
                    (int)Math.Round(anchoDisponible * 0.72D));
                int anchoAmplio = Math.Clamp(
                    anchoReservandoPersonaje,
                    anchoMinimo,
                    Math.Max(anchoMinimo, anchoMaximo));

                AplicarBoundsSiCambian(
                    panelPrincipal,
                    new Rectangle(
                        area.Left + margen,
                        limiteSuperior + margen,
                        anchoAmplio,
                        altoAmplio));
                return;
            }

            anchoDisponible = Math.Max(1, ClientSize.Width - 48);
            altoDisponible = Math.Max(
                1,
                ClientSize.Height - limiteSuperior - 48);
            int expansionHorizontal = Math.Max(140, tamanoPanelPrincipalNormal.Width * 18 / 100);
            int expansionVertical = Math.Max(120, tamanoPanelPrincipalNormal.Height * 32 / 100);
            int ancho = Math.Min(
                anchoDisponible,
                tamanoPanelPrincipalNormal.Width + expansionHorizontal);
            int alto = Math.Min(
                altoDisponible,
                tamanoPanelPrincipalNormal.Height + expansionVertical);

            AplicarBoundsSiCambian(
                panelPrincipal,
                new Rectangle(
                    Math.Max(area.Left, area.Left + (area.Width - ancho) / 2),
                    Math.Max(
                        limiteSuperior,
                        limiteSuperior +
                            (area.Bottom - limiteSuperior - alto) / 2),
                    Math.Max(1, ancho),
                    Math.Max(1, alto)));
            return;
        }

        if (distribucionPanelPrincipal == DistribucionPanelPrincipal.Curso) {
            Rectangle limitesCurso =
                CalculadorGeometriaNavegacionCurso.CalcularPanelPrincipalConMenu(
                    ClientRectangle,
                    Math.Max(ClientRectangle.Top, panelBarraTitulo.Bottom),
                    panelMenu.Visible
                        ? Math.Max(ClientRectangle.Left, panelMenu.Right)
                        : ClientRectangle.Left,
                    EscalarDiseno(24));

            AplicarBoundsSiCambian(panelPrincipal, limitesCurso);
            return;
        }

        if (distribucionPanelPrincipal != DistribucionPanelPrincipal.Normal) {
            Rectangle areaFondo = fondoEndForge.ClientRectangle;
            int limiteIzquierdo = panelMenu.Visible
                ? Math.Max(areaFondo.Left, panelMenu.Right)
                : areaFondo.Left;
            int limiteSuperior = Math.Max(areaFondo.Top, panelBarraTitulo.Bottom);
            int anchoArea = Math.Max(1, areaFondo.Right - limiteIzquierdo);
            int altoArea = Math.Max(1, areaFondo.Bottom - limiteSuperior);
            int margen = EscalarDiseno(24);
            int anchoUtil = Math.Max(1, anchoArea - margen * 2);
            int altoUtil = Math.Max(1, altoArea - margen * 2);
            int ancho;
            int alto;

            if (distribucionPanelPrincipal is
                DistribucionPanelPrincipal.Inicio or
                DistribucionPanelPrincipal.Curso or
                DistribucionPanelPrincipal.Estadisticas or
                DistribucionPanelPrincipal.Logros) {
                ancho = anchoUtil;
                alto = altoUtil;
            } else {
                int anchoMaximo = EscalarDiseno(1040);
                int altoMaximo = EscalarDiseno(590);
                int anchoMinimo = Math.Min(tamanoPanelPrincipalNormal.Width, anchoUtil);
                int altoMinimo = Math.Min(tamanoPanelPrincipalNormal.Height, altoUtil);
                ancho = Math.Max(anchoMinimo, Math.Min(anchoUtil, anchoMaximo));
                alto = Math.Max(altoMinimo, Math.Min(altoUtil, altoMaximo));
            }

            AplicarBoundsSiCambian(
                panelPrincipal,
                new Rectangle(
                    limiteIzquierdo + Math.Max(0, (anchoArea - ancho) / 2),
                    limiteSuperior + Math.Max(0, (altoArea - alto) / 2),
                    ancho,
                    alto));
            return;
        }

        Size tamano = tamanoPanelPrincipalNormal.IsEmpty
            ? panelPrincipal.Size
            : tamanoPanelPrincipalNormal;
        int anchoMenu = panelMenu.Visible ? panelMenu.Width : 0;
        int espacioDisponible = ClientSize.Width - anchoMenu;
        int x = anchoMenu + (espacioDisponible - tamano.Width) / 2;
        int y = (ClientSize.Height - tamano.Height) / 2;

        AplicarBoundsSiCambian(
            panelPrincipal,
            new Rectangle(x, y, tamano.Width, tamano.Height));
    }

    private int EscalarDiseno(int valor) {
        return Math.Max(1, (int)Math.Round(valor * DeviceDpi / 96D));
    }

    private void RecalcularDistribucionActual() {
        timerRecalcularVista.Stop();
        CentrarPanelPrincipal();
        SincronizarLimitesVistasAdaptables();
        RecalcularDistribucionCurso();
        AjustarGeometriaNuevaPractica();
        RecalcularGeometriaEstadisticas();
        ActualizarGeometriaInicio();
        ActualizarGeometriaLogros();
    }

    private void SincronizarLimitesVistasAdaptables() {
        Rectangle limites = panelPrincipal.ClientRectangle;
        Rectangle limitesNormalizados = new(
            limites.Left,
            limites.Top,
            Math.Max(1, limites.Width),
            Math.Max(1, limites.Height));

        if (distribucionPanelPrincipal == DistribucionPanelPrincipal.Inicio ||
            panelInicioVista.Visible) {
            AplicarBoundsSiCambian(panelInicioVista, limitesNormalizados);
        }

        if (distribucionPanelPrincipal == DistribucionPanelPrincipal.Logros ||
            (estructuraLogrosInicializada && panelLogrosVista.Visible)) {
            SincronizarLimitesVistaLogros(limites);
        }

        if (distribucionPanelPrincipal ==
                DistribucionPanelPrincipal.NuevaPractica ||
            panelVistaNuevaPractica.Visible) {
            AplicarBoundsSiCambian(panelVistaNuevaPractica, limitesNormalizados);
        }

        if (estructuraEstadisticasInicializada &&
            (distribucionPanelPrincipal ==
                DistribucionPanelPrincipal.Estadisticas ||
             panelEstadisticasVista.Visible)) {
            AplicarBoundsSiCambian(panelEstadisticasVista, limitesNormalizados);
        }

        if (!DebeRecalcularDistribucionCurso(
                cursoInicializado,
                distribucionPanelPrincipal == DistribucionPanelPrincipal.Curso,
                modoCursoInmersivo,
                vistaRutaActual != VistaRutaAprendizaje.Ninguna)) {
            return;
        }

        foreach (Control vista in ObtenerVistasRutaAprendizaje()) {
            AplicarBoundsSiCambian(vista, limitesNormalizados);
        }
    }

    internal static bool AplicarBoundsSiCambian(
        Control control,
        Rectangle limites) {
        ArgumentNullException.ThrowIfNull(control);

        if (control.Bounds == limites) {
            return false;
        }

        control.Bounds = limites;
        return true;
    }

    private void FrmPrincipal_Resize(object? sender, EventArgs e) {
        timerRecalcularVista.Stop();
        FormWindowState estadoAnterior = ultimoEstadoVentana;
        ultimoEstadoVentana = WindowState;

        if (WindowState == FormWindowState.Minimized) {
            return;
        }

        ActualizarBotonMaximizar();

        if (transicionandoDesdeBienvenida || transicionVisualCursoActiva) {
            recalculoPendienteDuranteTransicion = true;
            return;
        }

        if (DebeRecalcularInmediatamentePorCambioEstadoVentana(
                estadoAnterior,
                WindowState)) {
            recalculoPendienteDuranteTransicion = false;
            EjecutarRecalculoVistaFinal();
            return;
        }

        if (GeometriaVistaSigueVigente(
                ClientSize,
                DeviceDpi,
                ultimoTamanoVistaRecalculado,
                ultimoDpiVistaRecalculado)) {
            return;
        }

        timerRecalcularVista.Start();
    }

    internal static bool DebeRecalcularInmediatamentePorCambioEstadoVentana(
        FormWindowState estadoAnterior,
        FormWindowState estadoActual) {
        return estadoActual != FormWindowState.Minimized &&
            estadoActual != estadoAnterior;
    }

    internal static bool GeometriaVistaSigueVigente(
        Size tamanoActual,
        int dpiActual,
        Size ultimoTamano,
        int ultimoDpi) {
        return tamanoActual == ultimoTamano && dpiActual == ultimoDpi;
    }

    private void TimerRecalcularVista_Tick(object? sender, EventArgs e) {
        timerRecalcularVista.Stop();

        if (recalculandoVista || WindowState == FormWindowState.Minimized) {
            return;
        }

        if (transicionandoDesdeBienvenida || transicionVisualCursoActiva) {
            recalculoPendienteDuranteTransicion = true;
            return;
        }

        recalculoPendienteDuranteTransicion = false;
        EjecutarRecalculoVistaFinal();
    }

    private void AplicarRecalculoPendienteDespuesDeTransicionCurso() {
        timerRecalcularVista.Stop();

        if (!recalculoPendienteDuranteTransicion ||
            transicionandoDesdeBienvenida ||
            transicionVisualCursoActiva ||
            WindowState == FormWindowState.Minimized) {
            return;
        }

        recalculoPendienteDuranteTransicion = false;
        EjecutarRecalculoVistaFinal();
    }

    private void EjecutarRecalculoVistaFinal() {
        if (recalculandoVista || WindowState == FormWindowState.Minimized) {
            recalculoPendienteDuranteTransicion = true;
            return;
        }

        RegistrarTiempoInicio("Resize final: inicio de recálculo geométrico");
        recalculandoVista = true;
        fondoEndForge.SuspendLayout();
        panelPrincipal.SuspendLayout();

        try {
            RecalcularDistribucionActual();

            if (panelPantallaBienvenida.Visible) {
                CentrarContenidoBienvenida();
                if (panelPantallaBienvenida.ActualizarCacheImagenFondo()) {
                    panelPantallaBienvenida.Invalidate();
                }
            }
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
            fondoEndForge.ResumeLayout(performLayout: false);
            recalculandoVista = false;
        }

        fondoEndForge.ActualizarCacheImagenFondo();
        fondoEndForge.Invalidate();
        panelPrincipal.Invalidate();
        ultimoTamanoVistaRecalculado = ClientSize;
        ultimoDpiVistaRecalculado = DeviceDpi;
        RegistrarTiempoInicio("Resize final: recálculo e invalidación terminados");
    }
}
