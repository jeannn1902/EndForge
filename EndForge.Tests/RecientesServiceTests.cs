using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class RecientesServiceTests {
    [Fact]
    public void LeerProyectosRecientes_ConLineaCorrupta_ConservaRegistroValido() {
        using DirectorioTemporalRecientes temporal = new();
        string proyecto = temporal.CrearProyecto("ProyectoValido");
        File.WriteAllLines(
            temporal.RutaRecientes,
            new[] {
                $"{Path.GetFileName(proyecto)}|{proyecto}",
                "registro sin separador"
            });

        ResultadoLecturaRecientes resultado =
            new RecientesService(temporal.RutaRecientes).LeerProyectosRecientes();

        Assert.Equal(EstadoLecturaRecientes.ContenidoInvalido, resultado.Estado);
        ProyectoReciente reciente = Assert.Single(resultado.Proyectos);
        Assert.Equal(Path.GetFullPath(proyecto), reciente.Ruta);
        Assert.Equal(1, resultado.RegistrosInvalidos);
        Assert.Equal(0, resultado.RegistrosNoDisponibles);
    }

    [Fact]
    public void LeerProyectosRecientes_ProyectoEliminado_NoExponeRutaObsoleta() {
        using DirectorioTemporalRecientes temporal = new();
        string proyecto = temporal.CrearProyecto("ProyectoEliminado");
        File.WriteAllText(
            temporal.RutaRecientes,
            $"{Path.GetFileName(proyecto)}|{proyecto}");
        Directory.Delete(proyecto, recursive: true);

        ResultadoLecturaRecientes resultado =
            new RecientesService(temporal.RutaRecientes).LeerProyectosRecientes();

        Assert.Equal(EstadoLecturaRecientes.ContenidoInvalido, resultado.Estado);
        Assert.Empty(resultado.Proyectos);
        Assert.Equal(0, resultado.RegistrosInvalidos);
        Assert.Equal(1, resultado.RegistrosNoDisponibles);
    }

    [Fact]
    public void LeerProyectosRecientes_SolucionEliminada_NoExponeRutaObsoleta() {
        using DirectorioTemporalRecientes temporal = new();
        string proyecto = temporal.CrearProyecto("ProyectoSinSolucion");
        File.Delete(Path.Combine(proyecto, "ProyectoSinSolucion.sln"));
        File.WriteAllText(
            temporal.RutaRecientes,
            $"{Path.GetFileName(proyecto)}|{proyecto}");

        ResultadoLecturaRecientes resultado =
            new RecientesService(temporal.RutaRecientes).LeerProyectosRecientes();

        Assert.Equal(EstadoLecturaRecientes.ContenidoInvalido, resultado.Estado);
        Assert.Empty(resultado.Proyectos);
        Assert.Equal(0, resultado.RegistrosInvalidos);
        Assert.Equal(1, resultado.RegistrosNoDisponibles);
    }

    [Fact]
    public void GuardarProyectoReciente_RutasWindowsEquivalentes_ConservaUnaEntrada() {
        using DirectorioTemporalRecientes temporal = new();
        string proyecto = temporal.CrearProyecto("ProyectoCanonico");
        string alias = Path.Combine(proyecto, ".");
        RecientesService servicio = new(temporal.RutaRecientes);

        ResultadoEscrituraRecientes primero =
            servicio.GuardarProyectoReciente(proyecto);
        ResultadoEscrituraRecientes segundo =
            servicio.GuardarProyectoReciente(alias);
        ResultadoLecturaRecientes lectura = servicio.LeerProyectosRecientes();

        Assert.True(primero.EsExitosa);
        Assert.True(segundo.EsExitosa);
        ProyectoReciente reciente = Assert.Single(lectura.Proyectos);
        Assert.Equal(Path.GetFullPath(proyecto), reciente.Ruta);
    }

    [Fact]
    public void GuardarProyectoReciente_CarpetaDatosEliminada_LaRecrea() {
        string raiz = Path.Combine(
            Path.GetTempPath(),
            $"EndForge.Tests-Recientes-{Guid.NewGuid():N}");
        string carpetaDatos = Path.Combine(raiz, "datos");
        string rutaRecientes = Path.Combine(carpetaDatos, "recientes.txt");
        string proyecto = Path.Combine(raiz, "Proyecto");

        try {
            Directory.CreateDirectory(proyecto);
            CrearArchivosProyectoCompatible(proyecto, "Proyecto");

            ResultadoEscrituraRecientes resultado =
                new RecientesService(rutaRecientes).GuardarProyectoReciente(proyecto);

            Assert.True(resultado.EsExitosa);
            Assert.True(File.Exists(rutaRecientes));
        } finally {
            if (Directory.Exists(raiz)) {
                Directory.Delete(raiz, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GuardarProyectoReciente_ActualizacionesConcurrentes_NoPierdeRutas() {
        using DirectorioTemporalRecientes temporal = new();
        File.WriteAllText(temporal.RutaRecientes, "");
        string[] proyectos = Enumerable
            .Range(1, 10)
            .Select(indice => temporal.CrearProyecto($"Proyecto{indice:00}"))
            .ToArray();
        using ManualResetEventSlim inicio = new(initialState: false);

        Task<ResultadoEscrituraRecientes>[] escrituras = proyectos
            .Select(proyecto => Task.Factory.StartNew(
                () => {
                    inicio.Wait();
                    return new RecientesService(temporal.RutaRecientes)
                        .GuardarProyectoReciente(proyecto);
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default))
            .ToArray();

        inicio.Set();
        ResultadoEscrituraRecientes[] resultados = await Task.WhenAll(escrituras);
        ResultadoLecturaRecientes lectura =
            new RecientesService(temporal.RutaRecientes).LeerProyectosRecientes();

        Assert.All(resultados, resultado => Assert.True(resultado.EsExitosa));
        Assert.Equal(10, lectura.Proyectos.Count);
        Assert.Equal(
            proyectos.OrderBy(ruta => ruta, StringComparer.OrdinalIgnoreCase),
            lectura.Proyectos
                .Select(proyecto => proyecto.Ruta)
                .OrderBy(ruta => ruta, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        Assert.Empty(Directory.EnumerateFiles(
            temporal.Raiz,
            ".recientes-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    private static void CrearArchivosProyectoCompatible(
        string ruta,
        string nombre) {
        File.WriteAllText(
            Path.Combine(ruta, $"{nombre}.sln"),
            "Microsoft Visual Studio Solution File, Format Version 12.00\r\n" +
            "Project(\"{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}\") = " +
            $"\"{nombre}\", \"{nombre}.vcxproj\", " +
            $"\"{{{Guid.NewGuid():D}}}\"\r\n" +
            "EndProject\r\n");
        File.WriteAllText(
            Path.Combine(ruta, $"{nombre}.vcxproj"),
            """
            <?xml version="1.0" encoding="utf-8"?>
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <PropertyGroup Label="Configuration">
                <ConfigurationType>Application</ConfigurationType>
              </PropertyGroup>
              <ItemGroup>
                <ClCompile Include="main.cpp" />
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(
            Path.Combine(ruta, "main.cpp"),
            "int main() { return 0; }");
    }

    private sealed class DirectorioTemporalRecientes : IDisposable {
        public DirectorioTemporalRecientes() {
            Raiz = Path.Combine(
                Path.GetTempPath(),
                $"EndForge.Tests-Recientes-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Raiz);
            RutaRecientes = Path.Combine(Raiz, "recientes.txt");
        }

        public string Raiz { get; }

        public string RutaRecientes { get; }

        public string CrearProyecto(string nombre) {
            string ruta = Path.Combine(Raiz, nombre);
            Directory.CreateDirectory(ruta);
            CrearArchivosProyectoCompatible(ruta, nombre);
            return Path.GetFullPath(ruta);
        }

        public void Dispose() {
            if (Directory.Exists(Raiz)) {
                Directory.Delete(Raiz, recursive: true);
            }
        }
    }
}
