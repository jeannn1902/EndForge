using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EndForge {
    public class FondoEndForge : Control {

        private Image? imagenFondo;
        private Bitmap? imagenFondoAjustada;
        private Size tamanoImagenFondoAjustada;
        private int reconstruccionesImagenFondo;
        private Size tamanoCacheImagenFondoNoDisponible;

        [Category("Apariencia")]
        [Description("Imagen utilizada como fondo del control.")]
        public Image? ImagenFondo {
            get => imagenFondo;
            set {
                if (ReferenceEquals(imagenFondo, value))
                    return;

                imagenFondo = value;
                LiberarImagenFondoAjustada();
                tamanoCacheImagenFondoNoDisponible = Size.Empty;
                Invalidate();
            }
        }

        internal bool TieneImagenFondoAjustada => imagenFondoAjustada is not null;

        internal Size TamanoImagenFondoAjustada => tamanoImagenFondoAjustada;

        internal int ReconstruccionesImagenFondo => reconstruccionesImagenFondo;

        internal bool BordesCacheSonOpacos() {
            if (imagenFondoAjustada is null ||
                imagenFondoAjustada.Width <= 0 ||
                imagenFondoAjustada.Height <= 0) {
                return false;
            }

            int derecha = imagenFondoAjustada.Width - 1;
            int inferior = imagenFondoAjustada.Height - 1;
            return imagenFondoAjustada.GetPixel(0, 0).A == byte.MaxValue &&
                imagenFondoAjustada.GetPixel(derecha, 0).A == byte.MaxValue &&
                imagenFondoAjustada.GetPixel(0, inferior).A == byte.MaxValue &&
                imagenFondoAjustada.GetPixel(derecha, inferior).A == byte.MaxValue;
        }

        public FondoEndForge() {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (imagenFondo == null ||
                ClientSize.Width <= 0 ||
                ClientSize.Height <= 0)
                return;

            if (imagenFondoAjustada is null) {
                ActualizarCacheImagenFondo();
            }

            if (imagenFondoAjustada is null) {
                DibujarImagenOriginal(e.Graphics);
                return;
            }

            if (tamanoImagenFondoAjustada == ClientSize) {
                e.Graphics.DrawImageUnscaled(imagenFondoAjustada, Point.Empty);
                return;
            }

            InterpolationMode interpolacionAnterior = e.Graphics.InterpolationMode;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.DrawImage(imagenFondoAjustada, ClientRectangle);
            e.Graphics.InterpolationMode = interpolacionAnterior;
        }

        internal bool ActualizarCacheImagenFondo() {
            if (imagenFondo is null ||
                tamanoCacheImagenFondoNoDisponible == ClientSize ||
                ClientSize.Width <= 0 ||
                ClientSize.Height <= 0 ||
                (imagenFondoAjustada is not null &&
                 tamanoImagenFondoAjustada == ClientSize)) {
                return false;
            }

            Bitmap? nuevaImagen = null;

            try {
                nuevaImagen = new Bitmap(
                    ClientSize.Width,
                    ClientSize.Height,
                    PixelFormat.Format32bppPArgb);
                using Graphics graphics = Graphics.FromImage(nuevaImagen);
                using ImageAttributes atributosImagen = new();
                atributosImagen.SetWrapMode(WrapMode.TileFlipXY);
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(
                    imagenFondo,
                    new Rectangle(Point.Empty, ClientSize),
                    0,
                    0,
                    imagenFondo.Width,
                    imagenFondo.Height,
                    GraphicsUnit.Pixel,
                    atributosImagen);
            } catch (Exception ex) when (
                ex is ArgumentException or
                    ExternalException or
                    OutOfMemoryException) {
                nuevaImagen?.Dispose();
                tamanoCacheImagenFondoNoDisponible = ClientSize;
                return false;
            }

            Bitmap? imagenAnterior = imagenFondoAjustada;
            imagenFondoAjustada = nuevaImagen;
            tamanoImagenFondoAjustada = ClientSize;
            reconstruccionesImagenFondo++;
            tamanoCacheImagenFondoNoDisponible = Size.Empty;
            imagenAnterior?.Dispose();
            return true;
        }

        private void DibujarImagenOriginal(Graphics graphics) {
            if (imagenFondo is null) {
                return;
            }

            InterpolationMode interpolacionAnterior = graphics.InterpolationMode;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(imagenFondo, ClientRectangle);
            graphics.InterpolationMode = interpolacionAnterior;
        }

        private void LiberarImagenFondoAjustada() {
            Bitmap? imagenAnterior = imagenFondoAjustada;
            imagenFondoAjustada = null;
            tamanoImagenFondoAjustada = Size.Empty;
            imagenAnterior?.Dispose();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                LiberarImagenFondoAjustada();
            }

            base.Dispose(disposing);
        }
    }
}
