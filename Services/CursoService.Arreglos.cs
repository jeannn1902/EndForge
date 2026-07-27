using EndForge.Models;

namespace EndForge.Services;

public sealed partial class CursoService {
    public const string TemaArreglosGradoJuniorId = "grado2-arreglos";

    private const int TotalPracticasArreglosGradoJunior = 10;

    private static IReadOnlyList<TemaCurso> CrearTemasGradoJunior() {
        IReadOnlyList<PracticaCurso> practicasArreglos =
            CrearPracticasArreglosGradoJunior();

        return Array.AsReadOnly(new[] {
            new TemaCurso {
                Id = TemaArreglosGradoJuniorId,
                Numero = 1,
                Nombre = "Arreglos",
                NombreCarpeta = "01_Arreglos",
                Descripcion =
                    "Aprende a almacenar, recorrer, transformar y analizar varios valores del mismo tipo.",
                TotalPracticasPlaneadas = TotalPracticasArreglosGradoJunior,
                Practicas = practicasArreglos,
                EsProximamente = false
            }
        });
    }

    private static IReadOnlyList<PracticaCurso> CrearPracticasArreglosGradoJunior() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "grado2-arreglos-capturar-mostrar",
                TemaArreglosGradoJuniorId,
                1,
                "Capturar y mostrar un arreglo",
                "Capturar y mostrar un arreglo",
                "Capturar una cantidad controlada de enteros y mostrarlos en el mismo orden.",
                "Crear un programa que lea entre uno y diez enteros, los almacene en un arreglo y muestre exactamente los valores capturados.",
                new[] { "arreglos", "índices", "recorrido", "entrada y salida" },
                new[] {
                    "Leer y validar la cantidad de elementos.",
                    "Capturar cada entero en una posición del arreglo.",
                    "Recorrer nuevamente el arreglo para mostrar sus elementos en el mismo orden."
                },
                "La salida identifica el arreglo y conserva exactamente el orden, los duplicados, los negativos y los ceros.",
                "Inicial",
                "25–35 min",
                Array.Empty<string>(),
                CrearGuiaCapturarMostrarArreglo()),
            CrearPractica(
                "grado2-arreglos-suma-elementos",
                TemaArreglosGradoJuniorId,
                2,
                "Suma de elementos",
                "Suma de elementos",
                "Recorrer un arreglo y acumular la suma de sus elementos.",
                "Crear un programa que lea hasta veinte enteros y calcule su suma, incluyendo correctamente el caso de una colección vacía.",
                new[] { "arreglos", "acumulador", "recorrido", "suma" },
                new[] {
                    "Leer la cantidad de valores.",
                    "Capturar los elementos del arreglo.",
                    "Recorrerlos con un acumulador iniciado en cero.",
                    "Mostrar la suma con una etiqueta clara."
                },
                "La salida muestra la suma correcta; para cero elementos, el resultado es cero.",
                "Inicial",
                "25–35 min",
                new[] { "Arreglos 01" },
                CrearGuiaSumaElementosArreglo()),
            CrearPractica(
                "grado2-arreglos-promedio",
                TemaArreglosGradoJuniorId,
                3,
                "Promedio del arreglo",
                "Promedio del arreglo",
                "Calcular el promedio de varios valores decimales almacenados en un arreglo.",
                "Crear un programa que lea entre uno y diez valores double, calcule su suma y obtenga el promedio sin perder decimales.",
                new[] { "arreglos", "double", "acumulador", "promedio" },
                new[] {
                    "Leer la cantidad de valores.",
                    "Capturar los valores decimales.",
                    "Sumarlos mediante un recorrido.",
                    "Dividir la suma entre la cantidad y mostrar el promedio."
                },
                "La salida muestra el promedio con una tolerancia de 0.01.",
                "Fácil",
                "30–40 min",
                new[] { "Arreglos 01–02" },
                CrearGuiaPromedioArreglo()),
            CrearPractica(
                "grado2-arreglos-mayor-menor",
                TemaArreglosGradoJuniorId,
                4,
                "Mayor y menor elemento",
                "Mayor y menor elemento",
                "Encontrar los valores máximo y mínimo de un arreglo sin suponer que son positivos.",
                "Crear un programa que lea entre uno y quince enteros y determine el mayor y el menor mediante un recorrido.",
                new[] { "arreglos", "comparaciones", "máximo", "mínimo" },
                new[] {
                    "Leer los elementos del arreglo.",
                    "Tomar el primer elemento como mayor y menor inicial.",
                    "Comparar los elementos restantes con ambos valores.",
                    "Mostrar el mayor y el menor con etiquetas diferentes."
                },
                "La salida presenta el mayor y el menor correctos, incluso con negativos, duplicados o un solo elemento.",
                "Fácil",
                "30–40 min",
                new[] { "Arreglos 01–03" },
                CrearGuiaMayorMenorArreglo()),
            CrearPractica(
                "grado2-arreglos-contar-pares-impares",
                TemaArreglosGradoJuniorId,
                5,
                "Contar pares e impares",
                "Contar pares e impares",
                "Clasificar los enteros de un arreglo y contar cuántos son pares e impares.",
                "Crear un programa que lea hasta veinte enteros, considere el cero como par y clasifique también los valores negativos.",
                new[] { "arreglos", "módulo", "contadores", "paridad" },
                new[] {
                    "Leer y almacenar los enteros.",
                    "Iniciar en cero los contadores de pares e impares.",
                    "Recorrer el arreglo y comprobar el residuo entre dos.",
                    "Mostrar ambas cantidades."
                },
                "La salida muestra cantidades correctas de pares e impares; una colección vacía produce cero y cero.",
                "Fácil",
                "30–40 min",
                new[] { "Arreglos 01–04" },
                CrearGuiaParesImparesArreglo()),
            CrearPractica(
                "grado2-arreglos-buscar-valor",
                TemaArreglosGradoJuniorId,
                6,
                "Buscar un valor",
                "Buscar un valor",
                "Buscar un entero y reportar si existe, su primer índice y su frecuencia.",
                "Crear un programa que recorra un arreglo, localice un valor objetivo y distinga entre la primera posición y el total de apariciones.",
                new[] { "búsqueda lineal", "índices", "frecuencia", "bool" },
                new[] {
                    "Leer el arreglo y el valor objetivo.",
                    "Recorrer todos los elementos.",
                    "Guardar únicamente el primer índice encontrado.",
                    "Contar todas las apariciones.",
                    "Mostrar si existe, el índice base cero y la frecuencia."
                },
                "La salida informa encontrado, primer índice y apariciones; si no existe, usa no, -1 y 0.",
                "Intermedia",
                "35–50 min",
                new[] { "Arreglos 01–05" },
                CrearGuiaBuscarValorArreglo()),
            CrearPractica(
                "grado2-arreglos-invertir",
                TemaArreglosGradoJuniorId,
                7,
                "Invertir un arreglo",
                "Invertir un arreglo",
                "Mostrar los elementos de un arreglo desde la última posición hasta la primera.",
                "Crear un programa que lea entre uno y doce enteros y produzca exactamente la secuencia inversa, sin ordenarla.",
                new[] { "índices", "recorrido inverso", "orden", "arreglos" },
                new[] {
                    "Leer y almacenar los elementos.",
                    "Ubicar el último índice válido.",
                    "Recorrer hacia atrás hasta el índice cero.",
                    "Mostrar cada valor una sola vez."
                },
                "La salida contiene exactamente los elementos originales en orden inverso.",
                "Fácil",
                "30–40 min",
                new[] { "Arreglos 01–06" },
                CrearGuiaInvertirArreglo()),
            CrearPractica(
                "grado2-arreglos-intercalar",
                TemaArreglosGradoJuniorId,
                8,
                "Intercalar dos arreglos",
                "Intercalar dos arreglos",
                "Combinar dos arreglos del mismo tamaño alternando sus elementos.",
                "Crear un programa que lea dos arreglos de igual longitud y forme una salida con un elemento de A seguido por el correspondiente de B.",
                new[] { "dos arreglos", "índices", "intercalado", "recorrido" },
                new[] {
                    "Leer la longitud compartida.",
                    "Capturar los elementos de A y de B.",
                    "Recorrer ambas colecciones con el mismo índice.",
                    "Mostrar A[i] y después B[i] en cada repetición."
                },
                "La salida alterna exactamente a0, b0, a1, b1 hasta el último par.",
                "Intermedia",
                "40–55 min",
                new[] { "Arreglos 01–07" },
                CrearGuiaIntercalarArreglos()),
            CrearPractica(
                "grado2-arreglos-sin-duplicados",
                TemaArreglosGradoJuniorId,
                9,
                "Valores únicos",
                "Valores únicos",
                "Obtener los valores sin duplicados conservando el orden de primera aparición.",
                "Crear un programa que recorra un arreglo, muestre cada valor una sola vez y reporte la cantidad de valores únicos.",
                new[] { "duplicados", "búsqueda", "orden de aparición", "contador" },
                new[] {
                    "Leer los elementos.",
                    "Para cada valor, revisar si ya fue aceptado.",
                    "Agregarlo solo la primera vez que aparece.",
                    "Mostrar la colección resultante y su cantidad."
                },
                "La salida conserva la primera aparición de cada valor y muestra la cantidad correcta; para n igual a cero, queda vacía.",
                "Intermedia",
                "45–60 min",
                new[] { "Arreglos 01–08" },
                CrearGuiaValoresUnicosArreglo()),
            CrearPractica(
                "grado2-arreglos-ordenar-segundo-mayor",
                TemaArreglosGradoJuniorId,
                10,
                "Ordenar y encontrar el segundo mayor",
                "Ordenar y encontrar el segundo mayor",
                "Ordenar un arreglo ascendentemente y localizar el segundo mayor valor distinto.",
                "Crear un programa que conserve duplicados al ordenar y que distinga entre la penúltima posición y el segundo valor mayor diferente.",
                new[] { "ordenamiento", "duplicados", "segundo mayor", "recorrido" },
                new[] {
                    "Leer al menos dos enteros.",
                    "Ordenar todos los elementos de menor a mayor sin eliminar duplicados.",
                    "Buscar desde el final el primer valor distinto del mayor.",
                    "Mostrar el arreglo ordenado y el segundo mayor o indicar que no existe."
                },
                "La salida presenta el arreglo ascendente y el segundo mayor distinto; si todos son iguales, indica que no existe.",
                "Intermedia",
                "50–65 min",
                new[] { "Arreglos 01–09" },
                CrearGuiaOrdenarSegundoMayorArreglo())
        });
    }

    private static GuiaPractica CrearGuiaCapturarMostrarArreglo() {
        return CrearGuiaArreglos(
            "Un programa que reserva espacio para varios enteros, captura exactamente la cantidad indicada y después muestra el arreglo sin alterar su orden.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; debe estar entre 1 y 10", "4"),
                DatoArreglo("elementos", "int[]", "Valores enteros almacenados por posición", "3, -1, 3, 0")
            },
            new[] {
                ConceptoArreglo("Arreglo", "Agrupa varios valores del mismo tipo bajo un solo nombre.", "int valores[10];"),
                ConceptoArreglo("Índice", "Identifica una posición; el primer elemento utiliza el índice 0.", "valores[0] = 3;"),
                ConceptoArreglo("Recorrido", "Permite visitar cada posición válida una vez.", "for (int i = 0; i < n; i++) { /* usar valores[i] */ }")
            },
            new[] {
                "Lee n y confirma que esté entre 1 y 10.",
                "Usa un ciclo para capturar exactamente n enteros.",
                "Guarda cada valor en la posición indicada por el contador.",
                "Realiza otro recorrido para mostrar desde el índice 0 hasta n - 1.",
                "Incluye una etiqueta clara antes de la colección.",
                "Prueba con valores repetidos, negativos y cero."
            },
            HerramientaArreglo(
                "Separar captura y presentación",
                "Dos recorridos breves mantienen clara la responsabilidad de cada etapa.",
                "El primer recorrido llena el arreglo y el segundo lo muestra sin mezclar ambas tareas.",
                "for (int i = 0; i < n; i++) {\n    cin >> valores[i];\n}\n\n// Después puede hacerse un recorrido independiente para mostrar.",
                "Es una organización recomendada, no una forma obligatoria de resolver la práctica."),
            "4\n3\n-1\n3\n0",
            "Arreglo: 3 -1 3 0",
            new[] {
                "Comenzar en el índice 1 y dejar vacía la primera posición.",
                "Recorrer hasta i <= n y salir de los límites.",
                "Mostrar n como si fuera un elemento.",
                "Cambiar el orden de captura.",
                "Omitir valores repetidos o el cero."
            },
            "EndForge revisará una colección etiquetada con exactamente n enteros y en el mismo orden.");
    }

    private static GuiaPractica CrearGuiaSumaElementosArreglo() {
        return CrearGuiaArreglos(
            "Un programa que almacena hasta veinte enteros y obtiene un único total al recorrerlos.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de valores; puede ser 0", "4"),
                DatoArreglo("elementos", "int[]", "Enteros que participarán en la suma", "5, -2, 7, 0"),
                DatoArreglo("suma", "int", "Acumulador del total", "10")
            },
            new[] {
                ConceptoArreglo("Acumulador", "Conserva el resultado parcial entre repeticiones.", "int suma = 0;"),
                ConceptoArreglo("Actualización", "Agrega el valor actual sin perder lo acumulado.", "suma += valorActual;"),
                ConceptoArreglo("Colección vacía", "Si no hay elementos, el acumulador permanece en su valor inicial.", "int suma = 0;")
            },
            new[] {
                "Lee una cantidad entre 0 y 20.",
                "Captura exactamente esa cantidad de enteros.",
                "Inicia la suma en cero.",
                "Recorre el arreglo y agrega cada elemento.",
                "Muestra un solo resultado con una etiqueta reconocible.",
                "Comprueba el caso n igual a cero y una lista de números negativos."
            },
            HerramientaArreglo(
                "Patrón de acumulador",
                "Un acumulador guarda un resultado que crece o disminuye durante un recorrido.",
                "Evita crear una variable diferente para cada elemento.",
                "int acumulado = 0;\nacumulado += valorActual;",
                "El nombre de la variable y la forma del ciclo son opcionales."),
            "4\n5\n-2\n7\n0",
            "Suma total: 10",
            new[] {
                "No iniciar el acumulador.",
                "Usar suma = elemento y borrar el total anterior.",
                "Sumar la cantidad n como si fuera un elemento.",
                "Ignorar los negativos.",
                "No mostrar una etiqueta para el resultado."
            },
            "EndForge revisará que la suma etiquetada sea única y correcta, incluido el caso de cero elementos.");
    }

    private static GuiaPractica CrearGuiaPromedioArreglo() {
        return CrearGuiaArreglos(
            "Un programa que almacena valores decimales, calcula su suma y obtiene la media aritmética.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de calificaciones o mediciones; de 1 a 10", "3"),
                DatoArreglo("valores", "double[]", "Datos que pueden contener decimales", "8.5, 7, 9"),
                DatoArreglo("promedio", "double", "Suma dividida entre n", "8.1667")
            },
            new[] {
                ConceptoArreglo("double", "Conserva la parte decimal de los valores y del resultado.", "double suma = 0.0;"),
                ConceptoArreglo("Promedio", "Se obtiene dividiendo la suma entre la cantidad de datos.", "double promedio = suma / n;"),
                ConceptoArreglo("Precisión", "El resultado puede tener más decimales que las entradas.", "cout << promedio;")
            },
            new[] {
                "Lee n entre 1 y 10.",
                "Captura n valores double.",
                "Acumula todos los elementos.",
                "Divide la suma entre n después del recorrido.",
                "Muestra el promedio con una etiqueta clara.",
                "Prueba un conjunto cuyo promedio no sea entero."
            },
            HerramientaArreglo(
                "División decimal",
                "Una operación con double conserva los decimales del cociente.",
                "Evita perder precisión cuando el promedio no es entero.",
                "double total = 25.0;\nint cantidad = 3;\ndouble media = total / cantidad;",
                "Este fragmento ilustra la división decimal con otros datos; no resuelve la práctica."),
            "3\n8.5\n7\n9",
            "Promedio: 8.1667",
            new[] {
                "Usar int para la suma o el promedio.",
                "Dividir dentro del ciclo en cada repetición.",
                "Dividir entre 10 en lugar de entre n.",
                "Mostrar la suma en lugar del promedio.",
                "Aceptar n igual a cero y dividir entre cero."
            },
            "EndForge aceptará pequeñas diferencias de presentación, pero el promedio debe coincidir con tolerancia de 0.01.");
    }

    private static GuiaPractica CrearGuiaMayorMenorArreglo() {
        return CrearGuiaArreglos(
            "Un programa que analiza todos los enteros del arreglo para encontrar sus extremos.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; de 1 a 15", "4"),
                DatoArreglo("elementos", "int[]", "Valores que se compararán", "3, 9, -1, 9"),
                DatoArreglo("mayor y menor", "int", "Extremos encontrados durante el recorrido", "9 y -1")
            },
            new[] {
                ConceptoArreglo("Valor inicial", "El primer elemento es una referencia válida aun cuando todos los números sean negativos.", "int mayor = valores[0];"),
                ConceptoArreglo("Comparación", "Cada elemento puede actualizar el mayor o el menor conocido.", "if (actual > mayor) { mayor = actual; }"),
                ConceptoArreglo("Duplicados", "Repetir un extremo no cambia su valor.", "9, 9")
            },
            new[] {
                "Lee al menos un elemento.",
                "Captura todos los enteros.",
                "Inicializa mayor y menor con la primera posición.",
                "Compara las posiciones restantes con ambos extremos.",
                "Muestra resultados con etiquetas diferentes.",
                "Prueba con un solo valor y con una lista completamente negativa."
            },
            HerramientaArreglo(
                "Inicializar desde el arreglo",
                "Usar el primer elemento evita inventar un límite que podría no servir para todas las entradas.",
                "Funciona con positivos, negativos, cero y un solo elemento.",
                "int mayor = valores[0];\nint menor = valores[0];",
                "Después todavía es necesario recorrer y comparar los demás elementos."),
            "4\n3\n9\n-1\n9",
            "Mayor: 9\nMenor: -1",
            new[] {
                "Iniciar mayor o menor en cero.",
                "Intercambiar las comparaciones > y <.",
                "No revisar la última posición.",
                "Mostrar el mismo valor para ambos resultados.",
                "Fallar cuando n vale 1."
            },
            "EndForge revisará por separado el mayor y el menor y rechazará resultados contradictorios.");
    }

    private static GuiaPractica CrearGuiaParesImparesArreglo() {
        return CrearGuiaArreglos(
            "Un programa que recorre enteros y mantiene dos contadores según la paridad de cada valor.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; de 0 a 20", "4"),
                DatoArreglo("elementos", "int[]", "Enteros positivos, negativos o cero", "-3, -2, 0, 7"),
                DatoArreglo("pares e impares", "int", "Cantidades de cada clasificación", "2 y 2")
            },
            new[] {
                ConceptoArreglo("Operador módulo", "El residuo entre 2 permite reconocer números pares.", "valor % 2"),
                ConceptoArreglo("Dos contadores", "Cada elemento incrementa exactamente uno de los contadores.", "int pares = 0;\nint impares = 0;"),
                ConceptoArreglo("Cero", "Cero es divisible entre dos y por eso cuenta como par.", "0 % 2 == 0")
            },
            new[] {
                "Lee n y los elementos.",
                "Inicia ambos contadores en cero.",
                "Recorre todas las posiciones.",
                "Incrementa pares cuando el residuo sea cero; en caso contrario incrementa impares.",
                "Muestra las dos cantidades con etiquetas distintas.",
                "Prueba con cero, negativos y una colección vacía."
            },
            HerramientaArreglo(
                "Clasificación exhaustiva",
                "Una condición con dos caminos garantiza que cada elemento se cuente una sola vez.",
                "Evita omitir valores o sumarlos a ambos contadores.",
                "if (esPar) {\n    pares++;\n}\nelse {\n    impares++;\n}",
                "La expresión que calcula esPar queda a elección del estudiante."),
            "4\n-3\n-2\n0\n7",
            "Pares: 2\nImpares: 2",
            new[] {
                "Contar el cero como impar.",
                "Comprobar residuo igual a 1 y fallar con negativos.",
                "Incrementar ambos contadores.",
                "Confundir la cantidad n con un elemento.",
                "Omitir las etiquetas."
            },
            "EndForge revisará ambas cantidades y considerará el cero como par.");
    }

    private static GuiaPractica CrearGuiaBuscarValorArreglo() {
        return CrearGuiaArreglos(
            "Un buscador lineal que informa si un objetivo aparece, dónde aparece por primera vez y cuántas veces se repite.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; de 1 a 20", "4"),
                DatoArreglo("elementos", "int[]", "Colección donde se realizará la búsqueda", "4, 2, 4, 9"),
                DatoArreglo("objetivo", "int", "Valor que se desea localizar", "4"),
                DatoArreglo("primer índice y apariciones", "int", "Posición base cero y frecuencia total", "0 y 2")
            },
            new[] {
                ConceptoArreglo("Búsqueda lineal", "Examina cada posición desde el inicio hasta el final.", "for (int i = 0; i < n; i++) { /* comparar */ }"),
                ConceptoArreglo("Índice centinela", "El valor -1 indica que aún no existe una posición encontrada.", "int primerIndice = -1;"),
                ConceptoArreglo("Frecuencia", "Debe seguir contando aunque ya se haya encontrado la primera aparición.", "apariciones++;")
            },
            new[] {
                "Lee el arreglo y después el objetivo.",
                "Inicia el primer índice en -1 y la frecuencia en cero.",
                "Recorre todos los elementos.",
                "Cuando haya coincidencia, guarda el índice solo si todavía vale -1.",
                "Incrementa la frecuencia en cada coincidencia.",
                "Muestra encontrado, primer índice y apariciones."
            },
            HerramientaArreglo(
                "Separar primera posición y frecuencia",
                "Dos variables representan datos diferentes de la misma búsqueda.",
                "Permite conservar la primera coincidencia mientras el recorrido continúa contando.",
                "int primerIndice = -1;\nint apariciones = 0;",
                "Estas inicializaciones son una técnica sugerida; no imponen nombres concretos."),
            "4\n4\n2\n4\n9\n4",
            "Encontrado: sí\nPrimer índice: 0\nApariciones: 2",
            new[] {
                "Usar índices desde uno.",
                "Sobrescribir el primer índice con la última coincidencia.",
                "Detenerse al encontrar y no contar las repeticiones.",
                "Mostrar encontrado aunque la frecuencia sea cero.",
                "No usar -1 cuando el objetivo no existe."
            },
            "EndForge acepta equivalencias de sí/no y exige índice base cero, frecuencia total y resultados coherentes.");
    }

    private static GuiaPractica CrearGuiaInvertirArreglo() {
        return CrearGuiaArreglos(
            "Un programa que presenta una colección desde su última posición válida hasta la primera.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; de 1 a 12", "4"),
                DatoArreglo("elementos", "int[]", "Valores originales", "1, 4, -2, 8"),
                DatoArreglo("invertido", "int[]", "Mismos valores en orden opuesto", "8, -2, 4, 1")
            },
            new[] {
                ConceptoArreglo("Último índice", "Para n elementos, la última posición válida es n - 1.", "int ultimo = n - 1;"),
                ConceptoArreglo("Recorrido descendente", "El índice disminuye hasta llegar a cero.", "for (int i = n - 1; i >= 0; i--) { /* usar valores[i] */ }"),
                ConceptoArreglo("Invertir no es ordenar", "Solo cambia la dirección de lectura; no compara magnitudes.", "1, 4, -2 → -2, 4, 1")
            },
            new[] {
                "Lee y almacena los elementos en orden normal.",
                "Comienza la salida en el índice n - 1.",
                "Disminuye el índice hasta cero.",
                "Muestra cada elemento exactamente una vez.",
                "Añade una etiqueta a la colección invertida.",
                "Prueba con un solo elemento y con duplicados."
            },
            HerramientaArreglo(
                "Índice espejo",
                "La posición opuesta de i puede expresarse usando el tamaño del arreglo.",
                "Es útil si se decide construir otro arreglo invertido en lugar de mostrar hacia atrás.",
                "int posicionOpuesta = n - 1 - i;",
                "No es obligatorio crear un segundo arreglo; un recorrido descendente también es válido."),
            "4\n1\n4\n-2\n8",
            "Arreglo invertido: 8 -2 4 1",
            new[] {
                "Empezar en n y acceder fuera del arreglo.",
                "Detenerse antes del índice cero.",
                "Ordenar los valores en vez de invertirlos.",
                "Eliminar duplicados.",
                "Agregar la cantidad n a la salida."
            },
            "EndForge revisará una colección etiquetada con orden inverso exacto y sin elementos adicionales.");
    }

    private static GuiaPractica CrearGuiaIntercalarArreglos() {
        return CrearGuiaArreglos(
            "Un programa que combina dos arreglos de igual tamaño alternando valores correspondientes.",
            new[] {
                DatoArreglo("n", "int", "Longitud compartida; de 1 a 8", "3"),
                DatoArreglo("arreglo A", "int[]", "Primera colección", "1, 2, 3"),
                DatoArreglo("arreglo B", "int[]", "Segunda colección", "8, 9, 10"),
                DatoArreglo("intercalado", "int[]", "Salida de longitud 2 × n", "1, 8, 2, 9, 3, 10")
            },
            new[] {
                ConceptoArreglo("Índice compartido", "La misma posición permite tomar una pareja de ambos arreglos.", "int valorA = a[i];\nint valorB = b[i];"),
                ConceptoArreglo("Alternancia", "En cada repetición aparece primero A[i] y luego B[i].", "a[i], b[i]"),
                ConceptoArreglo("Tamaño resultante", "Cada una de las n parejas aporta dos elementos.", "int total = 2 * n;")
            },
            new[] {
                "Lee n y captura n valores para A.",
                "Captura después n valores para B.",
                "Recorre las posiciones de cero a n - 1.",
                "En cada posición muestra primero A y luego B.",
                "Verifica que la salida tenga exactamente 2 × n valores.",
                "Prueba con ceros, negativos y arreglos de un elemento."
            },
            HerramientaArreglo(
                "Dos salidas por repetición",
                "Un solo índice puede coordinar dos colecciones del mismo tamaño.",
                "Mantiene juntas las parejas relacionadas y evita concatenar primero toda A y luego toda B.",
                "int primero = arregloA[i];\nint segundo = arregloB[i];",
                "El fragmento muestra una pareja aislada; no contiene el recorrido completo."),
            "3\n1\n2\n3\n8\n9\n10",
            "Intercalado: 1 8 2 9 3 10",
            new[] {
                "Mostrar primero todo A y después todo B.",
                "Comenzar por B.",
                "Cambiar el índice de solo uno de los arreglos.",
                "Producir n elementos en lugar de 2 × n.",
                "Omitir o repetir la última pareja."
            },
            "EndForge revisará la alternancia, el orden y la cantidad exacta de la colección etiquetada.");
    }

    private static GuiaPractica CrearGuiaValoresUnicosArreglo() {
        return CrearGuiaArreglos(
            "Un programa que construye una colección sin duplicados y conserva el orden en que cada valor apareció por primera vez.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; de 0 a 15", "5"),
                DatoArreglo("elementos", "int[]", "Colección que puede tener repeticiones", "4, 2, 4, 3, 2"),
                DatoArreglo("únicos", "int[]", "Primera aparición de cada valor", "4, 2, 3"),
                DatoArreglo("cantidad", "int", "Número de valores distintos", "3")
            },
            new[] {
                ConceptoArreglo("Duplicado", "Un valor es duplicado si ya apareció entre los elementos aceptados.", "bool repetido = false;"),
                ConceptoArreglo("Orden de aparición", "No se ordena la colección; se conserva la primera vez que se vio cada dato.", "4, 2, 4, 3 → 4, 2, 3"),
                ConceptoArreglo("Colección vacía", "Con n igual a cero no se capturan elementos y la cantidad de únicos es cero.", "int cantidadUnicos = 0;")
            },
            new[] {
                "Lee n y captura los elementos.",
                "Inicia vacía la colección de únicos.",
                "Para cada valor original, busca si ya fue aceptado.",
                "Agrégalo únicamente cuando no se haya encontrado.",
                "Muestra los únicos en orden de primera aparición.",
                "Muestra también su cantidad y prueba n igual a cero."
            },
            HerramientaArreglo(
                "Buscar antes de agregar",
                "Una búsqueda en la parte ya construida permite decidir si el valor es nuevo.",
                "Evita eliminar solo repeticiones consecutivas y conserva el orden original.",
                "bool yaExiste = false;\n// Buscar el valor entre las posiciones aceptadas.",
                "La técnica es opcional y el fragmento no incluye la solución completa."),
            "5\n4\n2\n4\n3\n2",
            "Valores únicos: 4 2 3\nCantidad: 3",
            new[] {
                "Ordenar los valores únicos.",
                "Eliminar solo duplicados consecutivos.",
                "Agregar varias veces el mismo valor.",
                "Contar elementos originales en lugar de distintos.",
                "Fallar cuando n vale cero."
            },
            "EndForge revisará la colección sin duplicados, su orden de primera aparición y la cantidad.");
    }

    private static GuiaPractica CrearGuiaOrdenarSegundoMayorArreglo() {
        return CrearGuiaArreglos(
            "Un programa que ordena todos los enteros ascendentemente y después identifica el segundo valor mayor diferente.",
            new[] {
                DatoArreglo("n", "int", "Cantidad de elementos; de 2 a 15", "5"),
                DatoArreglo("elementos", "int[]", "Valores originales, incluidos duplicados", "4, 9, 1, 9, 6"),
                DatoArreglo("ordenado", "int[]", "Todos los valores en orden ascendente", "1, 4, 6, 9, 9"),
                DatoArreglo("segundo mayor", "int o mensaje", "Mayor valor distinto del máximo", "6")
            },
            new[] {
                ConceptoArreglo("Orden ascendente", "Cada valor menor aparece antes que uno mayor, conservando duplicados.", "1, 4, 6, 9, 9"),
                ConceptoArreglo("Valor distinto", "El segundo mayor no siempre está en la penúltima posición.", "5, 5 → no existe"),
                ConceptoArreglo("Búsqueda desde el final", "Después de ordenar, se busca el primer valor diferente del máximo.", "int mayor = valores[n - 1];")
            },
            new[] {
                "Lee al menos dos enteros.",
                "Ordena el arreglo de menor a mayor sin eliminar duplicados.",
                "Muestra todos los valores ordenados con una etiqueta.",
                "Toma como máximo el último elemento.",
                "Busca hacia atrás el primer valor diferente.",
                "Si no existe, muestra un mensaje claro."
            },
            HerramientaArreglo(
                "Separar ordenamiento y búsqueda",
                "Resolver una tarea a la vez facilita comprobar el resultado.",
                "Primero deja el arreglo ordenado; después analiza sus últimas posiciones sin modificarlo.",
                "int mayor = valores[n - 1];\nbool existeSegundo = false;",
                "El algoritmo de ordenamiento y los nombres de variables quedan a elección del estudiante."),
            "5\n4\n9\n1\n9\n6",
            "Arreglo ordenado: 1 4 6 9 9\nSegundo mayor: 6",
            new[] {
                "Ordenar de mayor a menor.",
                "Eliminar duplicados del arreglo ordenado.",
                "Tomar siempre la penúltima posición.",
                "Considerar otro 9 como segundo mayor.",
                "Inventar un segundo mayor cuando todos los valores son iguales."
            },
            "EndForge revisará por separado la colección ascendente y el segundo mayor distinto o el mensaje de que no existe.");
    }

    private static GuiaPractica CrearGuiaArreglos(
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

    private static DatoGuiaPractica DatoArreglo(
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

    private static ConceptoGuiaPractica ConceptoArreglo(
        string nombre,
        string explicacion,
        string fragmento) {
        return new ConceptoGuiaPractica {
            Nombre = nombre,
            Explicacion = explicacion,
            Fragmento = fragmento
        };
    }

    private static HerramientaGuiaPractica HerramientaArreglo(
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
