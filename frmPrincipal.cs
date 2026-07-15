using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal : Form {
    private readonly SeleccionSolucionesService seleccionSolucionesService = new();
    private readonly ProyectoService proyectoService;
    private readonly ConfiguracionService configuracionService;
    private readonly TemasService temasService = new();
    private readonly NombrePracticaService nombrePracticaService = new();
    private readonly AperturaPracticasService aperturaPracticasService;
    private readonly RecientesService recientesService;
    private readonly CreacionPracticasOrquestador creacionPracticasOrquestador;
    private readonly VistaPreviaPracticaService vistaPreviaPracticaService;
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

        InitializeComponent();

        ConfigurarBarraTitulo();
        ConfigurarTarjetasInicio();
        ActivarBarraTituloOscura();
        ConfigurarVentana();
        ConfigurarNavegacion();
        ConfigurarRecientes();
        ConfigurarEstadoInicial();
    }
}
