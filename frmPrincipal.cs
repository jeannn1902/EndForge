using EndForge.Services;
using System.Diagnostics;
using System.Linq;
using EndForge.Models;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace EndForge {

    public partial class frmPrincipal : Form {
        private readonly ProyectoService proyectoService = new();
        private readonly RecientesService recientesService = new();
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

        private int ObtenerSiguienteNumero(string rutaTema) {

            if (!Directory.Exists(rutaTema))
                return 1;

            string[] carpetas = Directory.GetDirectories(rutaTema);
            int mayorNumero = 0;

            foreach (string carpeta in carpetas) {
                string nombreCarpeta = Path.GetFileName(carpeta);
                string[] partes = nombreCarpeta.Split('_');

                if (partes.Length == 0)
                    continue;

                if (int.TryParse(partes[0], out int numero)) {
                    if (numero > mayorNumero)
                        mayorNumero = numero;
                }
            }

            return mayorNumero + 1;
        }

        private void MostrarVistaPreviaVacia() {
            lblNombreFinal.Text = "Esperando datos...";
            lblNombreFinal.ForeColor = Color.FromArgb(168, 85, 247);
            lblNombreFinal.Font = new Font("Segoe UI Light", 11F, FontStyle.Italic);
        }

        // Cargar la configuración desde el archivo config.txt
        private void CargarConfiguracion() {
            string carpetaDatos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EndForge");
            string rutaConfig = Path.Combine(carpetaDatos, "config.txt");
            rutaRecientes = Path.Combine(Path.GetDirectoryName(rutaConfig)!, "recientes.txt");

            rutaBase = "";
            rutaPlantilla = "";

            string[] lineas;

            try {
                if (!Directory.Exists(carpetaDatos)) {
                    Directory.CreateDirectory(carpetaDatos);
                }

                lineas = File.ReadAllLines(rutaConfig);
            } catch (FileNotFoundException) {
                return;
            } catch (UnauthorizedAccessException) {
                MessageBox.Show(
                    "No se pudo leer la configuración de EndForge porque no hay permisos para acceder a config.txt.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            } catch (IOException) {
                MessageBox.Show(
                    "No se pudo leer la configuración de EndForge. Verifica que config.txt no esté bloqueado o en uso por otra aplicación.",
                    "EndForge",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (lineas.Length < 2) {
                return;
            }

            rutaBase = lineas[0];
            rutaPlantilla = lineas[1];

            try {
                if (!File.Exists(rutaRecientes)) {
                    File.Create(rutaRecientes).Close();
                }
            } catch (UnauthorizedAccessException) {
                MessageBox.Show("No se pudo crear recientes.txt porque no hay permisos de acceso.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } catch (IOException) {
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

        }

        private void GuardarProyectoReciente(string rutaProyecto) {
            recientesService.GuardarProyectoReciente(rutaRecientes, rutaProyecto);
        }

        private bool IntentarAbrirPractica(string rutaProyecto) {
            if (!Directory.Exists(rutaProyecto)) {
                MessageBox.Show("La carpeta de esta práctica ya no existe.", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            try {
                proyectoService.AbrirProyecto(rutaProyecto, Path.GetFileName(rutaProyecto));

                return true;
            } catch (Exception ex) {
                MessageBox.Show("No se pudo abrir la práctica.\n\n" + ex.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
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

            if (!IntentarAbrirPractica(proyecto.Ruta))
                return;

            GuardarProyectoReciente(proyecto.Ruta);
            CargarRecientes();
        }

        private void CargarRecientes() {
            listRecientes.Items.Clear();
            LimpiarLabelsRecientes();

            if (!File.Exists(rutaRecientes))
                return;

            string[] recientes = File.ReadAllLines(rutaRecientes);

            if (recientes.Length > 0) {
                string[] datos = recientes[0].Split('|');

                if (datos.Length >= 2) {
                    lblCardRecientesDesc.Text = datos[0];
                    lblCardRecientesDesc.Tag = datos[1];

                    string tema = Path.GetFileName(Path.GetDirectoryName(datos[1])!);

                    lblCardContinuarDesc.Text = tema;
                    lblCardContinuarDesc.Tag = datos[1];
                }
            } else {
                lblCardRecientesDesc.Text = "No hay proyectos recientes.";
                lblCardContinuarDesc.Text = "No hay prácticas recientes.";
            }

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

            btnCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(190, 40, 40);
            btnCerrar.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 25, 25);

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

        private void ActualizarBotonMaximizar() {
            if (WindowState == FormWindowState.Maximized) {
                btnMaximizar.Text = "❐";
            } else {
                btnMaximizar.Text = "□";
            }
        }

        private void BtnMinimizar_Click(object? sender, EventArgs e) {
            WindowState = FormWindowState.Minimized;
        }

        private void BtnMaximizar_Click(object? sender, EventArgs e) {
            if (WindowState == FormWindowState.Maximized) {
                WindowState = FormWindowState.Normal;
            } else {
                WindowState = FormWindowState.Maximized;
            }

            ActualizarBotonMaximizar();
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

            ActualizarBotonMaximizar();

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

        private void Card_MouseEnter(object sender, EventArgs e) {

        }

        private void Card_MouseLeave(object sender, EventArgs e) {

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
            CargarTemas();

            if (txtTemas.Items.Count > 0) {
                txtTemas.SelectedIndex = 0;
            }

            btnCrearProyecto.Enabled = false;

            CargarConfiguracion();
            CargarRecientes();

            // panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);

            panelInicioVista.Visible = true;
            panelRecientesVista.Visible = false;
            panelConfiguracionVista.Visible = false;

            panelInicioVista.BringToFront();

            panelSeleccionado = panelInicio;
            panelInicio.BackColor = Color.FromArgb(111, 45, 189);

            AplicarFondoDinamicoPanelPrincipal();

            if (string.IsNullOrWhiteSpace(rutaBase) || string.IsNullOrWhiteSpace(rutaPlantilla)) {
                PanelConfiguracion_Click(panelConfiguracion, EventArgs.Empty);

                MessageBox.Show(
                    "¡Bienvenido a EndForge!\n\n" +
                    "Antes de comenzar, configura la carpeta de tus prácticas y la plantilla oficial.",
                    "Configuración inicial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            txtBuscarReciente.Text = "Buscar práctica...";
            txtBuscarReciente.ForeColor = Color.Gray;
        }

        private void CargarTemas() {
            txtTemas.Items.Clear();

            if (!Directory.Exists(rutaBase))
                return;

            string[] carpetas = Directory.GetDirectories(rutaBase).OrderBy(c => c).ToArray();

            foreach (string carpeta in carpetas) {
                string nombreCarpeta = Path.GetFileName(carpeta);

                if (nombreCarpeta.StartsWith("."))
                    continue;

                string[] partes = nombreCarpeta.Split('_');

                if (partes.Length < 2)
                    continue;

                if (!int.TryParse(partes[0], out _))
                    continue;
                txtTemas.Items.Add(nombreCarpeta);
                ActualizarVistaPrevia();
            }

            if (txtTemas.Items.Count > 0) {
                txtTemas.SelectedIndex = 0;
            }
        }

        private void ActualizarVistaPrevia() {
            if (txtTemas.SelectedItem == null || txtNombreProyecto.Text.Trim() == "") {
                MostrarVistaPreviaVacia();
                return;
            }

            string temaSeleccionado = txtTemas.Text;
            string rutaTema = Path.Combine(rutaBase, temaSeleccionado);

            int siguienteNumero = ObtenerSiguienteNumero(rutaTema);

            string numeroFormateado = siguienteNumero.ToString("00");
            lblNombreFinal.Text = numeroFormateado + "_" + txtNombreProyecto.Text.Trim();
            lblNombreFinal.ForeColor = Color.White;
            lblNombreFinal.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        }

        private void ValidarFormulario() {
            btnCrearProyecto.Enabled =
                txtTemas.SelectedItem != null &&
                !string.IsNullOrWhiteSpace(txtNombreProyecto.Text) &&
                !string.IsNullOrWhiteSpace(txtObjetivo.Text) &&
                !string.IsNullOrWhiteSpace(rutaBase) &&
                !string.IsNullOrWhiteSpace(rutaPlantilla);
        }

        private void CmbTemas_SelectedIndexChanged(object sender, EventArgs e) {
            ActualizarVistaPrevia();
        }

        private void BtnCrearProyecto_Click(object sender, EventArgs e) {
            string temaSeleccionado = txtTemas.Text;
            string nombreProyecto = lblNombreFinal.Text;
            string nombreUsuario = txtNombreProyecto.Text.Trim();
            temaSeleccionado = temaSeleccionado.Trim();
            nombreProyecto = nombreProyecto.Trim();

            if (string.IsNullOrWhiteSpace(nombreUsuario)) {
                MessageBox.Show("Escribe un nombre para el proyecto.");
                txtNombreProyecto.Focus();
                return;
            }

            if (nombreUsuario.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                MessageBox.Show("El nombre contiene caracteres no válidos.");
                txtNombreProyecto.Focus();
                return;
            }

            string rutaProyecto = Path.Combine(rutaBase, temaSeleccionado, nombreProyecto);

            try {
                proyectoService.CrearProyecto(rutaPlantilla, rutaProyecto, nombreProyecto, temaSeleccionado, txtObjetivo.Text.Trim());
            } catch (ProyectoService.ProyectoDestinoExistenteException) {
                MessageBox.Show("La práctica ya existe.");
                txtNombreProyecto.Focus();
                return;
            } catch (Exception ex) {
                MessageBox.Show("Ocurrió un error al crear la práctica.\n\n" + ex.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            try {
                recientesService.GuardarProyectoReciente(rutaRecientes, rutaProyecto);
                CargarRecientes();
            } catch (Exception ex) {
                MessageBox.Show("La práctica se creó correctamente, pero no pudo guardarse en Recientes.\n\n" + ex.Message, "EndForge",MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            txtNombreProyecto.Clear();
            txtObjetivo.Clear();
            txtNombreProyecto.Focus();

            ActualizarVistaPrevia();
            ValidarFormulario();

            try {
                proyectoService.AbrirProyecto(rutaProyecto, nombreProyecto);
            } catch (Exception ex) {
                MessageBox.Show("La práctica se creó correctamente, pero no pudo abrirse Visual Studio.\n\n" + ex.Message, "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show("El proyecto se creó correctamente.\n\n¡Visual Studio se abrirá automáticamente!", "EndForge", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            if (!IntentarAbrirPractica(proyecto.Ruta))
                return;

            GuardarProyectoReciente(proyecto.Ruta);
            CargarRecientes();
        }

        private void BtnCambiarRutaPlantilla_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
                carpeta.Description = "Selecciona la carpeta de la plantilla";

                if (carpeta.ShowDialog() == DialogResult.OK) {
                    txtRutaPlantillaConfig.Text = carpeta.SelectedPath;

                    btnGuardarConfiguracion.Enabled = true;
                    lblEstadoConfiguracion.Visible = false;
                }
            }
        }

        private void BtnCambiarRutaBase_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog carpeta = new FolderBrowserDialog()) {
                carpeta.Description = "Selecciona la carpeta base de tus proyectos";

                if (carpeta.ShowDialog() == DialogResult.OK) {
                    txtRutaBaseConfig.Text = carpeta.SelectedPath;

                    btnGuardarConfiguracion.Enabled = true;
                    lblEstadoConfiguracion.Visible = false;
                }
            }
        }

        private void BtnGuardarConfiguracion_Click(object sender, EventArgs e) {
            lblEstadoConfiguracion.Visible = false;
            if (!Directory.Exists(txtRutaBaseConfig.Text) || !Directory.Exists(txtRutaPlantillaConfig.Text)) {
                lblEstadoConfiguracion.Text = "❌ Una de las rutas seleccionadas no existe.";
                lblEstadoConfiguracion.ForeColor = Color.IndianRed;
                lblEstadoConfiguracion.Visible = true;
                return;
            }

            string[] soluciones = Directory.GetFiles(txtRutaPlantillaConfig.Text,
                "*.sln",
                SearchOption.TopDirectoryOnly
                );

            if (soluciones.Length == 0) {
                lblEstadoConfiguracion.Text = "❌ Plantilla de EndForge no válida.";
                lblEstadoConfiguracion.ForeColor = Color.LightCoral;
                lblEstadoConfiguracion.Visible = true;
                return;
            }

            string[] proyectos = Directory.GetFiles(txtRutaPlantillaConfig.Text,
                "*.vcxproj",
                SearchOption.AllDirectories
                );

            if (proyectos.Length == 0) {
                lblEstadoConfiguracion.Text = "❌ No se encontró un proyecto C++.";
                lblEstadoConfiguracion.ForeColor = Color.LightCoral;
                lblEstadoConfiguracion.Visible = true;
                return;
            }

            string[] cpp = Directory.GetFiles(txtRutaPlantillaConfig.Text,
                "*.cpp",
                SearchOption.AllDirectories
                );

            if (cpp.Length == 0) {
                lblEstadoConfiguracion.Text = "❌ No se encontraron archivos C++.";
                lblEstadoConfiguracion.ForeColor = Color.LightCoral;
                lblEstadoConfiguracion.Visible = true;
                return;
            }

            string carpetaDatos = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EndForge"
            );

            string rutaConfig = Path.Combine(carpetaDatos, "config.txt");

            File.WriteAllLines(rutaConfig, new string[] {
                txtRutaBaseConfig.Text, txtRutaPlantillaConfig.Text
            });

            CargarConfiguracion();

            lblEstadoConfiguracion.Text = "✅ Cambios guardados.";
            lblEstadoConfiguracion.ForeColor = Color.LightGreen;
            lblEstadoConfiguracion.Visible = true;

            btnGuardarConfiguracion.Enabled = false;

            // PanelInicio_Click(panelInicio, EventArgs.Empty);
            CargarRecientes();
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

        private void LblCardRecientesDesc_Click(object sender, EventArgs e) {
            string? rutaProyecto = lblCardRecientesDesc.Tag as string;

            if (!string.IsNullOrWhiteSpace(rutaProyecto)) {
                IntentarAbrirPractica(rutaProyecto);
            }
        }

        private void PanelCardRecientes_Click(object sender, EventArgs e) {
            LblCardRecientesDesc_Click(sender, e);
        }

        private void LblCardRecientesTitulo_Click(object sender, EventArgs e) {
            LblCardRecientesDesc_Click(sender, e);
        }

        private void PanelCardContinuar_Click(object sender, EventArgs e) {
            string? rutaProyecto = lblCardContinuarDesc.Tag?.ToString();

            if (string.IsNullOrWhiteSpace(rutaProyecto))
                return;

            IntentarAbrirPractica(rutaProyecto);
        }

        private void panelCardContinuar_Click(object sender, EventArgs e) {

        }

        private void TxtBuscarReciente_TextChanged(object sender, EventArgs e) {
            string filtro = txtBuscarReciente.Text.Trim().ToLower();

            listRecientes.Items.Clear();
            LimpiarLabelsRecientes();

            if (!File.Exists(rutaRecientes))
                return;

            string[] recientes = File.ReadAllLines(rutaRecientes);
            List<Label> labelsRecientes = ObtenerLabelsRecientes();
            int indice = 0;

            foreach (string reciente in recientes) {
                string[] datos = reciente.Split('|');

                if (datos.Length < 2)
                    continue;

                if (!datos[0].ToLower().Contains(filtro))
                    continue;

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

        private void TxtBuscarReciente_Enter(object sender, EventArgs e) {
            if (txtBuscarReciente.Text == "Buscar práctica...") {
                txtBuscarReciente.Text = "";
                txtBuscarReciente.ForeColor = Color.White;
            }
        }

        private void TxtBuscarReciente_Leave(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtBuscarReciente.Text)) {
                txtBuscarReciente.Text = "Buscar práctica...";
                txtBuscarReciente.ForeColor = Color.Gray;
            }
        }
    }
}
