using System;
using System.IO;
namespace EndForge.Services;

public class ProyectoService {

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

            if (extension == ".sln" || extension == ".vcxproj" || extension == ".filters" ||
                extension == ".cpp" || extension == ".user") {

                string contenido = File.ReadAllText(archivo);
                contenido = contenido.Replace("00_Plantilla", nombreProyecto);
                File.WriteAllText(archivo, contenido);
            }
        }
    }

    // =============================
    // Copia y preparación de la plantilla
    // =============================
    public void CopiarPlantilla(string rutaPlantilla, string rutaProyecto) {
        // TODO: Sustituir por un sistema de copia inteligente.
        foreach (string archivo in Directory.GetFiles(rutaPlantilla,
            "*",
            SearchOption.AllDirectories)) {

            string rutaRelativa = Path.GetRelativePath(rutaPlantilla, archivo);

            if (Path.GetExtension(archivo).Equals(".user", StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            if (rutaRelativa.StartsWith(".vs" + Path.DirectorySeparatorChar) ||
                rutaRelativa.StartsWith("x64" + Path.DirectorySeparatorChar)) {
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


    public void CrearProyecto(string rutaPlantilla, string rutaProyecto, string nombreProyecto, string temaSeleccionado, string objetivo) {
        if (Directory.Exists(rutaProyecto) || File.Exists(rutaProyecto)) {
            throw new ProyectoDestinoExistenteException(rutaProyecto);
        }

        string? carpetaPadre = Path.GetDirectoryName(rutaProyecto);

        if (string.IsNullOrWhiteSpace(carpetaPadre) || !Directory.Exists(carpetaPadre)) {
            throw new DirectoryNotFoundException("No existe la carpeta donde se creará la práctica.");
        }

        string rutaTemporal;

        do {
            rutaTemporal = Path.Combine(carpetaPadre, $".endforge-{Guid.NewGuid():N}.tmp");
        } while (Directory.Exists(rutaTemporal) || File.Exists(rutaTemporal));

        bool carpetaTemporalCreada = false;

        try {
            Directory.CreateDirectory(rutaTemporal);
            carpetaTemporalCreada = true;

            CopiarPlantilla(rutaPlantilla, rutaTemporal);

            RenombrarArchivos(rutaTemporal, nombreProyecto);

            RenombrarCarpetas(rutaTemporal, nombreProyecto);

            ActualizarReferencias(rutaTemporal, nombreProyecto);

            CrearReadme(rutaTemporal, nombreProyecto, temaSeleccionado, objetivo);

            if (Directory.Exists(rutaProyecto) || File.Exists(rutaProyecto)) {
                throw new ProyectoDestinoExistenteException(rutaProyecto);
            }

            try {
                Directory.Move(rutaTemporal, rutaProyecto);
            } catch (IOException ex) when (Directory.Exists(rutaProyecto) || File.Exists(rutaProyecto)) {
                throw new ProyectoDestinoExistenteException(rutaProyecto, ex);
            }

            carpetaTemporalCreada = false;
        } catch {
            if (carpetaTemporalCreada && Directory.Exists(rutaTemporal)) {
                try {
                    Directory.Delete(rutaTemporal, true);
                } catch {
                    // Evita ocultar el error original de creación.
                }
            }

            throw;
        }
    }

}
