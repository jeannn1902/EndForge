using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests;

public sealed class CreacionPracticasOrquestadorTests {
    private static readonly SolicitudCreacionPractica Solicitud = new() {
        RutaPlantilla = @"C:\Plantilla",
        RutaProyecto = @"C:\Curso\01_Practica",
        NombreProyecto = "01_Practica",
        Tema = "Variables",
        Objetivo = "Probar el flujo postpublicación.",
        RutaRelativaSolucionEsperada = "01_Practica.sln"
    };

    [Fact]
    public void RegistroFallido_DevuelveEstadoPostpublicacionExplicito() {
        CreacionPracticasOrquestador orquestador = CrearOrquestador(
            guardar: _ => new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.ErrorIo,
                Error = new IOException("recientes bloqueado")
            });

        ResultadoCreacionPractica resultado = orquestador.CrearPractica(
            Solicitud,
            _ => { },
            () => { });

        Assert.Equal(
            EstadoCreacionPractica.CreadaAbiertaSinRegistroReciente,
            resultado.Estado);
        Assert.Equal(
            EstadoEscrituraRecientes.ErrorIo,
            resultado.RegistroReciente?.Estado);
    }

    [Fact]
    public void CallbacksPostpublicacion_NoEscapanNiCambianElExitoFuncional() {
        CreacionPracticasOrquestador orquestador = CrearOrquestador();

        Exception? excepcion = Record.Exception(() => {
            ResultadoCreacionPractica resultado = orquestador.CrearPractica(
                Solicitud,
                _ => throw new InvalidOperationException("callback recientes"),
                () => throw new InvalidOperationException("callback apertura"));

            Assert.Equal(EstadoCreacionPractica.Exitosa, resultado.Estado);
            Assert.NotNull(resultado.ErrorSecundario);
        });

        Assert.Null(excepcion);
    }

    [Fact]
    public void AperturaFallida_NoIntentaRegistrarReciente() {
        bool intentoGuardar = false;
        CreacionPracticasOrquestador orquestador = CrearOrquestador(
            abrir: _ => new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.ErrorApertura,
                Error = new IOException("No se pudo abrir.")
            },
            guardar: _ => {
                intentoGuardar = true;
                return new ResultadoEscrituraRecientes {
                    Estado = EstadoEscrituraRecientes.Exitosa
                };
            });

        ResultadoCreacionPractica resultado = orquestador.CrearPractica(
            Solicitud,
            _ => { },
            () => { });

        Assert.Equal(EstadoCreacionPractica.ErrorApertura, resultado.Estado);
        Assert.False(intentoGuardar);
    }

    [Fact]
    public void AperturaFallida_ConservaPracticaYaPublicada() {
        string raiz = Path.Combine(
            Path.GetTempPath(),
            $"EndForge.Tests-Orquestador-{Guid.NewGuid():N}");
        string rutaProyecto = Path.Combine(raiz, "01_Practica");
        SolicitudCreacionPractica solicitud = new() {
            RutaPlantilla = Solicitud.RutaPlantilla,
            RutaProyecto = rutaProyecto,
            NombreProyecto = Solicitud.NombreProyecto,
            Tema = Solicitud.Tema,
            Objetivo = Solicitud.Objetivo,
            RutaRelativaSolucionEsperada =
                Solicitud.RutaRelativaSolucionEsperada
        };

        try {
            CreacionPracticasOrquestador orquestador = CrearOrquestador(
                crear: _ => {
                    Directory.CreateDirectory(rutaProyecto);
                    File.WriteAllText(
                        Path.Combine(rutaProyecto, "testigo.txt"),
                        "publicado");
                },
                abrir: _ => new ResultadoAperturaPractica {
                    Estado = EstadoAperturaPractica.ErrorApertura,
                    Error = new IOException("No se pudo abrir.")
                });

            ResultadoCreacionPractica resultado =
                orquestador.CrearPractica(
                    solicitud,
                    _ => { },
                    () => { });

            Assert.Equal(
                EstadoCreacionPractica.ErrorApertura,
                resultado.Estado);
            Assert.Equal(
                "publicado",
                File.ReadAllText(
                    Path.Combine(rutaProyecto, "testigo.txt")));
        } finally {
            if (Directory.Exists(raiz)) {
                Directory.Delete(raiz, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CrearPracticaAsync_DevuelveElControlMientrasEjecutaIo() {
        using ManualResetEventSlim inicioCreacion = new(initialState: false);
        using ManualResetEventSlim permitirFinalizacion = new(initialState: false);
        CreacionPracticasOrquestador orquestador = CrearOrquestador(
            crear: _ => {
                inicioCreacion.Set();
                permitirFinalizacion.Wait();
            });

        Task<ResultadoCreacionPractica> operacion =
            orquestador.CrearPracticaAsync(
                Solicitud,
                _ => { },
                () => { });

        Assert.True(inicioCreacion.Wait(TimeSpan.FromSeconds(5)));
        Assert.False(operacion.IsCompleted);

        permitirFinalizacion.Set();
        ResultadoCreacionPractica resultado = await operacion;

        Assert.Equal(EstadoCreacionPractica.Exitosa, resultado.Estado);
    }

    private static CreacionPracticasOrquestador CrearOrquestador(
        Action<SolicitudCreacionPractica>? crear = null,
        Func<SolicitudCreacionPractica, ResultadoAperturaPractica>? abrir = null,
        Func<string, ResultadoEscrituraRecientes>? guardar = null) {
        return new CreacionPracticasOrquestador(
            crear ?? (_ => { }),
            abrir ?? (_ => new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.Exitosa,
                RutaSolucion = Solicitud.RutaRelativaSolucionEsperada
            }),
            guardar ?? (_ => new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.Exitosa
            })
        );
    }
}
