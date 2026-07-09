namespace ProjectCreator
{
    partial class frmPrincipal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPrincipal));
            btnCrearProyecto = new Button();
            lblNombreFinal = new Label();
            lblVistaPrevia = new Label();
            txtObjetivo = new TextBox();
            lblObjetivo = new Label();
            txtNombreProyecto = new TextBox();
            lblNombre = new Label();
            lblTitulo = new Label();
            txtTemas = new ComboBox();
            lblTema = new Label();
            panelPrincipal = new Panel();
            panelInicioVista = new Panel();
            panelCardConfiguracion = new Panel();
            lblCardConfiguracionDesc = new Label();
            lblCardConfiguracionTitulo = new Label();
            panelCardRecientes = new Panel();
            lblCardRecientesDesc = new Label();
            lblCardRecientesTitulo = new Label();
            panelCardNuevaPractica = new Panel();
            lblCardNuevaPracticaDesc = new Label();
            lblCardNuevaPracticaTitulo = new Label();
            lblInicioSubtitulo = new Label();
            lblInicioTitulo = new Label();
            panelRecientesVista = new Panel();
            panelListaRecientes = new Panel();
            lblReciente10 = new Label();
            lblReciente9 = new Label();
            lblReciente8 = new Label();
            lblReciente7 = new Label();
            lblReciente6 = new Label();
            lblReciente5 = new Label();
            lblReciente4 = new Label();
            lblReciente3 = new Label();
            lblReciente2 = new Label();
            lblReciente1 = new Label();
            lblAyudaRecientes = new Label();
            lblRecientesSubtitulo = new Label();
            listRecientes = new ListBox();
            lblRecientesTitulo = new Label();
            panelConfiguracionVista = new Panel();
            btnGuardarConfiguracion = new Button();
            btnCambiarRutaPlantilla = new Button();
            btnCambiarRutaBase = new Button();
            txtRutaPlantillaConfig = new TextBox();
            lblRutaPlantilla = new Label();
            txtRutaBaseConfig = new TextBox();
            lblRutaBase = new Label();
            lblConfiguracionTitulo = new Label();
            fondoEndForge = new FondoEndForge();
            panelMenu = new Panel();
            panelAcercaDe = new Panel();
            panelIndicadorAD = new Panel();
            lblAcercaDe = new Label();
            pictureBoxAcercaDe = new PictureBox();
            panelConfiguracion = new Panel();
            panelIndicadorConfigs = new Panel();
            lblConfiguracion = new Label();
            pictureBoxConfiguracion = new PictureBox();
            panelRecientes = new Panel();
            panelIndicadorRecientes = new Panel();
            lblRecientes = new Label();
            pictureBoxRecientes = new PictureBox();
            panelAbrirPractica = new Panel();
            panelIndicadorAP = new Panel();
            lblAbrirPractica = new Label();
            pictureBoxAbrirPractica = new PictureBox();
            panelNuevaPractica = new Panel();
            panelIndicadorNP = new Panel();
            lblNuevaPractica = new Label();
            pictureBoxNuevaPractica = new PictureBox();
            panelInicio = new Panel();
            panelIndicadorInicio = new Panel();
            lblInicio = new Label();
            pictureBoxInicio = new PictureBox();
            pictureBoxLogo = new PictureBox();
            lblMenuSubtitulo = new Label();
            lblMenuTitulo = new Label();
            panelPrincipal.SuspendLayout();
            panelInicioVista.SuspendLayout();
            panelCardConfiguracion.SuspendLayout();
            panelCardRecientes.SuspendLayout();
            panelCardNuevaPractica.SuspendLayout();
            panelRecientesVista.SuspendLayout();
            panelListaRecientes.SuspendLayout();
            panelConfiguracionVista.SuspendLayout();
            panelMenu.SuspendLayout();
            panelAcercaDe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAcercaDe).BeginInit();
            panelConfiguracion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxConfiguracion).BeginInit();
            panelRecientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRecientes).BeginInit();
            panelAbrirPractica.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAbrirPractica).BeginInit();
            panelNuevaPractica.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxNuevaPractica).BeginInit();
            panelInicio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxInicio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            SuspendLayout();
            // 
            // btnCrearProyecto
            // 
            btnCrearProyecto.BackColor = Color.FromArgb(111, 45, 189);
            btnCrearProyecto.Cursor = Cursors.Hand;
            btnCrearProyecto.FlatAppearance.BorderSize = 0;
            btnCrearProyecto.FlatAppearance.MouseDownBackColor = Color.FromArgb(88, 35, 155);
            btnCrearProyecto.FlatAppearance.MouseOverBackColor = Color.FromArgb(126, 55, 210);
            btnCrearProyecto.FlatStyle = FlatStyle.Flat;
            btnCrearProyecto.Font = new Font("Segoe UI Semibold", 10F);
            btnCrearProyecto.ForeColor = Color.White;
            btnCrearProyecto.Location = new Point(123, 385);
            btnCrearProyecto.Name = "btnCrearProyecto";
            btnCrearProyecto.Size = new Size(180, 42);
            btnCrearProyecto.TabIndex = 6;
            btnCrearProyecto.Text = "Crear práctica";
            btnCrearProyecto.UseVisualStyleBackColor = false;
            btnCrearProyecto.Click += BtnCrearProyecto_Click;
            btnCrearProyecto.MouseEnter += BtnCrearProyecto_MouseEnter;
            btnCrearProyecto.MouseLeave += BtnCrearProyecto_MouseLeave;
            // 
            // lblNombreFinal
            // 
            lblNombreFinal.AutoSize = true;
            lblNombreFinal.BackColor = Color.Transparent;
            lblNombreFinal.Font = new Font("Segoe UI Light", 11F);
            lblNombreFinal.ForeColor = Color.FromArgb(168, 85, 247);
            lblNombreFinal.Location = new Point(125, 339);
            lblNombreFinal.Name = "lblNombreFinal";
            lblNombreFinal.Size = new Size(182, 30);
            lblNombreFinal.TabIndex = 5;
            lblNombreFinal.Text = "Esperando datos...";
            lblNombreFinal.Click += LblNombreFinal_Click;
            // 
            // lblVistaPrevia
            // 
            lblVistaPrevia.AutoSize = true;
            lblVistaPrevia.BackColor = Color.Transparent;
            lblVistaPrevia.Font = new Font("Segoe UI Semibold", 9F);
            lblVistaPrevia.ForeColor = Color.White;
            lblVistaPrevia.Location = new Point(160, 313);
            lblVistaPrevia.Name = "lblVistaPrevia";
            lblVistaPrevia.Size = new Size(113, 25);
            lblVistaPrevia.TabIndex = 4;
            lblVistaPrevia.Text = "Vista previa:";
            lblVistaPrevia.Click += LblVistaPrevia_Click;
            // 
            // txtObjetivo
            // 
            txtObjetivo.BackColor = Color.FromArgb(35, 35, 38);
            txtObjetivo.BorderStyle = BorderStyle.FixedSingle;
            txtObjetivo.Font = new Font("Segoe UI Light", 9F);
            txtObjetivo.ForeColor = Color.White;
            txtObjetivo.Location = new Point(88, 252);
            txtObjetivo.Multiline = true;
            txtObjetivo.Name = "txtObjetivo";
            txtObjetivo.ScrollBars = ScrollBars.Vertical;
            txtObjetivo.Size = new Size(250, 60);
            txtObjetivo.TabIndex = 9;
            txtObjetivo.TextChanged += TxtObjetivo_TextChanged;
            // 
            // lblObjetivo
            // 
            lblObjetivo.AutoSize = true;
            lblObjetivo.BackColor = Color.Transparent;
            lblObjetivo.Font = new Font("Segoe UI Semibold", 9F);
            lblObjetivo.ForeColor = Color.White;
            lblObjetivo.Location = new Point(115, 226);
            lblObjetivo.Name = "lblObjetivo";
            lblObjetivo.Size = new Size(193, 25);
            lblObjetivo.TabIndex = 8;
            lblObjetivo.Text = "Objetivo del ejercicio:";
            lblObjetivo.Click += LblObjetivo_Click;
            // 
            // txtNombreProyecto
            // 
            txtNombreProyecto.BackColor = Color.FromArgb(35, 35, 38);
            txtNombreProyecto.BorderStyle = BorderStyle.FixedSingle;
            txtNombreProyecto.Font = new Font("Segoe UI Light", 9F);
            txtNombreProyecto.ForeColor = Color.White;
            txtNombreProyecto.Location = new Point(88, 179);
            txtNombreProyecto.Name = "txtNombreProyecto";
            txtNombreProyecto.Size = new Size(250, 31);
            txtNombreProyecto.TabIndex = 3;
            txtNombreProyecto.TextChanged += TxtNombreProyecto_TextChanged;
            // 
            // lblNombre
            // 
            lblNombre.AutoSize = true;
            lblNombre.BackColor = Color.Transparent;
            lblNombre.Font = new Font("Segoe UI Semibold", 9F);
            lblNombre.ForeColor = Color.White;
            lblNombre.Location = new Point(117, 152);
            lblNombre.Name = "lblNombre";
            lblNombre.Size = new Size(190, 25);
            lblNombre.TabIndex = 2;
            lblNombre.Text = "Nombre del ejercicio:";
            // 
            // lblTitulo
            // 
            lblTitulo.BackColor = Color.Transparent;
            lblTitulo.Font = new Font("Segoe UI Semibold", 15F);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(23, 11);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(415, 93);
            lblTitulo.TabIndex = 7;
            lblTitulo.Text = "Automatiza la creación de proyectos de C++";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Click += Label1_Click_2;
            // 
            // txtTemas
            // 
            txtTemas.BackColor = Color.FromArgb(35, 35, 38);
            txtTemas.DropDownStyle = ComboBoxStyle.DropDownList;
            txtTemas.FlatStyle = FlatStyle.Flat;
            txtTemas.Font = new Font("Segoe UI Light", 9F);
            txtTemas.ForeColor = Color.White;
            txtTemas.FormattingEnabled = true;
            txtTemas.Location = new Point(88, 107);
            txtTemas.Name = "txtTemas";
            txtTemas.Size = new Size(250, 33);
            txtTemas.TabIndex = 1;
            txtTemas.SelectedIndexChanged += CmbTemas_SelectedIndexChanged;
            // 
            // lblTema
            // 
            lblTema.AutoSize = true;
            lblTema.BackColor = Color.Transparent;
            lblTema.Font = new Font("Segoe UI Semibold", 9F);
            lblTema.ForeColor = Color.White;
            lblTema.Location = new Point(182, 79);
            lblTema.Name = "lblTema";
            lblTema.Size = new Size(59, 25);
            lblTema.TabIndex = 0;
            lblTema.Text = "Tema:";
            lblTema.Click += Label1_Click_1;
            // 
            // panelPrincipal
            // 
            panelPrincipal.BackColor = Color.FromArgb(45, 45, 48);
            panelPrincipal.BackgroundImage = Properties.Resources.fondo_panelPrincipal_alineado_892x4702;
            panelPrincipal.BackgroundImageLayout = ImageLayout.Stretch;
            panelPrincipal.Controls.Add(panelInicioVista);
            panelPrincipal.Controls.Add(panelRecientesVista);
            panelPrincipal.Controls.Add(panelConfiguracionVista);
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(txtTemas);
            panelPrincipal.Controls.Add(lblObjetivo);
            panelPrincipal.Controls.Add(lblNombreFinal);
            panelPrincipal.Controls.Add(txtNombreProyecto);
            panelPrincipal.Controls.Add(btnCrearProyecto);
            panelPrincipal.Controls.Add(lblVistaPrevia);
            panelPrincipal.Controls.Add(txtObjetivo);
            panelPrincipal.Controls.Add(lblNombre);
            panelPrincipal.Controls.Add(lblTema);
            panelPrincipal.Location = new Point(264, 134);
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.Size = new Size(892, 470);
            panelPrincipal.TabIndex = 11;
            panelPrincipal.Paint += PanelControles_Paint;
            // 
            // panelInicioVista
            // 
            panelInicioVista.BackColor = Color.FromArgb(45, 45, 48);
            panelInicioVista.BackgroundImage = Properties.Resources.fondo_panelPrincipal_alineado_892x4702;
            panelInicioVista.Controls.Add(panelCardConfiguracion);
            panelInicioVista.Controls.Add(panelCardRecientes);
            panelInicioVista.Controls.Add(panelCardNuevaPractica);
            panelInicioVista.Controls.Add(lblInicioSubtitulo);
            panelInicioVista.Controls.Add(lblInicioTitulo);
            panelInicioVista.Location = new Point(0, 0);
            panelInicioVista.Name = "panelInicioVista";
            panelInicioVista.Size = new Size(892, 470);
            panelInicioVista.TabIndex = 16;
            // 
            // panelCardConfiguracion
            // 
            panelCardConfiguracion.BackColor = Color.FromArgb(28, 22, 40);
            panelCardConfiguracion.Controls.Add(lblCardConfiguracionDesc);
            panelCardConfiguracion.Controls.Add(lblCardConfiguracionTitulo);
            panelCardConfiguracion.Cursor = Cursors.Hand;
            panelCardConfiguracion.Location = new Point(604, 270);
            panelCardConfiguracion.Name = "panelCardConfiguracion";
            panelCardConfiguracion.Size = new Size(265, 80);
            panelCardConfiguracion.TabIndex = 4;
            // 
            // lblCardConfiguracionDesc
            // 
            lblCardConfiguracionDesc.BackColor = Color.Transparent;
            lblCardConfiguracionDesc.Font = new Font("Segoe UI Light", 8.5F);
            lblCardConfiguracionDesc.ForeColor = Color.Silver;
            lblCardConfiguracionDesc.Location = new Point(7, 33);
            lblCardConfiguracionDesc.Name = "lblCardConfiguracionDesc";
            lblCardConfiguracionDesc.Size = new Size(250, 46);
            lblCardConfiguracionDesc.TabIndex = 1;
            lblCardConfiguracionDesc.Text = "Ajusta las rutas donde guardarás tus prácticas y plantillas.";
            lblCardConfiguracionDesc.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblCardConfiguracionTitulo
            // 
            lblCardConfiguracionTitulo.AutoSize = true;
            lblCardConfiguracionTitulo.BackColor = Color.Transparent;
            lblCardConfiguracionTitulo.Font = new Font("Segoe UI Semibold", 12F);
            lblCardConfiguracionTitulo.ForeColor = Color.White;
            lblCardConfiguracionTitulo.Location = new Point(49, 1);
            lblCardConfiguracionTitulo.Name = "lblCardConfiguracionTitulo";
            lblCardConfiguracionTitulo.Size = new Size(166, 32);
            lblCardConfiguracionTitulo.TabIndex = 0;
            lblCardConfiguracionTitulo.Text = "Configuración";
            // 
            // panelCardRecientes
            // 
            panelCardRecientes.BackColor = Color.FromArgb(28, 22, 40);
            panelCardRecientes.Controls.Add(lblCardRecientesDesc);
            panelCardRecientes.Controls.Add(lblCardRecientesTitulo);
            panelCardRecientes.Cursor = Cursors.Hand;
            panelCardRecientes.Location = new Point(314, 270);
            panelCardRecientes.Name = "panelCardRecientes";
            panelCardRecientes.Size = new Size(265, 80);
            panelCardRecientes.TabIndex = 3;
            // 
            // lblCardRecientesDesc
            // 
            lblCardRecientesDesc.BackColor = Color.Transparent;
            lblCardRecientesDesc.Font = new Font("Segoe UI Light", 8.5F);
            lblCardRecientesDesc.ForeColor = Color.Silver;
            lblCardRecientesDesc.Location = new Point(35, 30);
            lblCardRecientesDesc.Name = "lblCardRecientesDesc";
            lblCardRecientesDesc.Size = new Size(190, 49);
            lblCardRecientesDesc.TabIndex = 1;
            lblCardRecientesDesc.Text = "Abre tus prácticas más recientes.";
            lblCardRecientesDesc.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblCardRecientesTitulo
            // 
            lblCardRecientesTitulo.AutoSize = true;
            lblCardRecientesTitulo.BackColor = Color.Transparent;
            lblCardRecientesTitulo.Font = new Font("Segoe UI Semibold", 12F);
            lblCardRecientesTitulo.ForeColor = Color.White;
            lblCardRecientesTitulo.Location = new Point(72, 1);
            lblCardRecientesTitulo.Name = "lblCardRecientesTitulo";
            lblCardRecientesTitulo.Size = new Size(117, 32);
            lblCardRecientesTitulo.TabIndex = 0;
            lblCardRecientesTitulo.Text = "Recientes";
            // 
            // panelCardNuevaPractica
            // 
            panelCardNuevaPractica.BackColor = Color.FromArgb(28, 22, 40);
            panelCardNuevaPractica.Controls.Add(lblCardNuevaPracticaDesc);
            panelCardNuevaPractica.Controls.Add(lblCardNuevaPracticaTitulo);
            panelCardNuevaPractica.Cursor = Cursors.Hand;
            panelCardNuevaPractica.Location = new Point(24, 270);
            panelCardNuevaPractica.Name = "panelCardNuevaPractica";
            panelCardNuevaPractica.Size = new Size(265, 80);
            panelCardNuevaPractica.TabIndex = 2;
            // 
            // lblCardNuevaPracticaDesc
            // 
            lblCardNuevaPracticaDesc.BackColor = Color.Transparent;
            lblCardNuevaPracticaDesc.Font = new Font("Segoe UI Light", 8.5F);
            lblCardNuevaPracticaDesc.ForeColor = Color.Silver;
            lblCardNuevaPracticaDesc.Location = new Point(12, 30);
            lblCardNuevaPracticaDesc.Name = "lblCardNuevaPracticaDesc";
            lblCardNuevaPracticaDesc.Size = new Size(241, 49);
            lblCardNuevaPracticaDesc.TabIndex = 1;
            lblCardNuevaPracticaDesc.Text = "Crea un proyecto C++ desde tu plantilla.";
            lblCardNuevaPracticaDesc.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblCardNuevaPracticaTitulo
            // 
            lblCardNuevaPracticaTitulo.AutoSize = true;
            lblCardNuevaPracticaTitulo.BackColor = Color.Transparent;
            lblCardNuevaPracticaTitulo.Font = new Font("Segoe UI Semibold", 12F);
            lblCardNuevaPracticaTitulo.ForeColor = Color.White;
            lblCardNuevaPracticaTitulo.Location = new Point(44, 1);
            lblCardNuevaPracticaTitulo.Name = "lblCardNuevaPracticaTitulo";
            lblCardNuevaPracticaTitulo.Size = new Size(177, 32);
            lblCardNuevaPracticaTitulo.TabIndex = 0;
            lblCardNuevaPracticaTitulo.Text = "Nueva práctica";
            // 
            // lblInicioSubtitulo
            // 
            lblInicioSubtitulo.AutoSize = true;
            lblInicioSubtitulo.BackColor = Color.Transparent;
            lblInicioSubtitulo.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblInicioSubtitulo.ForeColor = Color.Gainsboro;
            lblInicioSubtitulo.Location = new Point(225, 208);
            lblInicioSubtitulo.Name = "lblInicioSubtitulo";
            lblInicioSubtitulo.Size = new Size(442, 32);
            lblInicioSubtitulo.TabIndex = 1;
            lblInicioSubtitulo.Text = "Tu centro de control para aprender C++";
            // 
            // lblInicioTitulo
            // 
            lblInicioTitulo.AutoSize = true;
            lblInicioTitulo.BackColor = Color.Transparent;
            lblInicioTitulo.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInicioTitulo.ForeColor = Color.White;
            lblInicioTitulo.Location = new Point(172, 139);
            lblInicioTitulo.Name = "lblInicioTitulo";
            lblInicioTitulo.Size = new Size(548, 65);
            lblInicioTitulo.TabIndex = 0;
            lblInicioTitulo.Text = "Bienvenido a EndForge";
            // 
            // panelRecientesVista
            // 
            panelRecientesVista.BackColor = Color.Transparent;
            panelRecientesVista.Controls.Add(panelListaRecientes);
            panelRecientesVista.Controls.Add(lblAyudaRecientes);
            panelRecientesVista.Controls.Add(lblRecientesSubtitulo);
            panelRecientesVista.Controls.Add(listRecientes);
            panelRecientesVista.Controls.Add(lblRecientesTitulo);
            panelRecientesVista.Location = new Point(0, 0);
            panelRecientesVista.Name = "panelRecientesVista";
            panelRecientesVista.Size = new Size(892, 470);
            panelRecientesVista.TabIndex = 15;
            panelRecientesVista.Visible = false;
            // 
            // panelListaRecientes
            // 
            panelListaRecientes.BackColor = Color.Transparent;
            panelListaRecientes.Controls.Add(lblReciente10);
            panelListaRecientes.Controls.Add(lblReciente9);
            panelListaRecientes.Controls.Add(lblReciente8);
            panelListaRecientes.Controls.Add(lblReciente7);
            panelListaRecientes.Controls.Add(lblReciente6);
            panelListaRecientes.Controls.Add(lblReciente5);
            panelListaRecientes.Controls.Add(lblReciente4);
            panelListaRecientes.Controls.Add(lblReciente3);
            panelListaRecientes.Controls.Add(lblReciente2);
            panelListaRecientes.Controls.Add(lblReciente1);
            panelListaRecientes.Location = new Point(80, 78);
            panelListaRecientes.Name = "panelListaRecientes";
            panelListaRecientes.Size = new Size(729, 342);
            panelListaRecientes.TabIndex = 15;
            // 
            // lblReciente10
            // 
            lblReciente10.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente10.BorderStyle = BorderStyle.FixedSingle;
            lblReciente10.Cursor = Cursors.Hand;
            lblReciente10.Font = new Font("Segoe UI Light", 10F);
            lblReciente10.ForeColor = Color.White;
            lblReciente10.Location = new Point(0, 324);
            lblReciente10.Name = "lblReciente10";
            lblReciente10.Padding = new Padding(12, 0, 0, 0);
            lblReciente10.Size = new Size(729, 34);
            lblReciente10.TabIndex = 15;
            lblReciente10.Text = "Proyecto reciente";
            lblReciente10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente9
            // 
            lblReciente9.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente9.BorderStyle = BorderStyle.FixedSingle;
            lblReciente9.Cursor = Cursors.Hand;
            lblReciente9.Font = new Font("Segoe UI Light", 10F);
            lblReciente9.ForeColor = Color.White;
            lblReciente9.Location = new Point(0, 288);
            lblReciente9.Name = "lblReciente9";
            lblReciente9.Padding = new Padding(12, 0, 0, 0);
            lblReciente9.Size = new Size(729, 34);
            lblReciente9.TabIndex = 14;
            lblReciente9.Text = "Proyecto reciente";
            lblReciente9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente8
            // 
            lblReciente8.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente8.BorderStyle = BorderStyle.FixedSingle;
            lblReciente8.Cursor = Cursors.Hand;
            lblReciente8.Font = new Font("Segoe UI Light", 10F);
            lblReciente8.ForeColor = Color.White;
            lblReciente8.Location = new Point(0, 252);
            lblReciente8.Name = "lblReciente8";
            lblReciente8.Padding = new Padding(12, 0, 0, 0);
            lblReciente8.Size = new Size(729, 34);
            lblReciente8.TabIndex = 13;
            lblReciente8.Text = "Proyecto reciente";
            lblReciente8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente7
            // 
            lblReciente7.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente7.BorderStyle = BorderStyle.FixedSingle;
            lblReciente7.Cursor = Cursors.Hand;
            lblReciente7.Font = new Font("Segoe UI Light", 10F);
            lblReciente7.ForeColor = Color.White;
            lblReciente7.Location = new Point(0, 216);
            lblReciente7.Name = "lblReciente7";
            lblReciente7.Padding = new Padding(12, 0, 0, 0);
            lblReciente7.Size = new Size(729, 34);
            lblReciente7.TabIndex = 12;
            lblReciente7.Text = "Proyecto reciente";
            lblReciente7.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente6
            // 
            lblReciente6.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente6.BorderStyle = BorderStyle.FixedSingle;
            lblReciente6.Cursor = Cursors.Hand;
            lblReciente6.Font = new Font("Segoe UI Light", 10F);
            lblReciente6.ForeColor = Color.White;
            lblReciente6.Location = new Point(0, 180);
            lblReciente6.Name = "lblReciente6";
            lblReciente6.Padding = new Padding(12, 0, 0, 0);
            lblReciente6.Size = new Size(729, 34);
            lblReciente6.TabIndex = 11;
            lblReciente6.Text = "Proyecto reciente";
            lblReciente6.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente5
            // 
            lblReciente5.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente5.BorderStyle = BorderStyle.FixedSingle;
            lblReciente5.Cursor = Cursors.Hand;
            lblReciente5.Font = new Font("Segoe UI Light", 10F);
            lblReciente5.ForeColor = Color.White;
            lblReciente5.Location = new Point(0, 144);
            lblReciente5.Name = "lblReciente5";
            lblReciente5.Padding = new Padding(12, 0, 0, 0);
            lblReciente5.Size = new Size(729, 34);
            lblReciente5.TabIndex = 10;
            lblReciente5.Text = "Proyecto reciente";
            lblReciente5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente4
            // 
            lblReciente4.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente4.BorderStyle = BorderStyle.FixedSingle;
            lblReciente4.Cursor = Cursors.Hand;
            lblReciente4.Font = new Font("Segoe UI Light", 10F);
            lblReciente4.ForeColor = Color.White;
            lblReciente4.Location = new Point(0, 108);
            lblReciente4.Name = "lblReciente4";
            lblReciente4.Padding = new Padding(12, 0, 0, 0);
            lblReciente4.Size = new Size(729, 34);
            lblReciente4.TabIndex = 9;
            lblReciente4.Text = "Proyecto reciente";
            lblReciente4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente3
            // 
            lblReciente3.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente3.BorderStyle = BorderStyle.FixedSingle;
            lblReciente3.Cursor = Cursors.Hand;
            lblReciente3.Font = new Font("Segoe UI Light", 10F);
            lblReciente3.ForeColor = Color.White;
            lblReciente3.Location = new Point(0, 72);
            lblReciente3.Name = "lblReciente3";
            lblReciente3.Padding = new Padding(12, 0, 0, 0);
            lblReciente3.Size = new Size(729, 34);
            lblReciente3.TabIndex = 8;
            lblReciente3.Text = "Proyecto reciente";
            lblReciente3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente2
            // 
            lblReciente2.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente2.BorderStyle = BorderStyle.FixedSingle;
            lblReciente2.Cursor = Cursors.Hand;
            lblReciente2.Font = new Font("Segoe UI Light", 10F);
            lblReciente2.ForeColor = Color.White;
            lblReciente2.Location = new Point(0, 36);
            lblReciente2.Name = "lblReciente2";
            lblReciente2.Padding = new Padding(12, 0, 0, 0);
            lblReciente2.Size = new Size(729, 34);
            lblReciente2.TabIndex = 7;
            lblReciente2.Text = "Proyecto reciente";
            lblReciente2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblReciente1
            // 
            lblReciente1.BackColor = Color.FromArgb(20, 16, 30);
            lblReciente1.BorderStyle = BorderStyle.FixedSingle;
            lblReciente1.Cursor = Cursors.Hand;
            lblReciente1.Font = new Font("Segoe UI Light", 10F);
            lblReciente1.ForeColor = Color.White;
            lblReciente1.Location = new Point(0, 0);
            lblReciente1.Name = "lblReciente1";
            lblReciente1.Padding = new Padding(12, 0, 0, 0);
            lblReciente1.Size = new Size(729, 34);
            lblReciente1.TabIndex = 6;
            lblReciente1.Text = "Proyecto reciente";
            lblReciente1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAyudaRecientes
            // 
            lblAyudaRecientes.AutoSize = true;
            lblAyudaRecientes.BackColor = Color.Transparent;
            lblAyudaRecientes.Font = new Font("Segoe UI Light", 10F);
            lblAyudaRecientes.ForeColor = Color.Gainsboro;
            lblAyudaRecientes.Location = new Point(304, 435);
            lblAyudaRecientes.Name = "lblAyudaRecientes";
            lblAyudaRecientes.Size = new Size(285, 28);
            lblAyudaRecientes.TabIndex = 14;
            lblAyudaRecientes.Text = "Doble clic para abrir un proyecto";
            lblAyudaRecientes.Click += LblAyudaRecientes_Click;
            // 
            // lblRecientesSubtitulo
            // 
            lblRecientesSubtitulo.AutoSize = true;
            lblRecientesSubtitulo.BackColor = Color.Transparent;
            lblRecientesSubtitulo.Font = new Font("Segoe UI Light", 10F);
            lblRecientesSubtitulo.ForeColor = Color.Silver;
            lblRecientesSubtitulo.Location = new Point(331, 48);
            lblRecientesSubtitulo.Name = "lblRecientesSubtitulo";
            lblRecientesSubtitulo.Size = new Size(231, 28);
            lblRecientesSubtitulo.TabIndex = 13;
            lblRecientesSubtitulo.Text = "Continúa donde lo dejaste";
            // 
            // listRecientes
            // 
            listRecientes.BackColor = Color.FromArgb(20, 16, 30);
            listRecientes.BorderStyle = BorderStyle.FixedSingle;
            listRecientes.DrawMode = DrawMode.OwnerDrawFixed;
            listRecientes.Font = new Font("Segoe UI Light", 10F);
            listRecientes.ForeColor = Color.White;
            listRecientes.IntegralHeight = false;
            listRecientes.ItemHeight = 34;
            listRecientes.Location = new Point(80, 78);
            listRecientes.Name = "listRecientes";
            listRecientes.Size = new Size(729, 342);
            listRecientes.TabIndex = 12;
            listRecientes.Visible = false;
            listRecientes.DrawItem += ListRecientes_DrawItem;
            listRecientes.SelectedIndexChanged += ListRecientes_SelectedIndexChanged_1;
            listRecientes.DoubleClick += ListRecientes_DoubleClick;
            // 
            // lblRecientesTitulo
            // 
            lblRecientesTitulo.AutoSize = true;
            lblRecientesTitulo.BackColor = Color.Transparent;
            lblRecientesTitulo.Font = new Font("Segoe UI Semibold", 20F);
            lblRecientesTitulo.ForeColor = Color.Silver;
            lblRecientesTitulo.Location = new Point(260, 1);
            lblRecientesTitulo.Name = "lblRecientesTitulo";
            lblRecientesTitulo.Size = new Size(372, 54);
            lblRecientesTitulo.TabIndex = 11;
            lblRecientesTitulo.Text = "Proyectos recientes";
            // 
            // panelConfiguracionVista
            // 
            panelConfiguracionVista.Controls.Add(btnGuardarConfiguracion);
            panelConfiguracionVista.Controls.Add(btnCambiarRutaPlantilla);
            panelConfiguracionVista.Controls.Add(btnCambiarRutaBase);
            panelConfiguracionVista.Controls.Add(txtRutaPlantillaConfig);
            panelConfiguracionVista.Controls.Add(lblRutaPlantilla);
            panelConfiguracionVista.Controls.Add(txtRutaBaseConfig);
            panelConfiguracionVista.Controls.Add(lblRutaBase);
            panelConfiguracionVista.Controls.Add(lblConfiguracionTitulo);
            panelConfiguracionVista.Location = new Point(6, 5);
            panelConfiguracionVista.Name = "panelConfiguracionVista";
            panelConfiguracionVista.Size = new Size(892, 470);
            panelConfiguracionVista.TabIndex = 14;
            panelConfiguracionVista.Visible = false;
            // 
            // btnGuardarConfiguracion
            // 
            btnGuardarConfiguracion.AutoSize = true;
            btnGuardarConfiguracion.Location = new Point(641, 405);
            btnGuardarConfiguracion.Name = "btnGuardarConfiguracion";
            btnGuardarConfiguracion.Size = new Size(208, 35);
            btnGuardarConfiguracion.TabIndex = 7;
            btnGuardarConfiguracion.Text = "Guardar configuración";
            btnGuardarConfiguracion.UseVisualStyleBackColor = true;
            btnGuardarConfiguracion.Click += BtnGuardarConfiguracion_Click;
            // 
            // btnCambiarRutaPlantilla
            // 
            btnCambiarRutaPlantilla.Location = new Point(655, 117);
            btnCambiarRutaPlantilla.Name = "btnCambiarRutaPlantilla";
            btnCambiarRutaPlantilla.Size = new Size(174, 34);
            btnCambiarRutaPlantilla.TabIndex = 6;
            btnCambiarRutaPlantilla.Text = "Cambiar";
            btnCambiarRutaPlantilla.UseVisualStyleBackColor = true;
            btnCambiarRutaPlantilla.Click += BtnCambiarRutaPlantilla_Click;
            // 
            // btnCambiarRutaBase
            // 
            btnCambiarRutaBase.Location = new Point(656, 231);
            btnCambiarRutaBase.Name = "btnCambiarRutaBase";
            btnCambiarRutaBase.Size = new Size(174, 33);
            btnCambiarRutaBase.TabIndex = 5;
            btnCambiarRutaBase.Text = "Cambiar";
            btnCambiarRutaBase.UseVisualStyleBackColor = true;
            btnCambiarRutaBase.Click += BtnCambiarRutaBase_Click;
            // 
            // txtRutaPlantillaConfig
            // 
            txtRutaPlantillaConfig.Location = new Point(72, 232);
            txtRutaPlantillaConfig.Name = "txtRutaPlantillaConfig";
            txtRutaPlantillaConfig.ReadOnly = true;
            txtRutaPlantillaConfig.Size = new Size(526, 31);
            txtRutaPlantillaConfig.TabIndex = 4;
            // 
            // lblRutaPlantilla
            // 
            lblRutaPlantilla.AutoSize = true;
            lblRutaPlantilla.Font = new Font("Segoe UI", 11F, FontStyle.Italic);
            lblRutaPlantilla.ForeColor = Color.White;
            lblRutaPlantilla.Location = new Point(236, 199);
            lblRutaPlantilla.Name = "lblRutaPlantilla";
            lblRutaPlantilla.Size = new Size(195, 30);
            lblRutaPlantilla.TabIndex = 3;
            lblRutaPlantilla.Text = "Ruta de la plantilla";
            // 
            // txtRutaBaseConfig
            // 
            txtRutaBaseConfig.Location = new Point(72, 118);
            txtRutaBaseConfig.Name = "txtRutaBaseConfig";
            txtRutaBaseConfig.ReadOnly = true;
            txtRutaBaseConfig.Size = new Size(526, 31);
            txtRutaBaseConfig.TabIndex = 2;
            // 
            // lblRutaBase
            // 
            lblRutaBase.AutoSize = true;
            lblRutaBase.Font = new Font("Segoe UI", 11F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblRutaBase.ForeColor = Color.White;
            lblRutaBase.Location = new Point(216, 88);
            lblRutaBase.Name = "lblRutaBase";
            lblRutaBase.Size = new Size(234, 30);
            lblRutaBase.TabIndex = 1;
            lblRutaBase.Text = "Ruta base de proyectos";
            // 
            // lblConfiguracionTitulo
            // 
            lblConfiguracionTitulo.AutoSize = true;
            lblConfiguracionTitulo.Font = new Font("Cooper Black", 12F);
            lblConfiguracionTitulo.ForeColor = Color.White;
            lblConfiguracionTitulo.Location = new Point(353, 35);
            lblConfiguracionTitulo.Name = "lblConfiguracionTitulo";
            lblConfiguracionTitulo.Size = new Size(187, 27);
            lblConfiguracionTitulo.TabIndex = 0;
            lblConfiguracionTitulo.Text = "Configuración";
            // 
            // fondoEndForge
            // 
            fondoEndForge.Dock = DockStyle.Fill;
            fondoEndForge.ImagenFondo = Properties.Resources.fondo_endforge_892x470_optimizado;
            fondoEndForge.Location = new Point(0, 0);
            fondoEndForge.Name = "fondoEndForge";
            fondoEndForge.Size = new Size(1178, 744);
            fondoEndForge.TabIndex = 0;
            // 
            // panelMenu
            // 
            panelMenu.BackColor = Color.FromArgb(45, 45, 48);
            panelMenu.Controls.Add(panelAcercaDe);
            panelMenu.Controls.Add(panelConfiguracion);
            panelMenu.Controls.Add(panelRecientes);
            panelMenu.Controls.Add(panelAbrirPractica);
            panelMenu.Controls.Add(panelNuevaPractica);
            panelMenu.Controls.Add(panelInicio);
            panelMenu.Controls.Add(pictureBoxLogo);
            panelMenu.Controls.Add(lblMenuSubtitulo);
            panelMenu.Controls.Add(lblMenuTitulo);
            panelMenu.Location = new Point(22, 22);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new Size(220, 700);
            panelMenu.TabIndex = 10;
            panelMenu.Paint += PanelMenu_Paint;
            // 
            // panelAcercaDe
            // 
            panelAcercaDe.BackColor = Color.FromArgb(45, 45, 48);
            panelAcercaDe.Controls.Add(panelIndicadorAD);
            panelAcercaDe.Controls.Add(lblAcercaDe);
            panelAcercaDe.Controls.Add(pictureBoxAcercaDe);
            panelAcercaDe.Cursor = Cursors.Hand;
            panelAcercaDe.Location = new Point(12, 450);
            panelAcercaDe.Name = "panelAcercaDe";
            panelAcercaDe.Size = new Size(195, 48);
            panelAcercaDe.TabIndex = 8;
            // 
            // panelIndicadorAD
            // 
            panelIndicadorAD.BackColor = Color.FromArgb(168, 85, 247);
            panelIndicadorAD.Location = new Point(0, 0);
            panelIndicadorAD.Name = "panelIndicadorAD";
            panelIndicadorAD.Size = new Size(3, 48);
            panelIndicadorAD.TabIndex = 2;
            panelIndicadorAD.Visible = false;
            // 
            // lblAcercaDe
            // 
            lblAcercaDe.AutoSize = true;
            lblAcercaDe.BackColor = Color.Transparent;
            lblAcercaDe.Font = new Font("Segoe UI Semibold", 10F);
            lblAcercaDe.ForeColor = Color.White;
            lblAcercaDe.Location = new Point(49, 10);
            lblAcercaDe.Name = "lblAcercaDe";
            lblAcercaDe.Size = new Size(100, 28);
            lblAcercaDe.TabIndex = 1;
            lblAcercaDe.Text = "Acerca de";
            lblAcercaDe.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBoxAcercaDe
            // 
            pictureBoxAcercaDe.BackColor = Color.Transparent;
            pictureBoxAcercaDe.Location = new Point(15, 13);
            pictureBoxAcercaDe.Name = "pictureBoxAcercaDe";
            pictureBoxAcercaDe.Size = new Size(24, 24);
            pictureBoxAcercaDe.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxAcercaDe.TabIndex = 0;
            pictureBoxAcercaDe.TabStop = false;
            // 
            // panelConfiguracion
            // 
            panelConfiguracion.BackColor = Color.FromArgb(45, 45, 48);
            panelConfiguracion.Controls.Add(panelIndicadorConfigs);
            panelConfiguracion.Controls.Add(lblConfiguracion);
            panelConfiguracion.Controls.Add(pictureBoxConfiguracion);
            panelConfiguracion.Cursor = Cursors.Hand;
            panelConfiguracion.Location = new Point(12, 394);
            panelConfiguracion.Name = "panelConfiguracion";
            panelConfiguracion.Size = new Size(195, 48);
            panelConfiguracion.TabIndex = 7;
            // 
            // panelIndicadorConfigs
            // 
            panelIndicadorConfigs.BackColor = Color.FromArgb(168, 85, 247);
            panelIndicadorConfigs.Location = new Point(0, 0);
            panelIndicadorConfigs.Name = "panelIndicadorConfigs";
            panelIndicadorConfigs.Size = new Size(3, 48);
            panelIndicadorConfigs.TabIndex = 2;
            panelIndicadorConfigs.Visible = false;
            // 
            // lblConfiguracion
            // 
            lblConfiguracion.AutoSize = true;
            lblConfiguracion.BackColor = Color.Transparent;
            lblConfiguracion.Font = new Font("Segoe UI Semibold", 10F);
            lblConfiguracion.ForeColor = Color.White;
            lblConfiguracion.Location = new Point(49, 10);
            lblConfiguracion.Name = "lblConfiguracion";
            lblConfiguracion.Size = new Size(139, 28);
            lblConfiguracion.TabIndex = 1;
            lblConfiguracion.Text = "Configuración";
            lblConfiguracion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBoxConfiguracion
            // 
            pictureBoxConfiguracion.BackColor = Color.Transparent;
            pictureBoxConfiguracion.Location = new Point(15, 13);
            pictureBoxConfiguracion.Name = "pictureBoxConfiguracion";
            pictureBoxConfiguracion.Size = new Size(24, 24);
            pictureBoxConfiguracion.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxConfiguracion.TabIndex = 0;
            pictureBoxConfiguracion.TabStop = false;
            // 
            // panelRecientes
            // 
            panelRecientes.BackColor = Color.FromArgb(45, 45, 48);
            panelRecientes.Controls.Add(panelIndicadorRecientes);
            panelRecientes.Controls.Add(lblRecientes);
            panelRecientes.Controls.Add(pictureBoxRecientes);
            panelRecientes.Cursor = Cursors.Hand;
            panelRecientes.Location = new Point(12, 338);
            panelRecientes.Name = "panelRecientes";
            panelRecientes.Size = new Size(195, 48);
            panelRecientes.TabIndex = 6;
            // 
            // panelIndicadorRecientes
            // 
            panelIndicadorRecientes.BackColor = Color.FromArgb(168, 85, 247);
            panelIndicadorRecientes.Location = new Point(0, 0);
            panelIndicadorRecientes.Name = "panelIndicadorRecientes";
            panelIndicadorRecientes.Size = new Size(3, 48);
            panelIndicadorRecientes.TabIndex = 2;
            panelIndicadorRecientes.Visible = false;
            // 
            // lblRecientes
            // 
            lblRecientes.AutoSize = true;
            lblRecientes.BackColor = Color.Transparent;
            lblRecientes.Font = new Font("Segoe UI Semibold", 10F);
            lblRecientes.ForeColor = Color.White;
            lblRecientes.Location = new Point(49, 10);
            lblRecientes.Name = "lblRecientes";
            lblRecientes.Size = new Size(98, 28);
            lblRecientes.TabIndex = 1;
            lblRecientes.Text = "Recientes";
            lblRecientes.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBoxRecientes
            // 
            pictureBoxRecientes.BackColor = Color.Transparent;
            pictureBoxRecientes.Location = new Point(15, 13);
            pictureBoxRecientes.Name = "pictureBoxRecientes";
            pictureBoxRecientes.Size = new Size(24, 24);
            pictureBoxRecientes.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxRecientes.TabIndex = 0;
            pictureBoxRecientes.TabStop = false;
            // 
            // panelAbrirPractica
            // 
            panelAbrirPractica.BackColor = Color.FromArgb(45, 45, 48);
            panelAbrirPractica.Controls.Add(panelIndicadorAP);
            panelAbrirPractica.Controls.Add(lblAbrirPractica);
            panelAbrirPractica.Controls.Add(pictureBoxAbrirPractica);
            panelAbrirPractica.Cursor = Cursors.Hand;
            panelAbrirPractica.Location = new Point(12, 282);
            panelAbrirPractica.Name = "panelAbrirPractica";
            panelAbrirPractica.Size = new Size(195, 48);
            panelAbrirPractica.TabIndex = 5;
            // 
            // panelIndicadorAP
            // 
            panelIndicadorAP.BackColor = Color.FromArgb(168, 85, 247);
            panelIndicadorAP.Location = new Point(0, 0);
            panelIndicadorAP.Name = "panelIndicadorAP";
            panelIndicadorAP.Size = new Size(3, 48);
            panelIndicadorAP.TabIndex = 2;
            panelIndicadorAP.Visible = false;
            // 
            // lblAbrirPractica
            // 
            lblAbrirPractica.AutoSize = true;
            lblAbrirPractica.BackColor = Color.Transparent;
            lblAbrirPractica.Font = new Font("Segoe UI Semibold", 10F);
            lblAbrirPractica.ForeColor = Color.White;
            lblAbrirPractica.Location = new Point(49, 10);
            lblAbrirPractica.Name = "lblAbrirPractica";
            lblAbrirPractica.Size = new Size(131, 28);
            lblAbrirPractica.TabIndex = 1;
            lblAbrirPractica.Text = "Abrir práctica";
            lblAbrirPractica.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBoxAbrirPractica
            // 
            pictureBoxAbrirPractica.BackColor = Color.Transparent;
            pictureBoxAbrirPractica.Location = new Point(15, 13);
            pictureBoxAbrirPractica.Name = "pictureBoxAbrirPractica";
            pictureBoxAbrirPractica.Size = new Size(24, 24);
            pictureBoxAbrirPractica.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxAbrirPractica.TabIndex = 0;
            pictureBoxAbrirPractica.TabStop = false;
            // 
            // panelNuevaPractica
            // 
            panelNuevaPractica.BackColor = Color.FromArgb(45, 45, 48);
            panelNuevaPractica.Controls.Add(panelIndicadorNP);
            panelNuevaPractica.Controls.Add(lblNuevaPractica);
            panelNuevaPractica.Controls.Add(pictureBoxNuevaPractica);
            panelNuevaPractica.Cursor = Cursors.Hand;
            panelNuevaPractica.Location = new Point(12, 226);
            panelNuevaPractica.Name = "panelNuevaPractica";
            panelNuevaPractica.Size = new Size(195, 48);
            panelNuevaPractica.TabIndex = 4;
            // 
            // panelIndicadorNP
            // 
            panelIndicadorNP.BackColor = Color.FromArgb(168, 85, 247);
            panelIndicadorNP.Location = new Point(0, 0);
            panelIndicadorNP.Name = "panelIndicadorNP";
            panelIndicadorNP.Size = new Size(3, 48);
            panelIndicadorNP.TabIndex = 2;
            panelIndicadorNP.Visible = false;
            // 
            // lblNuevaPractica
            // 
            lblNuevaPractica.BackColor = Color.Transparent;
            lblNuevaPractica.Font = new Font("Segoe UI Semibold", 10F);
            lblNuevaPractica.ForeColor = Color.White;
            lblNuevaPractica.Location = new Point(49, 10);
            lblNuevaPractica.Name = "lblNuevaPractica";
            lblNuevaPractica.Size = new Size(145, 28);
            lblNuevaPractica.TabIndex = 1;
            lblNuevaPractica.Text = "Nueva práctica";
            lblNuevaPractica.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBoxNuevaPractica
            // 
            pictureBoxNuevaPractica.BackColor = Color.Transparent;
            pictureBoxNuevaPractica.Location = new Point(15, 13);
            pictureBoxNuevaPractica.Name = "pictureBoxNuevaPractica";
            pictureBoxNuevaPractica.Size = new Size(24, 24);
            pictureBoxNuevaPractica.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxNuevaPractica.TabIndex = 0;
            pictureBoxNuevaPractica.TabStop = false;
            // 
            // panelInicio
            // 
            panelInicio.BackColor = Color.FromArgb(46, 125, 50);
            panelInicio.Controls.Add(panelIndicadorInicio);
            panelInicio.Controls.Add(lblInicio);
            panelInicio.Controls.Add(pictureBoxInicio);
            panelInicio.Cursor = Cursors.Hand;
            panelInicio.Location = new Point(12, 170);
            panelInicio.Name = "panelInicio";
            panelInicio.Size = new Size(195, 48);
            panelInicio.TabIndex = 3;
            // 
            // panelIndicadorInicio
            // 
            panelIndicadorInicio.BackColor = Color.FromArgb(168, 85, 247);
            panelIndicadorInicio.Location = new Point(0, 0);
            panelIndicadorInicio.Name = "panelIndicadorInicio";
            panelIndicadorInicio.Size = new Size(4, 48);
            panelIndicadorInicio.TabIndex = 2;
            panelIndicadorInicio.Visible = false;
            // 
            // lblInicio
            // 
            lblInicio.AutoSize = true;
            lblInicio.BackColor = Color.Transparent;
            lblInicio.Font = new Font("Segoe UI Semibold", 10F);
            lblInicio.ForeColor = Color.White;
            lblInicio.Location = new Point(50, 10);
            lblInicio.Name = "lblInicio";
            lblInicio.Size = new Size(61, 28);
            lblInicio.TabIndex = 1;
            lblInicio.Text = "Inicio";
            lblInicio.TextAlign = ContentAlignment.MiddleLeft;
            lblInicio.Click += LblInicio_Click;
            // 
            // pictureBoxInicio
            // 
            pictureBoxInicio.BackColor = Color.Transparent;
            pictureBoxInicio.Location = new Point(15, 13);
            pictureBoxInicio.Name = "pictureBoxInicio";
            pictureBoxInicio.Size = new Size(24, 24);
            pictureBoxInicio.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxInicio.TabIndex = 0;
            pictureBoxInicio.TabStop = false;
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackColor = Color.Transparent;
            pictureBoxLogo.Image = (Image)resources.GetObject("pictureBoxLogo.Image");
            pictureBoxLogo.Location = new Point(10, 30);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(200, 70);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 2;
            pictureBoxLogo.TabStop = false;
            // 
            // lblMenuSubtitulo
            // 
            lblMenuSubtitulo.AutoSize = true;
            lblMenuSubtitulo.BackColor = Color.Transparent;
            lblMenuSubtitulo.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblMenuSubtitulo.ForeColor = Color.Silver;
            lblMenuSubtitulo.Location = new Point(26, 94);
            lblMenuSubtitulo.Name = "lblMenuSubtitulo";
            lblMenuSubtitulo.Size = new Size(169, 28);
            lblMenuSubtitulo.TabIndex = 1;
            lblMenuSubtitulo.Text = "Centro de Control";
            // 
            // lblMenuTitulo
            // 
            lblMenuTitulo.AutoSize = true;
            lblMenuTitulo.BackColor = Color.Transparent;
            lblMenuTitulo.Font = new Font("Segoe UI Semibold", 13F);
            lblMenuTitulo.ForeColor = Color.White;
            lblMenuTitulo.Location = new Point(46, 129);
            lblMenuTitulo.Name = "lblMenuTitulo";
            lblMenuTitulo.Size = new Size(128, 36);
            lblMenuTitulo.TabIndex = 0;
            lblMenuTitulo.Text = "EndForge";
            lblMenuTitulo.Visible = false;
            // 
            // frmPrincipal
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(168, 85, 247);
            ClientSize = new Size(1178, 744);
            Controls.Add(panelMenu);
            Controls.Add(panelPrincipal);
            Controls.Add(fondoEndForge);
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "frmPrincipal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "EndForge";
            Load += FrmPrincipal_Load;
            panelPrincipal.ResumeLayout(false);
            panelPrincipal.PerformLayout();
            panelInicioVista.ResumeLayout(false);
            panelInicioVista.PerformLayout();
            panelCardConfiguracion.ResumeLayout(false);
            panelCardConfiguracion.PerformLayout();
            panelCardRecientes.ResumeLayout(false);
            panelCardRecientes.PerformLayout();
            panelCardNuevaPractica.ResumeLayout(false);
            panelCardNuevaPractica.PerformLayout();
            panelRecientesVista.ResumeLayout(false);
            panelRecientesVista.PerformLayout();
            panelListaRecientes.ResumeLayout(false);
            panelConfiguracionVista.ResumeLayout(false);
            panelConfiguracionVista.PerformLayout();
            panelMenu.ResumeLayout(false);
            panelMenu.PerformLayout();
            panelAcercaDe.ResumeLayout(false);
            panelAcercaDe.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAcercaDe).EndInit();
            panelConfiguracion.ResumeLayout(false);
            panelConfiguracion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxConfiguracion).EndInit();
            panelRecientes.ResumeLayout(false);
            panelRecientes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRecientes).EndInit();
            panelAbrirPractica.ResumeLayout(false);
            panelAbrirPractica.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAbrirPractica).EndInit();
            panelNuevaPractica.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxNuevaPractica).EndInit();
            panelInicio.ResumeLayout(false);
            panelInicio.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxInicio).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button btnCrearProyecto;
        private Label lblNombreFinal;
        private Label lblVistaPrevia;
        private TextBox txtObjetivo;
        private Label lblObjetivo;
        private TextBox txtNombreProyecto;
        private Label lblNombre;
        private Label lblTitulo;
        private ComboBox txtTemas;
        private Label lblTema;
        private Panel panelPrincipal;
        private FondoEndForge fondoEndForge;
        private Panel panelMenu;
        private Label lblMenuTitulo;
        private Label lblMenuSubtitulo;
        private PictureBox pictureBoxLogo;
        private Panel panelInicio;
        private PictureBox pictureBoxInicio;
        private Label lblInicio;
        private Panel panelIndicadorInicio;
        private Panel panelNuevaPractica;
        private Panel panelIndicadorNP;
        private Label lblNuevaPractica;
        private PictureBox pictureBoxNuevaPractica;
        private Panel panelAbrirPractica;
        private Panel panelIndicadorAP;
        private Label lblAbrirPractica;
        private PictureBox pictureBoxAbrirPractica;
        private Panel panelRecientes;
        private Panel panelIndicadorRecientes;
        private Label lblRecientes;
        private PictureBox pictureBoxRecientes;
        private Panel panelConfiguracion;
        private Panel panelIndicadorConfigs;
        private Label lblConfiguracion;
        private PictureBox pictureBoxConfiguracion;
        private Panel panelAcercaDe;
        private Panel panelIndicadorAD;
        private Label lblAcercaDe;
        private PictureBox pictureBoxAcercaDe;
        private Panel panelConfiguracionVista;
        private Button btnGuardarConfiguracion;
        private Button btnCambiarRutaPlantilla;
        private Button btnCambiarRutaBase;
        private TextBox txtRutaPlantillaConfig;
        private Label lblRutaPlantilla;
        private TextBox txtRutaBaseConfig;
        private Label lblRutaBase;
        private Label lblConfiguracionTitulo;
        private Panel panelRecientesVista;
        private Label lblRecientesSubtitulo;
        private ListBox listRecientes;
        private Label lblRecientesTitulo;
        private Label lblAyudaRecientes;
        private Panel panelInicioVista;
        private Label lblInicioTitulo;
        private Label lblInicioSubtitulo;
        private Panel panelCardNuevaPractica;
        private Label lblCardNuevaPracticaTitulo;
        private Label lblCardNuevaPracticaDesc;
        private Panel panelCardRecientes;
        private Label lblCardRecientesDesc;
        private Label lblCardRecientesTitulo;
        private Panel panelCardConfiguracion;
        private Label lblCardConfiguracionDesc;
        private Label lblCardConfiguracionTitulo;
        private Panel panelListaRecientes;
        private Label lblReciente1;
        private Label lblReciente10;
        private Label lblReciente9;
        private Label lblReciente8;
        private Label lblReciente7;
        private Label lblReciente6;
        private Label lblReciente5;
        private Label lblReciente4;
        private Label lblReciente3;
        private Label lblReciente2;
    }
}
