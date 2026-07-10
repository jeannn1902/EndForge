using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
namespace ProjectCreator {

    public partial class frmPrincipal : Form {
        private Panel panelSeleccionado;
        private string rutaBase = "";
        private string rutaPlantilla = "";
        private string rutaRecientes = "";

        private System.Windows.Forms.Timer timerRecalcularVista = new System.Windows.Forms.Timer();

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private void ActivarBarraTituloOscura() {
            if (Environment.OSVersion.Version.Major >= 10) {
                int usarModoOscuro = 1;
                DwmSetWindowAttribute(this.Handle, 20, ref usarModoOscuro, sizeof(int));
            }
        }

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

        private List<Label> ObtenerLabelsRecientes() {
            return new List<Label> {
                lblReciente1,
                lblReciente2,
                lblReciente3,
                lblReciente4,
                lblReciente5,
                lblReciente6,
                lblReciente7,
                lblReciente8,
                lblReciente9,
                lblReciente10
            };
        }

        private void LimpiarLabelsRecientes() {
            foreach (Label label in ObtenerLabelsRecientes()) {
                label.Text = "";
                label.Visible = false;
                label.Tag = null;
            }
        }

        private void LabelReciente_DoubleClick(object? sender, EventArgs e) {
            Label? label = sender as Label;

            if (label?.Tag is not ProyectoReciente proyecto)
                return;

            if (!Directory.Exists(proyecto.Ruta)) {
                MessageBox.Show(
                    "La carpeta de esta práctica ya no existe.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            string[] soluciones = Directory.GetFiles(proyecto.Ruta, "*.sln");

            if (soluciones.Length == 0) {
                MessageBox.Show(
                    "No se encontró ningún archivo .sln en esta práctica.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            Process.Start(new ProcessStartInfo {
                FileName = soluciones[0],
                UseShellExecute = true
            });

            GuardarProyectoReciente(proyecto.Ruta);
            CargarRecientes();
        }

        private void CargarRecientes() {
            listRecientes.Items.Clear();
            LimpiarLabelsRecientes();

            if (!File.Exists(rutaRecientes))
                return;

            string[] recientes = File.ReadAllLines(rutaRecientes);
            List<Label> labelsRecientes = ObtenerLabelsRecientes();

            int indice = 0;

            foreach (string reciente in recientes) {
                string[] datos = reciente.Split('|');

                if (datos.Length >= 2) {
                    ProyectoReciente proyecto = new ProyectoReciente {
                        Nombre = datos[0],
                        Ruta = datos[1]
                    };

                    listRecientes.Items.Add(proyecto);

                    if (indice < labelsRecientes.Count) {
                        labelsRecientes[indice].Text = proyecto.Nombre;
                        labelsRecientes[indice].Tag = proyecto;
                        labelsRecientes[indice].Visible = true;
                        indice++;
                    }
                }
            }
        }

        public frmPrincipal() {
            InitializeComponent();

            PosicionarBotonesBarraTitulo();
            panelBarraTitulo.SizeChanged += (s, e) => PosicionarBotonesBarraTitulo();
            //
            // Conexion de clicks de botones minimizar, maximizar y cerrar
            //
            btnMinimizar.Click += BtnMinimizar_Click;
            btnMaximizar.Click += BtnMaximizar_Click;
            btnCerrar.Click += BtnCerrar_Click;
            //
            btnCerrar.MouseEnter += BtnCerrar_MouseEnter;
            btnCerrar.MouseLeave += BtnCerrar_MouseLeave;
            //
            btnMinimizar.MouseLeave += BtnVentana_MouseLeave;
            btnMaximizar.MouseLeave += BtnVentana_MouseLeave;
            //
            panelBarraTitulo.MouseDown += PanelBarraTitulo_MouseDown;
            lblBarraTitulo.MouseDown += PanelBarraTitulo_MouseDown;
            pictureBoxBarraIcono.MouseDown += PanelBarraTitulo_MouseDown;
            //
            // Hover de las tarjetas del inicio
            //
            lblCardNuevaPracticaTitulo.MouseEnter += CardInicio_MouseEnter;
            lblCardNuevaPracticaTitulo.MouseLeave += CardInicio_MouseLeave;
            lblCardNuevaPracticaDesc.MouseEnter += CardInicio_MouseEnter;
            lblCardNuevaPracticaDesc.MouseLeave += CardInicio_MouseLeave;

            lblCardRecientesTitulo.MouseEnter += CardInicio_MouseEnter;
            lblCardRecientesTitulo.MouseLeave += CardInicio_MouseLeave;
            lblCardRecientesDesc.MouseEnter += CardInicio_MouseEnter;
            lblCardRecientesDesc.MouseLeave += CardInicio_MouseLeave;

            lblCardConfiguracionTitulo.MouseEnter += CardInicio_MouseEnter;
            lblCardConfiguracionTitulo.MouseLeave += CardInicio_MouseLeave;
            lblCardConfiguracionDesc.MouseEnter += CardInicio_MouseEnter;
            lblCardConfiguracionDesc.MouseLeave += CardInicio_MouseLeave;
            // Fin de hover de las tarjetas del inicio
            //
            // Activar barra de título oscura en Windows 10 y versiones posteriores
            //
            ActivarBarraTituloOscura();
            //

            timerRecalcularVista.Interval = 150;
            timerRecalcularVista.Tick += TimerRecalcularVista_Tick;

            this.Resize += FrmPrincipal_Resize;

            ActivarDobleBuffer(this);
            ActivarDobleBuffer(panelPrincipal);
            ActivarDobleBuffer(panelInicioVista);
            ActivarDobleBuffer(panelRecientesVista);
            ActivarDobleBuffer(panelConfiguracionVista);
            ActivarDobleBuffer(panelListaRecientes);

            // panelPrincipal.LocationChanged += (s, e) => ReaplicarFondoDinamico();
            // panelPrincipal.SizeChanged += (s, e) => ReaplicarFondoDinamico();
            // fondoEndForge.SizeChanged += (s, e) => ReaplicarFondoDinamico();

            CargarConfiguracion();

            btnCrearProyecto.MouseEnter += BtnCrearProyecto_MouseEnter;
            btnCrearProyecto.MouseLeave += BtnCrearProyecto_MouseLeave;

            // Paneles del menú
            //
            // Inicio
            //
            panelInicio.MouseEnter += PanelMenu_MouseEnter;
            panelInicio.MouseLeave += PanelMenu_MouseLeave;
            lblInicio.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxInicio.MouseLeave += PanelMenu_MouseLeave;
            panelInicio.Click += PanelInicio_Click;
            lblInicio.Click += PanelInicio_Click;
            pictureBoxInicio.Click += PanelInicio_Click;
            //
            // Nueva práctica
            //
            panelNuevaPractica.MouseEnter += PanelMenu_MouseEnter;
            panelNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
            lblNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxNuevaPractica.MouseLeave += PanelMenu_MouseLeave;
            panelNuevaPractica.Click += panelNuevaPractica_Click;
            lblNuevaPractica.Click += panelNuevaPractica_Click;
            pictureBoxNuevaPractica.Click += panelNuevaPractica_Click;
            //
            // Abrir práctica
            //
            panelAbrirPractica.MouseEnter += PanelMenu_MouseEnter;
            panelAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
            panelAbrirPractica.Click += PanelAbrirPractica_Click;
            lblAbrirPractica.Click += PanelAbrirPractica_Click;
            pictureBoxAbrirPractica.Click += PanelAbrirPractica_Click;
            lblAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxAbrirPractica.MouseLeave += PanelMenu_MouseLeave;
            //
            // Recientes
            //
            panelRecientes.MouseEnter += PanelMenu_MouseEnter;
            panelRecientes.MouseLeave += PanelMenu_MouseLeave;
            panelRecientes.Click += PanelRecientes_Click;
            lblRecientes.Click += PanelRecientes_Click;
            pictureBoxRecientes.Click += PanelRecientes_Click;
            lblRecientes.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxRecientes.MouseLeave += PanelMenu_MouseLeave;
            //
            // Configuración
            //
            panelConfiguracion.MouseEnter += PanelMenu_MouseEnter;
            panelConfiguracion.MouseLeave += PanelMenu_MouseLeave;
            panelConfiguracion.Click += PanelConfiguracion_Click;
            lblConfiguracion.Click += PanelConfiguracion_Click;
            pictureBoxConfiguracion.Click += PanelConfiguracion_Click;
            lblConfiguracion.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxConfiguracion.MouseLeave += PanelMenu_MouseLeave;
            //
            // Acerca de
            //
            panelAcercaDe.MouseEnter += PanelMenu_MouseEnter;
            panelAcercaDe.MouseLeave += PanelMenu_MouseLeave;
            panelAcercaDe.Click += PanelAcercaDe_Click;
            lblAcercaDe.Click += PanelAcercaDe_Click;
            pictureBoxAcercaDe.Click += PanelAcercaDe_Click;
            lblAcercaDe.MouseLeave += PanelMenu_MouseLeave;
            pictureBoxAcercaDe.MouseLeave += PanelMenu_MouseLeave;
            //
            // Terminan los paneles del menú
            //
            // Tarjetas del Inicio
            //
            panelCardNuevaPractica.Click += panelNuevaPractica_Click;
            panelCardRecientes.Click += PanelRecientes_Click;
            panelCardConfiguracion.Click += PanelConfiguracion_Click;

            lblCardNuevaPracticaTitulo.Click += panelNuevaPractica_Click;
            lblCardNuevaPracticaDesc.Click += panelNuevaPractica_Click;

            lblCardRecientesTitulo.Click += PanelRecientes_Click;
            lblCardRecientesDesc.Click += PanelRecientes_Click;

            lblCardConfiguracionTitulo.Click += PanelConfiguracion_Click;
            lblCardConfiguracionDesc.Click += PanelConfiguracion_Click;
            //
            // Doble clic en los labels de recientes
            //
            foreach (Label label in ObtenerLabelsRecientes()) {
                label.DoubleClick += LabelReciente_DoubleClick;
            }
            //
            // Estado inicial
            //
            panelSeleccionado = panelInicio;
            panelInicio.BackColor = Color.FromArgb(111, 45, 189);
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;

            CentrarPanelPrincipal();
            AplicarFondoDinamicoPanelPrincipal();

            panelPrincipal.Invalidate(true);
            panelInicioVista.Invalidate(true);
            fondoEndForge.Invalidate(true);
        }

        // Activar el doble buffer para evitar parpadeos en los controles
        private void ActivarDobleBuffer(Control control) {
            typeof(Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(control, true, null);
        }

        private void AplicarFondoDinamicoPanelPrincipal() {
            panelPrincipal.BackgroundImage = CrearRecorteFondoParaPanel(panelPrincipal);
            panelPrincipal.BackgroundImageLayout = ImageLayout.Stretch;
            panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

            panelInicioVista.BackgroundImage = null;
            panelInicioVista.BackColor = Color.Transparent;

            panelRecientesVista.BackgroundImage = null;
            panelRecientesVista.BackColor = Color.Transparent;

            panelConfiguracionVista.BackgroundImage = null;
            panelConfiguracionVista.BackColor = Color.Transparent;

            panelListaRecientes.BackgroundImage = null;
            panelListaRecientes.BackColor = Color.Transparent;
        }

        private void ReaplicarFondoDinamico() {
            if (fondoEndForge.Width <= 0 || fondoEndForge.Height <= 0)
                return;

            if (panelPrincipal.Width <= 0 || panelPrincipal.Height <= 0)
                return;

            AplicarFondoDinamicoPanelPrincipal();
        }

        private void PosicionarBotonesBarraTitulo() {
            btnCerrar.Location = new Point(panelBarraTitulo.Width - btnCerrar.Width, 0);
            btnMaximizar.Location = new Point(btnCerrar.Left - btnMaximizar.Width, 0);
            btnMinimizar.Location = new Point(btnMaximizar.Left - btnMinimizar.Width, 0);
        }

        private void BtnMinimizar_Click(object? sender, EventArgs e) {
            WindowState = FormWindowState.Minimized;
        }

        private void BtnMaximizar_Click(object? sender, EventArgs e) {
            if (WindowState == FormWindowState.Maximized) {
                WindowState = FormWindowState.Normal;
                btnMaximizar.Text = "□";
            } else {
                WindowState = FormWindowState.Maximized;
                btnMaximizar.Text = "❐";
            }
        }

        private void BtnCerrar_Click(object? sender, EventArgs e) {
            Close();
        }

        private void BtnCerrar_MouseEnter(object? sender, EventArgs e) {
            btnCerrar.BackColor = Color.FromArgb(190, 40, 40);
            btnCerrar.ForeColor = Color.White;
        }

        private void BtnCerrar_MouseLeave(object? sender, EventArgs e) {
            btnCerrar.BackColor = Color.FromArgb(20, 16, 30);
            btnCerrar.ForeColor = Color.White;
        }

        private void BtnVentana_MouseLeave(object? sender, EventArgs e) {
            Button? boton = sender as Button;

            if (boton != null) {
                boton.BackColor = Color.FromArgb(20, 16, 30);
                boton.ForeColor = Color.White;
            }
        }

        private void PanelBarraTitulo_MouseDown(object? sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(this.Handle, 0x112, 0xf012, 0);
            }
        }

        private Image CrearRecorteFondoParaPanel(Control panelDestino) {
            Bitmap fondoRenderizado = new Bitmap(fondoEndForge.Width, fondoEndForge.Height);
            fondoEndForge.DrawToBitmap(
                fondoRenderizado,
                new Rectangle(0, 0, fondoEndForge.Width, fondoEndForge.Height)
            );

            Point posicionEnFondo = fondoEndForge.PointToClient(
                panelDestino.PointToScreen(Point.Empty)
            );

            Rectangle zonaRecorte = new Rectangle(
                posicionEnFondo.X,
                posicionEnFondo.Y,
                panelDestino.Width,
                panelDestino.Height
            );

            Bitmap recorte = new Bitmap(panelDestino.Width, panelDestino.Height);

            using (Graphics g = Graphics.FromImage(recorte)) {
                g.DrawImage(
                    fondoRenderizado,
                    new Rectangle(0, 0, panelDestino.Width, panelDestino.Height),
                    zonaRecorte,
                    GraphicsUnit.Pixel
                );
            }

            fondoRenderizado.Dispose();

            return recorte;
        }

        private void CentrarPanelPrincipal() {
            int espacioDisponible = this.ClientSize.Width - panelMenu.Width;

            int x = panelMenu.Width + (espacioDisponible - panelPrincipal.Width) / 2;
            int y = (this.ClientSize.Height - panelPrincipal.Height) / 2;

            panelPrincipal.Location = new Point(x, y);
        }

        private void FrmPrincipal_Resize(object? sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized)
                return;

            timerRecalcularVista.Stop();
            timerRecalcularVista.Start();
        }

        private void TimerRecalcularVista_Tick(object? sender, EventArgs e) {
            timerRecalcularVista.Stop();

            CentrarPanelPrincipal();

            fondoEndForge.Refresh();
            fondoEndForge.Update();

            AplicarFondoDinamicoPanelPrincipal();

            panelPrincipal.Refresh();
            panelPrincipal.Update();

            this.Refresh();
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

            panelInicioVista.Visible = false;
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;
            fondoEndForge.SendToBack();
        }

        private void PanelInicio_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelInicio);

            panelInicioVista.Visible = true;
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;

            panelInicioVista.BringToFront();
        }

        private void CardInicio_MouseEnter(object? sender, EventArgs e) {
            Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

            if (panel != null) {
                panel.BackColor = Color.FromArgb(35, 28, 48);
            }
        }

        private void CardInicio_MouseLeave(object? sender, EventArgs e) {
            Panel? panel = sender as Panel ?? (sender as Control)?.Parent as Panel;

            if (panel != null) {
                panel.BackColor = Color.FromArgb(20, 16, 30);
            }
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

            panelInicioVista.Visible = false;
            panelRecientesVista.Visible = true;
            panelConfiguracionVista.Visible = false;

            panelRecientesVista.BringToFront();

            CargarRecientes();
        }

        private void PanelConfiguracion_Click(object? sender, EventArgs e) {
            SeleccionarPanelMenu(panelConfiguracion);

            panelInicioVista.Visible = false;
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
            btnCrearProyecto.BackColor = Color.FromArgb(126, 55, 210);
            btnCrearProyecto.ForeColor = Color.White;
        }

        private void BtnCrearProyecto_MouseLeave(object? sender, EventArgs e) {
            btnCrearProyecto.BackColor = Color.FromArgb(111, 45, 189);
            btnCrearProyecto.ForeColor = Color.White;
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

            // panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

            panelInicioVista.Visible = true;
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;

            panelInicioVista.BringToFront();

            panelSeleccionado = panelInicio;
            panelInicio.BackColor = Color.FromArgb(111, 45, 189);

            AplicarFondoDinamicoPanelPrincipal();
        }

        private void ActualizarVistaPrevia() {
            if (txtTemas.SelectedItem == null || txtNombreProyecto.Text.Trim() == "") {
                lblNombreFinal.Text = "Esperando datos...";
                lblNombreFinal.ForeColor = Color.FromArgb(168, 85, 247);
                lblNombreFinal.Font = new Font("Segoe UI Light", 11F, FontStyle.Italic);
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
            lblNombreFinal.ForeColor = Color.White;
            lblNombreFinal.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
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

        private void ListRecientes_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0)
                return;

            bool seleccionado = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color colorFondo = seleccionado ? Color.FromArgb(111, 45, 189) : Color.FromArgb(20, 16, 30);
            Color colorTexto = Color.White;
            Color colorLinea = Color.FromArgb(55, 45, 70);

            using (SolidBrush fondo = new SolidBrush(colorFondo)) {
                e.Graphics.FillRectangle(fondo, e.Bounds);
            }

            string texto = listRecientes.Items[e.Index].ToString() ?? "";

            Rectangle areaTexto = new Rectangle(e.Bounds.Left + 12, e.Bounds.Top, e.Bounds.Width - 24, e.Bounds.Height - 1);

            TextRenderer.DrawText(e.Graphics, texto, listRecientes.Font, areaTexto, colorTexto, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            using (Pen linea = new Pen(colorLinea)) {
                e.Graphics.DrawLine(linea, e.Bounds.Left + 8, e.Bounds.Bottom - 1, e.Bounds.Right - 8, e.Bounds.Bottom - 1);
            }
        }   
          
    }
}