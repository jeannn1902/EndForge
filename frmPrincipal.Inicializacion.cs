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

    private void ConfigurarTarjetasInicio() {
        lblCardNuevaPracticaTitulo.MouseEnter += CardInicio_MouseEnter;
        lblCardNuevaPracticaTitulo.MouseLeave += CardInicio_MouseLeave;
        lblCardNuevaPracticaDesc.MouseEnter += CardInicio_MouseEnter;
        lblCardNuevaPracticaDesc.MouseLeave += CardInicio_MouseLeave;

        lblCardRecientesTitulo.MouseEnter += CardInicio_MouseEnter;
        lblCardRecientesTitulo.MouseLeave += CardInicio_MouseLeave;
        lblCardRecientesDesc.MouseEnter += CardInicio_MouseEnter;
        lblCardRecientesDesc.MouseLeave += CardInicio_MouseLeave;

        lblCardConfiguracionTitulo.MouseEnter += CardInicio_MouseEnter;
        lblCardConfiguracionTitulo.MouseLeave += CardInicio_MouseLeave;
        lblCardConfiguracionDesc.MouseEnter += CardInicio_MouseEnter;
        lblCardConfiguracionDesc.MouseLeave += CardInicio_MouseLeave;

        panelCardContinuar.MouseEnter += CardInicio_MouseEnter;
        panelCardContinuar.MouseLeave += CardInicio_MouseLeave;

        lblCardContinuarTitulo.MouseEnter += CardInicio_MouseEnter;
        lblCardContinuarTitulo.MouseLeave += CardInicio_MouseLeave;

        lblCardContinuarDesc.MouseEnter += CardInicio_MouseEnter;
        lblCardContinuarDesc.MouseLeave += CardInicio_MouseLeave;

        panelCardNuevaPractica.MouseEnter += CardInicio_MouseEnter;
        panelCardNuevaPractica.MouseLeave += CardInicio_MouseLeave;

        panelCardRecientes.MouseEnter += CardInicio_MouseEnter;
        panelCardRecientes.MouseLeave += CardInicio_MouseLeave;

        panelCardConfiguracion.MouseEnter += CardInicio_MouseEnter;
        panelCardConfiguracion.MouseLeave += CardInicio_MouseLeave;
    }

    private void ConfigurarVentana() {
        timerRecalcularVista.Interval = 150;
        timerRecalcularVista.Tick += TimerRecalcularVista_Tick;

        Resize += FrmPrincipal_Resize;

        ActivarDobleBuffer(this);
        ActivarDobleBuffer(panelPrincipal);
        ActivarDobleBuffer(panelInicioVista);
        ActivarDobleBuffer(panelRecientesVista);
        ActivarDobleBuffer(panelConfiguracionVista);
        ActivarDobleBuffer(panelListaRecientes);
    }

    private void ConfigurarNavegacion() {
        panelInicio.MouseEnter += PanelMenu_MouseEnter;
        panelInicio.MouseLeave += PanelMenu_MouseLeave;
        lblInicio.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxInicio.MouseLeave += PanelMenu_MouseLeave;
        panelInicio.Click += PanelInicio_Click;
        lblInicio.Click += PanelInicio_Click;
        pictureBoxInicio.Click += PanelInicio_Click;

        panelNuevaPractica.MouseEnter += PanelMenu_MouseEnter;
        panelNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
        lblNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
        panelNuevaPractica.Click += panelNuevaPractica_Click;
        lblNuevaPractica.Click += panelNuevaPractica_Click;
        pictureBoxNuevaPractica.Click += panelNuevaPractica_Click;

        panelAbrirPractica.MouseEnter += PanelMenu_MouseEnter;
        panelAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
        panelAbrirPractica.Click += PanelAbrirPractica_Click;
        lblAbrirPractica.Click += PanelAbrirPractica_Click;
        pictureBoxAbrirPractica.Click += PanelAbrirPractica_Click;
        lblAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxAbrirPractica.MouseLeave += PanelMenu_MouseLeave;

        panelRecientes.MouseEnter += PanelMenu_MouseEnter;
        panelRecientes.MouseLeave += PanelMenu_MouseLeave;
        panelRecientes.Click += PanelRecientes_Click;
        lblRecientes.Click += PanelRecientes_Click;
        pictureBoxRecientes.Click += PanelRecientes_Click;
        lblRecientes.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxRecientes.MouseLeave += PanelMenu_MouseLeave;

        panelConfiguracion.MouseEnter += PanelMenu_MouseEnter;
        panelConfiguracion.MouseLeave += PanelMenu_MouseLeave;
        panelConfiguracion.Click += PanelConfiguracion_Click;
        lblConfiguracion.Click += PanelConfiguracion_Click;
        pictureBoxConfiguracion.Click += PanelConfiguracion_Click;
        lblConfiguracion.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxConfiguracion.MouseLeave += PanelMenu_MouseLeave;

        panelAcercaDe.MouseEnter += PanelMenu_MouseEnter;
        panelAcercaDe.MouseLeave += PanelMenu_MouseLeave;
        panelAcercaDe.Click += PanelAcercaDe_Click;
        lblAcercaDe.Click += PanelAcercaDe_Click;
        pictureBoxAcercaDe.Click += PanelAcercaDe_Click;
        lblAcercaDe.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxAcercaDe.MouseLeave += PanelMenu_MouseLeave;

        panelCardNuevaPractica.Click += panelNuevaPractica_Click;
        panelCardConfiguracion.Click += PanelConfiguracion_Click;

        lblCardNuevaPracticaTitulo.Click += panelNuevaPractica_Click;
        lblCardNuevaPracticaDesc.Click += panelNuevaPractica_Click;

        lblCardConfiguracionTitulo.Click += PanelConfiguracion_Click;
        lblCardConfiguracionDesc.Click += PanelConfiguracion_Click;
    }

    private void ConfigurarRecientes() {
        foreach (Label label in ObtenerLabelsRecientes()) {
            label.DoubleClick += LabelReciente_DoubleClick;
        }
    }

    private void ConfigurarEstadoInicial() {
        panelSeleccionado = panelInicio;
        panelInicio.BackColor = Color.FromArgb(111, 45, 189);
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;

        CentrarPanelPrincipal();
        AplicarFondoDinamicoPanelPrincipal();

        panelPrincipal.Invalidate(true);
        panelInicioVista.Invalidate(true);
        fondoEndForge.Invalidate(true);
    }
}
