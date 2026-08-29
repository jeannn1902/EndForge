using EndForge.Models;

namespace EndForge.Services;

public sealed partial class CursoService {
    public const string TemaCadenasGradoJuniorId = "grado2-cadenas";

    private const int TotalPracticasCadenasGradoJunior = 8;

    private static IReadOnlyList<PracticaCurso> CrearPracticasCadenasGradoJunior() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "grado2-cadenas-capturar-mostrar",
                TemaCadenasGradoJuniorId,
                1,
                "Capturar y mostrar una cadena",
                "Capturar y mostrar una cadena",
                "Leer una línea completa y mostrarla sin alterar su contenido.",
                "Crear un programa que capture texto con espacios, números y signos, y que lo presente conservando exactamente sus espacios internos.",
                new[] { "string", "getline", "líneas completas", "salida de texto" },
                new[] {
                    "Leer una línea completa.",
                    "Conservar todos los caracteres capturados.",
                    "Mostrar la cadena después de una etiqueta clara."
                },
                "La salida reproduce toda la línea capturada, respetando mayúsculas, acentos, signos y espacios internos.",
                "Inicial",
                "25–35 min",
                Array.Empty<string>(),
                CrearGuiaCapturarMostrarCadena()),
            CrearPractica(
                "grado2-cadenas-longitud",
                TemaCadenasGradoJuniorId,
                2,
                "Calcular la longitud de una cadena",
                "Calcular la longitud de una cadena",
                "Contar todos los caracteres que forman una línea de texto.",
                "Crear un programa que lea una línea completa y reporte su longitud, considerando también espacios, números y signos.",
                new[] { "string", "length", "size", "conteo de caracteres" },
                new[] {
                    "Leer la línea completa.",
                    "Obtener la cantidad total de caracteres.",
                    "Mostrar la longitud con una etiqueta reconocible."
                },
                "La salida presenta la cantidad exacta de caracteres; una cadena vacía produce longitud cero.",
                "Inicial",
                "25–35 min",
                new[] { "Cadenas 01" },
                CrearGuiaLongitudCadena()),
            CrearPractica(
                "grado2-cadenas-mayusculas-minusculas",
                TemaCadenasGradoJuniorId,
                3,
                "Convertir a mayúsculas y minúsculas",
                "Convertir a mayúsculas y minúsculas",
                "Generar dos versiones de una cadena cambiando únicamente las letras.",
                "Crear un programa que muestre una versión en mayúsculas y otra en minúsculas, conservando números, espacios y signos.",
                new[] { "string", "mayúsculas", "minúsculas", "recorrido de caracteres" },
                new[] {
                    "Leer una línea completa.",
                    "Preparar una versión en mayúsculas.",
                    "Preparar otra versión en minúsculas.",
                    "Mostrar ambas con etiquetas diferentes."
                },
                "La salida conserva la estructura del texto y diferencia correctamente las dos transformaciones.",
                "Fácil",
                "30–40 min",
                new[] { "Cadenas 01–02" },
                CrearGuiaMayusculasMinusculasCadena()),
            CrearPractica(
                "grado2-cadenas-invertir",
                TemaCadenasGradoJuniorId,
                4,
                "Invertir una cadena",
                "Invertir una cadena",
                "Recorrer una cadena desde su último carácter hasta el primero.",
                "Crear un programa que invierta todos los caracteres de una línea, incluidos los espacios y los signos.",
                new[] { "string", "índices", "recorrido inverso", "orden" },
                new[] {
                    "Leer la línea completa.",
                    "Identificar el último carácter válido.",
                    "Recorrer o construir el texto en orden inverso.",
                    "Mostrar el resultado con una etiqueta clara."
                },
                "La salida contiene exactamente los mismos caracteres en el orden contrario.",
                "Fácil",
                "30–40 min",
                new[] { "Cadenas 01–03" },
                CrearGuiaInvertirCadena()),
            CrearPractica(
                "grado2-cadenas-palindromo",
                TemaCadenasGradoJuniorId,
                5,
                "Detectar si una cadena es palíndromo",
                "Detectar si una cadena es palíndromo",
                "Comparar una frase con su orden inverso ignorando mayúsculas y espacios.",
                "Crear un programa que indique si una línea se lee igual en ambos sentidos después de ignorar diferencias de mayúsculas y espacios.",
                new[] { "palíndromo", "normalización", "comparación", "bool" },
                new[] {
                    "Leer la línea completa.",
                    "Preparar una forma comparable sin espacios y con un solo uso de mayúsculas.",
                    "Comparar los caracteres en sentidos opuestos.",
                    "Mostrar una única respuesta afirmativa o negativa."
                },
                "La salida indica sin contradicciones si la frase es o no un palíndromo.",
                "Intermedia",
                "35–50 min",
                new[] { "Cadenas 01–04" },
                CrearGuiaPalindromoCadena()),
            CrearPractica(
                "grado2-cadenas-contar-caracteres",
                TemaCadenasGradoJuniorId,
                6,
                "Contar vocales, consonantes, dígitos y espacios",
                "Contar vocales, consonantes, dígitos y espacios",
                "Clasificar los caracteres de una línea y acumular cuatro cantidades.",
                "Crear un programa que cuente vocales, consonantes, dígitos y espacios, sin clasificar los signos dentro de esas categorías.",
                new[] { "caracteres", "clasificación", "contadores", "vocales acentuadas" },
                new[] {
                    "Leer una línea completa.",
                    "Iniciar en cero los cuatro contadores.",
                    "Examinar cada carácter y clasificarlo una sola vez cuando corresponda.",
                    "Mostrar las cuatro cantidades con etiquetas diferentes."
                },
                "La salida presenta los cuatro conteos correctos, considerando mayúsculas y vocales acentuadas.",
                "Intermedia",
                "40–55 min",
                new[] { "Cadenas 01–05" },
                CrearGuiaContarCaracteresCadena()),
            CrearPractica(
                "grado2-cadenas-reemplazar-caracter",
                TemaCadenasGradoJuniorId,
                7,
                "Reemplazar todas las apariciones de un carácter",
                "Reemplazar todas las apariciones de un carácter",
                "Transformar una cadena sustituyendo cada coincidencia exacta de un carácter.",
                "Crear un programa que lea un texto, un carácter de origen y uno de destino, y reemplace todas las apariciones respetando mayúsculas.",
                new[] { "string", "char", "recorrido", "reemplazo" },
                new[] {
                    "Leer el texto completo.",
                    "Leer los caracteres de origen y destino.",
                    "Recorrer toda la cadena.",
                    "Sustituir cada coincidencia exacta.",
                    "Mostrar la cadena resultante."
                },
                "La salida conserva el texto salvo por todas las sustituciones solicitadas.",
                "Fácil",
                "30–45 min",
                new[] { "Cadenas 01–06" },
                CrearGuiaReemplazarCaracterCadena()),
            CrearPractica(
                "grado2-cadenas-contar-palabras",
                TemaCadenasGradoJuniorId,
                8,
                "Contar palabras de una frase",
                "Contar palabras de una frase",
                "Reconocer grupos de caracteres separados por uno o varios espacios.",
                "Crear un programa que cuente las palabras de una línea, ignorando los espacios sobrantes al inicio, al final o entre palabras.",
                new[] { "string", "palabras", "separadores", "estado de recorrido" },
                new[] {
                    "Leer una línea completa.",
                    "Distinguir espacios de caracteres que pertenecen a una palabra.",
                    "Detectar el inicio de cada nueva palabra.",
                    "Mostrar la cantidad total con una etiqueta clara."
                },
                "La salida presenta cero para una línea vacía o solo con espacios y cuenta correctamente separaciones múltiples.",
                "Intermedia",
                "35–50 min",
                new[] { "Cadenas 01–07" },
                CrearGuiaContarPalabrasCadena())
        });
    }

    private static GuiaPractica CrearGuiaCapturarMostrarCadena() {
        return CrearGuiaCadenas(
            "Un programa que recibe una línea completa y la presenta sin truncarla ni alterar sus caracteres.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Línea completa que puede incluir espacios, números y signos",
                    "Hola mundo 2026!")
            },
            new[] {
                ConceptoCadena(
                    "Cadena",
                    "Una cadena almacena una secuencia de caracteres en el mismo orden en que fueron escritos.",
                    "string texto;"),
                ConceptoCadena(
                    "Línea completa",
                    "Leer una línea completa permite conservar los espacios entre palabras.",
                    "getline(cin, texto);"),
                ConceptoCadena(
                    "Preservación",
                    "Mostrar la variable directamente evita separar o reconstruir sus palabras.",
                    "cout << \"Texto: \" << texto;")
            },
            new[] {
                "Declara una variable capaz de guardar texto.",
                "Lee una línea completa en una sola operación.",
                "No dividas el contenido por palabras.",
                "Muestra una etiqueta reconocible seguida de toda la cadena.",
                "Prueba con espacios, números, signos y cambios de mayúsculas."
            },
            HerramientaCadena(
                "getline",
                "Lee caracteres hasta encontrar el salto de línea.",
                "Permite capturar nombres y frases con espacios sin perder palabras.",
                "string mensaje;\ngetline(cin, mensaje);",
                "La función facilita la captura, pero la forma de organizar el programa queda a tu elección."),
            "Hola mundo 2026!",
            "Texto: Hola mundo 2026!",
            new[] {
                "Usar una lectura que se detenga en el primer espacio.",
                "Mostrar únicamente la primera palabra.",
                "Cambiar mayúsculas o signos al presentar el texto.",
                "Eliminar o agregar espacios internos.",
                "Olvidar la etiqueta de salida."
            },
            "EndForge comparará el contenido completo después de una etiqueta y conservará mayúsculas, acentos y espacios internos.");
    }

    private static GuiaPractica CrearGuiaLongitudCadena() {
        return CrearGuiaCadenas(
            "Un programa que mide una línea completa y reporta cuántos caracteres contiene.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Línea cuyos caracteres se contarán",
                    "Hola mundo"),
                DatoCadena(
                    "longitud",
                    "entero sin signo o int",
                    "Cantidad total de caracteres del texto",
                    "10")
            },
            new[] {
                ConceptoCadena(
                    "Longitud",
                    "La longitud incluye letras, espacios, números y signos que forman parte de la cadena.",
                    "texto.length();"),
                ConceptoCadena(
                    "Espacios",
                    "Un espacio almacenado ocupa una posición y también cuenta como carácter.",
                    "string ejemplo = \"a b\"; // longitud 3"),
                ConceptoCadena(
                    "Cadena vacía",
                    "Una cadena sin caracteres tiene longitud cero.",
                    "string vacia = \"\";")
            },
            new[] {
                "Lee la línea completa.",
                "Obtén la cantidad de caracteres almacenados.",
                "No descartes espacios ni signos.",
                "Muestra una sola longitud con una etiqueta clara.",
                "Comprueba una cadena vacía y otra con varios espacios."
            },
            HerramientaCadena(
                "length y size",
                "Ambas operaciones informan cuántos caracteres contiene una cadena.",
                "Evitan contar manualmente cuando solo se necesita conocer el tamaño almacenado.",
                "string palabra = \"curso\";\nauto cantidad = palabra.length();",
                "Puedes usar la operación que te resulte más clara; no se exige un nombre de variable concreto."),
            "Hola mundo",
            "Longitud: 10",
            new[] {
                "Contar únicamente las letras.",
                "Ignorar los espacios.",
                "Agregar uno por un supuesto terminador.",
                "Mostrar más de un resultado contradictorio.",
                "No identificar la cantidad con una etiqueta."
            },
            "EndForge comprobará la cantidad total de caracteres, incluido el caso de una cadena vacía.");
    }

    private static GuiaPractica CrearGuiaMayusculasMinusculasCadena() {
        return CrearGuiaCadenas(
            "Un programa que conserva el texto original como referencia y produce una versión en mayúsculas y otra en minúsculas.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Línea original que será transformada",
                    "Hola Mundo"),
                DatoCadena(
                    "mayúsculas",
                    "string",
                    "Versión con las letras en mayúsculas",
                    "HOLA MUNDO"),
                DatoCadena(
                    "minúsculas",
                    "string",
                    "Versión con las letras en minúsculas",
                    "hola mundo")
            },
            new[] {
                ConceptoCadena(
                    "No destruyas el original",
                    "Primero conserva texto. Después crea dos copias: una se transformará a mayúsculas y la otra a minúsculas.",
                    "string mayusculas = texto;\nstring minusculas = texto;"),
                ConceptoCadena(
                    "Recorrido",
                    "Examinar cada carácter permite preservar espacios, números y signos.",
                    "for (char& caracter : mayusculas) {\n    // transformar este carácter\n}"),
                ConceptoCadena(
                    "toupper y tolower",
                    "Estas funciones transforman un carácter individual. No transforman una string completa; por eso se usan dentro de un recorrido.",
                    "#include <cctype>\n\ncaracter = static_cast<char>(\n    std::toupper(static_cast<unsigned char>(caracter)));\n\ncaracter = static_cast<char>(\n    std::tolower(static_cast<unsigned char>(caracter)));"),
                ConceptoCadena(
                    "¿Por qué tantos casts?",
                    "toupper y tolower reciben un carácter convertido a unsigned char y devuelven int. El cast final lo devuelve a char de forma explícita.",
                    "char& caracter\nstatic_cast<unsigned char>(caracter)\nstatic_cast<char>(resultado)"),
                ConceptoCadena(
                    "Dos resultados independientes",
                    "Recorre cada copia por separado. Así la versión en mayúsculas no destruye el texto que todavía necesitas convertir a minúsculas.",
                    "for (char& caracter : mayusculas) {\n    caracter = ...;\n}\nfor (char& caracter : minusculas) {\n    caracter = ...;\n}")
            },
            new[] {
                "Lee la línea completa.",
                "Crea una copia para mayúsculas y otra para minúsculas.",
                "Incluye <cctype> para disponer de std::toupper y std::tolower.",
                "Recorre la copia de mayúsculas y reemplaza cada carácter por std::toupper.",
                "Recorre la copia de minúsculas y reemplaza cada carácter por std::tolower.",
                "Deja intactos los caracteres que no sean letras transformables, como espacios, números y signos.",
                "Muestra el texto original, la versión en mayúsculas y la versión en minúsculas con etiquetas distintas.",
                "Prueba texto ya transformado y texto con caracteres variados."
            },
            HerramientaCadena(
                "Transformar las dos copias",
                "La conversión ocurre carácter por carácter dentro de un recorrido. std::toupper y std::tolower no reciben una string completa.",
                "Permite conservar el texto original y generar dos resultados sin mezclar las transformaciones.",
                "#include <cctype>\n#include <iostream>\n#include <string>\nusing namespace std;\n\nint main() {\n    string texto;\n    getline(cin, texto);\n\n    string mayusculas = texto;\n    string minusculas = texto;\n\n    for (char& caracter : mayusculas) {\n        caracter = static_cast<char>(\n            toupper(static_cast<unsigned char>(caracter)));\n    }\n\n    for (char& caracter : minusculas) {\n        caracter = static_cast<char>(\n            tolower(static_cast<unsigned char>(caracter)));\n    }\n\n    cout << \"Original: \" << texto << '\\n';\n    cout << \"Mayúsculas: \" << mayusculas << '\\n';\n    cout << \"Minúsculas: \" << minusculas << '\\n';\n}",
                "Puedes escribir std::toupper y std::tolower en lugar de toupper y tolower si no usas using namespace std. El ejemplo enseña la estructura, no es obligatorio copiar los nombres."),
            "Hola Mundo",
            "Mayúsculas: HOLA MUNDO\nMinúsculas: hola mundo",
            new[] {
                "Mostrar las dos versiones iguales.",
                "Eliminar espacios durante la transformación.",
                "Cambiar números o signos.",
                "Sobrescribir una versión antes de conservar la otra.",
                "Confundir las etiquetas de salida."
            },
            "EndForge comparará ambas versiones por separado, distinguiendo mayúsculas, acentos y espacios.");
    }

    private static GuiaPractica CrearGuiaInvertirCadena() {
        return CrearGuiaCadenas(
            "Un programa que presenta todos los caracteres de una línea desde el último hasta el primero.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Línea original que se recorrerá en sentido contrario",
                    "EndForge 2"),
                DatoCadena(
                    "invertida",
                    "string",
                    "Cadena con todos los caracteres en orden inverso",
                    "2 egroFdnE")
            },
            new[] {
                ConceptoCadena(
                    "Último índice",
                    "En una cadena no vacía, la última posición válida está antes de su longitud.",
                    "int ultimo = static_cast<int>(texto.length()) - 1;"),
                ConceptoCadena(
                    "Recorrido inverso",
                    "El índice disminuye para visitar las posiciones desde el final.",
                    "for (int i = ultimo; i >= 0; i--) { /* usar texto[i] */ }"),
                ConceptoCadena(
                    "Caracteres y palabras",
                    "Invertir caracteres no es lo mismo que cambiar el orden de las palabras.",
                    "\"a b\" se convierte en \"b a\"")
            },
            new[] {
                "Lee la línea completa.",
                "Identifica desde qué posición debe comenzar el recorrido.",
                "Visita cada carácter exactamente una vez en sentido inverso.",
                "Incluye también espacios y signos.",
                "Muestra la cadena resultante con una etiqueta.",
                "Comprueba el comportamiento con una cadena vacía."
            },
            HerramientaCadena(
                "Construcción progresiva",
                "Una cadena de resultado puede recibir un carácter en cada repetición.",
                "Permite conservar el original y observar cómo se forma la versión invertida.",
                "string resultado;\nresultado += caracterActual;",
                "El fragmento solo muestra cómo agregar un carácter; no determina el recorrido completo."),
            "EndForge 2",
            "Invertida: 2 egroFdnE",
            new[] {
                "Invertir únicamente el orden de las palabras.",
                "Omitir el primer o el último carácter.",
                "Perder espacios o signos.",
                "Mostrar la cadena original.",
                "Agregar texto dentro del valor resultante."
            },
            "EndForge comprobará la cadena carácter por carácter, con espacios exactos y sensibilidad a mayúsculas.");
    }

    private static GuiaPractica CrearGuiaPalindromoCadena() {
        return CrearGuiaCadenas(
            "Un programa que decide si una frase se lee igual de izquierda a derecha y de derecha a izquierda después de ignorar espacios y mayúsculas.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Palabra o frase que se analizará",
                    "Anita lava la tina"),
                DatoCadena(
                    "esPalindromo",
                    "bool",
                    "Resultado lógico de la comparación",
                    "true")
            },
            new[] {
                ConceptoCadena(
                    "Normalización",
                    "Antes de comparar, crea una versión auxiliar: recorre el texto, ignora los espacios y guarda las letras en un mismo formato.",
                    "string comparable;\n// \"A n a\" → \"ana\""),
                ConceptoCadena(
                    "Extremos",
                    "Es posible comparar el primer carácter con el último y avanzar hacia el centro.",
                    "int izquierda = 0;\nint derecha = longitud - 1;"),
                ConceptoCadena(
                    "Resultado booleano",
                    "Comienza suponiendo que coincide. Si encuentras una pareja diferente, cambia el resultado a false y puedes detener la comparación.",
                    "bool esPalindromo = true;\nif (comparable[izquierda] != comparable[derecha]) {\n    esPalindromo = false;\n}"),
                ConceptoCadena(
                    "No confundas los textos",
                    "El texto original sirve para mostrarlo; comparable sirve únicamente para decidir. No elimines espacios del original.",
                    "cout << \"Original: \" << texto;\ncout << \"Palíndromo: \" << esPalindromo;")
            },
            new[] {
                "Lee la línea completa.",
                "Conserva texto para mostrarlo y crea comparable para analizarlo.",
                "Recorre texto y agrega a comparable solo los caracteres que vas a comparar.",
                "Convierte las letras de comparable a un mismo formato antes de compararlas.",
                "Coloca un índice al inicio y otro al final de comparable.",
                "Compara los extremos y acércalos al centro en cada repetición.",
                "Marca false al encontrar una diferencia y no vuelvas a afirmar que es palíndromo.",
                "Muestra una sola respuesta con una etiqueta reconocible.",
                "Prueba una frase con espacios y otra que no sea palíndromo."
            },
            HerramientaCadena(
                "Preparar y comparar por separado",
                "Primero construye comparable; después revisa sus extremos. Separar ambas tareas evita mezclar limpieza y decisión.",
                "Permite conservar el texto original y hacer que la comparación sea más fácil de seguir.",
                "string comparable;\nfor (char caracter : texto) {\n    if (caracter != ' ') {\n        comparable += caracter;\n    }\n}\n\n// Después compara comparable[izquierda]\n// con comparable[derecha].",
                "El fragmento es una guía parcial: todavía debes decidir cómo unificar mayúsculas y cómo mover los índices."),
            "Anita lava la tina",
            "Palíndromo: Sí",
            new[] {
                "Considerar diferentes las mayúsculas y minúsculas.",
                "Contar los espacios como diferencias.",
                "Mostrar sí y no en la misma ejecución.",
                "Comparar únicamente la primera y última letra.",
                "No reconocer una cadena de un carácter."
            },
            "EndForge aceptará respuestas booleanas equivalentes y rechazará afirmaciones contradictorias.");
    }

    private static GuiaPractica CrearGuiaContarCaracteresCadena() {
        return CrearGuiaCadenas(
            "Un programa que recorre una línea y resume cuántas vocales, consonantes, cifras y espacios contiene.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Línea cuyos caracteres serán clasificados",
                    "Hola 123"),
                DatoCadena(
                    "vocales",
                    "int",
                    "Cantidad de vocales, incluidas las acentuadas",
                    "2"),
                DatoCadena(
                    "consonantes",
                    "int",
                    "Cantidad de letras que no son vocales",
                    "2"),
                DatoCadena(
                    "dígitos",
                    "int",
                    "Cantidad de caracteres del 0 al 9",
                    "3"),
                DatoCadena(
                    "espacios",
                    "int",
                    "Cantidad de espacios",
                    "1")
            },
            new[] {
                ConceptoCadena(
                    "Clasificación",
                    "Cada carácter debe revisarse para decidir si pertenece a una de las categorías contadas.",
                    "char actual = texto[i];"),
                ConceptoCadena(
                    "Contadores",
                    "Cada categoría conserva su propia cantidad y comienza en cero.",
                    "int vocales = 0;\nint consonantes = 0;"),
                ConceptoCadena(
                    "Signos",
                    "Un signo puede formar parte del texto sin ser vocal, consonante, dígito ni espacio.",
                    "'+' y '!' no aumentan estas cantidades")
            },
            new[] {
                "Lee una línea completa.",
                "Inicia los cuatro contadores en cero.",
                "Recorre cada carácter una vez.",
                "Reconoce vocales sin importar mayúsculas y considera las acentuadas.",
                "Cuenta consonantes, dígitos y espacios en sus variables correspondientes.",
                "Ignora los signos para estas cantidades.",
                "Muestra los cuatro resultados con etiquetas distintas."
            },
            HerramientaCadena(
                "Funciones de clasificación",
                "Las utilidades de caracteres ayudan a reconocer letras, dígitos y espacios.",
                "Permiten expresar la intención de una comprobación con claridad.",
                "bool esDigito = isdigit(static_cast<unsigned char>(caracter));\nbool esEspacio = isspace(static_cast<unsigned char>(caracter));",
                "Son opcionales. Las vocales acentuadas requieren una decisión explícita acorde con el contrato."),
            "Hola 123",
            "Vocales: 2\nConsonantes: 2\nDígitos: 3\nEspacios: 1",
            new[] {
                "Contar signos como consonantes.",
                "Ignorar vocales acentuadas.",
                "Clasificar una mayúscula de forma diferente.",
                "Usar un solo contador para varias categorías.",
                "Presentar cantidades sin etiquetas claras."
            },
            "EndForge comprobará por separado las cuatro cantidades y detectará resultados repetidos que se contradigan.");
    }

    private static GuiaPractica CrearGuiaReemplazarCaracterCadena() {
        return CrearGuiaCadenas(
            "Un programa que localiza todas las coincidencias exactas de un carácter y las sustituye por otro.",
            new[] {
                DatoCadena(
                    "texto",
                    "string",
                    "Cadena original que será modificada",
                    "banana"),
                DatoCadena(
                    "origen",
                    "char",
                    "Carácter que se buscará respetando mayúsculas",
                    "a"),
                DatoCadena(
                    "destino",
                    "char",
                    "Carácter que ocupará cada posición encontrada",
                    "o"),
                DatoCadena(
                    "resultado",
                    "string",
                    "Cadena después de realizar todos los reemplazos",
                    "bonono")
            },
            new[] {
                ConceptoCadena(
                    "Coincidencia exacta",
                    "Una letra mayúscula y su minúscula son caracteres diferentes para esta práctica.",
                    "'A' != 'a'"),
                ConceptoCadena(
                    "Modificación por posición",
                    "Una posición encontrada puede recibir el carácter de destino.",
                    "texto[indice] = destino;"),
                ConceptoCadena(
                    "Todas las apariciones",
                    "El recorrido continúa después de la primera coincidencia.",
                    "revisar desde la primera hasta la última posición")
            },
            new[] {
                "Lee el texto completo.",
                "Lee un carácter de origen y uno de destino.",
                "Recorre todas las posiciones de la cadena.",
                "Sustituye cada coincidencia exacta.",
                "Conserva sin cambios los demás caracteres.",
                "Muestra el resultado completo con una etiqueta."
            },
            HerramientaCadena(
                "Referencia al carácter actual",
                "Un recorrido puede permitir modificar directamente el carácter que ocupa cada posición.",
                "Evita buscar nuevamente una posición que ya se está visitando.",
                "for (char& actual : texto) {\n    // Comparar y modificar actual cuando corresponda.\n}",
                "Es una opción de implementación; también puedes trabajar mediante índices."),
            "banana\na\no",
            "Resultado: bonono",
            new[] {
                "Reemplazar solamente la primera aparición.",
                "Ignorar la diferencia entre mayúsculas y minúsculas.",
                "Eliminar el carácter en lugar de sustituirlo.",
                "Modificar caracteres que no coinciden.",
                "Truncar la cadena al mostrarla."
            },
            "EndForge comparará la cadena final completa y exigirá que todas las apariciones exactas hayan sido reemplazadas.");
    }

    private static GuiaPractica CrearGuiaContarPalabrasCadena() {
        return CrearGuiaCadenas(
            "Un programa que reconoce el inicio de cada grupo de caracteres separado por espacios y reporta cuántas palabras hay.",
            new[] {
                DatoCadena(
                    "frase",
                    "string",
                    "Línea completa que puede tener espacios repetidos",
                    "  C++   con   EndForge"),
                DatoCadena(
                    "palabras",
                    "int",
                    "Cantidad de grupos no vacíos separados por espacios",
                    "3")
            },
            new[] {
                ConceptoCadena(
                    "Separador",
                    "Uno o varios espacios separan palabras, pero no forman palabras por sí mismos.",
                    "' ' indica una separación"),
                ConceptoCadena(
                    "Cambio de estado",
                    "Una palabra nueva comienza al pasar de un espacio a un carácter no espacial.",
                    "bool dentroDePalabra = false;"),
                ConceptoCadena(
                    "Casos vacíos",
                    "Una línea vacía o compuesta solo por espacios no contiene palabras.",
                    "int cantidad = 0;")
            },
            new[] {
                "Lee la frase completa.",
                "Inicia el contador en cero.",
                "Recorre la línea distinguiendo espacios y caracteres no espaciales.",
                "Aumenta la cantidad únicamente al comenzar una palabra nueva.",
                "No cuentes varias separaciones como palabras.",
                "Muestra el total con una etiqueta reconocible.",
                "Prueba espacios al inicio, al final y entre palabras."
            },
            HerramientaCadena(
                "Estado dentro o fuera de una palabra",
                "Una variable lógica puede recordar si el recorrido ya está dentro de un grupo de caracteres.",
                "Ayuda a contar el inicio de cada palabra sin contar cada letra.",
                "bool dentroDePalabra = false;\n// Actualizar el estado al encontrar espacios o caracteres.",
                "Es una técnica opcional; no establece el ciclo ni entrega la solución completa."),
            "  C++   con   EndForge",
            "Palabras: 3",
            new[] {
                "Contar espacios en lugar de palabras.",
                "Producir uno para una línea vacía.",
                "Aumentar el contador por cada carácter.",
                "Contar cada espacio repetido como otra palabra.",
                "No ignorar los espacios de los extremos."
            },
            "EndForge comprobará la cantidad de grupos no vacíos y aceptará uno o varios espacios como separación.");
    }

    private static GuiaPractica CrearGuiaCadenas(
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

    private static DatoGuiaPractica DatoCadena(
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

    private static ConceptoGuiaPractica ConceptoCadena(
        string nombre,
        string explicacion,
        string fragmento) {
        return new ConceptoGuiaPractica {
            Nombre = nombre,
            Explicacion = explicacion,
            Fragmento = fragmento
        };
    }

    private static HerramientaGuiaPractica HerramientaCadena(
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
