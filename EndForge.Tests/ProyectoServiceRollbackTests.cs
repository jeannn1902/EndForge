using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class ProyectoServiceRollbackTests {
    [Fact]
    public void PlantillaModificadaDespuesDeValidarse_NoPublicaDestinoNiDejaTemporal() {
        using PlantillaTestHelper plantilla = new();
        ResultadoValidacionConfiguracion validacion =
            new ConfiguracionService().ValidarConfiguracionDetallada(
                plantilla.RutaBase,
                plantilla.RutaPlantilla);
        Assert.Equal(
            EstadoValidacionConfiguracion.Valida,
            validacion.Estado);
        string nombreProyecto = "01_Practica";
        string rutaEsperada = new SeleccionSolucionesService()
            .TransformarRutaRelativa(
                validacion.RutaRelativaSolucion,
                nombreProyecto);
        string destino = Path.Combine(
            plantilla.RutaBase,
            nombreProyecto);
        File.Delete(plantilla.RutaCppPlantilla);

        Assert.ThrowsAny<InvalidDataException>(() =>
            new ProyectoService().CrearProyecto(
                plantilla.RutaPlantilla,
                destino,
                nombreProyecto,
                "Tema",
                "Objetivo",
                rutaEsperada));

        Assert.False(Directory.Exists(destino));
        Assert.Empty(Directory.EnumerateFileSystemEntries(
            plantilla.RutaBase,
            ".endforge-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    [Fact]
    public void DestinoExistente_ConservaTestigoAjenoSinCopiarArchivos() {
        using PlantillaTestHelper plantilla = new();
        string destino = Path.Combine(
            plantilla.RutaBase,
            "01_Existente");
        Directory.CreateDirectory(destino);
        string testigo = Path.Combine(destino, "testigo-ajeno.txt");
        File.WriteAllText(testigo, "no modificar");

        Assert.Throws<ProyectoService.ProyectoDestinoExistenteException>(
            () => new ProyectoService().CrearProyecto(
                plantilla.RutaPlantilla,
                destino,
                "01_Existente",
                "Tema",
                "Objetivo"));

        Assert.Equal("no modificar", File.ReadAllText(testigo));
        Assert.Single(Directory.EnumerateFileSystemEntries(destino));
    }

    [Fact]
    public void CarreraAlReservarTemporal_NoAdoptaNiEliminaCarpetaAjena() {
        using PlantillaTestHelper plantilla = new();
        string? temporalAjeno = null;
        int intentos = 0;
        ProyectoService servicio = new(
            new SeleccionSolucionesService(),
            ruta => {
                intentos++;

                if (intentos == 1) {
                    temporalAjeno = ruta;
                    Directory.CreateDirectory(ruta);
                    File.WriteAllText(
                        Path.Combine(ruta, "testigo-ajeno.txt"),
                        "ajeno");
                    return false;
                }

                Directory.CreateDirectory(ruta);
                return true;
            });
        string destino = Path.Combine(
            plantilla.RutaBase,
            "01_Publicada");

        servicio.CrearProyecto(
            plantilla.RutaPlantilla,
            destino,
            "01_Publicada",
            "Tema",
            "Objetivo");

        Assert.True(intentos >= 2);
        Assert.NotNull(temporalAjeno);
        Assert.Equal(
            "ajeno",
            File.ReadAllText(
                Path.Combine(
                    temporalAjeno!,
                    "testigo-ajeno.txt")));
        Assert.True(Directory.Exists(destino));
    }

    [Fact]
    public void DestinoBajoJunction_DeRaizConfiableSeRechazaAntesDeCopiar() {
        using PlantillaTestHelper plantilla = new();
        string externo = plantilla.CrearRuta("TemaExterno");
        string enlace = Path.Combine(
            plantilla.RutaBase,
            "01_Tema");
        Directory.CreateDirectory(externo);

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(
            enlace,
            externo)) {
            return;
        }

        string destino = Path.Combine(enlace, "01_Practica");

        Assert.ThrowsAny<InvalidDataException>(() =>
            new ProyectoService().CrearProyecto(
                plantilla.RutaPlantilla,
                destino,
                "01_Practica",
                "Tema",
                "Objetivo",
                "",
                plantilla.RutaBase));

        Assert.False(Directory.Exists(
            Path.Combine(externo, "01_Practica")));
        Assert.Empty(Directory.EnumerateFileSystemEntries(
            externo,
            ".endforge-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    [Fact]
    public void CopiarPlantillaNoSiguePuntosDeReanalisis() {
        using PlantillaTestHelper plantilla = new();
        string externo = plantilla.CrearRuta("DatosExternos");
        string enlace = Path.Combine(plantilla.RutaPlantilla, "EnlaceExterno");
        string destino = plantilla.CrearRuta("Copia");
        Directory.CreateDirectory(externo);
        File.WriteAllText(Path.Combine(externo, "testigo-ajeno.txt"), "privado");

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(enlace, externo)) {
            return;
        }

        Directory.CreateDirectory(destino);
        new ProyectoService().CopiarPlantilla(
            plantilla.RutaPlantilla,
            destino);

        Assert.False(
            File.Exists(
                Path.Combine(destino, "EnlaceExterno", "testigo-ajeno.txt")));
    }

    [Fact]
    public void FalloConArchivoReadOnlyEliminaTemporalPropio() {
        using PlantillaTestHelper plantilla = new();
        string testigoReadOnly = Path.Combine(
            plantilla.RutaPlantilla,
            "testigo-readonly.bin");
        string filtersInvalido =
            plantilla.RutaProyectoPlantilla + ".filters";
        string destino = Path.Combine(
            plantilla.RutaBase,
            "01_Falla");
        File.WriteAllText(testigoReadOnly, "no debe impedir la limpieza");
        File.SetAttributes(testigoReadOnly, FileAttributes.ReadOnly);
        File.WriteAllText(filtersInvalido, "<Project>");

        Assert.ThrowsAny<Exception>(
            () => new ProyectoService().CrearProyecto(
                plantilla.RutaPlantilla,
                destino,
                "01_Falla",
                "Tema",
                "Objetivo"));

        Assert.False(Directory.Exists(destino));
        Assert.Empty(
            Directory.EnumerateFileSystemEntries(
                plantilla.RutaBase,
                ".endforge-*.tmp",
                SearchOption.TopDirectoryOnly));
    }
}
