using EndForge.Controls;
using EndForge.Models;
using System.Drawing.Drawing2D;

namespace EndForge;

public partial class frmPrincipal {
    private bool disenoNuevaPracticaConfigurado;
    private TextBoxMultilineaEndForge campoObjetivoEndForge = null!;

    private void ConfigurarDisenoNuevaPracticaAdaptable() {
        if (disenoNuevaPracticaConfigurado) {
            return;
        }

        panelVistaNuevaPractica.Dock = DockStyle.Fill;
        panelNuevaPracticaTarjeta.Anchor = AnchorStyles.None;
        ConfigurarCampoObjetivoEndForge();

        lblTitulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblNuevaPracticaSubtitulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblTema.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtTemas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblNombre.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtNombreProyecto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblObjetivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        campoObjetivoEndForge.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panelVistaPreviaNuevaPractica.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnCrearProyecto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblVistaPrevia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblNombreFinal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        disenoNuevaPracticaConfigurado = true;
    }

    private void AjustarGeometriaNuevaPractica() {
        if (!disenoNuevaPracticaConfigurado ||
            distribucionPanelPrincipal != DistribucionPanelPrincipal.NuevaPractica ||
            panelVistaNuevaPractica.IsDisposed) {
            return;
        }

        Rectangle area = panelVistaNuevaPractica.ClientRectangle;

        if (area.Width <= 0 || area.Height <= 0) {
            return;
        }

        int margenExterior = EscalarDiseno(16);
        int anchoDisponible = Math.Max(1, area.Width - margenExterior * 2);
        int altoDisponible = Math.Max(1, area.Height - margenExterior * 2);
        int anchoMinimo = Math.Min(EscalarDiseno(584), anchoDisponible);
        int anchoMaximo = Math.Max(anchoMinimo, Math.Min(EscalarDiseno(840), anchoDisponible));
        int anchoDeseado = (int)Math.Round(anchoDisponible * 0.90D);
        int anchoTarjeta = Math.Clamp(anchoDeseado, anchoMinimo, anchoMaximo);
        int altoMinimo = Math.Min(EscalarDiseno(446), altoDisponible);
        int altoMaximo = Math.Max(altoMinimo, Math.Min(EscalarDiseno(560), altoDisponible));
        int altoTarjeta = Math.Clamp(altoDisponible, altoMinimo, altoMaximo);
        int x = area.Left + Math.Max(0, (area.Width - anchoTarjeta) / 2);
        int y = area.Top + Math.Max(0, (area.Height - altoTarjeta) / 2);

        panelVistaNuevaPractica.SuspendLayout();
        panelNuevaPracticaTarjeta.SuspendLayout();

        try {
            panelNuevaPracticaTarjeta.SetBounds(x, y, anchoTarjeta, altoTarjeta);

            int margenContenido = Math.Min(
                EscalarDiseno(32),
                Math.Max(EscalarDiseno(18), panelNuevaPracticaTarjeta.ClientSize.Width / 8));
            int anchoContenido = Math.Max(
                1,
                panelNuevaPracticaTarjeta.ClientSize.Width - margenContenido * 2);
            int rellenoSuperior = EscalarDiseno(14);
            int rellenoInferior = EscalarDiseno(14);
            int separacionEncabezado = EscalarDiseno(2);
            int separacionSecciones = EscalarDiseno(7);
            int separacionEtiquetaControl = EscalarDiseno(3);
            int separacionObjetivoVistaPrevia = EscalarDiseno(10);
            int separacionVistaPreviaBoton = EscalarDiseno(10);
            int altoTitulo = CalcularAltoTextoNuevaPractica(lblTitulo, anchoContenido, 42);
            int altoSubtitulo = CalcularAltoTextoNuevaPractica(
                lblNuevaPracticaSubtitulo,
                anchoContenido,
                24);
            int altoEtiquetaTema = CalcularAltoTextoNuevaPractica(lblTema, anchoContenido, 20);
            int altoTema = Math.Max(EscalarDiseno(31), txtTemas.PreferredSize.Height);
            int altoEtiquetaNombre = CalcularAltoTextoNuevaPractica(lblNombre, anchoContenido, 20);
            int altoNombre = Math.Max(EscalarDiseno(29), txtNombreProyecto.PreferredSize.Height);
            int altoEtiquetaObjetivo = CalcularAltoTextoNuevaPractica(
                lblObjetivo,
                anchoContenido,
                20);
            int altoBoton = Math.Max(EscalarDiseno(42), btnCrearProyecto.PreferredSize.Height);
            int posicionY = rellenoSuperior;

            lblTitulo.SetBounds(
                margenContenido,
                posicionY,
                anchoContenido,
                altoTitulo);
            posicionY = lblTitulo.Bottom + separacionEncabezado;
            lblNuevaPracticaSubtitulo.SetBounds(
                margenContenido,
                posicionY,
                anchoContenido,
                altoSubtitulo);
            posicionY = lblNuevaPracticaSubtitulo.Bottom + separacionSecciones;

            lblTema.SetBounds(
                margenContenido,
                posicionY,
                anchoContenido,
                altoEtiquetaTema);
            posicionY = lblTema.Bottom + separacionEtiquetaControl;
            txtTemas.SetBounds(margenContenido, posicionY, anchoContenido, altoTema);
            posicionY = txtTemas.Bottom + separacionSecciones;

            lblNombre.SetBounds(
                margenContenido,
                posicionY,
                anchoContenido,
                altoEtiquetaNombre);
            posicionY = lblNombre.Bottom + separacionEtiquetaControl;
            txtNombreProyecto.SetBounds(margenContenido, posicionY, anchoContenido, altoNombre);
            posicionY = txtNombreProyecto.Bottom + separacionSecciones;

            lblObjetivo.SetBounds(
                margenContenido,
                posicionY,
                anchoContenido,
                altoEtiquetaObjetivo);
            int yObjetivo = lblObjetivo.Bottom + separacionEtiquetaControl;
            int yBoton = Math.Max(
                yObjetivo + 2,
                panelNuevaPracticaTarjeta.ClientSize.Height - rellenoInferior - altoBoton);
            int espacioFlexible = Math.Max(
                2,
                yBoton - separacionVistaPreviaBoton - yObjetivo -
                separacionObjetivoVistaPrevia);
            int altoMinimoObjetivo = EscalarDiseno(72);
            int altoMinimoVistaPrevia = EscalarDiseno(64);
            int altoMaximoVistaPrevia = EscalarDiseno(96);
            int altoVistaPrevia = Math.Clamp(
                (int)Math.Round(espacioFlexible * 0.28D),
                Math.Min(altoMinimoVistaPrevia, espacioFlexible - 1),
                Math.Max(1, Math.Min(altoMaximoVistaPrevia, espacioFlexible - 1)));
            int altoObjetivo = Math.Max(1, espacioFlexible - altoVistaPrevia);

            if (altoObjetivo < altoMinimoObjetivo && espacioFlexible > 1) {
                altoVistaPrevia = Math.Max(1, espacioFlexible - altoMinimoObjetivo);
                altoObjetivo = Math.Max(1, espacioFlexible - altoVistaPrevia);
            }

            campoObjetivoEndForge.SetBounds(
                margenContenido,
                yObjetivo,
                anchoContenido,
                altoObjetivo);
            panelVistaPreviaNuevaPractica.SetBounds(
                margenContenido,
                campoObjetivoEndForge.Bottom + separacionObjetivoVistaPrevia,
                anchoContenido,
                altoVistaPrevia);
            btnCrearProyecto.SetBounds(
                margenContenido,
                yBoton,
                anchoContenido,
                altoBoton);

            int margenVistaPrevia = EscalarDiseno(18);
            int anchoTextoVistaPrevia = Math.Max(
                1,
                panelVistaPreviaNuevaPractica.ClientSize.Width - margenVistaPrevia * 2);
            int rellenoVerticalVistaPrevia = EscalarDiseno(5);
            int altoEtiquetaVistaPrevia = CalcularAltoTextoNuevaPractica(
                lblVistaPrevia,
                anchoTextoVistaPrevia,
                18);
            int yNombreFinal = rellenoVerticalVistaPrevia + altoEtiquetaVistaPrevia;
            int altoNombreFinal = Math.Max(
                1,
                panelVistaPreviaNuevaPractica.ClientSize.Height -
                yNombreFinal - rellenoVerticalVistaPrevia);
            lblVistaPrevia.SetBounds(
                margenVistaPrevia,
                rellenoVerticalVistaPrevia,
                anchoTextoVistaPrevia,
                altoEtiquetaVistaPrevia);
            lblNombreFinal.SetBounds(
                margenVistaPrevia,
                yNombreFinal,
                anchoTextoVistaPrevia,
                altoNombreFinal);
        } finally {
            panelNuevaPracticaTarjeta.ResumeLayout(performLayout: true);
            panelVistaNuevaPractica.ResumeLayout(performLayout: true);
        }

        panelNuevaPracticaTarjeta.Invalidate();
        panelVistaPreviaNuevaPractica.Invalidate();
    }

    private void ConfigurarCampoObjetivoEndForge() {
        int indiceControl = panelNuevaPracticaTarjeta.Controls.GetChildIndex(txtObjetivo);
        Rectangle limites = txtObjetivo.Bounds;

        campoObjetivoEndForge = new TextBoxMultilineaEndForge(txtObjetivo) {
            AccessibleName = txtObjetivo.AccessibleName,
            Bounds = limites,
            Name = "campoObjetivoEndForge",
            TabIndex = txtObjetivo.TabIndex,
            TabStop = false
        };

        panelNuevaPracticaTarjeta.Controls.Add(campoObjetivoEndForge);
        panelNuevaPracticaTarjeta.Controls.SetChildIndex(campoObjetivoEndForge, indiceControl);
    }

    private int CalcularAltoTextoNuevaPractica(Label label, int ancho, int altoMinimo) {
        int altoTexto = TextRenderer.MeasureText(
            label.Text,
            label.Font,
            new Size(Math.Max(1, ancho), int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
        return Math.Max(EscalarDiseno(altoMinimo), altoTexto + EscalarDiseno(3));
    }

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

        panelInicioVista.Visible = true;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;
        OcultarVistasCurso();

        panelInicioVista.BringToFront();

        panelSeleccionado = panelInicio;
        panelInicio.BackColor = Color.FromArgb(111, 45, 189);

        InvalidarFondoContinuo();
        MostrarPantallaBienvenida();
    }

    private void CargarTemas(IReadOnlyList<string>? temasPrecargados = null) {
        txtTemas.Items.Clear();

        IReadOnlyList<string> temas =
            temasPrecargados ?? temasService.CargarTemas(rutaBase);

        foreach (string tema in temas) {
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

    private ResultadoCreacionPractica? EjecutarCreacionPractica(
        string temaSeleccionado,
        string nombreIntroducido,
        string objetivo,
        Action accionAlPrepararApertura,
        out string rutaProyecto
    ) {
        rutaProyecto = string.Empty;
        temaSeleccionado = temaSeleccionado.Trim();

        ResultadoValidacionNombrePractica validacionNombre = nombrePracticaService.Validar(nombreIntroducido);

        if (!validacionNombre.EsValido) {
            MessageBox.Show(validacionNombre.MensajeError);
            txtNombreProyecto.Focus();
            return null;
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
            return null;
        }

        if (!temasService.ExisteTema(rutaBase, temaSeleccionado)) {
            MessageBox.Show(
                "El tema seleccionado ya no está disponible en la ruta base configurada.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return null;
        }

        ResultadoVistaPreviaPractica vistaPrevia = vistaPreviaPracticaService.Calcular(
            rutaBase,
            temaSeleccionado,
            validacionNombre.NombreNormalizado
        );

        string nombreProyecto = vistaPrevia.NombreFinal.Trim();
        rutaProyecto = Path.Combine(rutaBase, temaSeleccionado, nombreProyecto);

        SolicitudCreacionPractica solicitud = new SolicitudCreacionPractica {
            RutaPlantilla = rutaPlantilla,
            RutaProyecto = rutaProyecto,
            NombreProyecto = nombreProyecto,
            Tema = temaSeleccionado,
            Objetivo = objetivo.Trim(),
            RutaRelativaSolucionEsperada = seleccionSolucionesService.TransformarRutaRelativa(
                validacionConfiguracion.RutaRelativaSolucion,
                nombreProyecto
            )
        };

        return creacionPracticasOrquestador.CrearPractica(
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
            accionAlPrepararApertura
        );
    }

    private void BtnCrearProyecto_Click(object sender, EventArgs e) {
        ResultadoCreacionPractica? resultado = EjecutarCreacionPractica(
            txtTemas.Text,
            txtNombreProyecto.Text,
            txtObjetivo.Text,
            () => {
                txtNombreProyecto.Clear();
                campoObjetivoEndForge.Clear();
                txtNombreProyecto.Focus();

                ActualizarVistaPrevia();
                ValidarFormulario();
            },
            out _
        );

        if (resultado is null) {
            return;
        }

        MostrarResultadoCreacionPractica(resultado, enfocarNombreProyecto: true);
    }

    private bool MostrarResultadoCreacionPractica(
        ResultadoCreacionPractica resultado,
        bool enfocarNombreProyecto
    ) {
        if (resultado.Estado == EstadoCreacionPractica.DestinoExistente) {
            MessageBox.Show("La práctica ya existe.");

            if (enfocarNombreProyecto) {
                txtNombreProyecto.Focus();
            }

            return false;
        }

        if (resultado.Estado == EstadoCreacionPractica.ErrorCreacion) {
            MessageBox.Show(
                "Ocurrió un error al crear la práctica.\n\n" + resultado.Error!.Message,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (resultado.Estado == EstadoCreacionPractica.ErrorApertura) {
            MessageBox.Show(
                "La práctica se creó correctamente, pero no pudo abrirse Visual Studio.\n\n" + resultado.Error!.Message,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        MessageBox.Show(
            "El proyecto se creó correctamente.\n\n¡Visual Studio se abrirá automáticamente!",
            "EndForge",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return true;
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
