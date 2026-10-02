using EndForge.Models;
using EndForge.Repositories;
using EndForge.Services;
using EndForge.App;
using EndForge.Logging;
using System.Windows.Forms;

namespace EndForge;

public partial class frmPrincipal : Form {
    private readonly SeleccionSolucionesService seleccionSolucionesService = new();
    private readonly IProgresoRepository? _progresoRepository;
    private readonly AppState? _appState;
    private ToolStripStatusLabel? _statusLabel;
    private readonly ProyectoService proyectoService;
    private readonly ConfiguracionService configuracionService;
    private readonly PreferenciasService preferenciasService = new();
    private readonly TemasService temasService = new();
    private readonly NombrePracticaService nombrePracticaService = new();
    private readonly AperturaPracticasService aperturaPracticasService;
    private readonly RecientesService recientesService;
    private readonly CreacionPracticasOrquestador creacionPracticasOrquestador;
    private readonly VistaPreviaPracticaService vistaPreviaPracticaService;
    private readonly MotivacionService motivacionService;
    private Panel panelSeleccionado = null!;
    private string rutaBase = "";
    private string rutaPlantilla = "";

    public frmPrincipal() {
        proyectoService = new ProyectoService(seleccionSolucionesService);
        configuracionService = new ConfiguracionService(seleccionSolucionesService);
        aperturaPracticasService = new AperturaPracticasService(seleccionSolucionesService);
        recientesService = new RecientesService(configuracionService.RutaRecientes);
        vistaPreviaPracticaService = new VistaPreviaPracticaService(temasService);
        creacionPracticasOrquestador = new CreacionPracticasOrquestador(proyectoService, recientesService, aperturaPracticasService);
        motivacionService = new MotivacionService();

        InitializeComponent();
        InicializarPreferenciasAprendizaje();
        InicializarEstructuraCurso();
        InicializarEstructuraEstadisticas();

        ConfigurarBarraTitulo();
        InicializarEstructuraInicio();
        InicializarEstructuraLogros();
        ConfigurarVentana();
        ActivarBarraTituloOscura();
        ConfigurarNavegacion();
        ConfigurarRecientes();
        ConfigurarEstadoInicial();
        InicializarBienvenida();
        InitializeStatusIndicator();
    }

    // Constructor usado por DI; llama al constructor por defecto y recibe el repositorio inyectado
    // y el estado de la aplicación.
    public frmPrincipal(IProgresoRepository progresoRepository, AppState appState) : this()
    {
        _progresoRepository = progresoRepository ?? throw new ArgumentNullException(nameof(progresoRepository));
        _appState = appState ?? throw new ArgumentNullException(nameof(appState));

        this.Load += frmPrincipal_Load;
        UpdateConnectionIndicator();
        AppEventLogger.Log($"frmPrincipal initialized. Mode={(appState.IsCloudConnected ? "Cloud" : "Fallback")}");
    }

    private async void frmPrincipal_Load(object? sender, EventArgs e)
    {
        try
        {
            if (_progresoRepository != null)
            {
                var registros = await _progresoRepository.GetByUsuarioAsync(1) ?? Enumerable.Empty<ProgresoUsuarioDb>();
                // No mostrar MessageBox en arranque; actualizar log con el conteo
                AppEventLogger.Log($"Progreso registros encontrados para usuario 1: {registros.Count()}");
            }
        }
        catch (Exception ex)
        {
            Program.RegistrarErrorRecuperable(ex);
            AppEventLogger.Log($"Error comprobando progreso: {ex.Message}");
        }
    }

    private void InitializeStatusIndicator()
    {
        try
        {
            var statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel();
            _statusLabel.Spring = false;
            _statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            statusStrip.Items.Add(_statusLabel);
            statusStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(statusStrip);
        }
        catch
        {
            // no afectar la ejecución si no se puede crear el indicador
        }
    }

    private void UpdateConnectionIndicator()
    {
        if (_statusLabel == null) return;
        if (_appState == null)
        {
            _statusLabel.Text = "⚪ Desconocido";
            return;
        }

        _statusLabel.Text = _appState.IsCloudConnected ? "🟢 Nube" : "🟡 Modo Offline";
    }
}
