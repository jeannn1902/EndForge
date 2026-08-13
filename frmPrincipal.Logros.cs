using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private sealed record TarjetaLogroVisual(
        Panel Tarjeta,
        Label Nombre,
        Label Descripcion,
        Label Estado,
        Label Detalle);

    private sealed record SeccionLogrosVisual(
        SeccionLogroPresentable Seccion,
        Panel Contenedor,
        Label Titulo,
        IReadOnlyList<TarjetaLogroVisual> Tarjetas);

    private static readonly Color ColorTarjetaLogroDesbloqueado =
        Color.FromArgb(34, 29, 48);
    private static readonly Color ColorEstadoLogroPendiente =
        Color.FromArgb(190, 181, 205);

    private bool estructuraLogrosInicializada;
    private bool tarjetasLogrosConstruidas;
    private int ultimoAnchoContenidoLogros = -1;
    private int ultimoDpiLogros = -1;
    private bool ultimoModoAmplioLogros;
    private int valorBarraResumenLogros;
    private PresentacionLogros? ultimaPresentacionLogros;
    private bool enfocarVerLogrosInicioAlMostrar;

    private Panel panelLogrosVista = null!;
    private Panel panelEncabezadoLogros = null!;
    private Label lblTituloLogros = null!;
    private Label lblSubtituloLogros = null!;
    private BotonInicio btnVolverInicioLogros = null!;
    private PanelDesplazableSinBarras desplazamientoLogros = null!;
    private FlowLayoutPanel contenidoLogros = null!;
    private Panel panelResumenLogros = null!;
    private Label lblTituloResumenLogros = null!;
    private Label lblConteoResumenLogros = null!;
    private Label lblMensajeResumenLogros = null!;
    private Panel panelPistaResumenLogros = null!;
    private Panel panelRellenoResumenLogros = null!;
    private readonly List<SeccionLogrosVisual> seccionesLogrosVisuales = new();
    private readonly Dictionary<string, TarjetaLogroVisual> tarjetasLogrosPorId =
        new(StringComparer.OrdinalIgnoreCase);

    private void InicializarEstructuraLogros() {
        if (estructuraLogrosInicializada) {
            return;
        }

        panelLogrosVista = new Panel {
            Name = "panelLogrosVista",
            BackColor = ColorFondoInicio,
            Margin = Padding.Empty,
            TabStop = false,
            Visible = false,
            AccessibleName = "Vista de logros",
            AccessibleDescription =
                "Consulta los logros desbloqueados y pendientes de EndForge."
        };
        panelEncabezadoLogros = CrearContenedorInicio("panelEncabezadoLogros");
        lblTituloLogros = CrearLabelInicio(
            "Logros",
            25F,
            FontStyle.Bold,
            Color.White,
            ColorFondoInicio);
        lblSubtituloLogros = CrearLabelInicio(
            "Reconoce tu avance a lo largo del curso.",
            10.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorFondoInicio);
        btnVolverInicioLogros = CrearBotonInicio(
            "Volver a Inicio",
            esPrimario: false,
            tabIndex: 0);
        btnVolverInicioLogros.AccessibleName = "Volver a Inicio";
        btnVolverInicioLogros.AccessibleDescription =
            "Cierra la vista de logros y vuelve al resumen de Inicio.";
        btnVolverInicioLogros.Click += (_, _) => VolverAInicioDesdeLogros();
        panelEncabezadoLogros.Controls.Add(lblTituloLogros);
        panelEncabezadoLogros.Controls.Add(lblSubtituloLogros);
        panelEncabezadoLogros.Controls.Add(btnVolverInicioLogros);

        desplazamientoLogros = new PanelDesplazableSinBarras {
            Name = "desplazamientoLogros",
            BackColor = ColorFondoInicio,
            ColorFondoContenido = ColorFondoInicio,
            Padding = Padding.Empty,
            MostrarBordeFoco = true,
            TabIndex = 1,
            AccessibleName = "Lista de logros",
            AccessibleDescription =
                "Lista desplazable de logros, organizada en cuatro secciones."
        };
        contenidoLogros = desplazamientoLogros.Contenido;
        contenidoLogros.Name = "contenidoLogros";
        contenidoLogros.BackColor = ColorFondoInicio;
        contenidoLogros.Padding = Padding.Empty;
        contenidoLogros.WrapContents = false;
        contenidoLogros.FlowDirection = FlowDirection.TopDown;

        ConstruirResumenLogros();
        contenidoLogros.Controls.Add(panelResumenLogros);

        panelLogrosVista.Controls.Add(panelEncabezadoLogros);
        panelLogrosVista.Controls.Add(desplazamientoLogros);
        panelPrincipal.Controls.Add(panelLogrosVista);
        ActivarDobleBuffer(panelLogrosVista);
        ActivarDobleBuffer(panelEncabezadoLogros);
        panelLogrosVista.VisibleChanged += PanelLogrosVista_VisibleChanged;
        estructuraLogrosInicializada = true;
        KeyDown += FrmPrincipal_LogrosKeyDown;
        ActualizarGeometriaLogros();
    }

    private void ConstruirResumenLogros() {
        panelResumenLogros = CrearTarjetaInicio(
            "panelResumenLogros",
            ColorTarjetaDestacadaInicio);
        lblTituloResumenLogros = CrearLabelInicio(
            "TU PROGRESO EN LOGROS",
            8.5F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaDestacadaInicio);
        lblConteoResumenLogros = CrearLabelInicio(
            "—",
            18F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaDestacadaInicio);
        lblMensajeResumenLogros = CrearLabelInicio(
            "Actualizando tus logros...",
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaDestacadaInicio,
            ContentAlignment.MiddleRight);
        panelPistaResumenLogros = new Panel {
            BackColor = Color.FromArgb(53, 45, 67),
            AccessibleName = "Progreso de logros"
        };
        panelRellenoResumenLogros = new Panel {
            BackColor = ColorMoradoCurso
        };
        panelPistaResumenLogros.Controls.Add(panelRellenoResumenLogros);
        panelResumenLogros.Controls.Add(lblTituloResumenLogros);
        panelResumenLogros.Controls.Add(lblConteoResumenLogros);
        panelResumenLogros.Controls.Add(lblMensajeResumenLogros);
        panelResumenLogros.Controls.Add(panelPistaResumenLogros);
    }

    private void AsegurarTarjetasLogrosConstruidas(
        PresentacionLogros presentacion) {
        if (tarjetasLogrosConstruidas) {
            return;
        }

        foreach (IGrouping<SeccionLogroPresentable, PresentacionLogro> grupo in
            presentacion.Logros
                .OrderBy(logro => logro.Orden)
                .GroupBy(logro => logro.Seccion)) {
            PresentacionLogro[] logros = grupo
                .OrderBy(logro => logro.Orden)
                .ToArray();
            Panel contenedor = CrearContenedorInicio(
                $"panelSeccionLogros{grupo.Key}");
            Label titulo = CrearLabelInicio(
                logros[0].TituloSeccion,
                10F,
                FontStyle.Bold,
                ColorMoradoClaroCurso,
                ColorFondoInicio);
            List<TarjetaLogroVisual> tarjetas = new(logros.Length);
            contenedor.Controls.Add(titulo);

            foreach (PresentacionLogro logro in logros) {
                TarjetaLogroVisual tarjeta = CrearTarjetaLogro(logro.Id);
                tarjetas.Add(tarjeta);
                tarjetasLogrosPorId.Add(logro.Id, tarjeta);
                contenedor.Controls.Add(tarjeta.Tarjeta);
            }

            SeccionLogrosVisual seccion = new(
                grupo.Key,
                contenedor,
                titulo,
                tarjetas.AsReadOnly());
            seccionesLogrosVisuales.Add(seccion);
            contenidoLogros.Controls.Add(contenedor);
        }

        tarjetasLogrosConstruidas = true;
    }

    private TarjetaLogroVisual CrearTarjetaLogro(string id) {
        Panel tarjeta = CrearTarjetaInicio(
            $"tarjetaLogro{tarjetasLogrosPorId.Count + 1}",
            ColorTarjetaInicio);
        tarjeta.TabStop = false;
        tarjeta.AccessibleRole = AccessibleRole.Grouping;
        tarjeta.Tag = id;
        Label nombre = CrearLabelInicio(
            string.Empty,
            11.5F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaInicio);
        Label descripcion = CrearLabelInicio(
            string.Empty,
            9.25F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio);
        Label estado = CrearLabelInicio(
            "PENDIENTE",
            8.25F,
            FontStyle.Bold,
            ColorEstadoLogroPendiente,
            ColorTarjetaInicio);
        Label detalle = CrearLabelInicio(
            string.Empty,
            8.5F,
            FontStyle.Regular,
            ColorTextoTenueInicio,
            ColorTarjetaInicio,
            ContentAlignment.MiddleRight);
        tarjeta.Controls.Add(nombre);
        tarjeta.Controls.Add(descripcion);
        tarjeta.Controls.Add(estado);
        tarjeta.Controls.Add(detalle);
        return new TarjetaLogroVisual(
            tarjeta,
            nombre,
            descripcion,
            estado,
            detalle);
    }

    private void AplicarPresentacionLogros(PresentacionLogros presentacion) {
        ArgumentNullException.ThrowIfNull(presentacion);
        ultimaPresentacionLogros = presentacion;
        btnVerLogrosInicio.Enabled = !accionInicioEnCurso;
        AsegurarTarjetasLogrosConstruidas(presentacion);

        lblConteoResumenLogros.Text = presentacion.TextoResumen;
        lblMensajeResumenLogros.Text = presentacion.MensajeDisponibilidad;
        valorBarraResumenLogros = presentacion.LogrosDesbloqueados.HasValue &&
            presentacion.TotalLogros > 0
                ? Math.Clamp(
                    (int)Math.Round(
                        presentacion.LogrosDesbloqueados.Value * 100D /
                        presentacion.TotalLogros),
                    0,
                    100)
                : 0;
        panelPistaResumenLogros.Visible =
            presentacion.LogrosDesbloqueados.HasValue;
        panelResumenLogros.AccessibleName = "Resumen de logros";
        panelResumenLogros.AccessibleDescription =
            presentacion.LogrosDesbloqueados.HasValue
                ? presentacion.TextoResumen
                : presentacion.MensajeDisponibilidad;

        foreach (PresentacionLogro logro in presentacion.Logros) {
            if (!tarjetasLogrosPorId.TryGetValue(
                    logro.Id,
                    out TarjetaLogroVisual? visual)) {
                continue;
            }

            bool desbloqueado = logro.Estado ==
                EstadoLogroPresentable.Desbloqueado;
            Color fondo = desbloqueado
                ? ColorTarjetaLogroDesbloqueado
                : ColorTarjetaInicio;
            visual.Tarjeta.BackColor = fondo;
            visual.Nombre.BackColor = fondo;
            visual.Descripcion.BackColor = fondo;
            visual.Estado.BackColor = fondo;
            visual.Detalle.BackColor = fondo;
            visual.Nombre.Text = logro.Nombre;
            visual.Descripcion.Text = logro.Descripcion;
            visual.Estado.Text = logro.TextoEstado;
            visual.Estado.ForeColor = logro.Estado switch {
                EstadoLogroPresentable.Desbloqueado => ColorExitoInicio,
                EstadoLogroPresentable.Pendiente => ColorEstadoLogroPendiente,
                _ => ColorTextoTenueInicio
            };
            visual.Detalle.Text = !string.IsNullOrWhiteSpace(logro.TextoFecha)
                ? logro.TextoFecha
                : logro.TextoProgreso;
            visual.Tarjeta.AccessibleName = logro.Nombre;
            visual.Tarjeta.AccessibleDescription =
                logro.DescripcionAccesible;
        }

        ultimoAnchoContenidoLogros = -1;

        if (panelLogrosVista.Visible &&
            distribucionPanelPrincipal == DistribucionPanelPrincipal.Logros) {
            ActualizarGeometriaLogros();
        }
    }

    private void MostrarLogrosDesdeInicio() {
        if (!estructuraLogrosInicializada ||
            accionInicioEnCurso ||
            ultimaPresentacionLogros is null) {
            return;
        }

        OcultarNotificacionLogrosInicio();
        NavegarVistaPrincipalConTransicion(
            panelLogrosVista,
            panelInicio,
            DistribucionPanelPrincipal.Logros,
            PrepararLogrosParaMostrar);
    }

    private void VolverAInicioDesdeLogros() {
        if (navegacionCursoEnCurso || transicionVisualCursoActiva) {
            return;
        }

        enfocarVerLogrosInicioAlMostrar = true;
        NavegarVistaPrincipalConTransicion(
            panelInicioVista,
            panelInicio,
            DistribucionPanelPrincipal.Inicio,
            PrepararInicioParaMostrar);
    }

    private void PrepararLogrosParaMostrar() {
        if (!estructuraLogrosInicializada) {
            return;
        }

        if (ultimaPresentacionLogros is not null) {
            AplicarPresentacionLogros(ultimaPresentacionLogros);
        } else {
            lblConteoResumenLogros.Text = "—";
            lblMensajeResumenLogros.Text =
                "Actualizando tus logros...";
            panelPistaResumenLogros.Visible = false;
        }

        ActualizarGeometriaLogros();
    }

    private void OcultarVistaLogros() {
        if (estructuraLogrosInicializada) {
            panelLogrosVista.Visible = false;
        }
    }

    private void SincronizarLimitesVistaLogros(Rectangle limites) {
        if (!estructuraLogrosInicializada) {
            return;
        }

        AplicarBoundsSiCambian(
            panelLogrosVista,
            new Rectangle(
                limites.Left,
                limites.Top,
                Math.Max(1, limites.Width),
                Math.Max(1, limites.Height)));
    }

    private void ActualizarGeometriaLogros() {
        if (!estructuraLogrosInicializada ||
            panelLogrosVista.IsDisposed ||
            (distribucionPanelPrincipal != DistribucionPanelPrincipal.Logros &&
             !panelLogrosVista.Visible) ||
            panelLogrosVista.ClientSize.Width <= 0 ||
            panelLogrosVista.ClientSize.Height <= 0) {
            return;
        }

        Rectangle area = panelLogrosVista.ClientRectangle;
        int margenSuperior = EscalarDiseno(12);
        int altoEncabezado = EscalarDiseno(78);
        int yDesplazamiento =
            margenSuperior + altoEncabezado + EscalarDiseno(8);
        AplicarBoundsSiCambian(
            desplazamientoLogros,
            new Rectangle(
                area.Left,
                yDesplazamiento,
                Math.Max(1, area.Width),
                Math.Max(1, area.Bottom - yDesplazamiento)));

        int anchoContenido = Math.Max(1, contenidoLogros.ClientSize.Width);
        int anchoContenidoLogico = ConvertirALogicoLogros(anchoContenido);
        MedidasLayoutLogros medidasVista = CalculadorLayoutLogros.Calcular(
            anchoContenidoLogico,
            0);
        int xEncabezado = area.Left + EscalarDiseno(
            medidasVista.XContenido);
        int anchoEncabezado = Math.Min(
            anchoContenido - EscalarDiseno(medidasVista.XContenido),
            EscalarDiseno(medidasVista.AnchoContenido));
        bool cambioEncabezado = AplicarBoundsSiCambian(
            panelEncabezadoLogros,
            new Rectangle(
                xEncabezado,
                margenSuperior,
                Math.Max(1, anchoEncabezado),
                altoEncabezado));

        if (cambioEncabezado || DeviceDpi != ultimoDpiLogros) {
            ActualizarGeometriaEncabezadoLogros();
        }

        bool modoAmplio = CalculadorLayoutLogros.DeterminarModoAmplio(
            anchoContenidoLogico);

        bool cambioGeometria =
            anchoContenido != ultimoAnchoContenidoLogros ||
            DeviceDpi != ultimoDpiLogros ||
            modoAmplio != ultimoModoAmplioLogros;

        if (cambioGeometria) {
            ActualizarGeometriaContenidoLogros(
                anchoContenido,
                anchoContenidoLogico);
            ultimoAnchoContenidoLogros = anchoContenido;
            ultimoDpiLogros = DeviceDpi;
            ultimoModoAmplioLogros = modoAmplio;
        } else {
            ActualizarRellenoResumenLogros();
        }

        if (cambioGeometria) {
            desplazamientoLogros.ActualizarContenido(volverAlInicio: false);
        }
    }

    private void ActualizarGeometriaEncabezadoLogros() {
        int ancho = panelEncabezadoLogros.ClientSize.Width;
        int anchoBoton = Math.Min(EscalarDiseno(150), Math.Max(1, ancho / 3));
        int separacion = EscalarDiseno(12);
        int anchoTexto = Math.Max(1, ancho - anchoBoton - separacion);
        lblTituloLogros.SetBounds(
            0,
            0,
            anchoTexto,
            EscalarDiseno(42));
        lblSubtituloLogros.SetBounds(
            0,
            EscalarDiseno(42),
            anchoTexto,
            EscalarDiseno(28));
        btnVolverInicioLogros.SetBounds(
            Math.Max(0, ancho - anchoBoton),
            EscalarDiseno(18),
            anchoBoton,
            EscalarDiseno(38));
    }

    private void ActualizarGeometriaContenidoLogros(
        int anchoReal,
        int anchoLogico) {
        contenidoLogros.SuspendLayout();

        try {
            MedidasLayoutLogros resumen = CalculadorLayoutLogros.Calcular(
                anchoLogico,
                0);
            int izquierda = EscalarDiseno(resumen.XContenido);
            int anchoBloque = Math.Max(
                1,
                Math.Min(
                    anchoReal - izquierda,
                    EscalarDiseno(resumen.AnchoContenido)));
            panelResumenLogros.Size = new Size(
                anchoBloque,
                EscalarDiseno(84));
            panelResumenLogros.Margin = new Padding(
                izquierda,
                0,
                0,
                EscalarDiseno(16));
            ActualizarGeometriaResumenLogros();

            foreach (SeccionLogrosVisual seccion in seccionesLogrosVisuales) {
                MedidasLayoutLogros medidas = CalculadorLayoutLogros.Calcular(
                    anchoLogico,
                    seccion.Tarjetas.Count);
                int altoTitulo = 30;
                int altoSeccion = altoTitulo + medidas.AltoContenido;
                seccion.Contenedor.Size = new Size(
                    Math.Max(1, EscalarDiseno(medidas.AnchoContenido)),
                    Math.Max(1, EscalarDiseno(altoSeccion)));
                seccion.Contenedor.Margin = new Padding(
                    EscalarDiseno(medidas.XContenido),
                    0,
                    0,
                    EscalarDiseno(16));
                seccion.Titulo.SetBounds(
                    0,
                    0,
                    seccion.Contenedor.Width,
                    EscalarDiseno(22));

                for (int indice = 0;
                    indice < seccion.Tarjetas.Count;
                    indice++) {
                    RectanguloLayoutLogro rectangulo =
                        medidas.ObtenerRectanguloTarjeta(indice);
                    int x = EscalarDiseno(
                        rectangulo.X - medidas.XContenido);
                    int derecha = EscalarDiseno(
                        rectangulo.Derecha - medidas.XContenido);
                    TarjetaLogroVisual tarjeta = seccion.Tarjetas[indice];
                    tarjeta.Tarjeta.SetBounds(
                        x,
                        EscalarDiseno(
                            altoTitulo + rectangulo.Y - medidas.Margen),
                        Math.Max(1, derecha - x),
                        EscalarDiseno(rectangulo.Alto));
                    ActualizarGeometriaTarjetaLogro(
                        tarjeta,
                        medidas.ModoAmplio);
                }
            }
        } finally {
            contenidoLogros.ResumeLayout(performLayout: false);
        }
    }

    private void ActualizarGeometriaResumenLogros() {
        int margen = EscalarDiseno(18);
        int ancho = Math.Max(1, panelResumenLogros.Width - margen * 2);
        int mitad = Math.Max(1, ancho / 2);
        lblTituloResumenLogros.SetBounds(
            margen,
            EscalarDiseno(9),
            ancho,
            EscalarDiseno(18));
        lblConteoResumenLogros.SetBounds(
            margen,
            EscalarDiseno(28),
            mitad,
            EscalarDiseno(30));
        lblMensajeResumenLogros.SetBounds(
            margen + mitad,
            EscalarDiseno(31),
            Math.Max(1, ancho - mitad),
            EscalarDiseno(24));
        panelPistaResumenLogros.SetBounds(
            margen,
            EscalarDiseno(65),
            ancho,
            EscalarDiseno(6));
        ActualizarRellenoResumenLogros();
    }

    private void ActualizarRellenoResumenLogros() {
        if (!estructuraLogrosInicializada ||
            panelPistaResumenLogros.IsDisposed) {
            return;
        }

        panelRellenoResumenLogros.SetBounds(
            0,
            0,
            (int)Math.Round(
                panelPistaResumenLogros.ClientSize.Width *
                valorBarraResumenLogros / 100D),
            panelPistaResumenLogros.ClientSize.Height);
    }

    private void ActualizarGeometriaTarjetaLogro(
        TarjetaLogroVisual visual,
        bool modoAmplio) {
        int margen = EscalarDiseno(16);
        int ancho = Math.Max(1, visual.Tarjeta.Width - margen * 2);
        int altoDescripcion = EscalarDiseno(modoAmplio ? 38 : 50);
        int yEstado = EscalarDiseno(modoAmplio ? 84 : 96);
        int separacion = EscalarDiseno(8);
        int anchoEstado = Math.Max(1, (ancho - separacion) * 2 / 5);
        visual.Nombre.SetBounds(
            margen,
            EscalarDiseno(10),
            ancho,
            EscalarDiseno(24));
        visual.Descripcion.SetBounds(
            margen,
            EscalarDiseno(34),
            ancho,
            altoDescripcion);
        visual.Estado.SetBounds(
            margen,
            yEstado,
            anchoEstado,
            EscalarDiseno(18));
        visual.Detalle.SetBounds(
            margen + anchoEstado + separacion,
            yEstado,
            Math.Max(1, ancho - anchoEstado - separacion),
            EscalarDiseno(18));
    }

    private int ConvertirALogicoLogros(int valor) {
        return Math.Max(
            1,
            (int)Math.Round(valor * 96D / Math.Max(1, DeviceDpi)));
    }

    private void FrmPrincipal_LogrosKeyDown(object? sender, KeyEventArgs e) {
        if (!entradaAplicacionRealizada ||
            e.KeyCode != Keys.Escape ||
            !estructuraLogrosInicializada ||
            !panelLogrosVista.Visible) {
            return;
        }

        VolverAInicioDesdeLogros();
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void PanelLogrosVista_VisibleChanged(
        object? sender,
        EventArgs e) {
        if (!panelLogrosVista.Visible ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        ProgramarAccionInterfazSegura(() => {
            if (panelLogrosVista.Visible &&
                !btnVolverInicioLogros.IsDisposed &&
                btnVolverInicioLogros.CanFocus) {
                btnVolverInicioLogros.Focus();
            }
        });
    }
}
