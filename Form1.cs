using System.Windows.Forms;

namespace ProjectCreator {

    public partial class frmPrincipal : Form {

        private string rutaBase = "";
        private string rutaPlantilla = "";

        // Cargar la configuración desde el archivo config.txt
        private void CargarConfiguracion() {
            string rutaConfig =
            @"C:\Users\jeanc\source\repos\Plantillas\ProjectCreator\config.txt";

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
        }

        public frmPrincipal() {
            InitializeComponent();
            CargarConfiguracion();
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
            cmbTemas.Items.Add("01_Variables");
            cmbTemas.Items.Add("02_Condicionales");
            cmbTemas.Items.Add("03_Ciclos");
            cmbTemas.Items.Add("04_Funciones");
            cmbTemas.Items.Add("05_Strings");
            cmbTemas.Items.Add("06_Arrays");
            cmbTemas.Items.Add("07_Structs");
            cmbTemas.Items.Add("08_Vectores");
            cmbTemas.Items.Add("09_Archivos");
            cmbTemas.Items.Add("10_POO");
            cmbTemas.SelectedIndex = 0;

            btnCrearProyecto.Enabled = false;

            string rutaImagen =
            @"C:\Users\jeanc\source\repos\Plantillas\ProjectCreator\Recursos\FondodeProyectoAutocpp.png";

            if (File.Exists(rutaImagen)) {
                pictureBoxfondo.Image = Image.FromFile(rutaImagen);
                pictureBoxfondo.SendToBack();
            } else {
                MessageBox.Show("No se encontró la imagen:\n" + rutaImagen);
            }
        }

        private void ActualizarVistaPrevia() {
            if (cmbTemas.SelectedItem == null || txtNombreProyecto.Text.Trim() == "") {
                lblNombreFinal.Text = "Esperando datos...";
                return;
            }

            string temaSeleccionado = cmbTemas.Text;
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
            string temaSeleccionado = cmbTemas.Text;
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
            Ejercicio creado automáticamente mediante ProjectCreator.";

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

            txtNombreProyecto.Clear();
            txtObjetivo.Clear();

            MessageBox.Show("Proyecto creado correctamente.");

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
    }
}