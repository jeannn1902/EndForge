using EndForge.Models;
using System.Globalization;

namespace EndForge.Services;

public sealed partial class CatalogoEvaluacionesService {
    public const string ArchivosEscribirTextoId =
        "grado2-archivos-escribir-texto";
    public const string ArchivosLeerTextoId =
        "grado2-archivos-leer-texto";
    public const string ArchivosContarLineasPalabrasId =
        "grado2-archivos-contar-lineas-palabras";
    public const string ArchivosGuardarEstudiantesId =
        "grado2-archivos-guardar-estudiantes";
    public const string ArchivosBuscarRegistroId =
        "grado2-archivos-buscar-registro";
    public const string ArchivosResumenNumericoId =
        "grado2-archivos-resumen-numerico";

    private const int PuntosPorCasoArchivos = 12;
    private const string RutaMensaje = "mensaje.txt";
    private const string RutaTexto = "texto.txt";
    private const string RutaEstudiantes = "estudiantes.txt";
    private const string RutaNumeros = "numeros.txt";

    private static DefinicionEvaluacionPractica CrearArchivosEscribirTexto(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionArchivos(
            ArchivosEscribirTextoId,
            "Escribir una línea en un archivo",
            "Crear o sobrescribir mensaje.txt con una línea capturada.",
            "Se inspeccionará el archivo real creado en el directorio aislado de cada caso.",
            "Una línea completa de texto.",
            new[] { "Línea que se guardará en mensaje.txt" },
            new[] {
                "Crear o sobrescribir mensaje.txt.",
                "Conservar exactamente el contenido capturado.",
                "Mostrar una confirmación breve."
            },
            new[] {
                CrearCasoEscrituraArchivo(
                    "archivos-escribir-hola",
                    "Texto con espacio",
                    "Hola EndForge",
                    visible: true),
                CrearCasoEscrituraArchivo(
                    "archivos-escribir-cpp",
                    "Letras, números y signos",
                    "C++ 2026!",
                    visible: true),
                CrearCasoEscrituraArchivo(
                    "archivos-escribir-vacio",
                    "Línea vacía",
                    string.Empty,
                    visible: true),
                CrearCasoEscrituraArchivo(
                    "archivos-escribir-espacios",
                    "Espacios internos",
                    "Texto   con varios espacios internos",
                    visible: true),
                CrearCasoEscrituraArchivo(
                    "archivos-escribir-acentos-oculto",
                    "Acentos y signos",
                    "¡Árbol, canción y acción!",
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearArchivosLeerTexto(
        IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionArchivos(
            ArchivosLeerTextoId,
            "Leer y mostrar el contenido de un archivo",
            "Leer todo mensaje.txt y reproducir su contenido en consola.",
            "Cada caso preparará un mensaje.txt real antes de ejecutar el programa.",
            "Sin entrada de consola; mensaje.txt ya existe en el directorio del caso.",
            new[] { "Archivo mensaje.txt preparado por EndForge" },
            new[] {
                "Leer el archivo completo.",
                "Preservar líneas y espacios.",
                "No agregar contenido al resultado."
            },
            new[] {
                CrearCasoLecturaExacta(
                    "archivos-leer-hola",
                    "Una línea",
                    "Hola mundo",
                    visible: true),
                CrearCasoLecturaExacta(
                    "archivos-leer-dos-lineas",
                    "Dos líneas",
                    "Primera línea\nSegunda línea",
                    visible: true),
                CrearCasoLecturaExacta(
                    "archivos-leer-vacio",
                    "Archivo vacío",
                    string.Empty,
                    visible: true),
                CrearCasoLecturaExacta(
                    "archivos-leer-signos",
                    "Números y signos",
                    "2026: C++ + EndForge!",
                    visible: true),
                CrearCasoLecturaExacta(
                    "archivos-leer-multilinea-oculto",
                    "Acentos y tres líneas",
                    "Árbol y canción\nLínea número 2\n¡Hasta pronto!",
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearArchivosContarLineasPalabras(
            IReadOnlyList<CriterioEvaluacion> rubrica) {
        string[] etiquetasLineas = {
            "Líneas",
            "Lineas",
            "Cantidad de líneas",
            "Cantidad de lineas"
        };
        string[] etiquetasPalabras = {
            "Palabras",
            "Total de palabras",
            "Cantidad de palabras"
        };

        return CrearDefinicionArchivos(
            ArchivosContarLineasPalabrasId,
            "Contar líneas y palabras de un archivo",
            "Contar todas las líneas y palabras almacenadas en texto.txt.",
            "Se preparará texto.txt y se comprobarán dos resultados etiquetados.",
            "Sin entrada de consola; texto.txt ya existe en el directorio del caso.",
            new[] { "Archivo texto.txt preparado por EndForge" },
            new[] {
                "Contar las líneas vacías como líneas.",
                "Contar palabras separadas por espacios.",
                "Mostrar cero líneas y cero palabras para un archivo vacío."
            },
            new[] {
                CrearCasoConteoArchivo(
                    "archivos-contar-una-linea",
                    "Una línea",
                    "Hola mundo",
                    1,
                    2,
                    etiquetasLineas,
                    etiquetasPalabras,
                    visible: true),
                CrearCasoConteoArchivo(
                    "archivos-contar-dos-lineas",
                    "Dos líneas",
                    "Hola\nEndForge C++",
                    2,
                    3,
                    etiquetasLineas,
                    etiquetasPalabras,
                    visible: true),
                CrearCasoConteoArchivo(
                    "archivos-contar-vacio",
                    "Archivo vacío",
                    string.Empty,
                    0,
                    0,
                    etiquetasLineas,
                    etiquetasPalabras,
                    visible: true),
                CrearCasoConteoArchivo(
                    "archivos-contar-linea-vacia",
                    "Tres líneas con una vacía",
                    "Uno dos\n\nTres",
                    3,
                    3,
                    etiquetasLineas,
                    etiquetasPalabras,
                    visible: true),
                CrearCasoConteoArchivo(
                    "archivos-contar-espacios-oculto",
                    "Espacios múltiples y cuatro líneas",
                    "  uno   dos\n\n tres  cuatro \ncinco",
                    4,
                    5,
                    etiquetasLineas,
                    etiquetasPalabras,
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearArchivosGuardarEstudiantes(
            IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionArchivos(
            ArchivosGuardarEstudiantesId,
            "Guardar estudiantes en un archivo",
            "Capturar estudiantes y guardarlos, uno por línea, en estudiantes.txt.",
            "Se inspeccionará una tabla real con cantidad y orden exactos.",
            "Primero n (1 a 6); por cada estudiante: ID, nombre completo y promedio.",
            new[] {
                "Cantidad de estudiantes",
                "ID, nombre completo y promedio de cada estudiante"
            },
            new[] {
                "Crear o sobrescribir estudiantes.txt.",
                "Guardar un estudiante por línea.",
                "Conservar el orden de captura y los nombres completos."
            },
            new[] {
                CrearCasoGuardarEstudiantes(
                    "archivos-guardar-dos",
                    "Dos estudiantes",
                    new[] {
                        new EstudianteArchivo(10, "Ana López", 8.5D),
                        new EstudianteArchivo(20, "Luis Pérez", 7D)
                    },
                    visible: true),
                CrearCasoGuardarEstudiantes(
                    "archivos-guardar-uno",
                    "Un estudiante",
                    new[] {
                        new EstudianteArchivo(1, "Marta", 10D)
                    },
                    visible: true),
                CrearCasoGuardarEstudiantes(
                    "archivos-guardar-nombres-compuestos",
                    "Nombres compuestos",
                    new[] {
                        new EstudianteArchivo(
                            31,
                            "María José Pérez",
                            9.25D),
                        new EstudianteArchivo(
                            32,
                            "Juan Carlos Ruiz",
                            6.5D),
                        new EstudianteArchivo(
                            33,
                            "Ana Sofía",
                            8D)
                    },
                    visible: true),
                CrearCasoGuardarEstudiantes(
                    "archivos-guardar-decimales",
                    "Promedios decimales",
                    new[] {
                        new EstudianteArchivo(7, "Luz", 7.75D),
                        new EstudianteArchivo(8, "Raúl", 8.125D)
                    },
                    visible: true),
                CrearCasoGuardarEstudiantes(
                    "archivos-guardar-seis-oculto",
                    "Seis estudiantes",
                    new[] {
                        new EstudianteArchivo(101, "Ada Lovelace", 10D),
                        new EstudianteArchivo(102, "Alan Turing", 9.5D),
                        new EstudianteArchivo(103, "Grace Hopper", 9.25D),
                        new EstudianteArchivo(104, "Edsger Dijkstra", 8.75D),
                        new EstudianteArchivo(105, "Barbara Liskov", 9D),
                        new EstudianteArchivo(106, "Donald Knuth", 8.5D)
                    },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearArchivosBuscarRegistro(
            IReadOnlyList<CriterioEvaluacion> rubrica) {
        EstudianteArchivo[] baseTres = {
            new(10, "Ana López", 8.5D),
            new(20, "Luis Pérez", 7D),
            new(30, "Marta Ruiz", 9.25D)
        };

        return CrearDefinicionArchivos(
            ArchivosBuscarRegistroId,
            "Buscar un estudiante guardado en archivo",
            "Buscar un ID en estudiantes.txt y mostrar el registro cuando exista.",
            "Cada caso preparará un archivo real y comprobará existencia y datos coherentes.",
            "Un ID objetivo; estudiantes.txt ya existe en el directorio del caso.",
            new[] {
                "ID objetivo",
                "Archivo estudiantes.txt preparado por EndForge"
            },
            new[] {
                "Indicar si el estudiante fue encontrado.",
                "Mostrar ID, nombre y promedio solo cuando exista.",
                "No inventar datos para un ID ausente."
            },
            new[] {
                CrearCasoBuscarEstudiante(
                    "archivos-buscar-intermedio",
                    "Registro intermedio",
                    baseTres,
                    20,
                    baseTres[1],
                    visible: true),
                CrearCasoBuscarEstudiante(
                    "archivos-buscar-inexistente",
                    "ID inexistente",
                    baseTres,
                    99,
                    esperado: null,
                    visible: true),
                CrearCasoBuscarEstudiante(
                    "archivos-buscar-primero",
                    "Primer registro",
                    baseTres,
                    10,
                    baseTres[0],
                    visible: true),
                CrearCasoBuscarEstudiante(
                    "archivos-buscar-ultimo",
                    "Último registro",
                    baseTres,
                    30,
                    baseTres[2],
                    visible: true),
                CrearCasoBuscarEstudiante(
                    "archivos-buscar-seis-oculto",
                    "Archivo con seis registros",
                    new[] {
                        new EstudianteArchivo(101, "Ana María", 6.5D),
                        new EstudianteArchivo(205, "Luis Alberto", 7.75D),
                        new EstudianteArchivo(309, "María Fernanda", 8.25D),
                        new EstudianteArchivo(412, "José Manuel", 9D),
                        new EstudianteArchivo(518, "Laura Sofía", 9.5D),
                        new EstudianteArchivo(625, "Carlos Eduardo", 10D)
                    },
                    518,
                    new EstudianteArchivo(518, "Laura Sofía", 9.5D),
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica
        CrearArchivosResumenNumerico(
            IReadOnlyList<CriterioEvaluacion> rubrica) {
        return CrearDefinicionArchivos(
            ArchivosResumenNumericoId,
            "Calcular resumen de números guardados en archivo",
            "Calcular cantidad, suma, promedio, mayor y menor de numeros.txt.",
            "Cada caso preparará números reales y comprobará cinco resultados etiquetados.",
            "Sin entrada de consola; numeros.txt contiene un número por línea.",
            new[] { "Archivo numeros.txt preparado por EndForge" },
            new[] {
                "Leer todos los valores.",
                "Calcular cantidad, suma y promedio sin perder decimales.",
                "Calcular mayor y menor incluso cuando todos sean negativos."
            },
            new[] {
                CrearCasoResumenNumerico(
                    "archivos-resumen-tres",
                    "Tres enteros positivos",
                    new[] { 1D, 2D, 3D },
                    visible: true),
                CrearCasoResumenNumerico(
                    "archivos-resumen-uno-negativo",
                    "Un valor negativo",
                    new[] { -7D },
                    visible: true),
                CrearCasoResumenNumerico(
                    "archivos-resumen-negativos",
                    "Solo números negativos",
                    new[] { -2D, -8D, -4D },
                    visible: true),
                CrearCasoResumenNumerico(
                    "archivos-resumen-decimales",
                    "Valores decimales",
                    new[] { 1.5D, 2.25D, -0.75D, 4D },
                    visible: true),
                CrearCasoResumenNumerico(
                    "archivos-resumen-mixto-oculto",
                    "Positivos, negativos y cero",
                    new[] { 10D, -5D, 0D, 2.5D, -2.5D },
                    visible: false)
            },
            rubrica);
    }

    private static DefinicionEvaluacionPractica CrearDefinicionArchivos(
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

    private static CasoPrueba CrearCasoEscrituraArchivo(
        string id,
        string nombre,
        string texto,
        bool visible) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = texto + "\n",
            SalidaEsperada = "Archivo guardado correctamente.",
            EsVisible = visible,
            Puntos = PuntosPorCasoArchivos,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Texto,
            Descripcion =
                "Comprueba el contenido exacto de mensaje.txt y una confirmación breve.",
            GruposTokensAlternativos = Array.AsReadOnly(new[] {
                new GrupoTokensEsperados {
                    Nombre = "Confirmación de escritura",
                    Alternativas = Array.AsReadOnly(new[] {
                        "guardado",
                        "creado",
                        "escrito",
                        "almacenado",
                        "correctamente"
                    })
                }
            }),
            ArchivosEsperados = Array.AsReadOnly(new[] {
                ArchivoExacto(RutaMensaje, texto, permitirSaltoFinal: true)
            })
        };
    }

    private static CasoPrueba CrearCasoLecturaExacta(
        string id,
        string nombre,
        string contenido,
        bool visible) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = string.Empty,
            SalidaEsperada = contenido,
            EsVisible = visible,
            Puntos = PuntosPorCasoArchivos,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Texto,
            Descripcion =
                "Comprueba que se lea el archivo completo sin añadir contenido.",
            ArchivosEntrada = Array.AsReadOnly(new[] {
                ArchivoEntrada(RutaMensaje, contenido)
            }),
            SalidaExactaEsperada = new ReglaSalidaExactaPrueba {
                ValorEsperado = contenido,
                PermitirUnSaltoLineaFinal = true
            }
        };
    }

    private static CasoPrueba CrearCasoConteoArchivo(
        string id,
        string nombre,
        string contenido,
        int lineas,
        int palabras,
        string[] etiquetasLineas,
        string[] etiquetasPalabras,
        bool visible) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = string.Empty,
            SalidaEsperada =
                $"Líneas: {lineas}\nPalabras: {palabras}",
            EsVisible = visible,
            Puntos = PuntosPorCasoArchivos,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion =
                "Comprueba ambos conteos sobre el archivo completo.",
            ValoresNumericosEsperados = Array.AsReadOnly(new[] {
                NumeroArchivo("Líneas", lineas, 0D, etiquetasLineas),
                NumeroArchivo("Palabras", palabras, 0D, etiquetasPalabras)
            }),
            ArchivosEntrada = Array.AsReadOnly(new[] {
                ArchivoEntrada(RutaTexto, contenido)
            })
        };
    }

    private static CasoPrueba CrearCasoGuardarEstudiantes(
        string id,
        string nombre,
        EstudianteArchivo[] estudiantes,
        bool visible) {
        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = CrearEntradaEstudiantes(estudiantes),
            SalidaEsperada = CrearContenidoEstudiantes(estudiantes),
            EsVisible = visible,
            Puntos = PuntosPorCasoArchivos,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion =
                "Comprueba cantidad, campos y orden en estudiantes.txt.",
            ArchivosEsperados = Array.AsReadOnly(new[] {
                new ArchivoEsperadoPrueba {
                    RutaRelativa = RutaEstudiantes,
                    ModoComparacion =
                        ModoComparacionArchivoPrueba.Estructurado,
                    TablasEsperadas = Array.AsReadOnly(new[] {
                        TablaEstudiantesArchivo(estudiantes)
                    })
                }
            })
        };
    }

    private static CasoPrueba CrearCasoBuscarEstudiante(
        string id,
        string nombre,
        EstudianteArchivo[] estudiantes,
        int idBuscado,
        EstudianteArchivo? esperado,
        bool visible) {
        bool encontrado = esperado is not null;
        string salida = encontrado
            ? $"Encontrado: sí\nID: {esperado!.Id}\n" +
              $"Nombre: {esperado.Nombre}\n" +
              $"Promedio: {FormatearNumeroArchivo(esperado.Promedio)}"
            : "Encontrado: no";

        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = idBuscado.ToString(CultureInfo.InvariantCulture) + "\n",
            SalidaEsperada = salida,
            EsVisible = visible,
            Puntos = PuntosPorCasoArchivos,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion =
                "Comprueba la existencia y, cuando corresponde, el registro completo.",
            ValoresBooleanosEsperados = Array.AsReadOnly(new[] {
                new ValorBooleanoEsperado {
                    Nombre = "Encontrado",
                    Valor = encontrado,
                    EtiquetasAlternativas = Array.AsReadOnly(new[] {
                        "Encontrado",
                        "Existe",
                        "Resultado"
                    })
                }
            }),
            ValoresNumericosEsperados = esperado is null
                ? Array.AsReadOnly(new[] {
                    NumeroAusenteArchivo(
                        "ID ausente",
                        "ID",
                        "Matrícula",
                        "Matricula",
                        "Identificador"),
                    NumeroAusenteArchivo(
                        "Promedio ausente",
                        "Promedio",
                        "Calificación",
                        "Calificacion",
                        "Media")
                })
                : Array.Empty<ValorNumericoEsperado>(),
            CadenasEsperadas = esperado is null
                ? Array.AsReadOnly(new[] {
                    CadenaAusenteArchivo(
                        "Nombre ausente",
                        "Nombre",
                        "Estudiante",
                        "Nombre completo")
                })
                : Array.Empty<ReglaCadenaEsperada>(),
            BloquesRegistroEsperados = Array.AsReadOnly(new[] {
                ReglaRegistroBuscado(esperado)
            }),
            ArchivosEntrada = Array.AsReadOnly(new[] {
                ArchivoEntrada(
                    RutaEstudiantes,
                    CrearContenidoEstudiantes(estudiantes))
            })
        };
    }

    private static CasoPrueba CrearCasoResumenNumerico(
        string id,
        string nombre,
        double[] valores,
        bool visible) {
        double suma = valores.Sum();
        double promedio = suma / valores.Length;
        double mayor = valores.Max();
        double menor = valores.Min();

        return new CasoPrueba {
            Id = id,
            Nombre = nombre,
            Entrada = string.Empty,
            SalidaEsperada =
                $"Cantidad: {valores.Length}\n" +
                $"Suma: {FormatearNumeroArchivo(suma)}\n" +
                $"Promedio: {FormatearNumeroArchivo(promedio)}\n" +
                $"Mayor: {FormatearNumeroArchivo(mayor)}\n" +
                $"Menor: {FormatearNumeroArchivo(menor)}",
            EsVisible = visible,
            Puntos = PuntosPorCasoArchivos,
            ComparacionFlexible = true,
            ModoComparacion = ModoComparacionCaso.Valores,
            Descripcion =
                "Comprueba los cinco resultados del archivo completo.",
            ValoresNumericosEsperados = Array.AsReadOnly(new[] {
                NumeroArchivo("Cantidad", valores.Length, 0D, "Cantidad"),
                NumeroArchivo("Suma", suma, 0.01D, "Suma"),
                NumeroArchivo("Promedio", promedio, 0.01D, "Promedio"),
                NumeroArchivo("Mayor", mayor, 0.01D, "Mayor"),
                NumeroArchivo("Menor", menor, 0.01D, "Menor")
            }),
            ArchivosEntrada = Array.AsReadOnly(new[] {
                ArchivoEntrada(
                    RutaNumeros,
                    string.Join(
                        "\n",
                        valores.Select(FormatearNumeroArchivo)))
            })
        };
    }

    private static ValorNumericoEsperado NumeroAusenteArchivo(
        string nombre,
        params string[] etiquetas) {
        return new ValorNumericoEsperado {
            Nombre = nombre,
            DebeEstarAusente = true,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas)
        };
    }

    private static ReglaCadenaEsperada CadenaAusenteArchivo(
        string nombre,
        params string[] etiquetas) {
        return new ReglaCadenaEsperada {
            Nombre = nombre,
            ValorEsperado = string.Empty,
            EtiquetasAlternativas = Array.AsReadOnly(etiquetas),
            Origen = OrigenCadenaEsperada.DespuesDeEtiqueta,
            DistinguirMayusculas = true,
            DistinguirAcentos = true,
            PoliticaEspacios = PoliticaEspaciosCadena.Exactos,
            PermitirTextoAdicional = false,
            Obligatoria = false,
            MensajeError =
                "No deben mostrarse datos de estudiante cuando el ID no existe."
        };
    }

    private static ArchivoEntradaPrueba ArchivoEntrada(
        string ruta,
        string contenido) {
        return new ArchivoEntradaPrueba {
            RutaRelativa = ruta,
            Contenido = contenido
        };
    }

    private static ArchivoEsperadoPrueba ArchivoExacto(
        string ruta,
        string contenido,
        bool permitirSaltoFinal) {
        return new ArchivoEsperadoPrueba {
            RutaRelativa = ruta,
            ContenidoEsperado = contenido,
            ModoComparacion = ModoComparacionArchivoPrueba.TextoExacto,
            PermitirUnSaltoLineaFinal = permitirSaltoFinal
        };
    }

    private static ValorNumericoEsperado NumeroArchivo(
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

    private static ReglaTablaEsperada TablaEstudiantesArchivo(
        IReadOnlyList<EstudianteArchivo> estudiantes) {
        return new ReglaTablaEsperada {
            Nombre = "Estudiantes guardados",
            FilasEsperadas = Array.AsReadOnly(estudiantes
                .Select((estudiante, indice) =>
                    FilaEstudianteArchivo(estudiante, indice))
                .ToArray()),
            CantidadFilasExacta = estudiantes.Count,
            CantidadColumnasExacta = 3,
            OrdenFilasObligatorio = true,
            PermitirFilasAdicionales = false,
            PermitirFilasDuplicadas = false,
            PermitirTextoNeutralEntreFilas = false,
            SeparadoresColumnas = Array.AsReadOnly(new[] { "|" }),
            Obligatoria = true,
            MensajeError =
                "estudiantes.txt debe contener exactamente un registro por línea y conservar el orden."
        };
    }

    private static FilaTablaEsperada FilaEstudianteArchivo(
        EstudianteArchivo estudiante,
        int indice) {
        return new FilaTablaEsperada {
            Nombre = $"Estudiante {indice + 1}",
            Celdas = Array.AsReadOnly(new[] {
                new CeldaTablaEsperada {
                    Nombre = "ID",
                    Posicion = 0,
                    Valor = ValorNumeroEstructurado(
                        "ID",
                        estudiante.Id,
                        0D)
                },
                new CeldaTablaEsperada {
                    Nombre = "Nombre",
                    Posicion = 1,
                    Valor = ValorTextoEstructurado(
                        "Nombre",
                        estudiante.Nombre)
                },
                new CeldaTablaEsperada {
                    Nombre = "Promedio",
                    Posicion = 2,
                    Valor = ValorNumeroEstructurado(
                        "Promedio",
                        estudiante.Promedio,
                        0.01D)
                }
            })
        };
    }

    private static ReglaBloquesRegistroEsperados ReglaRegistroBuscado(
        EstudianteArchivo? estudiante) {
        RegistroEsperado[] registros = estudiante is null
            ? Array.Empty<RegistroEsperado>()
            : new[] {
                new RegistroEsperado {
                    Nombre = $"Estudiante {estudiante.Id}",
                    Clave = ValorNumeroEstructurado(
                        "ID",
                        estudiante.Id,
                        0D),
                    Campos = Array.AsReadOnly(new[] {
                        new CampoRegistroEsperado {
                            Nombre = "Nombre",
                            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                                "Nombre",
                                "Estudiante",
                                "Nombre completo"
                            }),
                            Valor = ValorTextoEstructurado(
                                "Nombre",
                                estudiante.Nombre)
                        },
                        new CampoRegistroEsperado {
                            Nombre = "Promedio",
                            EtiquetasAlternativas = Array.AsReadOnly(new[] {
                                "Promedio",
                                "Calificación",
                                "Calificacion",
                                "Media"
                            }),
                            Valor = ValorNumeroEstructurado(
                                "Promedio",
                                estudiante.Promedio,
                                0.01D)
                        }
                    })
                }
            };

        return new ReglaBloquesRegistroEsperados {
            Nombre = "Estudiante encontrado",
            NombreCampoClave = "ID",
            EtiquetasClave = Array.AsReadOnly(new[] {
                "ID",
                "Matrícula",
                "Matricula",
                "Identificador"
            }),
            TipoClave = TipoValorEstructurado.Numerico,
            RegistrosEsperados = Array.AsReadOnly(registros),
            OrdenRegistrosObligatorio = true,
            PermitirRegistrosAdicionales = false,
            PermitirRegistrosDuplicados = false,
            PermitirTextoNeutralEntreBloques = true,
            Obligatoria = estudiante is not null,
            MensajeError =
                "La búsqueda debe mostrar únicamente el registro correspondiente al ID solicitado."
        };
    }

    private static ValorEstructuradoEsperado ValorNumeroEstructurado(
        string nombre,
        double valor,
        double tolerancia) {
        return new ValorEstructuradoEsperado {
            Nombre = nombre,
            Tipo = TipoValorEstructurado.Numerico,
            ValorNumerico = valor,
            ToleranciaNumerica = tolerancia
        };
    }

    private static ValorEstructuradoEsperado ValorTextoEstructurado(
        string nombre,
        string valor) {
        return new ValorEstructuradoEsperado {
            Nombre = nombre,
            Tipo = TipoValorEstructurado.Textual,
            ValorTextual = valor,
            DistinguirMayusculas = true,
            DistinguirAcentos = true,
            PoliticaEspacios = PoliticaEspaciosCadena.Exactos
        };
    }

    private static string CrearEntradaEstudiantes(
        IReadOnlyList<EstudianteArchivo> estudiantes) {
        return estudiantes.Count.ToString(CultureInfo.InvariantCulture) +
            "\n" +
            string.Join(
                "\n",
                estudiantes.Select(estudiante =>
                    estudiante.Id.ToString(CultureInfo.InvariantCulture) +
                    "\n" +
                    estudiante.Nombre +
                    "\n" +
                    FormatearNumeroArchivo(estudiante.Promedio))) +
            "\n";
    }

    private static string CrearContenidoEstudiantes(
        IEnumerable<EstudianteArchivo> estudiantes) {
        return string.Join(
            "\n",
            estudiantes.Select(estudiante =>
                estudiante.Id.ToString(CultureInfo.InvariantCulture) +
                "|" +
                estudiante.Nombre +
                "|" +
                FormatearNumeroArchivo(estudiante.Promedio)));
    }

    private static string FormatearNumeroArchivo(double valor) {
        return valor.ToString("G15", CultureInfo.InvariantCulture);
    }

    private sealed record EstudianteArchivo(
        int Id,
        string Nombre,
        double Promedio);
}
