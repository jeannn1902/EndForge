using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
namespace EndForge.Services;

public class ProyectoService {
    private const int MaximosIntentosReservaTemporal = 32;
    private const int ErrorArchivoExistente = 80;
    private const int ErrorDirectorioExistente = 183;
    private const string NombreMarcadorPropiedadTemporal =
        ".endforge-staging-owner";
    private readonly SeleccionSolucionesService seleccionSolucionesService;
    private readonly Func<string, bool> crearDirectorioExclusivo;

    public ProyectoService()
        : this(new SeleccionSolucionesService()) {
    }

    public ProyectoService(SeleccionSolucionesService seleccionSolucionesService)
        : this(
            seleccionSolucionesService,
            CrearDirectorioExclusivoWindows) {
    }

    internal ProyectoService(
        SeleccionSolucionesService seleccionSolucionesService,
        Func<string, bool> crearDirectorioExclusivoPruebas) {
        this.seleccionSolucionesService = seleccionSolucionesService;
        crearDirectorioExclusivo =
            crearDirectorioExclusivoPruebas;
    }

    public sealed class ProyectoDestinoExistenteException : IOException {
        public ProyectoDestinoExistenteException(string rutaProyecto)
            : base($"La carpeta de destino ya existe: {rutaProyecto}") {
        }

        public ProyectoDestinoExistenteException(string rutaProyecto, Exception innerException)
            : base($"La carpeta de destino ya existe: {rutaProyecto}", innerException) {
        }
    }


    // =============================
    // Creación de proyectos
    // =============================
    public void CrearReadme(string rutaProyecto, string nombreProyecto, string temaSeleccionado, string objetivo) {

        //Crear el archivo README.md
        string contenidoReadme = $@"# {nombreProyecto}

            ## Tema
            {temaSeleccionado}

            ## Objetivo
            {objetivo}

            ## Fecha de creación
            {DateTime.Now:dd/MM/yyyy}

            ## Descripción
            Ejercicio creado automáticamente mediante EndForge.";

        string rutaReadme = Path.Combine(rutaProyecto, "README.md");

        File.WriteAllText(rutaReadme, contenidoReadme);
    }

    // =============================
    // Actualización del contenido del proyecto
    // =============================
    public void ActualizarReferencias(string rutaProyecto, string nombreProyecto) {
        // Reemplazar "00_Plantilla" en el contenido de los archivos
        foreach (string archivo in Directory.GetFiles(rutaProyecto, "*", SearchOption.AllDirectories)) {

            string extension = Path.GetExtension(archivo);

            bool esXml = extension.Equals(".vcxproj", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".filters", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".user", StringComparison.OrdinalIgnoreCase);

            bool requiereActualizacion = esXml ||
                extension.Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".cpp", StringComparison.OrdinalIgnoreCase);

            if (!requiereActualizacion) {
                continue;
            }

            (string contenido, Encoding codificacion) = LeerContenido(archivo);

            if (esXml) {
                ActualizarXml(
                    archivo,
                    contenido,
                    codificacion,
                    nombreProyecto
                );
            } else {
                string contenidoActualizado = contenido.Replace(
                    SeleccionSolucionesService.MarcadorPlantilla,
                    nombreProyecto,
                    StringComparison.Ordinal
                );
                File.WriteAllText(
                    archivo,
                    contenidoActualizado,
                    codificacion
                );
            }
        }
    }

    private static void ActualizarXml(
        string rutaArchivo,
        string contenido,
        Encoding codificacion,
        string nombreProyecto) {
        XmlReaderSettings configuracionLectura = new() {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        XDocument documento;

        using (StringReader texto = new(contenido))
        using (XmlReader lector = XmlReader.Create(
            texto,
            configuracionLectura)) {
            documento = XDocument.Load(
                lector,
                LoadOptions.PreserveWhitespace
            );
        }

        foreach (XAttribute atributo in documento
            .Descendants()
            .Attributes()) {
            atributo.Value = atributo.Value.Replace(
                SeleccionSolucionesService.MarcadorPlantilla,
                nombreProyecto,
                StringComparison.Ordinal
            );
        }

        foreach (XText texto in documento
            .DescendantNodes()
            .OfType<XText>()) {
            texto.Value = texto.Value.Replace(
                SeleccionSolucionesService.MarcadorPlantilla,
                nombreProyecto,
                StringComparison.Ordinal
            );
        }

        using MemoryStream datos = new();
        XmlWriterSettings configuracionEscritura = new() {
            Encoding = codificacion,
            Indent = false,
            NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = documento.Declaration is null,
            CloseOutput = false
        };

        using (XmlWriter escritor = XmlWriter.Create(
            datos,
            configuracionEscritura)) {
            documento.Save(escritor);
        }

        File.WriteAllBytes(rutaArchivo, datos.ToArray());
    }

    private static (string Contenido, Encoding Codificacion) LeerContenido(string rutaArchivo) {
        byte[] datos = File.ReadAllBytes(rutaArchivo);
        (Encoding codificacion, int longitudPreambulo) = DetectarCodificacion(datos);
        string contenido = codificacion.GetString(datos, longitudPreambulo, datos.Length - longitudPreambulo);

        return (contenido, codificacion);
    }

    private static (Encoding Codificacion, int LongitudPreambulo) DetectarCodificacion(byte[] datos) {
        if (datos.Length >= 4 && datos[0] == 0x00 && datos[1] == 0x00 && datos[2] == 0xFE && datos[3] == 0xFF) {
            return (new UTF32Encoding(true, true, true), 4);
        }

        if (datos.Length >= 4 && datos[0] == 0xFF && datos[1] == 0xFE && datos[2] == 0x00 && datos[3] == 0x00) {
            return (new UTF32Encoding(false, true, true), 4);
        }

        if (datos.Length >= 3 && datos[0] == 0xEF && datos[1] == 0xBB && datos[2] == 0xBF) {
            return (new UTF8Encoding(true, true), 3);
        }

        if (datos.Length >= 2 && datos[0] == 0xFE && datos[1] == 0xFF) {
            return (new UnicodeEncoding(true, true, true), 2);
        }

        if (datos.Length >= 2 && datos[0] == 0xFF && datos[1] == 0xFE) {
            return (new UnicodeEncoding(false, true, true), 2);
        }

        return (new UTF8Encoding(false, true), 0);
    }

    // =============================
    // Copia y preparación de la plantilla
    // =============================
    public void CopiarPlantilla(string rutaPlantilla, string rutaProyecto) {
        // TODO: Sustituir por un sistema de copia inteligente.
        foreach (string archivo in
            DirectorioTemporalEvaluacionCpp.EnumerarArchivosSinPuntosDeReanalisis(
                rutaPlantilla)) {

            string rutaRelativa = Path.GetRelativePath(rutaPlantilla, archivo);

            if (Path.GetExtension(archivo).Equals(".user", StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            if (rutaRelativa.Equals(
                NombreMarcadorPropiedadTemporal,
                StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            if (rutaRelativa.StartsWith(
                    ".vs" + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase) ||
                rutaRelativa.StartsWith(
                    "x64" + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            string destino = Path.Combine(rutaProyecto, rutaRelativa);

            Directory.CreateDirectory(Path.GetDirectoryName(destino)!);

            File.Copy(archivo, destino, true);
        }
    }

    // =============================
    // Renombra los archivos de la plantilla
    // =============================
    public void RenombrarArchivos(string rutaProyecto, string nombreProyecto) {
        // Renombrar archivos de toda la estructura
        foreach (string archivo in Directory.GetFiles(
            rutaProyecto,
            "*",
            SearchOption.AllDirectories)) {
            string nombreArchivo = Path.GetFileName(archivo);

            if (!nombreArchivo.Contains("00_Plantilla"))
                continue;

            string carpetaArchivo = Path.GetDirectoryName(archivo)!;
            string nuevoNombre = nombreArchivo.Replace("00_Plantilla", nombreProyecto);
            string nuevaRuta = Path.Combine(carpetaArchivo, nuevoNombre);

            File.Move(archivo, nuevaRuta);
        }
    }

    // =============================
    // Renombra las carpetas de la plantilla
    // =============================
    public void RenombrarCarpetas(string rutaProyecto, string nombreProyecto) {
        // Renombrar carpetas desde las más profundas hacia las superiores
        string[] carpetasProyecto = Directory.GetDirectories(
            rutaProyecto,
            "*",
            SearchOption.AllDirectories
        )
        .OrderByDescending(carpeta => carpeta.Length)
        .ToArray();

        foreach (string carpeta in carpetasProyecto) {
            string nombreCarpeta = Path.GetFileName(carpeta);

            if (!nombreCarpeta.Contains("00_Plantilla"))
                continue;

            string carpetaPadre = Path.GetDirectoryName(carpeta)!;
            string nuevoNombre = nombreCarpeta.Replace("00_Plantilla", nombreProyecto);
            string nuevaRuta = Path.Combine(carpetaPadre, nuevoNombre);

            Directory.Move(carpeta, nuevaRuta);
        }
    }

    // =============================
    // Abrir el proyecto en Visual Studio
    // =============================
    public void AbrirProyecto(string rutaProyecto, string nombreProyecto) {
        string? rutaSolucion = Directory
            .GetFiles(rutaProyecto, "*.sln", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (rutaSolucion == null) {
            throw new FileNotFoundException("No se encontró ningún archivo .sln en la práctica.");
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
            FileName = rutaSolucion,
            UseShellExecute = true
        });
    }


    public void CrearProyecto(
        string rutaPlantilla,
        string rutaProyecto,
        string nombreProyecto,
        string temaSeleccionado,
        string objetivo) {
        CrearProyecto(
            rutaPlantilla,
            rutaProyecto,
            nombreProyecto,
            temaSeleccionado,
            objetivo,
            ""
        );
    }

    public void CrearProyecto(
        string rutaPlantilla,
        string rutaProyecto,
        string nombreProyecto,
        string temaSeleccionado,
        string objetivo,
        string rutaRelativaSolucionEsperada,
        string rutaRaizDestinoConfiable) {
        CrearProyectoInterno(
            rutaPlantilla,
            rutaProyecto,
            nombreProyecto,
            temaSeleccionado,
            objetivo,
            rutaRelativaSolucionEsperada,
            rutaRaizDestinoConfiable);
    }

    public void CrearProyecto(
        string rutaPlantilla,
        string rutaProyecto,
        string nombreProyecto,
        string temaSeleccionado,
        string objetivo,
        string rutaRelativaSolucionEsperada) {
        CrearProyectoInterno(
            rutaPlantilla,
            rutaProyecto,
            nombreProyecto,
            temaSeleccionado,
            objetivo,
            rutaRelativaSolucionEsperada,
            "");
    }

    private void CrearProyectoInterno(
        string rutaPlantilla,
        string rutaProyecto,
        string nombreProyecto,
        string temaSeleccionado,
        string objetivo,
        string rutaRelativaSolucionEsperada,
        string rutaRaizDestinoConfiable) {
        if (Directory.Exists(rutaProyecto) || File.Exists(rutaProyecto)) {
            throw new ProyectoDestinoExistenteException(rutaProyecto);
        }

        string? carpetaPadre = Path.GetDirectoryName(rutaProyecto);

        if (string.IsNullOrWhiteSpace(carpetaPadre) || !Directory.Exists(carpetaPadre)) {
            throw new DirectoryNotFoundException("No existe la carpeta donde se creará la práctica.");
        }

        ValidarDestinoSeguro(
            rutaProyecto,
            carpetaPadre,
            rutaRaizDestinoConfiable);

        string rutaRelativaSeleccionada =
            RevalidarPlantillaAntesDeCrear(
                rutaPlantilla,
                nombreProyecto,
                rutaRelativaSolucionEsperada);
        string rutaTemporal = "";
        string tokenPropiedad = Guid.NewGuid().ToString("N");

        bool carpetaTemporalCreada = false;

        try {
            for (int intento = 0;
                intento < MaximosIntentosReservaTemporal;
                intento++) {
                string candidata = Path.Combine(
                    carpetaPadre,
                    $".endforge-{Guid.NewGuid():N}.tmp");

                if (!crearDirectorioExclusivo(candidata)) {
                    continue;
                }

                rutaTemporal = candidata;

                try {
                    File.WriteAllText(
                        Path.Combine(
                            rutaTemporal,
                            NombreMarcadorPropiedadTemporal),
                        tokenPropiedad);
                    carpetaTemporalCreada = true;
                } catch {
                    try {
                        Directory.Delete(
                            rutaTemporal,
                            recursive: false);
                    } catch (Exception) {
                        // Si dejó de estar vacía, no se adopta ni se borra.
                    }

                    throw;
                }

                break;
            }

            if (!carpetaTemporalCreada) {
                throw new IOException(
                    "No se pudo reservar una carpeta temporal exclusiva para crear la práctica.");
            }

            CopiarPlantilla(rutaPlantilla, rutaTemporal);

            RenombrarArchivos(rutaTemporal, nombreProyecto);

            RenombrarCarpetas(rutaTemporal, nombreProyecto);

            ActualizarReferencias(rutaTemporal, nombreProyecto);

            ResultadoSeleccionSolucionCompatible validacionStaging =
                seleccionSolucionesService.SeleccionarSolucionParaPractica(
                    rutaTemporal,
                    rutaRelativaSeleccionada,
                    usarSeleccionGuardada: false);

            if (validacionStaging.Estado !=
                EstadoSeleccionSolucionCompatible.Exitosa) {
                throw new InvalidDataException(
                    "La copia preparada de la plantilla no es compatible con EndForge.",
                    validacionStaging.Error);
            }

            seleccionSolucionesService.GuardarSolucionSeleccionada(
                rutaTemporal,
                rutaRelativaSeleccionada
            );

            CrearReadme(rutaTemporal, nombreProyecto, temaSeleccionado, objetivo);

            if (Directory.Exists(rutaProyecto) || File.Exists(rutaProyecto)) {
                throw new ProyectoDestinoExistenteException(rutaProyecto);
            }

            ValidarDestinoSeguro(
                rutaProyecto,
                carpetaPadre,
                rutaRaizDestinoConfiable);

            try {
                Directory.Move(rutaTemporal, rutaProyecto);
            } catch (IOException ex) when (Directory.Exists(rutaProyecto) || File.Exists(rutaProyecto)) {
                throw new ProyectoDestinoExistenteException(rutaProyecto, ex);
            }

            carpetaTemporalCreada = false;
            IntentarEliminarMarcadorPublicado(rutaProyecto);
        } catch (Exception) {
            if (carpetaTemporalCreada && Directory.Exists(rutaTemporal)) {
                try {
                    EliminarTemporalPropio(
                        rutaTemporal,
                        tokenPropiedad);
                } catch (Exception) {
                    // Evita ocultar el error original de creación.
                }
            }

            throw;
        }
    }

    private string RevalidarPlantillaAntesDeCrear(
        string rutaPlantilla,
        string nombreProyecto,
        string rutaRelativaSolucionEsperada) {
        ResultadoSeleccionSolucionCompatible seleccion =
            seleccionSolucionesService.SeleccionarSolucionParaPlantilla(
                rutaPlantilla);

        if (seleccion.Estado !=
            EstadoSeleccionSolucionCompatible.Exitosa) {
            throw new InvalidDataException(
                "La plantilla cambió o ya no es compatible con EndForge.",
                seleccion.Error);
        }

        string rutaTransformada =
            seleccionSolucionesService.TransformarRutaRelativa(
                seleccion.RutaRelativaSolucion,
                nombreProyecto);

        if (!string.IsNullOrWhiteSpace(
                rutaRelativaSolucionEsperada) &&
            !RutasRelativasEquivalentes(
                rutaTransformada,
                rutaRelativaSolucionEsperada)) {
            throw new InvalidDataException(
                "La solución validada ya no coincide con la plantilla seleccionada.");
        }

        return rutaTransformada;
    }

    private static bool RutasRelativasEquivalentes(
        string primera,
        string segunda) {
        if (Path.IsPathRooted(primera) ||
            Path.IsPathRooted(segunda)) {
            return false;
        }

        try {
            string baseComparacion = Path.Combine(
                Path.GetTempPath(),
                "EndForge-Comparacion-Rutas");
            string primeraNormalizada =
                Path.GetFullPath(primera, baseComparacion);
            string segundaNormalizada =
                Path.GetFullPath(segunda, baseComparacion);

            return primeraNormalizada.Equals(
                segundaNormalizada,
                StringComparison.OrdinalIgnoreCase);
        } catch (Exception ex) when (
            ex is ArgumentException or NotSupportedException or
                PathTooLongException) {
            return false;
        }
    }

    private static void ValidarDestinoSeguro(
        string rutaProyecto,
        string carpetaPadre,
        string rutaRaizDestinoConfiable) {
        if (string.IsNullOrWhiteSpace(
            rutaRaizDestinoConfiable)) {
            if (File.GetAttributes(carpetaPadre)
                .HasFlag(FileAttributes.ReparsePoint)) {
                throw new InvalidDataException(
                    "La carpeta de destino contiene un enlace no permitido.");
            }

            return;
        }

        string raiz = Path.GetFullPath(
            rutaRaizDestinoConfiable);
        string destino = Path.GetFullPath(rutaProyecto);
        string padre = Path.GetFullPath(carpetaPadre);

        if (!Directory.Exists(raiz) ||
            !DirectorioTemporalEvaluacionCpp.EstaDentroDe(
                raiz,
                destino) ||
            !DirectorioTemporalEvaluacionCpp
                .EsRutaSinPuntosDeReanalisis(
                    raiz,
                    padre)) {
            throw new InvalidDataException(
                "La carpeta de destino está fuera de la ruta base o contiene un enlace no permitido.");
        }
    }

    private static bool CrearDirectorioExclusivoWindows(
        string rutaDirectorio) {
        if (CreateDirectoryW(
            PrepararRutaWin32(rutaDirectorio),
            IntPtr.Zero)) {
            return true;
        }

        int codigoError = Marshal.GetLastWin32Error();

        if (codigoError is ErrorArchivoExistente or
            ErrorDirectorioExistente) {
            return false;
        }

        throw new IOException(
            "No se pudo reservar la carpeta temporal de la práctica.",
            new Win32Exception(codigoError));
    }

    private static string PrepararRutaWin32(string ruta) {
        string rutaCompleta = Path.GetFullPath(ruta);

        if (rutaCompleta.StartsWith(
            @"\\?\",
            StringComparison.Ordinal)) {
            return rutaCompleta;
        }

        if (rutaCompleta.StartsWith(
            @"\\",
            StringComparison.Ordinal)) {
            return @"\\?\UNC\" + rutaCompleta[2..];
        }

        return @"\\?\" + rutaCompleta;
    }

    private static void IntentarEliminarMarcadorPublicado(
        string rutaProyecto) {
        try {
            string rutaMarcador = Path.Combine(
                rutaProyecto,
                NombreMarcadorPropiedadTemporal);

            if (File.Exists(rutaMarcador)) {
                File.SetAttributes(
                    rutaMarcador,
                    FileAttributes.Normal);
                File.Delete(rutaMarcador);
            }
        } catch (Exception) {
            // Un marcador interno residual no invalida una práctica publicada.
        }
    }

    private static void EliminarTemporalPropio(
        string rutaTemporal,
        string tokenPropiedad) {
        FileAttributes atributosRaiz = File.GetAttributes(rutaTemporal);

        if (atributosRaiz.HasFlag(FileAttributes.ReparsePoint)) {
            return;
        }

        string rutaMarcador = Path.Combine(
            rutaTemporal,
            NombreMarcadorPropiedadTemporal);

        try {
            if (!File.Exists(rutaMarcador) ||
                !File.ReadAllText(rutaMarcador).Equals(
                    tokenPropiedad,
                    StringComparison.Ordinal)) {
                return;
            }
        } catch (Exception) {
            return;
        }

        foreach (string entrada in Directory.EnumerateFileSystemEntries(
            rutaTemporal,
            "*",
            SearchOption.TopDirectoryOnly)) {
            FileAttributes atributos = File.GetAttributes(entrada);

            if (atributos.HasFlag(FileAttributes.Directory) &&
                !atributos.HasFlag(FileAttributes.ReparsePoint)) {
                EliminarArbolTemporal(entrada);
                continue;
            }

            if (atributos.HasFlag(FileAttributes.Directory)) {
                Directory.Delete(entrada, recursive: false);
            } else {
                File.SetAttributes(entrada, FileAttributes.Normal);
                File.Delete(entrada);
            }
        }

        File.SetAttributes(
            rutaTemporal,
            File.GetAttributes(rutaTemporal) & ~FileAttributes.ReadOnly);
        Directory.Delete(rutaTemporal, recursive: false);
    }

    private static void EliminarArbolTemporal(string rutaTemporal) {
        FileAttributes atributosRaiz =
            File.GetAttributes(rutaTemporal);

        if (atributosRaiz.HasFlag(FileAttributes.ReparsePoint)) {
            return;
        }

        foreach (string entrada in
            Directory.EnumerateFileSystemEntries(
                rutaTemporal,
                "*",
                SearchOption.TopDirectoryOnly)) {
            FileAttributes atributos = File.GetAttributes(entrada);

            if (atributos.HasFlag(FileAttributes.Directory) &&
                !atributos.HasFlag(FileAttributes.ReparsePoint)) {
                EliminarArbolTemporal(entrada);
                continue;
            }

            if (atributos.HasFlag(FileAttributes.Directory)) {
                Directory.Delete(entrada, recursive: false);
            } else {
                File.SetAttributes(
                    entrada,
                    FileAttributes.Normal);
                File.Delete(entrada);
            }
        }

        File.SetAttributes(
            rutaTemporal,
            File.GetAttributes(rutaTemporal) &
                ~FileAttributes.ReadOnly);
        Directory.Delete(rutaTemporal, recursive: false);
    }

    [DllImport(
        "kernel32.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true,
        EntryPoint = "CreateDirectoryW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateDirectoryW(
        string lpPathName,
        IntPtr lpSecurityAttributes);

}
