using EndForge.Models;
using EndForge.Services;
using System.Text;

namespace EndForge.Tests;

public sealed class EjecucionArchivosTests {
    private static readonly CancellationToken SinCancelacion =
        CancellationToken.None;

    [Fact]
    public async Task CadaCasoUsaUnDirectorioUnicoYLoElimina() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        CasoPrueba caso = CrearCasoConArchivoEsperado(
            "directorio.txt",
            "cd > directorio.txt");

        ResultadoEjecucionCasoPruebaCpp primero =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);
        ResultadoEjecucionCasoPruebaCpp segundo =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        string directorioPrimero = ObtenerContenidoDisponible(
            primero,
            "directorio.txt").Trim();
        string directorioSegundo = ObtenerContenidoDisponible(
            segundo,
            "directorio.txt").Trim();

        Assert.Equal(EstadoEjecucionPruebaCpp.Exitosa, primero.Ejecucion.Estado);
        Assert.Equal(EstadoEjecucionPruebaCpp.Exitosa, segundo.Ejecucion.Estado);
        Assert.NotEqual(directorioPrimero, directorioSegundo);
        Assert.StartsWith(
            sesion.DirectorioSesion,
            directorioPrimero,
            StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(
            sesion.DirectorioSesion,
            directorioSegundo,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "caso-archivos-",
            directorioPrimero,
            StringComparison.Ordinal);
        Assert.Contains(
            "caso-archivos-",
            directorioSegundo,
            StringComparison.Ordinal);
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Fact]
    public async Task PreparaEntradaUtf8SinBomYCapturaElContenido() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        const string contenido = "Línea ágil\r\nniñez y pingüino";
        string comandos = string.Join(
            "\r\n",
            "powershell.exe -NoLogo -NoProfile -NonInteractive -Command " +
                "\"$b=[IO.File]::ReadAllBytes('entrada.txt'); " +
                "if($b.Length -ge 3 -and $b[0] -eq 239 -and " +
                "$b[1] -eq 187 -and $b[2] -eq 191){exit 23}; " +
                "[IO.File]::WriteAllBytes('salida.txt',$b)\"",
            "exit /b %errorlevel%");
        CasoPrueba caso = new() {
            Id = "archivos-utf8",
            Entrada = comandos,
            ArchivosEntrada = new[] {
                new ArchivoEntradaPrueba {
                    RutaRelativa = "entrada.txt",
                    Contenido = contenido
                }
            },
            ArchivosEsperados = new[] {
                new ArchivoEsperadoPrueba {
                    RutaRelativa = "salida.txt"
                }
            }
        };

        ResultadoEjecucionCasoPruebaCpp resultado =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        Assert.Equal(EstadoEjecucionPruebaCpp.Exitosa, resultado.Ejecucion.Estado);
        Assert.Equal(
            contenido,
            ObtenerContenidoDisponible(resultado, "salida.txt"));
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Fact]
    public async Task RechazaRutasAbsolutasYSegmentosPadre() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        string rutaAbsoluta = Path.Combine(
            sesion.DirectorioRaiz,
            "fuera.txt");

        foreach (string ruta in new[] { rutaAbsoluta, @"..\fuera.txt" }) {
            CasoPrueba caso = new() {
                Id = $"ruta-invalida-{Guid.NewGuid():N}",
                Entrada = "exit /b 0",
                ArchivosEntrada = new[] {
                    new ArchivoEntradaPrueba {
                        RutaRelativa = ruta,
                        Contenido = "no debe escribirse"
                    }
                }
            };

            ResultadoEjecucionCasoPruebaCpp resultado =
                await servicio.EjecutarCasoAsync(
                    sesion.Sesion,
                    caso,
                    SinCancelacion);

            Assert.Equal(
                EstadoEjecucionPruebaCpp.ErrorInfraestructura,
                resultado.Ejecucion.Estado);
            Assert.IsType<InvalidOperationException>(
                resultado.Ejecucion.Error);
            Assert.False(File.Exists(rutaAbsoluta));
            sesion.AssertSinDirectoriosDeCaso();
        }
    }

    [Fact]
    public async Task RechazaArchivoDeEntradaMayorA256KiB() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        CasoPrueba caso = new() {
            Id = "entrada-demasiado-grande",
            Entrada = "exit /b 0",
            ArchivosEntrada = new[] {
                new ArchivoEntradaPrueba {
                    RutaRelativa = "grande.txt",
                    Contenido = new string('x', (256 * 1024) + 1)
                }
            }
        };

        ResultadoEjecucionCasoPruebaCpp resultado =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        Assert.Equal(
            EstadoEjecucionPruebaCpp.ErrorInfraestructura,
            resultado.Ejecucion.Estado);
        Assert.IsType<InvalidOperationException>(resultado.Ejecucion.Error);
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Fact]
    public async Task ReportaArchivoEsperadoMayorA256KiB() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        CasoPrueba caso = CrearCasoConArchivoEsperado(
            "grande.bin",
            "powershell.exe -NoLogo -NoProfile -NonInteractive -Command " +
                "\"[IO.File]::WriteAllBytes('grande.bin'," +
                "(New-Object byte[] 262145))\"");

        ResultadoEjecucionCasoPruebaCpp resultado =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        Assert.Equal(EstadoEjecucionPruebaCpp.Exitosa, resultado.Ejecucion.Estado);
        ResultadoArchivoPrueba archivo = Assert.Single(resultado.Archivos);
        Assert.Equal(EstadoArchivoPrueba.ContenidoExcesivo, archivo.Estado);
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Fact]
    public async Task RechazaPuntoDeReanalisisSinSeguirloNiBorrarElDestino() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        string destino = sesion.CrearDirectorioExterno("destino-enlace");
        string archivoExterno = Path.Combine(destino, "externo.txt");
        File.WriteAllText(archivoExterno, "intacto", new UTF8Encoding(false));
        string comandos = string.Join(
            "\r\n",
            $"mklink /J enlace \"{destino}\" >nul",
            "exit /b %errorlevel%");
        CasoPrueba caso = CrearCasoConArchivoEsperado(
            @"enlace\externo.txt",
            comandos);

        ResultadoEjecucionCasoPruebaCpp resultado =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        Assert.Equal(EstadoEjecucionPruebaCpp.Exitosa, resultado.Ejecucion.Estado);
        ResultadoArchivoPrueba archivo = Assert.Single(resultado.Archivos);
        Assert.Equal(EstadoArchivoPrueba.PuntoDeReanalisis, archivo.Estado);
        Assert.Equal("intacto", File.ReadAllText(archivoExterno));
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Theory]
    [InlineData(0, EstadoEjecucionPruebaCpp.Exitosa)]
    [InlineData(7, EstadoEjecucionPruebaCpp.CodigoSalidaNoCero)]
    public async Task LimpiaArchivosReadOnlyTrasExitoOFallo(
        int codigoSalida,
        EstadoEjecucionPruebaCpp estadoEsperado) {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        string comandos = string.Join(
            "\r\n",
            ">solo-lectura.txt echo contenido",
            "attrib +R solo-lectura.txt",
            $"exit /b {codigoSalida}");
        CasoPrueba caso = CrearCasoConArchivoEsperado(
            "solo-lectura.txt",
            comandos);

        ResultadoEjecucionCasoPruebaCpp resultado =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        Assert.Equal(estadoEsperado, resultado.Ejecucion.Estado);
        Assert.Equal(
            EstadoArchivoPrueba.Disponible,
            Assert.Single(resultado.Archivos).Estado);
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Fact]
    public async Task CasosConsecutivosNoCompartenArchivos() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        CasoPrueba primerCaso = CrearCasoConArchivoEsperado(
            "estado.txt",
            string.Join(
                "\r\n",
                ">residuo.txt echo dato",
                ">estado.txt echo primero"));
        CasoPrueba segundoCaso = CrearCasoConArchivoEsperado(
            "estado.txt",
            "if exist residuo.txt " +
                "(>estado.txt echo contaminado) " +
                "else (>estado.txt echo limpio)");

        ResultadoEjecucionCasoPruebaCpp primero =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                primerCaso,
                SinCancelacion);
        ResultadoEjecucionCasoPruebaCpp segundo =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                segundoCaso,
                SinCancelacion);

        Assert.Equal(
            "primero",
            ObtenerContenidoDisponible(primero, "estado.txt").Trim());
        Assert.Equal(
            "limpio",
            ObtenerContenidoDisponible(segundo, "estado.txt").Trim());
        sesion.AssertSinDirectoriosDeCaso();
    }

    [Fact]
    public async Task ArchivoEsperadoAusenteSeReportaSinDejarResiduos() {
        using SesionTemporal sesion = new();
        EjecucionPruebasService servicio = new();
        CasoPrueba caso = CrearCasoConArchivoEsperado(
            "nombre-esperado.txt",
            ">otro-nombre.txt echo contenido");

        ResultadoEjecucionCasoPruebaCpp resultado =
            await servicio.EjecutarCasoAsync(
                sesion.Sesion,
                caso,
                SinCancelacion);

        Assert.Equal(EstadoEjecucionPruebaCpp.Exitosa, resultado.Ejecucion.Estado);
        Assert.Equal(
            EstadoArchivoPrueba.Ausente,
            Assert.Single(resultado.Archivos).Estado);
        sesion.AssertSinDirectoriosDeCaso();
    }

    private static CasoPrueba CrearCasoConArchivoEsperado(
        string rutaEsperada,
        string comandos) {
        return new CasoPrueba {
            Id = $"archivos-{Guid.NewGuid():N}",
            Entrada = $"{comandos}\r\nexit /b %errorlevel%\r\n",
            ArchivosEsperados = new[] {
                new ArchivoEsperadoPrueba {
                    RutaRelativa = rutaEsperada
                }
            }
        };
    }

    private static string ObtenerContenidoDisponible(
        ResultadoEjecucionCasoPruebaCpp resultado,
        string rutaEsperada) {
        ResultadoArchivoPrueba archivo = Assert.Single(resultado.Archivos);
        Assert.Equal(rutaEsperada, archivo.RutaRelativa);
        Assert.Equal(EstadoArchivoPrueba.Disponible, archivo.Estado);
        return archivo.ContenidoObtenido;
    }

    private sealed class SesionTemporal : IDisposable {
        private static readonly UTF8Encoding Utf8SinBom = new(false);
        private bool eliminada;

        public SesionTemporal() {
            DirectorioRaiz = Path.Combine(
                Path.GetTempPath(),
                $"EndForge.Tests-{Guid.NewGuid():N}");
            Assert.StartsWith(
                Path.GetFullPath(Path.GetTempPath()),
                DirectorioRaiz,
                StringComparison.OrdinalIgnoreCase);

            string identificador = Guid.NewGuid().ToString("N");
            DirectorioSesion = Path.Combine(
                DirectorioRaiz,
                $"compilacion-{identificador}");
            string directorioPractica = Path.Combine(
                DirectorioRaiz,
                "practica");
            Directory.CreateDirectory(DirectorioSesion);
            Directory.CreateDirectory(directorioPractica);
            File.WriteAllText(
                Path.Combine(
                    DirectorioSesion,
                    ".endforge-evaluation-owned"),
                identificador,
                Utf8SinBom);

            string ejecutableSistema =
                Environment.GetEnvironmentVariable("ComSpec") ??
                Path.Combine(Environment.SystemDirectory, "cmd.exe");
            string ejecutableTemporal = Path.Combine(
                DirectorioSesion,
                "ejecutor-pruebas.exe");
            File.Copy(
                ejecutableSistema,
                ejecutableTemporal,
                overwrite: false);

            Sesion = new SesionCompilacionCpp(
                directorioPractica,
                rutaSolucion: "",
                rutaProyectoCpp: "",
                ejecutableTemporal,
                DirectorioSesion,
                DirectorioRaiz,
                identificador);
        }

        public string DirectorioRaiz { get; }

        public string DirectorioSesion { get; }

        public SesionCompilacionCpp Sesion { get; }

        public string CrearDirectorioExterno(string nombre) {
            string directorio = Path.Combine(DirectorioRaiz, nombre);
            Directory.CreateDirectory(directorio);
            return directorio;
        }

        public void AssertSinDirectoriosDeCaso() {
            Assert.Empty(Directory.EnumerateDirectories(
                DirectorioSesion,
                "caso-archivos-*",
                SearchOption.TopDirectoryOnly));
        }

        public void Dispose() {
            if (eliminada) {
                return;
            }

            eliminada = true;
            Sesion.Dispose();
            EliminarArbolPropio(DirectorioRaiz);
            Assert.False(Directory.Exists(DirectorioRaiz));
        }

        private static void EliminarArbolPropio(string directorio) {
            if (!Directory.Exists(directorio)) {
                return;
            }

            FileAttributes atributosDirectorio = File.GetAttributes(directorio);

            if (atributosDirectorio.HasFlag(FileAttributes.ReparsePoint)) {
                Directory.Delete(directorio, recursive: false);
                return;
            }

            foreach (string entrada in Directory.EnumerateFileSystemEntries(
                directorio,
                "*",
                SearchOption.TopDirectoryOnly)) {
                FileAttributes atributos = File.GetAttributes(entrada);

                if (atributos.HasFlag(FileAttributes.Directory)) {
                    EliminarArbolPropio(entrada);
                } else {
                    if (atributos.HasFlag(FileAttributes.ReadOnly)) {
                        File.SetAttributes(
                            entrada,
                            atributos & ~FileAttributes.ReadOnly);
                    }

                    File.Delete(entrada);
                }
            }

            if (atributosDirectorio.HasFlag(FileAttributes.ReadOnly)) {
                File.SetAttributes(
                    directorio,
                    atributosDirectorio & ~FileAttributes.ReadOnly);
            }

            Directory.Delete(directorio, recursive: false);
        }
    }
}
