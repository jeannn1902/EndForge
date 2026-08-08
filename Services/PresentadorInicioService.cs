using EndForge.Models;
using System.Globalization;

namespace EndForge.Services;

public sealed class PresentadorInicioService {
    private const string SubtituloInicio =
        "Continúa construyendo tus habilidades en C++.";
    private const string TextoNoDisponible = "No disponible";
    private const string TextoInformacionParcial = "Información parcial";
    private readonly TimeProvider reloj;

    public PresentadorInicioService()
        : this(TimeProvider.System) {
    }

    public PresentadorInicioService(TimeProvider reloj) {
        this.reloj = reloj ?? throw new ArgumentNullException(nameof(reloj));
    }

    public PresentacionInicio Crear(ResumenInicio resumen) {
        ArgumentNullException.ThrowIfNull(resumen);

        return Crear(resumen, CrearNivelNoDisponible());
    }

    public PresentacionInicio Crear(
        ResumenInicio resumen,
        ResumenMotivacion motivacion) {
        ArgumentNullException.ThrowIfNull(resumen);
        ArgumentNullException.ThrowIfNull(motivacion);

        return Crear(resumen, CrearNivel(motivacion));
    }

    private PresentacionInicio Crear(
        ResumenInicio resumen,
        PresentacionNivel nivel) {

        return new PresentacionInicio(
            resumen.Estado,
            new EncabezadoInicioPresentable(
                ObtenerSaludo(),
                SubtituloInicio),
            CrearContinuacion(resumen),
            CrearProgreso(resumen),
            CrearMetricas(resumen),
            CrearRecomendacion(resumen),
            CrearActividades(resumen),
            CrearBandaDatos(resumen)) {
            Nivel = nivel
        };
    }

    private static PresentacionNivel CrearNivel(
        ResumenMotivacion motivacion) {
        if (motivacion.Estado ==
            EstadoDisponibilidadMotivacion.VersionIncompatible) {
            return new PresentacionNivel(
                EstadoNivelInicio.VersionIncompatible,
                TextoNoDisponible,
                string.Empty,
                "Estos datos pertenecen a otra versión de EndForge.",
                null,
                "El nivel y la experiencia no pueden leerse porque " +
                    "motivacion.json pertenece a otra versión de EndForge.");
        }

        if (motivacion.Estado is not (
                EstadoDisponibilidadMotivacion.Disponible or
                EstadoDisponibilidadMotivacion.SinActividad) ||
            motivacion.XpTotal is null ||
            motivacion.Nivel is null) {
            return CrearNivelNoDisponible();
        }

        ResumenNivel nivel = motivacion.Nivel;
        string xpTotal = nivel.XpTotal.ToString(CultureInfo.InvariantCulture);
        string xpRestante = nivel.XpRestante.ToString(
            "0.##",
            CultureInfo.InvariantCulture);
        int porcentaje = Math.Clamp(
            (int)Math.Round(
                nivel.PorcentajeNivel,
                MidpointRounding.AwayFromZero),
            0,
            100);

        return new PresentacionNivel(
            EstadoNivelInicio.Disponible,
            $"Nivel {nivel.NivelActual}",
            $"{xpTotal} XP",
            $"{xpRestante} XP para el siguiente nivel",
            porcentaje,
            $"Nivel {nivel.NivelActual}. {xpTotal} XP totales. " +
                $"Faltan {xpRestante} XP para el siguiente nivel.");
    }

    private static PresentacionNivel CrearNivelNoDisponible() {
        return new PresentacionNivel(
            EstadoNivelInicio.NoDisponible,
            TextoNoDisponible,
            string.Empty,
            "No pudimos cargar tu nivel y XP.",
            null,
            "El nivel y la experiencia no están disponibles temporalmente.");
    }

    public static EstadoCargaInicioPresentable CrearEstadoCargando() {
        return new EstadoCargaInicioPresentable(
            EstadoCargaInicio.Cargando,
            "Actualizando tu progreso...",
            MostrarIndicador: true,
            MostrarReintentar: false);
    }

    public static EstadoCargaInicioPresentable CrearEstadoInactivo() {
        return new EstadoCargaInicioPresentable(
            EstadoCargaInicio.Inactivo,
            string.Empty,
            MostrarIndicador: false,
            MostrarReintentar: false);
    }

    public static EstadoCargaInicioPresentable CrearEstadoErrorRecuperable() {
        return new EstadoCargaInicioPresentable(
            EstadoCargaInicio.ErrorRecuperable,
            "No pudimos actualizar Inicio. Puedes volver a intentarlo.",
            MostrarIndicador: false,
            MostrarReintentar: true);
    }

    private string ObtenerSaludo() {
        int hora = reloj.GetLocalNow().Hour;

        if (hora >= 5 && hora < 12) {
            return "Buenos días";
        }

        if (hora >= 12 && hora < 19) {
            return "Buenas tardes";
        }

        return "Buenas noches";
    }

    private static ContinuacionInicioPresentable CrearContinuacion(
        ResumenInicio resumen) {
        ContinuacionAprendizaje continuacion = resumen.Continuacion;

        return continuacion.Estado switch {
            EstadoContinuacionAprendizaje.PracticaEnProgreso =>
                CrearContinuacionEnProgreso(continuacion),
            EstadoContinuacionAprendizaje.BasadaEnRecomendacion =>
                CrearContinuacionRecomendada(resumen),
            EstadoContinuacionAprendizaje.RutaCompletada =>
                CrearContinuacionRutaCompletada(
                    continuacion.BasadaEnDatosParciales),
            EstadoContinuacionAprendizaje.SinContenidoDisponible =>
                CrearContinuacionNoDisponible(
                    "Contenido no disponible",
                    "No hay prácticas publicadas para continuar.",
                    continuacion.BasadaEnDatosParciales),
            _ =>
                CrearContinuacionNoDisponible(
                    "Continuación no disponible",
                    "No pudimos determinar dónde te quedaste.",
                    basadaEnDatosParciales: true)
        };
    }

    private static ContinuacionInicioPresentable CrearContinuacionEnProgreso(
        ContinuacionAprendizaje continuacion) {
        ReferenciaPracticaAprendizaje? practica = continuacion.Practica;

        if (practica is null) {
            return CrearContinuacionNoDisponible(
                "Continuación no disponible",
                "No pudimos identificar la práctica en progreso.",
                continuacion.BasadaEnDatosParciales);
        }

        string textoRuta = continuacion.EstadoRuta switch {
            EstadoRutaProyectoAprendizaje.Disponible =>
                "Proyecto disponible.",
            EstadoRutaProyectoAprendizaje.NoDisponible =>
                "La carpeta vinculada ya no está disponible.",
            _ =>
                "Aún no hay un proyecto vinculado."
        };
        AccionInicioPresentable accion = new(
            TipoAccionInicio.ContinuarPractica,
            "Continuar",
            $"Continuar {practica.NombrePractica}",
            "Abre el proyecto vinculado si sigue disponible; " +
                "de lo contrario muestra el detalle de la práctica.",
            practica,
            continuacion.RutaProyecto);

        return new ContinuacionInicioPresentable(
            EstadoDatoInicio.Disponible,
            "Continúa donde lo dejaste",
            practica,
            CrearTextoGrado(practica),
            CrearTextoTema(practica),
            CrearTextoPractica(practica),
            "En progreso",
            textoRuta,
            continuacion.BasadaEnDatosParciales,
            accion,
            Array.Empty<AccionInicioPresentable>());
    }

    private static ContinuacionInicioPresentable CrearContinuacionRecomendada(
        ResumenInicio resumen) {
        ReferenciaPracticaAprendizaje? practica = resumen.Continuacion.Practica;

        if (practica is null) {
            return CrearContinuacionNoDisponible(
                "Continuación no disponible",
                "No pudimos identificar la siguiente práctica.",
                resumen.Continuacion.BasadaEnDatosParciales);
        }

        bool usuarioNuevo =
            resumen.Estado == EstadoDisponibilidadDatos.SinActividad;
        string titulo = usuarioNuevo
            ? "Empieza tu ruta de aprendizaje"
            : "Continúa tu ruta de aprendizaje";
        string textoAccion = usuarioNuevo
            ? "Explorar práctica"
            : "Ver práctica";
        AccionInicioPresentable accion = CrearAccionVerPractica(
            practica,
            textoAccion);

        return new ContinuacionInicioPresentable(
            EstadoDatoInicio.Disponible,
            titulo,
            practica,
            CrearTextoGrado(practica),
            CrearTextoTema(practica),
            CrearTextoPractica(practica),
            usuarioNuevo ? "Primera práctica recomendada" : "Siguiente práctica",
            string.Empty,
            resumen.Continuacion.BasadaEnDatosParciales,
            accion,
            Array.Empty<AccionInicioPresentable>());
    }

    private static ContinuacionInicioPresentable CrearContinuacionRutaCompletada(
        bool basadaEnDatosParciales) {
        AccionInicioPresentable verRuta = new(
            TipoAccionInicio.VerRutaAprendizaje,
            "Ver ruta",
            "Ver ruta de aprendizaje",
            "Muestra los grados y temas de la ruta de aprendizaje.");
        AccionInicioPresentable verEstadisticas = new(
            TipoAccionInicio.VerEstadisticas,
            "Ver estadísticas",
            "Ver estadísticas",
            "Muestra las estadísticas de aprendizaje disponibles.");

        return new ContinuacionInicioPresentable(
            EstadoDatoInicio.Disponible,
            "Ruta completada",
            null,
            string.Empty,
            string.Empty,
            "Completaste todas las prácticas publicadas.",
            "Completada",
            string.Empty,
            basadaEnDatosParciales,
            verRuta,
            Array.AsReadOnly(new[] { verEstadisticas }));
    }

    private static ContinuacionInicioPresentable CrearContinuacionNoDisponible(
        string titulo,
        string mensaje,
        bool basadaEnDatosParciales) {
        return new ContinuacionInicioPresentable(
            EstadoDatoInicio.NoDisponible,
            titulo,
            null,
            string.Empty,
            string.Empty,
            mensaje,
            TextoNoDisponible,
            string.Empty,
            basadaEnDatosParciales,
            null,
            Array.Empty<AccionInicioPresentable>());
    }

    private static ProgresoInicioPresentable CrearProgreso(ResumenInicio resumen) {
        ResumenProgresoGlobal progreso = resumen.Progreso;
        EstadoFuenteDatosAprendizaje estadoFuente =
            resumen.FuenteProgreso.Estado;
        DatoInicioPresentable realizadas = CrearConteoProgreso(
            progreso.PracticasRealizadas,
            progreso.TotalPracticasPublicadas,
            estadoFuente,
            "Prácticas realizadas");
        DatoInicioPresentable porcentaje = CrearPorcentajeProgreso(
            progreso.PorcentajeGlobal,
            estadoFuente);
        DatoInicioPresentable temas = CrearConteoProgreso(
            progreso.TemasCompletados,
            progreso.TotalTemas,
            estadoFuente,
            "Temas completados");
        DatoInicioPresentable grados = CrearConteoProgreso(
            progreso.GradosCompletados,
            progreso.TotalGrados,
            estadoFuente,
            "Grados completados");
        int? valorBarra =
            estadoFuente is EstadoFuenteDatosAprendizaje.Disponible or
                EstadoFuenteDatosAprendizaje.SinDatos
                ? progreso.PorcentajeGlobal
                : null;

        return new ProgresoInicioPresentable(
            realizadas,
            porcentaje,
            valorBarra,
            temas,
            grados);
    }

    private static DatoInicioPresentable CrearConteoProgreso(
        int? valor,
        int total,
        EstadoFuenteDatosAprendizaje estadoFuente,
        string descripcion) {
        return estadoFuente switch {
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible =>
                CrearDatoNoDisponible(descripcion),
            EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada =>
                valor is > 0
                    ? new DatoInicioPresentable(
                        EstadoDatoInicio.Parcial,
                        $"{valor.Value} confirmadas de {total}",
                        valor,
                        $"{descripcion}: información recuperada parcialmente.")
                    : CrearDatoParcial(descripcion),
            EstadoFuenteDatosAprendizaje.SinDatos =>
                new DatoInicioPresentable(
                    EstadoDatoInicio.SinDatos,
                    $"0 de {total}",
                    0,
                    $"{descripcion}: todavía no hay actividad registrada."),
            _ =>
                valor.HasValue
                    ? new DatoInicioPresentable(
                        EstadoDatoInicio.Disponible,
                        $"{valor.Value} de {total}",
                        valor,
                        $"{descripcion}: {valor.Value} de {total}.")
                    : CrearDatoNoDisponible(descripcion)
        };
    }

    private static DatoInicioPresentable CrearPorcentajeProgreso(
        int? porcentaje,
        EstadoFuenteDatosAprendizaje estadoFuente) {
        const string descripcion = "Progreso general";

        return estadoFuente switch {
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible =>
                CrearDatoNoDisponible(descripcion),
            EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada =>
                porcentaje.HasValue
                    ? new DatoInicioPresentable(
                        EstadoDatoInicio.Parcial,
                        $"{porcentaje.Value} % confirmado",
                        porcentaje,
                        "Porcentaje calculado con información recuperada parcialmente.")
                    : CrearDatoParcial(descripcion),
            EstadoFuenteDatosAprendizaje.SinDatos =>
                new DatoInicioPresentable(
                    EstadoDatoInicio.SinDatos,
                    "0 %",
                    0,
                    "Progreso general: todavía no hay actividad registrada."),
            _ =>
                porcentaje.HasValue
                    ? new DatoInicioPresentable(
                        EstadoDatoInicio.Disponible,
                        $"{porcentaje.Value} %",
                        porcentaje,
                        $"Progreso general: {porcentaje.Value} por ciento.")
                    : CrearDatoNoDisponible(descripcion)
        };
    }

    private static IReadOnlyList<MetricaInicioPresentable> CrearMetricas(
        ResumenInicio resumen) {
        List<MetricaInicioPresentable> metricas = new(4) {
            new(
                TipoMetricaInicio.EvaluacionesAprobadas,
                "Evaluaciones aprobadas",
                CrearMetricaEvaluacionesAprobadas(resumen)),
            new(
                TipoMetricaInicio.PromedioMejoresCalificaciones,
                "Promedio de mejores calificaciones",
                CrearMetricaCalificacion(
                    resumen,
                    resumen.Evaluaciones.PromedioMejoresCalificaciones,
                    "Promedio de mejores calificaciones")),
            new(
                TipoMetricaInicio.MejorCalificacion,
                "Mejor calificación",
                CrearMetricaCalificacion(
                    resumen,
                    resumen.Evaluaciones.MejorCalificacionGlobal,
                    "Mejor calificación")),
            new(
                TipoMetricaInicio.PracticasEnProgreso,
                "Prácticas en progreso",
                CrearMetricaPracticasEnProgreso(resumen))
        };

        return Array.AsReadOnly(metricas.ToArray());
    }

    private static DatoInicioPresentable CrearMetricaEvaluacionesAprobadas(
        ResumenInicio resumen) {
        const string descripcion =
            "Prácticas con al menos una evaluación aprobatoria";
        EstadoFuenteDatosAprendizaje estadoFuente =
            resumen.FuenteHistorial.Estado;

        if (estadoFuente ==
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible) {
            return CrearDatoNoDisponible(descripcion);
        }

        if (estadoFuente ==
            EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada) {
            int? valor =
                resumen.Evaluaciones.PracticasConEvaluacionAprobada;
            return valor is > 0
                ? new DatoInicioPresentable(
                    EstadoDatoInicio.Parcial,
                    $"{valor.Value} confirmadas",
                    valor,
                    $"{descripcion}: información recuperada parcialmente.")
                : CrearDatoParcial(descripcion);
        }

        if (NoHayEvaluaciones(resumen)) {
            return CrearDatoSinEvaluaciones(descripcion);
        }

        int? aprobadas =
            resumen.Evaluaciones.PracticasConEvaluacionAprobada;
        return aprobadas.HasValue
            ? new DatoInicioPresentable(
                EstadoDatoInicio.Disponible,
                aprobadas.Value.ToString(CultureInfo.InvariantCulture),
                aprobadas,
                $"{descripcion}: {aprobadas.Value}.")
            : CrearDatoNoDisponible(descripcion);
    }

    private static DatoInicioPresentable CrearMetricaCalificacion(
        ResumenInicio resumen,
        int? valor,
        string descripcion) {
        EstadoFuenteDatosAprendizaje estadoFuente =
            resumen.FuenteHistorial.Estado;

        if (estadoFuente ==
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible) {
            return CrearDatoNoDisponible(descripcion);
        }

        if (estadoFuente ==
            EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada) {
            return valor.HasValue
                ? new DatoInicioPresentable(
                    EstadoDatoInicio.Parcial,
                    $"{valor.Value}/100",
                    valor,
                    $"{descripcion}: información recuperada parcialmente.")
                : CrearDatoParcial(descripcion);
        }

        if (NoHayEvaluaciones(resumen)) {
            return CrearDatoSinEvaluaciones(descripcion);
        }

        return valor.HasValue
            ? new DatoInicioPresentable(
                EstadoDatoInicio.Disponible,
                $"{valor.Value}/100",
                valor,
                $"{descripcion}: {valor.Value} de 100.")
            : new DatoInicioPresentable(
                EstadoDatoInicio.SinDatos,
                "Sin calificaciones",
                null,
                $"{descripcion}: no hay calificaciones registradas.");
    }

    private static DatoInicioPresentable CrearMetricaPracticasEnProgreso(
        ResumenInicio resumen) {
        const string descripcion = "Prácticas en progreso";
        EstadoFuenteDatosAprendizaje estadoFuente =
            resumen.FuenteProgreso.Estado;
        int? valor = resumen.Progreso.PracticasEnProgreso;

        return estadoFuente switch {
            EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible =>
                CrearDatoNoDisponible(descripcion),
            EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada =>
                valor is > 0
                    ? new DatoInicioPresentable(
                        EstadoDatoInicio.Parcial,
                        $"{valor.Value} confirmadas",
                        valor,
                        $"{descripcion}: información recuperada parcialmente.")
                    : CrearDatoParcial(descripcion),
            EstadoFuenteDatosAprendizaje.SinDatos =>
                new DatoInicioPresentable(
                    EstadoDatoInicio.SinDatos,
                    "0",
                    0,
                    "Prácticas en progreso: todavía no hay actividad registrada."),
            _ =>
                valor.HasValue
                    ? new DatoInicioPresentable(
                        EstadoDatoInicio.Disponible,
                        valor.Value.ToString(CultureInfo.InvariantCulture),
                        valor,
                        $"{descripcion}: {valor.Value}.")
                    : CrearDatoNoDisponible(descripcion)
        };
    }

    private static bool NoHayEvaluaciones(ResumenInicio resumen) {
        return resumen.FuenteHistorial.Estado ==
                EstadoFuenteDatosAprendizaje.SinDatos ||
            resumen.Evaluaciones.TotalEvaluacionesRealizadas == 0 &&
            resumen.Evaluaciones.PracticasEvaluadas == 0;
    }

    private static DatoInicioPresentable CrearDatoSinEvaluaciones(
        string descripcion) {
        return new DatoInicioPresentable(
            EstadoDatoInicio.SinDatos,
            "Sin evaluaciones",
            null,
            $"{descripcion}: todavía no hay evaluaciones registradas.");
    }

    private static DatoInicioPresentable CrearDatoNoDisponible(
        string descripcion) {
        return new DatoInicioPresentable(
            EstadoDatoInicio.NoDisponible,
            TextoNoDisponible,
            null,
            $"{descripcion}: no disponible temporalmente.");
    }

    private static DatoInicioPresentable CrearDatoParcial(string descripcion) {
        return new DatoInicioPresentable(
            EstadoDatoInicio.Parcial,
            TextoInformacionParcial,
            null,
            $"{descripcion}: información recuperada parcialmente.");
    }

    private static RecomendacionInicioPresentable? CrearRecomendacion(
        ResumenInicio resumen) {
        RecomendacionAprendizaje recomendacion = resumen.Recomendacion;

        if (recomendacion.Estado != EstadoRecomendacionAprendizaje.Disponible ||
            recomendacion.Practica is null ||
            recomendacion.Motivo is null) {
            return null;
        }

        ReferenciaPracticaAprendizaje practica = recomendacion.Practica;
        return new RecomendacionInicioPresentable(
            "SIGUIENTE PRÁCTICA",
            practica,
            CrearTextoGrado(practica),
            CrearTextoTema(practica),
            CrearTextoPractica(practica),
            string.IsNullOrWhiteSpace(practica.Dificultad)
                ? "No especificada"
                : practica.Dificultad,
            string.IsNullOrWhiteSpace(practica.DuracionEstimada)
                ? "Sin duración estimada"
                : practica.DuracionEstimada,
            ObtenerRazonRecomendacion(resumen, recomendacion),
            recomendacion.BasadaEnDatosParciales,
            CrearAccionVerSiguientePractica(practica));
    }

    private static string ObtenerRazonRecomendacion(
        ResumenInicio resumen,
        RecomendacionAprendizaje recomendacion) {
        ReferenciaPracticaAprendizaje? practicaActual =
            resumen.Continuacion.Estado ==
                EstadoContinuacionAprendizaje.PracticaEnProgreso
                ? resumen.Continuacion.Practica
                : null;
        ReferenciaPracticaAprendizaje? practicaSiguiente =
            recomendacion.Practica;

        if (practicaActual is not null &&
            practicaSiguiente is not null &&
            !string.Equals(
                practicaActual.PracticaId,
                practicaSiguiente.PracticaId,
                StringComparison.OrdinalIgnoreCase)) {
            return $"Termina “{practicaActual.NombrePractica}” y " +
                "continúa con esta práctica.";
        }

        return ObtenerRazonRecomendacion(recomendacion.Motivo!.Value);
    }

    private static string ObtenerRazonRecomendacion(
        MotivoRecomendacionAprendizaje motivo) {
        return motivo switch {
            MotivoRecomendacionAprendizaje.PrimeraPractica =>
                "Empieza tu ruta de aprendizaje.",
            MotivoRecomendacionAprendizaje.TemaAvanzadoConProgreso =>
                "Siguiente práctica de tu tema actual.",
            MotivoRecomendacionAprendizaje.SiguienteTema =>
                "Avanza al siguiente tema.",
            MotivoRecomendacionAprendizaje.SiguienteGrado =>
                "Avanza al siguiente grado.",
            MotivoRecomendacionAprendizaje.PendienteAnterior =>
                "Retoma una práctica pendiente de tu ruta.",
            MotivoRecomendacionAprendizaje.RetomarPracticaEnProgreso =>
                "Continúa una práctica que ya comenzaste.",
            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(motivo),
                    motivo,
                    "El motivo de recomendación no tiene una presentación definida.")
        };
    }

    private IReadOnlyList<ActividadInicioPresentable> CrearActividades(
        ResumenInicio resumen) {
        if (resumen.UltimaActividad is null) {
            return Array.Empty<ActividadInicioPresentable>();
        }

        ActividadAprendizaje actividad = resumen.UltimaActividad;
        string texto = actividad.Fuente ==
            FuenteActividadAprendizaje.HistorialEvaluaciones
                ? $"Evaluación registrada en {actividad.Practica.NombreTema}."
                : $"Actividad registrada en {actividad.Practica.NombreTema}.";
        ActividadInicioPresentable presentable = new(
            actividad.Fecha,
            FormatearFechaActividad(actividad.Fecha),
            texto,
            actividad.Practica,
            actividad.Fuente,
            actividad.EsAproximada);

        return Array.AsReadOnly(new[] { presentable });
    }

    private string FormatearFechaActividad(DateTimeOffset fecha) {
        DateTime fechaLocal =
            TimeZoneInfo.ConvertTime(fecha, reloj.LocalTimeZone).Date;
        DateTime hoy = reloj.GetLocalNow().Date;
        int diferenciaDias = (hoy - fechaLocal).Days;

        return diferenciaDias switch {
            0 => "Hoy",
            1 => "Ayer",
            > 1 and <= 30 => $"Hace {diferenciaDias} días",
            _ => fechaLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        };
    }

    private static BandaDatosInicioPresentable? CrearBandaDatos(
        ResumenInicio resumen) {
        if (resumen.Estado is not (
            EstadoDisponibilidadDatos.DatosParcialmenteRecuperados or
            EstadoDisponibilidadDatos.DatosTemporalmenteNoDisponibles)) {
            return null;
        }

        List<string> detalles = new();
        AgregarDetalleFuente(
            detalles,
            resumen.FuenteProgreso,
            "progreso");
        AgregarDetalleFuente(
            detalles,
            resumen.FuenteHistorial,
            "historial de evaluaciones");

        if (detalles.Count == 0) {
            detalles.Add(
                "Mostramos únicamente la información que pudo confirmarse.");
        }

        bool noDisponible =
            resumen.Estado ==
            EstadoDisponibilidadDatos.DatosTemporalmenteNoDisponibles;
        return new BandaDatosInicioPresentable(
            resumen.Estado,
            noDisponible
                ? "Datos temporalmente no disponibles"
                : "Algunos datos no pudieron cargarse",
            string.Join(" ", detalles),
            new AccionInicioPresentable(
                TipoAccionInicio.Reintentar,
                "Reintentar",
                "Reintentar carga de Inicio",
                "Vuelve a cargar el progreso y el historial de evaluaciones."));
    }

    private static void AgregarDetalleFuente(
        ICollection<string> detalles,
        EstadoFuenteAprendizaje fuente,
        string nombreFuente) {
        switch (fuente.Estado) {
            case EstadoFuenteDatosAprendizaje.TemporalmenteNoDisponible:
                detalles.Add(
                    $"El {nombreFuente} no está disponible temporalmente.");
                break;
            case EstadoFuenteDatosAprendizaje.ParcialmenteRecuperada:
                detalles.Add(
                    $"Parte del {nombreFuente} no pudo recuperarse.");
                break;
        }

        if (fuente.RegistrosHuerfanos > 0) {
            detalles.Add(
                $"Se omitieron registros del {nombreFuente} " +
                "que ya no corresponden al contenido publicado.");
        }
    }

    private static AccionInicioPresentable CrearAccionVerPractica(
        ReferenciaPracticaAprendizaje practica,
        string texto) {
        return new AccionInicioPresentable(
            TipoAccionInicio.VerPractica,
            texto,
            $"{texto}: {practica.NombrePractica}",
            $"Muestra el detalle de {practica.NombrePractica}.",
            practica);
    }

    private static AccionInicioPresentable CrearAccionVerSiguientePractica(
        ReferenciaPracticaAprendizaje practica) {
        return new AccionInicioPresentable(
            TipoAccionInicio.VerPractica,
            "Ver siguiente práctica",
            $"Ver siguiente práctica: {practica.NombrePractica}",
            "Muestra el detalle de la siguiente práctica recomendada: " +
                $"{practica.NombrePractica}.",
            practica);
    }

    private static string CrearTextoGrado(
        ReferenciaPracticaAprendizaje practica) {
        return $"Grado {practica.NumeroGrado} · {practica.NombreGrado}";
    }

    private static string CrearTextoTema(
        ReferenciaPracticaAprendizaje practica) {
        return $"Tema {practica.NumeroTema} · {practica.NombreTema}";
    }

    private static string CrearTextoPractica(
        ReferenciaPracticaAprendizaje practica) {
        return $"{practica.NumeroPractica:D2} · {practica.NombrePractica}";
    }
}
