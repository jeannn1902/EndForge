using EndForge.Models;
using EndForge.Services;
using System.Text.Json;

namespace EndForge.Tests;

public sealed class HistorialEvaluacionesServiceTests {
    private const string PracticaId = "practica-fecha-intento";
    private static readonly DateTimeOffset FechaIntento =
        new(2026, 8, 8, 23, 58, 17, TimeSpan.FromHours(-6));

    [Fact]
    public void GuardarIntento_ExponeLaFechaExactaDelIntentoPublicado() {
        using EntornoHistorial entorno = new();
        HistorialEvaluacionesService servicio = new(entorno.Carpeta);
        IntentoPractica intento = CrearIntento(entorno.RutaProyecto);

        ResultadoEscrituraHistorialEvaluaciones resultado =
            servicio.GuardarIntento(intento);

        Assert.True(resultado.EsExitosa);
        TransicionEvaluacionPersistida transicion =
            Assert.IsType<TransicionEvaluacionPersistida>(
                resultado.TransicionPersistida);
        Assert.True(transicion.IntentoPublicado);
        Assert.Equal(intento.Id, transicion.IntentoId);
        Assert.Equal(FechaIntento, transicion.FechaIntento);
        Assert.Equal(FechaIntento.Offset, transicion.FechaIntento.Offset);
        Assert.Equal(
            FechaIntento,
            Assert.Single(resultado.HistorialActualizado!.Intentos).Fecha);
    }

    [Fact]
    public void GuardarIntento_NoAgregaLaTransicionAlJsonPersistido() {
        using EntornoHistorial entorno = new();
        HistorialEvaluacionesService servicio = new(entorno.Carpeta);

        ResultadoEscrituraHistorialEvaluaciones resultado =
            servicio.GuardarIntento(CrearIntento(entorno.RutaProyecto));

        Assert.True(resultado.EsExitosa);
        using JsonDocument documento = JsonDocument.Parse(
            File.ReadAllText(servicio.RutaEvaluaciones));
        JsonElement practica = Assert.Single(
            documento.RootElement.GetProperty("Practicas").EnumerateArray());
        Assert.False(practica.TryGetProperty("TransicionPersistida", out _));
        Assert.False(practica.TryGetProperty("FechaIntento", out _));
        JsonElement intento = Assert.Single(
            practica.GetProperty("Intentos").EnumerateArray());
        Assert.True(intento.TryGetProperty("Fecha", out _));
        Assert.False(intento.TryGetProperty("FechaIntento", out _));
    }

    private static IntentoPractica CrearIntento(string rutaProyecto) {
        return new IntentoPractica {
            Id = "intento-fecha-exacta",
            PracticaId = PracticaId,
            Fecha = FechaIntento,
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

    private sealed class EntornoHistorial : IDisposable {
        public EntornoHistorial() {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-Historial-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
            RutaProyecto = Path.Combine(Carpeta, "Practica");
        }

        public string Carpeta { get; }

        public string RutaProyecto { get; }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }
    }
}
