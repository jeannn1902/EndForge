using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private EstadoLecturaRecientes? ultimoEstadoLecturaRecientesNotificado;

    private ResultadoEscrituraRecientes GuardarProyectoReciente(string rutaProyecto) {
        return recientesService.GuardarProyectoReciente(rutaProyecto);
    }

    private bool IntentarAbrirPractica(string rutaProyecto, bool promoverReciente = false) {
        ResultadoAperturaPractica resultado = aperturaPracticasService.AbrirPractica(rutaProyecto);

        if (resultado.Estado == EstadoAperturaPractica.CarpetaInexistente) {
            MessageBox.Show("La carpeta de esta práctica ya no existe.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (resultado.Estado != EstadoAperturaPractica.Exitosa) {
            MessageBox.Show("No se pudo abrir la práctica.\n\n" + resultado.Error!.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (promoverReciente) {
            ResultadoEscrituraRecientes guardado = GuardarProyectoReciente(rutaProyecto);

            if (guardado.EsExitosa) {
                CargarRecientes();
            }
            MostrarResultadoEscrituraRecientes(guardado, "La práctica se abrió correctamente");
        }
        return true;
    }

    private List<Label> ObtenerLabelsRecientes() {
        return new List<Label> {
            lblReciente1,
            lblReciente2,
            lblReciente3,
            lblReciente4,
            lblReciente5,
            lblReciente6,
            lblReciente7,
            lblReciente8,
            lblReciente9,
            lblReciente10
        };
    }

    private void LimpiarLabelsRecientes() {
        foreach (Label label in ObtenerLabelsRecientes()) {
            label.Text = "";
            label.Visible = false;
            label.Tag = null;
        }
    }

    private void LimpiarVistaRecientes() {
        listRecientes.Items.Clear();
        LimpiarLabelsRecientes();

        lblCardRecientesDesc.Text = "";
        lblCardRecientesDesc.Tag = null;
        lblCardContinuarDesc.Text = "";
        lblCardContinuarDesc.Tag = null;
    }

    private void MostrarTarjetasRecientesVacias() {
        lblCardRecientesDesc.Text = "No hay proyectos recientes.";
        lblCardContinuarDesc.Text = "No hay prácticas recientes.";
    }

    private void LabelReciente_DoubleClick(object? sender, EventArgs e) {
        Label? label = sender as Label;

        if (label?.Tag is not ProyectoReciente proyecto)
            return;

        IntentarAbrirPractica(proyecto.Ruta, promoverReciente: true);
    }

    private void CargarRecientes() {
        ActualizarVistaRecientes();
    }

    private void ActualizarVistaRecientes(string? filtro = null) {
        LimpiarVistaRecientes();

        ResultadoLecturaRecientes resultado = recientesService.LeerProyectosRecientes();
        NotificarResultadoLecturaRecientes(resultado);

        if (!resultado.DatosDisponibles || resultado.Proyectos.Count == 0) {
            MostrarTarjetasRecientesVacias();
            return;
        }

        ProyectoReciente primerProyecto = resultado.Proyectos[0];
        lblCardRecientesDesc.Text = primerProyecto.Nombre;
        lblCardRecientesDesc.Tag = primerProyecto.Ruta;
        lblCardContinuarDesc.Text = primerProyecto.Nombre;
        lblCardContinuarDesc.Tag = primerProyecto.Ruta;

        IEnumerable<ProyectoReciente> proyectosVisibles = resultado.Proyectos;

        if (!string.IsNullOrEmpty(filtro)) {
            proyectosVisibles = proyectosVisibles.Where(proyecto =>
                proyecto.Nombre.Contains(filtro, StringComparison.CurrentCultureIgnoreCase));
        }

        List<Label> labelsRecientes = ObtenerLabelsRecientes();
        int indice = 0;

        foreach (ProyectoReciente proyecto in proyectosVisibles) {
            listRecientes.Items.Add(proyecto);

            if (indice < labelsRecientes.Count) {
                labelsRecientes[indice].Text = proyecto.Nombre;
                labelsRecientes[indice].Tag = proyecto;
                labelsRecientes[indice].Visible = true;
                indice++;
            }
        }
    }

    private void NotificarResultadoLecturaRecientes(ResultadoLecturaRecientes resultado) {
        if (resultado.Estado == EstadoLecturaRecientes.Exitosa ||
            resultado.Estado == EstadoLecturaRecientes.ArchivoInexistente) {
            ultimoEstadoLecturaRecientesNotificado = null;
            return;
        }

        if (ultimoEstadoLecturaRecientesNotificado == resultado.Estado) {
            return;
        }

        ultimoEstadoLecturaRecientesNotificado = resultado.Estado;

        string mensaje = resultado.Estado switch {
            EstadoLecturaRecientes.PermisosInsuficientes =>
                "No se pudieron cargar los proyectos recientes porque no hay permisos para acceder a recientes.txt.",
            EstadoLecturaRecientes.ErrorIo =>
                "No se pudieron cargar los proyectos recientes. Verifica que recientes.txt no esté bloqueado o en uso por otra aplicación.",
            EstadoLecturaRecientes.ContenidoInvalido =>
                $"Se ignoraron {resultado.RegistrosInvalidos} registros dañados de recientes.txt. Los demás proyectos se cargaron correctamente.",
            _ => "No se pudieron cargar los proyectos recientes."
        };

        MessageBox.Show(mensaje, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void MostrarResultadoEscrituraRecientes(
        ResultadoEscrituraRecientes resultado,
        string operacionExitosa) {
        if (resultado.EsExitosa) {
            if (resultado.RegistrosInvalidosIgnorados > 0) {
                MessageBox.Show(
                    $"{operacionExitosa}, pero se ignoraron {resultado.RegistrosInvalidosIgnorados} registros dañados al actualizar Recientes.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            return;
        }

        string mensaje = resultado.Estado == EstadoEscrituraRecientes.PermisosInsuficientes
            ? $"{operacionExitosa}, pero no pudo guardarse en Recientes porque no hay permisos para acceder a recientes.txt."
            : $"{operacionExitosa}, pero no pudo guardarse en Recientes. Verifica que recientes.txt no esté bloqueado y que su carpeta permita crear y reemplazar archivos.";

        MessageBox.Show(mensaje, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ListRecientes_DoubleClick(object sender, EventArgs e) {
        if (listRecientes.SelectedItem == null)
            return;

        ProyectoReciente proyecto = (ProyectoReciente)listRecientes.SelectedItem;
        IntentarAbrirPractica(proyecto.Ruta, promoverReciente: true);
    }

    private void ListRecientes_SelectedIndexChanged(object sender, EventArgs e) {
    }

    private void LblAyudaRecientes_Click(object sender, EventArgs e) {
    }

    private void ListRecientes_SelectedIndexChanged_1(object sender, EventArgs e) {
    }

    private void ListRecientes_DrawItem(object sender, DrawItemEventArgs e) {
        if (e.Index < 0)
            return;

        bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color colorFondo = seleccionado ? Color.FromArgb(111, 45, 189) : Color.FromArgb(20, 16, 30);
        Color colorTexto = Color.White;
        Color colorLinea = Color.FromArgb(55, 45, 70);

        using (SolidBrush fondo = new SolidBrush(colorFondo)) {
            e.Graphics.FillRectangle(fondo, e.Bounds);
        }

        string texto = listRecientes.Items[e.Index].ToString() ?? "";
        Rectangle areaTexto = new Rectangle(e.Bounds.Left + 12, e.Bounds.Top, e.Bounds.Width - 24, e.Bounds.Height - 1);

        TextRenderer.DrawText(e.Graphics, texto, listRecientes.Font, areaTexto, colorTexto, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

        using (Pen linea = new Pen(colorLinea)) {
            e.Graphics.DrawLine(linea, e.Bounds.Left + 8, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
        }
    }

    private void LblCardRecientesDesc_Click(object sender, EventArgs e) {
        string? rutaProyecto = lblCardRecientesDesc.Tag as string;

        if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
            IntentarAbrirPractica(rutaProyecto, true);
        }
    }   

    private void PanelCardRecientes_Click(object sender, EventArgs e) {
        LblCardRecientesDesc_Click(sender, e);
    }

    private void LblCardRecientesTitulo_Click(object sender, EventArgs e) {
        LblCardRecientesDesc_Click(sender, e);
    }

private void PanelCardContinuar_Click(object sender, EventArgs e) {
    string? rutaProyecto = lblCardContinuarDesc.Tag?.ToString();

    if (string.IsNullOrWhiteSpace(rutaProyecto))
        return;

    IntentarAbrirPractica(rutaProyecto, true);
}

    private void panelCardContinuar_Click(object sender, EventArgs e) {
    }

    private void TxtBuscarReciente_TextChanged(object sender, EventArgs e) {
        string filtro = txtBuscarReciente.Text.Trim();

        if (filtro == "Buscar práctica...") {
            filtro = "";
        }

        ActualizarVistaRecientes(filtro);
    }

    private void TxtBuscarReciente_Enter(object sender, EventArgs e) {
        if (txtBuscarReciente.Text == "Buscar práctica...") {
            txtBuscarReciente.Text = "";
            txtBuscarReciente.ForeColor = Color.White;
        }
    }

    private void TxtBuscarReciente_Leave(object sender, EventArgs e) {
        if (string.IsNullOrWhiteSpace(txtBuscarReciente.Text)) {
            txtBuscarReciente.Text = "Buscar práctica...";
            txtBuscarReciente.ForeColor = Color.Gray;
        }
    }
}
