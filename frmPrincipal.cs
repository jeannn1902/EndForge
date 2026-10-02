using EndForge.Models;
using EndForge.Repositories;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal : Form {
    private readonly SeleccionSolucionesService seleccionSolucionesService = new();
    private readonly IProgresoRepository? _progresoRepository;
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
    }

    // Constructor usado por DI; llama al constructor por defecto y recibe el repositorio inyectado.
    public frmPrincipal(IProgresoRepository progresoRepository) : this()
    {
        _progresoRepository = progresoRepository ?? throw new ArgumentNullException(nameof(progresoRepository));

        this.Load += frmPrincipal_Load;
    }

    private async void frmPrincipal_Load(object? sender, EventArgs e)
    {
        try
        {
            var registros = await _progresoRepository!.GetByUsuarioAsync(1) ?? Enumerable.Empty<ProgresoUsuarioDb>();
            MessageBox.Show($"Registros de progreso encontrados: {registros.Count()}", "Conexión Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Program.RegistrarErrorRecuperable(ex);
            MessageBox.Show($"Error comprobando progreso: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
