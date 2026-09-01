using EndForge.Models;

namespace EndForge.Services;

public sealed partial class CursoService {
    public const string TemaEstructurasGradoJuniorId = "grado2-estructuras";

    private static IReadOnlyList<PracticaCurso>
        CrearPracticasEstructurasGradoJunior() {
        return Array.AsReadOnly(new[] {
            CrearPractica(
                "grado2-estructuras-datos-estudiante",
                TemaEstructurasGradoJuniorId,
                1,
                "Capturar datos de estudiante",
                "Capturar datos de estudiante",
                "Agrupar y presentar los datos relacionados con un estudiante.",
                "Crear un programa que lea ID, nombre completo, edad y promedio, y muestre los cuatro campos sin perder información.",
                new[] { "registro", "campos", "tipos de datos", "getline" },
                new[] {
                    "Definir los datos que forman un estudiante.",
                    "Leer cada campo con el tipo apropiado.",
                    "Conservar los espacios del nombre.",
                    "Mostrar un bloque con los cuatro campos."
                },
                "La salida contiene un bloque consistente con ID, nombre completo, edad y promedio.",
                "Inicial",
                "30–40 min",
                Array.Empty<string>(),
                CrearGuiaDatosEstudianteEstructuras()),
            CrearPractica(
                "grado2-estructuras-promedio-estudiante",
                TemaEstructurasGradoJuniorId,
                2,
                "Calcular el promedio de un estudiante",
                "Calcular el promedio de un estudiante",
                "Combinar datos de identidad, calificaciones y un resultado calculado.",
                "Crear un programa que lea un estudiante y tres calificaciones, calcule su promedio y determine si está aprobado.",
                new[] { "registro", "promedio", "bool", "campos calculados" },
                new[] {
                    "Leer ID, nombre completo y tres calificaciones.",
                    "Calcular la media aritmética.",
                    "Clasificar como aprobado un promedio mayor o igual que seis.",
                    "Mostrar ID, nombre, promedio y resultado lógico."
                },
                "La salida presenta el registro y una clasificación de aprobación coherente con el promedio.",
                "Fácil",
                "35–45 min",
                new[] { "Estructuras 01" },
                CrearGuiaPromedioEstudianteEstructuras()),
            CrearPractica(
                "grado2-estructuras-arreglo-estudiantes",
                TemaEstructurasGradoJuniorId,
                3,
                "Capturar y mostrar varios estudiantes",
                "Capturar y mostrar varios estudiantes",
                "Almacenar una colección de registros y conservar su orden.",
                "Crear un programa que lea de uno a seis estudiantes con ID único, nombre completo y promedio, y muestre exactamente los registros capturados.",
                new[] { "colección de registros", "índices", "orden", "ID único" },
                new[] {
                    "Leer y validar la cantidad de estudiantes.",
                    "Capturar los tres campos de cada registro.",
                    "Conservar el orden de entrada.",
                    "Mostrar cada estudiante como un bloque independiente."
                },
                "La salida contiene exactamente la cantidad solicitada de registros, sin mezclar sus campos.",
                "Fácil",
                "40–50 min",
                new[] { "Estructuras 01–02" },
                CrearGuiaArregloEstudiantesEstructuras()),
            CrearPractica(
                "grado2-estructuras-buscar-estudiante",
                TemaEstructurasGradoJuniorId,
                4,
                "Buscar un estudiante por ID",
                "Buscar un estudiante por ID",
                "Localizar un registro mediante su identificador sin inventar resultados.",
                "Crear un programa que busque un ID dentro de varios estudiantes e informe si existe; cuando exista, debe mostrar el registro correspondiente.",
                new[] { "búsqueda", "clave", "bool", "registro opcional" },
                new[] {
                    "Leer los estudiantes y el ID objetivo.",
                    "Comparar el objetivo con cada ID.",
                    "Conservar el registro que coincide.",
                    "Mostrar una respuesta lógica.",
                    "Mostrar datos únicamente cuando se encuentre."
                },
                "La salida indica encontrado sí o no y solo presenta un registro cuando corresponde.",
                "Intermedia",
                "40–55 min",
                new[] { "Estructuras 01–03" },
                CrearGuiaBuscarEstudianteEstructuras()),
            CrearPractica(
                "grado2-estructuras-mejor-promedio",
                TemaEstructurasGradoJuniorId,
                5,
                "Encontrar estudiante con mejor promedio",
                "Encontrar estudiante con mejor promedio",
                "Comparar registros completos mediante uno de sus campos.",
                "Crear un programa que encuentre al estudiante con el promedio más alto y conserve la primera aparición cuando exista un empate.",
                new[] { "máximo", "registro candidato", "empate", "primera aparición" },
                new[] {
                    "Leer todos los estudiantes.",
                    "Tomar el primer registro como candidato inicial.",
                    "Comparar los promedios restantes.",
                    "Reemplazar el candidato solo ante un promedio estrictamente mayor.",
                    "Mostrar el registro ganador."
                },
                "La salida contiene ID, nombre y promedio del primer estudiante que alcanza el valor máximo.",
                "Intermedia",
                "40–55 min",
                new[] { "Estructuras 01–04" },
                CrearGuiaMejorPromedioEstructuras()),
            CrearPractica(
                "grado2-estructuras-ordenar-estudiantes",
                TemaEstructurasGradoJuniorId,
                6,
                "Ordenar estudiantes por promedio",
                "Ordenar estudiantes por promedio",
                "Reordenar registros completos sin separar sus campos.",
                "Crear un programa que muestre todos los estudiantes por promedio descendente y conserve el orden de captura en los empates.",
                new[] { "ordenamiento estable", "registros", "orden descendente", "empates" },
                new[] {
                    "Leer los registros en su orden original.",
                    "Comparar los promedios para ordenar de mayor a menor.",
                    "Mover siempre el registro completo.",
                    "Conservar el orden original entre promedios iguales.",
                    "Mostrar todos los estudiantes ordenados."
                },
                "La salida incluye todos los registros en orden descendente y mantiene estables los empates.",
                "Intermedia",
                "50–65 min",
                new[] { "Estructuras 01–05" },
                CrearGuiaOrdenarEstudiantesEstructuras()),
            CrearPractica(
                "grado2-estructuras-inventario-productos",
                TemaEstructurasGradoJuniorId,
                7,
                "Gestionar un inventario de productos",
                "Gestionar un inventario de productos",
                "Relacionar datos de productos con cálculos individuales y un total general.",
                "Crear un programa que lea productos, calcule precio por cantidad para cada uno y acumule el valor total del inventario.",
                new[] { "productos", "campos calculados", "acumulador", "inventario" },
                new[] {
                    "Leer código, nombre, precio y cantidad de cada producto.",
                    "Calcular el valor de cada producto.",
                    "Acumular el valor general.",
                    "Mostrar cada registro con su total.",
                    "Mostrar el total del inventario por separado."
                },
                "La salida conserva todos los productos, sus datos, sus subtotales y el total general.",
                "Intermedia",
                "50–65 min",
                new[] { "Estructuras 01–06" },
                CrearGuiaInventarioProductosEstructuras()),
            CrearPractica(
                "grado2-estructuras-registro-empleados",
                TemaEstructurasGradoJuniorId,
                8,
                "Clasificar empleados por salario",
                "Clasificar empleados por salario",
                "Analizar registros según un estado lógico y calcular un resumen filtrado.",
                "Crear un programa que muestre empleados y calcule la cantidad y el salario promedio de quienes están activos.",
                new[] { "empleados", "bool", "filtro", "promedio condicionado" },
                new[] {
                    "Leer ID, nombre, salario y estado de cada empleado.",
                    "Mostrar todos los registros.",
                    "Contar únicamente los empleados activos.",
                    "Acumular únicamente sus salarios.",
                    "Calcular el promedio activo o cero si no hay activos."
                },
                "La salida contiene todos los empleados y un resumen calculado solo con los registros activos.",
                "Intermedia",
                "50–65 min",
                new[] { "Estructuras 01–07" },
                CrearGuiaRegistroEmpleadosEstructuras())
        });
    }

    private static GuiaPractica CrearGuiaDatosEstudianteEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que reúne en un solo registro la identidad y el desempeño de un estudiante, y después presenta sus campos como un bloque coherente.",
            new[] {
                DatoEstructura("ID", "int", "Identificador entero del estudiante", "10"),
                DatoEstructura("nombre", "string", "Nombre completo, incluidos espacios", "Ana López"),
                DatoEstructura("edad", "int", "Edad del estudiante", "17"),
                DatoEstructura("promedio", "double", "Promedio con posibles decimales", "8.5")
            },
            new[] {
                ConceptoEstructura(
                    "Registro",
                    "Agrupa datos que describen a una misma entidad aunque tengan tipos diferentes.",
                    "struct Estudiante { /* campos relacionados */ };"),
                ConceptoEstructura(
                    "Campo",
                    "Cada dato conserva un nombre y un tipo dentro del registro.",
                    "estudiante.edad = 17;"),
                ConceptoEstructura(
                    "Coherencia",
                    "Los campos mostrados deben pertenecer al mismo estudiante.",
                    "ID, nombre, edad y promedio forman un solo bloque")
            },
            new[] {
                "Define qué datos representan a un estudiante.",
                "Lee primero el ID y después el nombre completo.",
                "Captura edad y promedio con sus tipos apropiados.",
                "Conserva los espacios del nombre.",
                "Muestra los cuatro campos con etiquetas claras.",
                "Prueba nombres de una y de varias palabras."
            },
            HerramientaEstructura(
                "Acceso mediante punto",
                "El operador punto permite leer o modificar un campo de un registro.",
                "Mantiene explícito qué dato de la entidad se está utilizando.",
                "Estudiante alumno;\nalumno.edad = 17;",
                "El nombre del tipo y de la variable son libres; el fragmento no resuelve la captura completa."),
            "10\nAna López\n17\n8.5",
            "ID: 10\nNombre: Ana López\nEdad: 17\nPromedio: 8.5",
            new[] {
                "Leer únicamente la primera palabra del nombre.",
                "Mostrar un campo con el valor de otro.",
                "Usar un tipo entero para el promedio.",
                "Omitir una etiqueta.",
                "Mezclar datos de dos registros distintos."
            },
            "EndForge revisará un bloque con los cuatro campos obligatorios, nombre exacto y promedio con tolerancia de 0.01.");
    }

    private static GuiaPractica CrearGuiaPromedioEstudianteEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que combina la identidad de un estudiante con tres calificaciones y obtiene un resumen de promedio y aprobación.",
            new[] {
                DatoEstructura("ID", "int", "Identificador del estudiante", "1"),
                DatoEstructura("nombre", "string", "Nombre completo", "Ana López"),
                DatoEstructura("calificaciones", "double", "Tres valores usados en el promedio", "8, 9, 7"),
                DatoEstructura("promedio", "double", "Suma de las tres calificaciones dividida entre 3.0", "8"),
                DatoEstructura("aprobado", "bool", "Verdadero si el promedio es al menos 6", "true")
            },
            new[] {
                ConceptoEstructura(
                    "Campo calculado",
                    "Un registro puede presentar información obtenida a partir de sus datos de entrada.",
                    "double promedio = suma / 3.0;"),
                ConceptoEstructura(
                    "Condición de aprobación",
                    "El límite seis está incluido dentro de los resultados aprobatorios.",
                    "bool aprobado = promedio >= 6.0;"),
                ConceptoEstructura(
                    "Responsabilidad",
                    "El cálculo y la presentación pueden organizarse por separado sin perder la relación del registro.",
                    "primero calcular, después mostrar")
            },
            new[] {
                "Lee ID y nombre completo.",
                "Captura exactamente tres calificaciones decimales.",
                "Suma las calificaciones y divide entre 3.0.",
                "Compara el promedio con el límite inclusivo de seis.",
                "Muestra ID, nombre, promedio y una sola respuesta de aprobación.",
                "Prueba el límite exacto y un promedio con varios decimales."
            },
            HerramientaEstructura(
                "Resultado lógico derivado",
                "Una comparación puede guardarse como un valor booleano relacionado con el registro.",
                "Evita repetir la misma condición al presentar el resultado.",
                "bool cumpleLimite = resultado >= limite;",
                "La técnica es opcional y usa nombres distintos a los de la práctica."),
            "1\nAna López\n8\n9\n7",
            "ID: 1\nNombre: Ana López\nPromedio: 8\nAprobado: Sí",
            new[] {
                "Promediar solo dos calificaciones.",
                "Usar división entera.",
                "Exigir un promedio mayor que seis y rechazar el límite.",
                "Mostrar una aprobación que contradice el promedio.",
                "Truncar el nombre completo."
            },
            "EndForge comprobará promedio, aprobación, ID y nombre dentro del mismo bloque.");
    }

    private static GuiaPractica CrearGuiaArregloEstudiantesEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que almacena varios estudiantes como registros completos y los presenta en el mismo orden de captura.",
            new[] {
                DatoEstructura("n", "int", "Cantidad de estudiantes entre 1 y 6", "3"),
                DatoEstructura("ID", "int", "Clave única de cada estudiante", "5"),
                DatoEstructura("nombre", "string", "Nombre completo del registro actual", "María José"),
                DatoEstructura("promedio", "double", "Promedio del registro actual", "9.25")
            },
            new[] {
                ConceptoEstructura(
                    "Colección de registros",
                    "Cada posición conserva juntos todos los campos de un estudiante.",
                    "Estudiante grupo[6];"),
                ConceptoEstructura(
                    "Clave única",
                    "El ID distingue un registro de los demás.",
                    "cada estudiante usa un ID diferente"),
                ConceptoEstructura(
                    "Orden de captura",
                    "Recorrer desde la primera posición conserva la secuencia original.",
                    "grupo[0], grupo[1], grupo[2]")
            },
            new[] {
                "Lee una cantidad entre uno y seis.",
                "Captura ID, nombre completo y promedio para cada posición.",
                "Conserva los campos dentro del mismo registro.",
                "Evita aceptar IDs repetidos.",
                "Recorre la colección en el orden original.",
                "Muestra cada estudiante como un bloque separado."
            },
            HerramientaEstructura(
                "Variable para el registro actual",
                "Preparar un registro completo antes de almacenarlo reduce el riesgo de mezclar campos.",
                "Permite verificar que ID, nombre y promedio pertenecen a la misma persona.",
                "Estudiante actual;\n// Capturar sus campos antes de guardarlo en la colección.",
                "Es una estrategia de organización opcional."),
            "2\n10\nAna López\n8.5\n20\nLuis Pérez\n7",
            "ID: 10\nNombre: Ana López\nPromedio: 8.5\nID: 20\nNombre: Luis Pérez\nPromedio: 7",
            new[] {
                "Sobrescribir siempre la misma posición.",
                "Alterar el orden de los registros.",
                "Separar nombres y promedios en recorridos que pierdan su correspondencia.",
                "Aceptar un ID duplicado.",
                "Mostrar más o menos registros que n."
            },
            "EndForge revisará exactamente n bloques, claves únicas y campos asociados al ID correcto.");
    }

    private static GuiaPractica CrearGuiaBuscarEstudianteEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que recorre una colección de estudiantes y recupera el registro cuya clave coincide con un ID solicitado.",
            new[] {
                DatoEstructura("estudiantes", "colección", "Registros con ID, nombre y promedio", "10, Ana López, 8.5"),
                DatoEstructura("ID objetivo", "int", "Clave que se desea localizar", "20"),
                DatoEstructura("encontrado", "bool", "Indica si existe un registro con esa clave", "true")
            },
            new[] {
                ConceptoEstructura(
                    "Búsqueda por clave",
                    "La comparación se realiza con el ID, no con la posición del registro.",
                    "actual.ID == objetivo"),
                ConceptoEstructura(
                    "Registro encontrado",
                    "Cuando hay coincidencia, todos los campos deben provenir de ese mismo registro.",
                    "conservar el registro completo"),
                ConceptoEstructura(
                    "Ausencia",
                    "Si no hay coincidencia, se informa no encontrado sin inventar datos.",
                    "bool encontrado = false;")
            },
            new[] {
                "Lee la colección y después el ID objetivo.",
                "Inicia el estado como no encontrado.",
                "Compara el objetivo con los IDs disponibles.",
                "Conserva el registro completo cuando coincida.",
                "Muestra una sola respuesta lógica.",
                "Presenta ID, nombre y promedio únicamente si existe."
            },
            HerramientaEstructura(
                "Separar resultado y dato",
                "Un booleano indica si la búsqueda tuvo éxito y un registro conserva el resultado.",
                "Evita usar contenido no inicializado cuando no existe coincidencia.",
                "bool encontrado = false;\nEstudiante resultado;",
                "La forma de detener o completar el recorrido queda a tu elección."),
            "3\n10\nAna\n8.5\n20\nLuis Pérez\n7\n30\nCarla\n9\n20",
            "Encontrado: Sí\nID: 20\nNombre: Luis Pérez\nPromedio: 7",
            new[] {
                "Comparar el objetivo con el índice.",
                "Devolver los datos de otro estudiante.",
                "Indicar no encontrado y mostrar un registro.",
                "Inventar campos si el ID no existe.",
                "Mezclar nombre y promedio de registros diferentes."
            },
            "EndForge comprobará el booleano y, solo cuando corresponda, el bloque completo asociado al ID objetivo.");
    }

    private static GuiaPractica CrearGuiaMejorPromedioEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que conserva un registro candidato mientras compara los promedios de toda la colección.",
            new[] {
                DatoEstructura("estudiantes", "colección", "Uno a ocho registros con promedio", "Ana, 8.5"),
                DatoEstructura("mejor", "registro", "Primer estudiante con el promedio máximo", "Luis, 9")
            },
            new[] {
                ConceptoEstructura(
                    "Candidato inicial",
                    "El primer registro es una referencia segura aunque todos los promedios sean cero o negativos.",
                    "Estudiante mejor = estudiantes[0];"),
                ConceptoEstructura(
                    "Comparación estricta",
                    "Actualizar solo con un valor mayor conserva al primero cuando hay empate.",
                    "actual.promedio > mejor.promedio"),
                ConceptoEstructura(
                    "Registro completo",
                    "Al cambiar de candidato deben cambiar juntos ID, nombre y promedio.",
                    "mejor = actual;")
            },
            new[] {
                "Lee al menos un estudiante.",
                "Usa el primer registro como candidato inicial.",
                "Compara los demás promedios con el candidato.",
                "Actualiza únicamente ante un valor estrictamente mayor.",
                "No reemplaces el candidato en un empate.",
                "Muestra ID, nombre y promedio del ganador."
            },
            HerramientaEstructura(
                "Inicializar desde datos reales",
                "Tomar el primer elemento evita suponer que los promedios siempre son positivos.",
                "Hace que el mismo recorrido funcione con cero y valores negativos.",
                "Registro candidato = registros[0];",
                "El tipo y los nombres son ilustrativos; no se exige esta sintaxis."),
            "3\n1\nAna\n8.5\n2\nLuis\n9\n3\nCarla\n7",
            "ID: 2\nNombre: Luis\nPromedio: 9",
            new[] {
                "Iniciar el mejor promedio en cero.",
                "Actualizar también cuando los valores sean iguales.",
                "Mostrar el promedio máximo con datos de otro estudiante.",
                "Elegir siempre el último registro.",
                "Omitir algún campo del ganador."
            },
            "EndForge revisará el registro completo del primer estudiante que alcance el promedio máximo.");
    }

    private static GuiaPractica CrearGuiaOrdenarEstudiantesEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que reorganiza estudiantes completos por promedio descendente sin alterar el orden relativo de los empates.",
            new[] {
                DatoEstructura("estudiantes", "colección", "Registros con ID, nombre y promedio", "1, Ana, 8"),
                DatoEstructura("orden", "descendente estable", "Mayor promedio primero; empates en orden de captura", "9, 8, 8, 7")
            },
            new[] {
                ConceptoEstructura(
                    "Orden descendente",
                    "Los promedios más altos deben aparecer antes que los menores.",
                    "9 antes de 8 y 8 antes de 7"),
                ConceptoEstructura(
                    "Estabilidad",
                    "Dos registros con el mismo promedio conservan su orden original.",
                    "Ana antes que Luis si ambos tienen 8 y Ana fue capturada primero"),
                ConceptoEstructura(
                    "Unidad del registro",
                    "Al intercambiar posiciones se mueve la entidad completa.",
                    "ID, nombre y promedio viajan juntos")
            },
            new[] {
                "Lee todos los registros en el orden de captura.",
                "Compara los promedios de acuerdo con un orden descendente.",
                "Conserva juntos todos los campos al mover un estudiante.",
                "No intercambies dos registros cuando sus promedios sean iguales.",
                "Muestra exactamente todos los registros ordenados.",
                "Prueba varios empates consecutivos."
            },
            HerramientaEstructura(
                "Comparación por un campo",
                "El criterio observa el promedio, pero el elemento que se reordena es el registro completo.",
                "Evita que nombres, IDs y promedios pierdan su correspondencia.",
                "bool vaAntes = primero.promedio > segundo.promedio;",
                "Puedes elegir cualquier método de ordenamiento que respete el contrato observable."),
            "3\n1\nAna\n8\n2\nLuis\n9\n3\nCarla\n7",
            "ID: 2\nNombre: Luis\nPromedio: 9\nID: 1\nNombre: Ana\nPromedio: 8\nID: 3\nNombre: Carla\nPromedio: 7",
            new[] {
                "Ordenar de menor a mayor.",
                "Intercambiar solo los promedios.",
                "Cambiar el orden de estudiantes empatados.",
                "Eliminar un registro repetido en promedio.",
                "Mostrar un registro adicional o faltante."
            },
            "EndForge comparará todos los bloques por ID y exigirá el orden descendente estable.");
    }

    private static GuiaPractica CrearGuiaInventarioProductosEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que representa productos, calcula el valor almacenado de cada uno y obtiene el valor total del inventario.",
            new[] {
                DatoEstructura("código", "int", "Clave única del producto", "1"),
                DatoEstructura("nombre", "string", "Nombre completo del producto", "Cuaderno profesional"),
                DatoEstructura("precio", "double", "Precio de una unidad", "25.5"),
                DatoEstructura("cantidad", "int", "Unidades disponibles", "2"),
                DatoEstructura("total producto", "double", "Precio multiplicado por cantidad", "51"),
                DatoEstructura("total inventario", "double", "Suma de todos los totales de producto", "51")
            },
            new[] {
                ConceptoEstructura(
                    "Campo calculado",
                    "El valor de un producto depende de dos campos del mismo registro.",
                    "double total = precio * cantidad;"),
                ConceptoEstructura(
                    "Acumulador general",
                    "Cada subtotal se agrega al total del inventario una sola vez.",
                    "inventario += totalProducto;"),
                ConceptoEstructura(
                    "Cantidad cero",
                    "Un producto sin unidades conserva sus datos y aporta cero al total.",
                    "precio * 0 produce 0")
            },
            new[] {
                "Lee la cantidad de productos.",
                "Captura código, nombre completo, precio y cantidad.",
                "Calcula precio por cantidad para cada registro.",
                "Agrega cada subtotal al acumulador general.",
                "Muestra todos los campos y el total de cada producto.",
                "Presenta el valor total del inventario por separado."
            },
            HerramientaEstructura(
                "Calcular antes de acumular",
                "Guardar el subtotal del registro facilita comprobarlo y sumarlo al total general.",
                "Evita sumar precios o cantidades por separado.",
                "double subtotal = precio * cantidad;\ntotalGeneral += subtotal;",
                "El fragmento muestra el patrón matemático, no la solución completa."),
            "2\n1\nLápiz\n10\n3\n2\nRegla\n15\n2",
            "Código: 1\nNombre: Lápiz\nPrecio: 10\nCantidad: 3\nTotal producto: 30\nCódigo: 2\nNombre: Regla\nPrecio: 15\nCantidad: 2\nTotal producto: 30\nTotal inventario: 60",
            new[] {
                "Sumar precios sin multiplicar cantidades.",
                "Mezclar el total de un producto con otro.",
                "Omitir productos con cantidad cero.",
                "Calcular mal el acumulado general.",
                "Perder decimales de los precios."
            },
            "EndForge revisará cada producto por código y el total general con tolerancia de 0.01.");
    }

    private static GuiaPractica CrearGuiaRegistroEmpleadosEstructuras() {
        return CrearGuiaEstructuras(
            "Un programa que conserva todos los empleados y obtiene un resumen únicamente de quienes tienen estado activo.",
            new[] {
                DatoEstructura("ID", "int", "Identificador único del empleado", "1"),
                DatoEstructura("nombre", "string", "Nombre completo", "Ana Torres"),
                DatoEstructura("salario", "double", "Salario del empleado", "10000"),
                DatoEstructura("activo", "bool", "Indica si participa en el resumen", "true"),
                DatoEstructura("cantidad activa", "int", "Número de empleados activos", "1"),
                DatoEstructura("promedio activo", "double", "Suma de salarios activos dividida entre su cantidad", "10000")
            },
            new[] {
                ConceptoEstructura(
                    "Filtro",
                    "Una condición decide qué registros participan en un cálculo sin eliminarlos de la salida.",
                    "if (empleado.activo) { /* acumular */ }"),
                ConceptoEstructura(
                    "Resumen condicionado",
                    "El contador y la suma aumentan solo para registros activos.",
                    "activos++;\nsumaActivos += salario;"),
                ConceptoEstructura(
                    "Conjunto vacío",
                    "Si no hay activos, el promedio definido por el contrato es cero.",
                    "promedioActivos = 0.0;")
            },
            new[] {
                "Lee todos los campos de cada empleado.",
                "Muestra cada registro aunque esté inactivo.",
                "Cuando esté activo, aumenta el contador y suma su salario.",
                "No incluyas salarios inactivos.",
                "Divide entre la cantidad activa cuando sea mayor que cero.",
                "Usa cero si no existen activos.",
                "Muestra cantidad y promedio con etiquetas distintas."
            },
            HerramientaEstructura(
                "Acumulación con filtro",
                "Aplicar la condición antes de actualizar el resumen mantiene fuera a los registros no seleccionados.",
                "Sirve para obtener estadísticas de un subconjunto sin crear otra colección.",
                "if (seleccionado) {\n    cantidad++;\n    suma += valor;\n}",
                "La técnica es reutilizable y no obliga una representación concreta del registro."),
            "2\n1\nAna\n10000\n1\n2\nLuis\n8000\n0",
            "ID: 1\nNombre: Ana\nSalario: 10000\nActivo: Sí\nID: 2\nNombre: Luis\nSalario: 8000\nActivo: No\nActivos: 1\nPromedio activos: 10000",
            new[] {
                "Incluir salarios inactivos en la suma.",
                "Dividir entre el total de empleados.",
                "Dividir entre cero cuando no hay activos.",
                "Mostrar un estado que contradice el dato capturado.",
                "Omitir empleados inactivos de la lista."
            },
            "EndForge revisará todos los bloques y calculará el resumen únicamente con empleados activos.");
    }

    private static GuiaPractica CrearGuiaEstructuras(
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

    private static DatoGuiaPractica DatoEstructura(
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

    private static ConceptoGuiaPractica ConceptoEstructura(
        string nombre,
        string explicacion,
        string fragmento) {
        return new ConceptoGuiaPractica {
            Nombre = nombre,
            Explicacion = explicacion,
            Fragmento = fragmento
        };
    }

    private static HerramientaGuiaPractica HerramientaEstructura(
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
