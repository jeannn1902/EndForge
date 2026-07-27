using EndForge.Models;
using EndForge.Services;
using System.Diagnostics;

namespace EndForge.Tests;

public sealed class SeleccionSolucionTests {
    [Fact]
    public void SinSeleccionPersistidaIgnoraSolucionIncompatible() {
        using PlantillaTestHelper practica = new();
        string rutaValida = Path.Combine(
            practica.RutaPlantilla,
            "Z_Valida.sln");
        File.Move(practica.RutaSolucionPlantilla, rutaValida);
        practica.EscribirSolucion("A_Invalida.sln");
        List<ProcessStartInfo> lanzamientos = new();
        AperturaPracticasService servicio = new(
            new SeleccionSolucionesService(),
            lanzamientos.Add);

        ResultadoAperturaPractica resultado = servicio.AbrirPractica(
            practica.RutaPlantilla);

        Assert.Equal(EstadoAperturaPractica.Exitosa, resultado.Estado);
        Assert.Equal(
            Path.GetFullPath(rutaValida),
            Path.GetFullPath(resultado.RutaSolucion!));
        Assert.Single(lanzamientos);
    }

    [Fact]
    public void SinSeleccionPersistidaReportaAmbiguedadEntreDosCompatibles() {
        using PlantillaTestHelper practica = new();
        string segundoProyecto = Path.Combine(
            practica.RutaPlantilla,
            "Segundo",
            "Segundo.vcxproj");
        PlantillaTestHelper.EscribirProyecto(
            segundoProyecto,
            PlantillaTestHelper.CrearProyectoXml("Segundo.cpp"));
        PlantillaTestHelper.EscribirTexto(
            Path.Combine(Path.GetDirectoryName(segundoProyecto)!, "Segundo.cpp"),
            "int main() { return 0; }",
            new System.Text.UTF8Encoding(false, true));
        practica.EscribirSolucion(
            "Segundo.sln",
            @"Segundo\Segundo.vcxproj");
        List<ProcessStartInfo> lanzamientos = new();

        ResultadoAperturaPractica resultado =
            new AperturaPracticasService(
                new SeleccionSolucionesService(),
                lanzamientos.Add
            ).AbrirPractica(practica.RutaPlantilla);

        Assert.Equal(EstadoAperturaPractica.SolucionAmbigua, resultado.Estado);
        Assert.Null(resultado.RutaSolucion);
        Assert.Empty(lanzamientos);
    }

    [Fact]
    public void RechazaSolucionQueCruzaUnPuntoDeReanalisis() {
        using PlantillaTestHelper practica = new();
        string externo = practica.CrearRuta("Externo");
        string enlace = Path.Combine(practica.RutaPlantilla, "Enlace");
        Directory.CreateDirectory(externo);
        PlantillaTestHelper.EscribirProyecto(
            Path.Combine(externo, "Fuera.vcxproj"),
            PlantillaTestHelper.CrearProyectoXml("Fuera.cpp"));
        PlantillaTestHelper.EscribirTexto(
            Path.Combine(externo, "Fuera.cpp"),
            "int main() { return 0; }",
            new System.Text.UTF8Encoding(false, true));

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(enlace, externo)) {
            return;
        }

        practica.EscribirSolucion(
            "SoloEnlace.sln",
            @"Enlace\Fuera.vcxproj");

        bool resuelta = new SeleccionSolucionesService().IntentarResolverRutaRelativa(
            practica.RutaPlantilla,
            @"Enlace\Fuera.vcxproj",
            out _);

        Assert.False(resuelta);
    }

    [Fact]
    public void AbrirPractica_NoSiguePuntosDeReanalisisAlBuscarCpp() {
        using PlantillaTestHelper practica = new();
        string enlaceCircular = Path.Combine(
            practica.RutaPlantilla,
            "EnlaceCircular");

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(
            enlaceCircular,
            practica.RutaPlantilla)) {
            return;
        }

        List<ProcessStartInfo> lanzamientos = new();
        AperturaPracticasService servicio = new(
            new SeleccionSolucionesService(),
            lanzamientos.Add);

        ResultadoAperturaPractica resultado = servicio.AbrirPractica(
            practica.RutaPlantilla);

        Assert.Equal(EstadoAperturaPractica.Exitosa, resultado.Estado);
        Assert.Single(lanzamientos);
    }
}
