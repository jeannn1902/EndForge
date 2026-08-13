using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using EndForge.Controls;

namespace EndForge.Tests;

public sealed class PanelDesplazableSinBarrasTests {
    [Fact]
    public void MoverA_DestinoDistinto_AplicaExactamenteUnaVez() {
        EstadoDesplazamientoSincrono estado = new();
        int aplicaciones = 0;
        int anterior = -1;
        int nueva = -1;

        bool cambio = estado.IntentarMover(240, 500, (origen, destino) => {
            aplicaciones++;
            anterior = origen;
            nueva = destino;
        });

        Assert.True(cambio);
        Assert.Equal(1, aplicaciones);
        Assert.Equal(0, anterior);
        Assert.Equal(240, nueva);
        Assert.Equal(240, estado.PosicionActual);
        Assert.False(estado.AplicacionEnCurso);
        Assert.Equal(0, estado.OperacionesPendientes);
    }

    [Fact]
    public void MoverA_MismaPosicionONuevoLimiteEquivalente_NoHaceTrabajo() {
        EstadoDesplazamientoSincrono estado = new();
        int aplicaciones = 0;
        Assert.True(estado.IntentarMover(500, 500, (_, _) => aplicaciones++));

        Assert.False(estado.IntentarMover(500, 500, (_, _) => aplicaciones++));
        Assert.False(estado.IntentarMover(900, 500, (_, _) => aplicaciones++));

        Assert.Equal(1, aplicaciones);
        Assert.Equal(500, estado.PosicionActual);
        Assert.Equal(0, estado.OperacionesPendientes);
    }

    [Fact]
    public void MoverA_SolicitudReentrante_NoIniciaOtraAplicacion() {
        EstadoDesplazamientoSincrono estado = new();
        int aplicaciones = 0;
        bool? resultadoReentrante = null;

        Assert.True(estado.IntentarMover(120, 500, (_, _) => {
            aplicaciones++;
            Assert.True(estado.AplicacionEnCurso);
            resultadoReentrante = estado.IntentarMover(
                300,
                500,
                (_, _) => aplicaciones++);
        }));

        Assert.False(resultadoReentrante);
        Assert.Equal(1, aplicaciones);
        Assert.Equal(120, estado.PosicionActual);
        Assert.False(estado.AplicacionEnCurso);
    }

    [Fact]
    public void AjustarMaximo_PosicionValida_NoLaModifica() {
        EstadoDesplazamientoSincrono estado = new();
        Assert.True(estado.IntentarMover(180, 500, (_, _) => { }));

        Assert.False(estado.AjustarMaximo(300));

        Assert.Equal(180, estado.PosicionActual);
        Assert.Equal(300, estado.MaximoActual);
        Assert.Equal(0, estado.OperacionesPendientes);
    }

    [Fact]
    public void AjustarMaximo_Reducido_LimitaLaPosicionUnaSolaVez() {
        EstadoDesplazamientoSincrono estado = new();
        Assert.True(estado.IntentarMover(480, 500, (_, _) => { }));

        Assert.True(estado.AjustarMaximo(180));
        Assert.False(estado.AjustarMaximo(180));

        Assert.Equal(180, estado.PosicionActual);
        Assert.Equal(180, estado.MaximoActual);
        Assert.Equal(0, estado.OperacionesPendientes);
    }

    [Fact]
    public void DesplazamientosIntensivos_ConvergenSinOperacionPendiente() {
        EstadoDesplazamientoSincrono estado = new();
        int aplicaciones = 0;

        for (int indice = 0; indice < 1000; indice++) {
            int destino = indice * 37 % 501;
            estado.IntentarMover(destino, 500, (_, _) => aplicaciones++);
        }

        Assert.Equal(999 * 37 % 501, estado.PosicionActual);
        Assert.Equal(999, aplicaciones);
        Assert.False(estado.AplicacionEnCurso);
        Assert.Equal(0, estado.OperacionesPendientes);
    }

    [Fact]
    public void Rueda_DeltasParciales_SeAcumulanHastaCompletarUnPaso() {
        int posicion = 0;
        int acumulado = 0;

        for (int indice = 0; indice < 4; indice++) {
            ResultadoRuedaDesplazamiento resultado =
                PanelDesplazableSinBarras.CalcularDestinoRueda(
                    posicion,
                    acumulado,
                    delta: -30,
                    lineas: 1,
                    altoViewport: 180,
                    maximo: 500);
            posicion = resultado.PosicionDestino;
            acumulado = resultado.AcumuladoRestante;

            if (indice < 3) {
                Assert.Equal(0, posicion);
            }
        }

        Assert.Equal(40, posicion);
        Assert.Equal(0, acumulado);
    }

    [Fact]
    public void RuedaYArrastre_ConvergenEnLaMismaPosicion() {
        int posicionRueda = 0;
        int acumulado = 0;

        for (int indice = 0; indice < 10; indice++) {
            ResultadoRuedaDesplazamiento resultado =
                PanelDesplazableSinBarras.CalcularDestinoRueda(
                    posicionRueda,
                    acumulado,
                    delta: -120,
                    lineas: 1,
                    altoViewport: 180,
                    maximo: 400);
            posicionRueda = resultado.PosicionDestino;
            acumulado = resultado.AcumuladoRestante;
        }

        int posicionArrastre = PanelDesplazableSinBarras.CalcularDestinoIndicador(
            posicionY: 108,
            desfase: 8,
            inicioPista: 0,
            recorrido: 100,
            maximo: 400);

        Assert.Equal(400, posicionRueda);
        Assert.Equal(posicionRueda, posicionArrastre);
        Assert.Equal(0, acumulado);
    }

    [Fact]
    public void ControlConHandle_IrAlFinal_AplicaAntesDeRetornar() {
        EjecutarEnSta(() => {
            using Form host = new();
            using PanelDesplazableSinBarras desplazamiento = CrearDesplazamiento();
            host.Controls.Add(desplazamiento);
            host.CreateControl();
            desplazamiento.CreateControl();
            desplazamiento.Contenido.CreateControl();
            desplazamiento.ActualizarContenido(volverAlInicio: true);
            int cambiosUbicacion = 0;
            desplazamiento.Contenido.LocationChanged += (_, _) => cambiosUbicacion++;

            desplazamiento.IrAlFinal();
            int topFinal = desplazamiento.Contenido.Top;

            Assert.True(topFinal < desplazamiento.Padding.Top);
            Assert.Equal(1, cambiosUbicacion);
            Assert.Equal(0, desplazamiento.OperacionesDesplazamientoPendientes);

            desplazamiento.IrAlFinal();

            Assert.Equal(topFinal, desplazamiento.Contenido.Top);
            Assert.Equal(1, cambiosUbicacion);
        });
    }

    [Fact]
    public void Resize_LimitaLaPosicionYNoAcumulaOperaciones() {
        EjecutarEnSta(() => {
            using PanelDesplazableSinBarras desplazamiento = CrearDesplazamiento();
            desplazamiento.ActualizarContenido(volverAlInicio: true);
            desplazamiento.IrAlFinal();
            int posicionAntes = desplazamiento.PosicionDesplazamientoActual;

            desplazamiento.Height = 420;
            int posicionDespues = desplazamiento.PosicionDesplazamientoActual;

            Assert.True(posicionDespues < posicionAntes);
            Assert.Equal(
                desplazamiento.Padding.Top - posicionDespues,
                desplazamiento.Contenido.Top);
            Assert.Equal(0, desplazamiento.OperacionesDesplazamientoPendientes);

            desplazamiento.ActualizarContenido(volverAlInicio: false);

            Assert.Equal(posicionDespues, desplazamiento.PosicionDesplazamientoActual);
            Assert.Equal(0, desplazamiento.OperacionesDesplazamientoPendientes);
        });
    }

    [Fact]
    public void DesplazamientosRepetidos_ConservanBoundsInternosYSinLayouts() {
        EjecutarEnSta(() => {
            using PanelDesplazableSinBarras desplazamiento = CrearDesplazamiento();
            Panel tarjeta = Assert.IsType<Panel>(desplazamiento.Contenido.Controls[0]);
            Label texto = Assert.IsType<Label>(tarjeta.Controls[0]);
            desplazamiento.ActualizarContenido(volverAlInicio: true);
            Rectangle boundsTarjeta = tarjeta.Bounds;
            Rectangle boundsTexto = texto.Bounds;
            int layoutsHost = 0;
            int layoutsContenido = 0;
            desplazamiento.Layout += (_, _) => layoutsHost++;
            desplazamiento.Contenido.Layout += (_, _) => layoutsContenido++;

            for (int indice = 0; indice < 50; indice++) {
                desplazamiento.IrAlFinal();
                desplazamiento.IrAlInicio();
            }

            Assert.Equal(boundsTarjeta, tarjeta.Bounds);
            Assert.Equal(boundsTexto, texto.Bounds);
            Assert.Equal(0, layoutsHost);
            Assert.Equal(0, layoutsContenido);
            Assert.Equal(desplazamiento.Padding.Top, desplazamiento.Contenido.Top);
            Assert.Equal(0, desplazamiento.OperacionesDesplazamientoPendientes);
        });
    }

    [Fact]
    public void TransferirRestaurarYDispose_ConservaCompatibilidadSinPendientes() {
        EjecutarEnSta(() => {
            PanelDesplazableSinBarras desplazamiento = CrearDesplazamiento();
            using Panel contenedorExterno = new();
            desplazamiento.ActualizarContenido(volverAlInicio: true);
            desplazamiento.IrAlFinal();

            desplazamiento.TransferirContenidoA(contenedorExterno);
            Assert.Same(contenedorExterno, desplazamiento.Contenido.Parent);
            Assert.Equal(0, desplazamiento.OperacionesDesplazamientoPendientes);

            desplazamiento.RestaurarContenido();
            Assert.Same(desplazamiento, desplazamiento.Contenido.Parent);
            desplazamiento.IrAlFinal();
            Assert.True(desplazamiento.PosicionDesplazamientoActual > 0);

            desplazamiento.Dispose();

            Assert.True(desplazamiento.IsDisposed);
            Assert.Equal(0, desplazamiento.OperacionesDesplazamientoPendientes);
        });
    }

    private static PanelDesplazableSinBarras CrearDesplazamiento() {
        PanelDesplazableSinBarras desplazamiento = new() {
            Size = new Size(360, 180),
            Padding = Padding.Empty
        };
        Panel tarjeta = new() {
            Size = new Size(300, 720),
            Margin = Padding.Empty
        };
        tarjeta.Controls.Add(new Label {
            Bounds = new Rectangle(20, 24, 220, 30),
            Text = "Contenido estable"
        });
        desplazamiento.Contenido.Controls.Add(tarjeta);
        return desplazamiento;
    }

    private static void EjecutarEnSta(Action accion) {
        Exception? error = null;
        Thread hilo = new(() => {
            try {
                accion();
            } catch (Exception ex) {
                error = ex;
            }
        });
        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        hilo.Join();

        if (error is not null) {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
