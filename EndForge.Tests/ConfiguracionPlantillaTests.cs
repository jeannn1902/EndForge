using EndForge.Models;
using EndForge.Services;
using System.Text;

namespace EndForge.Tests;

public sealed class ConfiguracionPlantillaTests {
    [Fact]
    public void RechazaMarcadorSoloEnPropiedadArbitraria() {
        using PlantillaTestHelper plantilla = new();
        PlantillaTestHelper.EscribirProyecto(
            plantilla.RutaProyectoPlantilla,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <PropertyGroup>
                <ConfigurationType>Application</ConfigurationType>
                <PropiedadIrrelevante>00_Plantilla</PropiedadIrrelevante>
              </PropertyGroup>
            </Project>
            """);

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.NotEqual(EstadoValidacionConfiguracion.Valida, resultado.Estado);
    }

    [Fact]
    public void RechazaClCompileQueNoExisteAunqueHayaOtroCpp() {
        using PlantillaTestHelper plantilla = new();
        File.Delete(plantilla.RutaCppPlantilla);
        PlantillaTestHelper.EscribirTexto(
            Path.Combine(plantilla.RutaPlantilla, "huerfano.cpp"),
            "int huerfano = 1;",
            new UTF8Encoding(false, true));

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.NotEqual(EstadoValidacionConfiguracion.Valida, resultado.Estado);
    }

    [Fact]
    public void RechazaSiCualquierProyectoReferenciadoNoExiste() {
        using PlantillaTestHelper plantilla = new();
        plantilla.EscribirSolucion(
            "00_Plantilla.sln",
            @"00_Plantilla\00_Plantilla.vcxproj",
            @"Biblioteca\Faltante.vcxproj");

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.Equal(
            EstadoValidacionConfiguracion.PlantillaProyectoReferenciadoNoDisponible,
            resultado.Estado);
    }

    [Fact]
    public void RechazaFiltersConXmlInvalido() {
        using PlantillaTestHelper plantilla = new();
        PlantillaTestHelper.EscribirTexto(
            plantilla.RutaProyectoPlantilla + ".filters",
            "<Project><ItemGroup>",
            new UTF8Encoding(false, true));

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.NotEqual(EstadoValidacionConfiguracion.Valida, resultado.Estado);
    }

    [Fact]
    public void RechazaFiltersConClCompileHuerfano() {
        using PlantillaTestHelper plantilla = new();
        PlantillaTestHelper.EscribirProyecto(
            plantilla.RutaProyectoPlantilla + ".filters",
            PlantillaTestHelper.CrearFiltersXml("Fantasma.cpp"));

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.NotEqual(EstadoValidacionConfiguracion.Valida, resultado.Estado);
    }

    [Fact]
    public void RechazaXmlHuerfanoInvalidoQueElGeneradorTransformaria() {
        using PlantillaTestHelper plantilla = new();
        PlantillaTestHelper.EscribirTexto(
            Path.Combine(
                plantilla.RutaPlantilla,
                "ProyectoHuerfano.vcxproj"),
            "<Project><ItemGroup>",
            new UTF8Encoding(false, true));

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.Equal(
            EstadoValidacionConfiguracion.PlantillaProyectoXmlInvalido,
            resultado.Estado);
    }

    [Fact]
    public void AceptaExtensionesEnMayusculas() {
        using PlantillaTestHelper plantilla = new();
        string directorioProyecto = Path.GetDirectoryName(
            plantilla.RutaProyectoPlantilla)!;
        string proyectoMayusculas = Path.Combine(
            directorioProyecto,
            "00_Plantilla.VCXPROJ");
        string cppMayusculas = Path.Combine(
            directorioProyecto,
            "00_Plantilla.CPP");
        string solucionMayusculas = Path.Combine(
            plantilla.RutaPlantilla,
            "00_Plantilla.SLN");

        File.Move(plantilla.RutaProyectoPlantilla, proyectoMayusculas);
        File.Move(plantilla.RutaCppPlantilla, cppMayusculas);
        File.Move(plantilla.RutaSolucionPlantilla, solucionMayusculas);
        PlantillaTestHelper.EscribirProyecto(
            proyectoMayusculas,
            PlantillaTestHelper.CrearProyectoXml("00_Plantilla.CPP"));
        plantilla.EscribirSolucion(
            "00_Plantilla.SLN",
            @"00_Plantilla\00_Plantilla.VCXPROJ");
        PlantillaTestHelper.EscribirProyecto(
            proyectoMayusculas + ".FILTERS",
            PlantillaTestHelper.CrearFiltersXml("00_Plantilla.CPP"));

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);

        Assert.Equal(EstadoValidacionConfiguracion.Valida, resultado.Estado);
        Assert.Equal("00_Plantilla.SLN", resultado.RutaRelativaSolucion);
    }

    [Fact]
    public void RechazaRutaBaseQueEsPuntoDeReanalisis() {
        using PlantillaTestHelper plantilla = new();
        string rutaBaseEnlace = plantilla.CrearRuta("BaseEnlace");

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(
            rutaBaseEnlace,
            plantilla.RutaBase)) {
            return;
        }

        ResultadoValidacionConfiguracion resultado =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                rutaBaseEnlace,
                plantilla.RutaPlantilla);

        Assert.Equal(
            EstadoValidacionConfiguracion.RutaBaseNoSegura,
            resultado.Estado);
    }
}
