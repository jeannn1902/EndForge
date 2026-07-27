using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

internal static class CanonicalEvaluationFactory {
    private static readonly CatalogoEvaluacionesService Catalogo = new();
    private static readonly EvaluacionPracticaService Evaluador = new();

    public static IReadOnlyList<DefinicionEvaluacionPractica> Definiciones =>
        Catalogo.CargarDefiniciones();

    public static DefinicionEvaluacionPractica ObtenerDefinicion(
        string practicaId) {
        return Catalogo.ObtenerDefinicion(practicaId)
            ?? throw new InvalidOperationException(
                $"No existe la evaluación '{practicaId}'.");
    }

    public static EvaluacionPracticaService.CasoEvaluado Evaluar(
        CasoPrueba caso) {
        string salida = ObtenerSalidaCanonica(caso);
        ResultadoArchivoPrueba[] archivos = caso.ArchivosEsperados
            .Select(esperado => new ResultadoArchivoPrueba {
                RutaRelativa = esperado.RutaRelativa,
                Estado = EstadoArchivoPrueba.Disponible,
                ContenidoObtenido = esperado.ModoComparacion ==
                    ModoComparacionArchivoPrueba.TextoExacto
                        ? esperado.ContenidoEsperado
                        : caso.SalidaEsperada
            })
            .ToArray();

        return Evaluador.EvaluarCaso(
            caso,
            new ResultadoEjecucionCasoPruebaCpp {
                Ejecucion = new ResultadoEjecucionPruebaCpp {
                    Estado = EstadoEjecucionPruebaCpp.Exitosa,
                    SalidaEstandar = salida,
                    CodigoSalida = 0
                },
                Archivos = archivos
            });
    }

    private static string ObtenerSalidaCanonica(CasoPrueba caso) {
        if (caso.SalidaExactaEsperada is not null) {
            return caso.SalidaExactaEsperada.ValorEsperado;
        }

        return TieneReglasDeConsola(caso)
            ? caso.SalidaEsperada
            : string.Empty;
    }

    private static bool TieneReglasDeConsola(CasoPrueba caso) {
        return caso.TokensObligatorios.Count > 0 ||
            caso.GruposTokensAlternativos.Count > 0 ||
            caso.ValoresNumericosEsperados.Count > 0 ||
            caso.ValoresBooleanosEsperados.Count > 0 ||
            caso.ValoresTextualesEsperados.Count > 0 ||
            caso.SecuenciasEsperadas.Count > 0 ||
            caso.SecuenciasCompuestasEsperadas.Count > 0 ||
            caso.ColeccionesEsperadas.Count > 0 ||
            caso.CadenasEsperadas.Count > 0 ||
            caso.TablasEsperadas.Count > 0 ||
            caso.MatricesEsperadas.Count > 0 ||
            caso.BloquesRegistroEsperados.Count > 0;
    }
}
