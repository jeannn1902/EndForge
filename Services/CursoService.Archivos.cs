using EndForge.Models;

namespace EndForge.Services;

public sealed partial class CursoService {
    public const string TemaArchivosGradoJuniorId = "grado2-archivos";

    private static IReadOnlyList<PracticaCurso> CrearPracticasArchivosGradoJunior() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "grado2-archivos-escribir-texto",
                TemaArchivosGradoJuniorId,
                1,
                "Escribir una línea en un archivo",
                "Escribir una línea en un archivo",
                "Crear o sobrescribir un archivo de texto con una línea capturada desde la consola.",
                "Crear un programa que lea una línea completa, la guarde exactamente en mensaje.txt y confirme brevemente que terminó la operación.",
                new[] { "archivos de texto", "apertura", "escritura", "sobrescritura", "cierre" },
                new[] {
                    "Leer una línea completa desde la consola.",
                    "Abrir mensaje.txt mediante una ruta relativa y en modo de sobrescritura.",
                    "Comprobar que el archivo se haya abierto.",
                    "Escribir exactamente el contenido capturado.",
                    "Cerrar el archivo y mostrar una confirmación breve."
                },
                "El archivo mensaje.txt existe y contiene exactamente la línea capturada; una entrada vacía produce un archivo vacío.",
                "Inicial",
                "35–45 min",
                new[] { "Grado 2 · Cadenas" },
                CrearGuiaEscribirTextoArchivo()),
            CrearPractica(
                "grado2-archivos-leer-texto",
                TemaArchivosGradoJuniorId,
                2,
                "Leer y mostrar contenido de archivo",
                "Leer y mostrar contenido de archivo",
                "Abrir mensaje.txt y presentar todo su contenido sin perder líneas ni espacios.",
                "Crear un programa que lea por completo un archivo de texto preparado previamente y reproduzca su contenido en la consola.",
                new[] { "archivos de texto", "lectura", "líneas", "fin de archivo", "cierre" },
                new[] {
                    "Abrir mensaje.txt mediante una ruta relativa.",
                    "Comprobar que la apertura haya sido satisfactoria.",
                    "Leer todas las líneas, incluida una posible línea vacía.",
                    "Conservar los saltos y espacios del contenido.",
                    "Cerrar el archivo y mostrar únicamente el texto leído."
                },
                "La consola reproduce el contenido completo de mensaje.txt, respetando líneas y espacios y sin agregar datos ajenos.",
                "Fácil",
                "35–45 min",
                new[] { "Archivos 01" },
                CrearGuiaLeerTextoArchivo()),
            CrearPractica(
                "grado2-archivos-contar-lineas-palabras",
                TemaArchivosGradoJuniorId,
                3,
                "Contar líneas y palabras de un archivo",
                "Contar líneas y palabras de un archivo",
                "Recorrer texto.txt para contar todas sus líneas y las palabras separadas por espacios.",
                "Crear un programa que lea el archivo completo, considere también las líneas vacías y muestre cantidades independientes de líneas y palabras.",
                new[] { "lectura de archivos", "getline", "contadores", "líneas vacías", "palabras" },
                new[] {
                    "Abrir texto.txt y comprobar el resultado.",
                    "Leer el archivo línea por línea.",
                    "Incrementar el contador por cada línea, incluso si está vacía.",
                    "Contar dentro de cada línea las secuencias separadas por espacios.",
                    "Mostrar ambos totales con etiquetas claras y cerrar el archivo."
                },
                "La salida muestra el número exacto de líneas y palabras; un archivo completamente vacío produce cero y cero.",
                "Intermedia",
                "45–60 min",
                new[] { "Archivos 01–02" },
                CrearGuiaContarLineasPalabrasArchivo()),
            CrearPractica(
                "grado2-archivos-guardar-estudiantes",
                TemaArchivosGradoJuniorId,
                4,
                "Guardar estudiantes en un archivo",
                "Guardar estudiantes en un archivo",
                "Persistir una colección pequeña de estudiantes como registros de texto ordenados.",
                "Crear un programa que lea de uno a seis estudiantes y sobrescriba estudiantes.txt con una línea ID|Nombre|Promedio por cada registro.",
                new[] { "registros de texto", "separadores", "escritura", "orden", "nombres completos" },
                new[] {
                    "Leer y validar la cantidad de estudiantes.",
                    "Capturar ID, nombre completo y promedio de cada estudiante.",
                    "Abrir estudiantes.txt para sobrescribir su contenido.",
                    "Escribir un registro completo por línea en el orden de captura.",
                    "Cerrar el archivo después de comprobar que todas las escrituras terminaron."
                },
                "estudiantes.txt contiene exactamente un registro por estudiante, con sus tres campos completos y en el orden capturado.",
                "Intermedia",
                "55–70 min",
                new[] { "Archivos 01–03" },
                CrearGuiaGuardarEstudiantesArchivo()),
            CrearPractica(
                "grado2-archivos-buscar-registro",
                TemaArchivosGradoJuniorId,
                5,
                "Buscar un estudiante guardado en archivo",
                "Buscar un estudiante guardado en archivo",
                "Localizar por ID un registro almacenado en estudiantes.txt y presentar sus datos.",
                "Crear un programa que lea registros ID|Nombre|Promedio, busque un ID recibido por consola e informe si existe sin inventar datos cuando no haya coincidencia.",
                new[] { "lectura de registros", "separadores", "búsqueda", "bool", "conversión de campos" },
                new[] {
                    "Leer el ID objetivo desde la consola.",
                    "Abrir estudiantes.txt y comprobar la apertura.",
                    "Leer y separar cada registro completo.",
                    "Comparar el ID del registro con el objetivo.",
                    "Mostrar encontrado y los datos solo cuando exista una coincidencia.",
                    "Cerrar el archivo al terminar la búsqueda."
                },
                "La salida indica sí o no; cuando existe el ID, muestra el registro correcto, y cuando no existe no presenta datos inventados.",
                "Intermedia",
                "50–65 min",
                new[] { "Archivos 01–04" },
                CrearGuiaBuscarRegistroArchivo()),
            CrearPractica(
                "grado2-archivos-resumen-numerico",
                TemaArchivosGradoJuniorId,
                6,
                "Resumen numérico de archivo",
                "Resumen numérico de archivo",
                "Leer todos los números de numeros.txt y obtener cantidad, suma, promedio, mayor y menor.",
                "Crear un programa que procese un archivo no vacío con un número por línea y muestre un resumen numérico completo con precisión decimal.",
                new[] { "lectura numérica", "acumulador", "promedio", "máximo", "mínimo" },
                new[] {
                    "Abrir numeros.txt y comprobar que pueda leerse.",
                    "Leer todos los valores, no solamente el primero.",
                    "Contar y acumular cada número.",
                    "Actualizar mayor y menor usando un valor real del archivo como referencia.",
                    "Calcular el promedio después de la lectura.",
                    "Cerrar el archivo y mostrar los cinco resultados."
                },
                "La salida contiene cantidad, suma, promedio, mayor y menor correctos, con tolerancia decimal de 0.01.",
                "Intermedia",
                "50–65 min",
                new[] { "Archivos 01–05" },
                CrearGuiaResumenNumericoArchivo())
        });
    }

    private static GuiaPractica CrearGuiaEscribirTextoArchivo() {
        return CrearGuiaArchivos(
            "Un programa que recibe una línea completa, crea o reemplaza mensaje.txt y conserva allí exactamente el texto capturado.",
            new[] {
                DatoArchivo("texto", "línea de texto", "Contenido que se guardará, incluidos espacios internos, números y signos", "Hola EndForge"),
                DatoArchivo("mensaje.txt", "archivo de texto", "Destino relativo que debe crearse o sobrescribirse", "mensaje.txt"),
                DatoArchivo("confirmación", "texto breve", "Aviso en consola después de completar la escritura", "Archivo guardado")
            },
            new[] {
                ConceptoArchivo(
                    "Apertura",
                    "Antes de escribir se solicita acceso a un archivo ubicado en el directorio de ejecución.",
                    "Una ruta relativa como mensaje.txt evita depender de carpetas personales."),
                ConceptoArchivo(
                    "Sobrescritura",
                    "El contenido anterior debe reemplazarse; agregar al final produciría datos ajenos al caso actual.",
                    "Abrir para salida normalmente inicia un contenido nuevo; el modo de anexado no corresponde aquí."),
                ConceptoArchivo(
                    "Escritura y cierre",
                    "La línea se envía al archivo y después se cierra el recurso para completar la operación.",
                    "archivo << texto;\narchivo.close();")
            },
            new[] {
                "Lee la entrada con una operación capaz de conservar espacios.",
                "Usa únicamente el nombre relativo mensaje.txt.",
                "Abre el archivo para reemplazar su contenido anterior.",
                "Comprueba la apertura antes de escribir.",
                "Guarda exactamente la línea, sin prefijos ni etiquetas dentro del archivo.",
                "Cierra el archivo y muestra una confirmación corta en consola."
            },
            HerramientaArchivo(
                "Comprobar la apertura",
                "Una operación de archivo puede fallar por nombre, ubicación o permisos.",
                "Comprobar el estado permite evitar una confirmación falsa y mostrar un error entendible.",
                "if (!archivo) {\n    // Informar que no fue posible abrir el archivo.\n}",
                "La comprobación es una técnica recomendada; no impone un tipo concreto de flujo ni una solución completa."),
            "Hola EndForge",
            "Archivo mensaje.txt:\nHola EndForge\n\nConsola:\nArchivo guardado.",
            new[] {
                "Usar una ruta absoluta del equipo.",
                "Abrir en modo de agregar y conservar contenido anterior.",
                "Guardar solo la primera palabra.",
                "Escribir la etiqueta Texto dentro de mensaje.txt.",
                "Confirmar el guardado aunque el archivo no se haya abierto."
            },
            "EndForge inspeccionará el archivo real mensaje.txt y comparará exactamente su contenido.");
    }

    private static GuiaPractica CrearGuiaLeerTextoArchivo() {
        return CrearGuiaArchivos(
            "Un programa que abre mensaje.txt, recorre todo su contenido y lo reproduce en consola conservando su estructura.",
            new[] {
                DatoArchivo("mensaje.txt", "archivo de texto", "Fuente preparada antes de ejecutar el programa", "Hola\\nEndForge C++"),
                DatoArchivo("contenido", "texto multilínea", "Todas las líneas y espacios leídos del archivo", "Hola\\nEndForge C++")
            },
            new[] {
                ConceptoArchivo(
                    "Lectura",
                    "Abrir un archivo para leer no debe alterar su contenido.",
                    "El archivo se consulta desde el comienzo hasta su final."),
                ConceptoArchivo(
                    "Línea completa",
                    "Leer por líneas conserva espacios internos y permite reconstruir saltos entre renglones.",
                    "getline(archivo, linea);"),
                ConceptoArchivo(
                    "Fin de archivo",
                    "La lectura continúa mientras sea posible obtener contenido; no debe detenerse tras la primera línea.",
                    "Una entrada vacía no es lo mismo que inventar un mensaje.")
            },
            new[] {
                "Abre mensaje.txt desde el directorio de ejecución.",
                "Comprueba que el archivo exista y pueda leerse.",
                "Recorre todo el contenido, no solo el primer renglón.",
                "Conserva los espacios internos y los saltos entre líneas.",
                "No agregues títulos dentro de la región que reproduce el contenido.",
                "Cierra el archivo después de completar la lectura."
            },
            HerramientaArchivo(
                "Leer una línea completa",
                "Una lectura por línea evita dividir el texto cuando contiene espacios.",
                "Permite procesar archivos de una o varias líneas con la misma idea.",
                "string linea;\ngetline(archivo, linea);",
                "El fragmento obtiene una sola línea; todavía hace falta decidir cómo recorrer el archivo completo."),
            "Archivo mensaje.txt:\nHola\nEndForge C++",
            "Hola\nEndForge C++",
            new[] {
                "Mostrar únicamente la primera línea.",
                "Separar el contenido por palabras.",
                "Eliminar espacios internos.",
                "Agregar mensajes dentro del texto reproducido.",
                "Intentar leer después de una apertura fallida."
            },
            "EndForge preparará mensaje.txt y comparará el texto completo mostrado, incluidas sus líneas y espacios.");
    }

    private static GuiaPractica CrearGuiaContarLineasPalabrasArchivo() {
        return CrearGuiaArchivos(
            "Un programa que analiza texto.txt completo y presenta dos cantidades: líneas y palabras.",
            new[] {
                DatoArchivo("texto.txt", "archivo de texto", "Documento que puede contener varias líneas, líneas vacías y espacios repetidos", "Hola\\nEndForge C++"),
                DatoArchivo("líneas", "int", "Número de renglones; una línea vacía también cuenta", "2"),
                DatoArchivo("palabras", "int", "Secuencias no vacías separadas por espacios", "3")
            },
            new[] {
                ConceptoArchivo(
                    "Conteo de líneas",
                    "Cada línea obtenida incrementa el contador aunque no contenga caracteres.",
                    "Un archivo completamente vacío produce cero líneas."),
                ConceptoArchivo(
                    "Palabra",
                    "Una palabra es una secuencia de caracteres separada por espacios; varios espacios no crean palabras adicionales.",
                    "“Hola   mundo” contiene dos palabras."),
                ConceptoArchivo(
                    "Dos niveles de lectura",
                    "Primero se obtiene una línea del archivo y después se analizan sus palabras.",
                    "Los contadores conservan sus valores entre líneas.")
            },
            new[] {
                "Abre texto.txt y comprueba la apertura.",
                "Inicia ambos contadores en cero.",
                "Lee todas las líneas hasta llegar al final.",
                "Incrementa líneas por cada renglón leído, incluso si está vacío.",
                "Cuenta solo las secuencias no vacías separadas por espacios.",
                "Cierra el archivo y muestra ambos resultados con etiquetas distintas."
            },
            HerramientaArchivo(
                "Analizar una línea por separado",
                "Separar la lectura del archivo y el análisis del renglón reduce confusiones entre líneas y palabras.",
                "Permite que una línea vacía sume una línea, pero ninguna palabra.",
                "string linea;\n// Leer una línea y después analizar solamente su contenido.",
                "La técnica no obliga a utilizar una clase concreta para separar palabras."),
            "Archivo texto.txt:\nHola\nEndForge C++",
            "Líneas: 2\nPalabras: 3",
            new[] {
                "Ignorar líneas vacías.",
                "Contar espacios como si fueran palabras.",
                "Detenerse después del primer renglón.",
                "Contar el archivo vacío como una línea.",
                "Mezclar o contradecir los dos resultados."
            },
            "EndForge preparará texto.txt y revisará de forma independiente las cantidades de líneas y palabras.");
    }

    private static GuiaPractica CrearGuiaGuardarEstudiantesArchivo() {
        return CrearGuiaArchivos(
            "Un programa que captura estudiantes y reemplaza estudiantes.txt con registros completos, uno por línea y en orden.",
            new[] {
                DatoArchivo("n", "int", "Cantidad de estudiantes; debe estar entre 1 y 6", "2"),
                DatoArchivo("ID", "int", "Identificador de cada estudiante", "10"),
                DatoArchivo("nombre", "línea de texto", "Nombre completo, incluidos sus espacios", "Ana López"),
                DatoArchivo("promedio", "double", "Promedio académico que puede contener decimales", "8.5"),
                DatoArchivo("estudiantes.txt", "archivo de registros", "Un registro ID|Nombre|Promedio por línea", "10|Ana López|8.5")
            },
            new[] {
                ConceptoArchivo(
                    "Registro delimitado",
                    "Un carácter separador permite distinguir los campos sin perder los espacios del nombre.",
                    "ID|Nombre|Promedio"),
                ConceptoArchivo(
                    "Un registro por línea",
                    "Cada estudiante ocupa un renglón independiente y conserva el orden de captura.",
                    "10|Ana López|8.5"),
                ConceptoArchivo(
                    "Reemplazar, no anexar",
                    "Cada ejecución debe crear el conjunto solicitado sin conservar registros de una ejecución anterior.",
                    "estudiantes.txt se sobrescribe al comenzar.")
            },
            new[] {
                "Lee n entre 1 y 6.",
                "Captura el ID, el nombre completo y el promedio de cada estudiante.",
                "Abre estudiantes.txt para sobrescribir y comprueba la apertura.",
                "Escribe los tres campos separados claramente.",
                "Termina cada registro con un salto de línea y conserva el orden.",
                "Cierra el archivo después del último estudiante."
            },
            HerramientaArchivo(
                "Construir una línea de registro",
                "El separador vertical hace explícito dónde termina cada campo.",
                "Facilita recuperar nombres con espacios sin confundirlos con el promedio.",
                "archivo << id << '|' << nombre << '|' << promedio << '\\n';",
                "Es solo el formato de una línea; no contiene la captura, el ciclo ni el programa completo."),
            "2\n10\nAna López\n8.5\n20\nLuis Pérez\n7",
            "Archivo estudiantes.txt:\n10|Ana López|8.5\n20|Luis Pérez|7",
            new[] {
                "Abrir en modo de anexado y conservar estudiantes anteriores.",
                "Guardar el nombre incompleto.",
                "Omitir un campo o un registro.",
                "Cambiar el orden de captura.",
                "Escribir líneas adicionales o mezclar los campos."
            },
            "EndForge inspeccionará estudiantes.txt y comprobará cantidad, orden y campos de cada registro con tolerancia 0.01 para promedios.");
    }

    private static GuiaPractica CrearGuiaBuscarRegistroArchivo() {
        return CrearGuiaArchivos(
            "Un programa que consulta estudiantes.txt, busca un ID y comunica el registro correspondiente solo cuando existe.",
            new[] {
                DatoArchivo("estudiantes.txt", "archivo de registros", "Fuente con líneas ID|Nombre|Promedio", "10|Ana López|8.5"),
                DatoArchivo("ID objetivo", "int", "Identificador solicitado desde consola", "20"),
                DatoArchivo("encontrado", "bool o texto equivalente", "Indica si hubo una coincidencia", "sí"),
                DatoArchivo("registro", "ID, nombre y promedio", "Datos mostrados únicamente si el ID existe", "20, Luis Pérez, 7")
            },
            new[] {
                ConceptoArchivo(
                    "Separación de campos",
                    "Cada línea debe dividirse respetando el delimitador vertical y los espacios del nombre.",
                    "ID | Nombre | Promedio"),
                ConceptoArchivo(
                    "Búsqueda por clave",
                    "El ID identifica el registro; otros valores iguales no deben decidir la coincidencia.",
                    "Se compara el ID leído con el ID objetivo."),
                ConceptoArchivo(
                    "Resultado ausente",
                    "Si no hay coincidencia se informa no encontrado sin fabricar nombre, promedio ni ID.",
                    "Encontrado: no")
            },
            new[] {
                "Lee el ID objetivo.",
                "Abre estudiantes.txt y comprueba que pueda leerse.",
                "Obtén cada línea completa del archivo.",
                "Separa y convierte sus tres campos de forma controlada.",
                "Compara el ID y conserva el registro solo cuando coincida.",
                "Cierra el archivo y muestra un resultado coherente."
            },
            HerramientaArchivo(
                "Extraer hasta un delimitador",
                "Una lectura delimitada permite obtener un campo sin dividir los espacios internos del nombre.",
                "Resulta útil para procesar el formato ID|Nombre|Promedio.",
                "getline(flujoDeLinea, campo, '|');",
                "El fragmento extrae un campo; validar y convertir todos los campos sigue siendo parte de la práctica."),
            "Archivo estudiantes.txt:\n10|Ana López|8.5\n20|Luis Pérez|7\n\nID objetivo:\n20",
            "Encontrado: sí\nID: 20\nNombre: Luis Pérez\nPromedio: 7",
            new[] {
                "Buscar por nombre en lugar de por ID.",
                "Leer únicamente el primer registro.",
                "Cortar un nombre compuesto al encontrar un espacio.",
                "Mostrar datos de otro estudiante.",
                "Inventar un registro cuando el ID no existe."
            },
            "EndForge preparará estudiantes.txt y revisará el estado encontrado y, cuando corresponda, todos los campos del registro correcto.");
    }

    private static GuiaPractica CrearGuiaResumenNumericoArchivo() {
        return CrearGuiaArchivos(
            "Un programa que procesa todos los números de numeros.txt y construye un resumen estadístico básico.",
            new[] {
                DatoArchivo("numeros.txt", "archivo numérico", "Contiene al menos un número por línea", "1\\n2\\n3"),
                DatoArchivo("cantidad", "int", "Número de valores leídos correctamente", "3"),
                DatoArchivo("suma y promedio", "double", "Total acumulado y media aritmética", "6 y 2"),
                DatoArchivo("mayor y menor", "double", "Extremos reales del archivo", "3 y 1")
            },
            new[] {
                ConceptoArchivo(
                    "Lectura numérica completa",
                    "Cada valor válido debe procesarse hasta alcanzar el final del archivo.",
                    "Un número por línea también puede leerse como una secuencia numérica."),
                ConceptoArchivo(
                    "Resumen acumulado",
                    "Cantidad y suma se actualizan por cada número; el promedio se calcula al final.",
                    "promedio = suma / cantidad;"),
                ConceptoArchivo(
                    "Extremos desde datos reales",
                    "El primer número es una referencia válida para mayor y menor, incluso con valores negativos.",
                    "double mayor = primerValor;\ndouble menor = primerValor;")
            },
            new[] {
                "Abre numeros.txt y comprueba que pueda leerse.",
                "Obtén el primer valor para iniciar cantidad, suma, mayor y menor.",
                "Continúa leyendo hasta el final.",
                "Actualiza los acumuladores y extremos una vez por valor.",
                "Calcula el promedio mediante división decimal.",
                "Cierra el archivo y presenta los cinco resultados etiquetados."
            },
            HerramientaArchivo(
                "Inicializar desde el primer dato",
                "Usar un valor real evita suponer que cero es mayor o menor que todos los datos.",
                "Funciona con archivos totalmente negativos y también con un solo número.",
                "double mayor = primerValor;\ndouble menor = primerValor;",
                "El fragmento solo establece referencias iniciales; la lectura y las comparaciones todavía deben resolverse."),
            "Archivo numeros.txt:\n1\n2\n3",
            "Cantidad: 3\nSuma: 6\nPromedio: 2\nMayor: 3\nMenor: 1",
            new[] {
                "Leer únicamente el primer número.",
                "Usar división entera para el promedio.",
                "Iniciar mayor o menor en cero.",
                "Contar una línea que no fue leída.",
                "Mostrar resultados contradictorios."
            },
            "EndForge preparará numeros.txt y comprobará cantidad, suma, promedio y extremos con tolerancia 0.01.");
    }

    private static GuiaPractica CrearGuiaArchivos(
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

    private static DatoGuiaPractica DatoArchivo(
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

    private static ConceptoGuiaPractica ConceptoArchivo(
        string nombre,
        string explicacion,
        string fragmento) {
        return new ConceptoGuiaPractica {
            Nombre = nombre,
            Explicacion = explicacion,
            Fragmento = fragmento
        };
    }

    private static HerramientaGuiaPractica HerramientaArchivo(
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
