using EndForge.Models;
using EndForge.Services;

namespace EndForge.Tests.Comparadores;

internal static class ComparadorTestFactory {
    public static ResultadoComparacionSalida Comparar(
        CasoPrueba caso,
        string? salida) {
        return new ComparadorSalidaService().Comparar(caso, salida);
    }

    public static CasoPrueba CasoMixto(
        IReadOnlyList<ReglaColeccionEsperada>? colecciones = null,
        IReadOnlyList<ReglaCadenaEsperada>? cadenas = null,
        IReadOnlyList<ReglaTablaEsperada>? tablas = null,
        IReadOnlyList<ReglaMatrizEsperada>? matrices = null,
        IReadOnlyList<ReglaBloquesRegistroEsperados>? registros = null) {
        return new CasoPrueba {
            Id = "caso-comparador",
            Nombre = "Caso del comparador",
            ModoComparacion = ModoComparacionCaso.Mixto,
            ColeccionesEsperadas = colecciones ?? Array.Empty<ReglaColeccionEsperada>(),
            CadenasEsperadas = cadenas ?? Array.Empty<ReglaCadenaEsperada>(),
            TablasEsperadas = tablas ?? Array.Empty<ReglaTablaEsperada>(),
            MatricesEsperadas = matrices ?? Array.Empty<ReglaMatrizEsperada>(),
            BloquesRegistroEsperados =
                registros ?? Array.Empty<ReglaBloquesRegistroEsperados>()
        };
    }

    public static ValorEstructuradoEsperado Numero(
        double valor,
        double tolerancia = 0.01D) {
        return new ValorEstructuradoEsperado {
            Tipo = TipoValorEstructurado.Numerico,
            ValorNumerico = valor,
            ToleranciaNumerica = tolerancia
        };
    }

    public static ValorEstructuradoEsperado Texto(
        string valor,
        bool distinguirMayusculas = false,
        bool distinguirAcentos = false,
        PoliticaEspaciosCadena espacios =
            PoliticaEspaciosCadena.RecortarExtremos) {
        return new ValorEstructuradoEsperado {
            Tipo = TipoValorEstructurado.Textual,
            ValorTextual = valor,
            DistinguirMayusculas = distinguirMayusculas,
            DistinguirAcentos = distinguirAcentos,
            PoliticaEspacios = espacios
        };
    }

    public static ValorEstructuradoEsperado Booleano(bool valor) {
        return new ValorEstructuradoEsperado {
            Tipo = TipoValorEstructurado.Booleano,
            ValorBooleano = valor
        };
    }
}
