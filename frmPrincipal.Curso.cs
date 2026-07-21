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

    private sealed class PresentacionTarjetaPractica {
        public PresentacionTarjetaPractica(
            Label numero,
            Label nombre,
            Label descripcion,
            Label estado,
            Label metadatos) {
            Numero = numero;
            Nombre = nombre;
            Descripcion = descripcion;
            Estado = estado;
            Metadatos = metadatos;
        }

        public Label Numero { get; }
        public Label Nombre { get; }
        public Label Descripcion { get; }
        public Label Estado { get; }
        public Label Metadatos { get; }
    }

    private readonly record struct EstadoContenidoTema(
        string Id,
        int Numero,
        string Nombre,
        string Descripcion,
        bool EsProximamente,
        string MensajeDisponibilidad,
        int TotalPracticasPlaneadas,
        int PracticasRealizadas,
        int Porcentaje,
        EstadoPracticaCurso Estado,
        string FirmaPracticas);

    private readonly record struct EstadoContenidoPractica(
        string Id,
        string TemaId,
        int Numero,
        string Nombre,
        string Objetivo,
        string Dificultad,
        string DuracionEstimada,
        EstadoPracticaCurso Estado,
        bool MostrarTiempos,
        string FirmaContenido);

    private sealed class PresentacionSeccionDetalle {
        public PresentacionSeccionDetalle(
            Panel acento,
            Label encabezado,
            Label texto) {
            Acento = acento;
            Encabezado = encabezado;
            Texto = texto;
        }

        public Panel Acento { get; }
        public Label Encabezado { get; }
        public Label Texto { get; }
    }

    private sealed class PresentacionAccionesDetalle {
        public PresentacionAccionesDetalle(Button realizada, Button pendiente) {
            Realizada = realizada;
            Pendiente = pendiente;
        }

        public Button Realizada { get; }
        public Button Pendiente { get; }
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
    private static readonly Color ColorBordeCurso = Color.FromArgb(116, 67, 163);
    private static readonly Color ColorBordeHoverCurso = Color.FromArgb(174, 108, 232);
    private static readonly Color ColorMoradoCurso = Color.FromArgb(145, 82, 214);
    private static readonly Color ColorMoradoClaroCurso = Color.FromArgb(202, 151, 247);
    private static readonly Color ColorTextoSecundarioCurso = Color.FromArgb(190, 181, 204);
    private const int DuracionTransicionVistaCursoMs = 235;

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
    private Label lblContenidoGradoCurso = null!;
    private Label lblResumenTemasCurso = null!;
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
    private Label lblNumeroDetallePracticaCurso = null!;
    private Label lblTituloDetallePracticaCurso = null!;
    private Size tamanoPanelPrincipalNormal;
    private bool modoCursoInmersivo;
    private bool cursoModoCompacto;
    private bool jerarquiaCursoPrincipalInicializada;
    private bool estructuraCursoInicializada;
    private bool inicializandoCurso;
    private bool cursoInicializado;
    private bool preparandoCurso;
    private bool cursoPreparado;
    private bool entradaCursoPendiente;
    private bool navegacionCursoEnCurso;
    private bool transicionVisualCursoActiva;
    private bool retirandoCubiertaTransicionCurso;
    private bool cubiertaTransicionCursoPintada;
    private bool destinoTransicionCursoListo;
    private bool preparacionDestinoTransicionCursoProgramada;
    private Task? tareaPreparacionCurso;
    private Action? preparacionDestinoTransicionCursoPendiente;
    private Panel? cubiertaTransicionCurso;
    private System.Windows.Forms.Timer? timerTransicionCurso;
    private Control? destinoTransicionCurso;
    private TarjetaCursoInteractiva? tarjetaCursoConHover;
    private long inicioRetiroCubiertaTransicionCurso;
    private long secuenciaTransicionVisualCurso;
    private long idTransicionVisualCursoActiva;
    private CursoService? cursoServicePrecargado;
    private ProgresoCursoService? progresoCursoServicePrecargado;
    private ResultadoCargaProgreso? resultadoProgresoCursoPrecargado;
    private int ultimoAnchoTarjetasTemas = -1;
    private int ultimoDpiTarjetasTemas = -1;
    private bool ultimoModoCompactoTarjetasTemas;
    private EstadoContenidoTema[] estadoTarjetasTemas = Array.Empty<EstadoContenidoTema>();
    private int ultimoAnchoTarjetasPracticas = -1;
    private int ultimoDpiTarjetasPracticas = -1;
    private string temaTarjetasPracticasId = string.Empty;
    private EstadoContenidoPractica[] estadoTarjetasPracticas =
        Array.Empty<EstadoContenidoPractica>();
    private int ultimoAnchoContenidoDetallePractica = -1;
    private int ultimoDpiContenidoDetallePractica = -1;
    private PracticaCurso? practicaDetalleConstruida;
    private EstadoPracticaCurso estadoPracticaDetalleConstruida;
    private string rutaProyectoDetalleConstruida = string.Empty;
    private bool rutaProyectoDetalleExistia;
    private bool mostrarTiemposDetalleConstruido;
    private bool evaluacionEnCursoDetalleConstruida;

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
            if (!PuedeContinuarConstruccionCurso()) {
                return;
            }

            cursoService = cursoServicePrecargado ?? new CursoService();
            gradosService = new GradosService(cursoService);
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
                ConstruirVistaGrados,
                "Curso UI: ruta de aprendizaje creada");
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
            ActivarDobleBuffer(panelGradosVista);
            ActivarDobleBuffer(panelPracticasTemaVista);
            ActivarDobleBuffer(panelDetallePracticaVista);
            ActivarDobleBuffer(desplazamientoCursoPrincipal);
            ActivarDobleBuffer(desplazamientoGrados);
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

        return PuedeContinuarConstruccionCurso();
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
        panelGradosVista = CrearVistaCurso("panelGradosVista");
        panelCursoVista = CrearVistaCurso("panelCursoVista");
        panelPracticasTemaVista = CrearVistaCurso("panelPracticasTemaVista");
        panelDetallePracticaVista = CrearVistaCurso("panelDetallePracticaVista");

        panelPrincipal.Controls.Add(panelGradosVista);
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
        btnVolverGradosCurso = CrearBotonSecundarioCurso(
            "← Volver a grados",
            new Point(EscalarDiseno(18), EscalarDiseno(12)),
            new Size(EscalarDiseno(182), EscalarDiseno(32)));
        btnVolverGradosCurso.TabIndex = 0;
        btnVolverGradosCurso.Click += (_, _) => MostrarGrados();

        panelPerfilCurso = CrearTarjetaCurso(new Point(18, 18), new Size(278, 434), 18);
        panelPerfilCurso.TabIndex = 0;

        lblTituloPerfilCurso = CrearLabelCurso(
            "Grado 1",
            new Point(22, 18),
            new Size(232, 34),
            TamanoFuenteCurso.TituloPerfil,
            FontStyle.Bold,
            Color.White);
        lblNombrePerfilCurso = CrearLabelCurso(
            "Fundamentos de C++",
            new Point(22, 54),
            new Size(232, 24),
            TamanoFuenteCurso.NombrePerfil,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        lblNivelPerfilCurso = CrearLabelCurso(
            "Aprende las bases esenciales de la programación en C++, desde variables y decisiones hasta archivos e introducción a la programación orientada a objetos.",
            new Point(22, 80),
            new Size(232, 22),
            TamanoFuenteCurso.TextoPerfil,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        lblProgresoGeneralCurso = CrearLabelCurso(
            $"PROGRESO DISPONIBLE\n0 de {cursoService.TotalPracticasDisponibles} prácticas realizadas\n0% del contenido disponible",
            new Point(22, 106),
            new Size(232, 58),
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

        lblContenidoGradoCurso = CrearLabelCurso(
            $"CONTENIDO DEL GRADO\n{cursoService.TotalPracticasDisponibles} de " +
            $"{GradosService.MetaCurricularPracticasGradoFundamentos} prácticas publicadas",
            new Point(22, 174),
            new Size(232, 42),
            TamanoFuenteCurso.ProgresoPerfil,
            FontStyle.Bold,
            Color.White,
            ContentAlignment.TopLeft);

        lblResumenTemasCurso = CrearLabelCurso(
            "TEMAS DEL GRADO\n0 iniciados\n0 completados",
            new Point(22, 222),
            new Size(232, 54),
            TamanoFuenteCurso.ProgresoPerfil,
            FontStyle.Bold,
            Color.White,
            ContentAlignment.TopLeft);

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
        panelPerfilCurso.Controls.Add(lblContenidoGradoCurso);
        panelPerfilCurso.Controls.Add(lblResumenTemasCurso);
        panelPerfilCurso.Controls.Add(lblSiguientePasoCurso);
        panelPerfilCurso.Controls.Add(lblUltimaPracticaCurso);
        panelPerfilCurso.Controls.Add(btnContinuarAprendizaje);
        panelPerfilCurso.Controls.Add(lblTituloRecomendacionCurso);
        panelPerfilCurso.Controls.Add(lblPracticaRecomendadaCurso);
        panelPerfilCurso.Controls.Add(btnVerPracticaRecomendadaCurso);
        panelPerfilCurso.Controls.Add(lblAvisoProgresoCurso);

        lblTituloTemasCurso = CrearLabelCurso(
            "TEMAS DEL GRADO",
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
        panelCursoVista.Controls.Add(btnVolverGradosCurso);
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
            new Padding(4, 2, 4, EscalarDiseno(24)));
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

        lblNumeroDetallePracticaCurso = CrearLabelCurso(
            "Práctica No. 1",
            new Point(20, 58),
            new Size(850, 24),
            TamanoFuenteCurso.NumeroCabecera,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        lblTituloDetallePracticaCurso = CrearLabelCurso(
            string.Empty,
            new Point(20, 79),
            new Size(850, 47),
            TamanoFuenteCurso.TituloPrincipal,
            FontStyle.Bold,
            Color.White);

        desplazamientoDetallePractica = CrearPanelDesplazableCurso(
            "desplazamientoDetallePractica",
            "contenidoDetallePractica",
            new Point(16, 130),
            new Size(860, 322),
            new Padding(12, 10, 12, EscalarDiseno(24)));
        contenidoDetallePractica = desplazamientoDetallePractica.Contenido;

        desplazamientoDetallePractica.TabIndex = 1;
        panelDetallePracticaVista.Controls.Add(btnVolverPracticasCurso);
        panelDetallePracticaVista.Controls.Add(lblNumeroDetallePracticaCurso);
        panelDetallePracticaVista.Controls.Add(lblTituloDetallePracticaCurso);
        panelDetallePracticaVista.Controls.Add(desplazamientoDetallePractica);
    }

    private async void PanelCurso_Click(object? sender, EventArgs e) {
        if (entradaCursoPendiente ||
            navegacionCursoEnCurso ||
            transicionVisualCursoActiva) {
            return;
        }

        Panel panelSeleccionadoAlSolicitar = panelSeleccionado;
        long secuenciaTransicionAlSolicitar = secuenciaTransicionVisualCurso;
        entradaCursoPendiente = true;

        try {
            await PrepararCursoParaInteraccionAsync();

            if (!cursoPreparado || IsDisposed || Disposing ||
                navegacionCursoEnCurso || transicionVisualCursoActiva ||
                !ReferenceEquals(panelSeleccionado, panelSeleccionadoAlSolicitar) ||
                secuenciaTransicionVisualCurso != secuenciaTransicionAlSolicitar) {
                return;
            }

            SeleccionarPanelMenu(panelCurso);
            OcultarVistasPrincipalesFueraDelCurso();

            if (gradoSeleccionadoEnSesion) {
                MostrarCursoPrincipal();
                return;
            }

            MostrarRutaAprendizajeInmersiva(reconstruirContenido: true);
        } finally {
            entradaCursoPendiente = false;
        }
    }

    private void MostrarCursoPrincipal() {
        if (!cursoPreparado || navegacionCursoEnCurso) {
            return;
        }

        bool destinoYaVisible =
            panelCursoVista.Visible &&
            vistaRutaActual == VistaRutaAprendizaje.DetalleGrado &&
            !modoCursoInmersivo &&
            panelMenu.Visible;

        if (destinoYaVisible) {
            return;
        }

        bool diferirPreparacionHastaCubierta =
            panelGradosVista.Visible &&
            WindowState == FormWindowState.Maximized;

        if (!IniciarTransicionVisualCurso(panelCursoVista)) {
            return;
        }

        RestablecerEstadosVisualesVistaActualCurso();

        if (diferirPreparacionHastaCubierta) {
            PrepararDestinoDespuesDeCubiertaTransicionCurso(
                PrepararYMostrarCursoPrincipal);
            return;
        }

        PrepararYMostrarCursoPrincipal();
    }

    private void PrepararYMostrarCursoPrincipal() {
        panelPrincipal.SuspendLayout();

        try {
            PrepararNavegacionPrincipalDesdeRuta();
            MostrarNavegacionPrincipal(
                DistribucionPanelPrincipal.Curso,
                invalidarFondo: false);
            SeleccionarPanelMenu(panelCurso);
            ActualizarResumenCurso();
            AsegurarTarjetasTemasVigentes(
                volverAlInicio: false,
                actualizarGeometriaSiVigente: false);
            AjustarCubiertaTransicionCurso();
            MostrarSubvistaCurso(panelCursoVista, VistaRutaAprendizaje.DetalleGrado);
            RestablecerEstadosVisualesTarjetasTemas();
        } catch {
            CancelarTransicionVisualCurso();
            throw;
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
        }

        fondoEndForge.Invalidate();
        ConfirmarDestinoTransicionCurso(panelCursoVista);
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

        bool mismoTemaVisible =
            panelPracticasTemaVista.Visible &&
            vistaRutaActual == VistaRutaAprendizaje.PracticasTema &&
            temaCursoSeleccionado?.Id.Equals(
                tema.Id,
                StringComparison.OrdinalIgnoreCase) == true;

        if (navegacionCursoEnCurso || mismoTemaVisible) {
            return;
        }

        if (!IniciarTransicionVisualCurso(panelPracticasTemaVista)) {
            return;
        }

        panelPrincipal.SuspendLayout();

        try {
            RestablecerEstadosVisualesVistaActualCurso();
            temaCursoSeleccionado = tema;
            lblNumeroTemaCurso.Text = $"Tema {tema.Numero:00}";
            lblNombreTemaCurso.Text = tema.Nombre;
            lblDescripcionTemaCurso.Text = tema.Descripcion;
            MostrarModoCursoInmersivo(invalidarFondo: false);
            AsegurarTarjetasPracticasVigentes(
                tema,
                volverAlInicio: false,
                actualizarGeometriaSiVigente: false);
            AjustarCubiertaTransicionCurso();
            MostrarSubvistaCurso(
                panelPracticasTemaVista,
                VistaRutaAprendizaje.PracticasTema);
            RestablecerEstadosVisualesTarjetasPracticas();
        } catch {
            CancelarTransicionVisualCurso();
            throw;
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
        }

        fondoEndForge.Invalidate();
        ConfirmarDestinoTransicionCurso(panelPracticasTemaVista);
    }

    private void MostrarDetallePractica(PracticaCurso practica) {
        bool mismoDetalleVisible =
            panelDetallePracticaVista.Visible &&
            vistaRutaActual == VistaRutaAprendizaje.DetallePractica &&
            practicaCursoSeleccionada?.Id.Equals(
                practica.Id,
                StringComparison.OrdinalIgnoreCase) == true;

        if (navegacionCursoEnCurso || mismoDetalleVisible) {
            return;
        }

        if (!IniciarTransicionVisualCurso(panelDetallePracticaVista)) {
            return;
        }

        RestablecerEstadosVisualesVistaActualCurso();
        PrepararDestinoDespuesDeCubiertaTransicionCurso(
            () => PrepararYMostrarDetallePractica(practica));
    }

    private void PrepararYMostrarDetallePractica(PracticaCurso practica) {
        panelPrincipal.SuspendLayout();

        try {
            practicaCursoSeleccionada = practica;
            temaCursoSeleccionado = cursoService.ObtenerTema(practica.TemaId);
            lblNumeroDetallePracticaCurso.Text = $"Práctica No. {practica.Numero}";
            lblTituloDetallePracticaCurso.Text = practica.Nombre;
            bool requiereModoInmersivo = !modoCursoInmersivo || panelMenu.Visible;

            if (requiereModoInmersivo) {
                MostrarModoCursoInmersivo(invalidarFondo: false);
            } else {
                ActualizarGeometriaDetallePractica();
            }

            AsegurarDetallePracticaVigente(practica);
            AjustarCubiertaTransicionCurso();
            MostrarSubvistaCurso(
                panelDetallePracticaVista,
                VistaRutaAprendizaje.DetallePractica);
        } catch {
            CancelarTransicionVisualCurso();
            throw;
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
        }

        fondoEndForge.Invalidate();
        ConfirmarDestinoTransicionCurso(panelDetallePracticaVista);
    }

    private void FrmPrincipal_CursoKeyDown(object? sender, KeyEventArgs e) {
        if (!entradaAplicacionRealizada || e.KeyCode != Keys.Escape) {
            return;
        }

        if (!NavegarAtrasRuta()) {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void MostrarNavegacionPrincipal(
        DistribucionPanelPrincipal distribucion = DistribucionPanelPrincipal.Normal,
        bool invalidarFondo = true) {
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

        if (invalidarFondo && !transicionandoDesdeBienvenida) {
            InvalidarFondoContinuo();
        }
    }

    private void MostrarModoCursoInmersivo(bool invalidarFondo = true) {
        timerRecalcularVista.Stop();
        modoCursoInmersivo = true;
        distribucionPanelPrincipal = DistribucionPanelPrincipal.Curso;
        panelMenu.Visible = false;
        panelPrincipal.Visible = true;
        panelBarraTitulo.BringToFront();
        RecalcularDistribucionActual();
        if (invalidarFondo) {
            InvalidarFondoContinuo();
        }
    }

    private bool IniciarTransicionVisualCurso(Control destino) {
        if (navegacionCursoEnCurso ||
            transicionVisualCursoActiva ||
            IsDisposed ||
            Disposing) {
            return false;
        }

        AsegurarCubiertaTransicionCurso();

        if (cubiertaTransicionCurso is null || cubiertaTransicionCurso.IsDisposed) {
            return false;
        }

        LimpiarHoverTarjetaCurso();
        navegacionCursoEnCurso = true;
        transicionVisualCursoActiva = true;
        retirandoCubiertaTransicionCurso = false;
        cubiertaTransicionCursoPintada = false;
        destinoTransicionCursoListo = false;
        preparacionDestinoTransicionCursoProgramada = false;
        preparacionDestinoTransicionCursoPendiente = null;
        idTransicionVisualCursoActiva = ++secuenciaTransicionVisualCurso;
        destinoTransicionCurso = destino;
        destinoTransicionCurso.Paint += DestinoTransicionCurso_Paint;

        RestablecerRegionCubiertaTransicionCurso();
        AjustarCubiertaTransicionCurso();
        cubiertaTransicionCurso.Visible = true;
        cubiertaTransicionCurso.BringToFront();
        cubiertaTransicionCurso.Invalidate();
        return true;
    }

    private void AsegurarCubiertaTransicionCurso() {
        if (cubiertaTransicionCurso is not null &&
            !cubiertaTransicionCurso.IsDisposed) {
            return;
        }

        cubiertaTransicionCurso = new Panel {
            Name = "panelCubiertaTransicionCurso",
            BackColor = Color.FromArgb(18, 14, 27),
            Location = Point.Empty,
            Size = panelPrincipal.ClientSize,
            TabStop = false,
            Visible = false
        };
        ActivarDobleBuffer(cubiertaTransicionCurso);
        cubiertaTransicionCurso.Paint += CubiertaTransicionCurso_Paint;
        panelPrincipal.Controls.Add(cubiertaTransicionCurso);
        panelPrincipal.SizeChanged += PanelPrincipal_TransicionVisualSizeChanged;

        timerTransicionCurso = new System.Windows.Forms.Timer {
            Interval = 15
        };
        timerTransicionCurso.Tick += TimerTransicionCurso_Tick;
        cubiertaTransicionCurso.Disposed += (_, _) => {
            panelPrincipal.SizeChanged -= PanelPrincipal_TransicionVisualSizeChanged;

            if (timerTransicionCurso is not null) {
                timerTransicionCurso.Stop();
                timerTransicionCurso.Tick -= TimerTransicionCurso_Tick;
                timerTransicionCurso.Dispose();
            }

            timerTransicionCurso = null;
            cubiertaTransicionCurso = null;
        };
    }

    private void AjustarCubiertaTransicionCurso() {
        if (cubiertaTransicionCurso is null || cubiertaTransicionCurso.IsDisposed) {
            return;
        }

        Rectangle limites = panelPrincipal.ClientRectangle;

        if (cubiertaTransicionCurso.Bounds != limites) {
            cubiertaTransicionCurso.SetBounds(
                limites.X,
                limites.Y,
                limites.Width,
                limites.Height);
        }
    }

    private void ConfirmarDestinoTransicionCurso(Control destino) {
        if (!transicionVisualCursoActiva ||
            !ReferenceEquals(destinoTransicionCurso, destino) ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            CancelarTransicionVisualCurso();
            return;
        }

        AjustarCubiertaTransicionCurso();
        cubiertaTransicionCurso?.BringToFront();
        cubiertaTransicionCurso?.Invalidate();
        destino.Invalidate();
        long idTransicion = idTransicionVisualCursoActiva;

        try {
            BeginInvoke((Action)(() => {
                MarcarDestinoTransicionCursoListo(idTransicion, destino);
            }));
        } catch (InvalidOperationException) when (
            IsDisposed || Disposing || !IsHandleCreated) {
            CancelarTransicionVisualCurso();
        }
    }

    private void DestinoTransicionCurso_Paint(object? sender, PaintEventArgs e) {
        if (sender is Control destino) {
            MarcarDestinoTransicionCursoListo(
                idTransicionVisualCursoActiva,
                destino);
        }
    }

    private void CubiertaTransicionCurso_Paint(object? sender, PaintEventArgs e) {
        if (!transicionVisualCursoActiva ||
            retirandoCubiertaTransicionCurso ||
            !ReferenceEquals(sender, cubiertaTransicionCurso)) {
            return;
        }

        cubiertaTransicionCursoPintada = true;
        ProgramarPreparacionDestinoTransicionCurso();
        IntentarIniciarRetiroCubiertaTransicionCurso();
    }

    private void PrepararDestinoDespuesDeCubiertaTransicionCurso(Action preparacion) {
        ArgumentNullException.ThrowIfNull(preparacion);

        if (!transicionVisualCursoActiva) {
            return;
        }

        preparacionDestinoTransicionCursoPendiente = preparacion;
        ProgramarPreparacionDestinoTransicionCurso();
    }

    private void ProgramarPreparacionDestinoTransicionCurso() {
        if (!transicionVisualCursoActiva ||
            !cubiertaTransicionCursoPintada ||
            preparacionDestinoTransicionCursoProgramada ||
            preparacionDestinoTransicionCursoPendiente is null ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        preparacionDestinoTransicionCursoProgramada = true;
        long idTransicion = idTransicionVisualCursoActiva;

        try {
            BeginInvoke((Action)(() => {
                if (!transicionVisualCursoActiva ||
                    idTransicion != idTransicionVisualCursoActiva) {
                    return;
                }

                if (!cubiertaTransicionCursoPintada) {
                    preparacionDestinoTransicionCursoProgramada = false;
                    return;
                }

                Action? preparacion = preparacionDestinoTransicionCursoPendiente;
                preparacionDestinoTransicionCursoPendiente = null;
                preparacionDestinoTransicionCursoProgramada = false;

                try {
                    preparacion?.Invoke();
                } catch {
                    CancelarTransicionVisualCurso();
                    throw;
                }
            }));
        } catch (InvalidOperationException) when (
            IsDisposed || Disposing || !IsHandleCreated) {
            CancelarTransicionVisualCurso();
        }
    }

    private void MarcarDestinoTransicionCursoListo(
        long idTransicion,
        Control destino) {
        if (!transicionVisualCursoActiva ||
            idTransicion != idTransicionVisualCursoActiva ||
            destinoTransicionCursoListo ||
            !ReferenceEquals(destinoTransicionCurso, destino)) {
            return;
        }

        destinoTransicionCursoListo = true;
        DesconectarEventoPaintDestinoTransicionCurso();
        IntentarIniciarRetiroCubiertaTransicionCurso();
    }

    private void IntentarIniciarRetiroCubiertaTransicionCurso() {
        if (cubiertaTransicionCursoPintada && destinoTransicionCursoListo) {
            IniciarRetiroCubiertaTransicionCurso();
        }
    }

    private void IniciarRetiroCubiertaTransicionCurso() {
        if (!transicionVisualCursoActiva ||
            retirandoCubiertaTransicionCurso ||
            !cubiertaTransicionCursoPintada ||
            !destinoTransicionCursoListo) {
            return;
        }

        retirandoCubiertaTransicionCurso = true;
        inicioRetiroCubiertaTransicionCurso = Environment.TickCount64;

        if (timerTransicionCurso is null) {
            CancelarTransicionVisualCurso();
            return;
        }

        timerTransicionCurso.Start();
    }

    private void TimerTransicionCurso_Tick(object? sender, EventArgs e) {
        if (!transicionVisualCursoActiva ||
            !retirandoCubiertaTransicionCurso ||
            cubiertaTransicionCurso is null ||
            cubiertaTransicionCurso.IsDisposed) {
            CancelarTransicionVisualCurso();
            return;
        }

        float progreso = ObtenerProgresoTransicionVisualCurso();

        if (progreso >= 1F) {
            FinalizarTransicionVisualCurso();
            return;
        }

        AplicarRegionCubiertaTransicionCurso(progreso);
    }

    private float ObtenerProgresoTransicionVisualCurso() {
        if (!retirandoCubiertaTransicionCurso) {
            return 0F;
        }

        double transcurrido = Environment.TickCount64 - inicioRetiroCubiertaTransicionCurso;
        return (float)Math.Clamp(
            transcurrido / DuracionTransicionVistaCursoMs,
            0D,
            1D);
    }

    private void PanelPrincipal_TransicionVisualSizeChanged(object? sender, EventArgs e) {
        if (!transicionVisualCursoActiva ||
            cubiertaTransicionCurso is null ||
            cubiertaTransicionCurso.IsDisposed) {
            return;
        }

        float progreso = ObtenerProgresoTransicionVisualCurso();
        RestablecerRegionCubiertaTransicionCurso();
        AjustarCubiertaTransicionCurso();

        if (retirandoCubiertaTransicionCurso) {
            if (progreso >= 1F) {
                FinalizarTransicionVisualCurso();
                return;
            }

            AplicarRegionCubiertaTransicionCurso(progreso);
        } else {
            cubiertaTransicionCursoPintada = false;
        }

        cubiertaTransicionCurso.BringToFront();
        cubiertaTransicionCurso.Invalidate();
    }

    private void AplicarRegionCubiertaTransicionCurso(float progreso) {
        if (cubiertaTransicionCurso is null || cubiertaTransicionCurso.IsDisposed) {
            return;
        }

        int ancho = cubiertaTransicionCurso.ClientSize.Width;
        int alto = cubiertaTransicionCurso.ClientSize.Height;

        if (ancho <= 0 || alto <= 0) {
            return;
        }

        float proporcionRestante = 1F - progreso;
        int centro = ancho / 2;
        int anchoIzquierdo = (int)Math.Ceiling(centro * proporcionRestante);
        int anchoDerecho = (int)Math.Ceiling((ancho - centro) * proporcionRestante);

        using GraphicsPath regionVisible = new();

        if (anchoIzquierdo > 0) {
            regionVisible.AddRectangle(new Rectangle(0, 0, anchoIzquierdo, alto));
        }

        if (anchoDerecho > 0) {
            regionVisible.AddRectangle(
                new Rectangle(ancho - anchoDerecho, 0, anchoDerecho, alto));
        }

        Region? regionAnterior = cubiertaTransicionCurso.Region;
        cubiertaTransicionCurso.Region = new Region(regionVisible);
        regionAnterior?.Dispose();
    }

    private void FinalizarTransicionVisualCurso() {
        if (!transicionVisualCursoActiva) {
            LimpiarHoverTarjetaCurso();
            navegacionCursoEnCurso = false;
            return;
        }

        timerTransicionCurso?.Stop();
        transicionVisualCursoActiva = false;
        retirandoCubiertaTransicionCurso = false;

        if (cubiertaTransicionCurso is not null &&
            !cubiertaTransicionCurso.IsDisposed) {
            cubiertaTransicionCurso.Visible = false;
            RestablecerRegionCubiertaTransicionCurso();
        }

        LimpiarHoverTarjetaCurso();
        RestablecerEstadosVisualesVistaActualCurso(aplicarHoverReal: true);
        DesconectarDestinoTransicionCurso();
        cubiertaTransicionCursoPintada = false;
        destinoTransicionCursoListo = false;
        preparacionDestinoTransicionCursoProgramada = false;
        preparacionDestinoTransicionCursoPendiente = null;
        idTransicionVisualCursoActiva = 0;
        navegacionCursoEnCurso = false;
    }

    private void CancelarTransicionVisualCurso() {
        timerTransicionCurso?.Stop();
        transicionVisualCursoActiva = false;
        retirandoCubiertaTransicionCurso = false;
        LimpiarHoverTarjetaCurso();

        if (cubiertaTransicionCurso is not null &&
            !cubiertaTransicionCurso.IsDisposed) {
            cubiertaTransicionCurso.Visible = false;
            RestablecerRegionCubiertaTransicionCurso();
        }

        DesconectarDestinoTransicionCurso();
        cubiertaTransicionCursoPintada = false;
        destinoTransicionCursoListo = false;
        preparacionDestinoTransicionCursoProgramada = false;
        preparacionDestinoTransicionCursoPendiente = null;
        idTransicionVisualCursoActiva = 0;
        navegacionCursoEnCurso = false;
    }

    private void DesconectarEventoPaintDestinoTransicionCurso() {
        if (destinoTransicionCurso is not null &&
            !destinoTransicionCurso.IsDisposed) {
            destinoTransicionCurso.Paint -= DestinoTransicionCurso_Paint;
        }
    }

    private void DesconectarDestinoTransicionCurso() {
        DesconectarEventoPaintDestinoTransicionCurso();
        destinoTransicionCurso = null;
    }

    private void RestablecerRegionCubiertaTransicionCurso() {
        if (cubiertaTransicionCurso is null || cubiertaTransicionCurso.IsDisposed) {
            return;
        }

        Region? regionAnterior = cubiertaTransicionCurso.Region;
        cubiertaTransicionCurso.Region = null;
        regionAnterior?.Dispose();
    }

    private void MostrarSubvistaCurso(
        Panel vista,
        VistaRutaAprendizaje? estado = null) {
        LimpiarHoverTarjetaCurso();

        foreach (Panel subvista in ObtenerVistasRutaAprendizaje()) {
            subvista.Visible = ReferenceEquals(vista, subvista);
        }

        vistaRutaActual = estado ?? ObtenerEstadoVista(vista);
        vista.BringToFront();
    }

    private void OcultarVistasCurso() {
        if (!cursoInicializado) {
            return;
        }

        LimpiarHoverTarjetaCurso();

        foreach (Panel vista in ObtenerVistasRutaAprendizaje()) {
            vista.Visible = false;
        }

        vistaRutaActual = VistaRutaAprendizaje.Ninguna;
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

        RecalcularDistribucionGrados();
        RecalcularDistribucionEvaluacion();

        if (!modoCursoInmersivo &&
            (distribucionPanelPrincipal == DistribucionPanelPrincipal.Curso ||
             panelCursoVista.Visible)) {
            RecalcularVistaPrincipalCurso();
            ActualizarGeometriaTarjetasTemas(volverAlInicio: false);
        }

        ActualizarGeometriaVistaPracticas();
        ActualizarGeometriaDetallePractica();
    }

    private void ActualizarGeometriaVistaPracticas() {
        if (panelPracticasTemaVista is null || panelPracticasTemaVista.IsDisposed) {
            return;
        }

        bool contextoEducativoInmersivo =
            vistaRutaActual is VistaRutaAprendizaje.DetalleGrado or
                VistaRutaAprendizaje.PracticasTema or
                VistaRutaAprendizaje.DetallePractica ||
            vistaRutaActual == VistaRutaAprendizaje.Evaluacion &&
                !evaluacionInmersivaAmpliaActiva;
        bool usarAnchoInmersivoAmplio =
            modoCursoInmersivo &&
            WindowState == FormWindowState.Maximized &&
            !rutaAprendizajeInmersivaActiva &&
            !evaluacionInmersivaAmpliaActiva &&
            contextoEducativoInmersivo;

        if (usarAnchoInmersivoAmplio) {
            Rectangle areaInmersiva = ClientRectangle;
            int margenIzquierdoContenedor = EscalarDiseno(48);
            int margenDerechoContenedor = EscalarDiseno(48);
            int anchoMaximoContenedor = EscalarDiseno(1500);
            int anchoContenedor = Math.Min(
                anchoMaximoContenedor,
                Math.Max(
                    1,
                    areaInmersiva.Width - margenIzquierdoContenedor -
                    margenDerechoContenedor));
            Rectangle limitesContenedor = new(
                areaInmersiva.Left + margenIzquierdoContenedor,
                panelPrincipal.Top,
                anchoContenedor,
                panelPrincipal.Height);

            if (panelPrincipal.Bounds != limitesContenedor) {
                panelPrincipal.Bounds = limitesContenedor;
                panelPrincipal.PerformLayout();
            }

            Rectangle limitesVista = panelPrincipal.ClientRectangle;
            if (panelPracticasTemaVista.Dock != DockStyle.None) {
                panelPracticasTemaVista.Dock = DockStyle.None;
            }

            if (panelPracticasTemaVista.Bounds != limitesVista) {
                panelPracticasTemaVista.Bounds = limitesVista;
            }
        } else if (panelPracticasTemaVista.Dock != DockStyle.Fill) {
            panelPracticasTemaVista.Dock = DockStyle.Fill;
        }

        Rectangle area = panelPracticasTemaVista.ClientRectangle;
        bool distribucionAmplia = area.Height >= EscalarDiseno(620);
        int margenHorizontal = usarAnchoInmersivoAmplio
            ? 0
            : EscalarDiseno(distribucionAmplia ? 16 : 12);
        int margenSuperior = EscalarDiseno(distribucionAmplia ? 4 : 8);
        int margenInferior = EscalarDiseno(distribucionAmplia ? 20 : 14);
        int anchoDisponible = Math.Max(1, area.Width - margenHorizontal * 2);
        int anchoContenido = Math.Min(
            anchoDisponible,
            EscalarDiseno(usarAnchoInmersivoAmplio ? 1500 : 1180));
        int xContenido = area.Left + margenHorizontal;
        int xTexto = xContenido + EscalarDiseno(4);
        int anchoTexto = Math.Max(1, anchoContenido - EscalarDiseno(8));
        int altoBotonVolver = EscalarDiseno(32);

        btnVolverTemasCurso.SetBounds(
            xContenido + EscalarDiseno(2),
            area.Top + margenSuperior,
            EscalarDiseno(182),
            altoBotonVolver);
        int yNumeroTema = btnVolverTemasCurso.Bottom +
            EscalarDiseno(distribucionAmplia ? 6 : 8);
        int altoNumeroTema = CalcularAltoTextoCurso(
            lblNumeroTemaCurso,
            anchoTexto,
            20);
        lblNumeroTemaCurso.SetBounds(
            xTexto,
            yNumeroTema,
            anchoTexto,
            altoNumeroTema);
        int yNombreTema = lblNumeroTemaCurso.Bottom - EscalarDiseno(2);
        int altoNombreTema = CalcularAltoTextoCurso(
            lblNombreTemaCurso,
            anchoTexto,
            distribucionAmplia ? 36 : 40);
        lblNombreTemaCurso.SetBounds(
            xTexto,
            yNombreTema,
            anchoTexto,
            altoNombreTema);
        int yDescripcionTema = lblNombreTemaCurso.Bottom - EscalarDiseno(1);
        int altoDescripcionTema = CalcularAltoTextoCurso(
            lblDescripcionTemaCurso,
            anchoTexto,
            distribucionAmplia ? 26 : 30);
        lblDescripcionTemaCurso.SetBounds(
            xTexto,
            yDescripcionTema,
            anchoTexto,
            altoDescripcionTema);
        int limiteInferior = Math.Max(area.Top + 1, area.Bottom - margenInferior);
        int yListaPracticas = Math.Min(
            lblDescripcionTemaCurso.Bottom +
                EscalarDiseno(distribucionAmplia ? 6 : 4),
            limiteInferior - 1);
        desplazamientoPracticasTema.SetBounds(
            xContenido,
            yListaPracticas,
            anchoContenido,
            Math.Max(1, limiteInferior - yListaPracticas));

        ActualizarGeometriaTarjetasPracticas(volverAlInicio: false);
    }

    private void ActualizarGeometriaDetallePractica() {
        if (panelDetallePracticaVista is null || panelDetallePracticaVista.IsDisposed) {
            return;
        }

        bool contextoEducativoInmersivo =
            vistaRutaActual is VistaRutaAprendizaje.DetalleGrado or
                VistaRutaAprendizaje.PracticasTema or
                VistaRutaAprendizaje.DetallePractica ||
            vistaRutaActual == VistaRutaAprendizaje.Evaluacion &&
                !evaluacionInmersivaAmpliaActiva;
        bool usarAnchoInmersivoAmplio =
            modoCursoInmersivo &&
            WindowState == FormWindowState.Maximized &&
            !rutaAprendizajeInmersivaActiva &&
            !evaluacionInmersivaAmpliaActiva &&
            contextoEducativoInmersivo;

        if (usarAnchoInmersivoAmplio) {
            Rectangle areaInmersiva = ClientRectangle;
            int margenIzquierdoContenedor = EscalarDiseno(48);
            int margenDerechoContenedor = EscalarDiseno(48);
            int anchoMaximoContenedor = EscalarDiseno(1500);
            int anchoContenedor = Math.Min(
                anchoMaximoContenedor,
                Math.Max(
                    1,
                    areaInmersiva.Width - margenIzquierdoContenedor -
                    margenDerechoContenedor));
            Rectangle limitesContenedor = new(
                areaInmersiva.Left + margenIzquierdoContenedor,
                panelPrincipal.Top,
                anchoContenedor,
                panelPrincipal.Height);

            if (panelPrincipal.Bounds != limitesContenedor) {
                panelPrincipal.Bounds = limitesContenedor;
                panelPrincipal.PerformLayout();
            }

            Rectangle limitesVista = panelPrincipal.ClientRectangle;
            if (panelDetallePracticaVista.Dock != DockStyle.None) {
                panelDetallePracticaVista.Dock = DockStyle.None;
            }

            if (panelDetallePracticaVista.Bounds != limitesVista) {
                panelDetallePracticaVista.Bounds = limitesVista;
            }
        } else if (panelDetallePracticaVista.Dock != DockStyle.Fill) {
            panelDetallePracticaVista.Dock = DockStyle.Fill;
        }

        Rectangle area = panelDetallePracticaVista.ClientRectangle;
        bool distribucionAmplia = area.Height >= EscalarDiseno(620);
        int margenHorizontal = usarAnchoInmersivoAmplio
            ? 0
            : EscalarDiseno(distribucionAmplia ? 16 : 12);
        int margenSuperior = EscalarDiseno(distribucionAmplia ? 4 : 8);
        int margenInferior = EscalarDiseno(distribucionAmplia ? 20 : 14);
        int anchoDisponible = Math.Max(1, area.Width - margenHorizontal * 2);
        int anchoContenido = Math.Min(
            anchoDisponible,
            EscalarDiseno(usarAnchoInmersivoAmplio ? 1500 : 1180));
        int xContenido = area.Left + margenHorizontal;
        int xTexto = xContenido + EscalarDiseno(4);
        int anchoTexto = Math.Max(1, anchoContenido - EscalarDiseno(8));
        int altoBotonVolver = EscalarDiseno(32);

        btnVolverPracticasCurso.SetBounds(
            xContenido + EscalarDiseno(2),
            area.Top + margenSuperior,
            EscalarDiseno(202),
            altoBotonVolver);
        int yNumeroPractica = btnVolverPracticasCurso.Bottom +
            EscalarDiseno(distribucionAmplia ? 6 : 8);
        int altoNumeroPractica = CalcularAltoTextoCurso(
            lblNumeroDetallePracticaCurso,
            anchoTexto,
            20);
        lblNumeroDetallePracticaCurso.SetBounds(
            xTexto,
            yNumeroPractica,
            anchoTexto,
            altoNumeroPractica);
        int yTituloPractica = lblNumeroDetallePracticaCurso.Bottom - EscalarDiseno(2);
        int altoTituloPractica = CalcularAltoTextoCurso(
            lblTituloDetallePracticaCurso,
            anchoTexto,
            distribucionAmplia ? 36 : 40);
        lblTituloDetallePracticaCurso.SetBounds(
            xTexto,
            yTituloPractica,
            anchoTexto,
            altoTituloPractica);
        int limiteInferior = Math.Max(area.Top + 1, area.Bottom - margenInferior);
        int yDetalle = Math.Min(
            lblTituloDetallePracticaCurso.Bottom +
                EscalarDiseno(distribucionAmplia ? 6 : 8),
            limiteInferior - 1);
        desplazamientoDetallePractica.SetBounds(
            xContenido,
            yDetalle,
            anchoContenido,
            Math.Max(1, limiteInferior - yDetalle));

        ActualizarGeometriaContenidoDetallePractica(volverAlInicio: false);
    }

    private void RecalcularVistaPrincipalCurso() {
        int anchoVista = Math.Max(1, panelCursoVista.ClientSize.Width);
        int altoVista = Math.Max(1, panelCursoVista.ClientSize.Height);
        int altoNavegacion = EscalarDiseno(50);
        btnVolverGradosCurso.SetBounds(
            EscalarDiseno(18),
            EscalarDiseno(12),
            EscalarDiseno(182),
            EscalarDiseno(32));
        int margenHorizontal = EscalarDiseno(18);
        int margenVertical = EscalarDiseno(12);
        int anchoDisponible = Math.Max(1, anchoVista - margenHorizontal * 2);
        int altoDisponible = Math.Max(
            1,
            altoVista - altoNavegacion - margenVertical * 2);
        int separacionColumnas = EscalarDiseno(24);
        int anchoMinimoPerfil = EscalarDiseno(300);
        int anchoMinimoTemas = EscalarDiseno(620);
        int anchoMinimoAmplio = anchoMinimoPerfil + separacionColumnas + anchoMinimoTemas;
        bool usarModoCompacto =
            anchoDisponible < anchoMinimoAmplio ||
            altoDisponible < EscalarDiseno(540);

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
        int yBloque = altoNavegacion +
            Math.Max(0, (altoVista - altoNavegacion - altoBloque) / 2);
        int altoTitulo = Math.Min(
            altoBloque,
            CalcularAltoTextoCurso(lblTituloTemasCurso, anchoTemas, 30));
        int separacionTitulo = Math.Min(
            EscalarDiseno(8),
            Math.Max(0, altoBloque - altoTitulo - 1));
        int altoLista = Math.Max(1, altoBloque - altoTitulo - separacionTitulo);
        int xTemas = xBloque + anchoPerfil + separacionColumnas;

        panelPerfilCurso.SetBounds(
            xBloque,
            yBloque,
            anchoPerfil,
            altoBloque);
        AjustarContenidoPerfilCursoAlEspacioDisponible();

        lblTituloTemasCurso.SetBounds(
            xTemas,
            yBloque,
            anchoTemas,
            altoTitulo);
        desplazamientoTemasCurso.SetBounds(
            xTemas,
            yBloque + altoTitulo + separacionTitulo,
            anchoTemas,
            altoLista);
    }

    private void RecalcularVistaPrincipalCursoCompacta(int anchoVista, int altoVista) {
        int yContenido = EscalarDiseno(50);
        desplazamientoCursoPrincipal.SetBounds(
            0,
            yContenido,
            anchoVista,
            Math.Max(1, altoVista - yContenido));
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

            listaTemasCurso.Width = anchoContenido;
            int altoTemas = CalcularAltoListaTemasCompacta();
            listaTemasCurso.Height = altoTemas;
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
        int anchoDisponible = Math.Max(1, listaTemasCurso.ClientSize.Width);
        int altoContenido = listaTemasCurso
            .GetPreferredSize(new Size(anchoDisponible, 0))
            .Height;

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

    private int AjustarContenidoPerfilCursoAlEspacioDisponible() {
        int altoNatural = AjustarContenidoPerfilCurso(
            compactarVerticalmente: false);

        if (cursoModoCompacto) {
            panelPerfilCurso.Height = altoNatural;
            return altoNatural;
        }

        if (altoNatural <= panelPerfilCurso.ClientSize.Height) {
            return altoNatural;
        }

        return AjustarContenidoPerfilCurso(
            compactarVerticalmente: true);
    }

    private int AjustarContenidoPerfilCurso(
        bool compactarVerticalmente = false) {
        int anchoPerfil = Math.Max(1, panelPerfilCurso.ClientSize.Width);
        int margenHorizontal = Math.Min(EscalarDiseno(26), Math.Max(EscalarDiseno(16), anchoPerfil / 13));
        int anchoContenido = Math.Max(1, anchoPerfil - margenHorizontal * 2);
        int y = EscalarDiseno(compactarVerticalmente ? 13 : 16);
        int espacioTrasTitulo = EscalarDiseno(compactarVerticalmente ? 1 : 2);
        int espacioTrasNivel = EscalarDiseno(compactarVerticalmente ? 4 : 6);
        int espacioTrasProgreso = EscalarDiseno(compactarVerticalmente ? 4 : 7);
        int espacioTrasBarra = EscalarDiseno(compactarVerticalmente ? 6 : 11);
        int espacioTrasContenido = EscalarDiseno(compactarVerticalmente ? 5 : 9);
        int espacioTrasResumen = EscalarDiseno(compactarVerticalmente ? 6 : 11);
        int espacioTrasSiguientePaso = EscalarDiseno(compactarVerticalmente ? 2 : 4);
        int espacioTrasUltimaPractica = EscalarDiseno(compactarVerticalmente ? 3 : 6);
        int espacioTrasContinuar = EscalarDiseno(compactarVerticalmente ? 6 : 10);
        int espacioTrasTituloRecomendacion = EscalarDiseno(compactarVerticalmente ? 2 : 4);
        int espacioTrasRecomendacion = EscalarDiseno(compactarVerticalmente ? 3 : 6);
        int espacioTrasVerPractica = EscalarDiseno(compactarVerticalmente ? 3 : 5);
        int margenInferior = EscalarDiseno(compactarVerticalmente ? 11 : 16);

        int altoTitulo = CalcularAltoTextoCurso(lblTituloPerfilCurso, anchoContenido, 34);
        lblTituloPerfilCurso.SetBounds(margenHorizontal, y, anchoContenido, altoTitulo);
        y = lblTituloPerfilCurso.Bottom + espacioTrasTitulo;

        int altoNombre = CalcularAltoTextoCurso(lblNombrePerfilCurso, anchoContenido, 24);
        lblNombrePerfilCurso.SetBounds(margenHorizontal, y, anchoContenido, altoNombre);
        y = lblNombrePerfilCurso.Bottom;

        int altoNivel = CalcularAltoTextoCurso(lblNivelPerfilCurso, anchoContenido, 22);
        lblNivelPerfilCurso.SetBounds(margenHorizontal, y, anchoContenido, altoNivel);
        y = lblNivelPerfilCurso.Bottom + espacioTrasNivel;

        int altoProgreso = CalcularAltoTextoCurso(lblProgresoGeneralCurso, anchoContenido, 42);
        lblProgresoGeneralCurso.SetBounds(margenHorizontal, y, anchoContenido, altoProgreso);
        y = lblProgresoGeneralCurso.Bottom + espacioTrasProgreso;

        int altoBarra = EscalarDiseno(9);
        panelFondoProgresoCurso.SetBounds(margenHorizontal, y, anchoContenido, altoBarra);
        panelRellenoProgresoCurso.Height = altoBarra;
        y = panelFondoProgresoCurso.Bottom + espacioTrasBarra;

        int altoContenidoGrado = CalcularAltoTextoCurso(
            lblContenidoGradoCurso,
            anchoContenido,
            42);
        lblContenidoGradoCurso.SetBounds(
            margenHorizontal,
            y,
            anchoContenido,
            altoContenidoGrado);
        y = lblContenidoGradoCurso.Bottom + espacioTrasContenido;

        int altoResumenTemas = CalcularAltoTextoCurso(
            lblResumenTemasCurso,
            anchoContenido,
            54);
        lblResumenTemasCurso.SetBounds(
            margenHorizontal,
            y,
            anchoContenido,
            altoResumenTemas);
        y = lblResumenTemasCurso.Bottom + espacioTrasResumen;

        int altoSiguientePaso = CalcularAltoTextoCurso(lblSiguientePasoCurso, anchoContenido, 18);
        lblSiguientePasoCurso.SetBounds(margenHorizontal, y, anchoContenido, altoSiguientePaso);
        y = lblSiguientePasoCurso.Bottom + espacioTrasSiguientePaso;

        int altoUltimaPractica = CalcularAltoTextoCurso(lblUltimaPracticaCurso, anchoContenido, 50);
        lblUltimaPracticaCurso.SetBounds(margenHorizontal, y, anchoContenido, altoUltimaPractica);
        y = lblUltimaPracticaCurso.Bottom + espacioTrasUltimaPractica;

        int altoBoton = EscalarDiseno(34);

        if (btnContinuarAprendizaje.Visible) {
            btnContinuarAprendizaje.SetBounds(
                margenHorizontal, y,
                anchoContenido,
                altoBoton);

            y = btnContinuarAprendizaje.Bottom + espacioTrasContinuar;
        } else {
            btnContinuarAprendizaje.SetBounds(
                margenHorizontal,
                y,
                anchoContenido,
                0);
        }

        int altoTituloRecomendacion = CalcularAltoTextoCurso(
            lblTituloRecomendacionCurso,
            anchoContenido,
            18);
        lblTituloRecomendacionCurso.SetBounds(
            margenHorizontal,
            y,
            anchoContenido,
            altoTituloRecomendacion);
        y = lblTituloRecomendacionCurso.Bottom + espacioTrasTituloRecomendacion;

        int altoRecomendacion = CalcularAltoTextoCurso(
            lblPracticaRecomendadaCurso,
            anchoContenido,
            50);
        lblPracticaRecomendadaCurso.SetBounds(
            margenHorizontal,
            y,
            anchoContenido,
            altoRecomendacion);
        y = lblPracticaRecomendadaCurso.Bottom + espacioTrasRecomendacion;

        btnVerPracticaRecomendadaCurso.SetBounds(margenHorizontal, y, anchoContenido, altoBoton);
        y = btnVerPracticaRecomendadaCurso.Bottom + espacioTrasVerPractica;

        if (lblAvisoProgresoCurso.Visible) {
            int altoAviso = CalcularAltoTextoCurso(
                lblAvisoProgresoCurso,
                anchoContenido,
                29);

            lblAvisoProgresoCurso.SetBounds(
                margenHorizontal,
                y,
                anchoContenido,
                altoAviso);

            y = lblAvisoProgresoCurso.Bottom;
        } else {
            lblAvisoProgresoCurso.SetBounds(
                margenHorizontal,
                y,
                anchoContenido,
                0);
        }

        ActualizarAnchoProgresoGeneral(ContarPracticasDisponiblesRealizadas());
        return y + margenInferior;
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
        GradoCurso? grado = gradosService.ObtenerGrado(
            GradosService.GradoFundamentosId,
            progresoCurso);
        int totalPlaneadas = grado?.CantidadPracticasPlaneadas ??
            GradosService.MetaCurricularPracticasGradoFundamentos;
        int porcentajeDisponible = totalDisponibles == 0
            ? 0
            : Math.Clamp(
                (int)Math.Round(realizadas * 100D / totalDisponibles),
                0,
                100);
        TemaCurso[] temasDisponibles = cursoService.CargarTemas()
            .Where(tema => !tema.EsProximamente && tema.Practicas.Count > 0)
            .ToArray();
        int temasIniciados = temasDisponibles.Count(tema =>
            tema.Practicas.Any(practica =>
                ObtenerEstadoPractica(practica.Id) != EstadoPracticaCurso.Pendiente));
        int temasCompletados = temasDisponibles.Count(tema =>
            tema.Practicas.All(practica =>
                ObtenerEstadoPractica(practica.Id) == EstadoPracticaCurso.Realizada));

        bool contenidoDisponibleCompletado =
            totalDisponibles > 0 && realizadas >= totalDisponibles;
        string estadoProgreso = contenidoDisponibleCompletado
            ? totalDisponibles < totalPlaneadas
                ? "Contenido disponible completado"
                : "Grado completado"
            : $"{porcentajeDisponible}% del contenido disponible";
        lblProgresoGeneralCurso.Text =
            $"PROGRESO DISPONIBLE\n{realizadas} de {totalDisponibles} prácticas realizadas\n" +
            estadoProgreso;
        lblContenidoGradoCurso.Text =
            $"CONTENIDO DEL GRADO\n{totalDisponibles} de {totalPlaneadas} prácticas publicadas";
        lblResumenTemasCurso.Text =
            $"TEMAS DEL GRADO\n{temasIniciados} iniciados\n{temasCompletados} completados";
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
        AjustarContenidoPerfilCursoAlEspacioDisponible();
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
            GradoCurso? grado = gradosService.ObtenerGrado(
                GradosService.GradoFundamentosId,
                progresoCurso);
            bool faltaContenidoPorPublicar =
                grado is not null &&
                grado.CantidadPracticasDisponibles < grado.CantidadPracticasPlaneadas;
            lblPracticaRecomendadaCurso.Text = hayContenido
                ? faltaContenidoPorPublicar
                    ? "Contenido disponible completado"
                    : "Grado completado"
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
            (MostrarTiemposOrientativos
                ? $"{NormalizarDificultad(practica.Dificultad)} · {practica.DuracionEstimada}"
                : NormalizarDificultad(practica.Dificultad));
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

    private void ReconstruirTarjetasTemas(
        bool volverAlInicio = true,
        bool actualizarGeometriaSiVigente = true) {
        IReadOnlyList<TemaCurso> temas = cursoService.CargarTemas();
        EstadoContenidoTema[] estadoActual = CapturarEstadoTarjetasTemas(temas);
        bool contenidoVigente =
            listaTemasCurso.Controls.Count == temas.Count &&
            listaTemasCurso.Controls.OfType<Panel>().All(tarjeta =>
                tarjeta.Tag is PresentacionTarjetaTema) &&
            estadoTarjetasTemas.SequenceEqual(estadoActual);

        if (contenidoVigente) {
            if (actualizarGeometriaSiVigente) {
                ActualizarGeometriaTarjetasTemas(volverAlInicio);
            }

            return;
        }

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

            foreach (TemaCurso tema in temas) {
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

        estadoTarjetasTemas = estadoActual;
    }

    private void AsegurarTarjetasTemasVigentes(
        bool volverAlInicio,
        bool actualizarGeometriaSiVigente = true) {
        ReconstruirTarjetasTemas(
            volverAlInicio,
            actualizarGeometriaSiVigente);
    }

    private EstadoContenidoTema[] CapturarEstadoTarjetasTemas(
        IReadOnlyList<TemaCurso> temas) {
        return temas
            .Select(tema => {
                int realizadas = ContarPracticasRealizadas(tema);
                int porcentaje = (int)Math.Round(
                    realizadas * 100D /
                    Math.Max(1, tema.TotalPracticasPlaneadas));

                return new EstadoContenidoTema(
                    tema.Id,
                    tema.Numero,
                    tema.Nombre,
                    tema.Descripcion,
                    tema.EsProximamente,
                    tema.MensajeDisponibilidad,
                    tema.TotalPracticasPlaneadas,
                    realizadas,
                    porcentaje,
                    ObtenerEstadoTema(tema, realizadas),
                    CrearFirmaContenidoPracticas(tema.Practicas));
            })
            .ToArray();
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

    private void ReconstruirTarjetasPracticas(
        TemaCurso tema,
        bool volverAlInicio = true,
        bool actualizarGeometriaSiVigente = true) {
        PracticaCurso[] practicas = tema.Practicas
            .OrderBy(item => item.Numero)
            .ToArray();
        EstadoContenidoPractica[] estadoActual =
            CapturarEstadoTarjetasPracticas(practicas);
        bool contenidoVigente =
            temaTarjetasPracticasId.Equals(
                tema.Id,
                StringComparison.OrdinalIgnoreCase) &&
            listaPracticasTema.Controls.Count == practicas.Length &&
            listaPracticasTema.Controls.OfType<Panel>().All(tarjeta =>
                tarjeta.Tag is PresentacionTarjetaPractica) &&
            estadoTarjetasPracticas.SequenceEqual(estadoActual);

        if (contenidoVigente) {
            if (actualizarGeometriaSiVigente) {
                ActualizarGeometriaTarjetasPracticas(volverAlInicio);
            }

            return;
        }

        listaPracticasTema.SuspendLayout();

        try {
            VaciarYDisponerControles(listaPracticasTema);
            int anchoTarjeta = Math.Max(
                1,
                listaPracticasTema.ClientSize.Width -
                listaPracticasTema.Padding.Horizontal -
                EscalarDiseno(4));

            foreach (PracticaCurso practica in practicas) {
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
                    (MostrarTiemposOrientativos
                        ? $"Tiempo orientativo {practica.DuracionEstimada}. "
                        : string.Empty) +
                    $"Estado {ObtenerTextoEstadoPractica(estadoPractica)}.";

                Label numero = CrearLabelCurso(
                    practica.Numero.ToString("00"),
                    Point.Empty,
                    new Size(1, 1),
                    TamanoFuenteCurso.NumeroTarjeta,
                    FontStyle.Bold,
                    ColorMoradoClaroCurso,
                    ContentAlignment.TopLeft);
                Label nombre = CrearLabelCurso(
                    practica.Nombre,
                    Point.Empty,
                    new Size(1, 1),
                    TamanoFuenteCurso.TituloTarjetaPractica,
                    FontStyle.Bold,
                    Color.White,
                    ContentAlignment.TopLeft);
                Label descripcion = CrearLabelCurso(
                    practica.Objetivo,
                    Point.Empty,
                    new Size(1, 1),
                    TamanoFuenteCurso.DescripcionTarjetaPractica,
                    FontStyle.Regular,
                    ColorTextoSecundarioCurso,
                    ContentAlignment.TopLeft);
                Label estado = CrearLabelCurso(
                    ObtenerTextoEstadoPractica(estadoPractica),
                    Point.Empty,
                    new Size(1, 1),
                    TamanoFuenteCurso.EstadoTarjetaPractica,
                    FontStyle.Bold,
                    ObtenerColorEstado(estadoPractica),
                    ContentAlignment.MiddleRight);
                Label metadatos = CrearLabelCurso(
                    MostrarTiemposOrientativos
                        ? $"{NormalizarDificultad(practica.Dificultad)} · {practica.DuracionEstimada}"
                        : NormalizarDificultad(practica.Dificultad),
                    Point.Empty,
                    new Size(1, 1),
                    TamanoFuenteCurso.MetadatoTarjetaPractica,
                    FontStyle.Regular,
                    ColorTextoSecundarioCurso,
                    ContentAlignment.MiddleRight);

                tarjeta.Controls.Add(numero);
                tarjeta.Controls.Add(nombre);
                tarjeta.Controls.Add(descripcion);
                tarjeta.Controls.Add(estado);
                tarjeta.Controls.Add(metadatos);
                PresentacionTarjetaPractica presentacion = new(
                    numero,
                    nombre,
                    descripcion,
                    estado,
                    metadatos);
                tarjeta.Tag = presentacion;
                AplicarGeometriaTarjetaPractica(tarjeta, presentacion, anchoTarjeta);
                ConfigurarInteraccionTarjeta(tarjeta, () => MostrarDetallePractica(practica));
                listaPracticasTema.Controls.Add(tarjeta);
            }
        } finally {
            listaPracticasTema.ResumeLayout(performLayout: true);
            ultimoAnchoTarjetasPracticas = Math.Max(
                1,
                listaPracticasTema.ClientSize.Width -
                listaPracticasTema.Padding.Horizontal -
                EscalarDiseno(4));
            ultimoDpiTarjetasPracticas = DeviceDpi;
            desplazamientoPracticasTema.ActualizarContenido(volverAlInicio);
        }

        temaTarjetasPracticasId = tema.Id;
        estadoTarjetasPracticas = estadoActual;
    }

    private void AsegurarTarjetasPracticasVigentes(
        TemaCurso tema,
        bool volverAlInicio,
        bool actualizarGeometriaSiVigente = true) {
        ReconstruirTarjetasPracticas(
            tema,
            volverAlInicio,
            actualizarGeometriaSiVigente);
    }

    private EstadoContenidoPractica[] CapturarEstadoTarjetasPracticas(
        IReadOnlyList<PracticaCurso> practicas) {
        return practicas
            .Select(practica => new EstadoContenidoPractica(
                practica.Id,
                practica.TemaId,
                practica.Numero,
                practica.Nombre,
                practica.Objetivo,
                practica.Dificultad,
                practica.DuracionEstimada,
                ObtenerEstadoPractica(practica.Id),
                MostrarTiemposOrientativos,
                CrearFirmaContenidoPractica(practica)))
            .ToArray();
    }

    private static string CrearFirmaContenidoPracticas(
        IReadOnlyList<PracticaCurso> practicas) {
        return string.Join(
            "\u001e",
            practicas
                .OrderBy(practica => practica.Numero)
                .Select(CrearFirmaContenidoPractica));
    }

    private static string CrearFirmaContenidoPractica(PracticaCurso practica) {
        return string.Join(
            "\u001f",
            practica.Id,
            practica.TemaId,
            practica.Numero,
            practica.Nombre,
            practica.Objetivo,
            practica.Descripcion,
            string.Join("\u001d", practica.Conceptos),
            string.Join("\u001d", practica.Instrucciones),
            practica.ResultadoEsperado,
            practica.Dificultad,
            practica.DuracionEstimada,
            string.Join("\u001d", practica.RequisitosPrevios),
            practica.EstadoInicial,
            practica.NombreProyecto);
    }

    private void ActualizarGeometriaTarjetasPracticas(bool volverAlInicio) {
        if (listaPracticasTema is null || listaPracticasTema.IsDisposed) {
            return;
        }

        int anchoTarjeta = Math.Max(
            1,
            listaPracticasTema.ClientSize.Width -
            listaPracticasTema.Padding.Horizontal -
            EscalarDiseno(4));
        bool geometriaCambio =
            anchoTarjeta != ultimoAnchoTarjetasPracticas ||
            DeviceDpi != ultimoDpiTarjetasPracticas;

        if (geometriaCambio && listaPracticasTema.Controls.Count > 0) {
            listaPracticasTema.SuspendLayout();

            try {
                foreach (Panel tarjeta in listaPracticasTema.Controls.OfType<Panel>()) {
                    if (tarjeta.Tag is PresentacionTarjetaPractica presentacion) {
                        AplicarGeometriaTarjetaPractica(tarjeta, presentacion, anchoTarjeta);
                    }
                }
            } finally {
                listaPracticasTema.ResumeLayout(performLayout: true);
            }
        }

        ultimoAnchoTarjetasPracticas = anchoTarjeta;
        ultimoDpiTarjetasPracticas = DeviceDpi;
        desplazamientoPracticasTema.ActualizarContenido(volverAlInicio);
    }

    private void AplicarGeometriaTarjetaPractica(
        Panel tarjeta,
        PresentacionTarjetaPractica presentacion,
        int anchoTarjeta) {
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
        int ySuperior = EscalarDiseno(10);
        int altoNumero = CalcularAltoTextoCurso(
            presentacion.Numero,
            anchoNumero,
            26);
        int altoNombre = CalcularAltoTextoCurso(
            presentacion.Nombre,
            anchoTexto,
            28);
        int yDescripcion = ySuperior + altoNombre + EscalarDiseno(3);
        int altoDescripcion = CalcularAltoTextoCurso(
            presentacion.Descripcion,
            anchoTexto,
            35);
        int altoEstado = CalcularAltoTextoCurso(
            presentacion.Estado,
            anchoMetadatos,
            25);
        int yMetadatos = ySuperior + altoEstado + EscalarDiseno(3);
        int altoMetadatos = CalcularAltoTextoCurso(
            presentacion.Metadatos,
            anchoMetadatos,
            24);
        int altoTarjeta = Math.Max(
            yDescripcion + altoDescripcion,
            yMetadatos + altoMetadatos) + EscalarDiseno(12);

        tarjeta.SuspendLayout();

        try {
            tarjeta.Size = new Size(anchoTarjeta, altoTarjeta);
            presentacion.Numero.SetBounds(
                margenIzquierdo,
                ySuperior,
                anchoNumero,
                altoNumero);
            presentacion.Nombre.SetBounds(xTexto, ySuperior, anchoTexto, altoNombre);
            presentacion.Descripcion.SetBounds(
                xTexto,
                yDescripcion,
                anchoTexto,
                altoDescripcion);
            presentacion.Estado.SetBounds(
                xMetadatos,
                ySuperior,
                anchoMetadatos,
                altoEstado);
            presentacion.Metadatos.SetBounds(
                xMetadatos,
                yMetadatos,
                anchoMetadatos,
                altoMetadatos);
        } finally {
            tarjeta.ResumeLayout(performLayout: false);
        }

        tarjeta.Invalidate();
    }

    private void ActualizarGeometriaContenidoDetallePractica(bool volverAlInicio) {
        if (contenidoDetallePractica is null || contenidoDetallePractica.IsDisposed) {
            return;
        }

        int anchoContenido = Math.Max(
            1,
            contenidoDetallePractica.ClientSize.Width -
            contenidoDetallePractica.Padding.Horizontal -
            EscalarDiseno(4));
        bool geometriaCambio =
            anchoContenido != ultimoAnchoContenidoDetallePractica ||
            DeviceDpi != ultimoDpiContenidoDetallePractica;

        if (geometriaCambio && contenidoDetallePractica.Controls.Count > 0) {
            contenidoDetallePractica.SuspendLayout();

            try {
                foreach (Control control in contenidoDetallePractica.Controls) {
                    switch (control) {
                        case Panel panel when panel.Tag is PresentacionSeccionDetalle seccion:
                            AplicarGeometriaSeccionDetalle(panel, seccion, anchoContenido);
                            break;
                        case Panel panel when panel.Tag is PresentacionAccionesDetalle acciones:
                            AplicarGeometriaAccionesDetalle(panel, acciones, anchoContenido);
                            break;
                        case Label etiqueta when etiqueta.AutoSize:
                            etiqueta.MinimumSize = new Size(anchoContenido, 0);
                            etiqueta.MaximumSize = new Size(anchoContenido, 0);
                            break;
                        default:
                            control.Width = anchoContenido;
                            break;
                    }
                }
            } finally {
                contenidoDetallePractica.ResumeLayout(performLayout: true);
            }
        }

        ultimoAnchoContenidoDetallePractica = anchoContenido;
        ultimoDpiContenidoDetallePractica = DeviceDpi;
        desplazamientoDetallePractica.ActualizarContenido(volverAlInicio);
    }

    private void AplicarGeometriaSeccionDetalle(
        Panel tarjeta,
        PresentacionSeccionDetalle presentacion,
        int ancho) {
        int margenHorizontal = EscalarDiseno(16);
        int margenSuperior = EscalarDiseno(12);
        int margenInferior = EscalarDiseno(16);
        int separacion = EscalarDiseno(6);
        int anchoInterior = Math.Max(1, ancho - margenHorizontal * 2);
        int altoTitulo = CalcularAltoTextoCurso(
            presentacion.Encabezado,
            anchoInterior,
            24);
        int altoContenido = CalcularAltoTextoCurso(
            presentacion.Texto,
            anchoInterior,
            28);
        int altoTarjeta =
            margenSuperior + altoTitulo + separacion + altoContenido + margenInferior;

        tarjeta.SuspendLayout();

        try {
            tarjeta.Size = new Size(ancho, altoTarjeta);
            presentacion.Acento.SetBounds(
                0,
                0,
                EscalarDiseno(4),
                altoTarjeta);
            presentacion.Encabezado.SetBounds(
                margenHorizontal,
                margenSuperior,
                anchoInterior,
                altoTitulo);
            presentacion.Texto.SetBounds(
                margenHorizontal,
                margenSuperior + altoTitulo + separacion,
                anchoInterior,
                altoContenido);
        } finally {
            tarjeta.ResumeLayout(performLayout: false);
        }

        tarjeta.Invalidate();
    }

    private void AplicarGeometriaAccionesDetalle(
        Panel panel,
        PresentacionAccionesDetalle presentacion,
        int ancho) {
        int separacion = EscalarDiseno(14);
        int altoBoton = EscalarDiseno(38);
        bool apilarBotones = ancho < EscalarDiseno(360);
        int anchoBoton = apilarBotones
            ? ancho
            : Math.Max(1, (ancho - separacion) / 2);
        int altoPanel = apilarBotones
            ? altoBoton * 2 + separacion
            : altoBoton;

        panel.Size = new Size(ancho, altoPanel);
        presentacion.Realizada.SetBounds(0, 0, anchoBoton, altoBoton);
        presentacion.Pendiente.SetBounds(
            apilarBotones ? 0 : anchoBoton + separacion,
            apilarBotones ? altoBoton + separacion : 0,
            anchoBoton,
            altoBoton);
    }

    private void AsegurarDetallePracticaVigente(PracticaCurso practica) {
        ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);
        string rutaProyecto = progreso?.RutaProyecto ?? string.Empty;
        bool rutaProyectoExiste =
            !string.IsNullOrWhiteSpace(rutaProyecto) &&
            Directory.Exists(rutaProyecto);
        bool contenidoVigente =
            contenidoDetallePractica.Controls.Count > 0 &&
            ReferenceEquals(practicaDetalleConstruida, practica) &&
            estadoPracticaDetalleConstruida == ObtenerEstadoPractica(practica.Id) &&
            rutaProyectoDetalleConstruida.Equals(
                rutaProyecto,
                StringComparison.OrdinalIgnoreCase) &&
            rutaProyectoDetalleExistia == rutaProyectoExiste &&
            mostrarTiemposDetalleConstruido == MostrarTiemposOrientativos &&
            evaluacionEnCursoDetalleConstruida == evaluacionEnCurso;

        if (contenidoVigente) {
            desplazamientoDetallePractica.ActualizarContenido(volverAlInicio: true);
            return;
        }

        ReconstruirDetallePractica(practica);
    }

    private void RegistrarEstadoDetallePractica(PracticaCurso practica) {
        ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);
        string rutaProyecto = progreso?.RutaProyecto ?? string.Empty;

        practicaDetalleConstruida = practica;
        estadoPracticaDetalleConstruida = ObtenerEstadoPractica(practica.Id);
        rutaProyectoDetalleConstruida = rutaProyecto;
        rutaProyectoDetalleExistia =
            !string.IsNullOrWhiteSpace(rutaProyecto) &&
            Directory.Exists(rutaProyecto);
        mostrarTiemposDetalleConstruido = MostrarTiemposOrientativos;
        evaluacionEnCursoDetalleConstruida = evaluacionEnCurso;
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
            if (MostrarTiemposOrientativos) {
                AgregarSeccionDetalle(
                    contenidoDetallePractica,
                    "Tiempo orientativo",
                    string.IsNullOrWhiteSpace(practica.DuracionEstimada)
                        ? "Tiempo por definir"
                        : practica.DuracionEstimada,
                    anchoContenido);
            }

            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Ritmo de aprendizaje",
                "Avanza a tu propio ritmo.",
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

            AgregarControlesEvaluacionPractica(practica, anchoContenido);

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
            panelAccionesEstado.Tag = new PresentacionAccionesDetalle(
                btnRealizada,
                btnPendiente);
            contenidoDetallePractica.Controls.Add(panelAccionesEstado);
        } finally {
            contenidoDetallePractica.ResumeLayout(performLayout: true);
            ultimoAnchoContenidoDetallePractica = Math.Max(
                1,
                contenidoDetallePractica.ClientSize.Width -
                contenidoDetallePractica.Padding.Horizontal -
                EscalarDiseno(4));
            ultimoDpiContenidoDetallePractica = DeviceDpi;
            desplazamientoDetallePractica.ActualizarContenido(volverAlInicio);
        }

        RegistrarEstadoDetallePractica(practica);
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

        bool enProgreso = tema.Practicas.Any(practica =>
            ObtenerEstadoPractica(practica.Id) != EstadoPracticaCurso.Pendiente
        );

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

        if (tarjeta is TarjetaCursoInteractiva tarjetaInteractiva) {
            tarjeta.AccessibleRole = AccessibleRole.PushButton;
            tarjetaInteractiva.RadioEsquinas = radio;
            tarjetaInteractiva.ColorBordeNormal = ColorBordeCurso;
            tarjetaInteractiva.ColorBordeHover = ColorBordeHoverCurso;
            tarjetaInteractiva.ColorBordeFoco = ColorMoradoClaroCurso;
        }

        AplicarRegionRedondeada(tarjeta, radio);
        tarjeta.SizeChanged += (_, _) => AplicarRegionRedondeada(tarjeta, radio);

        if (tarjeta is not TarjetaCursoInteractiva) {
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
        }

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

    private void ConfigurarInteraccionTarjeta(Panel tarjeta, Action alHacerClick) {
        tarjeta.AccessibleRole = AccessibleRole.PushButton;

        if (tarjeta is not TarjetaCursoInteractiva tarjetaInteractiva) {
            return;
        }

        foreach (Control control in ObtenerArbolControles(tarjeta)) {
            control.Cursor = Cursors.Hand;
            control.MouseClick += (_, e) => {
                if (e.Button == MouseButtons.Left) {
                    alHacerClick();
                }
            };
            control.MouseEnter += (_, _) =>
                ActivarHoverTarjetaCurso(tarjetaInteractiva);
            control.MouseLeave += (_, _) => {
                if (!TarjetaVisibleContieneCursor(tarjetaInteractiva)) {
                    DesactivarHoverTarjetaCurso(tarjetaInteractiva);
                }
            };
        }

        tarjetaInteractiva.ActivadaPorTeclado += (_, _) => alHacerClick();
    }

    private void ActivarHoverTarjetaCurso(TarjetaCursoInteractiva tarjeta) {
        if (tarjeta.IsDisposed || !tarjeta.Visible) {
            return;
        }

        TarjetaCursoInteractiva? tarjetaAnterior = tarjetaCursoConHover;

        if (tarjetaAnterior is not null &&
            !ReferenceEquals(tarjetaAnterior, tarjeta)) {
            AplicarEstadoVisualTarjetaCurso(tarjetaAnterior, tieneHover: false);
        }

        AplicarEstadoVisualTarjetaCurso(tarjeta, tieneHover: true);
        tarjetaCursoConHover = tarjeta;
    }

    private void DesactivarHoverTarjetaCurso(TarjetaCursoInteractiva tarjeta) {
        if (!tarjeta.IsDisposed && TarjetaVisibleContieneCursor(tarjeta)) {
            return;
        }

        AplicarEstadoVisualTarjetaCurso(tarjeta, tieneHover: false);

        if (ReferenceEquals(tarjetaCursoConHover, tarjeta)) {
            tarjetaCursoConHover = null;
        }
    }

    private void LimpiarHoverTarjetaCurso() {
        TarjetaCursoInteractiva? tarjetaActiva = tarjetaCursoConHover;
        tarjetaCursoConHover = null;

        if (tarjetaActiva is not null) {
            AplicarEstadoVisualTarjetaCurso(tarjetaActiva, tieneHover: false);
        }
    }

    private void LimpiarHoverTarjetaCursoEnContenedor(Control contenedor) {
        if (tarjetaCursoConHover is not null &&
            EsMismoControlODescendiente(contenedor, tarjetaCursoConHover)) {
            LimpiarHoverTarjetaCurso();
        }
    }

    private static bool EsMismoControlODescendiente(
        Control contenedor,
        Control control) {
        for (Control? actual = control; actual is not null; actual = actual.Parent) {
            if (ReferenceEquals(actual, contenedor)) {
                return true;
            }
        }

        return false;
    }

    private static void AplicarEstadoVisualTarjetaCurso(
        TarjetaCursoInteractiva tarjeta,
        bool tieneHover) {
        if (tarjeta.IsDisposed) {
            return;
        }

        tarjeta.EstaEnHover = tieneHover;
    }

    private void RestablecerEstadosVisualesTarjetasTemas(bool aplicarHoverReal = true) {
        RestablecerEstadosVisualesTarjetas(listaTemasCurso, aplicarHoverReal);
    }

    private void RestablecerEstadosVisualesTarjetasPracticas(bool aplicarHoverReal = true) {
        RestablecerEstadosVisualesTarjetas(listaPracticasTema, aplicarHoverReal);
    }

    private void RestablecerEstadosVisualesTarjetasGrados(bool aplicarHoverReal = true) {
        RestablecerEstadosVisualesTarjetas(contenidoGrados, aplicarHoverReal);
    }

    private void RestablecerEstadosVisualesVistaActualCurso(
        bool aplicarHoverReal = false) {
        if (!cursoInicializado) {
            return;
        }

        if (panelGradosVista.Visible) {
            RestablecerEstadosVisualesTarjetasGrados(aplicarHoverReal);
        } else if (panelCursoVista.Visible) {
            RestablecerEstadosVisualesTarjetasTemas(aplicarHoverReal);
        } else if (panelPracticasTemaVista.Visible) {
            RestablecerEstadosVisualesTarjetasPracticas(aplicarHoverReal);
        }
    }

    private void RestablecerEstadosVisualesTarjetas(
        Control? contenedor,
        bool aplicarHoverReal) {
        if (contenedor is null || contenedor.IsDisposed) {
            return;
        }

        LimpiarHoverTarjetaCursoEnContenedor(contenedor);

        TarjetaCursoInteractiva[] tarjetas = ObtenerArbolControles(contenedor)
            .OfType<TarjetaCursoInteractiva>()
            .ToArray();

        foreach (TarjetaCursoInteractiva tarjeta in tarjetas) {
            AplicarEstadoVisualTarjetaCurso(tarjeta, tieneHover: false);
        }

        if (!aplicarHoverReal || !contenedor.Visible) {
            return;
        }

        TarjetaCursoInteractiva? tarjetaBajoCursor = tarjetas.FirstOrDefault(
            TarjetaVisibleContieneCursor);

        if (tarjetaBajoCursor is not null) {
            ActivarHoverTarjetaCurso(tarjetaBajoCursor);
        }
    }

    private static bool TarjetaVisibleContieneCursor(TarjetaCursoInteractiva tarjeta) {
        if (!tarjeta.Visible || tarjeta.IsDisposed || tarjeta.ClientSize.IsEmpty) {
            return false;
        }

        Rectangle areaVisible = tarjeta.RectangleToScreen(tarjeta.ClientRectangle);
        Control? ancestro = tarjeta.Parent;

        while (ancestro is not null) {
            if (!ancestro.Visible || ancestro.IsDisposed || ancestro.ClientSize.IsEmpty) {
                return false;
            }

            areaVisible = Rectangle.Intersect(
                areaVisible,
                ancestro.RectangleToScreen(ancestro.ClientRectangle));

            if (areaVisible.IsEmpty) {
                return false;
            }

            ancestro = ancestro.Parent;
        }

        return areaVisible.Contains(Cursor.Position);
    }

    private static IEnumerable<Control> ObtenerArbolControles(Control raiz) {
        yield return raiz;

        foreach (Control hijo in raiz.Controls) {
            foreach (Control descendiente in ObtenerArbolControles(hijo)) {
                yield return descendiente;
            }
        }
    }

    private void VaciarYDisponerControles(Control contenedor) {
        LimpiarHoverTarjetaCursoEnContenedor(contenedor);
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

        if (ReferenceEquals(contenedor, contenidoDetallePractica)) {
            tarjeta.Tag = new PresentacionSeccionDetalle(acento, encabezado, texto);
        }

        contenedor.Controls.Add(tarjeta);
    }
}
