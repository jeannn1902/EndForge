using EndForge.Services;
using System.Text;
using System.Xml.Linq;

namespace EndForge.Tests;

public sealed class EncodingPlantillaTests {
    [Fact]
    public void ActualizarReferenciasNoModificaNombresDeNodosXml() {
        using PlantillaTestHelper plantilla = new();
        string ruta = Path.Combine(
            plantilla.RutaPlantilla,
            "nodos.vcxproj");
        PlantillaTestHelper.EscribirProyecto(
            ruta,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Project>
              <Custom00_PlantillaSetting>sin cambios</Custom00_PlantillaSetting>
              <Elemento Include="00_Plantilla.cpp">00_Plantilla</Elemento>
            </Project>
            """);

        Exception? error = Record.Exception(
            () => new ProyectoService().ActualizarReferencias(
                plantilla.RutaPlantilla,
                "01_Mi Proyecto"));

        Assert.Null(error);
        XDocument documento = XDocument.Load(ruta);
        Assert.NotNull(documento.Descendants("Custom00_PlantillaSetting").Single());
        XElement elemento = documento.Descendants("Elemento").Single();
        Assert.Equal("01_Mi Proyecto.cpp", elemento.Attribute("Include")!.Value);
        Assert.Equal("01_Mi Proyecto", elemento.Value);
    }

    [Fact]
    public void PreservaUtf8ConYSinBomYUtf16ConAcentos() {
        using PlantillaTestHelper plantilla = new();
        string rutaUtf8Bom = Path.Combine(
            plantilla.RutaPlantilla,
            "bom.cpp");
        string rutaUtf8SinBom = Path.Combine(
            plantilla.RutaPlantilla,
            "sin-bom.sln");
        string rutaUtf16 = Path.Combine(
            plantilla.RutaPlantilla,
            "unicode.filters");
        PlantillaTestHelper.EscribirTexto(
            rutaUtf8Bom,
            "// acción y niñez: 00_Plantilla",
            new UTF8Encoding(true, true));
        PlantillaTestHelper.EscribirTexto(
            rutaUtf8SinBom,
            "acción y niñez: 00_Plantilla",
            new UTF8Encoding(false, true));
        PlantillaTestHelper.EscribirTexto(
            rutaUtf16,
            "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
            "<Project><Texto>acción, niñez y 00_Plantilla</Texto></Project>",
            new UnicodeEncoding(false, true, true));

        new ProyectoService().ActualizarReferencias(
            plantilla.RutaPlantilla,
            "01_Año");

        byte[] utf8Bom = File.ReadAllBytes(rutaUtf8Bom);
        byte[] utf8SinBom = File.ReadAllBytes(rutaUtf8SinBom);
        byte[] utf16 = File.ReadAllBytes(rutaUtf16);

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, utf8Bom[..3]);
        Assert.False(
            utf8SinBom.Length >= 3 &&
            utf8SinBom[0] == 0xEF &&
            utf8SinBom[1] == 0xBB &&
            utf8SinBom[2] == 0xBF);
        Assert.Equal(new byte[] { 0xFF, 0xFE }, utf16[..2]);
        Assert.Contains(
            "acción y niñez: 01_Año",
            File.ReadAllText(rutaUtf8Bom, new UTF8Encoding(true, true)));
        Assert.Contains(
            "acción y niñez: 01_Año",
            File.ReadAllText(rutaUtf8SinBom, new UTF8Encoding(false, true)));
        Assert.Contains(
            "acción, niñez y 01_Año",
            File.ReadAllText(rutaUtf16, Encoding.Unicode));
    }
}
