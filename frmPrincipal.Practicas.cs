using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private void MostrarVistaPreviaVacia() {
        lblNombreFinal.Text = "Esperando datos...";
        lblNombreFinal.ForeColor = Color.FromArgb(168, 85, 247);
        lblNombreFinal.Font = new Font("Segoe UI Light", 11F, FontStyle.Italic);
    }

    private void BtnCrearProyecto_MouseEnter(object? sender, EventArgs e) {
        btnCrearProyecto.BackColor = Color.FromArgb(126, 55, 210);
        btnCrearProyecto.ForeColor = Color.White;
    }

    private void BtnCrearProyecto_MouseLeave(object? sender, EventArgs e) {
        btnCrearProyecto.BackColor = Color.FromArgb(111, 45, 189);
        btnCrearProyecto.ForeColor = Color.White;
    }

    private void Label1_Click(object sender, EventArgs e) {
    }

    private void Label2_Click(object sender, EventArgs e) {
    }

    private void Label1_Click_1(object sender, EventArgs e) {
    }

    private void LblVistaPrevia_Click(object sender, EventArgs e) {
    }

    private void TxtNombreProyecto_TextChanged(object sender, EventArgs e) {
        ActualizarVistaPrevia();
        ValidarFormulario();
    }

    private void FrmPrincipal_Load(object sender, EventArgs e) {
        btnCrearProyecto.Enabled = false;
        CargarConfiguracion();
        CargarTemas();
        CargarRecientes();

        // panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

        panelInicioVista.Visible = true;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;

        panelInicioVista.BringToFront();

        panelSeleccionado = panelInicio;
        panelInicio.BackColor = Color.FromArgb(111, 45, 189);

        AplicarFondoDinamicoPanelPrincipal();

        if (string.IsNullOrWhiteSpace(rutaBase) || string.IsNullOrWhiteSpace(rutaPlantilla)) {
            PanelConfiguracion_Click(panelConfiguracion, EventArgs.Empty);

            MessageBox.Show(
                "¡Bienvenido a EndForge!\n\n" +
                "Antes de comenzar, configura la carpeta de tus prácticas y la plantilla oficial.",
                "Configuración inicial",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        txtBuscarReciente.Text = "Buscar práctica...";
        txtBuscarReciente.ForeColor = Color.Gray;
    }

    private void CargarTemas() {
        txtTemas.Items.Clear();

        foreach (string tema in temasService.CargarTemas(rutaBase)) {
            txtTemas.Items.Add(tema);
            ActualizarVistaPrevia();
        }

        if (txtTemas.Items.Count > 0) {
            txtTemas.SelectedIndex = 0;
        }
    }

    private void ActualizarVistaPrevia() {
        ResultadoVistaPreviaPractica resultado = vistaPreviaPracticaService.Calcular(
            rutaBase,
            txtTemas.SelectedItem?.ToString(),
            txtNombreProyecto.Text
        );

        if (resultado.Estado == EstadoVistaPreviaPractica.Vacia) {
            MostrarVistaPreviaVacia();
            return;
        }

        lblNombreFinal.Text = resultado.NombreFinal;
        lblNombreFinal.ForeColor = Color.White;
        lblNombreFinal.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
    }

    private void ValidarFormulario() {
        btnCrearProyecto.Enabled =
            txtTemas.SelectedItem != null &&
            !string.IsNullOrWhiteSpace(txtNombreProyecto.Text) &&
            !string.IsNullOrWhiteSpace(txtObjetivo.Text) &&
            !string.IsNullOrWhiteSpace(rutaBase) &&
            !string.IsNullOrWhiteSpace(rutaPlantilla) &&
            temasService.ExisteTema(rutaBase, txtTemas.Text);
    }

    private void CmbTemas_SelectedIndexChanged(object sender, EventArgs e) {
        ActualizarVistaPrevia();
    }

    private void BtnCrearProyecto_Click(object sender, EventArgs e) {
        string temaSeleccionado = txtTemas.Text;
        temaSeleccionado = temaSeleccionado.Trim();

        ResultadoValidacionNombrePractica validacionNombre = nombrePracticaService.Validar(txtNombreProyecto.Text);

        if (!validacionNombre.EsValido) {
            MessageBox.Show(validacionNombre.MensajeError);
            txtNombreProyecto.Focus();
            return;
        }

        ResultadoValidacionConfiguracion validacionConfiguracion =
            configuracionService.ValidarConfiguracionDetallada(rutaBase, rutaPlantilla);

        if (validacionConfiguracion.Estado != EstadoValidacionConfiguracion.Valida) {
            MessageBox.Show(
                ObtenerMensajeValidacionConfiguracion(validacionConfiguracion.Estado),
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        ResultadoVistaPreviaPractica vistaPrevia = vistaPreviaPracticaService.Calcular(
            rutaBase,
            temaSeleccionado,
            validacionNombre.NombreNormalizado
        );

        string nombreProyecto = vistaPrevia.NombreFinal.Trim();
        string rutaProyecto = Path.Combine(rutaBase, temaSeleccionado, nombreProyecto);

        SolicitudCreacionPractica solicitud = new SolicitudCreacionPractica {
            RutaPlantilla = rutaPlantilla,
            RutaProyecto = rutaProyecto,
            NombreProyecto = nombreProyecto,
            Tema = temaSeleccionado,
            Objetivo = txtObjetivo.Text.Trim(),
            RutaRelativaSolucionEsperada = seleccionSolucionesService.TransformarRutaRelativa(
                validacionConfiguracion.RutaRelativaSolucion,
                nombreProyecto
            )
        };

        ResultadoCreacionPractica resultado = creacionPracticasOrquestador.CrearPractica(
            solicitud,
            errorRecientes => {
                if (errorRecientes == null) {
                    CargarRecientes();
                } else {
                    MessageBox.Show("La práctica se creó correctamente, pero no pudo guardarse en Recientes.\n\n" + errorRecientes.Message, "EndForge",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            },
            () => {
                txtNombreProyecto.Clear();
                txtObjetivo.Clear();
                txtNombreProyecto.Focus();

                ActualizarVistaPrevia();
                ValidarFormulario();
            }
        );

        if (resultado.Estado == EstadoCreacionPractica.DestinoExistente) {
            MessageBox.Show("La práctica ya existe.");
            txtNombreProyecto.Focus();
            return;
        }

        if (resultado.Estado == EstadoCreacionPractica.ErrorCreacion) {
            MessageBox.Show("Ocurrió un error al crear la práctica.\n\n" + resultado.Error!.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (resultado.Estado == EstadoCreacionPractica.ErrorApertura) {
            MessageBox.Show("La práctica se creó correctamente, pero no pudo abrirse Visual Studio.\n\n" + resultado.Error!.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("El proyecto se creó correctamente.\n\n¡Visual Studio se abrirá automáticamente!", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LblNombreFinal_Click(object sender, EventArgs e) {
    }

    private void Label1_Click_2(object sender, EventArgs e) {
    }

    private void TxtObjetivo_TextChanged(object sender, EventArgs e) {
        ValidarFormulario();
    }

    private void PictureBox1_Click(object sender, EventArgs e) {
    }

    private void LblObjetivo_Click(object sender, EventArgs e) {
    }

    private void PanelControles_Paint(object sender, PaintEventArgs e) {
    }

    private void PanelMenu_Paint(object sender, PaintEventArgs e) {
    }

    private void LblInicio_Click(object sender, EventArgs e) {
    }
}
