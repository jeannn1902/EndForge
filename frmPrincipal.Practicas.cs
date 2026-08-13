using EndForge.Controls;
using EndForge.Models;
using System.Drawing.Drawing2D;

namespace EndForge;

public partial class frmPrincipal {
    private bool disenoNuevaPracticaConfigurado;
    private bool creacionPracticaEnCurso;
    private Task? tareaCreacionPracticaActiva;
    private TextBoxMultilineaEndForge campoObjetivoEndForge = null!;
    private Font fuenteNombreFinalVacia = null!;
    private Font fuenteNombreFinalCompleta = null!;
    private ResultadoVistaPreviaPractica ultimoResultadoVistaPrevia = new();
    private ResultadoCargaTemas? ultimoResultadoCargaTemas;
    private EstadoCargaTemas? ultimoEstadoCargaTemasNotificado;
    private int versionSolicitudVistaPrevia;
    private bool calculoVistaPreviaEnCurso;

    private void ConfigurarDisenoNuevaPracticaAdaptable() {
        if (disenoNuevaPracticaConfigurado) {
            return;
        }

        panelVistaNuevaPractica.Dock = DockStyle.Fill;
        panelNuevaPracticaTarjeta.Anchor = AnchorStyles.None;
        ConfigurarCampoObjetivoEndForge();
        fuenteNombreFinalVacia = new Font(
            "Segoe UI",
            10F,
            FontStyle.Italic);
        fuenteNombreFinalCompleta = new Font(
            "Segoe UI Semibold",
            11F,
            FontStyle.Bold);
        lblNombreFinal.Disposed += (_, _) => {
            fuenteNombreFinalVacia.Dispose();
            fuenteNombreFinalCompleta.Dispose();
        };

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

        FormClosing += FrmPrincipal_OperacionesFormClosing;
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
        lblNombreFinal.Font = fuenteNombreFinalVacia;
    }

    private void MostrarVistaPreviaNoDisponible(
        ResultadoVistaPreviaPractica resultado) {
        lblNombreFinal.Text = resultado.EstadoNumeracion switch {
            EstadoNumeracionPractica.TemaInexistente =>
                "El tema ya no está disponible.",
            EstadoNumeracionPractica.PermisosInsuficientes =>
                "No hay permisos para calcular el siguiente número.",
            EstadoNumeracionPractica.LimiteAlcanzado =>
                "No hay un número de práctica disponible.",
            _ => "No se pudo calcular el siguiente número."
        };
        lblNombreFinal.ForeColor = Color.LightCoral;
        lblNombreFinal.Font = fuenteNombreFinalVacia;
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

    private void CargarTemas(
        ResultadoCargaTemas? resultadoPrecargado = null) {
        txtTemas.Items.Clear();

        ResultadoCargaTemas resultado =
            resultadoPrecargado ??
            temasService.CargarTemasDetallado(rutaBase);
        ultimoResultadoCargaTemas = resultado;

        foreach (string tema in resultado.Temas) {
            txtTemas.Items.Add(tema);
        }

        if (txtTemas.Items.Count > 0) {
            txtTemas.SelectedIndex = 0;
        }

        ActualizarVistaPrevia();
        NotificarResultadoCargaTemas(resultado);
        ValidarFormulario();
    }

    private void NotificarResultadoCargaTemas(
        ResultadoCargaTemas resultado) {
        if (resultado.EsExitosa ||
            string.IsNullOrWhiteSpace(rutaBase)) {
            ultimoEstadoCargaTemasNotificado = null;
            return;
        }

        ultimoResultadoVistaPrevia = new ResultadoVistaPreviaPractica {
            Estado = EstadoVistaPreviaPractica.NumeracionNoDisponible,
            EstadoNumeracion = resultado.Estado switch {
                EstadoCargaTemas.PermisosInsuficientes =>
                    EstadoNumeracionPractica.PermisosInsuficientes,
                EstadoCargaTemas.ErrorIo =>
                    EstadoNumeracionPractica.ErrorIo,
                _ => EstadoNumeracionPractica.TemaInexistente
            },
            Error = resultado.Error
        };
        MostrarVistaPreviaNoDisponible(ultimoResultadoVistaPrevia);

        if (ultimoEstadoCargaTemasNotificado == resultado.Estado) {
            return;
        }

        ultimoEstadoCargaTemasNotificado = resultado.Estado;
        string mensaje = resultado.Estado switch {
            EstadoCargaTemas.RutaInexistente =>
                "La ruta base configurada ya no está disponible. Abre Configuración para repararla.",
            EstadoCargaTemas.PermisosInsuficientes =>
                "No hay permisos para cargar los temas de la ruta base. Revisa Configuración o los permisos de la carpeta.",
            _ =>
                "No se pudieron cargar los temas. La unidad o carpeta puede estar desconectada o en uso."
        };

        MessageBox.Show(
            mensaje,
            "EndForge",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private async void ActualizarVistaPrevia() {
        versionSolicitudVistaPrevia++;
        string? temaInicial = txtTemas.SelectedItem?.ToString();
        string nombreInicial = txtNombreProyecto.Text;

        if (string.IsNullOrEmpty(temaInicial) ||
            string.IsNullOrWhiteSpace(nombreInicial)) {
            ultimoResultadoVistaPrevia = new ResultadoVistaPreviaPractica {
                Estado = EstadoVistaPreviaPractica.Vacia
            };
            MostrarVistaPreviaVacia();
            ValidarFormulario();
            return;
        }

        ultimoResultadoVistaPrevia = new ResultadoVistaPreviaPractica {
            Estado = EstadoVistaPreviaPractica.Vacia
        };
        ValidarFormulario();

        if (calculoVistaPreviaEnCurso) {
            return;
        }

        calculoVistaPreviaEnCurso = true;

        try {
            while (!IsDisposed && !Disposing) {
                int versionCalculada = versionSolicitudVistaPrevia;
                string rutaBaseCalculada = rutaBase;
                string? temaCalculado =
                    txtTemas.SelectedItem?.ToString();
                string nombreCalculado = txtNombreProyecto.Text;
                ResultadoVistaPreviaPractica resultado =
                    await Task.Run(() => vistaPreviaPracticaService.Calcular(
                        rutaBaseCalculada,
                        temaCalculado,
                        nombreCalculado));

                if (IsDisposed || Disposing) {
                    return;
                }

                if (versionCalculada != versionSolicitudVistaPrevia) {
                    continue;
                }

                ultimoResultadoVistaPrevia = resultado;

                if (resultado.Estado ==
                    EstadoVistaPreviaPractica.Vacia) {
                    MostrarVistaPreviaVacia();
                } else if (resultado.Estado ==
                    EstadoVistaPreviaPractica.NumeracionNoDisponible) {
                    MostrarVistaPreviaNoDisponible(resultado);
                } else {
                    lblNombreFinal.Text = resultado.NombreFinal;
                    lblNombreFinal.ForeColor =
                        Color.FromArgb(196, 128, 255);
                    lblNombreFinal.Font = fuenteNombreFinalCompleta;
                }

                ValidarFormulario();
                return;
            }
        } finally {
            calculoVistaPreviaEnCurso = false;
        }
    }

    private void ValidarFormulario() {
        btnCrearProyecto.Enabled =
            txtTemas.SelectedItem != null &&
            !string.IsNullOrWhiteSpace(txtNombreProyecto.Text) &&
            !string.IsNullOrWhiteSpace(txtObjetivo.Text) &&
            !string.IsNullOrWhiteSpace(rutaBase) &&
            !string.IsNullOrWhiteSpace(rutaPlantilla) &&
            ultimoResultadoCargaTemas?.EsExitosa == true &&
            ultimoResultadoVistaPrevia.Estado ==
                EstadoVistaPreviaPractica.Completa &&
            !creacionPracticaEnCurso;
    }

    private void CmbTemas_SelectedIndexChanged(object sender, EventArgs e) {
        ActualizarVistaPrevia();
    }

    private async Task<(
        ResultadoCreacionPractica Resultado,
        string RutaProyecto)?> EjecutarCreacionPracticaAsync(
        string temaSeleccionado,
        string nombreIntroducido,
        string objetivo,
        Action accionAlPrepararApertura,
        bool crearCarpetaTemaSiNoExiste = false,
        string? nombreTemaParaDocumentacion = null
    ) {
        if (creacionPracticaEnCurso) {
            return null;
        }

        creacionPracticaEnCurso = true;
        btnCrearProyecto.Enabled = false;
        TaskCompletionSource<bool> finalizacionCreacion = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        tareaCreacionPracticaActiva = finalizacionCreacion.Task;

        temaSeleccionado = temaSeleccionado.Trim();

        try {
            ResultadoValidacionNombrePractica validacionNombre =
                nombrePracticaService.Validar(nombreIntroducido);

            if (!validacionNombre.EsValido) {
                MessageBox.Show(validacionNombre.MensajeError);
                txtNombreProyecto.Focus();
                return null;
            }

            string rutaBaseActual = rutaBase;
            string rutaPlantillaActual = rutaPlantilla;
            ResultadoValidacionConfiguracion validacionConfiguracion =
                await Task.Run(() =>
                    configuracionService.ValidarConfiguracionDetallada(
                        rutaBaseActual,
                        rutaPlantillaActual));

            if (IsDisposed || Disposing) {
                return null;
            }

            if (validacionConfiguracion.Estado !=
                EstadoValidacionConfiguracion.Valida) {
                MessageBox.Show(
                    ObtenerMensajeValidacionConfiguracion(
                        validacionConfiguracion.Estado),
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return null;
            }

            bool temaExiste = await Task.Run(() =>
                temasService.ExisteTema(rutaBaseActual, temaSeleccionado));

            if (!temaExiste &&
                crearCarpetaTemaSiNoExiste &&
                !IntentarCrearCarpetaTemaCurso(
                    temaSeleccionado,
                    rutaBaseActual)) {
                return null;
            }

            temaExiste = await Task.Run(() =>
                temasService.ExisteTema(rutaBaseActual, temaSeleccionado));

            if (!temaExiste) {
                MessageBox.Show(
                    "El tema seleccionado ya no está disponible en la ruta base configurada.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return null;
            }

            ResultadoVistaPreviaPractica vistaPrevia = await Task.Run(() =>
                vistaPreviaPracticaService.Calcular(
                    rutaBaseActual,
                    temaSeleccionado,
                    validacionNombre.NombreNormalizado));

            if (IsDisposed || Disposing) {
                return null;
            }

            if (vistaPrevia.Estado != EstadoVistaPreviaPractica.Completa) {
                ultimoResultadoVistaPrevia = vistaPrevia;
                MostrarVistaPreviaNoDisponible(vistaPrevia);
                MessageBox.Show(
                    "No se pudo determinar de forma segura el siguiente número de práctica. Verifica la ruta base y sus permisos.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return null;
            }

            string nombreProyecto = vistaPrevia.NombreFinal.Trim();
            string rutaProyecto = Path.Combine(
                rutaBaseActual,
                temaSeleccionado,
                nombreProyecto);

            SolicitudCreacionPractica solicitud = new() {
                RutaPlantilla = rutaPlantillaActual,
                RutaProyecto = rutaProyecto,
                RutaBaseConfiable = rutaBaseActual,
                NombreProyecto = nombreProyecto,
                Tema = string.IsNullOrWhiteSpace(nombreTemaParaDocumentacion)
                    ? temaSeleccionado
                    : nombreTemaParaDocumentacion.Trim(),
                Objetivo = objetivo.Trim(),
                RutaRelativaSolucionEsperada =
                    seleccionSolucionesService.TransformarRutaRelativa(
                        validacionConfiguracion.RutaRelativaSolucion,
                        nombreProyecto)
            };

            ResultadoCreacionPractica resultado =
                await creacionPracticasOrquestador.CrearPracticaAsync(
                    solicitud,
                    resultadoRecientes => {
                        if (IsDisposed ||
                            Disposing ||
                            esperandoCierreOperaciones) {
                            return;
                        }

                        if (resultadoRecientes.EsExitosa) {
                            CargarRecientes();
                        }
                    },
                    () => {
                        if (!IsDisposed &&
                            !Disposing &&
                            !esperandoCierreOperaciones) {
                            accionAlPrepararApertura();
                        }
                    });

            return (resultado, rutaProyecto);
        } finally {
            creacionPracticaEnCurso = false;
            tareaCreacionPracticaActiva = null;

            if (!IsDisposed &&
                !Disposing &&
                !esperandoCierreOperaciones) {
                ValidarFormulario();
            }

            finalizacionCreacion.TrySetResult(true);
        }
    }

    private bool IntentarCrearCarpetaTemaCurso(
        string rutaRelativaTema,
        string? rutaBaseOperacion = null) {
        try {
            if (!temasService.IntentarObtenerRutaTemaSeguraParaCreacion(
                    rutaBaseOperacion ?? rutaBase,
                    rutaRelativaTema,
                    out string rutaTemaCompleta) ||
                File.Exists(rutaTemaCompleta)) {
                MessageBox.Show(
                    "No se pudo preparar la carpeta del tema dentro de la ruta base.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            Directory.CreateDirectory(rutaTemaCompleta);
            return temasService.ExisteTema(
                rutaBaseOperacion ?? rutaBase,
                rutaRelativaTema);
        } catch (UnauthorizedAccessException) {
            MessageBox.Show(
                "No hay permisos para crear la carpeta del grado y el tema.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        } catch (IOException) {
            MessageBox.Show(
                "No se pudo crear la carpeta del grado y el tema. Verifica que la ruta no esté bloqueada.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        } catch (Exception) {
            MessageBox.Show(
                "No se pudo preparar la carpeta del grado y el tema.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }
    }

    private async void BtnCrearProyecto_Click(object sender, EventArgs e) {
        (
            ResultadoCreacionPractica Resultado,
            string RutaProyecto)? ejecucion =
            await EjecutarCreacionPracticaAsync(
            txtTemas.Text,
            txtNombreProyecto.Text,
            txtObjetivo.Text,
            () => {
                txtNombreProyecto.Clear();
                campoObjetivoEndForge.Clear();
                txtNombreProyecto.Focus();

                ActualizarVistaPrevia();
                ValidarFormulario();
            }
        );

        if (ejecucion is null ||
            IsDisposed ||
            Disposing ||
            esperandoCierreOperaciones) {
            return;
        }

        MostrarResultadoCreacionPractica(
            ejecucion.Value.Resultado,
            enfocarNombreProyecto: true);
    }

    private bool MostrarResultadoCreacionPractica(
        ResultadoCreacionPractica resultado,
        bool enfocarNombreProyecto
    ) {
        if (resultado.ErrorSecundario is not null) {
            Program.RegistrarErrorRecuperable(resultado.ErrorSecundario);
        }

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

        if (resultado.Estado ==
            EstadoCreacionPractica.CreadaAbiertaSinRegistroReciente) {
            if (resultado.RegistroReciente is not null) {
                MostrarResultadoEscrituraRecientes(
                    resultado.RegistroReciente,
                    "La práctica se creó y abrió correctamente");
            } else {
                MessageBox.Show(
                    "La práctica se creó y abrió correctamente, pero no pudo guardarse en Recientes.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return true;
        }

        if (resultado.RegistroReciente is not null) {
            int registrosIgnorados =
                resultado.RegistroReciente.RegistrosInvalidosIgnorados +
                resultado.RegistroReciente.RegistrosNoDisponiblesIgnorados;

            if (registrosIgnorados > 0) {
                MostrarResultadoEscrituraRecientes(
                    resultado.RegistroReciente,
                    "La práctica se creó y abrió correctamente");
                return true;
            }
        }

        if (resultado.ErrorSecundario is not null) {
            MessageBox.Show(
                "La práctica se creó y abrió correctamente, pero EndForge no pudo actualizar por completo la interfaz.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return true;
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

}
