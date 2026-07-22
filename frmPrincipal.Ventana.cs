using System.Runtime.InteropServices;

namespace EndForge;

public partial class frmPrincipal {
    private enum DistribucionPanelPrincipal {
        Normal,
        Curso,
        NuevaPractica,
        Estadisticas
    }

    private System.Windows.Forms.Timer timerRecalcularVista = new System.Windows.Forms.Timer();
    private bool recalculandoVista;
    private bool recalculoPendienteDuranteTransicion;
    private DistribucionPanelPrincipal distribucionPanelPrincipal;

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

        fondoEndForge.Invalidate(true);
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

            panelPrincipal.SetBounds(
                area.Left + margen,
                limiteSuperior + margen,
                ancho,
                alto);
            return;
        }

        if (evaluacionInmersivaAmpliaActiva) {
            int limiteSuperior = Math.Max(ClientRectangle.Top, panelBarraTitulo.Bottom);
            panelPrincipal.SetBounds(
                ClientRectangle.Left,
                limiteSuperior,
                Math.Max(1, ClientRectangle.Width),
                Math.Max(1, ClientRectangle.Bottom - limiteSuperior));
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

                panelPrincipal.SetBounds(
                    area.Left + margen,
                    limiteSuperior + margen,
                    anchoAmplio,
                    altoAmplio);
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

            panelPrincipal.SetBounds(
                Math.Max(area.Left, area.Left + (area.Width - ancho) / 2),
                Math.Max(
                    limiteSuperior,
                    limiteSuperior + (area.Bottom - limiteSuperior - alto) / 2),
                Math.Max(1, ancho),
                Math.Max(1, alto));
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
                DistribucionPanelPrincipal.Curso or
                DistribucionPanelPrincipal.Estadisticas) {
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

            panelPrincipal.SetBounds(
                limiteIzquierdo + Math.Max(0, (anchoArea - ancho) / 2),
                limiteSuperior + Math.Max(0, (altoArea - alto) / 2),
                ancho,
                alto);
            return;
        }

        if (!tamanoPanelPrincipalNormal.IsEmpty) {
            panelPrincipal.Size = tamanoPanelPrincipalNormal;
        }

        int anchoMenu = panelMenu.Visible ? panelMenu.Width : 0;
        int espacioDisponible = ClientSize.Width - anchoMenu;

        int x = anchoMenu + (espacioDisponible - panelPrincipal.Width) / 2;
        int y = (ClientSize.Height - panelPrincipal.Height) / 2;

        panelPrincipal.Location = new Point(x, y);
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
    }

    private void SincronizarLimitesVistasAdaptables() {
        Rectangle limites = panelPrincipal.ClientRectangle;
        panelVistaNuevaPractica.SetBounds(
            limites.Left,
            limites.Top,
            Math.Max(1, limites.Width),
            Math.Max(1, limites.Height));

        if (estructuraEstadisticasInicializada) {
            panelEstadisticasVista.SetBounds(
                limites.Left,
                limites.Top,
                Math.Max(1, limites.Width),
                Math.Max(1, limites.Height));
        }

        if (!cursoInicializado) {
            return;
        }

        foreach (Control vista in ObtenerVistasRutaAprendizaje()) {
            vista.SetBounds(
                limites.Left,
                limites.Top,
                Math.Max(1, limites.Width),
                Math.Max(1, limites.Height));
        }
    }

    private void FrmPrincipal_Resize(object? sender, EventArgs e) {
        timerRecalcularVista.Stop();

        if (WindowState == FormWindowState.Minimized) {
            return;
        }

        ActualizarBotonMaximizar();

        if (transicionandoDesdeBienvenida || transicionVisualCursoActiva) {
            recalculoPendienteDuranteTransicion = true;
            return;
        }

        timerRecalcularVista.Start();
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
            }
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
            fondoEndForge.ResumeLayout(performLayout: false);
            recalculandoVista = false;
        }

        fondoEndForge.Invalidate();
        panelPrincipal.Invalidate();
        RegistrarTiempoInicio("Resize final: recálculo e invalidación terminados");
    }
}
