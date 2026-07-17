using EndForge.Models;
using EndForge.Services;
using EndForge.Controls;
using System.Drawing.Drawing2D;

namespace EndForge;

public partial class frmPrincipal {
    private sealed class PresentacionTarjetaTema {
        public PresentacionTarjetaTema(
            Label numero,
            Label nombre,
            Label descripcion,
            Label porcentaje,
            Label progreso,
            Label estado,
            Panel fondoProgreso,
            Panel rellenoProgreso,
            int valorPorcentaje) {
            Numero = numero;
            Nombre = nombre;
            Descripcion = descripcion;
            Porcentaje = porcentaje;
            Progreso = progreso;
            Estado = estado;
            FondoProgreso = fondoProgreso;
            RellenoProgreso = rellenoProgreso;
            ValorPorcentaje = valorPorcentaje;
        }

        public Label Numero { get; }
        public Label Nombre { get; }
        public Label Descripcion { get; }
        public Label Porcentaje { get; }
        public Label Progreso { get; }
        public Label Estado { get; }
        public Panel FondoProgreso { get; }
        public Panel RellenoProgreso { get; }
        public int ValorPorcentaje { get; }
    }

    private static class TamanoFuenteCurso {
        public const float TituloPerfil = 16F;
        public const float NombrePerfil = 12F;
        public const float TextoPerfil = 10.5F;
        public const float ProgresoPerfil = 9.8F;
        public const float EncabezadoPerfil = 9F;
        public const float TextoSecundarioPerfil = 9.5F;
        public const float RecomendacionPerfil = 9.3F;
        public const float AvisoPerfil = 8.5F;
        public const float TituloSeccion = 16F;
        public const float NumeroCabecera = 10F;
        public const float TituloPrincipal = 22F;
        public const float DescripcionCabecera = 10.5F;
        public const float NumeroTarjeta = 13F;
        public const float TituloTarjetaTema = 13.5F;
        public const float DescripcionTarjetaTema = 9F;
        public const float ProgresoTarjetaTema = 9.5F;
        public const float MetadatoTarjetaTema = 9F;
        public const float TituloTarjetaPractica = 13F;
        public const float DescripcionTarjetaPractica = 9.8F;
        public const float EstadoTarjetaPractica = 10F;
        public const float MetadatoTarjetaPractica = 9F;
        public const float EncabezadoDetalle = 11F;
        public const float TextoDetalle = 10.2F;
        public const float BotonPrimario = 10.5F;
        public const float BotonSecundario = 10F;
    }

    private static readonly Color ColorTarjetaCurso = Color.FromArgb(24, 19, 36);
    private static readonly Color ColorTarjetaCursoHover = Color.FromArgb(41, 31, 58);
    private static readonly Color ColorBordeCurso = Color.FromArgb(116, 67, 163);
    private static readonly Color ColorMoradoCurso = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorMoradoClaroCurso = Color.FromArgb(202, 151, 247);
    private static readonly Color ColorTextoSecundarioCurso = Color.FromArgb(190, 181, 204);

    private CursoService cursoService = null!;
    private ProgresoCursoService progresoCursoService = null!;
    private ProgresoCurso progresoCurso = new();
    private TemaCurso? temaCursoSeleccionado;
    private PracticaCurso? practicaCursoSeleccionada;
    private string mensajeEstadoProgresoCurso = string.Empty;

    private Panel panelCurso = null!;
    private Label lblCurso = null!;
    private PictureBox pictureBoxCurso = null!;
    private Panel panelIndicadorCurso = null!;
    private Panel panelCursoVista = null!;
    private Panel panelPracticasTemaVista = null!;
    private Panel panelDetallePracticaVista = null!;
    private PanelDesplazableSinBarras desplazamientoCursoPrincipal = null!;
    private PanelDesplazableSinBarras desplazamientoTemasCurso = null!;
    private PanelDesplazableSinBarras desplazamientoPracticasTema = null!;
    private PanelDesplazableSinBarras desplazamientoDetallePractica = null!;
    private Panel panelPerfilCurso = null!;
    private Label lblTituloTemasCurso = null!;
    private FlowLayoutPanel listaTemasCurso = null!;
    private FlowLayoutPanel listaPracticasTema = null!;
    private FlowLayoutPanel contenidoDetallePractica = null!;
    private Label lblTituloPerfilCurso = null!;
    private Label lblNombrePerfilCurso = null!;
    private Label lblNivelPerfilCurso = null!;
    private Label lblProgresoGeneralCurso = null!;
    private Panel panelFondoProgresoCurso = null!;
    private Label lblSiguientePasoCurso = null!;
    private Label lblUltimaPracticaCurso = null!;
    private Label lblAvisoProgresoCurso = null!;
    private Panel panelRellenoProgresoCurso = null!;
    private Button btnContinuarAprendizaje = null!;
    private Label lblTituloRecomendacionCurso = null!;
    private Label lblPracticaRecomendadaCurso = null!;
    private Button btnVerPracticaRecomendadaCurso = null!;
    private Button btnVolverTemasCurso = null!;
    private Button btnVolverPracticasCurso = null!;
    private Label lblNumeroTemaCurso = null!;
    private Label lblNombreTemaCurso = null!;
    private Label lblDescripcionTemaCurso = null!;
    private Size tamanoPanelPrincipalNormal;
    private bool modoCursoInmersivo;
    private bool cursoModoCompacto;
    private bool jerarquiaCursoPrincipalInicializada;
    private bool estructuraCursoInicializada;
    private bool inicializandoCurso;
    private bool cursoInicializado;
    private bool preparandoCurso;
    private bool cursoPreparado;
    private bool tarjetasTemasActualizadas;
    private Task? tareaPreparacionCurso;
    private CursoService? cursoServicePrecargado;
    private ProgresoCursoService? progresoCursoServicePrecargado;
    private ResultadoCargaProgreso? resultadoProgresoCursoPrecargado;
    private int ultimoAnchoTarjetasTemas = -1;
    private int ultimoDpiTarjetasTemas = -1;
    private bool ultimoModoCompactoTarjetasTemas;

    private void InicializarEstructuraCurso() {
        if (estructuraCursoInicializada) {
            return;
        }

        tamanoPanelPrincipalNormal = panelPrincipal.Size;
        InicializarOpcionCursoMenu();
        estructuraCursoInicializada = true;
    }

    private void EstablecerDatosPrecargadosCurso(
        CursoService servicioCurso,
        ProgresoCursoService servicioProgreso,
        ResultadoCargaProgreso resultadoProgreso) {
        if (cursoInicializado || inicializandoCurso) {
            return;
        }

        cursoServicePrecargado = servicioCurso;
        progresoCursoServicePrecargado = servicioProgreso;
        resultadoProgresoCursoPrecargado = resultadoProgreso;
    }

    private Task PrepararCursoDiferidoAsync() {
        if (cursoPreparado || IsDisposed || Disposing) {
            return Task.CompletedTask;
        }

        tareaPreparacionCurso ??= PrepararCursoDiferidoNucleoAsync();
        return tareaPreparacionCurso;
    }

    private async Task PrepararCursoDiferidoNucleoAsync() {
        if (cursoPreparado || preparandoCurso || IsDisposed || Disposing) {
            return;
        }

        preparandoCurso = true;
        inicializandoCurso = true;
        RegistrarTiempoInicio("Curso UI: inicio de construcción diferida");

        try {
            if (!await EsperarFinalizacionTransicionBienvenidaAsync() ||
                !PuedeContinuarConstruccionCurso()) {
                return;
            }

            cursoService = cursoServicePrecargado ?? new CursoService();
            progresoCursoService =
                progresoCursoServicePrecargado ?? new ProgresoCursoService();
            cursoServicePrecargado = null;
            progresoCursoServicePrecargado = null;

            EjecutarFaseConstruccionCurso(
                InicializarVistasCurso,
                "Curso UI: estructura base creada");
            if (!await CederCicloConstruccionCursoAsync()) {
                return;
            }

            EjecutarFaseConstruccionCurso(
                ConstruirResumenCurso,
                "Curso UI: perfil y contenedor de temas creados");
            if (!await CederCicloConstruccionCursoAsync()) {
                return;
            }

            EjecutarFaseConstruccionCurso(
                ConstruirVistaPracticasTema,
                "Curso UI: vista de prácticas creada");
            if (!await CederCicloConstruccionCursoAsync()) {
                return;
            }

            EjecutarFaseConstruccionCurso(
                ConstruirVistaDetallePractica,
                "Curso UI: vista de detalle creada");
            if (!await CederCicloConstruccionCursoAsync()) {
                return;
            }

            ActivarDobleBuffer(panelCursoVista);
            ActivarDobleBuffer(panelPracticasTemaVista);
            ActivarDobleBuffer(panelDetallePracticaVista);
            ActivarDobleBuffer(desplazamientoCursoPrincipal);
            ActivarDobleBuffer(desplazamientoTemasCurso);
            ActivarDobleBuffer(desplazamientoPracticasTema);
            ActivarDobleBuffer(desplazamientoDetallePractica);

            KeyDown += FrmPrincipal_CursoKeyDown;
            cursoInicializado = true;
            inicializandoCurso = false;
            RegistrarTiempoInicio("Curso UI: controles base finalizados");

            CargarProgresoCurso(
                mostrarAvisoInmediato: false,
                resultadoProgresoCursoPrecargado);
            resultadoProgresoCursoPrecargado = null;
            RegistrarTiempoInicio("Curso UI: progreso visual aplicado");
            if (!await CederCicloConstruccionCursoAsync()) {
                return;
            }

            ReconstruirTarjetasTemas();
            cursoPreparado = true;
            RegistrarTiempoInicio("Curso UI: tarjetas de temas finalizadas");
        } finally {
            inicializandoCurso = false;
            preparandoCurso = false;
            tareaPreparacionCurso = null;
        }
    }

    private void EjecutarFaseConstruccionCurso(Action fase, string etapa) {
        panelPrincipal.SuspendLayout();

        try {
            fase();
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
        }

        RegistrarTiempoInicio(etapa);
    }

    private bool PuedeContinuarConstruccionCurso() {
        return !IsDisposed && !Disposing && !inicializacionSecundariaCancelada;
    }

    private async Task<bool> CederCicloConstruccionCursoAsync() {
        await Task.Yield();

        if (!PuedeContinuarConstruccionCurso()) {
            return false;
        }

        return await EsperarFinalizacionTransicionBienvenidaAsync();
    }

    private void InicializarOpcionCursoMenu() {
        panelAbrirPractica.Location = new Point(12, 338);
        panelRecientes.Location = new Point(12, 394);
        panelConfiguracion.Location = new Point(12, 450);
        panelAcercaDe.Location = new Point(12, 506);

        panelCurso = new Panel {
            Name = "panelCurso",
            BackColor = Color.FromArgb(45, 45, 48),
            Cursor = Cursors.Hand,
            Location = new Point(12, 282),
            Size = new Size(195, 48),
            TabIndex = 5
        };

        panelIndicadorCurso = new Panel {
            Name = "panelIndicadorCurso",
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(3, 48),
            TabIndex = 2,
            Visible = false
        };

        pictureBoxCurso = new PictureBox {
            Name = "pictureBoxCurso",
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Location = new Point(15, 13),
            Size = new Size(24, 24),
            SizeMode = PictureBoxSizeMode.Zoom,
            TabIndex = 0,
            TabStop = false
        };

        lblCurso = new Label {
            Name = "lblCurso",
            AutoSize = false,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 10F),
            ForeColor = Color.White,
            Location = new Point(49, 10),
            Size = new Size(146, 28),
            TabIndex = 1,
            Text = "Curso",
            TextAlign = ContentAlignment.MiddleLeft
        };

        panelCurso.Controls.Add(panelIndicadorCurso);
        panelCurso.Controls.Add(lblCurso);
        panelCurso.Controls.Add(pictureBoxCurso);
        panelMenu.Controls.Add(panelCurso);

        panelCurso.MouseEnter += PanelMenu_MouseEnter;
        panelCurso.MouseLeave += PanelMenu_MouseLeave;
        lblCurso.MouseEnter += PanelMenu_MouseEnter;
        lblCurso.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxCurso.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxCurso.MouseLeave += PanelMenu_MouseLeave;
        panelCurso.Click += PanelCurso_Click;
        lblCurso.Click += PanelCurso_Click;
        pictureBoxCurso.Click += PanelCurso_Click;
    }

    private void InicializarVistasCurso() {
        panelCursoVista = CrearVistaCurso("panelCursoVista");
        panelPracticasTemaVista = CrearVistaCurso("panelPracticasTemaVista");
        panelDetallePracticaVista = CrearVistaCurso("panelDetallePracticaVista");

        panelPrincipal.Controls.Add(panelCursoVista);
        panelPrincipal.Controls.Add(panelPracticasTemaVista);
        panelPrincipal.Controls.Add(panelDetallePracticaVista);
    }

    private static Panel CrearVistaCurso(string nombre) {
        return new Panel {
            Name = nombre,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            Location = Point.Empty,
            Size = new Size(892, 470),
            Visible = false
        };
    }

    private static PanelDesplazableSinBarras CrearPanelDesplazableCurso(
        string nombre,
        string nombreContenido,
        Point ubicacion,
        Size tamano,
        Padding rellenoContenido) {
        PanelDesplazableSinBarras desplazamiento = new() {
            Name = nombre,
            BackColor = ColorBordeCurso,
            ColorFondoContenido = Color.FromArgb(18, 14, 27),
            Location = ubicacion,
            Size = tamano
        };
        desplazamiento.Contenido.Name = nombreContenido;
        desplazamiento.Contenido.Padding = rellenoContenido;
        return desplazamiento;
    }

    private void ConstruirResumenCurso() {
        panelPerfilCurso = CrearTarjetaCurso(new Point(18, 18), new Size(278, 434), 18);
        panelPerfilCurso.TabIndex = 0;

        lblTituloPerfilCurso = CrearLabelCurso(
            "Curso de C++",
            new Point(22, 18),
            new Size(232, 34),
            TamanoFuenteCurso.TituloPerfil,
            FontStyle.Bold,
            Color.White);
        lblNombrePerfilCurso = CrearLabelCurso(
            "Jean Pérez",
            new Point(22, 54),
            new Size(232, 24),
            TamanoFuenteCurso.NombrePerfil,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        lblNivelPerfilCurso = CrearLabelCurso(
            "Nivel actual: Pre-Junior",
            new Point(22, 80),
            new Size(232, 22),
            TamanoFuenteCurso.TextoPerfil,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        lblProgresoGeneralCurso = CrearLabelCurso(
            $"Progreso general\n0 de {cursoService.TotalPracticasDisponibles} prácticas disponibles",
            new Point(22, 106),
            new Size(232, 42),
            TamanoFuenteCurso.ProgresoPerfil,
            FontStyle.Bold,
            Color.White,
            ContentAlignment.TopLeft);

        panelFondoProgresoCurso = new Panel {
            BackColor = Color.FromArgb(55, 45, 70),
            Location = new Point(22, 151),
            Size = new Size(232, 9)
        };
        panelRellenoProgresoCurso = new Panel {
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(0, 9)
        };
        panelFondoProgresoCurso.Controls.Add(panelRellenoProgresoCurso);

        lblSiguientePasoCurso = CrearLabelCurso(
            "TU SIGUIENTE PASO",
            new Point(22, 171),
            new Size(232, 18),
            TamanoFuenteCurso.EncabezadoPerfil,
            FontStyle.Bold,
            Color.FromArgb(157, 135, 181));

        lblUltimaPracticaCurso = CrearLabelCurso(
            "Aún no hay prácticas iniciadas.",
            new Point(22, 191),
            new Size(232, 50),
            TamanoFuenteCurso.TextoSecundarioPerfil,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        btnContinuarAprendizaje = CrearBotonCurso(
            "Continuar aprendizaje",
            new Point(22, 244),
            new Size(232, 34),
            ColorMoradoCurso);
        btnContinuarAprendizaje.TabIndex = 0;
        btnContinuarAprendizaje.AccessibleDescription =
            "Abre el detalle de la última práctica en progreso sin iniciar Visual Studio.";
        btnContinuarAprendizaje.Visible = false;
        btnContinuarAprendizaje.Click += BtnContinuarAprendizaje_Click;

        lblTituloRecomendacionCurso = CrearLabelCurso(
            "PRÁCTICA RECOMENDADA",
            new Point(22, 290),
            new Size(232, 18),
            TamanoFuenteCurso.EncabezadoPerfil,
            FontStyle.Bold,
            Color.FromArgb(157, 135, 181));

        lblPracticaRecomendadaCurso = CrearLabelCurso(
            "Tema 01 · Práctica 01\nDatos personales\nFácil · 15–20 min",
            new Point(22, 310),
            new Size(232, 50),
            TamanoFuenteCurso.RecomendacionPerfil,
            FontStyle.Regular,
            ColorTextoSecundarioCurso,
            ContentAlignment.TopLeft);

        btnVerPracticaRecomendadaCurso = CrearBotonSecundarioCurso(
            "Ver práctica",
            new Point(22, 363),
            new Size(232, 34));
        btnVerPracticaRecomendadaCurso.TabIndex = 1;
        btnVerPracticaRecomendadaCurso.AccessibleDescription =
            "Abre el detalle de la práctica recomendada sin crear ni abrir el proyecto.";
        btnVerPracticaRecomendadaCurso.Click += BtnVerPracticaRecomendadaCurso_Click;

        lblAvisoProgresoCurso = CrearLabelCurso(
            string.Empty,
            new Point(22, 400),
            new Size(232, 29),
            TamanoFuenteCurso.AvisoPerfil,
            FontStyle.Regular,
            Color.FromArgb(235, 181, 89));
        lblAvisoProgresoCurso.Visible = false;

        panelPerfilCurso.Controls.Add(lblTituloPerfilCurso);
        panelPerfilCurso.Controls.Add(lblNombrePerfilCurso);
        panelPerfilCurso.Controls.Add(lblNivelPerfilCurso);
        panelPerfilCurso.Controls.Add(lblProgresoGeneralCurso);
        panelPerfilCurso.Controls.Add(panelFondoProgresoCurso);
        panelPerfilCurso.Controls.Add(lblSiguientePasoCurso);
        panelPerfilCurso.Controls.Add(lblUltimaPracticaCurso);
        panelPerfilCurso.Controls.Add(btnContinuarAprendizaje);
        panelPerfilCurso.Controls.Add(lblTituloRecomendacionCurso);
        panelPerfilCurso.Controls.Add(lblPracticaRecomendadaCurso);
        panelPerfilCurso.Controls.Add(btnVerPracticaRecomendadaCurso);
        panelPerfilCurso.Controls.Add(lblAvisoProgresoCurso);

        lblTituloTemasCurso = CrearLabelCurso(
            "TEMAS DEL CURSO",
            new Point(320, 20),
            new Size(554, 30),
            TamanoFuenteCurso.TituloSeccion,
            FontStyle.Bold,
            Color.White);

        desplazamientoTemasCurso = CrearPanelDesplazableCurso(
            "desplazamientoTemasCurso",
            "listaTemasCurso",
            new Point(316, 56),
            new Size(560, 396),
            new Padding(4, 3, 4, 4));
        listaTemasCurso = desplazamientoTemasCurso.Contenido;
        desplazamientoTemasCurso.TabIndex = 1;
        desplazamientoTemasCurso.BackColor = Color.FromArgb(18, 14, 27);
        desplazamientoTemasCurso.Padding = Padding.Empty;
        desplazamientoTemasCurso.MostrarBordeFoco = false;

        desplazamientoCursoPrincipal = new PanelDesplazableSinBarras {
            Name = "desplazamientoCursoPrincipal",
            BackColor = Color.FromArgb(18, 14, 27),
            ColorFondoContenido = Color.FromArgb(18, 14, 27),
            Location = Point.Empty,
            Padding = Padding.Empty,
            Size = panelCursoVista.ClientSize,
            TabIndex = 0,
            TabStop = false,
            Visible = false
        };
        desplazamientoCursoPrincipal.MostrarBordeFoco = false;
        desplazamientoCursoPrincipal.Contenido.Name = "contenidoCursoPrincipal";

        panelCursoVista.Controls.Add(desplazamientoCursoPrincipal);
        panelCursoVista.Controls.Add(panelPerfilCurso);
        panelCursoVista.Controls.Add(lblTituloTemasCurso);
        panelCursoVista.Controls.Add(desplazamientoTemasCurso);
    }

    private void ConstruirVistaPracticasTema() {
        btnVolverTemasCurso = CrearBotonSecundarioCurso(
            "← Volver a temas",
            new Point(18, 14),
            new Size(182, 32));
        btnVolverTemasCurso.TabIndex = 0;
        btnVolverTemasCurso.Click += (_, _) => MostrarCursoPrincipal();

        lblNumeroTemaCurso = CrearLabelCurso(
            "Tema 01",
            new Point(20, 58),
            new Size(150, 24),
            TamanoFuenteCurso.NumeroCabecera,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        lblNombreTemaCurso = CrearLabelCurso(
            "Variables",
            new Point(20, 79),
            new Size(850, 47),
            TamanoFuenteCurso.TituloPrincipal,
            FontStyle.Bold,
            Color.White);
        lblDescripcionTemaCurso = CrearLabelCurso(
            string.Empty,
            new Point(20, 125),
            new Size(850, 34),
            TamanoFuenteCurso.DescripcionCabecera,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        desplazamientoPracticasTema = CrearPanelDesplazableCurso(
            "desplazamientoPracticasTema",
            "listaPracticasTema",
            new Point(16, 163),
            new Size(860, 289),
            new Padding(4, 2, 4, 4));
        listaPracticasTema = desplazamientoPracticasTema.Contenido;

        desplazamientoPracticasTema.TabIndex = 1;
        panelPracticasTemaVista.Controls.Add(btnVolverTemasCurso);
        panelPracticasTemaVista.Controls.Add(lblNumeroTemaCurso);
        panelPracticasTemaVista.Controls.Add(lblNombreTemaCurso);
        panelPracticasTemaVista.Controls.Add(lblDescripcionTemaCurso);
        panelPracticasTemaVista.Controls.Add(desplazamientoPracticasTema);
    }

    private void ConstruirVistaDetallePractica() {
        btnVolverPracticasCurso = CrearBotonSecundarioCurso(
            "← Volver a prácticas",
            new Point(18, 14),
            new Size(202, 32));
        btnVolverPracticasCurso.TabIndex = 0;
        btnVolverPracticasCurso.Click += (_, _) => {
            if (temaCursoSeleccionado is not null) {
                MostrarPracticasTema(temaCursoSeleccionado);
            } else {
                MostrarCursoPrincipal();
            }
        };

        desplazamientoDetallePractica = CrearPanelDesplazableCurso(
            "desplazamientoDetallePractica",
            "contenidoDetallePractica",
            new Point(16, 54),
            new Size(860, 398),
            new Padding(12, 10, 12, 18));
        contenidoDetallePractica = desplazamientoDetallePractica.Contenido;

        desplazamientoDetallePractica.TabIndex = 1;
        panelDetallePracticaVista.Controls.Add(btnVolverPracticasCurso);
        panelDetallePracticaVista.Controls.Add(desplazamientoDetallePractica);
    }

    private async void PanelCurso_Click(object? sender, EventArgs e) {
        await PrepararCursoParaInteraccionAsync();

        if (!cursoPreparado || IsDisposed || Disposing) {
            return;
        }

        SeleccionarPanelMenu(panelCurso);

        panelInicioVista.Visible = false;
        panelRecientesVista.Visible = false;
        panelConfiguracionVista.Visible = false;
        panelVistaNuevaPractica.Visible = false;

        MostrarCursoPrincipal();
    }

    private void MostrarCursoPrincipal() {
        if (!cursoPreparado) {
            return;
        }

        MostrarNavegacionPrincipal(DistribucionPanelPrincipal.Curso);
        ActualizarResumenCurso();

        if (!tarjetasTemasActualizadas) {
            ReconstruirTarjetasTemas();
        }

        MostrarSubvistaCurso(panelCursoVista);
    }

    private void MostrarPracticasTema(TemaCurso tema) {
        if (tema.EsProximamente) {
            MessageBox.Show(
                tema.MensajeDisponibilidad,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (tema.Practicas.Count == 0) {
            MessageBox.Show(
                "Este tema todavía no tiene prácticas disponibles.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        temaCursoSeleccionado = tema;
        MostrarModoCursoInmersivo();
        lblNumeroTemaCurso.Text = $"Tema {tema.Numero:00}";
        lblNombreTemaCurso.Text = tema.Nombre;
        lblDescripcionTemaCurso.Text = tema.Descripcion;
        ReconstruirTarjetasPracticas(tema);
        MostrarSubvistaCurso(panelPracticasTemaVista);
    }

    private void MostrarDetallePractica(PracticaCurso practica) {
        practicaCursoSeleccionada = practica;
        temaCursoSeleccionado = cursoService.ObtenerTema(practica.TemaId);
        MostrarModoCursoInmersivo();
        ReconstruirDetallePractica(practica);
        MostrarSubvistaCurso(panelDetallePracticaVista);
    }

    private void FrmPrincipal_CursoKeyDown(object? sender, KeyEventArgs e) {
        if (!entradaAplicacionRealizada || e.KeyCode != Keys.Escape) {
            return;
        }

        if (panelDetallePracticaVista.Visible) {
            if (temaCursoSeleccionado is not null) {
                MostrarPracticasTema(temaCursoSeleccionado);
            } else {
                MostrarCursoPrincipal();
            }
        } else if (panelPracticasTemaVista.Visible) {
            MostrarCursoPrincipal();
        } else {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void MostrarNavegacionPrincipal(
        DistribucionPanelPrincipal distribucion = DistribucionPanelPrincipal.Normal) {
        timerRecalcularVista.Stop();
        modoCursoInmersivo = false;
        distribucionPanelPrincipal = distribucion;
        panelMenu.Visible = true;
        panelPrincipal.Visible = true;

        if (transicionandoDesdeBienvenida) {
            recalculoPendienteDuranteTransicion = true;
        } else {
            RecalcularDistribucionActual();
        }

        panelMenu.BringToFront();
        panelBarraTitulo.BringToFront();

        if (!transicionandoDesdeBienvenida) {
            InvalidarFondoContinuo();
        }
    }

    private void MostrarModoCursoInmersivo() {
        timerRecalcularVista.Stop();
        modoCursoInmersivo = true;
        distribucionPanelPrincipal = DistribucionPanelPrincipal.Curso;
        panelMenu.Visible = false;
        panelPrincipal.Visible = true;
        panelBarraTitulo.BringToFront();
        RecalcularDistribucionActual();
        InvalidarFondoContinuo();
    }

    private void MostrarSubvistaCurso(Panel vista) {
        panelCursoVista.Visible = ReferenceEquals(vista, panelCursoVista);
        panelPracticasTemaVista.Visible = ReferenceEquals(vista, panelPracticasTemaVista);
        panelDetallePracticaVista.Visible = ReferenceEquals(vista, panelDetallePracticaVista);
        vista.BringToFront();
    }

    private void OcultarVistasCurso() {
        if (!cursoInicializado) {
            return;
        }

        panelCursoVista.Visible = false;
        panelPracticasTemaVista.Visible = false;
        panelDetallePracticaVista.Visible = false;
    }

    private void RecalcularDistribucionCurso() {
        if (!cursoInicializado ||
            desplazamientoTemasCurso is null ||
            desplazamientoPracticasTema is null ||
            desplazamientoDetallePractica is null ||
            panelPerfilCurso is null ||
            lblTituloTemasCurso is null) {
            return;
        }

        int ancho = Math.Max(1, panelPrincipal.ClientSize.Width);
        int alto = Math.Max(1, panelPrincipal.ClientSize.Height);

        if (!modoCursoInmersivo &&
            (distribucionPanelPrincipal == DistribucionPanelPrincipal.Curso ||
             panelCursoVista.Visible)) {
            RecalcularVistaPrincipalCurso();
            ActualizarGeometriaTarjetasTemas(volverAlInicio: false);
        }

        int margenVistaInmersiva = EscalarDiseno(16);
        int anchoVistaInmersiva = Math.Max(1, ancho - margenVistaInmersiva * 2);
        int altoBotonVolver = EscalarDiseno(32);
        btnVolverTemasCurso.SetBounds(
            EscalarDiseno(18),
            EscalarDiseno(14),
            EscalarDiseno(182),
            altoBotonVolver);
        int yNumeroTema = btnVolverTemasCurso.Bottom + EscalarDiseno(12);
        int altoNumeroTema = CalcularAltoTextoCurso(
            lblNumeroTemaCurso,
            anchoVistaInmersiva,
            24);
        lblNumeroTemaCurso.SetBounds(
            EscalarDiseno(20),
            yNumeroTema,
            anchoVistaInmersiva,
            altoNumeroTema);
        int yNombreTema = lblNumeroTemaCurso.Bottom - EscalarDiseno(3);
        int altoNombreTema = CalcularAltoTextoCurso(
            lblNombreTemaCurso,
            anchoVistaInmersiva,
            47);
        lblNombreTemaCurso.SetBounds(
            EscalarDiseno(20),
            yNombreTema,
            anchoVistaInmersiva,
            altoNombreTema);
        int yDescripcionTema = lblNombreTemaCurso.Bottom - EscalarDiseno(1);
        int altoDescripcionTema = CalcularAltoTextoCurso(
            lblDescripcionTemaCurso,
            anchoVistaInmersiva,
            34);
        lblDescripcionTemaCurso.SetBounds(
            EscalarDiseno(20),
            yDescripcionTema,
            anchoVistaInmersiva,
            altoDescripcionTema);
        int yListaPracticas = lblDescripcionTemaCurso.Bottom + EscalarDiseno(4);
        desplazamientoPracticasTema.SetBounds(
            margenVistaInmersiva,
            yListaPracticas,
            anchoVistaInmersiva,
            Math.Max(1, alto - yListaPracticas - EscalarDiseno(18)));

        btnVolverPracticasCurso.SetBounds(
            EscalarDiseno(18),
            EscalarDiseno(14),
            EscalarDiseno(202),
            altoBotonVolver);
        int yDetalle = btnVolverPracticasCurso.Bottom + EscalarDiseno(8);
        desplazamientoDetallePractica.SetBounds(
            margenVistaInmersiva,
            yDetalle,
            anchoVistaInmersiva,
            Math.Max(1, alto - yDetalle - EscalarDiseno(18)));

        if (panelPracticasTemaVista.Visible) {
            desplazamientoPracticasTema.ActualizarContenido(volverAlInicio: false);
        }

        if (panelDetallePracticaVista.Visible) {
            desplazamientoDetallePractica.ActualizarContenido(volverAlInicio: false);
        }
    }

    private void RecalcularVistaPrincipalCurso() {
        int anchoVista = Math.Max(1, panelCursoVista.ClientSize.Width);
        int altoVista = Math.Max(1, panelCursoVista.ClientSize.Height);
        int margenHorizontal = EscalarDiseno(18);
        int margenVertical = EscalarDiseno(12);
        int anchoDisponible = Math.Max(1, anchoVista - margenHorizontal * 2);
        int altoDisponible = Math.Max(1, altoVista - margenVertical * 2);
        int separacionColumnas = EscalarDiseno(24);
        int anchoMinimoPerfil = EscalarDiseno(300);
        int anchoMinimoTemas = EscalarDiseno(620);
        int anchoMinimoAmplio = anchoMinimoPerfil + separacionColumnas + anchoMinimoTemas;
        bool usarModoCompacto =
            anchoDisponible < anchoMinimoAmplio ||
            altoDisponible < EscalarDiseno(470);

        EstablecerJerarquiaCursoPrincipal(usarModoCompacto);

        if (usarModoCompacto) {
            RecalcularVistaPrincipalCursoCompacta(anchoVista, altoVista);
            return;
        }

        int anchoBloque = Math.Min(EscalarDiseno(1280), anchoDisponible);
        int altoBloque = Math.Min(EscalarDiseno(720), altoDisponible);
        int anchoPerfil = Math.Clamp(
            (int)Math.Round(anchoBloque * 0.28D),
            anchoMinimoPerfil,
            EscalarDiseno(360));
        int anchoTemas = Math.Max(1, anchoBloque - anchoPerfil - separacionColumnas);
        int xBloque = Math.Max(0, (anchoVista - anchoBloque) / 2);
        int yBloque = Math.Max(0, (altoVista - altoBloque) / 2);
        int altoTitulo = Math.Min(
            altoBloque,
            CalcularAltoTextoCurso(lblTituloTemasCurso, anchoTemas, 30));
        int separacionTitulo = Math.Min(
            EscalarDiseno(8),
            Math.Max(0, altoBloque - altoTitulo - 1));
        int altoLista = Math.Max(1, altoBloque - altoTitulo - separacionTitulo);
        int xTemas = xBloque + anchoPerfil + separacionColumnas;

        panelPerfilCurso.SetBounds(xBloque, yBloque, anchoPerfil, altoBloque);
        lblTituloTemasCurso.SetBounds(xTemas, yBloque, anchoTemas, altoTitulo);
        desplazamientoTemasCurso.SetBounds(
            xTemas,
            yBloque + altoTitulo + separacionTitulo,
            anchoTemas,
            altoLista);
        AjustarContenidoPerfilCurso();
    }

    private void RecalcularVistaPrincipalCursoCompacta(int anchoVista, int altoVista) {
        desplazamientoCursoPrincipal.SetBounds(0, 0, anchoVista, altoVista);
        FlowLayoutPanel contenido = desplazamientoCursoPrincipal.Contenido;
        contenido.SuspendLayout();
        listaTemasCurso.SuspendLayout();

        try {
            contenido.Padding = new Padding(
                EscalarDiseno(18),
                EscalarDiseno(12),
                EscalarDiseno(18),
                EscalarDiseno(22));

            int anchoContenido = Math.Max(
                1,
                contenido.ClientSize.Width - contenido.Padding.Horizontal);
            panelPerfilCurso.Width = anchoContenido;
            int altoPerfil = AjustarContenidoPerfilCurso();
            panelPerfilCurso.Size = new Size(anchoContenido, altoPerfil);

            int altoTitulo = CalcularAltoTextoCurso(lblTituloTemasCurso, anchoContenido, 30);
            lblTituloTemasCurso.Size = new Size(anchoContenido, altoTitulo);

            int altoTemas = CalcularAltoListaTemasCompacta();
            listaTemasCurso.Size = new Size(anchoContenido, altoTemas);
        } finally {
            listaTemasCurso.ResumeLayout(performLayout: false);
            contenido.ResumeLayout(performLayout: false);
        }
    }

    private void EstablecerJerarquiaCursoPrincipal(bool usarModoCompacto) {
        if (jerarquiaCursoPrincipalInicializada && cursoModoCompacto == usarModoCompacto) {
            return;
        }

        FlowLayoutPanel contenidoCompacto = desplazamientoCursoPrincipal.Contenido;
        bool propietarioTeniaFocoDirecto =
            desplazamientoCursoPrincipal.Focused ||
            desplazamientoTemasCurso.Focused;
        Control? controlContenidoEnfocado = ObtenerControlEnfocado(listaTemasCurso);
        panelCursoVista.SuspendLayout();
        contenidoCompacto.SuspendLayout();

        try {
            cursoModoCompacto = usarModoCompacto;
            desplazamientoCursoPrincipal.CancelarInteraccion();
            desplazamientoTemasCurso.CancelarInteraccion();

            if (usarModoCompacto) {
                MoverControlCurso(panelPerfilCurso, contenidoCompacto);
                MoverControlCurso(lblTituloTemasCurso, contenidoCompacto);
                desplazamientoTemasCurso.TransferirContenidoA(contenidoCompacto);
                panelPerfilCurso.Margin = new Padding(0, 0, 0, EscalarDiseno(18));
                lblTituloTemasCurso.Margin = new Padding(0, 0, 0, EscalarDiseno(8));
                listaTemasCurso.Margin = Padding.Empty;
                desplazamientoTemasCurso.Visible = false;
                desplazamientoCursoPrincipal.TabStop = true;
                desplazamientoCursoPrincipal.Visible = true;
                desplazamientoCursoPrincipal.BringToFront();
            } else {
                desplazamientoCursoPrincipal.Visible = false;
                desplazamientoCursoPrincipal.TabStop = false;
                desplazamientoTemasCurso.RestaurarContenido();
                desplazamientoTemasCurso.Visible = true;
                desplazamientoTemasCurso.TabStop = true;
                MoverControlCurso(panelPerfilCurso, panelCursoVista);
                MoverControlCurso(lblTituloTemasCurso, panelCursoVista);
                MoverControlCurso(desplazamientoTemasCurso, panelCursoVista);
                panelPerfilCurso.Margin = Padding.Empty;
                lblTituloTemasCurso.Margin = Padding.Empty;
                listaTemasCurso.Margin = Padding.Empty;
                desplazamientoTemasCurso.Margin = Padding.Empty;
            }
        } finally {
            jerarquiaCursoPrincipalInicializada = true;
            contenidoCompacto.ResumeLayout(performLayout: false);
            panelCursoVista.ResumeLayout(performLayout: false);
        }

        if (controlContenidoEnfocado is { IsDisposed: false, Visible: true } &&
            controlContenidoEnfocado.CanFocus) {
            controlContenidoEnfocado.Focus();
        } else if (propietarioTeniaFocoDirecto) {
            PanelDesplazableSinBarras propietarioActual = usarModoCompacto
                ? desplazamientoCursoPrincipal
                : desplazamientoTemasCurso;

            if (propietarioActual.CanFocus) {
                propietarioActual.Focus();
            }
        }
    }

    private static Control? ObtenerControlEnfocado(Control contenedor) {
        if (contenedor.Focused) {
            return contenedor;
        }

        foreach (Control control in contenedor.Controls) {
            if (control.ContainsFocus) {
                return ObtenerControlEnfocado(control);
            }
        }

        return null;
    }

    private static void MoverControlCurso(Control control, Control nuevoParent) {
        if (!ReferenceEquals(control.Parent, nuevoParent)) {
            nuevoParent.Controls.Add(control);
        }
    }

    private int CalcularAltoListaTemasCompacta() {
        int altoContenido = listaTemasCurso.Padding.Vertical;

        foreach (Control control in listaTemasCurso.Controls) {
            if (control.Visible) {
                altoContenido += control.Height + control.Margin.Vertical;
            }
        }

        return Math.Max(
            EscalarDiseno(160),
            altoContenido);
    }

    private void ActualizarAlturaTemasCursoCompacto(bool volverAlInicio) {
        if (!cursoModoCompacto ||
            !ReferenceEquals(listaTemasCurso.Parent, desplazamientoCursoPrincipal.Contenido)) {
            return;
        }

        listaTemasCurso.Height = CalcularAltoListaTemasCompacta();
        desplazamientoCursoPrincipal.ActualizarContenido(volverAlInicio);
    }

    private int AjustarContenidoPerfilCurso() {
        int anchoPerfil = Math.Max(1, panelPerfilCurso.ClientSize.Width);
        int margenHorizontal = Math.Min(EscalarDiseno(26), Math.Max(EscalarDiseno(16), anchoPerfil / 13));
        int anchoContenido = Math.Max(1, anchoPerfil - margenHorizontal * 2);
        int y = EscalarDiseno(16);

        int altoTitulo = CalcularAltoTextoCurso(lblTituloPerfilCurso, anchoContenido, 34);
        lblTituloPerfilCurso.SetBounds(margenHorizontal, y, anchoContenido, altoTitulo);
        y = lblTituloPerfilCurso.Bottom + EscalarDiseno(2);

        int altoNombre = CalcularAltoTextoCurso(lblNombrePerfilCurso, anchoContenido, 24);
        lblNombrePerfilCurso.SetBounds(margenHorizontal, y, anchoContenido, altoNombre);
        y = lblNombrePerfilCurso.Bottom;

        int altoNivel = CalcularAltoTextoCurso(lblNivelPerfilCurso, anchoContenido, 22);
        lblNivelPerfilCurso.SetBounds(margenHorizontal, y, anchoContenido, altoNivel);
        y = lblNivelPerfilCurso.Bottom + EscalarDiseno(6);

        int altoProgreso = CalcularAltoTextoCurso(lblProgresoGeneralCurso, anchoContenido, 42);
        lblProgresoGeneralCurso.SetBounds(margenHorizontal, y, anchoContenido, altoProgreso);
        y = lblProgresoGeneralCurso.Bottom + EscalarDiseno(7);

        int altoBarra = EscalarDiseno(9);
        panelFondoProgresoCurso.SetBounds(margenHorizontal, y, anchoContenido, altoBarra);
        panelRellenoProgresoCurso.Height = altoBarra;
        y = panelFondoProgresoCurso.Bottom + EscalarDiseno(11);

        int altoSiguientePaso = CalcularAltoTextoCurso(lblSiguientePasoCurso, anchoContenido, 18);
        lblSiguientePasoCurso.SetBounds(margenHorizontal, y, anchoContenido, altoSiguientePaso);
        y = lblSiguientePasoCurso.Bottom + EscalarDiseno(4);

        int altoUltimaPractica = CalcularAltoTextoCurso(lblUltimaPracticaCurso, anchoContenido, 50);
        lblUltimaPracticaCurso.SetBounds(margenHorizontal, y, anchoContenido, altoUltimaPractica);
        y = lblUltimaPracticaCurso.Bottom + EscalarDiseno(6);

        int altoBoton = EscalarDiseno(34);
        btnContinuarAprendizaje.SetBounds(margenHorizontal, y, anchoContenido, altoBoton);
        y = btnContinuarAprendizaje.Bottom + EscalarDiseno(10);

        int altoTituloRecomendacion = CalcularAltoTextoCurso(
            lblTituloRecomendacionCurso,
            anchoContenido,
            18);
        lblTituloRecomendacionCurso.SetBounds(
            margenHorizontal,
            y,
            anchoContenido,
            altoTituloRecomendacion);
        y = lblTituloRecomendacionCurso.Bottom + EscalarDiseno(4);

        int altoRecomendacion = CalcularAltoTextoCurso(
            lblPracticaRecomendadaCurso,
            anchoContenido,
            50);
        lblPracticaRecomendadaCurso.SetBounds(
            margenHorizontal,
            y,
            anchoContenido,
            altoRecomendacion);
        y = lblPracticaRecomendadaCurso.Bottom + EscalarDiseno(6);

        btnVerPracticaRecomendadaCurso.SetBounds(margenHorizontal, y, anchoContenido, altoBoton);
        y = btnVerPracticaRecomendadaCurso.Bottom + EscalarDiseno(5);

        int altoAviso = CalcularAltoTextoCurso(lblAvisoProgresoCurso, anchoContenido, 29);
        lblAvisoProgresoCurso.SetBounds(margenHorizontal, y, anchoContenido, altoAviso);

        ActualizarAnchoProgresoGeneral(ContarPracticasDisponiblesRealizadas());
        return lblAvisoProgresoCurso.Bottom + EscalarDiseno(16);
    }

    private int CalcularAltoTextoCurso(Label label, int ancho, int altoMinimo) {
        int altoTexto = TextRenderer.MeasureText(
            label.Text,
            label.Font,
            new Size(Math.Max(1, ancho), int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
        return Math.Max(EscalarDiseno(altoMinimo), altoTexto + EscalarDiseno(4));
    }

    private void CargarProgresoCurso() {
        CargarProgresoCurso(mostrarAvisoInmediato: false);
    }

    private void CargarProgresoCurso(
        bool mostrarAvisoInmediato,
        ResultadoCargaProgreso? resultadoPrecargado = null) {
        ResultadoCargaProgreso resultado =
            resultadoPrecargado ?? progresoCursoService.CargarProgreso();

        if (resultado.DatosDisponibles) {
            progresoCurso = resultado.Progreso;
        }

        tarjetasTemasActualizadas = false;

        mensajeEstadoProgresoCurso = resultado.Estado switch {
            EstadoCargaProgreso.ContenidoInvalido when resultado.Progreso.Practicas.Count > 0 =>
                $"Se ignoraron {resultado.RegistrosInvalidos} registros dañados; el resto del progreso se conservó.",
            EstadoCargaProgreso.ContenidoInvalido =>
                "progreso.json está dañado. No se modificará su contenido.",
            EstadoCargaProgreso.PermisosInsuficientes =>
                "Sin permisos para leer progreso.json. EndForge continuará de forma segura.",
            EstadoCargaProgreso.ErrorIo =>
                "progreso.json está bloqueado o no está disponible.",
            _ => string.Empty
        };

        if (mostrarAvisoInmediato && !string.IsNullOrWhiteSpace(mensajeEstadoProgresoCurso)) {
            MessageBox.Show(
                mensajeEstadoProgresoCurso,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        if (panelPerfilCurso is not null) {
            ActualizarResumenCurso();
        }
    }

    private void ActualizarResumenCurso() {
        int realizadas = ContarPracticasDisponiblesRealizadas();
        int totalDisponibles = cursoService.TotalPracticasDisponibles;

        lblProgresoGeneralCurso.Text =
            $"Progreso general\n{realizadas} de {totalDisponibles} prácticas disponibles";
        ActualizarAnchoProgresoGeneral(realizadas);

        ProgresoPractica? ultimaEnProgreso = progresoCurso.Practicas
            .Where(progreso =>
                progreso.Estado == EstadoPracticaCurso.EnProgreso &&
                cursoService.ObtenerPractica(progreso.PracticaId) is not null)
            .OrderByDescending(ObtenerFechaActividad)
            .ThenBy(progreso => progreso.PracticaId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        if (ultimaEnProgreso is null) {
            lblUltimaPracticaCurso.Text = realizadas > 0
                ? $"No hay práctica en progreso.\n{realizadas} práctica{(realizadas == 1 ? string.Empty : "s")} completada{(realizadas == 1 ? string.Empty : "s")}."
                : "No hay práctica en progreso.";
            btnContinuarAprendizaje.Visible = false;
            btnContinuarAprendizaje.Tag = null;
        } else {
            PracticaCurso practica = cursoService.ObtenerPractica(ultimaEnProgreso.PracticaId)!;
            TemaCurso? tema = cursoService.ObtenerTema(practica.TemaId);
            lblUltimaPracticaCurso.Text =
                $"Última: {practica.Nombre}\n" +
                $"Tema: {tema?.Nombre ?? "Curso"}\n" +
                $"Estado: {ObtenerTextoEstadoPractica(ultimaEnProgreso.Estado)}";
            btnContinuarAprendizaje.Visible = true;
            btnContinuarAprendizaje.Tag = practica;
        }

        ActualizarPracticaRecomendada();
        lblAvisoProgresoCurso.Text = mensajeEstadoProgresoCurso;
        lblAvisoProgresoCurso.Visible = !string.IsNullOrWhiteSpace(mensajeEstadoProgresoCurso);
        AjustarContenidoPerfilCurso();
    }

    private int ContarPracticasDisponiblesRealizadas() {
        HashSet<string> identificadoresDisponibles = cursoService.CargarTemas()
            .Where(tema => !tema.EsProximamente)
            .SelectMany(tema => tema.Practicas)
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return progresoCurso.Practicas
            .Where(progreso =>
                progreso.Estado == EstadoPracticaCurso.Realizada &&
                identificadoresDisponibles.Contains(progreso.PracticaId))
            .Select(progreso => progreso.PracticaId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private void ActualizarAnchoProgresoGeneral(int realizadas) {
        int totalDisponibles = cursoService.TotalPracticasDisponibles;
        int anchoDisponible = Math.Max(0, panelFondoProgresoCurso.ClientSize.Width);
        int anchoBarra = totalDisponibles == 0
            ? 0
            : (int)Math.Round(anchoDisponible * realizadas / (double)totalDisponibles);
        panelRellenoProgresoCurso.Width = Math.Clamp(anchoBarra, 0, anchoDisponible);
    }

    private void BtnContinuarAprendizaje_Click(object? sender, EventArgs e) {
        if (btnContinuarAprendizaje.Tag is PracticaCurso practica) {
            ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);

            if (string.IsNullOrWhiteSpace(progreso?.RutaProyecto) ||
                !Directory.Exists(progreso.RutaProyecto)) {
                MessageBox.Show(
                    "La carpeta guardada para esta práctica ya no existe o no está disponible. " +
                    "Puedes revisar el detalle sin perder tu progreso.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            MostrarDetallePractica(practica);
        }
    }

    private void BtnVerPracticaRecomendadaCurso_Click(object? sender, EventArgs e) {
        if (btnVerPracticaRecomendadaCurso.Tag is PracticaCurso practica) {
            MostrarDetallePractica(practica);
        }
    }

    private void ActualizarPracticaRecomendada() {
        (TemaCurso Tema, PracticaCurso Practica)? recomendacion = ObtenerPracticaRecomendada();

        if (recomendacion is null) {
            bool hayContenido = cursoService.CargarTemas().Any(tema =>
                !tema.EsProximamente && tema.Practicas.Count > 0);
            lblPracticaRecomendadaCurso.Text = hayContenido
                ? "Curso completado"
                : "No hay una práctica recomendada disponible.";
            btnVerPracticaRecomendadaCurso.Visible = false;
            btnVerPracticaRecomendadaCurso.Tag = null;
            return;
        }

        TemaCurso tema = recomendacion.Value.Tema;
        PracticaCurso practica = recomendacion.Value.Practica;
        lblPracticaRecomendadaCurso.Text =
            $"Tema {tema.Numero:00} · Práctica {practica.Numero:00}\n" +
            $"{practica.Nombre}\n" +
            $"{NormalizarDificultad(practica.Dificultad)} · {practica.DuracionEstimada}";
        btnVerPracticaRecomendadaCurso.Visible = true;
        btnVerPracticaRecomendadaCurso.Tag = practica;
    }

    private (TemaCurso Tema, PracticaCurso Practica)? ObtenerPracticaRecomendada() {
        TemaCurso[] temasDisponibles = cursoService.CargarTemas()
            .Where(tema => !tema.EsProximamente && tema.Practicas.Count > 0)
            .OrderBy(tema => tema.Numero)
            .ToArray();

        if (temasDisponibles.Length == 0) {
            return null;
        }

        bool cursoCompletado = temasDisponibles
            .SelectMany(tema => tema.Practicas)
            .All(practica => ObtenerEstadoPractica(practica.Id) == EstadoPracticaCurso.Realizada);

        if (cursoCompletado) {
            return null;
        }

        TemaCurso? temaAvanzadoConPendientes = temasDisponibles
            .Where(TemaTieneProgreso)
            .Where(tema => tema.Practicas.Any(practica =>
                ObtenerEstadoPractica(practica.Id) == EstadoPracticaCurso.Pendiente))
            .OrderByDescending(tema => tema.Numero)
            .FirstOrDefault();

        if (temaAvanzadoConPendientes is not null) {
            PracticaCurso pendiente = temaAvanzadoConPendientes.Practicas
                .OrderBy(practica => practica.Numero)
                .First(practica =>
                    ObtenerEstadoPractica(practica.Id) == EstadoPracticaCurso.Pendiente);
            return (temaAvanzadoConPendientes, pendiente);
        }

        TemaCurso? ultimoTemaIniciado = temasDisponibles
            .Where(TemaTieneProgreso)
            .OrderByDescending(tema => tema.Numero)
            .FirstOrDefault();

        if (ultimoTemaIniciado is null) {
            return (temasDisponibles[0], temasDisponibles[0].Practicas.OrderBy(
                practica => practica.Numero).First());
        }

        TemaCurso? siguienteTema = temasDisponibles
            .FirstOrDefault(tema =>
                tema.Numero > ultimoTemaIniciado.Numero &&
                tema.Practicas.Any(practica =>
                    ObtenerEstadoPractica(practica.Id) != EstadoPracticaCurso.Realizada));

        if (siguienteTema is not null) {
            PracticaCurso siguiente = siguienteTema.Practicas
                .OrderBy(practica => practica.Numero)
                .First(practica =>
                    ObtenerEstadoPractica(practica.Id) != EstadoPracticaCurso.Realizada);
            return (siguienteTema, siguiente);
        }

        foreach (TemaCurso tema in temasDisponibles.OrderByDescending(tema => tema.Numero)) {
            PracticaCurso? pendiente = tema.Practicas
                .OrderBy(practica => practica.Numero)
                .FirstOrDefault(practica =>
                    ObtenerEstadoPractica(practica.Id) != EstadoPracticaCurso.Realizada);

            if (pendiente is not null) {
                return (tema, pendiente);
            }
        }

        return null;
    }

    private bool TemaTieneProgreso(TemaCurso tema) {
        HashSet<string> ids = tema.Practicas
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return progresoCurso.Practicas.Any(progreso =>
            ids.Contains(progreso.PracticaId) &&
            (progreso.Estado != EstadoPracticaCurso.Pendiente ||
             !string.IsNullOrWhiteSpace(progreso.RutaProyecto)));
    }

    private static DateTimeOffset ObtenerFechaActividad(ProgresoPractica progreso) {
        return progreso.FechaActualizacion ??
            progreso.FechaFinalizacion ??
            progreso.FechaCreacion;
    }

    private void ReconstruirTarjetasTemas(bool volverAlInicio = true) {
        listaTemasCurso.SuspendLayout();

        try {
            VaciarYDisponerControles(listaTemasCurso);

            int anchoTarjeta = Math.Max(
                1,
                listaTemasCurso.ClientSize.Width -
                listaTemasCurso.Padding.Horizontal -
                EscalarDiseno(4));
            int margenIzquierdo = EscalarDiseno(18);
            int margenDerecho = EscalarDiseno(18);
            int anchoNumero = EscalarDiseno(42);
            int xTexto = margenIzquierdo + anchoNumero + EscalarDiseno(4);
            int separacionColumnas = EscalarDiseno(16);
            int anchoInterior = Math.Max(1, anchoTarjeta - margenIzquierdo - margenDerecho);
            int anchoMaximoEstadisticas = Math.Max(
                1,
                anchoTarjeta - xTexto - separacionColumnas - margenDerecho);
            int anchoMinimoEstadisticas = Math.Min(EscalarDiseno(150), anchoMaximoEstadisticas);
            int limiteEstadisticas = Math.Min(EscalarDiseno(190), anchoMaximoEstadisticas);
            int anchoEstadisticas = Math.Clamp(
                (int)Math.Round(anchoTarjeta * 0.28D),
                anchoMinimoEstadisticas,
                limiteEstadisticas);
            int xEstadisticas = anchoTarjeta - margenDerecho - anchoEstadisticas;
            int anchoTextoAmplio = Math.Max(1, xEstadisticas - separacionColumnas - xTexto);
            bool presentacionCompacta =
                cursoModoCompacto ||
                anchoTarjeta < EscalarDiseno(680) ||
                anchoTextoAmplio < EscalarDiseno(260);
            int anchoTitulo = presentacionCompacta
                ? Math.Max(1, anchoTarjeta - margenDerecho - xTexto)
                : anchoTextoAmplio;
            int anchoDescripcion = presentacionCompacta
                ? anchoInterior
                : anchoTextoAmplio;

            foreach (TemaCurso tema in cursoService.CargarTemas()) {
                int realizadas = ContarPracticasRealizadas(tema);
                int porcentaje = (int)Math.Round(
                    realizadas * 100D / Math.Max(1, tema.TotalPracticasPlaneadas));
                EstadoPracticaCurso estadoTema = ObtenerEstadoTema(tema, realizadas);
                Panel tarjeta = CrearTarjetaCurso(
                    Point.Empty,
                    new Size(anchoTarjeta, EscalarDiseno(112)),
                    14,
                    interactiva: true);
                tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(10));
                tarjeta.TabIndex = Math.Max(0, tema.Numero - 1);
                tarjeta.AccessibleName = $"Tema {tema.Numero:00}: {tema.Nombre}";
                tarjeta.AccessibleDescription = tema.EsProximamente
                    ? tema.MensajeDisponibilidad
                    : $"{porcentaje}% completado. {realizadas} de {tema.TotalPracticasPlaneadas} prácticas.";

                Label numero = CrearLabelCurso(
                    tema.Numero.ToString("00"),
                    Point.Empty,
                    new Size(anchoNumero, 1),
                    TamanoFuenteCurso.NumeroTarjeta,
                    FontStyle.Bold,
                    ColorMoradoClaroCurso,
                    ContentAlignment.TopLeft);
                Label nombre = CrearLabelCurso(
                    tema.Nombre,
                    Point.Empty,
                    new Size(anchoTitulo, 1),
                    TamanoFuenteCurso.TituloTarjetaTema,
                    FontStyle.Bold,
                    Color.White,
                    ContentAlignment.TopLeft);
                Label descripcion = CrearLabelCurso(
                    tema.Descripcion,
                    Point.Empty,
                    new Size(anchoDescripcion, 1),
                    TamanoFuenteCurso.DescripcionTarjetaTema,
                    FontStyle.Regular,
                    ColorTextoSecundarioCurso,
                    ContentAlignment.TopLeft);
                Label porcentajeCompletado = CrearLabelCurso(
                    $"{porcentaje}% completado",
                    Point.Empty,
                    new Size(anchoEstadisticas, 1),
                    TamanoFuenteCurso.ProgresoTarjetaTema,
                    FontStyle.Bold,
                    ColorMoradoClaroCurso,
                    ContentAlignment.MiddleRight);
                Label progreso = CrearLabelCurso(
                    $"{realizadas} de {tema.TotalPracticasPlaneadas} prácticas",
                    Point.Empty,
                    new Size(anchoEstadisticas, 1),
                    TamanoFuenteCurso.MetadatoTarjetaTema,
                    FontStyle.Regular,
                    Color.White,
                    ContentAlignment.MiddleRight);
                Label estado = CrearLabelCurso(
                    tema.EsProximamente
                        ? tema.MensajeDisponibilidad
                        : ObtenerTextoEstadoTema(estadoTema),
                    Point.Empty,
                    new Size(anchoEstadisticas, 1),
                    TamanoFuenteCurso.MetadatoTarjetaTema,
                    FontStyle.Bold,
                    tema.EsProximamente
                        ? ColorTextoSecundarioCurso
                        : ObtenerColorEstado(estadoTema),
                    ContentAlignment.MiddleRight);

                int ySuperior = EscalarDiseno(10);
                int altoNumero = CalcularAltoTextoCurso(numero, anchoNumero, 26);
                int altoNombre = CalcularAltoTextoCurso(nombre, anchoTitulo, 28);
                int altoBarra = EscalarDiseno(8);
                numero.SetBounds(margenIzquierdo, ySuperior, anchoNumero, altoNumero);
                nombre.SetBounds(xTexto, ySuperior, anchoTitulo, altoNombre);

                int xBarra;
                int yBarra;
                int anchoBarra;
                int altoTarjeta;

                if (presentacionCompacta) {
                    int altoFilaTitulo = Math.Max(altoNumero, altoNombre);
                    int yDescripcion = ySuperior + altoFilaTitulo + EscalarDiseno(5);
                    int altoDescripcion = CalcularAltoTextoCurso(
                        descripcion,
                        anchoDescripcion,
                        36);
                    descripcion.SetBounds(
                        margenIzquierdo,
                        yDescripcion,
                        anchoDescripcion,
                        altoDescripcion);

                    int separacionEstadisticas = EscalarDiseno(8);
                    int anchoColumnaEstadisticas = Math.Max(
                        1,
                        (anchoInterior - separacionEstadisticas * 2) / 3);
                    int xPorcentaje = margenIzquierdo;
                    int xProgreso = xPorcentaje + anchoColumnaEstadisticas + separacionEstadisticas;
                    int xEstado = xProgreso + anchoColumnaEstadisticas + separacionEstadisticas;
                    int anchoEstado = Math.Max(
                        1,
                        margenIzquierdo + anchoInterior - xEstado);
                    int yEstadisticas = yDescripcion + altoDescripcion + EscalarDiseno(7);
                    int altoPorcentaje = CalcularAltoTextoCurso(
                        porcentajeCompletado,
                        anchoColumnaEstadisticas,
                        21);
                    int altoProgreso = CalcularAltoTextoCurso(
                        progreso,
                        anchoColumnaEstadisticas,
                        21);
                    int altoEstado = CalcularAltoTextoCurso(estado, anchoEstado, 23);
                    int altoFilaEstadisticas = Math.Max(
                        altoPorcentaje,
                        Math.Max(altoProgreso, altoEstado));

                    porcentajeCompletado.TextAlign = ContentAlignment.MiddleLeft;
                    progreso.TextAlign = ContentAlignment.MiddleCenter;
                    estado.TextAlign = ContentAlignment.MiddleRight;
                    porcentajeCompletado.SetBounds(
                        xPorcentaje,
                        yEstadisticas,
                        anchoColumnaEstadisticas,
                        altoFilaEstadisticas);
                    progreso.SetBounds(
                        xProgreso,
                        yEstadisticas,
                        anchoColumnaEstadisticas,
                        altoFilaEstadisticas);
                    estado.SetBounds(
                        xEstado,
                        yEstadisticas,
                        anchoEstado,
                        altoFilaEstadisticas);

                    xBarra = margenIzquierdo;
                    anchoBarra = anchoInterior;
                    yBarra = yEstadisticas + altoFilaEstadisticas + EscalarDiseno(7);
                    altoTarjeta = yBarra + altoBarra + EscalarDiseno(12);
                } else {
                    int yDescripcion = ySuperior + altoNombre + EscalarDiseno(4);
                    int altoDescripcion = CalcularAltoTextoCurso(
                        descripcion,
                        anchoDescripcion,
                        36);
                    int altoPorcentaje = CalcularAltoTextoCurso(
                        porcentajeCompletado,
                        anchoEstadisticas,
                        21);
                    int yProgreso = ySuperior + altoPorcentaje + EscalarDiseno(2);
                    int altoProgreso = CalcularAltoTextoCurso(
                        progreso,
                        anchoEstadisticas,
                        21);
                    int yEstado = yProgreso + altoProgreso + EscalarDiseno(3);
                    int altoEstado = CalcularAltoTextoCurso(estado, anchoEstadisticas, 23);

                    descripcion.SetBounds(
                        xTexto,
                        yDescripcion,
                        anchoDescripcion,
                        altoDescripcion);
                    porcentajeCompletado.SetBounds(
                        xEstadisticas,
                        ySuperior,
                        anchoEstadisticas,
                        altoPorcentaje);
                    progreso.SetBounds(
                        xEstadisticas,
                        yProgreso,
                        anchoEstadisticas,
                        altoProgreso);
                    estado.SetBounds(xEstadisticas, yEstado, anchoEstadisticas, altoEstado);

                    xBarra = xEstadisticas;
                    anchoBarra = anchoEstadisticas;
                    yBarra = yEstado + altoEstado + EscalarDiseno(7);
                    altoTarjeta = Math.Max(
                        yDescripcion + altoDescripcion,
                        yBarra + altoBarra) + EscalarDiseno(12);
                }

                tarjeta.Size = new Size(anchoTarjeta, altoTarjeta);

                Panel fondoProgresoTema = new() {
                    BackColor = Color.FromArgb(55, 45, 70),
                    Location = new Point(xBarra, yBarra),
                    Size = new Size(anchoBarra, altoBarra)
                };
                Panel rellenoProgresoTema = new() {
                    BackColor = ColorMoradoCurso,
                    Location = Point.Empty,
                    Size = new Size(
                        (int)Math.Round(anchoBarra * porcentaje / 100D),
                        altoBarra)
                };
                fondoProgresoTema.Controls.Add(rellenoProgresoTema);

                tarjeta.Controls.Add(numero);
                tarjeta.Controls.Add(nombre);
                tarjeta.Controls.Add(descripcion);
                tarjeta.Controls.Add(porcentajeCompletado);
                tarjeta.Controls.Add(progreso);
                tarjeta.Controls.Add(estado);
                tarjeta.Controls.Add(fondoProgresoTema);
                tarjeta.Tag = new PresentacionTarjetaTema(
                    numero,
                    nombre,
                    descripcion,
                    porcentajeCompletado,
                    progreso,
                    estado,
                    fondoProgresoTema,
                    rellenoProgresoTema,
                    porcentaje);
                ConfigurarInteraccionTarjeta(tarjeta, () => MostrarPracticasTema(tema));
                listaTemasCurso.Controls.Add(tarjeta);
            }
        } finally {
            listaTemasCurso.ResumeLayout(performLayout: false);
        }

        ultimoAnchoTarjetasTemas = Math.Max(
            1,
            listaTemasCurso.ClientSize.Width -
            listaTemasCurso.Padding.Horizontal -
            EscalarDiseno(4));
        ultimoDpiTarjetasTemas = DeviceDpi;
        ultimoModoCompactoTarjetasTemas = cursoModoCompacto;

        if (cursoModoCompacto) {
            listaTemasCurso.PerformLayout();
            ActualizarAlturaTemasCursoCompacto(volverAlInicio);
        } else {
            desplazamientoTemasCurso.ActualizarContenido(volverAlInicio);
        }

        tarjetasTemasActualizadas = true;
    }

    private void ActualizarGeometriaTarjetasTemas(bool volverAlInicio) {
        if (listaTemasCurso is null || listaTemasCurso.IsDisposed) {
            return;
        }

        int anchoTarjeta = Math.Max(
            1,
            listaTemasCurso.ClientSize.Width -
            listaTemasCurso.Padding.Horizontal -
            EscalarDiseno(4));
        bool geometriaCambio =
            anchoTarjeta != ultimoAnchoTarjetasTemas ||
            DeviceDpi != ultimoDpiTarjetasTemas ||
            cursoModoCompacto != ultimoModoCompactoTarjetasTemas;

        if (geometriaCambio) {
            listaTemasCurso.SuspendLayout();

            try {
                foreach (Panel tarjeta in listaTemasCurso.Controls.OfType<Panel>()) {
                    if (tarjeta.Tag is PresentacionTarjetaTema presentacion) {
                        AplicarGeometriaTarjetaTema(tarjeta, presentacion, anchoTarjeta);
                    }
                }
            } finally {
                listaTemasCurso.ResumeLayout(performLayout: false);
            }

            ultimoAnchoTarjetasTemas = anchoTarjeta;
            ultimoDpiTarjetasTemas = DeviceDpi;
            ultimoModoCompactoTarjetasTemas = cursoModoCompacto;
        }

        if (cursoModoCompacto) {
            if (geometriaCambio) {
                listaTemasCurso.PerformLayout();
            }

            ActualizarAlturaTemasCursoCompacto(volverAlInicio);
        } else {
            desplazamientoTemasCurso.ActualizarContenido(volverAlInicio);
        }
    }

    private void AplicarGeometriaTarjetaTema(
        Panel tarjeta,
        PresentacionTarjetaTema presentacion,
        int anchoTarjeta) {
        int margenIzquierdo = EscalarDiseno(18);
        int margenDerecho = EscalarDiseno(18);
        int anchoNumero = EscalarDiseno(42);
        int xTexto = margenIzquierdo + anchoNumero + EscalarDiseno(4);
        int separacionColumnas = EscalarDiseno(16);
        int anchoInterior = Math.Max(1, anchoTarjeta - margenIzquierdo - margenDerecho);
        int anchoMaximoEstadisticas = Math.Max(
            1,
            anchoTarjeta - xTexto - separacionColumnas - margenDerecho);
        int anchoMinimoEstadisticas = Math.Min(EscalarDiseno(150), anchoMaximoEstadisticas);
        int limiteEstadisticas = Math.Min(EscalarDiseno(190), anchoMaximoEstadisticas);
        int anchoEstadisticas = Math.Clamp(
            (int)Math.Round(anchoTarjeta * 0.28D),
            anchoMinimoEstadisticas,
            limiteEstadisticas);
        int xEstadisticas = anchoTarjeta - margenDerecho - anchoEstadisticas;
        int anchoTextoAmplio = Math.Max(1, xEstadisticas - separacionColumnas - xTexto);
        bool presentacionCompacta =
            cursoModoCompacto ||
            anchoTarjeta < EscalarDiseno(680) ||
            anchoTextoAmplio < EscalarDiseno(260);
        int anchoTitulo = presentacionCompacta
            ? Math.Max(1, anchoTarjeta - margenDerecho - xTexto)
            : anchoTextoAmplio;
        int anchoDescripcion = presentacionCompacta
            ? anchoInterior
            : anchoTextoAmplio;
        int ySuperior = EscalarDiseno(10);
        int altoNumero = CalcularAltoTextoCurso(presentacion.Numero, anchoNumero, 26);
        int altoNombre = CalcularAltoTextoCurso(presentacion.Nombre, anchoTitulo, 28);
        int altoBarra = EscalarDiseno(8);
        int xBarra;
        int yBarra;
        int anchoBarra;
        int altoTarjeta;

        tarjeta.SuspendLayout();

        try {
            presentacion.Numero.SetBounds(
                margenIzquierdo,
                ySuperior,
                anchoNumero,
                altoNumero);
            presentacion.Nombre.SetBounds(xTexto, ySuperior, anchoTitulo, altoNombre);

            if (presentacionCompacta) {
                int altoFilaTitulo = Math.Max(altoNumero, altoNombre);
                int yDescripcion = ySuperior + altoFilaTitulo + EscalarDiseno(5);
                int altoDescripcion = CalcularAltoTextoCurso(
                    presentacion.Descripcion,
                    anchoDescripcion,
                    36);
                presentacion.Descripcion.SetBounds(
                    margenIzquierdo,
                    yDescripcion,
                    anchoDescripcion,
                    altoDescripcion);

                int separacionEstadisticas = EscalarDiseno(8);
                int anchoColumnaEstadisticas = Math.Max(
                    1,
                    (anchoInterior - separacionEstadisticas * 2) / 3);
                int xPorcentaje = margenIzquierdo;
                int xProgreso = xPorcentaje + anchoColumnaEstadisticas + separacionEstadisticas;
                int xEstado = xProgreso + anchoColumnaEstadisticas + separacionEstadisticas;
                int anchoEstado = Math.Max(1, margenIzquierdo + anchoInterior - xEstado);
                int yEstadisticas = yDescripcion + altoDescripcion + EscalarDiseno(7);
                int altoPorcentaje = CalcularAltoTextoCurso(
                    presentacion.Porcentaje,
                    anchoColumnaEstadisticas,
                    21);
                int altoProgreso = CalcularAltoTextoCurso(
                    presentacion.Progreso,
                    anchoColumnaEstadisticas,
                    21);
                int altoEstado = CalcularAltoTextoCurso(
                    presentacion.Estado,
                    anchoEstado,
                    23);
                int altoFilaEstadisticas = Math.Max(
                    altoPorcentaje,
                    Math.Max(altoProgreso, altoEstado));

                presentacion.Porcentaje.TextAlign = ContentAlignment.MiddleLeft;
                presentacion.Progreso.TextAlign = ContentAlignment.MiddleCenter;
                presentacion.Estado.TextAlign = ContentAlignment.MiddleRight;
                presentacion.Porcentaje.SetBounds(
                    xPorcentaje,
                    yEstadisticas,
                    anchoColumnaEstadisticas,
                    altoFilaEstadisticas);
                presentacion.Progreso.SetBounds(
                    xProgreso,
                    yEstadisticas,
                    anchoColumnaEstadisticas,
                    altoFilaEstadisticas);
                presentacion.Estado.SetBounds(
                    xEstado,
                    yEstadisticas,
                    anchoEstado,
                    altoFilaEstadisticas);

                xBarra = margenIzquierdo;
                anchoBarra = anchoInterior;
                yBarra = yEstadisticas + altoFilaEstadisticas + EscalarDiseno(7);
                altoTarjeta = yBarra + altoBarra + EscalarDiseno(12);
            } else {
                int yDescripcion = ySuperior + altoNombre + EscalarDiseno(4);
                int altoDescripcion = CalcularAltoTextoCurso(
                    presentacion.Descripcion,
                    anchoDescripcion,
                    36);
                int altoPorcentaje = CalcularAltoTextoCurso(
                    presentacion.Porcentaje,
                    anchoEstadisticas,
                    21);
                int yProgreso = ySuperior + altoPorcentaje + EscalarDiseno(2);
                int altoProgreso = CalcularAltoTextoCurso(
                    presentacion.Progreso,
                    anchoEstadisticas,
                    21);
                int yEstado = yProgreso + altoProgreso + EscalarDiseno(3);
                int altoEstado = CalcularAltoTextoCurso(
                    presentacion.Estado,
                    anchoEstadisticas,
                    23);

                presentacion.Descripcion.SetBounds(
                    xTexto,
                    yDescripcion,
                    anchoDescripcion,
                    altoDescripcion);
                presentacion.Porcentaje.TextAlign = ContentAlignment.MiddleRight;
                presentacion.Progreso.TextAlign = ContentAlignment.MiddleRight;
                presentacion.Estado.TextAlign = ContentAlignment.MiddleRight;
                presentacion.Porcentaje.SetBounds(
                    xEstadisticas,
                    ySuperior,
                    anchoEstadisticas,
                    altoPorcentaje);
                presentacion.Progreso.SetBounds(
                    xEstadisticas,
                    yProgreso,
                    anchoEstadisticas,
                    altoProgreso);
                presentacion.Estado.SetBounds(
                    xEstadisticas,
                    yEstado,
                    anchoEstadisticas,
                    altoEstado);

                xBarra = xEstadisticas;
                anchoBarra = anchoEstadisticas;
                yBarra = yEstado + altoEstado + EscalarDiseno(7);
                altoTarjeta = Math.Max(
                    yDescripcion + altoDescripcion,
                    yBarra + altoBarra) + EscalarDiseno(12);
            }

            tarjeta.Size = new Size(anchoTarjeta, altoTarjeta);
            presentacion.FondoProgreso.SetBounds(xBarra, yBarra, anchoBarra, altoBarra);
            presentacion.RellenoProgreso.SetBounds(
                0,
                0,
                (int)Math.Round(anchoBarra * presentacion.ValorPorcentaje / 100D),
                altoBarra);
        } finally {
            tarjeta.ResumeLayout(performLayout: false);
        }

        tarjeta.Invalidate();
    }

    private void ReconstruirTarjetasPracticas(TemaCurso tema, bool volverAlInicio = true) {
        listaPracticasTema.SuspendLayout();

        try {
            VaciarYDisponerControles(listaPracticasTema);
            int anchoTarjeta = Math.Max(
                1,
                listaPracticasTema.ClientSize.Width - listaPracticasTema.Padding.Horizontal - 4);
            int margenIzquierdo = EscalarDiseno(18);
            int margenDerecho = EscalarDiseno(18);
            int anchoNumero = EscalarDiseno(45);
            int xTexto = margenIzquierdo + anchoNumero + EscalarDiseno(5);
            int separacionColumnas = EscalarDiseno(16);
            int anchoMaximoMetadatos = Math.Max(
                1,
                anchoTarjeta - xTexto - separacionColumnas - margenDerecho);
            int anchoMinimoMetadatos = Math.Min(EscalarDiseno(180), anchoMaximoMetadatos);
            int anchoMetadatos = Math.Min(EscalarDiseno(210), anchoMaximoMetadatos);
            anchoMetadatos = Math.Max(anchoMinimoMetadatos, anchoMetadatos);
            int xMetadatos = anchoTarjeta - margenDerecho - anchoMetadatos;
            int anchoTexto = Math.Max(1, xMetadatos - separacionColumnas - xTexto);

            foreach (PracticaCurso practica in tema.Practicas.OrderBy(item => item.Numero)) {
                EstadoPracticaCurso estadoPractica = ObtenerEstadoPractica(practica.Id);
                Panel tarjeta = CrearTarjetaCurso(
                    Point.Empty,
                    new Size(anchoTarjeta, EscalarDiseno(86)),
                    14,
                    interactiva: true);
                tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(10));
                tarjeta.TabIndex = Math.Max(0, practica.Numero - 1);
                tarjeta.AccessibleName = $"Práctica {practica.Numero:00}: {practica.Nombre}";
                tarjeta.AccessibleDescription =
                    $"{NormalizarDificultad(practica.Dificultad)}. " +
                    $"Duración {practica.DuracionEstimada}. " +
                    $"Estado {ObtenerTextoEstadoPractica(estadoPractica)}.";

                Label numero = CrearLabelCurso(
                    practica.Numero.ToString("00"),
                    Point.Empty,
                    new Size(anchoNumero, 1),
                    TamanoFuenteCurso.NumeroTarjeta,
                    FontStyle.Bold,
                    ColorMoradoClaroCurso,
                    ContentAlignment.TopLeft);
                Label nombre = CrearLabelCurso(
                    practica.Nombre,
                    Point.Empty,
                    new Size(anchoTexto, 1),
                    TamanoFuenteCurso.TituloTarjetaPractica,
                    FontStyle.Bold,
                    Color.White,
                    ContentAlignment.TopLeft);
                Label descripcion = CrearLabelCurso(
                    practica.Objetivo,
                    Point.Empty,
                    new Size(anchoTexto, 1),
                    TamanoFuenteCurso.DescripcionTarjetaPractica,
                    FontStyle.Regular,
                    ColorTextoSecundarioCurso,
                    ContentAlignment.TopLeft);
                Label estado = CrearLabelCurso(
                    ObtenerTextoEstadoPractica(estadoPractica),
                    Point.Empty,
                    new Size(anchoMetadatos, 1),
                    TamanoFuenteCurso.EstadoTarjetaPractica,
                    FontStyle.Bold,
                    ObtenerColorEstado(estadoPractica),
                    ContentAlignment.MiddleRight);
                Label metadatos = CrearLabelCurso(
                    $"{NormalizarDificultad(practica.Dificultad)} · {practica.DuracionEstimada}",
                    Point.Empty,
                    new Size(anchoMetadatos, 1),
                    TamanoFuenteCurso.MetadatoTarjetaPractica,
                    FontStyle.Regular,
                    ColorTextoSecundarioCurso,
                    ContentAlignment.MiddleRight);

                int ySuperior = EscalarDiseno(10);
                int altoNumero = CalcularAltoTextoCurso(numero, anchoNumero, 26);
                int altoNombre = CalcularAltoTextoCurso(nombre, anchoTexto, 28);
                int yDescripcion = ySuperior + altoNombre + EscalarDiseno(3);
                int altoDescripcion = CalcularAltoTextoCurso(descripcion, anchoTexto, 35);
                int altoEstado = CalcularAltoTextoCurso(estado, anchoMetadatos, 25);
                int yMetadatos = ySuperior + altoEstado + EscalarDiseno(3);
                int altoMetadatos = CalcularAltoTextoCurso(metadatos, anchoMetadatos, 24);
                int altoTarjeta = Math.Max(
                    yDescripcion + altoDescripcion,
                    yMetadatos + altoMetadatos) + EscalarDiseno(12);

                tarjeta.Size = new Size(anchoTarjeta, altoTarjeta);
                numero.SetBounds(margenIzquierdo, ySuperior, anchoNumero, altoNumero);
                nombre.SetBounds(xTexto, ySuperior, anchoTexto, altoNombre);
                descripcion.SetBounds(xTexto, yDescripcion, anchoTexto, altoDescripcion);
                estado.SetBounds(xMetadatos, ySuperior, anchoMetadatos, altoEstado);
                metadatos.SetBounds(
                    xMetadatos,
                    yMetadatos,
                    anchoMetadatos,
                    altoMetadatos);

                tarjeta.Controls.Add(numero);
                tarjeta.Controls.Add(nombre);
                tarjeta.Controls.Add(descripcion);
                tarjeta.Controls.Add(estado);
                tarjeta.Controls.Add(metadatos);
                ConfigurarInteraccionTarjeta(tarjeta, () => MostrarDetallePractica(practica));
                listaPracticasTema.Controls.Add(tarjeta);
            }
        } finally {
            listaPracticasTema.ResumeLayout(performLayout: true);
            desplazamientoPracticasTema.ActualizarContenido(volverAlInicio);
        }
    }

    private void ReconstruirDetallePractica(PracticaCurso practica, bool volverAlInicio = true) {
        contenidoDetallePractica.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoDetallePractica);
            int anchoContenido = Math.Max(
                1,
                contenidoDetallePractica.ClientSize.Width -
                contenidoDetallePractica.Padding.Horizontal -
                4);
            EstadoPracticaCurso estado = ObtenerEstadoPractica(practica.Id);

            AgregarLabelFluido(
                contenidoDetallePractica,
                $"Práctica No. {practica.Numero}",
                anchoContenido,
                TamanoFuenteCurso.NumeroCabecera,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            AgregarLabelFluido(
                contenidoDetallePractica,
                practica.Nombre,
                anchoContenido,
                TamanoFuenteCurso.TituloPrincipal,
                FontStyle.Bold,
                Color.White);

            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Estado actual",
                ObtenerTextoEstadoPractica(estado),
                anchoContenido,
                ObtenerColorEstado(estado));
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Dificultad",
                NormalizarDificultad(practica.Dificultad),
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Duración estimada",
                string.IsNullOrWhiteSpace(practica.DuracionEstimada)
                    ? "Duración por definir"
                    : practica.DuracionEstimada,
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Requisitos previos",
                practica.RequisitosPrevios.Count == 0
                    ? "Sin requisitos previos"
                    : string.Join("   •   ", practica.RequisitosPrevios),
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Objetivo",
                practica.Objetivo,
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Descripción",
                practica.Descripcion,
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Conceptos que practicarás",
                string.Join("   •   ", practica.Conceptos),
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Instrucciones generales",
                string.Join(Environment.NewLine, practica.Instrucciones.Select(
                    (instruccion, indice) => $"{indice + 1}. {instruccion}")),
                anchoContenido);
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Resultado esperado",
                practica.ResultadoEsperado,
                anchoContenido);

            Button btnAccionPractica = CrearBotonCurso(
                ObtenerTextoAccionPractica(practica),
                Point.Empty,
                new Size(anchoContenido, EscalarDiseno(44)),
                ColorMoradoCurso);
            btnAccionPractica.AccessibleDescription =
                "Crea el proyecto o abre la práctica existente según su estado actual.";
            btnAccionPractica.Margin = new Padding(
                0,
                EscalarDiseno(10),
                0,
                EscalarDiseno(8));
            btnAccionPractica.TabIndex = 0;
            btnAccionPractica.Click += BtnAccionPracticaCurso_Click;
            contenidoDetallePractica.Controls.Add(btnAccionPractica);

            int separacionBotones = EscalarDiseno(14);
            int altoBotonEstado = EscalarDiseno(38);
            bool apilarBotones = anchoContenido < EscalarDiseno(360);
            int anchoBotonEstado = apilarBotones
                ? anchoContenido
                : Math.Max(1, (anchoContenido - separacionBotones) / 2);
            int altoPanelAcciones = apilarBotones
                ? altoBotonEstado * 2 + separacionBotones
                : altoBotonEstado;
            Panel panelAccionesEstado = new() {
                BackColor = Color.FromArgb(18, 14, 27),
                Margin = new Padding(0, 0, 0, EscalarDiseno(16)),
                Size = new Size(anchoContenido, altoPanelAcciones)
            };
            Button btnRealizada = CrearBotonSecundarioCurso(
                "Marcar como realizada",
                Point.Empty,
                new Size(anchoBotonEstado, altoBotonEstado));
            Button btnPendiente = CrearBotonSecundarioCurso(
                "Marcar como pendiente",
                apilarBotones
                    ? new Point(0, altoBotonEstado + separacionBotones)
                    : new Point(anchoBotonEstado + separacionBotones, 0),
                new Size(anchoBotonEstado, altoBotonEstado));
            btnRealizada.TabIndex = 0;
            btnPendiente.TabIndex = 1;
            btnRealizada.Click += (_, _) => CambiarEstadoPracticaCurso(EstadoPracticaCurso.Realizada);
            btnPendiente.Click += (_, _) => CambiarEstadoPracticaCurso(EstadoPracticaCurso.Pendiente);
            panelAccionesEstado.Controls.Add(btnRealizada);
            panelAccionesEstado.Controls.Add(btnPendiente);
            contenidoDetallePractica.Controls.Add(panelAccionesEstado);
        } finally {
            contenidoDetallePractica.ResumeLayout(performLayout: true);
            desplazamientoDetallePractica.ActualizarContenido(volverAlInicio);
        }
    }

    private void BtnAccionPracticaCurso_Click(object? sender, EventArgs e) {
        if (practicaCursoSeleccionada is null || temaCursoSeleccionado is null) {
            return;
        }

        ProgresoPractica? progreso = ObtenerProgresoPractica(practicaCursoSeleccionada.Id);
        string rutaProyecto = progreso?.RutaProyecto ?? string.Empty;
        bool proyectoExiste =
            !string.IsNullOrWhiteSpace(rutaProyecto) && Directory.Exists(rutaProyecto);

        if (proyectoExiste) {
            bool aperturaExitosa = IntentarAbrirPractica(rutaProyecto, promoverReciente: true);

            if (aperturaExitosa && progreso?.Estado != EstadoPracticaCurso.Realizada) {
                PersistirEstadoPractica(
                    practicaCursoSeleccionada.Id,
                    EstadoPracticaCurso.EnProgreso,
                    rutaProyecto);
            }

            if (aperturaExitosa) {
                ReconstruirDetallePractica(practicaCursoSeleccionada);
            }
            return;
        }

        ResultadoCreacionPractica? resultado = EjecutarCreacionPractica(
            temaCursoSeleccionado.NombreCarpeta,
            practicaCursoSeleccionada.NombreProyecto,
            practicaCursoSeleccionada.Objetivo,
            () => { },
            out rutaProyecto);

        if (resultado is null) {
            return;
        }

        if (resultado.Estado == EstadoCreacionPractica.ErrorApertura) {
            MostrarResultadoCreacionPractica(resultado, enfocarNombreProyecto: false);
            PersistirEstadoPractica(
                practicaCursoSeleccionada.Id,
                EstadoPracticaCurso.Pendiente,
                rutaProyecto);
            ReconstruirDetallePractica(practicaCursoSeleccionada);
            return;
        }

        if (!MostrarResultadoCreacionPractica(resultado, enfocarNombreProyecto: false)) {
            return;
        }

        PersistirEstadoPractica(
            practicaCursoSeleccionada.Id,
            EstadoPracticaCurso.EnProgreso,
            rutaProyecto);
        ReconstruirDetallePractica(practicaCursoSeleccionada);
    }

    private void CambiarEstadoPracticaCurso(EstadoPracticaCurso estado) {
        if (practicaCursoSeleccionada is null) {
            return;
        }

        if (!PersistirEstadoPractica(practicaCursoSeleccionada.Id, estado)) {
            return;
        }

        ReconstruirTarjetasTemas();

        if (temaCursoSeleccionado is not null) {
            ReconstruirTarjetasPracticas(temaCursoSeleccionado);
        }

        ReconstruirDetallePractica(practicaCursoSeleccionada);
        ActualizarResumenCurso();
    }

    private bool PersistirEstadoPractica(
        string practicaId,
        EstadoPracticaCurso estado,
        string? rutaProyecto = null) {
        ResultadoEscrituraProgreso resultado = progresoCursoService.ActualizarEstado(
            practicaId,
            estado,
            rutaProyecto);

        if (!resultado.EsExitosa) {
            if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
                ActualizarProgresoEnMemoria(practicaId, estado, rutaProyecto);
            }

            string mensaje = resultado.Estado switch {
                EstadoEscrituraProgreso.PermisosInsuficientes =>
                    "No se pudo guardar el progreso porque no hay permisos para escribir progreso.json.",
                EstadoEscrituraProgreso.ContenidoInvalido =>
                    "No se pudo guardar el progreso porque progreso.json contiene información dañada que no puede recuperarse de forma segura.",
                _ =>
                    "No se pudo guardar el progreso. Verifica que progreso.json no esté bloqueado y que su carpeta permita crear y reemplazar archivos."
            };

            if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
                mensaje += " La práctica seguirá disponible durante esta sesión.";
            }

            MessageBox.Show(mensaje, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        ActualizarProgresoEnMemoria(practicaId, estado, rutaProyecto);
        CargarProgresoCurso(mostrarAvisoInmediato: false);
        return true;
    }

    private void ActualizarProgresoEnMemoria(
        string practicaId,
        EstadoPracticaCurso estado,
        string? rutaProyecto) {
        ProgresoPractica? progreso = ObtenerProgresoPractica(practicaId);
        DateTimeOffset ahora = DateTimeOffset.Now;

        if (progreso is null) {
            progreso = new ProgresoPractica {
                PracticaId = practicaId,
                FechaCreacion = ahora,
                FechaActualizacion = ahora
            };
            progresoCurso.Practicas.Add(progreso);
        }

        progreso.Estado = estado;
        progreso.FechaActualizacion = ahora;

        if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
            progreso.RutaProyecto = rutaProyecto;
        }

        tarjetasTemasActualizadas = false;

        progreso.FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
            ? ahora
            : null;
    }

    private string ObtenerTextoAccionPractica(PracticaCurso practica) {
        ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);
        bool existe =
            !string.IsNullOrWhiteSpace(progreso?.RutaProyecto) &&
            Directory.Exists(progreso.RutaProyecto);

        if (!existe) {
            return "Crear práctica";
        }

        return progreso!.Estado == EstadoPracticaCurso.Realizada
            ? "Abrir práctica"
            : "Continuar práctica";
    }

    private ProgresoPractica? ObtenerProgresoPractica(string practicaId) {
        return progresoCurso.Practicas.FirstOrDefault(progreso =>
            progreso.PracticaId.Equals(practicaId, StringComparison.OrdinalIgnoreCase));
    }

    private EstadoPracticaCurso ObtenerEstadoPractica(string practicaId) {
        return ObtenerProgresoPractica(practicaId)?.Estado ?? EstadoPracticaCurso.Pendiente;
    }

    private int ContarPracticasRealizadas(TemaCurso tema) {
        HashSet<string> identificadores = tema.Practicas
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return progresoCurso.Practicas.Count(progreso =>
            progreso.Estado == EstadoPracticaCurso.Realizada &&
            identificadores.Contains(progreso.PracticaId));
    }

    private EstadoPracticaCurso ObtenerEstadoTema(TemaCurso tema, int realizadas) {
        if (realizadas >= tema.TotalPracticasPlaneadas) {
            return EstadoPracticaCurso.Realizada;
        }

        bool enProgreso = tema.Practicas.Any(practica => {
            ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);
            return progreso is not null &&
                (progreso.Estado != EstadoPracticaCurso.Pendiente ||
                 !string.IsNullOrWhiteSpace(progreso.RutaProyecto));
        });

        return enProgreso
            ? EstadoPracticaCurso.EnProgreso
            : EstadoPracticaCurso.Pendiente;
    }

    private static string ObtenerTextoEstadoTema(EstadoPracticaCurso estado) {
        return estado switch {
            EstadoPracticaCurso.Realizada => "✓ Completado",
            EstadoPracticaCurso.EnProgreso => "● En progreso",
            _ => "○ Pendiente"
        };
    }

    private static string ObtenerTextoEstadoPractica(EstadoPracticaCurso estado) {
        return estado switch {
            EstadoPracticaCurso.Realizada => "✓ Realizada",
            EstadoPracticaCurso.EnProgreso => "● En progreso",
            _ => "○ Pendiente"
        };
    }

    private static Color ObtenerColorEstado(EstadoPracticaCurso estado) {
        return estado switch {
            EstadoPracticaCurso.Realizada => Color.FromArgb(100, 214, 144),
            EstadoPracticaCurso.EnProgreso => Color.FromArgb(194, 128, 255),
            _ => Color.FromArgb(180, 174, 189)
        };
    }

    private static string NormalizarDificultad(string dificultad) {
        return dificultad.Equals("Inicial", StringComparison.OrdinalIgnoreCase)
            ? "Fácil"
            : dificultad;
    }

    private static Panel CrearTarjetaCurso(
        Point ubicacion,
        Size tamano,
        int radio,
        bool interactiva = false) {
        Panel tarjeta = interactiva
            ? new TarjetaCursoInteractiva()
            : new Panel();
        tarjeta.BackColor = ColorTarjetaCurso;
        tarjeta.Location = ubicacion;
        tarjeta.Size = tamano;

        if (interactiva) {
            tarjeta.AccessibleRole = AccessibleRole.PushButton;
        }

        AplicarRegionRedondeada(tarjeta, radio);
        tarjeta.SizeChanged += (_, _) => AplicarRegionRedondeada(tarjeta, radio);
        tarjeta.Paint += (_, e) => {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle limites = new(1, 1, tarjeta.Width - 3, tarjeta.Height - 3);
            Color colorBorde = tarjeta.ContainsFocus
                ? ColorMoradoClaroCurso
                : ColorBordeCurso;
            float anchoBorde = tarjeta.ContainsFocus ? 2.2F : 1.2F;

            using GraphicsPath contorno = CrearContornoRedondeado(limites, radio);
            using Pen borde = new(colorBorde, anchoBorde);
            e.Graphics.DrawPath(borde, contorno);
        };

        return tarjeta;
    }

    private static void AplicarRegionRedondeada(Control control, int radio) {
        if (control.Width <= 0 || control.Height <= 0) {
            return;
        }

        using GraphicsPath contorno = CrearContornoRedondeado(
            new Rectangle(0, 0, control.Width, control.Height),
            radio);
        Region? regionAnterior = control.Region;
        control.Region = new Region(contorno);
        regionAnterior?.Dispose();
    }

    private static Label CrearLabelCurso(
        string texto,
        Point ubicacion,
        Size tamano,
        float tamanoFuente,
        FontStyle estilo,
        Color color,
        ContentAlignment alineacion = ContentAlignment.MiddleLeft) {
        Font fuente = new("Segoe UI", tamanoFuente, estilo);
        Label label = new() {
            AutoEllipsis = false,
            BackColor = Color.Transparent,
            Font = fuente,
            ForeColor = color,
            Location = ubicacion,
            Size = tamano,
            Text = texto,
            TextAlign = alineacion
        };
        label.Disposed += (_, _) => fuente.Dispose();
        return label;
    }

    private static Button CrearBotonCurso(
        string texto,
        Point ubicacion,
        Size tamano,
        Color colorFondo) {
        Font fuente = new(
            "Segoe UI Semibold",
            TamanoFuenteCurso.BotonPrimario,
            FontStyle.Bold);
        Button boton = new() {
            BackColor = colorFondo,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            Font = fuente,
            ForeColor = Color.White,
            Location = ubicacion,
            Size = tamano,
            Text = texto,
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseDownBackColor = Color.FromArgb(116, 55, 178);
        boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(174, 108, 232);
        boton.Disposed += (_, _) => fuente.Dispose();
        return boton;
    }

    private static Button CrearBotonSecundarioCurso(string texto, Point ubicacion, Size tamano) {
        Font fuente = new(
            "Segoe UI Semibold",
            TamanoFuenteCurso.BotonSecundario,
            FontStyle.Bold);
        Button boton = new() {
            BackColor = Color.FromArgb(35, 28, 48),
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            Font = fuente,
            ForeColor = Color.FromArgb(220, 210, 232),
            Location = ubicacion,
            Size = tamano,
            Text = texto,
            UseVisualStyleBackColor = false
        };
        boton.FlatAppearance.BorderColor = ColorBordeCurso;
        boton.FlatAppearance.BorderSize = 1;
        boton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 33, 62);
        boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 41, 74);
        boton.Disposed += (_, _) => fuente.Dispose();
        return boton;
    }

    private static void ConfigurarInteraccionTarjeta(Panel tarjeta, Action alHacerClick) {
        tarjeta.AccessibleRole = AccessibleRole.PushButton;

        foreach (Control control in ObtenerArbolControles(tarjeta)) {
            control.Cursor = Cursors.Hand;
            control.Click += (_, _) => alHacerClick();
            control.MouseEnter += (_, _) => {
                tarjeta.BackColor = ColorTarjetaCursoHover;
                tarjeta.Invalidate();
            };
            control.MouseLeave += (_, _) => {
                if (!tarjeta.ClientRectangle.Contains(tarjeta.PointToClient(Cursor.Position))) {
                    tarjeta.BackColor = ColorTarjetaCurso;
                    tarjeta.Invalidate();
                }
            };
        }
    }

    private static IEnumerable<Control> ObtenerArbolControles(Control raiz) {
        yield return raiz;

        foreach (Control hijo in raiz.Controls) {
            foreach (Control descendiente in ObtenerArbolControles(hijo)) {
                yield return descendiente;
            }
        }
    }

    private static void VaciarYDisponerControles(Control contenedor) {
        Control[] controles = contenedor.Controls.Cast<Control>().ToArray();
        contenedor.Controls.Clear();

        foreach (Control control in controles) {
            control.Dispose();
        }
    }

    private void AgregarLabelFluido(
        FlowLayoutPanel contenedor,
        string texto,
        int ancho,
        float tamanoFuente,
        FontStyle estilo,
        Color color) {
        Label label = CrearLabelCurso(
            texto,
            Point.Empty,
            new Size(ancho, 1),
            tamanoFuente,
            estilo,
            color,
            ContentAlignment.TopLeft);
        label.BackColor = contenedor.BackColor;
        label.AutoSize = true;
        label.MinimumSize = new Size(ancho, 0);
        label.MaximumSize = new Size(ancho, 0);
        label.Margin = new Padding(0, 0, 0, EscalarDiseno(4));
        contenedor.Controls.Add(label);
    }

    private void AgregarSeccionDetalle(
        FlowLayoutPanel contenedor,
        string titulo,
        string contenido,
        int ancho,
        Color? colorContenido = null) {
        int margenHorizontal = EscalarDiseno(16);
        int margenSuperior = EscalarDiseno(12);
        int margenInferior = EscalarDiseno(16);
        int separacion = EscalarDiseno(6);
        int anchoInterior = Math.Max(1, ancho - margenHorizontal * 2);
        Label encabezado = CrearLabelCurso(
            titulo,
            Point.Empty,
            new Size(anchoInterior, 1),
            TamanoFuenteCurso.EncabezadoDetalle,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ContentAlignment.TopLeft);
        Label texto = CrearLabelCurso(
            contenido,
            Point.Empty,
            new Size(anchoInterior, 1),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            colorContenido ?? ColorTextoSecundarioCurso,
            ContentAlignment.TopLeft);
        int altoTitulo = CalcularAltoTextoCurso(encabezado, anchoInterior, 24);
        int altoContenido = CalcularAltoTextoCurso(texto, anchoInterior, 28);
        int altoTarjeta =
            margenSuperior + altoTitulo + separacion + altoContenido + margenInferior;
        Panel tarjeta = CrearTarjetaCurso(
            Point.Empty,
            new Size(ancho, altoTarjeta),
            12);
        tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(10));

        Panel acento = new() {
            BackColor = ColorMoradoCurso,
            Location = new Point(0, 0),
            Size = new Size(EscalarDiseno(4), altoTarjeta)
        };
        encabezado.SetBounds(
            margenHorizontal,
            margenSuperior,
            anchoInterior,
            altoTitulo);
        texto.SetBounds(
            margenHorizontal,
            margenSuperior + altoTitulo + separacion,
            anchoInterior,
            altoContenido);

        tarjeta.Controls.Add(acento);
        tarjeta.Controls.Add(encabezado);
        tarjeta.Controls.Add(texto);
        contenedor.Controls.Add(tarjeta);
    }
}
