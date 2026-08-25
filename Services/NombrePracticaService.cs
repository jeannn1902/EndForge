using EndForge.Models;

namespace EndForge.Services;

public sealed class NombrePracticaService {
    public const int LongitudMaxima = 40;

    private static readonly HashSet<string> NombresReservados = new(StringComparer.OrdinalIgnoreCase) {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    private static readonly char[] CaracteresXmlNoSeguros = ['&', '\'', '"', '<', '>'];

    public ResultadoValidacionNombrePractica Validar(string? nombre) {
        if (string.IsNullOrWhiteSpace(nombre)) {
            return Invalido("Escribe un nombre para el proyecto.");
        }

        if (nombre.EndsWith(' ') || nombre.EndsWith('.')) {
            return Invalido("El nombre no puede terminar en punto o espacio.");
        }

        string nombreNormalizado = nombre.Trim(' ');

        if (nombreNormalizado.Length == 0) {
            return Invalido("Escribe un nombre para el proyecto.");
        }

        if (nombreNormalizado.Length > LongitudMaxima) {
            return Invalido($"El nombre es demasiado largo. Usa un máximo de {LongitudMaxima} caracteres.");
        }

        if (nombreNormalizado.IndexOfAny(CaracteresXmlNoSeguros) >= 0) {
            return Invalido("El nombre contiene caracteres que no son seguros para Visual Studio o archivos XML.");
        }

        if (nombreNormalizado.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            return Invalido("El nombre contiene caracteres no válidos.");
        }

        string nombreBase = nombreNormalizado.Split('.')[0];

        if (NombresReservados.Contains(nombreBase)) {
            return Invalido("El nombre está reservado por Windows.");
        }

        return new ResultadoValidacionNombrePractica {
            EsValido = true,
            NombreNormalizado = nombreNormalizado
        };
    }

    private static ResultadoValidacionNombrePractica Invalido(string mensaje) {
        return new ResultadoValidacionNombrePractica {
            EsValido = false,
            MensajeError = mensaje
        };
    }
}
