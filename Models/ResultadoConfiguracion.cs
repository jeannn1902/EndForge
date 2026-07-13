namespace EndForge.Models;

public enum EstadoCargaConfiguracion {
    NoDisponible,
    Cargada,
    ErrorPermisosConfiguracion,
    ErrorLecturaConfiguracion,
    ErrorPermisosRecientes,
    ErrorCreacionRecientes
}

public sealed class ResultadoCargaConfiguracion {
    public EstadoCargaConfiguracion Estado { get; init; }

    public string RutaBase { get; init; } = "";

    public string RutaPlantilla { get; init; } = "";

    public bool ConfiguracionDisponible =>
        Estado == EstadoCargaConfiguracion.Cargada ||
        Estado == EstadoCargaConfiguracion.ErrorPermisosRecientes ||
        Estado == EstadoCargaConfiguracion.ErrorCreacionRecientes;
}

public enum EstadoValidacionConfiguracion {
    Valida,
    RutasNoExistentes,
    PlantillaSinSolucion,
    PlantillaSinProyectoCpp,
    PlantillaSinArchivosCpp
}
