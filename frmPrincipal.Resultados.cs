using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private readonly HistorialEvaluacionesService historialEvaluacionesService = new();
    private Panel panelResultadoEvaluacionVista = null!;
    private Panel panelHistorialEvaluacionesVista = null!;
    private PanelDesplazableSinBarras desplazamientoResultadoEvaluacion = null!;
    private PanelDesplazableSinBarras desplazamientoHistorialEvaluaciones = null!;
    private FlowLayoutPanel contenidoResultadoEvaluacion = null!;
    private FlowLayoutPanel contenidoHistorialEvaluaciones = null!;
    private IntentoPractica? intentoResultadoActual;
    private HistorialPractica? resumenHistorialActual;
    private bool resultadoMostradoDesdeHistorial;
    private bool historialAbiertoDesdeResultado;
    private ResultadoCargaHistorialEvaluaciones? cargaHistorialEnMemoria;
    private string practicaCargaHistorial = string.Empty;
    private int versionSolicitudHistorial;
    private bool actualizandoHistorial;
    private bool cargandoHistorial;
    private int ultimoAnchoResultado = -1;
    private int ultimoAnchoHistorial = -1;

    private void ConstruirVistasResultadoEHistorial() {
        desplazamientoResultadoEvaluacion = CrearPanelDesplazableCurso(
            "desplazamientoResultadoEvaluacion",
            "contenidoResultadoEvaluacion",
            Point.Empty,
            panelResultadoEvaluacionVista.ClientSize,
            new Padding(22, 18, 22, 28));
        desplazamientoResultadoEvaluacion.Dock = DockStyle.Fill;
        desplazamientoResultadoEvaluacion.BackColor = Color.FromArgb(18, 14, 27);
        desplazamientoResultadoEvaluacion.Padding = Padding.Empty;
        desplazamientoResultadoEvaluacion.MostrarBordeFoco = false;
        contenidoResultadoEvaluacion = desplazamientoResultadoEvaluacion.Contenido;
        panelResultadoEvaluacionVista.Controls.Add(desplazamientoResultadoEvaluacion);

        desplazamientoHistorialEvaluaciones = CrearPanelDesplazableCurso(
            "desplazamientoHistorialEvaluaciones",
            "contenidoHistorialEvaluaciones",
            Point.Empty,
            panelHistorialEvaluacionesVista.ClientSize,
            new Padding(22, 18, 22, 28));
        desplazamientoHistorialEvaluaciones.Dock = DockStyle.Fill;
        desplazamientoHistorialEvaluaciones.BackColor = Color.FromArgb(18, 14, 27);
        desplazamientoHistorialEvaluaciones.Padding = Padding.Empty;
        desplazamientoHistorialEvaluaciones.MostrarBordeFoco = false;
        contenidoHistorialEvaluaciones = desplazamientoHistorialEvaluaciones.Contenido;
        panelHistorialEvaluacionesVista.Controls.Add(desplazamientoHistorialEvaluaciones);
    }

    private void MostrarResultadoEvaluacion(
        IntentoPractica intento,
        HistorialPractica? historial,
        bool desdeHistorial) {
        AsegurarVistasEvaluacionInicializadas();
        intentoResultadoActual = intento;
        resumenHistorialActual = historial;
        resultadoMostradoDesdeHistorial = desdeHistorial;
        ReconstruirResultadoEvaluacion(volverAlInicio: true);
        MostrarModoCursoInmersivo();
        MostrarSubvistaCurso(
            panelResultadoEvaluacionVista,
            VistaRutaAprendizaje.Resultado);
    }

    private void ReconstruirResultadoEvaluacion(bool volverAlInicio) {
        if (intentoResultadoActual is null) {
            return;
        }

        IntentoPractica intento = intentoResultadoActual;
        int ancho = Math.Max(
            1,
            contenidoResultadoEvaluacion.ClientSize.Width -
            contenidoResultadoEvaluacion.Padding.Horizontal);
        ultimoAnchoResultado = ancho;
        contenidoResultadoEvaluacion.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoResultadoEvaluacion);
            AgregarLabelFluido(
                contenidoResultadoEvaluacion,
                $"Ruta de aprendizaje  ›  {ObtenerEtiquetaGradoActual()}  ›  " +
                $"{temaCursoSeleccionado?.Nombre ?? "Tema"}  ›  " +
                $"{practicaCursoSeleccionada?.Nombre ?? "Práctica"}  ›  " +
                "Evaluación  ›  Resultado",
                ancho,
                TamanoFuenteCurso.NumeroCabecera,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            AgregarLabelFluido(
                contenidoResultadoEvaluacion,
                "Resultado de evaluación",
                ancho,
                TamanoFuenteCurso.TituloPrincipal,
                FontStyle.Bold,
                Color.White);

            string estado = ObtenerEstadoCalificacion(intento.Calificacion);
            Panel resumen = CrearTarjetaCurso(
                Point.Empty,
                new Size(ancho, EscalarDiseno(142)),
                16);
            resumen.Margin = new Padding(0, EscalarDiseno(5), 0, EscalarDiseno(14));
            Label calificacion = CrearLabelCurso(
                $"{intento.Calificacion}",
                new Point(EscalarDiseno(20), EscalarDiseno(12)),
                new Size(EscalarDiseno(126), EscalarDiseno(68)),
                38F,
                FontStyle.Bold,
                ColorMoradoClaroCurso,
                ContentAlignment.MiddleCenter);
            Label puntos = CrearLabelCurso(
                $"de {intento.PuntosMaximos} puntos",
                new Point(EscalarDiseno(20), EscalarDiseno(80)),
                new Size(EscalarDiseno(126), EscalarDiseno(24)),
                TamanoFuenteCurso.MetadatoTarjetaTema,
                FontStyle.Regular,
                ColorTextoSecundarioCurso,
                ContentAlignment.MiddleCenter);
            int xDetalle = EscalarDiseno(166);
            int anchoDetalle = Math.Max(1, ancho - xDetalle - EscalarDiseno(20));
            Label lblEstado = CrearLabelCurso(
                estado,
                new Point(xDetalle, EscalarDiseno(18)),
                new Size(anchoDetalle, EscalarDiseno(28)),
                TamanoFuenteCurso.TituloTarjetaTema,
                FontStyle.Bold,
                ObtenerColorCalificacion(intento.Calificacion));
            int? numeroIntento = ObtenerNumeroIntento(
                intento,
                resumenHistorialActual);
            string resumenPersistencia;

            if (numeroIntento is int numeroConfirmado) {
                string mejorConfirmado = resumenHistorialActual?.MejorCalificacion?.ToString() ??
                    "no disponible";
                resumenPersistencia =
                    $"Intento {numeroConfirmado}  ·  Mejor calificación {mejorConfirmado}";
            } else if (resumenHistorialActual?.MejorCalificacion is int mejorGuardado) {
                resumenPersistencia =
                    $"Número de intento no disponible  ·  Mejor calificación guardada {mejorGuardado}";
            } else {
                resumenPersistencia =
                    "Número de intento no disponible  ·  Historial no disponible";
            }

            Label detalle = CrearLabelCurso(
                $"{intento.Fecha.LocalDateTime:g}  ·  {resumenPersistencia}\n" +
                $"Compilación: {(intento.Compilo ? "correcta" : "pendiente")}  ·  " +
                $"Pruebas: {intento.PruebasSuperadas} de {intento.PruebasTotales}",
                new Point(xDetalle, EscalarDiseno(50)),
                new Size(anchoDetalle, EscalarDiseno(60)),
                TamanoFuenteCurso.TextoDetalle,
                FontStyle.Regular,
                ColorTextoSecundarioCurso);
            resumen.Controls.Add(calificacion);
            resumen.Controls.Add(puntos);
            resumen.Controls.Add(lblEstado);
            resumen.Controls.Add(detalle);
            contenidoResultadoEvaluacion.Controls.Add(resumen);

            AgregarSeccionDetalle(
                contenidoResultadoEvaluacion,
                "Desglose de puntuación",
                $"Compilación: {intento.PuntosCompilacion}/{intento.PuntosMaximos - 80} puntos\n" +
                $"Casos de prueba: {intento.PuntosObtenidos - intento.PuntosCompilacion - intento.PuntosValidacion - intento.PuntosClaridad}/60 puntos\n" +
                $"Validación del comportamiento: {intento.PuntosValidacion}/15 puntos\n" +
                $"Claridad de la salida: {intento.PuntosClaridad}/5 puntos",
                ancho,
                ColorMoradoClaroCurso);
            AgregarSeccionDetalle(
                contenidoResultadoEvaluacion,
                "Retroalimentación",
                intento.Retroalimentacion.Count == 0
                    ? ObtenerMensajeCalificacion(intento.Calificacion)
                    : string.Join(Environment.NewLine, intento.Retroalimentacion),
                ancho,
                ObtenerColorCalificacion(intento.Calificacion));

            Label tituloCasos = CrearLabelCurso(
                "RESULTADOS DE LAS PRUEBAS",
                Point.Empty,
                new Size(ancho, EscalarDiseno(24)),
                TamanoFuenteCurso.EncabezadoPerfil,
                FontStyle.Bold,
                Color.FromArgb(157, 135, 181));
            tituloCasos.Margin = new Padding(0, EscalarDiseno(9), 0, EscalarDiseno(6));
            contenidoResultadoEvaluacion.Controls.Add(tituloCasos);

            DefinicionEvaluacionPractica? definicion =
                catalogoEvaluacionesService.ObtenerDefinicion(intento.PracticaId);
            int numeroAdicional = 0;

            foreach ((ResultadoCasoPrueba resultado, int indice) in
                intento.Resultados.Select((item, indice) => (item, indice))) {
                CasoPrueba? caso = definicion?.CasosPrueba.FirstOrDefault(item =>
                    item.Id.Equals(resultado.CasoPruebaId, StringComparison.OrdinalIgnoreCase));
                bool esVisible = resultado.EsVisible && (caso?.EsVisible ?? true);
                contenidoResultadoEvaluacion.Controls.Add(
                    CrearTarjetaResultadoCaso(
                        resultado,
                        caso,
                        indice + 1,
                        esVisible ? 0 : ++numeroAdicional,
                        ancho));
            }

            Panel acciones = CrearPanelAccionesResultado(ancho, intento);
            contenidoResultadoEvaluacion.Controls.Add(acciones);
        } finally {
            contenidoResultadoEvaluacion.ResumeLayout(performLayout: true);
            desplazamientoResultadoEvaluacion.ActualizarContenido(volverAlInicio);
        }
    }

    private Panel CrearTarjetaResultadoCaso(
        ResultadoCasoPrueba resultado,
        CasoPrueba? caso,
        int numero,
        int numeroAdicional,
        int ancho) {
        bool visible = resultado.EsVisible && (caso?.EsVisible ?? true);
        string titulo = visible
            ? caso?.Nombre ?? $"Caso {numero}"
            : $"Caso adicional de esta práctica {numeroAdicional}";
        string estado = resultado.Aprobado ? "APROBADO" : "PENDIENTE";
        string detalle = visible
            ? $"Entrada utilizada:\n{resultado.Entrada.TrimEnd()}\n\n" +
              $"Resultado esperado:\n{resultado.SalidaEsperada.TrimEnd()}\n\n" +
              $"Resultado obtenido:\n{(string.IsNullOrWhiteSpace(resultado.SalidaObtenida) ? "Sin salida" : resultado.SalidaObtenida.TrimEnd())}\n\n" +
              resultado.Mensaje
            : resultado.Aprobado
                ? "La prueba adicional se completó correctamente."
                : $"{resultado.Mensaje} Vuelve a intentarlo después de revisar este caso.";
        Label medidor = CrearLabelCurso(
            detalle,
            Point.Empty,
            new Size(Math.Max(1, ancho - EscalarDiseno(36)), 1),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);
        int altoDetalle = CalcularAltoTextoCurso(
            medidor,
            Math.Max(1, ancho - EscalarDiseno(36)),
            visible ? 96 : 42);
        medidor.Dispose();
        int alto = EscalarDiseno(64) + altoDetalle;
        Panel tarjeta = CrearTarjetaCurso(Point.Empty, new Size(ancho, alto), 14);
        tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(10));
        Label nombre = CrearLabelCurso(
            titulo,
            new Point(EscalarDiseno(18), EscalarDiseno(12)),
            new Size(Math.Max(1, ancho - EscalarDiseno(174)), EscalarDiseno(28)),
            TamanoFuenteCurso.TituloTarjetaPractica,
            FontStyle.Bold,
            Color.White);
        Label lblEstado = CrearLabelCurso(
            $"{estado} · {resultado.PuntosObtenidos}/{resultado.PuntosMaximos}",
            new Point(Math.Max(EscalarDiseno(18), ancho - EscalarDiseno(154)), EscalarDiseno(12)),
            new Size(EscalarDiseno(136), EscalarDiseno(28)),
            TamanoFuenteCurso.MetadatoTarjetaPractica,
            FontStyle.Bold,
            resultado.Aprobado ? Color.LightGreen : ColorMoradoClaroCurso,
            ContentAlignment.MiddleRight);
        Label contenido = CrearLabelCurso(
            detalle,
            new Point(EscalarDiseno(18), EscalarDiseno(45)),
            new Size(Math.Max(1, ancho - EscalarDiseno(36)), altoDetalle),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);
        tarjeta.Controls.Add(nombre);
        tarjeta.Controls.Add(lblEstado);
        tarjeta.Controls.Add(contenido);
        return tarjeta;
    }

    private Panel CrearPanelAccionesResultado(int ancho, IntentoPractica intento) {
        bool apilar = ancho < EscalarDiseno(680);
        int separacion = EscalarDiseno(10);
        int altoBoton = EscalarDiseno(40);
        int columnas = apilar ? 1 : 3;
        int anchoBoton = apilar
            ? ancho
            : Math.Max(1, (ancho - separacion * (columnas - 1)) / columnas);
        int cantidadFilas = apilar ? 3 : 1;
        bool ofrecerRealizada = intento.Calificacion >= 70 &&
            ObtenerEstadoPractica(intento.PracticaId) != EstadoPracticaCurso.Realizada;

        if (ofrecerRealizada) {
            cantidadFilas++;
        }

        Panel panel = new() {
            BackColor = Color.FromArgb(18, 14, 27),
            Margin = new Padding(0, EscalarDiseno(8), 0, EscalarDiseno(8)),
            Size = new Size(
                ancho,
                cantidadFilas * altoBoton + (cantidadFilas - 1) * separacion)
        };
        Button volver = CrearBotonSecundarioCurso(
            "Volver a practicar",
            Point.Empty,
            new Size(anchoBoton, altoBoton));
        Button repetir = CrearBotonCurso(
            "Evaluar de nuevo",
            apilar ? new Point(0, altoBoton + separacion) : new Point(anchoBoton + separacion, 0),
            new Size(anchoBoton, altoBoton),
            ColorMoradoCurso);
        Button historial = CrearBotonSecundarioCurso(
            "Ver historial",
            apilar
                ? new Point(0, (altoBoton + separacion) * 2)
                : new Point((anchoBoton + separacion) * 2, 0),
            new Size(anchoBoton, altoBoton));
        volver.TabIndex = 0;
        repetir.TabIndex = 1;
        historial.TabIndex = 2;
        volver.Click += (_, _) => VolverADetallePracticaDesdeEvaluacion();
        repetir.Click += (_, _) => MostrarEvaluacionPracticaActual();
        historial.Click += (_, _) => MostrarHistorialEvaluaciones(desdeResultado: true);
        panel.Controls.Add(volver);
        panel.Controls.Add(repetir);
        panel.Controls.Add(historial);

        if (ofrecerRealizada) {
            int y = apilar
                ? (altoBoton + separacion) * 3
                : altoBoton + separacion;
            Button realizada = CrearBotonSecundarioCurso(
                "Marcar práctica como realizada",
                new Point(0, y),
                new Size(ancho, altoBoton));
            realizada.TabIndex = 3;
            realizada.Click += (_, _) => {
                CambiarEstadoPracticaCurso(EstadoPracticaCurso.Realizada);

                if (ObtenerEstadoPractica(intento.PracticaId) == EstadoPracticaCurso.Realizada) {
                    realizada.Enabled = false;
                    realizada.Text = "Práctica marcada como realizada";
                }
            };
            panel.Controls.Add(realizada);
        }

        return panel;
    }

    private async void MostrarHistorialEvaluaciones(bool desdeResultado) {
        if (practicaCursoSeleccionada is null || cargandoHistorial) {
            return;
        }

        AsegurarVistasEvaluacionInicializadas();
        historialAbiertoDesdeResultado = desdeResultado;
        string practicaId = practicaCursoSeleccionada.Id;
        VistaRutaAprendizaje vistaOrigen = vistaRutaActual;
        int versionSolicitud = ++versionSolicitudHistorial;
        ResultadoCargaHistorialEvaluaciones carga;
        cargandoHistorial = true;

        try {
            carga = await Task.Run(historialEvaluacionesService.CargarHistorial);
        } catch (Exception ex) {
            carga = new ResultadoCargaHistorialEvaluaciones {
                Estado = EstadoCargaHistorialEvaluaciones.ErrorIo,
                Error = ex
            };
        } finally {
            cargandoHistorial = false;
        }

        if (IsDisposed ||
            Disposing ||
            versionSolicitud != versionSolicitudHistorial ||
            vistaRutaActual != vistaOrigen ||
            practicaCursoSeleccionada is null ||
            !practicaCursoSeleccionada.Id.Equals(
                practicaId,
                StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        cargaHistorialEnMemoria = carga;
        practicaCargaHistorial = practicaId;
        ReconstruirHistorialEvaluaciones(volverAlInicio: true);
        MostrarModoCursoInmersivo();
        MostrarSubvistaCurso(
            panelHistorialEvaluacionesVista,
            VistaRutaAprendizaje.Historial);
    }

    private void ReconstruirHistorialEvaluaciones(bool volverAlInicio) {
        if (practicaCursoSeleccionada is null) {
            return;
        }

        ResultadoCargaHistorialEvaluaciones carga =
            cargaHistorialEnMemoria is not null &&
            practicaCargaHistorial.Equals(
                practicaCursoSeleccionada.Id,
                StringComparison.OrdinalIgnoreCase)
                ? cargaHistorialEnMemoria
                : new ResultadoCargaHistorialEvaluaciones {
                    Estado = EstadoCargaHistorialEvaluaciones.ArchivoInexistente
                };
        HistorialPractica? historial = carga.DatosDisponibles
            ? carga.Historial.Practicas.FirstOrDefault(item =>
                item.PracticaId.Equals(
                    practicaCursoSeleccionada.Id,
                    StringComparison.OrdinalIgnoreCase))
            : null;
        if (carga.DatosDisponibles) {
            resumenHistorialActual = historial;
        } else if (resumenHistorialActual is not null &&
            !resumenHistorialActual.PracticaId.Equals(
                practicaCursoSeleccionada.Id,
                StringComparison.OrdinalIgnoreCase)) {
            resumenHistorialActual = null;
        }
        int ancho = Math.Max(
            1,
            contenidoHistorialEvaluaciones.ClientSize.Width -
            contenidoHistorialEvaluaciones.Padding.Horizontal);
        ultimoAnchoHistorial = ancho;
        contenidoHistorialEvaluaciones.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoHistorialEvaluaciones);
            AgregarLabelFluido(
                contenidoHistorialEvaluaciones,
                $"Ruta de aprendizaje  ›  {ObtenerEtiquetaGradoActual()}  ›  " +
                $"{temaCursoSeleccionado?.Nombre ?? "Tema"}  ›  " +
                $"{practicaCursoSeleccionada.Nombre}  ›  Historial",
                ancho,
                TamanoFuenteCurso.NumeroCabecera,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            AgregarLabelFluido(
                contenidoHistorialEvaluaciones,
                $"Historial · {practicaCursoSeleccionada.Nombre}",
                ancho,
                TamanoFuenteCurso.TituloPrincipal,
                FontStyle.Bold,
                Color.White);

            if (carga.Estado ==
                EstadoCargaHistorialEvaluaciones.ContenidoParcialmenteInvalido) {
                AgregarSeccionDetalle(
                    contenidoHistorialEvaluaciones,
                    "Algunos registros no pudieron recuperarse",
                    $"Se ignoraron {carga.RegistrosInvalidos} registros dañados y se conservaron los intentos válidos.",
                    ancho,
                    Color.FromArgb(235, 181, 89));
            }

            if (!carga.DatosDisponibles) {
                AgregarSeccionDetalle(
                    contenidoHistorialEvaluaciones,
                    "Historial no disponible",
                    ObtenerMensajeCargaHistorial(carga.Estado),
                    ancho,
                    Color.LightCoral);
            } else if (historial is null || historial.Intentos.Count == 0) {
                AgregarSeccionDetalle(
                    contenidoHistorialEvaluaciones,
                    "Aún no hay intentos guardados",
                    "Puedes evaluar esta práctica cuantas veces lo necesites. No existe penalización por intentos.",
                    ancho,
                    ColorTextoSecundarioCurso);
            } else {
                AgregarSeccionDetalle(
                    contenidoHistorialEvaluaciones,
                    "Resumen",
                    $"Mejor calificación: {historial.MejorCalificacion ?? 0}\n" +
                    $"Última calificación: {historial.UltimaCalificacion ?? 0}\n" +
                    $"Intentos realizados: {historial.TotalIntentos}",
                    ancho,
                    ColorMoradoClaroCurso);

                foreach ((IntentoPractica intento, int indice) in historial.Intentos
                    .OrderByDescending(item => item.Fecha)
                    .Select((item, indice) => (item, indice))) {
                    contenidoHistorialEvaluaciones.Controls.Add(
                        CrearTarjetaIntentoHistorial(intento, indice, historial, ancho));
                }
            }

            Panel acciones = CrearAccionesHistorial(ancho, historial is not null);
            contenidoHistorialEvaluaciones.Controls.Add(acciones);
        } finally {
            contenidoHistorialEvaluaciones.ResumeLayout(performLayout: true);
            desplazamientoHistorialEvaluaciones.ActualizarContenido(volverAlInicio);
        }
    }

    private Panel CrearTarjetaIntentoHistorial(
        IntentoPractica intento,
        int indice,
        HistorialPractica historial,
        int ancho) {
        Panel tarjeta = CrearTarjetaCurso(
            Point.Empty,
            new Size(ancho, EscalarDiseno(88)),
            14,
            interactiva: true);
        tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(10));
        tarjeta.TabIndex = indice;
        tarjeta.AccessibleName =
            $"Intento del {intento.Fecha.LocalDateTime:g}, calificación {intento.Calificacion}.";
        Label fecha = CrearLabelCurso(
            intento.Fecha.LocalDateTime.ToString("g"),
            new Point(EscalarDiseno(18), EscalarDiseno(12)),
            new Size(Math.Max(1, ancho - EscalarDiseno(190)), EscalarDiseno(26)),
            TamanoFuenteCurso.TituloTarjetaPractica,
            FontStyle.Bold,
            Color.White);
        Label detalle = CrearLabelCurso(
            $"{intento.PruebasSuperadas} de {intento.PruebasTotales} pruebas · {intento.ResultadoGeneral}",
            new Point(EscalarDiseno(18), EscalarDiseno(43)),
            new Size(Math.Max(1, ancho - EscalarDiseno(190)), EscalarDiseno(28)),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            ColorTextoSecundarioCurso);
        Label calificacion = CrearLabelCurso(
            intento.Calificacion.ToString(),
            new Point(Math.Max(EscalarDiseno(18), ancho - EscalarDiseno(142)), EscalarDiseno(15)),
            new Size(EscalarDiseno(124), EscalarDiseno(48)),
            22F,
            FontStyle.Bold,
            ObtenerColorCalificacion(intento.Calificacion),
            ContentAlignment.MiddleCenter);
        tarjeta.Controls.Add(fecha);
        tarjeta.Controls.Add(detalle);
        tarjeta.Controls.Add(calificacion);
        ConfigurarInteraccionTarjeta(
            tarjeta,
            () => MostrarResultadoEvaluacion(intento, historial, desdeHistorial: true));
        return tarjeta;
    }

    private Panel CrearAccionesHistorial(int ancho, bool puedeEliminar) {
        int alto = EscalarDiseno(40);
        int separacion = EscalarDiseno(12);
        int anchoBoton = Math.Max(1, (ancho - separacion) / 2);
        Panel panel = new() {
            BackColor = Color.FromArgb(18, 14, 27),
            Margin = new Padding(0, EscalarDiseno(8), 0, EscalarDiseno(8)),
            Size = new Size(ancho, alto)
        };
        Button volver = CrearBotonSecundarioCurso(
            "Volver",
            Point.Empty,
            new Size(anchoBoton, alto));
        Button eliminar = CrearBotonSecundarioCurso(
            "Eliminar historial de esta práctica",
            new Point(anchoBoton + separacion, 0),
            new Size(anchoBoton, alto));
        volver.TabIndex = 0;
        eliminar.TabIndex = 1;
        volver.Enabled = !actualizandoHistorial;
        eliminar.Enabled = puedeEliminar && !actualizandoHistorial;
        volver.Click += (_, _) => VolverDesdeHistorial();
        eliminar.Click += (_, _) => EliminarHistorialPracticaActual();
        panel.Controls.Add(volver);
        panel.Controls.Add(eliminar);
        return panel;
    }

    private void VolverDesdeHistorial() {
        if (historialAbiertoDesdeResultado && intentoResultadoActual is not null) {
            MostrarResultadoEvaluacion(
                intentoResultadoActual,
                resumenHistorialActual,
                desdeHistorial: resultadoMostradoDesdeHistorial);
            return;
        }

        VolverADetallePracticaDesdeEvaluacion();
    }

    private async void EliminarHistorialPracticaActual() {
        if (practicaCursoSeleccionada is null || actualizandoHistorial) {
            return;
        }

        DialogResult confirmacion = MessageBox.Show(
            "Se eliminarán únicamente los intentos de evaluación de esta práctica. " +
            "El proyecto y el progreso del curso se conservarán.\n\n¿Deseas continuar?",
            "Eliminar historial",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirmacion != DialogResult.Yes) {
            return;
        }

        string practicaId = practicaCursoSeleccionada.Id;
        actualizandoHistorial = true;
        ReconstruirHistorialEvaluaciones(volverAlInicio: false);
        ResultadoEliminacionHistorialEvaluaciones resultado;

        try {
            Task<ResultadoEliminacionHistorialEvaluaciones> tareaEliminacion =
                Task.Run(() =>
                    historialEvaluacionesService.EliminarHistorialPractica(practicaId));
            tareaEvaluacionActiva = tareaEliminacion;
            resultado = await tareaEliminacion;
        } catch (Exception ex) {
            resultado = new ResultadoEliminacionHistorialEvaluaciones {
                Estado = EstadoEliminacionHistorialEvaluaciones.ErrorIo,
                Error = ex
            };
        } finally {
            tareaEvaluacionActiva = null;
            actualizandoHistorial = false;
        }

        if (resultado.EsExitosa) {
            MarcarInicioPendienteDeRecarga();
        }

        if (IsDisposed || Disposing) {
            return;
        }

        if (!resultado.EsExitosa) {
            MessageBox.Show(
                ObtenerMensajeEliminacionHistorial(resultado.Estado),
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        intentoResultadoActual = null;
        resumenHistorialActual = null;
        historialAbiertoDesdeResultado = false;
        resultadoMostradoDesdeHistorial = false;
        cargaHistorialEnMemoria = null;
        practicaCargaHistorial = string.Empty;
        MostrarHistorialEvaluaciones(desdeResultado: false);
    }

    private static string ObtenerEstadoCalificacion(int calificacion) {
        return calificacion switch {
            100 => "Dominada",
            >= 90 => "Excelente",
            >= 80 => "Buen trabajo",
            >= 70 => "Aprobada",
            _ => "Necesita revisión"
        };
    }

    private static int? ObtenerNumeroIntento(
        IntentoPractica intento,
        HistorialPractica? historial) {
        if (historial is null || historial.TotalIntentos <= 0) {
            return null;
        }

        IntentoPractica[] ordenados = historial.Intentos
            .OrderByDescending(item => item.Fecha)
            .ThenByDescending(item => item.Id, StringComparer.Ordinal)
            .ToArray();
        int indice = Array.FindIndex(ordenados, item =>
            item.Id.Equals(intento.Id, StringComparison.OrdinalIgnoreCase));
        return indice < 0
            ? null
            : Math.Max(1, historial.TotalIntentos - indice);
    }

    private static string ObtenerMensajeCalificacion(int calificacion) {
        return calificacion switch {
            100 => "Dominaste todos los criterios de esta práctica.",
            >= 90 => "La práctica funciona correctamente.",
            >= 70 => "Buen avance. Revisa los casos que no pasaron.",
            _ => "Tu proyecto todavía necesita algunos ajustes."
        };
    }

    private static Color ObtenerColorCalificacion(int calificacion) {
        return calificacion >= 70
            ? Color.LightGreen
            : Color.FromArgb(202, 151, 247);
    }

    private static string ObtenerMensajeCargaHistorial(
        EstadoCargaHistorialEvaluaciones estado) {
        return estado switch {
            EstadoCargaHistorialEvaluaciones.PermisosInsuficientes =>
                "No hay permisos para leer evaluaciones.json. EndForge continuará abierto.",
            EstadoCargaHistorialEvaluaciones.ErrorIo =>
                "evaluaciones.json está bloqueado o no está disponible.",
            EstadoCargaHistorialEvaluaciones.VersionNoCompatible =>
                "La versión de evaluaciones.json no es compatible. El archivo no se modificará.",
            _ =>
                "evaluaciones.json no se puede recuperar de forma segura. El archivo no se modificará."
        };
    }

    private static string ObtenerMensajeEliminacionHistorial(
        EstadoEliminacionHistorialEvaluaciones estado) {
        return estado switch {
            EstadoEliminacionHistorialEvaluaciones.PermisosInsuficientes =>
                "No se pudo eliminar el historial porque no hay permisos. El archivo anterior se conservó.",
            EstadoEliminacionHistorialEvaluaciones.ContenidoIrrecuperable =>
                "No se modificó evaluaciones.json porque su contenido no se puede recuperar con seguridad.",
            EstadoEliminacionHistorialEvaluaciones.VersionNoCompatible =>
                "No se modificó evaluaciones.json porque su versión no es compatible.",
            _ =>
                "No se pudo actualizar evaluaciones.json porque está bloqueado o no está disponible."
        };
    }

    private void RecalcularDistribucionResultados() {
        if (!vistasEvaluacionInicializadas) {
            return;
        }

        if (panelResultadoEvaluacionVista.Visible) {
            int ancho = Math.Max(
                1,
                contenidoResultadoEvaluacion.ClientSize.Width -
                contenidoResultadoEvaluacion.Padding.Horizontal);

            if (Math.Abs(ancho - ultimoAnchoResultado) >= EscalarDiseno(8)) {
                ReconstruirResultadoEvaluacion(volverAlInicio: false);
            } else {
                desplazamientoResultadoEvaluacion.ActualizarContenido(false);
            }
        }

        if (panelHistorialEvaluacionesVista.Visible) {
            int ancho = Math.Max(
                1,
                contenidoHistorialEvaluaciones.ClientSize.Width -
                contenidoHistorialEvaluaciones.Padding.Horizontal);

            if (Math.Abs(ancho - ultimoAnchoHistorial) >= EscalarDiseno(8)) {
                ReconstruirHistorialEvaluaciones(volverAlInicio: false);
            } else {
                desplazamientoHistorialEvaluaciones.ActualizarContenido(false);
            }
        }
    }
}
