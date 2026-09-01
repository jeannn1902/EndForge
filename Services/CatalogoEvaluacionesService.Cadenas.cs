using EndForge.Models;

namespace EndForge.Services;

public sealed partial class CatalogoEvaluacionesService {
    public const string CadenasCapturarMostrarId =
        "grado2-cadenas-capturar-mostrar";
    public const string CadenasLongitudId = "grado2-cadenas-longitud";
    public const string CadenasMayusculasMinusculasId =
        "grado2-cadenas-mayusculas-minusculas";
    public const string CadenasInvertirId = "grado2-cadenas-invertir";
    public const string CadenasPalindromoId = "grado2-cadenas-palindromo";
    public const string CadenasContarCaracteresId =
        "grado2-cadenas-contar-caracteres";
    public const string CadenasReemplazarCaracterId =
        "grado2-cadenas-reemplazar-caracter";
    public const string CadenasContarPalabrasId =
        "grado2-cadenas-contar-palabras";

    private const int PuntosPorCasoCadenas = 12;

    private static DefinicionEvaluacionPractica CrearCadenasCapturarMostrar(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Texto",
            "Cadena",
            "Frase",
            "Contenido",
            "Resultado"
        };

        return CrearDefinicionCadenas(
            CadenasCapturarMostrarId,
            "Capturar y mostrar una cadena",
            "Leer una línea completa y mostrar exactamente su contenido.",
            "Se comprobará el texto completo después de una etiqueta reconocible.",
            "Una línea de texto que puede contener espacios, números y signos.",
            new[] { "Línea completa" },
            new[] {
                "Conservar todos los caracteres capturados.",
                "Preservar mayúsculas, acentos y espacios internos.",
                "Mostrar una etiqueta reconocible."
            },
            new[] {
                CrearCasoCadenaExactaCadenas(
                    "cadenas-capturar-hola-mundo",
                    "Frase con espacio",
                    EntradaCadena("Hola mundo"),
                    "Texto: Hola mundo",
                    "Comprueba que la lectura no termine en el primer espacio.",
                    "Hola mundo",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-capturar-cpp",
                    "Letras y signos",
                    EntradaCadena("C++ y EndForge"),
                    "Cadena: C++ y EndForge",
                    "Comprueba mayúsculas, signos y espacios.",
                    "C++ y EndForge",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-capturar-mixta",
                    "Números, letras y signos",
                    EntradaCadena("123 abc !?"),
                    "Contenido: 123 abc !?",
                    "Comprueba una cadena con distintos tipos de caracteres.",
                    "123 abc !?",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-capturar-varios-espacios",
                    "Frase de varias palabras",
                    EntradaCadena("Una frase con varios espacios"),
                    "Frase: Una frase con varios espacios",
                    "Comprueba que se preserve el contenido completo.",
                    "Una frase con varios espacios",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-capturar-oculto",
                    "Texto oculto",
                    EntradaCadena("Programar también es practicar."),
                    "Resultado: Programar también es practicar.",
                    "Comprueba una oración completa sin revelar el valor esperado.",
                    "Programar también es practicar.",
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasLongitud(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Longitud",
            "Caracteres",
            "Cantidad",
            "Tamaño",
            "Tamano",
            "Total de caracteres"
        };

        return CrearDefinicionCadenas(
            CadenasLongitudId,
            "Calcular la longitud de una cadena",
            "Contar todos los caracteres que forman una línea.",
            "Se comprobará una única longitud etiquetada, incluyendo espacios y signos.",
            "Una línea completa; puede estar vacía.",
            new[] { "Línea completa" },
            new[] {
                "Contar letras, espacios, números y signos.",
                "Mostrar cero para una cadena vacía.",
                "No incluir el terminador como carácter."
            },
            new[] {
                CrearCasoNumeroCadenas(
                    "cadenas-longitud-hola",
                    "Palabra corta",
                    EntradaCadena("Hola"),
                    "Longitud: 4",
                    "Comprueba cuatro letras.",
                    "Longitud",
                    4D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-longitud-con-espacio",
                    "Texto con espacio",
                    EntradaCadena("Hola mundo"),
                    "Caracteres: 10",
                    "Comprueba que el espacio también cuente.",
                    "Longitud",
                    10D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-longitud-vacia",
                    "Cadena vacía",
                    EntradaCadena(string.Empty),
                    "Cantidad: 0",
                    "Comprueba el caso sin caracteres.",
                    "Longitud",
                    0D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-longitud-signos",
                    "Letras y signos",
                    EntradaCadena("C++!"),
                    "Tamaño: 4",
                    "Comprueba que los signos formen parte de la longitud.",
                    "Longitud",
                    4D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-longitud-oculto",
                    "Texto mixto oculto",
                    EntradaCadena("123 abc"),
                    "Total de caracteres: 7",
                    "Comprueba números y un espacio sin revelar la respuesta.",
                    "Longitud",
                    7D,
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasMayusculasMinusculas(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasMayusculas = {
            "Mayúsculas",
            "Mayusculas",
            "En mayúsculas",
            "En mayusculas",
            "Texto superior"
        };
        string[] etiquetasMinusculas = {
            "Minúsculas",
            "Minusculas",
            "En minúsculas",
            "En minusculas",
            "Texto inferior"
        };

        return CrearDefinicionCadenas(
            CadenasMayusculasMinusculasId,
            "Convertir a mayúsculas y minúsculas",
            "Generar dos versiones de una línea transformando sus letras.",
            "Se compararán de forma independiente las versiones en mayúsculas y minúsculas.",
            "Una línea completa.",
            new[] { "Línea que se transformará" },
            new[] {
                "Mostrar las dos versiones.",
                "Preservar espacios, números y signos.",
                "Distinguir mayúsculas y minúsculas sin alterar otros caracteres."
            },
            new[] {
                CrearCasoDosCadenasExactasCadenas(
                    "cadenas-caso-hola-mundo",
                    "Mayúsculas mezcladas",
                    EntradaCadena("Hola Mundo"),
                    "Mayúsculas: HOLA MUNDO\nMinúsculas: hola mundo",
                    "Comprueba ambas transformaciones en una frase.",
                    "HOLA MUNDO",
                    "hola mundo",
                    etiquetasMayusculas,
                    etiquetasMinusculas,
                    visible: true),
                CrearCasoDosCadenasExactasCadenas(
                    "cadenas-caso-cpp",
                    "Texto con números y signos",
                    EntradaCadena("C++ 2026"),
                    "Mayúsculas: C++ 2026\nMinúsculas: c++ 2026",
                    "Comprueba que los caracteres no alfabéticos se conserven.",
                    "C++ 2026",
                    "c++ 2026",
                    etiquetasMayusculas,
                    etiquetasMinusculas,
                    visible: true),
                CrearCasoDosCadenasExactasCadenas(
                    "cadenas-caso-endforge",
                    "Texto inicialmente en mayúsculas",
                    EntradaCadena("ENDFORGE"),
                    "Mayúsculas: ENDFORGE\nMinúsculas: endforge",
                    "Comprueba una entrada ya convertida.",
                    "ENDFORGE",
                    "endforge",
                    etiquetasMayusculas,
                    etiquetasMinusculas,
                    visible: true),
                CrearCasoDosCadenasExactasCadenas(
                    "cadenas-caso-acentos",
                    "Texto con espacio y números",
                    EntradaCadena("programacion 2026"),
                    "Mayúsculas: PROGRAMACION 2026\nMinúsculas: programacion 2026",
                    "Comprueba que las letras, espacios y números se transformen sin cambiar su posición.",
                    "PROGRAMACION 2026",
                    "programacion 2026",
                    etiquetasMayusculas,
                    etiquetasMinusculas,
                    visible: true),
                CrearCasoDosCadenasExactasCadenas(
                    "cadenas-caso-mixto-oculto",
                    "Texto mixto oculto",
                    EntradaCadena("Mi Cadena #7"),
                    "Mayúsculas: MI CADENA #7\nMinúsculas: mi cadena #7",
                    "Comprueba ambas versiones sin revelar el texto esperado.",
                    "MI CADENA #7",
                    "mi cadena #7",
                    etiquetasMayusculas,
                    etiquetasMinusculas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasInvertir(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Invertida",
            "Cadena invertida",
            "Texto invertido",
            "Resultado",
            "Orden inverso"
        };

        return CrearDefinicionCadenas(
            CadenasInvertirId,
            "Invertir una cadena",
            "Mostrar todos los caracteres de una línea en orden inverso.",
            "Se comprobará una cadena exacta, incluidos espacios, signos y mayúsculas.",
            "Una línea completa; puede estar vacía.",
            new[] { "Línea que se invertirá" },
            new[] {
                "Invertir todos los caracteres.",
                "Preservar espacios y signos.",
                "No invertir únicamente el orden de las palabras."
            },
            new[] {
                CrearCasoCadenaExactaCadenas(
                    "cadenas-invertir-hola",
                    "Palabra simple",
                    EntradaCadena("Hola"),
                    "Invertida: aloH",
                    "Comprueba el orden inverso de cuatro caracteres.",
                    "aloH",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-invertir-cpp",
                    "Texto con signos",
                    EntradaCadena("C++"),
                    "Cadena invertida: ++C",
                    "Comprueba que los signos también cambien de posición.",
                    "++C",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-invertir-espacio",
                    "Texto con espacio",
                    EntradaCadena("a b"),
                    "Texto invertido: b a",
                    "Comprueba la posición exacta del espacio.",
                    "b a",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-invertir-vacia",
                    "Cadena vacía",
                    EntradaCadena(string.Empty),
                    "Resultado:",
                    "Comprueba que una cadena vacía produzca otra cadena vacía.",
                    string.Empty,
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-invertir-oculto",
                    "Texto oculto",
                    EntradaCadena("EndForge 2"),
                    "Orden inverso: 2 egroFdnE",
                    "Comprueba una cadena más larga sin revelar el resultado.",
                    "2 egroFdnE",
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasPalindromo(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Palíndromo",
            "Palindromo",
            "Es palíndromo",
            "Es palindromo",
            "Resultado"
        };

        return CrearDefinicionCadenas(
            CadenasPalindromoId,
            "Detectar si una cadena es palíndromo",
            "Decidir si una línea se lee igual en ambos sentidos al ignorar espacios y mayúsculas.",
            "Se comprobará una respuesta booleana flexible y se rechazarán contradicciones.",
            "Una línea completa.",
            new[] { "Texto que se analizará" },
            new[] {
                "Ignorar diferencias entre mayúsculas y minúsculas.",
                "Ignorar espacios al comparar.",
                "Mostrar una sola clasificación lógica."
            },
            new[] {
                CrearCasoBooleanoCadenas(
                    "cadenas-palindromo-reconocer",
                    "Palabra palíndroma",
                    EntradaCadena("reconocer"),
                    "Palíndromo: Sí",
                    "Comprueba una palabra de longitud impar.",
                    true,
                    etiquetas,
                    visible: true),
                CrearCasoBooleanoCadenas(
                    "cadenas-palindromo-frase",
                    "Frase con espacios y mayúscula",
                    EntradaCadena("Anita lava la tina"),
                    "Es palíndromo: Verdadero",
                    "Comprueba que se ignoren espacios y mayúsculas.",
                    true,
                    etiquetas,
                    visible: true),
                CrearCasoBooleanoCadenas(
                    "cadenas-palindromo-negativo",
                    "Texto no palíndromo",
                    EntradaCadena("EndForge"),
                    "Resultado: No",
                    "Comprueba una respuesta negativa.",
                    false,
                    etiquetas,
                    visible: true),
                CrearCasoBooleanoCadenas(
                    "cadenas-palindromo-un-caracter",
                    "Un solo carácter",
                    EntradaCadena("A"),
                    "Palindromo: True",
                    "Comprueba el caso mínimo no vacío.",
                    true,
                    etiquetas,
                    visible: true),
                CrearCasoBooleanoCadenas(
                    "cadenas-palindromo-oculto",
                    "Frase oculta",
                    EntradaCadena("Luz azul"),
                    "Resultado: Sí",
                    "Comprueba otra frase sin revelar su clasificación.",
                    true,
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasContarCaracteres(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasVocales = {
            "Vocales",
            "Cantidad de vocales",
            "Total vocales"
        };
        string[] etiquetasConsonantes = {
            "Consonantes",
            "Cantidad de consonantes",
            "Total consonantes"
        };
        string[] etiquetasDigitos = {
            "Dígitos",
            "Digitos",
            "Números",
            "Numeros",
            "Total dígitos",
            "Total digitos"
        };
        string[] etiquetasEspacios = {
            "Espacios",
            "Cantidad de espacios",
            "Total espacios"
        };

        return CrearDefinicionCadenas(
            CadenasContarCaracteresId,
            "Contar tipos de caracteres",
            "Clasificar los caracteres de una línea en cuatro cantidades.",
            "Se comprobarán cuatro resultados numéricos independientes.",
            "Una línea completa; puede estar vacía.",
            new[] { "Texto que se clasificará" },
            new[] {
                "Contar vocales acentuadas como vocales.",
                "Contar mayúsculas y minúsculas de la misma manera.",
                "Ignorar los signos en las cuatro cantidades."
            },
            new[] {
                CrearCasoConteosCadenas(
                    "cadenas-conteos-hola",
                    "Letras, espacio y dígitos",
                    EntradaCadena("Hola 123"),
                    "Vocales: 2\nConsonantes: 2\nDígitos: 3\nEspacios: 1",
                    "Comprueba las cuatro categorías en una entrada mixta.",
                    2,
                    2,
                    3,
                    1,
                    etiquetasVocales,
                    etiquetasConsonantes,
                    etiquetasDigitos,
                    etiquetasEspacios,
                    visible: true),
                CrearCasoConteosCadenas(
                    "cadenas-conteos-signos",
                    "Letra y signos",
                    EntradaCadena("C++"),
                    "Vocales: 0\nConsonantes: 1\nDígitos: 0\nEspacios: 0",
                    "Comprueba que los signos no se cuenten como consonantes.",
                    0,
                    1,
                    0,
                    0,
                    etiquetasVocales,
                    etiquetasConsonantes,
                    etiquetasDigitos,
                    etiquetasEspacios,
                    visible: true),
                CrearCasoConteosCadenas(
                    "cadenas-conteos-acento",
                    "Palabra con vocal acentuada",
                    EntradaCadena("programación"),
                    "Vocales: 5\nConsonantes: 7\nDígitos: 0\nEspacios: 0",
                    "Comprueba una vocal acentuada.",
                    5,
                    7,
                    0,
                    0,
                    etiquetasVocales,
                    etiquetasConsonantes,
                    etiquetasDigitos,
                    etiquetasEspacios,
                    visible: true),
                CrearCasoConteosCadenas(
                    "cadenas-conteos-vacia",
                    "Cadena vacía",
                    EntradaCadena(string.Empty),
                    "Vocales: 0\nConsonantes: 0\nDígitos: 0\nEspacios: 0",
                    "Comprueba cuatro cantidades en cero.",
                    0,
                    0,
                    0,
                    0,
                    etiquetasVocales,
                    etiquetasConsonantes,
                    etiquetasDigitos,
                    etiquetasEspacios,
                    visible: true),
                CrearCasoConteosCadenas(
                    "cadenas-conteos-oculto",
                    "Texto mixto oculto",
                    EntradaCadena("Año 2026!"),
                    "Vocales: 2\nConsonantes: 1\nDígitos: 4\nEspacios: 1",
                    "Comprueba mayúscula, eñe, dígitos y signo sin revelar cantidades.",
                    2,
                    1,
                    4,
                    1,
                    etiquetasVocales,
                    etiquetasConsonantes,
                    etiquetasDigitos,
                    etiquetasEspacios,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasReemplazarCaracter(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Resultado",
            "Reemplazada",
            "Cadena resultante",
            "Texto final",
            "Nueva cadena"
        };

        return CrearDefinicionCadenas(
            CadenasReemplazarCaracterId,
            "Reemplazar apariciones de un carácter",
            "Sustituir todas las coincidencias exactas de un carácter dentro de una línea.",
            "Se comprobará la cadena final completa y con sensibilidad a mayúsculas.",
            "Tres líneas: texto completo; carácter de origen; carácter de destino.",
            new[] { "Texto", "Carácter de origen", "Carácter de destino" },
            new[] {
                "Reemplazar todas las apariciones.",
                "Distinguir mayúsculas y minúsculas.",
                "Conservar espacios y caracteres no coincidentes."
            },
            new[] {
                CrearCasoCadenaExactaCadenas(
                    "cadenas-reemplazar-repetidas",
                    "Varias apariciones",
                    EntradaReemplazoCadenas("banana", "a", "o"),
                    "Resultado: bonono",
                    "Comprueba que se reemplacen todas las coincidencias.",
                    "bonono",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-reemplazar-mayusculas",
                    "Coincidencia sensible a mayúsculas",
                    EntradaReemplazoCadenas("EndForge", "e", "X"),
                    "Reemplazada: EndForgX",
                    "Comprueba que una E mayúscula no coincida con e.",
                    "EndForgX",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-reemplazar-ausente",
                    "Carácter ausente",
                    EntradaReemplazoCadenas("abc", "z", "x"),
                    "Cadena resultante: abc",
                    "Comprueba que la cadena se conserve si no hay coincidencias.",
                    "abc",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-reemplazar-signo",
                    "Reemplazo de signo",
                    EntradaReemplazoCadenas("1-2-3", "-", ":"),
                    "Texto final: 1:2:3",
                    "Comprueba caracteres no alfabéticos.",
                    "1:2:3",
                    etiquetas,
                    visible: true),
                CrearCasoCadenaExactaCadenas(
                    "cadenas-reemplazar-oculto",
                    "Mayúsculas y espacios",
                    EntradaReemplazoCadenas("A a A", "A", "x"),
                    "Nueva cadena: x a x",
                    "Comprueba coincidencias exactas sin revelar el resultado.",
                    "x a x",
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearCadenasContarPalabras(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetas = {
            "Palabras",
            "Cantidad de palabras",
            "Total de palabras",
            "Número de palabras",
            "Numero de palabras"
        };

        return CrearDefinicionCadenas(
            CadenasContarPalabrasId,
            "Contar palabras de una frase",
            "Contar grupos de caracteres separados por uno o varios espacios.",
            "Se comprobará una cantidad etiquetada, incluidos los casos vacío y con espacios repetidos.",
            "Una línea completa; puede estar vacía o contener espacios sobrantes.",
            new[] { "Frase completa" },
            new[] {
                "Ignorar espacios al inicio y al final.",
                "Tratar uno o varios espacios como separación.",
                "Mostrar cero si no hay palabras."
            },
            new[] {
                CrearCasoNumeroCadenas(
                    "cadenas-palabras-dos",
                    "Dos palabras",
                    EntradaCadena("Hola mundo"),
                    "Palabras: 2",
                    "Comprueba una separación simple.",
                    "Palabras",
                    2D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-palabras-espacios",
                    "Espacios múltiples",
                    EntradaCadena("  C++   con   EndForge"),
                    "Cantidad de palabras: 3",
                    "Comprueba espacios al inicio y separaciones repetidas.",
                    "Palabras",
                    3D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-palabras-vacia",
                    "Cadena vacía",
                    EntradaCadena(string.Empty),
                    "Total de palabras: 0",
                    "Comprueba una línea sin contenido.",
                    "Palabras",
                    0D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-palabras-una",
                    "Una palabra",
                    EntradaCadena("una"),
                    "Número de palabras: 1",
                    "Comprueba una entrada sin separadores.",
                    "Palabras",
                    1D,
                    etiquetas,
                    visible: true),
                CrearCasoNumeroCadenas(
                    "cadenas-palabras-oculto",
                    "Frase oculta",
                    EntradaCadena("Aprender requiere mucha práctica"),
                    "Palabras: 4",
                    "Comprueba una frase más larga sin revelar la cantidad.",
                    "Palabras",
                    4D,
                    etiquetas,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearDefinicionCadenas(
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

    private static CasoPrueba CrearCasoCadenaExactaCadenas(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        string valorEsperado,
        string[] etiquetas,
        bool visible) {
        return CrearCasoCadenas(
            id,
            nombre,
            entrada,
            salidaEsperada,
            descripcion,
            visible,
            cadenas: new[] {
                CadenaExactaCadenas(
                    "Texto resultante",
                    valorEsperado,
                    etiquetas)
            });
    }

    private static CasoPrueba CrearCasoDosCadenasExactasCadenas(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        string mayusculas,
        string minusculas,
        string[] etiquetasMayusculas,
        string[] etiquetasMinusculas,
        bool visible) {
        return CrearCasoCadenas(
            id,
            nombre,
            entrada,
            salidaEsperada,
            descripcion,
            visible,
            cadenas: new[] {
                CadenaExactaCadenas(
                    "Versión en mayúsculas",
                    mayusculas,
                    etiquetasMayusculas),
                CadenaExactaCadenas(
                    "Versión en minúsculas",
                    minusculas,
                    etiquetasMinusculas)
            });
    }

    private static CasoPrueba CrearCasoNumeroCadenas(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        string nombreValor,
        double valor,
        string[] etiquetas,
        bool visible) {
        return CrearCasoCadenas(
            id,
            nombre,
            entrada,
            salidaEsperada,
            descripcion,
            visible,
            numeros: new[] {
                NumeroCadenas(nombreValor, valor, etiquetas)
            });
    }

    private static CasoPrueba CrearCasoBooleanoCadenas(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        bool valor,
        string[] etiquetas,
        bool visible) {
        return CrearCasoCadenas(
            id,
            nombre,
            entrada,
            salidaEsperada,
            descripcion,
            visible,
            booleanos: new[] {
                BooleanoPalindromoCadenas(valor, etiquetas)
            });
    }

    private static CasoPrueba CrearCasoConteosCadenas(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        int vocales,
        int consonantes,
        int digitos,
        int espacios,
        string[] etiquetasVocales,
        string[] etiquetasConsonantes,
        string[] etiquetasDigitos,
        string[] etiquetasEspacios,
        bool visible) {
        return CrearCasoCadenas(
            id,
            nombre,
            entrada,
            salidaEsperada,
            descripcion,
            visible,
            numeros: new[] {
                NumeroCadenas("Vocales", vocales, etiquetasVocales),
                NumeroCadenas(
                    "Consonantes",
                    consonantes,
                    etiquetasConsonantes),
                NumeroCadenas("Dígitos", digitos, etiquetasDigitos),
                NumeroCadenas("Espacios", espacios, etiquetasEspacios)
            });
    }

    private static CasoPrueba CrearCasoCadenas(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        bool visible,
        ValorNumericoEsperado[]? numeros = null,
        ValorBooleanoEsperado[]? booleanos = null,
        ReglaCadenaEsperada[]? cadenas = null) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = salidaEsperada,
            EsVisible = visible,
            Puntos = PuntosPorCasoCadenas,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion = descripcion,
            ValoresNumericosEsperados = Array.AsReadOnly(
                numeros ?? Array.Empty<ValorNumericoEsperado>()),
            ValoresBooleanosEsperados = Array.AsReadOnly(
                booleanos ?? Array.Empty<ValorBooleanoEsperado>()),
            CadenasEsperadas = Array.AsReadOnly(
                cadenas ?? Array.Empty<ReglaCadenaEsperada>())
        };
    }

    private static ReglaCadenaEsperada CadenaExactaCadenas(
        string nombre,
        string valorEsperado,
        string[] etiquetas) {
        return new ReglaCadenaEsperada {
            Nombre = nombre,
            ValorEsperado = valorEsperado,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            Origen = OrigenCadenaEsperada.DespuesDeEtiqueta,
            DistinguirMayusculas = true,
            DistinguirAcentos = true,
            PoliticaEspacios = PoliticaEspaciosCadena.Exactos,
            PermitirTextoAdicional = false,
            Obligatoria = true,
            MensajeError =
                "El texto debe conservar exactamente sus caracteres y espacios."
        };
    }

    private static ValorNumericoEsperado NumeroCadenas(
        string nombre,
        double valor,
        string[] etiquetas) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            Valor = valor,
            Tolerancia = 0D,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas)
        };
    }

    private static ValorBooleanoEsperado BooleanoPalindromoCadenas(
        bool valor,
        string[] etiquetas) {
        return new ValorBooleanoEsperado {
            Nombre = "Palíndromo",
            Valor = valor,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            RepresentacionesVerdaderas = Array.AsReadOnly(new[] {
                "Sí",
                "Si",
                "Verdadero",
                "True",
                "Es palíndromo",
                "Es palindromo",
                "1"
            }),
            RepresentacionesFalsas = Array.AsReadOnly(new[] {
                "No",
                "Falso",
                "False",
                "No es palíndromo",
                "No es palindromo",
                "0"
            })
        };
    }

    private static string EntradaCadena(string texto) {
        return texto + "\n";
    }

    private static string EntradaReemplazoCadenas(
        string texto,
        string origen,
        string destino) {
        return texto + "\n" + origen + "\n" + destino + "\n";
    }
}
