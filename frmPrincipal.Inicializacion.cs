namespace EndForge;

public partial class frmPrincipal {
    private void ConfigurarBarraTitulo() {
        btnCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(190, 40, 40);
        btnCerrar.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 25, 25);

        PosicionarBotonesBarraTitulo();
        panelBarraTitulo.SizeChanged += (s, e) => PosicionarBotonesBarraTitulo();

        btnMinimizar.Click += BtnMinimizar_Click;
        btnMaximizar.Click += BtnMaximizar_Click;
        btnCerrar.Click += BtnCerrar_Click;

        btnCerrar.MouseEnter += BtnCerrar_MouseEnter;
        btnCerrar.MouseLeave += BtnCerrar_MouseLeave;

        btnMinimizar.MouseLeave += BtnVentana_MouseLeave;
        btnMaximizar.MouseLeave += BtnVentana_MouseLeave;

        panelBarraTitulo.MouseDown += PanelBarraTitulo_MouseDown;
        lblBarraTitulo.MouseDown += PanelBarraTitulo_MouseDown;
        pictureBoxBarraIcono.MouseDown += PanelBarraTitulo_MouseDown;
    }

    private void ConfigurarVentana() {
        panelPrincipal.BackgroundImage = null;
        panelPrincipal.BackColor = Color.Transparent;
        panelInicioVista.BackgroundImage = null;
        panelInicioVista.BackColor = ColorFondoInicio;
        panelRecientesVista.BackgroundImage = null;
        panelRecientesVista.BackColor = Color.Transparent;
        panelConfiguracionVista.BackgroundImage = null;
        panelConfiguracionVista.BackColor = Color.Transparent;
        panelListaRecientes.BackgroundImage = null;
        panelListaRecientes.BackColor = Color.Transparent;

        fondoEndForge.Controls.Add(panelPrincipal);
        panelPrincipal.BringToFront();

        timerRecalcularVista.Interval = 150;
        timerRecalcularVista.Tick += TimerRecalcularVista_Tick;

        Resize += FrmPrincipal_Resize;

        ActivarDobleBuffer(this);
        ActivarDobleBuffer(panelPrincipal);
        ActivarDobleBuffer(panelInicioVista);
        ActivarDobleBuffer(panelRecientesVista);
        ActivarDobleBuffer(panelConfiguracionVista);
        ActivarDobleBuffer(panelListaRecientes);
        ActivarDobleBuffer(panelVistaNuevaPractica);
        ActivarDobleBuffer(panelNuevaPracticaTarjeta);
        ActivarDobleBuffer(panelVistaPreviaNuevaPractica);
        ConfigurarDisenoNuevaPracticaAdaptable();
    }

    private void ConfigurarNavegacion() {
        panelInicio.MouseEnter += PanelMenu_MouseEnter;
        panelInicio.MouseLeave += PanelMenu_MouseLeave;
        lblInicio.MouseEnter += PanelMenu_MouseEnter;
        lblInicio.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxInicio.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxInicio.MouseLeave += PanelMenu_MouseLeave;
        panelInicio.Click += PanelInicio_Click;
        lblInicio.Click += PanelInicio_Click;
        pictureBoxInicio.Click += PanelInicio_Click;

        panelNuevaPractica.MouseEnter += PanelMenu_MouseEnter;
        panelNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
        lblNuevaPractica.MouseEnter += PanelMenu_MouseEnter;
        lblNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxNuevaPractica.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
        panelNuevaPractica.Click += panelNuevaPractica_Click;
        lblNuevaPractica.Click += panelNuevaPractica_Click;
        pictureBoxNuevaPractica.Click += panelNuevaPractica_Click;

        panelAbrirPractica.MouseEnter += PanelMenu_MouseEnter;
        panelAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
        lblAbrirPractica.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxAbrirPractica.MouseEnter += PanelMenu_MouseEnter;
        panelAbrirPractica.Click += PanelAbrirPractica_Click;
        lblAbrirPractica.Click += PanelAbrirPractica_Click;
        pictureBoxAbrirPractica.Click += PanelAbrirPractica_Click;
        lblAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxAbrirPractica.MouseLeave += PanelMenu_MouseLeave;

        panelRecientes.MouseEnter += PanelMenu_MouseEnter;
        panelRecientes.MouseLeave += PanelMenu_MouseLeave;
        lblRecientes.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxRecientes.MouseEnter += PanelMenu_MouseEnter;
        panelRecientes.Click += PanelRecientes_Click;
        lblRecientes.Click += PanelRecientes_Click;
        pictureBoxRecientes.Click += PanelRecientes_Click;
        lblRecientes.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxRecientes.MouseLeave += PanelMenu_MouseLeave;

        panelConfiguracion.MouseEnter += PanelMenu_MouseEnter;
        panelConfiguracion.MouseLeave += PanelMenu_MouseLeave;
        lblConfiguracion.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxConfiguracion.MouseEnter += PanelMenu_MouseEnter;
        panelConfiguracion.Click += PanelConfiguracion_Click;
        lblConfiguracion.Click += PanelConfiguracion_Click;
        pictureBoxConfiguracion.Click += PanelConfiguracion_Click;
        lblConfiguracion.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxConfiguracion.MouseLeave += PanelMenu_MouseLeave;

        panelAcercaDe.MouseEnter += PanelMenu_MouseEnter;
        panelAcercaDe.MouseLeave += PanelMenu_MouseLeave;
        lblAcercaDe.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxAcercaDe.MouseEnter += PanelMenu_MouseEnter;
        panelAcercaDe.Click += PanelAcercaDe_Click;
        lblAcercaDe.Click += PanelAcercaDe_Click;
        pictureBoxAcercaDe.Click += PanelAcercaDe_Click;
        lblAcercaDe.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxAcercaDe.MouseLeave += PanelMenu_MouseLeave;

    }

    private void ConfigurarRecientes() {
        foreach (Label label in ObtenerLabelsRecientes()) {
            label.DoubleClick += LabelReciente_DoubleClick;
        }
    }

    private void ConfigurarEstadoInicial() {
        ultimoEstadoVentana = WindowState;
        ultimoTamanoVistaRecalculado = ClientSize;
        ultimoDpiVistaRecalculado = DeviceDpi;
        distribucionPanelPrincipal = DistribucionPanelPrincipal.Inicio;
        panelSeleccionado = panelInicio;
        panelInicio.BackColor = Color.FromArgb(111, 45, 189);
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;
        OcultarVistaLogros();
        OcultarVistasCurso();

        CentrarPanelPrincipal();
        SincronizarLimitesVistasAdaptables();
        PrepararInicioParaMostrar();
        InvalidarFondoContinuo();
    }
}
