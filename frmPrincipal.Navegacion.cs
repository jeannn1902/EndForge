using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private void PanelMenu_MouseEnter(object? sender, EventArgs e) {
        Panel? panel = sender as Panel;

        if (panel != null) {
            if (panel != panelSeleccionado) {
                panel.BackColor = Color.FromArgb(74, 35, 110);
            }
        }
    }

    private void Card_MouseEnter(object sender, EventArgs e) {
    }

    private void Card_MouseLeave(object sender, EventArgs e) {
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

    private void panelNuevaPractica_Click(object? sender, EventArgs e) {
        SeleccionarPanelMenu(panelNuevaPractica);

        panelInicioVista.Visible = false;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = true;

        panelVistaNuevaPractica.BringToFront();
        fondoEndForge.SendToBack();
    }

    private void PanelInicio_Click(object? sender, EventArgs e) {
        SeleccionarPanelMenu(panelInicio);

        panelInicioVista.Visible = true;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;

        panelInicioVista.BringToFront();
    }

    private void CardInicio_MouseEnter(object? sender, EventArgs e) {
        Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

        if (panel != null) {
            panel.BackColor = Color.FromArgb(35, 28, 48);
        }
    }

    private void CardInicio_MouseLeave(object? sender, EventArgs e) {
        Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

        if (panel != null) {
            panel.BackColor = Color.FromArgb(20, 16, 30);
        }
    }

    private void PanelAbrirPractica_Click(object? sender, EventArgs e) {
        using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
            carpeta.Description = "Selecciona la carpeta del proyecto";

            if (carpeta.ShowDialog() != DialogResult.OK) {
                RestaurarColorPanel(panelAbrirPractica);
                return;
            }

            SeleccionarPanelMenu(panelAbrirPractica);
            panelVistaNuevaPractica.Visible = false;

            bool aperturaExitosa = IntentarAbrirPractica(carpeta.SelectedPath, promoverReciente: true);

            if (!aperturaExitosa) {
                RestaurarColorPanel(panelAbrirPractica);
            }
        }
    }

    private void RestaurarColorPanel(Panel panel) {
        if (panel == panelSeleccionado) {
            panel.BackColor = Color.ForestGreen;
        } else {
            panel.BackColor = Color.FromArgb(45, 45, 48);
        }
    }

    private void PanelRecientes_Click(object? sender, EventArgs e) {
        SeleccionarPanelMenu(panelRecientes);

        panelInicioVista.Visible = false;
        panelRecientesVista.Visible = true;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;

        panelRecientesVista.BringToFront();

        CargarRecientes();
    }

    private void PanelConfiguracion_Click(object? sender, EventArgs e) {
        SeleccionarPanelMenu(panelConfiguracion);

        panelInicioVista.Visible = false;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = true;
        panelVistaNuevaPractica.Visible = false;

        panelConfiguracionVista.BringToFront();
    }

    private void PanelAcercaDe_Click(object? sender, EventArgs e) {
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
    }
}
