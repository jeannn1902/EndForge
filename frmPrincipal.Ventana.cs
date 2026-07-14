using System.Runtime.InteropServices;

namespace EndForge;

public partial class frmPrincipal {
    private System.Windows.Forms.Timer timerRecalcularVista = new System.Windows.Forms.Timer();

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

    private void AplicarFondoDinamicoPanelPrincipal() {
        Image? fondoAnterior = panelPrincipal.BackgroundImage;
        panelPrincipal.BackgroundImage = CrearRecorteFondoParaPanel(panelPrincipal);
        fondoAnterior?.Dispose();
        
        panelPrincipal.BackgroundImageLayout = ImageLayout.Stretch;
        panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

        panelInicioVista.BackgroundImage = null;
        panelInicioVista.BackColor = Color.Transparent;

        panelRecientesVista.BackgroundImage = null;
        panelRecientesVista.BackColor = Color.Transparent;

        panelConfiguracionVista.BackgroundImage = null;
        panelConfiguracionVista.BackColor = Color.Transparent;

        panelListaRecientes.BackgroundImage = null;
        panelListaRecientes.BackColor = Color.Transparent;
    }

    private void ReaplicarFondoDinamico() {
        if (fondoEndForge.Width <= 0 || fondoEndForge.Height <= 0)
            return;

        if (panelPrincipal.Width <= 0 || panelPrincipal.Height <= 0)
            return;

        AplicarFondoDinamicoPanelPrincipal();
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
        WindowState = FormWindowState.Minimized;
    }

    private void BtnMaximizar_Click(object? sender, EventArgs e) {
        if (WindowState == FormWindowState.Maximized) {
            WindowState = FormWindowState.Normal;
        } else {
            WindowState = FormWindowState.Maximized;
        }

        ActualizarBotonMaximizar();
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
        if (e.Button == MouseButtons.Left) {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
        }
    }

    private Image CrearRecorteFondoParaPanel(Control panelDestino) {
        Bitmap fondoRenderizado = new Bitmap(fondoEndForge.Width, fondoEndForge.Height);
        fondoEndForge.DrawToBitmap(
            fondoRenderizado,
            new Rectangle(0, 0, fondoEndForge.Width, fondoEndForge.Height)
        );

        Point posicionEnFondo = fondoEndForge.PointToClient(
            panelDestino.PointToScreen(Point.Empty)
        );

        Rectangle zonaRecorte = new Rectangle(
            posicionEnFondo.X,
            posicionEnFondo.Y,
            panelDestino.Width,
            panelDestino.Height
        );

        Bitmap recorte = new Bitmap(panelDestino.Width, panelDestino.Height);

        using (Graphics g = Graphics.FromImage(recorte)) {
            g.DrawImage(
                fondoRenderizado,
                new Rectangle(0, 0, panelDestino.Width, panelDestino.Height),
                zonaRecorte,
                GraphicsUnit.Pixel
            );
        }

        fondoRenderizado.Dispose();

        return recorte;
    }

    private void CentrarPanelPrincipal() {
        int espacioDisponible = ClientSize.Width - panelMenu.Width;

        int x = panelMenu.Width + (espacioDisponible - panelPrincipal.Width) / 2;
        int y = (ClientSize.Height - panelPrincipal.Height) / 2;

        panelPrincipal.Location = new Point(x, y);
    }

    private void FrmPrincipal_Resize(object? sender, EventArgs e) {
        if (WindowState == FormWindowState.Minimized)
            return;

        ActualizarBotonMaximizar();

        timerRecalcularVista.Stop();
        timerRecalcularVista.Start();
    }

    private void TimerRecalcularVista_Tick(object? sender, EventArgs e) {
        timerRecalcularVista.Stop();

        CentrarPanelPrincipal();

        fondoEndForge.Refresh();
        fondoEndForge.Update();

        AplicarFondoDinamicoPanelPrincipal();

        panelPrincipal.Refresh();
        panelPrincipal.Update();

        Refresh();
    }
}
