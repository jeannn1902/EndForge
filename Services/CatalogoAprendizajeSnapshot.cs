using EndForge.Models;
using System.Collections.ObjectModel;

namespace EndForge.Services;

internal sealed class CatalogoAprendizajeSnapshot {
    private CatalogoAprendizajeSnapshot(
        IReadOnlyList<GradoCurso> grados,
        IReadOnlyList<TemaCatalogoAprendizaje> temas,
        IReadOnlyList<PracticaCatalogoAprendizaje> practicas,
        IReadOnlyDictionary<string, PracticaCatalogoAprendizaje> practicasPorId) {
        Grados = grados;
        Temas = temas;
        Practicas = practicas;
        PracticasPorId = practicasPorId;
    }

    public IReadOnlyList<GradoCurso> Grados { get; }

    public IReadOnlyList<TemaCatalogoAprendizaje> Temas { get; }

    public IReadOnlyList<PracticaCatalogoAprendizaje> Practicas { get; }

    public IReadOnlyDictionary<string, PracticaCatalogoAprendizaje> PracticasPorId {
        get;
    }

    public static CatalogoAprendizajeSnapshot Crear(
        IEnumerable<GradoCurso> grados,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(grados);

        GradoCurso[] gradosPublicados = grados
            .Where(grado => grado.EsContenidoDisponible)
            .OrderBy(grado => grado.Numero)
            .ThenBy(grado => grado.Id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(grado => grado.Id, StringComparer.Ordinal)
            .ToArray();
        HashSet<string> idsGrados = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> idsTemas = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, PracticaCatalogoAprendizaje> practicasPorId =
            new(StringComparer.OrdinalIgnoreCase);
        List<TemaCatalogoAprendizaje> temas = new();
        List<PracticaCatalogoAprendizaje> practicas = new();

        foreach (GradoCurso grado in gradosPublicados) {
            cancellationToken.ThrowIfCancellationRequested();
            ValidarIdentificadorUnico(grado.Id, idsGrados, "grado");

            TemaCurso[] temasPublicados = grado.Temas
                .Where(tema => !tema.EsProximamente)
                .OrderBy(tema => tema.Numero)
                .ThenBy(tema => tema.Id, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tema => tema.Id, StringComparer.Ordinal)
                .ToArray();

            foreach (TemaCurso tema in temasPublicados) {
                cancellationToken.ThrowIfCancellationRequested();
                ValidarIdentificadorUnico(tema.Id, idsTemas, "tema");
                int indiceTema = temas.Count;
                PracticaCurso[] practicasPublicadas = tema.Practicas
                    .OrderBy(practica => practica.Numero)
                    .ThenBy(practica => practica.Id, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(practica => practica.Id, StringComparer.Ordinal)
                    .ToArray();

                foreach (PracticaCurso practica in practicasPublicadas) {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(practica.Id) ||
                        practicasPorId.ContainsKey(practica.Id)) {
                        throw new InvalidOperationException(
                            $"El identificador de práctica '{practica.Id}' no es válido o está duplicado.");
                    }

                    PracticaCatalogoAprendizaje entrada = new(
                        grado,
                        tema,
                        practica,
                        practicas.Count,
                        indiceTema);
                    practicas.Add(entrada);
                    practicasPorId.Add(practica.Id, entrada);
                }

                temas.Add(new TemaCatalogoAprendizaje(
                    grado,
                    tema,
                    indiceTema,
                    Array.AsReadOnly(practicas
                        .Where(item => item.IndiceTema == indiceTema)
                        .ToArray())));
            }
        }

        return new CatalogoAprendizajeSnapshot(
            Array.AsReadOnly(gradosPublicados),
            Array.AsReadOnly(temas.ToArray()),
            Array.AsReadOnly(practicas.ToArray()),
            new ReadOnlyDictionary<string, PracticaCatalogoAprendizaje>(
                practicasPorId));
    }

    private static void ValidarIdentificadorUnico(
        string identificador,
        ISet<string> identificadores,
        string tipo) {
        if (string.IsNullOrWhiteSpace(identificador) ||
            !identificadores.Add(identificador)) {
            throw new InvalidOperationException(
                $"El identificador de {tipo} '{identificador}' no es válido o está duplicado.");
        }
    }
}

internal sealed record TemaCatalogoAprendizaje(
    GradoCurso Grado,
    TemaCurso Tema,
    int Indice,
    IReadOnlyList<PracticaCatalogoAprendizaje> Practicas);

internal sealed record PracticaCatalogoAprendizaje(
    GradoCurso Grado,
    TemaCurso Tema,
    PracticaCurso Practica,
    int Indice,
    int IndiceTema) {
    public ReferenciaPracticaAprendizaje CrearReferencia() {
        return new ReferenciaPracticaAprendizaje(
            Grado.Id,
            Grado.Numero,
            Grado.Nombre,
            Tema.Id,
            Tema.Numero,
            Tema.Nombre,
            Practica.Id,
            Practica.Numero,
            Practica.Nombre);
    }
}
