using EndForge.Models;

namespace EndForge.Services;

public sealed class CatalogoEvaluacionesService {
    public const string DatosPersonalesId = "variables-datos-personales";
    public const string TicketCompraId = "variables-ticket-compra";
    public const string ConversorTemperaturaId = "variables-conversor-temperatura";
    public const string PromedioCalificacionesId = "variables-promedio-calificaciones";
    public const string MiniReciboId = "variables-mini-recibo";
    public const string ClasificarNumeroId = "condicionales-clasificar-numero";
    public const string MayorDeEdadId = "condicionales-mayor-de-edad";
    public const string CalificacionAprobatoriaId =
        "condicionales-calificacion-aprobatoria";

    private const int PuntosCompilacion = 20;
    private const int PuntosCasosPrueba = 60;
    private const int PuntosValidacion = 15;
    private const int PuntosCalidadBasica = 5;
    private const int PuntosPorCaso = 20;
    private const int PuntosTotales = 100;

    private readonly IReadOnlyList<DefinicionEvaluacionPractica> definiciones;
    private readonly IReadOnlyDictionary<string, DefinicionEvaluacionPractica> definicionesPorId;

    public CatalogoEvaluacionesService() {
        definiciones = CrearDefiniciones();
        ValidarDefiniciones(definiciones);
        definicionesPorId = definiciones.ToDictionary(
            definicion => definicion.PracticaId,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<DefinicionEvaluacionPractica> CargarDefiniciones() {
        return definiciones;
    }

    public DefinicionEvaluacionPractica? ObtenerDefinicion(string practicaId) {
        if (string.IsNullOrWhiteSpace(practicaId)) {
            return null;
        }

        return definicionesPorId.TryGetValue(practicaId.Trim(), out DefinicionEvaluacionPractica? definicion)
            ? definicion
            : null;
    }

    public bool EsPracticaEvaluable(string practicaId) {
        return ObtenerDefinicion(practicaId) is not null;
    }

    private static IReadOnlyList<DefinicionEvaluacionPractica> CrearDefiniciones() {
        IReadOnlyList<CriterioEvaluacion> rubrica = CrearRubrica();

        return Array.AsReadOnly(new[] {
            CrearDatosPersonales(rubrica),
            CrearTicketCompra(rubrica),
            CrearConversorTemperatura(rubrica),
            CrearPromedioCalificaciones(rubrica),
            CrearMiniRecibo(rubrica),
            CrearClasificarNumero(rubrica),
            CrearMayorDeEdad(rubrica),
            CrearCalificacionAprobatoria(rubrica)
        });
    }

    private static DefinicionEvaluacionPractica CrearDatosPersonales(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = DatosPersonalesId,
            NombrePractica = "Datos personales",
            Objetivo = "Declarar variables de distintos tipos y mostrar su contenido.",
            Descripcion = "Se comprobará que el programa capture y muestre nombre, edad, estatura y estado de estudiante.",
            ContratoEntrada = "4 líneas: nombre; edad entera; estatura decimal; estado de estudiante (1 = sí, 0 = no).",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Nombre",
                "Edad entera",
                "Estatura decimal",
                "Estado de estudiante: 1 o 0"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar los cuatro datos capturados.",
                "Identificar cada dato con una etiqueta clara.",
                "Representar correctamente el estado de estudiante."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "datos-personales-estudiante",
                    "Datos normales de estudiante",
                    "Ana\n20\n1.65\n1\n",
                    "Nombre: Ana\nEdad: 20\nEstatura: 1.65\nEstudiante: sí",
                    "Comprueba valores habituales y un estado de estudiante afirmativo.",
                    new[] { "nombre", "ana", "edad", "estatura" },
                    new[] {
                        CrearValor("Edad", 20D, 0D),
                        CrearValor("Estatura", 1.65D)
                    },
                    valoresBooleanos: new[] {
                        CrearBooleano(
                            "Estudiante",
                            true,
                            "Es estudiante",
                            "Estado de estudiante",
                            "Alumno",
                            "Es alumno")
                    }),
                CrearCaso(
                    "datos-personales-no-estudiante",
                    "Datos normales de no estudiante",
                    "Luis\n42\n1.80\n0\n",
                    "Nombre: Luis\nEdad: 42\nEstatura: 1.80\nEstudiante: no",
                    "Comprueba otro conjunto de datos y un estado negativo.",
                    new[] { "nombre", "luis", "edad", "estatura" },
                    new[] {
                        CrearValor("Edad", 42D, 0D),
                        CrearValor("Estatura", 1.8D)
                    },
                    valoresBooleanos: new[] {
                        CrearBooleano(
                            "Estudiante",
                            false,
                            "Es estudiante",
                            "Estado de estudiante",
                            "Alumno",
                            "Es alumno")
                    }),
                CrearCaso(
                    "datos-personales-valores-cero",
                    "Valores numéricos límite",
                    "Bebe\n0\n0.50\n0\n",
                    "Nombre: Bebe\nEdad: 0\nEstatura: 0.50\nEstudiante: no",
                    "Comprueba que los valores cero se conserven sin alterar los datos.",
                    new[] { "nombre", "bebe", "edad", "estatura" },
                    new[] {
                        CrearValor("Edad", 0D, 0D),
                        CrearValor("Estatura", 0.5D)
                    },
                    valoresBooleanos: new[] {
                        CrearBooleano(
                            "Estudiante",
                            false,
                            "Es estudiante",
                            "Estado de estudiante",
                            "Alumno",
                            "Es alumno")
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearTicketCompra(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = TicketCompraId,
            NombrePractica = "Ticket de compra",
            Objetivo = "Guardar precio y cantidad para calcular subtotal y total.",
            Descripcion = "Se comprobará que el ticket muestre el producto, sus datos y el importe calculado.",
            ContratoEntrada = "3 líneas: nombre del producto; precio unitario decimal; cantidad entera.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Nombre del producto",
                "Precio unitario decimal",
                "Cantidad entera"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar producto, precio y cantidad.",
                "Calcular subtotal como precio por cantidad.",
                "Mostrar subtotal y total con etiquetas claras."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "ticket-compra-calculo-normal",
                    "Compra normal",
                    "Cuaderno\n25.50\n2\n",
                    "Producto: Cuaderno\nPrecio: 25.50\nCantidad: 2\nSubtotal: 51.00\nTotal: 51.00",
                    "Comprueba una compra habitual con dos unidades.",
                    new[] { "producto", "cuaderno", "precio", "cantidad", "subtotal", "total" },
                    new[] {
                        CrearValor("Precio", 25.5D),
                        CrearValor("Cantidad", 2D, 0D),
                        CrearValor("Subtotal", 51D),
                        CrearValor("Total", 51D)
                    }),
                CrearCaso(
                    "ticket-compra-resultado-decimal",
                    "Importe con decimales",
                    "Cable\n19.99\n3\n",
                    "Producto: Cable\nPrecio: 19.99\nCantidad: 3\nSubtotal: 59.97\nTotal: 59.97",
                    "Comprueba que no se pierda precisión en un importe decimal.",
                    new[] { "producto", "cable", "precio", "cantidad", "subtotal", "total" },
                    new[] {
                        CrearValor("Precio", 19.99D),
                        CrearValor("Cantidad", 3D, 0D),
                        CrearValor("Subtotal", 59.97D),
                        CrearValor("Total", 59.97D)
                    }),
                CrearCaso(
                    "ticket-compra-cantidad-cero",
                    "Cantidad igual a cero",
                    "Muestra\n4.25\n0\n",
                    "Producto: Muestra\nPrecio: 4.25\nCantidad: 0\nSubtotal: 0\nTotal: 0",
                    "Comprueba el límite de una cantidad igual a cero.",
                    new[] { "producto", "muestra", "precio", "cantidad", "subtotal", "total" },
                    new[] {
                        CrearValor("Precio", 4.25D),
                        CrearValor("Cantidad", 0D, 0D),
                        CrearValor("Subtotal", 0D),
                        CrearValor("Total", 0D)
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearConversorTemperatura(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = ConversorTemperaturaId,
            NombrePractica = "Conversor de temperatura",
            Objetivo = "Aplicar fórmulas usando variables decimales.",
            Descripcion = "Se comprobará la conversión de grados Celsius a Fahrenheit mediante F = C × 9 / 5 + 32.",
            ContratoEntrada = "1 línea: temperatura decimal expresada en grados Celsius.",
            CamposEntrada = Array.AsReadOnly(new[] { "Temperatura Celsius decimal" }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar la temperatura original en Celsius.",
                "Mostrar el resultado convertido en Fahrenheit.",
                "Conservar valores decimales y negativos."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "temperatura-positiva",
                    "Temperatura positiva",
                    "25\n",
                    "Celsius: 25\nFahrenheit: 77",
                    "Comprueba una conversión positiva y detecta división entera incorrecta.",
                    new[] { "celsius", "fahrenheit" },
                    new[] {
                        CrearValorFlexible(
                            "Celsius",
                            25D,
                            "Grados Celsius",
                            "Temperatura inicial",
                            "Temperatura de entrada"),
                        CrearValorFlexible(
                            "Fahrenheit",
                            77D,
                            "Grados Fahrenheit",
                            "Temperatura convertida",
                            "Temperatura final")
                    },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Valores),
                CrearCaso(
                    "temperatura-cero",
                    "Punto de congelación",
                    "0\n",
                    "Celsius: 0\nFahrenheit: 32",
                    "Comprueba el punto de referencia de cero grados Celsius.",
                    new[] { "celsius", "fahrenheit" },
                    new[] {
                        CrearValorFlexible(
                            "Celsius",
                            0D,
                            "Grados Celsius",
                            "Temperatura inicial",
                            "Temperatura de entrada"),
                        CrearValorFlexible(
                            "Fahrenheit",
                            32D,
                            "Grados Fahrenheit",
                            "Temperatura convertida",
                            "Temperatura final")
                    },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Valores),
                CrearCaso(
                    "temperatura-menos-cuarenta",
                    "Temperatura negativa límite",
                    "-40\n",
                    "Celsius: -40\nFahrenheit: -40",
                    "Comprueba números negativos en el punto donde ambas escalas coinciden.",
                    new[] { "celsius", "fahrenheit" },
                    new[] {
                        CrearValorFlexible(
                            "Celsius",
                            -40D,
                            "Grados Celsius",
                            "Temperatura inicial",
                            "Temperatura de entrada"),
                        CrearValorFlexible(
                            "Fahrenheit",
                            -40D,
                            "Grados Fahrenheit",
                            "Temperatura convertida",
                            "Temperatura final")
                    },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Valores),
                CrearCaso(
                    "temperatura-ebullicion-oculto",
                    "Temperatura de ebullición",
                    "100\n",
                    "Celsius: 100\nFahrenheit: 212",
                    "Comprueba una conversión adicional sin revelar sus datos antes de evaluar.",
                    Array.Empty<string>(),
                    new[] {
                        CrearValorFlexible(
                            "Celsius",
                            100D,
                            "Grados Celsius",
                            "Temperatura inicial",
                            "Temperatura de entrada"),
                        CrearValorFlexible(
                            "Fahrenheit",
                            212D,
                            "Grados Fahrenheit",
                            "Temperatura convertida",
                            "Temperatura final")
                    },
                    puntos: 15,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Valores)
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearPromedioCalificaciones(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = PromedioCalificacionesId,
            NombrePractica = "Promedio de calificaciones",
            Objetivo = "Guardar varias calificaciones y calcular su promedio.",
            Descripcion = "La evaluación utiliza exactamente tres calificaciones decimales y comprueba su promedio aritmético.",
            ContratoEntrada = "3 líneas: primera, segunda y tercera calificación decimal. Siempre se evalúan exactamente tres valores.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Primera calificación decimal",
                "Segunda calificación decimal",
                "Tercera calificación decimal"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Leer exactamente tres calificaciones.",
                "Sumar los tres valores y dividir el resultado entre tres.",
                "Mostrar el promedio con una etiqueta clara."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "promedio-calificaciones-enteras",
                    "Promedio entero",
                    "8\n9\n10\n",
                    "Promedio: 9",
                    "Comprueba un promedio exacto con calificaciones enteras.",
                    new[] { "promedio" },
                    new[] { CrearValor("Promedio", 9D) }),
                CrearCaso(
                    "promedio-calificaciones-decimales",
                    "Promedio decimal",
                    "6.5\n7.5\n8.5\n",
                    "Promedio: 7.5",
                    "Comprueba que se utilicen variables y operaciones decimales.",
                    new[] { "promedio" },
                    new[] { CrearValor("Promedio", 7.5D) }),
                CrearCaso(
                    "promedio-calificaciones-con-cero",
                    "Promedio con valores cero",
                    "0\n0\n10\n",
                    "Promedio: 3.33",
                    "Comprueba un límite con ceros y un resultado periódico.",
                    new[] { "promedio" },
                    new[] { CrearValor("Promedio", 10D / 3D, 0.01D) })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearMiniRecibo(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = MiniReciboId,
            NombrePractica = "Mini recibo",
            Objetivo = "Combinar texto, números y operaciones.",
            Descripcion = "La evaluación utiliza exactamente dos productos y comprueba sus subtotales y el total general.",
            ContratoEntrada = "7 líneas: cliente; producto 1; precio 1; cantidad 1; producto 2; precio 2; cantidad 2. Siempre se evalúan exactamente dos productos.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Nombre del cliente",
                "Nombre del producto 1",
                "Precio unitario del producto 1",
                "Cantidad del producto 1",
                "Nombre del producto 2",
                "Precio unitario del producto 2",
                "Cantidad del producto 2"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar el cliente y los dos productos.",
                "Mostrar cantidades y precios unitarios.",
                "Calcular un subtotal por producto y el total general."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "mini-recibo-dos-productos",
                    "Recibo con dos productos",
                    "Ana\nPan\n12.50\n2\nLeche\n18.00\n1\n",
                    "Cliente: Ana\nProducto 1: Pan\nPrecio 1: 12.50\nCantidad 1: 2\nSubtotal 1: 25.00\nProducto 2: Leche\nPrecio 2: 18.00\nCantidad 2: 1\nSubtotal 2: 18.00\nTotal: 43.00",
                    "Comprueba un recibo normal con dos subtotales diferentes.",
                    new[] { "cliente", "ana", "pan", "leche", "subtotal", "total" },
                    new[] {
                        CrearValor("Precio 1", 12.5D),
                        CrearValor("Cantidad 1", 2D, 0D),
                        CrearValor("Subtotal 1", 25D),
                        CrearValor("Precio 2", 18D),
                        CrearValor("Cantidad 2", 1D, 0D),
                        CrearValor("Subtotal 2", 18D),
                        CrearValor("Total", 43D)
                    }),
                CrearCaso(
                    "mini-recibo-importes-decimales",
                    "Recibo con importes decimales",
                    "Luis\nLapiz\n4.25\n3\nCuaderno\n25.50\n2\n",
                    "Cliente: Luis\nProducto 1: Lapiz\nPrecio 1: 4.25\nCantidad 1: 3\nSubtotal 1: 12.75\nProducto 2: Cuaderno\nPrecio 2: 25.50\nCantidad 2: 2\nSubtotal 2: 51.00\nTotal: 63.75",
                    "Comprueba precisión decimal en subtotales y total.",
                    new[] { "cliente", "luis", "lapiz", "cuaderno", "subtotal", "total" },
                    new[] {
                        CrearValor("Precio 1", 4.25D),
                        CrearValor("Cantidad 1", 3D, 0D),
                        CrearValor("Subtotal 1", 12.75D),
                        CrearValor("Precio 2", 25.5D),
                        CrearValor("Cantidad 2", 2D, 0D),
                        CrearValor("Subtotal 2", 51D),
                        CrearValor("Total", 63.75D)
                    }),
                CrearCaso(
                    "mini-recibo-valores-cero",
                    "Recibo con importes límite",
                    "Eva\nMuestra\n99.99\n0\nBolsa\n0\n7\n",
                    "Cliente: Eva\nProducto 1: Muestra\nPrecio 1: 99.99\nCantidad 1: 0\nSubtotal 1: 0\nProducto 2: Bolsa\nPrecio 2: 0\nCantidad 2: 7\nSubtotal 2: 0\nTotal: 0",
                    "Comprueba cantidades o precios iguales a cero.",
                    new[] { "cliente", "eva", "muestra", "bolsa", "subtotal", "total" },
                    new[] {
                        CrearValor("Precio 1", 99.99D),
                        CrearValor("Cantidad 1", 0D, 0D),
                        CrearValor("Subtotal 1", 0D),
                        CrearValor("Precio 2", 0D),
                        CrearValor("Cantidad 2", 7D, 0D),
                        CrearValor("Subtotal 2", 0D),
                        CrearValor("Total", 0D)
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearClasificarNumero(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = ClasificarNumeroId,
            NombrePractica = "Número positivo, negativo o cero",
            Objetivo = "Clasificar un número decimal según su relación con cero.",
            Descripcion = "Se comprobará que el programa repita el valor recibido y muestre una sola clasificación válida: positivo, negativo o cero.",
            ContratoEntrada = "1 línea: número decimal. Los valores mayores que cero son positivos, los menores que cero son negativos y el valor cero se clasifica como cero.",
            CamposEntrada = Array.AsReadOnly(new[] { "Número decimal" }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar el número recibido con una etiqueta reconocible.",
                "Clasificar correctamente valores positivos, negativos y cero.",
                "Mostrar una sola clasificación sin resultados contradictorios."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "clasificar-numero-positivo",
                    "Número positivo",
                    "7.5\n",
                    "Número: 7.5\nClasificación: Positivo",
                    "Comprueba la clasificación de un número decimal mayor que cero.",
                    Array.Empty<string>(),
                    new[] { CrearNumeroClasificacion(7.5D) },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] { CrearClasificacionEsperada("Positivo") }),
                CrearCaso(
                    "clasificar-numero-negativo",
                    "Número negativo",
                    "-3.2\n",
                    "Número: -3.2\nClasificación: Negativo",
                    "Comprueba la clasificación de un número decimal menor que cero.",
                    Array.Empty<string>(),
                    new[] { CrearNumeroClasificacion(-3.2D) },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] { CrearClasificacionEsperada("Negativo") }),
                CrearCaso(
                    "clasificar-numero-cero",
                    "Número igual a cero",
                    "0\n",
                    "Número: 0\nClasificación: Cero",
                    "Comprueba que cero no se clasifique como positivo ni negativo.",
                    Array.Empty<string>(),
                    new[] { CrearNumeroClasificacion(0D) },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] { CrearClasificacionEsperada("Cero") }),
                CrearCaso(
                    "clasificar-numero-decimal-negativo-oculto",
                    "Número decimal negativo adicional",
                    "-0.5\n",
                    "Número: -0.5\nClasificación: Negativo",
                    "Comprueba de forma adicional un valor negativo cercano a cero.",
                    Array.Empty<string>(),
                    new[] { CrearNumeroClasificacion(-0.5D) },
                    puntos: 15,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] { CrearClasificacionEsperada("Negativo") })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearMayorDeEdad(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = MayorDeEdadId,
            NombrePractica = "Mayor de edad",
            Objetivo = "Clasificar una edad válida y detectar valores fuera del rango permitido.",
            Descripcion = "Se comprobará que el programa repita la edad recibida y la clasifique como mayor de edad, menor de edad o edad inválida.",
            ContratoEntrada = "1 línea: edad entera. El rango válido es de 0 a 120; desde 18 se considera mayor de edad y los valores fuera del rango son inválidos.",
            CamposEntrada = Array.AsReadOnly(new[] { "Edad entera" }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar la edad recibida con una etiqueta reconocible.",
                "Clasificar las edades válidas usando el límite de 18 años.",
                "Rechazar edades negativas o mayores que 120.",
                "Mostrar una sola clasificación sin resultados contradictorios."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "mayor-edad-limite-adulto",
                    "Límite de mayoría de edad",
                    "18\n",
                    "Edad: 18\nClasificación: Mayor de edad",
                    "Comprueba que el límite de 18 años se clasifique como mayor de edad.",
                    Array.Empty<string>(),
                    new[] { CrearEdadEsperada(18) },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionEdadEsperada("Mayor de edad")
                    }),
                CrearCaso(
                    "mayor-edad-menor",
                    "Edad menor al límite",
                    "17\n",
                    "Edad: 17\nClasificación: Menor de edad",
                    "Comprueba una edad válida inmediatamente menor que 18.",
                    Array.Empty<string>(),
                    new[] { CrearEdadEsperada(17) },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionEdadEsperada(
                            "Menor de edad",
                            permitirAdolescente: true)
                    }),
                CrearCaso(
                    "mayor-edad-negativa",
                    "Edad negativa inválida",
                    "-1\n",
                    "Edad: -1\nClasificación: Edad inválida",
                    "Comprueba que una edad negativa se rechace como inválida.",
                    Array.Empty<string>(),
                    new[] { CrearEdadEsperada(-1) },
                    puntos: 15,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionEdadEsperada("Edad inválida")
                    }),
                CrearCaso(
                    "mayor-edad-superior-oculto",
                    "Edad superior al rango",
                    "121\n",
                    "Edad: 121\nClasificación: Edad inválida",
                    "Comprueba de forma adicional una edad superior a 120.",
                    Array.Empty<string>(),
                    new[] { CrearEdadEsperada(121) },
                    puntos: 15,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionEdadEsperada("Edad inválida")
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearCalificacionAprobatoria(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = CalificacionAprobatoriaId,
            NombrePractica = "Calificación aprobatoria",
            Objetivo = "Clasificar una calificación decimal dentro de la escala de 0 a 10.",
            Descripcion = "Se comprobará que el programa repita la calificación recibida y la clasifique como reprobatoria, suficiente, buena, excelente o inválida.",
            ContratoEntrada = "1 línea: calificación decimal. La escala válida es de 0 a 10: menos de 6 es reprobatoria; de 6 a menos de 8, suficiente; de 8 a menos de 9, buena; y de 9 a 10, excelente.",
            CamposEntrada = Array.AsReadOnly(new[] { "Calificación decimal" }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar la calificación recibida con una etiqueta reconocible.",
                "Aplicar correctamente los límites 6, 8 y 9.",
                "Rechazar calificaciones menores que 0 o mayores que 10.",
                "Mostrar una sola clasificación sin resultados contradictorios."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "calificacion-reprobatoria",
                    "Calificación reprobatoria",
                    "5.5\n",
                    "Calificación: 5.5\nClasificación: Reprobatoria",
                    "Comprueba una calificación decimal menor que 6.",
                    Array.Empty<string>(),
                    new[] { CrearCalificacionEsperada(5.5D) },
                    puntos: 12,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionCalificacionEsperada("Reprobatoria")
                    }),
                CrearCaso(
                    "calificacion-suficiente",
                    "Límite de calificación suficiente",
                    "6\n",
                    "Calificación: 6\nClasificación: Suficiente",
                    "Comprueba que el límite de 6 se clasifique como suficiente.",
                    Array.Empty<string>(),
                    new[] { CrearCalificacionEsperada(6D) },
                    puntos: 12,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionCalificacionEsperada("Suficiente")
                    }),
                CrearCaso(
                    "calificacion-buena",
                    "Calificación buena",
                    "8.5\n",
                    "Calificación: 8.5\nClasificación: Buena",
                    "Comprueba una calificación decimal entre 8 y 9.",
                    Array.Empty<string>(),
                    new[] { CrearCalificacionEsperada(8.5D) },
                    puntos: 12,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionCalificacionEsperada("Buena")
                    }),
                CrearCaso(
                    "calificacion-excelente",
                    "Límite de calificación excelente",
                    "9\n",
                    "Calificación: 9\nClasificación: Excelente",
                    "Comprueba que el límite de 9 se clasifique como excelente.",
                    Array.Empty<string>(),
                    new[] { CrearCalificacionEsperada(9D) },
                    puntos: 12,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionCalificacionEsperada("Excelente")
                    }),
                CrearCaso(
                    "calificacion-superior-oculta",
                    "Calificación superior a la escala",
                    "10.5\n",
                    "Calificación: 10.5\nClasificación: Calificación inválida",
                    "Comprueba de forma adicional una calificación mayor que 10.",
                    Array.Empty<string>(),
                    new[] { CrearCalificacionEsperada(10.5D) },
                    puntos: 12,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearClasificacionCalificacionEsperada(
                            "Calificación inválida")
                    })
            }),
            Criterios = rubrica
        };
    }

    private static IReadOnlyList<CriterioEvaluacion> CrearRubrica() {
        return Array.AsReadOnly(new[] {
            new CriterioEvaluacion {
                Id = "compilacion",
                Nombre = "Compilación",
                Descripcion = "El proyecto compila correctamente y genera el ejecutable esperado.",
                PuntosMaximos = PuntosCompilacion,
                Tipo = TipoCriterioEvaluacion.Compilacion
            },
            new CriterioEvaluacion {
                Id = "casos-prueba",
                Nombre = "Casos de prueba",
                Descripcion = "El programa produce resultados correctos para los casos de prueba configurados.",
                PuntosMaximos = PuntosCasosPrueba,
                Tipo = TipoCriterioEvaluacion.CasoPrueba
            },
            new CriterioEvaluacion {
                Id = "validacion-comportamiento",
                Nombre = "Validaciones y comportamiento requerido",
                Descripcion = "La salida contiene todos los datos y cálculos requeridos por la práctica.",
                PuntosMaximos = PuntosValidacion,
                Tipo = TipoCriterioEvaluacion.Validacion
            },
            new CriterioEvaluacion {
                Id = "claridad-basica",
                Nombre = "Claridad básica del resultado",
                Descripcion = "La salida es legible e identifica sus valores con etiquetas claras.",
                PuntosMaximos = PuntosCalidadBasica,
                Tipo = TipoCriterioEvaluacion.CalidadBasica
            }
        });
    }

    private static CasoPrueba CrearCaso(
        string id,
        string nombre,
        string entrada,
        string salidaEsperada,
        string descripcion,
        string[] tokensObligatorios,
        ValorNumericoEsperado[] valoresNumericos,
        GrupoTokensEsperados[]? gruposAlternativos = null,
        int puntos = PuntosPorCaso,
        bool esVisible = true,
        ModoComparacionCaso modoComparacion = ModoComparacionCaso.Mixto,
        ValorBooleanoEsperado[]? valoresBooleanos = null,
        ValorTextualEsperado[]? valoresTextuales = null) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = entrada,
            SalidaEsperada = salidaEsperada,
            EsVisible = esVisible,
            Puntos = puntos,
            ComparacionFlexible = true,
            ModoComparacion = modoComparacion,
            Descripcion = descripcion,
            TokensObligatorios = Array.AsReadOnly(tokensObligatorios),
            GruposTokensAlternativos = Array.AsReadOnly(
                gruposAlternativos ?? Array.Empty<GrupoTokensEsperados>()),
            ValoresNumericosEsperados = Array.AsReadOnly(valoresNumericos),
            ValoresBooleanosEsperados = Array.AsReadOnly(
                valoresBooleanos ?? Array.Empty<ValorBooleanoEsperado>()),
            ValoresTextualesEsperados = Array.AsReadOnly(
                valoresTextuales ?? Array.Empty<ValorTextualEsperado>())
        };
    }

    private static GrupoTokensEsperados CrearGrupo(
        string nombre,
        params string[] alternativas) {
        return new GrupoTokensEsperados {
            Nombre = nombre,
            Alternativas = Array.AsReadOnly(alternativas),
            EtiquetasAsociadas = Array.AsReadOnly(new[] {
                nombre,
                "estudiante"
            })
        };
    }

    private static ValorNumericoEsperado CrearValor(
        string nombre,
        double valor,
        double tolerancia = 0.01D) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            Valor = valor,
            Tolerancia = tolerancia
        };
    }

    private static ValorNumericoEsperado CrearValorFlexible(
        string nombre,
        double valor,
        params string[] etiquetasAlternativas) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            Valor = valor,
            Tolerancia = 0.01D,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetasAlternativas)
        };
    }

    private static ValorBooleanoEsperado CrearBooleano(
        string nombre,
        bool valor,
        params string[] etiquetasAlternativas) {
        return new ValorBooleanoEsperado {
            Nombre = nombre,
            Valor = valor,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetasAlternativas)
        };
    }

    private static ValorNumericoEsperado CrearNumeroClasificacion(double valor) {
        return CrearValorFlexible(
            "Número",
            valor,
            "Numero",
            "Valor",
            "Valor ingresado",
            "Dato");
    }

    private static ValorTextualEsperado CrearClasificacionEsperada(
        string valorEsperado) {
        return new ValorTextualEsperado {
            Nombre = "Clasificación",
            Valor = valorEsperado,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Clasificacion",
                "Resultado",
                "Tipo",
                "Signo"
            }),
            Opciones = Array.AsReadOnly(new[] {
                CrearOpcionTextual(
                    "Positivo",
                    "positivo",
                    "número positivo",
                    "numero positivo",
                    "mayor que cero",
                    "mayor a cero"),
                CrearOpcionTextual(
                    "Negativo",
                    "negativo",
                    "número negativo",
                    "numero negativo",
                    "menor que cero",
                    "menor a cero"),
                CrearOpcionTextual(
                    "Cero",
                    "cero",
                    "igual a cero",
                    "es cero",
                    "neutro")
            })
        };
    }

    private static ValorNumericoEsperado CrearEdadEsperada(int edad) {
        return new ValorNumericoEsperado {
            Nombre = "Edad",
            Valor = edad,
            Tolerancia = 0D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Años",
                "Edad ingresada",
                "Valor ingresado"
            })
        };
    }

    private static ValorTextualEsperado CrearClasificacionEdadEsperada(
        string valorEsperado,
        bool permitirAdolescente = false) {
        List<string> alternativasMenor = new() {
            "menor de edad",
            "no es mayor de edad"
        };

        if (permitirAdolescente) {
            alternativasMenor.Add("adolescente");
        }

        return new ValorTextualEsperado {
            Nombre = "Clasificación",
            Valor = valorEsperado,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Clasificacion",
                "Resultado",
                "Estado",
                "Condición"
            }),
            Opciones = Array.AsReadOnly(new[] {
                CrearOpcionTextual(
                    "Mayor de edad",
                    "mayor de edad",
                    "adulto",
                    "es mayor",
                    "puede considerarse adulto"),
                CrearOpcionTextual(
                    "Menor de edad",
                    alternativasMenor.ToArray()),
                CrearOpcionTextual(
                    "Edad inválida",
                    "edad inválida",
                    "edad no válida",
                    "valor inválido",
                    "fuera de rango",
                    "rango inválido")
            })
        };
    }

    private static ValorNumericoEsperado CrearCalificacionEsperada(
        double calificacion) {
        return new ValorNumericoEsperado {
            Nombre = "Calificación",
            Valor = calificacion,
            Tolerancia = 0D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Calificacion",
                "Nota",
                "Puntaje",
                "Valor ingresado"
            })
        };
    }

    private static ValorTextualEsperado CrearClasificacionCalificacionEsperada(
        string valorEsperado) {
        return new ValorTextualEsperado {
            Nombre = "Clasificación",
            Valor = valorEsperado,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Clasificacion",
                "Resultado",
                "Nivel",
                "Estado"
            }),
            Opciones = Array.AsReadOnly(new[] {
                CrearOpcionTextual(
                    "Reprobatoria",
                    "reprobatoria",
                    "reprobado",
                    "no aprobatoria",
                    "insuficiente"),
                CrearOpcionTextual(
                    "Suficiente",
                    "suficiente",
                    "aprobatoria",
                    "aprobado",
                    "regular"),
                CrearOpcionTextual(
                    "Buena",
                    "buena",
                    "buen desempeño"),
                CrearOpcionTextual(
                    "Excelente",
                    "excelente",
                    "sobresaliente",
                    "muy buena"),
                CrearOpcionTextual(
                    "Calificación inválida",
                    "calificación inválida",
                    "calificación no válida",
                    "nota inválida",
                    "fuera de rango",
                    "valor inválido")
            })
        };
    }

    private static OpcionValorTextual CrearOpcionTextual(
        string valor,
        params string[] alternativas) {
        return new OpcionValorTextual {
            Valor = valor,
            Alternativas = Array.AsReadOnly(alternativas)
        };
    }

    private static void ValidarDefiniciones(
        IReadOnlyList<DefinicionEvaluacionPractica> definiciones) {
        if (definiciones.Count == 0 ||
            definiciones.Select(definicion => definicion.PracticaId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != definiciones.Count) {
            throw new InvalidOperationException(
                "El catálogo debe contener prácticas evaluables con identificadores únicos.");
        }

        foreach (DefinicionEvaluacionPractica definicion in definiciones) {
            bool casosInvalidos = definicion.CasosPrueba.Count == 0 ||
                definicion.CasosPrueba
                    .Select(caso => caso.Id)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count() != definicion.CasosPrueba.Count ||
                definicion.CasosPrueba.Any(caso =>
                    string.IsNullOrWhiteSpace(caso.Id) ||
                    caso.Puntos <= 0 ||
                    !TieneReglasAplicables(caso)) ||
                definicion.CasosPrueba.Sum(caso => caso.Puntos) != PuntosCasosPrueba;

            if (casosInvalidos ||
                definicion.PuntosCasosPrueba != PuntosCasosPrueba ||
                definicion.PuntosMaximos != PuntosTotales) {
                throw new InvalidOperationException(
                    $"La definición de {definicion.PracticaId} no cumple la rúbrica de evaluación.");
            }
        }
    }

    private static bool TieneReglasAplicables(CasoPrueba caso) {
        bool tieneTexto = caso.TokensObligatorios.Count > 0 ||
            caso.GruposTokensAlternativos.Count > 0 ||
            caso.ValoresTextualesEsperados.Count > 0;
        bool tieneValores = caso.ValoresNumericosEsperados.Count > 0 ||
            caso.ValoresBooleanosEsperados.Count > 0;

        return caso.ModoComparacion switch {
            ModoComparacionCaso.Texto => tieneTexto,
            ModoComparacionCaso.Valores => tieneValores,
            _ => tieneTexto || tieneValores
        };
    }
}
