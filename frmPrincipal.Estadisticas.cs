using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;
using System.Globalization;
using System.Text;

namespace EndForge;

public partial class frmPrincipal {
    private const string SegmentoLlenoEstadisticas = "🟩";
    private const string SegmentoVacioEstadisticas = "⬜";

    private sealed class BarraSegmentadaEstadisticas : Label {
        public BarraSegmentadaEstadisticas() {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e) {
            TextFormatFlags formato =
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.SingleLine |
                TextFormatFlags.VerticalCenter;
            int x = 0;

            foreach (Rune rune in Text.EnumerateRunes()) {
                string segmento = rune.ToString();
                Color color = segmento == SegmentoLlenoEstadisticas
                    ? ColorVerdeEstadisticas
                    : segmento == SegmentoVacioEstadisticas
                        ? Color.FromArgb(232, 232, 235)
                        : ForeColor;
                Size medida = TextRenderer.MeasureText(
                    e.Graphics,
                    segmento,
                    Font,
                    Size.Empty,
                    formato);
                int ancho = Math.Max(1, medida.Width);
                TextRenderer.DrawText(
                    e.Graphics,
                    segmento,
                    Font,
                    new Rectangle(x, 0, ancho, ClientSize.Height),
                    color,
                    formato);
                x += ancho;
            }
        }
    }

    private sealed class PresentacionMetricaEstadistica {
        public PresentacionMetricaEstadistica(
            Panel tarjeta,
            Label titulo,
            Label valor,
            Label descripcion) {
            Tarjeta = tarjeta;
            Titulo = titulo;
            Valor = valor;
            Descripcion = descripcion;
        }

        public Panel Tarjeta { get; }
        public Label Titulo { get; }
        public Label Valor { get; }
        public Label Descripcion { get; }
    }

    private sealed class PresentacionDominioTemaEstadistica {
        public PresentacionDominioTemaEstadistica(
            Panel fila,
            Label tema,
            Label barra,
            Label valor,
            Label nivel) {
            Fila = fila;
            Tema = tema;
            Barra = barra;
            Valor = valor;
            Nivel = nivel;
        }

        public Panel Fila { get; }
        public Label Tema { get; }
        public Label Barra { get; }
        public Label Valor { get; }
        public Label Nivel { get; }
    }

    private sealed record DatosDominioTemaEstadistica(
        string Tema,
        int Valor,
        int Maximo,
        string Nivel);

    private sealed record DatosVistaEstadisticas(
        int PracticasCompletadas,
        int TotalPracticas,
        int? PromedioGeneral,
        long IntentosRealizados,
        string UltimaActividad,
        int ProgresoGeneral,
        int MaximoProgresoGeneral,
        IReadOnlyList<DatosDominioTemaEstadistica> DominioTemas,
        string MejorCalificacion,
        string TiempoPromedioPractica,
        string TiempoTotalProgramando,
        string TemaMasPracticado);

    private readonly record struct ResultadoBarraEstadistica(
        string Segmentos,
        string TextoNumerico,
        int ValorNormalizado,
        int SegmentosActivos);

    private static readonly Color ColorFondoEstadisticas = Color.FromArgb(18, 14, 27);
    private static readonly Color ColorFilaEstadisticas = Color.FromArgb(30, 25, 43);
    private static readonly Color ColorVerdeEstadisticas = Color.FromArgb(100, 214, 144);
    private static readonly Color ColorVerdeClaroEstadisticas = Color.FromArgb(150, 229, 177);
    private static readonly Color ColorCalidoEstadisticas = Color.FromArgb(235, 181, 89);

    private bool estructuraEstadisticasInicializada;
    private bool vistaEstadisticasConstruida;
    private int ultimoAnchoContenidoEstadisticas = -1;
    private int ultimoDpiEstadisticas = -1;
    private bool ultimoModoAmplioEstadisticas;
    private readonly ProgresoCursoService progresoEstadisticasService = new();
    private readonly CursoService cursoEstadisticasService = new();

    private Panel panelEstadisticas = null!;
    private Panel panelIndicadorEstadisticas = null!;
    private PictureBox pictureBoxEstadisticas = null!;
    private Label lblEstadisticas = null!;
    private Panel panelEstadisticasVista = null!;
    private PanelDesplazableSinBarras desplazamientoEstadisticas = null!;
    private FlowLayoutPanel contenidoEstadisticas = null!;

    private Label lblTituloEstadisticas = null!;
    private Label lblSubtituloEstadisticas = null!;
    private Label lblTituloResumenEstadisticas = null!;
    private Panel panelResumenEstadisticas = null!;
    private readonly List<PresentacionMetricaEstadistica> tarjetasResumenEstadisticas = new();

    private Panel panelProgresoGeneralEstadisticas = null!;
    private Label lblTituloProgresoGeneralEstadisticas = null!;
    private Label lblDescripcionProgresoGeneralEstadisticas = null!;
    private Label lblBarraProgresoGeneralEstadisticas = null!;
    private Label lblValorProgresoGeneralEstadisticas = null!;

    private Label lblTituloDominioEstadisticas = null!;
    private Panel panelDominioEstadisticas = null!;
    private Label lblDescripcionDominioEstadisticas = null!;
    private readonly List<PresentacionDominioTemaEstadistica> filasDominioEstadisticas = new();

    private Label lblTituloActividadEstadisticas = null!;
    private Panel panelActividadEstadisticas = null!;
    private readonly List<PresentacionMetricaEstadistica> tarjetasActividadEstadisticas = new();

    private void InicializarEstructuraEstadisticas() {
        if (estructuraEstadisticasInicializada) {
            return;
        }

        ReubicarOpcionesMenuParaEstadisticas();
        CrearOpcionMenuEstadisticas();
        ConfigurarIconosEmojiMenu();

        panelEstadisticasVista = new Panel {
            Name = "panelEstadisticasVista",
            BackColor = ColorFondoEstadisticas,
            Dock = DockStyle.Fill,
            Location = Point.Empty,
            Visible = false
        };

        panelPrincipal.Controls.Add(panelEstadisticasVista);
        ActivarDobleBuffer(panelEstadisticasVista);
        estructuraEstadisticasInicializada = true;
    }

    private void ReubicarOpcionesMenuParaEstadisticas() {
        panelAbrirPractica.Location = new Point(12, 394);
        panelRecientes.Location = new Point(12, 450);
        panelConfiguracion.Location = new Point(12, 506);
        panelAcercaDe.Location = new Point(12, 562);

        panelAbrirPractica.TabIndex = 7;
        panelRecientes.TabIndex = 8;
        panelConfiguracion.TabIndex = 9;
        panelAcercaDe.TabIndex = 10;
    }

    private void CrearOpcionMenuEstadisticas() {
        panelEstadisticas = new Panel {
            Name = "panelEstadisticas",
            BackColor = Color.FromArgb(45, 45, 48),
            Cursor = Cursors.Hand,
            Location = new Point(12, 338),
            Size = new Size(195, 48),
            TabIndex = 6
        };

        panelIndicadorEstadisticas = new Panel {
            Name = "panelIndicadorEstadisticas",
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(3, 48),
            TabIndex = 2,
            Visible = false
        };

        pictureBoxEstadisticas = new PictureBox {
            Name = "pictureBoxEstadisticas",
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Location = new Point(15, 13),
            Size = new Size(24, 24),
            SizeMode = PictureBoxSizeMode.Zoom,
            TabIndex = 0,
            TabStop = false
        };
        Font fuenteMenuEstadisticas = new("Segoe UI Semibold", 10F);
        lblEstadisticas = new Label {
            Name = "lblEstadisticas",
            AutoSize = false,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Font = fuenteMenuEstadisticas,
            ForeColor = Color.White,
            Location = new Point(49, 10),
            Size = new Size(146, 28),
            TabIndex = 1,
            Text = "Estadísticas",
            TextAlign = ContentAlignment.MiddleLeft
        };
        lblEstadisticas.Disposed += (_, _) => fuenteMenuEstadisticas.Dispose();

        panelEstadisticas.Controls.Add(panelIndicadorEstadisticas);
        panelEstadisticas.Controls.Add(lblEstadisticas);
        panelEstadisticas.Controls.Add(pictureBoxEstadisticas);
        panelMenu.Controls.Add(panelEstadisticas);

        panelEstadisticas.MouseEnter += PanelMenu_MouseEnter;
        panelEstadisticas.MouseLeave += PanelMenu_MouseLeave;
        lblEstadisticas.MouseEnter += PanelMenu_MouseEnter;
        lblEstadisticas.MouseLeave += PanelMenu_MouseLeave;
        pictureBoxEstadisticas.MouseEnter += PanelMenu_MouseEnter;
        pictureBoxEstadisticas.MouseLeave += PanelMenu_MouseLeave;
        panelEstadisticas.Click += PanelEstadisticas_Click;
        lblEstadisticas.Click += PanelEstadisticas_Click;
        pictureBoxEstadisticas.Click += PanelEstadisticas_Click;
    }

    private void ConfigurarIconosEmojiMenu() {
        ConfigurarIconoEmojiMenu(pictureBoxInicio, "🏠", "Inicio");
        ConfigurarIconoEmojiMenu(pictureBoxNuevaPractica, "✨", "Nueva práctica");
        ConfigurarIconoEmojiMenu(pictureBoxCurso, "📚", "Curso");
        ConfigurarIconoEmojiMenu(pictureBoxEstadisticas, "📊", "Estadísticas");
        ConfigurarIconoEmojiMenu(pictureBoxAbrirPractica, "📂", "Abrir práctica");
        ConfigurarIconoEmojiMenu(pictureBoxRecientes, "🕘", "Recientes");
        ConfigurarIconoEmojiMenu(pictureBoxConfiguracion, "⚙️", "Configuración");
        ConfigurarIconoEmojiMenu(pictureBoxAcercaDe, "ℹ️", "Acerca de");
    }

    private static void ConfigurarIconoEmojiMenu(
        PictureBox icono,
        string emoji,
        string nombreAccesible) {
        icono.Tag = emoji;
        icono.AccessibleName = $"Icono de {nombreAccesible}";
        icono.Paint -= PictureBoxEmojiMenu_Paint;
        icono.Paint += PictureBoxEmojiMenu_Paint;
        icono.Invalidate();
    }

    private static void PictureBoxEmojiMenu_Paint(object? sender, PaintEventArgs e) {
        if (sender is not PictureBox icono ||
            icono.Tag is not string emoji ||
            string.IsNullOrEmpty(emoji) ||
            icono.ClientSize.Width <= 0 ||
            icono.ClientSize.Height <= 0) {
            return;
        }

        float tamanoFuente = Math.Max(12F, icono.ClientSize.Height * 0.7F);
        using Font fuente = new(
            "Segoe UI Emoji",
            tamanoFuente,
            FontStyle.Regular,
            GraphicsUnit.Pixel);
        TextRenderer.DrawText(
            e.Graphics,
            emoji,
            fuente,
            icono.ClientRectangle,
            Color.White,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPadding |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.SingleLine);
    }

    private void PanelEstadisticas_Click(object? sender, EventArgs e) {
        if (!estructuraEstadisticasInicializada) {
            return;
        }

        NavegarVistaPrincipalConTransicion(
            panelEstadisticasVista,
            panelEstadisticas,
            DistribucionPanelPrincipal.Estadisticas,
            AsegurarVistaEstadisticasConstruida);
    }

    private void OcultarVistaEstadisticas() {
        if (estructuraEstadisticasInicializada) {
            panelEstadisticasVista.Visible = false;
        }
    }

    private void AsegurarVistaEstadisticasConstruida() {
        if (!vistaEstadisticasConstruida) {
            ConstruirVistaEstadisticas();
            vistaEstadisticasConstruida = true;
        }

        ActualizarDatosEstadisticas(CrearDatosEstadisticasReales());
    }

    private void ConstruirVistaEstadisticas() {
        desplazamientoEstadisticas = CrearPanelDesplazableCurso(
            "desplazamientoEstadisticas",
            "contenidoEstadisticas",
            Point.Empty,
            new Size(1, 1),
            Padding.Empty);
        desplazamientoEstadisticas.BackColor = ColorFondoEstadisticas;
        desplazamientoEstadisticas.ColorFondoContenido = ColorFondoEstadisticas;
        desplazamientoEstadisticas.Padding = Padding.Empty;
        desplazamientoEstadisticas.MostrarBordeFoco = false;
        contenidoEstadisticas = desplazamientoEstadisticas.Contenido;

        lblTituloEstadisticas = CrearLabelCurso(
            "Tus estadísticas",
            Point.Empty,
            new Size(1, EscalarDiseno(48)),
            24F,
            FontStyle.Bold,
            Color.White);
        lblTituloEstadisticas.Margin = new Padding(0, 0, 0, EscalarDiseno(2));

        lblSubtituloEstadisticas = CrearLabelCurso(
            "Consulta tu progreso, rendimiento y actividad dentro de EndForge.",
            Point.Empty,
            new Size(1, EscalarDiseno(42)),
            10.5F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);
        lblSubtituloEstadisticas.Margin = new Padding(0, 0, 0, EscalarDiseno(18));

        lblTituloResumenEstadisticas = CrearEtiquetaSeccionEstadisticas("RESUMEN");
        panelResumenEstadisticas = CrearContenedorTransparenteEstadisticas();
        panelResumenEstadisticas.Margin = new Padding(0, 0, 0, EscalarDiseno(18));

        tarjetasResumenEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Prácticas completadas",
            "Avance acumulado",
            ColorVerdeEstadisticas));
        tarjetasResumenEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Promedio general",
            "Calificaciones registradas",
            ColorCalidoEstadisticas));
        tarjetasResumenEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Intentos realizados",
            "Actividad de evaluación",
            ColorMoradoClaroCurso));
        tarjetasResumenEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Última actividad",
            "Sesión más reciente",
            ColorVerdeClaroEstadisticas));

        foreach (PresentacionMetricaEstadistica tarjeta in tarjetasResumenEstadisticas) {
            panelResumenEstadisticas.Controls.Add(tarjeta.Tarjeta);
        }

        panelProgresoGeneralEstadisticas = CrearTarjetaCurso(
            Point.Empty,
            new Size(1, EscalarDiseno(156)),
            EscalarDiseno(16));
        panelProgresoGeneralEstadisticas.Margin = new Padding(0, 0, 0, EscalarDiseno(18));
        lblTituloProgresoGeneralEstadisticas = CrearLabelCurso(
            "Progreso general",
            Point.Empty,
            new Size(1, EscalarDiseno(30)),
            14F,
            FontStyle.Bold,
            Color.White);
        lblDescripcionProgresoGeneralEstadisticas = CrearLabelCurso(
            "Una lectura rápida del avance actual en tu ruta.",
            Point.Empty,
            new Size(1, EscalarDiseno(24)),
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);
        lblBarraProgresoGeneralEstadisticas = CrearLabelBarraEstadistica(14F);
        lblValorProgresoGeneralEstadisticas = CrearLabelCurso(
            "0/0",
            Point.Empty,
            new Size(1, EscalarDiseno(30)),
            13F,
            FontStyle.Bold,
            ColorVerdeEstadisticas,
            ContentAlignment.MiddleRight);
        panelProgresoGeneralEstadisticas.Controls.Add(lblTituloProgresoGeneralEstadisticas);
        panelProgresoGeneralEstadisticas.Controls.Add(lblDescripcionProgresoGeneralEstadisticas);
        panelProgresoGeneralEstadisticas.Controls.Add(lblBarraProgresoGeneralEstadisticas);
        panelProgresoGeneralEstadisticas.Controls.Add(lblValorProgresoGeneralEstadisticas);

        lblTituloDominioEstadisticas = CrearEtiquetaSeccionEstadisticas("DOMINIO POR TEMA");
        panelDominioEstadisticas = CrearTarjetaCurso(
            Point.Empty,
            new Size(1, EscalarDiseno(390)),
            EscalarDiseno(16));
        panelDominioEstadisticas.Margin = new Padding(0, 0, 0, EscalarDiseno(18));
        lblDescripcionDominioEstadisticas = CrearLabelCurso(
            "Tu nivel actual en los fundamentos principales de C++.",
            Point.Empty,
            new Size(1, EscalarDiseno(26)),
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);
        panelDominioEstadisticas.Controls.Add(lblDescripcionDominioEstadisticas);

        for (int indice = 0; indice < 4; indice++) {
            PresentacionDominioTemaEstadistica fila = CrearFilaDominioTemaEstadistica(string.Empty);
            filasDominioEstadisticas.Add(fila);
            panelDominioEstadisticas.Controls.Add(fila.Fila);
        }

        lblTituloActividadEstadisticas = CrearEtiquetaSeccionEstadisticas("ACTIVIDAD Y RENDIMIENTO");
        panelActividadEstadisticas = CrearContenedorTransparenteEstadisticas();
        panelActividadEstadisticas.Margin = new Padding(0, 0, 0, EscalarDiseno(8));

        tarjetasActividadEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Mejor calificación",
            "Tu resultado más alto",
            ColorCalidoEstadisticas));
        tarjetasActividadEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Tiempo promedio por práctica",
            "Ritmo de resolución",
            ColorVerdeEstadisticas));
        tarjetasActividadEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Tiempo total programando",
            "Actividad acumulada",
            ColorMoradoClaroCurso));
        tarjetasActividadEstadisticas.Add(CrearTarjetaMetricaEstadistica(
            "Tema más practicado",
            "Tu enfoque principal",
            ColorVerdeClaroEstadisticas));

        foreach (PresentacionMetricaEstadistica tarjeta in tarjetasActividadEstadisticas) {
            panelActividadEstadisticas.Controls.Add(tarjeta.Tarjeta);
        }

        contenidoEstadisticas.Controls.Add(lblTituloEstadisticas);
        contenidoEstadisticas.Controls.Add(lblSubtituloEstadisticas);
        contenidoEstadisticas.Controls.Add(lblTituloResumenEstadisticas);
        contenidoEstadisticas.Controls.Add(panelResumenEstadisticas);
        contenidoEstadisticas.Controls.Add(panelProgresoGeneralEstadisticas);
        contenidoEstadisticas.Controls.Add(lblTituloDominioEstadisticas);
        contenidoEstadisticas.Controls.Add(panelDominioEstadisticas);
        contenidoEstadisticas.Controls.Add(lblTituloActividadEstadisticas);
        contenidoEstadisticas.Controls.Add(panelActividadEstadisticas);
        panelEstadisticasVista.Controls.Add(desplazamientoEstadisticas);

        ActivarDobleBuffer(desplazamientoEstadisticas);
        ActivarDobleBuffer(contenidoEstadisticas);
        ActivarDobleBuffer(panelResumenEstadisticas);
        ActivarDobleBuffer(panelDominioEstadisticas);
        ActivarDobleBuffer(panelActividadEstadisticas);
    }

    private Label CrearEtiquetaSeccionEstadisticas(string texto) {
        Label etiqueta = CrearLabelCurso(
            texto,
            Point.Empty,
            new Size(1, EscalarDiseno(28)),
            9F,
            FontStyle.Bold,
            ColorCalidoEstadisticas);
        etiqueta.Margin = new Padding(0, 0, 0, EscalarDiseno(8));
        return etiqueta;
    }

    private static Panel CrearContenedorTransparenteEstadisticas() {
        return new Panel {
            BackColor = Color.Transparent,
            Location = Point.Empty
        };
    }

    private PresentacionMetricaEstadistica CrearTarjetaMetricaEstadistica(
        string titulo,
        string descripcion,
        Color colorValor) {
        Panel tarjeta = CrearTarjetaCurso(
            Point.Empty,
            new Size(EscalarDiseno(220), EscalarDiseno(108)),
            EscalarDiseno(14));
        Label lblTitulo = CrearLabelCurso(
            titulo,
            Point.Empty,
            new Size(1, EscalarDiseno(24)),
            9.2F,
            FontStyle.Bold,
            ColorTextoSecundarioCurso);
        Label lblValor = CrearLabelCurso(
            "—",
            Point.Empty,
            new Size(1, EscalarDiseno(36)),
            18F,
            FontStyle.Bold,
            colorValor);
        Label lblDescripcion = CrearLabelCurso(
            descripcion,
            Point.Empty,
            new Size(1, EscalarDiseno(22)),
            8.7F,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);

        tarjeta.Controls.Add(lblTitulo);
        tarjeta.Controls.Add(lblValor);
        tarjeta.Controls.Add(lblDescripcion);
        ActivarDobleBuffer(tarjeta);
        return new PresentacionMetricaEstadistica(
            tarjeta,
            lblTitulo,
            lblValor,
            lblDescripcion);
    }

    private PresentacionDominioTemaEstadistica CrearFilaDominioTemaEstadistica(
        string tema) {
        Panel fila = new() {
            BackColor = ColorFilaEstadisticas,
            Size = new Size(1, EscalarDiseno(68))
        };
        int ObtenerRadioFila() => Math.Max(
            1,
            (int)Math.Round(11D * fila.DeviceDpi / 96D));
        AplicarRegionRedondeada(fila, ObtenerRadioFila());
        fila.SizeChanged += (_, _) =>
            AplicarRegionRedondeada(fila, ObtenerRadioFila());

        Label lblTema = CrearLabelCurso(
            tema,
            Point.Empty,
            new Size(1, EscalarDiseno(26)),
            10.5F,
            FontStyle.Bold,
            Color.White);
        Label lblBarra = CrearLabelBarraEstadistica(10.5F);
        Label lblValor = CrearLabelCurso(
            "0/0",
            Point.Empty,
            new Size(1, EscalarDiseno(24)),
            9.5F,
            FontStyle.Bold,
            ColorVerdeEstadisticas,
            ContentAlignment.MiddleRight);
        Label lblNivel = CrearLabelCurso(
            "Inicial",
            Point.Empty,
            new Size(1, EscalarDiseno(24)),
            9F,
            FontStyle.Bold,
            ColorCalidoEstadisticas,
            ContentAlignment.MiddleRight);

        fila.Controls.Add(lblTema);
        fila.Controls.Add(lblBarra);
        fila.Controls.Add(lblValor);
        fila.Controls.Add(lblNivel);
        ActivarDobleBuffer(fila);
        return new PresentacionDominioTemaEstadistica(
            fila,
            lblTema,
            lblBarra,
            lblValor,
            lblNivel);
    }

    private static Label CrearLabelBarraEstadistica(float tamanoFuente) {
        Font fuente = new("Segoe UI Emoji", tamanoFuente, FontStyle.Regular);
        Label label = new BarraSegmentadaEstadisticas {
            AutoEllipsis = false,
            BackColor = Color.Transparent,
            Font = fuente,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft
        };
        label.Disposed += (_, _) => fuente.Dispose();
        return label;
    }

    private DatosVistaEstadisticas CrearDatosEstadisticasReales() {
        IReadOnlyList<TemaCurso> temasDisponibles = cursoEstadisticasService
            .CargarTemas()
            .Where(tema => !tema.EsProximamente)
            .OrderBy(tema => tema.Numero)
            .ToArray();
        Dictionary<string, PracticaCurso> practicasConocidas = temasDisponibles
            .SelectMany(tema => tema.Practicas)
            .GroupBy(practica => practica.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.First(),
                StringComparer.OrdinalIgnoreCase);
        ResultadoCargaProgreso cargaProgreso =
            progresoEstadisticasService.CargarProgreso();
        IReadOnlyList<ProgresoPractica> progreso = cargaProgreso.DatosDisponibles
            ? cargaProgreso.Progreso.Practicas
            : Array.Empty<ProgresoPractica>();
        ResultadoCargaHistorialEvaluaciones cargaHistorial =
            historialEvaluacionesService.CargarHistorial();
        IReadOnlyList<HistorialPractica> historial = cargaHistorial.DatosDisponibles
            ? cargaHistorial.Historial.Practicas
            : Array.Empty<HistorialPractica>();

        HashSet<string> practicasRealizadas = progreso
            .Where(item =>
                item.Estado == EstadoPracticaCurso.Realizada &&
                practicasConocidas.ContainsKey(item.PracticaId))
            .Select(item => item.PracticaId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int totalPracticas = practicasConocidas.Count;
        int totalRealizadas = practicasRealizadas.Count;
        int porcentajeGeneral = CalcularPorcentajeEstadistica(
            totalRealizadas,
            totalPracticas);
        IReadOnlyList<HistorialPractica> historialConocido = historial
            .Where(item => practicasConocidas.ContainsKey(item.PracticaId))
            .ToArray();
        int[] mejoresCalificaciones = historialConocido
            .Where(item => item.MejorCalificacion.HasValue)
            .Select(item => item.MejorCalificacion!.Value)
            .ToArray();
        int? promedioGeneral = mejoresCalificaciones.Length == 0
            ? null
            : (int)Math.Round(
                mejoresCalificaciones.Average(),
                MidpointRounding.AwayFromZero);
        int? mejorCalificacion = mejoresCalificaciones.Length == 0
            ? null
            : mejoresCalificaciones.Max();
        long intentosRealizados = historialConocido.Sum(item =>
            (long)item.TotalIntentos);
        DateTimeOffset? ultimaActividad = ObtenerUltimaActividadEstadisticas(
            progreso,
            historialConocido,
            practicasConocidas);
        IReadOnlyList<DatosDominioTemaEstadistica> dominioTemas =
            CrearProgresoTemasEstadisticas(
                temasDisponibles,
                practicasRealizadas);
        string temaMasPracticado = ObtenerTemaMasPracticadoEstadisticas(
            historialConocido,
            practicasConocidas,
            temasDisponibles);

        return new DatosVistaEstadisticas(
            PracticasCompletadas: totalRealizadas,
            TotalPracticas: totalPracticas,
            PromedioGeneral: promedioGeneral,
            IntentosRealizados: intentosRealizados,
            UltimaActividad: FormatearUltimaActividadEstadisticas(ultimaActividad),
            ProgresoGeneral: porcentajeGeneral,
            MaximoProgresoGeneral: 100,
            DominioTemas: dominioTemas,
            MejorCalificacion: mejorCalificacion.HasValue
                ? $"{mejorCalificacion.Value}/100"
                : "—",
            TiempoPromedioPractica: "No medido",
            TiempoTotalProgramando: "No medido",
            TemaMasPracticado: temaMasPracticado);
    }

    private static int CalcularPorcentajeEstadistica(int valor, int total) {
        return total <= 0
            ? 0
            : Math.Clamp(
                (int)Math.Round(
                    valor * 100D / total,
                    MidpointRounding.AwayFromZero),
                0,
                100);
    }

    private static IReadOnlyList<DatosDominioTemaEstadistica>
        CrearProgresoTemasEstadisticas(
            IReadOnlyList<TemaCurso> temasDisponibles,
            IReadOnlySet<string> practicasRealizadas) {
        string[] temasMostrados = {
            "variables",
            "condicionales",
            "ciclos",
            "funciones"
        };
        Dictionary<string, TemaCurso> temasPorId = temasDisponibles.ToDictionary(
            tema => tema.Id,
            StringComparer.OrdinalIgnoreCase);
        List<DatosDominioTemaEstadistica> resultado = new(temasMostrados.Length);

        foreach (string temaId in temasMostrados) {
            if (!temasPorId.TryGetValue(temaId, out TemaCurso? tema)) {
                continue;
            }

            int publicadas = tema.Practicas.Count;
            int realizadas = tema.Practicas.Count(practica =>
                practicasRealizadas.Contains(practica.Id));
            int porcentaje = CalcularPorcentajeEstadistica(realizadas, publicadas);
            string nivel = porcentaje switch {
                100 => "Dominado",
                >= 50 => "Competente",
                >= 1 => "En progreso",
                _ => "Inicial"
            };
            resultado.Add(new DatosDominioTemaEstadistica(
                tema.Nombre,
                realizadas,
                publicadas,
                nivel));
        }

        return resultado;
    }

    private static DateTimeOffset? ObtenerUltimaActividadEstadisticas(
        IEnumerable<ProgresoPractica> progreso,
        IEnumerable<HistorialPractica> historial,
        IReadOnlyDictionary<string, PracticaCurso> practicasConocidas) {
        DateTimeOffset? ultimaActividad = null;

        foreach (ProgresoPractica practica in progreso.Where(item =>
            practicasConocidas.ContainsKey(item.PracticaId))) {
            ultimaActividad = ObtenerFechaMasReciente(
                ultimaActividad,
                practica.FechaCreacion,
                practica.FechaActualizacion,
                practica.FechaFinalizacion);
        }

        foreach (DateTimeOffset fecha in historial
            .Select(item => item.FechaUltimoIntento)
            .Where(fecha => fecha.HasValue)
            .Select(fecha => fecha!.Value)) {
            ultimaActividad = ObtenerFechaMasReciente(ultimaActividad, fecha);
        }

        return ultimaActividad;
    }

    private static DateTimeOffset? ObtenerFechaMasReciente(
        DateTimeOffset? fechaActual,
        params DateTimeOffset?[] fechas) {
        DateTimeOffset? resultado = fechaActual;

        foreach (DateTimeOffset? fecha in fechas) {
            if (fecha.HasValue &&
                (!resultado.HasValue || fecha.Value > resultado.Value)) {
                resultado = fecha;
            }
        }

        return resultado;
    }

    private static string FormatearUltimaActividadEstadisticas(
        DateTimeOffset? fecha) {
        if (!fecha.HasValue) {
            return "—";
        }

        DateTime fechaLocal = fecha.Value.ToLocalTime().Date;
        DateTime hoy = DateTimeOffset.Now.LocalDateTime.Date;
        int dias = (hoy - fechaLocal).Days;

        return dias switch {
            0 => "Hoy",
            1 => "Ayer",
            >= 2 and <= 30 => $"Hace {dias} días",
            _ => fechaLocal.ToString("d", CultureInfo.CurrentCulture)
        };
    }

    private static string ObtenerTemaMasPracticadoEstadisticas(
        IEnumerable<HistorialPractica> historial,
        IReadOnlyDictionary<string, PracticaCurso> practicasConocidas,
        IReadOnlyList<TemaCurso> temasDisponibles) {
        Dictionary<string, TemaCurso> temasPorId = temasDisponibles.ToDictionary(
            tema => tema.Id,
            StringComparer.OrdinalIgnoreCase);
        var temaMasPracticado = historial
            .Where(item =>
                item.TotalIntentos > 0 &&
                practicasConocidas.ContainsKey(item.PracticaId))
            .Select(item => new {
                Practica = practicasConocidas[item.PracticaId],
                Intentos = (long)item.TotalIntentos
            })
            .Where(item => temasPorId.ContainsKey(item.Practica.TemaId))
            .GroupBy(
                item => item.Practica.TemaId,
                StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new {
                Tema = temasPorId[grupo.Key],
                Intentos = grupo.Sum(item => item.Intentos)
            })
            .OrderByDescending(item => item.Intentos)
            .ThenBy(item => item.Tema.Numero)
            .FirstOrDefault();

        return temaMasPracticado?.Tema.Nombre ?? "—";
    }

    private void ActualizarDatosEstadisticas(DatosVistaEstadisticas datos) {
        if (!vistaEstadisticasConstruida) {
            return;
        }

        tarjetasResumenEstadisticas[0].Valor.Text =
            $"{datos.PracticasCompletadas} de {datos.TotalPracticas}";
        tarjetasResumenEstadisticas[1].Valor.Text = datos.PromedioGeneral.HasValue
            ? $"{datos.PromedioGeneral.Value}/100"
            : "—";
        tarjetasResumenEstadisticas[2].Valor.Text =
            $"{datos.IntentosRealizados} intentos";
        tarjetasResumenEstadisticas[3].Valor.Text = datos.UltimaActividad;

        ResultadoBarraEstadistica progresoGeneral = CrearBarraEstadistica(
            datos.ProgresoGeneral,
            datos.MaximoProgresoGeneral,
            segmentos: 10);
        lblBarraProgresoGeneralEstadisticas.Text = progresoGeneral.Segmentos;
        lblValorProgresoGeneralEstadisticas.Text =
            $"{progresoGeneral.ValorNormalizado}%";

        int filasDisponibles = Math.Min(
            filasDominioEstadisticas.Count,
            datos.DominioTemas.Count);

        for (int indice = 0; indice < filasDominioEstadisticas.Count; indice++) {
            PresentacionDominioTemaEstadistica presentacion =
                filasDominioEstadisticas[indice];
            bool tieneDatos = indice < filasDisponibles;
            presentacion.Fila.Visible = tieneDatos;

            if (!tieneDatos) {
                continue;
            }

            DatosDominioTemaEstadistica tema = datos.DominioTemas[indice];
            ResultadoBarraEstadistica barra = CrearBarraEstadistica(
                tema.Valor,
                tema.Maximo,
                segmentos: 10);
            presentacion.Tema.Text = tema.Tema;
            presentacion.Barra.Text = barra.Segmentos;
            presentacion.Valor.Text = barra.TextoNumerico;
            presentacion.Nivel.Text = tema.Nivel;
            presentacion.Nivel.ForeColor = ObtenerColorNivelEstadistica(tema.Nivel);
        }

        tarjetasActividadEstadisticas[0].Valor.Text = datos.MejorCalificacion;
        tarjetasActividadEstadisticas[1].Valor.Text = datos.TiempoPromedioPractica;
        tarjetasActividadEstadisticas[2].Valor.Text = datos.TiempoTotalProgramando;
        tarjetasActividadEstadisticas[3].Valor.Text = datos.TemaMasPracticado;
    }

    private static Color ObtenerColorNivelEstadistica(string nivel) {
        if (nivel.Equals("Dominado", StringComparison.OrdinalIgnoreCase)) {
            return ColorVerdeEstadisticas;
        }

        if (nivel.Equals("Competente", StringComparison.OrdinalIgnoreCase)) {
            return ColorVerdeClaroEstadisticas;
        }

        if (nivel.Equals("En progreso", StringComparison.OrdinalIgnoreCase)) {
            return ColorMoradoClaroCurso;
        }

        return ColorCalidoEstadisticas;
    }

    private static ResultadoBarraEstadistica CrearBarraEstadistica(
        int valor,
        int maximo,
        int segmentos) {
        int cantidadSegmentos = Math.Max(0, segmentos);
        int maximoSeguro = Math.Max(0, maximo);
        int valorNormalizado = maximoSeguro == 0
            ? 0
            : Math.Clamp(valor, 0, maximoSeguro);
        int segmentosActivos = maximoSeguro == 0
            ? 0
            : Math.Clamp(
                (int)Math.Round(
                    valorNormalizado * cantidadSegmentos / (double)maximoSeguro,
                    MidpointRounding.AwayFromZero),
                0,
                cantidadSegmentos);
        string barra =
            string.Concat(Enumerable.Repeat(SegmentoLlenoEstadisticas, segmentosActivos)) +
            string.Concat(Enumerable.Repeat(
                SegmentoVacioEstadisticas,
                cantidadSegmentos - segmentosActivos));

        return new ResultadoBarraEstadistica(
            barra,
            $"{valorNormalizado}/{maximoSeguro}",
            valorNormalizado,
            segmentosActivos);
    }

    private void RecalcularGeometriaEstadisticas() {
        if (!estructuraEstadisticasInicializada ||
            distribucionPanelPrincipal != DistribucionPanelPrincipal.Estadisticas ||
            panelEstadisticasVista.IsDisposed ||
            panelEstadisticasVista.ClientSize.Width <= 0 ||
            panelEstadisticasVista.ClientSize.Height <= 0) {
            return;
        }

        Rectangle area = panelEstadisticasVista.ClientRectangle;
        bool modoAmplio = area.Width >= EscalarDiseno(1100);
        int margenHorizontal = EscalarDiseno(modoAmplio ? 24 : 12);
        int margenVertical = EscalarDiseno(modoAmplio ? 16 : 10);
        int anchoDisponible = Math.Max(1, area.Width - margenHorizontal * 2);
        int anchoViewport = Math.Min(EscalarDiseno(1500), anchoDisponible);
        int altoViewport = Math.Max(1, area.Height - margenVertical * 2);
        int x = area.Left + margenHorizontal;

        if (!vistaEstadisticasConstruida) {
            return;
        }

        desplazamientoEstadisticas.SetBounds(
            x,
            area.Top + margenVertical,
            anchoViewport,
            altoViewport);

        Padding relleno = new(
            EscalarDiseno(modoAmplio ? 24 : 18),
            EscalarDiseno(modoAmplio ? 20 : 16),
            EscalarDiseno(modoAmplio ? 24 : 18),
            EscalarDiseno(32));

        if (contenidoEstadisticas.Padding != relleno) {
            contenidoEstadisticas.Padding = relleno;
        }

        desplazamientoEstadisticas.ActualizarContenido(volverAlInicio: false);
        int anchoContenido = Math.Max(
            1,
            contenidoEstadisticas.ClientSize.Width - contenidoEstadisticas.Padding.Horizontal);

        if (anchoContenido != ultimoAnchoContenidoEstadisticas ||
            DeviceDpi != ultimoDpiEstadisticas ||
            modoAmplio != ultimoModoAmplioEstadisticas) {
            ActualizarGeometriaContenidoEstadisticas(anchoContenido, modoAmplio);
            ultimoAnchoContenidoEstadisticas = anchoContenido;
            ultimoDpiEstadisticas = DeviceDpi;
            ultimoModoAmplioEstadisticas = modoAmplio;
        }

        desplazamientoEstadisticas.ActualizarContenido(volverAlInicio: false);
    }

    private void ActualizarGeometriaContenidoEstadisticas(
        int anchoContenido,
        bool modoAmplio) {
        contenidoEstadisticas.SuspendLayout();

        try {
            lblTituloEstadisticas.Width = anchoContenido;
            lblSubtituloEstadisticas.Width = anchoContenido;
            lblTituloResumenEstadisticas.Width = anchoContenido;
            lblTituloDominioEstadisticas.Width = anchoContenido;
            lblTituloActividadEstadisticas.Width = anchoContenido;

            ActualizarGeometriaGrupoMetricas(
                panelResumenEstadisticas,
                tarjetasResumenEstadisticas,
                anchoContenido,
                modoAmplio);
            ActualizarGeometriaProgresoGeneral(anchoContenido, modoAmplio);
            ActualizarGeometriaDominioTemas(anchoContenido, modoAmplio);
            ActualizarGeometriaGrupoMetricas(
                panelActividadEstadisticas,
                tarjetasActividadEstadisticas,
                anchoContenido,
                modoAmplio);
        } finally {
            contenidoEstadisticas.ResumeLayout(performLayout: false);
        }
    }

    private void ActualizarGeometriaGrupoMetricas(
        Panel contenedor,
        IReadOnlyList<PresentacionMetricaEstadistica> tarjetas,
        int anchoContenido,
        bool modoAmplio) {
        int separacion = EscalarDiseno(12);
        int columnas = modoAmplio
            ? 4
            : anchoContenido >= EscalarDiseno(620)
                ? 2
                : 1;
        int filas = (int)Math.Ceiling(tarjetas.Count / (double)columnas);
        int altoTarjeta = EscalarDiseno(108);
        int anchoTarjeta = Math.Max(
            1,
            (anchoContenido - separacion * (columnas - 1)) / columnas);

        contenedor.Size = new Size(
            anchoContenido,
            filas * altoTarjeta + Math.Max(0, filas - 1) * separacion);

        for (int indice = 0; indice < tarjetas.Count; indice++) {
            int columna = indice % columnas;
            int fila = indice / columnas;
            PresentacionMetricaEstadistica tarjeta = tarjetas[indice];
            tarjeta.Tarjeta.SetBounds(
                columna * (anchoTarjeta + separacion),
                fila * (altoTarjeta + separacion),
                anchoTarjeta,
                altoTarjeta);
            ActualizarGeometriaTarjetaMetrica(tarjeta, anchoTarjeta, altoTarjeta);
        }
    }

    private void ActualizarGeometriaTarjetaMetrica(
        PresentacionMetricaEstadistica tarjeta,
        int ancho,
        int alto) {
        int margen = EscalarDiseno(16);
        int anchoInterior = Math.Max(1, ancho - margen * 2);
        tarjeta.Titulo.SetBounds(
            margen,
            EscalarDiseno(12),
            anchoInterior,
            EscalarDiseno(22));
        tarjeta.Valor.SetBounds(
            margen,
            EscalarDiseno(34),
            anchoInterior,
            EscalarDiseno(38));
        tarjeta.Descripcion.SetBounds(
            margen,
            alto - EscalarDiseno(32),
            anchoInterior,
            EscalarDiseno(20));
    }

    private void ActualizarGeometriaProgresoGeneral(
        int anchoContenido,
        bool modoAmplio) {
        int alto = EscalarDiseno(modoAmplio ? 150 : 162);
        int margen = EscalarDiseno(modoAmplio ? 24 : 18);
        int anchoInterior = Math.Max(1, anchoContenido - margen * 2);
        int anchoValor = EscalarDiseno(96);
        int separacion = EscalarDiseno(12);

        panelProgresoGeneralEstadisticas.Size = new Size(anchoContenido, alto);
        lblTituloProgresoGeneralEstadisticas.SetBounds(
            margen,
            EscalarDiseno(16),
            anchoInterior,
            EscalarDiseno(30));
        lblDescripcionProgresoGeneralEstadisticas.SetBounds(
            margen,
            EscalarDiseno(44),
            anchoInterior,
            EscalarDiseno(24));
        lblBarraProgresoGeneralEstadisticas.SetBounds(
            margen,
            EscalarDiseno(78),
            Math.Max(1, anchoInterior - anchoValor - separacion),
            EscalarDiseno(48));
        lblValorProgresoGeneralEstadisticas.SetBounds(
            anchoContenido - margen - anchoValor,
            EscalarDiseno(84),
            anchoValor,
            EscalarDiseno(36));
    }

    private void ActualizarGeometriaDominioTemas(
        int anchoContenido,
        bool modoAmplio) {
        int margen = EscalarDiseno(modoAmplio ? 22 : 16);
        int separacion = EscalarDiseno(8);
        int altoFila = EscalarDiseno(modoAmplio ? 66 : 76);
        int inicioFilas = EscalarDiseno(58);
        int anchoFila = Math.Max(1, anchoContenido - margen * 2);
        int altoPanel = inicioFilas +
            filasDominioEstadisticas.Count * altoFila +
            Math.Max(0, filasDominioEstadisticas.Count - 1) * separacion +
            margen;

        panelDominioEstadisticas.Size = new Size(anchoContenido, altoPanel);
        lblDescripcionDominioEstadisticas.SetBounds(
            margen,
            EscalarDiseno(18),
            anchoFila,
            EscalarDiseno(26));

        for (int indice = 0; indice < filasDominioEstadisticas.Count; indice++) {
            PresentacionDominioTemaEstadistica fila = filasDominioEstadisticas[indice];
            fila.Fila.SetBounds(
                margen,
                inicioFilas + indice * (altoFila + separacion),
                anchoFila,
                altoFila);
            ActualizarGeometriaFilaDominio(fila, anchoFila, altoFila, modoAmplio);
        }
    }

    private void ActualizarGeometriaFilaDominio(
        PresentacionDominioTemaEstadistica fila,
        int ancho,
        int alto,
        bool modoAmplio) {
        int margen = EscalarDiseno(16);

        if (modoAmplio) {
            int margenVertical = EscalarDiseno(18);
            int anchoTema = EscalarDiseno(168);
            int anchoValor = EscalarDiseno(62);
            int anchoNivel = EscalarDiseno(126);
            int separacion = EscalarDiseno(12);
            int xBarra = margen + anchoTema + separacion;
            int anchoBarra = Math.Max(
                1,
                ancho - margen * 2 - anchoTema - anchoValor - anchoNivel - separacion * 3);

            fila.Tema.SetBounds(
                margen,
                margenVertical,
                anchoTema,
                Math.Max(1, alto - margenVertical * 2));
            fila.Barra.SetBounds(
                xBarra,
                EscalarDiseno(11),
                anchoBarra,
                Math.Max(1, alto - EscalarDiseno(22)));
            fila.Valor.SetBounds(
                xBarra + anchoBarra + separacion,
                margenVertical,
                anchoValor,
                Math.Max(1, alto - margenVertical * 2));
            fila.Nivel.SetBounds(
                ancho - margen - anchoNivel,
                margenVertical,
                anchoNivel,
                Math.Max(1, alto - margenVertical * 2));
            return;
        }

        int anchoNivelCompacto = EscalarDiseno(126);
        int anchoValorCompacto = EscalarDiseno(66);
        fila.Tema.SetBounds(
            margen,
            EscalarDiseno(7),
            Math.Max(1, ancho - margen * 2 - anchoNivelCompacto),
            EscalarDiseno(25));
        fila.Nivel.SetBounds(
            ancho - margen - anchoNivelCompacto,
            EscalarDiseno(7),
            anchoNivelCompacto,
            EscalarDiseno(25));
        fila.Barra.SetBounds(
            margen,
            EscalarDiseno(33),
            Math.Max(1, ancho - margen * 2 - anchoValorCompacto),
            EscalarDiseno(34));
        fila.Valor.SetBounds(
            ancho - margen - anchoValorCompacto,
            EscalarDiseno(36),
            anchoValorCompacto,
            EscalarDiseno(28));
    }
}
