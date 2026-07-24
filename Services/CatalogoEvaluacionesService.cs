using EndForge.Models;
using System.Globalization;

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
    public const string DescuentoCompraId = "condicionales-descuento-compra";
    public const string MenuOperacionesId = "condicionales-menu-operaciones";
    public const string ContarUnoADiezId = "ciclos-contar-uno-a-diez";
    public const string TablaMultiplicarId = "ciclos-tabla-multiplicar";
    public const string SumaAcumuladaId = "ciclos-suma-acumulada";
    public const string AdivinaNumeroId = "ciclos-adivina-numero";
    public const string MenuRepetitivoId = "ciclos-menu-repetitivo";

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
            CrearCalificacionAprobatoria(rubrica),
            CrearDescuentoCompra(rubrica),
            CrearMenuOperaciones(rubrica),
            CrearContarUnoADiez(rubrica),
            CrearTablaMultiplicar(rubrica),
            CrearSumaAcumulada(rubrica),
            CrearAdivinaNumero(rubrica),
            CrearMenuRepetitivo(rubrica)
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

    private static DefinicionEvaluacionPractica CrearDescuentoCompra(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = DescuentoCompraId,
            NombrePractica = "Descuento según total de compra",
            Objetivo = "Aplicar un porcentaje de descuento según el total original de una compra.",
            Descripcion = "Se comprobarán el total original, el porcentaje aplicado, el importe descontado y el total final; los totales negativos deben rechazarse.",
            ContratoEntrada = "1 línea: total decimal de la compra. Menos de 500 aplica 0 %; desde 500, 5 %; desde 1000, 10 %; y desde 2000, 15 %. Un total negativo es inválido.",
            CamposEntrada = Array.AsReadOnly(new[] { "Total decimal de la compra" }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar el total original con una etiqueta inequívoca.",
                "Mostrar el porcentaje aplicado, el descuento y el total final.",
                "Calcular el descuento multiplicando el total original por la tasa correspondiente.",
                "Rechazar totales negativos sin exigir cálculos de descuento."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCasoDescuento(
                    "descuento-sin-descuento",
                    "Compra sin descuento",
                    400D,
                    0D,
                    0D,
                    400D,
                    esVisible: true),
                CrearCasoDescuento(
                    "descuento-cinco-por-ciento",
                    "Descuento del cinco por ciento",
                    500D,
                    5D,
                    25D,
                    475D,
                    esVisible: true),
                CrearCasoDescuento(
                    "descuento-diez-por-ciento",
                    "Descuento del diez por ciento",
                    1500D,
                    10D,
                    150D,
                    1350D,
                    esVisible: true),
                CrearCasoDescuento(
                    "descuento-quince-por-ciento",
                    "Descuento del quince por ciento",
                    2000D,
                    15D,
                    300D,
                    1700D,
                    esVisible: true),
                CrearCaso(
                    "descuento-total-negativo-oculto",
                    "Total negativo inválido",
                    "-100\n",
                    "Total de compra: -100\nEstado: Total inválido",
                    "Comprueba de forma adicional que un total negativo se rechace.",
                    Array.Empty<string>(),
                    new[] { CrearTotalOriginalEsperado(-100D) },
                    puntos: 12,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearEstadoTotalEsperado(
                            invalido: true,
                            opcional: false)
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearMenuOperaciones(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = MenuOperacionesId,
            NombrePractica = "Menú de operaciones",
            Objetivo = "Ejecutar una operación aritmética seleccionada en un menú de una sola ejecución.",
            Descripcion = "Se comprobarán suma, resta, multiplicación, división, división entre cero y una opción fuera del menú.",
            ContratoEntrada = "La primera línea contiene la opción. Las opciones 1 a 4 reciben después dos operandos decimales; cualquier otra opción debe rechazarse sin realizar una operación.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Opción entera",
                "Primer operando decimal cuando la opción es válida",
                "Segundo operando decimal cuando la opción es válida"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar el resultado correcto para las cuatro operaciones.",
                "Rechazar la división cuando el segundo operando es cero.",
                "Rechazar opciones distintas de 1, 2, 3 y 4.",
                "No mostrar un resultado numérico en los casos de error."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCasoOperacionNumerica(
                    "menu-operaciones-suma",
                    "Suma decimal",
                    1,
                    5.5D,
                    2D,
                    7.5D,
                    "Suma",
                    esVisible: true),
                CrearCasoOperacionNumerica(
                    "menu-operaciones-resta",
                    "Resta decimal",
                    2,
                    10D,
                    3.5D,
                    6.5D,
                    "Resta",
                    esVisible: true),
                CrearCasoOperacionNumerica(
                    "menu-operaciones-multiplicacion",
                    "Multiplicación con signo",
                    3,
                    -4D,
                    2.5D,
                    -10D,
                    "Multiplicación",
                    esVisible: true),
                CrearCasoOperacionNumerica(
                    "menu-operaciones-division",
                    "División decimal",
                    4,
                    9D,
                    2D,
                    4.5D,
                    "División",
                    esVisible: true),
                CrearCaso(
                    "menu-operaciones-division-cero",
                    "División entre cero",
                    "4\n9\n0\n",
                    "Operación: División\nNo se puede dividir entre cero",
                    "Comprueba que no se muestre un resultado numérico cuando el divisor es cero.",
                    Array.Empty<string>(),
                    new[] {
                        CrearOpcionMenuEsperada(4, opcional: true),
                        CrearResultadoOperacionProhibido()
                    },
                    gruposAlternativos: new[] {
                        CrearGrupoTextoLibre(
                            "Error de división entre cero",
                            "no se puede dividir entre cero",
                            "división entre cero",
                            "division entre cero",
                            "divisor inválido",
                            "divisor igual a cero",
                            "operación inválida por división entre cero")
                    },
                    puntos: 10,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearOperacionMenuEsperada(
                            "División",
                            opcional: true)
                    }),
                CrearCaso(
                    "menu-operaciones-opcion-invalida-oculta",
                    "Opción fuera del menú",
                    "9\n",
                    "Opción: 9\nEstado: Opción inválida",
                    "Comprueba de forma adicional una opción que no pertenece al menú.",
                    Array.Empty<string>(),
                    new[] {
                        CrearOpcionMenuEsperada(9, opcional: true),
                        CrearResultadoOperacionProhibido()
                    },
                    gruposAlternativos: new[] {
                        CrearGrupoTextoLibre(
                            "Opción inválida",
                            "opción inválida",
                            "opcion invalida",
                            "operación inválida",
                            "operacion invalida",
                            "selección inválida",
                            "seleccion invalida",
                            "opción no válida",
                            "fuera del menú")
                    },
                    puntos: 10,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Mixto,
                    valoresTextuales: new[] {
                        CrearOperacionMenuEsperada(
                            "Opción inválida",
                            opcional: true)
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearContarUnoADiez(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = ContarUnoADiezId,
            NombrePractica = "Contar del 1 al 10",
            Objetivo = "Mostrar los números del 1 al 10 en orden mediante una estructura repetitiva.",
            Descripcion = "Se comprobarán el orden, la cantidad exacta, los duplicados y cualquier número adicional.",
            ContratoEntrada = "Esta práctica no necesita entrada. La salida debe contener exactamente los diez números del 1 al 10.",
            CamposEntrada = Array.Empty<string>(),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mostrar los números del 1 al 10 en orden ascendente.",
                "Mostrar exactamente diez valores numéricos.",
                "No repetir valores.",
                "No incluir 0, 11 ni ningún otro número adicional."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCaso(
                    "contar-uno-diez-secuencia-visible",
                    "Secuencia ordenada",
                    "",
                    "1 2 3 4 5 6 7 8 9 10",
                    "Comprueba que aparezcan exactamente los números del 1 al 10 en orden.",
                    Array.Empty<string>(),
                    Array.Empty<ValorNumericoEsperado>(),
                    puntos: 30,
                    modoComparacion: ModoComparacionCaso.Secuencia,
                    secuencias: new[] {
                        CrearSecuenciaConteoUnoADiez()
                    }),
                CrearCaso(
                    "contar-uno-diez-limites-oculto",
                    "Límites exactos de la secuencia",
                    "",
                    "1 2 3 4 5 6 7 8 9 10",
                    "Comprueba de forma adicional que no existan límites, duplicados o repeticiones inesperadas.",
                    Array.Empty<string>(),
                    Array.Empty<ValorNumericoEsperado>(),
                    puntos: 30,
                    esVisible: false,
                    modoComparacion: ModoComparacionCaso.Secuencia,
                    secuencias: new[] {
                        CrearSecuenciaConteoUnoADiez()
                    })
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearTablaMultiplicar(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = TablaMultiplicarId,
            NombrePractica = "Tabla de multiplicar",
            Objetivo = "Mostrar la tabla de un número entero desde el multiplicador 1 hasta el 10.",
            Descripcion = "Se comprobarán diez filas con base, multiplicador y producto relacionados correctamente.",
            ContratoEntrada = "La entrada contiene un número entero. Cada operación debe aparecer en una fila independiente.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Número base entero"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Mantener la misma base en las diez filas.",
                "Recorrer los multiplicadores del 1 al 10 en orden.",
                "Mostrar el producto correcto en cada fila.",
                "No omitir, duplicar ni agregar filas."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCasoTablaMultiplicar(
                    "tabla-multiplicar-cinco",
                    "Tabla de un número positivo",
                    5,
                    esVisible: true),
                CrearCasoTablaMultiplicar(
                    "tabla-multiplicar-cero",
                    "Tabla de cero",
                    0,
                    esVisible: true),
                CrearCasoTablaMultiplicar(
                    "tabla-multiplicar-negativa",
                    "Tabla de un número negativo",
                    -3,
                    esVisible: true),
                CrearCasoTablaMultiplicar(
                    "tabla-multiplicar-siete-oculta",
                    "Tabla adicional",
                    7,
                    esVisible: false)
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearSumaAcumulada(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = SumaAcumuladaId,
            NombrePractica = "Suma acumulada",
            Objetivo = "Leer una cantidad válida de valores decimales y mostrar su suma total.",
            Descripcion = "Se comprobará la suma de todos los valores indicados y el rechazo de cantidades fuera del rango de 0 a 100.",
            ContratoEntrada = "La primera línea contiene una cantidad entera. Si está entre 0 y 100, siguen exactamente esa cantidad de valores decimales.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Cantidad entera de valores",
                "Valores decimales que deben acumularse"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Aceptar cantidades desde 0 hasta 100.",
                "Leer exactamente la cantidad indicada de valores.",
                "Iniciar la suma en cero y acumular únicamente los valores leídos.",
                "Rechazar cantidades negativas o mayores que 100 sin calcular una suma."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCasoSumaAcumuladaValida(
                    "suma-acumulada-decimales",
                    "Suma de tres valores",
                    "3\n5.5\n2\n-1\n",
                    3,
                    6.5D,
                    esVisible: true),
                CrearCasoSumaAcumuladaValida(
                    "suma-acumulada-cero-valores",
                    "Cantidad igual a cero",
                    "0\n",
                    0,
                    0D,
                    esVisible: true),
                CrearCasoSumaAcumuladaValida(
                    "suma-acumulada-mixta",
                    "Suma con valores positivos, negativos y cero",
                    "4\n-2\n3.5\n0\n1.5\n",
                    4,
                    3D,
                    esVisible: true),
                CrearCasoSumaAcumuladaInvalida(
                    "suma-acumulada-cantidad-negativa",
                    "Cantidad negativa",
                    -1,
                    esVisible: true),
                CrearCasoSumaAcumuladaInvalida(
                    "suma-acumulada-cantidad-mayor-cien-oculta",
                    "Cantidad mayor que cien",
                    101,
                    esVisible: false)
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearAdivinaNumero(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = AdivinaNumeroId,
            NombrePractica = "Adivina el número",
            Objetivo = "Guiar cada intento hasta encontrar el número secreto 7 y mostrar cuántos intentos se realizaron.",
            Descripcion = "Se comprobarán las pistas, los intentos inválidos, el acierto, el orden de los eventos y el total final.",
            ContratoEntrada = "Cada línea contiene un intento entero. Los valores válidos están entre 1 y 10; el programa termina al recibir el número secreto 7.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Secuencia de intentos enteros"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Indicar que el número secreto es mayor cuando el intento es menor que 7.",
                "Indicar que el número secreto es menor cuando el intento es mayor que 7.",
                "Contar los intentos fuera del rango de 1 a 10 y mostrarlos como inválidos.",
                "Contar el intento ganador, mostrar el total y terminar sin procesar entradas posteriores."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCasoAdivinaNumero(
                    "adivina-numero-pistas-opuestas",
                    "Pistas mayor y menor",
                    "3\n9\n7\n",
                    3,
                    true,
                    "El número secreto es mayor",
                    "El número secreto es menor",
                    "Correcto"),
                CrearCasoAdivinaNumero(
                    "adivina-numero-primer-intento",
                    "Acierto en el primer intento",
                    "7\n",
                    1,
                    true,
                    "Correcto"),
                CrearCasoAdivinaNumero(
                    "adivina-numero-intento-cero",
                    "Intento inválido antes de acertar",
                    "0\n5\n7\n",
                    3,
                    true,
                    "Intento inválido",
                    "El número secreto es mayor",
                    "Correcto"),
                CrearCasoAdivinaNumero(
                    "adivina-numero-dos-pistas-menor",
                    "Dos intentos mayores que el secreto",
                    "10\n8\n7\n",
                    3,
                    true,
                    "El número secreto es menor",
                    "El número secreto es menor",
                    "Correcto"),
                CrearCasoAdivinaNumero(
                    "adivina-numero-entrada-posterior-oculta",
                    "Terminación después del acierto",
                    "2\n11\n6\n7\n9\n",
                    4,
                    false,
                    "El número secreto es mayor",
                    "Intento inválido",
                    "El número secreto es mayor",
                    "Correcto")
            }),
            Criterios = rubrica
        };
    }

    private static DefinicionEvaluacionPractica CrearMenuRepetitivo(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return new DefinicionEvaluacionPractica {
            PracticaId = MenuRepetitivoId,
            NombrePractica = "Menú repetitivo",
            Objetivo = "Procesar opciones de un menú hasta seleccionar Salir.",
            Descripcion = "Se comprobarán las acciones en el orden solicitado, las opciones inválidas y la terminación después de la despedida.",
            ContratoEntrada = "Cada línea contiene una opción numérica. Las opciones 1, 2 y 3 ejecutan una acción; la opción 4 muestra una despedida y termina el programa.",
            CamposEntrada = Array.AsReadOnly(new[] {
                "Secuencia de opciones numéricas"
            }),
            ValidacionesRequeridas = Array.AsReadOnly(new[] {
                "Responder con un saludo para la opción 1.",
                "Mostrar un mensaje motivador reconocido para la opción 2.",
                "Mostrar el valor 10 con una etiqueta inequívoca para la opción 3.",
                "Rechazar opciones fuera del menú.",
                "Mostrar una despedida y no procesar opciones posteriores a la opción 4."
            }),
            CasosPrueba = Array.AsReadOnly(new[] {
                CrearCasoMenuRepetitivo(
                    "menu-repetitivo-saludo",
                    "Saludo y salida",
                    "1\n4\n",
                    true,
                    "Hola",
                    "Hasta luego"),
                CrearCasoMenuRepetitivo(
                    "menu-repetitivo-motivacion-numero",
                    "Motivación, número y salida",
                    "2\n3\n4\n",
                    true,
                    "Sigue adelante",
                    "Número 10",
                    "Hasta luego"),
                CrearCasoMenuRepetitivo(
                    "menu-repetitivo-opcion-invalida",
                    "Opción inválida y salida",
                    "9\n4\n",
                    true,
                    "Opción inválida",
                    "Hasta luego"),
                CrearCasoMenuRepetitivo(
                    "menu-repetitivo-varias-acciones",
                    "Varias acciones antes de salir",
                    "1\n3\n2\n4\n",
                    true,
                    "Hola",
                    "Número 10",
                    "Sigue adelante",
                    "Hasta luego"),
                CrearCasoMenuRepetitivo(
                    "menu-repetitivo-entrada-posterior-oculta",
                    "Terminación después de salir",
                    "3\n9\n4\n1\n",
                    false,
                    "Número 10",
                    "Opción inválida",
                    "Hasta luego")
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
        ValorTextualEsperado[]? valoresTextuales = null,
        SecuenciaEsperada[]? secuencias = null,
        SecuenciaCompuestaEsperada[]? secuenciasCompuestas = null) {
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
                valoresTextuales ?? Array.Empty<ValorTextualEsperado>()),
            SecuenciasEsperadas = Array.AsReadOnly(
                secuencias ?? Array.Empty<SecuenciaEsperada>()),
            SecuenciasCompuestasEsperadas = Array.AsReadOnly(
                secuenciasCompuestas ?? Array.Empty<SecuenciaCompuestaEsperada>())
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

    private static CasoPrueba CrearCasoDescuento(
        string id,
        string nombre,
        double totalOriginal,
        double porcentaje,
        double descuento,
        double totalFinal,
        bool esVisible) {
        string totalTexto = FormatearNumeroCatalogo(totalOriginal);
        string porcentajeTexto = FormatearNumeroCatalogo(porcentaje);
        string descuentoTexto = FormatearNumeroCatalogo(descuento);
        string totalFinalTexto = FormatearNumeroCatalogo(totalFinal);

        return CrearCaso(
            id,
            nombre,
            totalTexto + "\n",
            $"Total original: {totalTexto}\n" +
            $"Porcentaje: {porcentajeTexto}%\n" +
            $"Descuento: {descuentoTexto}\n" +
            $"Total final: {totalFinalTexto}",
            "Comprueba el porcentaje, el importe descontado y el total final para este rango de compra.",
            Array.Empty<string>(),
            new[] {
                CrearTotalOriginalEsperado(totalOriginal),
                CrearPorcentajeDescuentoEsperado(porcentaje),
                CrearImporteDescuentoEsperado(descuento),
                CrearTotalFinalEsperado(totalFinal)
            },
            puntos: 12,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Mixto,
            valoresTextuales: new[] {
                CrearEstadoTotalEsperado(
                    invalido: false,
                    opcional: true)
            });
    }

    private static ValorNumericoEsperado CrearTotalOriginalEsperado(double total) {
        return new ValorNumericoEsperado {
            Nombre = "Total original",
            Valor = total,
            Tolerancia = 0.01D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Total de compra",
                "Compra",
                "Importe original",
                "Subtotal"
            })
        };
    }

    private static ValorNumericoEsperado CrearPorcentajeDescuentoEsperado(
        double porcentaje) {
        return new ValorNumericoEsperado {
            Nombre = "Porcentaje",
            Valor = porcentaje,
            Tolerancia = 0.0001D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Descuento aplicado",
                "Porcentaje de descuento",
                "Tasa de descuento"
            }),
            ValoresEquivalentes = Array.AsReadOnly(new[] { porcentaje / 100D })
        };
    }

    private static ValorNumericoEsperado CrearImporteDescuentoEsperado(
        double descuento) {
        return new ValorNumericoEsperado {
            Nombre = "Descuento",
            Valor = descuento,
            Tolerancia = 0.01D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Importe descontado",
                "Cantidad descontada",
                "Ahorro"
            })
        };
    }

    private static ValorNumericoEsperado CrearTotalFinalEsperado(double totalFinal) {
        return new ValorNumericoEsperado {
            Nombre = "Total final",
            Valor = totalFinal,
            Tolerancia = 0.01D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Total a pagar",
                "Importe final",
                "Pago final",
                "Resultado final"
            })
        };
    }

    private static ValorTextualEsperado CrearEstadoTotalEsperado(
        bool invalido,
        bool opcional) {
        return new ValorTextualEsperado {
            Nombre = "Validación del total",
            Valor = invalido ? "Total inválido" : "Total válido",
            EsOpcional = opcional,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Resultado",
                "Estado",
                "Validación",
                "Condición"
            }),
            Opciones = Array.AsReadOnly(new[] {
                CrearOpcionTextual(
                    "Total válido",
                    "total válido",
                    "compra válida",
                    "importe válido"),
                CrearOpcionTextual(
                    "Total inválido",
                    "total inválido",
                    "compra inválida",
                    "importe inválido",
                    "valor negativo",
                    "fuera de rango",
                    "valor no válido")
            })
        };
    }

    private static string FormatearNumeroCatalogo(double valor) {
        return valor.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static CasoPrueba CrearCasoOperacionNumerica(
        string id,
        string nombre,
        int opcion,
        double primerOperando,
        double segundoOperando,
        double resultado,
        string operacion,
        bool esVisible) {
        string opcionTexto = opcion.ToString(CultureInfo.InvariantCulture);
        string primeroTexto = FormatearNumeroCatalogo(primerOperando);
        string segundoTexto = FormatearNumeroCatalogo(segundoOperando);
        string resultadoTexto = FormatearNumeroCatalogo(resultado);

        return CrearCaso(
            id,
            nombre,
            $"{opcionTexto}\n{primeroTexto}\n{segundoTexto}\n",
            $"Operación: {operacion}\nResultado: {resultadoTexto}",
            $"Comprueba el resultado decimal de la operación {operacion.ToLowerInvariant()}.",
            Array.Empty<string>(),
            new[] {
                CrearOpcionMenuEsperada(opcion, opcional: true),
                CrearResultadoOperacionEsperado(resultado)
            },
            puntos: 10,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Mixto,
            valoresTextuales: new[] {
                CrearOperacionMenuEsperada(operacion, opcional: true)
            });
    }

    private static ValorNumericoEsperado CrearOpcionMenuEsperada(
        int opcion,
        bool opcional) {
        return new ValorNumericoEsperado {
            Nombre = "Opción",
            Valor = opcion,
            Tolerancia = 0D,
            EsOpcional = opcional,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Opcion",
                "Operación",
                "Operacion",
                "Selección",
                "Seleccion"
            })
        };
    }

    private static ValorNumericoEsperado CrearResultadoOperacionEsperado(
        double resultado) {
        return new ValorNumericoEsperado {
            Nombre = "Resultado",
            Valor = resultado,
            Tolerancia = 0.01D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Total",
                "Respuesta",
                "Valor obtenido",
                "Resultado final"
            })
        };
    }

    private static ValorNumericoEsperado CrearResultadoOperacionProhibido() {
        return new ValorNumericoEsperado {
            Nombre = "Resultado",
            DebeEstarAusente = true,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Total",
                "Respuesta",
                "Valor obtenido",
                "Resultado final"
            })
        };
    }

    private static ValorTextualEsperado CrearOperacionMenuEsperada(
        string operacion,
        bool opcional) {
        return new ValorTextualEsperado {
            Nombre = "Operación",
            Valor = operacion,
            EsOpcional = opcional,
            EtiquetasAlternativas = Array.AsReadOnly(new[] { "Operacion" }),
            Opciones = Array.AsReadOnly(new[] {
                CrearOpcionTextual(
                    "Suma",
                    "suma",
                    "sumando",
                    "adición",
                    "adicion"),
                CrearOpcionTextual(
                    "Resta",
                    "resta",
                    "sustracción",
                    "sustraccion"),
                CrearOpcionTextual(
                    "Multiplicación",
                    "multiplicación",
                    "multiplicacion",
                    "producto"),
                CrearOpcionTextual(
                    "División",
                    "división",
                    "division",
                    "cociente")
            })
        };
    }

    private static GrupoTokensEsperados CrearGrupoTextoLibre(
        string nombre,
        params string[] alternativas) {
        return new GrupoTokensEsperados {
            Nombre = nombre,
            Alternativas = Array.AsReadOnly(alternativas),
            EtiquetasAsociadas = Array.Empty<string>()
        };
    }

    private static SecuenciaEsperada CrearSecuenciaConteoUnoADiez() {
        return new SecuenciaEsperada {
            Nombre = "Números del 1 al 10",
            Tipo = TipoSecuenciaEsperada.Numerica,
            ValoresNumericosEsperados = Array.AsReadOnly(new[] {
                1D,
                2D,
                3D,
                4D,
                5D,
                6D,
                7D,
                8D,
                9D,
                10D
            }),
            OrdenObligatorio = true,
            CantidadExacta = 10,
            PermitirDuplicados = false,
            PermitirElementosAdicionales = false,
            ToleranciaNumerica = 0D,
            SeparadoresPermitidos = Array.AsReadOnly(new[] {
                " ",
                "\t",
                "\r",
                "\n",
                ",",
                ";"
            }),
            PermitirTextoAdicional = true
        };
    }

    private static CasoPrueba CrearCasoTablaMultiplicar(
        string id,
        string nombre,
        int numeroBase,
        bool esVisible) {
        return CrearCaso(
            id,
            nombre,
            numeroBase.ToString(CultureInfo.InvariantCulture) + "\n",
            CrearSalidaTablaMultiplicar(numeroBase),
            $"Comprueba las diez operaciones de la tabla de {numeroBase}.",
            Array.Empty<string>(),
            Array.Empty<ValorNumericoEsperado>(),
            puntos: 15,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Secuencia,
            secuenciasCompuestas: new[] {
                CrearSecuenciaCompuestaTabla(numeroBase)
            });
    }

    private static string CrearSalidaTablaMultiplicar(int numeroBase) {
        return string.Join(
            Environment.NewLine,
            Enumerable.Range(1, 10).Select(multiplicador =>
                $"{numeroBase} x {multiplicador} = {numeroBase * multiplicador}"));
    }

    private static SecuenciaCompuestaEsperada CrearSecuenciaCompuestaTabla(
        int numeroBase) {
        return new SecuenciaCompuestaEsperada {
            Nombre = $"Tabla de {numeroBase}",
            PasosEsperados = Array.AsReadOnly(
                Enumerable.Range(1, 10)
                    .Select(multiplicador =>
                        CrearPasoTabla(numeroBase, multiplicador))
                    .ToArray()),
            OrdenObligatorio = true,
            CantidadExacta = 10,
            PermitirPasosAdicionales = false,
            PermitirPasosDuplicados = false,
            PermitirTextoAdicional = true,
            RequerirMismaLinea = true,
            SeparadoresTextualesPermitidos = Array.AsReadOnly(new[] {
                "x",
                "×",
                "*",
                "por",
                "="
            })
        };
    }

    private static PasoSecuenciaCompuestaEsperado CrearPasoTabla(
        int numeroBase,
        int multiplicador) {
        return new PasoSecuenciaCompuestaEsperado {
            Nombre = $"{numeroBase} por {multiplicador}",
            Componentes = Array.AsReadOnly(new[] {
                new ComponenteNumericoPasoEsperado {
                    Nombre = "Base",
                    Valor = numeroBase,
                    Tolerancia = 0D,
                    Posicion = 0
                },
                new ComponenteNumericoPasoEsperado {
                    Nombre = "Multiplicador",
                    Valor = multiplicador,
                    Tolerancia = 0D,
                    Posicion = 1,
                    EtiquetasOSeparadoresOpcionales = Array.AsReadOnly(new[] {
                        "x",
                        "×",
                        "*",
                        "por"
                    })
                },
                new ComponenteNumericoPasoEsperado {
                    Nombre = "Resultado",
                    Valor = numeroBase * multiplicador,
                    Tolerancia = 0D,
                    Posicion = 2,
                    EtiquetasOSeparadoresOpcionales = Array.AsReadOnly(new[] {
                        "="
                    })
                }
            })
        };
    }

    private static CasoPrueba CrearCasoSumaAcumuladaValida(
        string id,
        string nombre,
        string entrada,
        int cantidad,
        double suma,
        bool esVisible) {
        return CrearCaso(
            id,
            nombre,
            entrada,
            $"Suma total: {FormatearNumeroCatalogo(suma)}",
            "Comprueba que se acumulen únicamente los valores indicados.",
            Array.Empty<string>(),
            new[] {
                CrearCantidadValoresEsperada(cantidad),
                CrearSumaAcumuladaEsperada(suma)
            },
            puntos: 12,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Mixto,
            valoresTextuales: new[] {
                CrearEstadoCantidadEsperado(
                    invalida: false,
                    opcional: true)
            });
    }

    private static CasoPrueba CrearCasoSumaAcumuladaInvalida(
        string id,
        string nombre,
        int cantidad,
        bool esVisible) {
        return CrearCaso(
            id,
            nombre,
            cantidad.ToString(CultureInfo.InvariantCulture) + "\n",
            "Cantidad inválida",
            "Comprueba que una cantidad fuera del rango de 0 a 100 se rechace sin calcular una suma.",
            Array.Empty<string>(),
            new[] {
                CrearCantidadValoresEsperada(cantidad),
                CrearSumaAcumuladaProhibida()
            },
            puntos: 12,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Mixto,
            valoresTextuales: new[] {
                CrearEstadoCantidadEsperado(
                    invalida: true,
                    opcional: false)
            });
    }

    private static ValorNumericoEsperado CrearCantidadValoresEsperada(
        int cantidad) {
        return new ValorNumericoEsperado {
            Nombre = "Cantidad",
            Valor = cantidad,
            Tolerancia = 0D,
            EsOpcional = true,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Número de valores",
                "Numero de valores",
                "Total de valores",
                "Elementos"
            })
        };
    }

    private static ValorNumericoEsperado CrearSumaAcumuladaEsperada(double suma) {
        return new ValorNumericoEsperado {
            Nombre = "Suma total",
            Valor = suma,
            Tolerancia = 0.01D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Total",
                "Resultado",
                "Acumulado",
                "Suma acumulada"
            })
        };
    }

    private static ValorNumericoEsperado CrearSumaAcumuladaProhibida() {
        return new ValorNumericoEsperado {
            Nombre = "Suma total",
            DebeEstarAusente = true,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Total",
                "Resultado",
                "Acumulado",
                "Suma acumulada"
            })
        };
    }

    private static ValorTextualEsperado CrearEstadoCantidadEsperado(
        bool invalida,
        bool opcional) {
        return new ValorTextualEsperado {
            Nombre = "Resultado",
            Valor = invalida ? "Cantidad inválida" : "Cantidad válida",
            EsOpcional = opcional,
            PermitirSinEtiqueta = true,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Estado",
                "Validación",
                "Validacion",
                "Condición",
                "Condicion"
            }),
            Opciones = Array.AsReadOnly(new[] {
                CrearOpcionTextual(
                    "Cantidad válida",
                    "cantidad válida",
                    "cantidad dentro de rango",
                    "entrada válida"),
                CrearOpcionTextual(
                    "Cantidad inválida",
                    "cantidad inválida",
                    "cantidad no válida",
                    "número de valores inválido",
                    "numero de valores invalido",
                    "fuera de rango",
                    "valor inválido",
                    "entrada inválida")
            })
        };
    }

    private static CasoPrueba CrearCasoAdivinaNumero(
        string id,
        string nombre,
        string entrada,
        int totalIntentos,
        bool esVisible,
        params string[] eventos) {
        string salidaEsperada = string.Join(
            Environment.NewLine,
            eventos.Concat(new[] { $"Intentos: {totalIntentos}" }));

        return CrearCaso(
            id,
            nombre,
            entrada,
            salidaEsperada,
            "Comprueba las respuestas en el orden de los intentos y el total contabilizado hasta el acierto.",
            Array.Empty<string>(),
            new[] {
                CrearTotalIntentosEsperado(totalIntentos)
            },
            puntos: 12,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Mixto,
            secuencias: new[] {
                CrearSecuenciaEventosAdivinaNumero(eventos)
            });
    }

    private static ValorNumericoEsperado CrearTotalIntentosEsperado(
        int totalIntentos) {
        return new ValorNumericoEsperado {
            Nombre = "Intentos",
            Valor = totalIntentos,
            Tolerancia = 0D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Número de intentos",
                "Numero de intentos",
                "Total de intentos",
                "Cantidad de intentos"
            })
        };
    }

    private static SecuenciaEsperada CrearSecuenciaEventosAdivinaNumero(
        IReadOnlyList<string> eventos) {
        return new SecuenciaEsperada {
            Nombre = "Respuestas a los intentos",
            Tipo = TipoSecuenciaEsperada.Textual,
            AlternativasTextualesEsperadas = Array.AsReadOnly(
                eventos.Select(CrearEventoAdivinaNumero).ToArray()),
            OrdenObligatorio = true,
            CantidadExacta = eventos.Count,
            PermitirDuplicados = true,
            PermitirElementosAdicionales = false,
            PermitirTextoAdicional = true,
            RequerirEventosEnLineasIndependientes = true
        };
    }

    private static ElementoTextualSecuenciaEsperado CrearEventoAdivinaNumero(
        string evento) {
        return evento switch {
            "El número secreto es mayor" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    Alternativas = Array.AsReadOnly(new[] {
                        "El numero secreto es mayor",
                        "Es mayor",
                        "Debes intentar un número mayor",
                        "Intenta con un número mayor",
                        "Muy bajo"
                    })
                },
            "El número secreto es menor" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    Alternativas = Array.AsReadOnly(new[] {
                        "El numero secreto es menor",
                        "Es menor",
                        "Debes intentar un número menor",
                        "Intenta con un número menor",
                        "Muy alto"
                    })
                },
            "Intento inválido" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    Alternativas = Array.AsReadOnly(new[] {
                        "Intento invalido",
                        "Número fuera de rango",
                        "Numero fuera de rango",
                        "Valor inválido",
                        "Entrada inválida"
                    })
                },
            "Correcto" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    Alternativas = Array.AsReadOnly(new[] {
                        "Adivinaste",
                        "Número encontrado",
                        "Numero encontrado",
                        "Acertaste",
                        "Es el número secreto",
                        "Es el numero secreto"
                    }),
                    EtiquetasNumericasPosteriores = Array.AsReadOnly(new[] {
                        "Intentos",
                        "Número de intentos",
                        "Numero de intentos",
                        "Total de intentos",
                        "Cantidad de intentos"
                    })
                },
            _ => throw new ArgumentException(
                "El evento del juego no está definido.",
                nameof(evento))
        };
    }

    private static CasoPrueba CrearCasoMenuRepetitivo(
        string id,
        string nombre,
        string entrada,
        bool esVisible,
        params string[] eventos) {
        bool incluyeNumero = eventos.Contains(
            "Número 10",
            StringComparer.OrdinalIgnoreCase);
        ValorNumericoEsperado[] valoresNumericos = incluyeNumero
            ? new[] { CrearNumeroMostradoEsperado() }
            : Array.Empty<ValorNumericoEsperado>();
        string salidaEsperada = string.Join(
            Environment.NewLine,
            eventos.Select(FormatearEventoMenuRepetitivo));

        return CrearCaso(
            id,
            nombre,
            entrada,
            salidaEsperada,
            "Comprueba que cada opción produzca una sola acción y que el programa termine después de la despedida.",
            Array.Empty<string>(),
            valoresNumericos,
            puntos: 12,
            esVisible: esVisible,
            modoComparacion: ModoComparacionCaso.Mixto,
            secuencias: new[] {
                CrearSecuenciaEventosMenuRepetitivo(eventos)
            });
    }

    private static string FormatearEventoMenuRepetitivo(string evento) {
        return evento.Equals(
            "Número 10",
            StringComparison.OrdinalIgnoreCase)
                ? "Número: 10"
                : evento;
    }

    private static ValorNumericoEsperado CrearNumeroMostradoEsperado() {
        return new ValorNumericoEsperado {
            Nombre = "Número",
            Valor = 10D,
            Tolerancia = 0D,
            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                "Numero",
                "Valor",
                "Resultado",
                "Número mostrado",
                "Numero mostrado"
            })
        };
    }

    private static SecuenciaEsperada CrearSecuenciaEventosMenuRepetitivo(
        IReadOnlyList<string> eventos) {
        return new SecuenciaEsperada {
            Nombre = "Acciones del menú",
            Tipo = TipoSecuenciaEsperada.Textual,
            AlternativasTextualesEsperadas = Array.AsReadOnly(
                eventos.Select(CrearEventoMenuRepetitivo).ToArray()),
            EventosTextualesReconocibles = Array.AsReadOnly(new[] {
                CrearEventoMenuRepetitivo("Hola"),
                CrearEventoMenuRepetitivo("Sigue adelante"),
                CrearEventoMenuRepetitivo("Número 10"),
                CrearEventoMenuRepetitivo("Opción inválida"),
                CrearEventoMenuRepetitivo("Hasta luego")
            }),
            OrdenObligatorio = true,
            CantidadExacta = eventos.Count,
            PermitirDuplicados = true,
            PermitirElementosAdicionales = false,
            PermitirTextoAdicional = true,
            RequerirEventosEnLineasIndependientes = true
        };
    }

    private static ElementoTextualSecuenciaEsperado CrearEventoMenuRepetitivo(
        string evento) {
        return evento switch {
            "Hola" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    RequerirTextoAlInicioDeLinea = true,
                    Alternativas = Array.AsReadOnly(new[] {
                        "Bienvenido",
                        "Bienvenida",
                        "Saludos",
                        "Buen día",
                        "Buen dia"
                    })
                },
            "Sigue adelante" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    RequerirTextoAlInicioDeLinea = true,
                    Alternativas = Array.AsReadOnly(new[] {
                        "Tú puedes",
                        "Tu puedes",
                        "Continúa aprendiendo",
                        "Continua aprendiendo",
                        "No te rindas",
                        "Sigue practicando",
                        "Vas muy bien"
                    })
                },
            "Número 10" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    BuscarComoTexto = false,
                    ValorNumericoAsociado = 10D,
                    EtiquetasNumericasAsociadas = Array.AsReadOnly(new[] {
                        "Número",
                        "Numero",
                        "Valor",
                        "Resultado",
                        "Número mostrado",
                        "Numero mostrado"
                    }),
                    RequerirSeparadorEtiquetaNumerica = true,
                    RequerirEtiquetaNumericaAlInicioDeLinea = true
                },
            "Opción inválida" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    RequerirTextoAlInicioDeLinea = true,
                    Alternativas = Array.AsReadOnly(new[] {
                        "Opcion invalida",
                        "Opción no válida",
                        "Opcion no valida",
                        "Selección inválida",
                        "Seleccion invalida",
                        "Fuera del menú",
                        "Opción desconocida",
                        "Opcion desconocida"
                    })
                },
            "Hasta luego" =>
                new ElementoTextualSecuenciaEsperado {
                    Valor = evento,
                    RequerirTextoAlInicioDeLinea = true,
                    Alternativas = Array.AsReadOnly(new[] {
                        "Adiós",
                        "Adios",
                        "Programa finalizado",
                        "Fin del programa",
                        "Gracias por usar el programa",
                        "Saliendo"
                    })
                },
            _ => throw new ArgumentException(
                "El evento del menú no está definido.",
                nameof(evento))
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
                    !TieneReglasAplicables(caso) ||
                    caso.SecuenciasEsperadas.Any(SecuenciaInvalida) ||
                    caso.SecuenciasCompuestasEsperadas.Any(
                        SecuenciaCompuestaInvalida)) ||
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
        bool tieneSecuencias = caso.SecuenciasEsperadas.Count > 0;
        bool tieneSecuenciasCompuestas =
            caso.SecuenciasCompuestasEsperadas.Count > 0;

        return caso.ModoComparacion switch {
            ModoComparacionCaso.Texto => tieneTexto,
            ModoComparacionCaso.Valores => tieneValores,
            ModoComparacionCaso.Secuencia =>
                tieneSecuencias || tieneSecuenciasCompuestas,
            _ =>
                tieneTexto ||
                tieneValores ||
                tieneSecuencias ||
                tieneSecuenciasCompuestas
        };
    }

    private static bool SecuenciaInvalida(SecuenciaEsperada secuencia) {
        if (string.IsNullOrWhiteSpace(secuencia.Nombre) ||
            secuencia.CantidadExacta.HasValue &&
            secuencia.CantidadExacta.Value <= 0 ||
            secuencia.SeparadoresPermitidos.Count == 0 ||
            secuencia.SeparadoresPermitidos.Any(string.IsNullOrEmpty) ||
            !double.IsFinite(secuencia.ToleranciaNumerica) ||
            secuencia.ToleranciaNumerica < 0D) {
            return true;
        }

        return secuencia.Tipo switch {
            TipoSecuenciaEsperada.Numerica =>
                secuencia.ValoresNumericosEsperados.Count == 0 ||
                secuencia.ValoresNumericosEsperados.Any(valor =>
                    !double.IsFinite(valor)),
            TipoSecuenciaEsperada.Textual =>
                secuencia.AlternativasTextualesEsperadas.Count == 0 ||
                secuencia.AlternativasTextualesEsperadas.Any(
                    EventoTextualInvalido) ||
                secuencia.EventosTextualesReconocibles.Any(
                    EventoTextualInvalido),
            _ => true
        };
    }

    private static bool EventoTextualInvalido(
        ElementoTextualSecuenciaEsperado evento) {
        bool tieneValorNumerico = evento.ValorNumericoAsociado.HasValue;
        bool tieneEtiquetasNumericas =
            evento.EtiquetasNumericasAsociadas.Count > 0;

        return string.IsNullOrWhiteSpace(evento.Valor) ||
            !evento.BuscarComoTexto &&
            !tieneValorNumerico ||
            tieneValorNumerico != tieneEtiquetasNumericas ||
            evento.ValorNumericoAsociado.HasValue &&
            !double.IsFinite(evento.ValorNumericoAsociado.Value) ||
            evento.EtiquetasNumericasAsociadas.Any(string.IsNullOrWhiteSpace) ||
            evento.EtiquetasNumericasPosteriores.Any(string.IsNullOrWhiteSpace);
    }

    private static bool SecuenciaCompuestaInvalida(
        SecuenciaCompuestaEsperada secuencia) {
        if (string.IsNullOrWhiteSpace(secuencia.Nombre) ||
            !secuencia.RequerirMismaLinea ||
            secuencia.CantidadExacta.HasValue &&
            secuencia.CantidadExacta.Value <= 0 ||
            secuencia.PasosEsperados.Count == 0 ||
            secuencia.SeparadoresTextualesPermitidos.Count == 0) {
            return true;
        }

        return secuencia.PasosEsperados.Any(paso => {
            ComponenteNumericoPasoEsperado[] componentes = paso.Componentes
                .OrderBy(componente => componente.Posicion)
                .ToArray();

            return string.IsNullOrWhiteSpace(paso.Nombre) ||
                componentes.Length != 3 ||
                componentes.Select(componente => componente.Posicion)
                    .Distinct()
                    .Count() != 3 ||
                componentes.Any(componente =>
                    string.IsNullOrWhiteSpace(componente.Nombre) ||
                    !double.IsFinite(componente.Valor) ||
                    !double.IsFinite(componente.Tolerancia) ||
                    componente.Tolerancia < 0D) ||
                componentes.Skip(1).Any(componente =>
                    componente.EtiquetasOSeparadoresOpcionales.Count == 0);
        });
    }
}
