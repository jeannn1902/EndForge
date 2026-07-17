using EndForge.Models;
using System.Drawing.Drawing2D;

namespace EndForge;

public partial class frmPrincipal {
    private void MostrarVistaPreviaVacia() {
        lblNombreFinal.Text = "Esperando datos...";
        lblNombreFinal.ForeColor = Color.FromArgb(156, 115, 194);
        lblNombreFinal.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
    }

    private void BtnCrearProyecto_MouseEnter(object? sender, EventArgs e) {
        if (!btnCrearProyecto.Enabled) {
            return;
        }

        btnCrearProyecto.BackColor = Color.FromArgb(126, 55, 210);
        btnCrearProyecto.ForeColor = Color.White;
    }

    private void BtnCrearProyecto_MouseLeave(object? sender, EventArgs e) {
        ActualizarAparienciaBotonCrear();
    }

    private void BtnCrearProyecto_EnabledChanged(object? sender, EventArgs e) {
        ActualizarAparienciaBotonCrear();
    }

    private void BtnCrearProyecto_Paint(object? sender, PaintEventArgs e) {
        if (btnCrearProyecto.Enabled) {
            return;
        }

        TextRenderer.DrawText(
            e.Graphics,
            btnCrearProyecto.Text,
            btnCrearProyecto.Font,
            btnCrearProyecto.ClientRectangle,
            Color.FromArgb(174, 168, 184),
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine
        );
    }

    private void TxtTemas_DrawItem(object? sender, DrawItemEventArgs e) {
        Color fondo = (e.State & DrawItemState.Selected) == DrawItemState.Selected
            ? Color.FromArgb(74, 45, 104)
            : Color.FromArgb(28, 24, 38);

        using SolidBrush pincelFondo = new(fondo);
        e.Graphics.FillRectangle(pincelFondo, e.Bounds);

        if (e.Index < 0) {
            return;
        }

        string texto = txtTemas.Items[e.Index]?.ToString() ?? "";
        Rectangle limitesTexto = new(
            e.Bounds.Left + 8,
            e.Bounds.Top,
            Math.Max(0, e.Bounds.Width - 16),
            e.Bounds.Height
        );

        TextRenderer.DrawText(
            e.Graphics,
            texto,
            txtTemas.Font,
            limitesTexto,
            Color.White,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.SingleLine
        );
    }

    private void ActualizarAparienciaBotonCrear() {
        if (btnCrearProyecto.Enabled) {
            btnCrearProyecto.BackColor = Color.FromArgb(111, 45, 189);
            btnCrearProyecto.ForeColor = Color.White;
            btnCrearProyecto.Cursor = Cursors.Hand;
            btnCrearProyecto.FlatAppearance.MouseOverBackColor = Color.FromArgb(140, 74, 218);
            btnCrearProyecto.FlatAppearance.MouseDownBackColor = Color.FromArgb(88, 35, 155);
            return;
        }

        Color colorInactivo = Color.FromArgb(48, 43, 58);
        btnCrearProyecto.BackColor = colorInactivo;
        btnCrearProyecto.ForeColor = Color.FromArgb(174, 168, 184);
        btnCrearProyecto.Cursor = Cursors.Default;
        btnCrearProyecto.FlatAppearance.MouseOverBackColor = colorInactivo;
        btnCrearProyecto.FlatAppearance.MouseDownBackColor = colorInactivo;
    }

    private void PanelNuevaPracticaTarjeta_Paint(object? sender, PaintEventArgs e) {
        Rectangle limites = new(0, 0, panelNuevaPracticaTarjeta.Width - 1, panelNuevaPracticaTarjeta.Height - 1);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using GraphicsPath contorno = CrearContornoRedondeado(limites, 18);
        using SolidBrush fondo = new(Color.FromArgb(224, 15, 11, 27));
        using Pen borde = new(Color.FromArgb(88, 168, 85, 247), 1F);

        e.Graphics.FillPath(fondo, contorno);
        e.Graphics.DrawPath(borde, contorno);
    }

    private void PanelVistaPreviaNuevaPractica_Paint(object? sender, PaintEventArgs e) {
        Rectangle limites = new(0, 0, panelVistaPreviaNuevaPractica.Width - 1, panelVistaPreviaNuevaPractica.Height - 1);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using GraphicsPath contorno = CrearContornoRedondeado(limites, 10);
        using SolidBrush fondo = new(Color.FromArgb(235, 32, 25, 46));
        using Pen borde = new(Color.FromArgb(72, 196, 128, 255), 1F);

        e.Graphics.FillPath(fondo, contorno);
        e.Graphics.DrawPath(borde, contorno);
    }

    private static GraphicsPath CrearContornoRedondeado(Rectangle limites, int radio) {
        int diametro = radio * 2;
        GraphicsPath contorno = new();

        contorno.AddArc(limites.Left, limites.Top, diametro, diametro, 180, 90);
        contorno.AddArc(limites.Right - diametro, limites.Top, diametro, diametro, 270, 90);
        contorno.AddArc(limites.Right - diametro, limites.Bottom - diametro, diametro, diametro, 0, 90);
        contorno.AddArc(limites.Left, limites.Bottom - diametro, diametro, diametro, 90, 90);
        contorno.CloseFigure();

        return contorno;
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
        ResultadoCargaConfiguracion cargaConfiguracion = CargarConfiguracion();
        CargarTemas();
        CargarRecientes();

        // panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

        panelInicioVista.Visible = true;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;

        panelInicioVista.BringToFront();

        panelSeleccionado = panelInicio;
        panelInicio.BackColor = Color.FromArgb(111, 45, 189);

        AplicarFondoDinamicoPanelPrincipal();

        if (string.IsNullOrWhiteSpace(rutaBase) || string.IsNullOrWhiteSpace(rutaPlantilla)) {
            PanelConfiguracion_Click(panelConfiguracion, EventArgs.Empty);

            if (cargaConfiguracion.Estado == EstadoCargaConfiguracion.NoDisponible) {
                MessageBox.Show(
                    "¡Bienvenido a EndForge!\n\n" +
                    "Antes de comenzar, configura la carpeta de tus prácticas y la plantilla oficial.",
                    "Configuración inicial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        txtBuscarReciente.Text = "Buscar práctica...";
        txtBuscarReciente.ForeColor = Color.Gray;
    }

    private void CargarTemas() {
        txtTemas.Items.Clear();

        foreach (string tema in temasService.CargarTemas(rutaBase)) {
            txtTemas.Items.Add(tema);
        }

        if (txtTemas.Items.Count > 0) {
            txtTemas.SelectedIndex = 0;
        }

        ActualizarVistaPrevia();
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
        lblNombreFinal.ForeColor = Color.FromArgb(196, 128, 255);
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
            resultadoRecientes => {
                if (resultadoRecientes.EsExitosa) {
                    CargarRecientes();
                }

                MostrarResultadoEscrituraRecientes(
                    resultadoRecientes,
                    "La práctica se creó y abrió correctamente"
                );
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
