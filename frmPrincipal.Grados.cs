using EndForge.Controls;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private enum VistaRutaAprendizaje {
        Ninguna,
        Grados,
        DetalleGrado,
        PracticasTema,
        DetallePractica,
        Evaluacion,
        Resultado,
        Historial
    }

    private GradosService gradosService = null!;
    private VistaRutaAprendizaje vistaRutaActual;
    private Panel panelGradosVista = null!;
    private PanelDesplazableSinBarras desplazamientoGrados = null!;
    private FlowLayoutPanel contenidoGrados = null!;
    private Button btnVolverGradosCurso = null!;
    private int ultimoAnchoEfectivoGrados = -1;
    private int ultimoMargenIzquierdoGrados = -1;
    private int ultimoDpiContenidoGrados = -1;
    private bool contenidoGradosConstruido;
    private EstadoContenidoGrado[] estadoContenidoGrados = Array.Empty<EstadoContenidoGrado>();

    private readonly record struct EstadoContenidoGrado(
        string Id,
        int Numero,
        string Nombre,
        string Descripcion,
        EstadoGradoCurso Estado,
        bool EsContenidoDisponible,
        int Porcentaje,
        int CantidadPracticasDisponibles,
        int CantidadPracticasCompletadas,
        int CantidadPracticasPlaneadas,
        int CantidadTemas,
        int CantidadTemasDisponibles);

    private sealed record PresentacionTarjetaGrado(
        bool Disponible,
        int Porcentaje,
        Label Numero,
        Label Nombre,
        Label Descripcion,
        Label Estado,
        Label? Resumen,
        Label? PorcentajeTexto,
        Panel? FondoProgreso,
        Panel? RellenoProgreso,
        Button? Boton);

    private void ConstruirVistaGrados() {
        desplazamientoGrados = CrearPanelDesplazableCurso(
            "desplazamientoGrados",
            "contenidoGrados",
            Point.Empty,
            panelGradosVista.ClientSize,
            new Padding(22, 18, 22, 24));
        desplazamientoGrados.Dock = DockStyle.None;
        desplazamientoGrados.BackColor = Color.FromArgb(18, 14, 27);
        desplazamientoGrados.Padding = Padding.Empty;
        desplazamientoGrados.MostrarBordeFoco = false;
        desplazamientoGrados.TabIndex = 0;
        contenidoGrados = desplazamientoGrados.Contenido;
        panelGradosVista.Controls.Add(desplazamientoGrados);
    }

    private void MostrarGrados() {
        if (!cursoPreparado || navegacionCursoEnCurso) {
            return;
        }

        bool destinoYaVisible =
            panelGradosVista.Visible &&
            vistaRutaActual == VistaRutaAprendizaje.Grados &&
            rutaAprendizajeInmersivaActiva;

        if (destinoYaVisible || !IniciarTransicionVisualCurso(panelGradosVista)) {
            return;
        }

        RestablecerEstadosVisualesVistaActualCurso();
        PrepararDestinoDespuesDeCubiertaTransicionCurso(
            PrepararYMostrarGrados);
    }

    private void PrepararYMostrarGrados() {
        panelPrincipal.SuspendLayout();

        try {
            MostrarRutaAprendizajeInmersiva(
                reconstruirContenido: true,
                invalidarFondo: false,
                prepararContenidoAntesDeMostrar: true);
            AjustarCubiertaTransicionCurso();
            RestablecerEstadosVisualesTarjetasGrados();
        } catch {
            CancelarTransicionVisualCurso();
            throw;
        } finally {
            panelPrincipal.ResumeLayout(performLayout: false);
        }

        fondoEndForge.Invalidate();
        ConfirmarDestinoTransicionCurso(panelGradosVista);
    }

    private void AbrirGrado(GradoCurso grado) {
        if (!grado.EsContenidoDisponible) {
            return;
        }

        gradoSeleccionadoEnSesion = true;
        MostrarCursoPrincipal();
    }

    private void ReconstruirVistaGrados(bool volverAlInicio) {
        if (!cursoPreparado || contenidoGrados is null || contenidoGrados.IsDisposed) {
            return;
        }

        int anchoAreaDisponible = Math.Max(
            1,
            contenidoGrados.ClientSize.Width - contenidoGrados.Padding.Horizontal);
        bool usarDistribucionAmplia =
            rutaAprendizajeInmersivaActiva &&
            WindowState == FormWindowState.Maximized;
        int anchoContenido = Math.Min(
            EscalarDiseno(usarDistribucionAmplia ? 1500 : 1120),
            anchoAreaDisponible);
        int margenIzquierdo = usarDistribucionAmplia
            ? 0
            : Math.Max(0, (anchoAreaDisponible - anchoContenido) / 2);
        IReadOnlyList<GradoCurso> grados = gradosService
            .CargarGrados(progresoCurso)
            .OrderBy(item => item.Numero)
            .ToArray();
        EstadoContenidoGrado[] estadoActual = CapturarEstadoContenidoGrados(grados);
        bool controlesVigentes =
            contenidoGradosConstruido &&
            contenidoGrados.Controls.Count == grados.Count + 2 &&
            contenidoGrados.Controls.OfType<Panel>().All(tarjeta =>
                tarjeta.Tag is PresentacionTarjetaGrado);
        bool datosVigentes =
            controlesVigentes &&
            estadoContenidoGrados.SequenceEqual(estadoActual);

        if (datosVigentes) {
            bool cambioGeometria =
                ultimoAnchoEfectivoGrados != anchoContenido ||
                ultimoMargenIzquierdoGrados != margenIzquierdo ||
                ultimoDpiContenidoGrados != DeviceDpi;

            if (cambioGeometria) {
                ActualizarGeometriaContenidoGrados(anchoContenido, margenIzquierdo);
            }

            ultimoAnchoEfectivoGrados = anchoContenido;
            ultimoMargenIzquierdoGrados = margenIzquierdo;
            ultimoDpiContenidoGrados = DeviceDpi;
            desplazamientoGrados.ActualizarContenido(volverAlInicio);
            return;
        }

        contenidoGrados.SuspendLayout();

        try {
            VaciarYDisponerControles(contenidoGrados);
            Label titulo = CrearLabelCurso(
                "Ruta de aprendizaje",
                Point.Empty,
                new Size(anchoContenido, EscalarDiseno(42)),
                TamanoFuenteCurso.TituloPrincipal,
                FontStyle.Bold,
                Color.White);
            titulo.Margin = new Padding(margenIzquierdo, 0, 0, EscalarDiseno(2));
            contenidoGrados.Controls.Add(titulo);

            Label subtitulo = CrearLabelCurso(
                "Avanza a tu propio ritmo y construye tus habilidades paso a paso.",
                Point.Empty,
                new Size(anchoContenido, EscalarDiseno(38)),
                TamanoFuenteCurso.DescripcionCabecera,
                FontStyle.Regular,
                ColorTextoSecundarioCurso);
            subtitulo.Margin = new Padding(margenIzquierdo, 0, 0, EscalarDiseno(14));
            contenidoGrados.Controls.Add(subtitulo);

            foreach (GradoCurso grado in grados) {
                Panel tarjeta = CrearTarjetaGrado(grado, anchoContenido);
                tarjeta.Margin = new Padding(
                    margenIzquierdo,
                    0,
                    0,
                    EscalarDiseno(14));
                contenidoGrados.Controls.Add(tarjeta);
            }
        } finally {
            contenidoGrados.ResumeLayout(performLayout: true);
            desplazamientoGrados.ActualizarContenido(volverAlInicio);
        }

        contenidoGradosConstruido = true;
        estadoContenidoGrados = estadoActual;
        ultimoAnchoEfectivoGrados = anchoContenido;
        ultimoMargenIzquierdoGrados = margenIzquierdo;
        ultimoDpiContenidoGrados = DeviceDpi;
    }

    private void AsegurarVistaGradosVigente(bool volverAlInicio) {
        ReconstruirVistaGrados(volverAlInicio);
    }

    private static EstadoContenidoGrado[] CapturarEstadoContenidoGrados(
        IReadOnlyList<GradoCurso> grados) {
        return grados
            .Select(grado => new EstadoContenidoGrado(
                grado.Id,
                grado.Numero,
                grado.Nombre,
                grado.Descripcion,
                grado.Estado,
                grado.EsContenidoDisponible,
                grado.Porcentaje,
                grado.CantidadPracticasDisponibles,
                grado.CantidadPracticasCompletadas,
                grado.CantidadPracticasPlaneadas,
                grado.Temas.Count,
                grado.Temas.Count(tema =>
                    !tema.EsProximamente && tema.Practicas.Count > 0)))
            .ToArray();
    }

    private void ActualizarGeometriaContenidoGrados(
        int anchoContenido,
        int margenIzquierdo) {
        contenidoGrados.SuspendLayout();

        try {
            if (contenidoGrados.Controls.Count >= 2) {
                Control titulo = contenidoGrados.Controls[0];
                titulo.Size = new Size(anchoContenido, EscalarDiseno(42));
                titulo.Margin = new Padding(
                    margenIzquierdo,
                    0,
                    0,
                    EscalarDiseno(2));

                Control subtitulo = contenidoGrados.Controls[1];
                subtitulo.Size = new Size(anchoContenido, EscalarDiseno(38));
                subtitulo.Margin = new Padding(
                    margenIzquierdo,
                    0,
                    0,
                    EscalarDiseno(14));
            }

            foreach (Panel tarjeta in contenidoGrados.Controls.OfType<Panel>()) {
                if (tarjeta.Tag is not PresentacionTarjetaGrado presentacion) {
                    continue;
                }

                ActualizarGeometriaTarjetaGrado(tarjeta, presentacion, anchoContenido);
                tarjeta.Margin = new Padding(
                    margenIzquierdo,
                    0,
                    0,
                    EscalarDiseno(14));
            }
        } finally {
            contenidoGrados.ResumeLayout(performLayout: true);
        }
    }

    private Panel CrearTarjetaGrado(GradoCurso grado, int ancho) {
        bool disponible = grado.EsContenidoDisponible;
        int margen = EscalarDiseno(20);
        bool apilarAccion = disponible && ancho < EscalarDiseno(560);
        int alto = disponible
            ? apilarAccion
                ? EscalarDiseno(286)
                : EscalarDiseno(238)
            : EscalarDiseno(152);
        Panel tarjeta = CrearTarjetaCurso(
            Point.Empty,
            new Size(ancho, alto),
            16,
            interactiva: disponible);
        tarjeta.Margin = new Padding(0, 0, 0, EscalarDiseno(14));
        tarjeta.TabIndex = Math.Max(0, grado.Numero - 1);
        tarjeta.TabStop = disponible;
        tarjeta.AccessibleName = grado.Titulo;
        tarjeta.AccessibleDescription = disponible
            ? $"{ObtenerTextoEstadoGrado(grado.Estado)}. " +
              $"{grado.CantidadPracticasCompletadas} de {grado.CantidadPracticasDisponibles} prácticas disponibles realizadas. " +
              $"{grado.CantidadPracticasDisponibles} de {grado.CantidadPracticasPlaneadas} prácticas publicadas. " +
              $"{grado.Porcentaje} por ciento de progreso disponible."
            : "Contenido próximamente.";

        Label numero = CrearLabelCurso(
            $"GRADO {grado.Numero:00}",
            new Point(margen, EscalarDiseno(15)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(22)),
            TamanoFuenteCurso.NumeroCabecera,
            FontStyle.Bold,
            disponible ? ColorMoradoClaroCurso : Color.FromArgb(139, 132, 151));
        Label nombre = CrearLabelCurso(
            grado.Nombre,
            new Point(margen, EscalarDiseno(38)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(34)),
            TamanoFuenteCurso.TituloSeccion,
            FontStyle.Bold,
            disponible ? Color.White : Color.FromArgb(185, 180, 193));
        Label descripcion = CrearLabelCurso(
            grado.Descripcion,
            new Point(margen, EscalarDiseno(73)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(44)),
            TamanoFuenteCurso.DescripcionCabecera,
            FontStyle.Regular,
            disponible ? ColorTextoSecundarioCurso : Color.FromArgb(145, 139, 153));
        Label estado = CrearLabelCurso(
            ObtenerTextoEstadoGrado(grado.Estado),
            new Point(margen, disponible ? EscalarDiseno(121) : EscalarDiseno(120)),
            new Size(Math.Max(1, ancho - margen * 2), EscalarDiseno(24)),
            TamanoFuenteCurso.MetadatoTarjetaTema,
            FontStyle.Bold,
            disponible ? ColorMoradoClaroCurso : Color.FromArgb(157, 148, 169));

        tarjeta.Controls.Add(numero);
        tarjeta.Controls.Add(nombre);
        tarjeta.Controls.Add(descripcion);
        tarjeta.Controls.Add(estado);

        Label? resumen = null;
        Label? porcentajeTexto = null;
        Panel? fondoProgreso = null;
        Panel? rellenoProgreso = null;
        Button? boton = null;

        if (disponible) {
            int anchoBoton = apilarAccion
                ? Math.Max(1, ancho - margen * 2)
                : EscalarDiseno(172);
            int anchoMetricas = apilarAccion
                ? Math.Max(1, ancho - margen * 2)
                : Math.Max(1, ancho - margen * 2 - anchoBoton - EscalarDiseno(16));
            resumen = CrearLabelCurso(
                $"{grado.Temas.Count(tema => !tema.EsProximamente && tema.Practicas.Count > 0)} temas disponibles\n" +
                $"{grado.CantidadPracticasDisponibles} de {grado.CantidadPracticasPlaneadas} prácticas publicadas",
                new Point(margen, EscalarDiseno(145)),
                new Size(anchoMetricas, EscalarDiseno(42)),
                TamanoFuenteCurso.MetadatoTarjetaTema,
                FontStyle.Regular,
                ColorTextoSecundarioCurso,
                ContentAlignment.TopLeft);
            porcentajeTexto = CrearLabelCurso(
                $"{grado.Porcentaje}% de progreso disponible",
                new Point(margen, EscalarDiseno(187)),
                new Size(anchoMetricas, EscalarDiseno(22)),
                TamanoFuenteCurso.ProgresoTarjetaTema,
                FontStyle.Bold,
                ColorMoradoClaroCurso);
            int anchoBarra = anchoMetricas;
            fondoProgreso = new Panel {
                BackColor = Color.FromArgb(55, 45, 70),
                Location = new Point(margen, EscalarDiseno(211)),
                Size = new Size(anchoBarra, EscalarDiseno(9))
            };
            rellenoProgreso = new Panel {
                BackColor = ColorMoradoCurso,
                Location = Point.Empty,
                Size = new Size(
                    (int)Math.Round(anchoBarra * grado.Porcentaje / 100D),
                    fondoProgreso.Height)
            };
            fondoProgreso.Controls.Add(rellenoProgreso);
            int altoBoton = EscalarDiseno(36);
            int yBoton = CalcularTopBotonTarjetaGrado(
                tarjeta,
                fondoProgreso,
                altoBoton,
                apilarAccion);
            boton = CrearBotonCurso(
                grado.Estado == EstadoGradoCurso.EnProgreso
                    ? "Continuar grado"
                    : "Ver grado",
                apilarAccion
                    ? new Point(margen, yBoton)
                    : new Point(
                        Math.Max(margen, ancho - margen - anchoBoton),
                        yBoton),
                new Size(anchoBoton, altoBoton),
                ColorMoradoCurso);
            boton.TabStop = false;
            boton.AccessibleDescription = $"Abre {grado.Titulo}.";

            tarjeta.Controls.Add(resumen);
            tarjeta.Controls.Add(porcentajeTexto);
            tarjeta.Controls.Add(fondoProgreso);
            tarjeta.Controls.Add(boton);
            ConfigurarInteraccionTarjeta(tarjeta, () => AbrirGrado(grado));
        }

        tarjeta.Tag = new PresentacionTarjetaGrado(
            disponible,
            grado.Porcentaje,
            numero,
            nombre,
            descripcion,
            estado,
            resumen,
            porcentajeTexto,
            fondoProgreso,
            rellenoProgreso,
            boton);

        return tarjeta;
    }

    private void ActualizarGeometriaTarjetaGrado(
        Panel tarjeta,
        PresentacionTarjetaGrado presentacion,
        int ancho) {
        int margen = EscalarDiseno(20);
        bool apilarAccion =
            presentacion.Disponible && ancho < EscalarDiseno(560);
        int alto = presentacion.Disponible
            ? apilarAccion
                ? EscalarDiseno(286)
                : EscalarDiseno(238)
            : EscalarDiseno(152);
        int anchoInterior = Math.Max(1, ancho - margen * 2);

        tarjeta.Size = new Size(ancho, alto);
        presentacion.Numero.SetBounds(
            margen,
            EscalarDiseno(15),
            anchoInterior,
            EscalarDiseno(22));
        presentacion.Nombre.SetBounds(
            margen,
            EscalarDiseno(38),
            anchoInterior,
            EscalarDiseno(34));
        presentacion.Descripcion.SetBounds(
            margen,
            EscalarDiseno(73),
            anchoInterior,
            EscalarDiseno(44));
        presentacion.Estado.SetBounds(
            margen,
            presentacion.Disponible
                ? EscalarDiseno(121)
                : EscalarDiseno(120),
            anchoInterior,
            EscalarDiseno(24));

        if (!presentacion.Disponible ||
            presentacion.Resumen is null ||
            presentacion.PorcentajeTexto is null ||
            presentacion.FondoProgreso is null ||
            presentacion.RellenoProgreso is null ||
            presentacion.Boton is null) {
            return;
        }

        int anchoBoton = apilarAccion
            ? anchoInterior
            : EscalarDiseno(172);
        int anchoMetricas = apilarAccion
            ? anchoInterior
            : Math.Max(1, anchoInterior - anchoBoton - EscalarDiseno(16));
        presentacion.Resumen.SetBounds(
            margen,
            EscalarDiseno(145),
            anchoMetricas,
            EscalarDiseno(42));
        presentacion.PorcentajeTexto.SetBounds(
            margen,
            EscalarDiseno(187),
            anchoMetricas,
            EscalarDiseno(22));
        presentacion.FondoProgreso.SetBounds(
            margen,
            EscalarDiseno(211),
            anchoMetricas,
            EscalarDiseno(9));
        presentacion.RellenoProgreso.SetBounds(
            0,
            0,
            (int)Math.Round(anchoMetricas * presentacion.Porcentaje / 100D),
            presentacion.FondoProgreso.Height);
        int altoBoton = EscalarDiseno(36);
        int yBoton = CalcularTopBotonTarjetaGrado(
            tarjeta,
            presentacion.FondoProgreso,
            altoBoton,
            apilarAccion);
        presentacion.Boton.SetBounds(
            apilarAccion
                ? margen
                : Math.Max(margen, ancho - margen - anchoBoton),
            yBoton,
            anchoBoton,
            altoBoton);
    }

    private int CalcularTopBotonTarjetaGrado(
        Panel tarjeta,
        Panel fondoProgreso,
        int altoBoton,
        bool apilarAccion) {
        int topDeseado = apilarAccion
            ? fondoProgreso.Bottom + EscalarDiseno(15)
            : fondoProgreso.Top +
                (int)Math.Round((fondoProgreso.Height - altoBoton) / 2D) -
                EscalarDiseno(6);
        int margenInferiorSeguro = EscalarDiseno(10);
        int topMaximo = Math.Max(
            0,
            tarjeta.ClientSize.Height - margenInferiorSeguro - altoBoton);

        return Math.Clamp(topDeseado, 0, topMaximo);
    }

    private static string ObtenerTextoEstadoGrado(EstadoGradoCurso estado) {
        return estado switch {
            EstadoGradoCurso.EnProgreso => "EN PROGRESO",
            EstadoGradoCurso.ContenidoDisponibleCompletado =>
                "CONTENIDO DISPONIBLE COMPLETADO",
            EstadoGradoCurso.Completado => "COMPLETADO",
            EstadoGradoCurso.Proximamente => "PRÓXIMAMENTE",
            _ => "DISPONIBLE"
        };
    }

    private IEnumerable<Panel> ObtenerVistasRutaAprendizaje() {
        if (!cursoInicializado) {
            yield break;
        }

        yield return panelGradosVista;
        yield return panelCursoVista;
        yield return panelPracticasTemaVista;
        yield return panelDetallePracticaVista;

        foreach (Panel vista in ObtenerVistasEvaluacionInicializadas()) {
            yield return vista;
        }
    }

    private VistaRutaAprendizaje ObtenerEstadoVista(Panel vista) {
        if (ReferenceEquals(vista, panelGradosVista)) {
            return VistaRutaAprendizaje.Grados;
        }

        if (ReferenceEquals(vista, panelCursoVista)) {
            return VistaRutaAprendizaje.DetalleGrado;
        }

        if (ReferenceEquals(vista, panelPracticasTemaVista)) {
            return VistaRutaAprendizaje.PracticasTema;
        }

        if (ReferenceEquals(vista, panelDetallePracticaVista)) {
            return VistaRutaAprendizaje.DetallePractica;
        }

        return ObtenerEstadoVistaEvaluacion(vista);
    }

    private bool NavegarAtrasRuta() {
        switch (vistaRutaActual) {
            case VistaRutaAprendizaje.DetalleGrado:
                MostrarGrados();
                return true;
            case VistaRutaAprendizaje.PracticasTema:
                MostrarCursoPrincipal();
                return true;
            case VistaRutaAprendizaje.DetallePractica:
                if (temaCursoSeleccionado is not null) {
                    MostrarPracticasTema(temaCursoSeleccionado);
                } else {
                    MostrarCursoPrincipal();
                }
                return true;
            case VistaRutaAprendizaje.Evaluacion:
            case VistaRutaAprendizaje.Resultado:
            case VistaRutaAprendizaje.Historial:
                return NavegarAtrasEvaluacion(vistaRutaActual);
            default:
                return false;
        }
    }

    private void RecalcularDistribucionGrados() {
        if (!cursoInicializado || desplazamientoGrados is null) {
            return;
        }

        int anchoVista = Math.Max(1, panelGradosVista.ClientSize.Width);
        int altoVista = Math.Max(1, panelGradosVista.ClientSize.Height);
        bool usarDistribucionAmplia =
            rutaAprendizajeInmersivaActiva &&
            WindowState == FormWindowState.Maximized;
        int margenHorizontal = usarDistribucionAmplia
            ? EscalarDiseno(24)
            : 0;
        bool reservarZonaDecorativa =
            !usarDistribucionAmplia &&
            anchoVista >= EscalarDiseno(1200);
        int anchoDesplazamiento = usarDistribucionAmplia
            ? Math.Min(
                EscalarDiseno(1500),
                Math.Max(1, anchoVista - margenHorizontal * 2))
            : reservarZonaDecorativa
                ? Math.Max(1, (int)Math.Round(anchoVista * 0.72D))
                : anchoVista;
        desplazamientoGrados.SetBounds(
            margenHorizontal,
            0,
            anchoDesplazamiento,
            altoVista);

        if (!panelGradosVista.Visible) {
            return;
        }

        AsegurarVistaGradosVigente(volverAlInicio: false);
    }
}
