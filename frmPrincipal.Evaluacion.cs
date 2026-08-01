using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private readonly CatalogoEvaluacionesService catalogoEvaluacionesService = new();
    private readonly EvaluacionPracticaService evaluacionPracticaService = new();
    private Panel panelEvaluacionVista = null!;
    private PanelDesplazableSinBarras desplazamientoEvaluacion = null!;
    private FlowLayoutPanel contenidoEvaluacion = null!;
    private DefinicionEvaluacionPractica? definicionEvaluacionActual;
    private CancellationTokenSource? cancelacionEvaluacion;
    private Task? tareaEvaluacionActiva;
    private Label lblEstadoEvaluacion = null!;
    private Panel panelFondoProgresoEvaluacion = null!;
    private Panel panelRellenoProgresoEvaluacion = null!;
    private Button btnIniciarEvaluacion = null!;
    private Button btnCancelarEvaluacion = null!;
    private Button btnVolverPracticaEvaluacion = null!;
    private readonly CoordinadorCierreOperacionesAsync
        coordinadorCierreOperaciones = new();
    private bool vistasEvaluacionInicializadas;
    private bool evaluacionEnCurso;
    private bool esperandoCierreOperaciones;
    private bool cierreTrasOperacionesAutorizado;
    private bool evaluacionInmersivaAmpliaActiva;
    private string textoEstadoEvaluacion = "Lista para iniciar.";
    private int etapaVisualEvaluacion;
    private int ultimoAnchoEvaluacion = -1;

    private const int AnchoMaximoContenidoEvaluacion = 1120;

    private void AsegurarVistasEvaluacionInicializadas() {
        if (vistasEvaluacionInicializadas || IsDisposed || Disposing) {
            return;
        }

        panelEvaluacionVista = CrearVistaCurso("panelEvaluacionVista");
        panelResultadoEvaluacionVista = CrearVistaCurso("panelResultadoEvaluacionVista");
        panelHistorialEvaluacionesVista = CrearVistaCurso("panelHistorialEvaluacionesVista");
        panelPrincipal.Controls.Add(panelEvaluacionVista);
        panelPrincipal.Controls.Add(panelResultadoEvaluacionVista);
        panelPrincipal.Controls.Add(panelHistorialEvaluacionesVista);
        ConstruirVistaEvaluacion();
        ConstruirVistasResultadoEHistorial();
        ActivarDobleBuffer(panelEvaluacionVista);
        ActivarDobleBuffer(panelResultadoEvaluacionVista);
        ActivarDobleBuffer(panelHistorialEvaluacionesVista);
        ActivarDobleBuffer(desplazamientoEvaluacion);
        ActivarDobleBuffer(desplazamientoResultadoEvaluacion);
        ActivarDobleBuffer(desplazamientoHistorialEvaluaciones);
        vistasEvaluacionInicializadas = true;
        SincronizarLimitesVistasAdaptables();
    }

    private void ConstruirVistaEvaluacion() {
        panelEvaluacionVista.BackColor = Color.FromArgb(18, 14, 27);
        panelEvaluacionVista.BorderStyle = BorderStyle.None;
        desplazamientoEvaluacion = CrearPanelDesplazableCurso(
            "desplazamientoEvaluacion",
            "contenidoEvaluacion",
            Point.Empty,
            panelEvaluacionVista.ClientSize,
            new Padding(22, 18, 22, 28));
        desplazamientoEvaluacion.Dock = DockStyle.Fill;
        desplazamientoEvaluacion.BorderStyle = BorderStyle.None;
        desplazamientoEvaluacion.BackColor = Color.FromArgb(18, 14, 27);
        desplazamientoEvaluacion.ColorFondoContenido = Color.FromArgb(18, 14, 27);
        desplazamientoEvaluacion.Padding = Padding.Empty;
        desplazamientoEvaluacion.MostrarBordeFoco = false;
        contenidoEvaluacion = desplazamientoEvaluacion.Contenido;
        contenidoEvaluacion.BackColor = Color.FromArgb(18, 14, 27);
        panelEvaluacionVista.Controls.Add(desplazamientoEvaluacion);
    }

    private void AgregarControlesEvaluacionPractica(
        PracticaCurso practica,
        int anchoContenido) {
        DefinicionEvaluacionPractica? definicion =
            catalogoEvaluacionesService.ObtenerDefinicion(practica.Id);

        if (definicion is null) {
            AgregarSeccionDetalle(
                contenidoDetallePractica,
                "Evaluación",
                "Evaluación automática próximamente para esta práctica.",
                anchoContenido,
                ColorTextoSecundarioCurso);
            return;
        }

        ProgresoPractica? progreso = ObtenerProgresoPractica(practica.Id);
        string rutaProyecto = progreso?.RutaProyecto ?? string.Empty;
        ResultadoResolucionProyectoEvaluacionCpp? resolucion =
            ResolverProyectoEvaluacionSeguro(rutaProyecto);
        bool disponible = !evaluacionEnCurso && resolucion?.EsExitosa == true;
        Button evaluar = CrearBotonCurso(
            "Evaluar práctica",
            Point.Empty,
            new Size(anchoContenido, EscalarDiseno(42)),
            ColorMoradoCurso);
        evaluar.Margin = new Padding(0, EscalarDiseno(5), 0, EscalarDiseno(7));
        evaluar.TabIndex = 1;
        evaluar.AccessibleDescription =
            "Abre la evaluación local de esta práctica. No existe límite de tiempo para resolverla.";
        ConfigurarEstadoBotonEvaluacion(evaluar, disponible);

        if (disponible) {
            evaluar.Click += (_, _) => AbrirEvaluacionPractica(practica, definicion);
        }

        contenidoDetallePractica.Controls.Add(evaluar);

        if (!disponible) {
            string mensaje = evaluacionEnCurso
                ? "Ya hay otra evaluación en ejecución."
                : ObtenerMensajeDisponibilidadEvaluacion(rutaProyecto, resolucion);
            Label aviso = CrearLabelCurso(
                mensaje,
                Point.Empty,
                new Size(anchoContenido, EscalarDiseno(38)),
                TamanoFuenteCurso.MetadatoTarjetaPractica,
                FontStyle.Regular,
                ColorTextoSecundarioCurso);
            aviso.Margin = new Padding(0, 0, 0, EscalarDiseno(8));
            contenidoDetallePractica.Controls.Add(aviso);
        }
    }

    private ResultadoResolucionProyectoEvaluacionCpp? ResolverProyectoEvaluacionSeguro(
        string rutaProyecto) {
        if (string.IsNullOrWhiteSpace(rutaProyecto) || !Directory.Exists(rutaProyecto)) {
            return null;
        }

        try {
            return seleccionSolucionesService.ResolverProyectoParaEvaluacion(rutaProyecto);
        } catch (Exception ex) {
            return new ResultadoResolucionProyectoEvaluacionCpp {
                Estado = EstadoResolucionProyectoEvaluacionCpp.ErrorLectura,
                Error = ex
            };
        }
    }

    private static string ObtenerMensajeDisponibilidadEvaluacion(
        string rutaProyecto,
        ResultadoResolucionProyectoEvaluacionCpp? resolucion) {
        if (string.IsNullOrWhiteSpace(rutaProyecto) || !Directory.Exists(rutaProyecto)) {
            return "Crea o vincula primero el proyecto de esta práctica para poder evaluarlo.";
        }

        return resolucion?.Estado switch {
            EstadoResolucionProyectoEvaluacionCpp.SolucionInexistente =>
                "La solución guardada para esta práctica ya no existe.",
            EstadoResolucionProyectoEvaluacionCpp.SolucionAmbigua =>
                "Hay varias soluciones y no se puede determinar de forma segura cuál evaluar.",
            EstadoResolucionProyectoEvaluacionCpp.MarcadorIlegible =>
                "No se pudo leer .endforge-solution porque está bloqueado o no hay permisos.",
            EstadoResolucionProyectoEvaluacionCpp.MarcadorInvalido =>
                ".endforge-solution no contiene una ruta válida.",
            EstadoResolucionProyectoEvaluacionCpp.ProyectoAmbiguo =>
                "La solución contiene varios proyectos ejecutables y no se puede elegir uno de forma segura.",
            EstadoResolucionProyectoEvaluacionCpp.ProyectoNoEjecutable =>
                "El proyecto C++ asociado no genera una aplicación ejecutable.",
            EstadoResolucionProyectoEvaluacionCpp.ProyectoInexistente =>
                "La solución referencia un proyecto C++ que ya no existe.",
            _ =>
                "La solución o el proyecto C++ no están disponibles para una evaluación segura."
        };
    }

    private static void ConfigurarEstadoBotonEvaluacion(Button boton, bool habilitado) {
        boton.Enabled = habilitado;

        if (habilitado) {
            boton.BackColor = ColorMoradoCurso;
            boton.ForeColor = Color.White;
            boton.Cursor = Cursors.Hand;
            boton.FlatAppearance.MouseOverBackColor = ColorMoradoClaroCurso;
            boton.FlatAppearance.MouseDownBackColor = Color.FromArgb(105, 55, 165);
        } else {
            Color fondo = Color.FromArgb(48, 43, 56);
            boton.BackColor = fondo;
            boton.ForeColor = Color.FromArgb(165, 160, 173);
            boton.Cursor = Cursors.Default;
            boton.FlatAppearance.MouseOverBackColor = fondo;
            boton.FlatAppearance.MouseDownBackColor = fondo;
        }
    }

    private void AbrirEvaluacionPractica(
        PracticaCurso practica,
        DefinicionEvaluacionPractica definicion) {
        if (evaluacionEnCurso) {
            return;
        }

        practicaCursoSeleccionada = practica;
        temaCursoSeleccionado = cursoService.ObtenerTema(practica.TemaId);
        definicionEvaluacionActual = definicion;
        textoEstadoEvaluacion = "Lista para iniciar.";
        etapaVisualEvaluacion = 0;
        evaluacionInmersivaAmpliaActiva = true;
        AsegurarVistasEvaluacionInicializadas();
        MostrarModoCursoInmersivo();
        MostrarSubvistaCurso(panelEvaluacionVista, VistaRutaAprendizaje.Evaluacion);
        ReconstruirVistaEvaluacion(volverAlInicio: true);
    }

    private void MostrarEvaluacionPracticaActual() {
        if (practicaCursoSeleccionada is null) {
            return;
        }

        DefinicionEvaluacionPractica? definicion =
            catalogoEvaluacionesService.ObtenerDefinicion(practicaCursoSeleccionada.Id);

        if (definicion is null) {
            VolverADetallePracticaDesdeEvaluacion();
            return;
        }

        AbrirEvaluacionPractica(practicaCursoSeleccionada, definicion);
    }

    private void ReconstruirVistaEvaluacion(bool volverAlInicio) {
        if (definicionEvaluacionActual is null || practicaCursoSeleccionada is null) {
            return;
        }

        DefinicionEvaluacionPractica definicion = definicionEvaluacionActual;
        ProgresoPractica? progresoPractica =
            ObtenerProgresoPractica(practicaCursoSeleccionada.Id);
        string rutaProyecto = progresoPractica?.RutaProyecto ?? string.Empty;
        desplazamientoEvaluacion.ActualizarContenido(volverAlInicio: false);
        int ancho = AjustarColumnaCentralEvaluacion();
        ultimoAnchoEvaluacion = ancho;
        contenidoEvaluacion.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoEvaluacion);
            AgregarLabelFluido(
                contenidoEvaluacion,
                $"Ruta de aprendizaje › {ObtenerEtiquetaGradoActual()} › " +
                $"{temaCursoSeleccionado?.Nombre ?? "Tema"} › " +
                $"{practicaCursoSeleccionada.Nombre} › Evaluación",
                ancho,
                TamanoFuenteCurso.MetadatoTarjetaPractica,
                FontStyle.Regular,
                ColorMoradoClaroCurso);
            AgregarLabelFluido(
                contenidoEvaluacion,
                $"Evaluar · {definicion.NombrePractica}",
                ancho,
                TamanoFuenteCurso.TituloPrincipal,
                FontStyle.Bold,
                Color.White);
            AgregarSeccionDetalle(
                contenidoEvaluacion,
                "Proyecto",
                string.IsNullOrWhiteSpace(rutaProyecto)
                    ? "Proyecto no vinculado"
                    : PrepararRutaMultilinea(rutaProyecto),
                ancho);
            AgregarSeccionDetalle(
                contenidoEvaluacion,
                "Qué revisará EndForge",
                definicion.Descripcion + Environment.NewLine +
                definicion.ContratoEntrada,
                ancho);

            Label tituloCasos = CrearLabelCurso(
                "CASOS VISIBLES",
                Point.Empty,
                new Size(ancho, EscalarDiseno(24)),
                TamanoFuenteCurso.EncabezadoPerfil,
                FontStyle.Bold,
                Color.FromArgb(157, 135, 181));
            tituloCasos.Margin = new Padding(0, EscalarDiseno(7), 0, EscalarDiseno(6));
            contenidoEvaluacion.Controls.Add(tituloCasos);

            foreach ((CasoPrueba caso, int indice) in definicion.CasosPrueba
                .Where(item => item.EsVisible)
                .Select((item, indice) => (item, indice))) {
                contenidoEvaluacion.Controls.Add(CrearTarjetaCasoEvaluacion(caso, indice, ancho));
            }

            AgregarSeccionDetalle(
                contenidoEvaluacion,
                "Criterios y puntuación",
                string.Join(Environment.NewLine, definicion.Criterios.Select(criterio =>
                    $"{criterio.Nombre}: {criterio.PuntosMaximos} puntos — {criterio.Descripcion}")),
                ancho,
                ColorMoradoClaroCurso);
            AgregarSeccionDetalle(
                contenidoEvaluacion,
                "Aprende sin presión",
                "No hay límite de tiempo para resolver la práctica. Los intentos son ilimitados y no existe penalización por tiempo ni por volver a evaluar.\n" +
                "El programa solo se detendrá automáticamente si no termina su ejecución.",
                ancho,
                ColorTextoSecundarioCurso);
            AgregarSeccionDetalle(
                contenidoEvaluacion,
                "Ejecución local",
                "La evaluación compila y ejecuta el proyecto en esta computadora con los permisos de tu usuario. " +
                "Evalúa únicamente proyectos propios; esta versión local no es un entorno aislado.",
                ancho,
                Color.FromArgb(214, 198, 231));

            Panel estado = CrearPanelEstadoEvaluacion(ancho);
            contenidoEvaluacion.Controls.Add(estado);
            Panel acciones = CrearPanelAccionesEvaluacion(ancho, rutaProyecto);
            contenidoEvaluacion.Controls.Add(acciones);
        } finally {
            contenidoEvaluacion.ResumeLayout(performLayout: true);
            desplazamientoEvaluacion.ActualizarContenido(volverAlInicio);
        }
    }

    private Panel CrearTarjetaCasoEvaluacion(CasoPrueba caso, int indice, int ancho) {
        int margenHorizontal = EscalarDiseno(18);
        int margenSuperior = EscalarDiseno(12);
        int margenInferior = EscalarDiseno(15);
        int separacion = EscalarDiseno(5);
        int altoMetadatos = EscalarDiseno(22);
        int anchoInterior = Math.Max(1, ancho - margenHorizontal * 2);
        int anchoPuntos = Math.Min(EscalarDiseno(112), anchoInterior / 3);
        Label numero = CrearLabelCurso(
            $"CASO {indice + 1:00}",
            Point.Empty,
            new Size(Math.Max(1, anchoInterior - anchoPuntos), altoMetadatos),
            TamanoFuenteCurso.NumeroCabecera,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        Label puntos = CrearLabelCurso(
            $"{caso.Puntos} puntos",
            Point.Empty,
            new Size(anchoPuntos, altoMetadatos),
            TamanoFuenteCurso.MetadatoTarjetaPractica,
            FontStyle.Bold,
            ColorTextoSecundarioCurso,
            ContentAlignment.MiddleRight);
        Label nombre = CrearLabelCurso(
            caso.Nombre,
            Point.Empty,
            new Size(anchoInterior, 1),
            TamanoFuenteCurso.TituloTarjetaPractica,
            FontStyle.Bold,
            Color.White,
            ContentAlignment.TopLeft);
        Label descripcion = CrearLabelCurso(
            caso.Descripcion,
            Point.Empty,
            new Size(anchoInterior, 1),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            ColorTextoSecundarioCurso,
            ContentAlignment.TopLeft);
        Label tituloEntrada = CrearLabelCurso(
            "Entrada",
            Point.Empty,
            new Size(anchoInterior, EscalarDiseno(18)),
            TamanoFuenteCurso.EncabezadoDetalle,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ContentAlignment.TopLeft);
        Label entrada = CrearLabelCurso(
            FormatearBloqueCaso(caso.Entrada),
            Point.Empty,
            new Size(anchoInterior, 1),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            ColorTextoSecundarioCurso,
            ContentAlignment.TopLeft);
        Label tituloEsperado = CrearLabelCurso(
            "Comportamiento esperado",
            Point.Empty,
            new Size(anchoInterior, EscalarDiseno(18)),
            TamanoFuenteCurso.EncabezadoDetalle,
            FontStyle.Bold,
            ColorMoradoClaroCurso,
            ContentAlignment.TopLeft);
        Label esperado = CrearLabelCurso(
            FormatearBloqueCaso(caso.SalidaEsperada),
            Point.Empty,
            new Size(anchoInterior, 1),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Regular,
            ColorTextoSecundarioCurso,
            ContentAlignment.TopLeft);

        int altoNombre = CalcularAltoTextoCurso(nombre, anchoInterior, 28);
        int altoDescripcion = CalcularAltoTextoCurso(descripcion, anchoInterior, 24);
        int altoEntrada = CalcularAltoTextoCurso(entrada, anchoInterior, 24);
        int altoEsperado = CalcularAltoTextoCurso(esperado, anchoInterior, 24);
        int y = margenSuperior;

        numero.SetBounds(margenHorizontal, y, Math.Max(1, anchoInterior - anchoPuntos), altoMetadatos);
        puntos.SetBounds(
            margenHorizontal + Math.Max(1, anchoInterior - anchoPuntos),
            y,
            anchoPuntos,
            altoMetadatos);
        y += altoMetadatos + separacion;
        nombre.SetBounds(margenHorizontal, y, anchoInterior, altoNombre);
        y += altoNombre + separacion;
        descripcion.SetBounds(margenHorizontal, y, anchoInterior, altoDescripcion);
        y += altoDescripcion + EscalarDiseno(8);
        tituloEntrada.SetBounds(margenHorizontal, y, anchoInterior, tituloEntrada.Height);
        y += tituloEntrada.Height + EscalarDiseno(2);
        entrada.SetBounds(margenHorizontal, y, anchoInterior, altoEntrada);
        y += altoEntrada + EscalarDiseno(8);
        tituloEsperado.SetBounds(margenHorizontal, y, anchoInterior, tituloEsperado.Height);
        y += tituloEsperado.Height + EscalarDiseno(2);
        esperado.SetBounds(margenHorizontal, y, anchoInterior, altoEsperado);
        y += altoEsperado + margenInferior;

        Panel tarjeta = CrearTarjetaCurso(Point.Empty, new Size(ancho, y), 14);
        tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(10));
        Panel acento = new() {
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(EscalarDiseno(4), y)
        };
        tarjeta.Controls.Add(acento);
        tarjeta.Controls.Add(numero);
        tarjeta.Controls.Add(nombre);
        tarjeta.Controls.Add(puntos);
        tarjeta.Controls.Add(descripcion);
        tarjeta.Controls.Add(tituloEntrada);
        tarjeta.Controls.Add(entrada);
        tarjeta.Controls.Add(tituloEsperado);
        tarjeta.Controls.Add(esperado);
        return tarjeta;
    }

    private static string FormatearBloqueCaso(string texto) {
        return texto
            .TrimEnd()
            .Replace("\r\n", "  ·  ", StringComparison.Ordinal)
            .Replace("\n", "  ·  ", StringComparison.Ordinal);
    }

    private static string PrepararRutaMultilinea(string ruta) {
        return ruta
            .Replace("\\", "\\\u200B", StringComparison.Ordinal)
            .Replace("/", "/\u200B", StringComparison.Ordinal);
    }

    private int AjustarColumnaCentralEvaluacion() {
        int anchoVista = Math.Max(1, contenidoEvaluacion.ClientSize.Width);
        int margenPreferido = EscalarDiseno(24);
        int margenCompacto = Math.Min(margenPreferido, Math.Max(0, (anchoVista - 1) / 2));
        int anchoDisponible = Math.Max(1, anchoVista - margenCompacto * 2);
        int anchoContenido = Math.Min(
            EscalarDiseno(AnchoMaximoContenidoEvaluacion),
            anchoDisponible);
        int espacioRestante = Math.Max(0, anchoVista - anchoContenido);
        int margenIzquierdo = espacioRestante / 2;
        int margenDerecho = espacioRestante - margenIzquierdo;

        contenidoEvaluacion.Padding = new Padding(
            margenIzquierdo,
            EscalarDiseno(18),
            margenDerecho,
            EscalarDiseno(28));
        return anchoContenido;
    }

    private Panel CrearPanelEstadoEvaluacion(int ancho) {
        Panel panel = CrearTarjetaCurso(
            Point.Empty,
            new Size(ancho, EscalarDiseno(78)),
            14);
        panel.Margin = new Padding(0, EscalarDiseno(6), 0, EscalarDiseno(10));
        lblEstadoEvaluacion = CrearLabelCurso(
            textoEstadoEvaluacion,
            new Point(EscalarDiseno(18), EscalarDiseno(12)),
            new Size(Math.Max(1, ancho - EscalarDiseno(36)), EscalarDiseno(30)),
            TamanoFuenteCurso.TextoDetalle,
            FontStyle.Bold,
            ColorMoradoClaroCurso);
        panelFondoProgresoEvaluacion = new Panel {
            BackColor = Color.FromArgb(55, 45, 70),
            Location = new Point(EscalarDiseno(18), EscalarDiseno(52)),
            Size = new Size(Math.Max(1, ancho - EscalarDiseno(36)), EscalarDiseno(9))
        };
        panelRellenoProgresoEvaluacion = new Panel {
            BackColor = ColorMoradoCurso,
            Location = Point.Empty,
            Size = new Size(
                CalcularAnchoProgresoEvaluacion(panelFondoProgresoEvaluacion.Width),
                panelFondoProgresoEvaluacion.Height)
        };
        panelFondoProgresoEvaluacion.Controls.Add(panelRellenoProgresoEvaluacion);
        panel.Controls.Add(lblEstadoEvaluacion);
        panel.Controls.Add(panelFondoProgresoEvaluacion);
        return panel;
    }

    private Panel CrearPanelAccionesEvaluacion(int ancho, string rutaProyecto) {
        int separacion = EscalarDiseno(10);
        int alto = EscalarDiseno(40);
        bool apilar = ancho < EscalarDiseno(620);
        int anchoBoton = apilar ? ancho : Math.Max(1, (ancho - separacion * 2) / 3);
        Panel panel = new() {
            BackColor = Color.FromArgb(18, 14, 27),
            Margin = new Padding(0, 0, 0, EscalarDiseno(8)),
            Size = new Size(ancho, apilar ? alto * 3 + separacion * 2 : alto)
        };
        btnIniciarEvaluacion = CrearBotonCurso(
            "Iniciar evaluación",
            Point.Empty,
            new Size(anchoBoton, alto),
            ColorMoradoCurso);
        btnCancelarEvaluacion = CrearBotonSecundarioCurso(
            "Cancelar evaluación",
            apilar ? new Point(0, alto + separacion) : new Point(anchoBoton + separacion, 0),
            new Size(anchoBoton, alto));
        btnVolverPracticaEvaluacion = CrearBotonSecundarioCurso(
            "Volver a la práctica",
            apilar
                ? new Point(0, (alto + separacion) * 2)
                : new Point((anchoBoton + separacion) * 2, 0),
            new Size(anchoBoton, alto));
        ResultadoResolucionProyectoEvaluacionCpp? resolucion =
            ResolverProyectoEvaluacionSeguro(rutaProyecto);
        ConfigurarEstadoBotonEvaluacion(
            btnIniciarEvaluacion,
            !evaluacionEnCurso && resolucion?.EsExitosa == true);
        btnCancelarEvaluacion.Enabled =
            evaluacionEnCurso && cancelacionEvaluacion is not null;
        btnVolverPracticaEvaluacion.Enabled = !evaluacionEnCurso;
        btnIniciarEvaluacion.TabIndex = 0;
        btnCancelarEvaluacion.TabIndex = 1;
        btnVolverPracticaEvaluacion.TabIndex = 2;
        btnIniciarEvaluacion.Click += IniciarEvaluacion_Click;
        btnCancelarEvaluacion.Click += (_, _) => CancelarEvaluacionActiva();
        btnVolverPracticaEvaluacion.Click += (_, _) =>
            VolverADetallePracticaDesdeEvaluacion();
        panel.Controls.Add(btnIniciarEvaluacion);
        panel.Controls.Add(btnCancelarEvaluacion);
        panel.Controls.Add(btnVolverPracticaEvaluacion);
        return panel;
    }

    private async void IniciarEvaluacion_Click(object? sender, EventArgs e) {
        if (evaluacionEnCurso ||
            practicaCursoSeleccionada is null ||
            definicionEvaluacionActual is null) {
            return;
        }

        ProgresoPractica? progresoPractica =
            ObtenerProgresoPractica(practicaCursoSeleccionada.Id);
        string rutaProyecto = progresoPractica?.RutaProyecto ?? string.Empty;
        ResultadoResolucionProyectoEvaluacionCpp? resolucion =
            ResolverProyectoEvaluacionSeguro(rutaProyecto);

        if (resolucion?.EsExitosa != true) {
            textoEstadoEvaluacion = ObtenerMensajeDisponibilidadEvaluacion(
                rutaProyecto,
                resolucion);
            ActualizarEstadoEvaluacionVisual();
            return;
        }

        PracticaCurso practica = practicaCursoSeleccionada;
        DefinicionEvaluacionPractica definicion = definicionEvaluacionActual;
        Task flujoEvaluacion = EjecutarFlujoEvaluacionAsync(
            practica,
            definicion,
            rutaProyecto);
        tareaEvaluacionActiva = flujoEvaluacion;

        try {
            await flujoEvaluacion;
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            Program.RegistrarErrorRecuperable(ex);

            if (PuedeActualizarInterfazEvaluacion()) {
                etapaVisualEvaluacion = 0;
                textoEstadoEvaluacion =
                    "No se pudo completar la evaluación local.";
                ReconstruirVistaEvaluacion(volverAlInicio: false);
            }
        } finally {
            if (ReferenceEquals(
                    tareaEvaluacionActiva,
                    flujoEvaluacion)) {
                tareaEvaluacionActiva = null;
            }
        }
    }

    private async Task EjecutarFlujoEvaluacionAsync(
        PracticaCurso practica,
        DefinicionEvaluacionPractica definicion,
        string rutaProyecto) {
        evaluacionEnCurso = true;
        etapaVisualEvaluacion = 1;
        textoEstadoEvaluacion = "Preparando evaluación…";
        cancelacionEvaluacion?.Dispose();
        cancelacionEvaluacion = new CancellationTokenSource();
        ActualizarEstadoEvaluacionVisual();
        ReconstruirDetallePractica(practica, volverAlInicio: false);
        Progress<ProgresoEvaluacionPractica> progreso = new(ActualizarProgresoEvaluacion);

        try {
            ResultadoProcesoEvaluacionPractica resultado;

            try {
                resultado = await evaluacionPracticaService.EvaluarAsync(
                    new SolicitudEvaluacionPractica {
                        PracticaId = practica.Id,
                        RutaProyecto = rutaProyecto
                    },
                    progreso,
                    cancelacionEvaluacion.Token);
            } catch (OperationCanceledException) {
                resultado = new ResultadoProcesoEvaluacionPractica {
                    Estado = EstadoProcesoEvaluacionPractica.Cancelada,
                    Mensaje = "Evaluación cancelada."
                };
            } catch (Exception ex) {
                resultado = new ResultadoProcesoEvaluacionPractica {
                    Estado = EstadoProcesoEvaluacionPractica.ErrorInfraestructura,
                    Mensaje = "No se pudo completar la evaluación local.",
                    Error = ex
                };
            } finally {
                cancelacionEvaluacion?.Dispose();
                cancelacionEvaluacion = null;
            }

            if (PuedeActualizarInterfazEvaluacion()) {
                ReconstruirDetallePractica(
                    practica,
                    volverAlInicio: false);
            }

            if (!resultado.EsIntentoCalificable ||
                resultado.Resultado is null) {
                etapaVisualEvaluacion = 0;
                textoEstadoEvaluacion =
                    resultado.Estado ==
                        EstadoProcesoEvaluacionPractica.Cancelada
                    ? "Evaluación cancelada. No se registró ningún intento."
                    : resultado.Mensaje;

                if (PuedeActualizarInterfazEvaluacion()) {
                    ReconstruirVistaEvaluacion(
                        volverAlInicio: false);
                }

                return;
            }

            etapaVisualEvaluacion = 4;
            textoEstadoEvaluacion = "Guardando intento…";

            if (PuedeActualizarInterfazEvaluacion()) {
                ActualizarEstadoEvaluacionVisual();
            }

            IntentoPractica intento =
                CrearIntento(resultado.Resultado);
            ResultadoEscrituraHistorialEvaluaciones guardado;

            try {
                guardado = await Task.Run(() =>
                    historialEvaluacionesService.GuardarIntento(
                        intento));
            } catch (Exception ex) {
                guardado = new ResultadoEscrituraHistorialEvaluaciones {
                    Estado =
                        EstadoEscrituraHistorialEvaluaciones.ErrorIo,
                    Error = ex
                };
            }

            if (guardado.EsExitosa) {
                MarcarInicioPendienteDeRecarga();
            }

            textoEstadoEvaluacion = "Resultado generado.";

            if (!PuedeActualizarInterfazEvaluacion()) {
                return;
            }

            HistorialPractica? historial =
                guardado.HistorialActualizado;

            if (!guardado.EsExitosa) {
                MessageBox.Show(
                    ObtenerMensajeGuardadoHistorial(guardado.Estado),
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            MostrarResultadoEvaluacion(
                intento,
                historial,
                desdeHistorial: false);
        } finally {
            evaluacionEnCurso = false;
            cancelacionEvaluacion?.Dispose();
            cancelacionEvaluacion = null;

            if (PuedeActualizarInterfazEvaluacion()) {
                ActualizarEstadoEvaluacionVisual();
            }
        }
    }

    private bool PuedeActualizarInterfazEvaluacion() {
        return coordinadorCierreOperaciones
                .PuedeActualizarInterfaz &&
            !esperandoCierreOperaciones &&
            !IsDisposed &&
            !Disposing &&
            IsHandleCreated;
    }

    private void ActualizarProgresoEvaluacion(ProgresoEvaluacionPractica progreso) {
        if (!evaluacionEnCurso ||
            !PuedeActualizarInterfazEvaluacion()) {
            return;
        }

        etapaVisualEvaluacion = progreso.Etapa switch {
            EtapaEvaluacionPractica.Preparando => 1,
            EtapaEvaluacionPractica.Compilando => 2,
            EtapaEvaluacionPractica.EjecutandoPruebas => 3,
            EtapaEvaluacionPractica.GenerandoResultado => 4,
            _ => etapaVisualEvaluacion
        };
        textoEstadoEvaluacion = progreso.Mensaje;
        ActualizarEstadoEvaluacionVisual();
    }

    private void ActualizarEstadoEvaluacionVisual() {
        if (lblEstadoEvaluacion is not null && !lblEstadoEvaluacion.IsDisposed) {
            lblEstadoEvaluacion.Text = textoEstadoEvaluacion;
        }

        if (panelRellenoProgresoEvaluacion is not null &&
            panelFondoProgresoEvaluacion is not null &&
            !panelRellenoProgresoEvaluacion.IsDisposed) {
            panelRellenoProgresoEvaluacion.Width =
                CalcularAnchoProgresoEvaluacion(panelFondoProgresoEvaluacion.Width);
        }

        if (btnIniciarEvaluacion is not null && !btnIniciarEvaluacion.IsDisposed) {
            ConfigurarEstadoBotonEvaluacion(btnIniciarEvaluacion, !evaluacionEnCurso);
        }

        if (btnCancelarEvaluacion is not null && !btnCancelarEvaluacion.IsDisposed) {
            btnCancelarEvaluacion.Enabled =
                evaluacionEnCurso && cancelacionEvaluacion is not null;
        }

        if (btnVolverPracticaEvaluacion is not null && !btnVolverPracticaEvaluacion.IsDisposed) {
            btnVolverPracticaEvaluacion.Enabled = !evaluacionEnCurso;
        }
    }

    private int CalcularAnchoProgresoEvaluacion(int anchoDisponible) {
        return Math.Clamp(
            (int)Math.Round(anchoDisponible * Math.Clamp(etapaVisualEvaluacion, 0, 4) / 4D),
            0,
            Math.Max(0, anchoDisponible));
    }

    private void CancelarEvaluacionActiva() {
        if (!evaluacionEnCurso || cancelacionEvaluacion is null) {
            return;
        }

        textoEstadoEvaluacion = "Cancelando evaluación…";

        if (PuedeActualizarInterfazEvaluacion()) {
            ActualizarEstadoEvaluacionVisual();
        }

        cancelacionEvaluacion.Cancel();
    }

    private IntentoPractica CrearIntento(ResultadoEvaluacion resultado) {
        return new IntentoPractica {
            Id = Guid.NewGuid().ToString("N"),
            PracticaId = resultado.PracticaId,
            Fecha = resultado.Fecha,
            Calificacion = resultado.Calificacion,
            Compilo = resultado.Compilo,
            PruebasSuperadas = resultado.PruebasSuperadas,
            PruebasTotales = resultado.PruebasTotales,
            ResultadoGeneral = ObtenerEstadoCalificacion(resultado.Calificacion),
            EjecucionFinalizada = resultado.EjecucionFinalizada,
            PuntosObtenidos = resultado.PuntosObtenidos,
            PuntosMaximos = resultado.PuntosMaximos,
            RutaProyecto = resultado.RutaProyecto,
            Resultados = resultado.Resultados.ToArray(),
            Retroalimentacion = resultado.Retroalimentacion.ToArray()
        };
    }

    private static string ObtenerMensajeGuardadoHistorial(
        EstadoEscrituraHistorialEvaluaciones estado) {
        return estado switch {
            EstadoEscrituraHistorialEvaluaciones.PermisosInsuficientes =>
                "La evaluación terminó, pero no hay permisos para guardar el intento. La práctica y EndForge continúan abiertos.",
            EstadoEscrituraHistorialEvaluaciones.ContenidoIrrecuperable =>
                "La evaluación terminó, pero evaluaciones.json está dañado. El archivo anterior se conservó y el resultado sigue visible.",
            EstadoEscrituraHistorialEvaluaciones.VersionNoCompatible =>
                "La evaluación terminó, pero evaluaciones.json usa una versión no compatible. El archivo anterior se conservó.",
            _ =>
                "La evaluación terminó, pero no se pudo guardar el intento porque evaluaciones.json está bloqueado o no está disponible."
        };
    }

    private void VolverADetallePracticaDesdeEvaluacion() {
        if (evaluacionEnCurso) {
            CancelarEvaluacionActiva();
            return;
        }

        if (practicaCursoSeleccionada is not null) {
            evaluacionInmersivaAmpliaActiva = false;
            MostrarDetallePractica(practicaCursoSeleccionada);
        }
    }

    private IEnumerable<Panel> ObtenerVistasEvaluacionInicializadas() {
        if (!vistasEvaluacionInicializadas) {
            yield break;
        }

        yield return panelEvaluacionVista;
        yield return panelResultadoEvaluacionVista;
        yield return panelHistorialEvaluacionesVista;
    }

    private VistaRutaAprendizaje ObtenerEstadoVistaEvaluacion(Panel vista) {
        if (!vistasEvaluacionInicializadas) {
            return VistaRutaAprendizaje.Ninguna;
        }

        if (ReferenceEquals(vista, panelEvaluacionVista)) {
            return VistaRutaAprendizaje.Evaluacion;
        }

        if (ReferenceEquals(vista, panelResultadoEvaluacionVista)) {
            return VistaRutaAprendizaje.Resultado;
        }

        if (ReferenceEquals(vista, panelHistorialEvaluacionesVista)) {
            return VistaRutaAprendizaje.Historial;
        }

        return VistaRutaAprendizaje.Ninguna;
    }

    private bool NavegarAtrasEvaluacion(VistaRutaAprendizaje vista) {
        if (actualizandoHistorial) {
            return true;
        }

        if (vista == VistaRutaAprendizaje.Evaluacion) {
            VolverADetallePracticaDesdeEvaluacion();
            return true;
        }

        if (vista == VistaRutaAprendizaje.Resultado) {
            if (resultadoMostradoDesdeHistorial) {
                MostrarHistorialEvaluaciones(desdeResultado: false);
            } else {
                VolverADetallePracticaDesdeEvaluacion();
            }
            return true;
        }

        if (vista == VistaRutaAprendizaje.Historial) {
            VolverDesdeHistorial();
            return true;
        }

        return false;
    }

    private void RecalcularDistribucionEvaluacion() {
        if (!vistasEvaluacionInicializadas) {
            return;
        }

        if (panelEvaluacionVista.Visible) {
            int ancho = AjustarColumnaCentralEvaluacion();

            if (Math.Abs(ancho - ultimoAnchoEvaluacion) >= EscalarDiseno(8)) {
                ReconstruirVistaEvaluacion(volverAlInicio: false);
            } else {
                desplazamientoEvaluacion.ActualizarContenido(false);
            }
        }

        RecalcularDistribucionResultados();
    }

    private async void FrmPrincipal_OperacionesFormClosing(
        object? sender,
        FormClosingEventArgs e) {
        CancelarCargaInicioAlCerrar();
        Task? evaluacionPendiente =
            tareaEvaluacionActiva is { IsCompleted: false }
                ? tareaEvaluacionActiva
                : null;
        Task? creacionPendiente =
            creacionPracticaEnCurso &&
            tareaCreacionPracticaActiva is { IsCompleted: false }
                ? tareaCreacionPracticaActiva
                : null;
        DecisionCierreOperacionesAsync decision =
            coordinadorCierreOperaciones.SolicitarCierre(
                evaluacionPendiente,
                creacionPendiente);

        if (cierreTrasOperacionesAutorizado ||
            decision.PermitirCierre) {
            return;
        }

        e.Cancel = true;

        if (!decision.DebeEsperar ||
            decision.FinalizacionPendiente is null) {
            return;
        }

        esperandoCierreOperaciones = true;

        if (evaluacionPendiente is not null) {
            CancelarEvaluacionActiva();
        }

        try {
            await decision.FinalizacionPendiente;
        } catch (Exception) {
            // El cierre solo espera la limpieza de los procesos iniciados por EndForge.
        } finally {
            esperandoCierreOperaciones = false;

            if (coordinadorCierreOperaciones
                    .IntentarAutorizarReintento()) {
                cierreTrasOperacionesAutorizado = true;

                if (!IsDisposed &&
                    !Disposing &&
                    IsHandleCreated) {
                    BeginInvoke(Close);
                }
            }
        }
    }

    private void CancelarEvaluacionAlCerrar() {
        cancelacionEvaluacion?.Cancel();
        cancelacionEvaluacion?.Dispose();
        cancelacionEvaluacion = null;
    }
}
