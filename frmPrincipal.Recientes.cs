using EndForge.Models;

namespace EndForge;

public partial class frmPrincipal {
    private EstadoLecturaRecientes? ultimoEstadoLecturaRecientesNotificado;
    private ResultadoLecturaRecientes? ultimoResultadoLecturaRecientes;
    private bool aperturaPracticaEnCurso;

    private ResultadoEscrituraRecientes GuardarProyectoReciente(string rutaProyecto) {
        return recientesService.GuardarProyectoReciente(rutaProyecto);
    }

    private async Task<bool> IntentarAbrirPracticaAsync(
        string rutaProyecto,
        bool promoverReciente = false) {
        if (aperturaPracticaEnCurso) {
            return false;
        }

        aperturaPracticaEnCurso = true;

        try {
            ResultadoAperturaPractica resultado = await Task.Run(() =>
                aperturaPracticasService.AbrirPractica(rutaProyecto));

            if (IsDisposed || Disposing) {
                return false;
            }

            if (resultado.Estado ==
                EstadoAperturaPractica.CarpetaInexistente) {
                MessageBox.Show(
                    "La carpeta de esta práctica ya no existe.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                if (promoverReciente) {
                    CargarRecientes();
                }

                return false;
            }

            if (resultado.Estado != EstadoAperturaPractica.Exitosa) {
                MessageBox.Show(
                    "No se pudo abrir la práctica.\n\n" +
                        (resultado.Error?.Message ??
                            "La solución no está disponible."),
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (promoverReciente) {
                    CargarRecientes();
                }

                return false;
            }

            if (promoverReciente) {
                ResultadoEscrituraRecientes guardado = await Task.Run(() =>
                    GuardarProyectoReciente(rutaProyecto));

                if (IsDisposed || Disposing) {
                    return true;
                }

                if (guardado.EsExitosa) {
                    CargarRecientes();
                }

                MostrarResultadoEscrituraRecientes(
                    guardado,
                    "La práctica se abrió correctamente");
            }

            return true;
        } finally {
            aperturaPracticaEnCurso = false;
        }
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
    }

    private async void LabelReciente_DoubleClick(object? sender, EventArgs e) {
        Label? label = sender as Label;

        if (label?.Tag is not ProyectoReciente proyecto)
            return;

        await IntentarAbrirPracticaAsync(
            proyecto.Ruta,
            promoverReciente: true);
    }

    private void CargarRecientes(
        ResultadoLecturaRecientes? resultadoPrecargado = null,
        bool notificar = true) {
        ActualizarVistaRecientes(
            resultadoPrecargado: resultadoPrecargado,
            notificar: notificar);
    }

    private void ActualizarVistaRecientes(
        string? filtro = null,
        ResultadoLecturaRecientes? resultadoPrecargado = null,
        bool notificar = true) {
        LimpiarVistaRecientes();

        ResultadoLecturaRecientes resultado =
            resultadoPrecargado ??
            (filtro is not null && ultimoResultadoLecturaRecientes is not null
                ? ultimoResultadoLecturaRecientes
                : recientesService.LeerProyectosRecientes());
        ultimoResultadoLecturaRecientes = resultado;

        if (notificar) {
            NotificarResultadoLecturaRecientes(resultado);
        }

        if (!resultado.DatosDisponibles || resultado.Proyectos.Count == 0) {
            return;
        }

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
                CrearMensajeRegistrosRecientesIgnorados(resultado),
            _ => "No se pudieron cargar los proyectos recientes."
        };

        MessageBox.Show(mensaje, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void MostrarResultadoEscrituraRecientes(
        ResultadoEscrituraRecientes resultado,
        string operacionExitosa) {
        if (resultado.EsExitosa) {
            int totalIgnorados =
                resultado.RegistrosInvalidosIgnorados +
                resultado.RegistrosNoDisponiblesIgnorados;

            if (totalIgnorados > 0) {
                MessageBox.Show(
                    $"{operacionExitosa}, pero se ignoraron {totalIgnorados} registros dañados o no disponibles al actualizar Recientes.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            return;
        }

        string mensaje = resultado.Estado switch {
            EstadoEscrituraRecientes.PermisosInsuficientes =>
                $"{operacionExitosa}, pero no pudo guardarse en Recientes porque no hay permisos para acceder a recientes.txt.",
            EstadoEscrituraRecientes.RutaProyectoInvalida or
                EstadoEscrituraRecientes.ProyectoNoDisponible =>
                $"{operacionExitosa}, pero no pudo guardarse en Recientes porque la carpeta o su solución ya no están disponibles.",
            EstadoEscrituraRecientes.ArchivoBloqueado =>
                $"{operacionExitosa}, pero no pudo guardarse en Recientes porque recientes.txt está siendo actualizado por otra instancia.",
            _ =>
                $"{operacionExitosa}, pero no pudo guardarse en Recientes. Verifica que recientes.txt no esté bloqueado y que su carpeta permita crear y reemplazar archivos."
        };

        MessageBox.Show(mensaje, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static string CrearMensajeRegistrosRecientesIgnorados(
        ResultadoLecturaRecientes resultado) {
        int total =
            resultado.RegistrosInvalidos +
            resultado.RegistrosNoDisponibles;

        return total == 1
            ? "Se ignoró un registro dañado o no disponible de recientes.txt. Los demás proyectos se cargaron correctamente."
            : $"Se ignoraron {total} registros dañados o no disponibles de recientes.txt. Los demás proyectos se cargaron correctamente.";
    }

    private async void ListRecientes_DoubleClick(object sender, EventArgs e) {
        if (listRecientes.SelectedItem == null)
            return;

        ProyectoReciente proyecto = (ProyectoReciente)listRecientes.SelectedItem;
        await IntentarAbrirPracticaAsync(
            proyecto.Ruta,
            promoverReciente: true);
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
