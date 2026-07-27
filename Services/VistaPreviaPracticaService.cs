using EndForge.Models;

namespace EndForge.Services;

public sealed class VistaPreviaPracticaService {
    private readonly TemasService temasService;

    public VistaPreviaPracticaService(TemasService temasService) {
        this.temasService = temasService;
    }

    public ResultadoVistaPreviaPractica Calcular(
        string rutaBase,
        string? temaSeleccionado,
        string nombreIntroducido) {
        string nombreNormalizado = nombreIntroducido.Trim();

        if (string.IsNullOrEmpty(temaSeleccionado) || nombreNormalizado == "") {
            return new ResultadoVistaPreviaPractica {
                Estado = EstadoVistaPreviaPractica.Vacia
            };
        }

        ResultadoNumeracionPractica numeracion =
            temasService.ObtenerSiguienteNumero(
            rutaBase,
            temaSeleccionado
        );

        if (!numeracion.EsExitosa) {
            return new ResultadoVistaPreviaPractica {
                Estado = EstadoVistaPreviaPractica.NumeracionNoDisponible,
                EstadoNumeracion = numeracion.Estado,
                Error = numeracion.Error
            };
        }

        return new ResultadoVistaPreviaPractica {
            Estado = EstadoVistaPreviaPractica.Completa,
            NombreFinal =
                numeracion.Numero!.Value.ToString("00") +
                "_" +
                nombreNormalizado,
            EstadoNumeracion = numeracion.Estado
        };
    }
}
