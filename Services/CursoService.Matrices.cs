using EndForge.Models;

namespace EndForge.Services;

public sealed partial class CursoService {
    public const string TemaMatricesGradoJuniorId = "grado2-matrices";

    private static IReadOnlyList<PracticaCurso> CrearPracticasMatricesGradoJunior() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "grado2-matrices-capturar-mostrar",
                TemaMatricesGradoJuniorId,
                1,
                "Capturar y mostrar una matriz",
                "Capturar y mostrar una matriz",
                "Capturar una matriz de enteros y mostrarla sin alterar sus filas, columnas ni posiciones.",
                "Crear un programa que lea dimensiones entre uno y cinco, capture exactamente cada elemento y presente la matriz con su estructura bidimensional.",
                new[] { "matrices", "filas", "columnas", "índices", "recorrido bidimensional" },
                new[] {
                    "Leer y validar la cantidad de filas y columnas.",
                    "Capturar exactamente filas por columnas valores enteros.",
                    "Conservar cada valor en la posición correspondiente.",
                    "Mostrar una fila de salida por cada fila de la matriz."
                },
                "La salida contiene una matriz con dimensiones, posiciones y valores idénticos a los capturados; no se acepta una lista plana.",
                "Inicial",
                "35–45 min",
                new[] { "Grado 2 · Arreglos" },
                CrearGuiaCapturarMostrarMatriz()),
            CrearPractica(
                "grado2-matrices-suma-elementos",
                TemaMatricesGradoJuniorId,
                2,
                "Sumar todos los elementos de una matriz",
                "Sumar todos los elementos de una matriz",
                "Recorrer una matriz completa y acumular la suma de todas sus posiciones.",
                "Crear un programa que lea una matriz de enteros de hasta cinco por cinco y calcule un único total que incluya cada elemento exactamente una vez.",
                new[] { "matrices", "recorrido bidimensional", "acumulador", "suma" },
                new[] {
                    "Leer las dimensiones y todos los elementos.",
                    "Iniciar un acumulador en cero.",
                    "Visitar cada fila y cada columna.",
                    "Agregar el valor de cada posición al acumulador.",
                    "Mostrar la suma total con una etiqueta clara."
                },
                "La salida muestra la suma correcta de todos los elementos, incluidos negativos y ceros.",
                "Fácil",
                "35–45 min",
                new[] { "Matrices 01" },
                CrearGuiaSumaElementosMatriz()),
            CrearPractica(
                "grado2-matrices-sumas-filas-columnas",
                TemaMatricesGradoJuniorId,
                3,
                "Calcular sumas por filas y columnas",
                "Calcular sumas por filas y columnas",
                "Calcular por separado la suma de cada fila y la suma de cada columna.",
                "Crear un programa que recorra una matriz de enteros y presente las sumas de sus filas y columnas en orden, usando índices desde cero.",
                new[] { "matrices", "sumas parciales", "filas", "columnas", "índices" },
                new[] {
                    "Leer las dimensiones y la matriz.",
                    "Calcular una suma independiente para cada fila.",
                    "Calcular una suma independiente para cada columna.",
                    "Mostrar cada resultado con su índice base cero.",
                    "Conservar el orden original de filas y columnas."
                },
                "La salida identifica todas las filas y columnas, con índice base cero y una suma correcta para cada una.",
                "Intermedia",
                "45–60 min",
                new[] { "Matrices 01–02" },
                CrearGuiaSumasFilasColumnasMatriz()),
            CrearPractica(
                "grado2-matrices-diagonales",
                TemaMatricesGradoJuniorId,
                4,
                "Calcular las diagonales de una matriz cuadrada",
                "Calcular las diagonales de una matriz cuadrada",
                "Extraer las diagonales principal y secundaria y calcular la suma de cada una.",
                "Crear un programa que lea una matriz cuadrada de orden uno a cinco, muestre ambas diagonales en su orden natural y calcule sus sumas.",
                new[] { "matriz cuadrada", "diagonal principal", "diagonal secundaria", "índices", "acumuladores" },
                new[] {
                    "Leer el orden de la matriz cuadrada y sus elementos.",
                    "Reconocer las posiciones de la diagonal principal.",
                    "Reconocer las posiciones de la diagonal secundaria.",
                    "Conservar el orden de arriba hacia abajo de ambas diagonales.",
                    "Calcular y mostrar una suma para cada diagonal."
                },
                "La salida contiene ambas diagonales completas y sus sumas; el centro de una matriz impar pertenece a las dos.",
                "Intermedia",
                "45–60 min",
                new[] { "Matrices 01–03" },
                CrearGuiaDiagonalesMatriz()),
            CrearPractica(
                "grado2-matrices-transpuesta",
                TemaMatricesGradoJuniorId,
                5,
                "Obtener la transpuesta de una matriz",
                "Obtener la transpuesta de una matriz",
                "Intercambiar filas por columnas para construir la transpuesta de una matriz.",
                "Crear un programa que lea una matriz de hasta cinco por cinco y muestre otra donde cada elemento de la posición [i,j] aparezca en [j,i].",
                new[] { "matrices", "transpuesta", "intercambio de índices", "dimensiones" },
                new[] {
                    "Leer filas, columnas y elementos de la matriz original.",
                    "Determinar las dimensiones de la transpuesta.",
                    "Relacionar cada posición original con su posición intercambiada.",
                    "Mostrar la transpuesta respetando sus nuevas filas y columnas."
                },
                "La salida es una matriz de columnas por filas con cada valor en su posición transpuesta exacta.",
                "Intermedia",
                "45–60 min",
                new[] { "Matrices 01–04" },
                CrearGuiaTranspuestaMatriz()),
            CrearPractica(
                "grado2-matrices-sumar-dos",
                TemaMatricesGradoJuniorId,
                6,
                "Sumar dos matrices",
                "Sumar dos matrices",
                "Combinar dos matrices de iguales dimensiones mediante una suma posición por posición.",
                "Crear un programa que lea dos matrices A y B del mismo tamaño y muestre una matriz C donde cada elemento sea la suma de las posiciones correspondientes.",
                new[] { "dos matrices", "suma elemento a elemento", "índices", "matriz resultante" },
                new[] {
                    "Leer las dimensiones compartidas.",
                    "Capturar todos los elementos de A y de B.",
                    "Relacionar posiciones con los mismos índices.",
                    "Sumar cada pareja correspondiente.",
                    "Mostrar únicamente la matriz resultante en la región de resultado."
                },
                "La salida contiene una matriz de las mismas dimensiones con la suma correcta de cada pareja de elementos.",
                "Intermedia",
                "45–60 min",
                new[] { "Matrices 01–05" },
                CrearGuiaSumarDosMatrices()),
            CrearPractica(
                "grado2-matrices-multiplicar",
                TemaMatricesGradoJuniorId,
                7,
                "Multiplicar dos matrices",
                "Multiplicar dos matrices",
                "Calcular el producto matricial de dos matrices con dimensiones compatibles.",
                "Crear un programa que lea matrices A y B compatibles y produzca A por B mediante productos entre filas de A y columnas de B.",
                new[] { "producto matricial", "dimensiones compatibles", "fila por columna", "acumulador" },
                new[] {
                    "Leer las dimensiones de A y B.",
                    "Comprobar que las columnas de A coincidan con las filas de B.",
                    "Capturar ambas matrices.",
                    "Calcular cada celda del resultado combinando una fila de A con una columna de B.",
                    "Mostrar la matriz con filas de A por columnas de B."
                },
                "La salida presenta A por B con dimensiones y valores correctos; no se acepta una multiplicación elemento a elemento.",
                "Avanzada",
                "60–80 min",
                new[] { "Matrices 01–06" },
                CrearGuiaMultiplicarMatrices()),
            CrearPractica(
                "grado2-matrices-mayor-menor-posicion",
                TemaMatricesGradoJuniorId,
                8,
                "Encontrar mayor y menor con sus posiciones",
                "Encontrar mayor y menor con sus posiciones",
                "Localizar los valores extremo de una matriz y la primera posición de cada uno.",
                "Crear un programa que recorra una matriz por filas, encuentre el mayor y el menor y reporte sus primeras coordenadas con índices base cero.",
                new[] { "máximo", "mínimo", "coordenadas", "primera aparición", "recorrido por filas" },
                new[] {
                    "Leer las dimensiones y todos los elementos.",
                    "Tomar la primera posición como referencia inicial.",
                    "Recorrer cada fila de izquierda a derecha.",
                    "Actualizar un extremo y sus coordenadas solo al encontrar un valor estrictamente mejor.",
                    "Mostrar valores, filas y columnas con etiquetas diferentes."
                },
                "La salida muestra mayor y menor con su primera posición en recorrido por filas e índices base cero.",
                "Intermedia",
                "50–65 min",
                new[] { "Matrices 01–07" },
                CrearGuiaMayorMenorPosicionMatriz())
        });
    }

    private static GuiaPractica CrearGuiaCapturarMostrarMatriz() {
        return CrearGuiaMatrices(
            "Un programa que organiza enteros en filas y columnas y reproduce la matriz completa sin convertirla en una lista plana.",
            new[] {
                DatoMatriz("filas", "int", "Cantidad de renglones; debe estar entre 1 y 5", "2"),
                DatoMatriz("columnas", "int", "Cantidad de posiciones por fila; debe estar entre 1 y 5", "3"),
                DatoMatriz("elementos", "enteros organizados en matriz", "Valores almacenados en una posición [fila, columna]", "1 2 3 / 4 5 6")
            },
            new[] {
                ConceptoMatriz(
                    "Matriz",
                    "Organiza datos en dos dimensiones: filas y columnas.",
                    "Una posición puede describirse como matriz[fila][columna]."),
                ConceptoMatriz(
                    "Coordenadas",
                    "Dos índices identifican una celda; ambos comienzan en cero.",
                    "La celda [1, 2] pertenece a la segunda fila y la tercera columna."),
                ConceptoMatriz(
                    "Recorrido por filas",
                    "Se visitan las columnas de una fila antes de avanzar a la siguiente.",
                    "Al terminar una fila de salida se agrega un salto de línea.")
            },
            new[] {
                "Lee filas y columnas y confirma que estén entre 1 y 5.",
                "Captura exactamente filas × columnas enteros.",
                "Asocia cada dato con la fila y columna que le corresponden.",
                "Prepara una región de salida con una etiqueta como Matriz.",
                "Muestra una línea independiente por cada fila.",
                "Comprueba matrices rectangulares para evitar intercambiar dimensiones."
            },
            HerramientaMatriz(
                "Pensar en coordenadas",
                "Nombrar fila y columna por separado ayuda a comprender dónde se encuentra cada dato.",
                "Permite conservar la estructura al capturar y al presentar la matriz.",
                "int valorActual = matriz[fila][columna];",
                "Es un acceso aislado a una celda; no impone una forma concreta de almacenar ni recorrer toda la matriz."),
            "2\n3\n1\n2\n3\n4\n5\n6",
            "Matriz:\n1 2 3\n4 5 6",
            new[] {
                "Mostrar todos los valores en una sola línea.",
                "Intercambiar filas y columnas.",
                "Leer menos o más de filas × columnas elementos.",
                "Avanzar una fila antes de terminar sus columnas.",
                "Agregar las dimensiones dentro de la región que contiene el resultado."
            },
            "EndForge revisará las dimensiones implícitas de la salida y el valor exacto de cada posición.");
    }

    private static GuiaPractica CrearGuiaSumaElementosMatriz() {
        return CrearGuiaMatrices(
            "Un programa que visita todas las celdas de una matriz y conserva un total acumulado.",
            new[] {
                DatoMatriz("filas y columnas", "int", "Dimensiones de la matriz; ambas entre 1 y 5", "2 y 2"),
                DatoMatriz("elementos", "int", "Valores que participan en la suma", "1, 2, 3, 4"),
                DatoMatriz("suma", "int", "Total de todas las posiciones", "10")
            },
            new[] {
                ConceptoMatriz(
                    "Acumulador",
                    "Guarda la suma parcial y debe comenzar en cero.",
                    "int suma = 0;"),
                ConceptoMatriz(
                    "Recorrido completo",
                    "Cada posición debe contribuir exactamente una vez al total.",
                    "Una matriz de 2 × 3 contiene seis valores."),
                ConceptoMatriz(
                    "Signos",
                    "Los negativos reducen el acumulador y los ceros no lo modifican.",
                    "-2 + 0 + 7 produce 5.")
            },
            new[] {
                "Lee las dimensiones y captura toda la matriz.",
                "Inicia un único acumulador en cero.",
                "Visita todas las columnas de cada fila.",
                "Agrega el valor actual sin reemplazar el total previo.",
                "Muestra la suma después de terminar el recorrido.",
                "Prueba matrices con negativos y una suma final igual a cero."
            },
            HerramientaMatriz(
                "Acumular una celda",
                "La misma operación puede aplicarse a cada posición visitada.",
                "Mantiene un único total aunque la información esté distribuida en dos dimensiones.",
                "suma += valorActual;",
                "El fragmento muestra solo la actualización del acumulador; el recorrido queda a elección del estudiante."),
            "2\n2\n1\n2\n3\n4",
            "Suma total: 10",
            new[] {
                "Reiniciar la suma al comenzar cada fila.",
                "Sumar únicamente la primera fila o columna.",
                "Reemplazar la suma por el valor actual.",
                "Ignorar números negativos.",
                "Mostrar varios totales contradictorios."
            },
            "EndForge comprobará un único total etiquetado que incluya todas las posiciones de la matriz.");
    }

    private static GuiaPractica CrearGuiaSumasFilasColumnasMatriz() {
        return CrearGuiaMatrices(
            "Un programa que produce un resumen ordenado con una suma para cada fila y otra para cada columna.",
            new[] {
                DatoMatriz("matriz", "enteros organizados por filas", "Datos cuyos subtotales se calcularán", "1 2 3 / 4 5 6"),
                DatoMatriz("sumas de filas", "int", "Un resultado por fila, en orden", "6, 15"),
                DatoMatriz("sumas de columnas", "int", "Un resultado por columna, en orden", "5, 7, 9"),
                DatoMatriz("índices", "int", "Identificadores base cero de cada resultado", "Fila 0, Columna 0")
            },
            new[] {
                ConceptoMatriz(
                    "Suma por fila",
                    "Agrupa las celdas que comparten el mismo índice de fila.",
                    "Fila 0: 1 + 2 + 3 = 6."),
                ConceptoMatriz(
                    "Suma por columna",
                    "Agrupa las celdas que comparten el mismo índice de columna.",
                    "Columna 1: 2 + 5 = 7."),
                ConceptoMatriz(
                    "Subtotal independiente",
                    "Cada fila o columna necesita iniciar su propia suma en cero.",
                    "El resultado anterior no debe mezclarse con el siguiente.")
            },
            new[] {
                "Lee y conserva la matriz completa.",
                "Calcula las sumas de filas desde la fila cero hasta la última.",
                "Reinicia el subtotal antes de procesar la siguiente fila.",
                "Calcula después las sumas de columnas desde la columna cero.",
                "Muestra índice y suma de cada resultado.",
                "Verifica matrices de una sola fila y de una sola columna."
            },
            HerramientaMatriz(
                "Separar regiones de resultados",
                "Presentar primero todas las filas y después todas las columnas evita confundir ambos grupos.",
                "Facilita comprobar cantidad, índices y orden de los subtotales.",
                "Sumas de filas:\nFila 0: ...\n\nSumas de columnas:\nColumna 0: ...",
                "El formato es una recomendación; pueden usarse etiquetas equivalentes reconocibles."),
            "2\n3\n1\n2\n3\n4\n5\n6",
            "Fila 0: 6\nFila 1: 15\nColumna 0: 5\nColumna 1: 7\nColumna 2: 9",
            new[] {
                "Usar índices desde uno.",
                "No reiniciar el subtotal entre filas o columnas.",
                "Intercambiar los resultados de filas con los de columnas.",
                "Omitir un índice o alterar su orden.",
                "Mostrar solo la suma total de la matriz."
            },
            "EndForge revisará dos grupos independientes, completos y ordenados, con índices base cero.");
    }

    private static GuiaPractica CrearGuiaDiagonalesMatriz() {
        return CrearGuiaMatrices(
            "Un programa que identifica los dos recorridos diagonales de una matriz cuadrada y obtiene sus totales.",
            new[] {
                DatoMatriz("n", "int", "Orden de la matriz cuadrada; entre 1 y 5", "3"),
                DatoMatriz("diagonal principal", "colección de enteros", "Celdas con índices de fila y columna iguales", "1, 5, 9"),
                DatoMatriz("diagonal secundaria", "colección de enteros", "Celdas que recorren de la esquina superior derecha a la inferior izquierda", "3, 5, 7"),
                DatoMatriz("sumas", "int", "Total independiente de cada diagonal", "15 y 15")
            },
            new[] {
                ConceptoMatriz(
                    "Diagonal principal",
                    "Avanza desde la esquina superior izquierda hasta la inferior derecha.",
                    "Sus coordenadas siguen [0,0], [1,1], [2,2]..."),
                ConceptoMatriz(
                    "Diagonal secundaria",
                    "Avanza desde la esquina superior derecha hasta la inferior izquierda.",
                    "En orden 3 sigue [0,2], [1,1], [2,0]."),
                ConceptoMatriz(
                    "Centro compartido",
                    "En una matriz impar, la celda central pertenece a ambas diagonales.",
                    "Cada suma diagonal incluye su propia visita al centro.")
            },
            new[] {
                "Lee n y captura la matriz cuadrada n × n.",
                "Identifica las posiciones de la diagonal principal en orden descendente por filas.",
                "Identifica de forma independiente la diagonal secundaria.",
                "Muestra los elementos de cada diagonal sin invertirlos.",
                "Acumula y presenta una suma para cada colección.",
                "Prueba una matriz de orden uno y otra de orden impar."
            },
            HerramientaMatriz(
                "Relacionar índices",
                "Las diagonales pueden describirse mediante relaciones entre fila y columna.",
                "Ayuda a seleccionar una celda por fila sin recorrer posiciones que no pertenecen a la diagonal.",
                "principal: columna = fila\nsecundaria: columna = n - 1 - fila",
                "Estas relaciones explican las coordenadas, pero no constituyen un programa completo."),
            "3\n1\n2\n3\n4\n5\n6\n7\n8\n9",
            "Diagonal principal: 1 5 9\nDiagonal secundaria: 3 5 7\nSuma principal: 15\nSuma secundaria: 15",
            new[] {
                "Invertir el orden de una diagonal.",
                "Omitir el centro de una matriz impar.",
                "Usar una fila o columna completa.",
                "Mezclar los acumuladores de ambas diagonales.",
                "Agregar elementos que no pertenecen a la diagonal."
            },
            "EndForge revisará cada diagonal como colección ordenada y comprobará por separado ambas sumas.");
    }

    private static GuiaPractica CrearGuiaTranspuestaMatriz() {
        return CrearGuiaMatrices(
            "Un programa que transforma las filas de una matriz en columnas y sus columnas en filas.",
            new[] {
                DatoMatriz("matriz original", "valores organizados en filas × columnas", "Datos que se desean transponer", "1 2 3 / 4 5 6"),
                DatoMatriz("transpuesta", "valores organizados en columnas × filas", "Resultado con índices intercambiados", "1 4 / 2 5 / 3 6"),
                DatoMatriz("dimensiones", "int", "Tamaños antes y después de intercambiarse", "2 × 3 pasa a 3 × 2")
            },
            new[] {
                ConceptoMatriz(
                    "Transposición",
                    "Convierte cada fila original en una columna del resultado.",
                    "La posición [fila, columna] pasa a [columna, fila]."),
                ConceptoMatriz(
                    "Dimensiones intercambiadas",
                    "La cantidad de filas resultantes es la cantidad de columnas originales.",
                    "Una matriz 1 × 4 se convierte en 4 × 1."),
                ConceptoMatriz(
                    "Posición, no ordenamiento",
                    "Los valores no se comparan ni se invierten; únicamente cambian de coordenada.",
                    "Transponer no significa mostrar desde el final.")
            },
            new[] {
                "Lee filas, columnas y la matriz original.",
                "Determina que el resultado tendrá columnas filas y filas columnas.",
                "Relaciona cada celda original con la coordenada intercambiada.",
                "Muestra la transpuesta con una línea por cada nueva fila.",
                "Conserva todos los valores exactamente una vez.",
                "Prueba matrices no cuadradas para comprobar las nuevas dimensiones."
            },
            HerramientaMatriz(
                "Comprobar una coordenada",
                "Seguir una sola celda permite verificar si el intercambio de índices es correcto.",
                "Es útil antes de revisar toda la salida.",
                "original[0][2] corresponde a transpuesta[2][0]",
                "La igualdad describe una relación entre posiciones y no obliga a construir una segunda matriz."),
            "2\n3\n1\n2\n3\n4\n5\n6",
            "Transpuesta:\n1 4\n2 5\n3 6",
            new[] {
                "Mostrar nuevamente la matriz original.",
                "Conservar dimensiones 2 × 3 para una salida que debe ser 3 × 2.",
                "Invertir filas o columnas.",
                "Intercambiar valores dentro de la misma fila.",
                "Presentar una lista plana."
            },
            "EndForge comprobará las dimensiones transpuestas y el valor exacto de cada celda.");
    }

    private static GuiaPractica CrearGuiaSumarDosMatrices() {
        return CrearGuiaMatrices(
            "Un programa que combina dos matrices del mismo tamaño y produce una tercera mediante sumas correspondientes.",
            new[] {
                DatoMatriz("filas y columnas", "int", "Dimensiones compartidas por A, B y C", "2 y 2"),
                DatoMatriz("matriz A", "enteros", "Primer conjunto de valores", "1 2 / 3 4"),
                DatoMatriz("matriz B", "enteros", "Segundo conjunto de valores", "5 6 / 7 8"),
                DatoMatriz("matriz C", "enteros", "Suma elemento a elemento", "6 8 / 10 12")
            },
            new[] {
                ConceptoMatriz(
                    "Dimensiones iguales",
                    "Solo se relacionan matrices con la misma cantidad de filas y columnas.",
                    "A, B y C conservan el mismo tamaño."),
                ConceptoMatriz(
                    "Posiciones correspondientes",
                    "Cada celda del resultado usa los dos valores que comparten coordenadas.",
                    "C[0,1] combina A[0,1] con B[0,1]."),
                ConceptoMatriz(
                    "Suma elemento a elemento",
                    "La operación se aplica de forma independiente en cada posición.",
                    "valorC = valorA + valorB;")
            },
            new[] {
                "Lee las dimensiones comunes.",
                "Captura todos los valores de A y después todos los de B.",
                "Relaciona únicamente celdas con los mismos índices.",
                "Calcula una celda resultante por cada pareja.",
                "Muestra C dentro de una región claramente etiquetada.",
                "Prueba matrices rectangulares y valores que se cancelan hasta cero."
            },
            HerramientaMatriz(
                "Verificar una posición antes del conjunto",
                "Comprobar una sola coordenada ayuda a detectar si se están mezclando filas o columnas.",
                "Después puede repetirse la misma relación para las demás celdas.",
                "resultado[fila][columna] =\n    a[fila][columna] + b[fila][columna];",
                "Es una operación aislada; la captura, el recorrido y la presentación quedan por resolver."),
            "2\n2\n1\n2\n3\n4\n5\n6\n7\n8",
            "Matriz resultante:\n6 8\n10 12",
            new[] {
                "Concatenar una matriz después de la otra.",
                "Sumar filas completas y producir un solo valor.",
                "Cruzar una fila de A con una columna de B.",
                "Mostrar A o B dentro de la región destinada a C.",
                "Transponer el resultado."
            },
            "EndForge revisará una matriz resultante con dimensiones y posiciones exactas, sin valores adicionales.");
    }

    private static GuiaPractica CrearGuiaMultiplicarMatrices() {
        return CrearGuiaMatrices(
            "Un programa que calcula A × B combinando cada fila de la primera matriz con cada columna de la segunda.",
            new[] {
                DatoMatriz("dimensiones de A", "int", "Filas y columnas de la primera matriz", "2 × 3"),
                DatoMatriz("dimensiones de B", "int", "Filas y columnas de la segunda matriz", "3 × 2"),
                DatoMatriz("compatibilidad", "relación de dimensiones", "Columnas de A iguales a filas de B", "3 = 3"),
                DatoMatriz("producto", "matriz numérica", "Resultado con filas de A y columnas de B", "2 × 2")
            },
            new[] {
                ConceptoMatriz(
                    "Dimensiones compatibles",
                    "El número de términos de una fila de A debe coincidir con el de una columna de B.",
                    "A de 2 × 3 puede multiplicarse por B de 3 × 4."),
                ConceptoMatriz(
                    "Producto fila por columna",
                    "Una celda resulta de multiplicar términos correspondientes y sumar esos productos.",
                    "[1, 2, 3] con [4, 5, 6] produce 1×4 + 2×5 + 3×6."),
                ConceptoMatriz(
                    "Tamaño del resultado",
                    "Las dimensiones externas determinan la forma final.",
                    "A de 2 × 3 por B de 3 × 1 produce 2 × 1.")
            },
            new[] {
                "Lee las cuatro dimensiones y comprueba su compatibilidad.",
                "Captura A y B conservando sus propias formas.",
                "Elige una fila de A y una columna de B para cada celda resultante.",
                "Inicia en cero el acumulador de esa celda.",
                "Combina los términos correspondientes y conserva el total.",
                "Muestra el producto con sus dimensiones externas."
            },
            HerramientaMatriz(
                "Producto punto",
                "El cálculo de una celda puede entenderse como el producto punto de dos colecciones del mismo tamaño.",
                "Se multiplican parejas correspondientes y después se suman los productos.",
                "fila:    1 2 3\ncolumna: 4 5 6\nproductos: 1×4, 2×5, 3×6",
                "El ejemplo explica una sola celda con datos distintos; no contiene el algoritmo completo de multiplicación."),
            "2\n2\n1\n2\n3\n4\n2\n2\n5\n6\n7\n8",
            "Producto:\n19 22\n43 50",
            new[] {
                "Multiplicar elementos con la misma coordenada.",
                "Sumar A y B.",
                "Calcular B × A en vez de A × B.",
                "No reiniciar el acumulador para una nueva celda.",
                "Usar dimensiones del resultado incorrectas."
            },
            "EndForge verificará las dimensiones y posiciones del producto matricial y rechazará operaciones elemento a elemento.");
    }

    private static GuiaPractica CrearGuiaMayorMenorPosicionMatriz() {
        return CrearGuiaMatrices(
            "Un programa que encuentra los dos extremos de una matriz y recuerda dónde apareció por primera vez cada uno.",
            new[] {
                DatoMatriz("matriz", "enteros", "Valores que se recorrerán por filas", "3 9 / -1 9"),
                DatoMatriz("mayor", "int", "Valor máximo de todas las celdas", "9"),
                DatoMatriz("posición del mayor", "fila y columna int", "Primera coordenada del máximo", "[0,1]"),
                DatoMatriz("menor y posición", "int y coordenadas", "Valor mínimo y su primera aparición", "-1 en [1,0]")
            },
            new[] {
                ConceptoMatriz(
                    "Referencia inicial válida",
                    "La primera celda sirve para iniciar mayor y menor incluso si todos los datos son negativos.",
                    "El recorrido comienza tomando como referencia [0,0]."),
                ConceptoMatriz(
                    "Primera aparición",
                    "Una coincidencia posterior no debe reemplazar las coordenadas guardadas.",
                    "Se actualiza solo ante un valor estrictamente mayor o menor."),
                ConceptoMatriz(
                    "Orden por filas",
                    "Se recorre cada fila de izquierda a derecha antes de avanzar.",
                    "[0,0], [0,1]... y después [1,0].")
            },
            new[] {
                "Lee y captura una matriz no vacía.",
                "Inicia ambos extremos y sus coordenadas con la primera celda.",
                "Recorre las posiciones en orden por filas.",
                "Actualiza mayor y su posición solo con un valor estrictamente superior.",
                "Haz lo equivalente para el menor con una comparación estricta.",
                "Muestra valores, filas y columnas con índices base cero."
            },
            HerramientaMatriz(
                "Guardar valor y coordenadas juntos",
                "Cuando cambia un extremo, sus dos índices deben actualizarse en el mismo momento.",
                "Evita reportar un valor correcto con la posición de otra celda.",
                "mayor = valorActual;\nfilaMayor = fila;\ncolumnaMayor = columna;",
                "El fragmento muestra una actualización aislada y no incluye la condición ni el recorrido completo."),
            "2\n2\n3\n9\n-1\n9",
            "Mayor: 9\nFila mayor: 0\nColumna mayor: 1\nMenor: -1\nFila menor: 1\nColumna menor: 0",
            new[] {
                "Iniciar mayor o menor en cero.",
                "Usar índices desde uno.",
                "Guardar la última aparición de un valor repetido.",
                "Intercambiar fila y columna.",
                "Actualizar el valor sin actualizar sus coordenadas."
            },
            "EndForge revisará por separado valores y coordenadas y exigirá la primera aparición en recorrido por filas.");
    }

    private static GuiaPractica CrearGuiaMatrices(
        string queVasAConstruir,
        DatoGuiaPractica[] datos,
        ConceptoGuiaPractica[] conceptos,
        string[] pasos,
        HerramientaGuiaPractica herramienta,
        string entrada,
        string salida,
        string[] errores,
        string advertenciaEvaluacion) {
        return new GuiaPractica {
            QueVasAConstruir = queVasAConstruir,
            DatosNecesarios = Array.AsReadOnly(datos),
            ExplicacionesConceptos = Array.AsReadOnly(conceptos),
            PasosSugeridos = Array.AsReadOnly(pasos),
            HerramientaUtil = herramienta,
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = entrada,
                SalidaEsperada = salida
            },
            ErroresComunes = Array.AsReadOnly(errores),
            AdvertenciaEvaluacion = advertenciaEvaluacion
        };
    }

    private static DatoGuiaPractica DatoMatriz(
        string nombre,
        string tipo,
        string descripcion,
        string ejemplo) {
        return new DatoGuiaPractica {
            Nombre = nombre,
            Tipo = tipo,
            Descripcion = descripcion,
            Ejemplo = ejemplo
        };
    }

    private static ConceptoGuiaPractica ConceptoMatriz(
        string nombre,
        string explicacion,
        string fragmento) {
        return new ConceptoGuiaPractica {
            Nombre = nombre,
            Explicacion = explicacion,
            Fragmento = fragmento
        };
    }

    private static HerramientaGuiaPractica HerramientaMatriz(
        string nombre,
        string descripcion,
        string paraQueSirve,
        string codigo,
        string aclaracion) {
        return new HerramientaGuiaPractica {
            Nombre = nombre,
            Descripcion = descripcion,
            ParaQueSirve = paraQueSirve,
            Codigo = codigo,
            AclaracionOpcional = aclaracion
        };
    }
}
