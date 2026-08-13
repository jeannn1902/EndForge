using EndForge.Models;
using EndForge.Services;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EndForge.Tests;

public sealed class MotivacionValidacionV2Tests {
    private static readonly DateTimeOffset FechaBase =
        new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions OpcionesJson =
        new(JsonSerializerOptions.Default) {
        WriteIndented = true
    };

    [Theory]
    [InlineData("")]
    [InlineData("identificador-sin-prefijo")]
    [InlineData("logro:control\u0001")]
    public void LogroIdInvalido_SeRechazaSinSobrescribir(string logroId) {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        ObtenerPrimerLogro(documento)["LogroId"] = logroId;

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void LogroIdMayorA256_SeRechazaSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        ObtenerPrimerLogro(documento)["LogroId"] =
            "logro:" + new string('a', 251);

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void LogroDuplicadoSinDistinguirMayusculas_SeRechaza() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        JsonArray logros = documento["LogrosDesbloqueados"]!.AsArray();
        JsonObject duplicado = logros[0]!.DeepClone().AsObject();
        duplicado["LogroId"] = duplicado["LogroId"]!
            .GetValue<string>()
            .ToUpperInvariant();
        logros.Add(duplicado);

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void MasDe512Logros_SeRechazanSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        string fecha = ObtenerPrimerLogro(documento)["FechaReconocimientoUtc"]!
            .GetValue<string>();
        JsonArray logros = new();

        for (int indice = 0; indice <= 512; indice++) {
            logros.Add(new JsonObject {
                ["LogroId"] = $"logro:prueba:{indice:D3}",
                ["FechaReconocimientoUtc"] = fecha,
                ["EsImportado"] = true
            });
        }

        documento["LogrosDesbloqueados"] = logros;
        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Theory]
    [InlineData("0001-01-01T00:00:00+00:00")]
    [InlineData("2026-08-01T06:00:00-06:00")]
    [InlineData("2026-08-02T12:00:00+00:00")]
    public void FechaReconocimientoInvalida_SeRechazaSinSobrescribir(
        string fecha) {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        ObtenerPrimerLogro(documento)["FechaReconocimientoUtc"] = fecha;

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void DiaDefault_SeRechazaSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        documento["DiasActividadAcademica"] = new JsonArray("0001-01-01");

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void DiasDuplicados_SeRechazanSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        documento["DiasActividadAcademica"] = new JsonArray(
            "2026-07-31",
            "2026-07-31");

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void DiasDesordenados_SeRechazanSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        documento["DiasActividadAcademica"] = new JsonArray(
            "2026-07-31",
            "2026-07-30");

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void MasDe36600Dias_SeRechazanSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        JsonArray dias = new();
        DateOnly inicial = new(1900, 1, 1);

        for (int indice = 0; indice <= 36_600; indice++) {
            dias.Add(inicial.AddDays(indice).ToString("yyyy-MM-dd"));
        }

        documento["DiasActividadAcademica"] = dias;
        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void DiaFuturo_SeRechazaSinSobrescribir() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        documento["DiasActividadAcademica"] = new JsonArray("2026-08-02");

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Theory]
    [InlineData("LogrosDesbloqueados")]
    [InlineData("DiasActividadAcademica")]
    public void PropiedadRaizVersion2Ausente_SeRechaza(string propiedad) {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        Assert.True(documento.Remove(propiedad));

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Theory]
    [InlineData("MigracionVersion2Completada")]
    [InlineData("FechaMigracionVersion2Utc")]
    [InlineData("LogrosHistoricosProcesados")]
    [InlineData("ActividadHistoricaProcesada")]
    [InlineData("HistoriaActividadParcial")]
    public void MetadatoVersion2Ausente_SeRechaza(string propiedad) {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        JsonObject metadatos = documento["MetadatosMigracion"]!.AsObject();
        Assert.True(metadatos.Remove(propiedad));

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void CampoObligatorioDeLogroAusente_SeRechaza() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        Assert.True(ObtenerPrimerLogro(documento).Remove("EsImportado"));

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void CampoObligatorioDeConcesionAusente_SeRechaza() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        JsonObject concesion = documento["ConcesionesXP"]!
            .AsArray()[0]!
            .AsObject();
        Assert.True(concesion.Remove("EsImportada"));

        entorno.AssertDocumentoRechazado(Serializar(documento));
    }

    [Fact]
    public void VersionFutura_SeReportaYNoSeSobrescribe() {
        using EntornoValidacion entorno = new();
        JsonObject documento = entorno.CargarDocumento();
        documento["Version"] = 99;

        entorno.AssertDocumentoRechazado(
            Serializar(documento),
            EstadoProcesamientoMotivacion.VersionIncompatible);
    }

    [Fact]
    public void PropiedadJsonDuplicada_SeRechazaSinSobrescribir() {
        using EntornoValidacion entorno = new();
        string valido = File.ReadAllText(entorno.Servicio.RutaMotivacion);
        int inicioObjeto = valido.IndexOf('{');
        Assert.True(inicioObjeto >= 0);
        string duplicado = valido.Insert(
            inicioObjeto + 1,
            "\n  \"version\": 2,");

        entorno.AssertDocumentoRechazado(duplicado);
    }

    private static JsonObject ObtenerPrimerLogro(JsonObject documento) {
        return documento["LogrosDesbloqueados"]!
            .AsArray()[0]!
            .AsObject();
    }

    private static string Serializar(JsonObject documento) {
        return documento.ToJsonString(OpcionesJson);
    }

    private sealed class EntornoValidacion : IDisposable {
        public EntornoValidacion() {
            Carpeta = Path.Combine(
                Path.GetTempPath(),
                $"EndForge-Motivacion-V2-Validacion-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Carpeta);
            Servicio = CrearServicio();
            ResultadoProcesamientoMotivacion creacion =
                Servicio.ReconciliarEstadoActual();
            Assert.Equal(EstadoProcesamientoMotivacion.Aplicada, creacion.Estado);
            Assert.NotEmpty(creacion.Resumen.LogrosDesbloqueados);
            Assert.NotNull(creacion.Resumen.Racha.UltimoDiaEstudio);
        }

        public string Carpeta { get; }

        public MotivacionService Servicio { get; }

        public JsonObject CargarDocumento() {
            return JsonNode
                .Parse(File.ReadAllText(Servicio.RutaMotivacion))!
                .AsObject();
        }

        public void AssertDocumentoRechazado(
            string contenido,
            EstadoProcesamientoMotivacion estadoEsperado =
                EstadoProcesamientoMotivacion.ErrorRecuperable) {
            File.WriteAllText(Servicio.RutaMotivacion, contenido);

            ResultadoProcesamientoMotivacion resultado =
                Servicio.ReconciliarEstadoActual();

            Assert.Equal(estadoEsperado, resultado.Estado);
            Assert.Equal(
                estadoEsperado ==
                    EstadoProcesamientoMotivacion.VersionIncompatible
                    ? EstadoDisponibilidadMotivacion.VersionIncompatible
                    : EstadoDisponibilidadMotivacion.NoDisponible,
                resultado.Resumen.Estado);
            Assert.NotNull(resultado.Error);
            Assert.Equal(contenido, File.ReadAllText(Servicio.RutaMotivacion));
        }

        public void Dispose() {
            try {
                Directory.Delete(Carpeta, recursive: true);
            } catch (Exception) {
                // La limpieza no debe ocultar el resultado de la prueba.
            }
        }

        private MotivacionService CrearServicio() {
            return new MotivacionService(
                Carpeta,
                new TimeProviderFijo(FechaBase),
                CrearCatalogo,
                CrearProgreso,
                CrearHistorial);
        }

        private static IReadOnlyList<GradoCurso> CrearCatalogo() {
            const string temaId = "tema-validacion-v2";
            PracticaCurso practica = new() {
                Id = "practica-validacion-v2",
                TemaId = temaId,
                Numero = 1,
                Nombre = "Practica"
            };
            TemaCurso tema = new() {
                Id = temaId,
                Numero = 1,
                Nombre = "Tema",
                Practicas = new[] { practica }
            };
            return new[] {
                new GradoCurso {
                    Id = GradosService.GradoFundamentosId,
                    Numero = 1,
                    Nombre = "Grado 1",
                    EsContenidoDisponible = true,
                    Temas = new[] { tema }
                }
            };
        }

        private static ResultadoCargaProgreso CrearProgreso() {
            ProgresoPractica practica = new() {
                PracticaId = "practica-validacion-v2",
                Estado = EstadoPracticaCurso.Realizada,
                RutaProyecto = @"C:\Practicas\validacion-v2",
                FechaCreacion = FechaBase.AddDays(-1),
                FechaActualizacion = FechaBase.AddDays(-1),
                FechaFinalizacion = FechaBase.AddDays(-1)
            };
            return new ResultadoCargaProgreso {
                Estado = EstadoCargaProgreso.Exitosa,
                Progreso = new ProgresoCurso {
                    Practicas = new List<ProgresoPractica> { practica }
                }
            };
        }

        private static ResultadoCargaHistorialEvaluaciones CrearHistorial() {
            return new ResultadoCargaHistorialEvaluaciones {
                Estado = EstadoCargaHistorialEvaluaciones.Exitosa,
                Historial = new HistorialEvaluaciones()
            };
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
}
