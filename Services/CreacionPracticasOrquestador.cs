using EndForge.Models;

namespace EndForge.Services;

public sealed class CreacionPracticasOrquestador {
    private readonly ProyectoService proyectoService;
    private readonly RecientesService recientesService;
    private readonly AperturaPracticasService aperturaPracticasService;

    public CreacionPracticasOrquestador(
        ProyectoService proyectoService,
        RecientesService recientesService,
        AperturaPracticasService aperturaPracticasService) {
        this.proyectoService = proyectoService;
        this.recientesService = recientesService;
        this.aperturaPracticasService = aperturaPracticasService;
    }

    public ResultadoCreacionPractica CrearPractica(
        SolicitudCreacionPractica solicitud,
        Action<ResultadoEscrituraRecientes> alFinalizarRegistroReciente,
        Action alPrepararApertura) {
        try {
            proyectoService.CrearProyecto(
                solicitud.RutaPlantilla,
                solicitud.RutaProyecto,
                solicitud.NombreProyecto,
                solicitud.Tema,
                solicitud.Objetivo,
                solicitud.RutaRelativaSolucionEsperada
            );
        } catch (ProyectoService.ProyectoDestinoExistenteException ex) {
            return new ResultadoCreacionPractica {
                Estado = EstadoCreacionPractica.DestinoExistente,
                Error = ex
            };
        } catch (Exception ex) {
            return new ResultadoCreacionPractica {
                Estado = EstadoCreacionPractica.ErrorCreacion,
                Error = ex
            };
        }

        alPrepararApertura();

        ResultadoAperturaPractica apertura = aperturaPracticasService.AbrirPractica(
            solicitud.RutaProyecto,
            solicitud.RutaRelativaSolucionEsperada
        );

        if (apertura.Estado != EstadoAperturaPractica.Exitosa) {
            return new ResultadoCreacionPractica {
                Estado = EstadoCreacionPractica.ErrorApertura,
                Error = apertura.Error
            };
        }

        ResultadoEscrituraRecientes registroReciente =
            recientesService.GuardarProyectoReciente(solicitud.RutaProyecto);
        alFinalizarRegistroReciente(registroReciente);

        return new ResultadoCreacionPractica {
            Estado = EstadoCreacionPractica.Exitosa
        };
    }
}
