using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class ProgresoCursoServiceTests {
    private static readonly DateTimeOffset FechaBase =
        new(2026, 7, 31, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ActualizarEstado_DevuelveElDocumentoNormalizadoPublicado() {
        using EntornoProgreso entorno = new();
        ProgresoCursoService servicio = new(entorno.Carpeta);
        ResultadoEscrituraProgreso inicial = servicio.GuardarProgreso(
            CrearCurso(
                CrearPractica("practica-b", EstadoPracticaCurso.Pendiente),
                CrearPractica("practica-a", EstadoPracticaCurso.EnProgreso)));
        Assert.True(inicial.EsExitosa);

        ResultadoEscrituraProgreso resultado = servicio.ActualizarEstado(
            "practica-b",
            EstadoPracticaCurso.Realizada,
            @"C:\Practicas\B");
        ResultadoCargaProgreso recargado = servicio.CargarProgreso();

        Assert.True(resultado.EsExitosa);
        Assert.NotNull(resultado.ProgresoPersistido);
        Assert.Equal(
            Serializar(resultado.ProgresoPersistido!),
            Serializar(recargado.Progreso));
        Assert.Equal(
            new[] { "practica-a", "practica-b" },
            resultado.ProgresoPersistido!.Practicas
                .Select(item => item.PracticaId)
                .ToArray());
    }

    [Fact]
    public void GuardarProgreso_DevuelveUnaCopiaDefensiva() {
        using EntornoProgreso entorno = new();
        ProgresoCursoService servicio = new(entorno.Carpeta);
        ProgresoCurso original = CrearCurso(
            CrearPractica(
                "practica-a",
                EstadoPracticaCurso.EnProgreso,
                @"C:\Practicas\A"));

        ResultadoEscrituraProgreso resultado = servicio.GuardarProgreso(original);

        Assert.True(resultado.EsExitosa);
        ProgresoCurso snapshot = Assert.IsType<ProgresoCurso>(
            resultado.ProgresoPersistido);
        Assert.NotSame(original, snapshot);
        Assert.NotSame(original.Practicas, snapshot.Practicas);
        Assert.NotSame(original.Practicas[0], snapshot.Practicas[0]);

        original.Practicas[0].RutaProyecto = @"C:\Mutado\Original";
        snapshot.Practicas[0].RutaProyecto = @"C:\Mutado\Snapshot";
        ResultadoCargaProgreso recargado = servicio.CargarProgreso();

        Assert.Equal(
            @"C:\Practicas\A",
            recargado.Progreso.Practicas.Single().RutaProyecto);
    }

    [Fact]
    public void EscrituraInvalida_NoDevuelveSnapshotPersistido() {
        using EntornoProgreso entorno = new();
        ProgresoCursoService servicio = new(entorno.Carpeta);
        ProgresoCurso invalido = CrearCurso(
            new ProgresoPractica {
                PracticaId = "practica-a",
                Estado = EstadoPracticaCurso.Pendiente
            });

        ResultadoEscrituraProgreso resultado = servicio.GuardarProgreso(invalido);

        Assert.False(resultado.EsExitosa);
        Assert.Null(resultado.ProgresoPersistido);
        Assert.False(File.Exists(servicio.RutaProgreso));
    }

    [Fact]
    public void ActualizarEstado_DerivaLaTransicionDesdeElEstadoPersistidoBajoBloqueo() {
        using EntornoProgreso entorno = new();
        ProgresoCursoService servicio = new(entorno.Carpeta);

        ResultadoEscrituraProgreso realizada = servicio.ActualizarEstado(
            "practica-a",
            EstadoPracticaCurso.Realizada,
            @"C:\Practicas\A");
        ResultadoEscrituraProgreso pendiente = servicio.ActualizarEstado(
            "practica-a",
            EstadoPracticaCurso.Pendiente);
        ResultadoEscrituraProgreso realizadaOtraVez = servicio.ActualizarEstado(
            "practica-a",
            EstadoPracticaCurso.Realizada);

        TransicionProgresoPersistida primera = Assert.IsType<TransicionProgresoPersistida>(
            realizada.TransicionPersistida);
        TransicionProgresoPersistida segunda = Assert.IsType<TransicionProgresoPersistida>(
            pendiente.TransicionPersistida);
        TransicionProgresoPersistida tercera = Assert.IsType<TransicionProgresoPersistida>(
            realizadaOtraVez.TransicionPersistida);
        Assert.True(primera.PracticaCreada);
        Assert.True(primera.VinculoPersistidoAhora);
        Assert.True(primera.RealizadaPersistidaAhora);
        Assert.False(segunda.PracticaCreada);
        Assert.False(segunda.VinculoPersistidoAhora);
        Assert.False(segunda.RealizadaPersistidaAhora);
        Assert.Equal(EstadoPracticaCurso.Pendiente, tercera.ProgresoAnterior!.Estado);
        Assert.Equal(EstadoPracticaCurso.Pendiente, segunda.ProgresoFinal.Estado);
        Assert.True(tercera.RealizadaPersistidaAhora);
        Assert.False(tercera.VinculoPersistidoAhora);
    }

    private static ProgresoCurso CrearCurso(params ProgresoPractica[] practicas) {
        return new ProgresoCurso {
            Practicas = practicas.ToList()
        };
    }

    private static ProgresoPractica CrearPractica(
        string id,
        EstadoPracticaCurso estado,
        string ruta = "") {
        return new ProgresoPractica {
            PracticaId = id,
            Estado = estado,
            RutaProyecto = ruta,
            FechaCreacion = FechaBase,
            FechaActualizacion = FechaBase,
            FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
                ? FechaBase
                : null
        };
    }

    private static string Serializar(ProgresoCurso progreso) {
        return string.Join(
            "|",
            progreso.Practicas.Select(item => string.Join(
                ";",
                item.PracticaId,
                item.Estado,
                item.RutaProyecto,
                item.FechaCreacion,
                item.FechaActualizacion,
                item.FechaFinalizacion)));
    }

    private sealed class EntornoProgreso : IDisposable {
        public EntornoProgreso() {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-Progreso-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
        }

        public string Carpeta { get; }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }
    }
}
