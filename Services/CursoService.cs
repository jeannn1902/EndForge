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
                new[] { "Variables 01", "Variables 02" },
                CrearGuiaMayorDeEdad()),
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
                new[] { "Condicionales 01" },
                CrearGuiaClasificarNumero()),
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
                new[] { "Condicionales 01", "Condicionales 02" },
                CrearGuiaCalificacionAprobatoria()),
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
                new[] { "Variables 02", "Variables 05", "Condicionales 03" },
                CrearGuiaDescuentoCompra()),
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
                new[] { "Condicionales 01–04", "Variables 02" },
                CrearGuiaMenuOperaciones())
        });
    }

    private static GuiaPractica CrearGuiaClasificarNumero() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que solicite un número y determine si es positivo, negativo o igual a cero.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Número",
                    Tipo = "double",
                    Descripcion = "Almacena el valor que escriba el usuario",
                    Ejemplo = "-7.5"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Comparación con cero",
                    Explicacion = "Permite conocer de qué lado del cero se encuentra un número.",
                    Fragmento = "numero > 0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operador >",
                    Explicacion = "Comprueba si el valor de la izquierda es mayor que el de la derecha.",
                    Fragmento = "numero > 0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operador <",
                    Explicacion = "Comprueba si el valor de la izquierda es menor que el de la derecha.",
                    Fragmento = "numero < 0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Igualdad",
                    Explicacion = "El operador == comprueba si dos valores son iguales. El else final también puede representar el caso cero.",
                    Fragmento = "numero == 0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "if",
                    Explicacion = "Ejecuta un bloque cuando su condición es verdadera.",
                    Fragmento = "if (numero > 0)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "else if",
                    Explicacion = "Comprueba otra condición únicamente cuando la anterior no se cumplió.",
                    Fragmento = "else if (numero < 0)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "else",
                    Explicacion = "Atiende el único caso restante sin necesitar otra comparación.",
                    Fragmento = "else"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Caminos mutuamente excluyentes",
                    Explicacion = "Significa que solo una clasificación puede elegirse durante cada ejecución.",
                    Fragmento = ""
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Solicita un número.",
                "Guárdalo en una variable double.",
                "Comprueba si es mayor que cero.",
                "En caso contrario, comprueba si es menor que cero.",
                "Usa else para identificar el cero.",
                "Muestra una sola clasificación."
            }),
            AdvertenciaEvaluacion =
                "La evaluación automática para esta práctica se añadirá próximamente. " +
                "Por ahora, revisa que el programa compile y clasifique correctamente valores positivos, negativos y cero.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Cadena if, else if y else",
                Descripcion =
                    "Permite representar varias posibilidades que no pueden ocurrir al mismo tiempo.",
                ParaQueSirve =
                    "En esta práctica garantiza que el número reciba una sola clasificación: positivo, negativo o cero.",
                Codigo =
                    "double numero;" + Environment.NewLine + Environment.NewLine +
                    "if (numero > 0) {" + Environment.NewLine +
                    "    // Caso positivo" + Environment.NewLine +
                    "}" + Environment.NewLine +
                    "else if (numero < 0) {" + Environment.NewLine +
                    "    // Caso negativo" + Environment.NewLine +
                    "}" + Environment.NewLine +
                    "else {" + Environment.NewLine +
                    "    // Caso cero" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta estructura es una herramienta opcional: no es obligatorio escribirla exactamente así si la solución " +
                    "produce una sola clasificación correcta. El fragmento no contiene la lectura ni la salida final."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "-7.5",
                SalidaEsperada =
                    "Número: -7.5" + Environment.NewLine +
                    "Clasificación: Negativo" + Environment.NewLine + Environment.NewLine +
                    "OTROS COMPORTAMIENTOS" + Environment.NewLine +
                    "Entrada 4.25 → Clasificación: Positivo" + Environment.NewLine +
                    "Entrada -2 → Clasificación: Negativo" + Environment.NewLine +
                    "Entrada 0 → Clasificación: Cero"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar >= 0 y clasificar el cero como positivo.",
                "Usar varios if independientes y mostrar más de un mensaje.",
                "Olvidar el caso cero.",
                "Invertir los operadores > y <.",
                "No mostrar la clasificación.",
                "Usar int y perder los decimales."
            })
        };
    }

    private static GuiaPractica CrearGuiaMayorDeEdad() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que solicite una edad válida y muestre si la persona es mayor o menor de edad, " +
                "o indique que la edad es inválida.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Edad",
                    Tipo = "int",
                    Descripcion = "Años completos de la persona; debe estar entre 0 y 120",
                    Ejemplo = "18"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "int",
                    Explicacion = "Guarda una edad expresada en años completos, sin decimales.",
                    Fragmento = "int edad;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación de rango",
                    Explicacion = "Comprueba primero que la edad se encuentre entre 0 y 120.",
                    Fragmento = "edad >= 0 && edad <= 120"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operador >=",
                    Explicacion = "Incluye el valor límite. Una persona con 18 años ya es mayor de edad.",
                    Fragmento = "edad >= 18"
                },
                new ConceptoGuiaPractica {
                    Nombre = "if, else if y else",
                    Explicacion = "Permiten separar edad inválida, mayoría de edad y minoría de edad.",
                    Fragmento = "if (edad < 0 || edad > 120)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "bool",
                    Explicacion = "Puede guardar el resultado verdadero o falso de una comparación.",
                    Fragmento = "bool esMayorDeEdad = edad >= 18;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Solicita la edad y guárdala en una variable int.",
                "Comprueba primero si es menor que 0 o mayor que 120.",
                "Si está fuera del rango, muestra Edad inválida.",
                "Si es válida, comprueba si es mayor o igual que 18.",
                "Muestra Mayor de edad cuando cumpla la condición.",
                "Usa el caso restante para mostrar Menor de edad.",
                "Muestra una sola clasificación por ejecución."
            }),
            AdvertenciaEvaluacion =
                "La evaluación automática para esta práctica se añadirá próximamente. " +
                "Por ahora, comprueba edades válidas, los límites 0, 18 y 120, y valores fuera de rango.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "bool para guardar una condición",
                Descripcion =
                    "Una variable bool puede conservar si una comparación resultó verdadera o falsa.",
                ParaQueSirve =
                    "Permite nombrar la condición edad >= 18 y hacer más clara una decisión posterior.",
                Codigo =
                    "int edadEjemplo = 18;" + Environment.NewLine +
                    "bool esMayorDeEdad = edadEjemplo >= 18;",
                AclaracionOpcional =
                    "Guardar la condición en bool es opcional; también puede compararse la edad directamente. " +
                    "Este fragmento no valida la edad ni muestra la clasificación final."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "18",
                SalidaEsperada =
                    "Edad: 18" + Environment.NewLine +
                    "Clasificación: Mayor de edad" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Edad 0 → Menor de edad" + Environment.NewLine +
                    "Edad 17 → Menor de edad" + Environment.NewLine +
                    "Edad 18 → Mayor de edad" + Environment.NewLine +
                    "Edad 120 → Mayor de edad" + Environment.NewLine +
                    "Edad -1 o 121 → Edad inválida"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar edad > 18 y clasificar los 18 años como menor de edad.",
                "Clasificar la edad antes de validar el rango de 0 a 120.",
                "Usar && en lugar de || para detectar valores fuera de rango.",
                "Olvidar el mensaje Edad inválida.",
                "Mostrar más de una clasificación.",
                "Usar un tipo decimal cuando la práctica solicita años completos."
            })
        };
    }

    private static GuiaPractica CrearGuiaCalificacionAprobatoria() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que solicite una calificación de 0 a 10, valide el valor y la clasifique como " +
                "Reprobatoria, Suficiente, Buena o Excelente.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Calificación",
                    Tipo = "double",
                    Descripcion = "Valor decimal que debe encontrarse entre 0 y 10",
                    Ejemplo = "8.5"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Conserva calificaciones que pueden contener decimales.",
                    Fragmento = "double calificacion;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Rango válido",
                    Explicacion = "Una calificación válida está entre 0 y 10, incluyendo ambos límites.",
                    Fragmento = "calificacion >= 0 && calificacion <= 10"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operadores && y ||",
                    Explicacion = "&& exige que dos condiciones se cumplan; || permite detectar cualquiera de dos errores.",
                    Fragmento = "calificacion < 0 || calificacion > 10"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Rangos ordenados",
                    Explicacion = "Cada else if atiende el intervalo que no fue clasificado antes.",
                    Fragmento = "calificacion < 6"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Valores frontera",
                    Explicacion = "Los valores 6, 8 y 9 comienzan categorías nuevas y deben probarse explícitamente.",
                    Fragmento = "6, 8, 9 y 10"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Solicita la calificación y guárdala como double.",
                "Comprueba primero si es menor que 0 o mayor que 10.",
                "Muestra Calificación inválida cuando esté fuera del rango.",
                "Clasifica como Reprobatoria un valor válido menor que 6.",
                "Clasifica como Suficiente desde 6 y antes de 8.",
                "Clasifica como Buena desde 8 y antes de 9.",
                "Usa el rango restante, de 9 a 10, para Excelente.",
                "Muestra una sola categoría."
            }),
            AdvertenciaEvaluacion =
                "La evaluación automática para esta práctica se añadirá próximamente. " +
                "Por ahora, prueba cada frontera del rango y valores inferiores a 0 o superiores a 10.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Validación de rango con && y ||",
                Descripcion =
                    "Los operadores lógicos permiten combinar comparaciones relacionadas.",
                ParaQueSirve =
                    "Ayudan a expresar tanto el rango válido de 0 a 10 como los dos posibles casos inválidos.",
                Codigo =
                    "double calificacionEjemplo = 8.5;" + Environment.NewLine +
                    "bool estaEnRango = calificacionEjemplo >= 0 && calificacionEjemplo <= 10;" + Environment.NewLine +
                    "bool estaFueraDeRango = calificacionEjemplo < 0 || calificacionEjemplo > 10;",
                AclaracionOpcional =
                    "Estas variables bool son opcionales; las comparaciones pueden usarse directamente en una condición. " +
                    "El fragmento no clasifica la calificación."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "8.5",
                SalidaEsperada =
                    "Calificación: 8.5" + Environment.NewLine +
                    "Clasificación: Buena" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "-0.1 → Calificación inválida" + Environment.NewLine +
                    "0 → Reprobatoria" + Environment.NewLine +
                    "6 → Suficiente" + Environment.NewLine +
                    "8 → Buena" + Environment.NewLine +
                    "9 y 10 → Excelente" + Environment.NewLine +
                    "10.1 → Calificación inválida"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar int y perder calificaciones decimales.",
                "Clasificar antes de validar el rango de 0 a 10.",
                "Dejar huecos o solapamientos entre categorías.",
                "Evaluar primero una condición demasiado amplia.",
                "Clasificar incorrectamente los valores exactos 6, 8, 9 o 10.",
                "Mostrar más de una categoría.",
                "Olvidar el caso Calificación inválida."
            })
        };
    }

    private static GuiaPractica CrearGuiaDescuentoCompra() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que reciba el total de una compra, seleccione el porcentaje de descuento correcto " +
                "y muestre total original, porcentaje, descuento y total final.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Total original",
                    Tipo = "double",
                    Descripcion = "Importe de la compra antes de aplicar descuentos",
                    Ejemplo = "1200.00"
                },
                new DatoGuiaPractica {
                    Nombre = "Porcentaje",
                    Tipo = "double",
                    Descripcion = "Porcentaje seleccionado según el rango de la compra",
                    Ejemplo = "10"
                },
                new DatoGuiaPractica {
                    Nombre = "Descuento",
                    Tipo = "double",
                    Descripcion = "Cantidad monetaria que se resta al total original",
                    Ejemplo = "120.00"
                },
                new DatoGuiaPractica {
                    Nombre = "Total final",
                    Tipo = "double",
                    Descripcion = "Importe que queda después de restar el descuento",
                    Ejemplo = "1080.00"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Conserva importes, porcentajes y resultados con decimales.",
                    Fragmento = "double totalCompra;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación",
                    Explicacion = "Un total negativo es inválido y no debe recibir descuento.",
                    Fragmento = "totalCompra < 0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Rangos de mayor a menor",
                    Explicacion = "Comprobar primero el límite más alto evita que un importe grande caiga en un rango menor.",
                    Fragmento = "totalCompra >= 2000"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Porcentaje",
                    Explicacion = "El descuento monetario se obtiene multiplicando el total por el porcentaje dividido entre 100.",
                    Fragmento = "double descuentoEjemplo = 1200.0 * 10.0 / 100.0;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Total final",
                    Explicacion = "Se obtiene restando el descuento calculado al total original.",
                    Fragmento = "double finalEjemplo = 1200.0 - 120.0;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Solicita el total original y guárdalo como double.",
                "Si es negativo, muestra Total inválido y no realices el cálculo.",
                "Para un total válido, comprueba primero si es mayor o igual que 2000 y asigna 15 %.",
                "Después comprueba desde 1000 para asignar 10 %.",
                "Después comprueba desde 500 para asignar 5 %.",
                "Usa 0 % para cualquier total válido menor que 500.",
                "Calcula el importe del descuento y réstalo al total original.",
                "Muestra total original, porcentaje, descuento y total final."
            }),
            AdvertenciaEvaluacion =
                "La evaluación automática para esta práctica se añadirá próximamente. " +
                "Por ahora, prueba valores negativos y los límites 500, 1000 y 2000.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Constantes y rangos de mayor a menor",
                Descripcion =
                    "Las constantes asignan nombres claros a límites y porcentajes que no cambian.",
                ParaQueSirve =
                    "Facilitan reconocer las reglas del descuento. Evaluar primero el rango mayor evita coincidencias prematuras.",
                Codigo =
                    "const double LIMITE_DESCUENTO_15 = 2000.0;" + Environment.NewLine +
                    "const double PORCENTAJE_15 = 15.0;" + Environment.NewLine + Environment.NewLine +
                    "if (totalCompra >= LIMITE_DESCUENTO_15) {" + Environment.NewLine +
                    "    porcentaje = PORCENTAJE_15;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Las constantes y este orden son sugerencias para hacer el código más claro; no son requisitos obligatorios. " +
                    "El fragmento muestra un solo rango y no resuelve todos los descuentos."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "1200",
                SalidaEsperada =
                    "Total original: 1200.00" + Environment.NewLine +
                    "Porcentaje: 10 %" + Environment.NewLine +
                    "Descuento: 120.00" + Environment.NewLine +
                    "Total final: 1080.00" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "-1 → Total inválido" + Environment.NewLine +
                    "0 y 499.99 → 0 %" + Environment.NewLine +
                    "500 y 999.99 → 5 %" + Environment.NewLine +
                    "1000 y 1999.99 → 10 %" + Environment.NewLine +
                    "2000 → 15 %"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Aplicar un descuento a un total negativo.",
                "Evaluar primero el rango más bajo y no alcanzar los descuentos mayores.",
                "Restar 5, 10 o 15 como importe fijo en lugar de calcular un porcentaje.",
                "Confundir el porcentaje con el importe del descuento.",
                "Clasificar mal los valores exactos 500, 1000 o 2000.",
                "Usar int y perder los decimales.",
                "Omitir alguno de los cuatro datos solicitados en la salida."
            })
        };
    }

    private static GuiaPractica CrearGuiaMenuOperaciones() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Una calculadora de consola que reciba una opción y dos números, ejecute suma, resta, " +
                "multiplicación o división y maneje opciones inválidas y división entre cero.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Opción",
                    Tipo = "int",
                    Descripcion = "Selecciona 1 Suma, 2 Resta, 3 Multiplicación o 4 División",
                    Ejemplo = "3"
                },
                new DatoGuiaPractica {
                    Nombre = "Primer operando",
                    Tipo = "double",
                    Descripcion = "Primer número utilizado por la operación",
                    Ejemplo = "4.5"
                },
                new DatoGuiaPractica {
                    Nombre = "Segundo operando",
                    Tipo = "double",
                    Descripcion = "Segundo número y divisor cuando la opción es 4",
                    Ejemplo = "2"
                },
                new DatoGuiaPractica {
                    Nombre = "Resultado",
                    Tipo = "double",
                    Descripcion = "Valor calculado únicamente para una operación válida",
                    Ejemplo = "9"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "switch",
                    Explicacion = "Selecciona un camino según el valor exacto de la opción.",
                    Fragmento = "switch (opcion)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "case",
                    Explicacion = "Representa una opción concreta del menú.",
                    Fragmento = "case 1:"
                },
                new ConceptoGuiaPractica {
                    Nombre = "break",
                    Explicacion = "Finaliza un case para impedir que continúe con la operación siguiente.",
                    Fragmento = "break;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "default",
                    Explicacion = "Atiende cualquier opción distinta de 1, 2, 3 o 4.",
                    Fragmento = "default:"
                },
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Conserva operandos y resultados con decimales, especialmente en la división.",
                    Fragmento = "double numero1, numero2;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación del divisor",
                    Explicacion = "Antes de dividir, comprueba que el segundo operando no sea cero.",
                    Fragmento = "numero2 == 0"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Muestra las opciones 1 Suma, 2 Resta, 3 Multiplicación y 4 División.",
                "Lee la opción como int y después los dos operandos como double.",
                "Usa switch para seleccionar la operación correspondiente.",
                "Añade break al terminar cada case.",
                "En la división, comprueba primero si el segundo operando es cero.",
                "Si el divisor es cero, muestra un error y no calcules ni muestres un resultado.",
                "Usa default para mostrar Opción inválida.",
                "Muestra el resultado solo cuando la opción y la operación sean válidas."
            }),
            AdvertenciaEvaluacion =
                "La evaluación automática para esta práctica se añadirá próximamente. " +
                "Por ahora, prueba las cuatro opciones, una opción inválida y una división entre cero.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "switch con validación adicional",
                Descripcion =
                    "switch, case, break y default organizan opciones exactas de un menú.",
                ParaQueSirve =
                    "Permite separar cada operación. Un if dentro de División atiende el caso especial del divisor cero.",
                Codigo =
                    "switch (opcion) {" + Environment.NewLine +
                    "    case 1:" + Environment.NewLine +
                    "        // Suma" + Environment.NewLine +
                    "        break;" + Environment.NewLine +
                    "    case 4:" + Environment.NewLine +
                    "        if (numero2 == 0) {" + Environment.NewLine +
                    "            // Informar el error" + Environment.NewLine +
                    "        }" + Environment.NewLine +
                    "        break;" + Environment.NewLine +
                    "    default:" + Environment.NewLine +
                    "        // Opción inválida" + Environment.NewLine +
                    "        break;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta organización es una sugerencia y no obliga a copiar exactamente el fragmento. " +
                    "Solo muestra dos ramas y no contiene cálculos, lectura ni salida final."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "3" + Environment.NewLine +
                    "4.5" + Environment.NewLine +
                    "2",
                SalidaEsperada =
                    "Operación: Multiplicación" + Environment.NewLine +
                    "Resultado: 9" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Opción 1 → Suma" + Environment.NewLine +
                    "Opción 2 → Resta" + Environment.NewLine +
                    "Opción 3 → Multiplicación" + Environment.NewLine +
                    "Opción 4 con divisor distinto de cero → División" + Environment.NewLine +
                    "Opción 4 con divisor cero → Error de división entre cero, sin resultado" + Environment.NewLine +
                    "Cualquier otra opción → Opción inválida"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar un tipo entero para los operandos y perder resultados decimales.",
                "Asociar una opción con la operación equivocada.",
                "Olvidar break y ejecutar más de un case.",
                "Dividir entre cero.",
                "Mostrar un resultado después de detectar división entre cero.",
                "Olvidar default para las opciones inválidas.",
                "Calcular o mostrar un resultado para una opción desconocida."
            })
        };
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
                new[] { "Condicionales 01", "Variables 01" },
                CrearGuiaContarUnoADiez()),
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
                new[] { "Ciclos 01" },
                CrearGuiaTablaMultiplicar()),
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
                new[] { "Ciclos 01", "Ciclos 02" },
                CrearGuiaSumaAcumulada()),
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
                new[] { "Condicionales 02", "Ciclos 03" },
                CrearGuiaAdivinaNumero()),
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
                new[] { "Condicionales 05", "Ciclos 01–04" },
                CrearGuiaMenuRepetitivo())
        });
    }

    private static GuiaPractica CrearGuiaContarUnoADiez() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que utilice un ciclo para mostrar los números del 1 al 10 en orden.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Contador",
                    Tipo = "int",
                    Descripcion = "Controla el número actual de cada repetición",
                    Ejemplo = "Valor inicial sugerido: 1"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Ciclo for",
                    Explicacion = "Repite un bloque cuando se conocen el inicio, el límite y la forma de avanzar.",
                    Fragmento = "for (inicio; condicion; incremento)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Contador",
                    Explicacion = "Es una variable que representa la repetición actual.",
                    Fragmento = "int contador;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Inicialización",
                    Explicacion = "Asigna el primer valor antes de comenzar las repeticiones.",
                    Fragmento = "int contador = 1"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Condición",
                    Explicacion = "Indica si el ciclo debe ejecutar otra repetición.",
                    Fragmento = "contador <= 10"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Incremento",
                    Explicacion = "Aumenta el contador en una unidad después de cada repetición.",
                    Fragmento = "contador++"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Límite inclusivo",
                    Explicacion = "El operador <= permite incluir el valor 10 en la secuencia.",
                    Fragmento = "contador <= 10"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Repetición controlada",
                    Explicacion = "El ciclo termina cuando el contador deja de cumplir la condición.",
                    Fragmento = ""
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Crea una variable contador.",
                "Inicia el contador en 1.",
                "Mantén el ciclo mientras contador sea menor o igual que 10.",
                "Muestra el valor actual.",
                "Incrementa el contador en uno.",
                "Confirma que se muestran exactamente diez números."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Las tres partes de un ciclo for",
                Descripcion =
                    "Un ciclo for reúne el valor inicial, la condición de repetición y el incremento.",
                ParaQueSirve =
                    "Permite controlar claramente desde qué número comienza el ciclo, hasta cuándo se repite y cómo cambia el contador.",
                Codigo =
                    "for (int contador = 1; contador <= 10; contador++) {" + Environment.NewLine +
                    "    // Instrucción que se repetirá" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta herramienta es opcional. Una solución correcta puede usar otra estructura repetitiva si produce " +
                    "el comportamiento solicitado. El fragmento solo muestra la estructura del ciclo y no contiene la salida completa."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "Esta práctica no necesita entrada.",
                SalidaEsperada = "1 2 3 4 5 6 7 8 9 10"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Comenzar en 0.",
                "Usar contador < 10 y omitir el 10.",
                "Usar contador <= 11.",
                "Olvidar incrementar el contador.",
                "Imprimir siempre el mismo valor.",
                "Escribir manualmente los diez números sin utilizar un ciclo.",
                "Crear un ciclo infinito."
            })
        };
    }

    private static GuiaPractica CrearGuiaTablaMultiplicar() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que solicite un número entero y muestre su tabla de multiplicar desde 1 hasta 10.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Número base",
                    Tipo = "int",
                    Descripcion = "Valor positivo, negativo o cero cuya tabla se calculará",
                    Ejemplo = "5"
                },
                new DatoGuiaPractica {
                    Nombre = "Factor",
                    Tipo = "int",
                    Descripcion = "Contador que recorre los valores del 1 al 10",
                    Ejemplo = "1"
                },
                new DatoGuiaPractica {
                    Nombre = "Resultado",
                    Tipo = "int",
                    Descripcion = "Producto del número base por el factor actual",
                    Ejemplo = "5"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Ciclo for",
                    Explicacion = "Repite el cálculo diez veces usando factores del 1 al 10.",
                    Fragmento = "for (int factor = 1; factor <= 10; factor++)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Número entero",
                    Explicacion = "La práctica acepta números positivos, negativos y cero sin decimales.",
                    Fragmento = "int numero;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Multiplicación repetitiva",
                    Explicacion = "En cada repetición se multiplica el mismo número por un factor diferente.",
                    Fragmento = "numero * factor"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Contador",
                    Explicacion = "El factor comienza en 1, aumenta de uno en uno y termina en 10.",
                    Fragmento = "factor++"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Resultado por iteración",
                    Explicacion = "Cada repetición obtiene un resultado nuevo sin alterar el número base.",
                    Fragmento = "int resultado = numero * factor;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Solicita un número entero.",
                "Crea un ciclo for cuyo factor comience en 1.",
                "Mantén el ciclo hasta incluir el factor 10.",
                "Multiplica el número por el factor actual.",
                "Muestra número, factor y resultado en cada repetición.",
                "Incrementa el factor en uno.",
                "Confirma que se generan exactamente diez operaciones."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Resultado local dentro del ciclo",
                Descripcion =
                    "Una variable local puede guardar el producto calculado en la repetición actual.",
                ParaQueSirve =
                    "El resultado se vuelve a calcular con cada factor y queda separado de la presentación de la operación.",
                Codigo =
                    "for (int factorEjemplo = 1; factorEjemplo <= 3; factorEjemplo++) {" + Environment.NewLine +
                    "    int resultadoEjemplo = numero * factorEjemplo;" + Environment.NewLine +
                    "    // Usar el resultado de esta repetición" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "La variable local es una sugerencia y no un requisito. El fragmento recorre solo tres factores de ejemplo " +
                    "y no contiene la salida completa de la tabla."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "5",
                SalidaEsperada =
                    "5 x 1 = 5" + Environment.NewLine +
                    "5 x 2 = 10" + Environment.NewLine +
                    "..." + Environment.NewLine +
                    "5 x 10 = 50" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Número 0 → diez resultados iguales a 0" + Environment.NewLine +
                    "Número -3 → tabla con productos negativos del -3 al -30"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Comenzar el factor en 0.",
                "Usar factor < 10 y omitir la última operación.",
                "Modificar el número base dentro del ciclo.",
                "Multiplicar siempre por el mismo factor.",
                "Calcular una sola operación fuera del ciclo.",
                "Mostrar únicamente el resultado sin la operación correspondiente.",
                "Rechazar cero o números negativos aunque son entradas válidas."
            })
        };
    }

    private static GuiaPractica CrearGuiaSumaAcumulada() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que solicite una cantidad de valores, lea exactamente esa cantidad y muestre su suma total.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Cantidad",
                    Tipo = "int",
                    Descripcion = "Número de valores que se leerán; debe estar entre 0 y 100",
                    Ejemplo = "3"
                },
                new DatoGuiaPractica {
                    Nombre = "Valor actual",
                    Tipo = "double",
                    Descripcion = "Número decimal leído durante la repetición actual",
                    Ejemplo = "5.5"
                },
                new DatoGuiaPractica {
                    Nombre = "Suma",
                    Tipo = "double",
                    Descripcion = "Acumulador que comienza en 0 y conserva el total parcial",
                    Ejemplo = "6.5"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Cantidad de repeticiones",
                    Explicacion = "La cantidad indica cuántos valores deben leerse, pero no forma parte de la suma.",
                    Fragmento = "int cantidad;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Ciclo for",
                    Explicacion = "Es apropiado porque la cantidad de repeticiones se conoce antes de iniciar.",
                    Fragmento = "for (int indice = 0; indice < cantidad; indice++)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Acumulador",
                    Explicacion = "Guarda el total parcial y debe comenzar en cero.",
                    Fragmento = "double suma = 0;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operador +=",
                    Explicacion = "Suma el valor nuevo al contenido anterior del acumulador.",
                    Fragmento = "suma += valor;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación",
                    Explicacion = "Una cantidad menor que 0 o mayor que 100 es inválida.",
                    Fragmento = "cantidad < 0 || cantidad > 100"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Cantidad cero",
                    Explicacion = "No se lee ningún valor y la suma inicial permanece en 0.",
                    Fragmento = "cantidad == 0"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Solicita la cantidad y guárdala como int.",
                "Valida que la cantidad esté entre 0 y 100.",
                "Si está fuera del rango, muestra Entrada inválida y no leas valores.",
                "Inicializa la suma en 0.",
                "Para una cantidad válida, repite la lectura exactamente esa cantidad de veces.",
                "Añade cada valor double al acumulador.",
                "No incluyas la cantidad dentro de la suma.",
                "Muestra la suma total; para cantidad 0 debe ser 0."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Patrón de acumulador",
                Descripcion =
                    "Un acumulador conserva el resultado parcial mientras llegan nuevos valores.",
                ParaQueSirve =
                    "suma = valor reemplaza el total anterior; suma += valor añade el nuevo dato sin perder lo acumulado.",
                Codigo =
                    "double sumaEjemplo = 0;" + Environment.NewLine +
                    "double valorEjemplo = 2.5;" + Environment.NewLine +
                    "sumaEjemplo += valorEjemplo;",
                AclaracionOpcional =
                    "Este patrón es opcional como forma de escribir la actualización, aunque el comportamiento debe acumular. " +
                    "El fragmento procesa un solo valor y no contiene el ciclo completo."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "3" + Environment.NewLine +
                    "5.5" + Environment.NewLine +
                    "2" + Environment.NewLine +
                    "-1",
                SalidaEsperada =
                    "Suma total: 6.5" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Cantidad 0 → Suma total: 0" + Environment.NewLine +
                    "Cantidad -1 o 101 → Entrada inválida" + Environment.NewLine +
                    "Cantidad 1 → La suma es exactamente el único valor leído"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "No inicializar la suma en 0.",
                "Usar suma = valor y perder el total anterior.",
                "Sumar también el valor de cantidad.",
                "Leer un valor menos o uno más.",
                "Leer valores cuando la cantidad es inválida.",
                "Tratar cantidad 0 como error.",
                "Usar int para los valores y perder decimales."
            })
        };
    }

    private static GuiaPractica CrearGuiaAdivinaNumero() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un juego que mantenga fijo el número secreto 7, lea intentos del 1 al 10 y termine únicamente al acertar.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Número secreto",
                    Tipo = "int",
                    Descripcion = "Valor fijo que debe permanecer igual a 7",
                    Ejemplo = "7"
                },
                new DatoGuiaPractica {
                    Nombre = "Intento",
                    Tipo = "int",
                    Descripcion = "Número escrito por el usuario; el rango válido es 1 a 10",
                    Ejemplo = "3"
                },
                new DatoGuiaPractica {
                    Nombre = "Contador de intentos",
                    Tipo = "int",
                    Descripcion = "Cantidad total de intentos leídos, incluidos inválidos y el ganador",
                    Ejemplo = "3"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Ciclo while",
                    Explicacion = "Repite la lectura mientras el intento sea diferente del número secreto.",
                    Fragmento = "while (intento != numeroSecreto)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Valor fijo",
                    Explicacion = "El número secreto es 7 y no cambia durante la ejecución.",
                    Fragmento = "const int numeroSecreto = 7;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación del intento",
                    Explicacion = "Los valores menores que 1 o mayores que 10 son inválidos, pero también cuentan.",
                    Fragmento = "intento < 1 || intento > 10"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Pistas",
                    Explicacion = "Un intento menor que 7 necesita la pista Mayor; uno mayor necesita la pista Menor.",
                    Fragmento = "intento < numeroSecreto"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Contador",
                    Explicacion = "Se incrementa una sola vez después de leer cada intento.",
                    Fragmento = "intentos++;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Define el número secreto fijo con valor 7.",
                "Inicializa el contador de intentos en 0.",
                "Lee un intento entero y aumenta el contador una sola vez.",
                "Si está fuera de 1 a 10, muestra Intento inválido y continúa con una nueva lectura.",
                "Si es menor que 7, muestra El número secreto es mayor.",
                "Si es mayor que 7, muestra El número secreto es menor.",
                "Termina únicamente cuando el intento sea 7.",
                "Cuenta también el intento ganador y muestra el total al finalizar."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Contador de intentos",
                Descripcion =
                    "Un contador registra cuántos valores se han leído durante el juego.",
                ParaQueSirve =
                    "Incrementarlo inmediatamente después de cada lectura cuenta intentos válidos, inválidos y el acierto exactamente una vez.",
                Codigo =
                    "int intentos = 0;" + Environment.NewLine +
                    "int intentoEjemplo;" + Environment.NewLine +
                    "cin >> intentoEjemplo;" + Environment.NewLine +
                    "intentos++;",
                AclaracionOpcional =
                    "La ubicación mostrada para el incremento es una sugerencia. No uses break como forma principal de terminar: " +
                    "la condición del ciclo debe reflejar cuándo se acertó. El fragmento no contiene el juego completo."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "3" + Environment.NewLine +
                    "9" + Environment.NewLine +
                    "7",
                SalidaEsperada =
                    "El número secreto es mayor." + Environment.NewLine +
                    "El número secreto es menor." + Environment.NewLine +
                    "¡Correcto!" + Environment.NewLine +
                    "Intentos: 3" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Primer intento 7 → Correcto e Intentos: 1" + Environment.NewLine +
                    "Intentos 0 u 11 → Intento inválido y aumentan el contador" + Environment.NewLine +
                    "Intento 1 → El número secreto es mayor" + Environment.NewLine +
                    "Intento 10 → El número secreto es menor"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Cambiar el número secreto durante la ejecución.",
                "Invertir las pistas Mayor y Menor.",
                "No contar el intento ganador.",
                "No contar los intentos fuera del rango.",
                "Incrementar el contador más de una vez por lectura.",
                "Terminar después de un intento inválido.",
                "Crear un ciclo infinito al no leer un intento nuevo."
            })
        };
    }

    private static GuiaPractica CrearGuiaMenuRepetitivo() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un menú que se muestre al menos una vez, procese opciones sencillas y se repita hasta seleccionar 4 Salir.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Opción",
                    Tipo = "int",
                    Descripcion = "Selección: 1 Saludo, 2 Mensaje motivador, 3 Número 10 o 4 Salir",
                    Ejemplo = "1"
                },
                new DatoGuiaPractica {
                    Nombre = "Estado de repetición",
                    Tipo = "bool o valor centinela",
                    Descripcion = "Indica si el menú debe volver a mostrarse",
                    Ejemplo = "true"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Ciclo do-while",
                    Explicacion = "Ejecuta primero el menú y comprueba después si debe repetirse.",
                    Fragmento = "do { ... } while (opcion != 4);"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Valor centinela",
                    Explicacion = "La opción 4 funciona como señal para terminar el ciclo.",
                    Fragmento = "opcion != 4"
                },
                new ConceptoGuiaPractica {
                    Nombre = "switch",
                    Explicacion = "Selecciona la acción asociada con cada opción numérica.",
                    Fragmento = "switch (opcion)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "break en switch",
                    Explicacion = "Termina un case, pero no finaliza por sí solo el ciclo do-while.",
                    Fragmento = "break;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "default",
                    Explicacion = "Muestra Opción inválida para cualquier número distinto de 1, 2, 3 o 4.",
                    Fragmento = "default:"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Muestra las cuatro opciones dentro de un ciclo do-while.",
                "Lee la opción como int.",
                "Usa switch para procesar 1 Saludo, 2 Mensaje motivador y 3 Número 10.",
                "Usa la opción 4 para mostrar la despedida.",
                "Usa default para mostrar Opción inválida.",
                "Añade break al final de cada case para no ejecutar otra acción.",
                "Repite mientras la opción sea diferente de 4.",
                "Después de elegir 4, no ejecutes ninguna otra acción."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Centinela o bandera de continuación",
                Descripcion =
                    "Un valor centinela o una variable bool puede representar si el menú debe seguir activo.",
                ParaQueSirve =
                    "Separa la decisión de repetir del break usado para cerrar cada case del switch.",
                Codigo =
                    "bool continuar = true;" + Environment.NewLine +
                    "switch (opcion) {" + Environment.NewLine +
                    "    case 4:" + Environment.NewLine +
                    "        continuar = false;" + Environment.NewLine +
                    "        break;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "La bandera bool es opcional; también puede usarse directamente opcion != 4 como condición. " +
                    "El fragmento solo distingue el estado del ciclo y el break del switch."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "1" + Environment.NewLine +
                    "3" + Environment.NewLine +
                    "4",
                SalidaEsperada =
                    "Hola." + Environment.NewLine +
                    "Número: 10" + Environment.NewLine +
                    "Hasta luego." + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Primera opción 4 → Mostrar la despedida y terminar" + Environment.NewLine +
                    "Otra opción numérica → Opción inválida y volver a mostrar el menú" + Environment.NewLine +
                    "Después de la opción 4 → No ejecutar otra acción"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar un ciclo que no muestre el menú al menos una vez.",
                "Invertir la condición de repetición.",
                "No actualizar la opción dentro del ciclo.",
                "Confundir break del switch con la terminación del do-while.",
                "Olvidar default para opciones inválidas.",
                "Ejecutar otra acción después de seleccionar 4.",
                "Crear un ciclo infinito."
            })
        };
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
                new[] { "Variables 01", "Condicionales 01" },
                CrearGuiaSaludoPersonalizado()),
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
                new[] { "Funciones 01", "Variables 02" },
                CrearGuiaSumarDosNumeros()),
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
                new[] { "Funciones 02", "Condicionales 01" },
                CrearGuiaNumeroPar()),
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
                new[] { "Funciones 01–03", "Ciclos 03" },
                CrearGuiaCalcularPromedio()),
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
                new[] { "Funciones 01–04", "Condicionales 05", "Ciclos 05" },
                CrearGuiaCalculadoraModular())
        });
    }

    private static GuiaPractica CrearGuiaNumeroPar() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que envíe un número entero a una función y reciba un valor booleano que indique si es par.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Número",
                    Tipo = "int",
                    Descripcion = "Almacena el entero positivo, negativo o cero que se comprobará",
                    Ejemplo = "-8"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Función",
                    Explicacion = "Agrupa una tarea que puede recibir datos, procesarlos y devolver una respuesta.",
                    Fragmento = "bool nombreFuncion(int numero)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Parámetro",
                    Explicacion = "Es la variable declarada por la función para recibir un dato.",
                    Fragmento = "int numero"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Argumento",
                    Explicacion = "Es el valor que se envía al llamar la función.",
                    Fragmento = "nombreFuncion(valor)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Parámetro por valor",
                    Explicacion = "La función recibe una copia del entero; modificar esa copia no cambia la variable original de main.",
                    Fragmento = "int numero"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Tipo de retorno",
                    Explicacion = "El tipo bool antes del nombre indica que la función devolverá verdadero o falso.",
                    Fragmento = "bool nombreFuncion(int numero)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "return",
                    Explicacion = "Entrega el resultado calculado al lugar desde donde se llamó la función.",
                    Fragmento = "return resultado;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "bool",
                    Explicacion = "Representa una respuesta lógica con los valores true o false.",
                    Fragmento = "bool resultado;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operador módulo",
                    Explicacion = "Obtiene el residuo de una división entera y permite comprobar divisibilidad.",
                    Fragmento = "valor % divisor"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Llamada de función",
                    Explicacion = "Ejecuta la función enviándole el número como argumento.",
                    Fragmento = "nombreFuncion(valor)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Uso del valor retornado",
                    Explicacion = "main puede guardar o consultar el bool devuelto para decidir si muestra Par o Impar.",
                    Fragmento = "bool resultado = nombreFuncion(valor);"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Lee un número entero en main.",
                "Envía ese número como argumento a una función.",
                "Recibe el número en un parámetro int por valor.",
                "Comprueba dentro de la función si el número es divisible entre 2.",
                "Retorna true o false.",
                "Guarda o utiliza el valor retornado en main.",
                "Muestra Par o Impar según el resultado."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Retornar una expresión booleana",
                Descripcion =
                    "Una comparación puede producir directamente un valor true o false.",
                ParaQueSirve =
                    "Permite que una función responda una pregunta lógica sin encargarse de mostrar mensajes.",
                Codigo =
                    "bool esPositivo(int numero) {" + Environment.NewLine +
                    "    return numero > 0;" + Environment.NewLine +
                    "}" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    int valor = 5;" + Environment.NewLine +
                    "    bool resultado = esPositivo(valor);" + Environment.NewLine + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta herramienta es opcional. El ejemplo enseña cómo una función recibe un parámetro y devuelve un bool, " +
                    "pero no resuelve la práctica de número par. También puedes guardar primero una respuesta lógica en una " +
                    "variable bool y después retornarla."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "-8",
                SalidaEsperada =
                    "Número: -8" + Environment.NewLine +
                    "Clasificación: Par" + Environment.NewLine + Environment.NewLine +
                    "COMPORTAMIENTOS" + Environment.NewLine +
                    "0 → Par" + Environment.NewLine +
                    "7 → Impar" + Environment.NewLine +
                    "-3 → Impar" + Environment.NewLine +
                    "-8 → Par"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar división en lugar del operador módulo.",
                "Comprobar únicamente si el residuo es igual a 1.",
                "Clasificar cero como impar.",
                "No aceptar enteros negativos.",
                "Imprimir dentro de la función en lugar de retornar bool.",
                "Declarar una función bool y olvidar return.",
                "No utilizar el valor retornado desde main.",
                "Devolver texto en lugar de true o false.",
                "Realizar toda la comprobación directamente en main sin delegarla a una función."
            })
        };
    }

    private static GuiaPractica CrearGuiaSaludoPersonalizado() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que lea un nombre completo y lo envíe a una función void para mostrar un saludo personalizado.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Nombre completo",
                    Tipo = "string",
                    Descripcion = "Guarda el nombre escrito por el usuario, incluidos los espacios interiores",
                    Ejemplo = "Ana López"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "getline",
                    Explicacion = "Lee una línea completa y conserva los espacios que forman parte del nombre.",
                    Fragmento = "getline(cin, nombreCompleto);"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Declaración de una función",
                    Explicacion = "Informa antes de main el tipo de retorno y los datos que recibirá la función.",
                    Fragmento = "void nombreFuncion(string texto);"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Definición de una función",
                    Explicacion = "Contiene las instrucciones que se ejecutarán cuando se llame la función.",
                    Fragmento = "void nombreFuncion(string texto) { ... }"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Función void",
                    Explicacion = "Realiza una acción, como mostrar un mensaje, sin devolver un valor.",
                    Fragmento = "void nombreFuncion(string texto)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Parámetro string por valor",
                    Explicacion = "La función recibe una copia del texto; no necesita modificar la variable original de main.",
                    Fragmento = "string texto"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Llamada de función",
                    Explicacion = "Ejecuta la función y envía el nombre completo como argumento.",
                    Fragmento = "nombreFuncion(nombreCompleto);"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación de entrada",
                    Explicacion = "Una línea vacía no contiene un nombre y debe producir el mensaje Nombre inválido.",
                    Fragmento = "nombreCompleto.empty()"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Declara una variable string en main.",
                "Lee el nombre completo con getline para conservar sus espacios.",
                "Comprueba si la línea está vacía.",
                "Si está vacía, muestra Nombre inválido y no llames la función de saludo.",
                "Si contiene un nombre, envíalo como argumento a una función void.",
                "Recibe el nombre en un parámetro string por valor.",
                "Muestra desde la función un saludo que incluya el nombre completo."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Declarar, definir y llamar una función void",
                Descripcion =
                    "Una función puede declararse antes de main, llamarse desde main y definirse después.",
                ParaQueSirve =
                    "Permite organizar el programa y usar getline para enviar una línea completa a una función sin mezclar todas las tareas.",
                Codigo =
                    "void mostrarAviso(string texto);" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    string mensaje;" + Environment.NewLine +
                    "    getline(cin, mensaje);" + Environment.NewLine +
                    "    mostrarAviso(mensaje);" + Environment.NewLine + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}" + Environment.NewLine + Environment.NewLine +
                    "void mostrarAviso(string texto) {" + Environment.NewLine +
                    "    cout << \"Aviso: \" << texto << '\\n';" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta organización es opcional y los nombres pueden cambiar. El fragmento muestra un aviso distinto, " +
                    "no valida el nombre y no contiene el saludo solicitado por la práctica."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "Ana López",
                SalidaEsperada =
                    "Hola, Ana López." + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Línea vacía → Nombre inválido" + Environment.NewLine +
                    "Ana María López → El saludo conserva el nombre completo" + Environment.NewLine +
                    "Luis → El saludo también acepta un nombre sin espacios"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar cin >> nombre y perder las palabras posteriores al primer espacio.",
                "Llamar la función cuando el nombre está vacío.",
                "No incluir el nombre recibido dentro del saludo.",
                "Declarar un tipo de retorno distinto de void.",
                "Recibir el string por referencia aunque no es necesario.",
                "Escribir un nombre fijo dentro de la función.",
                "Realizar el saludo directamente en main sin delegarlo a una función.",
                "Usar una declaración y una definición con parámetros diferentes."
            })
        };
    }

    private static GuiaPractica CrearGuiaSumarDosNumeros() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que envíe dos valores decimales a una función, reciba su suma y muestre el resultado desde main.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Primer valor",
                    Tipo = "double",
                    Descripcion = "Primer número que se enviará a la función",
                    Ejemplo = "5.5"
                },
                new DatoGuiaPractica {
                    Nombre = "Segundo valor",
                    Tipo = "double",
                    Descripcion = "Segundo número que se enviará a la función",
                    Ejemplo = "-2"
                },
                new DatoGuiaPractica {
                    Nombre = "Resultado",
                    Tipo = "double",
                    Descripcion = "Valor retornado por la función y utilizado en main",
                    Ejemplo = "3.5"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Función con retorno",
                    Explicacion = "Calcula un valor y lo devuelve al lugar desde donde fue llamada.",
                    Fragmento = "double nombreFuncion(double valor1, double valor2)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Dos parámetros",
                    Explicacion = "Cada parámetro recibe uno de los valores necesarios para realizar el cálculo.",
                    Fragmento = "double valor1, double valor2"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Parámetros por valor",
                    Explicacion = "La función trabaja con copias de ambos números y no modifica las variables de main.",
                    Fragmento = "double valor1, double valor2"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Argumentos",
                    Explicacion = "Son los dos valores enviados al llamar la función.",
                    Fragmento = "nombreFuncion(numero1, numero2)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Tipo double",
                    Explicacion = "Permite trabajar con positivos, negativos, cero y valores con decimales.",
                    Fragmento = "double numero;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "return",
                    Explicacion = "Entrega el resultado double para que main pueda utilizarlo.",
                    Fragmento = "return resultado;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Recepción del resultado",
                    Explicacion = "main puede guardar el valor retornado antes de mostrarlo.",
                    Fragmento = "double resultado = nombreFuncion(numero1, numero2);"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Lee dos valores double en main.",
                "Envía ambos valores como argumentos a una función.",
                "Recibe los valores en dos parámetros double por valor.",
                "Realiza la suma dentro de la función.",
                "Retorna el resultado como double.",
                "Recibe o guarda el valor retornado en main.",
                "Muestra el resultado retornado."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Parámetros por valor y recepción de return",
                Descripcion =
                    "Una función puede recibir copias de varios datos y devolver un nuevo valor calculado.",
                ParaQueSirve =
                    "Mantiene el cálculo dentro de la función y permite que main decida cómo utilizar o mostrar el resultado.",
                Codigo =
                    "double multiplicar(double valor1, double valor2) {" + Environment.NewLine +
                    "    return valor1 * valor2;" + Environment.NewLine +
                    "}" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    double resultado = multiplicar(2.5, 4);" + Environment.NewLine + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "La variable resultado es opcional y los nombres pueden cambiar. El ejemplo utiliza multiplicación para " +
                    "enseñar parámetros y retorno, por lo que no contiene la solución de la suma solicitada."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "5.5" + Environment.NewLine +
                    "-2",
                SalidaEsperada =
                    "Resultado: 3.5" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "0 y 0 → Resultado: 0" + Environment.NewLine +
                    "-4 y -6 → Resultado: -10" + Environment.NewLine +
                    "2.25 y 0.5 → Resultado: 2.75"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar int y perder la parte decimal.",
                "Declarar menos o más de dos parámetros.",
                "Modificar los argumentos mediante paso por referencia.",
                "Mostrar el cálculo dentro de la función en lugar de retornarlo.",
                "Declarar retorno double y olvidar return.",
                "Ignorar uno de los dos parámetros.",
                "No utilizar en main el valor retornado.",
                "Realizar toda la suma directamente en main."
            })
        };
    }

    private static GuiaPractica CrearGuiaCalcularPromedio() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Un programa que use funciones separadas para leer tres calificaciones, calcular su promedio y mostrar el resultado.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Primera calificación",
                    Tipo = "double",
                    Descripcion = "Primer valor de la escala válida de 0 a 10",
                    Ejemplo = "8"
                },
                new DatoGuiaPractica {
                    Nombre = "Segunda calificación",
                    Tipo = "double",
                    Descripcion = "Segundo valor de la escala válida de 0 a 10",
                    Ejemplo = "9.5"
                },
                new DatoGuiaPractica {
                    Nombre = "Tercera calificación",
                    Tipo = "double",
                    Descripcion = "Tercer valor de la escala válida de 0 a 10",
                    Ejemplo = "7"
                },
                new DatoGuiaPractica {
                    Nombre = "Promedio",
                    Tipo = "double",
                    Descripcion = "Resultado retornado por la función de cálculo",
                    Ejemplo = "8.17"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Separación de responsabilidades",
                    Explicacion = "Cada función realiza una sola tarea: leer, calcular o presentar.",
                    Fragmento = "leer → calcular → mostrar"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Función de captura",
                    Explicacion = "Lee una calificación double y devuelve ese valor a main.",
                    Fragmento = "double leerCalificacion()"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Función de cálculo",
                    Explicacion = "Recibe exactamente tres calificaciones por valor y retorna el promedio como double.",
                    Fragmento = "double calcularPromedio(double valor1, double valor2, double valor3)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Función de presentación",
                    Explicacion = "Recibe el promedio y lo muestra; puede ser void porque no necesita devolver otro dato.",
                    Fragmento = "void mostrarPromedio(double promedio)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Parámetros por valor",
                    Explicacion = "La función de cálculo recibe copias de las tres calificaciones y no modifica las originales.",
                    Fragmento = "double valor1, double valor2, double valor3"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación de rango",
                    Explicacion = "Cada calificación debe ser mayor o igual que 0 y menor o igual que 10.",
                    Fragmento = "calificacion < 0 || calificacion > 10"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Fórmula del promedio",
                    Explicacion = "Primero se suman las tres calificaciones y después se divide el total entre 3.0.",
                    Fragmento = "(calificacion1 + calificacion2 + calificacion3) / 3.0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Retorno double",
                    Explicacion = "Conserva la parte decimal del promedio calculado.",
                    Fragmento = "return promedio;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Crea una función que lea y retorne una calificación double.",
                "Llama esa función exactamente tres veces desde main.",
                "Valida que cada calificación esté entre 0 y 10.",
                "Si alguna está fuera del rango, muestra Calificación inválida y no calcules el promedio.",
                "Envía las tres calificaciones por valor a una función de cálculo.",
                "Calcula (calificacion1 + calificacion2 + calificacion3) / 3.0.",
                "Retorna el promedio como double.",
                "Envía el valor retornado a una función de presentación y muéstralo."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Una función para cada responsabilidad",
                Descripcion =
                    "Dividir captura, cálculo y presentación hace que cada parte del programa tenga una tarea clara.",
                ParaQueSirve =
                    "Permite revisar y reutilizar cada operación por separado sin mezclar entrada, fórmula y salida en una sola función.",
                Codigo =
                    "double leerDato();" + Environment.NewLine +
                    "double calcularResultado(double valor1, double valor2, double valor3);" + Environment.NewLine +
                    "void mostrarResultado(double resultado);" + Environment.NewLine + Environment.NewLine +
                    "double dato1 = leerDato();" + Environment.NewLine +
                    "double dato2 = leerDato();" + Environment.NewLine +
                    "double dato3 = leerDato();" + Environment.NewLine +
                    "double resultado = calcularResultado(dato1, dato2, dato3);" + Environment.NewLine +
                    "mostrarResultado(resultado);",
                AclaracionOpcional =
                    "Los nombres mostrados son solo ejemplos y esta organización es opcional mientras se respeten las tres " +
                    "responsabilidades. El fragmento no implementa la lectura, la validación, la fórmula ni la presentación."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "8" + Environment.NewLine +
                    "9.5" + Environment.NewLine +
                    "7",
                SalidaEsperada =
                    "Promedio: 8.17" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "0, 0 y 0 → Promedio: 0" + Environment.NewLine +
                    "10, 10 y 10 → Promedio: 10" + Environment.NewLine +
                    "-0.1 en cualquier posición → Calificación inválida" + Environment.NewLine +
                    "10.1 en cualquier posición → Calificación inválida"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Leer una cantidad diferente de tres calificaciones.",
                "Aceptar valores menores que 0 o mayores que 10.",
                "Calcular el promedio aunque exista una calificación inválida.",
                "Usar división entera en lugar de dividir entre 3.0.",
                "Olvidar los paréntesis alrededor de la suma.",
                "Usar paso por referencia para las calificaciones.",
                "Hacer captura, cálculo y presentación dentro de una sola función.",
                "No retornar el promedio desde la función de cálculo.",
                "No utilizar desde main el valor retornado."
            })
        };
    }

    private static GuiaPractica CrearGuiaCalculadoraModular() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Una calculadora de una sola ejecución que use una función independiente para cada operación.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Opción",
                    Tipo = "int",
                    Descripcion = "Selecciona 1 Suma, 2 Resta, 3 Multiplicación o 4 División",
                    Ejemplo = "4"
                },
                new DatoGuiaPractica {
                    Nombre = "Primer operando",
                    Tipo = "double",
                    Descripcion = "Primer valor enviado a la función seleccionada",
                    Ejemplo = "10"
                },
                new DatoGuiaPractica {
                    Nombre = "Segundo operando",
                    Tipo = "double",
                    Descripcion = "Segundo valor y divisor cuando se selecciona la división",
                    Ejemplo = "2"
                },
                new DatoGuiaPractica {
                    Nombre = "Resultado",
                    Tipo = "double",
                    Descripcion = "Valor retornado por la función correspondiente",
                    Ejemplo = "5"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "Prototipo",
                    Explicacion = "Declara antes de main qué recibe y qué retorna cada función.",
                    Fragmento = "double nombreOperacion(double valor1, double valor2);"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Una función por operación",
                    Explicacion = "Suma, resta, multiplicación y división se implementan por separado.",
                    Fragmento = "double operacion(double valor1, double valor2)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Parámetros por valor",
                    Explicacion = "Cada operación recibe copias de los dos operandos double.",
                    Fragmento = "double valor1, double valor2"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Retorno double",
                    Explicacion = "Cada función entrega su resultado para que main lo muestre.",
                    Fragmento = "return resultado;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "switch como coordinador",
                    Explicacion = "El switch decide qué función llamar, pero no realiza directamente las operaciones.",
                    Fragmento = "switch (opcion)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Validación de opción",
                    Explicacion = "Solo las opciones 1, 2, 3 y 4 permiten solicitar operandos.",
                    Fragmento = "opcion < 1 || opcion > 4"
                },
                new ConceptoGuiaPractica {
                    Nombre = "División entre cero",
                    Explicacion = "El segundo operando debe comprobarse antes de llamar la función de división.",
                    Fragmento = "segundoOperando == 0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Ejecución única",
                    Explicacion = "El menú se procesa una sola vez y no necesita ningún ciclo.",
                    Fragmento = ""
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Declara una función independiente para cada una de las cuatro operaciones.",
                "Muestra el menú y lee primero la opción como int.",
                "Si la opción no está entre 1 y 4, muestra Opción inválida y no solicites operandos.",
                "Si la opción es válida, lee dos valores double.",
                "Usa switch únicamente para decidir qué función llamar.",
                "Envía ambos operandos por valor y utiliza el double retornado.",
                "Antes de llamar la función de división, comprueba si el segundo operando es cero.",
                "Si el divisor es cero, muestra No se puede dividir entre cero.",
                "En otro caso, muestra Resultado y finaliza sin repetir el menú."
            }),
            AdvertenciaEvaluacion = "",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Prototipos y selección de funciones",
                Descripcion =
                    "Los prototipos separan las operaciones y el switch coordina cuál debe ejecutarse.",
                ParaQueSirve =
                    "Evita escribir los cálculos dentro del menú y mantiene una responsabilidad clara para cada función.",
                Codigo =
                    "double sumarValores(double valor1, double valor2);" + Environment.NewLine +
                    "double restarValores(double valor1, double valor2);" + Environment.NewLine +
                    "double multiplicarValores(double valor1, double valor2);" + Environment.NewLine +
                    "double dividirValores(double valor1, double valor2);" + Environment.NewLine + Environment.NewLine +
                    "switch (opcion) {" + Environment.NewLine +
                    "    case 1:" + Environment.NewLine +
                    "        resultado = sumarValores(valor1, valor2);" + Environment.NewLine +
                    "        break;" + Environment.NewLine +
                    "    // Los demás casos siguen el mismo patrón." + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Los nombres son ejemplos y pueden cambiar. El fragmento solo muestra prototipos y una llamada; no " +
                    "implementa las operaciones, la lectura, las validaciones ni la salida completa de la calculadora."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "4" + Environment.NewLine +
                    "10" + Environment.NewLine +
                    "2",
                SalidaEsperada =
                    "Resultado: 5" + Environment.NewLine + Environment.NewLine +
                    "CASOS LÍMITE" + Environment.NewLine +
                    "Opción 0 o 5 → Opción inválida y no se solicitan operandos" + Environment.NewLine +
                    "Opción 4 con segundo operando 0 → No se puede dividir entre cero" + Environment.NewLine +
                    "Operandos negativos o decimales → Se procesan normalmente" + Environment.NewLine +
                    "Después del resultado → El menú no se repite"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Pedir los operandos antes de validar la opción.",
                "Repetir el menú mediante un ciclo.",
                "Realizar las operaciones directamente dentro del switch.",
                "Usar una sola función para todas las operaciones.",
                "Declarar funciones void en lugar de retornar double.",
                "Recibir operandos por referencia.",
                "Usar int y perder decimales.",
                "Comprobar el divisor después de llamar la función de división.",
                "Olvidar break y ejecutar más de una opción.",
                "Solicitar operandos o calcular un resultado para una opción inválida."
            })
        };
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
        string[] requisitosPrevios,
        GuiaPractica? guia = null) {
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
            RequisitosPrevios = Array.AsReadOnly(requisitosPrevios),
            Guia = guia
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
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01" }),
                Guia = CrearGuiaTicketCompra()
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
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01", "Variables 02" }),
                Guia = CrearGuiaConversorTemperatura()
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
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01–03" }),
                Guia = CrearGuiaPromedioCalificaciones()
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
                RequisitosPrevios = Array.AsReadOnly(new[] { "Variables 01–04" }),
                Guia = CrearGuiaMiniRecibo()
            }
        });
    }

    private static GuiaPractica CrearGuiaTicketCompra() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Crear un ticket de consola que solicite un producto, su precio y la cantidad comprada, " +
                "calcule el importe y muestre todos los datos con etiquetas claras.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Producto",
                    Tipo = "string",
                    Descripcion = "Nombre del artículo comprado",
                    Ejemplo = "Cuaderno"
                },
                new DatoGuiaPractica {
                    Nombre = "Precio unitario",
                    Tipo = "double",
                    Descripcion = "Precio de una unidad, conservando decimales",
                    Ejemplo = "25.50"
                },
                new DatoGuiaPractica {
                    Nombre = "Cantidad",
                    Tipo = "int",
                    Descripcion = "Número entero de unidades compradas",
                    Ejemplo = "2"
                },
                new DatoGuiaPractica {
                    Nombre = "Subtotal",
                    Tipo = "double",
                    Descripcion = "Resultado de multiplicar precio por cantidad",
                    Ejemplo = "51.00"
                },
                new DatoGuiaPractica {
                    Nombre = "Total",
                    Tipo = "double",
                    Descripcion = "Importe final; coincide con el subtotal si no hay impuestos ni descuentos",
                    Ejemplo = "51.00"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "string",
                    Explicacion = "Guarda texto, como el nombre de un producto.",
                    Fragmento = "string producto;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Guarda precios e importes con parte decimal.",
                    Fragmento = "double precio;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "int",
                    Explicacion = "Guarda cantidades enteras de productos.",
                    Fragmento = "int cantidad;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Multiplicación",
                    Explicacion = "Permite obtener un importe a partir del precio de una unidad y la cantidad.",
                    Fragmento = "double importeEjemplo = 12.50 * 3;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Subtotal y total",
                    Explicacion = "Sin impuestos ni descuentos, el total puede ser igual al subtotal.",
                    Fragmento = "double totalEjemplo = importeEjemplo;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "cin y cout",
                    Explicacion = "cin lee los datos y cout presenta el ticket en la consola.",
                    Fragmento = "cin >> producto >> precio >> cantidad;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Declara variables para producto, precio, cantidad, subtotal y total.",
                "Lee producto, precio unitario y cantidad en ese orden.",
                "Calcula el subtotal multiplicando el precio por la cantidad.",
                "Asigna al total el importe final; sin cargos adicionales puede coincidir con el subtotal.",
                "Muestra producto, precio, cantidad, subtotal y total con etiquetas claras.",
                "Prueba un precio decimal y también una cantidad igual a cero.",
                "No escribas directamente los valores del ejemplo en el código."
            }),
            AdvertenciaEvaluacion =
                "EndForge envía exactamente tres líneas: producto, precio decimal y cantidad entera." +
                Environment.NewLine +
                "El programa debe conservar ese orden y calcular con los valores recibidos.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "<iomanip>, fixed y setprecision(2)",
                Descripcion =
                    "La biblioteca <iomanip> ofrece herramientas para controlar cómo se muestran los números.",
                ParaQueSirve =
                    "fixed y setprecision(2) permiten presentar importes con dos cifras decimales.",
                Codigo =
                    "#include <iostream>" + Environment.NewLine +
                    "#include <iomanip>" + Environment.NewLine +
                    "using namespace std;" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    double importeEjemplo = 19.5;" + Environment.NewLine +
                    "    cout << fixed << setprecision(2) << importeEjemplo << '\\n';" + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta herramienta es opcional: solo mejora la presentación de los importes. " +
                    "El fragmento no calcula ni resuelve el ticket completo."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "Cuaderno" + Environment.NewLine +
                    "25.50" + Environment.NewLine +
                    "2",
                SalidaEsperada =
                    "Producto: Cuaderno" + Environment.NewLine +
                    "Precio: 25.50" + Environment.NewLine +
                    "Cantidad: 2" + Environment.NewLine +
                    "Subtotal: 51.00" + Environment.NewLine +
                    "Total: 51.00"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar int para el precio y perder los decimales.",
                "Leer precio y cantidad en un orden diferente.",
                "Sumar precio y cantidad en lugar de multiplicarlos.",
                "Olvidar mostrar el subtotal o el total.",
                "Mostrar los números sin etiquetas claras.",
                "Suponer que una cantidad igual a cero debe reemplazarse por otro valor.",
                "Escribir directamente los resultados del ejemplo."
            })
        };
    }

    private static GuiaPractica CrearGuiaConversorTemperatura() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Crear un conversor de consola que reciba una temperatura en grados Celsius, " +
                "calcule su equivalente en Fahrenheit y muestre ambos valores.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Temperatura Celsius",
                    Tipo = "double",
                    Descripcion = "Valor original que puede contener decimales o ser negativo",
                    Ejemplo = "25.0"
                },
                new DatoGuiaPractica {
                    Nombre = "Temperatura Fahrenheit",
                    Tipo = "double",
                    Descripcion = "Resultado decimal de aplicar la fórmula de conversión",
                    Ejemplo = "77.0"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Conserva temperaturas con decimales y admite valores negativos.",
                    Fragmento = "double celsius;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Operaciones decimales",
                    Explicacion = "Usar 9.0 y 5.0 deja claro que la división debe conservar decimales.",
                    Fragmento = "double factorEjemplo = 9.0 / 5.0;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Fórmula de conversión",
                    Explicacion = "Primero se escala el valor Celsius y después se suma 32.",
                    Fragmento = "fahrenheit = (celsius * 9.0 / 5.0) + 32;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Paréntesis",
                    Explicacion = "Agrupan la parte principal del cálculo y hacen más fácil leer la fórmula.",
                    Fragmento = "(celsius * 9.0 / 5.0)"
                },
                new ConceptoGuiaPractica {
                    Nombre = "cin y cout",
                    Explicacion = "cin recibe la temperatura y cout muestra el valor original y el convertido.",
                    Fragmento = "cin >> celsius;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Declara variables double para Celsius y Fahrenheit.",
                "Lee la temperatura Celsius recibida por consola.",
                "Aplica la fórmula fahrenheit = (celsius * 9.0 / 5.0) + 32.",
                "Usa 9.0 y 5.0 para expresar una operación decimal.",
                "Muestra tanto el valor Celsius como el resultado Fahrenheit con etiquetas.",
                "Prueba con un valor positivo, con cero y con un valor negativo.",
                "No escribas respuestas específicas para los ejemplos."
            }),
            AdvertenciaEvaluacion =
                "EndForge envía una sola temperatura decimal en Celsius." + Environment.NewLine +
                "También puede utilizar cero o valores negativos, por lo que no debes limitar la entrada a números positivos.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Constantes con const double",
                Descripcion =
                    "Una constante guarda un valor que no debe cambiar durante la ejecución del programa.",
                ParaQueSirve =
                    "Permite dar nombres claros al factor y al ajuste usados en una fórmula.",
                Codigo =
                    "#include <iostream>" + Environment.NewLine +
                    "using namespace std;" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    const double factorConversion = 9.0 / 5.0;" + Environment.NewLine +
                    "    const double ajusteFahrenheit = 32.0;" + Environment.NewLine +
                    "    cout << factorConversion << ' ' << ajusteFahrenheit << '\\n';" + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Usar constantes es opcional, pero ayuda a explicar los números de la fórmula. " +
                    "El fragmento no recibe ni convierte una temperatura."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada = "25",
                SalidaEsperada =
                    "Celsius: 25" + Environment.NewLine +
                    "Fahrenheit: 77"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Guardar las temperaturas en variables int.",
                "Usar una fórmula diferente o sumar 32 en el lugar incorrecto.",
                "Escribir 9 / 5 sin dejar clara la intención decimal.",
                "Omitir el valor Celsius original en la salida.",
                "No contemplar temperaturas negativas.",
                "Mostrar valores sin las etiquetas Celsius y Fahrenheit.",
                "Escribir directamente 77 para el ejemplo de 25 grados."
            })
        };
    }

    private static GuiaPractica CrearGuiaPromedioCalificaciones() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Crear un programa de consola que solicite exactamente tres calificaciones, " +
                "calcule su promedio aritmético y muestre el resultado con una etiqueta clara.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Primera calificación",
                    Tipo = "double",
                    Descripcion = "Primer valor que participa en el promedio",
                    Ejemplo = "8.0"
                },
                new DatoGuiaPractica {
                    Nombre = "Segunda calificación",
                    Tipo = "double",
                    Descripcion = "Segundo valor que participa en el promedio",
                    Ejemplo = "9.0"
                },
                new DatoGuiaPractica {
                    Nombre = "Tercera calificación",
                    Tipo = "double",
                    Descripcion = "Tercer valor que participa en el promedio",
                    Ejemplo = "10.0"
                },
                new DatoGuiaPractica {
                    Nombre = "Promedio",
                    Tipo = "double",
                    Descripcion = "Suma de las tres calificaciones dividida entre 3.0",
                    Ejemplo = "9.0"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Conserva calificaciones y resultados que pueden tener decimales.",
                    Fragmento = "double calificacion1;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Suma",
                    Explicacion = "Reúne las tres calificaciones antes de calcular el promedio.",
                    Fragmento = "double sumaEjemplo = 8.0 + 9.0 + 10.0;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "División entre 3.0",
                    Explicacion = "Reparte la suma entre las tres calificaciones manteniendo decimales.",
                    Fragmento = "double promedioEjemplo = sumaEjemplo / 3.0;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Paréntesis",
                    Explicacion = "Garantizan que primero se sumen todas las calificaciones.",
                    Fragmento = "(calificacion1 + calificacion2 + calificacion3) / 3.0"
                },
                new ConceptoGuiaPractica {
                    Nombre = "cin y cout",
                    Explicacion = "cin lee los tres valores y cout muestra el promedio calculado.",
                    Fragmento = "cin >> calificacion1 >> calificacion2 >> calificacion3;"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Declara tres variables double para las calificaciones y otra para el promedio.",
                "Lee la primera, segunda y tercera calificación en ese orden.",
                "Suma los tres valores dentro de paréntesis.",
                "Divide la suma entre 3.0 para obtener el promedio.",
                "Muestra el resultado con la etiqueta Promedio.",
                "Prueba valores enteros, decimales y algún cero.",
                "Calcula siempre con la entrada recibida y no con los ejemplos."
            }),
            AdvertenciaEvaluacion =
                "EndForge envía exactamente tres calificaciones decimales, una por línea." + Environment.NewLine +
                "Debes usar las tres y dividir su suma entre 3.0.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "Constante para la cantidad de calificaciones",
                Descripcion =
                    "Una constante asigna un nombre claro a un valor que no cambia.",
                ParaQueSirve =
                    "const int cantidadCalificaciones = 3 evita dejar un número sin explicación dentro del código.",
                Codigo =
                    "#include <iostream>" + Environment.NewLine +
                    "using namespace std;" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    const int cantidadCalificaciones = 3;" + Environment.NewLine +
                    "    cout << \"Cantidad: \" << cantidadCalificaciones << '\\n';" + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "La constante es opcional: dividir directamente entre 3.0 también es válido. " +
                    "Este fragmento no lee calificaciones ni calcula el promedio."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "8" + Environment.NewLine +
                    "9" + Environment.NewLine +
                    "10",
                SalidaEsperada = "Promedio: 9"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Usar int y perder resultados decimales.",
                "Leer menos o más de tres calificaciones.",
                "Dividir solamente la última calificación por no usar paréntesis.",
                "Dividir entre un valor distinto de 3.0.",
                "Mostrar la suma en lugar del promedio.",
                "Omitir la etiqueta Promedio.",
                "Escribir directamente el resultado de los ejemplos."
            })
        };
    }

    private static GuiaPractica CrearGuiaMiniRecibo() {
        return new GuiaPractica {
            QueVasAConstruir =
                "Crear un recibo de consola para un cliente y exactamente dos productos, " +
                "calculando el subtotal de cada producto y el total general.",
            DatosNecesarios = Array.AsReadOnly(new[] {
                new DatoGuiaPractica {
                    Nombre = "Cliente",
                    Tipo = "string",
                    Descripcion = "Nombre de la persona que recibe el comprobante",
                    Ejemplo = "Ana"
                },
                new DatoGuiaPractica {
                    Nombre = "Producto 1 y producto 2",
                    Tipo = "string",
                    Descripcion = "Nombres de los dos artículos del recibo",
                    Ejemplo = "Pan y Leche"
                },
                new DatoGuiaPractica {
                    Nombre = "Precio 1 y precio 2",
                    Tipo = "double",
                    Descripcion = "Precios unitarios que pueden contener decimales",
                    Ejemplo = "12.50 y 18.00"
                },
                new DatoGuiaPractica {
                    Nombre = "Cantidad 1 y cantidad 2",
                    Tipo = "int",
                    Descripcion = "Número entero de unidades de cada producto",
                    Ejemplo = "2 y 1"
                },
                new DatoGuiaPractica {
                    Nombre = "Subtotal 1 y subtotal 2",
                    Tipo = "double",
                    Descripcion = "Precio por cantidad para cada producto",
                    Ejemplo = "25.00 y 18.00"
                },
                new DatoGuiaPractica {
                    Nombre = "Total general",
                    Tipo = "double",
                    Descripcion = "Suma de los dos subtotales",
                    Ejemplo = "43.00"
                }
            }),
            ExplicacionesConceptos = Array.AsReadOnly(new[] {
                new ConceptoGuiaPractica {
                    Nombre = "string",
                    Explicacion = "Guarda el nombre del cliente y los nombres de los productos.",
                    Fragmento = "string cliente;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "int",
                    Explicacion = "Guarda las cantidades enteras compradas.",
                    Fragmento = "int cantidadProducto1;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "double",
                    Explicacion = "Guarda precios, subtotales y el total con decimales.",
                    Fragmento = "double precioProducto1;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Subtotal",
                    Explicacion = "Cada producto necesita su propio cálculo de precio por cantidad.",
                    Fragmento = "double importeEjemplo = 4.25 * 3;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Total general",
                    Explicacion = "Se obtiene sumando los dos subtotales ya calculados.",
                    Fragmento = "double totalEjemplo = 12.75 + 51.00;"
                },
                new ConceptoGuiaPractica {
                    Nombre = "Texto después de números",
                    Explicacion = "Al combinar cin >> con getline puede quedar pendiente un salto de línea.",
                    Fragmento = "cin.ignore();"
                }
            }),
            PasosSugeridos = Array.AsReadOnly(new[] {
                "Declara variables separadas para el cliente y para los datos de los dos productos.",
                "Lee cliente, producto 1, precio 1, cantidad 1, producto 2, precio 2 y cantidad 2 en ese orden.",
                "Calcula el subtotal del primer producto multiplicando su precio por su cantidad.",
                "Calcula el subtotal del segundo producto de la misma forma.",
                "Suma ambos subtotales para obtener el total general.",
                "Muestra cliente, productos, precios, cantidades, subtotales y total con etiquetas claras.",
                "Prueba cantidades o precios iguales a cero y no reemplaces los valores recibidos."
            }),
            AdvertenciaEvaluacion =
                "EndForge envía exactamente siete líneas: cliente; producto 1; precio 1; cantidad 1; " +
                "producto 2; precio 2; cantidad 2." + Environment.NewLine +
                "Conserva ese orden y no sobrescribas los datos del primer producto al leer el segundo.",
            HerramientaUtil = new HerramientaGuiaPractica {
                Nombre = "cin.ignore() antes de getline",
                Descripcion =
                    "cin.ignore() puede descartar el salto de línea que queda después de leer un número con cin >>.",
                ParaQueSirve =
                    "Ayuda a que un getline posterior espere correctamente una nueva línea de texto.",
                Codigo =
                    "#include <iostream>" + Environment.NewLine +
                    "#include <string>" + Environment.NewLine +
                    "using namespace std;" + Environment.NewLine + Environment.NewLine +
                    "int main() {" + Environment.NewLine +
                    "    int cantidadEjemplo;" + Environment.NewLine +
                    "    string descripcionEjemplo;" + Environment.NewLine + Environment.NewLine +
                    "    cin >> cantidadEjemplo;" + Environment.NewLine +
                    "    cin.ignore();" + Environment.NewLine +
                    "    getline(cin, descripcionEjemplo);" + Environment.NewLine +
                    "    return 0;" + Environment.NewLine +
                    "}",
                AclaracionOpcional =
                    "Esta técnica es opcional y resulta útil solo cuando combinas cin >> con getline. " +
                    "El fragmento no crea ni calcula el mini recibo completo."
            },
            EjemploEjecucion = new EjemploEjecucionPractica {
                Entrada =
                    "Ana" + Environment.NewLine +
                    "Pan" + Environment.NewLine +
                    "12.50" + Environment.NewLine +
                    "2" + Environment.NewLine +
                    "Leche" + Environment.NewLine +
                    "18.00" + Environment.NewLine +
                    "1",
                SalidaEsperada =
                    "Cliente: Ana" + Environment.NewLine +
                    "Producto 1: Pan" + Environment.NewLine +
                    "Precio 1: 12.50" + Environment.NewLine +
                    "Cantidad 1: 2" + Environment.NewLine +
                    "Subtotal 1: 25.00" + Environment.NewLine +
                    "Producto 2: Leche" + Environment.NewLine +
                    "Precio 2: 18.00" + Environment.NewLine +
                    "Cantidad 2: 1" + Environment.NewLine +
                    "Subtotal 2: 18.00" + Environment.NewLine +
                    "Total: 43.00"
            },
            ErroresComunes = Array.AsReadOnly(new[] {
                "Leer los siete datos en un orden diferente.",
                "Reutilizar las mismas variables y perder los datos del primer producto.",
                "Usar int para precios, subtotales o total.",
                "Olvidar limpiar el salto de línea antes de un getline posterior.",
                "Calcular el total sin obtener antes ambos subtotales.",
                "Omitir precios, cantidades o etiquetas en la salida.",
                "Escribir directamente los productos o importes del ejemplo."
            })
        };
    }
}
