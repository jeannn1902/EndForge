using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
namespace ProjectCreator {

    public partial class frmPrincipal : Form {
        private Panel panelSeleccionado;
        private string rutaBase = "";
        private string rutaPlantilla = "";
        private string rutaRecientes = "";

        private class ProyectoReciente {
            public string Nombre { get; set; } = "";
            public string Ruta { get; set; } = "";

            public override string ToString() {
                return Nombre;
            }
        }

        // Cargar la configuración desde el archivo config.txt
        private void CargarConfiguracion() {
            string rutaConfig = @"C:\Users\jeanc\source\repos\Plantillas\ProjectCreator\config.txt";
            rutaRecientes = Path.Combine(Path.GetDirectoryName(rutaConfig)!, "recientes.txt");

            if (!File.Exists(rutaConfig)) {
                MessageBox.Show("No se encontró el archivo config.txt");
                Application.Exit();
                return;
            }

            string[] lineas = File.ReadAllLines(rutaConfig);

            if (lineas.Length < 2) {
                MessageBox.Show("El archivo config.txt está incompleto.");
                Application.Exit();
                return;
            }

            rutaBase = lineas[0];
            rutaPlantilla = lineas[1];

            if (!File.Exists(rutaRecientes)) {
                File.Create(rutaRecientes).Close();
            }
        }

        private void GuardarProyectoReciente(string rutaProyecto) {
            List<string> recientes = new List<string>();

            if (File.Exists(rutaRecientes)) {
                recientes = File.ReadAllLines(rutaRecientes).ToList();
            }

            string nombreProyecto = Path.GetFileName(rutaProyecto);
            string registro = nombreProyecto + "|" + rutaProyecto;

            recientes.RemoveAll(x => x.EndsWith("|" + rutaProyecto));
            recientes.Insert(0, registro);

            if (recientes.Count > 10) {
                recientes = recientes.Take(10).ToList();
            }

            File.WriteAllLines(rutaRecientes, recientes);
        }

        private void CargarRecientes() {
            listRecientes.Items.Clear();

            if (!File.Exists(rutaRecientes))
                return;

            string[] recientes = File.ReadAllLines(rutaRecientes);

            foreach (string reciente in recientes) {
                string[] datos = reciente.Split('|');

                if (datos.Length >= 2) {
                    ProyectoReciente proyecto = new ProyectoReciente {
                        Nombre = datos[0],
                        Ruta = datos[1]
                    };

                    listRecientes.Items.Add(proyecto);
                }
            }
        }

        public frmPrincipal() {
            InitializeComponent();
            CargarConfiguracion();

            btnCrearProyecto.MouseEnter += BtnCrearProyecto_MouseEnter;
            btnCrearProyecto.MouseLeave += BtnCrearProyecto_MouseLeave;

            // Paneles del menú

            // Inicio
            panelInicio.MouseEnter += PanelMenu_MouseEnter;
            panelInicio.MouseLeave += PanelMenu_MouseLeave;
            lblInicio.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxInicio.MouseLeave += PanelMenu_MouseLeave;
            panelInicio.Click += PanelInicio_Click;
            lblInicio.Click += PanelInicio_Click;
            pictureBoxInicio.Click += PanelInicio_Click;

            // Nueva práctica
            panelNuevaPractica.MouseEnter += PanelMenu_MouseEnter;
            panelNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
            lblNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
            panelNuevaPractica.Click += panelNuevaPractica_Click;
            lblNuevaPractica.Click += panelNuevaPractica_Click;
            pictureBoxNuevaPractica.Click += panelNuevaPractica_Click;

            // Abrir práctica
            panelAbrirPractica.MouseEnter += PanelMenu_MouseEnter;
            panelAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
            panelAbrirPractica.Click += PanelAbrirPractica_Click;
            lblAbrirPractica.Click += PanelAbrirPractica_Click;
            pictureBoxAbrirPractica.Click += PanelAbrirPractica_Click;
            lblAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxAbrirPractica.MouseLeave += PanelMenu_MouseLeave;

            // Recientes
            panelRecientes.MouseEnter += PanelMenu_MouseEnter;
            panelRecientes.MouseLeave += PanelMenu_MouseLeave;
            panelRecientes.Click += PanelRecientes_Click;
            lblRecientes.Click += PanelRecientes_Click;
            pictureBoxRecientes.Click += PanelRecientes_Click;
            lblRecientes.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxRecientes.MouseLeave += PanelMenu_MouseLeave;

            // Configuración
            panelConfiguracion.MouseEnter += PanelMenu_MouseEnter;
            panelConfiguracion.MouseLeave += PanelMenu_MouseLeave;
            panelConfiguracion.Click += PanelConfiguracion_Click;
            lblConfiguracion.Click += PanelConfiguracion_Click;
            pictureBoxConfiguracion.Click += PanelConfiguracion_Click;
            lblConfiguracion.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxConfiguracion.MouseLeave += PanelMenu_MouseLeave;

            // Acerca de
            panelAcercaDe.MouseEnter += PanelMenu_MouseEnter;
            panelAcercaDe.MouseLeave += PanelMenu_MouseLeave;
            panelAcercaDe.Click += PanelAcercaDe_Click;
            lblAcercaDe.Click += PanelAcercaDe_Click;
            pictureBoxAcercaDe.Click += PanelAcercaDe_Click;
            lblAcercaDe.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxAcercaDe.MouseLeave += PanelMenu_MouseLeave;

            // Terminan los paneles del menú

            panelSeleccionado = panelInicio;
            panelInicio.BackColor = Color.FromArgb(111, 45, 189);
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;
        }

        private void PanelMenu_MouseEnter(object? sender, EventArgs e) {
            Panel? panel = sender as Panel;

            if (panel != null) {
                if (panel != panelSeleccionado) {
                    panel.BackColor = Color.FromArgb(74, 35, 110);
                }
            }
        }

        private void PanelMenu_MouseLeave(object? sender, EventArgs e) {
            Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

            if (panel != null) {
                if (!panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position))) {

                    if (panel != panelSeleccionado) {
                        panel.BackColor = Color.FromArgb(45, 45, 48);
                    }
                }
            }
        }

        private void SeleccionarPanelMenu(Panel panel) {
            if (panelSeleccionado == panel) {
                return;
            }

            panelSeleccionado.BackColor = Color.FromArgb(45, 45, 48);
            panel.BackColor = Color.FromArgb(111, 45, 189);

            panelSeleccionado = panel;
        }

        private void panelNuevaPractica_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelNuevaPractica);

            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;
        }

        private void PanelInicio_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelInicio);

            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;
        }

        private void PanelAbrirPractica_Click(object? sender, EventArgs e) {
            using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
                carpeta.Description = "Selecciona la carpeta del proyecto";

                if (carpeta.ShowDialog() != DialogResult.OK) {
                    RestaurarColorPanel(panelAbrirPractica);
                    return;
                }

                string[] soluciones = Directory.GetFiles(
                    carpeta.SelectedPath,
                    "*.sln",
                    SearchOption.TopDirectoryOnly);

                if (soluciones.Length == 0) {
                    MessageBox.Show("No se encontró ningún archivo .sln en la carpeta seleccionada.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RestaurarColorPanel(panelAbrirPractica);
                    return;
                }

                SeleccionarPanelMenu(panelAbrirPractica);

                Process.Start(new ProcessStartInfo() {
                    FileName = soluciones[0],
                    UseShellExecute = true
                });
            }
        }

        private void RestaurarColorPanel(Panel panel) {
            if (panel == panelSeleccionado) {
                panel.BackColor = Color.ForestGreen;
            } else {
                panel.BackColor = Color.FromArgb(45, 45, 48);
            }
        }

        private void PanelRecientes_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelRecientes);

            panelConfiguracionVista.Visible = false;
            panelRecientesVista.Visible = true;
            panelRecientesVista.BringToFront();

            CargarRecientes();
        }

        private void PanelConfiguracion_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelConfiguracion);

            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = true;
            panelConfiguracionVista.BringToFront();

            txtRutaBaseConfig.Text = rutaBase;
            txtRutaPlantillaConfig.Text = rutaPlantilla;
        }

        private void PanelAcercaDe_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelAcercaDe);

            MessageBox.Show(
                "EndForge 1.0\n\n" +
                "Desarrollado por:\n" +
                "Jeancarlo Pérez Pérez\n\n" +
                "Herramienta para automatizar la creación y gestión " +
                "de prácticas de C++.\n\n" +
                "© 2026",
                "Acerca de EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnCrearProyecto_MouseEnter(object? sender, EventArgs e) {
            btnCrearProyecto.BackColor = Color.FromArgb(56, 142, 60);
        }

        private void BtnCrearProyecto_MouseLeave(object? sender, EventArgs e) {
            btnCrearProyecto.BackColor = Color.FromArgb(46, 125, 50);
        }

        private void Label1_Click(object sender, EventArgs e) {
        }

        private void Label2_Click(object sender, EventArgs e) {
        }

        private void Label1_Click_1(object sender, EventArgs e) {
        }

        private void LblVistaPrevia_Click(object sender, EventArgs e) {
        }

        private void TxtNombreProyecto_TextChanged(object sender, EventArgs e) {
            ActualizarVistaPrevia();
            ValidarFormulario();
        }

        private void FrmPrincipal_Load(object sender, EventArgs e) {
            txtTemas.Items.Add("01_Variables");
            txtTemas.Items.Add("02_Condicionales");
            txtTemas.Items.Add("03_Ciclos");
            txtTemas.Items.Add("04_Funciones");
            txtTemas.Items.Add("05_Strings");
            txtTemas.Items.Add("06_Arrays");
            txtTemas.Items.Add("07_Structs");
            txtTemas.Items.Add("08_Vectores");
            txtTemas.Items.Add("09_Archivos");
            txtTemas.Items.Add("10_POO");
            txtTemas.SelectedIndex = 0;

            btnCrearProyecto.Enabled = false;

            string rutaImagen =
            @"C:\Users\jeanc\source\repos\Plantillas\ProjectCreator\Recursos\FondodeProyectoAutocpp.png";

            if (File.Exists(rutaImagen)) {
                pictureBoxfondo.Image = Image.FromFile(rutaImagen);
                pictureBoxfondo.SendToBack();
            } else {
                MessageBox.Show("No se encontró la imagen:\n" + rutaImagen);
            }

            panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

        }

        private void ActualizarVistaPrevia() {
            if (txtTemas.SelectedItem == null || txtNombreProyecto.Text.Trim() == "") {
                lblNombreFinal.Text = "Esperando datos...";
                return;
            }

            string temaSeleccionado = txtTemas.Text;
            string rutaTema = Path.Combine(rutaBase, temaSeleccionado);
            int siguienteNumero = 1;

            if (Directory.Exists(rutaTema)) {
                string[] carpetas = Directory.GetDirectories(rutaTema);
                siguienteNumero = carpetas.Length + 1;
            }

            string numeroFormateado = siguienteNumero.ToString("00");
            lblNombreFinal.Text = numeroFormateado + "_" + txtNombreProyecto.Text.Trim();
        }

        private void ValidarFormulario() {
            btnCrearProyecto.Enabled = txtNombreProyecto.Text.Trim() != "" &&
            txtObjetivo.Text.Trim() != "";
        }

        private void CmbTemas_SelectedIndexChanged(object sender, EventArgs e) {
            ActualizarVistaPrevia();
        }

        private void BtnCrearProyecto_Click(object sender, EventArgs e) {
            string temaSeleccionado = txtTemas.Text;
            string nombreProyecto = lblNombreFinal.Text;
            string nombreUsuario = txtNombreProyecto.Text.Trim();

            if (nombreUsuario == "") {
                MessageBox.Show("Escribe un nombre para el proyecto.");
                return;
            }

            char[] caracteresInvalidos = Path.GetInvalidFileNameChars();

            if (nombreUsuario.IndexOfAny(caracteresInvalidos) >= 0) {
                MessageBox.Show("El nombre contiene caracteres no válidos.");
                return;
            }

            string rutaProyecto = Path.Combine(rutaBase, temaSeleccionado, nombreProyecto);

            if (Directory.Exists(rutaProyecto)) {
                MessageBox.Show("El proyecto ya existe.");
                return;
            }

            Directory.CreateDirectory(rutaProyecto);

            // Copiar archivos de la plantilla al nuevo proyecto
            foreach (string archivo in Directory.GetFiles(rutaPlantilla)) {
                string nombreArchivo = Path.GetFileName(archivo);
                string destino = Path.Combine(rutaProyecto, nombreArchivo);
                File.Copy(archivo, destino);
            }

            // Renombrar archivos que contienen "00_Plantilla" en su nombre
            foreach (string archivo in Directory.GetFiles(rutaProyecto)) {
                string nombreArchivo = Path.GetFileName(archivo);

                if (nombreArchivo.Contains("00_Plantilla")) {
                    string nuevoNombre = nombreArchivo.Replace("00_Plantilla", nombreProyecto);
                    string nuevaRuta = Path.Combine(rutaProyecto, nuevoNombre);
                    File.Move(archivo, nuevaRuta);
                }
            }


            // Reemplazar "00_Plantilla" en el contenido de los archivos
            foreach (string archivo in Directory.GetFiles(rutaProyecto)) {
                string extension = Path.GetExtension(archivo);

                if (extension == ".sln" || extension == ".vcxproj" || extension == ".filters" ||
                    extension == ".cpp" || extension == ".user") {

                    string contenido = File.ReadAllText(archivo);
                    contenido = contenido.Replace("00_Plantilla", nombreProyecto);
                    File.WriteAllText(archivo, contenido);
                }
            }

            //Crear el archivo README.md
            string contenidoReadme =
            $@"# {nombreProyecto}

            ## Tema
            {temaSeleccionado}

            ## Objetivo
            {txtObjetivo.Text.Trim()}

            ## Fecha de creación
            {DateTime.Now:dd/MM/yyyy}

            ## Descripción
            Ejercicio creado automáticamente mediante EndForge.";

            string rutaReadme = Path.Combine(rutaProyecto, "README.md");

            File.WriteAllText(rutaReadme, contenidoReadme);
            string rutaSolucion = Path.Combine(rutaProyecto, nombreProyecto + ".sln");

            if (!File.Exists(rutaSolucion)) {
                MessageBox.Show("No se encontró la solución:\n" + rutaSolucion);
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
                FileName = rutaSolucion,
                UseShellExecute = true
            });

            GuardarProyectoReciente(rutaProyecto);

            txtNombreProyecto.Clear();
            txtObjetivo.Clear();

            MessageBox.Show(
                "El proyecto se creó correctamente.\n\n¡Visual Studio se abrirá automáticamente!",
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        }

        private void LblNombreFinal_Click(object sender, EventArgs e) {
        }

        private void Label1_Click_2(object sender, EventArgs e) {
        }

        private void TxtObjetivo_TextChanged(object sender, EventArgs e) {
            ValidarFormulario();
        }

        private void PictureBox1_Click(object sender, EventArgs e) {
        }

        private void LblObjetivo_Click(object sender, EventArgs e) {

        }

        private void PanelControles_Paint(object sender, PaintEventArgs e) {

        }

        private void PanelMenu_Paint(object sender, PaintEventArgs e) {

        }

        private void LblInicio_Click(object sender, EventArgs e) {

        }

        private void ListRecientes_DoubleClick(object sender, EventArgs e) {
            if (listRecientes.SelectedItem == null)
                return;

            ProyectoReciente proyecto = (ProyectoReciente)listRecientes.SelectedItem;

            string? rutaSolucion = Directory.GetFiles(proyecto.Ruta, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (rutaSolucion == null) {
                MessageBox.Show(
                    "No se encontró la solución del proyecto.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Process.Start(new ProcessStartInfo() {
                FileName = rutaSolucion,
                UseShellExecute = true
            });

            GuardarProyectoReciente(proyecto.Ruta);
            CargarRecientes();
        }

        private void BtnCambiarRutaPlantilla_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
                carpeta.Description = "Selecciona la carpeta base de tus proyectos";

                if (carpeta.ShowDialog() == DialogResult.OK) {
                    txtRutaBaseConfig.Text = carpeta.SelectedPath;
                }
            }
        }

        private void BtnCambiarRutaBase_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
                carpeta.Description = "Selecciona la carpeta de la plantilla";

                if (carpeta.ShowDialog() == DialogResult.OK) {
                    txtRutaPlantillaConfig.Text = carpeta.SelectedPath;
                }
            }
        }

        private void BtnGuardarConfiguracion_Click(object sender, EventArgs e) {
            if (!Directory.Exists(txtRutaBaseConfig.Text) || !Directory.Exists(txtRutaPlantillaConfig.Text)) {
                MessageBox.Show("Una de las rutas seleccionadas no existe.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rutaConfig = @"C:\Users\jeanc\source\repos\Plantillas\ProjectCreator\config.txt";

            File.WriteAllLines(rutaConfig, new string[] {
                txtRutaBaseConfig.Text, txtRutaPlantillaConfig.Text
            });

            rutaBase = txtRutaBaseConfig.Text;
            rutaPlantilla = txtRutaPlantillaConfig.Text;

            MessageBox.Show("Configuración guardada correctamente.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ListRecientes_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void LblAyudaRecientes_Click(object sender, EventArgs e) {

        }

        private void ListRecientes_SelectedIndexChanged_1(object sender, EventArgs e) {

        }
    }
}