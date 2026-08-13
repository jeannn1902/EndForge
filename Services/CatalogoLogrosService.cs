namespace EndForge.Services;

public enum CriterioLogro {
    PracticasVinculadasDistintas,
    PracticasRealizadasDistintas,
    PracticasAprobadasDistintas,
    PracticasPerfectasDistintas,
    TemasCompletadosDistintos,
    GradosCompletadosDistintos,
    GradoEspecificoCompletado
}

public sealed record DefinicionLogro(
    string Id,
    CriterioLogro Criterio,
    int Umbral,
    string? GradoId = null);

public sealed class CatalogoLogrosService {
    public const string PrimeraPracticaVinculadaId =
        "logro:practica:primera-vinculada";
    public const string PrimeraPracticaRealizadaId =
        "logro:practica:primera-realizada";
    public const string PrimeraEvaluacionAprobadaId =
        "logro:evaluacion:primera-aprobada";
    public const string PrimeraEvaluacionPerfectaId =
        "logro:evaluacion:primera-perfecta";
    public const string PrimerTemaCompletadoId =
        "logro:tema:primero-completado";
    public const string PrimerGradoCompletadoId =
        "logro:grado:primero-completado";
    public const string CincoPracticasRealizadasId =
        "logro:practicas:realizadas:5";
    public const string DiezPracticasRealizadasId =
        "logro:practicas:realizadas:10";
    public const string VeinticincoPracticasRealizadasId =
        "logro:practicas:realizadas:25";
    public const string GradoFundamentosCompletoId =
        "logro:grado:" + GradosService.GradoFundamentosId + ":completo";
    public const string GradoJuniorCompletoId =
        "logro:grado:" + GradosService.GradoJuniorId + ":completo";
    public const string CincoPracticasAprobadasId =
        "logro:evaluaciones:aprobadas:5";
    public const string DiezPracticasAprobadasId =
        "logro:evaluaciones:aprobadas:10";
    public const string CincoPracticasPerfectasId =
        "logro:evaluaciones:perfectas:5";

    private readonly IReadOnlyList<DefinicionLogro> definiciones;
    private readonly IReadOnlyDictionary<string, DefinicionLogro> definicionesPorId;

    public CatalogoLogrosService() {
        definiciones = CrearDefiniciones();
        ValidarDefiniciones(definiciones);
        definicionesPorId = definiciones.ToDictionary(
            definicion => definicion.Id,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<DefinicionLogro> CargarDefiniciones() {
        return definiciones;
    }

    public DefinicionLogro? ObtenerDefinicion(string logroId) {
        if (string.IsNullOrWhiteSpace(logroId)) {
            return null;
        }

        return definicionesPorId.TryGetValue(
            logroId,
            out DefinicionLogro? definicion)
            ? definicion
            : null;
    }

    public bool EsLogroConocido(string logroId) {
        return ObtenerDefinicion(logroId) is not null;
    }

    private static IReadOnlyList<DefinicionLogro> CrearDefiniciones() {
        return Array.AsReadOnly(new[] {
            new DefinicionLogro(
                PrimeraPracticaVinculadaId,
                CriterioLogro.PracticasVinculadasDistintas,
                1),
            new DefinicionLogro(
                PrimeraPracticaRealizadaId,
                CriterioLogro.PracticasRealizadasDistintas,
                1),
            new DefinicionLogro(
                PrimeraEvaluacionAprobadaId,
                CriterioLogro.PracticasAprobadasDistintas,
                1),
            new DefinicionLogro(
                PrimeraEvaluacionPerfectaId,
                CriterioLogro.PracticasPerfectasDistintas,
                1),
            new DefinicionLogro(
                PrimerTemaCompletadoId,
                CriterioLogro.TemasCompletadosDistintos,
                1),
            new DefinicionLogro(
                PrimerGradoCompletadoId,
                CriterioLogro.GradosCompletadosDistintos,
                1),
            new DefinicionLogro(
                CincoPracticasRealizadasId,
                CriterioLogro.PracticasRealizadasDistintas,
                5),
            new DefinicionLogro(
                DiezPracticasRealizadasId,
                CriterioLogro.PracticasRealizadasDistintas,
                10),
            new DefinicionLogro(
                VeinticincoPracticasRealizadasId,
                CriterioLogro.PracticasRealizadasDistintas,
                25),
            new DefinicionLogro(
                GradoFundamentosCompletoId,
                CriterioLogro.GradoEspecificoCompletado,
                1,
                GradosService.GradoFundamentosId),
            new DefinicionLogro(
                GradoJuniorCompletoId,
                CriterioLogro.GradoEspecificoCompletado,
                1,
                GradosService.GradoJuniorId),
            new DefinicionLogro(
                CincoPracticasAprobadasId,
                CriterioLogro.PracticasAprobadasDistintas,
                5),
            new DefinicionLogro(
                DiezPracticasAprobadasId,
                CriterioLogro.PracticasAprobadasDistintas,
                10),
            new DefinicionLogro(
                CincoPracticasPerfectasId,
                CriterioLogro.PracticasPerfectasDistintas,
                5)
        });
    }

    private static void ValidarDefiniciones(
        IReadOnlyList<DefinicionLogro> definiciones) {
        HashSet<string> identificadores = new(StringComparer.OrdinalIgnoreCase);

        foreach (DefinicionLogro definicion in definiciones) {
            if (string.IsNullOrWhiteSpace(definicion.Id) ||
                !identificadores.Add(definicion.Id)) {
                throw new InvalidOperationException(
                    $"El identificador de logro '{definicion.Id}' no es valido o esta duplicado.");
            }

            if (definicion.Umbral <= 0) {
                throw new InvalidOperationException(
                    $"El umbral del logro '{definicion.Id}' debe ser mayor que cero.");
            }

            bool requiereGrado =
                definicion.Criterio == CriterioLogro.GradoEspecificoCompletado;
            if (requiereGrado != !string.IsNullOrWhiteSpace(definicion.GradoId)) {
                throw new InvalidOperationException(
                    $"El grado asociado al logro '{definicion.Id}' no es valido.");
            }

            if (requiereGrado &&
                !definicion.GradoId!.Equals(
                    GradosService.GradoFundamentosId,
                    StringComparison.OrdinalIgnoreCase) &&
                !definicion.GradoId.Equals(
                    GradosService.GradoJuniorId,
                    StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException(
                    $"El grado asociado al logro '{definicion.Id}' no existe.");
            }
        }
    }
}
