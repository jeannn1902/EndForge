using EndForge.Models;
using EndForge.Services;
using System.Text.Json;

namespace EndForge.Tests;

public sealed class MotivacionIntegracionPersistenciaTests {
    private const string PracticaId = "practica-integracion";
    private static readonly DateTimeOffset FechaBase =
        new(2026, 7, 31, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FalloMotivacionalPosterior_NoRevierteProgresoPublicado() {
        using EntornoPersistencia entorno = new();
        ResultadoEscrituraProgreso escritura = entorno.Progreso.ActualizarEstado(
            PracticaId,
            EstadoPracticaCurso.Realizada,
            entorno.RutaProyecto);
        Assert.True(escritura.EsExitosa);
        ProgresoCurso snapshot = Assert.IsType<ProgresoCurso>(
            escritura.ProgresoPersistido);
        byte[] bytesPublicados = File.ReadAllBytes(entorno.Progreso.RutaProgreso);
        string snapshotPublicado = Serializar(snapshot);
        SistemaArchivosMotivacionConFalloPublicacion archivos = new();
        MotivacionService motivacion = entorno.CrearMotivacion(archivos);

        ResultadoProcesamientoMotivacion resultado =
            motivacion.ProcesarProgresoPersistido(
                PracticaId,
                snapshot,
                vinculoPersistidoAhora: true,
                realizadaPersistidaAhora: true);

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.IsType<IOException>(resultado.Error);
        Assert.Equal(1, archivos.EscriturasDurables);
        Assert.Equal(1, archivos.IntentosPublicacion);
        Assert.Equal(
            bytesPublicados,
            File.ReadAllBytes(entorno.Progreso.RutaProgreso));
        Assert.Equal(snapshotPublicado, Serializar(snapshot));
        ResultadoCargaProgreso recarga = entorno.Progreso.CargarProgreso();
        Assert.True(recarga.DatosDisponibles);
        Assert.Equal(snapshotPublicado, Serializar(recarga.Progreso));
        Assert.False(File.Exists(motivacion.RutaMotivacion));
        Assert.Empty(Directory.EnumerateFiles(
            entorno.Carpeta,
            ".motivacion-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    [Fact]
    public void FalloMotivacionalPosterior_NoRevierteHistorialPublicado() {
        using EntornoPersistencia entorno = new();
        IntentoPractica intento = CrearIntento(entorno.RutaProyecto);
        ResultadoEscrituraHistorialEvaluaciones escritura =
            entorno.Historial.GuardarIntento(intento);
        Assert.True(escritura.EsExitosa);
        HistorialPractica snapshot = Assert.IsType<HistorialPractica>(
            escritura.HistorialActualizado);
        byte[] bytesPublicados = File.ReadAllBytes(
            entorno.Historial.RutaEvaluaciones);
        string snapshotPublicado = Serializar(snapshot);
        SistemaArchivosMotivacionConFalloPublicacion archivos = new();
        MotivacionService motivacion = entorno.CrearMotivacion(archivos);

        ResultadoProcesamientoMotivacion resultado =
            motivacion.ProcesarEvaluacionPersistida(
                PracticaId,
                snapshot,
                intento);

        Assert.Equal(
            EstadoProcesamientoMotivacion.ErrorRecuperable,
            resultado.Estado);
        Assert.IsType<IOException>(resultado.Error);
        Assert.Equal(1, archivos.EscriturasDurables);
        Assert.Equal(1, archivos.IntentosPublicacion);
        Assert.Equal(
            bytesPublicados,
            File.ReadAllBytes(entorno.Historial.RutaEvaluaciones));
        Assert.Equal(snapshotPublicado, Serializar(snapshot));
        ResultadoCargaHistorialEvaluaciones recarga =
            entorno.Historial.CargarHistorial();
        Assert.True(recarga.DatosDisponibles);
        Assert.Equal(
            snapshotPublicado,
            Serializar(Assert.Single(recarga.Historial.Practicas)));
        Assert.False(File.Exists(motivacion.RutaMotivacion));
        Assert.Empty(Directory.EnumerateFiles(
            entorno.Carpeta,
            ".motivacion-*.tmp",
            SearchOption.TopDirectoryOnly));
    }

    private static IntentoPractica CrearIntento(string rutaProyecto) {
        return new IntentoPractica {
            Id = "intento-integracion",
            PracticaId = PracticaId,
            Fecha = FechaBase,
            Calificacion = 85,
            Compilo = true,
            PruebasSuperadas = 1,
            PruebasTotales = 1,
            ResultadoGeneral = "Aprobada",
            EjecucionFinalizada = true,
            PuntosObtenidos = 85,
            PuntosMaximos = 100,
            RutaProyecto = rutaProyecto
        };
    }

    private static IReadOnlyList<GradoCurso> CrearCatalogo() {
        PracticaCurso practica = new() {
            Id = PracticaId,
            TemaId = "tema-integracion",
            Numero = 1,
            Nombre = "Practica de integracion"
        };
        TemaCurso tema = new() {
            Id = "tema-integracion",
            Numero = 1,
            Nombre = "Tema de integracion",
            Practicas = new[] { practica }
        };
        return new[] {
            new GradoCurso {
                Id = "grado-integracion",
                Numero = 1,
                Nombre = "Grado de integracion",
                EsContenidoDisponible = true,
                Temas = new[] { tema }
            }
        };
    }

    private static string Serializar<T>(T valor) => JsonSerializer.Serialize(valor);

    private sealed class EntornoPersistencia : IDisposable {
        public EntornoPersistencia() {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-Motivacion-Integracion-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
            RutaProyecto = Path.Combine(Carpeta, "Practica");
            Progreso = new ProgresoCursoService(Carpeta);
            Historial = new HistorialEvaluacionesService(Carpeta);
        }

        public string Carpeta { get; }

        public string RutaProyecto { get; }

        public ProgresoCursoService Progreso { get; }

        public HistorialEvaluacionesService Historial { get; }

        public MotivacionService CrearMotivacion(
            ISistemaArchivosMotivacion archivos) {
            return new MotivacionService(
                Carpeta,
                new TimeProviderFijo(FechaBase),
                CrearCatalogo,
                Progreso.CargarProgreso,
                Historial.CargarHistorial,
                archivos);
        }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }
    }

    private sealed class TimeProviderFijo : TimeProvider {
        private readonly DateTimeOffset ahora;

        public TimeProviderFijo(DateTimeOffset ahora) {
            this.ahora = ahora;
        }

        public override DateTimeOffset GetUtcNow() => ahora;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class SistemaArchivosMotivacionConFalloPublicacion :
        ISistemaArchivosMotivacion {
        private readonly SistemaArchivosMotivacion real = new();

        public int EscriturasDurables { get; private set; }

        public int IntentosPublicacion { get; private set; }

        public bool ArchivoExiste(string ruta) => real.ArchivoExiste(ruta);

        public long ObtenerLongitud(string ruta) => real.ObtenerLongitud(ruta);

        public string LeerTodoTexto(string ruta) => real.LeerTodoTexto(ruta);

        public void CrearDirectorio(string ruta) => real.CrearDirectorio(ruta);

        public void EscribirTodoTextoDurable(string ruta, string contenido) {
            real.EscribirTodoTextoDurable(ruta, contenido);
            EscriturasDurables++;
        }

        public void Reemplazar(string origen, string destino) {
            IntentosPublicacion++;
            throw new IOException("Fallo de publicacion motivacional simulado.");
        }

        public void Mover(string origen, string destino) {
            IntentosPublicacion++;
            throw new IOException("Fallo de publicacion motivacional simulado.");
        }

        public void Eliminar(string ruta) => real.Eliminar(ruta);

        public IEnumerable<string> EnumerarArchivos(
            string carpeta,
            string patron) => real.EnumerarArchivos(carpeta, patron);

        public DateTime ObtenerUltimaEscrituraUtc(string ruta) =>
            real.ObtenerUltimaEscrituraUtc(ruta);
    }
}
