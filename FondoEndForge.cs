using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ProjectCreator {
    public class FondoEndForge : Control {

        private Image? imagenFondo;

        [Category("Apariencia")]
        [Description("Imagen utilizada como fondo del control.")]
        public Image? ImagenFondo {
            get => imagenFondo;
            set {
                imagenFondo = value;
                Invalidate();
            }
        }

        public FondoEndForge() {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (imagenFondo == null)
                return;

            e.Graphics.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            e.Graphics.DrawImage(
                imagenFondo,
                ClientRectangle
            );
        }
    }
}