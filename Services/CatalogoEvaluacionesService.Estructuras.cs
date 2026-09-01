using EndForge.Models;
using System.Globalization;

namespace EndForge.Services;

public sealed partial class CatalogoEvaluacionesService {
    public const string EstructurasDatosEstudianteId =
        "grado2-estructuras-datos-estudiante";
    public const string EstructurasPromedioEstudianteId =
        "grado2-estructuras-promedio-estudiante";
    public const string EstructurasArregloEstudiantesId =
        "grado2-estructuras-arreglo-estudiantes";
    public const string EstructurasBuscarEstudianteId =
        "grado2-estructuras-buscar-estudiante";
    public const string EstructurasMejorPromedioId =
        "grado2-estructuras-mejor-promedio";
    public const string EstructurasOrdenarEstudiantesId =
        "grado2-estructuras-ordenar-estudiantes";
    public const string EstructurasInventarioProductosId =
        "grado2-estructuras-inventario-productos";
    public const string EstructurasRegistroEmpleadosId =
        "grado2-estructuras-registro-empleados";

    private const int PuntosPorCasoEstructuras = 12;

    private static readonly string[] EtiquetasIdEstructuras = {
        "ID",
        "Matrícula",
        "Matricula",
        "Identificador"
    };

    private static readonly string[] EtiquetasNombreEstructuras = {
        "Nombre completo",
        "Nombre",
        "Estudiante"
    };

    private static readonly string[] EtiquetasPromedioEstructuras = {
        "Promedio",
        "Calificación",
        "Calificacion",
        "Media"
    };

    private static DefinicionEvaluacionPractica CrearEstructurasDatosEstudiante(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionEstructuras(
            EstructurasDatosEstudianteId,
            "Capturar datos de estudiante",
            "Agrupar y mostrar los datos completos de un estudiante.",
            "Se comprobará un único bloque con ID, nombre completo, edad y promedio.",
            "4 líneas: ID entero; nombre completo; edad entera; promedio decimal.",
            new[] { "ID", "Nombre completo", "Edad", "Promedio" },
            new[] {
                "Mostrar los cuatro campos en un mismo bloque.",
                "Preservar exactamente el nombre completo.",
                "Mostrar el promedio con una tolerancia de 0.01."
            },
            new[] {
                CrearCasoDatosEstudianteEstructuras(
                    "estructuras-datos-ana",
                    "Nombre compuesto",
                    10,
                    "Ana López",
                    17,
                    8.5D,
                    visible: true),
                CrearCasoDatosEstudianteEstructuras(
                    "estructuras-datos-luis",
                    "Promedio máximo",
                    1,
                    "Luis",
                    18,
                    10D,
                    visible: true),
                CrearCasoDatosEstudianteEstructuras(
                    "estructuras-datos-maria",
                    "Nombre de tres palabras",
                    25,
                    "María José Pérez",
                    16,
                    7.25D,
                    visible: true),
                CrearCasoDatosEstudianteEstructuras(
                    "estructuras-datos-minimos",
                    "Nombre breve y promedio cero",
                    99,
                    "A",
                    20,
                    0D,
                    visible: true),
                CrearCasoDatosEstudianteEstructuras(
                    "estructuras-datos-oculto",
                    "Registro oculto",
                    305,
                    "Carlos Hernández Ruiz",
                    19,
                    9.75D,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasPromedioEstudiante(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionEstructuras(
            EstructurasPromedioEstudianteId,
            "Calcular el promedio de un estudiante",
            "Calcular tres calificaciones y clasificar el resultado con el límite inclusivo de seis.",
            "Se comprobarán ID, nombre, promedio y una respuesta booleana coherente dentro de un solo registro.",
            "5 líneas: ID; nombre completo; tres calificaciones decimales.",
            new[] {
                "ID",
                "Nombre completo",
                "Calificación 1",
                "Calificación 2",
                "Calificación 3"
            },
            new[] {
                "Promediar exactamente las tres calificaciones.",
                "Considerar aprobado un promedio mayor o igual que 6.",
                "Preservar el nombre completo y evitar respuestas contradictorias."
            },
            new[] {
                CrearCasoPromedioEstudianteEstructuras(
                    "estructuras-promedio-ana",
                    "Promedio entero aprobatorio",
                    1,
                    "Ana López",
                    new[] { 8D, 9D, 7D },
                    8D,
                    aprobado: true,
                    visible: true),
                CrearCasoPromedioEstudianteEstructuras(
                    "estructuras-promedio-luis",
                    "Promedio reprobatorio",
                    2,
                    "Luis Pérez",
                    new[] { 5D, 5.5D, 6D },
                    5.5D,
                    aprobado: false,
                    visible: true),
                CrearCasoPromedioEstudianteEstructuras(
                    "estructuras-promedio-limite",
                    "Límite exacto de aprobación",
                    3,
                    "Carla",
                    new[] { 6D, 6D, 6D },
                    6D,
                    aprobado: true,
                    visible: true),
                CrearCasoPromedioEstudianteEstructuras(
                    "estructuras-promedio-decimal",
                    "Promedio periódico",
                    4,
                    "José Manuel",
                    new[] { 10D, 9.5D, 10D },
                    9.833333D,
                    aprobado: true,
                    visible: true),
                CrearCasoPromedioEstudianteEstructuras(
                    "estructuras-promedio-oculto",
                    "Promedio oculto",
                    5,
                    "Sofía Ruiz",
                    new[] { 0D, 10D, 5D },
                    5D,
                    aprobado: false,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasArregloEstudiantes(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionEstructuras(
            EstructurasArregloEstudiantesId,
            "Capturar y mostrar varios estudiantes",
            "Conservar una colección ordenada de estudiantes con identificadores únicos.",
            "Se comprobarán exactamente los registros capturados, su orden y la pertenencia de cada campo.",
            "Primera línea n (1 a 6); después ID, nombre completo y promedio por estudiante.",
            new[] { "Cantidad", "ID", "Nombre completo", "Promedio" },
            new[] {
                "Mostrar exactamente n registros.",
                "Conservar el orden de captura.",
                "No duplicar IDs ni mezclar campos entre estudiantes."
            },
            new[] {
                CrearCasoArregloEstudiantesEstructuras(
                    "estructuras-arreglo-dos",
                    "Dos estudiantes",
                    new[] {
                        new EstudianteEstructuras(10, "Ana López", 8.5D),
                        new EstudianteEstructuras(20, "Luis Pérez", 7D)
                    },
                    visible: true),
                CrearCasoArregloEstudiantesEstructuras(
                    "estructuras-arreglo-uno",
                    "Un estudiante",
                    new[] {
                        new EstudianteEstructuras(1, "Carla", 10D)
                    },
                    visible: true),
                CrearCasoArregloEstudiantesEstructuras(
                    "estructuras-arreglo-tres",
                    "Tres promedios variados",
                    new[] {
                        new EstudianteEstructuras(5, "A", 0D),
                        new EstudianteEstructuras(6, "B", 5.5D),
                        new EstudianteEstructuras(7, "C", 9.25D)
                    },
                    visible: true),
                CrearCasoArregloEstudiantesEstructuras(
                    "estructuras-arreglo-nombres-compuestos",
                    "Cuatro nombres compuestos",
                    new[] {
                        new EstudianteEstructuras(11, "Ana María", 8.2D),
                        new EstudianteEstructuras(12, "Juan Carlos Ruiz", 6.75D),
                        new EstudianteEstructuras(13, "Luz Elena", 9.1D),
                        new EstudianteEstructuras(14, "Pedro de la Cruz", 5.25D)
                    },
                    visible: true),
                CrearCasoArregloEstudiantesEstructuras(
                    "estructuras-arreglo-seis-oculto",
                    "Seis estudiantes ocultos",
                    new[] {
                        new EstudianteEstructuras(101, "Nora Salas", 4.5D),
                        new EstudianteEstructuras(102, "Omar Díaz", 6D),
                        new EstudianteEstructuras(103, "Pía Torres", 7.75D),
                        new EstudianteEstructuras(104, "Raúl Vega", 10D),
                        new EstudianteEstructuras(105, "Sara León", 0D),
                        new EstudianteEstructuras(106, "Tomás Gil", 8.33D)
                    },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasBuscarEstudiante(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        EstudianteEstructuras[] casoUno = {
            new(10, "Ana López", 8.5D),
            new(20, "Luis Pérez", 7D),
            new(30, "Carla Ruiz", 9D)
        };
        EstudianteEstructuras[] casoDos = {
            new(1, "Ana", 8D),
            new(2, "Luis", 7D)
        };
        EstudianteEstructuras[] casoTres = {
            new(5, "Carla", 10D)
        };
        EstudianteEstructuras[] casoCuatro = {
            new(7, "María José", 6.5D),
            new(8, "Pedro", 8D),
            new(9, "Lucía", 9D)
        };
        EstudianteEstructuras[] casoOculto = {
            new(31, "Alicia Paz", 6D),
            new(32, "Bruno Sol", 7D),
            new(33, "Celia Mar", 8D),
            new(34, "Diego Luz", 9D),
            new(35, "Elena Río", 5D),
            new(36, "Fabio Cruz", 9.5D)
        };

        return CrearDefinicionEstructuras(
            EstructurasBuscarEstudianteId,
            "Buscar un estudiante por ID",
            "Localizar un registro por su identificador e informar si existe.",
            "Se comprobará la respuesta encontrada y, solo cuando corresponda, el registro exacto.",
            "Primera línea n; después ID, nombre y promedio por estudiante; al final, ID objetivo.",
            new[] { "Cantidad", "Registros", "ID objetivo" },
            new[] {
                "Mostrar encontrado sí o no.",
                "Mostrar el registro correcto cuando existe.",
                "No inventar un registro cuando el ID no existe."
            },
            new[] {
                CrearCasoBuscarEstudianteEstructuras(
                    "estructuras-buscar-intermedio",
                    "Registro intermedio",
                    casoUno,
                    20,
                    casoUno[1],
                    visible: true),
                CrearCasoBuscarEstudianteEstructuras(
                    "estructuras-buscar-ausente",
                    "ID inexistente",
                    casoDos,
                    9,
                    encontrado: null,
                    visible: true),
                CrearCasoBuscarEstudianteEstructuras(
                    "estructuras-buscar-unico",
                    "Único registro",
                    casoTres,
                    5,
                    casoTres[0],
                    visible: true),
                CrearCasoBuscarEstudianteEstructuras(
                    "estructuras-buscar-primero",
                    "Primer registro",
                    casoCuatro,
                    7,
                    casoCuatro[0],
                    visible: true),
                CrearCasoBuscarEstudianteEstructuras(
                    "estructuras-buscar-ultimo-oculto",
                    "Último registro oculto",
                    casoOculto,
                    36,
                    casoOculto[^1],
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasMejorPromedio(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        EstudianteEstructuras[] casoUno = {
            new(1, "Ana", 8.5D),
            new(2, "Luis", 9D),
            new(3, "Carla", 7D)
        };
        EstudianteEstructuras[] casoDos = {
            new(8, "Mario", 6.25D)
        };
        EstudianteEstructuras[] casoTres = {
            new(1, "Ana", 8D),
            new(2, "Luis", 8D)
        };
        EstudianteEstructuras[] casoCuatro = {
            new(11, "Alicia", -3D),
            new(12, "Bruno", 0D),
            new(13, "Celia", -1D)
        };
        EstudianteEstructuras[] casoOculto = {
            new(21, "Nora", 1D),
            new(22, "Omar", 9.25D),
            new(23, "Pía", 7D),
            new(24, "Raúl", 9.25D)
        };

        return CrearDefinicionEstructuras(
            EstructurasMejorPromedioId,
            "Encontrar estudiante con mejor promedio",
            "Seleccionar el registro con el promedio máximo y resolver empates por primera aparición.",
            "Se comprobarán juntos el ID, nombre y promedio del estudiante ganador.",
            "Primera línea n; después ID, nombre completo y promedio por estudiante.",
            new[] { "Cantidad", "ID", "Nombre completo", "Promedio" },
            new[] {
                "Seleccionar el promedio más alto.",
                "Conservar el primer estudiante cuando exista empate.",
                "No separar el promedio de su registro."
            },
            new[] {
                CrearCasoMejorPromedioEstructuras(
                    "estructuras-mejor-intermedio",
                    "Máximo intermedio",
                    casoUno,
                    casoUno[1],
                    visible: true),
                CrearCasoMejorPromedioEstructuras(
                    "estructuras-mejor-unico",
                    "Un estudiante",
                    casoDos,
                    casoDos[0],
                    visible: true),
                CrearCasoMejorPromedioEstructuras(
                    "estructuras-mejor-empate",
                    "Empate al inicio",
                    casoTres,
                    casoTres[0],
                    visible: true),
                CrearCasoMejorPromedioEstructuras(
                    "estructuras-mejor-no-positivo",
                    "Promedios negativos y cero",
                    casoCuatro,
                    casoCuatro[1],
                    visible: true),
                CrearCasoMejorPromedioEstructuras(
                    "estructuras-mejor-empate-oculto",
                    "Empate no adyacente oculto",
                    casoOculto,
                    casoOculto[1],
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasOrdenarEstudiantes(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        EstudianteEstructuras[] casoUno = {
            new(1, "Ana", 8D),
            new(2, "Luis", 9D),
            new(3, "Carla", 7D)
        };
        EstudianteEstructuras[] casoDos = {
            new(10, "Ana", 8D),
            new(20, "Luis", 8D),
            new(30, "Carla", 7D)
        };
        EstudianteEstructuras[] casoTres = {
            new(5, "Sofía", 9.5D)
        };
        EstudianteEstructuras[] casoCuatro = {
            new(11, "A", 7D),
            new(12, "B", 9D),
            new(13, "C", 7D),
            new(14, "D", 9D),
            new(15, "E", 8D)
        };
        EstudianteEstructuras[] casoOculto = {
            new(101, "Nora", 6D),
            new(102, "Omar", 10D),
            new(103, "Pía", 6D),
            new(104, "Raúl", 8D),
            new(105, "Sara", 10D),
            new(106, "Tomás", 8D)
        };

        return CrearDefinicionEstructuras(
            EstructurasOrdenarEstudiantesId,
            "Ordenar estudiantes por promedio",
            "Ordenar registros completos de mayor a menor promedio sin alterar empates.",
            "Se comprobarán cantidad, claves, campos, orden descendente y estabilidad.",
            "Primera línea n; después ID, nombre completo y promedio por estudiante.",
            new[] { "Cantidad", "ID", "Nombre completo", "Promedio" },
            new[] {
                "Mostrar todos los registros.",
                "Ordenar por promedio descendente.",
                "Conservar el orden de captura entre promedios iguales."
            },
            new[] {
                CrearCasoOrdenarEstudiantesEstructuras(
                    "estructuras-ordenar-tres",
                    "Tres promedios diferentes",
                    casoUno,
                    new[] { casoUno[1], casoUno[0], casoUno[2] },
                    visible: true),
                CrearCasoOrdenarEstudiantesEstructuras(
                    "estructuras-ordenar-empate",
                    "Empate estable",
                    casoDos,
                    new[] { casoDos[0], casoDos[1], casoDos[2] },
                    visible: true),
                CrearCasoOrdenarEstudiantesEstructuras(
                    "estructuras-ordenar-unico",
                    "Un estudiante",
                    casoTres,
                    casoTres,
                    visible: true),
                CrearCasoOrdenarEstudiantesEstructuras(
                    "estructuras-ordenar-repetidos",
                    "Cinco estudiantes con empates",
                    casoCuatro,
                    new[] {
                        casoCuatro[1],
                        casoCuatro[3],
                        casoCuatro[4],
                        casoCuatro[0],
                        casoCuatro[2]
                    },
                    visible: true),
                CrearCasoOrdenarEstudiantesEstructuras(
                    "estructuras-ordenar-oculto",
                    "Seis estudiantes con tres empates",
                    casoOculto,
                    new[] {
                        casoOculto[1],
                        casoOculto[4],
                        casoOculto[3],
                        casoOculto[5],
                        casoOculto[0],
                        casoOculto[2]
                    },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasInventarioProductos(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        ProductoEstructuras[] casoUno = {
            new(1, "Cuaderno", 25.5D, 2)
        };
        ProductoEstructuras[] casoDos = {
            new(10, "Lápiz", 10D, 3),
            new(20, "Regla", 15D, 2)
        };
        ProductoEstructuras[] casoTres = {
            new(7, "Muestra", 99.9D, 0)
        };
        ProductoEstructuras[] casoCuatro = {
            new(31, "Cable de red", 19.99D, 3),
            new(32, "Memoria USB", 125.5D, 2),
            new(33, "Base portátil", 87.25D, 1)
        };
        ProductoEstructuras[] casoOculto = {
            new(101, "Producto A", 1.25D, 4),
            new(102, "Producto B", 10D, 0),
            new(103, "Producto C", 7.5D, 3),
            new(104, "Producto D", 100.99D, 2),
            new(105, "Producto E", 0.5D, 10)
        };

        return CrearDefinicionEstructuras(
            EstructurasInventarioProductosId,
            "Gestionar un inventario de productos",
            "Calcular el valor de cada producto y el total acumulado del inventario.",
            "Se comprobará cada producto por código y un total general separado.",
            "Primera línea n; después código, nombre, precio y cantidad por producto.",
            new[] { "Cantidad", "Código", "Nombre", "Precio", "Cantidad" },
            new[] {
                "Calcular precio por cantidad para cada producto.",
                "Mostrar todos los productos sin mezclar campos.",
                "Acumular correctamente el total del inventario."
            },
            new[] {
                CrearCasoInventarioProductosEstructuras(
                    "estructuras-inventario-uno",
                    "Un producto",
                    casoUno,
                    visible: true),
                CrearCasoInventarioProductosEstructuras(
                    "estructuras-inventario-dos",
                    "Dos subtotales iguales",
                    casoDos,
                    visible: true),
                CrearCasoInventarioProductosEstructuras(
                    "estructuras-inventario-cero",
                    "Cantidad cero",
                    casoTres,
                    visible: true),
                CrearCasoInventarioProductosEstructuras(
                    "estructuras-inventario-decimales",
                    "Nombres compuestos y precios decimales",
                    casoCuatro,
                    visible: true),
                CrearCasoInventarioProductosEstructuras(
                    "estructuras-inventario-oculto",
                    "Cinco productos ocultos",
                    casoOculto,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearEstructurasRegistroEmpleados(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        EmpleadoEstructuras[] casoUno = {
            new(1, "Ana", 10000D, true),
            new(2, "Luis", 8000D, false)
        };
        EmpleadoEstructuras[] casoDos = {
            new(10, "Carla", 5000D, true),
            new(20, "Mario", 7000D, true),
            new(30, "Sofía", 9000D, false)
        };
        EmpleadoEstructuras[] casoTres = {
            new(7, "Elena", 4500D, false),
            new(8, "Pablo", 6500D, false)
        };
        EmpleadoEstructuras[] casoCuatro = {
            new(31, "Ana María", 5000.25D, true),
            new(32, "Juan Carlos", 7000.75D, true),
            new(33, "Luz Elena", 9000.5D, true)
        };
        EmpleadoEstructuras[] casoOculto = {
            new(101, "Alicia", 4100D, true),
            new(102, "Bruno", 5200D, false),
            new(103, "Celia", 6300D, true),
            new(104, "Diego", 7400D, false),
            new(105, "Elena", 8500D, true),
            new(106, "Fabio", 9600D, false)
        };

        return CrearDefinicionEstructuras(
            EstructurasRegistroEmpleadosId,
            "Clasificar empleados por salario",
            "Mostrar empleados y resumir únicamente los salarios de quienes están activos.",
            "Se comprobará cada empleado, su estado flexible y el resumen de cantidad y promedio activos.",
            "Primera línea n; después ID, nombre, salario y estado 1/0 por empleado.",
            new[] { "Cantidad", "ID", "Nombre", "Salario", "Activo" },
            new[] {
                "Mostrar todos los empleados.",
                "Contar y sumar únicamente empleados activos.",
                "Mostrar promedio cero cuando no existan activos."
            },
            new[] {
                CrearCasoRegistroEmpleadosEstructuras(
                    "estructuras-empleados-uno-activo",
                    "Un activo y un inactivo",
                    casoUno,
                    visible: true),
                CrearCasoRegistroEmpleadosEstructuras(
                    "estructuras-empleados-dos-activos",
                    "Dos empleados activos",
                    casoDos,
                    visible: true),
                CrearCasoRegistroEmpleadosEstructuras(
                    "estructuras-empleados-ninguno",
                    "Ningún empleado activo",
                    casoTres,
                    visible: true),
                CrearCasoRegistroEmpleadosEstructuras(
                    "estructuras-empleados-todos",
                    "Todos activos con decimales",
                    casoCuatro,
                    visible: true),
                CrearCasoRegistroEmpleadosEstructuras(
                    "estructuras-empleados-oculto",
                    "Estados alternados ocultos",
                    casoOculto,
                    visible: false)
            },
            rubrica);
    }

    private static CasoPrueba CrearCasoDatosEstudianteEstructuras(
        string idCaso,
        string nombreCaso,
        int id,
        string nombre,
        int edad,
        double promedio,
        bool visible) {
        RegistroEsperado registro = CrearRegistroEstructuras(
            $"Estudiante {id}",
            ClaveNumericaEstructuras("ID", id),
            CampoTextoEstructuras(
                "Nombre",
                nombre,
                EtiquetasNombreEstructuras),
            CampoNumeroEstructuras(
                "Edad",
                edad,
                0D,
                "Edad",
                "Años",
                "Anos"),
            CampoNumeroEstructuras(
                "Promedio",
                promedio,
                0.01D,
                EtiquetasPromedioEstructuras));

        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            $"{id}\n{nombre}\n{edad}\n{FormatearNumeroEstructuras(promedio)}\n",
            SalidaRegistroEstudianteEstructuras(
                new EstudianteEstructuras(id, nombre, promedio, edad)),
            "Comprueba que los cuatro campos pertenezcan al mismo estudiante.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Datos del estudiante",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    new[] { registro },
                    ordenObligatorio: false)
            });
    }

    private static CasoPrueba CrearCasoPromedioEstudianteEstructuras(
        string idCaso,
        string nombreCaso,
        int id,
        string nombre,
        double[] calificaciones,
        double promedio,
        bool aprobado,
        bool visible) {
        RegistroEsperado registro = CrearRegistroEstructuras(
            $"Estudiante {id}",
            ClaveNumericaEstructuras("ID", id),
            CampoTextoEstructuras(
                "Nombre",
                nombre,
                EtiquetasNombreEstructuras),
            CampoNumeroEstructuras(
                "Promedio",
                promedio,
                0.01D,
                EtiquetasPromedioEstructuras),
            CampoBooleanoEstructuras(
                "Aprobado",
                aprobado,
                "Aprobado",
                "Aprobada",
                "Resultado",
                "Estado"));
        string entrada =
            $"{id}\n{nombre}\n" +
            string.Join(
                "\n",
                calificaciones.Select(FormatearNumeroEstructuras)) +
            "\n";
        string salida =
            $"ID: {id}\n" +
            $"Nombre: {nombre}\n" +
            $"Promedio: {FormatearNumeroEstructuras(promedio)}\n" +
            $"Aprobado: {(aprobado ? "Sí" : "No")}";

        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            entrada,
            salida,
            "Comprueba el promedio de tres notas y el límite inclusivo de aprobación.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Promedio del estudiante",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    new[] { registro },
                    ordenObligatorio: false)
            });
    }

    private static CasoPrueba CrearCasoArregloEstudiantesEstructuras(
        string idCaso,
        string nombreCaso,
        EstudianteEstructuras[] estudiantes,
        bool visible) {
        RegistroEsperado[] registros = estudiantes
            .Select(estudiante => CrearRegistroEstudianteEstructuras(
                estudiante,
                incluirEdad: false))
            .ToArray();

        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            EntradaEstudiantesEstructuras(estudiantes),
            SalidaEstudiantesEstructuras(estudiantes),
            "Comprueba cantidad exacta, IDs únicos, orden y campos completos.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Estudiantes capturados",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    registros,
                    ordenObligatorio: true)
            });
    }

    private static CasoPrueba CrearCasoBuscarEstudianteEstructuras(
        string idCaso,
        string nombreCaso,
        EstudianteEstructuras[] estudiantes,
        int idObjetivo,
        EstudianteEstructuras? encontrado,
        bool visible) {
        bool existe = encontrado is not null;
        RegistroEsperado[] registros = existe
            ? new[] {
                CrearRegistroEstudianteEstructuras(
                    encontrado!,
                    incluirEdad: false)
            }
            : Array.Empty<RegistroEsperado>();
        string salida = existe
            ? $"Encontrado: Sí\n{SalidaRegistroEstudianteEstructuras(encontrado!)}"
            : "Encontrado: No";
        ValorNumericoEsperado[] valoresAusentes = existe
            ? Array.Empty<ValorNumericoEsperado>()
            : new[] {
                NumeroAusenteEstructuras(
                    "ID de registro",
                    EtiquetasIdEstructuras),
                NumeroAusenteEstructuras(
                    "Promedio de registro",
                    EtiquetasPromedioEstructuras)
            };
        ReglaCadenaEsperada[] cadenasAusentes = existe
            ? Array.Empty<ReglaCadenaEsperada>()
            : new[] {
                CadenaAusenteEstructuras(
                    "Nombre de registro",
                    EtiquetasNombreEstructuras)
            };

        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            EntradaEstudiantesEstructuras(estudiantes) +
            idObjetivo.ToString(CultureInfo.InvariantCulture) + "\n",
            salida,
            existe
                ? "Comprueba la existencia y el registro asociado al ID objetivo."
                : "Comprueba que un ID ausente no produzca datos inventados.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Estudiante encontrado",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    registros,
                    ordenObligatorio: false,
                    obligatoria: existe)
            },
            numeros: valoresAusentes,
            booleanos: new[] {
                BooleanoEstructuras(
                    "Encontrado",
                    existe,
                    "Encontrado",
                    "Existe",
                    "Se encontró",
                    "Se encontro",
                    "Hallado")
            },
            cadenas: cadenasAusentes);
    }

    private static CasoPrueba CrearCasoMejorPromedioEstructuras(
        string idCaso,
        string nombreCaso,
        EstudianteEstructuras[] estudiantes,
        EstudianteEstructuras esperado,
        bool visible) {
        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            EntradaEstudiantesEstructuras(estudiantes),
            SalidaRegistroEstudianteEstructuras(esperado),
            "Comprueba el registro completo del primer estudiante con el promedio máximo.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Mejor estudiante",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    new[] {
                        CrearRegistroEstudianteEstructuras(
                            esperado,
                            incluirEdad: false)
                    },
                    ordenObligatorio: false)
            });
    }

    private static CasoPrueba CrearCasoOrdenarEstudiantesEstructuras(
        string idCaso,
        string nombreCaso,
        EstudianteEstructuras[] entrada,
        EstudianteEstructuras[] ordenEsperado,
        bool visible) {
        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            EntradaEstudiantesEstructuras(entrada),
            SalidaEstudiantesEstructuras(ordenEsperado),
            "Comprueba orden descendente estable sin perder ni duplicar registros.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Estudiantes ordenados",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    ordenEsperado
                        .Select(estudiante =>
                            CrearRegistroEstudianteEstructuras(
                                estudiante,
                                incluirEdad: false))
                        .ToArray(),
                    ordenObligatorio: true)
            });
    }

    private static CasoPrueba CrearCasoInventarioProductosEstructuras(
        string idCaso,
        string nombreCaso,
        ProductoEstructuras[] productos,
        bool visible) {
        double totalInventario = productos.Sum(producto => producto.Total);
        RegistroEsperado[] registros = productos
            .Select(producto => CrearRegistroEstructuras(
                $"Producto {producto.Codigo}",
                ClaveNumericaEstructuras("Código", producto.Codigo),
                CampoTextoEstructuras(
                    "Nombre",
                    producto.Nombre,
                    "Nombre",
                    "Producto",
                    "Nombre producto"),
                CampoNumeroEstructuras(
                    "Precio",
                    producto.Precio,
                    0.01D,
                    "Precio",
                    "Precio unitario",
                    "Costo"),
                CampoNumeroEstructuras(
                    "Cantidad",
                    producto.Cantidad,
                    0D,
                    "Cantidad",
                    "Existencias",
                    "Unidades"),
                CampoNumeroEstructuras(
                    "Total del producto",
                    producto.Total,
                    0.01D,
                    "Total producto",
                    "Valor producto",
                    "Subtotal")))
            .ToArray();

        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            EntradaProductosEstructuras(productos),
            SalidaProductosEstructuras(productos, totalInventario),
            "Comprueba cada producto y el total acumulado del inventario.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Productos del inventario",
                    new[] {
                        "Código",
                        "Codigo",
                        "Código producto",
                        "Codigo producto"
                    },
                    TipoValorEstructurado.Numerico,
                    registros,
                    ordenObligatorio: false)
            },
            numeros: new[] {
                NumeroEstructuras(
                    "Total del inventario",
                    totalInventario,
                    0.01D,
                    "Total inventario",
                    "Valor inventario",
                    "Total general")
            });
    }

    private static CasoPrueba CrearCasoRegistroEmpleadosEstructuras(
        string idCaso,
        string nombreCaso,
        EmpleadoEstructuras[] empleados,
        bool visible) {
        EmpleadoEstructuras[] activos = empleados
            .Where(empleado => empleado.Activo)
            .ToArray();
        int cantidadActivos = activos.Length;
        double promedioActivos = cantidadActivos == 0
            ? 0D
            : activos.Average(empleado => empleado.Salario);
        RegistroEsperado[] registros = empleados
            .Select(empleado => CrearRegistroEstructuras(
                $"Empleado {empleado.Id}",
                ClaveNumericaEstructuras("ID", empleado.Id),
                CampoTextoEstructuras(
                    "Nombre",
                    empleado.Nombre,
                    "Nombre",
                    "Empleado",
                    "Nombre completo"),
                CampoNumeroEstructuras(
                    "Salario",
                    empleado.Salario,
                    0.01D,
                    "Salario",
                    "Sueldo",
                    "Ingreso"),
                CampoBooleanoEstructuras(
                    "Activo",
                    empleado.Activo,
                    "Activo",
                    "Activa",
                    "Estado",
                    "En activo")))
            .ToArray();

        return CrearCasoEstructuras(
            idCaso,
            nombreCaso,
            EntradaEmpleadosEstructuras(empleados),
            SalidaEmpleadosEstructuras(
                empleados,
                cantidadActivos,
                promedioActivos),
            "Comprueba todos los empleados y resume únicamente los salarios activos.",
            visible,
            new[] {
                CrearReglaRegistrosEstructuras(
                    "Empleados registrados",
                    EtiquetasIdEstructuras,
                    TipoValorEstructurado.Numerico,
                    registros,
                    ordenObligatorio: false)
            },
            numeros: new[] {
                NumeroEstructuras(
                    "Cantidad de activos",
                    cantidadActivos,
                    0D,
                    "Cantidad activos",
                    "Empleados activos",
                    "Total activos"),
                NumeroEstructuras(
                    "Promedio de activos",
                    promedioActivos,
                    0.01D,
                    "Promedio activos",
                    "Promedio activo",
                    "Salario promedio activos",
                    "Promedio de salarios activos")
            });
    }

    private static CasoPrueba CrearCasoEstructuras(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        bool visible,
        ReglaBloquesRegistroEsperados[] bloques,
        ValorNumericoEsperado[]? numeros = null,
        ValorBooleanoEsperado[]? booleanos = null,
        ReglaCadenaEsperada[]? cadenas = null) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = salidaEsperada,
            EsVisible = visible,
            Puntos = PuntosPorCasoEstructuras,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion = descripcion,
            ValoresNumericosEsperados = Array.AsReadOnly(
                numeros ?? Array.Empty<ValorNumericoEsperado>()),
            ValoresBooleanosEsperados = Array.AsReadOnly(
                booleanos ?? Array.Empty<ValorBooleanoEsperado>()),
            CadenasEsperadas = Array.AsReadOnly(
                cadenas ?? Array.Empty<ReglaCadenaEsperada>()),
            BloquesRegistroEsperados = Array.AsReadOnly(bloques)
        };
    }

    private static DefinicionEvaluacionPractica CrearDefinicionEstructuras(
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

    private static ReglaBloquesRegistroEsperados
        CrearReglaRegistrosEstructuras(
        string nombre,
        string[] etiquetasClave,
        TipoValorEstructurado tipoClave,
        RegistroEsperado[] registros,
        bool ordenObligatorio,
        bool obligatoria = true) {
        return new ReglaBloquesRegistroEsperados {
            Nombre = nombre,
            NombreCampoClave = nombre,
            EtiquetasClave = Array.AsReadOnly(etiquetasClave),
            TipoClave = tipoClave,
            RegistrosEsperados = Array.AsReadOnly(registros),
            OrdenRegistrosObligatorio = ordenObligatorio,
            PermitirRegistrosAdicionales = false,
            PermitirRegistrosDuplicados = false,
            PermitirTextoNeutralEntreBloques = true,
            Obligatoria = obligatoria,
            MensajeError =
                $"Los bloques de {nombre.ToLowerInvariant()} deben conservar sus claves, campos y valores."
        };
    }

    private static RegistroEsperado CrearRegistroEstudianteEstructuras(
        EstudianteEstructuras estudiante,
        bool incluirEdad) {
        List<CampoRegistroEsperado> campos = new() {
            CampoTextoEstructuras(
                "Nombre",
                estudiante.Nombre,
                EtiquetasNombreEstructuras)
        };

        if (incluirEdad && estudiante.Edad.HasValue) {
            campos.Add(CampoNumeroEstructuras(
                "Edad",
                estudiante.Edad.Value,
                0D,
                "Edad",
                "Años",
                "Anos"));
        }

        campos.Add(CampoNumeroEstructuras(
            "Promedio",
            estudiante.Promedio,
            0.01D,
            EtiquetasPromedioEstructuras));

        return CrearRegistroEstructuras(
            $"Estudiante {estudiante.Id}",
            ClaveNumericaEstructuras("ID", estudiante.Id),
            campos.ToArray());
    }

    private static RegistroEsperado CrearRegistroEstructuras(
        string nombre,
        ValorEstructuradoEsperado clave,
        params CampoRegistroEsperado[] campos) {
        return new RegistroEsperado {
            Nombre = nombre,
            Clave = clave,
            Campos = Array.AsReadOnly(campos)
        };
    }

    private static CampoRegistroEsperado CampoNumeroEstructuras(
        string nombre,
        double valor,
        double tolerancia,
        params string[] etiquetas) {
        return new CampoRegistroEsperado {
            Nombre = nombre,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            Valor = new ValorEstructuradoEsperado {
                Nombre = nombre,
                Tipo = TipoValorEstructurado.Numerico,
                ValorNumerico = valor,
                ToleranciaNumerica = tolerancia
            },
            Obligatorio = true
        };
    }

    private static CampoRegistroEsperado CampoTextoEstructuras(
        string nombre,
        string valor,
        params string[] etiquetas) {
        return new CampoRegistroEsperado {
            Nombre = nombre,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            Valor = new ValorEstructuradoEsperado {
                Nombre = nombre,
                Tipo = TipoValorEstructurado.Textual,
                ValorTextual = valor,
                DistinguirMayusculas = true,
                DistinguirAcentos = true,
                PoliticaEspacios = PoliticaEspaciosCadena.Exactos
            },
            Obligatorio = true
        };
    }

    private static CampoRegistroEsperado CampoBooleanoEstructuras(
        string nombre,
        bool valor,
        params string[] etiquetas) {
        return new CampoRegistroEsperado {
            Nombre = nombre,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            Valor = new ValorEstructuradoEsperado {
                Nombre = nombre,
                Tipo = TipoValorEstructurado.Booleano,
                ValorBooleano = valor
            },
            Obligatorio = true
        };
    }

    private static ValorEstructuradoEsperado ClaveNumericaEstructuras(
        string nombre,
        double valor) {
        return new ValorEstructuradoEsperado {
            Nombre = nombre,
            Tipo = TipoValorEstructurado.Numerico,
            ValorNumerico = valor,
            ToleranciaNumerica = 0D
        };
    }

    private static ValorNumericoEsperado NumeroEstructuras(
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

    private static ValorNumericoEsperado NumeroAusenteEstructuras(
        string nombre,
        params string[] etiquetas) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            DebeEstarAusente = true,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas)
        };
    }

    private static ReglaCadenaEsperada CadenaAusenteEstructuras(
        string nombre,
        params string[] etiquetas) {
        return new ReglaCadenaEsperada {
            Nombre = nombre,
            ValorEsperado = string.Empty,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            Origen = OrigenCadenaEsperada.DespuesDeEtiqueta,
            DistinguirMayusculas = false,
            DistinguirAcentos = false,
            PoliticaEspacios = PoliticaEspaciosCadena.Exactos,
            PermitirTextoAdicional = false,
            Obligatoria = false,
            MensajeError =
                "No deben mostrarse datos de un estudiante cuando el ID no existe."
        };
    }

    private static ValorBooleanoEsperado BooleanoEstructuras(
        string nombre,
        bool valor,
        params string[] etiquetas) {
        return new ValorBooleanoEsperado {
            Nombre = nombre,
            Valor = valor,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas)
        };
    }

    private static string EntradaEstudiantesEstructuras(
        IEnumerable<EstudianteEstructuras> estudiantes) {
        EstudianteEstructuras[] elementos = estudiantes.ToArray();
        return elementos.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                elementos.SelectMany(estudiante => new[] {
                    estudiante.Id.ToString(CultureInfo.InvariantCulture),
                    estudiante.Nombre,
                    FormatearNumeroEstructuras(estudiante.Promedio)
                })) +
            "\n";
    }

    private static string SalidaEstudiantesEstructuras(
        IEnumerable<EstudianteEstructuras> estudiantes) {
        return string.Join(
            "\n",
            estudiantes.Select(SalidaRegistroEstudianteEstructuras));
    }

    private static string SalidaRegistroEstudianteEstructuras(
        EstudianteEstructuras estudiante) {
        List<string> lineas = new() {
            $"ID: {estudiante.Id}",
            $"Nombre: {estudiante.Nombre}"
        };

        if (estudiante.Edad.HasValue) {
            lineas.Add($"Edad: {estudiante.Edad.Value}");
        }

        lineas.Add(
            $"Promedio: {FormatearNumeroEstructuras(estudiante.Promedio)}");
        return string.Join("\n", lineas);
    }

    private static string EntradaProductosEstructuras(
        IEnumerable<ProductoEstructuras> productos) {
        ProductoEstructuras[] elementos = productos.ToArray();
        return elementos.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                elementos.SelectMany(producto => new[] {
                    producto.Codigo.ToString(CultureInfo.InvariantCulture),
                    producto.Nombre,
                    FormatearNumeroEstructuras(producto.Precio),
                    producto.Cantidad.ToString(CultureInfo.InvariantCulture)
                })) +
            "\n";
    }

    private static string SalidaProductosEstructuras(
        IEnumerable<ProductoEstructuras> productos,
        double totalInventario) {
        List<string> lineas = productos
            .Select(producto =>
                $"Código: {producto.Codigo}\n" +
                $"Nombre: {producto.Nombre}\n" +
                $"Precio: {FormatearNumeroEstructuras(producto.Precio)}\n" +
                $"Cantidad: {producto.Cantidad}\n" +
                $"Total producto: {FormatearNumeroEstructuras(producto.Total)}")
            .ToList();
        lineas.Add(
            $"Total inventario: {FormatearNumeroEstructuras(totalInventario)}");
        return string.Join("\n", lineas);
    }

    private static string EntradaEmpleadosEstructuras(
        IEnumerable<EmpleadoEstructuras> empleados) {
        EmpleadoEstructuras[] elementos = empleados.ToArray();
        return elementos.Length.ToString(CultureInfo.InvariantCulture) + "\n" +
            string.Join(
                "\n",
                elementos.SelectMany(empleado => new[] {
                    empleado.Id.ToString(CultureInfo.InvariantCulture),
                    empleado.Nombre,
                    FormatearNumeroEstructuras(empleado.Salario),
                    empleado.Activo ? "1" : "0"
                })) +
            "\n";
    }

    private static string SalidaEmpleadosEstructuras(
        IEnumerable<EmpleadoEstructuras> empleados,
        int cantidadActivos,
        double promedioActivos) {
        List<string> lineas = empleados
            .Select(empleado =>
                $"ID: {empleado.Id}\n" +
                $"Nombre: {empleado.Nombre}\n" +
                $"Salario: {FormatearNumeroEstructuras(empleado.Salario)}\n" +
                $"Activo: {(empleado.Activo ? "Sí" : "No")}")
            .ToList();
        lineas.Add($"Cantidad activos: {cantidadActivos}");
        lineas.Add(
            $"Promedio activos: {FormatearNumeroEstructuras(promedioActivos)}");
        return string.Join("\n", lineas);
    }

    private static string FormatearNumeroEstructuras(double valor) {
        return valor.ToString("G15", CultureInfo.InvariantCulture);
    }

    private sealed record EstudianteEstructuras(
        int Id,
        string Nombre,
        double Promedio,
        int? Edad = null);

    private sealed record ProductoEstructuras(
        int Codigo,
        string Nombre,
        double Precio,
        int Cantidad) {
        public double Total => Precio * Cantidad;
    }

    private sealed record EmpleadoEstructuras(
        int Id,
        string Nombre,
        double Salario,
        bool Activo);
}
