using System.Diagnostics;
using EndForge.Models;
using EndForge.Services;

namespace EndForge;

public partial class frmPrincipal {
    private enum EstadoVisualBienvenida {
        EsperandoEntrada,
        Preparando,
        Desvaneciendo,
        Finalizada
    }

    private const int DuracionDesvanecimientoBienvenidaMs = 210;

    private sealed class DatosPrecargaInicio {
        public required ResultadoCargaConfiguracion Configuracion { get; init; }
        public required ResultadoLecturaRecientes Recientes { get; init; }
        public required ResultadoCargaProgreso Progreso { get; init; }
        public required CursoService Curso { get; init; }
        public required ProgresoCursoService ServicioProgreso { get; init; }
        public required IReadOnlyList<string> Temas { get; init; }
    }

    private Panel panelPantallaBienvenida = null!;
    private Panel panelContenidoBienvenida = null!;
    private Label lblLogoBienvenida = null!;
    private Label lblSubtituloBienvenida = null!;
    private Label lblContinuarBienvenida = null!;
    private System.Windows.Forms.Timer? timerBienvenida;
    private bool entradaAplicacionRealizada;
    private bool transicionandoDesdeBienvenida;
    private bool inicializacionSecundariaProgramada;
    private bool inicializacionSecundariaEnCurso;
    private bool inicializacionSecundariaCompletada;
    private bool inicializacionSecundariaCancelada;
    private bool precargaDatosIniciada;
    private volatile bool precargaDatosCompletada;
    private CancellationTokenSource? cancelacionPrecargaDatos;
    private Task<DatosPrecargaInicio>? tareaPrecargaDatos;
    private Task? tareaConstruccionCursoDuranteBienvenida;
    private TaskCompletionSource<bool>? finalizacionTransicionBienvenida;
    private DatosPrecargaInicio? datosPrecargadosInicio;
    private int intensidadContinuar = 120;
    private int direccionAnimacionBienvenida = 1;
    private int ticksPreparacionBienvenida;
    private long marcaInicioDesvanecimientoBienvenida;
    private Color colorContinuarAntesDesvanecimiento;
    private EstadoVisualBienvenida estadoVisualBienvenida;
    private bool inicioPintadoParaTransicion;
#if DEBUG
    private readonly Stopwatch cronometroInicio = Stopwatch.StartNew();
    private long marcaEntradaTransicionBienvenida;
    private long marcaInicioListoTransicionBienvenida;
    private int paintsBienvenidaDuranteTransicion;
    private int paintsInicioDuranteTransicion;
    private int recalculosGeometriaDuranteTransicion;
    private int cambiosVisibleDuranteTransicion;
#endif

    private void InicializarBienvenida() {
        panelPantallaBienvenida = new Panel {
            Name = "panelPantallaBienvenida",
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.FromArgb(20, 16, 30),
            BackgroundImage = fondoEndForge.ImagenFondo,
            BackgroundImageLayout = ImageLayout.Stretch,
            Cursor = Cursors.Hand,
            Location = Point.Empty,
            Size = ClientSize
        };

        panelContenidoBienvenida = new Panel {
            Name = "panelContenidoBienvenida",
            BackColor = Color.Transparent,
            Size = new Size(850, 250),
            Cursor = Cursors.Hand
        };

        lblLogoBienvenida = new Label {
            Name = "lblLogoBienvenida",
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 38F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(0, 5),
            Size = new Size(850, 105),
            Text = "EndForge",
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        lblSubtituloBienvenida = new Label {
            Name = "lblSubtituloBienvenida",
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 14F),
            ForeColor = Color.FromArgb(214, 200, 232),
            Location = new Point(0, 110),
            Size = new Size(850, 42),
            Text = "Tu entorno para aprender y crear en C++",
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        lblContinuarBienvenida = new Label {
            Name = "lblContinuarBienvenida",
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI Semibold", 10.5F),
            ForeColor = Color.FromArgb(intensidadContinuar, intensidadContinuar, intensidadContinuar),
            Location = new Point(0, 185),
            Size = new Size(850, 38),
            Text = "Presiona Enter o haz clic para comenzar",
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        panelContenidoBienvenida.Controls.Add(lblLogoBienvenida);
        panelContenidoBienvenida.Controls.Add(lblSubtituloBienvenida);
        panelContenidoBienvenida.Controls.Add(lblContinuarBienvenida);
        panelPantallaBienvenida.Controls.Add(panelContenidoBienvenida);

        panelPantallaBienvenida.Click += PantallaBienvenida_Click;
        panelPantallaBienvenida.Paint += PanelPantallaBienvenida_PrimerPaint;
#if DEBUG
        panelPantallaBienvenida.Paint += PanelPantallaBienvenida_ContarPaint;
        panelInicioVista.Paint += PanelInicioVista_ContarPaint;
#endif
        panelContenidoBienvenida.Click += PantallaBienvenida_Click;
        lblLogoBienvenida.Click += PantallaBienvenida_Click;
        lblSubtituloBienvenida.Click += PantallaBienvenida_Click;
        lblContinuarBienvenida.Click += PantallaBienvenida_Click;
        panelBarraTitulo.Click += PantallaBienvenida_Click;
        lblBarraTitulo.Click += PantallaBienvenida_Click;
        pictureBoxBarraIcono.Click += PantallaBienvenida_Click;

        KeyPreview = true;
        KeyDown += FrmPrincipal_BienvenidaKeyDown;
        FormClosed += FrmPrincipal_BienvenidaFormClosed;

        Controls.Add(panelPantallaBienvenida);
        ActivarDobleBuffer(panelPantallaBienvenida);
        ActivarDobleBuffer(panelContenidoBienvenida);
        panelPantallaBienvenida.BringToFront();
        panelBarraTitulo.BringToFront();

        timerBienvenida = new System.Windows.Forms.Timer {
            Interval = 45
        };
        timerBienvenida.Tick += TimerBienvenida_Tick;
        timerBienvenida.Start();

        CentrarContenidoBienvenida();
        MostrarPantallaBienvenida();
        RegistrarTiempoInicio("Bienvenida configurada");
    }

    private void MostrarPantallaBienvenida() {
        panelMenu.Visible = false;
        panelPrincipal.Visible = false;
        panelPantallaBienvenida.Visible = true;
        panelPantallaBienvenida.BringToFront();
        panelBarraTitulo.BringToFront();
        CentrarContenidoBienvenida();
    }

    private void CentrarContenidoBienvenida() {
        if (panelPantallaBienvenida.ClientSize.Width <= 0 ||
            panelPantallaBienvenida.ClientSize.Height <= 0) {
            return;
        }

        AjustarDisenoBienvenida();

        int x = Math.Max(0, (panelPantallaBienvenida.ClientSize.Width - panelContenidoBienvenida.Width) / 2);
        int y = Math.Max(
            panelBarraTitulo.Height,
            (panelPantallaBienvenida.ClientSize.Height - panelContenidoBienvenida.Height) / 2
        );

        panelContenidoBienvenida.Location = new Point(x, y);
    }

    private void AjustarDisenoBienvenida() {
        const int anchoMaximo = 850;
        const int margenHorizontal = 40;
        int anchoDisponible = Math.Max(
            1,
            panelPantallaBienvenida.ClientSize.Width - margenHorizontal * 2);
        int anchoContenido = Math.Min(anchoMaximo, anchoDisponible);

        Size medidaLogo = TextRenderer.MeasureText(
            lblLogoBienvenida.Text,
            lblLogoBienvenida.Font,
            new Size(anchoContenido, int.MaxValue),
            TextFormatFlags.SingleLine);
        int altoLogo = Math.Max(96, medidaLogo.Height + 24);

        Size medidaSubtitulo = TextRenderer.MeasureText(
            lblSubtituloBienvenida.Text,
            lblSubtituloBienvenida.Font,
            new Size(anchoContenido, int.MaxValue),
            TextFormatFlags.SingleLine);
        int altoSubtitulo = Math.Max(42, medidaSubtitulo.Height + 10);

        lblLogoBienvenida.SetBounds(0, 0, anchoContenido, altoLogo);
        lblSubtituloBienvenida.SetBounds(
            0,
            lblLogoBienvenida.Bottom + 8,
            anchoContenido,
            altoSubtitulo);
        lblContinuarBienvenida.SetBounds(
            0,
            lblSubtituloBienvenida.Bottom + 34,
            anchoContenido,
            38);
        panelContenidoBienvenida.Size = new Size(
            anchoContenido,
            lblContinuarBienvenida.Bottom + 8);
    }

    private void TimerBienvenida_Tick(object? sender, EventArgs e) {
        if (estadoVisualBienvenida == EstadoVisualBienvenida.Preparando) {
            AnimarPreparacionBienvenida();
            return;
        }

        if (estadoVisualBienvenida == EstadoVisualBienvenida.Desvaneciendo) {
            ActualizarDesvanecimientoBienvenida();
            return;
        }

        if (estadoVisualBienvenida != EstadoVisualBienvenida.EsperandoEntrada) {
            return;
        }

        intensidadContinuar += direccionAnimacionBienvenida * 5;

        if (intensidadContinuar >= 235) {
            intensidadContinuar = 235;
            direccionAnimacionBienvenida = -1;
        } else if (intensidadContinuar <= 120) {
            intensidadContinuar = 120;
            direccionAnimacionBienvenida = 1;
        }

        int rojo = intensidadContinuar;
        int verde = Math.Max(105, intensidadContinuar - 10);
        int azul = Math.Min(255, intensidadContinuar + 20);
        lblContinuarBienvenida.ForeColor = Color.FromArgb(rojo, verde, azul);
    }

    private void IniciarEstadoPreparacionBienvenida() {
        estadoVisualBienvenida = EstadoVisualBienvenida.Preparando;
        ticksPreparacionBienvenida = 0;
        lblContinuarBienvenida.Text = "Preparando EndForge\u2026";
        lblContinuarBienvenida.ForeColor = Color.FromArgb(202, 151, 247);

        foreach (Control control in new Control[] {
            panelPantallaBienvenida,
            panelContenidoBienvenida,
            lblLogoBienvenida,
            lblSubtituloBienvenida,
            lblContinuarBienvenida
        }) {
            control.Cursor = Cursors.Default;
        }

        if (timerBienvenida is not null) {
            timerBienvenida.Interval = 70;
            timerBienvenida.Start();
        }

        lblContinuarBienvenida.Invalidate();
    }

    private void AnimarPreparacionBienvenida() {
        ticksPreparacionBienvenida++;

        if (ticksPreparacionBienvenida % 4 != 0) {
            return;
        }

        int cantidadPuntos = ticksPreparacionBienvenida / 4 % 4;
        lblContinuarBienvenida.Text =
            "Preparando EndForge" + new string('.', cantidadPuntos);
    }

    private void IniciarDesvanecimientoBienvenida() {
        if (estadoVisualBienvenida != EstadoVisualBienvenida.Preparando ||
            inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing) {
            return;
        }

        estadoVisualBienvenida = EstadoVisualBienvenida.Desvaneciendo;
        marcaInicioDesvanecimientoBienvenida = Stopwatch.GetTimestamp();
        colorContinuarAntesDesvanecimiento = lblContinuarBienvenida.ForeColor;

        if (timerBienvenida is not null) {
            timerBienvenida.Interval = 30;
            timerBienvenida.Start();
        }

        RegistrarTiempoInicio("Inicio listo; desvanecimiento de Bienvenida iniciado");
    }

    private void ActualizarDesvanecimientoBienvenida() {
        double milisegundosTranscurridos = Stopwatch
            .GetElapsedTime(marcaInicioDesvanecimientoBienvenida)
            .TotalMilliseconds;
        float progreso = Math.Clamp(
            (float)(milisegundosTranscurridos / DuracionDesvanecimientoBienvenidaMs),
            0F,
            1F);
        Color colorSalida = Color.FromArgb(38, 28, 49);

        lblLogoBienvenida.ForeColor = MezclarColor(Color.White, colorSalida, progreso);
        lblSubtituloBienvenida.ForeColor = MezclarColor(
            Color.FromArgb(214, 200, 232),
            colorSalida,
            progreso);
        lblContinuarBienvenida.ForeColor = MezclarColor(
            colorContinuarAntesDesvanecimiento,
            colorSalida,
            progreso);

        if (progreso >= 1F) {
            FinalizarTransicionBienvenida();
        }
    }

    private static Color MezclarColor(Color origen, Color destino, float progreso) {
        int rojo = (int)Math.Round(origen.R + (destino.R - origen.R) * progreso);
        int verde = (int)Math.Round(origen.G + (destino.G - origen.G) * progreso);
        int azul = (int)Math.Round(origen.B + (destino.B - origen.B) * progreso);
        return Color.FromArgb(rojo, verde, azul);
    }

    private void PantallaBienvenida_Click(object? sender, EventArgs e) {
        EntrarAAplicacion();
    }

    private void FrmPrincipal_BienvenidaKeyDown(object? sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Enter) {
            e.Handled = true;
            e.SuppressKeyPress = true;
            EntrarAAplicacion();
            return;
        }

        if (e.KeyCode == Keys.Escape) {
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private void PanelPantallaBienvenida_PrimerPaint(object? sender, PaintEventArgs e) {
        panelPantallaBienvenida.Paint -= PanelPantallaBienvenida_PrimerPaint;
        RegistrarTiempoInicio("Primer Paint estable de Bienvenida");

        if (inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        ProgramarAccionInterfazSegura(IniciarPrecargaDatos);
    }

    private void IniciarPrecargaDatos() {
        if (precargaDatosIniciada ||
            inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing) {
            return;
        }

        precargaDatosIniciada = true;
        cancelacionPrecargaDatos = new CancellationTokenSource();
        CancellationToken token = cancelacionPrecargaDatos.Token;
        RegistrarTiempoInicio("Precarga de datos iniciada");
        tareaPrecargaDatos = Task.Run(() => PrecargarDatosInicio(token), token);
        tareaConstruccionCursoDuranteBienvenida =
            PrepararCursoTrasPrecargaAsync(tareaPrecargaDatos, token);
    }

    private async Task PrepararCursoTrasPrecargaAsync(
        Task<DatosPrecargaInicio> tareaPrecarga,
        CancellationToken token) {
        try {
            DatosPrecargaInicio datos = await tareaPrecarga;

            if (token.IsCancellationRequested ||
                inicializacionSecundariaCancelada ||
                IsDisposed ||
                Disposing) {
                return;
            }

            EstablecerDatosPrecargadosCurso(
                datos.Curso,
                datos.ServicioProgreso,
                datos.Progreso);
            RegistrarTiempoInicio("Curso UI: construcción programada durante Bienvenida");
            await PrepararCursoDiferidoAsync();
        } catch (OperationCanceledException) {
            // El cierre de la aplicación cancela la preparación sin tocar controles destruidos.
        } catch (Exception ex) {
            RegistrarErrorPrecarga(ex);
        }
    }

    private DatosPrecargaInicio PrecargarDatosInicio(CancellationToken token) {
        token.ThrowIfCancellationRequested();
        RegistrarTiempoInicio("Precarga: inicio de configuración");
        ResultadoCargaConfiguracion configuracion =
            configuracionService.CargarConfiguracion();
        RegistrarTiempoInicio("Precarga: configuración terminada");

        token.ThrowIfCancellationRequested();
        RegistrarTiempoInicio("Precarga: inicio de recientes");
        ResultadoLecturaRecientes recientes =
            recientesService.LeerProyectosRecientes();
        RegistrarTiempoInicio("Precarga: recientes terminados");

        token.ThrowIfCancellationRequested();
        ProgresoCursoService servicioProgreso = new();
        RegistrarTiempoInicio("Precarga: inicio de progreso");
        ResultadoCargaProgreso progreso = servicioProgreso.CargarProgreso();
        RegistrarTiempoInicio("Precarga: progreso terminado");

        token.ThrowIfCancellationRequested();
        RegistrarTiempoInicio("Precarga: inicio de modelos del Curso");
        CursoService curso = new();
        RegistrarTiempoInicio("Precarga: modelos del Curso terminados");

        token.ThrowIfCancellationRequested();
        string rutaBasePrecargada = configuracion.ConfiguracionDisponible
            ? configuracion.RutaBase
            : string.Empty;
        IReadOnlyList<string> temas = temasService.CargarTemas(rutaBasePrecargada);

        DatosPrecargaInicio datos = new() {
            Configuracion = configuracion,
            Recientes = recientes,
            Progreso = progreso,
            Curso = curso,
            ServicioProgreso = servicioProgreso,
            Temas = temas
        };

        datosPrecargadosInicio = datos;
        precargaDatosCompletada = true;
        RegistrarTiempoInicio("Precarga de datos completada");
        return datos;
    }

    private async Task<DatosPrecargaInicio?> ObtenerDatosPrecargadosAsync() {
        if (precargaDatosCompletada && datosPrecargadosInicio is not null) {
            return datosPrecargadosInicio;
        }

        IniciarPrecargaDatos();
        Task<DatosPrecargaInicio>? tarea = tareaPrecargaDatos;

        if (tarea is null) {
            return datosPrecargadosInicio;
        }

        try {
            DatosPrecargaInicio datos = await tarea;

            if (inicializacionSecundariaCancelada ||
                cancelacionPrecargaDatos?.IsCancellationRequested == true) {
                return null;
            }

            datosPrecargadosInicio = datos;
            precargaDatosCompletada = true;
            return datos;
        } catch (OperationCanceledException) {
            return null;
        } catch (Exception ex) {
            RegistrarErrorPrecarga(ex);
            return null;
        }
    }

    private async Task PrepararCursoParaInteraccionAsync() {
        DatosPrecargaInicio? datos = await ObtenerDatosPrecargadosAsync();

        if (inicializacionSecundariaCancelada || IsDisposed || Disposing) {
            return;
        }

        if (datos is not null) {
            EstablecerDatosPrecargadosCurso(
                datos.Curso,
                datos.ServicioProgreso,
                datos.Progreso);
        }

        await PrepararCursoDiferidoAsync();
    }

    private async Task<bool> EsperarFinalizacionTransicionBienvenidaAsync() {
        if (!transicionandoDesdeBienvenida) {
            return !inicializacionSecundariaCancelada;
        }

        TaskCompletionSource<bool> finalizacion =
            finalizacionTransicionBienvenida ??=
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
        return await finalizacion.Task;
    }

    [Conditional("DEBUG")]
    private void RegistrarTiempoInicio(string etapa) {
#if DEBUG
        Debug.WriteLine(
            $"[Inicio +{cronometroInicio.Elapsed.TotalMilliseconds,8:0.0} ms] " +
            $"[hilo {Environment.CurrentManagedThreadId}] {etapa}");
#endif
    }

    [Conditional("DEBUG")]
    private static void RegistrarErrorPrecarga(Exception error) {
        Debug.WriteLine($"[Inicio] La precarga terminó con error: {error}");
    }

    private void ProgramarAccionInterfazSegura(Action accion) {
        if (inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        try {
            BeginInvoke((Action)(() => {
                if (!inicializacionSecundariaCancelada &&
                    !IsDisposed &&
                    !Disposing) {
                    accion();
                }
            }));
        } catch (InvalidOperationException) when (
            inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            RegistrarTiempoInicio("Programación UI cancelada durante el cierre");
        }
    }

    private void EntrarAAplicacion() {
        if (entradaAplicacionRealizada || transicionandoDesdeBienvenida) {
            return;
        }

        RegistrarTiempoInicio("EntrarAAplicacion: inicio");
        entradaAplicacionRealizada = true;
        transicionandoDesdeBienvenida = true;
#if DEBUG
        marcaEntradaTransicionBienvenida = Stopwatch.GetTimestamp();
        marcaInicioListoTransicionBienvenida = 0;
        paintsBienvenidaDuranteTransicion = 0;
        paintsInicioDuranteTransicion = 0;
        recalculosGeometriaDuranteTransicion = 0;
        cambiosVisibleDuranteTransicion = 0;
#endif
        finalizacionTransicionBienvenida = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        timerRecalcularVista.Stop();
        recalculoPendienteDuranteTransicion = false;
        inicioPintadoParaTransicion = false;
        DesconectarEntradaBienvenida();
        lblContinuarBienvenida.Paint -= LblContinuarBienvenida_PreparacionPaint;
        lblContinuarBienvenida.Paint += LblContinuarBienvenida_PreparacionPaint;
        IniciarEstadoPreparacionBienvenida();
        RegistrarTiempoInicio("EntrarAAplicacion: estado Preparando solicitado");
    }

    private void LblContinuarBienvenida_PreparacionPaint(
        object? sender,
        PaintEventArgs e) {
        lblContinuarBienvenida.Paint -= LblContinuarBienvenida_PreparacionPaint;

        if (inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        RegistrarTiempoInicio("Estado Preparando pintado");
        ProgramarAccionInterfazSegura(PrepararInicioCubiertoPorBienvenida);
    }

    private void PrepararInicioCubiertoPorBienvenida() {
        if (inicializacionSecundariaCancelada ||
            !transicionandoDesdeBienvenida ||
            IsDisposed ||
            Disposing) {
            return;
        }

        IniciarPrecargaDatos();
        panelInicioVista.Paint -= PanelInicioVista_PrimerPaint;
        panelInicioVista.Paint += PanelInicioVista_PrimerPaint;

        fondoEndForge.SuspendLayout();
        panelPrincipal.SuspendLayout();
        panelMenu.SuspendLayout();

        try {
            modoCursoInmersivo = false;
            distribucionPanelPrincipal = DistribucionPanelPrincipal.Normal;
            EstablecerVisibleDuranteTransicion(panelMenu, true, nameof(panelMenu));
            EstablecerVisibleDuranteTransicion(
                panelPrincipal,
                true,
                nameof(panelPrincipal));
            RegistrarTiempoInicio("EntrarAAplicacion: menú y panel principal visibles");
            EstablecerVisibleDuranteTransicion(
                panelInicioVista,
                true,
                nameof(panelInicioVista));
            EstablecerVisibleDuranteTransicion(
                panelRecientesVista,
                false,
                nameof(panelRecientesVista));
            EstablecerVisibleDuranteTransicion(
                panelConfiguracionVista,
                false,
                nameof(panelConfiguracionVista));
            EstablecerVisibleDuranteTransicion(
                panelVistaNuevaPractica,
                false,
                nameof(panelVistaNuevaPractica));
            OcultarVistasCurso();
            RegistrarTiempoInicio("EntrarAAplicacion: Inicio seleccionado");

            panelInicioVista.BringToFront();
            RegistrarTiempoInicio("EntrarAAplicacion: BringToFront de Inicio");
#if DEBUG
            recalculosGeometriaDuranteTransicion++;
#endif
            PrepararGeometriaInicioTransicion();
            RegistrarTiempoInicio("EntrarAAplicacion: panelPrincipal dimensionado");
        } finally {
            panelMenu.ResumeLayout(performLayout: false);
            panelPrincipal.ResumeLayout(performLayout: false);
            fondoEndForge.ResumeLayout(performLayout: false);
            RegistrarTiempoInicio("EntrarAAplicacion: ResumeLayout completado");
        }

        panelMenu.PerformLayout();
        panelPrincipal.PerformLayout();
        panelInicioVista.PerformLayout();
        panelPantallaBienvenida.BringToFront();
        panelBarraTitulo.BringToFront();
        ActiveControl = null;
        panelInicioVista.Invalidate(true);
        RegistrarTiempoInicio("EntrarAAplicacion: área de Inicio invalidada");
        RegistrarTiempoInicio("EntrarAAplicacion: fondo continuo conservado sin invalidación global");
        ProgramarConfirmacionPaintInicio();
        RegistrarTiempoInicio("EntrarAAplicacion: Inicio preparado detrás de Bienvenida");
    }

    private void PrepararGeometriaInicioTransicion() {
        CentrarPanelPrincipal();
        Rectangle limites = panelPrincipal.ClientRectangle;
        panelInicioVista.SetBounds(
            limites.Left,
            limites.Top,
            Math.Max(1, limites.Width),
            Math.Max(1, limites.Height));
    }

    private void ProgramarConfirmacionPaintInicio() {
        if (inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        ProgramarAccionInterfazSegura(() => {
            if (inicializacionSecundariaCancelada ||
                !transicionandoDesdeBienvenida ||
                estadoVisualBienvenida != EstadoVisualBienvenida.Preparando ||
                IsDisposed ||
                Disposing ||
                !panelInicioVista.IsHandleCreated) {
                return;
            }

            panelPantallaBienvenida.SendToBack();

            try {
                panelInicioVista.Invalidate(true);
                panelInicioVista.Update();
            } finally {
                panelPantallaBienvenida.BringToFront();
                panelBarraTitulo.BringToFront();
            }

            if (!inicioPintadoParaTransicion) {
                RegistrarTiempoInicio(
                    "Inicio confirmado por geometría estable; Paint cubierto por Windows");
                MarcarInicioListoParaMetricas();
                ProgramarAccionInterfazSegura(IniciarDesvanecimientoBienvenida);
            }
        });
    }

    private async void CompletarInicioAplicacion() {
        if (inicializacionSecundariaCancelada ||
            inicializacionSecundariaCompletada ||
            inicializacionSecundariaEnCurso ||
            IsDisposed ||
            Disposing) {
            return;
        }

        inicializacionSecundariaEnCurso = true;
        RegistrarTiempoInicio("CompletarInicioAplicacion: inicio");

        try {
            DatosPrecargaInicio? datos = await ObtenerDatosPrecargadosAsync();

            if (inicializacionSecundariaCancelada || IsDisposed || Disposing) {
                return;
            }

            ResultadoCargaConfiguracion cargaConfiguracion;

            if (datos is not null) {
                EstablecerDatosPrecargadosCurso(
                    datos.Curso,
                    datos.ServicioProgreso,
                    datos.Progreso);

                RegistrarTiempoInicio("Inicialización secundaria: aplicar configuración");
                cargaConfiguracion = CargarConfiguracion(datos.Configuracion);
                CargarTemas(datos.Temas);
                RegistrarTiempoInicio("Inicialización secundaria: configuración y temas aplicados");
                CargarRecientes(datos.Recientes);
                RegistrarTiempoInicio("Inicialización secundaria: recientes aplicados");
            } else {
                RegistrarTiempoInicio("Inicialización secundaria: precarga no disponible; ruta segura");
                cargaConfiguracion = CargarConfiguracion();
                CargarTemas();
                CargarRecientes();
            }

            string estadoCurso = cursoPreparado
                ? "Inicialización secundaria: Curso ya preparado"
                : tareaConstruccionCursoDuranteBienvenida is { IsCompleted: false }
                    ? "Inicialización secundaria: Curso continúa en segundo plano visual"
                    : "Inicialización secundaria: Curso pendiente de interacción";
            RegistrarTiempoInicio(estadoCurso);

            txtBuscarReciente.Text = "Buscar práctica...";
            txtBuscarReciente.ForeColor = Color.Gray;

            if (string.IsNullOrWhiteSpace(rutaBase) || string.IsNullOrWhiteSpace(rutaPlantilla)) {
                PanelConfiguracion_Click(panelConfiguracion, EventArgs.Empty);

                if (cargaConfiguracion.Estado == EstadoCargaConfiguracion.NoDisponible) {
                    MessageBox.Show(
                        "¡Bienvenido a EndForge!\n\n" +
                        "Antes de comenzar, configura la carpeta de tus prácticas y la plantilla oficial.",
                        "Configuración inicial",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }

            inicializacionSecundariaCompletada = true;
            RegistrarTiempoInicio("CompletarInicioAplicacion: finalizado");
        } finally {
            inicializacionSecundariaEnCurso = false;
        }
    }

    private void ProgramarInicializacionSecundaria() {
        if (inicializacionSecundariaProgramada ||
            inicializacionSecundariaCompletada ||
            inicializacionSecundariaCancelada) {
            return;
        }

        inicializacionSecundariaProgramada = true;
        RegistrarTiempoInicio("Inicialización secundaria programada tras la transición");

        if (!IsHandleCreated) {
            return;
        }

        ProgramarAccionInterfazSegura(CompletarInicioAplicacion);
    }

    private void PanelInicioVista_PrimerPaint(object? sender, PaintEventArgs e) {
        panelInicioVista.Paint -= PanelInicioVista_PrimerPaint;
        inicioPintadoParaTransicion = true;
        RegistrarTiempoInicio("Primer Paint de Inicio completado");
        MarcarInicioListoParaMetricas();

        if (inicializacionSecundariaCancelada ||
            IsDisposed ||
            Disposing ||
            !IsHandleCreated) {
            return;
        }

        ProgramarAccionInterfazSegura(IniciarDesvanecimientoBienvenida);
    }

    private void MarcarInicioListoParaMetricas() {
#if DEBUG
        if (marcaInicioListoTransicionBienvenida != 0) {
            return;
        }

        marcaInicioListoTransicionBienvenida = Stopwatch.GetTimestamp();
        double milisegundosHastaInicioListo = Stopwatch
            .GetElapsedTime(marcaEntradaTransicionBienvenida)
            .TotalMilliseconds;
        Debug.WriteLine(
            $"[Transición] Enter -> Inicio listo: {milisegundosHastaInicioListo:0.0} ms");
#endif
    }

    private void FinalizarTransicionBienvenida() {
        if (estadoVisualBienvenida == EstadoVisualBienvenida.Finalizada) {
            return;
        }

        estadoVisualBienvenida = EstadoVisualBienvenida.Finalizada;

        if (inicializacionSecundariaCancelada || IsDisposed || Disposing) {
            DetenerTemporizadorBienvenida();
            return;
        }

        bool requiereRecalculo = recalculoPendienteDuranteTransicion;
        recalculoPendienteDuranteTransicion = false;

        if (requiereRecalculo) {
#if DEBUG
            recalculosGeometriaDuranteTransicion++;
#endif
            PrepararGeometriaInicioTransicion();
            panelInicioVista.Invalidate(true);
            panelInicioVista.Update();
            panelPantallaBienvenida.BringToFront();
            panelBarraTitulo.BringToFront();
            RegistrarTiempoInicio("Transición: recálculo final de Resize");
        }

        EstablecerVisibleDuranteTransicion(
            panelPantallaBienvenida,
            false,
            nameof(panelPantallaBienvenida));
        panelMenu.BringToFront();
        panelBarraTitulo.BringToFront();
        DetenerTemporizadorBienvenida();
        transicionandoDesdeBienvenida = false;
        ProgramarInicializacionSecundaria();
        finalizacionTransicionBienvenida?.TrySetResult(true);
        RegistrarMetricasFinTransicion();
        panelInicioVista.Invalidate();
        RegistrarTiempoInicio("Transición de Bienvenida finalizada");
    }

    private void EstablecerVisibleDuranteTransicion(
        Control control,
        bool visible,
        string nombreControl) {
        bool valorAnterior = control.Visible;
        control.Visible = visible;

        if (valorAnterior != visible) {
            RegistrarCambioVisibleTransicion(nombreControl, visible);
        }
    }

    [Conditional("DEBUG")]
    private void RegistrarCambioVisibleTransicion(string nombreControl, bool visible) {
#if DEBUG
        cambiosVisibleDuranteTransicion++;
        Debug.WriteLine(
            $"[Transición] Visible {nombreControl} = {visible}; " +
            $"cambios={cambiosVisibleDuranteTransicion}");
#endif
    }

    [Conditional("DEBUG")]
    private void RegistrarMetricasFinTransicion() {
#if DEBUG
        double tiempoTotal = Stopwatch
            .GetElapsedTime(marcaEntradaTransicionBienvenida)
            .TotalMilliseconds;
        double tiempoDesdeInicioListo = marcaInicioListoTransicionBienvenida == 0
            ? 0
            : Stopwatch
                .GetElapsedTime(marcaInicioListoTransicionBienvenida)
                .TotalMilliseconds;

        Debug.WriteLine(
            $"[Transición] Inicio listo -> fin del fade: " +
            $"{tiempoDesdeInicioListo:0.0} ms");
        Debug.WriteLine(
            $"[Transición] Total percibido: {tiempoTotal:0.0} ms; " +
            $"Paint Bienvenida={paintsBienvenidaDuranteTransicion}; " +
            $"Paint Inicio={paintsInicioDuranteTransicion}; " +
            $"recalculos={recalculosGeometriaDuranteTransicion}; " +
            $"cambios Visible={cambiosVisibleDuranteTransicion}; " +
            $"ClientSize={ClientSize}; WindowState={WindowState}; " +
            $"Inicio={panelInicioVista.Bounds}");
#endif
    }

#if DEBUG
    private void PanelPantallaBienvenida_ContarPaint(object? sender, PaintEventArgs e) {
        if (transicionandoDesdeBienvenida) {
            paintsBienvenidaDuranteTransicion++;
        }
    }

    private void PanelInicioVista_ContarPaint(object? sender, PaintEventArgs e) {
        if (transicionandoDesdeBienvenida) {
            paintsInicioDuranteTransicion++;
        }
    }
#endif

    private void DesconectarEntradaBienvenida() {
        KeyDown -= FrmPrincipal_BienvenidaKeyDown;
        panelPantallaBienvenida.Click -= PantallaBienvenida_Click;
        panelContenidoBienvenida.Click -= PantallaBienvenida_Click;
        lblLogoBienvenida.Click -= PantallaBienvenida_Click;
        lblSubtituloBienvenida.Click -= PantallaBienvenida_Click;
        lblContinuarBienvenida.Click -= PantallaBienvenida_Click;
        panelBarraTitulo.Click -= PantallaBienvenida_Click;
        lblBarraTitulo.Click -= PantallaBienvenida_Click;
        pictureBoxBarraIcono.Click -= PantallaBienvenida_Click;
    }

    private void DetenerTemporizadorBienvenida() {
        if (timerBienvenida == null) {
            return;
        }

        timerBienvenida.Stop();
        timerBienvenida.Tick -= TimerBienvenida_Tick;
        timerBienvenida.Dispose();
        timerBienvenida = null;
    }

    private void FrmPrincipal_BienvenidaFormClosed(object? sender, FormClosedEventArgs e) {
        inicializacionSecundariaCancelada = true;
        transicionandoDesdeBienvenida = false;
        finalizacionTransicionBienvenida?.TrySetResult(false);
        panelPantallaBienvenida.Paint -= PanelPantallaBienvenida_PrimerPaint;
        panelInicioVista.Paint -= PanelInicioVista_PrimerPaint;
        lblContinuarBienvenida.Paint -= LblContinuarBienvenida_PreparacionPaint;
#if DEBUG
        panelPantallaBienvenida.Paint -= PanelPantallaBienvenida_ContarPaint;
        panelInicioVista.Paint -= PanelInicioVista_ContarPaint;
#endif
        cancelacionPrecargaDatos?.Cancel();
        DetenerTemporizadorBienvenida();
        timerRecalcularVista.Stop();
        timerRecalcularVista.Tick -= TimerRecalcularVista_Tick;
        timerRecalcularVista.Dispose();
        cancelacionPrecargaDatos?.Dispose();
        cancelacionPrecargaDatos = null;
    }
}
