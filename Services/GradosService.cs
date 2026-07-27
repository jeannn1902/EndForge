using EndForge.Models;

namespace EndForge.Services;

public sealed class GradosService {
    public const string GradoFundamentosId = "grado-1-fundamentos-cpp";
    public const string GradoJuniorId = "grado-2-cpp-junior";
    public const int MetaCurricularPracticasGradoFundamentos =
        CursoService.TotalPracticasPlaneadasGradoFundamentos;
    public const int MetaCurricularPracticasGradoJunior =
        CursoService.TotalPracticasPlaneadasGradoJunior;

    private readonly IReadOnlyDictionary<string, CursoService> catalogos;

    public GradosService(CursoService cursoGradoFundamentos) {
        ArgumentNullException.ThrowIfNull(cursoGradoFundamentos);

        if (!cursoGradoFundamentos.GradoId.Equals(
            GradoFundamentosId,
            StringComparison.OrdinalIgnoreCase)) {
            throw new ArgumentException(
                "El catálogo precargado debe corresponder a Grado 1.",
                nameof(cursoGradoFundamentos));
        }

        CursoService cursoGradoJunior = CursoService.CrearCatalogoGradoJunior();
        Dictionary<string, CursoService> catalogosPorId = new(
            StringComparer.OrdinalIgnoreCase) {
                [cursoGradoFundamentos.GradoId] = cursoGradoFundamentos,
                [cursoGradoJunior.GradoId] = cursoGradoJunior
            };
        ValidarIdentificadoresPracticas(catalogosPorId.Values);
        catalogos = catalogosPorId;
    }

    public IReadOnlyList<GradoCurso> CargarGrados(ProgresoCurso? progreso) {
        GradoCurso[] gradosDisponibles = catalogos.Values
            .OrderBy(curso => curso.NumeroGrado)
            .Select(curso => CrearGradoDisponible(curso, progreso))
            .ToArray();

        return Array.AsReadOnly(gradosDisponibles
            .Concat(new[] {
                CrearProximamente(3, "C++ Intermedio", "Profundiza en diseño, memoria, estructuras y herramientas del lenguaje."),
                CrearProximamente(4, "Desarrollo avanzado", "Explora técnicas avanzadas para proyectos de mayor escala."),
                CrearProximamente(5, "Especializaciones", "Elige rutas especializadas y aplica C++ en contextos profesionales.")
            })
            .ToArray());
    }

    public CursoService? ObtenerCurso(string gradoId) {
        if (string.IsNullOrWhiteSpace(gradoId)) {
            return null;
        }

        return catalogos.TryGetValue(gradoId, out CursoService? curso)
            ? curso
            : null;
    }

    public GradoCurso? ObtenerGrado(string gradoId, ProgresoCurso? progreso) {
        CursoService? curso = ObtenerCurso(gradoId);

        if (curso is not null) {
            return CrearGradoDisponible(curso, progreso);
        }

        return CargarGrados(progreso).FirstOrDefault(grado =>
            grado.Id.Equals(gradoId, StringComparison.OrdinalIgnoreCase));
    }

    private static GradoCurso CrearGradoDisponible(
        CursoService cursoService,
        ProgresoCurso? progreso) {
        IReadOnlyList<TemaCurso> temas = cursoService.CargarTemas();
        HashSet<string> practicasDisponibles = temas
            .Where(tema => !tema.EsProximamente)
            .SelectMany(tema => tema.Practicas)
            .Select(practica => practica.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int realizadas = progreso?.Practicas
            .Where(item =>
                item.Estado == EstadoPracticaCurso.Realizada &&
                practicasDisponibles.Contains(item.PracticaId))
            .Select(item => item.PracticaId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() ?? 0;
        bool existeProgreso = progreso?.Practicas.Any(item =>
            practicasDisponibles.Contains(item.PracticaId) &&
            (item.Estado != EstadoPracticaCurso.Pendiente ||
             !string.IsNullOrWhiteSpace(item.RutaProyecto))) == true;
        int disponibles = practicasDisponibles.Count;
        int porcentaje = disponibles == 0
            ? 0
            : Math.Clamp((int)Math.Round(realizadas * 100D / disponibles), 0, 100);
        EstadoGradoCurso estado = realizadas >= disponibles && disponibles > 0
            ? disponibles < cursoService.TotalPracticasPlaneadas
                ? EstadoGradoCurso.ContenidoDisponibleCompletado
                : EstadoGradoCurso.Completado
            : existeProgreso
                ? EstadoGradoCurso.EnProgreso
                : EstadoGradoCurso.Disponible;

        return new GradoCurso {
            Id = cursoService.GradoId,
            Numero = cursoService.NumeroGrado,
            Nombre = cursoService.NombreGrado,
            Descripcion = cursoService.DescripcionGrado,
            Estado = estado,
            Temas = temas,
            Porcentaje = porcentaje,
            CantidadPracticasDisponibles = disponibles,
            CantidadPracticasCompletadas = realizadas,
            CantidadPracticasPlaneadas = cursoService.TotalPracticasPlaneadas,
            EsContenidoDisponible = true
        };
    }

    private static GradoCurso CrearProximamente(
        int numero,
        string nombre,
        string descripcion) {
        return new GradoCurso {
            Id = $"grado-{numero}",
            Numero = numero,
            Nombre = nombre,
            Descripcion = descripcion,
            Estado = EstadoGradoCurso.Proximamente,
            EsContenidoDisponible = false
        };
    }

    private static void ValidarIdentificadoresPracticas(
        IEnumerable<CursoService> cursos) {
        HashSet<string> identificadores = new(StringComparer.OrdinalIgnoreCase);

        foreach (PracticaCurso practica in cursos
            .SelectMany(curso => curso.CargarTemas())
            .SelectMany(tema => tema.Practicas)) {
            if (string.IsNullOrWhiteSpace(practica.Id) ||
                !identificadores.Add(practica.Id)) {
                throw new InvalidOperationException(
                    $"El identificador de práctica '{practica.Id}' no es válido o está duplicado.");
            }
        }
    }
}
