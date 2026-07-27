using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class TemasServiceTests {
    [Fact]
    public void CargarTemasDetallado_RutaInexistente_NoSeConfundeConCarpetaVacia() {
        using DirectorioTemporalTemas temporal = new();
        string rutaInexistente = Path.Combine(temporal.Raiz, "eliminada");

        ResultadoCargaTemas resultado =
            new TemasService().CargarTemasDetallado(rutaInexistente);

        Assert.Equal(EstadoCargaTemas.RutaInexistente, resultado.Estado);
        Assert.Empty(resultado.Temas);
    }

    [Fact]
    public void CargarTemasDetallado_EnumeracionFalla_DevuelveErrorIo() {
        TemasService servicio = CrearServicioConEnumeracion(
            _ => throw new IOException("Fallo simulado de enumeración."));

        ResultadoCargaTemas resultado =
            servicio.CargarTemasDetallado(Path.GetTempPath());

        Assert.Equal(EstadoCargaTemas.ErrorIo, resultado.Estado);
        Assert.Empty(resultado.Temas);
    }

    [Fact]
    public void CargarTemasDetallado_CarpetaVacia_EsExitosa() {
        using DirectorioTemporalTemas temporal = new();

        ResultadoCargaTemas resultado =
            new TemasService().CargarTemasDetallado(temporal.Raiz);

        Assert.Equal(EstadoCargaTemas.Exitosa, resultado.Estado);
        Assert.Empty(resultado.Temas);
    }

    [Fact]
    public void ObtenerSiguienteNumero_TemaInexistente_NoDevuelveUno() {
        using DirectorioTemporalTemas temporal = new();

        ResultadoNumeracionPractica resultado =
            new TemasService().ObtenerSiguienteNumero(
                temporal.Raiz,
                "01_TemaEliminado");

        Assert.Equal(
            EstadoNumeracionPractica.TemaInexistente,
            resultado.Estado);
        Assert.Null(resultado.Numero);
    }

    [Fact]
    public void ObtenerSiguienteNumero_EnumeracionFalla_NoInventaNumero() {
        TemasService servicio = CrearServicioConEnumeracion(
            _ => throw new UnauthorizedAccessException("Fallo simulado de permisos."));

        ResultadoNumeracionPractica resultado =
            servicio.ObtenerSiguienteNumero(
                Path.GetTempPath(),
                "01_Tema");

        Assert.Equal(
            EstadoNumeracionPractica.PermisosInsuficientes,
            resultado.Estado);
        Assert.Null(resultado.Numero);
    }

    [Fact]
    public void ObtenerSiguienteNumero_ConHuecosEInvalidos_UsaMayorValido() {
        using DirectorioTemporalTemas temporal = new();
        string tema = temporal.CrearTema("01_Tema");
        Directory.CreateDirectory(Path.Combine(tema, "01_Uno"));
        Directory.CreateDirectory(Path.Combine(tema, "03_Tres"));
        Directory.CreateDirectory(Path.Combine(tema, "sin_numero"));
        Directory.CreateDirectory(Path.Combine(tema, "04_"));

        ResultadoNumeracionPractica resultado =
            new TemasService().ObtenerSiguienteNumero(
                temporal.Raiz,
                "01_Tema");

        Assert.Equal(EstadoNumeracionPractica.Exitosa, resultado.Estado);
        Assert.Equal(4, resultado.Numero);
    }

    [Fact]
    public void ObtenerSiguienteNumero_AceptaRutaAnidadaSeguraDelCurso() {
        using DirectorioTemporalTemas temporal = new();
        string grado = temporal.CrearTema("Grado_01");
        string tema = Path.Combine(grado, "01_Tema");
        Directory.CreateDirectory(tema);
        Directory.CreateDirectory(Path.Combine(tema, "01_Practica"));

        ResultadoNumeracionPractica resultado =
            new TemasService().ObtenerSiguienteNumero(
                temporal.Raiz,
                Path.Combine("Grado_01", "01_Tema"));

        Assert.Equal(
            EstadoNumeracionPractica.Exitosa,
            resultado.Estado);
        Assert.Equal(2, resultado.Numero);
    }

    [Fact]
    public void ObtenerSiguienteNumero_RechazaEscapeConPuntoPunto() {
        using DirectorioTemporalTemas temporal = new();
        temporal.CrearTema("01_Tema");

        ResultadoNumeracionPractica resultado =
            new TemasService().ObtenerSiguienteNumero(
                temporal.Raiz,
                Path.Combine("Grado_01", "..", "01_Tema"));

        Assert.Equal(
            EstadoNumeracionPractica.TemaInexistente,
            resultado.Estado);
        Assert.Null(resultado.Numero);
    }

    [Fact]
    public void ObtenerSiguienteNumero_RechazaPuntoDeReanalisisIntermedio() {
        using PlantillaTestHelper temporal = new();
        string gradoExterno = temporal.CrearRuta("GradoExterno");
        Directory.CreateDirectory(
            Path.Combine(gradoExterno, "01_Tema"));
        string enlaceGrado = Path.Combine(
            temporal.RutaBase,
            "Grado_01");

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(
            enlaceGrado,
            gradoExterno)) {
            return;
        }

        ResultadoNumeracionPractica resultado =
            new TemasService().ObtenerSiguienteNumero(
                temporal.RutaBase,
                Path.Combine("Grado_01", "01_Tema"));

        Assert.Equal(
            EstadoNumeracionPractica.TemaInexistente,
            resultado.Estado);
        Assert.Null(resultado.Numero);
    }

    [Fact]
    public void CargarTemasDetallado_IgnoraEnlaceQueEscapaDeRutaBase() {
        using PlantillaTestHelper temporal = new();
        string externo = temporal.CrearRuta("TemaExterno");
        string enlace = Path.Combine(
            temporal.RutaBase,
            "01_Enlace");
        Directory.CreateDirectory(externo);

        if (!PlantillaTestHelper.IntentarCrearEnlaceDirectorio(
            enlace,
            externo)) {
            return;
        }

        ResultadoCargaTemas resultado =
            new TemasService().CargarTemasDetallado(
                temporal.RutaBase);

        Assert.Equal(EstadoCargaTemas.Exitosa, resultado.Estado);
        Assert.DoesNotContain("01_Enlace", resultado.Temas);
    }

    [Fact]
    public void ObtenerSiguienteNumero_IntMaxValue_NoDesborda() {
        using DirectorioTemporalTemas temporal = new();
        string tema = temporal.CrearTema("01_Tema");
        Directory.CreateDirectory(
            Path.Combine(tema, $"{int.MaxValue}_Ultima"));

        ResultadoNumeracionPractica resultado =
            new TemasService().ObtenerSiguienteNumero(
                temporal.Raiz,
                "01_Tema");

        Assert.Equal(
            EstadoNumeracionPractica.LimiteAlcanzado,
            resultado.Estado);
        Assert.Null(resultado.Numero);
    }

    [Fact]
    public void VistaPrevia_TemaInexistente_NoDevuelveNombreCompleto() {
        using DirectorioTemporalTemas temporal = new();
        VistaPreviaPracticaService servicio =
            new(new TemasService());

        ResultadoVistaPreviaPractica resultado = servicio.Calcular(
            temporal.Raiz,
            "01_TemaEliminado",
            "Mi practica");

        Assert.Equal(
            EstadoVistaPreviaPractica.NumeracionNoDisponible,
            resultado.Estado);
        Assert.Empty(resultado.NombreFinal);
    }

    private static TemasService CrearServicioConEnumeracion(
        Func<string, string[]> enumerarDirectorios) {
        return new TemasService(enumerarDirectorios);
    }

    private sealed class DirectorioTemporalTemas : IDisposable {
        public DirectorioTemporalTemas() {
            Raiz = Path.Combine(
                Path.GetTempPath(),
                $"EndForge.Tests-Temas-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Raiz);
        }

        public string Raiz { get; }

        public string CrearTema(string nombre) {
            string ruta = Path.Combine(Raiz, nombre);
            Directory.CreateDirectory(ruta);
            return ruta;
        }

        public void Dispose() {
            if (Directory.Exists(Raiz)) {
                Directory.Delete(Raiz, recursive: true);
            }
        }
    }
}
