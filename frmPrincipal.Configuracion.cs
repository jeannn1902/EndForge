using EndForge.Models;
using System.Security;

namespace EndForge;

public partial class frmPrincipal {
    private bool actualizandoCamposConfiguracion;

    private ResultadoCargaConfiguracion CargarConfiguracion() {
        ResultadoCargaConfiguracion resultado = configuracionService.CargarConfiguracion();

        if (resultado.Estado == EstadoCargaConfiguracion.ErrorPermisosConfiguracion) {
            MessageBox.Show(
                "No se pudo leer la configuración de EndForge porque no hay permisos para acceder a config.txt.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return resultado;
        }

        if (resultado.Estado == EstadoCargaConfiguracion.ErrorLecturaConfiguracion) {
            MessageBox.Show(
                "No se pudo leer la configuración. Verifica que config.txt no esté bloqueado o en uso por otra aplicación.",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return resultado;
        }

        if (!resultado.ConfiguracionDisponible) {
            rutaBase = "";
            rutaPlantilla = "";
            EstablecerRutasConfiguracionEnVista(rutaBase, rutaPlantilla);
            lblEstadoConfiguracion.Visible = false;
            ValidarFormulario();
            return resultado;
        }

        rutaBase = resultado.RutaBase;
        rutaPlantilla = resultado.RutaPlantilla;
        EstablecerRutasConfiguracionEnVista(rutaBase, rutaPlantilla);

        if (resultado.Estado == EstadoCargaConfiguracion.ErrorPermisosRecientes) {
            MessageBox.Show("No se pudo crear recientes.txt porque no hay permisos de acceso.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        } else if (resultado.Estado == EstadoCargaConfiguracion.ErrorCreacionRecientes) {
            MessageBox.Show("No se pudo crear recientes.txt porque el archivo está bloqueado o en uso.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        ValidarFormulario();

        if (!string.IsNullOrWhiteSpace(rutaBase) && !string.IsNullOrWhiteSpace(rutaPlantilla)) {
            lblEstadoConfiguracion.Text = "✅ Configuración lista.";
            lblEstadoConfiguracion.ForeColor = Color.LightGreen;
            lblEstadoConfiguracion.Visible = true;
        } else {
            lblEstadoConfiguracion.Visible = false;
        }

        return resultado;
    }

    private void BtnCambiarRutaPlantilla_Click(object sender, EventArgs e) {
        using FolderBrowserDialog carpeta = new() {
            Description = "Selecciona la carpeta de la plantilla"
        };

        if (Directory.Exists(txtRutaPlantillaConfig.Text)) {
            carpeta.SelectedPath = txtRutaPlantillaConfig.Text;
        }

        if (carpeta.ShowDialog() == DialogResult.OK) {
            txtRutaPlantillaConfig.Text = carpeta.SelectedPath;
        }
    }

    private void BtnCambiarRutaBase_Click(object sender, EventArgs e) {
        using FolderBrowserDialog carpeta = new() {
            Description = "Selecciona la carpeta base de tus proyectos"
        };

        if (Directory.Exists(txtRutaBaseConfig.Text)) {
            carpeta.SelectedPath = txtRutaBaseConfig.Text;
        }

        if (carpeta.ShowDialog() == DialogResult.OK) {
            txtRutaBaseConfig.Text = carpeta.SelectedPath;
        }
    }

    private void RutaConfiguracion_TextChanged(object sender, EventArgs e) {
        if (actualizandoCamposConfiguracion) {
            return;
        }

        bool hayCambios = ActualizarEstadoCambiosConfiguracion();

        if (hayCambios) {
            lblEstadoConfiguracion.Visible = false;
        }
    }

    private void BtnGuardarConfiguracion_Click(object sender, EventArgs e) {
        if (!HayCambiosConfiguracion()) {
            ActualizarEstadoCambiosConfiguracion();
            return;
        }

        lblEstadoConfiguracion.Visible = false;
        string nuevaRutaBase = txtRutaBaseConfig.Text.Trim();
        string nuevaRutaPlantilla = txtRutaPlantillaConfig.Text.Trim();
        EstadoValidacionConfiguracion validacion = configuracionService.ValidarConfiguracion(
            nuevaRutaBase,
            nuevaRutaPlantilla
        );

        if (validacion != EstadoValidacionConfiguracion.Valida) {
            lblEstadoConfiguracion.Text = ObtenerMensajeValidacionConfiguracion(validacion);
            lblEstadoConfiguracion.ForeColor = validacion == EstadoValidacionConfiguracion.RutasNoExistentes
                ? Color.IndianRed
                : Color.LightCoral;
            lblEstadoConfiguracion.Visible = true;
            return;
        }

        try {
            configuracionService.GuardarConfiguracion(nuevaRutaBase, nuevaRutaPlantilla);
        } catch (UnauthorizedAccessException) {
            MostrarErrorGuardadoConfiguracion(
                "❌ No se pudieron guardar los cambios porque no hay permisos para acceder a config.txt. La configuración anterior se conservó."
            );
            return;
        } catch (SecurityException) {
            MostrarErrorGuardadoConfiguracion(
                "❌ No se pudieron guardar los cambios porque no hay permisos para acceder a config.txt. La configuración anterior se conservó."
            );
            return;
        } catch (IOException) {
            MostrarErrorGuardadoConfiguracion(
                "❌ No se pudieron guardar los cambios. Verifica que config.txt no esté bloqueado o en uso por otra aplicación. La configuración anterior se conservó."
            );
            return;
        } catch (Exception ex) {
            MostrarErrorGuardadoConfiguracion(
                "❌ No se pudieron guardar los cambios. La configuración anterior se conservó.\n" + ex.Message
            );
            return;
        }

        rutaBase = nuevaRutaBase;
        rutaPlantilla = nuevaRutaPlantilla;
        EstablecerRutasConfiguracionEnVista(rutaBase, rutaPlantilla);

        CargarTemas();
        ValidarFormulario();

        lblEstadoConfiguracion.Text = "✅ Cambios guardados.";
        lblEstadoConfiguracion.ForeColor = Color.LightGreen;
        lblEstadoConfiguracion.Visible = true;
    }

    private void BtnRestaurarConfiguracion_Click(object sender, EventArgs e) {
        if (!HayCambiosConfiguracion()) {
            ActualizarEstadoCambiosConfiguracion();
            return;
        }

        EstablecerRutasConfiguracionEnVista(rutaBase, rutaPlantilla);
        lblEstadoConfiguracion.Text = "Cambios descartados.";
        lblEstadoConfiguracion.ForeColor = Color.Gainsboro;
        lblEstadoConfiguracion.Visible = true;
    }

    private void EstablecerRutasConfiguracionEnVista(string rutaBaseGuardada, string rutaPlantillaGuardada) {
        actualizandoCamposConfiguracion = true;

        try {
            txtRutaBaseConfig.Text = rutaBaseGuardada;
            txtRutaPlantillaConfig.Text = rutaPlantillaGuardada;
        } finally {
            actualizandoCamposConfiguracion = false;
        }

        ActualizarEstadoCambiosConfiguracion();
    }

    private bool ActualizarEstadoCambiosConfiguracion() {
        bool hayCambios = HayCambiosConfiguracion();
        btnGuardarConfiguracion.Enabled = hayCambios;
        btnRestaurarConfiguracion.Enabled = hayCambios;
        return hayCambios;
    }

    private bool HayCambiosConfiguracion() {
        return !RutasEquivalentes(txtRutaBaseConfig.Text, rutaBase) ||
            !RutasEquivalentes(txtRutaPlantillaConfig.Text, rutaPlantilla);
    }

    private static bool RutasEquivalentes(string primeraRuta, string segundaRuta) {
        string primeraNormalizada = NormalizarRutaParaComparacion(primeraRuta);
        string segundaNormalizada = NormalizarRutaParaComparacion(segundaRuta);
        return primeraNormalizada.Equals(segundaNormalizada, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizarRutaParaComparacion(string ruta) {
        if (string.IsNullOrWhiteSpace(ruta)) {
            return "";
        }

        string rutaSinEspacios = ruta.Trim();

        try {
            string rutaCompleta = Path.GetFullPath(rutaSinEspacios);
            string? raiz = Path.GetPathRoot(rutaCompleta);

            if (!string.IsNullOrEmpty(raiz) &&
                rutaCompleta.Equals(raiz, StringComparison.OrdinalIgnoreCase)) {
                return rutaCompleta;
            }

            return rutaCompleta.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );
        } catch (Exception) {
            return rutaSinEspacios;
        }
    }

    private void MostrarErrorGuardadoConfiguracion(string mensaje) {
        lblEstadoConfiguracion.Text = mensaje;
        lblEstadoConfiguracion.ForeColor = Color.LightCoral;
        lblEstadoConfiguracion.Visible = true;
    }

    private static string ObtenerMensajeValidacionConfiguracion(EstadoValidacionConfiguracion validacion) {
        return validacion switch {
            EstadoValidacionConfiguracion.RutasNoExistentes => "❌ Una de las rutas seleccionadas no existe.",
            EstadoValidacionConfiguracion.PlantillaSinSolucion => "❌ La plantilla no contiene una solución .sln en su carpeta raíz.",
            EstadoValidacionConfiguracion.PlantillaSolucionSinMarcador => "❌ El nombre de la solución debe contener 00_Plantilla.",
            EstadoValidacionConfiguracion.PlantillaSolucionSinReferenciaMarcador => "❌ La solución .sln no contiene una referencia con 00_Plantilla.",
            EstadoValidacionConfiguracion.PlantillaSinProyectoCpp => "❌ La plantilla no contiene ningún archivo .vcxproj.",
            EstadoValidacionConfiguracion.PlantillaProyectoSinMarcador => "❌ El nombre del proyecto .vcxproj debe contener 00_Plantilla.",
            EstadoValidacionConfiguracion.PlantillaProyectoReferenciadoNoDisponible => "❌ La solución referencia un proyecto .vcxproj inexistente o ubicado fuera de la plantilla.",
            EstadoValidacionConfiguracion.PlantillaProyectoSinReferenciaMarcador => "❌ El proyecto .vcxproj no contiene referencias con 00_Plantilla.",
            EstadoValidacionConfiguracion.PlantillaProyectoXmlInvalido => "❌ El archivo .vcxproj no contiene XML válido.",
            EstadoValidacionConfiguracion.PlantillaSinArchivosCpp => "❌ No se encontraron archivos C++.",
            EstadoValidacionConfiguracion.ErrorLecturaPlantilla => "❌ No se pudo leer la plantilla. Verifica sus permisos y que los archivos no estén bloqueados.",
            _ => "❌ La plantilla no es compatible con EndForge."
        };
    }
}
