using EndForge.Services;

namespace EndForge.Tests;

public sealed class RegistroErroresTests {
    [Fact]
    public void ConstructorPredeterminado_UsaDirectorioLocalAppDataDeEndForge() {
        RegistroErroresService servicio = new();

        string rutaEsperada = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EndForge",
            "Logs");

        Assert.Equal(Path.GetFullPath(rutaEsperada), servicio.RutaDirectorioLogs);
    }

    [Fact]
    public void Registrar_CreaLogAcotadoSinIncluirContenidoSensible() {
        using CarpetaTemporal carpeta = new();
        RegistroErroresService servicio = new(
            carpeta.Ruta,
            cantidadMaximaArchivos: 5,
            tamanoMaximoArchivoBytes: 1024);
        InvalidOperationException error = CrearExcepcionConStack(
            "CASO_OCULTO entrada=917; codigo=int main() { return 917; }; " +
            carpeta.Ruta,
            new Exception("SALIDA_ESPERADA_SECRETA"));
        error.Data["codigo-estudiante"] = "cout << 917;";

        bool registrado = servicio.Registrar(
            error,
            OrigenRegistroError.Interfaz,
            esTerminante: false);

        Assert.True(registrado);
        string archivo = Assert.Single(Directory.GetFiles(carpeta.Ruta, "*.log"));
        string contenido = File.ReadAllText(archivo);

        Assert.Contains(typeof(InvalidOperationException).FullName!, contenido);
        Assert.Contains(nameof(OrigenRegistroError.Interfaz), contenido);
        Assert.Contains("Terminante: False", contenido);
        Assert.Contains($"0x{error.HResult:X8}", contenido);
        Assert.Contains(nameof(CrearExcepcionConStack), contenido);
        Assert.DoesNotContain("CASO_OCULTO", contenido, StringComparison.Ordinal);
        Assert.DoesNotContain("int main", contenido, StringComparison.Ordinal);
        Assert.DoesNotContain("SALIDA_ESPERADA_SECRETA", contenido, StringComparison.Ordinal);
        Assert.DoesNotContain("cout", contenido, StringComparison.Ordinal);
        Assert.DoesNotContain(carpeta.Ruta, contenido, StringComparison.OrdinalIgnoreCase);
        Assert.InRange(new FileInfo(archivo).Length, 1, 1024);
    }

    [Fact]
    public void Registrar_RespetaCantidadMaximaDeArchivos() {
        using CarpetaTemporal carpeta = new();
        RegistroErroresService servicio = new(
            carpeta.Ruta,
            cantidadMaximaArchivos: 3,
            tamanoMaximoArchivoBytes: 1024);

        for (int indice = 0; indice < 8; indice++) {
            Assert.True(servicio.Registrar(
                new IOException($"mensaje sensible {indice}"),
                OrigenRegistroError.TareaNoObservada,
                esTerminante: false));
        }

        Assert.Equal(3, Directory.GetFiles(carpeta.Ruta, "*.log").Length);
        Assert.Empty(Directory.GetFiles(carpeta.Ruta, "*.tmp"));
    }

    [Fact]
    public void Registrar_RespetaTamanoMaximoInclusoConExcepcionProfunda() {
        using CarpetaTemporal carpeta = new();
        RegistroErroresService servicio = new(
            carpeta.Ruta,
            cantidadMaximaArchivos: 2,
            tamanoMaximoArchivoBytes: 96);
        Exception error = CrearCadenaDeExcepciones(20);

        Assert.True(servicio.Registrar(
            error,
            OrigenRegistroError.DominioAplicacion,
            esTerminante: true));

        string archivo = Assert.Single(Directory.GetFiles(carpeta.Ruta, "*.log"));
        Assert.InRange(new FileInfo(archivo).Length, 1, 96);
    }

    [Fact]
    public void Registrar_SiNoPuedeCrearDirectorio_NoPropagaLaExcepcion() {
        using CarpetaTemporal carpeta = new();
        string archivoBloqueandoRuta = Path.Combine(carpeta.Ruta, "archivo");
        File.WriteAllText(archivoBloqueandoRuta, "contenido");
        RegistroErroresService servicio = new(
            Path.Combine(archivoBloqueandoRuta, "Logs"),
            cantidadMaximaArchivos: 3,
            tamanoMaximoArchivoBytes: 1024);

        Exception? excepcion = Record.Exception(() => {
            bool registrado = servicio.Registrar(
                new UnauthorizedAccessException("detalle sensible"),
                OrigenRegistroError.Interfaz,
                esTerminante: false);

            Assert.False(registrado);
        });

        Assert.Null(excepcion);
    }

    [Theory]
    [InlineData(typeof(OutOfMemoryException))]
    [InlineData(typeof(StackOverflowException))]
    [InlineData(typeof(AccessViolationException))]
    [InlineData(typeof(BadImageFormatException))]
    public void EsExcepcionCritica_ReconoceErroresQueNoDebenRecuperarse(Type tipoError) {
        Exception error = (Exception)Activator.CreateInstance(tipoError)!;

        Assert.True(RegistroErroresService.EsExcepcionCritica(error));
    }

    [Fact]
    public void EsExcepcionCritica_RevisaExcepcionesAgregadasEInternas() {
        AggregateException agregada = new(
            new IOException("recuperable"),
            new InvalidOperationException(
                "contenedor",
                new AccessViolationException("critica")));

        Assert.True(RegistroErroresService.EsExcepcionCritica(agregada));
    }

    [Theory]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    public void EsExcepcionCritica_NoMarcaErroresOperativosComoFatales(Type tipoError) {
        Exception error = (Exception)Activator.CreateInstance(
            tipoError,
            "mensaje sensible")!;

        Assert.False(RegistroErroresService.EsExcepcionCritica(error));
    }

    [Fact]
    public void MensajeRecuperable_NoExponeRutaDeLogsNiDetallesTecnicos() {
        string mensaje = Program.MensajeErrorRecuperable;

        Assert.DoesNotContain("LocalAppData", mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Logs", mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(":\\", mensaje, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", mensaje, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Stack", mensaje, StringComparison.OrdinalIgnoreCase);
    }

    private static Exception CrearCadenaDeExcepciones(int profundidad) {
        Exception actual = new InvalidOperationException("contenido sensible");

        for (int indice = 0; indice < profundidad; indice++) {
            actual = new Exception($"secreto {indice}", actual);
        }

        return actual;
    }

    private static InvalidOperationException CrearExcepcionConStack(
        string mensaje,
        Exception interna) {
        try {
            throw new InvalidOperationException(mensaje, interna);
        } catch (InvalidOperationException error) {
            return error;
        }
    }

    private sealed class CarpetaTemporal : IDisposable {
        public string Ruta { get; } = Path.Combine(
            Path.GetTempPath(),
            $"endforge-registro-{Guid.NewGuid():N}");

        public CarpetaTemporal() {
            Directory.CreateDirectory(Ruta);
        }

        public void Dispose() {
            try {
                Directory.Delete(Ruta, recursive: true);
            } catch (IOException) {
                // La limpieza de una carpeta temporal no debe ocultar el resultado de la prueba.
            } catch (UnauthorizedAccessException) {
                // La limpieza de una carpeta temporal no debe ocultar el resultado de la prueba.
            }
        }
    }
}
