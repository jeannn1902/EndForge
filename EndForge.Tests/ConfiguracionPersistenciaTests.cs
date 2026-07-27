using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class ConfiguracionPersistenciaTests : IDisposable {
    private readonly string carpetaDatos = Path.Combine(
        Path.GetTempPath(),
        $"EndForge.Tests-Configuracion-{Guid.NewGuid():N}");

    [Fact]
    public void CargarConfiguracionValida_NoCreaNiModificaRecientes() {
        Directory.CreateDirectory(carpetaDatos);
        string rutaConfig = Path.Combine(carpetaDatos, "config.txt");
        string rutaRecientes = Path.Combine(carpetaDatos, "recientes.txt");
        File.WriteAllLines(rutaConfig, ["C:\\Curso", "C:\\Plantilla"]);
        ConfiguracionService servicio = CrearServicio();

        ResultadoCargaConfiguracion resultado =
            servicio.CargarConfiguracion();

        Assert.Equal(EstadoCargaConfiguracion.Cargada, resultado.Estado);
        Assert.False(File.Exists(rutaRecientes));
    }

    [Fact]
    public void GuardarConfiguracion_SiCarpetaFueEliminada_LaRecrea() {
        Directory.CreateDirectory(carpetaDatos);
        ConfiguracionService servicio = CrearServicio();
        Directory.Delete(carpetaDatos);

        servicio.GuardarConfiguracion("C:\\Curso", "C:\\Plantilla");

        Assert.Equal(
            ["C:\\Curso", "C:\\Plantilla"],
            File.ReadAllLines(Path.Combine(carpetaDatos, "config.txt")));
    }

    [Fact]
    public void GuardarConfiguracion_SiReemplazoFalla_ConservaOriginalYLimpiaTemporal() {
        Directory.CreateDirectory(carpetaDatos);
        string rutaConfig = Path.Combine(carpetaDatos, "config.txt");
        File.WriteAllLines(rutaConfig, ["C:\\Anterior", "C:\\PlantillaAnterior"]);
        ConfiguracionService servicio = CrearServicio();

        using (FileStream bloqueo = new(
            rutaConfig,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None)) {
            Assert.ThrowsAny<IOException>(() =>
                servicio.GuardarConfiguracion(
                    "C:\\Nueva",
                    "C:\\PlantillaNueva"));
        }

        Assert.Equal(
            ["C:\\Anterior", "C:\\PlantillaAnterior"],
            File.ReadAllLines(rutaConfig));
        Assert.Empty(Directory.EnumerateFiles(
            carpetaDatos,
            ".config-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    [Fact]
    public void CargarConfiguracion_Bloqueada_DevuelveErrorRecuperableSinRutas() {
        Directory.CreateDirectory(carpetaDatos);
        string rutaConfig = Path.Combine(carpetaDatos, "config.txt");
        File.WriteAllLines(rutaConfig, ["C:\\Curso", "C:\\Plantilla"]);
        ConfiguracionService servicio = CrearServicio();

        using FileStream bloqueo = new(
            rutaConfig,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);
        ResultadoCargaConfiguracion resultado =
            servicio.CargarConfiguracion();

        Assert.Equal(
            EstadoCargaConfiguracion.ErrorLecturaConfiguracion,
            resultado.Estado);
        Assert.Empty(resultado.RutaBase);
        Assert.Empty(resultado.RutaPlantilla);
    }

    private ConfiguracionService CrearServicio() {
        return new ConfiguracionService(
            new SeleccionSolucionesService(),
            carpetaDatos);
    }

    public void Dispose() {
        if (Directory.Exists(carpetaDatos)) {
            Directory.Delete(carpetaDatos, recursive: true);
        }
    }
}
