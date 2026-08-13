using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class RecomendacionAprendizajeServiceTests {
    private static readonly DateTimeOffset FechaBase =
        new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Resolver_UsuarioNuevo_RecomiendaPrimeraPracticaGlobal() {
        RecomendacionAprendizajeService servicio = CrearServicio();

        ResultadoContinuidadAprendizaje resultado = servicio.Resolver(
            CrearGrados(),
            new ProgresoCurso(),
            new HistorialEvaluaciones());

        Assert.Equal(
            EstadoRecomendacionAprendizaje.Disponible,
            resultado.Recomendacion.Estado);
        Assert.Equal(
            MotivoRecomendacionAprendizaje.PrimeraPractica,
            resultado.Recomendacion.Motivo);
        Assert.Equal(
            "variables-datos-personales",
            resultado.Recomendacion.Practica?.PracticaId);
        Assert.Equal(
            EstadoContinuacionAprendizaje.BasadaEnRecomendacion,
            resultado.Continuacion.Estado);
        Assert.Equal(
            resultado.Recomendacion.Practica,
            resultado.Continuacion.Practica);
    }

    [Fact]
    public void Resolver_TemaAvanzadoConProgreso_EligePrimeraPendienteDelTema() {
        ProgresoCurso progreso = CrearProgreso(
            CrearRegistro(
                "condicionales-mayor-de-edad",
                EstadoPracticaCurso.Realizada,
                FechaBase));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            CrearGrados(),
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso,
            resultado.Recomendacion.Motivo);
        Assert.Equal(
            "condicionales-clasificar-numero",
            resultado.Recomendacion.Practica?.PracticaId);
    }

    [Fact]
    public void Resolver_TemaActualCompletado_ContinuaEnSiguienteTema() {
        IReadOnlyList<GradoCurso> grados = CrearGrados();
        string[] idsVariables = ObtenerTema(grados, "variables")
            .Practicas
            .Select(practica => practica.Id)
            .ToArray();
        ProgresoCurso progreso = CrearProgreso(idsVariables.Select((id, indice) =>
            CrearRegistro(
                id,
                EstadoPracticaCurso.Realizada,
                FechaBase.AddMinutes(indice))));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            grados,
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            MotivoRecomendacionAprendizaje.SiguienteTema,
            resultado.Recomendacion.Motivo);
        Assert.Equal(
            "condicionales-mayor-de-edad",
            resultado.Recomendacion.Practica?.PracticaId);
    }

    [Fact]
    public void Resolver_GradoUnoCompletado_ContinuaEnGradoDos() {
        IReadOnlyList<GradoCurso> grados = CrearGrados();
        string[] idsGradoUno = grados
            .Single(grado => grado.Id == GradosService.GradoFundamentosId)
            .Temas
            .SelectMany(tema => tema.Practicas)
            .Select(practica => practica.Id)
            .ToArray();
        ProgresoCurso progreso = CrearProgreso(idsGradoUno.Select((id, indice) =>
            CrearRegistro(
                id,
                EstadoPracticaCurso.Realizada,
                FechaBase.AddMinutes(indice))));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            grados,
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            MotivoRecomendacionAprendizaje.SiguienteGrado,
            resultado.Recomendacion.Motivo);
        Assert.Equal(
            "grado2-arreglos-capturar-mostrar",
            resultado.Recomendacion.Practica?.PracticaId);
        Assert.Equal(2, resultado.Recomendacion.Practica?.NumeroGrado);
    }

    [Fact]
    public void Resolver_ProgresoEnGradoDos_PriorizaTemaMasAvanzadoIniciado() {
        HistorialEvaluaciones historial = CrearHistorial(
            CrearHistorialPractica(
                "grado2-matrices-capturar-mostrar",
                FechaBase));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            CrearGrados(),
            new ProgresoCurso(),
            historial);

        Assert.Equal(
            MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso,
            resultado.Recomendacion.Motivo);
        Assert.Equal(
            "grado2-matrices-capturar-mostrar",
            resultado.Recomendacion.Practica?.PracticaId);
        Assert.Equal(2, resultado.Recomendacion.Practica?.NumeroGrado);
        Assert.Equal(3, resultado.Recomendacion.Practica?.NumeroTema);
    }

    [Fact]
    public void Resolver_MismosDatosEnOrdenDistinto_ConservaResultadoDeterminista() {
        ProgresoPractica primera = CrearRegistro(
            "variables-datos-personales",
            EstadoPracticaCurso.EnProgreso,
            FechaBase);
        ProgresoPractica segunda = CrearRegistro(
            "variables-ticket-compra",
            EstadoPracticaCurso.EnProgreso,
            FechaBase);
        RecomendacionAprendizajeService servicio = CrearServicio();
        IReadOnlyList<GradoCurso> grados = CrearGrados();

        ResultadoContinuidadAprendizaje resultadoUno = servicio.Resolver(
            grados,
            CrearProgreso(primera, segunda),
            new HistorialEvaluaciones());
        ResultadoContinuidadAprendizaje resultadoDos = servicio.Resolver(
            grados,
            CrearProgreso(segunda, primera),
            new HistorialEvaluaciones());

        Assert.Equal(
            "variables-datos-personales",
            resultadoUno.Continuacion.Practica?.PracticaId);
        Assert.Equal(
            resultadoUno.Continuacion,
            resultadoDos.Continuacion);
        Assert.Equal(
            resultadoUno.Recomendacion,
            resultadoDos.Recomendacion);
    }

    [Fact]
    public void Resolver_VariasEnProgreso_ContinuaLaDeActividadMasReciente() {
        ProgresoCurso progreso = CrearProgreso(
            CrearRegistro(
                "variables-datos-personales",
                EstadoPracticaCurso.EnProgreso,
                FechaBase),
            CrearRegistro(
                "condicionales-mayor-de-edad",
                EstadoPracticaCurso.EnProgreso,
                FechaBase.AddDays(1)));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            CrearGrados(),
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            EstadoContinuacionAprendizaje.PracticaEnProgreso,
            resultado.Continuacion.Estado);
        Assert.Equal(
            "condicionales-mayor-de-edad",
            resultado.Continuacion.Practica?.PracticaId);
        Assert.Equal(FechaBase.AddDays(1), resultado.Continuacion.FechaActividad);
    }

    [Fact]
    public void Resolver_EvaluacionMasReciente_SuperaFechaDelProgreso() {
        ProgresoCurso progreso = CrearProgreso(
            CrearRegistro(
                "variables-datos-personales",
                EstadoPracticaCurso.EnProgreso,
                FechaBase.AddDays(2)),
            CrearRegistro(
                "variables-ticket-compra",
                EstadoPracticaCurso.EnProgreso,
                FechaBase));
        HistorialEvaluaciones historial = CrearHistorial(
            CrearHistorialPractica(
                "variables-ticket-compra",
                FechaBase.AddDays(3)));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            CrearGrados(),
            progreso,
            historial);

        Assert.Equal(
            "variables-ticket-compra",
            resultado.Continuacion.Practica?.PracticaId);
        Assert.Equal(FechaBase.AddDays(3), resultado.Continuacion.FechaActividad);
    }

    [Fact]
    public void Resolver_RutaExistente_PrefierePracticaAbribleAunqueSeaMasAntigua() {
        const string rutaDisponible = @"C:\EndForgeTests\disponible";
        const string rutaAusente = @"C:\EndForgeTests\ausente";
        ProgresoCurso progreso = CrearProgreso(
            CrearRegistro(
                "variables-datos-personales",
                EstadoPracticaCurso.EnProgreso,
                FechaBase,
                rutaDisponible),
            CrearRegistro(
                "variables-ticket-compra",
                EstadoPracticaCurso.EnProgreso,
                FechaBase.AddDays(2),
                rutaAusente));

        ResultadoContinuidadAprendizaje resultado =
            CrearServicio(rutaDisponible).Resolver(
                CrearGrados(),
                progreso,
                new HistorialEvaluaciones());

        Assert.Equal(
            "variables-datos-personales",
            resultado.Continuacion.Practica?.PracticaId);
        Assert.Equal(rutaDisponible, resultado.Continuacion.RutaProyecto);
        Assert.Equal(
            EstadoRutaProyectoAprendizaje.Disponible,
            resultado.Continuacion.EstadoRuta);
    }

    [Fact]
    public void Resolver_RutaAusente_ConservaPracticaRutaYProgreso() {
        const string rutaAusente = @"C:\EndForgeTests\ausente";
        ProgresoPractica registro = CrearRegistro(
            "variables-datos-personales",
            EstadoPracticaCurso.EnProgreso,
            FechaBase,
            rutaAusente);
        ProgresoCurso progreso = CrearProgreso(registro);

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            CrearGrados(),
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            "variables-datos-personales",
            resultado.Continuacion.Practica?.PracticaId);
        Assert.Equal(rutaAusente, resultado.Continuacion.RutaProyecto);
        Assert.Equal(
            EstadoRutaProyectoAprendizaje.NoDisponible,
            resultado.Continuacion.EstadoRuta);
        Assert.Equal(EstadoPracticaCurso.EnProgreso, registro.Estado);
        Assert.Equal(rutaAusente, registro.RutaProyecto);
    }

    [Fact]
    public void Resolver_SinPendientesPosteriores_RecuperaPendienteAnterior() {
        IReadOnlyList<GradoCurso> grados = CrearGrados();
        TemaCurso temaArchivos = ObtenerTema(grados, CursoService.TemaArchivosGradoJuniorId);
        ProgresoCurso progreso = CrearProgreso(temaArchivos.Practicas.Select((practica, indice) =>
            CrearRegistro(
                practica.Id,
                EstadoPracticaCurso.Realizada,
                FechaBase.AddMinutes(indice))));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            grados,
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            MotivoRecomendacionAprendizaje.PendienteAnterior,
            resultado.Recomendacion.Motivo);
        Assert.Equal(
            "grado2-estructuras-datos-estudiante",
            resultado.Recomendacion.Practica?.PracticaId);
    }

    [Fact]
    public void Resolver_TodasRealizadas_DevuelveRutaCompleta() {
        IReadOnlyList<GradoCurso> grados = CrearGrados();
        ProgresoCurso progreso = CrearProgreso(ObtenerPracticas(grados)
            .Select((practica, indice) => CrearRegistro(
                practica.Id,
                EstadoPracticaCurso.Realizada,
                FechaBase.AddMinutes(indice))));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            grados,
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            EstadoRecomendacionAprendizaje.RutaCompletada,
            resultado.Recomendacion.Estado);
        Assert.Null(resultado.Recomendacion.Practica);
        Assert.Equal(
            EstadoContinuacionAprendizaje.RutaCompletada,
            resultado.Continuacion.Estado);
        Assert.Null(resultado.Continuacion.Practica);
    }

    [Fact]
    public void Resolver_SoloQuedanPracticasEnProgreso_NoDeclaraRutaCompleta() {
        IReadOnlyList<GradoCurso> grados = CrearGrados();
        PracticaCurso ultima = ObtenerPracticas(grados).Last();
        ProgresoCurso progreso = CrearProgreso(ObtenerPracticas(grados)
            .Select((practica, indice) => CrearRegistro(
                practica.Id,
                practica.Id == ultima.Id
                    ? EstadoPracticaCurso.EnProgreso
                    : EstadoPracticaCurso.Realizada,
                FechaBase.AddMinutes(indice))));

        ResultadoContinuidadAprendizaje resultado = CrearServicio().Resolver(
            grados,
            progreso,
            new HistorialEvaluaciones());

        Assert.Equal(
            EstadoRecomendacionAprendizaje.Disponible,
            resultado.Recomendacion.Estado);
        Assert.Equal(
            MotivoRecomendacionAprendizaje.RetomarPracticaEnProgreso,
            resultado.Recomendacion.Motivo);
        Assert.Equal(ultima.Id, resultado.Recomendacion.Practica?.PracticaId);
        Assert.Equal(
            EstadoContinuacionAprendizaje.PracticaEnProgreso,
            resultado.Continuacion.Estado);
        Assert.Equal(ultima.Id, resultado.Continuacion.Practica?.PracticaId);
    }

    [Fact]
    public void Resolver_RegistrosHuerfanos_NoAlteranContinuacionNiRecomendacion() {
        ProgresoCurso progreso = CrearProgreso(
            CrearRegistro(
                "practica-huerfana",
                EstadoPracticaCurso.EnProgreso,
                FechaBase.AddYears(10),
                @"C:\EndForgeTests\huerfana"));
        HistorialEvaluaciones historial = CrearHistorial(
            CrearHistorialPractica(
                "evaluacion-huerfana",
                FechaBase.AddYears(20)));

        ResultadoContinuidadAprendizaje resultado = CrearServicio(
            @"C:\EndForgeTests\huerfana").Resolver(
                CrearGrados(),
                progreso,
                historial);

        Assert.Equal(
            EstadoContinuacionAprendizaje.BasadaEnRecomendacion,
            resultado.Continuacion.Estado);
        Assert.Equal(
            "variables-datos-personales",
            resultado.Continuacion.Practica?.PracticaId);
        Assert.Equal(
            "variables-datos-personales",
            resultado.Recomendacion.Practica?.PracticaId);
    }

    private static RecomendacionAprendizajeService CrearServicio(
        params string[] rutasExistentes) {
        HashSet<string> rutas = rutasExistentes.ToHashSet(
            StringComparer.OrdinalIgnoreCase);
        return new RecomendacionAprendizajeService(ruta => rutas.Contains(ruta));
    }

    private static IReadOnlyList<GradoCurso> CrearGrados() {
        CursoService cursoGradoUno = new();
        return new GradosService(cursoGradoUno).CargarGrados(null);
    }

    private static TemaCurso ObtenerTema(
        IEnumerable<GradoCurso> grados,
        string temaId) {
        return grados
            .SelectMany(grado => grado.Temas)
            .Single(tema => tema.Id.Equals(
                temaId,
                StringComparison.OrdinalIgnoreCase));
    }

    private static PracticaCurso[] ObtenerPracticas(
        IEnumerable<GradoCurso> grados) {
        return grados
            .Where(grado => grado.EsContenidoDisponible)
            .OrderBy(grado => grado.Numero)
            .SelectMany(grado => grado.Temas
                .Where(tema => !tema.EsProximamente)
                .OrderBy(tema => tema.Numero))
            .SelectMany(tema => tema.Practicas.OrderBy(practica => practica.Numero))
            .ToArray();
    }

    private static ProgresoCurso CrearProgreso(
        params ProgresoPractica[] registros) {
        return CrearProgreso((IEnumerable<ProgresoPractica>)registros);
    }

    private static ProgresoCurso CrearProgreso(
        IEnumerable<ProgresoPractica> registros) {
        return new ProgresoCurso {
            Practicas = registros.ToList()
        };
    }

    private static ProgresoPractica CrearRegistro(
        string practicaId,
        EstadoPracticaCurso estado,
        DateTimeOffset fecha,
        string rutaProyecto = "") {
        return new ProgresoPractica {
            PracticaId = practicaId,
            Estado = estado,
            RutaProyecto = rutaProyecto,
            FechaCreacion = fecha,
            FechaActualizacion = fecha,
            FechaFinalizacion = estado == EstadoPracticaCurso.Realizada
                ? fecha
                : null
        };
    }

    private static HistorialEvaluaciones CrearHistorial(
        params HistorialPractica[] practicas) {
        return new HistorialEvaluaciones {
            Practicas = practicas
        };
    }

    private static HistorialPractica CrearHistorialPractica(
        string practicaId,
        DateTimeOffset fecha) {
        return new HistorialPractica {
            PracticaId = practicaId,
            TotalIntentos = 1,
            MejorCalificacion = 80,
            UltimaCalificacion = 80,
            FechaUltimoIntento = fecha
        };
    }
}
