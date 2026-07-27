using System.Resources;
using System.Runtime.ExceptionServices;
using EndForge.Services;

[assembly: NeutralResourcesLanguage("es-MX")]

namespace EndForge;

internal static class Program {
    internal const string MensajeErrorRecuperable =
        "EndForge encontró un error inesperado y no pudo completar la operación. " +
        "La aplicación intentará continuar.";

    private static readonly RegistroErroresService registroErrores = new();
    private static int mostrandoMensajeError;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main() {
        RegistrarManejadoresGlobales();

        try {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmPrincipal());
        } catch (Exception error) {
            bool esCritica = RegistroErroresService.EsExcepcionCritica(error);
            registroErrores.Registrar(
                error,
                OrigenRegistroError.InicioAplicacion,
                esTerminante: true);

            if (esCritica) {
                throw;
            }

            MostrarMensajeErrorRecuperable();
        }
    }

    private static void RegistrarManejadoresGlobales() {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += Aplicacion_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += Dominio_UnhandledException;
        TaskScheduler.UnobservedTaskException += Tareas_UnobservedTaskException;
    }

    private static void Aplicacion_ThreadException(
        object sender,
        ThreadExceptionEventArgs e) {
        bool esCritica = RegistroErroresService.EsExcepcionCritica(e.Exception);
        registroErrores.Registrar(
            e.Exception,
            OrigenRegistroError.Interfaz,
            esTerminante: esCritica);

        if (esCritica) {
            ExceptionDispatchInfo.Capture(e.Exception).Throw();
        }

        MostrarMensajeErrorRecuperable();
    }

    private static void Dominio_UnhandledException(
        object sender,
        UnhandledExceptionEventArgs e) {
        if (e.ExceptionObject is not Exception error) {
            return;
        }

        registroErrores.Registrar(
            error,
            OrigenRegistroError.DominioAplicacion,
            esTerminante: e.IsTerminating ||
                RegistroErroresService.EsExcepcionCritica(error));
    }

    private static void Tareas_UnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e) {
        bool esCritica = RegistroErroresService.EsExcepcionCritica(e.Exception);
        registroErrores.Registrar(
            e.Exception,
            OrigenRegistroError.TareaNoObservada,
            esTerminante: esCritica);

        if (!esCritica) {
            e.SetObserved();
        }
    }

    private static void MostrarMensajeErrorRecuperable() {
        if (Interlocked.Exchange(ref mostrandoMensajeError, 1) != 0) {
            return;
        }

        try {
            MessageBox.Show(
                MensajeErrorRecuperable,
                "EndForge",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        } catch (Exception errorAviso)
            when (!RegistroErroresService.EsExcepcionCritica(errorAviso)) {
            // El aviso tampoco debe provocar un segundo error no controlado.
        } finally {
            Volatile.Write(ref mostrandoMensajeError, 0);
        }
    }

    internal static void RegistrarErrorRecuperable(Exception error) {
        registroErrores.Registrar(
            error,
            OrigenRegistroError.Interfaz,
            esTerminante: false);
    }
}
