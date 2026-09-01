using EndForge.Models;
using System.Globalization;

namespace EndForge.Services;

public sealed partial class CatalogoEvaluacionesService {
    public const string MatricesCapturarMostrarId =
        "grado2-matrices-capturar-mostrar";
    public const string MatricesSumaElementosId =
        "grado2-matrices-suma-elementos";
    public const string MatricesSumasFilasColumnasId =
        "grado2-matrices-sumas-filas-columnas";
    public const string MatricesDiagonalesId =
        "grado2-matrices-diagonales";
    public const string MatricesTranspuestaId =
        "grado2-matrices-transpuesta";
    public const string MatricesSumarDosId =
        "grado2-matrices-sumar-dos";
    public const string MatricesMultiplicarId =
        "grado2-matrices-multiplicar";
    public const string MatricesMayorMenorPosicionId =
        "grado2-matrices-mayor-menor-posicion";

    private const int PuntosPorCasoMatrices = 12;

    private static DefinicionEvaluacionPractica CrearMatricesCapturarMostrar(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Matriz",
            "Datos",
            "Valores",
            "Contenido"
        };

        return CrearDefinicionMatrices(
            MatricesCapturarMostrarId,
            "Capturar y mostrar una matriz",
            "Capturar una matriz de enteros y conservar sus filas, columnas y posiciones.",
            "Se comprobarán las dimensiones y cada posición de la matriz; una lista plana no es equivalente.",
            "La entrada contiene filas, columnas y exactamente filas × columnas enteros.",
            new[] { "Número de filas", "Número de columnas", "Elementos de la matriz" },
            new[] {
                "Aceptar dimensiones de 1 a 5.",
                "Mostrar exactamente las filas y columnas indicadas.",
                "Conservar el valor de cada posición."
            },
            new[] {
                CrearCasoMatrizMatrices(
                    "matrices-capturar-dos-por-tres",
                    "Matriz rectangular 2×3",
                    EntradaMatrizMatrices(
                        2,
                        3,
                        new[] {
                            new[] { 1D, 2D, 3D },
                            new[] { 4D, 5D, 6D }
                        }),
                    "Comprueba una matriz rectangular con seis elementos.",
                    new[] {
                        new[] { 1D, 2D, 3D },
                        new[] { 4D, 5D, 6D }
                    },
                    etiquetas,
                    requerirEtiqueta: false,
                    visible: true),
                CrearCasoMatrizMatrices(
                    "matrices-capturar-uno-por-uno",
                    "Matriz mínima 1×1",
                    EntradaMatrizMatrices(
                        1,
                        1,
                        new[] {
                            new[] { -7D }
                        }),
                    "Comprueba el tamaño mínimo y un valor negativo.",
                    new[] {
                        new[] { -7D }
                    },
                    etiquetas,
                    requerirEtiqueta: false,
                    visible: true),
                CrearCasoMatrizMatrices(
                    "matrices-capturar-tres-por-dos",
                    "Matriz rectangular 3×2",
                    EntradaMatrizMatrices(
                        3,
                        2,
                        new[] {
                            new[] { 0D, 1D },
                            new[] { 2D, 3D },
                            new[] { 4D, 5D }
                        }),
                    "Comprueba que no se transpongan filas y columnas.",
                    new[] {
                        new[] { 0D, 1D },
                        new[] { 2D, 3D },
                        new[] { 4D, 5D }
                    },
                    etiquetas,
                    requerirEtiqueta: false,
                    visible: true),
                CrearCasoMatrizMatrices(
                    "matrices-capturar-repetidos",
                    "Valores repetidos",
                    EntradaMatrizMatrices(
                        2,
                        2,
                        new[] {
                            new[] { 9D, 9D },
                            new[] { 9D, 9D }
                        }),
                    "Comprueba que los valores repetidos conserven su posición.",
                    new[] {
                        new[] { 9D, 9D },
                        new[] { 9D, 9D }
                    },
                    etiquetas,
                    requerirEtiqueta: false,
                    visible: true),
                CrearCasoMatrizMatrices(
                    "matrices-capturar-cuatro-por-tres-oculto",
                    "Matriz rectangular amplia",
                    EntradaMatrizMatrices(
                        4,
                        3,
                        new[] {
                            new[] { 1D, 0D, -1D },
                            new[] { 2D, 0D, -2D },
                            new[] { 3D, 0D, -3D },
                            new[] { 4D, 0D, -4D }
                        }),
                    "Comprueba una matriz rectangular más amplia sin revelar su contenido.",
                    new[] {
                        new[] { 1D, 0D, -1D },
                        new[] { 2D, 0D, -2D },
                        new[] { 3D, 0D, -3D },
                        new[] { 4D, 0D, -4D }
                    },
                    etiquetas,
                    requerirEtiqueta: false,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesSumaElementos(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Suma",
            "Total",
            "Resultado",
            "Suma total",
            "Acumulado"
        };

        return CrearDefinicionMatrices(
            MatricesSumaElementosId,
            "Sumar todos los elementos de una matriz",
            "Calcular la suma total de los enteros almacenados en una matriz.",
            "Se comprobará un único total etiquetado, incluidos negativos y resultados iguales a cero.",
            "La entrada contiene filas, columnas y los enteros de la matriz en orden por filas.",
            new[] { "Número de filas", "Número de columnas", "Elementos de la matriz" },
            new[] {
                "Incluir todos los elementos una sola vez.",
                "Conservar los signos de los valores.",
                "Mostrar el total con una etiqueta reconocible."
            },
            new[] {
                CrearCasoEscalarMatrices(
                    "matrices-suma-dos-por-dos",
                    "Suma positiva",
                    EntradaMatrizMatrices(
                        2,
                        2,
                        new[] {
                            new[] { 1D, 2D },
                            new[] { 3D, 4D }
                        }),
                    "Suma total: 10",
                    "Comprueba la suma de una matriz 2×2.",
                    new[] {
                        NumeroMatrices("Suma total", 10D, etiquetas)
                    },
                    visible: true),
                CrearCasoEscalarMatrices(
                    "matrices-suma-una-fila",
                    "Una fila con signos distintos",
                    EntradaMatrizMatrices(
                        1,
                        3,
                        new[] {
                            new[] { -2D, 0D, 7D }
                        }),
                    "Resultado: 5",
                    "Comprueba negativos y cero en una sola fila.",
                    new[] {
                        NumeroMatrices("Suma total", 5D, etiquetas)
                    },
                    visible: true),
                CrearCasoEscalarMatrices(
                    "matrices-suma-negativos",
                    "Matriz de negativos",
                    EntradaMatrizMatrices(
                        3,
                        2,
                        new[] {
                            new[] { -1D, -2D },
                            new[] { -3D, -4D },
                            new[] { -5D, -6D }
                        }),
                    "Total: -21",
                    "Comprueba que los signos negativos se conserven.",
                    new[] {
                        NumeroMatrices("Suma total", -21D, etiquetas)
                    },
                    visible: true),
                CrearCasoEscalarMatrices(
                    "matrices-suma-un-elemento",
                    "Un solo elemento",
                    EntradaMatrizMatrices(
                        1,
                        1,
                        new[] {
                            new[] { 100D }
                        }),
                    "Suma: 100",
                    "Comprueba el tamaño mínimo.",
                    new[] {
                        NumeroMatrices("Suma total", 100D, etiquetas)
                    },
                    visible: true),
                CrearCasoEscalarMatrices(
                    "matrices-suma-cancelada-oculto",
                    "Suma cancelada",
                    EntradaMatrizMatrices(
                        3,
                        3,
                        new[] {
                            new[] { 1D, 0D, -1D },
                            new[] { 2D, 0D, -2D },
                            new[] { 3D, 0D, -3D }
                        }),
                    "Acumulado: 0",
                    "Comprueba cancelaciones entre valores positivos y negativos.",
                    new[] {
                        NumeroMatrices("Suma total", 0D, etiquetas)
                    },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesSumasFilasColumnas(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionMatrices(
            MatricesSumasFilasColumnasId,
            "Calcular sumas por filas y columnas",
            "Calcular por separado la suma de cada fila y de cada columna.",
            "Se comprobarán dos tablas delimitadas, ordenadas y con índices en base cero.",
            "La entrada contiene filas, columnas y los enteros de la matriz en orden por filas.",
            new[] { "Número de filas", "Número de columnas", "Elementos de la matriz" },
            new[] {
                "Mostrar una suma por cada fila.",
                "Mostrar una suma por cada columna.",
                "Usar índices base cero y conservar su orden."
            },
            new[] {
                CrearCasoSumasFilasColumnasMatrices(
                    "matrices-sumas-filas-columnas-dos-por-tres",
                    "Matriz 2×3",
                    new[] {
                        new[] { 1D, 2D, 3D },
                        new[] { 4D, 5D, 6D }
                    },
                    new[] { 6D, 15D },
                    new[] { 5D, 7D, 9D },
                    visible: true),
                CrearCasoSumasFilasColumnasMatrices(
                    "matrices-sumas-filas-columnas-una-fila",
                    "Una fila",
                    new[] {
                        new[] { 2D, -2D, 5D, 0D }
                    },
                    new[] { 5D },
                    new[] { 2D, -2D, 5D, 0D },
                    visible: true),
                CrearCasoSumasFilasColumnasMatrices(
                    "matrices-sumas-filas-columnas-una-columna",
                    "Una columna",
                    new[] {
                        new[] { 1D },
                        new[] { 2D },
                        new[] { 3D }
                    },
                    new[] { 1D, 2D, 3D },
                    new[] { 6D },
                    visible: true),
                CrearCasoSumasFilasColumnasMatrices(
                    "matrices-sumas-filas-columnas-negativos",
                    "Valores negativos",
                    new[] {
                        new[] { -1D, -2D },
                        new[] { -3D, -4D }
                    },
                    new[] { -3D, -7D },
                    new[] { -4D, -6D },
                    visible: true),
                CrearCasoSumasFilasColumnasMatrices(
                    "matrices-sumas-filas-columnas-tres-oculto",
                    "Matriz cuadrada amplia",
                    new[] {
                        new[] { 1D, 2D, 3D },
                        new[] { 4D, 5D, 6D },
                        new[] { 7D, 8D, 9D }
                    },
                    new[] { 6D, 15D, 24D },
                    new[] { 12D, 15D, 18D },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesDiagonales(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionMatrices(
            MatricesDiagonalesId,
            "Calcular diagonales de una matriz",
            "Obtener las diagonales principal y secundaria junto con sus sumas.",
            "Se comprobarán dos colecciones ordenadas y dos sumas escalares independientes.",
            "La entrada contiene n y los n × n enteros de una matriz cuadrada.",
            new[] { "Dimensión n", "Elementos de la matriz cuadrada" },
            new[] {
                "Conservar el orden de ambas diagonales.",
                "Incluir el elemento central en las dos diagonales cuando n sea impar.",
                "Mostrar la suma correspondiente a cada diagonal."
            },
            new[] {
                CrearCasoDiagonalesMatrices(
                    "matrices-diagonales-dos",
                    "Matriz 2×2",
                    new[] {
                        new[] { 1D, 2D },
                        new[] { 3D, 4D }
                    },
                    new[] { 1D, 4D },
                    new[] { 2D, 3D },
                    5D,
                    5D,
                    visible: true),
                CrearCasoDiagonalesMatrices(
                    "matrices-diagonales-uno",
                    "Matriz 1×1",
                    new[] {
                        new[] { 7D }
                    },
                    new[] { 7D },
                    new[] { 7D },
                    7D,
                    7D,
                    visible: true),
                CrearCasoDiagonalesMatrices(
                    "matrices-diagonales-tres",
                    "Matriz 3×3",
                    new[] {
                        new[] { 1D, 2D, 3D },
                        new[] { 4D, 5D, 6D },
                        new[] { 7D, 8D, 9D }
                    },
                    new[] { 1D, 5D, 9D },
                    new[] { 3D, 5D, 7D },
                    15D,
                    15D,
                    visible: true),
                CrearCasoDiagonalesMatrices(
                    "matrices-diagonales-negativos",
                    "Matriz con negativos",
                    new[] {
                        new[] { -1D, 0D, 2D },
                        new[] { 3D, -4D, 5D },
                        new[] { 6D, 7D, -8D }
                    },
                    new[] { -1D, -4D, -8D },
                    new[] { 2D, -4D, 6D },
                    -13D,
                    4D,
                    visible: true),
                CrearCasoDiagonalesMatrices(
                    "matrices-diagonales-cuatro-oculto",
                    "Matriz 4×4",
                    new[] {
                        new[] { 1D, 2D, 3D, 4D },
                        new[] { 5D, 6D, 7D, 8D },
                        new[] { 9D, 10D, 11D, 12D },
                        new[] { 13D, 14D, 15D, 16D }
                    },
                    new[] { 1D, 6D, 11D, 16D },
                    new[] { 4D, 7D, 10D, 13D },
                    34D,
                    34D,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesTranspuesta(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Transpuesta",
            "Matriz transpuesta",
            "Resultado",
            "Nueva matriz"
        };

        return CrearDefinicionMatrices(
            MatricesTranspuestaId,
            "Obtener la transpuesta de una matriz",
            "Intercambiar las filas y columnas de una matriz.",
            "Se comprobarán las dimensiones transpuestas y el valor exacto de cada posición.",
            "La entrada contiene filas, columnas y los elementos de la matriz original.",
            new[] { "Número de filas", "Número de columnas", "Elementos de la matriz" },
            new[] {
                "Convertir una matriz de filas × columnas en columnas × filas.",
                "Mover cada elemento [i,j] a [j,i].",
                "Mostrar solamente la matriz transpuesta en la región de resultado."
            },
            new[] {
                CrearCasoMatrizConOrigenMatrices(
                    "matrices-transpuesta-dos-por-tres",
                    "Matriz 2×3",
                    new[] {
                        new[] { 1D, 2D, 3D },
                        new[] { 4D, 5D, 6D }
                    },
                    new[] {
                        new[] { 1D, 4D },
                        new[] { 2D, 5D },
                        new[] { 3D, 6D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoMatrizConOrigenMatrices(
                    "matrices-transpuesta-una-fila",
                    "Matriz 1×4",
                    new[] {
                        new[] { 1D, 2D, 3D, 4D }
                    },
                    new[] {
                        new[] { 1D },
                        new[] { 2D },
                        new[] { 3D },
                        new[] { 4D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoMatrizConOrigenMatrices(
                    "matrices-transpuesta-una-columna",
                    "Matriz 3×1",
                    new[] {
                        new[] { -1D },
                        new[] { 0D },
                        new[] { 7D }
                    },
                    new[] {
                        new[] { -1D, 0D, 7D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoMatrizConOrigenMatrices(
                    "matrices-transpuesta-cuadrada",
                    "Matriz 2×2 no simétrica",
                    new[] {
                        new[] { 1D, 9D },
                        new[] { 3D, 4D }
                    },
                    new[] {
                        new[] { 1D, 3D },
                        new[] { 9D, 4D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoMatrizConOrigenMatrices(
                    "matrices-transpuesta-tres-por-cuatro-oculto",
                    "Matriz rectangular amplia",
                    new[] {
                        new[] { 1D, 2D, 3D, 4D },
                        new[] { 5D, 6D, 7D, 8D },
                        new[] { 9D, 10D, 11D, 12D }
                    },
                    new[] {
                        new[] { 1D, 5D, 9D },
                        new[] { 2D, 6D, 10D },
                        new[] { 3D, 7D, 11D },
                        new[] { 4D, 8D, 12D }
                    },
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesSumarDos(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Suma",
            "Matriz resultante",
            "Resultado",
            "Matriz C"
        };

        return CrearDefinicionMatrices(
            MatricesSumarDosId,
            "Sumar dos matrices",
            "Sumar elemento a elemento dos matrices de iguales dimensiones.",
            "Se comprobarán las dimensiones y cada posición de la matriz resultante.",
            "La entrada contiene filas, columnas, todos los elementos de A y después todos los de B.",
            new[] {
                "Número de filas",
                "Número de columnas",
                "Elementos de A",
                "Elementos de B"
            },
            new[] {
                "Sumar los elementos que ocupan la misma posición.",
                "Conservar las dimensiones originales.",
                "Mostrar únicamente la matriz resultante en su región."
            },
            new[] {
                CrearCasoSumaMatrices(
                    "matrices-sumar-dos-cuadradas",
                    "Matrices 2×2",
                    new[] {
                        new[] { 1D, 2D },
                        new[] { 3D, 4D }
                    },
                    new[] {
                        new[] { 5D, 6D },
                        new[] { 7D, 8D }
                    },
                    new[] {
                        new[] { 6D, 8D },
                        new[] { 10D, 12D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoSumaMatrices(
                    "matrices-sumar-dos-una-fila",
                    "Matrices 1×3",
                    new[] {
                        new[] { -1D, 0D, 5D }
                    },
                    new[] {
                        new[] { 1D, 2D, -5D }
                    },
                    new[] {
                        new[] { 0D, 2D, 0D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoSumaMatrices(
                    "matrices-sumar-dos-una-columna",
                    "Matrices 2×1",
                    new[] {
                        new[] { 10D },
                        new[] { -3D }
                    },
                    new[] {
                        new[] { -4D },
                        new[] { 3D }
                    },
                    new[] {
                        new[] { 6D },
                        new[] { 0D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoSumaMatrices(
                    "matrices-sumar-dos-tres-por-dos",
                    "Matrices 3×2",
                    new[] {
                        new[] { 1D, 2D },
                        new[] { 3D, 4D },
                        new[] { 5D, 6D }
                    },
                    new[] {
                        new[] { 6D, 5D },
                        new[] { 4D, 3D },
                        new[] { 2D, 1D }
                    },
                    new[] {
                        new[] { 7D, 7D },
                        new[] { 7D, 7D },
                        new[] { 7D, 7D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoSumaMatrices(
                    "matrices-sumar-dos-tres-por-tres-oculto",
                    "Matrices 3×3",
                    new[] {
                        new[] { 1D, 0D, -1D },
                        new[] { 2D, 0D, -2D },
                        new[] { 3D, 0D, -3D }
                    },
                    new[] {
                        new[] { -1D, 4D, 1D },
                        new[] { -2D, 5D, 2D },
                        new[] { -3D, 6D, 3D }
                    },
                    new[] {
                        new[] { 0D, 4D, 0D },
                        new[] { 0D, 5D, 0D },
                        new[] { 0D, 6D, 0D }
                    },
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesMultiplicar(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Producto",
            "Resultado",
            "Matriz producto",
            "Matriz C",
            "Multiplicación",
            "Multiplicacion"
        };

        return CrearDefinicionMatrices(
            MatricesMultiplicarId,
            "Multiplicar dos matrices",
            "Calcular el producto matricial de dos matrices compatibles.",
            "Se comprobarán las dimensiones del producto y el valor exacto de cada posición.",
            "La entrada contiene las dimensiones de A, sus elementos, las dimensiones de B y sus elementos.",
            new[] {
                "Filas y columnas de A",
                "Elementos de A",
                "Filas y columnas de B",
                "Elementos de B"
            },
            new[] {
                "Usar matrices con columnas de A iguales a filas de B.",
                "Obtener una matriz de filas de A × columnas de B.",
                "Realizar producto matricial, no multiplicación elemento a elemento."
            },
            new[] {
                CrearCasoProductoMatrices(
                    "matrices-multiplicar-dos-cuadradas",
                    "Producto 2×2 por 2×2",
                    new[] {
                        new[] { 1D, 2D },
                        new[] { 3D, 4D }
                    },
                    new[] {
                        new[] { 5D, 6D },
                        new[] { 7D, 8D }
                    },
                    new[] {
                        new[] { 19D, 22D },
                        new[] { 43D, 50D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoProductoMatrices(
                    "matrices-multiplicar-fila-por-columna",
                    "Producto 1×3 por 3×1",
                    new[] {
                        new[] { 1D, 2D, 3D }
                    },
                    new[] {
                        new[] { 4D },
                        new[] { 5D },
                        new[] { 6D }
                    },
                    new[] {
                        new[] { 32D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoProductoMatrices(
                    "matrices-multiplicar-rectangulares",
                    "Producto 2×3 por 3×2",
                    new[] {
                        new[] { 1D, 0D, 2D },
                        new[] { -1D, 3D, 1D }
                    },
                    new[] {
                        new[] { 3D, 1D },
                        new[] { 2D, 1D },
                        new[] { 1D, 0D }
                    },
                    new[] {
                        new[] { 5D, 1D },
                        new[] { 4D, 2D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoProductoMatrices(
                    "matrices-multiplicar-columna-por-fila",
                    "Producto 3×1 por 1×2",
                    new[] {
                        new[] { 2D },
                        new[] { -1D },
                        new[] { 4D }
                    },
                    new[] {
                        new[] { 3D, 5D }
                    },
                    new[] {
                        new[] { 6D, 10D },
                        new[] { -3D, -5D },
                        new[] { 12D, 20D }
                    },
                    etiquetas,
                    visible: true),
                CrearCasoProductoMatrices(
                    "matrices-multiplicar-oculto",
                    "Producto rectangular oculto",
                    new[] {
                        new[] { 2D, 1D, 0D },
                        new[] { -1D, 3D, 2D }
                    },
                    new[] {
                        new[] { 1D, 4D },
                        new[] { 2D, 0D },
                        new[] { -2D, 5D }
                    },
                    new[] {
                        new[] { 4D, 8D },
                        new[] { 1D, 6D }
                    },
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearMatricesMayorMenorPosicion(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasMayor = {
            "Mayor",
            "Máximo",
            "Maximo",
            "Valor mayor"
        };
        string[] etiquetasFilaMayor = {
            "Fila mayor",
            "Fila del mayor"
        };
        string[] etiquetasColumnaMayor = {
            "Columna mayor",
            "Columna del mayor"
        };
        string[] etiquetasMenor = {
            "Menor",
            "Mínimo",
            "Minimo",
            "Valor menor"
        };
        string[] etiquetasFilaMenor = {
            "Fila menor",
            "Fila del menor"
        };
        string[] etiquetasColumnaMenor = {
            "Columna menor",
            "Columna del menor"
        };

        return CrearDefinicionMatrices(
            MatricesMayorMenorPosicionId,
            "Encontrar mayor y menor con posiciones",
            "Encontrar los extremos de una matriz y la primera posición de cada uno.",
            "Se comprobarán los valores y sus índices en base cero, respetando la primera aparición por filas.",
            "La entrada contiene filas, columnas y los enteros de la matriz en orden por filas.",
            new[] { "Número de filas", "Número de columnas", "Elementos de la matriz" },
            new[] {
                "Mostrar el valor mayor y su primera posición.",
                "Mostrar el valor menor y su primera posición.",
                "Usar índices base cero y recorrer por filas de izquierda a derecha."
            },
            new[] {
                CrearCasoExtremosPosicionMatrices(
                    "matrices-extremos-posicion-duplicado-mayor",
                    "Mayor repetido",
                    new[] {
                        new[] { 3D, 9D },
                        new[] { -1D, 9D }
                    },
                    mayor: 9D,
                    filaMayor: 0,
                    columnaMayor: 1,
                    menor: -1D,
                    filaMenor: 1,
                    columnaMenor: 0,
                    etiquetasMayor,
                    etiquetasFilaMayor,
                    etiquetasColumnaMayor,
                    etiquetasMenor,
                    etiquetasFilaMenor,
                    etiquetasColumnaMenor,
                    visible: true),
                CrearCasoExtremosPosicionMatrices(
                    "matrices-extremos-posicion-un-elemento",
                    "Un solo elemento",
                    new[] {
                        new[] { -7D }
                    },
                    mayor: -7D,
                    filaMayor: 0,
                    columnaMayor: 0,
                    menor: -7D,
                    filaMenor: 0,
                    columnaMenor: 0,
                    etiquetasMayor,
                    etiquetasFilaMayor,
                    etiquetasColumnaMayor,
                    etiquetasMenor,
                    etiquetasFilaMenor,
                    etiquetasColumnaMenor,
                    visible: true),
                CrearCasoExtremosPosicionMatrices(
                    "matrices-extremos-posicion-negativos",
                    "Valores negativos repetidos",
                    new[] {
                        new[] { -10D, -3D, -20D },
                        new[] { -1D, -4D, -1D }
                    },
                    mayor: -1D,
                    filaMayor: 1,
                    columnaMayor: 0,
                    menor: -20D,
                    filaMenor: 0,
                    columnaMenor: 2,
                    etiquetasMayor,
                    etiquetasFilaMayor,
                    etiquetasColumnaMayor,
                    etiquetasMenor,
                    etiquetasFilaMenor,
                    etiquetasColumnaMenor,
                    visible: true),
                CrearCasoExtremosPosicionMatrices(
                    "matrices-extremos-posicion-cero",
                    "Mínimo igual a cero",
                    new[] {
                        new[] { 0D, 4D, 2D },
                        new[] { 8D, 1D, 3D }
                    },
                    mayor: 8D,
                    filaMayor: 1,
                    columnaMayor: 0,
                    menor: 0D,
                    filaMenor: 0,
                    columnaMenor: 0,
                    etiquetasMayor,
                    etiquetasFilaMayor,
                    etiquetasColumnaMayor,
                    etiquetasMenor,
                    etiquetasFilaMenor,
                    etiquetasColumnaMenor,
                    visible: true),
                CrearCasoExtremosPosicionMatrices(
                    "matrices-extremos-posicion-oculto",
                    "Extremos duplicados ocultos",
                    new[] {
                        new[] { 100D, 50D, 100D },
                        new[] { -100D, 0D, 25D },
                        new[] { -100D, 75D, 50D }
                    },
                    mayor: 100D,
                    filaMayor: 0,
                    columnaMayor: 0,
                    menor: -100D,
                    filaMenor: 1,
                    columnaMenor: 0,
                    etiquetasMayor,
                    etiquetasFilaMayor,
                    etiquetasColumnaMayor,
                    etiquetasMenor,
                    etiquetasFilaMenor,
                    etiquetasColumnaMenor,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearDefinicionMatrices(
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

    private static CasoPrueba CrearCasoEscalarMatrices(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        ValorNumericoEsperado[] valores,
        bool visible) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = salidaEsperada,
            EsVisible = visible,
            Puntos = PuntosPorCasoMatrices,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion = descripcion,
            ValoresNumericosEsperados = Array.AsReadOnly(valores)
        };
    }

    private static CasoPrueba CrearCasoMatrizMatrices(
        string id,
        string nombre,
        string entrada,
        string descripcion,
        double[][] resultado,
        string[] etiquetas,
        bool requerirEtiqueta,
        bool visible) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = FormatearMatrizMatrices(etiquetas[0], resultado),
            EsVisible = visible,
            Puntos = PuntosPorCasoMatrices,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion = descripcion,
            MatricesEsperadas = Array.AsReadOnly(new[] {
                MatrizNumericaMatrices(
                    "Matriz resultante",
                    resultado,
                    etiquetas,
                    requerirEtiqueta)
            })
        };
    }

    private static CasoPrueba CrearCasoSumasFilasColumnasMatrices(
        string id,
        string nombre,
        double[][] matriz,
        double[] sumasFilas,
        double[] sumasColumnas,
        bool visible) {
        string salida = "Sumas por fila:\n" +
            string.Join(
                "\n",
                sumasFilas.Select((valor, indice) =>
                    $"Fila {indice}: {FormatearNumeroMatrices(valor)}")) +
            "\nSumas por columna:\n" +
            string.Join(
                "\n",
                sumasColumnas.Select((valor, indice) =>
                    $"Columna {indice}: {FormatearNumeroMatrices(valor)}"));

        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = EntradaMatrizMatrices(
                matriz.Length,
                matriz[0].Length,
                matriz),
            SalidaEsperada = salida,
            EsVisible = visible,
            Puntos = PuntosPorCasoMatrices,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion =
                "Comprueba las sumas de filas y columnas con índices base cero.",
            TablasEsperadas = Array.AsReadOnly(new[] {
                TablaSumasMatrices(
                    "Sumas por fila",
                    sumasFilas,
                    esFila: true),
                TablaSumasMatrices(
                    "Sumas por columna",
                    sumasColumnas,
                    esFila: false)
            })
        };
    }

    private static CasoPrueba CrearCasoDiagonalesMatrices(
        string id,
        string nombre,
        double[][] matriz,
        double[] principal,
        double[] secundaria,
        double sumaPrincipal,
        double sumaSecundaria,
        bool visible) {
        string[] etiquetasPrincipal = {
            "Diagonal principal",
            "Principal",
            "Elementos principales"
        };
        string[] etiquetasSecundaria = {
            "Diagonal secundaria",
            "Secundaria",
            "Elementos secundarios"
        };
        string[] etiquetasSumaPrincipal = {
            "Suma principal",
            "Total principal"
        };
        string[] etiquetasSumaSecundaria = {
            "Suma secundaria",
            "Total secundaria"
        };

        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = EntradaMatrizCuadradaMatrices(matriz),
            SalidaEsperada =
                $"Diagonal principal: {UnirNumerosMatrices(principal)}\n" +
                $"Diagonal secundaria: {UnirNumerosMatrices(secundaria)}\n" +
                $"Suma principal: {FormatearNumeroMatrices(sumaPrincipal)}\n" +
                $"Suma secundaria: {FormatearNumeroMatrices(sumaSecundaria)}",
            EsVisible = visible,
            Puntos = PuntosPorCasoMatrices,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion =
                "Comprueba ambas diagonales en orden y sus sumas.",
            ValoresNumericosEsperados = Array.AsReadOnly(new[] {
                NumeroMatrices(
                    "Suma principal",
                    sumaPrincipal,
                    etiquetasSumaPrincipal),
                NumeroMatrices(
                    "Suma secundaria",
                    sumaSecundaria,
                    etiquetasSumaSecundaria)
            }),
            ColeccionesEsperadas = Array.AsReadOnly(new[] {
                ColeccionNumericaMatrices(
                    "Diagonal principal",
                    principal,
                    etiquetasPrincipal),
                ColeccionNumericaMatrices(
                    "Diagonal secundaria",
                    secundaria,
                    etiquetasSecundaria)
            })
        };
    }

    private static CasoPrueba CrearCasoMatrizConOrigenMatrices(
        string id,
        string nombre,
        double[][] origen,
        double[][] resultado,
        string[] etiquetas,
        bool visible) {
        return CrearCasoMatrizMatrices(
            id,
            nombre,
            EntradaMatrizMatrices(
                origen.Length,
                origen[0].Length,
                origen),
            "Comprueba dimensiones y posiciones después de transponer la matriz.",
            resultado,
            etiquetas,
            requerirEtiqueta: true,
            visible);
    }

    private static CasoPrueba CrearCasoSumaMatrices(
        string id,
        string nombre,
        double[][] matrizA,
        double[][] matrizB,
        double[][] resultado,
        string[] etiquetas,
        bool visible) {
        return CrearCasoMatrizMatrices(
            id,
            nombre,
            EntradaDosMatricesMismasDimensionesMatrices(matrizA, matrizB),
            "Comprueba la suma elemento a elemento en cada posición.",
            resultado,
            etiquetas,
            requerirEtiqueta: true,
            visible);
    }

    private static CasoPrueba CrearCasoProductoMatrices(
        string id,
        string nombre,
        double[][] matrizA,
        double[][] matrizB,
        double[][] resultado,
        string[] etiquetas,
        bool visible) {
        return CrearCasoMatrizMatrices(
            id,
            nombre,
            EntradaProductoMatrices(matrizA, matrizB),
            "Comprueba el producto matricial y sus dimensiones.",
            resultado,
            etiquetas,
            requerirEtiqueta: true,
            visible);
    }

    private static CasoPrueba CrearCasoExtremosPosicionMatrices(
        string id,
        string nombre,
        double[][] matriz,
        double mayor,
        int filaMayor,
        int columnaMayor,
        double menor,
        int filaMenor,
        int columnaMenor,
        string[] etiquetasMayor,
        string[] etiquetasFilaMayor,
        string[] etiquetasColumnaMayor,
        string[] etiquetasMenor,
        string[] etiquetasFilaMenor,
        string[] etiquetasColumnaMenor,
        bool visible) {
        return CrearCasoEscalarMatrices(
            id,
            nombre,
            EntradaMatrizMatrices(
                matriz.Length,
                matriz[0].Length,
                matriz),
            $"Mayor: {FormatearNumeroMatrices(mayor)}\n" +
            $"Fila mayor: {filaMayor}\n" +
            $"Columna mayor: {columnaMayor}\n" +
            $"Menor: {FormatearNumeroMatrices(menor)}\n" +
            $"Fila menor: {filaMenor}\n" +
            $"Columna menor: {columnaMenor}",
            "Comprueba ambos extremos y la primera posición de cada uno.",
            new[] {
                // Las posiciones se consumen primero para que etiquetas amplias
                // como "Mayor" no reutilicen los índices de "Fila mayor".
                NumeroMatrices("Fila mayor", filaMayor, etiquetasFilaMayor),
                NumeroMatrices(
                    "Columna mayor",
                    columnaMayor,
                    etiquetasColumnaMayor),
                NumeroMatrices("Fila menor", filaMenor, etiquetasFilaMenor),
                NumeroMatrices(
                    "Columna menor",
                    columnaMenor,
                    etiquetasColumnaMenor),
                NumeroMatrices("Mayor", mayor, etiquetasMayor),
                NumeroMatrices("Menor", menor, etiquetasMenor)
            },
            visible);
    }

    private static ReglaMatrizEsperada MatrizNumericaMatrices(
        string nombre,
        double[][] valores,
        string[] etiquetas,
        bool requerirEtiqueta) {
        return new ReglaMatrizEsperada {
            Nombre = nombre,
            EtiquetasInicio = Array.AsReadOnly(etiquetas),
            RequerirEtiqueta = requerirEtiqueta,
            FilasEsperadas = valores.Length,
            ColumnasEsperadas = valores[0].Length,
            TipoElementos = TipoValorEstructurado.Numerico,
            ValoresNumericosEsperados = Array.AsReadOnly(
                valores
                    .Select(fila =>
                        (IReadOnlyList<double>)Array.AsReadOnly(fila))
                    .ToArray()),
            ToleranciaNumerica = 0D,
            SeparadoresColumnas = Array.AsReadOnly(new[] {
                " ",
                "\t",
                ",",
                ";"
            }),
            PermitirElementosAdicionales = false,
            PermitirTextoNeutralExterno = true,
            MensajeError =
                "La matriz debe conservar exactamente sus dimensiones, filas, columnas y posiciones."
        };
    }

    private static ReglaTablaEsperada TablaSumasMatrices(
        string nombre,
        double[] sumas,
        bool esFila) {
        string[] etiquetasInicio = esFila
            ? new[] {
                "Sumas por fila",
                "Resultados por fila",
                "Filas"
            }
            : new[] {
                "Sumas por columna",
                "Resultados por columna",
                "Columnas"
            };
        string[] etiquetasFin = esFila
            ? new[] {
                "Sumas por columna",
                "Resultados por columna",
                "Columnas"
            }
            : Array.Empty<string>();

        FilaTablaEsperada[] filas = sumas
            .Select((valor, indice) => new FilaTablaEsperada {
                Nombre = $"{(esFila ? "Fila" : "Columna")} {indice}",
                Clave = $"{(esFila ? "Fila" : "Columna")} {indice}",
                ClavesAlternativas = Array.AsReadOnly(
                    esFila
                        ? new[] {
                            $"Suma fila {indice}",
                            $"Renglón {indice}",
                            $"Renglon {indice}"
                        }
                        : new[] {
                            $"Suma columna {indice}"
                        }),
                Celdas = Array.AsReadOnly(new[] {
                    new CeldaTablaEsperada {
                        Nombre = "Suma",
                        Posicion = 0,
                        Valor = new ValorEstructuradoEsperado {
                            Nombre = "Suma",
                            Tipo = TipoValorEstructurado.Numerico,
                            ValorNumerico = valor,
                            ToleranciaNumerica = 0D
                        }
                    }
                })
            })
            .ToArray();

        return new ReglaTablaEsperada {
            Nombre = nombre,
            EtiquetasInicio = Array.AsReadOnly(etiquetasInicio),
            EtiquetasFin = Array.AsReadOnly(etiquetasFin),
            FilasEsperadas = Array.AsReadOnly(filas),
            CantidadFilasExacta = filas.Length,
            CantidadColumnasExacta = 1,
            OrdenFilasObligatorio = true,
            PermitirFilasAdicionales = false,
            PermitirFilasDuplicadas = false,
            PermitirTextoNeutralEntreFilas = true,
            MensajeError =
                $"Las {nombre.ToLowerInvariant()} deben usar índices base cero y conservar su orden."
        };
    }

    private static ReglaColeccionEsperada ColeccionNumericaMatrices(
        string nombre,
        double[] esperados,
        string[] etiquetas) {
        return new ReglaColeccionEsperada {
            Nombre = nombre,
            TipoElementos = TipoValorEstructurado.Numerico,
            ElementosEsperados = Array.AsReadOnly(
                esperados
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
                $"La colección {nombre} debe conservar exactamente sus elementos y su orden."
        };
    }

    private static ValorNumericoEsperado NumeroMatrices(
        string nombre,
        double valor,
        params string[] etiquetas) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            Valor = valor,
            Tolerancia = 0D,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas)
        };
    }

    private static string EntradaMatrizMatrices(
        int filas,
        int columnas,
        double[][] matriz) {
        return filas.ToString(CultureInfo.InvariantCulture) + "\n" +
            columnas.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                matriz.SelectMany(fila => fila)
                    .Select(FormatearNumeroMatrices)) +
            "\n";
    }

    private static string EntradaMatrizCuadradaMatrices(double[][] matriz) {
        return matriz.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                matriz.SelectMany(fila => fila)
                    .Select(FormatearNumeroMatrices)) +
            "\n";
    }

    private static string EntradaDosMatricesMismasDimensionesMatrices(
        double[][] matrizA,
        double[][] matrizB) {
        return matrizA.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            matrizA[0].Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                matrizA.Concat(matrizB)
                    .SelectMany(fila => fila)
                    .Select(FormatearNumeroMatrices)) +
            "\n";
    }

    private static string EntradaProductoMatrices(
        double[][] matrizA,
        double[][] matrizB) {
        return matrizA.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            matrizA[0].Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                matrizA.SelectMany(fila => fila)
                    .Select(FormatearNumeroMatrices)) +
            "\n" +
            matrizB.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            matrizB[0].Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                matrizB.SelectMany(fila => fila)
                    .Select(FormatearNumeroMatrices)) +
            "\n";
    }

    private static string FormatearMatrizMatrices(
        string etiqueta,
        double[][] matriz) {
        return etiqueta + ":\n" +
            string.Join(
                "\n",
                matriz.Select(UnirNumerosMatrices));
    }

    private static string UnirNumerosMatrices(IEnumerable<double> valores) {
        return string.Join(" ", valores.Select(FormatearNumeroMatrices));
    }

    private static string FormatearNumeroMatrices(double valor) {
        return valor.ToString("G15", CultureInfo.InvariantCulture);
    }
}
