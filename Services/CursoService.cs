using EndForge.Models;

namespace EndForge.Services;

public sealed class CursoService {
    public const int TotalPracticasPlaneadas = 50;
    public const int PracticasPlaneadasPorTema = 5;

    private const string MensajeProximamente = "Contenido próximamente";
    private readonly IReadOnlyList<TemaCurso> temas;

    public CursoService() {
        temas = CrearTemas();
    }

    public int TotalPracticasDisponibles => temas
        .Where(tema => !tema.EsProximamente)
        .Sum(tema => tema.Practicas.Count);

    public IReadOnlyList<TemaCurso> CargarTemas() {
        return temas;
    }

    public TemaCurso? ObtenerTema(string temaId) {
        if (string.IsNullOrWhiteSpace(temaId)) {
            return null;
        }

        return temas.FirstOrDefault(tema =>
            tema.Id.Equals(temaId, StringComparison.OrdinalIgnoreCase));
    }

    public PracticaCurso? ObtenerPractica(string practicaId) {
        if (string.IsNullOrWhiteSpace(practicaId)) {
            return null;
        }

        return temas
            .SelectMany(tema => tema.Practicas)
            .FirstOrDefault(practica =>
                practica.Id.Equals(practicaId, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<TemaCurso> CrearTemas() {
        IReadOnlyList<PracticaCurso> practicasVariables = CrearPracticasVariables();
        IReadOnlyList<PracticaCurso> practicasCondicionales = CrearPracticasCondicionales();
        IReadOnlyList<PracticaCurso> practicasCiclos = CrearPracticasCiclos();
        IReadOnlyList<PracticaCurso> practicasFunciones = CrearPracticasFunciones();

        return Array.AsReadOnly(new[] {
            CrearTemaDisponible(
                "variables",
                1,
                "Variables",
                "Aprende a guardar, modificar y utilizar información en tus programas.",
                practicasVariables),
            CrearTemaDisponible(
                "condicionales",
                2,
                "Condicionales",
                "Aprende a tomar decisiones en tus programas y ejecutar diferentes acciones según una condición.",
                practicasCondicionales),
            CrearTemaDisponible(
                "ciclos",
                3,
                "Ciclos",
                "Aprende a repetir acciones, recorrer secuencias y controlar procesos que se ejecutan varias veces.",
                practicasCiclos),
            CrearTemaDisponible(
                "funciones",
                4,
                "Funciones",
                "Aprende a dividir tus programas en bloques reutilizables, claros y fáciles de mantener.",
                practicasFunciones),
            CrearTemaProximamente(
                "strings",
                5,
                "Strings",
                "Trabaja con texto, caracteres y operaciones sobre cadenas."),
            CrearTemaProximamente(
                "arrays",
                6,
                "Arrays",
                "Agrupa y procesa colecciones de datos de tamaño fijo."),
            CrearTemaProximamente(
                "structs",
                7,
                "Structs",
                "Modela información relacionada mediante estructuras."),
            CrearTemaProximamente(
                "vectores",
                8,
                "Vectores",
                "Administra colecciones dinámicas con la biblioteca estándar."),
            CrearTemaProximamente(
                "archivos",
                9,
                "Archivos",
                "Guarda y recupera información utilizando archivos."),
            CrearTemaProximamente(
                "poo",
                10,
                "POO",
                "Construye programas mediante clases, objetos y encapsulamiento.")
        });
    }

    private static TemaCurso CrearTemaDisponible(
        string id,
        int numero,
        string nombre,
        string descripcion,
        IReadOnlyList<PracticaCurso> practicas) {
        return new TemaCurso {
            Id = id,
            Numero = numero,
            Nombre = nombre,
            NombreCarpeta = $"{numero:00}_{nombre}",
            Descripcion = descripcion,
            TotalPracticasPlaneadas = PracticasPlaneadasPorTema,
            Practicas = practicas,
            EsProximamente = false
        };
    }

    private static TemaCurso CrearTemaProximamente(
        string id,
        int numero,
        string nombre,
        string descripcion) {
        return new TemaCurso {
            Id = id,
            Numero = numero,
            Nombre = nombre,
            NombreCarpeta = $"{numero:00}_{nombre}",
            Descripcion = descripcion,
            TotalPracticasPlaneadas = PracticasPlaneadasPorTema,
            EsProximamente = true,
            MensajeDisponibilidad = MensajeProximamente
        };
    }

    private static IReadOnlyList<PracticaCurso> CrearPracticasCondicionales() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "condicionales-mayor-de-edad",
                "condicionales",
                1,
                "Mayor de edad",
                "MayorDeEdad",
                "Comparar la edad del usuario y mostrar si es mayor o menor de edad.",
                "Crear un programa que solicite la edad de una persona y determine si ya es mayor de edad. El resultado debe mostrarse con un mensaje claro.",
                new[] { "if", "else", "operadores relacionales", "bool" },
                new[] {
                    "Solicitar la edad.",
                    "Guardar el valor en una variable.",
                    "Comparar la edad con el límite correspondiente.",
                    "Mostrar uno de dos mensajes posibles."
                },
                "El programa indica correctamente si la persona es mayor o menor de edad.",
                "Fácil",
                "15–20 min",
                new[] { "Variables 01", "Variables 02" }),
            CrearPractica(
                "condicionales-clasificar-numero",
                "condicionales",
                2,
                "Número positivo, negativo o cero",
                "ClasificarNumero",
                "Clasificar un número según su valor.",
                "Crear un programa que solicite un número y determine si es positivo, negativo o igual a cero.",
                new[] { "if", "else if", "else", "operadores relacionales" },
                new[] {
                    "Pedir un número.",
                    "Compararlo con cero.",
                    "Mostrar una clasificación única."
                },
                "El programa muestra correctamente una de las tres clasificaciones.",
                "Fácil",
                "15–25 min",
                new[] { "Condicionales 01" }),
            CrearPractica(
                "condicionales-calificacion-aprobatoria",
                "condicionales",
                3,
                "Calificación aprobatoria",
                "CalificacionAprobatoria",
                "Determinar si una calificación es aprobatoria y mostrar su categoría.",
                "Crear un programa que solicite una calificación y muestre si es reprobatoria, suficiente, buena o excelente según rangos definidos.",
                new[] { "rangos", "operadores lógicos", "else if", "validación básica" },
                new[] {
                    "Solicitar una calificación.",
                    "Validar que esté dentro de un rango razonable.",
                    "Clasificarla mediante condiciones.",
                    "Mostrar el resultado."
                },
                "El programa clasifica correctamente la calificación y rechaza valores fuera de rango.",
                "Intermedia",
                "25–35 min",
                new[] { "Condicionales 01", "Condicionales 02" }),
            CrearPractica(
                "condicionales-descuento-compra",
                "condicionales",
                4,
                "Descuento de compra",
                "DescuentoCompra",
                "Aplicar diferentes descuentos según el total de una compra.",
                "Crear un programa que solicite el total de una compra y aplique un porcentaje de descuento según el rango alcanzado.",
                new[] { "condiciones anidadas", "porcentajes", "variables decimales", "rangos" },
                new[] {
                    "Solicitar el total de compra.",
                    "Determinar el porcentaje aplicable.",
                    "Calcular descuento y total final.",
                    "Mostrar un resumen."
                },
                "El programa aplica el descuento correcto y muestra el total antes y después del descuento.",
                "Intermedia",
                "25–40 min",
                new[] { "Variables 02", "Variables 05", "Condicionales 03" }),
            CrearPractica(
                "condicionales-menu-operaciones",
                "condicionales",
                5,
                "Menú de operaciones",
                "MenuOperaciones",
                "Crear un menú que permita elegir suma, resta, multiplicación o división.",
                "Crear una calculadora sencilla basada en opciones. El usuario elige una operación, ingresa dos valores y recibe el resultado correspondiente.",
                new[] {
                    "switch",
                    "validación",
                    "división entre cero",
                    "opciones de menú",
                    "control de flujo"
                },
                new[] {
                    "Mostrar un menú.",
                    "Leer la opción elegida.",
                    "Solicitar dos números.",
                    "Ejecutar la operación.",
                    "Validar opciones inválidas.",
                    "Evitar división entre cero."
                },
                "El programa ejecuta la operación seleccionada y maneja errores básicos.",
                "Reto",
                "40–60 min",
                new[] { "Condicionales 01–04", "Variables 02" })
        });
    }

    private static IReadOnlyList<PracticaCurso> CrearPracticasCiclos() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "ciclos-contar-uno-a-diez",
                "ciclos",
                1,
                "Contar del 1 al 10",
                "ContarUnoADiez",
                "Mostrar una secuencia utilizando un ciclo.",
                "Crear un programa que muestre en consola los números del 1 al 10 utilizando una estructura repetitiva.",
                new[] { "for", "contador", "incremento" },
                new[] {
                    "Crear un ciclo.",
                    "Iniciar un contador.",
                    "Mostrar cada valor.",
                    "Detenerse al llegar a 10."
                },
                "El programa imprime del 1 al 10 en orden.",
                "Fácil",
                "15–20 min",
                new[] { "Condicionales 01", "Variables 01" }),
            CrearPractica(
                "ciclos-tabla-multiplicar",
                "ciclos",
                2,
                "Tabla de multiplicar",
                "TablaMultiplicar",
                "Generar la tabla de multiplicar de un número elegido.",
                "Crear un programa que solicite un número y muestre su tabla de multiplicar del 1 al 10.",
                new[] { "for", "entrada del usuario", "operaciones repetitivas", "contador" },
                new[] {
                    "Pedir un número.",
                    "Recorrer del 1 al 10.",
                    "Multiplicar en cada iteración.",
                    "Mostrar cada operación."
                },
                "El programa genera correctamente la tabla del número elegido.",
                "Fácil",
                "20–25 min",
                new[] { "Ciclos 01" }),
            CrearPractica(
                "ciclos-suma-acumulada",
                "ciclos",
                3,
                "Suma acumulada",
                "SumaAcumulada",
                "Solicitar varios números y obtener su suma total.",
                "Crear un programa que solicite una cantidad definida de números y acumule su suma.",
                new[] { "acumulador", "for", "variables", "entrada repetida" },
                new[] {
                    "Solicitar cuántos valores se capturarán.",
                    "Repetir la lectura.",
                    "Acumular cada número.",
                    "Mostrar el total."
                },
                "El programa suma correctamente todos los valores ingresados.",
                "Intermedia",
                "25–35 min",
                new[] { "Ciclos 01", "Ciclos 02" }),
            CrearPractica(
                "ciclos-adivina-numero",
                "ciclos",
                4,
                "Adivina el número",
                "AdivinaNumero",
                "Repetir intentos hasta que el usuario adivine un número definido.",
                "Crear un juego sencillo donde el usuario intenta adivinar un número secreto. El programa debe indicar si el intento es mayor o menor.",
                new[] { "while", "comparación", "contador de intentos", "condiciones" },
                new[] {
                    "Definir un número secreto.",
                    "Solicitar intentos.",
                    "Repetir hasta acertar.",
                    "Dar pistas.",
                    "Mostrar cantidad de intentos."
                },
                "El programa termina únicamente cuando el usuario acierta.",
                "Intermedia",
                "30–40 min",
                new[] { "Condicionales 02", "Ciclos 03" }),
            CrearPractica(
                "ciclos-menu-repetitivo",
                "ciclos",
                5,
                "Menú repetitivo",
                "MenuRepetitivo",
                "Crear un menú que siga ejecutándose hasta elegir la opción de salida.",
                "Crear una aplicación de consola con varias opciones que se repita hasta que el usuario seleccione salir.",
                new[] { "do while", "switch", "control de flujo", "validación", "menús" },
                new[] {
                    "Mostrar opciones.",
                    "Leer la selección.",
                    "Ejecutar una acción sencilla.",
                    "Repetir el menú.",
                    "Finalizar solo con la opción de salida."
                },
                "El menú continúa activo y termina correctamente cuando el usuario lo indica.",
                "Reto",
                "40–60 min",
                new[] { "Condicionales 05", "Ciclos 01–04" })
        });
    }

    private static IReadOnlyList<PracticaCurso> CrearPracticasFunciones() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "funciones-saludo-personalizado",
                "funciones",
                1,
                "Saludo personalizado",
                "SaludoPersonalizado",
                "Crear una función que reciba un nombre y muestre un saludo.",
                "Crear un programa que solicite el nombre de una persona y utilice una función para mostrar un saludo personalizado.",
                new[] { "funciones", "parámetros", "string", "void" },
                new[] {
                    "Declarar una función.",
                    "Recibir un nombre como parámetro.",
                    "Llamarla desde main.",
                    "Mostrar el saludo."
                },
                "La función muestra correctamente el saludo utilizando el dato recibido.",
                "Fácil",
                "15–25 min",
                new[] { "Variables 01", "Condicionales 01" }),
            CrearPractica(
                "funciones-sumar-dos-numeros",
                "funciones",
                2,
                "Sumar dos números",
                "SumarDosNumeros",
                "Crear una función que reciba dos valores y devuelva su suma.",
                "Crear una función que reciba dos números y retorne el resultado de sumarlos.",
                new[] { "parámetros", "return", "int", "double", "llamada de funciones" },
                new[] {
                    "Declarar una función con retorno.",
                    "Recibir dos parámetros.",
                    "Retornar la suma.",
                    "Mostrar el resultado desde main."
                },
                "El programa obtiene y muestra correctamente la suma devuelta por la función.",
                "Fácil",
                "20–25 min",
                new[] { "Funciones 01", "Variables 02" }),
            CrearPractica(
                "funciones-numero-par",
                "funciones",
                3,
                "Determinar número par",
                "NumeroPar",
                "Crear una función booleana que indique si un número es par.",
                "Crear una función que reciba un número entero y retorne verdadero o falso según sea par.",
                new[] { "bool", "módulo", "return", "parámetros" },
                new[] {
                    "Crear una función booleana.",
                    "Evaluar el residuo.",
                    "Retornar el resultado.",
                    "Mostrar un mensaje desde main."
                },
                "El programa identifica correctamente números pares e impares.",
                "Intermedia",
                "25–30 min",
                new[] { "Funciones 02", "Condicionales 01" }),
            CrearPractica(
                "funciones-calcular-promedio",
                "funciones",
                4,
                "Calcular promedio",
                "CalcularPromedio",
                "Dividir el programa en funciones para capturar datos y calcular un promedio.",
                "Crear varias funciones para solicitar calificaciones, calcular el promedio y mostrar el resultado.",
                new[] {
                    "modularidad",
                    "parámetros",
                    "retorno",
                    "reutilización",
                    "separación de responsabilidades"
                },
                new[] {
                    "Crear una función de captura.",
                    "Crear una función de cálculo.",
                    "Crear una función de presentación.",
                    "Coordinar todo desde main."
                },
                "El programa calcula el promedio utilizando funciones separadas y reutilizables.",
                "Intermedia",
                "30–40 min",
                new[] { "Funciones 01–03", "Ciclos 03" }),
            CrearPractica(
                "funciones-calculadora-modular",
                "funciones",
                5,
                "Calculadora modular",
                "CalculadoraModular",
                "Crear una calculadora donde cada operación esté separada en una función.",
                "Crear una calculadora con menú en la que suma, resta, multiplicación y división estén implementadas en funciones independientes.",
                new[] {
                    "prototipos",
                    "múltiples funciones",
                    "switch",
                    "validación",
                    "división entre cero",
                    "modularidad"
                },
                new[] {
                    "Crear una función por operación.",
                    "Mostrar un menú.",
                    "Leer datos.",
                    "Llamar la función correcta.",
                    "Validar opciones.",
                    "Evitar división entre cero."
                },
                "La calculadora ejecuta cada operación mediante una función independiente.",
                "Reto",
                "40–60 min",
                new[] { "Funciones 01–04", "Condicionales 05", "Ciclos 05" })
        });
    }

    private static PracticaCurso CrearPractica(
        string id,
        string temaId,
        int numero,
        string nombre,
        string nombreProyecto,
        string objetivo,
        string descripcion,
        string[] conceptos,
        string[] instrucciones,
        string resultadoEsperado,
        string dificultad,
        string duracionEstimada,
        string[] requisitosPrevios) {
        return new PracticaCurso {
            Id = id,
            TemaId = temaId,
            Numero = numero,
            Nombre = nombre,
            NombreProyecto = nombreProyecto,
            Objetivo = objetivo,
            Descripcion = descripcion,
            Conceptos = Array.AsReadOnly(conceptos),
            Instrucciones = Array.AsReadOnly(instrucciones),
            ResultadoEsperado = resultadoEsperado,
            Dificultad = dificultad,
            DuracionEstimada = duracionEstimada,
            RequisitosPrevios = Array.AsReadOnly(requisitosPrevios)
        };
    }

    private static IReadOnlyList<PracticaCurso> CrearPracticasVariables() {
        return Array.AsReadOnly(new[] {
            new PracticaCurso {
                Id = "variables-datos-personales",
                TemaId = "variables",
                Numero = 1,
                Nombre = "Datos personales",
                NombreProyecto = "Datos personales",
                Objetivo = "Declarar variables de distintos tipos y mostrar su contenido.",
                Descripcion = "Crear un programa que almacene nombre, edad, estatura y estado de estudiante, y después muestre toda la información ordenada.",
                Conceptos = Array.AsReadOnly(new[] { "int", "double", "string", "bool", "cout", "cin" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Declara variables para guardar el nombre, la edad, la estatura y el estado de estudiante.",
                    "Solicita al usuario los valores necesarios desde la consola.",
                    "Muestra todos los datos con etiquetas claras y en un orden fácil de leer."
                }),
                ResultadoEsperado = "La consola solicita los datos personales y después presenta un resumen ordenado con todos los valores.",
                Dificultad = "Inicial",
                DuracionEstimada = "15–20 min",
                RequisitosPrevios = Array.Empty<string>(),
                Guia = new GuiaPractica {
                    QueVasAConstruir =
                        "Crear un programa de consola que solicite nombre, edad, estatura y estado de estudiante, " +
                        "guarde cada dato en una variable adecuada y muestre un resumen ordenado.",
                    DatosNecesarios = Array.AsReadOnly(new[] {
                        new DatoGuiaPractica {
                            Nombre = "Nombre",
                            Tipo = "string",
                            Descripcion = "Texto con el nombre",
                            Ejemplo = "Ana"
                        },
                        new DatoGuiaPractica {
                            Nombre = "Edad",
                            Tipo = "int",
                            Descripcion = "Años completos",
                            Ejemplo = "20"
                        },
                        new DatoGuiaPractica {
                            Nombre = "Estatura",
                            Tipo = "double",
                            Descripcion = "Metros con decimales",
                            Ejemplo = "1.65"
                        },
                        new DatoGuiaPractica {
                            Nombre = "Es estudiante",
                            Tipo = "bool",
                            Descripcion = "Verdadero o falso",
                            Ejemplo = "1"
                        }
                    }),
                    ExplicacionesConceptos = Array.AsReadOnly(new[] {
                        new ConceptoGuiaPractica {
                            Nombre = "string",
                            Explicacion = "Guarda texto, como un nombre o una ciudad.",
                            Fragmento = "string nombre;"
                        },
                        new ConceptoGuiaPractica {
                            Nombre = "int",
                            Explicacion = "Guarda números enteros, sin parte decimal.",
                            Fragmento = "int edad;"
                        },
                        new ConceptoGuiaPractica {
                            Nombre = "double",
                            Explicacion = "Guarda números que pueden tener decimales.",
                            Fragmento = "double estatura;"
                        },
                        new ConceptoGuiaPractica {
                            Nombre = "bool",
                            Explicacion = "Guarda un estado lógico: verdadero o falso.",
                            Fragmento = "bool esEstudiante;"
                        },
                        new ConceptoGuiaPractica {
                            Nombre = "cin",
                            Explicacion = "Lee un dato de la consola y lo guarda en una variable.",
                            Fragmento = "cin >> edad;"
                        },
                        new ConceptoGuiaPractica {
                            Nombre = "cout",
                            Explicacion = "Muestra texto y valores en la consola.",
                            Fragmento = "cout << \"Edad: \" << edad;"
                        }
                    }),
                    PasosSugeridos = Array.AsReadOnly(new[] {
                        "Declara una variable para cada dato.",
                        "Lee nombre, edad, estatura y estado de estudiante en ese orden.",
                        "Conserva la estatura como un valor decimal.",
                        "Muestra cada dato con una etiqueta clara.",
                        "Representa de forma comprensible el estado de estudiante.",
                        "Prueba el programa una vez con 1 y otra con 0.",
                        "No escribas directamente los datos de ejemplo en el código."
                    }),
                    AdvertenciaEvaluacion =
                        "EndForge envía el estado de estudiante como número:" + Environment.NewLine +
                        "1 representa verdadero." + Environment.NewLine +
                        "0 representa falso." + Environment.NewLine + Environment.NewLine +
                        "El programa no debe esperar las palabras “sí” o “no” como entrada.",
                    HerramientaUtil = new HerramientaGuiaPractica {
                        Nombre = "<string> y getline",
                        Descripcion =
                            "La biblioteca <string> permite guardar y trabajar con texto mediante el tipo string.",
                        ParaQueSirve =
                            "En esta práctica permite guardar nombres. getline puede leer una línea completa, incluyendo espacios.",
                        Codigo =
                            "#include <iostream>" + Environment.NewLine +
                            "#include <string>" + Environment.NewLine +
                            "using namespace std;" + Environment.NewLine + Environment.NewLine +
                            "int main() {" + Environment.NewLine +
                            "    string nombreCompleto;" + Environment.NewLine +
                            "    getline(cin, nombreCompleto);" + Environment.NewLine + Environment.NewLine +
                            "    return 0;" + Environment.NewLine +
                            "}",
                        AclaracionOpcional =
                            "Este fragmento únicamente muestra dónde se incluyen las bibliotecas y cómo se usa getline. " +
                            "No contiene la solución completa de la práctica."
                    },
                    EjemploEjecucion = new EjemploEjecucionPractica {
                        Entrada =
                            "Ana" + Environment.NewLine +
                            "20" + Environment.NewLine +
                            "1.65" + Environment.NewLine +
                            "1",
                        SalidaEsperada =
                            "Nombre: Ana" + Environment.NewLine +
                            "Edad: 20" + Environment.NewLine +
                            "Estatura: 1.65" + Environment.NewLine +
                            "Estudiante: sí"
                    },
                    ErroresComunes = Array.AsReadOnly(new[] {
                        "Usar int para la estatura.",
                        "Leer los datos en otro orden.",
                        "Esperar “sí” o “no” para el bool.",
                        "Escribir los ejemplos directamente en el código.",
                        "Mostrar valores sin etiquetas.",
                        "Omitir alguno de los cuatro datos.",
                        "Confundir >> con <<.",
                        "Olvidar iostream o string.",
                        "Usar coma decimal en vez de punto."
                    })
                }
            },
            new PracticaCurso {
                Id = "variables-ticket-compra",
                TemaId = "variables",
                Numero = 2,
                Nombre = "Ticket de compra",
                NombreProyecto = "Ticket de compra",
                Objetivo = "Guardar precio y cantidad para calcular subtotal y total.",
                Descripcion = "Crear un programa que solicite el nombre de un producto, su precio y la cantidad comprada, y muestre un ticket sencillo.",
                Conceptos = Array.AsReadOnly(new[] { "string", "double", "int", "cout", "cin", "operadores aritméticos" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Solicita el nombre, el precio unitario y la cantidad del producto.",
                    "Calcula el subtotal multiplicando el precio por la cantidad.",
                    "Presenta un ticket con los datos capturados y el total calculado."
                }),
                ResultadoEsperado = "La consola muestra un ticket legible con producto, precio, cantidad, subtotal y total.",
                Dificultad = "Inicial",
                DuracionEstimada = "20–25 min",
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01" })
            },
            new PracticaCurso {
                Id = "variables-conversor-temperatura",
                TemaId = "variables",
                Numero = 3,
                Nombre = "Conversor de temperatura",
                NombreProyecto = "Conversor de temperatura",
                Objetivo = "Aplicar fórmulas usando variables decimales.",
                Descripcion = "Crear un programa que convierta una temperatura de grados Celsius a Fahrenheit.",
                Conceptos = Array.AsReadOnly(new[] { "double", "cout", "cin", "operadores aritméticos" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Solicita una temperatura expresada en grados Celsius.",
                    "Aplica la fórmula de conversión utilizando variables decimales.",
                    "Muestra en consola el valor original y su equivalente en Fahrenheit."
                }),
                ResultadoEsperado = "La consola convierte correctamente una temperatura en Celsius y muestra el resultado en Fahrenheit.",
                Dificultad = "Inicial",
                DuracionEstimada = "20–25 min",
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01", "Variables 02" })
            },
            new PracticaCurso {
                Id = "variables-promedio-calificaciones",
                TemaId = "variables",
                Numero = 4,
                Nombre = "Promedio de calificaciones",
                NombreProyecto = "Promedio de calificaciones",
                Objetivo = "Guardar varias calificaciones y calcular su promedio.",
                Descripcion = "Crear un programa que solicite varias calificaciones, calcule el promedio y muestre el resultado.",
                Conceptos = Array.AsReadOnly(new[] { "double", "cout", "cin", "suma", "división" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Declara una variable para cada calificación solicitada.",
                    "Suma las calificaciones y divide el resultado entre la cantidad de valores.",
                    "Muestra el promedio con una etiqueta clara."
                }),
                ResultadoEsperado = "La consola captura las calificaciones y muestra el promedio calculado con valores decimales.",
                Dificultad = "Inicial",
                DuracionEstimada = "25–35 min",
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01–03" })
            },
            new PracticaCurso {
                Id = "variables-mini-recibo",
                TemaId = "variables",
                Numero = 5,
                Nombre = "Mini recibo",
                NombreProyecto = "Mini recibo",
                Objetivo = "Combinar texto, números y operaciones.",
                Descripcion = "Crear un recibo que incluya cliente, productos, cantidades, precios, subtotal y total.",
                Conceptos = Array.AsReadOnly(new[] { "string", "int", "double", "cout", "cin", "operadores aritméticos" }),
                Instrucciones = Array.AsReadOnly(new[] {
                    "Solicita el nombre del cliente y los datos de los productos.",
                    "Calcula los importes a partir de cantidades y precios.",
                    "Muestra un recibo ordenado con subtotales y total general."
                }),
                ResultadoEsperado = "La consola presenta un recibo completo y legible con cliente, productos, subtotales y total.",
                Dificultad = "Inicial",
                DuracionEstimada = "25–40 min",
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01–04" })
            }
        });
    }
}
