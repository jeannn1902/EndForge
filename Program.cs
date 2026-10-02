using EndForge.Data;
using EndForge.DependencyInjection;
using EndForge.Repositories;
using EndForge.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Resources;
using System.Runtime.ExceptionServices;

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
    private static void Main()
    {
        RegistrarManejadoresGlobales();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Construir el contenedor de servicios y registrar dependencias
        var services = new ServiceCollection();
        services.AddPersistenceServices();
        services.AddScoped<frmPrincipal>();

        // Determinar cadena de conexión; si falta, usar LocalDB con timeout corto para fallback inmediato
        var configuredConnection = configuration.GetConnectionString("DefaultConnection");
        string connectionToUse;
        if (string.IsNullOrWhiteSpace(configuredConnection))
        {
            connectionToUse = "Server=(localdb)\\mssqllocaldb;Database=EndForgeCloudDev;Trusted_Connection=True;Connection Timeout=3;";
        }
        else
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(configuredConnection)
                {
                    ConnectTimeout = 3
                };
                connectionToUse = builder.ConnectionString;
            }
            catch
            {
                connectionToUse = configuredConnection;
            }
        }

        services.AddDbContext<EndForgeDbContext>(options =>
            options.UseSqlServer(connectionToUse, sqlOptions => sqlOptions.EnableRetryOnFailure())
        );

        var provider = services.BuildServiceProvider();

        try
        {
            // Intentar aplicar migraciones antes de iniciar la UI. Si falla, se registra como recuperable.
            try
            {
                var repo = provider.GetRequiredService<IProgresoRepository>();
                repo.MigrateIfNeededAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                RegistrarErrorRecuperable(ex);
            }

            ApplicationConfiguration.Initialize();
            // Ejecutar la aplicación pidiendo frmPrincipal al contenedor DI
            Application.Run(provider.GetRequiredService<frmPrincipal>());
        }
        catch (Exception error)
        {
            bool esCritica = RegistroErroresService.EsExcepcionCritica(error);
            registroErrores.Registrar(error, OrigenRegistroError.InicioAplicacion, esTerminante: true);

            if (esCritica)
            {
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
        try
        {
            // No mostrar MessageBox en el arranque para evitar bloquear la interfaz.
            // Registramos un aviso al registro de errores y continuamos silenciosamente.
            registroErrores.Registrar(new Exception(MensajeErrorRecuperable), OrigenRegistroError.InicioAplicacion, esTerminante: false);
        }
        finally {
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
