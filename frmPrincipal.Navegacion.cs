using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private bool eventosVistasRutaAprendizajeConfigurados;
    private bool rutaAprendizajeInmersivaActiva;

    private void ConfigurarEventosVistasRutaAprendizaje() {
        if (eventosVistasRutaAprendizajeConfigurados || !cursoInicializado) {
            return;
        }

        panelGradosVista.VisibleChanged += PanelGradosVista_VisibleChanged;
        panelCursoVista.VisibleChanged += PanelDetalleGradoVista_VisibleChanged;
        eventosVistasRutaAprendizajeConfigurados = true;
    }

    private void PanelGradosVista_VisibleChanged(object? sender, EventArgs e) {
        if (!panelGradosVista.Visible) {
            return;
        }

        if (rutaAprendizajeInmersivaActiva &&
            modoCursoInmersivo &&
            !panelMenu.Visible) {
            return;
        }

        AplicarGeometriaRutaAprendizajeInmersiva();
    }

    private void PanelDetalleGradoVista_VisibleChanged(object? sender, EventArgs e) {
        if (!panelCursoVista.Visible || navegacionCursoEnCurso) {
            return;
        }

        RestaurarNavegacionDetalleGrado();
    }

    private void MostrarRutaAprendizajeInmersiva(
        bool reconstruirContenido,
        bool invalidarFondo = true,
        bool prepararContenidoAntesDeMostrar = false) {
        if (!cursoPreparado) {
            return;
        }

        ConfigurarEventosVistasRutaAprendizaje();
        OcultarVistasPrincipalesFueraDelCurso();
        SeleccionarPanelMenu(panelCurso);
        AplicarGeometriaRutaAprendizajeInmersiva(invalidarFondo);

        if (reconstruirContenido && prepararContenidoAntesDeMostrar) {
            AsegurarVistaGradosVigente(volverAlInicio: false);
        }

        MostrarSubvistaCurso(panelGradosVista, VistaRutaAprendizaje.Grados);

        if (reconstruirContenido && !prepararContenidoAntesDeMostrar) {
            AsegurarVistaGradosVigente(volverAlInicio: false);
        }
    }

    private void AplicarGeometriaRutaAprendizajeInmersiva(bool invalidarFondo = true) {
        timerRecalcularVista.Stop();
        rutaAprendizajeInmersivaActiva = true;
        modoCursoInmersivo = true;
        distribucionPanelPrincipal = DistribucionPanelPrincipal.Curso;
        panelMenu.Visible = false;
        panelPrincipal.Visible = true;
        RecalcularDistribucionActual();
        panelBarraTitulo.BringToFront();

        if (invalidarFondo && !transicionandoDesdeBienvenida) {
            InvalidarFondoContinuo();
        }
    }

    private void RestaurarNavegacionDetalleGrado() {
        bool requiereRecalculo =
            rutaAprendizajeInmersivaActiva ||
            modoCursoInmersivo ||
            !panelMenu.Visible;

        rutaAprendizajeInmersivaActiva = false;
        modoCursoInmersivo = false;
        distribucionPanelPrincipal = DistribucionPanelPrincipal.Curso;
        panelMenu.Visible = true;
        panelPrincipal.Visible = true;
        SeleccionarPanelMenu(panelCurso);

        if (requiereRecalculo) {
            RecalcularDistribucionActual();
        }

        panelMenu.BringToFront();
        panelBarraTitulo.BringToFront();
    }

    private void PrepararNavegacionPrincipalDesdeRuta() {
        rutaAprendizajeInmersivaActiva = false;
    }

    private void OcultarVistasPrincipalesFueraDelCurso() {
        panelInicioVista.Visible = false;
        OcultarVistaLogros();
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;
        OcultarVistaEstadisticas();
    }

    private void PanelMenu_MouseEnter(object? sender, EventArgs e) {
        Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

        if (panel != null) {
            if (panel != panelSeleccionado) {
                panel.BackColor = Color.FromArgb(74, 35, 110);
            }
        }
    }

    private void PanelMenu_MouseLeave(object? sender, EventArgs e) {
        Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

        if (panel != null) {
            if (!panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position))) {
                if (panel != panelSeleccionado) {
                    panel.BackColor = Color.FromArgb(45, 45, 48);
                }
            }
        }
    }

    private void SeleccionarPanelMenu(Panel panel) {
        if (panelSeleccionado == panel) {
            return;
        }

        panelSeleccionado.BackColor = Color.FromArgb(45, 45, 48);
        panel.BackColor = Color.FromArgb(111, 45, 189);

        panelSeleccionado = panel;
    }

    private void NavegarVistaPrincipalConTransicion(
        Panel vistaDestino,
        Panel opcionMenu,
        DistribucionPanelPrincipal distribucion,
        Action? prepararContenido = null,
        bool omitirTransicionDuranteInicializacion = false) {
        if (navegacionCursoEnCurso || transicionVisualCursoActiva) {
            return;
        }

        if (vistaDestino.Visible && panelSeleccionado == opcionMenu) {
            prepararContenido?.Invoke();
            return;
        }

        if (omitirTransicionDuranteInicializacion && inicializacionSecundariaEnCurso) {
            PrepararYMostrarVistaPrincipal(
                vistaDestino,
                opcionMenu,
                distribucion,
                prepararContenido,
                usarTransicion: false);
            return;
        }

        if (!IniciarTransicionVisualCurso(vistaDestino)) {
            return;
        }

        PrepararDestinoDespuesDeCubiertaTransicionCurso(
            () => PrepararYMostrarVistaPrincipal(
                vistaDestino,
                opcionMenu,
                distribucion,
                prepararContenido,
                usarTransicion: true));
    }

    private void PrepararYMostrarVistaPrincipal(
        Panel vistaDestino,
        Panel opcionMenu,
        DistribucionPanelPrincipal distribucion,
        Action? prepararContenido,
        bool usarTransicion) {
        panelPrincipal.SuspendLayout();

        try {
            PrepararNavegacionPrincipalDesdeRuta();
            panelInicioVista.Visible = false;
            OcultarVistaLogros();
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;
            panelVistaNuevaPractica.Visible = false;
            OcultarVistaEstadisticas();
            OcultarVistasCurso();

            prepararContenido?.Invoke();
            MostrarNavegacionPrincipal(distribucion, invalidarFondo: false);
            SeleccionarPanelMenu(opcionMenu);

            if (usarTransicion) {
                AjustarCubiertaTransicionCurso();
            }

            vistaDestino.Visible = true;
            vistaDestino.BringToFront();
        } catch {
            if (usarTransicion) {
                CancelarTransicionVisualCurso();
            }

            throw;
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
        }

        if (usarTransicion) {
            fondoEndForge.Invalidate();
            ConfirmarDestinoTransicionCurso(vistaDestino);
        } else {
            InvalidarFondoContinuo();
        }
    }

    private void panelNuevaPractica_Click(object? sender, EventArgs e) {
        if (navegacionCursoEnCurso || transicionVisualCursoActiva) {
            return;
        }

        OcultarVistaEstadisticas();
        PrepararNavegacionPrincipalDesdeRuta();
        MostrarNavegacionPrincipal(DistribucionPanelPrincipal.NuevaPractica);
        SeleccionarPanelMenu(panelNuevaPractica);

        panelInicioVista.Visible = false;
        OcultarVistaLogros();
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = true;
        OcultarVistasCurso();

        panelVistaNuevaPractica.BringToFront();
        fondoEndForge.SendToBack();
    }

    private void PanelInicio_Click(object? sender, EventArgs e) {
        NavegarVistaPrincipalConTransicion(
            panelInicioVista,
            panelInicio,
            DistribucionPanelPrincipal.Inicio,
            PrepararInicioParaMostrar);
    }

    private async void PanelAbrirPractica_Click(object? sender, EventArgs e) {
        if (navegacionCursoEnCurso || transicionVisualCursoActiva) {
            return;
        }

        Panel panelAnterior = panelSeleccionado;

        using FolderBrowserDialog carpeta = new() {
            Description = "Selecciona la carpeta del proyecto"
        };

        DialogResult resultado = carpeta.ShowDialog(this);

        if (resultado != DialogResult.OK) {
            RestaurarColorPanel(panelAbrirPractica);
            SeleccionarPanelMenu(panelAnterior);
            return;
        }

        bool aperturaExitosa = await IntentarAbrirPracticaAsync(
            carpeta.SelectedPath,
            promoverReciente: true
        );

        if (IsDisposed || Disposing) {
            return;
        }

        if (!aperturaExitosa) {
            RestaurarColorPanel(panelAbrirPractica);
        }

        SeleccionarPanelMenu(panelAnterior);
    }

    private void RestaurarColorPanel(Panel panel) {
        if (panel == panelSeleccionado) {
            panel.BackColor = Color.ForestGreen;
        } else {
            panel.BackColor = Color.FromArgb(45, 45, 48);
        }
    }

    private void PanelRecientes_Click(object? sender, EventArgs e) {
        NavegarVistaPrincipalConTransicion(
            panelRecientesVista,
            panelRecientes,
            DistribucionPanelPrincipal.Normal,
            () => CargarRecientes());
    }

    private void PanelConfiguracion_Click(object? sender, EventArgs e) {
        NavegarVistaPrincipalConTransicion(
            panelConfiguracionVista,
            panelConfiguracion,
            DistribucionPanelPrincipal.Normal,
            MostrarAvisoPreferenciasSiCorresponde,
            omitirTransicionDuranteInicializacion: true);
    }

    private void PanelAcercaDe_Click(object? sender, EventArgs e) {
        if (navegacionCursoEnCurso || transicionVisualCursoActiva) {
            return;
        }

        PrepararNavegacionPrincipalDesdeRuta();
        Panel panelAnterior = panelSeleccionado;
        bool logrosEraVisible =
            estructuraLogrosInicializada && panelLogrosVista.Visible;

        if (modoCursoInmersivo) {
            MostrarCursoPrincipal();
        } else if (panelAnterior == panelEstadisticas) {
            MostrarNavegacionPrincipal(DistribucionPanelPrincipal.Estadisticas);
        } else if (logrosEraVisible) {
            MostrarNavegacionPrincipal(DistribucionPanelPrincipal.Logros);
        } else {
            MostrarNavegacionPrincipal();
        }

        SeleccionarPanelMenu(panelAcercaDe);
        panelVistaNuevaPractica.Visible = false;

        MessageBox.Show(
            "EndForge 1.0\n\n" +
            "Desarrollado por:\n" +
            "Jeancarlo Pérez Pérez\n\n" +
            "Herramienta para automatizar la creación y gestión " +
            "de prácticas de C++.\n\n" +
            "© 2026",
            "Acerca de EndForge",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        if (panelAnterior == panelCurso) {
            SeleccionarPanelMenu(panelCurso);
        } else if (panelAnterior == panelEstadisticas) {
            SeleccionarPanelMenu(panelEstadisticas);
            panelEstadisticasVista.BringToFront();
        } else if (logrosEraVisible) {
            SeleccionarPanelMenu(panelInicio);
            panelLogrosVista.BringToFront();
        }
    }
}
