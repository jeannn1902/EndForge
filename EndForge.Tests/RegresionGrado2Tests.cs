using EndForge.Models;

namespace EndForge.Tests;

public sealed class RegresionGrado2Tests {
    public static IEnumerable<object[]> EvaluacionesGrado2() {
        return CanonicalEvaluationFactory.Definiciones
            .Where(definicion =>
                definicion.PracticaId.StartsWith(
                    "grado2-",
                    StringComparison.OrdinalIgnoreCase))
            .Select(definicion => new object[] { definicion.PracticaId });
    }

    [Theory]
    [MemberData(nameof(EvaluacionesGrado2))]
    public void TodosLosCasosCanonicosApruebanConPuntajeCompleto(
        string practicaId) {
        DefinicionEvaluacionPractica definicion =
            CanonicalEvaluationFactory.ObtenerDefinicion(practicaId);

        Assert.Equal(5, definicion.CasosPrueba.Count);

        foreach (CasoPrueba caso in definicion.CasosPrueba) {
            var evaluado = CanonicalEvaluationFactory.Evaluar(caso);

            Assert.True(
                evaluado.Resultado.Aprobado,
                $"{practicaId}/{caso.Id}: {evaluado.Resultado.Mensaje}");
            Assert.Equal(12, evaluado.Resultado.PuntosObtenidos);
            Assert.True(evaluado.Comparacion?.EsCorrecta);
            Assert.Empty(
                evaluado.Comparacion?.ContradiccionesDetectadas ??
                Array.Empty<string>());
        }
    }
}
