using EndForge.Models;
using System.Globalization;

namespace EndForge.Services;

public sealed partial class CatalogoEvaluacionesService {
    public const string ArreglosCapturarMostrarId =
        "grado2-arreglos-capturar-mostrar";
    public const string ArreglosSumaElementosId =
        "grado2-arreglos-suma-elementos";
    public const string ArreglosPromedioId = "grado2-arreglos-promedio";
    public const string ArreglosMayorMenorId = "grado2-arreglos-mayor-menor";
    public const string ArreglosContarParesImparesId =
        "grado2-arreglos-contar-pares-impares";
    public const string ArreglosBuscarValorId =
        "grado2-arreglos-buscar-valor";
    public const string ArreglosInvertirId = "grado2-arreglos-invertir";
    public const string ArreglosIntercalarId = "grado2-arreglos-intercalar";
    public const string ArreglosSinDuplicadosId =
        "grado2-arreglos-sin-duplicados";
    public const string ArreglosOrdenarSegundoMayorId =
        "grado2-arreglos-ordenar-segundo-mayor";

    private const int PuntosPorCasoArreglos = 12;

    private static readonly string[] EtiquetasColeccionArreglo = {
        "Arreglo",
        "Elementos",
        "Valores",
        "Datos",
        "Contenido"
    };

    private static DefinicionEvaluacionPractica CrearArreglosCapturarMostrar(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionArreglos(
            ArreglosCapturarMostrarId,
            "Capturar y mostrar un arreglo",
            "Capturar enteros y mostrarlos en el mismo orden.",
            "Se comprobará una colección etiquetada con cantidad, orden y valores exactos.",
            "La primera línea contiene n (1 a 10) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Mostrar exactamente n valores.",
                "Conservar el orden de entrada.",
                "Conservar duplicados, negativos y cero.",
                "Identificar la colección con una etiqueta clara."
            },
            new[] {
                CrearCasoColeccionArreglos(
                    "arreglos-capturar-mixto",
                    "Negativos, duplicados y cero",
                    EntradaArreglo(3D, -1D, 3D, 0D),
                    "Arreglo: 3 -1 3 0",
                    "Comprueba cuatro valores variados y conserva su orden.",
                    new[] { 3D, -1D, 3D, 0D },
                    EtiquetasColeccionArreglo,
                    visible: true),
                CrearCasoColeccionArreglos(
                    "arreglos-capturar-un-elemento",
                    "Un solo elemento",
                    EntradaArreglo(8D),
                    "Elementos: 8",
                    "Comprueba el tamaño mínimo.",
                    new[] { 8D },
                    EtiquetasColeccionArreglo,
                    visible: true),
                CrearCasoColeccionArreglos(
                    "arreglos-capturar-ceros",
                    "Valores repetidos en cero",
                    EntradaArreglo(0D, 0D, 0D, 0D, 0D),
                    "Valores: 0 0 0 0 0",
                    "Comprueba que los duplicados no se eliminen.",
                    new[] { 0D, 0D, 0D, 0D, 0D },
                    EtiquetasColeccionArreglo,
                    visible: true),
                CrearCasoColeccionArreglos(
                    "arreglos-capturar-seis",
                    "Seis enteros variados",
                    EntradaArreglo(-5D, 10D, -2D, 7D, 7D, 1D),
                    "Datos: -5 10 -2 7 7 1",
                    "Comprueba una colección más larga.",
                    new[] { -5D, 10D, -2D, 7D, 7D, 1D },
                    EtiquetasColeccionArreglo,
                    visible: true),
                CrearCasoColeccionArreglos(
                    "arreglos-capturar-diez-oculto",
                    "Límite superior",
                    EntradaArreglo(9D, 8D, 7D, 6D, 5D, 4D, 3D, 2D, 1D, 0D),
                    "Arreglo: 9 8 7 6 5 4 3 2 1 0",
                    "Comprueba el límite superior sin revelar sus valores en la interfaz.",
                    new[] { 9D, 8D, 7D, 6D, 5D, 4D, 3D, 2D, 1D, 0D },
                    EtiquetasColeccionArreglo,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosSumaElementos(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Suma",
            "Total",
            "Resultado",
            "Acumulado",
            "Suma total"
        };
        return CrearDefinicionArreglos(
            ArreglosSumaElementosId,
            "Suma de elementos",
            "Sumar todos los enteros almacenados en un arreglo.",
            "Se comprobará un único total etiquetado, incluidos negativos y la colección vacía.",
            "La primera línea contiene n (0 a 20) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Sumar todos los elementos una sola vez.",
                "Mostrar cero cuando n sea cero.",
                "Identificar el resultado con una etiqueta reconocible."
            },
            new[] {
                CrearCasoEscalarArreglos(
                    "arreglos-suma-mixta",
                    "Suma con positivos y negativos",
                    EntradaArreglo(5D, -2D, 7D, 0D),
                    "Suma total: 10",
                    "Comprueba una suma de cuatro elementos.",
                    new[] { NumeroArreglos("Suma", 10D, 0D, etiquetas) },
                    visible: true),
                CrearCasoEscalarArreglos(
                    "arreglos-suma-vacia",
                    "Colección vacía",
                    EntradaArreglo(),
                    "Suma: 0",
                    "Comprueba que el acumulador conserve cero.",
                    new[] { NumeroArreglos("Suma", 0D, 0D, etiquetas) },
                    visible: true),
                CrearCasoEscalarArreglos(
                    "arreglos-suma-negativos",
                    "Solo valores negativos",
                    EntradaArreglo(-3D, -2D, -1D, -4D, -5D),
                    "Total: -15",
                    "Comprueba que los signos se conserven.",
                    new[] { NumeroArreglos("Suma", -15D, 0D, etiquetas) },
                    visible: true),
                CrearCasoEscalarArreglos(
                    "arreglos-suma-un-elemento",
                    "Un solo elemento",
                    EntradaArreglo(100D),
                    "Resultado: 100",
                    "Comprueba el tamaño mínimo no vacío.",
                    new[] { NumeroArreglos("Suma", 100D, 0D, etiquetas) },
                    visible: true),
                CrearCasoEscalarArreglos(
                    "arreglos-suma-cancelada-oculto",
                    "Suma cancelada",
                    EntradaArreglo(10D, -10D, 5D, -5D, 2D, -2D, 1D, -1D),
                    "Acumulado: 0",
                    "Comprueba cancelaciones positivas y negativas.",
                    new[] { NumeroArreglos("Suma", 0D, 0D, etiquetas) },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosPromedio(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Promedio",
            "Media",
            "Resultado",
            "Valor promedio",
            "Promedio final"
        };
        return CrearDefinicionArreglos(
            ArreglosPromedioId,
            "Promedio del arreglo",
            "Calcular la media aritmética de valores decimales.",
            "Se comprobará el promedio con tolerancia de 0.01 y una etiqueta reconocible.",
            "La primera línea contiene n (1 a 10) y las siguientes n líneas contienen valores double.",
            new[] { "Cantidad n", "n valores decimales" },
            new[] {
                "Sumar los n valores.",
                "Dividir entre n sin perder decimales.",
                "Mostrar el promedio con tolerancia de 0.01."
            },
            new[] {
                CrearCasoPromedioArreglos(
                    "arreglos-promedio-decimal",
                    "Promedio decimal",
                    new[] { 8.5D, 7D, 9D },
                    8.1666666667D,
                    etiquetas,
                    visible: true),
                CrearCasoPromedioArreglos(
                    "arreglos-promedio-un-elemento",
                    "Un solo valor",
                    new[] { 6.75D },
                    6.75D,
                    etiquetas,
                    visible: true),
                CrearCasoPromedioArreglos(
                    "arreglos-promedio-ceros",
                    "Todos los valores en cero",
                    new[] { 0D, 0D, 0D, 0D },
                    0D,
                    etiquetas,
                    visible: true),
                CrearCasoPromedioArreglos(
                    "arreglos-promedio-simetrico",
                    "Valores simétricos",
                    new[] { -2D, -1D, 0D, 1D, 2D },
                    0D,
                    etiquetas,
                    visible: true),
                CrearCasoPromedioArreglos(
                    "arreglos-promedio-seis-oculto",
                    "Seis valores decimales",
                    new[] { 10D, 9.5D, 8D, 7.5D, 6D, 5D },
                    7.6666666667D,
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosMayorMenor(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasMayor = {
            "Mayor",
            "Máximo",
            "Maximo",
            "Valor mayor",
            "Número mayor"
        };
        string[] etiquetasMenor = {
            "Menor",
            "Mínimo",
            "Minimo",
            "Valor menor",
            "Número menor"
        };
        return CrearDefinicionArreglos(
            ArreglosMayorMenorId,
            "Mayor y menor elemento",
            "Encontrar los valores máximo y mínimo de un arreglo.",
            "Se comprobarán dos extremos etiquetados y coherentes, incluso con números negativos.",
            "La primera línea contiene n (1 a 15) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Mostrar el mayor.",
                "Mostrar el menor.",
                "Tratar un único elemento como ambos extremos."
            },
            new[] {
                CrearCasoMayorMenorArreglos(
                    "arreglos-extremos-duplicados",
                    "Máximo duplicado",
                    new[] { 3D, 9D, -1D, 9D },
                    9D,
                    -1D,
                    etiquetasMayor,
                    etiquetasMenor,
                    visible: true),
                CrearCasoMayorMenorArreglos(
                    "arreglos-extremos-un-elemento",
                    "Un solo valor",
                    new[] { -7D },
                    -7D,
                    -7D,
                    etiquetasMayor,
                    etiquetasMenor,
                    visible: true),
                CrearCasoMayorMenorArreglos(
                    "arreglos-extremos-negativos",
                    "Solo valores negativos",
                    new[] { -10D, -3D, -20D, -1D, -4D },
                    -1D,
                    -20D,
                    etiquetasMayor,
                    etiquetasMenor,
                    visible: true),
                CrearCasoMayorMenorArreglos(
                    "arreglos-extremos-cero",
                    "Mínimo igual a cero",
                    new[] { 0D, 4D, 2D, 8D, 1D, 3D },
                    8D,
                    0D,
                    etiquetasMayor,
                    etiquetasMenor,
                    visible: true),
                CrearCasoMayorMenorArreglos(
                    "arreglos-extremos-amplios-oculto",
                    "Extremos repetidos",
                    new[] { 100D, 50D, 100D, -100D, 0D, 25D, -100D, 75D },
                    100D,
                    -100D,
                    etiquetasMayor,
                    etiquetasMenor,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosContarParesImpares(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasPares = {
            "Pares",
            "Cantidad de pares",
            "Números pares",
            "Total pares"
        };
        string[] etiquetasImpares = {
            "Impares",
            "Cantidad de impares",
            "Números impares",
            "Total impares"
        };
        return CrearDefinicionArreglos(
            ArreglosContarParesImparesId,
            "Contar pares e impares",
            "Contar los enteros pares e impares de un arreglo.",
            "Se comprobarán ambos contadores; cero se considera par y los negativos se clasifican normalmente.",
            "La primera línea contiene n (0 a 20) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Contar cero como par.",
                "Clasificar también números negativos.",
                "Mostrar por separado las cantidades de pares e impares."
            },
            new[] {
                CrearCasoParesImparesArreglos(
                    "arreglos-paridad-mixta",
                    "Valores mixtos",
                    new[] { -3D, -2D, 0D, 7D },
                    2,
                    2,
                    etiquetasPares,
                    etiquetasImpares,
                    visible: true),
                CrearCasoParesImparesArreglos(
                    "arreglos-paridad-vacia",
                    "Colección vacía",
                    Array.Empty<double>(),
                    0,
                    0,
                    etiquetasPares,
                    etiquetasImpares,
                    visible: true),
                CrearCasoParesImparesArreglos(
                    "arreglos-paridad-solo-pares",
                    "Solo pares",
                    new[] { 2D, 4D, 6D, 8D, 10D },
                    5,
                    0,
                    etiquetasPares,
                    etiquetasImpares,
                    visible: true),
                CrearCasoParesImparesArreglos(
                    "arreglos-paridad-solo-impares",
                    "Solo impares",
                    new[] { 1D, 3D, 5D, 7D, 9D },
                    0,
                    5,
                    etiquetasPares,
                    etiquetasImpares,
                    visible: true),
                CrearCasoParesImparesArreglos(
                    "arreglos-paridad-negativos-oculto",
                    "Negativos y cero",
                    new[] { -8D, -5D, -2D, -1D, 0D, 3D, 6D, 11D },
                    4,
                    4,
                    etiquetasPares,
                    etiquetasImpares,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosBuscarValor(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionArreglos(
            ArreglosBuscarValorId,
            "Buscar un valor",
            "Localizar un objetivo y reportar existencia, primer índice y frecuencia.",
            "Se comprobará una búsqueda lineal observable con índices base cero y equivalencias booleanas.",
            "La entrada contiene n (1 a 20), n enteros y finalmente el valor objetivo.",
            new[] { "Cantidad n", "n valores enteros", "Valor objetivo" },
            new[] {
                "Indicar si el objetivo existe.",
                "Mostrar el primer índice en base cero.",
                "Mostrar el total de apariciones.",
                "Usar índice -1 y frecuencia 0 cuando no exista."
            },
            new[] {
                CrearCasoBusquedaArreglos(
                    "arreglos-busqueda-repetido",
                    "Objetivo repetido",
                    new[] { 4D, 2D, 4D, 9D },
                    4,
                    encontrado: true,
                    indice: 0,
                    apariciones: 2,
                    visible: true),
                CrearCasoBusquedaArreglos(
                    "arreglos-busqueda-ausente",
                    "Objetivo ausente",
                    new[] { 5D, 7D, 9D },
                    2,
                    encontrado: false,
                    indice: -1,
                    apariciones: 0,
                    visible: true),
                CrearCasoBusquedaArreglos(
                    "arreglos-busqueda-final",
                    "Objetivo al final",
                    new[] { 8D, 1D, 2D, 3D },
                    3,
                    encontrado: true,
                    indice: 3,
                    apariciones: 1,
                    visible: true),
                CrearCasoBusquedaArreglos(
                    "arreglos-busqueda-negativo",
                    "Objetivo negativo repetido",
                    new[] { -5D, -5D, -5D, 0D },
                    -5,
                    encontrado: true,
                    indice: 0,
                    apariciones: 3,
                    visible: true),
                CrearCasoBusquedaArreglos(
                    "arreglos-busqueda-intermedia-oculto",
                    "Objetivo repetido en posiciones intermedias",
                    new[] { 1D, 2D, 3D, 2D, 4D, 2D, 5D },
                    2,
                    encontrado: true,
                    indice: 1,
                    apariciones: 3,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosInvertir(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Invertido",
            "Arreglo invertido",
            "Resultado",
            "Orden inverso",
            "Valores invertidos"
        };
        return CrearDefinicionArreglos(
            ArreglosInvertirId,
            "Invertir un arreglo",
            "Mostrar los elementos de un arreglo en orden inverso.",
            "Se comprobará una colección etiquetada con orden y cantidad exactos.",
            "La primera línea contiene n (1 a 12) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Mostrar los mismos valores en orden inverso.",
                "No ordenar ni eliminar duplicados.",
                "No agregar otros valores numéricos a la colección."
            },
            new[] {
                CrearCasoColeccionConEntradaArreglos(
                    "arreglos-invertir-cuatro",
                    "Cuatro valores",
                    new[] { 1D, 4D, -2D, 8D },
                    new[] { 8D, -2D, 4D, 1D },
                    etiquetas,
                    visible: true),
                CrearCasoColeccionConEntradaArreglos(
                    "arreglos-invertir-un-elemento",
                    "Un solo elemento",
                    new[] { 7D },
                    new[] { 7D },
                    etiquetas,
                    visible: true),
                CrearCasoColeccionConEntradaArreglos(
                    "arreglos-invertir-palindromo",
                    "Secuencia simétrica",
                    new[] { 1D, 2D, 1D, 2D, 1D },
                    new[] { 1D, 2D, 1D, 2D, 1D },
                    etiquetas,
                    visible: true),
                CrearCasoColeccionConEntradaArreglos(
                    "arreglos-invertir-tres",
                    "Negativo, cero y positivo",
                    new[] { -3D, 0D, 5D },
                    new[] { 5D, 0D, -3D },
                    etiquetas,
                    visible: true),
                CrearCasoColeccionConEntradaArreglos(
                    "arreglos-invertir-seis-oculto",
                    "Secuencia descendente",
                    new[] { 10D, 9D, 8D, 7D, 6D, 5D },
                    new[] { 5D, 6D, 7D, 8D, 9D, 10D },
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosIntercalar(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Intercalado",
            "Combinado",
            "Resultado",
            "Arreglo combinado",
            "Valores intercalados"
        };
        return CrearDefinicionArreglos(
            ArreglosIntercalarId,
            "Intercalar dos arreglos",
            "Alternar los elementos correspondientes de dos arreglos del mismo tamaño.",
            "Se comprobarán orden, alternancia y longitud exacta de la colección resultante.",
            "La entrada contiene n (1 a 8), después n enteros de A y n enteros de B.",
            new[] { "Cantidad n", "n valores de A", "n valores de B" },
            new[] {
                "Comenzar con A[0].",
                "Alternar A[i] y B[i].",
                "Mostrar exactamente 2 × n elementos."
            },
            new[] {
                CrearCasoIntercaladoArreglos(
                    "arreglos-intercalar-tres",
                    "Dos arreglos crecientes",
                    new[] { 1D, 2D, 3D },
                    new[] { 8D, 9D, 10D },
                    new[] { 1D, 8D, 2D, 9D, 3D, 10D },
                    etiquetas,
                    visible: true),
                CrearCasoIntercaladoArreglos(
                    "arreglos-intercalar-un-elemento",
                    "Una pareja",
                    new[] { 5D },
                    new[] { -5D },
                    new[] { 5D, -5D },
                    etiquetas,
                    visible: true),
                CrearCasoIntercaladoArreglos(
                    "arreglos-intercalar-ceros-unos",
                    "Valores repetidos",
                    new[] { 0D, 0D, 0D },
                    new[] { 1D, 1D, 1D },
                    new[] { 0D, 1D, 0D, 1D, 0D, 1D },
                    etiquetas,
                    visible: true),
                CrearCasoIntercaladoArreglos(
                    "arreglos-intercalar-signos",
                    "Valores con signo",
                    new[] { -3D, -2D, -1D, 0D },
                    new[] { 3D, 2D, 1D, 0D },
                    new[] { -3D, 3D, -2D, 2D, -1D, 1D, 0D, 0D },
                    etiquetas,
                    visible: true),
                CrearCasoIntercaladoArreglos(
                    "arreglos-intercalar-cinco-oculto",
                    "Cinco parejas",
                    new[] { 10D, 20D, 30D, 40D, 50D },
                    new[] { 1D, 2D, 3D, 4D, 5D },
                    new[] { 10D, 1D, 20D, 2D, 30D, 3D, 40D, 4D, 50D, 5D },
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosSinDuplicados(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasColeccion = {
            "Únicos",
            "Unicos",
            "Valores únicos",
            "Valores unicos",
            "Sin duplicados",
            "Resultado"
        };
        string[] etiquetasCantidad = {
            "Cantidad",
            "Total únicos",
            "Total unicos",
            "Número de únicos",
            "Numero de unicos"
        };
        return CrearDefinicionArreglos(
            ArreglosSinDuplicadosId,
            "Valores únicos",
            "Eliminar duplicados conservando el orden de primera aparición.",
            "Se comprobarán la colección sin duplicados y su cantidad total.",
            "La primera línea contiene n (0 a 15) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Conservar la primera aparición de cada valor.",
                "No ordenar la colección resultante.",
                "Mostrar la cantidad de valores únicos.",
                "Aceptar una colección vacía."
            },
            new[] {
                CrearCasoUnicosArreglos(
                    "arreglos-unicos-repetidos",
                    "Duplicados no consecutivos",
                    new[] { 4D, 2D, 4D, 3D, 2D },
                    new[] { 4D, 2D, 3D },
                    3,
                    etiquetasColeccion,
                    etiquetasCantidad,
                    visible: true),
                CrearCasoUnicosArreglos(
                    "arreglos-unicos-vacio",
                    "Colección vacía",
                    Array.Empty<double>(),
                    Array.Empty<double>(),
                    0,
                    etiquetasColeccion,
                    etiquetasCantidad,
                    visible: true),
                CrearCasoUnicosArreglos(
                    "arreglos-unicos-uno",
                    "Un solo valor distinto",
                    new[] { 7D, 7D, 7D, 7D },
                    new[] { 7D },
                    1,
                    etiquetasColeccion,
                    etiquetasCantidad,
                    visible: true),
                CrearCasoUnicosArreglos(
                    "arreglos-unicos-todos",
                    "Todos distintos",
                    new[] { 1D, 2D, 3D, 4D, 5D },
                    new[] { 1D, 2D, 3D, 4D, 5D },
                    5,
                    etiquetasColeccion,
                    etiquetasCantidad,
                    visible: true),
                CrearCasoUnicosArreglos(
                    "arreglos-unicos-signos-oculto",
                    "Repetidos con signos y cero",
                    new[] { -1D, 0D, -1D, 2D, 0D, 3D, 2D, 4D },
                    new[] { -1D, 0D, 2D, 3D, 4D },
                    5,
                    etiquetasColeccion,
                    etiquetasCantidad,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArreglosOrdenarSegundoMayor(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasColeccion = {
            "Ordenado",
            "Arreglo ordenado",
            "Resultado ordenado",
            "Ascendente"
        };
        string[] etiquetasSegundo = {
            "Segundo mayor",
            "Segundo mayor distinto",
            "Segundo máximo",
            "Segundo maximo"
        };
        return CrearDefinicionArreglos(
            ArreglosOrdenarSegundoMayorId,
            "Ordenar y encontrar el segundo mayor",
            "Ordenar ascendentemente y encontrar el segundo mayor valor distinto.",
            "Se comprobarán la colección ordenada completa y el segundo mayor distinto o su ausencia.",
            "La primera línea contiene n (2 a 15) y las siguientes n líneas contienen enteros.",
            new[] { "Cantidad n", "n valores enteros" },
            new[] {
                "Ordenar ascendentemente sin eliminar duplicados.",
                "Encontrar el segundo mayor valor distinto.",
                "Indicar claramente cuando no exista."
            },
            new[] {
                CrearCasoOrdenSegundoArreglos(
                    "arreglos-orden-segundo-duplicado",
                    "Máximo duplicado",
                    new[] { 4D, 9D, 1D, 9D, 6D },
                    new[] { 1D, 4D, 6D, 9D, 9D },
                    6D,
                    existeSegundo: true,
                    etiquetasColeccion,
                    etiquetasSegundo,
                    visible: true),
                CrearCasoOrdenSegundoArreglos(
                    "arreglos-orden-segundo-inexistente",
                    "Todos los valores iguales",
                    new[] { 5D, 5D },
                    new[] { 5D, 5D },
                    0D,
                    existeSegundo: false,
                    etiquetasColeccion,
                    etiquetasSegundo,
                    visible: true),
                CrearCasoOrdenSegundoArreglos(
                    "arreglos-orden-segundo-negativos",
                    "Valores negativos",
                    new[] { -10D, -3D, -20D, -3D },
                    new[] { -20D, -10D, -3D, -3D },
                    -10D,
                    existeSegundo: true,
                    etiquetasColeccion,
                    etiquetasSegundo,
                    visible: true),
                CrearCasoOrdenSegundoArreglos(
                    "arreglos-orden-segundo-varios",
                    "Varios duplicados",
                    new[] { 1D, 2D, 2D, 3D, 3D, 4D },
                    new[] { 1D, 2D, 2D, 3D, 3D, 4D },
                    3D,
                    existeSegundo: true,
                    etiquetasColeccion,
                    etiquetasSegundo,
                    visible: true),
                CrearCasoOrdenSegundoArreglos(
                    "arreglos-orden-segundo-amplio-oculto",
                    "Colección amplia con duplicados",
                    new[] { 100D, 50D, 100D, 25D, 75D, 75D, 10D },
                    new[] { 10D, 25D, 50D, 75D, 75D, 100D, 100D },
                    75D,
                    existeSegundo: true,
                    etiquetasColeccion,
                    etiquetasSegundo,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearDefinicionArreglos(
        string id,
        string nombre,
        string objetivo,
        string descripcion,
        string contratoEntrada,
        string[] camposEntrada,
        string[] validaciones,
        CasoPrueba[] casos,
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = id,
            NombrePractica = nombre,
            Objetivo = objetivo,
            Descripcion = descripcion,
            ContratoEntrada = contratoEntrada,
            CamposEntrada = Array.AsReadOnly(camposEntrada),
            ValidacionesRequeridas = Array.AsReadOnly(validaciones),
            CasosPrueba = Array.AsReadOnly(casos),
            Criterios = rubrica
        };
    }

    private static CasoPrueba CrearCasoEscalarArreglos(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        ValorNumericoEsperado[] numeros,
        bool visible,
        ValorBooleanoEsperado[]? booleanos = null,
        ValorTextualEsperado[]? textos = null) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = salidaEsperada,
            EsVisible = visible,
            Puntos = PuntosPorCasoArreglos,
            ComparacionFlexible = true,
            ModoComparacion = textos is { Length: > 0 }
                ? ModoComparacionCaso.Mixto
                : ModoComparacionCaso.Valores,
            Descripcion = descripcion,
            ValoresNumericosEsperados = Array.AsReadOnly(numeros),
            ValoresBooleanosEsperados = Array.AsReadOnly(
                booleanos ?? Array.Empty<ValorBooleanoEsperado>()),
            ValoresTextualesEsperados = Array.AsReadOnly(
                textos ?? Array.Empty<ValorTextualEsperado>())
        };
    }

    private static CasoPrueba CrearCasoColeccionArreglos(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        double[] esperados,
        string[] etiquetas,
        bool visible,
        ValorNumericoEsperado[]? numeros = null,
        ValorTextualEsperado[]? textos = null) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = salidaEsperada,
            EsVisible = visible,
            Puntos = PuntosPorCasoArreglos,
            ComparacionFlexible = true,
            ModoComparacion = textos is { Length: > 0 }
                ? ModoComparacionCaso.Mixto
                : ModoComparacionCaso.Valores,
            Descripcion = descripcion,
            ValoresNumericosEsperados = Array.AsReadOnly(
                numeros ?? Array.Empty<ValorNumericoEsperado>()),
            ValoresTextualesEsperados = Array.AsReadOnly(
                textos ?? Array.Empty<ValorTextualEsperado>()),
            ColeccionesEsperadas = Array.AsReadOnly(new[] {
                ColeccionNumericaArreglos(
                    "Elementos del arreglo",
                    esperados,
                    etiquetas)
            })
        };
    }

    private static CasoPrueba CrearCasoPromedioArreglos(
        string id,
        string nombre,
        double[] entrada,
        double promedio,
        string[] etiquetas,
        bool visible) {
        string promedioTexto = promedio.ToString("G8", CultureInfo.InvariantCulture);
        return CrearCasoEscalarArreglos(
            id,
            nombre,
            EntradaArreglo(entrada),
            $"Promedio: {promedioTexto}",
            "Comprueba la media aritmética sin perder decimales.",
            new[] {
                NumeroArreglos("Promedio", promedio, 0.01D, etiquetas)
            },
            visible);
    }

    private static CasoPrueba CrearCasoMayorMenorArreglos(
        string id,
        string nombre,
        double[] entrada,
        double mayor,
        double menor,
        string[] etiquetasMayor,
        string[] etiquetasMenor,
        bool visible) {
        return CrearCasoEscalarArreglos(
            id,
            nombre,
            EntradaArreglo(entrada),
            $"Mayor: {FormatearNumeroArreglos(mayor)}\n" +
            $"Menor: {FormatearNumeroArreglos(menor)}",
            "Comprueba ambos extremos del arreglo.",
            new[] {
                NumeroArreglos("Mayor", mayor, 0D, etiquetasMayor),
                NumeroArreglos("Menor", menor, 0D, etiquetasMenor)
            },
            visible);
    }

    private static CasoPrueba CrearCasoParesImparesArreglos(
        string id,
        string nombre,
        double[] entrada,
        int pares,
        int impares,
        string[] etiquetasPares,
        string[] etiquetasImpares,
        bool visible) {
        return CrearCasoEscalarArreglos(
            id,
            nombre,
            EntradaArreglo(entrada),
            $"Pares: {pares}\nImpares: {impares}",
            "Comprueba las dos clasificaciones, incluido el cero.",
            new[] {
                NumeroArreglos("Pares", pares, 0D, etiquetasPares),
                NumeroArreglos("Impares", impares, 0D, etiquetasImpares)
            },
            visible);
    }

    private static CasoPrueba CrearCasoBusquedaArreglos(
        string id,
        string nombre,
        double[] entrada,
        int objetivo,
        bool encontrado,
        int indice,
        int apariciones,
        bool visible) {
        string entradaCompleta = EntradaArreglo(entrada) +
            objetivo.ToString(CultureInfo.InvariantCulture) + "\n";
        return CrearCasoEscalarArreglos(
            id,
            nombre,
            entradaCompleta,
            $"Encontrado: {(encontrado ? "sí" : "no")}\n" +
            $"Primer índice: {indice}\n" +
            $"Apariciones: {apariciones}",
            "Comprueba existencia, primera posición base cero y frecuencia total.",
            new[] {
                NumeroArreglos(
                    "Primer índice",
                    indice,
                    0D,
                    "Primer índice",
                    "Primer indice",
                    "Índice",
                    "Indice",
                    "Posición",
                    "Posicion"),
                NumeroArreglos(
                    "Apariciones",
                    apariciones,
                    0D,
                    "Apariciones",
                    "Frecuencia",
                    "Veces",
                    "Cantidad")
            },
            visible,
            booleanos: new[] {
                new ValorBooleanoEsperado {
                    Nombre = "Encontrado",
                    Valor = encontrado,
                    EtiquetasAlternativas = Array.AsReadOnly(new[] {
                        "Encontrado",
                        "Existe",
                        "Se encontró",
                        "Hallado"
                    })
                }
            });
    }

    private static CasoPrueba CrearCasoColeccionConEntradaArreglos(
        string id,
        string nombre,
        double[] entrada,
        double[] esperado,
        string[] etiquetas,
        bool visible) {
        return CrearCasoColeccionArreglos(
            id,
            nombre,
            EntradaArreglo(entrada),
            $"{etiquetas[0]}: {UnirNumerosArreglos(esperado)}",
            "Comprueba la colección resultante con orden y cantidad exactos.",
            esperado,
            etiquetas,
            visible);
    }

    private static CasoPrueba CrearCasoIntercaladoArreglos(
        string id,
        string nombre,
        double[] arregloA,
        double[] arregloB,
        double[] esperado,
        string[] etiquetas,
        bool visible) {
        string entrada = arregloA.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                arregloA.Concat(arregloB).Select(FormatearNumeroArreglos)) +
            "\n";
        return CrearCasoColeccionArreglos(
            id,
            nombre,
            entrada,
            $"Intercalado: {UnirNumerosArreglos(esperado)}",
            "Comprueba la alternancia exacta entre ambos arreglos.",
            esperado,
            etiquetas,
            visible);
    }

    private static CasoPrueba CrearCasoUnicosArreglos(
        string id,
        string nombre,
        double[] entrada,
        double[] unicos,
        int cantidad,
        string[] etiquetasColeccion,
        string[] etiquetasCantidad,
        bool visible) {
        return CrearCasoColeccionArreglos(
            id,
            nombre,
            EntradaArreglo(entrada),
            $"Valores únicos: {UnirNumerosArreglos(unicos)}\n" +
            $"Cantidad: {cantidad}",
            "Comprueba los valores únicos en orden de aparición y su cantidad.",
            unicos,
            etiquetasColeccion,
            visible,
            numeros: new[] {
                NumeroArreglos(
                    "Cantidad de únicos",
                    cantidad,
                    0D,
                    etiquetasCantidad)
            });
    }

    private static CasoPrueba CrearCasoOrdenSegundoArreglos(
        string id,
        string nombre,
        double[] entrada,
        double[] ordenado,
        double segundoMayor,
        bool existeSegundo,
        string[] etiquetasColeccion,
        string[] etiquetasSegundo,
        bool visible) {
        ValorNumericoEsperado[] numeros = existeSegundo
            ? new[] {
                NumeroArreglos(
                    "Segundo mayor",
                    segundoMayor,
                    0D,
                    etiquetasSegundo)
            }
            : Array.Empty<ValorNumericoEsperado>();
        ValorTextualEsperado[] textos = existeSegundo
            ? Array.Empty<ValorTextualEsperado>()
            : new[] {
                new ValorTextualEsperado {
                    Nombre = "Segundo mayor",
                    Valor = "No existe",
                    EtiquetasAlternativas = Array.AsReadOnly(etiquetasSegundo),
                    Opciones = Array.AsReadOnly(new[] {
                        new OpcionValorTextual {
                            Valor = "No existe",
                            Alternativas = Array.AsReadOnly(new[] {
                                "No existe",
                                "No hay segundo mayor",
                                "Sin segundo mayor",
                                "Todos los valores son iguales"
                            })
                        }
                    })
                }
            };
        string resultado = existeSegundo
            ? FormatearNumeroArreglos(segundoMayor)
            : "No existe";

        return CrearCasoColeccionArreglos(
            id,
            nombre,
            EntradaArreglo(entrada),
            $"Arreglo ordenado: {UnirNumerosArreglos(ordenado)}\n" +
            $"Segundo mayor: {resultado}",
            "Comprueba el orden ascendente y el segundo mayor distinto.",
            ordenado,
            etiquetasColeccion,
            visible,
            numeros,
            textos);
    }

    private static ReglaColeccionEsperada ColeccionNumericaArreglos(
        string nombre,
        double[] esperados,
        string[] etiquetas) {
        return new ReglaColeccionEsperada {
            Nombre = nombre,
            TipoElementos = TipoValorEstructurado.Numerico,
            ElementosEsperados = Array.AsReadOnly(esperados
                .Select(valor => new ValorEstructuradoEsperado {
                    Nombre = "Elemento",
                    Tipo = TipoValorEstructurado.Numerico,
                    ValorNumerico = valor,
                    ToleranciaNumerica = 0D
                })
                .ToArray()),
            EtiquetasInicio = Array.AsReadOnly(etiquetas),
            Region = ModoRegionColeccion.MismaLineaTrasEtiqueta,
            RequerirEtiqueta = true,
            OrdenObligatorio = true,
            CantidadExacta = esperados.Length,
            PermitirDuplicados = esperados
                .GroupBy(valor => valor)
                .Any(grupo => grupo.Count() > 1),
            PermitirElementosAdicionales = false,
            ToleranciaNumerica = 0D,
            ConsumirAparicionesUnaVez = true,
            MensajeError =
                "La colección debe conservar exactamente los valores y el orden solicitados."
        };
    }

    private static ValorNumericoEsperado NumeroArreglos(
        string nombre,
        double valor,
        double tolerancia,
        params string[] etiquetas) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            Valor = valor,
            Tolerancia = tolerancia,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas)
        };
    }

    private static string EntradaArreglo(params double[] valores) {
        return valores.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            (valores.Length == 0
                ? string.Empty
                : string.Join("\n", valores.Select(FormatearNumeroArreglos)) + "\n");
    }

    private static string UnirNumerosArreglos(IEnumerable<double> valores) {
        return string.Join(" ", valores.Select(FormatearNumeroArreglos));
    }

    private static string FormatearNumeroArreglos(double valor) {
        return valor.ToString("G15", CultureInfo.InvariantCulture);
    }
}
