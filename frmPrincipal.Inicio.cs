using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private sealed record TarjetaMetricaInicioVisual(
        Panel Tarjeta,
        Label Titulo,
        Label Valor,
        Label Descripcion);

    private sealed record FilaActividadInicioVisual(
        Panel Fila,
        Label Fecha,
        Label Descripcion);

    private static readonly Color ColorFondoInicio = Color.FromArgb(18, 14, 27);
    private static readonly Color ColorTarjetaInicio = Color.FromArgb(30, 25, 43);
    private static readonly Color ColorTarjetaDestacadaInicio =
        Color.FromArgb(35, 27, 51);
    private static readonly Color ColorBandaInicio = Color.FromArgb(46, 36, 63);
    private static readonly Color ColorTextoSecundarioInicio =
        Color.FromArgb(190, 181, 205);
    private static readonly Color ColorTextoTenueInicio =
        Color.FromArgb(154, 143, 171);
    private static readonly Color ColorExitoInicio = Color.FromArgb(100, 214, 144);

    private bool estructuraInicioInicializada;
    private bool inicioPendienteRecarga = true;
    private bool inicioRecargaSolicitadaDuranteCarga;
    private bool accionInicioEnCurso;
    private int ultimoAnchoContenidoInicio = -1;
    private int ultimoDpiInicio = -1;
    private bool ultimoModoAmplioInicio;
    private int valorBarraProgresoInicio;
    private int valorBarraNivelInicio;
    private Task? tareaCargaInicio;
    private CancellationTokenSource? cancelacionCargaInicio;

    private ResumenAprendizajeService resumenAprendizajeInicioService = null!;
    private PresentadorInicioService presentadorInicioService = null!;
    private PresentadorLogrosService presentadorLogrosService = null!;
    private CoordinadorCargaInicio coordinadorCargaInicio = null!;
    private readonly CoordinadorNotificacionesLogros
        coordinadorNotificacionesLogros = new();
    private PresentacionInicio? ultimaPresentacionInicio;
    private IReadOnlyList<LogroDesbloqueado> loteNotificacionLogrosVisible =
        Array.Empty<LogroDesbloqueado>();
    private bool notificacionesLogrosCerradas;

    private PanelDesplazableSinBarras desplazamientoInicio = null!;
    private FlowLayoutPanel contenidoInicio = null!;
    private Panel panelEncabezadoInicio = null!;
    private Label lblSaludoInicio = null!;
    private Label lblSubtituloDashboardInicio = null!;

    private Panel panelBandaDatosInicio = null!;
    private Label lblTituloBandaDatosInicio = null!;
    private Label lblMensajeBandaDatosInicio = null!;
    private BotonInicio btnReintentarInicio = null!;

    private Panel panelBandaLogroNuevoInicio = null!;
    private Label lblBandaLogroNuevoInicio = null!;
    private BotonInicio btnVerLogrosBandaInicio = null!;
    private BotonInicio btnCerrarBandaLogroInicio = null!;
    private System.Windows.Forms.Timer timerNotificacionLogrosInicio = null!;

    private Panel panelFilaPrincipalInicio = null!;
    private Panel panelContinuacionInicio = null!;
    private Label lblSeccionContinuacionInicio = null!;
    private Label lblTituloContinuacionInicio = null!;
    private Label lblGradoContinuacionInicio = null!;
    private Label lblTemaContinuacionInicio = null!;
    private Label lblPracticaContinuacionInicio = null!;
    private Label lblEstadoContinuacionInicio = null!;
    private Label lblRutaContinuacionInicio = null!;
    private BotonInicio btnContinuarInicio = null!;
    private BotonInicio btnAccionSecundariaInicio = null!;

    private Panel panelProgresoInicio = null!;
    private Label lblTituloProgresoInicio = null!;
    private Label lblPracticasProgresoInicio = null!;
    private Label lblPorcentajeProgresoInicio = null!;
    private Panel panelPistaProgresoInicio = null!;
    private Panel panelRellenoProgresoInicio = null!;
    private Label lblTemasProgresoInicio = null!;
    private Label lblGradosProgresoInicio = null!;
    private Label lblNivelInicio = null!;
    private Label lblXpTotalInicio = null!;
    private Panel panelPistaNivelInicio = null!;
    private Panel panelRellenoNivelInicio = null!;
    private Label lblXpRestanteInicio = null!;
    private Panel panelSeparadorMotivacionInicio = null!;
    private Label lblTituloRachaInicio = null!;
    private Label lblValorRachaInicio = null!;
    private Label lblDetalleRachaInicio = null!;
    private Label lblTituloLogrosInicio = null!;
    private Label lblValorLogrosInicio = null!;
    private BotonInicio btnVerLogrosInicio = null!;

    private Panel panelMetricasInicio = null!;
    private readonly List<TarjetaMetricaInicioVisual> tarjetasMetricasInicio = new();

    private Panel panelFilaInferiorInicio = null!;
    private Panel panelRecomendacionInicio = null!;
    private Label lblTituloRecomendacionInicio = null!;
    private Label lblPracticaRecomendacionInicio = null!;
    private Label lblContextoRecomendacionInicio = null!;
    private Label lblMetadatosRecomendacionInicio = null!;
    private Label lblRazonRecomendacionInicio = null!;
    private BotonInicio btnRecomendacionInicio = null!;

    private Panel panelActividadInicio = null!;
    private Label lblTituloActividadInicio = null!;
    private Label lblActividadVaciaInicio = null!;
    private readonly List<FilaActividadInicioVisual> filasActividadInicio = new();

    private void InicializarEstructuraInicio() {
        if (estructuraInicioInicializada) {
            return;
        }

        resumenAprendizajeInicioService = new ResumenAprendizajeService();
        presentadorLogrosService = new PresentadorLogrosService();
        presentadorInicioService = new PresentadorInicioService();
        coordinadorCargaInicio = new CoordinadorCargaInicio(
            resumenAprendizajeInicioService.CrearResumenAsync,
            CargarMotivacionInicioAsync,
            presentadorInicioService,
            presentadorLogrosService);
        cancelacionCargaInicio = new CancellationTokenSource();

        panelInicioVista.SuspendLayout();

        try {
            panelInicioVista.Controls.Clear();
            panelInicioVista.BackColor = ColorFondoInicio;

            desplazamientoInicio = new PanelDesplazableSinBarras {
                Name = "desplazamientoInicio",
                BackColor = ColorFondoInicio,
                ColorFondoContenido = ColorFondoInicio,
                Padding = Padding.Empty,
                MostrarBordeFoco = false,
                AccessibleName = "Panel Inicio",
                AccessibleDescription =
                    "Resumen desplazable de progreso, continuación y actividad."
            };
            contenidoInicio = desplazamientoInicio.Contenido;
            contenidoInicio.Name = "contenidoInicio";
            contenidoInicio.BackColor = ColorFondoInicio;
            contenidoInicio.WrapContents = false;
            contenidoInicio.FlowDirection = FlowDirection.TopDown;

            ConstruirEncabezadoInicio();
            ConstruirBandaDatosInicio();
            ConstruirBandaLogroNuevoInicio();
            ConstruirFilaPrincipalInicio();
            ConstruirMetricasInicio();
            ConstruirFilaInferiorInicio();

            contenidoInicio.Controls.Add(panelEncabezadoInicio);
            contenidoInicio.Controls.Add(panelBandaDatosInicio);
            contenidoInicio.Controls.Add(panelBandaLogroNuevoInicio);
            contenidoInicio.Controls.Add(panelFilaPrincipalInicio);
            contenidoInicio.Controls.Add(panelMetricasInicio);
            contenidoInicio.Controls.Add(panelFilaInferiorInicio);
            panelInicioVista.Controls.Add(desplazamientoInicio);
            estructuraInicioInicializada = true;
            panelInicioVista.VisibleChanged +=
                PanelInicioVista_NotificacionLogrosVisibleChanged;
            Disposed += (_, _) => CerrarNotificacionesLogrosAlCerrar();
        } finally {
            panelInicioVista.ResumeLayout(performLayout: false);
        }

        AplicarEstadoCargaInicio(
            PresentadorInicioService.CrearEstadoCargando());
        ActualizarGeometriaInicio();
    }

    private Task<ResumenMotivacion?> CargarMotivacionInicioAsync(
        CancellationToken cancellationToken) {
        return Task.Run<ResumenMotivacion?>(() => {
            cancellationToken.ThrowIfCancellationRequested();
            ResultadoProcesamientoMotivacion resultado =
                motivacionService.ReconciliarEstadoActual();
            cancellationToken.ThrowIfCancellationRequested();
            return PrepararResumenMotivacionInicio(resultado);
        }, cancellationToken);
    }

    internal static ResumenMotivacion PrepararResumenMotivacionInicio(
        ResultadoProcesamientoMotivacion resultado) {
        ArgumentNullException.ThrowIfNull(resultado);

        if (resultado.Error is not null &&
            resultado.Resumen.Error is null) {
            return resultado.Resumen with {
                Error = resultado.Error
            };
        }

        return resultado.Resumen;
    }

    private void ConstruirEncabezadoInicio() {
        panelEncabezadoInicio = CrearContenedorInicio("panelEncabezadoInicio");
        lblSaludoInicio = CrearLabelInicio(
            "Buenos días",
            27F,
            FontStyle.Bold,
            Color.White,
            ColorFondoInicio);
        lblSubtituloDashboardInicio = CrearLabelInicio(
            "Continúa construyendo tus habilidades en C++.",
            11F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorFondoInicio);
        panelEncabezadoInicio.Controls.Add(lblSaludoInicio);
        panelEncabezadoInicio.Controls.Add(lblSubtituloDashboardInicio);
    }

    private void ConstruirBandaDatosInicio() {
        panelBandaDatosInicio = CrearTarjetaInicio(
            "panelBandaDatosInicio",
            ColorBandaInicio);
        lblTituloBandaDatosInicio = CrearLabelInicio(
            "Actualizando Inicio",
            10.5F,
            FontStyle.Bold,
            Color.White,
            ColorBandaInicio);
        lblMensajeBandaDatosInicio = CrearLabelInicio(
            "Actualizando tu progreso...",
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorBandaInicio);
        btnReintentarInicio = CrearBotonInicio(
            "Reintentar",
            esPrimario: false,
            tabIndex: 0);
        btnReintentarInicio.Click += AccionInicio_Click;
        panelBandaDatosInicio.Controls.Add(lblTituloBandaDatosInicio);
        panelBandaDatosInicio.Controls.Add(lblMensajeBandaDatosInicio);
        panelBandaDatosInicio.Controls.Add(btnReintentarInicio);
    }

    private void ConstruirBandaLogroNuevoInicio() {
        panelBandaLogroNuevoInicio = CrearTarjetaInicio(
            "panelBandaLogroNuevoInicio",
            Color.FromArgb(49, 35, 68));
        panelBandaLogroNuevoInicio.Visible = false;
        panelBandaLogroNuevoInicio.AccessibleName =
            "Notificación de logro nuevo";
        lblBandaLogroNuevoInicio = CrearLabelInicio(
            string.Empty,
            10F,
            FontStyle.Bold,
            Color.White,
            panelBandaLogroNuevoInicio.BackColor);
        btnVerLogrosBandaInicio = CrearBotonInicio(
            "Ver logros",
            esPrimario: true,
            tabIndex: 5);
        btnVerLogrosBandaInicio.AccessibleName =
            "Ver logros desbloqueados";
        btnVerLogrosBandaInicio.AccessibleDescription =
            "Abre la vista de logros para consultar el avance actualizado.";
        btnVerLogrosBandaInicio.Click += (_, _) =>
            MostrarLogrosDesdeInicio();
        btnCerrarBandaLogroInicio = CrearBotonInicio(
            "Cerrar",
            esPrimario: false,
            tabIndex: 6);
        btnCerrarBandaLogroInicio.AccessibleName =
            "Cerrar notificación de logro";
        btnCerrarBandaLogroInicio.AccessibleDescription =
            "Oculta este aviso sin abrir la vista de logros.";
        btnCerrarBandaLogroInicio.Click += (_, _) =>
            OcultarNotificacionLogrosInicio();
        panelBandaLogroNuevoInicio.Controls.Add(lblBandaLogroNuevoInicio);
        panelBandaLogroNuevoInicio.Controls.Add(btnVerLogrosBandaInicio);
        panelBandaLogroNuevoInicio.Controls.Add(btnCerrarBandaLogroInicio);

        timerNotificacionLogrosInicio = new System.Windows.Forms.Timer {
            Interval = 8000
        };
        timerNotificacionLogrosInicio.Tick += (_, _) => {
            OcultarNotificacionLogrosInicio();
            IntentarMostrarNotificacionLogrosPendiente();
        };
    }

    private void ConstruirFilaPrincipalInicio() {
        panelFilaPrincipalInicio = CrearContenedorInicio("panelFilaPrincipalInicio");
        panelContinuacionInicio = CrearTarjetaInicio(
            "panelContinuacionInicio",
            ColorTarjetaDestacadaInicio);
        lblSeccionContinuacionInicio = CrearLabelInicio(
            "CONTINUAR",
            8.5F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaDestacadaInicio);
        lblTituloContinuacionInicio = CrearLabelInicio(
            "Cargando tu resumen...",
            17F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaDestacadaInicio);
        lblGradoContinuacionInicio = CrearLabelInicio(
            string.Empty,
            9.5F,
            FontStyle.Bold,
            ColorTextoSecundarioInicio,
            ColorTarjetaDestacadaInicio);
        lblTemaContinuacionInicio = CrearLabelInicio(
            string.Empty,
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaDestacadaInicio);
        lblPracticaContinuacionInicio = CrearLabelInicio(
            "Preparando la siguiente acción.",
            12F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaDestacadaInicio);
        lblEstadoContinuacionInicio = CrearLabelInicio(
            string.Empty,
            9F,
            FontStyle.Bold,
            ColorExitoInicio,
            ColorTarjetaDestacadaInicio);
        lblRutaContinuacionInicio = CrearLabelInicio(
            string.Empty,
            9F,
            FontStyle.Regular,
            ColorTextoTenueInicio,
            ColorTarjetaDestacadaInicio);
        btnContinuarInicio = CrearBotonInicio(
            "Continuar",
            esPrimario: true,
            tabIndex: 1);
        btnContinuarInicio.Click += AccionInicio_Click;
        btnContinuarInicio.Enabled = false;
        btnAccionSecundariaInicio = CrearBotonInicio(
            "Ver estadísticas",
            esPrimario: false,
            tabIndex: 2);
        btnAccionSecundariaInicio.Click += AccionInicio_Click;
        btnAccionSecundariaInicio.Visible = false;

        panelContinuacionInicio.Controls.Add(lblSeccionContinuacionInicio);
        panelContinuacionInicio.Controls.Add(lblTituloContinuacionInicio);
        panelContinuacionInicio.Controls.Add(lblGradoContinuacionInicio);
        panelContinuacionInicio.Controls.Add(lblTemaContinuacionInicio);
        panelContinuacionInicio.Controls.Add(lblPracticaContinuacionInicio);
        panelContinuacionInicio.Controls.Add(lblEstadoContinuacionInicio);
        panelContinuacionInicio.Controls.Add(lblRutaContinuacionInicio);
        panelContinuacionInicio.Controls.Add(btnContinuarInicio);
        panelContinuacionInicio.Controls.Add(btnAccionSecundariaInicio);

        panelProgresoInicio = CrearTarjetaInicio(
            "panelProgresoInicio",
            ColorTarjetaInicio);
        lblTituloProgresoInicio = CrearLabelInicio(
            "PROGRESO GENERAL",
            8.5F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaInicio);
        lblPracticasProgresoInicio = CrearLabelInicio(
            "—",
            18F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaInicio);
        lblPorcentajeProgresoInicio = CrearLabelInicio(
            "Cargando...",
            10F,
            FontStyle.Bold,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio,
            ContentAlignment.MiddleRight);
        panelPistaProgresoInicio = new Panel {
            BackColor = Color.FromArgb(53, 45, 67)
        };
        panelRellenoProgresoInicio = new Panel {
            BackColor = ColorMoradoCurso
        };
        panelPistaProgresoInicio.Controls.Add(panelRellenoProgresoInicio);
        lblTemasProgresoInicio = CrearLabelInicio(
            "Temas: —",
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio);
        lblGradosProgresoInicio = CrearLabelInicio(
            "Grados: —",
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio,
            ContentAlignment.MiddleRight);
        lblNivelInicio = CrearLabelInicio(
            "Cargando nivel...",
            10.5F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaInicio);
        lblXpTotalInicio = CrearLabelInicio(
            string.Empty,
            9.5F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaInicio,
            ContentAlignment.MiddleRight);
        panelPistaNivelInicio = new Panel {
            BackColor = Color.FromArgb(53, 45, 67),
            Visible = false,
            AccessibleName = "Progreso al siguiente nivel"
        };
        panelRellenoNivelInicio = new Panel {
            BackColor = ColorMoradoCurso
        };
        panelPistaNivelInicio.Controls.Add(panelRellenoNivelInicio);
        lblXpRestanteInicio = CrearLabelInicio(
            "Preparando tu experiencia...",
            8.5F,
            FontStyle.Regular,
            ColorTextoTenueInicio,
            ColorTarjetaInicio);
        panelSeparadorMotivacionInicio = new Panel {
            BackColor = Color.FromArgb(58, 48, 72),
            AccessibleRole = AccessibleRole.Separator
        };
        lblTituloRachaInicio = CrearLabelInicio(
            "RACHA",
            7.75F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaInicio);
        lblValorRachaInicio = CrearLabelInicio(
            "—",
            9.25F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaInicio);
        lblDetalleRachaInicio = CrearLabelInicio(
            "Temporalmente no disponible",
            7.75F,
            FontStyle.Regular,
            ColorTextoTenueInicio,
            ColorTarjetaInicio);
        lblTituloLogrosInicio = CrearLabelInicio(
            "LOGROS",
            7.75F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaInicio);
        lblValorLogrosInicio = CrearLabelInicio(
            "—",
            9F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaInicio,
            ContentAlignment.MiddleRight);
        btnVerLogrosInicio = CrearBotonInicio(
            "Ver logros",
            esPrimario: false,
            tabIndex: 4);
        btnVerLogrosInicio.AccessibleName = "Ver logros";
        btnVerLogrosInicio.AccessibleDescription =
            "Abre la lista de logros desbloqueados y pendientes.";
        btnVerLogrosInicio.Click += (_, _) => MostrarLogrosDesdeInicio();
        btnVerLogrosInicio.Enabled = false;

        panelProgresoInicio.Controls.Add(lblTituloProgresoInicio);
        panelProgresoInicio.Controls.Add(lblPracticasProgresoInicio);
        panelProgresoInicio.Controls.Add(lblPorcentajeProgresoInicio);
        panelProgresoInicio.Controls.Add(panelPistaProgresoInicio);
        panelProgresoInicio.Controls.Add(lblTemasProgresoInicio);
        panelProgresoInicio.Controls.Add(lblGradosProgresoInicio);
        panelProgresoInicio.Controls.Add(lblNivelInicio);
        panelProgresoInicio.Controls.Add(lblXpTotalInicio);
        panelProgresoInicio.Controls.Add(panelPistaNivelInicio);
        panelProgresoInicio.Controls.Add(lblXpRestanteInicio);
        panelProgresoInicio.Controls.Add(panelSeparadorMotivacionInicio);
        panelProgresoInicio.Controls.Add(lblTituloRachaInicio);
        panelProgresoInicio.Controls.Add(lblValorRachaInicio);
        panelProgresoInicio.Controls.Add(lblDetalleRachaInicio);
        panelProgresoInicio.Controls.Add(lblTituloLogrosInicio);
        panelProgresoInicio.Controls.Add(lblValorLogrosInicio);
        panelProgresoInicio.Controls.Add(btnVerLogrosInicio);

        panelFilaPrincipalInicio.Controls.Add(panelContinuacionInicio);
        panelFilaPrincipalInicio.Controls.Add(panelProgresoInicio);
    }

    private void ConstruirMetricasInicio() {
        panelMetricasInicio = CrearContenedorInicio("panelMetricasInicio");
        string[] titulos = {
            "Evaluaciones aprobadas",
            "Promedio de mejores calificaciones",
            "Mejor calificación",
            "Prácticas en progreso"
        };

        foreach (string titulo in titulos) {
            Panel tarjeta = CrearTarjetaInicio(
                $"panelMetricaInicio{tarjetasMetricasInicio.Count + 1}",
                ColorTarjetaInicio);
            Label lblTitulo = CrearLabelInicio(
                titulo.ToUpperInvariant(),
                8F,
                FontStyle.Bold,
                ColorTextoTenueInicio,
                ColorTarjetaInicio);
            Label lblValor = CrearLabelInicio(
                "—",
                17F,
                FontStyle.Bold,
                Color.White,
                ColorTarjetaInicio);
            Label lblDescripcion = CrearLabelInicio(
                "Cargando...",
                8.5F,
                FontStyle.Regular,
                ColorTextoSecundarioInicio,
                ColorTarjetaInicio);
            tarjeta.Controls.Add(lblTitulo);
            tarjeta.Controls.Add(lblValor);
            tarjeta.Controls.Add(lblDescripcion);
            panelMetricasInicio.Controls.Add(tarjeta);
            tarjetasMetricasInicio.Add(new TarjetaMetricaInicioVisual(
                tarjeta,
                lblTitulo,
                lblValor,
                lblDescripcion));
        }
    }

    private void ConstruirFilaInferiorInicio() {
        panelFilaInferiorInicio = CrearContenedorInicio("panelFilaInferiorInicio");
        panelRecomendacionInicio = CrearTarjetaInicio(
            "panelRecomendacionInicio",
            ColorTarjetaInicio);
        lblTituloRecomendacionInicio = CrearLabelInicio(
            "SIGUIENTE PRÁCTICA",
            8.5F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaInicio);
        lblPracticaRecomendacionInicio = CrearLabelInicio(
            "Buscando una recomendación...",
            15F,
            FontStyle.Bold,
            Color.White,
            ColorTarjetaInicio);
        lblContextoRecomendacionInicio = CrearLabelInicio(
            string.Empty,
            9F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio);
        lblMetadatosRecomendacionInicio = CrearLabelInicio(
            string.Empty,
            9F,
            FontStyle.Bold,
            ColorExitoInicio,
            ColorTarjetaInicio);
        lblRazonRecomendacionInicio = CrearLabelInicio(
            string.Empty,
            9.5F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio);
        btnRecomendacionInicio = CrearBotonInicio(
            "Ver siguiente práctica",
            esPrimario: true,
            tabIndex: 3);
        btnRecomendacionInicio.Click += AccionInicio_Click;
        btnRecomendacionInicio.Enabled = false;

        panelRecomendacionInicio.Controls.Add(lblTituloRecomendacionInicio);
        panelRecomendacionInicio.Controls.Add(lblPracticaRecomendacionInicio);
        panelRecomendacionInicio.Controls.Add(lblContextoRecomendacionInicio);
        panelRecomendacionInicio.Controls.Add(lblMetadatosRecomendacionInicio);
        panelRecomendacionInicio.Controls.Add(lblRazonRecomendacionInicio);
        panelRecomendacionInicio.Controls.Add(btnRecomendacionInicio);

        panelActividadInicio = CrearTarjetaInicio(
            "panelActividadInicio",
            ColorTarjetaInicio);
        lblTituloActividadInicio = CrearLabelInicio(
            "ACTIVIDAD RECIENTE APROXIMADA",
            8.5F,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ColorTarjetaInicio);
        lblActividadVaciaInicio = CrearLabelInicio(
            "Todavía no hay actividad registrada.",
            10F,
            FontStyle.Regular,
            ColorTextoSecundarioInicio,
            ColorTarjetaInicio);
        panelActividadInicio.Controls.Add(lblTituloActividadInicio);
        panelActividadInicio.Controls.Add(lblActividadVaciaInicio);

        for (int indice = 0; indice < 3; indice++) {
            Panel fila = new() {
                BackColor = Color.FromArgb(38, 32, 51),
                Visible = false
            };
            Label fecha = CrearLabelInicio(
                string.Empty,
                8.5F,
                FontStyle.Bold,
                ColorMoradoClaroCurso,
                fila.BackColor);
            Label descripcion = CrearLabelInicio(
                string.Empty,
                9F,
                FontStyle.Regular,
                Color.White,
                fila.BackColor);
            fila.Controls.Add(fecha);
            fila.Controls.Add(descripcion);
            panelActividadInicio.Controls.Add(fila);
            filasActividadInicio.Add(new FilaActividadInicioVisual(
                fila,
                fecha,
                descripcion));
        }

        panelFilaPrincipalInicio.Controls.Add(panelActividadInicio);
        panelFilaInferiorInicio.Controls.Add(panelRecomendacionInicio);
    }

    private void PrepararInicioParaMostrar() {
        if (!estructuraInicioInicializada) {
            return;
        }

        ActualizarGeometriaInicio();

        if (!inicializacionSecundariaCompletada ||
            !IsHandleCreated ||
            inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing) {
            return;
        }

        if (inicioPendienteRecarga && !coordinadorCargaInicio.CargaEnCurso) {
            _ = RecargarInicioAsync();
        } else {
            IntentarMostrarNotificacionLogrosPendiente();
        }
    }

    private void MarcarInicioPendienteDeRecarga() {
        if (!estructuraInicioInicializada) {
            return;
        }

        inicioPendienteRecarga = true;

        if (coordinadorCargaInicio.CargaEnCurso) {
            inicioRecargaSolicitadaDuranteCarga = true;
            return;
        }

        if (EstaVisibleFamiliaInicio() &&
            inicializacionSecundariaCompletada &&
            IsHandleCreated &&
            !inicializacionSecundariaCancelada &&
            !IsDisposed &&
            !Disposing) {
            ProgramarAccionInterfazSegura(() => _ = RecargarInicioAsync());
        }
    }

    private async Task RecargarInicioAsync() {
        if (!PuedeActualizarInterfazInicio() ||
            coordinadorCargaInicio.CargaEnCurso) {
            inicioRecargaSolicitadaDuranteCarga |=
                coordinadorCargaInicio.CargaEnCurso;
            return;
        }

        inicioPendienteRecarga = false;
        inicioRecargaSolicitadaDuranteCarga = false;
        AplicarEstadoCargaInicio(
            PresentadorInicioService.CrearEstadoCargando());

        Task<ResultadoCargaInicio> carga =
            coordinadorCargaInicio.RecargarAsync(
                cancelacionCargaInicio?.Token ?? CancellationToken.None);
        tareaCargaInicio = carga;
        ResultadoCargaInicio resultado = await carga;

        try {
            if (!PuedeActualizarInterfazInicio() ||
                inicioRecargaSolicitadaDuranteCarga ||
                !coordinadorCargaInicio.PuedeAplicar(resultado)) {
                return;
            }

            if (resultado.Estado == EstadoResultadoCargaInicio.Completada &&
                resultado.Presentacion is not null) {
                if (resultado.AdvertenciaMotivacion is not null) {
                    Program.RegistrarErrorRecuperable(
                        resultado.AdvertenciaMotivacion);
                }

                ultimaPresentacionInicio = resultado.Presentacion;
                AplicarPresentacionInicio(resultado.Presentacion);
                if (resultado.Logros is not null) {
                    ultimaPresentacionLogros = resultado.Logros;
                    AplicarPresentacionLogros(resultado.Logros);
                }
                inicioPendienteRecarga =
                    resultado.Presentacion.Nivel.Estado ==
                        EstadoNivelInicio.NoDisponible ||
                    resultado.AdvertenciaMotivacion is not null;
                AplicarEstadoCargaInicio(
                    PresentadorInicioService.CrearEstadoInactivo());
                IntentarMostrarNotificacionLogrosPendiente();
            } else if (
                resultado.Estado ==
                    EstadoResultadoCargaInicio.ErrorRecuperable) {
                if (resultado.Error is not null) {
                    Program.RegistrarErrorRecuperable(resultado.Error);
                }

                inicioPendienteRecarga = true;
                AplicarEstadoCargaInicio(
                    PresentadorInicioService.CrearEstadoErrorRecuperable());
            }
        } finally {
            if (ReferenceEquals(tareaCargaInicio, carga)) {
                tareaCargaInicio = null;
            }

            if (inicioRecargaSolicitadaDuranteCarga &&
                PuedeActualizarInterfazInicio() &&
                EstaVisibleFamiliaInicio()) {
                inicioRecargaSolicitadaDuranteCarga = false;
                ProgramarAccionInterfazSegura(() => _ = RecargarInicioAsync());
            }
        }
    }

    private bool PuedeActualizarInterfazInicio() {
        return coordinadorCierreOperaciones.PuedeActualizarInterfaz &&
            estructuraInicioInicializada &&
            !inicializacionSecundariaCancelada &&
            !esperandoCierreOperaciones &&
            !IsDisposed &&
            !Disposing &&
            IsHandleCreated;
    }

    private bool EstaVisibleFamiliaInicio() {
        return panelInicioVista.Visible ||
            (estructuraLogrosInicializada && panelLogrosVista.Visible);
    }

    private void CancelarCargaInicioAlCerrar() {
        CerrarNotificacionesLogrosAlCerrar();

        if (!estructuraInicioInicializada) {
            return;
        }

        coordinadorCargaInicio.Cerrar();

        try {
            cancelacionCargaInicio?.Cancel();
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            Program.RegistrarErrorRecuperable(ex);
        } finally {
            cancelacionCargaInicio?.Dispose();
            cancelacionCargaInicio = null;
        }
    }

    private void RegistrarLogrosNuevosParaNotificacion(
        IReadOnlyList<LogroDesbloqueado> logros) {
        if (logros.Count == 0) {
            return;
        }

        coordinadorNotificacionesLogros.Registrar(logros);
    }

    private void IntentarMostrarNotificacionLogrosPendiente() {
        IReadOnlyList<LogroDesbloqueado> pendientes =
            coordinadorNotificacionesLogros.ConsultarPendientes();
        bool puedeMostrar = PuedeMostrarNotificacionLogros(
            entradaAplicacionRealizada,
            coordinadorCierreOperaciones.CierreSolicitado,
            PuedeActualizarInterfazInicio(),
            panelInicioVista.Visible,
            panelBandaDatosInicio.Visible,
            panelBandaLogroNuevoInicio.Visible,
            pendientes.Count);

        if (!puedeMostrar || ultimaPresentacionLogros is null) {
            return;
        }

        Dictionary<string, string> nombres = ultimaPresentacionLogros.Logros
            .ToDictionary(
                logro => logro.Id,
                logro => logro.Nombre,
                StringComparer.OrdinalIgnoreCase);
        IReadOnlyList<LogroDesbloqueado> consumidos =
            coordinadorNotificacionesLogros.ConsumirPendientes();
        LogroDesbloqueado[] logrosConocidos = consumidos
            .Where(logro => nombres.ContainsKey(logro.LogroId))
            .ToArray();
        string[] nombresConocidos = logrosConocidos
            .Select(logro => nombres[logro.LogroId])
            .ToArray();

        if (nombresConocidos.Length == 0) {
            return;
        }

        loteNotificacionLogrosVisible = Array.AsReadOnly(logrosConocidos);
        string mensaje = CrearTextoNotificacionLogros(nombresConocidos);
        lblBandaLogroNuevoInicio.Text = mensaje;
        panelBandaLogroNuevoInicio.AccessibleDescription = mensaje;
        panelBandaLogroNuevoInicio.Visible = true;
        timerNotificacionLogrosInicio.Stop();
        timerNotificacionLogrosInicio.Start();
        ultimoAnchoContenidoInicio = -1;
        ActualizarGeometriaInicio();
    }

    internal static bool PuedeMostrarNotificacionLogros(
        bool entradaRealizada,
        bool cierreSolicitado,
        bool puedeActualizarInterfaz,
        bool inicioVisible,
        bool bandaPrioritariaVisible,
        bool notificacionYaVisible,
        int cantidadPendientes) {
        return entradaRealizada &&
            !cierreSolicitado &&
            puedeActualizarInterfaz &&
            inicioVisible &&
            !bandaPrioritariaVisible &&
            !notificacionYaVisible &&
            cantidadPendientes > 0;
    }

    internal static string CrearTextoNotificacionLogros(
        IReadOnlyList<string> nombres) {
        ArgumentNullException.ThrowIfNull(nombres);

        if (nombres.Count == 0) {
            return string.Empty;
        }

        return nombres.Count == 1
            ? $"Nuevo logro: {nombres[0]}"
            : $"{nombres.Count} logros nuevos · " +
                $"{nombres[0]} y {nombres.Count - 1} más";
    }

    private void OcultarNotificacionLogrosInicio(
        bool conservarPendientes = false) {
        if (!estructuraInicioInicializada ||
            panelBandaLogroNuevoInicio is null) {
            return;
        }

        timerNotificacionLogrosInicio?.Stop();

        if (conservarPendientes && loteNotificacionLogrosVisible.Count > 0) {
            coordinadorNotificacionesLogros.ReponerPendientesAlInicio(
                loteNotificacionLogrosVisible);
        }

        loteNotificacionLogrosVisible = Array.Empty<LogroDesbloqueado>();

        if (panelBandaLogroNuevoInicio.Visible) {
            panelBandaLogroNuevoInicio.Visible = false;
            ultimoAnchoContenidoInicio = -1;
            ActualizarGeometriaInicio();
        }
    }

    private void CerrarNotificacionesLogrosAlCerrar() {
        if (notificacionesLogrosCerradas) {
            return;
        }

        notificacionesLogrosCerradas = true;
        loteNotificacionLogrosVisible = Array.Empty<LogroDesbloqueado>();
        coordinadorNotificacionesLogros.Cerrar();

        if (timerNotificacionLogrosInicio is not null) {
            timerNotificacionLogrosInicio.Stop();
            timerNotificacionLogrosInicio.Dispose();
        }
    }

    private void PanelInicioVista_NotificacionLogrosVisibleChanged(
        object? sender,
        EventArgs e) {
        if (!estructuraInicioInicializada || notificacionesLogrosCerradas) {
            return;
        }

        if (Disposing || IsDisposed) {
            CerrarNotificacionesLogrosAlCerrar();
            return;
        }

        if (!IsHandleCreated) {
            return;
        }

        if (!panelInicioVista.Visible) {
            OcultarNotificacionLogrosInicio(conservarPendientes: true);
            return;
        }

        IntentarMostrarNotificacionLogrosPendiente();

        if (enfocarVerLogrosInicioAlMostrar) {
            enfocarVerLogrosInicioAlMostrar = false;
            ProgramarAccionInterfazSegura(() => {
                if (panelInicioVista.Visible &&
                    !btnVerLogrosInicio.IsDisposed &&
                    btnVerLogrosInicio.CanFocus) {
                    btnVerLogrosInicio.Focus();
                }
            });
        }
    }

    private void AplicarEstadoCargaInicio(
        EstadoCargaInicioPresentable estado) {
        if (!estructuraInicioInicializada) {
            return;
        }

        if (estado.Estado != EstadoCargaInicio.Inactivo) {
            OcultarNotificacionLogrosInicio(conservarPendientes: true);
        }

        if (estado.Estado == EstadoCargaInicio.Inactivo) {
            if (ultimaPresentacionInicio?.BandaDatos is null) {
                panelBandaDatosInicio.Visible = false;
            }

            return;
        }

        panelBandaDatosInicio.Visible = true;
        lblTituloBandaDatosInicio.Text = estado.Estado ==
            EstadoCargaInicio.Cargando
                ? "Actualizando Inicio"
                : "No pudimos actualizar Inicio";
        lblMensajeBandaDatosInicio.Text = estado.Mensaje;
        ConfigurarAccionBoton(
            btnReintentarInicio,
            estado.MostrarReintentar
                ? new AccionInicioPresentable(
                    TipoAccionInicio.Reintentar,
                    "Reintentar",
                    "Reintentar carga de Inicio",
                    "Vuelve a cargar el resumen de aprendizaje.")
                : null);
        ultimoAnchoContenidoInicio = -1;
        ActualizarGeometriaInicio();
    }

    private void AplicarPresentacionInicio(PresentacionInicio presentacion) {
        lblSaludoInicio.Text = presentacion.Encabezado.Saludo;
        lblSubtituloDashboardInicio.Text = presentacion.Encabezado.Subtitulo;

        ContinuacionInicioPresentable continuacion = presentacion.Continuacion;
        lblTituloContinuacionInicio.Text = continuacion.Titulo;
        lblGradoContinuacionInicio.Text = continuacion.TextoGrado;
        lblTemaContinuacionInicio.Text = continuacion.TextoTema;
        lblPracticaContinuacionInicio.Text = continuacion.TextoPractica;
        lblEstadoContinuacionInicio.Text = continuacion.TextoEstado;
        lblRutaContinuacionInicio.Text = continuacion.TextoRuta;
        ConfigurarAccionBoton(
            btnContinuarInicio,
            continuacion.AccionPrincipal);
        ConfigurarAccionBoton(
            btnAccionSecundariaInicio,
            continuacion.AccionesSecundarias.FirstOrDefault());

        ProgresoInicioPresentable progreso = presentacion.Progreso;
        lblPracticasProgresoInicio.Text =
            progreso.PracticasRealizadas.Texto;
        lblPorcentajeProgresoInicio.Text = progreso.Porcentaje.Texto;
        lblTemasProgresoInicio.Text =
            $"Temas: {progreso.TemasCompletados.Texto}";
        lblGradosProgresoInicio.Text =
            $"Grados: {progreso.GradosCompletados.Texto}";
        valorBarraProgresoInicio = Math.Clamp(
            progreso.ValorBarra ?? 0,
            0,
            100);
        panelPistaProgresoInicio.Visible = progreso.ValorBarra.HasValue;
        AplicarNivelInicio(presentacion.Nivel);
        AplicarMotivacionInicio(presentacion.Motivacion);

        for (int indice = 0; indice < tarjetasMetricasInicio.Count; indice++) {
            TarjetaMetricaInicioVisual visual = tarjetasMetricasInicio[indice];

            if (indice >= presentacion.Metricas.Count) {
                visual.Valor.Text = "—";
                visual.Descripcion.Text = "No disponible";
                continue;
            }

            MetricaInicioPresentable metrica = presentacion.Metricas[indice];
            visual.Titulo.Text = metrica.Titulo.ToUpperInvariant();
            visual.Valor.Text = metrica.Dato.Texto;
            visual.Descripcion.Text = ObtenerDescripcionCortaDatoInicio(
                metrica.Dato);
            visual.Tarjeta.AccessibleName = metrica.Titulo;
            visual.Tarjeta.AccessibleDescription = metrica.Dato.Descripcion;
        }

        AplicarRecomendacionInicio(presentacion.Recomendacion);
        AplicarActividadesInicio(presentacion.Actividades);

        if (presentacion.BandaDatos is not null) {
            BandaDatosInicioPresentable banda = presentacion.BandaDatos;
            panelBandaDatosInicio.Visible = true;
            lblTituloBandaDatosInicio.Text = banda.Titulo;
            lblMensajeBandaDatosInicio.Text = banda.Mensaje;
            ConfigurarAccionBoton(
                btnReintentarInicio,
                banda.AccionReintentar);
        } else {
            panelBandaDatosInicio.Visible = false;
            ConfigurarAccionBoton(btnReintentarInicio, null);
        }

        ultimoAnchoContenidoInicio = -1;
        ActualizarGeometriaInicio();
    }

    private void AplicarNivelInicio(PresentacionNivel nivel) {
        lblNivelInicio.Text = nivel.TextoNivel;
        lblXpTotalInicio.Text = nivel.TextoXpTotal;
        lblXpRestanteInicio.Text = nivel.TextoXpRestante;
        valorBarraNivelInicio = Math.Clamp(nivel.ValorBarra ?? 0, 0, 100);
        panelPistaNivelInicio.Visible = nivel.ValorBarra.HasValue;
        panelPistaNivelInicio.AccessibleDescription =
            nivel.DescripcionAccesible;
        panelProgresoInicio.AccessibleDescription =
            nivel.DescripcionAccesible;
    }

    private void AplicarMotivacionInicio(
        PresentacionMotivacionInicio motivacion) {
        lblValorRachaInicio.Text = motivacion.Racha.TextoValor;
        lblDetalleRachaInicio.Text = motivacion.Racha.TextoDetalle;
        lblValorLogrosInicio.Text = motivacion.Logros.TextoValor;
        lblTituloRachaInicio.AccessibleName = "Racha de estudio";
        lblTituloRachaInicio.AccessibleDescription =
            motivacion.Racha.DescripcionAccesible;
        lblTituloLogrosInicio.AccessibleName = "Resumen de logros";
        lblTituloLogrosInicio.AccessibleDescription =
            motivacion.Logros.DescripcionAccesible;
        panelProgresoInicio.AccessibleDescription =
            $"{panelProgresoInicio.AccessibleDescription} " +
            $"{motivacion.Racha.DescripcionAccesible} " +
            motivacion.Logros.DescripcionAccesible;
    }

    private void AplicarRecomendacionInicio(
        RecomendacionInicioPresentable? recomendacion) {
        lblTituloRecomendacionInicio.Text =
            recomendacion?.TituloSeccion ?? "SIGUIENTE PRÁCTICA";

        if (recomendacion is null) {
            lblPracticaRecomendacionInicio.Text =
                "No hay una práctica pendiente";
            lblContextoRecomendacionInicio.Text =
                "Puedes revisar tu ruta o consultar tus estadísticas.";
            lblMetadatosRecomendacionInicio.Text = string.Empty;
            lblRazonRecomendacionInicio.Text = string.Empty;
            ConfigurarAccionBoton(btnRecomendacionInicio, null);
            return;
        }

        lblPracticaRecomendacionInicio.Text =
            recomendacion.TextoPractica;
        lblContextoRecomendacionInicio.Text =
            $"{recomendacion.TextoGrado}   ·   {recomendacion.TextoTema}";
        lblMetadatosRecomendacionInicio.Text =
            $"{recomendacion.Dificultad}   ·   {recomendacion.DuracionEstimada}";
        lblRazonRecomendacionInicio.Text = recomendacion.Razon;
        ConfigurarAccionBoton(
            btnRecomendacionInicio,
            recomendacion.Accion);
    }

    private void AplicarActividadesInicio(
        IReadOnlyList<ActividadInicioPresentable> actividades) {
        int cantidad = Math.Min(filasActividadInicio.Count, actividades.Count);
        lblActividadVaciaInicio.Visible = cantidad == 0;

        for (int indice = 0; indice < filasActividadInicio.Count; indice++) {
            FilaActividadInicioVisual fila = filasActividadInicio[indice];
            bool visible = indice < cantidad;
            fila.Fila.Visible = visible;

            if (!visible) {
                fila.Fecha.Text = string.Empty;
                fila.Descripcion.Text = string.Empty;
                continue;
            }

            ActividadInicioPresentable actividad = actividades[indice];
            fila.Fecha.Text = actividad.TextoFecha;
            fila.Descripcion.Text = actividad.Texto;
        }
    }

    private static string ObtenerDescripcionCortaDatoInicio(
        DatoInicioPresentable dato) {
        return dato.Estado switch {
            EstadoDatoInicio.Parcial => "Información recuperada parcialmente",
            EstadoDatoInicio.NoDisponible => "Temporalmente no disponible",
            EstadoDatoInicio.SinDatos => "Todavía no hay datos registrados",
            _ => "Información confirmada"
        };
    }

    private async void AccionInicio_Click(object? sender, EventArgs e) {
        if (sender is not Button boton ||
            boton.Tag is not AccionInicioPresentable accion ||
            accionInicioEnCurso ||
            (accion.Tipo == TipoAccionInicio.VerRutaAprendizaje &&
             entradaCursoPendiente)) {
            return;
        }

        long solicitudNavegacion = RegistrarSolicitudCargaNavegacion();
        accionInicioEnCurso = true;
        ActualizarEstadoBotonesInicio();

        try {
            switch (accion.Tipo) {
                case TipoAccionInicio.ContinuarPractica:
                    await ContinuarPracticaDesdeInicioAsync(
                        accion,
                        solicitudNavegacion);
                    break;
                case TipoAccionInicio.VerPractica:
                    if (accion.Practica is not null) {
                        await AbrirPracticaCurricularDesdeInicioAsync(
                            accion.Practica,
                            solicitudNavegacion);
                    }
                    break;
                case TipoAccionInicio.VerRutaAprendizaje:
                    await MostrarRutaAprendizajeDesdeMenuAsync(
                        solicitudNavegacion);
                    break;
                case TipoAccionInicio.VerEstadisticas: {
                    Panel panelSeleccionadoAlSolicitar = panelSeleccionado;
                    await PrepararCursoParaInteraccionAsync();
                    if (PuedeCompletarAccionInicioDespuesDeEspera(
                            PuedeActualizarInterfazInicio(),
                            panelInicioVista.Visible,
                            distribucionPanelPrincipal ==
                                DistribucionPanelPrincipal.Inicio,
                            cursoPreparado,
                            ReferenceEquals(
                                panelSeleccionado,
                                panelSeleccionadoAlSolicitar)) &&
                        EsSolicitudCargaNavegacionVigente(
                            solicitudNavegacion)) {
                        await MostrarEstadisticasAsync(solicitudNavegacion);
                    }
                    break;
                }
                case TipoAccionInicio.Reintentar:
                    inicioPendienteRecarga = true;
                    await RecargarInicioAsync();
                    break;
            }
        } finally {
            accionInicioEnCurso = false;

            if (PuedeActualizarInterfazInicio()) {
                ActualizarEstadoBotonesInicio();
            }
        }
    }

    private async Task ContinuarPracticaDesdeInicioAsync(
        AccionInicioPresentable accion,
        long solicitudNavegacion) {
        if (accion.Practica is null ||
            !EsSolicitudCargaNavegacionVigente(solicitudNavegacion)) {
            return;
        }

        string ruta = accion.RutaProyecto ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(ruta) && Directory.Exists(ruta)) {
            await IntentarAbrirPracticaAsync(
                ruta,
                promoverReciente: false);
            return;
        }

        await AbrirPracticaCurricularDesdeInicioAsync(
            accion.Practica,
            solicitudNavegacion);
    }

    private async Task AbrirPracticaCurricularDesdeInicioAsync(
        ReferenciaPracticaAprendizaje referencia,
        long solicitudNavegacion) {
        Panel panelSeleccionadoAlSolicitar = panelSeleccionado;
        await PrepararCursoParaInteraccionAsync();

        if (!EsSolicitudCargaNavegacionVigente(solicitudNavegacion) ||
            !PuedeCompletarAccionInicioDespuesDeEspera(
                PuedeActualizarInterfazInicio(),
                panelInicioVista.Visible,
                distribucionPanelPrincipal ==
                    DistribucionPanelPrincipal.Inicio,
                cursoPreparado,
                ReferenceEquals(
                    panelSeleccionado,
                    panelSeleccionadoAlSolicitar)) ||
            !IntentarSeleccionarGrado(referencia.GradoId)) {
            return;
        }

        PracticaCurso? practica = cursoService.ObtenerPractica(
            referencia.PracticaId);

        if (practica is null ||
            !practica.TemaId.Equals(
                referencia.TemaId,
                StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        MostrarDetallePractica(practica);
    }

    private void ConfigurarAccionBoton(
        BotonInicio boton,
        AccionInicioPresentable? accion) {
        boton.Tag = accion;
        boton.Visible = accion is not null;
        boton.Enabled = accion is not null && !accionInicioEnCurso;

        if (accion is null) {
            boton.AccessibleName = string.Empty;
            boton.AccessibleDescription = string.Empty;
            return;
        }

        boton.Text = accion.Texto;
        boton.AccessibleName = accion.AccessibleName;
        boton.AccessibleDescription = accion.AccessibleDescription;
    }

    internal static bool PuedeCompletarAccionInicioDespuesDeEspera(
        bool puedeActualizarInterfaz,
        bool inicioVisible,
        bool distribucionInicio,
        bool cursoPreparado,
        bool opcionMenuSinCambios) {
        return puedeActualizarInterfaz &&
            inicioVisible &&
            distribucionInicio &&
            cursoPreparado &&
            opcionMenuSinCambios;
    }

    private void ActualizarEstadoBotonesInicio() {
        foreach (BotonInicio boton in new[] {
            btnReintentarInicio,
            btnContinuarInicio,
            btnAccionSecundariaInicio,
            btnRecomendacionInicio
        }) {
            boton.Enabled = boton.Tag is AccionInicioPresentable &&
                !accionInicioEnCurso;
        }

        btnVerLogrosInicio.Enabled = !accionInicioEnCurso &&
            ultimaPresentacionLogros is not null;
        btnVerLogrosBandaInicio.Enabled = !accionInicioEnCurso;
    }

    private void ActualizarGeometriaInicio() {
        if (!estructuraInicioInicializada ||
            panelInicioVista.IsDisposed ||
            (distribucionPanelPrincipal != DistribucionPanelPrincipal.Inicio &&
             !panelInicioVista.Visible) ||
            panelInicioVista.ClientSize.Width <= 0 ||
            panelInicioVista.ClientSize.Height <= 0) {
            return;
        }

        Rectangle area = panelInicioVista.ClientRectangle;
        bool modoAmplioVista = area.Width >= EscalarDiseno(1040);
        int margenHorizontal = EscalarDiseno(modoAmplioVista ? 22 : 10);
        int margenVertical = EscalarDiseno(modoAmplioVista ? 14 : 8);
        int anchoDisponible = Math.Max(1, area.Width - margenHorizontal * 2);
        int anchoViewport = Math.Min(EscalarDiseno(1500), anchoDisponible);
        int x = area.Left + margenHorizontal +
            Math.Max(0, (anchoDisponible - anchoViewport) / 2);
        AplicarBoundsSiCambian(
            desplazamientoInicio,
            new Rectangle(
                x,
                area.Top + margenVertical,
                anchoViewport,
                Math.Max(1, area.Height - margenVertical * 2)));

        Padding relleno = new(
            EscalarDiseno(modoAmplioVista ? 22 : 14),
            EscalarDiseno(modoAmplioVista ? 16 : 12),
            EscalarDiseno(modoAmplioVista ? 22 : 14),
            EscalarDiseno(28));

        bool cambioRelleno = contenidoInicio.Padding != relleno;

        if (cambioRelleno) {
            contenidoInicio.Padding = relleno;
        }

        int anchoContenido = Math.Max(
            1,
            contenidoInicio.ClientSize.Width -
                contenidoInicio.Padding.Horizontal);
        int anchoContenidoLogico = Math.Max(
            1,
            (int)Math.Round(
                anchoContenido * 96D / Math.Max(1, DeviceDpi)));
        bool modoAmplio = CalculadorLayoutInicio.DeterminarModoAmplio(
            anchoContenidoLogico);

        bool cambioGeometria =
            anchoContenido != ultimoAnchoContenidoInicio ||
            DeviceDpi != ultimoDpiInicio ||
            modoAmplio != ultimoModoAmplioInicio;

        if (cambioGeometria) {
            ActualizarGeometriaContenidoInicio(
                anchoContenido,
                anchoContenidoLogico,
                modoAmplio);
            ultimoAnchoContenidoInicio = anchoContenido;
            ultimoDpiInicio = DeviceDpi;
            ultimoModoAmplioInicio = modoAmplio;
        } else {
            ActualizarRellenoProgresoInicio();
            ActualizarRellenoNivelInicio();
        }

        if (cambioRelleno || cambioGeometria) {
            desplazamientoInicio.ActualizarContenido(volverAlInicio: false);
        }
    }

    private void ActualizarGeometriaContenidoInicio(
        int ancho,
        int anchoLogico,
        bool modoAmplio) {
        contenidoInicio.SuspendLayout();

        try {
            int cantidadActividades = filasActividadInicio.Count(
                fila => fila.Fila.Visible);
            int altoViewportLogico = Math.Max(
                1,
                (int)Math.Round(
                    desplazamientoInicio.ClientSize.Height *
                    96D / Math.Max(1, DeviceDpi)));
            MedidasLayoutInicio medidas = CalculadorLayoutInicio.Calcular(
                modoAmplio,
                anchoLogico,
                altoViewportLogico,
                cantidadActividades);
            int separacion = EscalarDiseno(medidas.Separacion);
            ConfigurarControlFlujoInicio(
                panelEncabezadoInicio,
                ancho,
                EscalarDiseno(medidas.AltoEncabezado),
                EscalarDiseno(8));
            lblSaludoInicio.SetBounds(0, 0, ancho, EscalarDiseno(44));
            lblSubtituloDashboardInicio.SetBounds(
                0,
                EscalarDiseno(44),
                ancho,
                EscalarDiseno(28));

            bool bandaCompacta = btnReintentarInicio.Visible &&
                ancho < EscalarDiseno(620);
            int altoBandaDatos = CalcularAltoBandaInicio(
                ancho,
                bandaCompacta);
            ConfigurarControlFlujoInicio(
                panelBandaDatosInicio,
                ancho,
                altoBandaDatos,
                separacion);
            ActualizarGeometriaBandaInicio(ancho, bandaCompacta);

            bool notificacionCompacta = ancho < EscalarDiseno(620);
            int altoNotificacion = EscalarDiseno(
                notificacionCompacta ? 100 : 64);
            ConfigurarControlFlujoInicio(
                panelBandaLogroNuevoInicio,
                ancho,
                altoNotificacion,
                separacion);
            ActualizarGeometriaBandaLogroNuevoInicio(
                ancho,
                notificacionCompacta);

            int altoFilaPrincipal = EscalarDiseno(
                medidas.AltoFilaPrincipal);
            ConfigurarControlFlujoInicio(
                panelFilaPrincipalInicio,
                ancho,
                altoFilaPrincipal,
                separacion);

            AplicarRectanguloLayoutInicio(
                panelContinuacionInicio,
                medidas.Continuacion,
                ancho,
                medidas.AnchoContenido);
            AplicarRectanguloLayoutInicio(
                panelProgresoInicio,
                medidas.Progreso,
                ancho,
                medidas.AnchoContenido);
            AplicarRectanguloLayoutInicio(
                panelActividadInicio,
                medidas.Actividad,
                ancho,
                medidas.AnchoContenido);

            ActualizarGeometriaContinuacionInicio();
            ActualizarGeometriaProgresoInicio();
            ActualizarGeometriaActividadInicio();

            int altoTarjetaMetrica = EscalarDiseno(
                medidas.AltoTarjetaMetrica);
            int altoMetricas = EscalarDiseno(medidas.AltoMetricas);
            ConfigurarControlFlujoInicio(
                panelMetricasInicio,
                ancho,
                altoMetricas,
                separacion);
            ActualizarGeometriaMetricasInicio(
                ancho,
                altoTarjetaMetrica,
                separacion,
                modoAmplio);

            int altoFilaInferior = EscalarDiseno(
                medidas.AltoFilaInferior);
            ConfigurarControlFlujoInicio(
                panelFilaInferiorInicio,
                ancho,
                altoFilaInferior,
                0);

            AplicarRectanguloLayoutInicio(
                panelRecomendacionInicio,
                medidas.Recomendacion,
                ancho,
                medidas.AnchoContenido);

            ActualizarGeometriaRecomendacionInicio(modoAmplio);
        } finally {
            contenidoInicio.ResumeLayout(performLayout: false);
        }
    }

    private void AplicarRectanguloLayoutInicio(
        Control control,
        RectanguloLayoutInicio rectangulo,
        int anchoReal,
        int anchoLogico) {
        RectanguloLayoutInicio fisico =
            CalculadorLayoutInicio.EscalarRectanguloFisico(
                rectangulo,
                anchoReal,
                anchoLogico,
                DeviceDpi);
        control.SetBounds(
            fisico.X,
            fisico.Y,
            fisico.Ancho,
            fisico.Alto);
    }

    private void ActualizarGeometriaBandaInicio(
        int ancho,
        bool apilarAccion) {
        int margen = EscalarDiseno(16);
        int altoBoton = EscalarDiseno(apilarAccion ? 36 : 38);
        int anchoBoton = btnReintentarInicio.Visible
            ? EscalarDiseno(126)
            : 0;
        int separacion = btnReintentarInicio.Visible && !apilarAccion
            ? EscalarDiseno(14)
            : 0;
        int anchoTexto = Math.Max(
            1,
            ancho - margen * 2 -
                (apilarAccion ? 0 : anchoBoton + separacion));
        lblTituloBandaDatosInicio.SetBounds(
            margen,
            EscalarDiseno(10),
            anchoTexto,
            EscalarDiseno(24));
        int altoMensaje = MedirAltoTextoInicio(
            lblMensajeBandaDatosInicio,
            anchoTexto,
            EscalarDiseno(20));
        lblMensajeBandaDatosInicio.SetBounds(
            margen,
            EscalarDiseno(34),
            anchoTexto,
            altoMensaje);
        btnReintentarInicio.SetBounds(
            apilarAccion
                ? margen
                : Math.Max(margen, ancho - margen - anchoBoton),
            apilarAccion
                ? lblMensajeBandaDatosInicio.Bottom + EscalarDiseno(8)
                : Math.Max(
                    margen,
                    (panelBandaDatosInicio.Height - altoBoton) / 2),
            anchoBoton,
            altoBoton);
    }

    private void ActualizarGeometriaBandaLogroNuevoInicio(
        int ancho,
        bool apilarAcciones) {
        int margen = EscalarDiseno(16);
        int separacion = EscalarDiseno(8);
        int altoBoton = EscalarDiseno(32);
        int anchoCerrar = EscalarDiseno(76);
        int anchoVer = EscalarDiseno(112);

        if (apilarAcciones) {
            lblBandaLogroNuevoInicio.SetBounds(
                margen,
                EscalarDiseno(10),
                Math.Max(1, ancho - margen * 2),
                EscalarDiseno(38));
            int yBotones = EscalarDiseno(56);
            btnCerrarBandaLogroInicio.SetBounds(
                Math.Max(margen, ancho - margen - anchoCerrar),
                yBotones,
                anchoCerrar,
                altoBoton);
            btnVerLogrosBandaInicio.SetBounds(
                Math.Max(
                    margen,
                    btnCerrarBandaLogroInicio.Left - separacion - anchoVer),
                yBotones,
                anchoVer,
                altoBoton);
            return;
        }

        btnCerrarBandaLogroInicio.SetBounds(
            Math.Max(margen, ancho - margen - anchoCerrar),
            EscalarDiseno(16),
            anchoCerrar,
            altoBoton);
        btnVerLogrosBandaInicio.SetBounds(
            Math.Max(
                margen,
                btnCerrarBandaLogroInicio.Left - separacion - anchoVer),
            btnCerrarBandaLogroInicio.Top,
            anchoVer,
            altoBoton);
        lblBandaLogroNuevoInicio.SetBounds(
            margen,
            EscalarDiseno(10),
            Math.Max(
                1,
                btnVerLogrosBandaInicio.Left - margen - separacion),
            EscalarDiseno(44));
    }

    private int CalcularAltoBandaInicio(
        int ancho,
        bool apilarAccion) {
        int margen = EscalarDiseno(16);
        int anchoBoton = btnReintentarInicio.Visible
            ? EscalarDiseno(126)
            : 0;
        int separacion = btnReintentarInicio.Visible && !apilarAccion
            ? EscalarDiseno(14)
            : 0;
        int anchoTexto = Math.Max(
            1,
            ancho - margen * 2 -
                (apilarAccion ? 0 : anchoBoton + separacion));
        int altoMensaje = MedirAltoTextoInicio(
            lblMensajeBandaDatosInicio,
            anchoTexto,
            EscalarDiseno(20));
        int inferiorMensaje = EscalarDiseno(34) + altoMensaje;
        int altoMinimo = EscalarDiseno(82);

        if (!btnReintentarInicio.Visible) {
            return Math.Max(
                altoMinimo,
                inferiorMensaje + EscalarDiseno(12));
        }

        if (apilarAccion) {
            return Math.Max(
                altoMinimo,
                inferiorMensaje +
                    EscalarDiseno(8 + 36 + 16));
        }

        return Math.Max(
            altoMinimo,
            Math.Max(
                inferiorMensaje + EscalarDiseno(12),
                EscalarDiseno(22 + 38 + 16)));
    }

    private static int MedirAltoTextoInicio(
        Label label,
        int ancho,
        int altoMinimo) {
        Size medida = TextRenderer.MeasureText(
            label.Text,
            label.Font,
            new Size(Math.Max(1, ancho), int.MaxValue),
            TextFormatFlags.WordBreak |
                TextFormatFlags.NoPrefix);
        return Math.Max(altoMinimo, medida.Height);
    }

    private void ActualizarGeometriaContinuacionInicio() {
        int margen = EscalarDiseno(18);
        int ancho = Math.Max(1, panelContinuacionInicio.Width - margen * 2);
        lblSeccionContinuacionInicio.SetBounds(
            margen,
            EscalarDiseno(12),
            ancho,
            EscalarDiseno(19));
        lblTituloContinuacionInicio.SetBounds(
            margen,
            EscalarDiseno(31),
            ancho,
            EscalarDiseno(36));
        lblGradoContinuacionInicio.SetBounds(
            margen,
            EscalarDiseno(68),
            ancho,
            EscalarDiseno(20));
        lblTemaContinuacionInicio.SetBounds(
            margen,
            EscalarDiseno(87),
            ancho,
            EscalarDiseno(20));
        lblPracticaContinuacionInicio.SetBounds(
            margen,
            EscalarDiseno(106),
            ancho,
            EscalarDiseno(28));
        int anchoEstado = Math.Max(1, ancho / 2);
        lblEstadoContinuacionInicio.SetBounds(
            margen,
            EscalarDiseno(136),
            anchoEstado,
            EscalarDiseno(22));
        lblRutaContinuacionInicio.SetBounds(
            margen + anchoEstado,
            EscalarDiseno(136),
            Math.Max(1, ancho - anchoEstado),
            EscalarDiseno(22));

        int separacion = EscalarDiseno(12);
        bool tieneSecundaria = btnAccionSecundariaInicio.Visible;
        int anchoBoton = tieneSecundaria
            ? Math.Max(1, (ancho - separacion) / 2)
            : Math.Min(ancho, EscalarDiseno(220));
        btnContinuarInicio.SetBounds(
            margen,
            panelContinuacionInicio.Height - EscalarDiseno(14 + 38),
            anchoBoton,
            EscalarDiseno(38));
        btnAccionSecundariaInicio.SetBounds(
            margen + anchoBoton + separacion,
            btnContinuarInicio.Top,
            tieneSecundaria
                ? Math.Max(1, ancho - anchoBoton - separacion)
                : 0,
            EscalarDiseno(38));
    }

    private void ActualizarGeometriaProgresoInicio() {
        int margen = EscalarDiseno(18);
        int ancho = Math.Max(1, panelProgresoInicio.Width - margen * 2);
        int separacionColumnas = EscalarDiseno(8);
        int anchoColumna = Math.Max(1, (ancho - separacionColumnas) / 2);
        lblTituloProgresoInicio.SetBounds(
            margen,
            EscalarDiseno(10),
            ancho,
            EscalarDiseno(18));
        lblPracticasProgresoInicio.SetBounds(
            margen,
            EscalarDiseno(29),
            Math.Max(1, ancho * 2 / 3),
            EscalarDiseno(30));
        lblPorcentajeProgresoInicio.SetBounds(
            margen + ancho * 2 / 3,
            EscalarDiseno(31),
            Math.Max(1, ancho / 3),
            EscalarDiseno(24));
        panelPistaProgresoInicio.SetBounds(
            margen,
            EscalarDiseno(63),
            ancho,
            EscalarDiseno(6));
        lblTemasProgresoInicio.SetBounds(
            margen,
            EscalarDiseno(73),
            anchoColumna,
            EscalarDiseno(20));
        lblGradosProgresoInicio.SetBounds(
            margen + anchoColumna + separacionColumnas,
            EscalarDiseno(73),
            Math.Max(1, ancho - anchoColumna - separacionColumnas),
            EscalarDiseno(20));
        lblNivelInicio.SetBounds(
            margen,
            EscalarDiseno(99),
            anchoColumna,
            EscalarDiseno(24));
        lblXpTotalInicio.SetBounds(
            margen + anchoColumna + separacionColumnas,
            EscalarDiseno(101),
            Math.Max(1, ancho - anchoColumna - separacionColumnas),
            EscalarDiseno(20));
        panelPistaNivelInicio.SetBounds(
            margen,
            EscalarDiseno(127),
            ancho,
            EscalarDiseno(6));
        lblXpRestanteInicio.SetBounds(
            margen,
            EscalarDiseno(137),
            ancho,
            EscalarDiseno(20));
        MedidasFranjaMotivacionInicio franja =
            CalculadorLayoutFranjaMotivacionInicio.Calcular(
                panelProgresoInicio.ClientSize.Width,
                DeviceDpi);
        AplicarRectanguloFisicoInicio(
            panelSeparadorMotivacionInicio,
            franja.Separador);
        AplicarRectanguloFisicoInicio(
            lblTituloRachaInicio,
            franja.TituloRacha);
        AplicarRectanguloFisicoInicio(
            lblValorRachaInicio,
            franja.ValorRacha);
        AplicarRectanguloFisicoInicio(
            lblDetalleRachaInicio,
            franja.DetalleRacha);
        AplicarRectanguloFisicoInicio(
            lblTituloLogrosInicio,
            franja.TituloLogros);
        AplicarRectanguloFisicoInicio(
            lblValorLogrosInicio,
            franja.ValorLogros);
        AplicarRectanguloFisicoInicio(
            btnVerLogrosInicio,
            franja.BotonVerLogros);
        ActualizarRellenoProgresoInicio();
        ActualizarRellenoNivelInicio();
    }

    private static void AplicarRectanguloFisicoInicio(
        Control control,
        RectanguloLayoutInicio rectangulo) {
        control.SetBounds(
            rectangulo.X,
            rectangulo.Y,
            rectangulo.Ancho,
            rectangulo.Alto);
    }

    private void ActualizarRellenoProgresoInicio() {
        if (panelPistaProgresoInicio is null ||
            panelPistaProgresoInicio.IsDisposed) {
            return;
        }

        panelRellenoProgresoInicio.SetBounds(
            0,
            0,
            (int)Math.Round(
                panelPistaProgresoInicio.ClientSize.Width *
                valorBarraProgresoInicio / 100D),
            panelPistaProgresoInicio.ClientSize.Height);
    }

    private void ActualizarRellenoNivelInicio() {
        if (panelPistaNivelInicio is null || panelPistaNivelInicio.IsDisposed) {
            return;
        }

        panelRellenoNivelInicio.SetBounds(
            0,
            0,
            (int)Math.Round(
                panelPistaNivelInicio.ClientSize.Width *
                valorBarraNivelInicio / 100D),
            panelPistaNivelInicio.ClientSize.Height);
    }

    private void ActualizarGeometriaMetricasInicio(
        int ancho,
        int alto,
        int separacion,
        bool modoAmplio) {
        int columnas = modoAmplio ? 4 : 2;
        int anchoTarjeta = Math.Max(
            1,
            (ancho - separacion * (columnas - 1)) / columnas);

        for (int indice = 0; indice < tarjetasMetricasInicio.Count; indice++) {
            TarjetaMetricaInicioVisual visual = tarjetasMetricasInicio[indice];
            int fila = indice / columnas;
            int columna = indice % columnas;
            int x = columna * (anchoTarjeta + separacion);
            int anchoActual = columna == columnas - 1
                ? Math.Max(1, ancho - x)
                : anchoTarjeta;
            visual.Tarjeta.SetBounds(
                x,
                fila * (alto + separacion),
                anchoActual,
                alto);
            int margen = EscalarDiseno(16);
            int anchoInterior = Math.Max(1, anchoActual - margen * 2);
            visual.Titulo.SetBounds(
                margen,
                EscalarDiseno(9),
                anchoInterior,
                EscalarDiseno(28));
            visual.Valor.SetBounds(
                margen,
                EscalarDiseno(38),
                anchoInterior,
                EscalarDiseno(32));
            visual.Descripcion.SetBounds(
                margen,
                EscalarDiseno(72),
                anchoInterior,
                EscalarDiseno(24));
        }
    }

    private void ActualizarGeometriaRecomendacionInicio(bool modoAmplio) {
        int margen = EscalarDiseno(18);
        int ancho = Math.Max(1, panelRecomendacionInicio.Width - margen * 2);
        lblTituloRecomendacionInicio.SetBounds(
            margen,
            EscalarDiseno(12),
            ancho,
            EscalarDiseno(20));
        lblPracticaRecomendacionInicio.SetBounds(
            margen,
            EscalarDiseno(33),
            ancho,
            EscalarDiseno(34));
        lblContextoRecomendacionInicio.SetBounds(
            margen,
            EscalarDiseno(67),
            ancho,
            EscalarDiseno(22));
        lblMetadatosRecomendacionInicio.SetBounds(
            margen,
            EscalarDiseno(89),
            ancho,
            EscalarDiseno(22));
        int anchoBoton = Math.Min(ancho, EscalarDiseno(220));
        int separacion = EscalarDiseno(14);

        if (modoAmplio) {
            lblRazonRecomendacionInicio.SetBounds(
                margen,
                EscalarDiseno(112),
                Math.Max(1, ancho - anchoBoton - separacion),
                EscalarDiseno(52));
            btnRecomendacionInicio.SetBounds(
                margen + Math.Max(0, ancho - anchoBoton),
                panelRecomendacionInicio.Height - EscalarDiseno(14 + 38),
                anchoBoton,
                EscalarDiseno(38));
        } else {
            lblRazonRecomendacionInicio.SetBounds(
                margen,
                EscalarDiseno(112),
                ancho,
                EscalarDiseno(42));
            btnRecomendacionInicio.SetBounds(
                margen,
                panelRecomendacionInicio.Height - EscalarDiseno(14 + 38),
                anchoBoton,
                EscalarDiseno(38));
        }
    }

    private void ActualizarGeometriaActividadInicio() {
        int margen = EscalarDiseno(18);
        int ancho = Math.Max(1, panelActividadInicio.Width - margen * 2);
        lblTituloActividadInicio.SetBounds(
            margen,
            EscalarDiseno(12),
            ancho,
            EscalarDiseno(20));
        lblActividadVaciaInicio.SetBounds(
            margen,
            EscalarDiseno(39),
            ancho,
            EscalarDiseno(44));
        int y = EscalarDiseno(39);
        int altoFila = EscalarDiseno(48);
        int separacion = EscalarDiseno(6);

        foreach (FilaActividadInicioVisual fila in filasActividadInicio) {
            if (!fila.Fila.Visible) {
                continue;
            }

            fila.Fila.SetBounds(margen, y, ancho, altoFila);
            fila.Fecha.SetBounds(
                EscalarDiseno(12),
                EscalarDiseno(4),
                Math.Max(1, ancho - EscalarDiseno(24)),
                EscalarDiseno(18));
            fila.Descripcion.SetBounds(
                EscalarDiseno(12),
                EscalarDiseno(21),
                Math.Max(1, ancho - EscalarDiseno(24)),
                EscalarDiseno(23));
            y += altoFila + separacion;
        }
    }

    private static Panel CrearContenedorInicio(string nombre) {
        return new Panel {
            Name = nombre,
            BackColor = ColorFondoInicio,
            Margin = Padding.Empty
        };
    }

    private static Panel CrearTarjetaInicio(string nombre, Color color) {
        Panel tarjeta = CrearTarjetaCurso(
            Point.Empty,
            new Size(1, 1),
            16,
            resaltarFocoContenido: false);
        tarjeta.Name = nombre;
        tarjeta.BackColor = color;
        tarjeta.Margin = Padding.Empty;
        return tarjeta;
    }

    private static Label CrearLabelInicio(
        string texto,
        float tamano,
        FontStyle estilo,
        Color color,
        Color fondo,
        ContentAlignment alineacion = ContentAlignment.MiddleLeft) {
        Label label = CrearLabelCurso(
            texto,
            Point.Empty,
            new Size(1, 1),
            tamano,
            estilo,
            color,
            alineacion);
        label.BackColor = fondo;
        return label;
    }

    private static BotonInicio CrearBotonInicio(
        string texto,
        bool esPrimario,
        int tabIndex) {
        Font fuente = new(
            "Segoe UI Semibold",
            9.5F,
            FontStyle.Bold);
        BotonInicio boton = new() {
            BackColor = esPrimario
                ? ColorMoradoCurso
                : Color.FromArgb(47, 39, 62),
            ForeColor = Color.White,
            Font = fuente,
            Text = texto,
            TabIndex = tabIndex
        };
        boton.FlatAppearance.MouseOverBackColor = esPrimario
            ? Color.FromArgb(174, 108, 232)
            : Color.FromArgb(64, 52, 82);
        boton.FlatAppearance.MouseDownBackColor = esPrimario
            ? Color.FromArgb(116, 55, 178)
            : Color.FromArgb(39, 32, 52);
        boton.Disposed += (_, _) => fuente.Dispose();
        return boton;
    }

    private static void ConfigurarControlFlujoInicio(
        Control control,
        int ancho,
        int alto,
        int margenInferior) {
        control.Size = new Size(Math.Max(1, ancho), Math.Max(1, alto));
        control.Margin = new Padding(0, 0, 0, Math.Max(0, margenInferior));
    }
}
