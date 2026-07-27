using EndForge.Models;

namespace EndForge.Services;

public sealed class CreacionPracticasOrquestador {
    private readonly Action<SolicitudCreacionPractica> crearProyecto;
    private readonly Func<SolicitudCreacionPractica, ResultadoAperturaPractica> abrirPractica;
    private readonly Func<string, ResultadoEscrituraRecientes> guardarProyectoReciente;

    public CreacionPracticasOrquestador(
        ProyectoService proyectoService,
        RecientesService recientesService,
        AperturaPracticasService aperturaPracticasService) {
        crearProyecto = solicitud => proyectoService.CrearProyecto(
            solicitud.RutaPlantilla,
            solicitud.RutaProyecto,
            solicitud.NombreProyecto,
            solicitud.Tema,
            solicitud.Objetivo,
            solicitud.RutaRelativaSolucionEsperada,
            solicitud.RutaBaseConfiable
        );
        abrirPractica = solicitud => aperturaPracticasService.AbrirPractica(
            solicitud.RutaProyecto,
            solicitud.RutaRelativaSolucionEsperada
        );
        guardarProyectoReciente = recientesService.GuardarProyectoReciente;
    }

    internal CreacionPracticasOrquestador(
        Action<SolicitudCreacionPractica> crearProyecto,
        Func<SolicitudCreacionPractica, ResultadoAperturaPractica> abrirPractica,
        Func<string, ResultadoEscrituraRecientes> guardarProyectoReciente) {
        this.crearProyecto = crearProyecto;
        this.abrirPractica = abrirPractica;
        this.guardarProyectoReciente = guardarProyectoReciente;
    }

    public ResultadoCreacionPractica CrearPractica(
        SolicitudCreacionPractica solicitud,
        Action<ResultadoEscrituraRecientes> alFinalizarRegistroReciente,
        Action alPrepararApertura) {
        ResultadoCreacionPractica? errorCreacion = IntentarCrear(solicitud);

        if (errorCreacion is not null) {
            return errorCreacion;
        }

        Exception? errorSecundario = IntentarNotificar(alPrepararApertura);
        ResultadoAperturaPractica apertura = IntentarAbrir(solicitud);

        if (apertura.Estado != EstadoAperturaPractica.Exitosa) {
            return CrearResultadoErrorApertura(apertura, errorSecundario);
        }

        ResultadoEscrituraRecientes registroReciente =
            IntentarGuardarReciente(solicitud.RutaProyecto);
        errorSecundario = IntentarNotificar(
            () => alFinalizarRegistroReciente(registroReciente),
            errorSecundario);

        return CrearResultadoFinal(registroReciente, errorSecundario);
    }

    public async Task<ResultadoCreacionPractica> CrearPracticaAsync(
        SolicitudCreacionPractica solicitud,
        Action<ResultadoEscrituraRecientes> alFinalizarRegistroReciente,
        Action alPrepararApertura) {
        ResultadoCreacionPractica? errorCreacion = await Task.Run(
            () => IntentarCrear(solicitud));

        if (errorCreacion is not null) {
            return errorCreacion;
        }

        Exception? errorSecundario = IntentarNotificar(alPrepararApertura);
        ResultadoAperturaPractica apertura = await Task.Run(
            () => IntentarAbrir(solicitud));

        if (apertura.Estado != EstadoAperturaPractica.Exitosa) {
            return CrearResultadoErrorApertura(apertura, errorSecundario);
        }

        ResultadoEscrituraRecientes registroReciente = await Task.Run(
            () => IntentarGuardarReciente(solicitud.RutaProyecto));
        errorSecundario = IntentarNotificar(
            () => alFinalizarRegistroReciente(registroReciente),
            errorSecundario);

        return CrearResultadoFinal(registroReciente, errorSecundario);
    }

    private ResultadoCreacionPractica? IntentarCrear(
        SolicitudCreacionPractica solicitud) {
        try {
            crearProyecto(solicitud);
        } catch (ProyectoService.ProyectoDestinoExistenteException ex) {
            return new ResultadoCreacionPractica {
                Estado = EstadoCreacionPractica.DestinoExistente,
                Error = ex
            };
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return new ResultadoCreacionPractica {
                Estado = EstadoCreacionPractica.ErrorCreacion,
                Error = ex
            };
        }

        return null;
    }

    private ResultadoAperturaPractica IntentarAbrir(
        SolicitudCreacionPractica solicitud) {
        ResultadoAperturaPractica apertura;

        try {
            apertura = abrirPractica(solicitud);
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            apertura = new ResultadoAperturaPractica {
                Estado = EstadoAperturaPractica.ErrorApertura,
                Error = ex
            };
        }

        return apertura;
    }

    private ResultadoEscrituraRecientes IntentarGuardarReciente(
        string rutaProyecto) {
        ResultadoEscrituraRecientes registroReciente;

        try {
            registroReciente = guardarProyectoReciente(rutaProyecto);
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            registroReciente = new ResultadoEscrituraRecientes {
                Estado = EstadoEscrituraRecientes.ErrorIo,
                Error = ex
            };
        }

        return registroReciente;
    }

    private static Exception? IntentarNotificar(
        Action notificacion,
        Exception? errorAnterior = null) {
        try {
            notificacion();
        } catch (Exception ex)
            when (!RegistroErroresService.EsExcepcionCritica(ex)) {
            return errorAnterior ?? ex;
        }

        return errorAnterior;
    }

    private static ResultadoCreacionPractica CrearResultadoErrorApertura(
        ResultadoAperturaPractica apertura,
        Exception? errorSecundario) {
        return new ResultadoCreacionPractica {
            Estado = EstadoCreacionPractica.ErrorApertura,
            Error = apertura.Error,
            ErrorSecundario = errorSecundario
        };
    }

    private static ResultadoCreacionPractica CrearResultadoFinal(
        ResultadoEscrituraRecientes registroReciente,
        Exception? errorSecundario) {
        return new ResultadoCreacionPractica {
            Estado = registroReciente.EsExitosa
                ? EstadoCreacionPractica.Exitosa
                : EstadoCreacionPractica.CreadaAbiertaSinRegistroReciente,
            Error = registroReciente.EsExitosa ? null : registroReciente.Error,
            ErrorSecundario = errorSecundario,
            RegistroReciente = registroReciente
        };
    }
}
