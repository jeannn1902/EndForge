using EndForge.Models;

namespace EndForge.Tests;

public sealed class RegresionGrado1Tests {
    public static IEnumerable<object[]> EvaluacionesGrado1() {
        return CanonicalEvaluationFactory.Definiciones
            .Where(definicion =>
                !definicion.PracticaId.StartsWith(
                    "grado2-",
                    StringComparison.OrdinalIgnoreCase))
            .Select(definicion => new object[] { definicion.PracticaId });
    }

    [Theory]
    [MemberData(nameof(EvaluacionesGrado1))]
    public void TodosLosCasosCanonicosConservanSuResultado(
        string practicaId) {
        DefinicionEvaluacionPractica definicion =
            CanonicalEvaluationFactory.ObtenerDefinicion(practicaId);

        Assert.NotEmpty(definicion.CasosPrueba);

        foreach (CasoPrueba caso in definicion.CasosPrueba) {
            var evaluado = CanonicalEvaluationFactory.Evaluar(caso);

            Assert.True(
                evaluado.Resultado.Aprobado,
                $"{practicaId}/{caso.Id}: {evaluado.Resultado.Mensaje}");
            Assert.Equal(caso.Puntos, evaluado.Resultado.PuntosObtenidos);
            Assert.True(evaluado.Comparacion?.EsCorrecta);
            Assert.Empty(
                evaluado.Comparacion?.ContradiccionesDetectadas ??
                Array.Empty<string>());
        }
    }
}
