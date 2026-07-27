using System.Diagnostics;
using System.Text;

namespace EndForge.Tests;

internal sealed class PlantillaTestHelper : IDisposable {
    private const string TipoProyectoCpp =
        "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

    public PlantillaTestHelper() {
        DirectorioRaiz = Path.Combine(
            Path.GetTempPath(),
            $"EndForge.Tests-plantilla-{Guid.NewGuid():N}");
        RutaBase = Path.Combine(DirectorioRaiz, "Base");
        RutaPlantilla = Path.Combine(DirectorioRaiz, "Plantilla");
        RutaProyectoPlantilla = Path.Combine(
            RutaPlantilla,
            "00_Plantilla",
            "00_Plantilla.vcxproj");
        RutaCppPlantilla = Path.Combine(
            RutaPlantilla,
            "00_Plantilla",
            "00_Plantilla.cpp");

        Directory.CreateDirectory(RutaBase);
        Directory.CreateDirectory(Path.GetDirectoryName(RutaProyectoPlantilla)!);
        EscribirSolucion(
            "00_Plantilla.sln",
            @"00_Plantilla\00_Plantilla.vcxproj");
        EscribirProyecto(
            RutaProyectoPlantilla,
            CrearProyectoXml(@"00_Plantilla.cpp"));
        EscribirTexto(
            RutaCppPlantilla,
            "// código con acentos: acción, niñez\nint main() { return 0; }\n",
            new UTF8Encoding(false, true));
    }

    public string DirectorioRaiz { get; }

    public string RutaBase { get; }

    public string RutaPlantilla { get; }

    public string RutaProyectoPlantilla { get; }

    public string RutaCppPlantilla { get; }

    public string RutaSolucionPlantilla =>
        Path.Combine(RutaPlantilla, "00_Plantilla.sln");

    public string CrearRuta(string nombre) {
        return Path.Combine(DirectorioRaiz, nombre);
    }

    public void EscribirSolucion(
        string nombreArchivo,
        params string[] referenciasProyecto) {
        string contenido = "Microsoft Visual Studio Solution File, Format Version 12.00\r\n";

        foreach (string referencia in referenciasProyecto) {
            string nombre = Path.GetFileNameWithoutExtension(referencia);
            contenido +=
                $"Project(\"{TipoProyectoCpp}\") = \"{nombre}\", " +
                $"\"{referencia}\", \"{{{Guid.NewGuid():D}}}\"\r\n" +
                "EndProject\r\n";
        }

        EscribirTexto(
            Path.Combine(RutaPlantilla, nombreArchivo),
            contenido,
            new UTF8Encoding(false, true));
    }

    public static string CrearProyectoXml(params string[] referenciasCpp) {
        string elementos = string.Join(
            Environment.NewLine,
            referenciasCpp.Select(
                referencia => $"    <ClCompile Include=\"{referencia}\" />"));

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <PropertyGroup Label="Configuration">
                <ConfigurationType>Application</ConfigurationType>
              </PropertyGroup>
              <ItemGroup>
            {elementos}
              </ItemGroup>
            </Project>
            """;
    }

    public static string CrearFiltersXml(params string[] referenciasCpp) {
        string elementos = string.Join(
            Environment.NewLine,
            referenciasCpp.Select(
                referencia => $"    <ClCompile Include=\"{referencia}\" />"));

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <ItemGroup>
            {elementos}
              </ItemGroup>
            </Project>
            """;
    }

    public static void EscribirProyecto(
        string ruta,
        string contenido,
        Encoding? encoding = null) {
        EscribirTexto(
            ruta,
            contenido,
            encoding ?? new UTF8Encoding(false, true));
    }

    public static void EscribirTexto(
        string ruta,
        string contenido,
        Encoding encoding) {
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        File.WriteAllText(ruta, contenido, encoding);
    }

    public static bool IntentarCrearEnlaceDirectorio(
        string rutaEnlace,
        string rutaDestino) {
        try {
            Directory.CreateSymbolicLink(rutaEnlace, rutaDestino);
            return true;
        } catch (Exception) {
            // En Windows sin modo desarrollador se intenta crear una junction.
        }

        try {
            ProcessStartInfo inicio = new() {
                FileName = Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            inicio.ArgumentList.Add("/d");
            inicio.ArgumentList.Add("/c");
            inicio.ArgumentList.Add("mklink");
            inicio.ArgumentList.Add("/J");
            inicio.ArgumentList.Add(rutaEnlace);
            inicio.ArgumentList.Add(rutaDestino);

            using Process proceso = Process.Start(inicio)!;
            proceso.WaitForExit();
            return proceso.ExitCode == 0 && Directory.Exists(rutaEnlace);
        } catch (Exception) {
            return false;
        }
    }

    public void Dispose() {
        EliminarArbolSeguro(DirectorioRaiz);
    }

    private static void EliminarArbolSeguro(string ruta) {
        if (!Directory.Exists(ruta)) {
            return;
        }

        FileAttributes atributosRaiz = File.GetAttributes(ruta);

        if (atributosRaiz.HasFlag(FileAttributes.ReparsePoint)) {
            Directory.Delete(ruta, recursive: false);
            return;
        }

        foreach (string entrada in Directory.EnumerateFileSystemEntries(
            ruta,
            "*",
            SearchOption.TopDirectoryOnly)) {
            FileAttributes atributos = File.GetAttributes(entrada);

            if (atributos.HasFlag(FileAttributes.Directory) &&
                !atributos.HasFlag(FileAttributes.ReparsePoint)) {
                EliminarArbolSeguro(entrada);
                continue;
            }

            if (atributos.HasFlag(FileAttributes.Directory)) {
                Directory.Delete(entrada, recursive: false);
            } else {
                File.SetAttributes(entrada, FileAttributes.Normal);
                File.Delete(entrada);
            }
        }

        File.SetAttributes(
            ruta,
            File.GetAttributes(ruta) & ~FileAttributes.ReadOnly);
        Directory.Delete(ruta, recursive: false);
    }
}
